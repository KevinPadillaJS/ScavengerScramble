using UnityEngine;
using TMPro;

public class PlayerHealthManager : MonoBehaviour
{
    public TMP_Text healthText;
    public int maxHearts = 3;

    private int pigeontonHearts;
    private int squeaksHearts;

    void Start()
    {
        pigeontonHearts = maxHearts;
        squeaksHearts = maxHearts;
        UpdateHealthUI();
    }

    // -------------------------------
    // Main damage handler
    // -------------------------------
    public void TakeDamage(string playerTag, int amount)
    {
        if (playerTag == "Player1")
        {
            pigeontonHearts -= amount;
            if (pigeontonHearts < 0) pigeontonHearts = 0;
            UpdateHealthUI();
        }
        else if (playerTag == "Player2")
        {
            squeaksHearts -= amount;
            if (squeaksHearts < 0) squeaksHearts = 0;
            UpdateHealthUI();
        }
    }

    // Old direct damage calls now forward to TakeDamage
    public void DamagePlayer1(int amount)
    {
        TakeDamage("Player1", amount);
    }

    public void DamagePlayer2(int amount)
    {
        TakeDamage("Player2", amount);
    }

    // -------------------------------
    // UI update
    // -------------------------------
    private void UpdateHealthUI()
    {
        if (healthText == null)
            healthText = GameObject.Find("healthText")?.GetComponent<TMP_Text>();

        string pHearts = "";
        string sHearts = "";

        for (int i = 0; i < pigeontonHearts; i++)
            pHearts += "O ";

        for (int i = 0; i < squeaksHearts; i++)
            sHearts += "O ";

        healthText.text = $"Pigeonton: {pHearts.Trim()}\nSqueaks: {sHearts.Trim()}";
    }

    // -------------------------------
    // Getters & Setters (used by respawn)
    // -------------------------------
    public int GetPlayer1Hearts() => pigeontonHearts;
    public int GetPlayer2Hearts() => squeaksHearts;

    public void SetPlayer1Hearts(int value)
    {
        pigeontonHearts = Mathf.Clamp(value, 0, maxHearts);
        UpdateHealthUI();
    }

    public void SetPlayer2Hearts(int value)
    {
        squeaksHearts = Mathf.Clamp(value, 0, maxHearts);
        UpdateHealthUI();
    }
}





