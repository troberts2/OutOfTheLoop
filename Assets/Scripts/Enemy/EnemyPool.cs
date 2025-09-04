using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;
    [Header("Enemy Pool")]
    [SerializeField] private List<BasicEnemyCircle> enemyCircles;
    [SerializeField] private int poolSize = 20;

    private List<List<BasicEnemyCircle>> enemies; // 0 small - 4 largest

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
    private void CreatePool()
    {
        enemies = new List<List<BasicEnemyCircle>>();
        for(int j = 0; j < enemyCircles.Count; j++)
        {
            List<BasicEnemyCircle> newEnemyList = new List<BasicEnemyCircle>();
            enemies.Add(newEnemyList);
            for (int i = 0; i < poolSize; i++)
            {
                BasicEnemyCircle enemy = Instantiate(enemyCircles[j], transform);
                enemies[j].Add(enemy);
                enemy.gameObject.SetActive(false);
            }
        }
        
    }

    private BasicEnemyCircle GetAvailableSource(int random)
    {
        if (random > 90)
            return GetFromList(enemies[4]); //biggest 10%
        else if (random > 70)
            return GetFromList(enemies[3]); // second biggest 20%
        else if (random > 50) // middle circle 20%
            return GetFromList(enemies[2]);
        else if (random > 30)
            return GetFromList(enemies[1]); // second smallest 20%
        else
            return GetFromList(enemies[0]); //smallest 30%

        BasicEnemyCircle GetFromList(List<BasicEnemyCircle> enemies)
        {
            foreach (BasicEnemyCircle enemy in enemies)
            {
                if (!enemy.gameObject.activeInHierarchy)
                    return enemy;
            }

            // Fallback: reuse the first one (or expand pool if needed)
            return enemies[0];
        }
    }

    public void ActivateEnemy(Vector3 position, int random)
    {
        BasicEnemyCircle enemy = GetAvailableSource(random);
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        enemy.ActivateDeathCircle(false);

    }
}
