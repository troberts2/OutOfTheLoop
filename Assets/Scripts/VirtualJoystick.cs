using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform bg;
    private RectTransform handle;
    private CanvasGroup canvasGroup; // for hiding/showing
    private Vector2 inputVector;

    void Start()
    {
        bg = transform.GetChild(0).GetComponent<RectTransform>();
        handle = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        // Add or get CanvasGroup for fading
        canvasGroup = bg.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = bg.gameObject.AddComponent<CanvasGroup>();

        HideJoystick();
    }

    private void OnEnable()
    {
        PlayerCollision.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        PlayerCollision.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        HideJoystick();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / (bg.sizeDelta.x / 2));
            pos.y = (pos.y / (bg.sizeDelta.y / 2));

            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Move handle
            handle.anchoredPosition = new Vector2(
                inputVector.x * (bg.sizeDelta.x / 3),
                inputVector.y * (bg.sizeDelta.y / 3));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(GameManager.Instance.movementType != MovementType.JoyStick)
        {
            return;
        }

        // Reposition joystick at touch point
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPos);
        bg.anchoredPosition = localPos;

        ShowJoystick();
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameManager.Instance.movementType != MovementType.JoyStick)
        {
            return;
        }

        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        HideJoystick();
    }

    private void ShowJoystick()
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideJoystick()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public Vector2 GetInput() => inputVector;
}
