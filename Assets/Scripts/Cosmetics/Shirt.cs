
using UnityEngine;

[CreateAssetMenu(menuName = "Cosmetic/Shirt")]
public class Shirt : ScriptableObject
{
    public string shirtName;
    public CosmeticType type;
    public DirectionalSprites sprites;
    public DirectionalAnimations animations;
}
