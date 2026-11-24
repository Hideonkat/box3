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

    public int CompleteLanding(
        GameObject landedPlatform,
        float playerCenterX,
        float playerWidth)
    {
        if (landedPlatform == null ||
            landedPlatform == currentPlatform ||
            !activePlatforms.Contains(landedPlatform))
        {
            return 0;
        }

        int points = CalculateScore(
            landedPlatform,
            playerCenterX,
            playerWidth);

        GameObject oldPlatform = currentPlatform;

        currentPlatform = landedPlatform;
        successfulJumps++;

        if (oldPlatform != null)
        {
            activePlatforms.Remove(oldPlatform);
            Destroy(oldPlatform);
        }

        SpawnNextPlatform();

        return points;
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
        direction *= -1;

        float horizontalDistance = Random.Range(
        minHorizontalDistance,
        maxHorizontalDistance);

        float verticalDistance = Random.Range(
            minVerticalDistance,
            maxVerticalDistance);

        float spawnX = direction * horizontalDistance;

        spawnX = Mathf.Clamp(
            spawnX,
            minimumSpawnX,
            maximumSpawnX);

        float spawnY =
            lastPlatformY + verticalDistance;

        Vector3 spawnPosition = new Vector3(
            spawnX,
            spawnY,
            0f);

        GameObject newPlatform = Instantiate(
            platformPrefab,
            spawnPosition,
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

    private int CalculateScore(
        GameObject platform,
        float playerCenterX,
        float playerWidth)
    {
        float platformWidth = GetPlatformWidth(platform);
        float targetWidth = Mathf.Min(
            playerWidth,
            platformWidth);

        float platformCenterX =
            platform.transform.position.x;

        float centerOffset = Mathf.Abs(
            playerCenterX - platformCenterX);

        float maximumOffset = Mathf.Max(
            platformWidth * 0.5f,
            targetWidth * 0.5f);

        float accuracy = Mathf.Clamp01(
            1f - centerOffset / maximumOffset);

        return Mathf.RoundToInt(accuracy * 1000f);
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