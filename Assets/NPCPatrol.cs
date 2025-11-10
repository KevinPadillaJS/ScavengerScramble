using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Path")]
    public Transform[] points;              // assign 2+ points (e.g., left/right)
    public bool pingPong = true;            // bounce back & forth; off = loop
    public float arriveDistance = 0.05f;
    public float waitAtPoint = 0.5f;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float turnSpeed = 540f;          // deg/sec to face the move direction
    public float gravity = -9.81f;

    [Header("Animator (optional)")]
    public Animator animator;               // drag your cat Animator
    public string moveSpeedParam = "Speed"; // set to a float param in your controller

    CharacterController cc;
    int index = 0, dir = 1;
    float waitTimer = 0f;
    float verticalVel = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (points == null || points.Length == 0) return;

        // gravity
        if (cc.isGrounded && verticalVel < 0f) verticalVel = -2f;
        verticalVel += gravity * Time.deltaTime;

        // wait at point
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            cc.Move(Vector3.up * verticalVel * Time.deltaTime);
            if (animator && !string.IsNullOrEmpty(moveSpeedParam))
                animator.SetFloat(moveSpeedParam, 0f);
            return;
        }

        // target and horizontal move
        Vector3 target = points[index].position;
        Vector3 to = target - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        Vector3 moveDir = dist > 0.001f ? to.normalized : Vector3.zero;
        Vector3 velocity = moveDir * walkSpeed;

        // face movement (assumes Y-up)
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, face, turnSpeed * Time.deltaTime);
        }

        // move
        Vector3 total = velocity + Vector3.up * verticalVel;
        cc.Move(total * Time.deltaTime);

        if (animator && !string.IsNullOrEmpty(moveSpeedParam))
            animator.SetFloat(moveSpeedParam, velocity.magnitude);

        // arrived?
        if (dist <= arriveDistance)
        {
            waitTimer = waitAtPoint;

            if (pingPong)
            {
                if (index == 0) dir = 1;
                else if (index == points.Length - 1) dir = -1;
                index += dir;
            }
            else
            {
                index = (index + 1) % points.Length;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (points == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length; i++)
        {
            if (!points[i]) continue;
            Gizmos.DrawSphere(points[i].position + Vector3.up * 0.05f, 0.05f);
            if (i + 1 < points.Length && points[i + 1])
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
        }
    }
}
