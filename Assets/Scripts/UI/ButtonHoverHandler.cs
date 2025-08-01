using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [SerializeField] private float timeToFade = .1f;
    [SerializeField] private AudioClip defaultSelectSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private Color fadeToColor;
    [SerializeField] private bool fadeColor = true;

    private Image targetImage;
    private Color originalColor;
    private Tween fadeTween;

    /// <summary>
    /// gets variables
    /// </summary>
    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>(); // fallback

        if(targetImage)
        {
            originalColor = targetImage.color;
        }
        else
        {
            originalColor = Color.white;
        }
    }

    /// <summary>
    /// called on button hover
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //set event system selected
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    /// <summary>
    /// called on button hover exit
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        //set event system selected
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(fadeColor)
        {
            FadeTo(fadeToColor);
        }

        PlaySelectSound();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (fadeColor)
        {
            FadeTo(originalColor);
        }

        PlaySelectSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlaySelectSound();
    }

    private void FadeTo(Color targetColor)
    {
        // Kill previous tween if still active
        fadeTween?.Kill();

        fadeTween = targetImage.DOColor(targetColor, timeToFade).SetEase(Ease.OutQuad);
    }

    private void PlaySelectSound()
    {
        //ui sound
        if (selectSound != null)
        {
            AudioManager.Instance.PlaySound(selectSound);
        }
        else
        {
            AudioManager.Instance.PlaySound(defaultSelectSound);
        }
    }
}