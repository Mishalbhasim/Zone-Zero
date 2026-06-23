using UnityEngine;

public class ArenaLoader : MonoBehaviour
{
    void Start()
    {
        PhotonNetworkManager.Instance?.OnArenaLoaded();
    }
}