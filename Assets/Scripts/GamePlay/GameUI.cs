using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Charge")]
    [SerializeField] private Image chargeBarFill;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    private int totalScore;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SetCharge(0f);
        SetScore(0);
    }

    public void SetScore(int score)
    {
        totalScore = Mathf.Max(0, score);

        if (scoreText != null)
        {
            scoreText.text = totalScore.ToString("N0");
        }
    }

    public void AddScore(int points)
    {
        SetScore(totalScore + Mathf.Max(0, points));
    }

    public void SetCharge(float value)
    {
        if (chargeBarFill != null)
        {
            chargeBarFill.fillAmount = Mathf.Clamp01(value);
        }
    }

    public void ShowGameOver()
    {
        SetCharge(0f);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void HideGameplayUI()
    {
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        if (chargeBarFill != null)
        {
            chargeBarFill.gameObject.SetActive(false);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }
}