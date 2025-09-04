using UnityEngine;

public class ChildSnapper : MonoBehaviour
{
    [SerializeField] private Transform childObject;
    [SerializeField] private Animator bodyAnimator;

    // Predefined offsets per frame
    public Vector2[] frameOffsets;

    public void UpdateChildPosition(int frameIndex)
    {
        if (frameIndex < frameOffsets.Length)
        {
            Vector2 offset = frameOffsets[frameIndex];
            childObject.localPosition = offset;
        }
    }
}

