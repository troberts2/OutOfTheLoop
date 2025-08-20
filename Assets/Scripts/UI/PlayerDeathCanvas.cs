using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

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

    [SerializeField] private Image continueAfterDeathPanel;
    [SerializeField] private TextMeshProUGUI roundScoreContinue;
    [SerializeField] private RectTransform yesButton;
    [SerializeField] private RectTransform noButton;
    [SerializeField] private float closeContinueScreenAfter = 5f;
    [SerializeField] private Image timerImage;
    private float currentTimerImageFill = 0;

    private int playerDeaths = 0;

    private void OnEnable()
    {
        retryAnchorPos = retryButton.anchoredPosition;
        quitAnchorPos = quitButton.anchoredPosition;
        GameManager.OnGameReset += ResetPanel;
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetPanel;
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
    }

    private void Update()
    {
        if(currentTimerImageFill > 0)
        {
            currentTimerImageFill -= Time.deltaTime;
            timerImage.fillAmount = currentTimerImageFill / closeContinueScreenAfter;
        }
    }

    /// <summary>
    /// called in playercollision
    /// </summary>
    private void ShowDeathPanelAfterDelay()
    {
        //wait 2 seconds after dying for it to appear
        Invoke(nameof(ShowDeathPanel), 2f);
    }

    private void OnPlayerDeath()
    {
        //if reward ad is ready
        if(AdManager.Instance.rewardedVideoAd.IsAdReady() && !AdManager.Instance.hasWatchedAdThisRun && AdManager.Instance.isAdsEnabled)
        {
            //bring up watch video to continue screen
            Invoke(nameof(OpenContinueAdPanel), 2f);
        }
        else
        {
            //no ad ready so just play normal death screen
            ShowDeathPanelAfterDelay();
        }
    }

    private void OpenContinueAdPanel()
    {
        //reset panel

        continueAfterDeathPanel.gameObject.SetActive(true);

        // Slide panel in from top fast and fade in background
        Sequence panelIn = DOTween.Sequence();
        panelIn.Append(continueAfterDeathPanel.rectTransform.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutBack));

        panelIn.AppendCallback(() =>
        {
            roundScoreContinue.text = GameManager.Instance.playerScore.ToString();
            roundScoreContinue.transform.DOScale(1f, .2f);
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
            yesButton.DOAnchorPos(retryAnchorPos, .25f).SetEase(Ease.InSine);
            noButton.DOAnchorPos(quitAnchorPos, .25f).SetEase(Ease.InSine);

            //invoke close timer
            currentTimerImageFill = closeContinueScreenAfter;
            Invoke(nameof(CloseContinuePanel), closeContinueScreenAfter);
            Invoke(nameof(ShowDeathPanel), closeContinueScreenAfter);
        });
    }

    public void YesButton()
    {
        //play add pause game
        AdManager.Instance.ShowRewarded();
        CloseContinuePanel();
        ResetPanel();
    }

    public void NoButton()
    {
        CloseContinuePanel();
        CancelInvoke(nameof(ShowDeathPanel));
        Invoke(nameof(ShowDeathPanel), 0.25f);
    }

    private void CloseContinuePanel()
    {
        Sequence panelOut = DOTween.Sequence();
        panelOut.Append(continueAfterDeathPanel.rectTransform.DOAnchorPos(new Vector2(0, 1000), 0.25f).SetEase(Ease.InBack));

        panelOut.AppendCallback(() =>
        {
            continueAfterDeathPanel.gameObject.SetActive(false);   
        });
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
        if (deathPanel.gameObject.activeInHierarchy) return;

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
                    Invoke(nameof(CheckIfPlayInterstitial), .25f);
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
                    Invoke(nameof(CheckIfPlayInterstitial), .25f);
                });
            }
        });
        
    }

    private void CheckIfPlayInterstitial()
    {
        //check if should show interstitial ad
        playerDeaths++;
        if (playerDeaths % 3 == 0 && playerDeaths != 0)
        {
            //player interstitial
            if(AdManager.Instance.interstitialAd.IsAdReady() && AdManager.Instance.isAdsEnabled)
            {
                AdManager.Instance.ShowInterstitial();
            }
        }
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

