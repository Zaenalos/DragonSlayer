using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum DragonState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Hurt,
    Dead
}

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class DragonBoss : MonoBehaviour
{
    #region Variables

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damage = 10;
    private int currentHealth;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    // [SerializeField] private float verticalDetectionRange = 2f;
    // Removed: jumpCooldown, jumpCooldownTimer, isJumping

    private int facingDirection = 1; // 1 for right, -1 for left
    public bool isGrounded;

    [Header("Combat")]
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("References")]
    [SerializeField] private GameObject player;
    private Rigidbody2D rb2d;
    private Animator animator;
    private Vector2 patrolOrigin;
    private int patrolDirection = 1; // 1 for right, -1 for left
    private bool isAttacking;
    private HeroKnight playerComponent;

    [Header("Ground Check")]
    [SerializeField] private Sensor_Animal groundSensor;

    private DragonState currentState = DragonState.Idle;
    private Vector2 tempVelocity;
    private Vector3 tempScale = Vector3.one;
    private WaitForSeconds attackAnimationDelay;
    private WaitForSeconds attackCooldownDelay;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onDamageTaken;

    // Animation parameter hashes for improved performance
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int AirSpeed = Animator.StringToHash("AirSpeed");
    // Add these new static readonly fields along with the other animation parameter hashes
    private static readonly int AttackTrigger1 = Animator.StringToHash("Attack 1");
    private static readonly int AttackTrigger2 = Animator.StringToHash("Attack 2");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int DeathTrigger = Animator.StringToHash("Death");

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        attackAnimationDelay = new WaitForSeconds(0.3f);
        attackCooldownDelay = new WaitForSeconds(1f / attackRate);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        patrolOrigin = transform.position;
    }

    private void Update()
    {
        isGrounded = groundSensor.State();

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        UpdateStateTransitions(distanceToPlayer);
        HandleCurrentState(distanceToPlayer);
        UpdateAnimator();
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        currentState = DragonState.Idle;
        if (rb2d) rb2d.simulated = true;
        var collider = GetComponent<Collider2D>();
        if (collider) collider.enabled = true;
    }

    #endregion

    #region State Management

    private void UpdateStateTransitions(float distanceToPlayer)
    {
        if (currentState == DragonState.Dead || player == null ||
            (playerComponent != null && playerComponent.isDead)) return;
        if (currentState == DragonState.Hurt) return;

        if (currentState == DragonState.Idle)
        {
            if (distanceToPlayer <= detectionRange)
                currentState = DragonState.Chase;
            else if (Random.value < 0.01f)
                currentState = DragonState.Patrol;
        }
        else if (currentState == DragonState.Patrol && distanceToPlayer <= detectionRange)
        {
            currentState = DragonState.Chase;
        }
        else if (currentState == DragonState.Chase)
        {
            if (distanceToPlayer <= attackRange)
                currentState = DragonState.Attack;
            else if (distanceToPlayer > detectionRange * 1.2f)
                currentState = DragonState.Patrol;
        }
        else if (currentState == DragonState.Attack && distanceToPlayer > attackRange)
        {
            currentState = DragonState.Chase;
        }
    }

    private void HandleCurrentState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case DragonState.Idle:
                tempVelocity.x = 0;
                tempVelocity.y = rb2d.linearVelocity.y;
                rb2d.linearVelocity = tempVelocity;
                break;

            case DragonState.Patrol:
                Patrol();
                break;

            case DragonState.Chase:
                Chase();
                break;

            case DragonState.Attack:
                if (!isAttacking)
                    StartCoroutine(AttackRoutine());
                break;

            case DragonState.Hurt:
                tempVelocity.x = 0;
                tempVelocity.y = rb2d.linearVelocity.y;
                rb2d.linearVelocity = tempVelocity;
                break;
        }
    }

    private void Patrol()
    {
        float offset = transform.position.x - patrolOrigin.x;
        if (Mathf.Abs(offset) >= patrolRange)
            patrolDirection *= -1;

        tempScale.x = patrolDirection > 0 ? -1 : 1;
        transform.localScale = tempScale;

        tempVelocity.x = patrolSpeed * patrolDirection;
        tempVelocity.y = rb2d.linearVelocity.y;
        rb2d.linearVelocity = tempVelocity;
    }

    private void Chase()
    {
        Vector2 playerPos = player.transform.position;
        Vector2 direction = playerPos - (Vector2)transform.position;
        float directionX = direction.normalized.x;
        float differenceY = playerPos.y - transform.position.y;

        // Jump only if the player is above the enemy and within horizontal detection range.
        //if (differenceY > 2.0f && isGrounded && Mathf.Abs(direction.x) < detectionRange)
        //{
        //    Debug.Log("Jumping to reach player!");
        //    Jump();
        //}

        tempVelocity.x = directionX * chaseSpeed;
        // Preserve existing vertical velocity.
        tempVelocity.y = rb2d.linearVelocity.y;
        rb2d.linearVelocity = tempVelocity;
        FacePlayer();
    }

    // Modified AttackRoutine method to randomize the attack trigger
    private IEnumerator AttackRoutine()
    {
        if (player == null || (playerComponent != null && playerComponent.isDead))
        {
            currentState = DragonState.Idle;
            yield break;
        }

        isAttacking = true;
        tempVelocity.x = 0;
        tempVelocity.y = rb2d.linearVelocity.y;
        rb2d.linearVelocity = tempVelocity;
        FacePlayer();

        // Randomly choose between Attack1 and Attack2 triggers.
        if (Random.value < 0.5f)
            animator.SetTrigger(AttackTrigger1);
        else
            animator.SetTrigger(AttackTrigger2);

        yield return attackAnimationDelay;

        if (currentState == DragonState.Attack)
        {
            Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
            if (hit != null && playerComponent != null)
                playerComponent.TakeDamage(damage, facingDirection);
        }

        yield return attackCooldownDelay;
        isAttacking = false;
    }

    #endregion

    #region Helper Methods

    private void InitializeComponents()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            player = GameObject.FindWithTag("Player") ?? GameObject.Find("HeroKnight");
        }

        if (player != null)
            playerComponent = player.GetComponent<HeroKnight>();
        else
            Debug.LogWarning("No player reference assigned or found!");

        if (attackPoint == null)
            attackPoint = transform;
    }

    private void FacePlayer()
    {
        if (player == null) return;
        bool facingRight = player.transform.position.x > transform.position.x;
        facingDirection = facingRight ? 1 : -1;
        tempScale.x = facingRight ? -1 : 1;
        transform.localScale = tempScale;
    }

    private void UpdateAnimator()
    {
        int newAnimState = (currentState == DragonState.Patrol || currentState == DragonState.Chase) ? 2 : 0;
        animator.SetInteger(AnimState, newAnimState);

        animator.SetBool(Grounded, isGrounded);
        animator.SetFloat(AirSpeed, rb2d.linearVelocity.y);
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(0.2f);
        if (currentState == DragonState.Hurt)
            currentState = DragonState.Idle;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == DragonState.Dead) return;

        currentHealth -= damageAmount;
        onDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            currentState = DragonState.Dead;
            Die();
        }
        else
        {
            currentState = DragonState.Hurt;
            animator.SetTrigger(HurtTrigger);
            StartCoroutine(RecoverFromHurt());
        }
    }

    private void Die()
    {
        rb2d.linearVelocity = Vector2.zero;
        animator.SetTrigger(DeathTrigger);
        onDeath?.Invoke();
        //rb2d.simulated = false;
        //GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        else
            Gizmos.DrawWireSphere(transform.position, attackRange);

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y + verticalDetectionRange, transform.position.z));

        Gizmos.color = Color.green;
        if (!Application.isPlaying)
        {
            Vector2 startPos = transform.position;
            Gizmos.DrawLine(startPos, new Vector2(startPos.x + patrolRange, startPos.y));
            Gizmos.DrawLine(startPos, new Vector2(startPos.x - patrolRange, startPos.y));
        }
        else
        {
            Gizmos.DrawLine(patrolOrigin, new Vector2(patrolOrigin.x + patrolRange, patrolOrigin.y));
            Gizmos.DrawLine(patrolOrigin, new Vector2(patrolOrigin.x - patrolRange, patrolOrigin.y));
        }
#endif
    }

    #endregion
}
