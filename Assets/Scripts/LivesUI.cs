using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    public TMP_Text label;

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        TrySubscribe();                       // subscribe if manager already exists
        if (LivesManager.Instance)            // initialize text to current value
            UpdateText(LivesManager.Instance.CurrentLives);
        else
            Invoke(nameof(TrySubscribe), 0f); // try again next frame if manager wasn't ready
    }

    void OnDisable()
    {
        if (LivesManager.Instance)
            LivesManager.Instance.OnLivesChanged.RemoveListener(UpdateText);
    }

    void TrySubscribe()
    {
        if (LivesManager.Instance)
            LivesManager.Instance.OnLivesChanged.AddListener(UpdateText);
    }

    void UpdateText(int lives)
    {
        if (label) label.text = $"Lives: {lives}";
    }
}
