using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplierManager : MonoBehaviour
{
    public static MultiplierManager Instance;

    [Header("Multiplier Pool")]
    [SerializeField] private Multiplier multiplier;
    [SerializeField] private int poolSize = 5;

    private List<Multiplier> multipliers;

    private Coroutine spawnRoutine;
    [SerializeField] private float spawnInterval = 5f;
    private Camera mainCam;
    public MultiplierPointer multiplierPointer;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreatePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += OnGameReset;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerCollision.OnPlayerDeath += StopSpawning;
        AdManager.OnPlayerContinueReward += OnPlayerContinueAdReward;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnGameReset;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerCollision.OnPlayerDeath -= StopSpawning;
        AdManager.OnPlayerContinueReward -= OnPlayerContinueAdReward;
    }

    private void OnPlayerContinueAdReward()
    {
        StartSpawning();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;
        if(scene.name == "Tutorial" || scene.name == "GameScene")
        {
            multiplierPointer = FindAnyObjectByType<MultiplierPointer>();
        }
    }

    private void OnGameReset()
    {
        StopSpawning();
        //clear particles
        foreach (Multiplier m in multipliers)
        {
            m.TurnOff(); // fully reset
        }
    }

    private void CreatePool()
    {
        multipliers = new List<Multiplier>();
        for (int i = 0; i < poolSize; i++)
        {
            Multiplier m = Instantiate(multiplier, transform);
            m.TurnOff(true); // optional: deactivate until used
            multipliers.Add(m);
        }
    }

    private Multiplier GetAvailableSource()
    {
        foreach (Multiplier m in multipliers)
        {
            if (!m.IsActive)
                return m;
        }

        // Fallback: reuse the first one (or expand pool if needed)
        return multipliers[0];
    }

    public void SpawnMultiplier(Vector3 position)
    {
        Multiplier m = GetAvailableSource();
        m.transform.position = position;
        m.TurnOn();
        multiplierPointer.target = m.transform;
    }

    public void StartSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Vector3 screenPos = new Vector3(
                Random.Range(0f, Screen.width),
                Random.Range(0f, Screen.height),
                0f
            );

            if(mainCam ==null)
            {
                mainCam = Camera.main;
            }

            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            SpawnMultiplier(worldPos);
        }
    }
}
