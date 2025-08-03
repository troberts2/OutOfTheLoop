using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyCircleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyCircleList;

    [Header("Random Screen Spawn")]
    [SerializeField] private float randomSpawnRate = 1f; // seconds between spawns
    private Coroutine randomSpawnRoutine;

    [Header("Circular Spawn")]
    [SerializeField] private float circleSpawnInterval = 2f;
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnDecay = 0.99142f; // lower from 0.5 to 0.1 in about a minute
    private Coroutine circleSpawnRoutine;
    [SerializeField] private float playerSpawnRateMinimum = .2f;
    [SerializeField] private float randomSpawnRateMinimum = .09f;

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
        float tempRandomSpawnRate = randomSpawnRate;
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
            yield return new WaitForSeconds(tempRandomSpawnRate);
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                tempRandomSpawnRate *= spawnDecay;
                tempRandomSpawnRate = Mathf.Max(tempRandomSpawnRate, randomSpawnRateMinimum);
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
        float tempCircleSpawnInterval = circleSpawnInterval;
        while (true)
        {
            Vector2 randomPoint = Random.insideUnitCircle * circleRadius;
            Vector3 spawnPos = center.position + new Vector3(randomPoint.x, randomPoint.y, 0f);
            PickRandomCircleToSpawn(spawnPos);
            yield return new WaitForSeconds(tempCircleSpawnInterval);
            if(SceneManager.GetActiveScene().name == "GameScene")
            {
                tempCircleSpawnInterval *= spawnDecay;
                tempCircleSpawnInterval = Mathf.Max(tempCircleSpawnInterval, playerSpawnRateMinimum);
            }
        }
    }

    private void PickRandomCircleToSpawn(Vector3 position)
    {
        int random = Random.Range(0, 100);

        EnemyPool.Instance.ActivateEnemy(position, random);
    }
}
