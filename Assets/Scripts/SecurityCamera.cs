using UnityEngine;
using UnityEngine.Events;

public class SecurityCamera : MonoBehaviour
{
    [Header("References")]
    public Transform head; // assign the visible camera head (the one that should tilt down)

    [Header("Sweep")]
    public float sweepAngle = 60f;
    public float sweepSpeed = 30f;
    public float pauseAtEnds = 0.35f;

    [Header("Vision")]
    public float viewDistance = 12f;
    public float viewHalfAngle = 25f;
    public LayerMask obstructionMask;
    public LayerMask targetMask;

    [Header("Events")]
    public UnityEvent OnPlayerCaught;

    float _timeAtEnd;
    int _dir = 1;
    bool _atEnd;
    float _currentRotation;           // tracks rotation offset
    Quaternion _headInitialLocal;     // keeps the head’s tilt constant
    Quaternion _baseInitialRot;       // base reference rotation

    void Awake()
    {
        if (!head) head = transform;
        _baseInitialRot = transform.rotation;
        _headInitialLocal = head.localRotation;
    }

    void Update()
    {
        SweepUpdate();
        VisionUpdate();
    }

    void SweepUpdate()
    {
        if (_atEnd)
        {
            _timeAtEnd += Time.deltaTime;
            if (_timeAtEnd >= pauseAtEnds)
            {
                _timeAtEnd = 0f;
                _atEnd = false;
                _dir *= -1;
            }
            return;
        }

        float max = sweepAngle * 0.5f;
        float step = sweepSpeed * Time.deltaTime * _dir;
        _currentRotation = Mathf.Clamp(_currentRotation + step, -max, max);

        // ✅ Rotate around the LOCAL Z-axis instead of Y
        transform.rotation = _baseInitialRot * Quaternion.Euler(0f, 0f, _currentRotation);

        // Keep the head’s local tilt fixed
        head.localRotation = _headInitialLocal;

        if (Mathf.Approximately(_currentRotation, -max) || Mathf.Approximately(_currentRotation, max))
            _atEnd = true;
    }

    void VisionUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(head.position, viewDistance, targetMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 to = hits[i].transform.position - head.position;
            float dist = to.magnitude;
            Vector3 dir = to.normalized;

            if (Vector3.Angle(head.forward, dir) <= viewHalfAngle)
            {
                if (!Physics.Raycast(head.position, dir, dist, obstructionMask, QueryTriggerInteraction.Ignore))
                {
                    OnPlayerCaught?.Invoke();
                    break;
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!head) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(head.position, viewDistance);
        Vector3 left = Quaternion.Euler(0f, -viewHalfAngle, 0f) * head.forward;
        Vector3 right = Quaternion.Euler(0f, viewHalfAngle, 0f) * head.forward;
        Gizmos.DrawLine(head.position, head.position + left * viewDistance);
        Gizmos.DrawLine(head.position, head.position + right * viewDistance);
    }
#endif
}
