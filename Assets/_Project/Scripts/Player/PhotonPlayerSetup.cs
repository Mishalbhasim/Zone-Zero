using UnityEngine;
using Photon.Pun;

public class PhotonPlayerSetup : MonoBehaviourPun
{
    void Awake()
    {
        if (!photonView.IsMine)
        {
            // disable control scripts on remote players
            var tpc = GetComponent<StarterAssets.ThirdPersonController>();
            if (tpc != null) tpc.enabled = false;

            var psm = GetComponent<PlayerStateMachine>();
            if (psm != null) psm.enabled = false;

            return;
        }

        // local player setup
        var transformView = GetComponent<PhotonTransformView>();
        if (transformView != null)
            photonView.ObservedComponents.Add(transformView);

        var animView = GetComponent<PhotonAnimatorView>();
        if (animView != null)
            photonView.ObservedComponents.Add(animView);
    }
}