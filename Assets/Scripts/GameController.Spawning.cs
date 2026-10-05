using System.Collections.Generic;
using UnityEngine;

public partial class GameController
{
    [Header("Platform Spawner")]
    public GameObject startingPlatform;
    public GameObject platformPrefab;

    [Header("Platform Difficulty")]
    public float minHorizontalDistance = 1.8f;
    public float maxHorizontalDistance = 3.2f;
    public float minVerticalDistance = 2.5f;
    public float maxVerticalDistance = 4f;
    public float minWidthMultiplier = 0.7f;
    public float maxWidthMultiplier = 1.2f;
    public float minHeightMultiplier = 0.8f;
    public float maxHeightMultiplier = 1.1f;

    private readonly List<GameObject> activePlatforms =
        new List<GameObject>();

    private float lastPlatformY = -2f;
    private int direction = -1;

    private GameObject currentPlatform;
    private GameObject landedPlatform;

    private void RemovePreviousPlatform()
    {
        if (currentPlatform == null ||
            currentPlatform == landedPlatform)
        {
            return;
        }

        activePlatforms.Remove(currentPlatform);
        Destroy(currentPlatform);

        currentPlatform = landedPlatform;
    }

    public void SpawnNextPlatform()
    {
        if (platformPrefab == null)
        {
            return;
        }


        // luan phien trai fai

        direction *= -1;

        float difficulty = Mathf.Clamp01(score / 20f);

        float horizontalDistance = Random.Range(
            Mathf.Lerp(minHorizontalDistance, 2.5f, difficulty),
            Mathf.Lerp(maxHorizontalDistance, 4.2f, difficulty));

        float verticalDistance = Random.Range(
            Mathf.Lerp(minVerticalDistance, 2.8f, difficulty),
            Mathf.Lerp(maxVerticalDistance, 4.8f, difficulty));

        float spawnX = direction * horizontalDistance;
        float spawnY = lastPlatformY + verticalDistance;

        Vector3 spawnPosition = new Vector3(
            spawnX,
            spawnY,
            0f);

        GameObject newPlatform = Instantiate(
            platformPrefab,
            spawnPosition,
            Quaternion.identity);

        float widthMultiplier = Random.Range(
            Mathf.Lerp(maxWidthMultiplier, 1f, difficulty),
            Mathf.Lerp(minWidthMultiplier, 0.65f, difficulty));

        float heightMultiplier = Random.Range(
            minHeightMultiplier,
            maxHeightMultiplier);

        newPlatform.transform.localScale = Vector3.Scale(
            newPlatform.transform.localScale,
            new Vector3(
                widthMultiplier,
                heightMultiplier,
                1f));

        activePlatforms.Add(newPlatform);
        lastPlatformY = spawnY;
    }
}