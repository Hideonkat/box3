using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float verticalOffset = 2f;
    [SerializeField] private float followSpeed = 5f;

    private void Awake()
    {
        if (target == null)
        {
            PlayerController player =
                FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 cameraPosition =
            transform.position;

        float targetY =
            target.position.y + verticalOffset;

        if (targetY <= cameraPosition.y)
        {
            return;
        }

        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            targetY,
            followSpeed * Time.deltaTime);

        transform.position = cameraPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}