using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HeroKnight : MonoBehaviour
{
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

    #endregion

    #region Private Fields

    private Animator animator;
    private Rigidbody2D body2d;
    private SpriteRenderer spriteRenderer;
    private bool grounded, rolling, blocking;
    private int facingDirection = 1;
    private int currentAttack;
    private float timeSinceAttack, delayToIdle = 0.05f, rollCurrentTime;
    private float moveInput;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (groundSensor == null)
            groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();

        // Assign UI button listeners
        attackButton.onClick.AddListener(ProcessAttack);
        jumpButton.onClick.AddListener(Jump);
        rollButton.onClick.AddListener(StartRoll);
        blockButton.onClick.AddListener(() => blocking = true);
    }

    private void Update()
    {
        HandleTimers();
        UpdateGroundStatus();
        UpdateBlockingState();
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
        }
    }

    private void UpdateBlockingState()
    {
        if (!blocking)
        {
            animator.SetBool("IdleBlock", false);
        }
        else if (grounded && !rolling)
        {
            animator.SetTrigger("Block");
            animator.SetBool("IdleBlock", true);
        }
    }

    private void HandleTouchMovement()
    {
        body2d.linearVelocity = new Vector2(moveInput * speed, body2d.linearVelocity.y);
        animator.SetFloat("AirSpeedY", body2d.linearVelocity.y);

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            spriteRenderer.flipX = moveInput < 0;
            facingDirection = moveInput < 0 ? -1 : 1;
            animator.SetInteger("AnimState", 1);
        }
        else if (!blocking)
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0)
                animator.SetInteger("AnimState", 0);
        }
    }

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

    #endregion
}
