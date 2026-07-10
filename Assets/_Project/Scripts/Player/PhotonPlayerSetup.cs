using UnityEngine;
using Photon.Pun;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.InputSystem;
#endif

public class PhotonPlayerSetup : MonoBehaviourPun
{
    private PlayerStateMachine _psm;

    void Awake()
    {
        if (!photonView.IsMine)
        {
            var tpc = GetComponent<ThirdPersonController>();
            if (tpc != null) tpc.enabled = false;
            var psm = GetComponent<PlayerStateMachine>();
            if (psm != null) psm.enabled = false;

            // FIX: lock ragdoll bones kinematic for remote clone too
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = true;
            return;
        }

        // wire camera for local player
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            var camRoot = transform.Find("PlayerCameraRoot");
            if (camRoot != null)
            {
                vcam.Follow = camRoot;
                vcam.LookAt = camRoot;
            }
        }

        // wire ThirdPersonController camera target
        var tpc2 = GetComponent<ThirdPersonController>();
        if (tpc2 != null)
        {
            var camRoot = transform.Find("PlayerCameraRoot");
            if (camRoot != null)
                tpc2.CinemachineCameraTarget = camRoot.gameObject;
        }

        // mobile input
        var canvasInput = FindObjectOfType<UICanvasControllerInput>();
        if (canvasInput != null)
        {
            var starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            if (starterAssetsInputs != null)
                canvasInput.starterAssetsInputs = starterAssetsInputs;
        }

        // wire shoot button
        _psm = GetComponent<PlayerStateMachine>();
        var buttons = FindObjectsOfType<UnityEngine.UI.Button>();
        foreach (var button in buttons)
        {
            if (button.gameObject.name == "ShootButton")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => _psm.TryShoot());
            }
        }

        // lock cursor for PC
#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    void Update()
    {
        if (!photonView.IsMine) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current.leftButton.isPressed)
            _psm?.TryShoot();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
#endif
    }


}