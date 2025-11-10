using UnityEngine;

public class CaughtUI : MonoBehaviour
{
    public GameObject caughtPanel;
    public float lingerTime = 1.5f; // how long it stays visible

    private float timer;

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f && caughtPanel != null)
            {
                caughtPanel.SetActive(false);
            }
        }
    }

    // Called when player is caught
    public void ShowCaught()
    {
        if (caughtPanel != null)
        {
            caughtPanel.SetActive(true);
            timer = lingerTime;
        }
    }
}
