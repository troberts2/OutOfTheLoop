using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string onPlayClickedSceneName = "GameScene";

    [SerializeField] private Canvas mainMenuCanvas;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// called when play button on main menu is clicked
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene(onPlayClickedSceneName);
        GameManager.Instance.ResetGame();
    }

    /// <summary>
    /// Called when settings button on main menu clicked
    /// </summary>
    public void Settings()
    {
        PauseManager.Instance.OpenOptionsMenu();
    }

    /// <summary>
    /// Called when quit button on main menu is clicked
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainMenuCanvas.worldCamera = Camera.main;
    }
}
