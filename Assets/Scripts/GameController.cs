using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public Rigidbody2D playerRb;
    public float jumpAngle = 75f; // goc nhay 45
    public float minForce = 8f;
    public float maxForce = 22f;
    public float chargeSpeed = 1.2f;

    [Header("Visual Effects")]
    public ParticleSystem jumpExplosionFX;
    public ParticleSystem meteorImpactFX;
    public GameObject meteorFlameTrail;

    [Header("UI Elements")]
    public Image chargeBarFill;
    public Text scoreText;
    public GameObject gameOverPanel;

    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isGrounded = true;
    private bool isMeteorState = false;
    private bool isWaitingForMeteor = false;
    private bool hasScoredThisJump = true;
    private int score = 0;
    private int direction = 1; // 1: phai , -1: trai

    void Start()
    {
        Application.targetFrameRate = 60;
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (meteorFlameTrail) meteorFlameTrail.SetActive(false);

        score = 0; 
        UpdateScoreText(); 
    }

    void Update()
    {
        if (isWaitingForMeteor) return;

        // Touch
        if (Input.touchCount > 0 && isGrounded)
        {
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

        if (isGrounded && !hasScoredThisJump && playerRb.linearVelocity.magnitude < 0.1f)
        {
            score += 1;
            UpdateScoreText();
            hasScoredThisJump = true; // Mark score 
        }

        // Power up bar
        if (isCharging)
        {
            currentCharge += Time.deltaTime * chargeSpeed;
            currentCharge = Mathf.Clamp01(currentCharge);
            if (chargeBarFill) chargeBarFill.fillAmount = currentCharge;
        }

        // over the screen
        if (player.position.y > 10f && !isMeteorState && !isWaitingForMeteor)
        {
            StartCoroutine(MeteorRoutine());
        }
    }

    void LaunchPlayer()
    {
        isGrounded = false;
        hasScoredThisJump = false; // dat lai moi khi bat dau nhay

        float force = Mathf.Lerp(minForce, maxForce, currentCharge);
        float rad = jumpAngle * Mathf.Deg2Rad;
        Vector2 jumpDirection = new Vector2(Mathf.Cos(rad) * direction, Mathf.Sin(rad));

        playerRb.linearVelocity = jumpDirection * force;
        playerRb.AddTorque(-direction * 5f, ForceMode2D.Impulse);

        if (jumpExplosionFX) jumpExplosionFX.Play();
        if (chargeBarFill) chargeBarFill.fillAmount = 0;
    }

    IEnumerator MeteorRoutine()
    {
        isWaitingForMeteor = true;
        playerRb.simulated = false; // ?n v?t l� t?m th?i
        player.gameObject.SetActive(false); // Bi?n m?t kh?i m�n h�nh

        yield return new WaitForSeconds(2.0f); // T?m d?ng 2 gi�y

        // Hi?n l?i d?ng THI�N TH?CH r?i t? tr�n ??nh xu?ng
        isWaitingForMeteor = false;
        isMeteorState = true;
        player.position = new Vector3(0, 12f, 0); // Xu?t ph�t tr�n cao
        player.gameObject.SetActive(true);
        playerRb.simulated = true;

        if (meteorFlameTrail) meteorFlameTrail.SetActive(true);

        // Lao xu?ng c?c m?nh
        playerRb.linearVelocity = new Vector2(direction * 3f, -30f);
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

    void TriggerWorldShatter()
    {
        isMeteorState = false;
        if (meteorFlameTrail) meteorFlameTrail.SetActive(false);
        if (meteorImpactFX) meteorImpactFX.Play();

        // T�m t?t c? b? t??ng v� k�ch ho?t v? n�t
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach (GameObject plat in platforms)
        {
            // Th�m l?c ?�nh v? b? t??ng th�nh t?ng m?nh
            Rigidbody2D[] parts = plat.GetComponentsInChildren<Rigidbody2D>();
            foreach (Rigidbody2D part in parts)
            {
                part.bodyType = RigidbodyType2D.Dynamic;
                part.AddForce(Random.insideUnitCircle * 10f, ForceMode2D.Impulse);
            }
        }

        // Hi?n Game Over sau 1.5 gi�y
        Invoke("ShowGameOver", 1.5f);
    }

    void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // In
        }
    }
}