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
    [SerializeField] private AudioClip playerHeal;
    [SerializeField] private float freezeDuration = 0.1f; // How long the freeze lasts
    [SerializeField] private AudioClip multiplierPickup;
    private BoxCollider2D boxCollider;

    public static event Action OnPlayerDeath;
    public static event Action OnPlayerHurt;

    private void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        GameManager.OnGameReset += OnReset;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnReset;
    }

    public void HealPlayer()
    {
        currentHealth++;

        UpdateHeartUI();

        currentHealth = Mathf.Min(currentHealth, 2);

        AudioManager.Instance.PlaySound(playerHeal);
    }

    public void TakeDamage()
    {
        OnPlayerHurt?.Invoke();
        currentHealth--;

        UpdateHeartUI();

        if (currentHealth < 0)
        {
            Die();
            return;
        }

        AudioManager.Instance.PlaySound(playerHurt);
        TriggerHitStop();
        PixelPerfectShake.Instance.Shake();
        animator.SetTrigger("takeHit");
    }

    private void UpdateHeartUI()
    {
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
    }

    private void Die()
    {
        TriggerHitStop();
        PixelPerfectShake.Instance.Shake();
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

    private void TriggerHitStop()
    {
        StartCoroutine(DoFreezeFrame());
    }

    private IEnumerator DoFreezeFrame()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;

        // Wait in *real* time so it's not affected by timeScale
        yield return new WaitForSecondsRealtime(freezeDuration);

        Time.timeScale = originalTimeScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("multiplier2x"))
        {
            GameManager.Instance.multText.SetMultiplier(2);
            ParticleSystemPool.Instance.PlayMultiplierParticleSystem(collision.transform.position);
            AudioManager.Instance.PlaySound(multiplierPickup);
            collision.GetComponent<Multiplier>().TurnOff();
        }
    }
}
