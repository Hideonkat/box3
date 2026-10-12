using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Boundary Walls")]
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;

    [Header("Follow Settings")]
    [SerializeField] private float verticalOffset = 2f;
    [SerializeField] private float followSpeed = 5f;

    private Transform target;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 cameraPosition = transform.position;
            float targetY = target.position.y + verticalOffset;

            if (targetY > cameraPosition.y)
            {
                cameraPosition.y = Mathf.Lerp(
                    cameraPosition.y,
                    targetY,
                    followSpeed * Time.deltaTime);

                transform.position = cameraPosition;
            }
        }

        MoveBoundaryWalls();
    }

    private void MoveBoundaryWalls()
    {
        MoveWallToCameraHeight(leftBoundary);
        MoveWallToCameraHeight(rightBoundary);
    }

    private void MoveWallToCameraHeight(
        Transform boundaryWall)
    {
        if (boundaryWall == null)
        {
            return;
        }

        Vector3 wallPosition = boundaryWall.position;
        wallPosition.y = transform.position.y;
        boundaryWall.position = wallPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}