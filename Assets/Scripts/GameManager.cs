using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int playerScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector3 originalScale;

    public static event Action OnGameReset;

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

    public void AddToScore()
    {
        playerScore+= 100;
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
