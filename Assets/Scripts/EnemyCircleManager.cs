using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
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

            Instantiate(PickRandomCircleToSpawn(), worldPos, Quaternion.identity);
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
            Instantiate(PickRandomCircleToSpawn(), spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(circleSpawnInterval);
        }
    }

    private GameObject PickRandomCircleToSpawn()
    {
        float random = Random.Range(0, 100);

        if (random > 90)
            return enemyCircleList[4]; //biggest 10%
        else if (random > 70)
            return enemyCircleList[3]; // second biggest 20%
        else if (random > 50) // middle circle 20%
            return enemyCircleList[2];
        else if (random > 30)
            return enemyCircleList[1]; // second smallest 20%
        else
            return enemyCircleList[0]; //smallest 30%
    }
}
