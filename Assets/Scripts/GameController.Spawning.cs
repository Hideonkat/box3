using System.Collections.Generic;
using UnityEngine;

public partial class GameController
{
    [Header("Platform Spawner")]
    public GameObject platformPrefab;
    public float spawnYDistance = 3.2f;

    private readonly List<GameObject> activePlatforms = new List<GameObject>();
    private float lastPlatformY = -2f;

    public void SpawnNextPlatform()
    {
        if (platformPrefab == null)
        {
            return;
        }

        if (activePlatforms.Count >= 2)
        {
            Destroy(activePlatforms[0]);
            activePlatforms.RemoveAt(0);
        }

        direction = Random.value > 0.5f ? 1 : -1;

        float spawnX = direction * 2.2f;
        float spawnY = lastPlatformY + spawnYDistance;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        GameObject newPlatform = Instantiate(
            platformPrefab,
            spawnPosition,
            Quaternion.identity);

        activePlatforms.Add(newPlatform);
        lastPlatformY = spawnY;
    }
}