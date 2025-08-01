using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerCollision : MonoBehaviour
{
    private int currentHealth = 2;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        GameManager.OnGameReset += OnReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnReset;
    }

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

        animator.SetTrigger("takeHit");
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

        foreach(var heart in hearts)
        {
            if(heart.enabled)
            {
                heart.DOKill();
                heart.transform.DOKill();
                heart.transform.localScale = Vector3.one;
                heart.transform.DOScale(1.3f, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        heart.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InQuad);
                });
            }
        }
    }

    private void Die()
    {
        animator.SetTrigger("die");
        GameManager.Instance.ResetGame();
    }

    private void OnReset()
    {
        foreach(var heart in hearts)
        {
            heart.enabled = true;
        }
        currentHealth = 2;
    }
}
