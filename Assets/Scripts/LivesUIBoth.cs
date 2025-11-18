using UnityEngine;
using TMPro;

public class LivesUIBoth : MonoBehaviour
{
    [SerializeField] LivesManager player1;
    [SerializeField] LivesManager player2;
    [SerializeField] TMP_Text label;

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (player1) player1.OnLivesChanged.AddListener(UpdateText);
        if (player2) player2.OnLivesChanged.AddListener(UpdateText);
        UpdateText(0); // initialize
    }

    void OnDisable()
    {
        // Unsubscribe when disabled (scene unload, deactivate, etc.)
        if (player1) player1.OnLivesChanged.RemoveListener(UpdateText);
        if (player2) player2.OnLivesChanged.RemoveListener(UpdateText);
    }

    void OnDestroy()
    {
        // Extra safety for destruction order during playmode exit / scene unload
        if (player1) player1.OnLivesChanged.RemoveListener(UpdateText);
        if (player2) player2.OnLivesChanged.RemoveListener(UpdateText);
    }

    void UpdateText(int _)
    {
        // Guard in case the event fires during teardown
        if (!this || !label) return;

        int l1 = player1 ? player1.CurrentLives : 0;
        int l2 = player2 ? player2.CurrentLives : 0;
        label.text = $"Pigeon: {l1}   Rat: {l2}";
    }
}
