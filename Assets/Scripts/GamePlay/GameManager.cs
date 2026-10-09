using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlatformSpawner platformSpawner;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Fall Settings")]
    [SerializeField] private float fallDistanceBelowCamera = 5f;

    [Header("Meteor Settings")]
    [SerializeField] private float meteorHeightAboveCamera = 5f;
    [SerializeField] private float gameOverDelay = 1.5f;

    private Camera mainCamera;
    private bool isMeteorState;
    private bool isGameOver;
    private bool meteorImpactHandled;

    public bool IsMeteorState
    {
        get
        {
            return isMeteorState;
        }
    }

    public bool IsGameOver
    {
        get
        {
            return isGameOver;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (platformSpawner == null)
        {
            platformSpawner =
                FindFirstObjectByType<PlatformSpawner>();
        }

        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUI>();
        }

        if (cameraFollow == null)
        {
            cameraFollow =
                FindFirstObjectByType<CameraFollow>();
        }
    }

    private void Update()
    {
        if (isGameOver || player == null)
        {
            return;
        }

        if (isMeteorState)
        {
            return;
        }

        if (IsBelowCamera())
        {
            HandleFallGameOver();
            return;
        }

        if (IsHighAboveCamera())
        {
            StartMeteorState();
        }
    }

    public void HandleSuccessfulLanding(int points)
    {
        if (isGameOver || isMeteorState)
        {
            return;
        }

        if (gameUI != null)
        {
            gameUI.AddScore(points);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(player.transform);
        }
    }

    public void UpdateCharge(float value)
    {
        if (gameUI != null)
        {
            gameUI.SetCharge(value);
        }
    }

    public void StartMeteorState()
    {
        if (isMeteorState || isGameOver || player == null)
        {
            return;
        }

        isMeteorState = true;

        player.SetMeteorState(true);

        int direction = 1;

        if (platformSpawner != null)
        {
            direction = platformSpawner.NextDirection;
        }

        player.LaunchAsMeteor(direction);
    }

    public void HandleMeteorImpact()
    {
        if (!isMeteorState || meteorImpactHandled)
        {
            return;
        }

        meteorImpactHandled = true;
        isMeteorState = false;

        if (player != null)
        {
            player.DisablePhysics();
        }

        if (platformSpawner != null)
        {
            platformSpawner.DestroyAllPlatforms();
        }

        if (gameUI != null)
        {
            gameUI.HideGameplayUI();
        }

        Invoke(
            nameof(ShowGameOver),
            gameOverDelay);
    }

    private void HandleFallGameOver()
    {
        if (player != null)
        {
            player.DisablePhysics();
        }

        ShowGameOver();
    }

    private void ShowGameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        if (gameUI != null)
        {
            gameUI.ShowGameOver();
        }
    }

    private bool IsBelowCamera()
    {
        if (mainCamera == null)
        {
            return player.transform.position.y < -10f;
        }

        float cameraBottom =
            mainCamera.transform.position.y -
            mainCamera.orthographicSize;

        return player.transform.position.y <
            cameraBottom - fallDistanceBelowCamera;
    }

    private bool IsHighAboveCamera()
    {
        if (mainCamera == null)
        {
            return player.transform.position.y > 15f;
        }

        float cameraTop =
            mainCamera.transform.position.y +
            mainCamera.orthographicSize;

        return player.transform.position.y >
            cameraTop + meteorHeightAboveCamera;
    }
}