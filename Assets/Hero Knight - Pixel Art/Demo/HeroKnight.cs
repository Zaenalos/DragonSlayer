using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;  // Required for the new Input System

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class HeroKnight : MonoBehaviour
{
    // Input and audio.
    private InputSystem controls;
    private AudioSource audioSource;

    #region Serialized Fields

    [Header("UI Elements")]
    [SerializeField] private HealthBar healthBar;

    [Header("Player Attributes")]
    [SerializeField] public int health = 0;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int playerDamage = 35;
    [SerializeField] private int playerAttackRange = 1;
    [SerializeField] public bool isDead = false;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float rollDuration = 8.0f / 14.0f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject slideDust;

    [Header("Sensors")]
    [SerializeField] private Sensor_HeroKnight groundSensor;
    [SerializeField] private Transform attackPoint;

    [Header("UI Buttons")]
    public Button attackButton;
    public Button jumpButton;
    public Button rollButton;
    public Button blockButton;
    public Button moveLeftButton;
    public Button moveRightButton;

    #endregion

    #region Private Fields

    private Animator animator;
    private Rigidbody2D body2d;
    private SpriteRenderer spriteRenderer;
    private bool grounded, rolling;
    private int facingDirection = 1; // -1 = left; 1 = right
    private int currentAttack;
    private float timeSinceAttack;
    private float rollCurrentTime;
    private float moveInput;

    // Idle delay for animation state transition.
    private const float idleDelay = 0.05f;
    private float currentIdleDelay = idleDelay;

    // Block button parameters.
    private const float blockHoldThreshold = 0.1f;
    private bool blockButtonIsHeld;
    private float blockButtonPressTime;
    private bool isCheckingBlockHold;
    private bool blockHoldStartedInAir;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Initialize components and input system.
        healthBar = FindFirstObjectByType<HealthBar>();
        health = maxHealth;
        controls = new InputSystem();
        audioSource = GetComponent<AudioSource>();

        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        SetupInputCallbacks();
        SetupUIButtons();

        // Set up ground sensor if not assigned.
        if (groundSensor == null)
            groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();
    }

    private void Start()
    {
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(health);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        Die();
        HandleTimers();

        if (!rolling && blockButtonIsHeld && grounded && (Time.time - blockButtonPressTime >= blockHoldThreshold)
            && !animator.GetBool("IdleBlock"))
        {
            animator.SetBool("IdleBlock", true);
            moveInput = 0f;
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
        }

        UpdateGroundStatus();
        HandleTouchMovement();
    }
    #endregion

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy")) // Ignore the collision with the enemies.
    //    {
    //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
    //    }
    //}

    #region Input & UI Setup

    private void SetupInputCallbacks()
    {
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        controls.Player.Move.canceled += ctx => moveInput = 0f;
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Attack.performed += ctx => ProcessAttack();
        controls.Player.Block.started += ctx => OnBlockStarted();
        controls.Player.Block.canceled += ctx => OnBlockCanceled();
        controls.Player.Roll.performed += ctx => StartRoll();
    }

    private void SetupUIButtons()
    {
        if (attackButton) attackButton.onClick.AddListener(ProcessAttack);
        if (jumpButton) jumpButton.onClick.AddListener(Jump);
        if (rollButton) rollButton.onClick.AddListener(StartRoll);
        if (blockButton) SetupBlockButton(blockButton);
        if (moveLeftButton) SetupMovementButton(moveLeftButton, -1f);
        if (moveRightButton) SetupMovementButton(moveRightButton, 1f);
    }

    private void ToggleButtons(bool state)
    {
        if (attackButton) attackButton.interactable = state;
        if (jumpButton) jumpButton.interactable = state;
        if (rollButton) rollButton.interactable = state;
        if (blockButton) blockButton.interactable = state;
        if (moveLeftButton) moveLeftButton.interactable = state;
        if (moveRightButton) moveRightButton.interactable = state;
    }

    #endregion

    #region Update Helpers

    private void HandleTimers()
    {
        timeSinceAttack += Time.deltaTime;

        if (rolling)
        {
            rollCurrentTime += Time.deltaTime;
            if (rollCurrentTime > rollDuration)
            {
                rolling = false;
                rollCurrentTime = 0f;
            }
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
        healthBar.SetHealth(health);
    }

    public void DieTrap()
    {
        health = 0;
        healthBar.SetHealth(health);
        Die();
    }

    public void TakeDamage(int receivedDamage, int enemyDirection)
    {
        if (animator.GetBool("IdleBlock") && enemyDirection != facingDirection)
            return;
        animator.SetTrigger("Hurt");
        health -= receivedDamage;
        healthBar.SetHealth(health);
    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, playerAttackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
                enemy.GetComponent<EnemyNPC>().TakeDamage(playerDamage);
            if (enemy.CompareTag("EnemyAnimal"))
                enemy.GetComponent<AnimalEnemyNPC>().TakeDamage(playerDamage);
            if (enemy.CompareTag("EnemyBoss"))
                enemy.GetComponent<DragonBoss>().TakeDamage(playerDamage);
        }
    }

    public void Die()
    {
        if (isDead) return;
        if (health <= 0)
        {
            isDead = true;

            // Stop movement
            moveInput = 0f;
            body2d.linearVelocity = Vector2.zero;

            // If the UI any of the UI button is held, release i

            // Disable UI Buttons
            ToggleButtons(false);

            // Disable player controls
            controls.Disable();

            // Play death animation
            animator.SetTrigger("Death");
        }
    }

    public void OnDeathAnim() // Called by Animation Event
    {
        GetComponent<RespawnLogic>().Respawn();
        animator.SetBool("Block", true);

        isDead = false;

        // Re-enable controls
        controls.Enable();

        // Enable UI Buttons
        ToggleButtons(true);
    }

    private void UpdateGroundStatus()
    {
        bool sensorGrounded = groundSensor != null && groundSensor.State();
        if (grounded != sensorGrounded)
        {
            grounded = sensorGrounded;
            animator.SetBool("Grounded", grounded);

            if (grounded && blockButtonIsHeld)
            {
                if (blockHoldStartedInAir && !rolling && !animator.GetBool("IdleBlock"))
                {
                    animator.SetBool("IdleBlock", true);
                    moveInput = 0f;
                    body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
                    blockHoldStartedInAir = false;
                }
                else if (!isCheckingBlockHold)
                {
                    StartCoroutine(CheckBlockHold());
                }
            }
        }
    }

    private void HandleTouchMovement()
    {
        if (animator.GetBool("IdleBlock") && grounded && !rolling)
        {
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                spriteRenderer.flipX = (moveInput < 0);
                facingDirection = (moveInput < 0) ? -1 : 1;
            }
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
            return;
        }

        body2d.linearVelocity = new Vector2(moveInput * speed, body2d.linearVelocity.y);
        animator.SetFloat("AirSpeedY", body2d.linearVelocity.y);

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            currentIdleDelay = idleDelay;
            spriteRenderer.flipX = (moveInput < 0);
            facingDirection = (moveInput < 0) ? -1 : 1;
            animator.SetInteger("AnimState", 1);
        }
        else if (!blockButtonIsHeld)
        {
            currentIdleDelay -= Time.deltaTime;
            if (currentIdleDelay <= 0)
                animator.SetInteger("AnimState", 0);
        }
    }

    public void SetMovementInput(float input) => moveInput = input;

    private void ProcessAttack()
    {
        if (timeSinceAttack > 0.25f && !rolling)
        {
            currentAttack = (currentAttack >= 3 || timeSinceAttack > 1.0f) ? 1 : currentAttack + 1;
            animator.SetTrigger("Attack" + currentAttack);
            DealDamage();
            audioSource.Play();
            timeSinceAttack = 0f;
        }
    }

    private void StartRoll()
    {
        if (!rolling)
        {
            rolling = true;
            rollCurrentTime = 0f;
            animator.SetTrigger("Roll");
            body2d.linearVelocity = new Vector2(facingDirection * rollForce, body2d.linearVelocity.y);
        }
    }

    private void Jump()
    {
        if (grounded && !rolling)
        {
            animator.SetTrigger("Jump");
            grounded = false;
            animator.SetBool("Grounded", false);
            body2d.linearVelocity = new Vector2(body2d.linearVelocity.x, jumpForce);
        }
    }

    private void OnBlockStarted()
    {
        blockButtonIsHeld = true;
        blockButtonPressTime = Time.time;
        if (!grounded)
            blockHoldStartedInAir = true;

        if (grounded && !rolling)
        {
            moveInput = 0f;
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
            animator.SetBool("Block", true);
            animator.SetInteger("AnimState", 0);
        }

        if (!isCheckingBlockHold)
            StartCoroutine(CheckBlockHold());
    }

    private void OnBlockCanceled()
    {
        blockButtonIsHeld = false;
        blockHoldStartedInAir = false;
        float pressDuration = Time.time - blockButtonPressTime;
        if (pressDuration < blockHoldThreshold)
            animator.SetTrigger("Block");
        animator.SetBool("IdleBlock", false);
    }

    private IEnumerator CheckBlockHold()
    {
        isCheckingBlockHold = true;
        yield return new WaitForSeconds(blockHoldThreshold);
        if (blockButtonIsHeld && grounded && !rolling && !animator.GetBool("IdleBlock"))
        {
            animator.SetBool("IdleBlock", true);
            moveInput = 0f;
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
        }
        isCheckingBlockHold = false;
    }
    #endregion

    #region Movement & Block Button Setup

    private void SetupMovementButton(Button button, float direction)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        var pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((data) => SetMovementInput(direction));
        trigger.triggers.Add(pointerDownEntry);

        var pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((data) => SetMovementInput(0f));
        trigger.triggers.Add(pointerUpEntry);
    }

    private void SetupBlockButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        var pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((data) => OnBlockStarted());
        trigger.triggers.Add(pointerDownEntry);

        var pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((data) => OnBlockCanceled());
        trigger.triggers.Add(pointerUpEntry);
    }
    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint ? attackPoint.position : transform.position, playerAttackRange);
#endif
    }
    #endregion
}
