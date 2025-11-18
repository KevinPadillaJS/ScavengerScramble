using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableBox : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Push Settings")]
    public float pushForce = 5f;
    public float maxPushSpeed = 5f;

    // Original starting point
    private Vector3 startPos;
    private Quaternion startRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = 10f;
    }

    void Start()
    {
        // Save original spawn position
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player1")) return;

        float pushDir = (collision.transform.position.x < transform.position.x) ? 1f : -1f;

        Vector3 force = new Vector3(pushDir * pushForce, 0f, 0f);
        rb.AddForce(force, ForceMode.Force);

        Vector3 vel = rb.linearVelocity;
        vel.x = Mathf.Clamp(vel.x, -maxPushSpeed, maxPushSpeed);
        rb.linearVelocity = vel;
    }

    // --------------------------------
    // PUBLIC RESET FUNCTION
    // --------------------------------
    public void ResetBox()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPos;
        transform.rotation = startRot;
    }
}




