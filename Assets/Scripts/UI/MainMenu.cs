using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    private void Start()
    {
        SaveFile save = SaveSystem.Instance.LoadGame();
        if (!save.settings.hasPlayedBefore)
        {

            //if first time playing load tutorial
            SaveSystem.Instance.SaveHasPlayedBefore();
            SceneManager.LoadScene("Tutorial");
        }
    }

    /// <summary>
    /// called when play button on main menu is clicked
    /// </summary>
    public void Play()
    {
        EventSystem.current.enabled = false;
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

    public void TutorialScene()
    {
        EventSystem.current.enabled = false;
        SceneManager.LoadScene("Tutorial");
        GameManager.Instance.ResetGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainMenuCanvas.worldCamera = Camera.main;
    }
}
