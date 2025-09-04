using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour
{
    [SerializeField] private float floatDistance = 0.2f;      // How far up/down it moves
    [SerializeField] private float floatDuration = 1f;        // How long one up/down cycle takes
    [SerializeField] private float lifetime = 5f;             // Time before it fades and disappears
    [SerializeField] private float fadeDuration = 1f;         // How long the fade takes

    [SerializeField] private SpriteRenderer spriteRenderer;
    private Tween floatTween;
    private Tween colorTween;
    [SerializeField] private float colorChangeDuration = 0.5f; // Time to fade to next color
    [SerializeField] private BoxCollider2D boxCollider;
    public bool IsActive = false;

    private void OnEnable()
    {
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        floatTween.Kill();
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        TurnOff();
    }


    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void TurnOn(bool isTutorial = false)
    {
        boxCollider.enabled = true;
        spriteRenderer.enabled = true;
        IsActive = true;
        FloatTween(isTutorial);
        StartRainbow();
        Color nextColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 3.4f);
        spriteRenderer.color = nextColor;
    }

    private void FloatTween(bool isTutorial)
    {
        // Floating animation (yoyo loop forever)
        floatTween = transform.DOMoveY(transform.position.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        if(!isTutorial)
        {
            // Schedule fade out after lifetime
            Invoke(nameof(StartFadeOut), lifetime);
        }
    }

    private void StartFadeOut()
    {
        // Stop floating
        floatTween.Kill();

        // Fade alpha to 0
        spriteRenderer.DOFade(0f, fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                TurnOff(false) ;
            });
    }

    public void TurnOff(bool firstTurnOff = false)
    {
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
        floatTween.Kill();
        IsActive = false;
        spriteRenderer.DOFade(1, 0);
    }

    public void StartRainbow()
    {
        colorTween?.Kill();
        PickNextColor();
    }

    private void PickNextColor()
    {
        // Random hue (0–1) with full saturation and brightness
        Color nextColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 3.4f);

        colorTween = spriteRenderer.DOColor(nextColor, colorChangeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(PickNextColor); // Loop by picking new random color
    }
}
