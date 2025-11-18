using UnityEngine;
using UnityEngine.Events;
#if TMP_PRESENT
using TMPro;
#endif

[RequireComponent(typeof(Collider))]
public class ToggleSwitch : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Scripts like SecurityCamera, VisionCone, Light, etc.")]
    [SerializeField] private Behaviour[] behavioursToToggle;
    [Tooltip("Renderers to show/hide (optional).")]
    [SerializeField] private Renderer[] renderersToToggle;
    [Tooltip("Whole GameObjects to enable/disable (optional).")]
    [SerializeField] private GameObject[] objectsToToggle;

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

    [Header("UI Prompt (optional)")]
    [Tooltip("Root GameObject of your prompt (panel/text). Set active when in range.")]
    [SerializeField] private GameObject promptRoot;
#if TMP_PRESENT
    [Tooltip("Optional: TextMeshProUGUI to show dynamic text.")]
    [SerializeField] private TextMeshProUGUI promptText;
#endif

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
        ShowPrompt(false);  // hide at start
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        ShowPrompt(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        ShowPrompt(false);
    }

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

        // refresh prompt text while still in range
        if (playerInRange) ShowPrompt(true);

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

    void ShowPrompt(bool show)
    {
        if (promptRoot) promptRoot.SetActive(show);
#if TMP_PRESENT
        if (show && promptText)
            promptText.text = $"Press {useKey} to turn {(isOn ? "off" : "on")}";
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOn ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
    }
}
