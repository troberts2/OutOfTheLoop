using UnityEditor.Animations;
using UnityEngine;

public class CosmeticManager : MonoBehaviour
{
    [Header("Hat Layers")]
    [SerializeField] private SpriteRenderer hatRenderer;
    [SerializeField] private Animator hatAnimator;

    [Header("Shirt Layers")]
    [SerializeField] private SpriteRenderer shirtRenderer;
    [SerializeField] private Animator shirtAnimator;

    [Header("Costume Layers")]
    [SerializeField] private SpriteRenderer costumeRenderer;
    [SerializeField] private Animator costumeAnimator;

    [Header("Current Cosmetics")]
    [SerializeField] private Hat currentHat;
    [SerializeField] private Shirt currentShirt;
    [SerializeField] private Costume currentCostume;

    public enum Direction { Up, Down, Left, Right }
    private Direction currentDirection = Direction.Down;
    private Vector2 currentMoveInput;
    private float playerSpeed;

    private void Start()
    {
        UpdateSprites();
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
            shirtRenderer.sprite = null; shirtAnimator.runtimeAnimatorController = null;
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
            ApplyCosmetic(shirtRenderer, shirtAnimator, currentShirt.type, currentShirt.sprites, currentShirt.animations);
        else
        {
            shirtRenderer.sprite = null;
            shirtAnimator.runtimeAnimatorController = null;
        }
    }

    private void ApplyCosmetic(SpriteRenderer sr, Animator anim, CosmeticType type,
                               DirectionalSprites sprites, DirectionalAnimations anims)
    {
        sr.enabled = (type == CosmeticType.Sprite || type == CosmeticType.Animation);
        anim.enabled = (type == CosmeticType.Animation);

        switch (type)
        {
            case CosmeticType.Sprite:
                sr.sprite = GetSprite(sprites, currentDirection);
                anim.runtimeAnimatorController = null;
                break;

            case CosmeticType.Animation:
                anim.runtimeAnimatorController = GetAnimator(anims, currentDirection);
                if (playerSpeed > 0.1f)
                    anim.SetFloat("Speed", 1);
                else
                    anim.SetFloat("Speed", 0);
                anim.SetFloat("MoveX", currentMoveInput.x);
                anim.SetFloat("MoveY", currentMoveInput.y);
                sr.sprite = null;
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

    private AnimatorController GetAnimator(DirectionalAnimations da, Direction dir)
    {
        return da.controller;
    }
}
