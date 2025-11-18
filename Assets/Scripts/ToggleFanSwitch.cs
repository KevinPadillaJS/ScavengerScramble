using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ToggleFanSwitch : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("One or more FanPush components to toggle.")]
    [SerializeField] private FanPush[] fansToToggle;

    [Header("Interaction")]
    [SerializeField] private KeyCode useKey = KeyCode.E;
    [Tooltip("Only players with this tag can use the switch.")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Optional: the lever or button to animate between on/off rotations.")]
    [SerializeField] private Transform leverVisual;
    [SerializeField] private Vector3 leverOnEuler = new Vector3(0, 0, -25);
    [SerializeField] private Vector3 leverOffEuler = new Vector3(0, 0, 25);

    [Header("Indicator (optional)")]
    [Tooltip("Optional: a renderer on the green button to flip color.")]
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;

    [Header("Events")]
    public UnityEvent onTurnOn;
    public UnityEvent onTurnOff;

    private bool playerInRange = false;
    private bool isOn = true;   // start ‘on’; set in Start() based on first fan

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // make the switch area a trigger
    }

    void Start()
    {
        // If any fan reference exists, adopt its current enabled state
        foreach (var f in fansToToggle)
        {
            if (f != null) { isOn = f.enabled; break; }
        }
        ApplyStateVisuals();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInRange = false;
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(useKey)) Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;

        // Enable/disable wind by toggling the component(s)
        foreach (var f in fansToToggle)
        {
            if (f) f.enabled = isOn;
        }

        ApplyStateVisuals();

        if (isOn) onTurnOn?.Invoke();
        else onTurnOff?.Invoke();
    }

    private void ApplyStateVisuals()
    {
        if (leverVisual)
            leverVisual.localRotation = Quaternion.Euler(isOn ? leverOnEuler : leverOffEuler);

        if (indicatorRenderer)
        {
            // Use a unique material instance so we don't recolor a shared asset
            var mat = indicatorRenderer.material;
            var c = isOn ? onColor : offColor;
            mat.color = c;
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", c);
        }
    }

    // Small quality-of-life gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOn ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
    }
}
