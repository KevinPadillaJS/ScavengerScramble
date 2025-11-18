using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SecurityCamera : MonoBehaviour
{
    [Header("References")]
    public Transform head;

    [Header("Sweep")]
    public float sweepAngle = 60f;
    public float sweepSpeed = 30f;
    public float pauseAtEnds = 0.35f;

    [Header("Vision")]
    public float viewDistance = 12f;
    public float viewHalfAngle = 25f;
    public LayerMask obstructionMask;
    public LayerMask targetMask;

    [Header("Caught")]
    public int damage = 1;
    public float hitCooldown = 1f;            // seconds between hits on the same target
    public UnityEvent OnPlayerCaught;

    // --- private state ---
    float _timeAtEnd;
    int _dir = 1;
    bool _atEnd;
    float _currentRotation;
    Quaternion _headInitialLocal;
    Quaternion _baseInitialRot;
    readonly Dictionary<Transform, float> _nextHitTime = new();

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

        // rotate around local Z (your choice)
        transform.rotation = _baseInitialRot * Quaternion.Euler(0f, 0f, _currentRotation);
        head.localRotation = _headInitialLocal;

        if (Mathf.Approximately(_currentRotation, -max) || Mathf.Approximately(_currentRotation, max))
            _atEnd = true;
    }

    void VisionUpdate()
    {
        var hits = Physics.OverlapSphere(head.position, viewDistance, targetMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 to = hits[i].transform.position - head.position;
            float dist = to.magnitude;
            Vector3 dir = to.normalized;

            if (Vector3.Angle(head.forward, dir) <= viewHalfAngle)
            {
                // clear line of sight?
                if (!Physics.Raycast(head.position, dir, dist, obstructionMask, QueryTriggerInteraction.Ignore))
                {
                    // cooldown per target
                    Transform t = hits[i].transform;
                    if (!_nextHitTime.TryGetValue(t, out float readyAt) || Time.time >= readyAt)
                    {
                        // APPLY DAMAGE HERE
                        hits[i].GetComponent<PlayerRespawn>()?.TakeDamage(damage);

                        _nextHitTime[t] = Time.time + hitCooldown;
                        OnPlayerCaught?.Invoke();
                    }
                    // no break; so it can hit both players in the same frame if visible
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
