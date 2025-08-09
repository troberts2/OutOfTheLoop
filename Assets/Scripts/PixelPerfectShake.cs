
using UnityEngine;
using UnityEngine.SceneManagement;

public class PixelPerfectShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 1f; // in pixels
    private Vector3 originalPos;
    private float shakeTimer;

    public static PixelPerfectShake Instance;

    // This should be your pixel size (1 / pixels per unit * camera scale)
    public float pixelSize = 0.0625f; // example: 1/16
    private Camera mainCam;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;
        originalPos = mainCam.transform.localPosition;
    }

    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

    void LateUpdate()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;

            float offsetX = Mathf.Round(Random.Range(-shakeMagnitude, shakeMagnitude)) * pixelSize;
            float offsetY = Mathf.Round(Random.Range(-shakeMagnitude, shakeMagnitude)) * pixelSize;

            mainCam.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            mainCam.transform.localPosition = originalPos;
        }
    }
}
