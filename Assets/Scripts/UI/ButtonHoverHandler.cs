using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    private RectTransform _rectTransform;
    [SerializeField] private float scaleChange = 0.1f;
    [SerializeField] private float timeToScale = .25f;
    [SerializeField] private AudioClip defaultSelectSound;
    [SerializeField] private AudioClip selectSound;
    private Vector3 _originalScale;
    [SerializeField] private bool doesScale = false;

    /// <summary>
    /// gets variables
    /// </summary>
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = transform.localScale;
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
        IncreaseButtonSize();

        PlaySelectSound();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DecreaseButtonSize();

        PlaySelectSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlaySelectSound();
    }

    /// <summary>
    /// Increases button size animation
    /// </summary>
    private void IncreaseButtonSize()
    {
        _rectTransform.localScale = _originalScale;
        _rectTransform.DOScale(_rectTransform.localScale.x + scaleChange, timeToScale);
    }

    /// <summary>
    /// decreases button size animation
    /// </summary>
    private void DecreaseButtonSize()
    {
        _rectTransform.localScale = _originalScale * (1 + scaleChange);
        _rectTransform.DOScale(_originalScale, timeToScale);
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