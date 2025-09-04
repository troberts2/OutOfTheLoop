
using UnityEngine;

[CreateAssetMenu(menuName = "Cosmetic/Hat")]
public class Hat : ScriptableObject
{
    public string hatName;
    public CosmeticType type;
    public DirectionalSprites sprites;
    public DirectionalAnimations animations;
}
