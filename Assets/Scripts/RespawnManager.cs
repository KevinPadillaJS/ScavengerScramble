using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    public Transform defaultSpawnPoint;

    private Vector3 activeCheckpoint = Vector3.zero;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(Vector3 position)
    {
        activeCheckpoint = position;
        Debug.Log("Checkpoint set at: " + position);
    }

    public Vector3 GetRespawnPoint()
    {
        // Use the last checkpoint if available, otherwise the default spawn
        if (activeCheckpoint != Vector3.zero)
            return activeCheckpoint;

        if (defaultSpawnPoint != null)
            return defaultSpawnPoint.position;

        // fallback in case nothing is set
        Debug.LogWarning("No checkpoint or default spawn point found! Using Vector3.zero.");
        return Vector3.zero;
    }
}

