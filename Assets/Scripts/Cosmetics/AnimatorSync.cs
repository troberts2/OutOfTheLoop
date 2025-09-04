using UnityEngine;

public class AnimatorSync : MonoBehaviour
{
    [SerializeField] private Animator mainAnimator;  // Player
    [SerializeField] private Animator childAnimator; // Shirt/Hat

    void LateUpdate()
    {
        AnimatorStateInfo state = mainAnimator.GetCurrentAnimatorStateInfo(0);
        childAnimator.Play(state.shortNameHash, 0, state.normalizedTime);
        childAnimator.speed = mainAnimator.speed;
    }
}