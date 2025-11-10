using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FanPush : MonoBehaviour
{
    [Header("Direction & Area")]
    public Transform fanForward;
    public BoxCollider windArea;

    [Header("Force")]
    [Min(0f)] public float maxAcceleration = 25f;
    public AnimationCurve falloff = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Extras")]
    [Min(0f)] public float turbulence = 0f;
    [Min(0f)] public float turbulenceFreq = 1.2f;

    [Header("Filter")]
    public LayerMask affectedLayers = ~0;

    [Header("Occlusion")]
    [Tooltip("What layers count as solid and block the wind (e.g., Default, Environment). Exclude Player.")]
    public LayerMask occluderLayers;
    [Tooltip("How far in front of the fan face to start the ray.")]
    public float faceOffset = 0.05f;
    [Tooltip("Radius for a thicker probe that doesn't slip through gaps.")]
    public float probeRadius = 0.15f;

    void Reset()
    {
        windArea = GetComponent<BoxCollider>();
        windArea.isTrigger = true;
        if (!fanForward) fanForward = transform;
        // Sensible defaults: block everything except Nothing by default.
        occluderLayers = ~0;
    }

    void OnValidate()
    {
        if (windArea) windArea.isTrigger = true;
        if (!fanForward) fanForward = transform;
    }

    void OnTriggerStay(Collider other)
    {
        if (!enabled || !windArea || !fanForward) return;
        if (((1 << other.gameObject.layer) & affectedLayers) == 0) return;

        var rb = other.attachedRigidbody;
        if (!rb) return;

        // Work in the fan's local space so +Z is outward
        Vector3 local = fanForward.InverseTransformPoint(other.transform.position);
        if (local.z <= 0f) return; // ignore behind the fan

        // --- NEW: Occlusion test ---
        Vector3 origin = fanForward.position + fanForward.forward * faceOffset;
        Vector3 target = rb.worldCenterOfMass;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        // If something on occluderLayers is between the fan and the target, skip pushing.
        if (Physics.SphereCast(origin, probeRadius, dir, out RaycastHit hit, dist,
                               occluderLayers, QueryTriggerInteraction.Ignore))
        {
            // If the first thing we hit is NOT this same rigidbody, wind is blocked.
            if (hit.rigidbody != rb) return;
        }
        // --- end occlusion ---

        // Falloff along +Z within the wind box
        float rangeZ = windArea.size.z * windArea.transform.lossyScale.z;
        float t = Mathf.Clamp01(local.z / Mathf.Max(0.0001f, rangeZ));
        float strength = maxAcceleration * falloff.Evaluate(t);

        Vector3 accel = fanForward.forward * strength;

        if (turbulence > 0f)
        {
            float n = Mathf.PerlinNoise(Time.time * turbulenceFreq,
                                        other.transform.GetInstanceID() * 0.123f) - 0.5f;
            Vector3 lateral = fanForward.right * (n * 2f * turbulence);
            accel += lateral;
        }

        rb.AddForce(accel, ForceMode.Acceleration);
    }

    void OnDrawGizmosSelected()
    {
        if (!windArea || !fanForward) return;
        Gizmos.color = new Color(0, 1, 1, 0.15f);
        Gizmos.matrix = windArea.transform.localToWorldMatrix;
        Gizmos.DrawCube(windArea.center, windArea.size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(fanForward.position,
                        fanForward.position + fanForward.forward *
                        (windArea.size.z * windArea.transform.lossyScale.z));
    }
}
