using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int playerScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText;

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
    }

    public void AddToScore()
    {
        playerScore+= 100;
        scoreText.text = playerScore.ToString();
    }

    public void ResetGame()
    {
        playerScore = 0;
        scoreText.text = playerScore.ToString();
    }
}
