using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string onPlayClickedSceneName = "LevelSelect";

    /// <summary>
    /// called when play button on main menu is clicked
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene(onPlayClickedSceneName);
    }

    /// <summary>
    /// Called when settings button on main menu clicked
    /// </summary>
    public void Settings()
    {
        //TODO
    }

    /// <summary>
    /// Called when quit button on main menu is clicked
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
