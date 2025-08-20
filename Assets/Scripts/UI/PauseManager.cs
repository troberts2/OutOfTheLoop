using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    private PlayerControls playerControls;
    private InputAction pauseBack;
    public static event Action OnPause;
    public static event Action OnResume;
    public static event Action OnOptionsClose;
    private bool isPaused = false;
    private bool isOptions = false;
    [SerializeField] private float menuAnimTime = 0.35f;
    [SerializeField] private Canvas pauseCanvas;

    [Header("Pause Menu")]
    [SerializeField] private RectTransform pausePanel;
    [SerializeField] private CanvasGroup pauseGroup;
    [SerializeField] private GameObject pauseButton;

    [Header("Options Menu")]
    [SerializeField] private RectTransform optionsPanel;
    [SerializeField] private CanvasGroup optionsGroup;

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
        playerControls = new PlayerControls();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pauseCanvas.worldCamera = Camera.main;
        if(scene.name == "MainMenu")
        {
            pauseButton.SetActive(false);
        }
    }

    void OnPlayerDeath()
    {
        pauseButton.SetActive(false);
    }

    void OnGameReset()
    {
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            Invoke(nameof(EnablePauseButton), 3f);
        }
    }

    private void EnablePauseButton()
    {
        pauseButton.SetActive(true);
    }

    private void OnEnable()
    {
        playerControls.Enable();
        pauseBack = playerControls.Player.PauseBack;
        pauseBack.performed += OnPauseBack;
        pauseBack.Enable();

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
        GameManager.OnGameReset += OnGameReset;
        AdManager.OnPlayerContinueReward += EnablePauseButton;
    }

    private void OnDisable()
    {
        if (pauseBack != null)
        {
            pauseBack.performed -= OnPauseBack;
            pauseBack.Disable();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
        GameManager.OnGameReset -= OnGameReset;
        AdManager.OnPlayerContinueReward -= EnablePauseButton;
    }

    public void OnWebGLBlur()
    {
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            if(!isPaused)
            {
                //start pause and call OnPause functions from other scripts
                OnPause?.Invoke();

                //bring up pause menu
                OpenPauseMenu();
            }
        }
    }

    void OnPauseBack(InputAction.CallbackContext context)
    {
        if(SceneManager.GetActiveScene().name == "GameScene" && GameManager.Instance.isGameStarted)
        {
            if(!isPaused)
            {
                //start pause and call OnPause functions from other scripts
                OnPause?.Invoke();

                //bring up pause menu
                OpenPauseMenu();
            }
            else // is paused
            { 
                //if not in options menu (close pause screen)
                if(!isOptions)
                {
                    OnResume?.Invoke();

                    ClosePauseMenu();
                }
                else
                {
                    //if is in options menu (should close options to pause)
                    CloseOptionsMenu();
                }
            }
        }
        else if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            if(!isOptions)
            {
                OpenOptionsMenu();
            }
            else
            {
                CloseOptionsMenu();
            }
        }
        
    }

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;
        pausePanel.gameObject.SetActive(true);
        pauseButton.SetActive(false);
        isPaused = true;
        pausePanel.anchoredPosition = new Vector2(0, 1000);
        pauseGroup.alpha = 0.0f;

        pausePanel.DOAnchorPosY(0, menuAnimTime, false).SetEase(Ease.InBack).SetUpdate(true);
        pauseGroup.DOFade(1, menuAnimTime).SetEase(Ease.InBack).SetUpdate(true);
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1;
        pauseButton.SetActive(true);
        isPaused = false;
        pausePanel.anchoredPosition = new Vector2(0, 0);
        pauseGroup.alpha = 1.0f;

        pausePanel.DOAnchorPosY(1000, menuAnimTime, false).SetEase(Ease.OutBack).SetUpdate(true);
        pauseGroup.DOFade(0f, menuAnimTime).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OpenOptionsMenu()
    {
        optionsPanel.gameObject.SetActive(true);
        isOptions = true;
        optionsPanel.anchoredPosition = new Vector2(0, 1000);
        optionsGroup.alpha = 0.0f;

        optionsPanel.DOAnchorPosY(0, menuAnimTime, false).SetEase(Ease.InBack).SetUpdate(true);
        optionsGroup.DOFade(1, menuAnimTime).SetEase(Ease.InBack).SetUpdate(true);
    }

    public void CloseOptionsMenu()
    {
        OnOptionsClose?.Invoke();
        isOptions = false;
        optionsPanel.anchoredPosition = new Vector2(0, 0);
        optionsGroup.alpha = 1.0f;

        optionsPanel.DOAnchorPosY(1000, menuAnimTime, false).SetEase(Ease.OutBack).SetUpdate(true);
        optionsGroup.DOFade(0f, menuAnimTime).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        ClosePauseMenu();
    }
}
