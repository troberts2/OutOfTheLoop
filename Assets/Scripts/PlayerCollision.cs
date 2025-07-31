using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    private int currentHealth = 2;
    [SerializeField] private Image[] hearts;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            BasicEnemyCircle e = collision.GetComponent<BasicEnemyCircle>();
            if (!e.HasDamagedPlayer)
            {
                e.HasDamagedPlayer = true;
                TakeDamage();
            }
            
        }
    }

    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth < 0)
        {
            Die();
            return;
        }

        
        Debug.Log(currentHealth);
        for(int i = 0; i < hearts.Length; i++)
        {
            if(i <= currentHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled=false;
            }
        }
    }

    private void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.ResetGame();
    }
}
