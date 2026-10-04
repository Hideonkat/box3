using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class GameController
{
    public TextMeshProUGUI scoreText;
    public Image chargeBarFill;

    private int score;

    private void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

    private void AddScore()
    {
        score++;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateChargeBar(float value)
    {
        if (chargeBarFill)
        {
            chargeBarFill.fillAmount = value;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
