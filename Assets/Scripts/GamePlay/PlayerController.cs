using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private PlatformSpawner platformSpawner;
    [SerializeField] private GameManager gameManager;

    [Header("Jump Settings")]
    [SerializeField] private float jumpAngle = 75f;
    [SerializeField] private float minimumForce = 8f;
    [SerializeField] private float maximumForce = 22f;
    [SerializeField] private float chargeSpeed = 1.2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private float bounceSpeed = 5f;
    [SerializeField] private float highWallTolerance = 0.03f;
    [SerializeField] private float playerHeight = 0.22f;

    [Header("Wall References")]
    [SerializeField] private GameObject square1;
    [SerializeField] private GameObject square2;
    [SerializeField] private float highWallHeight = 0.221f;

    private GameObject bouncedPlatform;

    private Collider2D playerCollider;
    private float chargeAmount;
    private bool isGrounded = true;
    private bool isCharging;
    private bool hasJumped;
    private bool touchedHighWall;

    private bool IsWallCollision(Collision2D collision)
    {
        if (square1 == null || square2 == null)
        {
            return false;
        }

        Transform collisionTransform =
            collision.transform;

        bool isSquare1 =
            collisionTransform == square1.transform ||
            collisionTransform.IsChildOf(square1.transform);

        bool isSquare2 =
            collisionTransform == square2.transform ||
            collisionTransform.IsChildOf(square2.transform);

        return isSquare1 || isSquare2;
    }

    private void CheckHighWallContact(
    Collision2D collision)
    {
        if (playerCollider == null)
        {
            return;
        }

        float highWallLimit =
            playerCollider.bounds.min.y +
            highWallHeight;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            bool isSideContact =
                Mathf.Abs(contact.normal.x) > 0.5f;

            bool isAbovePlayerHeight =
                contact.point.y > highWallLimit;

            if (isSideContact &&
                isAbovePlayerHeight)
            {
                touchedHighWall = true;

                Debug.Log(
                    $"High wall detected | " +
                    $"Contact Y: {contact.point.y:F3} | " +
                    $"Limit Y: {highWallLimit:F3}");

                return;
            }
        }
    }

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        playerCollider = GetComponent<Collider2D>();

    }

    private void Update()
    {
        if (gameManager != null &&
    gameManager.IsGameOver)
        {
            return;
        }

        HandleInput();
        UpdateCharge();
    }

    private void HandleInput()
    {
        if (!isGrounded)
        {
            return;
        }

        bool pressStarted = false;
        bool pressEnded = false;

        if (Mouse.current != null)
        {
            pressStarted =
                Mouse.current.leftButton.wasPressedThisFrame;

            pressEnded =
                Mouse.current.leftButton.wasReleasedThisFrame;
        }

        if (Touchscreen.current != null)
        {
            var touch =
                Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                pressStarted = true;
            }

            if (touch.press.wasReleasedThisFrame)
            {
                pressEnded = true;
            }
        }

        if (pressStarted)
        {
            isCharging = true;
            chargeAmount = 0f;
        }

        if (pressEnded && isCharging)
        {
            isCharging = false;
            Launch();
        }
    }

    private void UpdateCharge()
    {
        if (!isCharging)
        {
            return;
        }

        chargeAmount +=
            Time.deltaTime * chargeSpeed;

        chargeAmount = Mathf.Clamp01(chargeAmount);

        if (gameManager != null)
        {
            gameManager.UpdateCharge(chargeAmount);
        }
    }

    private void Launch()
    {
        if (platformSpawner == null ||
            playerRigidbody == null)
        {
            return;
        }

        isGrounded = false;
        hasJumped = true;
        touchedHighWall = false;

        float force = Mathf.Lerp(
            minimumForce,
            maximumForce,
            chargeAmount);

        float radians =
            jumpAngle * Mathf.Deg2Rad;

        int direction =
            platformSpawner.NextDirection;

        Vector2 jumpDirection = new Vector2(
            Mathf.Cos(radians) * direction,
            Mathf.Sin(radians));

        playerRigidbody.linearVelocity =
            jumpDirection * force;

        playerRigidbody.AddTorque(
            -direction * 5f,
            ForceMode2D.Impulse);

        if (jumpEffect != null)
        {
            jumpEffect.Play();
        }

        if (gameManager != null)
        {
            gameManager.UpdateCharge(0f);
        }

        chargeAmount = 0f;
    }

    private void OnCollisionEnter2D(
    Collision2D collision)
    {
        if (IsWallCollision(collision))
        {
            if (hasJumped)
            {
                CheckHighWallContact(collision);
            }

            return;
        }

        if (!collision.gameObject.CompareTag("Platform"))
        {
            return;
        }

        bool landedOnTop = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                landedOnTop = true;
                break;
            }
        }

        if (!landedOnTop)
        {
            return;
        }

        isGrounded = true;

        if (!hasJumped)
        {
            return;
        }

        hasJumped = false;

        float playerWidth = GetPlayerWidth();

        if (platformSpawner != null &&
            gameManager != null)
        {
            bool landingSucceeded =
                platformSpawner.CompleteLanding(
                    collision.gameObject,
                    transform.position.x,
                    playerWidth);

            if (landingSucceeded)
            {
                bool isPlusJump =
                    !touchedHighWall;

                Debug.Log(
                    $"Landing | " +
                    $"Plus: {isPlusJump} | " +
                    $"High wall: {touchedHighWall}");

                gameManager.RegisterSuccessfulJump(
                    isPlusJump);

                gameManager.HandleSuccessfulLanding();
            }
        }

        Vector2 velocity =
            playerRigidbody.linearVelocity;

        velocity.y = bounceSpeed;
        playerRigidbody.linearVelocity = velocity;
    }

    private void OnCollisionExit2D(
    Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Platform"))
        {
            return;
        }

        isGrounded = false;

        if (bouncedPlatform == collision.gameObject)
        {
            bouncedPlatform = null;
        }
    }


    public void DisablePhysics()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.simulated = false;
        }
    }

    private float GetPlayerWidth()
    {
        if (playerCollider == null)
        {
            return 1f;
        }

        return playerCollider.bounds.size.x;
    }
}
