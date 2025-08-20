using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int playerScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector3 originalScale;
    [SerializeField] private Canvas scoreCanvas;
    public MultiplierText multText;
    public bool isGameStarted = false;
    public bool isTiltControls = false;

    public static event Action OnGameReset;

    public Vector3 calibrationOffset;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        originalScale = scoreText.transform.localScale;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
        AdManager.OnPlayerContinueReward += OnPlayerContinueRewardAd;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
        AdManager.OnPlayerContinueReward -= OnPlayerContinueRewardAd;
    }

    private void Start()
    {
        SaveFile save = SaveSystem.Instance.LoadGame();
        isTiltControls = save.settings.isTiltControls;
    }

    private void OnPlayerDeath()
    {
        isGameStarted = false;
        scoreText.enabled = false;
    }

    private void OnPlayerContinueRewardAd()
    {
        isGameStarted=true;
        scoreText.enabled = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameStarted = false;
        ResetGame();
        scoreCanvas.worldCamera = Camera.main;

        if(scene.name == "MainMenu")
        {
            //turn off score UI
            scoreText.enabled = false;
        }
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            //make player able to turn points from circle destroying
            Invoke(nameof(ChangeIsGameStartedTrue), 3f);
        }
    }

    public void ChangeIsGameStartedTrue()
    {
        isGameStarted = true;
        scoreText.enabled = true;
        //make multipliers spawn
        MultiplierManager.Instance.StartSpawning();
    }

    public void AddToScore()
    {
        if (!isGameStarted) return;

        playerScore+= 10 * multText.currentMultiplier;
        scoreText.text = playerScore.ToString();
        Pop();
    }

    private void Pop()
    {
        // Cancel any running tween on this transform to avoid stacking
        scoreText.transform.DOKill();
        scoreText.DOKill();

        // Reset to original scale in case it was left mid-animation
        scoreText.transform.localScale = originalScale;

        // Pop: scale up then back down
        scoreText.transform.DOScale(originalScale * 1.1f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScale(originalScale, 0.1f).SetEase(Ease.InQuad);
            });

        scoreText.DOColor(Color.yellow, 0.1f).OnComplete(() => {
            scoreText.DOColor(Color.white, 0.1f);
        });
    }

    public void ResetGame()
    {
        OnGameReset?.Invoke();
        playerScore = 0;
        scoreText.text = playerScore.ToString();
    }


}
