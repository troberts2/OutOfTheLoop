using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CosmeticManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Animator playerAnimator;

    [Header("Hat Layers")]
    [SerializeField] private SpriteRenderer hatRenderer;
    [SerializeField] private Animator hatAnimator;

    [Header("Shirt Layers")]
    [SerializeField] private SpriteRenderer shirtRenderer;
    [SerializeField] private Animator shirtAnimator;

    [Header("Costume Layers")]
    [SerializeField] private SpriteRenderer costumeRenderer;
    [SerializeField] private Animator costumeAnimator;

    [Header("Trail")]
    [SerializeField] private ParticleSystem trailPS;

    [Header("Current Cosmetics")]
    public Hat currentHat;
    public Shirt currentShirt;
    public Costume currentCostume;
    public Sprite currentTrail;

    public enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;
    private Vector2 currentMoveInput;
    private float playerSpeed;
    private bool isInvincible = false;

    private void OnEnable()
    {
        PlayerCollision.OnPlayerHurt += OnPlayerHurt;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        PlayerCollision.OnPlayerHurt -= OnPlayerHurt;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateSprites();
    }

    private void OnPlayerHurt()
    {
        StartCoroutine(IFrames());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        currentHat = GameManager.Instance.currentHat;
        currentShirt = GameManager.Instance.currentShirt;
        currentTrail = GameManager.Instance.currentTrail;
    }

    public void SetHat(Hat newHat)
    {
        currentHat = newHat;
        currentCostume = null;
        UpdateSprites();
    }

    public void SetShirt(Shirt newShirt)
    {
        currentShirt = newShirt;
        currentCostume = null;
        UpdateSprites();
    }

    public void SetCostume(Costume newCostume)
    {
        currentCostume = newCostume;
        currentHat = null;
        currentShirt = null;
        UpdateSprites();
    }

    public void SetDirection(Direction dir, Vector2 moveInput, float speed)
    {
        currentDirection = dir;
        currentMoveInput = moveInput;
        playerSpeed = speed;
        UpdateSprites();
    }

    private void UpdateSprites()
    {
        // Costume overrides everything
        if (currentCostume != null)
        {
            ApplyCosmetic(costumeRenderer, costumeAnimator, currentCostume.type, currentCostume.sprites, currentCostume.animations);
            hatRenderer.sprite = null; hatAnimator.runtimeAnimatorController = null;
            //shirtRenderer.sprite = null; shirtAnimator.runtimeAnimatorController = null;
            return;
        }

        costumeRenderer.sprite = null;
        costumeAnimator.runtimeAnimatorController = null;

        if (currentHat != null)
            ApplyCosmetic(hatRenderer, hatAnimator, currentHat.type, currentHat.sprites, currentHat.animations);
        else
        {
            hatRenderer.sprite = null;
            hatAnimator.runtimeAnimatorController = null;
        }

        if (currentShirt != null)
        {
            CheckWhatWeightToEnable(currentShirt);
            ApplyCosmetic(shirtRenderer, shirtAnimator, currentShirt.type, currentShirt.sprites, currentShirt.animations);
        }
        else
        {
            CheckWhatWeightToEnable(currentShirt);
            shirtRenderer.sprite = null;
        }

        if(currentTrail !=null)
        {
            ApplyTrail();
        }
        else
        {
            trailPS.Stop();
            trailPS.textureSheetAnimation.SetSprite(0, null);
        }
    }

    private void ApplyTrail()
    {
        trailPS.textureSheetAnimation.SetSprite(0, currentTrail);
        if (playerSpeed > 0)
        {
            if (!trailPS.isPlaying) trailPS.Play();
            SetTrailRotation();
        }
        else
        {
            trailPS.Stop();
        }
    }

    private void ApplyCosmetic(SpriteRenderer sr, Animator anim, CosmeticType type,
                               DirectionalSprites sprites, DirectionalAnimations anims)
    {
        if(!isInvincible)   sr.enabled = (type == CosmeticType.Sprite || type == CosmeticType.Animation);
        anim.enabled = (type == CosmeticType.Animation);

        switch (type)
        {
            case CosmeticType.Sprite:
                sr.sprite = GetSprite(sprites, currentDirection);
                anim.runtimeAnimatorController = null;
                break;

            case CosmeticType.Animation:
                if (currentMoveInput.x != 0)
                {
                    sr.flipX = currentMoveInput.x < 0;
                }
                break;
        }
    }

    private Sprite GetSprite(DirectionalSprites ds, Direction dir)
    {
        return dir switch
        {
            Direction.Up => ds.up,
            Direction.Down => ds.down,
            Direction.Left => ds.left,
            Direction.Right => ds.right,
            _ => ds.down
        };
    }

    private IEnumerator IFrames()
    {
        isInvincible = true;

        float elapsed = 0f;
        float iFrameDuration = 1.5f;

        while (elapsed < iFrameDuration)
        {
            // How far through the i-frames we are
            float t = elapsed / iFrameDuration;

            // Blink speed ramps up as we approach the end
            float currentBlinkInterval = Mathf.Lerp(.1f * 2f, .1f * 0.25f, t);

            // Toggle visibility
            if(currentShirt !=null)
            {
                shirtRenderer.enabled = !shirtRenderer.enabled;
            }
            if(currentHat != null)
            {
                hatRenderer.enabled = !hatRenderer.enabled;
            }

            yield return new WaitForSeconds(currentBlinkInterval);
            elapsed += currentBlinkInterval;
        }

        // Make sure sprite is visible again
        if(currentShirt !=null)
        {
            shirtRenderer.enabled = true;
        }
        
        if(currentHat !=null)
        {
            hatRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void CheckWhatWeightToEnable(Shirt currentShirt)
    {
        if(currentShirt == null)
        {
            DisableAllShirts();
            return;
        }

        string shirt = currentShirt.shirtName;
        switch(shirt)
        {
            case "Suit":
                SetLayerWeight("Suit", 1);
                break;
            default:
                DisableAllShirts();
                break;
        }
    }

    private void DisableAllShirts()
    {
        //default disable all shirts when none selected
        SetLayerWeight("Suit", 0);
        shirtRenderer.sprite = null;
        shirtRenderer.enabled = false;
    }

    private void SetLayerWeight(string layerName, float weight)
    {
        int layerIndex = playerAnimator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            playerAnimator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            Debug.LogWarning($"Layer '{layerName}' not found on {playerAnimator.gameObject.name}");
        }
    }

    private void SetTrailRotation()
    {
        if (currentMoveInput.sqrMagnitude > 0.01f)
        {
            // Get direction *away* from movement
            Vector2 awayDir = -currentMoveInput.normalized;

            // Calculate angle in degrees
            float angle = Mathf.Atan2(awayDir.y, awayDir.x) * Mathf.Rad2Deg;

            // Smooth rotate toward that angle
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            trailPS.transform.rotation = targetRotation;
        }
    }
}
