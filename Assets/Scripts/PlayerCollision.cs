using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class PlayerCollision : MonoBehaviour
{
    private int currentHealth = 2;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip playerHurt;
    [SerializeField] private AudioClip death;
    private BoxCollider2D boxCollider;

    public static event Action OnPlayerDeath;

    private void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        GameManager.OnGameReset += OnReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnReset;
    }

    public void TakeDamage()
    {
        currentHealth--;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i <= currentHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }

        foreach (var heart in hearts)
        {
            if (heart.enabled)
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

        if (currentHealth < 0)
        {
            Die();
            return;
        }

        AudioManager.Instance.PlaySound(playerHurt);
        animator.SetTrigger("takeHit");
    }

    private void Die()
    {
        animator.SetTrigger("die");
        AudioManager.Instance.PlaySound(death);
        boxCollider.enabled = false;
        OnPlayerDeath?.Invoke();
    }

    private void OnReset()
    {
        foreach(var heart in hearts)
        {
            heart.enabled = true;
        }
        currentHealth = 2;
        boxCollider.enabled = true;
    }
}
