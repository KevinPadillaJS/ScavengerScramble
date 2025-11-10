using UnityEngine;
using UnityEngine.Events;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Config")]
    public int maxLives = 3;

    [Header("References (optional)")]
    public Transform player;          // assign your player here
    public Transform respawnPoint;    // where to put them after losing a life

    [Header("Events")]
    public UnityEvent<int> OnLivesChanged;  // current lives
    public UnityEvent OnGameOver;

    int currentLives;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentLives = Mathf.Max(1, maxLives);
        OnLivesChanged?.Invoke(currentLives);
        // Optional: DontDestroyOnLoad(gameObject);
    }

    public int CurrentLives => currentLives;

    public void AddLife(int amount = 1)
    {
        int prev = currentLives;
        currentLives = Mathf.Clamp(currentLives + amount, 0, maxLives);
        if (currentLives != prev) OnLivesChanged?.Invoke(currentLives);
    }

    public void LoseLife(int amount = 1)
    {
        if (currentLives <= 0) return;

        currentLives -= amount;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            // Game over
            OnGameOver?.Invoke();
        }
        else
        {
            // Simple respawn
            if (player && respawnPoint)
            {
                player.position = respawnPoint.position;

                // clear velocity if using Rigidbody
                var rb = player.GetComponent<Rigidbody>();
                if (rb) rb.linearVelocity = Vector3.zero;  // or rb.velocity
            }
        }
    }

    public void ResetLives()
    {
        currentLives = Mathf.Max(1, maxLives);
        OnLivesChanged?.Invoke(currentLives);
    }
}
