using System.Collections;
using UnityEngine;

public partial class GameController
{
    private IEnumerator MeteorRoutine()
    {
        isWaitingForMeteor = true;
        playerRb.simulated = false;
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        isWaitingForMeteor = false;
        isMeteorState = true;

        player.position = new Vector3(0f, 12f, 0f);
        player.gameObject.SetActive(true);
        playerRb.simulated = true;

        if (meteorFlameTrail)
        {
            meteorFlameTrail.SetActive(true);
        }

        playerRb.linearVelocity = new Vector2(direction * 3f, -30f);
    }

    private void TriggerWorldShatter()
    {
        isMeteorState = false;

        if (meteorFlameTrail)
        {
            meteorFlameTrail.SetActive(false);
        }

        if (meteorImpactFX)
        {
            meteorImpactFX.Play();
        }

        foreach (GameObject platform in activePlatforms)
        {
            if (platform == null)
            {
                continue;
            }

            Rigidbody2D[] parts =
                platform.GetComponentsInChildren<Rigidbody2D>();

            foreach (Rigidbody2D part in parts)
            {
                part.bodyType = RigidbodyType2D.Dynamic;
                part.AddForce(
                    Random.insideUnitCircle * 12f,
                    ForceMode2D.Impulse);
            }
        }

        Invoke(nameof(ShowGameOver), 1.5f);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
