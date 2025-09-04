using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiplierText : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI multiplierText;
    public float popScale = 1.2f;
    public float popDuration = 0.3f;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 10f;
    public int shakeVibrato = 10;
    [SerializeField] private int maxMultiple = 32;

    public int currentMultiplier = 1;
    private Vector3 originalScale;

    public float pulseScale = 1.05f; // How big it gets during idle pulse
    public float pulseDuration = 0.6f; // Time to grow or shrink

    private Tween activeTween;
    private Tween pulseTween;

    void Awake()
    {
        if (multiplierText == null)
            multiplierText = GetComponent<TextMeshProUGUI>();

        originalScale = multiplierText.transform.localScale;
        multiplierText.enabled = false;
    }

    private void OnEnable()
    {
        PlayerCollision.OnPlayerHurt += HalfMultiplier;
        PlayerCollision.OnPlayerDeath += OnGameReset;
        GameManager.OnGameReset += OnGameReset;
    }

    private void OnDisable()
    {
        PlayerCollision.OnPlayerHurt -= HalfMultiplier;
        PlayerCollision.OnPlayerDeath -= OnGameReset;
        GameManager.OnGameReset -= OnGameReset;
    }

    private void OnGameReset()
    {
        currentMultiplier = 1;
        DisableMultiplier();
    }

    public void SetMultiplier(int multiplier)
    {
        if (multiplier <= 1)
        {
            DisableMultiplier();
            return;
        }

        multiplierText.text = "x" + Mathf.Min(currentMultiplier * multiplier, maxMultiple);

        if (currentMultiplier <= 1)
        {
            ShowMultiplier();
        }
        else
        {
            ShakeMultiplier(true);
        }

        currentMultiplier = Mathf.Min(currentMultiplier * multiplier, maxMultiple);
    }

    private void HalfMultiplier()
    {
        if(currentMultiplier > 1)
        {
            currentMultiplier /= 2;
        }

        multiplierText.text = "x" + currentMultiplier.ToString();

        if (currentMultiplier <= 1)
        {
            DisableMultiplier();
        }
        else
        {
            ShakeMultiplier(false);
        }
    }

    private void ShowMultiplier()
    {
        multiplierText.enabled = true ;
        multiplierText.transform.localScale = Vector3.zero;
        multiplierText.color = Color.yellow;

        KillTweens();

        activeTween = multiplierText.transform
            .DOScale(originalScale * popScale, popDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                multiplierText.transform.DOScale(originalScale, 0.15f)
                    .OnComplete(StartPulse);
            });
    }

    private void ShakeMultiplier(bool isGood)
    {
        KillTweens();

        if(isGood)
        {
            multiplierText.DOColor(Color.white, shakeDuration);
        }
        else
        {
            multiplierText.DOColor(Color.red, shakeDuration);
        }    
        
        activeTween = multiplierText.transform
            .DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeStrength), shakeVibrato, 90, false)
            .OnComplete(() =>
            {
                multiplierText.transform.rotation = Quaternion.identity;
                multiplierText.color = Color.yellow;
                StartPulse();
            });
    }

    public void DisableMultiplier()
    {
        KillTweens();

        multiplierText.DOColor(Color.red, popDuration);
        activeTween = multiplierText.transform
            .DOScale(Vector3.zero, popDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                multiplierText.enabled = false;
                multiplierText.transform.localScale = originalScale;
                multiplierText.transform.rotation = Quaternion.identity;
                currentMultiplier = 1;
            });
    }

    private void StartPulse()
    {
        KillPulse();
        pulseTween = multiplierText.transform
            .DOScale(originalScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void KillTweens()
    {
        activeTween?.Kill();
        KillPulse();
    }

    private void KillPulse()
    {
        pulseTween?.Kill();
        multiplierText.transform.localScale = originalScale;
    }
}
