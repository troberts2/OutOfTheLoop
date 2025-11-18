using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using ProfanityFilter.Interfaces;
using TMPro;
using UnityEngine.SceneManagement;


public class LeaderboardSystem : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TextMeshProUGUI[] rankTexts;
    [SerializeField] private GameObject openLeaderboardButton;
    [SerializeField] private Canvas leaderboardCanvas;

    [Serializable]
    public class ScoreMetadata
    {
        public string playerNameID;
    }

    private const string LEADERBOARD_ID = "High_Score";

    public static LeaderboardSystem Instance;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name == "MainMenu")
        {
            openLeaderboardButton.SetActive(true);
        }
        else
        {
            openLeaderboardButton.SetActive(false);
        }

        leaderboardCanvas.worldCamera = Camera.main;
    }

    public async void AddScoreWithMetadata(int score, string playerName)
    {
        var scoreMetadata = new ScoreMetadata { playerNameID = playerName };
        var playerEntry = await LeaderboardsService.Instance
            .AddPlayerScoreAsync(
                LEADERBOARD_ID,
                score,
                new AddPlayerScoreOptions { Metadata = scoreMetadata }
            );
        Debug.Log(JsonConvert.SerializeObject(playerEntry));
    }

    public async void GetScoresWithMetadataAndPutInText()
    {
        var scoreResponse = await LeaderboardsService.Instance
            .GetScoresAsync(
                LEADERBOARD_ID,
                new GetScoresOptions { IncludeMetadata = true }
            );

        for (int i = 0; i < 10; i++)
        {
            bool hasEntry = i < scoreResponse.Results.Count;

            if (hasEntry && scoreResponse.Results[i] != null)
            {
                var entry = scoreResponse.Results[i];
                if (entry != null && !string.IsNullOrEmpty(entry.Metadata) && rankTexts[i] != null)
                {
                    // Convert JSON string back to object
                    ScoreMetadata meta = JsonConvert.DeserializeObject<ScoreMetadata>(entry.Metadata);

                    rankTexts[i].text = "#" + (i + 1) + " " + meta.playerNameID + " - " + entry.Score;

                    /*var filter = new ProfanityFilter.ProfanityFilter();
                    Debug.Log(filter.IsProfanity("arsehole"));*/
                }
            }
            else
            {
                rankTexts[i].text = "#" + (i + 1);
            }
        }
    }

    #region Leaderboard UI

    public void OpenLeaderboard()
    {
        //open UI
        leaderboardPanel.SetActive(true);

        //populate leaderboard
        GetScoresWithMetadataAndPutInText();
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    #endregion
}


