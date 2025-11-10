using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Link to your logic (optional)")]
    public SecurityCamera cameraLogic;      // drag your Base (with SecurityCamera.cs) here

    [Header("Cone Settings")]
    public float viewDistance = 12f;
    [Tooltip("Half-angle of the cone (± this angle).")]
    public float viewHalfAngle = 25f;
    [Range(6, 128)] public int segments = 40;
    [Tooltip("Small lift above the floor to avoid z-fighting.")]
    public float yOffset = 0.02f;

    [Header("Occlusion Clipping")]
    [Tooltip("Clip the cone visually using raycasts so it stops at obstacles.")]
    public bool clipToObstacles = true;
    [Tooltip("Layers that block the cone (same as your detection's obstructionMask).")]
    public LayerMask obstructionMask;

    Mesh _mesh;

    void OnEnable()
    {
        EnsureMesh();
        Rebuild();
    }

    void OnValidate()
    {
        EnsureMesh();
        Rebuild();
    }

    void LateUpdate()
    {
        // keep in sync with SecurityCamera values (if linked)
        if (cameraLogic != null)
        {
            if (!Mathf.Approximately(viewDistance, cameraLogic.viewDistance) ||
                !Mathf.Approximately(viewHalfAngle, cameraLogic.viewHalfAngle))
            {
                viewDistance = cameraLogic.viewDistance;
                viewHalfAngle = cameraLogic.viewHalfAngle;
            }
        }

        // Rebuild every frame (cheap at ~40 segments). You can throttle if needed.
        Rebuild();
    }

    void EnsureMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh { name = "VisionConeMesh" };
            _mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }
    }

    public void Rebuild()
    {
        if (segments < 6) segments = 6;

        int vertCount = segments + 2;      // center + (segments + 1)
        int triCount  = segments;

        Vector3[] v = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] tris   = new int[triCount * 3];

        // Center at the lens plane
        Vector3 origin = transform.position;

        v[0] = new Vector3(0f, yOffset, 0f);
        uv[0] = new Vector2(0.5f, 0f);

        float start = -viewHalfAngle;
        float step = (viewHalfAngle * 2f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float ang = start + step * i;
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;

            float dist = viewDistance;

            if (clipToObstacles)
            {
                // Raycast from the lens (slightly lifted to avoid immediate ground hit)
                Vector3 castOrigin = origin + Vector3.up * Mathf.Max(0.001f, yOffset);
                if (Physics.Raycast(castOrigin, transform.TransformDirection(dir), out RaycastHit hit, viewDistance, obstructionMask, QueryTriggerInteraction.Ignore))
                {
                    dist = hit.distance;
                }
            }

            // Build in local space (mesh is a flat fan at yOffset)
            Vector3 local = new Vector3(dir.x * dist, yOffset, dir.z * dist);
            v[i + 1] = local;
            uv[i + 1] = new Vector2((float)i / segments, 1f);

            if (i < segments)
            {
                int t = i * 3;
                tris[t + 0] = 0;
                tris[t + 1] = i + 1;
                tris[t + 2] = i + 2;
            }
        }

        _mesh.Clear();
        _mesh.vertices   = v;
        _mesh.uv         = uv;
        _mesh.triangles  = tris;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}
