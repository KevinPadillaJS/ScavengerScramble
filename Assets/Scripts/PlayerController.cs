using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PotControllerLegacy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 7.5f;

    [Header("Grounding")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.24f;          // a bit larger
    [SerializeField] LayerMask groundMask;

    [Header("Jump Quality")]
    [SerializeField] float jumpBufferTime = 0.10f;         // jump pressed slightly before landing
    [SerializeField] float coyoteTime = 0.10f;             // small grace after leaving ground

    Rigidbody rb;

    // runtime
    bool grounded;
    float jumpBufferCounter = 0f;
    float coyoteCounter = 0f;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        // capture input in Update so we never miss a press between physics ticks
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
    }

    void FixedUpdate()
    {
        // horizontal move
        float x = Input.GetAxis("Horizontal");
        Vector3 v = rb.linearVelocity;                      // or rb.velocity in earlier Unity
        v.x = x * moveSpeed;
        rb.linearVelocity = v;

        // ground check each physics step
        grounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundMask,
            QueryTriggerInteraction.Ignore);

        // update coyote timer
        if (grounded) coyoteCounter = coyoteTime;
        else          coyoteCounter -= Time.fixedDeltaTime;

        // try to consume buffered jump when we're allowed to jump
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            // stable jump height (clear residual vertical vel)
            v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpBufferCounter = 0f;                         // consume
            coyoteCounter = 0f;
        }
        else
        {
            // tick down the buffer if we didn't jump this step
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
