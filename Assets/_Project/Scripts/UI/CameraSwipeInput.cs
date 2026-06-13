using UnityEngine;
using UnityEngine.EventSystems;

public class CameraSwipeInput : MonoBehaviour,
                                IPointerDownHandler,
                                IDragHandler,
                                IPointerUpHandler
{
    [SerializeField] private ThirdPersonCamera _camera;
    [SerializeField] private Transform _player;
    [SerializeField] private float _horizontalSensitivity = 0.15f;
    [SerializeField] private float _verticalSensitivity = 0.1f;

    private Vector2 _lastPosition;
    private bool _isTouching;

    public void OnPointerDown(PointerEventData e)
    {
        _lastPosition = e.position;
        _isTouching = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_isTouching) return;

        Vector2 delta = e.position - _lastPosition;

        // rotate player horizontally
        if (_player != null)
            _player.Rotate(0, delta.x * _horizontalSensitivity, 0);

        // tilt camera vertically
        if (_camera != null)
            _camera.RotateVertical(delta.y * _verticalSensitivity);

        _lastPosition = e.position;
    }

    public void OnPointerUp(PointerEventData e)
    {
        _isTouching = false;
    }
}