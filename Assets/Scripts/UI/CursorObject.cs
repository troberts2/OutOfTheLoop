
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CursorObject : MonoBehaviour
{
    Camera mainCam;

    [SerializeField] private SpriteRenderer cursor;

    public static CursorObject Instance;

    private bool isGameScene = false;

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

    void Start()
    {
        mainCam = Camera.main;
    }
#if UNITY_ANDROID
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerCollision.OnPlayerDeath += DisableCursor;
        AdManager.OnPlayerContinueReward += EnableCursor;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerCollision.OnPlayerDeath -= DisableCursor;
        AdManager.OnPlayerContinueReward -= EnableCursor;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        mainCam = Camera.main;
        if(scene.name == "GameScene")
        {
            EnableCursor();
            isGameScene = true;
        }
        else
        {
            DisableCursor();
            isGameScene= false;
        }
    }

    private void DisableCursor()
    {
        cursor.enabled = false;
    }

    private void EnableCursor()
    {
        if (Touchscreen.current == null) return;

        cursor.enabled = true;
    }

    void Update()
    {
        if(Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed && isGameScene && GameManager.Instance.isGameStarted)
        {
            EnableCursor();
        }
        else
        {
            DisableCursor();
        }

        // Make sure we have at least one touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // Get the screen position of the touch
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();

            // Convert to world position
            Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, mainCam.nearClipPlane));

            // Keep z consistent with your game objects
            worldPos.z = 0f; // Change if your game uses a different z-plane

            // Move the cursor
            transform.position = worldPos;
        }
    }
#endif
}
