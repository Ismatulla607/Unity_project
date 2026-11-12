using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField] private float radius = 70f;

    public Vector2 Value { get; private set; }

    private RectTransform background;
    private int? activePointer;

    private void Awake()
    {
        background = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointer.HasValue)
            return;

        activePointer = eventData.pointerId;
        UpdateHandle(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activePointer != eventData.pointerId)
            return;

        UpdateHandle(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointer != eventData.pointerId)
            return;

        ResetJoystick();
    }

    private void UpdateHandle(PointerEventData eventData)
    {
        if (handle == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            Vector2 offset = localPoint - background.rect.center;
            float safeRadius = Mathf.Max(radius, 1f);

            Value = Vector2.ClampMagnitude(offset / safeRadius, 1f);
            handle.anchoredPosition = Value * safeRadius;
        }
    }

    private void ResetJoystick()
    {
        activePointer = null;
        Value = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        ResetJoystick();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ResetJoystick();
    }
}