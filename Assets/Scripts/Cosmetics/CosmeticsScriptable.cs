using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public class DirectionalSprites
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;
}

[System.Serializable]
public class DirectionalAnimations
{
    public AnimatorController controller;
}

public enum CosmeticType
{
    Sprite,
    Animation
}
