using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DualPlayerController : MonoBehaviour
{
    public enum PlayerType { Rat, Pigeon }

    [Header("Who is this?")]
    public PlayerType player = PlayerType.Rat;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7.5f;

    [Header("Facing")]
    [SerializeField] Transform modelPivot;   // keep pivot turning

    [Header("Grounding")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.24f;
    [SerializeField] LayerMask groundMask;

    [Header("Jump Quality")]
    [SerializeField] float jumpBufferTime = 0.10f;
    [SerializeField] float coyoteTime = 0.10f;

    [Header("Jumps")]
    [SerializeField] int maxJumps = 1;       // Rat=1, Pigeon=2 (overridden in Awake)

    [Header("Keys (per player)")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;

    Rigidbody rb;

    // runtime
    bool grounded;
    bool wasGrounded;
    int jumpCount;
    float jumpBufferCounter;
    float coyoteCounter;

    void Awake()
    {
        if (player == PlayerType.Rat)
        {
            maxJumps = 1;
            leftKey = KeyCode.A; rightKey = KeyCode.D; jumpKey = KeyCode.W;
        }
        else
        {
            maxJumps = 2;
            leftKey = KeyCode.J; rightKey = KeyCode.L; jumpKey = KeyCode.Space;
        }

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (Input.GetKeyDown(jumpKey))
            jumpBufferCounter = jumpBufferTime;
    }

    void FixedUpdate()
    {
        HandleMove();

        // --- ground check ---
        wasGrounded = grounded;
        grounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (grounded)
        {
            // refresh coyote every grounded frame
            coyoteCounter = coyoteTime;

            // RESET JUMPS ONLY ON LANDING (transition false -> true)
            if (!wasGrounded)
                jumpCount = 0;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        HandleJump();
    }

    void HandleMove()
    {
        float dir = 0f;
        if (Input.GetKey(leftKey))  dir = -1f;
        if (Input.GetKey(rightKey)) dir =  1f;

        Vector3 v = rb.linearVelocity;    // use rb.velocity on older Unity
        v.x = dir * moveSpeed;
        rb.linearVelocity = v;

        if (dir != 0f && modelPivot != null)
        {
            var e = modelPivot.localEulerAngles;
            e.y = (dir > 0f) ? -90f : 90f;
            modelPivot.localEulerAngles = e;
        }
    }

    void HandleJump()
    {
        bool canCoyoteJump = coyoteCounter > 0f;           // grace after leaving ground
        bool hasAirJumpsLeft = jumpCount < maxJumps;       // 1 for Rat, 2 for Pigeon

        // We can jump if: we buffered a press AND (we're in coyote OR we have air jumps left)
        if (jumpBufferCounter > 0f && (canCoyoteJump || (!grounded && hasAirJumpsLeft) || (grounded && hasAirJumpsLeft)))
        {
            // remove vertical momentum for consistent height
            var v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            jumpCount++;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;           // once we jump, kill coyote
            grounded = false;             // prevent immediate re-grounding for this frame
        }
        else
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
