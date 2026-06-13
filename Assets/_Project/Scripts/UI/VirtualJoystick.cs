using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler,IPointerUpHandler
{

    [Header("Joystick Settings")]
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private float joystickRadius = 80f;
    

    public Vector2 InputDirection { get; private set; }
    public bool IsPressed { get; private set; }

    private RectTransform _rectTransform;
    private Vector2 _centerPosition;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
        _centerPosition = _rectTransform.position;
        OnDrag(eventData);

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - _centerPosition;
        Vector2 clampedDirection = Vector2.ClampMagnitude(direction, joystickRadius);
        joystickHandle.position = _centerPosition + clampedDirection;
        InputDirection = clampedDirection / joystickRadius;
        Debug.Log($"[Joystick] Input: {InputDirection}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputDirection = Vector2.zero;
        IsPressed = false;
        joystickHandle.localPosition = Vector2.zero;
    }
}
