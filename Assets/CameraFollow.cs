using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float followThreshold = 2f;

    private Vector3 offset;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned!");
            return;
        }
        
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;
        float distance = Vector3.Distance(transform.position, desiredPosition);

        if (distance > followThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, moveSpeed * Time.deltaTime);
        }
    }
}
