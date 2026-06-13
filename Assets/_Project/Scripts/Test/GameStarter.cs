using UnityEngine;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        PhotonNetworkManager.Instance.Connect();
    }
}