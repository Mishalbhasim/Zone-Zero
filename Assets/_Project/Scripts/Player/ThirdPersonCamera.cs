using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _height = 2f;

    private float _yaw;
    private float _pitch;

    void LateUpdate()
    {
        if (_target == null) return;

        // camera yaw follows player rotation
        _yaw = _target.eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -_distance);

        transform.position = _target.position + Vector3.up * _height + offset;
        transform.LookAt(_target.position + Vector3.up * 2f);
    }

    public void RotateHorizontal(float delta)
    {
        _yaw += delta;
    }

    public void RotateVertical(float delta)
    {
        _pitch -= delta;
        _pitch = Mathf.Clamp(_pitch, -20f, 40f);
    }
}