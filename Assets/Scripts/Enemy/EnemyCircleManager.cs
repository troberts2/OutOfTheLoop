using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyCircleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyCircleList;

    [Header("Random Screen Spawn")]
    private Coroutine randomSpawnRoutine;
    [SerializeField] private float startInterval = 0.5f;
    [SerializeField] private float endInterval = 0.1f;
    [SerializeField] private float difficultyRampTime = 45f; // seconds to reach max difficulty
    private float elapsedTime = 0;
    private float currentInterval;
    private float t = 0;

    [Header("Circular Spawn")]
    [SerializeField] private float circleSpawnInterval = 2f;
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private Transform playerTransform;
    private Coroutine circleSpawnRoutine;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GameManager.OnGameReset += OnGameReset;
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnGameReset;
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        StopCircleSpawning();
        StopRandomScreenSpawning();
    }

    private void OnGameReset()
    {
        currentInterval = startInterval;
        elapsedTime = 0;
        t = 0;

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            //invoke on 3 second delay for the countdown sequence
            Invoke(nameof(StartRandomSpawning), 3f);
        }
        else
        {
            //dont do player tracking on main menu
            StartRandomScreenSpawning();
        }
    }

    private void StartRandomSpawning()
    {
        StartCircleSpawning(playerTransform);
        StartRandomScreenSpawning();
    }

    // ------------ Random Screen Spawning ------------

    public void StartRandomScreenSpawning()
    {
        if (randomSpawnRoutine != null) StopCoroutine(randomSpawnRoutine);
        randomSpawnRoutine = StartCoroutine(RandomScreenSpawnLoop());
    }

    public void StopRandomScreenSpawning()
    {
        if (randomSpawnRoutine != null) StopCoroutine(randomSpawnRoutine);
    }

    private IEnumerator RandomScreenSpawnLoop()
    {
        while (true)
        {
            Vector3 screenPos = new Vector3(
                Random.Range(0f, Screen.width),
                Random.Range(0f, Screen.height),
                0f
            );

            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            PickRandomCircleToSpawn(worldPos);
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                yield return new WaitForSeconds(currentInterval);
            }
            else
            {
                yield return new WaitForSeconds(startInterval / 2);
            }
        }
    }

    // ------------ Circular Spawning Around Transform ------------

    public void StartCircleSpawning(Transform center)
    {
        if (circleSpawnRoutine != null) StopCoroutine(circleSpawnRoutine);
        circleSpawnRoutine = StartCoroutine(CircleSpawnLoop(center));
    }

    public void StopCircleSpawning()
    {
        if (circleSpawnRoutine != null) StopCoroutine(circleSpawnRoutine);
    }

    private IEnumerator CircleSpawnLoop(Transform center)
    {
        while (true)
        {
            Vector2 randomPoint = Random.insideUnitCircle * circleRadius;
            Vector3 spawnPos = center.position + new Vector3(randomPoint.x, randomPoint.y, 0f);
            PickRandomCircleToSpawn(spawnPos);

            if(SceneManager.GetActiveScene().name == "GameScene")
            {
                yield return new WaitForSeconds(currentInterval * 4);
            }
        }
    }

    private void PickRandomCircleToSpawn(Vector3 position)
    {
        int random = Random.Range(0, 100);

        EnemyPool.Instance.ActivateEnemy(position, random);
    }


    private void Update()
    {
        if(GameManager.Instance.isGameStarted)
        {
            elapsedTime += Time.deltaTime;

            t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
            currentInterval = Mathf.Lerp(startInterval, endInterval, t);
        }
    }
}
