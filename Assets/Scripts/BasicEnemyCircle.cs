using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BasicEnemyCircle : MonoBehaviour
{
    [SerializeField] private Transform indicatorCircle;
    [SerializeField] private SpriteRenderer baseCircle;
    [SerializeField] private float colorDuration = 0.5f;
    [SerializeField] private float turnRedDuration = .1f;
    [SerializeField] private float destroyAfterActivateDelay = 0.2f;
    [SerializeField] private CircleCollider2D circleCollider;
    private Sequence rainbowSequence;
    private float scaleDuration;
    [SerializeField] private float scaleDurationMinimum= 0.5f;
    [SerializeField] private float scaleDurationMaximum = 0.1f;
    [SerializeField] private LayerMask playerLayer;
    private Light2D spriteLight;
    private Material spriteMaterialInstance;
    [SerializeField] private ParticleSystem dieParticleSystem;
    [SerializeField] private AudioClip popSound;

    private bool hasDamagedPlayer = false;

    private Color[] rainbowColors = new Color[]
    {
        Color.red,
        new Color(1f, 0.5f, 0f),     // Orange
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.blue,
        new Color(0.6f, 0f, 1f),     // Indigo/Violet
    };

    public bool HasDamagedPlayer { get => hasDamagedPlayer; set => hasDamagedPlayer = value; }

    private void OnEnable()
    {
        GameManager.OnGameReset += OnGameReset;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= OnGameReset;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // Start is called before the first frame update
    private void Start()
    {
        spriteLight = GetComponentInChildren<Light2D>();
        spriteMaterialInstance = new Material(baseCircle.material);
        baseCircle.material = spriteMaterialInstance;
        spriteMaterialInstance.SetColor("_Color", baseCircle.color * 3.4f);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        OnGameReset();
    }

    public void ActivateDeathCircle()
    {
        scaleDuration = Random.Range(scaleDurationMinimum, scaleDurationMaximum);
        StartRandomRainbowLoop();
        GrowIndicator();

        Invoke(nameof(TurnRed), scaleDuration - turnRedDuration);
        Invoke(nameof(ActivateColliderAndDie), scaleDuration);
    }

    private void GrowIndicator()
    {
        indicatorCircle.DOScale(1, scaleDuration);
    }
    private void TurnRed()
    {
        baseCircle.DOColor(Color.red, .1f);
    }

    private void NearMiss()
    {

    }

    private void ActivateColliderAndDie()
    {
        circleCollider.enabled = true;

        // Manually check if overlapping player immediately
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useTriggers = true;

        Collider2D[] results = new Collider2D[5];
        if (circleCollider.OverlapCollider(filter, results) > 0)
        {
            foreach (var col in results)
            {
                if (col != null && col.CompareTag("Player"))
                {
                    col.GetComponent<PlayerCollision>().TakeDamage();
                    HasDamagedPlayer = true;
                }
            }
        }

        GameManager.Instance.AddToScore();
        if(SceneManager.GetActiveScene().name == "GameScene" && GameManager.Instance.isGameStarted)
        {
            AudioManager.Instance.PlaySound(popSound, true);
        }

        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            if(GameManager.Instance.isGameStarted)
            {
                ParticleSystemPool.Instance.PlayParticleSystem(transform.position);
            }
        }
        else //if other scene
        {
            ParticleSystemPool.Instance.PlayParticleSystem(transform.position);
        }
        

        Invoke(nameof(PlayDeathParticle), destroyAfterActivateDelay);
    }

    private void PlayDeathParticle()
    {
        OnGameReset();
    }

    private void StartRandomRainbowLoop()
    {
        rainbowSequence?.Kill();

        // Shuffle the color list
        List<Color> shuffledColors = new List<Color>(rainbowColors);
        Shuffle(shuffledColors);

        rainbowSequence = DOTween.Sequence();

        foreach (Color color in shuffledColors)
        {
            rainbowSequence.Append(
                baseCircle.DOColor(color, colorDuration).SetEase(Ease.Linear)
            );
        }

        // Loop the sequence infinitely, and reshuffle each loop
        rainbowSequence.OnComplete(() =>
        {
            StartRandomRainbowLoop();
        });
    }

    private void StopColorChange()
    {
        rainbowSequence?.Kill(); // Instantly stop the tween
    }

    // Fisher-Yates shuffle
    private void Shuffle(List<Color> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        spriteLight.color = baseCircle.color;
        spriteMaterialInstance.SetColor("_Color", baseCircle.color * 3.4f);
    }

    /// <summary>
    /// resets circle
    /// </summary>
    private void OnGameReset()
    {
        indicatorCircle.DOKill();
        indicatorCircle.localScale = Vector2.zero;

        baseCircle.DOKill();
        rainbowSequence.Kill();
        baseCircle.color = Color.white;

        circleCollider.enabled = false;

        gameObject.SetActive(false);
    }
}
