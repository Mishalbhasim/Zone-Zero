using UnityEngine;
using Photon.Pun;
using Cinemachine;
using StarterAssets;

public class PhotonPlayerSetup : MonoBehaviourPun
{
    void Awake()
    {
        if (!photonView.IsMine)
        {
            // disable control on remote players
            var tpc = GetComponent<ThirdPersonController>();
            if (tpc != null) tpc.enabled = false;

            var psm = GetComponent<PlayerStateMachine>();
            if (psm != null) psm.enabled = false;

            return;
        }

        // local player → wire camera
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


        //mobile input
        var canvasInput = FindObjectOfType<UICanvasControllerInput>();
        if (canvasInput != null)
        {
            var starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            if (starterAssetsInputs != null)
                canvasInput.starterAssetsInputs = starterAssetsInputs;
        }
        // wire shoot button
        var buttons = FindObjectsOfType<UnityEngine.UI.Button>();
        foreach (var button in buttons)
        {
            if (button.gameObject.name == "ShootButton")
            {
                button.onClick.RemoveAllListeners();
                var psm = GetComponent<PlayerStateMachine>();
                button.onClick.AddListener(() => psm.TryShoot());
            }
        }
    }
}