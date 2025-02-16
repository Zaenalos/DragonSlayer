using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;  // Required for the new Input System

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HeroKnight : MonoBehaviour
{
    // Generated Input Action class instance.
    private InputSystem controls;
    private AudioSource audioSource;
    public HealthBar healthBar;

    #region Serialized Fields

    [Header("Player Attributes")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int playerDamage = 10;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private float rollDuration = 8.0f / 14.0f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject slideDust;

    [Header("Sensors")]
    [SerializeField] private Sensor_HeroKnight groundSensor;

    [Header("UI Buttons")]
    public Button attackButton;
    public Button jumpButton;
    public Button rollButton;
    public Button blockButton;

    // Movement buttons for touch input.
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
    private const float blockHoldThreshold = 0.1f; // Seconds required to register as a hold.
    private bool blockButtonIsHeld = false;
    private float blockButtonPressTime = 0f;
    private bool isCheckingBlockHold = false;
    // Flag to track if block hold began while in mid-air.
    private bool blockHoldStartedInAir = false;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        controls = new InputSystem();
        audioSource = GetComponent<AudioSource>();

        // Subscribe to Input System actions.
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        controls.Player.Move.canceled += ctx => moveInput = 0f;
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Attack.performed += ctx => ProcessAttack();
        controls.Player.Block.started += ctx => OnBlockStarted();
        controls.Player.Block.canceled += ctx => OnBlockCanceled();
        controls.Player.Roll.performed += ctx => StartRoll();

        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (groundSensor == null)
            groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();

        // Assign UI button listeners.
        if (attackButton) attackButton.onClick.AddListener(ProcessAttack);
        if (jumpButton) jumpButton.onClick.AddListener(Jump);
        if (rollButton) rollButton.onClick.AddListener(StartRoll);

        if (blockButton != null)
            SetupBlockButton(blockButton);

        if (moveLeftButton != null)
            SetupMovementButton(moveLeftButton, -1f);
        if (moveRightButton != null)
            SetupMovementButton(moveRightButton, 1f);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        HandleTimers();

        // If a roll just ended and the block button is still held—with the hold duration met—trigger block.
        if (!rolling && blockButtonIsHeld && grounded &&
            (Time.time - blockButtonPressTime >= blockHoldThreshold) &&
            !animator.GetBool("IdleBlock"))
        {
            animator.SetBool("IdleBlock", true);
            moveInput = 0f;
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
        }

        UpdateGroundStatus();
        HandleTouchMovement();
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

    private void UpdateGroundStatus()
    {
        bool sensorGrounded = groundSensor != null && groundSensor.State();
        if (grounded != sensorGrounded)
        {
            grounded = sensorGrounded;
            animator.SetBool("Grounded", grounded);

            // When landing with block already held:
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
        // When in idle block state on the ground, allow flipping but disable horizontal movement.
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

        // Normal movement.
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

    // Called by movement UI button events.
    public void SetMovementInput(float input)
    {
        moveInput = input;
    }

    #endregion

    #region Input Handlers

    private void ProcessAttack()
    {
        if (timeSinceAttack > 0.25f && !rolling)
        {
            currentAttack = (currentAttack >= 3 || timeSinceAttack > 1.0f) ? 1 : currentAttack + 1;
            animator.SetTrigger("Attack" + currentAttack);
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
            groundSensor?.Disable(0.2f);
        }
    }

    // Common block start handler for both Input System and UI.
    private void OnBlockStarted()
    {
        blockButtonIsHeld = true;
        blockButtonPressTime = Time.time;
        if (!grounded)
            blockHoldStartedInAir = true;

        // Immediately cancel movement and enter block mode if grounded and not rolling.
        if (grounded && !rolling)
        {
            moveInput = 0f;
            body2d.linearVelocity = new Vector2(0f, body2d.linearVelocity.y);
            animator.SetBool("IdleBlock", true);
            animator.SetInteger("AnimState", 0);
        }

        if (!isCheckingBlockHold)
            StartCoroutine(CheckBlockHold());
    }

    // Common block cancel handler for both Input System and UI.
    private void OnBlockCanceled()
    {
        blockButtonIsHeld = false;
        blockHoldStartedInAir = false;
        float pressDuration = Time.time - blockButtonPressTime;
        if (pressDuration < blockHoldThreshold)
            animator.SetTrigger("Block");
        animator.SetBool("IdleBlock", false);
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
        // Remove any existing onClick listeners.
        button.onClick.RemoveAllListeners();

        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        // Use the common OnBlockStarted for pointer down.
        var pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((data) => OnBlockStarted());
        trigger.triggers.Add(pointerDownEntry);

        // Use the common OnBlockCanceled for pointer up.
        var pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((data) => OnBlockCanceled());
        trigger.triggers.Add(pointerUpEntry);
    }

    // Optimized CheckBlockHold coroutine using WaitForSeconds.
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
}
