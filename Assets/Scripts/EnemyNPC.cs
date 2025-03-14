using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Hurt,
    Dead
}

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class EnemyNPC : MonoBehaviour
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
    [SerializeField] private float jumpForce = 5f;
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
    [SerializeField] private Sensor_Bandit groundSensor;

    private EnemyState currentState = EnemyState.Idle;
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
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
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
        currentState = EnemyState.Idle;
        if (rb2d) rb2d.simulated = true;
        var collider = GetComponent<Collider2D>();
        if (collider) collider.enabled = true;
    }

    #endregion

    #region State Management

    private void UpdateStateTransitions(float distanceToPlayer)
    {
        if (currentState == EnemyState.Dead || player == null ||
            (playerComponent != null && playerComponent.isDead)) return;
        if (currentState == EnemyState.Hurt) return;

        if (currentState == EnemyState.Idle)
        {
            if (distanceToPlayer <= detectionRange)
                currentState = EnemyState.Chase;
            else if (Random.value < 0.01f)
                currentState = EnemyState.Patrol;
        }
        else if (currentState == EnemyState.Patrol && distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else if (currentState == EnemyState.Chase)
        {
            if (distanceToPlayer <= attackRange)
                currentState = EnemyState.Attack;
            else if (distanceToPlayer > detectionRange * 1.2f)
                currentState = EnemyState.Patrol;
        }
        else if (currentState == EnemyState.Attack && distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void HandleCurrentState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                tempVelocity.x = 0;
                tempVelocity.y = rb2d.linearVelocity.y;
                rb2d.linearVelocity = tempVelocity;
                break;

            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                if (!isAttacking)
                    StartCoroutine(AttackRoutine());
                break;

            case EnemyState.Hurt:
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
        if (differenceY > 2.0f && isGrounded && Mathf.Abs(direction.x) < detectionRange)
        {
            Debug.Log("Jumping to reach player!");
            Jump();
        }

        tempVelocity.x = directionX * chaseSpeed;
        // Preserve existing vertical velocity.
        tempVelocity.y = rb2d.linearVelocity.y;
        rb2d.linearVelocity = tempVelocity;
        FacePlayer();
    }


    private void Jump()
    {
        if (!isGrounded)
            return;

        animator.SetTrigger("Jump");
        // Reset vertical velocity for consistency.
        rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0f);
        rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator AttackRoutine()
    {
        if (player == null || (playerComponent != null && playerComponent.isDead))
        {
            currentState = EnemyState.Idle;
            yield break;
        }

        isAttacking = true;
        tempVelocity.x = 0;
        tempVelocity.y = rb2d.linearVelocity.y;
        rb2d.linearVelocity = tempVelocity;
        FacePlayer();
        animator.SetTrigger(AttackTrigger);
        yield return attackAnimationDelay;

        if (currentState == EnemyState.Attack)
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
        int newAnimState = (currentState == EnemyState.Patrol || currentState == EnemyState.Chase) ? 2 : 0;
        animator.SetInteger(AnimState, newAnimState);

        animator.SetBool(Grounded, isGrounded);
        animator.SetFloat(AirSpeed, rb2d.linearVelocity.y);
    }

    private IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(0.2f);
        if (currentState == EnemyState.Hurt)
            currentState = EnemyState.Idle;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damageAmount;
        onDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            currentState = EnemyState.Dead;
            Die();
        }
        else
        {
            currentState = EnemyState.Hurt;
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
