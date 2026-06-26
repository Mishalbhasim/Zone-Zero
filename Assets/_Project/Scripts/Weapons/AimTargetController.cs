using UnityEngine;
using Photon.Pun;


public class AimTargetController : MonoBehaviourPun
{
    [SerializeField] private Transform _aimTarget;
    [SerializeField] private float _aimDistance = 50f;
    [SerializeField] private bool _isAiming = false;

    private Camera _mainCamera;

    void Start()
    {
        if (!photonView.IsMine) return;
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (_aimTarget == null) return;
        if (_mainCamera == null) return;

        if (_isAiming)
        {
            // move aim target to where camera is looking
            Vector3 aimPoint = _mainCamera.transform.position +
                               _mainCamera.transform.forward * _aimDistance;
            _aimTarget.position = aimPoint;
        }
    }

    public void SetAiming(bool aiming)
    {
        _isAiming = aiming;
    }
}
