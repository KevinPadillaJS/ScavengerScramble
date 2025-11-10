using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ToggleSwitch : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Scripts like SecurityCamera, VisionCone, Light, etc.")]
    [SerializeField] private Behaviour[] behavioursToToggle;   // e.g. SecurityCamera, VisionCone, Light, etc.
    [Tooltip("Renderers to show/hide (optional).")]
    [SerializeField] private Renderer[] renderersToToggle;     // e.g. VisionCone's MeshRenderer
    [Tooltip("Whole GameObjects to enable/disable (optional).")]
    [SerializeField] private GameObject[] objectsToToggle;     // e.g. the VisionCone GameObject

    [Header("Interaction")]
    [SerializeField] private KeyCode useKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform leverVisual;
    [SerializeField] private Vector3 leverOnEuler = new Vector3(0, 0, -25);
    [SerializeField] private Vector3 leverOffEuler = new Vector3(0, 0, 25);

    [Header("Indicator (optional)")]
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;

    [Header("Events")]
    public UnityEvent onTurnOn;
    public UnityEvent onTurnOff;

    bool playerInRange;
    bool isOn = true;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Start()
    {
        // Try to adopt an initial state from the first valid target
        foreach (var b in behavioursToToggle) { if (b) { isOn = b.enabled; break; } }
        if (renderersToToggle != null && renderersToToggle.Length > 0 && renderersToToggle[0])
            isOn = renderersToToggle[0].enabled;

        if (objectsToToggle != null && objectsToToggle.Length > 0 && objectsToToggle[0])
            isOn = objectsToToggle[0].activeSelf;

        ApplyAllTargets();
        ApplyStateVisuals();
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag(playerTag)) playerInRange = true; }
    void OnTriggerExit(Collider other)  { if (other.CompareTag(playerTag)) playerInRange = false; }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(useKey)) Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;
        ApplyAllTargets();
        ApplyStateVisuals();
        if (isOn) onTurnOn?.Invoke(); else onTurnOff?.Invoke();
    }

    void ApplyAllTargets()
    {
        if (behavioursToToggle != null)
            foreach (var b in behavioursToToggle) if (b) b.enabled = isOn;

        if (renderersToToggle != null)
            foreach (var r in renderersToToggle) if (r) r.enabled = isOn;

        if (objectsToToggle != null)
            foreach (var go in objectsToToggle) if (go) go.SetActive(isOn);
    }

    void ApplyStateVisuals()
    {
        if (leverVisual)
            leverVisual.localRotation = Quaternion.Euler(isOn ? leverOnEuler : leverOffEuler);

        if (indicatorRenderer)
        {
            var mat = indicatorRenderer.material;
            var c = isOn ? onColor : offColor;
            mat.color = c;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOn ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
    }
}
