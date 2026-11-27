using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject startingPlatform;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject targetMarkerPrefab;

    [Header("Spawn Distance")]
    [SerializeField] private float minHorizontalDistance = 1.8f;
    [SerializeField] private float maxHorizontalDistance = 3.2f;
    [SerializeField] private float minVerticalDistance = 2.5f;
    [SerializeField] private float maxVerticalDistance = 4f;

    [Header("Platform Size")]
    [SerializeField] private float minWidthMultiplier = 0.9f;
    [SerializeField] private float maxWidthMultiplier = 1.2f;
    [SerializeField] private float minThicknessMultiplier = 0.8f;
    [SerializeField] private float maxThicknessMultiplier = 1.1f;

    [Header("Horizontal Limits")]
    [SerializeField] private float minimumSpawnX = -3.5f;
    [SerializeField] private float maximumSpawnX = 3.5f;

    [Header("Wall References")]
    [SerializeField] private GameObject square1;
    [SerializeField] private GameObject square2;
    [SerializeField] private float wallOverlap = 0.1f;

    private readonly List<GameObject> activePlatforms =
        new List<GameObject>();

    private GameObject currentPlatform;
    private float lastPlatformY;
    private int direction = -1;
    private int successfulJumps;

    public Transform CurrentPlatformTransform
    {
        get
        {
            if (currentPlatform == null)
            {
                return null;
            }

            return currentPlatform.transform;
        }
    }



    public int NextDirection
    {
        get
        {
            return direction;
        }
    }

    private void Awake()
    {
        if (startingPlatform == null)
        {
            Debug.LogError(
                "PlatformSpawner: Starting Platform chưa được gán.");
            return;
        }

        if (platformPrefab == null)
        {
            Debug.LogError(
                "PlatformSpawner: Platform Prefab chưa được gán.");
            return;
        }

        currentPlatform = startingPlatform;
        lastPlatformY = startingPlatform.transform.position.y;
        activePlatforms.Add(startingPlatform);

        SpawnNextPlatform();
    }

    public bool CompleteLanding(
    GameObject landedPlatform,
    float playerCenterX,
    float playerWidth)
    {
        if (landedPlatform == null ||
            landedPlatform == currentPlatform ||
            !activePlatforms.Contains(landedPlatform))
        {
            return false;
        }

        GameObject oldPlatform = currentPlatform;

        currentPlatform = landedPlatform;
        successfulJumps++;

        if (oldPlatform != null)
        {
            activePlatforms.Remove(oldPlatform);
            Destroy(oldPlatform);
        }

        SpawnNextPlatform();

        return true;
    }

    public void DestroyAllPlatforms()
    {
        foreach (GameObject platform in activePlatforms)
        {
            if (platform == null)
            {
                continue;
            }

            Rigidbody2D[] rigidbodies =
                platform.GetComponentsInChildren<Rigidbody2D>();

            if (rigidbodies.Length == 0)
            {
                Destroy(platform);
                continue;
            }

            foreach (Rigidbody2D rigidbody in rigidbodies)
            {
                rigidbody.bodyType = RigidbodyType2D.Dynamic;
                rigidbody.AddForce(
                    Random.insideUnitCircle * 12f,
                    ForceMode2D.Impulse);
            }
        }
    }

    private void SpawnNextPlatform()
    {
        if (platformPrefab == null ||
            square1 == null ||
            square2 == null)
        {
            return;
        }

        direction *= -1;

        float verticalDistance = Random.Range(
            minVerticalDistance,
            maxVerticalDistance);

        float spawnY =
            lastPlatformY + verticalDistance;

        GameObject newPlatform = Instantiate(
            platformPrefab,
            new Vector3(0f, spawnY, 0f),
            Quaternion.identity);

        float widthMultiplier = Random.Range(
            minWidthMultiplier,
            maxWidthMultiplier);

        float thicknessMultiplier = Random.Range(
            minThicknessMultiplier,
            maxThicknessMultiplier);

        newPlatform.transform.localScale = Vector3.Scale(
            newPlatform.transform.localScale,
            new Vector3(
                widthMultiplier,
                thicknessMultiplier,
                1f));


        Collider2D platformCollider =
    newPlatform.GetComponentInChildren<Collider2D>(
        true);

        Collider2D square1Collider =
            square1.GetComponentInChildren<Collider2D>(
                true);

        Collider2D square2Collider =
            square2.GetComponentInChildren<Collider2D>(
                true);

        if (platformCollider == null ||
            square1Collider == null ||
            square2Collider == null)
        {
            Destroy(newPlatform);
            return;
        }

        Collider2D leftWallCollider;
        Collider2D rightWallCollider;

        if (square1Collider.bounds.center.x <
            square2Collider.bounds.center.x)
        {
            leftWallCollider = square1Collider;
            rightWallCollider = square2Collider;
        }
        else
        {
            leftWallCollider = square2Collider;
            rightWallCollider = square1Collider;
        }

        Bounds platformBounds =
            platformCollider.bounds;

        float horizontalCorrection;

        if (direction < 0)
        {
            float targetRightEdge =
                leftWallCollider.bounds.max.x +
                wallOverlap;

            horizontalCorrection =
                targetRightEdge -
                platformBounds.max.x;
        }
        else
        {
            float targetLeftEdge =
                rightWallCollider.bounds.min.x -
                wallOverlap;

            horizontalCorrection =
                targetLeftEdge -
                platformBounds.min.x;
        }

        Vector3 platformPosition =
            newPlatform.transform.position;

        platformPosition.x += horizontalCorrection;

        newPlatform.transform.position =
            platformPosition;

        activePlatforms.Add(newPlatform);
        lastPlatformY = spawnY;

        CreateTargetMarker(newPlatform);
    }

    private void CreateTargetMarker(GameObject platform)
    {
        if (targetMarkerPrefab == null)
        {
            return;
        }

        float platformWidth = GetPlatformWidth(platform);

        GameObject marker = Instantiate(
            targetMarkerPrefab,
            platform.transform);

        marker.transform.localPosition = new Vector3(
            0f,
            0.55f,
            -0.1f);

        marker.transform.localScale = new Vector3(
            Mathf.Min(platformWidth, 1f),
            1f,
            1f);
    }


    private float GetPlatformWidth(GameObject platform)
    {
        Collider2D collider =
            platform.GetComponentInChildren<Collider2D>();

        if (collider == null)
        {
            return 1f;
        }

        return collider.bounds.size.x;
    }
}