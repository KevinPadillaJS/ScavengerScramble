using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerHealthManager healthManager;
    private string playerTag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthManager = FindFirstObjectByType<PlayerHealthManager>();
        playerTag = gameObject.tag; // Player1 or Player2
    }

    // Call this method whenever the player takes damage
    public void TakeDamage(int amount)
    {
        if (playerTag == "Player1")
            healthManager.DamagePlayer1(amount);
        else
            healthManager.DamagePlayer2(amount);

        CheckIfDead();
    }


    private void CheckIfDead()
    {
        int currentHearts = (playerTag == "Player1")
            ? healthManager.GetPlayer1Hearts()
            : healthManager.GetPlayer2Hearts();

        if (currentHearts <= 0)
        {
            RespawnBothPlayers();
        }
    }

    private void RespawnBothPlayers()
    {
        // Restore hearts
        healthManager.SetPlayer1Hearts(3);
        healthManager.SetPlayer2Hearts(3);

        Vector3 respawnPoint = RespawnManager.Instance.GetRespawnPoint();

        // Move players by teleporting the parent transform only
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player1)
            player1.transform.position = respawnPoint + new Vector3(-1f, 0.5f, 0f);

        if (player2)
            player2.transform.position = respawnPoint + new Vector3(1f, 0.5f, 0f);

        PushableBox[] boxes = FindObjectsOfType<PushableBox>();
        foreach (PushableBox b in boxes)
        {
            b.ResetBox();
        }


    }

}


