
using UnityEngine;

[CreateAssetMenu(menuName = "Cosmetic/Costume")]
public class Costume : ScriptableObject
{
    public string costumeName;
    public CosmeticType type;
    public DirectionalSprites sprites;
    public DirectionalAnimations animations;
}
