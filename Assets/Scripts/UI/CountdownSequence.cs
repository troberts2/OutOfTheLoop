using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class CountdownSequence : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] float numberScale = 2f;
    [SerializeField] float popDuration = 0.3f;
    [SerializeField] float holdTime = 0.7f;
    [SerializeField] private Color textColor;

    private void OnEnable()
    {
        GameManager.OnGameReset += OnResetGame;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnResetGame;
    }

    private void OnResetGame()
    {
        countdownText.enabled = false;
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            StartCoroutine(PlayCountdown());
        }
    }

    private IEnumerator PlayCountdown()
    {
        string[] sequence = { "3", "2", "1", "GO!" };

        for (int i = 0; i < sequence.Length; i++)
        {
            countdownText.enabled = true;
            countdownText.text = sequence[i];
            countdownText.transform.localScale = Vector3.zero;
            textColor.a = 1;
            countdownText.color = textColor; // make visible

            // Pop in
            countdownText.transform.DOScale(numberScale, popDuration)
                .SetEase(Ease.OutBack);

            // Wait for this number to finish its time
            float waitTime = (i < 3) ? 1f : 1f; // 1 sec per number including GO
            if(i == 3) //go
            {
                GameManager.Instance.ChangeIsGameStartedTrue();
            }
            yield return new WaitForSeconds(waitTime - .2f);

            // Fade out quickly
            countdownText.DOFade(0f, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }

        // Countdown done – game starts
        countdownText.enabled = false; // or trigger gameplay here
    }
}
