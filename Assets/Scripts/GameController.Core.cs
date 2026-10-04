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
    private int direction = 1;

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
        SpawnNextPlatform();
    }

    private void Update()
    {
        if (isWaitingForMeteor)
        {
            return;
        }

        HandleTouchInput();
        HandleLanding();
        HandleCharge();
        HandleScreenExit();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0 || !isGrounded)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            isCharging = true;
            currentCharge = 0f;
        }
        else if (touch.phase == TouchPhase.Ended && isCharging)
        {
            isCharging = false;
            LaunchPlayer();
        }
    }

    private void HandleLanding()
    {
        if (isGrounded &&
            !hasScoredThisJump &&
            playerRb.linearVelocity.sqrMagnitude < 0.01f)
        {
            AddScore();
            hasScoredThisJump = true;
            SpawnNextPlatform();
        }
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