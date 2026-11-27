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

    [Header("Attack Progression")]
    [SerializeField] private float firstPlusMultiplier = 1.5f;
    [SerializeField] private float plusMultiplierStep = 0.5f;

    private int totalAttack;
    private float currentPlusMultiplier = 1f;
    private bool hasPlusChain;

    private Camera mainCamera;
    private bool isGameOver;

    public int TotalAttack
    {
        get
        {
            return totalAttack;
        }
    }

    public float CurrentPlusMultiplier
    {
        get
        {
            return currentPlusMultiplier;
        }
    }

    public void RegisterSuccessfulJump(bool isPlusJump)
    {
        totalAttack++;

        if (!isPlusJump)
        {
            hasPlusChain = false;
            currentPlusMultiplier = 1f;
        }
        else if (!hasPlusChain)
        {
            hasPlusChain = true;
            currentPlusMultiplier =
                firstPlusMultiplier;
        }
        else
        {
            currentPlusMultiplier +=
                plusMultiplierStep;
        }

        Debug.Log(
            $"Jump success: " +
            $"ATK = {totalAttack}, " +
            $"Plus multiplier = x{currentPlusMultiplier}");
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

    private void Start()
    {
        if (cameraFollow != null &&
            platformSpawner != null)
        {
            cameraFollow.SetTarget(
                platformSpawner.CurrentPlatformTransform);
        }
    }

    private void Update()
    {
        if (isGameOver || player == null)
        {
            return;
        }

        if (IsBelowCamera())
        {
            HandleFallGameOver();
        }
    }

    public void HandleSuccessfulLanding()
    {
        if (isGameOver)
        {
            return;
        }

        if (cameraFollow != null &&
            platformSpawner != null)
        {
            cameraFollow.SetTarget(
                platformSpawner.CurrentPlatformTransform);
        }
    }

    public void UpdateCharge(float value)
    {
        if (gameUI != null)
        {
            gameUI.SetCharge(value);
        }
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
}