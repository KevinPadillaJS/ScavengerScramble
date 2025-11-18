using UnityEngine;

public class CameraFollowZoom : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("Camera Movement Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Zoom Settings")]
    public float minZoom = -10f;  // closest Z
    public float maxZoom = -20f;  // furthest Z
    public float zoomLimiter = 5f; // how fast it zooms

    [Header("Fixed Y Position")]
    public float fixedY = 5f; // lock Y height

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // Midpoint between players (X only)
        float midpointX = (player1.position.x + player2.position.x) / 2f;

        // Desired camera position (fixed Y)
        Vector3 desiredPosition = new Vector3(midpointX, fixedY, transform.position.z);

        // Smoothly move the camera horizontally
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Calculate distance between players to adjust zoom (Z)
        float distance = Mathf.Abs(player1.position.x - player2.position.x);
        float targetZ = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);

        // Smoothly adjust zoom
        Vector3 zoomedPosition = new Vector3(transform.position.x, transform.position.y, targetZ);
        transform.position = Vector3.Lerp(transform.position, zoomedPosition, smoothSpeed);
    }
}



