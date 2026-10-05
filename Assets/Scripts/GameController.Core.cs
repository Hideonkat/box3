using UnityEngine;

public partial class GameController 
{
    [Header("Player Settings")]
    public Transform player;
    public Rigidbody2D playerRb;
    public float jumpAngle = 75f;
    public float minForce = 8f;
    public float maxForce = 22f;
    public float chargeSpeed = 1.2f;

    [Header("Visual Effects")]
    public ParticleSystem jumpExplosionFX;
    public ParticleSystem meteorImpactFX;
    public GameObject meteorFlameTrail;

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    private float currentCharge;
    private bool isCharging;
    private bool isGrounded = true;
    private bool isMeteorState;
    private bool isWaitingForMeteor;
    private bool hasScoredThisJump = true;

    [Header("Game Over")]
    public float fallLimit = -8f;
    private bool isGameOver;

    [Header("Camera")]
    public Camera gameCamera;
    public float cameraFollowOffsetY = 2f;
    public float cameraFollowSpeed = 5f;

    private void Start()
    {
        Application.targetFrameRate = 60;

        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }

        if (meteorFlameTrail)
        {
            meteorFlameTrail.SetActive(false);
        }

        ResetScore();

        if (startingPlatform != null) 
        {
            currentPlatform = startingPlatform;
            activePlatforms.Add(startingPlatform);
            lastPlatformY = startingPlatform.transform.position.y;
        }

        SpawnNextPlatform();

        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (isWaitingForMeteor || isGameOver)
        {
            return;
        }

        HandleTouchInput();
        HandleLanding();
        HandleCharge();
        HandleScreenExit();
        HandleFall();
        HandleCameraFollow();
    }

    private void HandleCameraFollow()
    {
        if (gameCamera == null || player == null)
        {
            return;
        }

        float targetY = player.position.y + cameraFollowOffsetY;
        Vector3 cameraPosition = gameCamera.transform.position;

        if (targetY > cameraPosition.y)
        {
            cameraPosition.y = Mathf.Lerp(
                cameraPosition.y,
                targetY,
                cameraFollowSpeed * Time.deltaTime);

            gameCamera.transform.position = cameraPosition;
        }
    }

    private void HandleFall()
    {
        if (player == null)
        {
            return;
        }

        if (player.position.y < fallLimit)
        {
            isGameOver = true;
            isCharging = false;

            if (playerRb)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.simulated = false;
            }

            ShowGameOver();
        }
    }

    private void HandleTouchInput()
    {
        if (!isGrounded)
        {
            return;
        }

        bool pressStarted = false;
        bool pressEnded = false;

#if UNITY_EDITOR
        pressStarted = Input.GetMouseButtonDown(0);
        pressEnded = Input.GetMouseButtonUp(0);
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            pressStarted = touch.phase == TouchPhase.Began;
            pressEnded = touch.phase == TouchPhase.Ended;
        }

        if (pressStarted)
        {
            isCharging = true;
            currentCharge = 0f;
        }
        else if (pressEnded && isCharging)
        {
            isCharging = false;
            LaunchPlayer();
        }
    }

    private void HandleLanding()
    {
        if (!isGrounded ||
            hasScoredThisJump ||
            playerRb.linearVelocity.sqrMagnitude >= 0.01f)
        {
            return;
        }

        RemovePreviousPlatform();

        AddScore();
        hasScoredThisJump = true;
        SpawnNextPlatform();
    }

    private void HandleCharge()
    {
        if (!isCharging)
        {
            return;
        }

        currentCharge += Time.deltaTime * chargeSpeed;
        currentCharge = Mathf.Clamp01(currentCharge);

        UpdateChargeBar(currentCharge);
    }

    private void HandleScreenExit()
    {
        if (player.position.y > 10f &&
            !isMeteorState &&
            !isWaitingForMeteor)
        {
            StartCoroutine(MeteorRoutine());
        }
    }

    private void LaunchPlayer()
    {
        isGrounded = false;
        hasScoredThisJump = false;

        float force = Mathf.Lerp(minForce, maxForce, currentCharge);
        float radians = jumpAngle * Mathf.Deg2Rad;
        Vector2 jumpDirection = new Vector2(
            Mathf.Cos(radians) * direction,
            Mathf.Sin(radians));

        playerRb.linearVelocity = jumpDirection * force;
        playerRb.AddTorque(-direction * 5f, ForceMode2D.Impulse);

        if (jumpExplosionFX)
        {
            jumpExplosionFX.Play();
        }

        UpdateChargeBar(0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMeteorState)
        {
            TriggerWorldShatter();
            return;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
            landedPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
    }
}