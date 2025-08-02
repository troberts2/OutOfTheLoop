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
            yield return new WaitForSeconds(randomSpawnRate);
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
            yield return new WaitForSeconds(circleSpawnInterval);
        }
    }

    private void PickRandomCircleToSpawn(Vector3 position)
    {
        int random = Random.Range(0, 100);

        EnemyPool.Instance.ActivateEnemy(position, random);
    }
}
