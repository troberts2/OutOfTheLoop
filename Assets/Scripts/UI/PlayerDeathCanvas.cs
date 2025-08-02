using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerDeathCanvas : MonoBehaviour
{
    [SerializeField] private Image deathPanel;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI highScoreNumber;
    [SerializeField] private TextMeshProUGUI roundScoreNumber;
    [SerializeField] private RectTransform retryButton;
    [SerializeField] private RectTransform quitButton;
    private Vector3 retryAnchorPos;
    private Vector3 quitAnchorPos;

    private void OnEnable()
    {
        retryAnchorPos = retryButton.anchoredPosition;
        quitAnchorPos = quitButton.anchoredPosition;
        GameManager.OnGameReset += ResetPanel;
        PlayerCollision.OnPlayerDeath += ShowDeathPanelAfterDelay;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetPanel;
        PlayerCollision.OnPlayerDeath -= ShowDeathPanelAfterDelay;
    }

    /// <summary>
    /// called in playercollision
    /// </summary>
    private void ShowDeathPanelAfterDelay()
    {
        //wait 2 seconds after dying for it to appear
        Invoke(nameof(ShowDeathPanel), 2f);
    }

    private void ResetPanel()
    {
        // Reset panel
        deathPanel.rectTransform.anchoredPosition = new Vector2(0, 1000); // offscreen top
        retryButton.anchoredPosition = new Vector2(0, -1000);
        quitButton.anchoredPosition = new Vector2(0, -1000);
        retryButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        highScoreText.transform.localScale = Vector3.zero;
        highScoreNumber.transform.localScale = Vector3.one;
        roundScoreNumber.transform.localScale = Vector3.one;
        roundScoreNumber.text = "0";
        deathPanel.gameObject.SetActive(false);
    }

    private void ShowDeathPanel()
    {
        ResetPanel();
        deathPanel.gameObject.SetActive(true);

        // Slide panel in from top fast and fade in background
        Sequence panelIn = DOTween.Sequence();
        panelIn.Append(deathPanel.rectTransform.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutBack));

        // After panel in, check high score
        panelIn.AppendCallback(() =>
        {
            int playerScore = GameManager.Instance.playerScore;
            int currentHighscore = SaveSystem.Instance.LoadGame().highscoreData.HighScore;
            bool isNewHighScore = playerScore > currentHighscore;
            if (isNewHighScore)
            {
                SaveSystem.Instance.SaveHighScore(playerScore);
                highScoreNumber.text = playerScore.ToString();

                Sequence highScoreSeq = DOTween.Sequence();
                Sequence highScoreTextSeq = DOTween.Sequence();

                // Blink high score number
                highScoreSeq.Append(highScoreNumber.DOFade(0, 0.1f));
                highScoreSeq.Append(highScoreNumber.DOFade(1, 0.1f).SetLoops(3, LoopType.Yoyo));

                // Grow highScoreText
                highScoreTextSeq.AppendCallback(() =>
                {
                    highScoreText.transform.DOScale(1.0f, 0.25f).SetEase(Ease.OutBack);
                    highScoreText.transform.DOScale(1f, 0.2f);
                }); 

                // Show and count up round score
                highScoreSeq.AppendCallback(() =>
                {
                    roundScoreNumber.text = "0";
                    DOTween.To(() => 0, x =>
                    {
                        roundScoreNumber.text = Mathf.FloorToInt(x).ToString();
                    }, playerScore, 2f).SetEase(Ease.OutExpo);

                    roundScoreNumber.transform
                        .DOScale(1.5f, 0.2f)
                        .SetLoops(10, LoopType.Yoyo)
                        .SetEase(Ease.OutBack);
                });

                highScoreSeq.AppendCallback(() =>
                {
                    roundScoreNumber.transform.DOScale(1f, .2f);
                    quitButton.gameObject.SetActive(true);
                    retryButton.gameObject.SetActive(true);
                    quitButton.DOAnchorPos(quitAnchorPos, .25f).SetEase(Ease.InSine);
                    retryButton.DOAnchorPos(retryAnchorPos, .25f).SetEase(Ease.InSine);
                });

            }
            else
            {
                highScoreNumber.text = currentHighscore.ToString();

                Sequence normalScoreSeq = DOTween.Sequence();

                // Grow high score number
                normalScoreSeq.Append(highScoreNumber.transform
                    .DOScale(1.2f, 0.25f)
                    .SetEase(Ease.OutBack))
                    .Append(highScoreNumber.transform.DOScale(1f, 0.2f));

                // Show and count up round score
                normalScoreSeq.AppendCallback(() =>
                {
                    roundScoreNumber.text = "0";
                    DOTween.To(() => 0, x =>
                    {
                        roundScoreNumber.text = Mathf.FloorToInt(x).ToString();
                    }, playerScore, 2f).SetEase(Ease.OutExpo);

                    roundScoreNumber.transform
                        .DOScale(1.5f, 0.2f)
                        .SetLoops(10, LoopType.Yoyo)
                        .SetEase(Ease.OutBack);
                });

                normalScoreSeq.AppendCallback(() =>
                {
                    quitButton.gameObject.SetActive(true);
                    retryButton.gameObject.SetActive(true);
                    quitButton.DOAnchorPos(quitAnchorPos, .25f).SetEase(Ease.InSine);
                    retryButton.DOAnchorPos(retryAnchorPos, .25f).SetEase(Ease.InSine);
                });
            }
        });
        
    }

    public void CallResetGame()
    {
        GameManager.Instance.ResetGame();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

