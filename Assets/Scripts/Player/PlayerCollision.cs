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

    private Camera mainCam;
    [SerializeField] private Canvas playerUICanvas;

    [SerializeField] private float iFrameDuration = 2f;
    [SerializeField] private float blinkInterval = 0.2f;

    private bool isInvincible = false;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDeath;
    public static event Action OnPlayerHurt;

    private void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        GameManager.OnGameReset += OnReset;
        AdManager.OnPlayerContinueReward += OnReset;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnReset;
        AdManager.OnPlayerContinueReward -= OnReset;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        mainCam = Camera.main;
        playerUICanvas.worldCamera = mainCam;
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
        if (isInvincible)
        {
            return;
        }

        currentHealth--;

        UpdateHeartUI();

        if (currentHealth < 0)
        {
            Die();
            return;
        }

        OnPlayerHurt?.Invoke();
        AudioManager.Instance.PlaySound(playerHurt);
        TriggerHitStop();
        PixelPerfectShake.Instance.Shake();
        // Trigger iFrames
        StartCoroutine(IFrames());
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
        Time.timeScale = 0.0f;

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

            if(SceneManager.GetActiveScene().name == "Tutorial")
            {
                FindAnyObjectByType<TutorialManager>().multipliersCollected++;
            }

            GetComponentInChildren<MultiplierPointer>().target = null;
        }
    }

    private IEnumerator IFrames()
    {
        isInvincible = true;

        float elapsed = 0f;

        while (elapsed < iFrameDuration)
        {
            // How far through the i-frames we are
            float t = elapsed / iFrameDuration;

            // Blink speed ramps up as we approach the end
            float currentBlinkInterval = Mathf.Lerp(blinkInterval * 2f, blinkInterval * 0.25f, t);

            // Toggle visibility
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(currentBlinkInterval);
            elapsed += currentBlinkInterval;
        }

        // Make sure sprite is visible again
        spriteRenderer.enabled = true;

        isInvincible = false;
    }
}
