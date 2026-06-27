using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : Singleton<PhotonNetworkManager>,
                                    IConnectionCallbacks,
                                    IMatchmakingCallbacks
{
    public int MapSeed { get; private set; }
    public bool IsConnected => PhotonNetwork.IsConnected;
    public bool IsInRoom => PhotonNetwork.InRoom;
    public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

    [SerializeField] private GameObject _playerPrefab;

    protected override void Awake()
    {
        base.Awake();
        PhotonNetwork.AddCallbackTarget(this);

       
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected) return;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[PhotonNetworkManager] Connecting...");
    }

    public void JoinOrCreateRoom()
    {
        MapSeed = Random.Range(0, 99999);
        // try joining random room first
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnJoinRandomFailed(short code, string msg)
    {
        Debug.Log("[PhotonNetworkManager] No room found → creating new room");
        var options = new RoomOptions { MaxPlayers = 30 };
        string roomName = $"ZZ_{Random.Range(1000, 9999)}";
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }



    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    // ── Callbacks ────────────────────────────
    public void OnConnected() { }

    public void OnConnectedToMaster()
    {
        
       
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonNetworkManager] Disconnected: {cause}");
    }

    public void OnJoinedRoom()
    {
        Debug.Log($"[PhotonNetworkManager] Joined room. Seed: {MapSeed}");

        if (PhotonNetwork.IsMasterClient)
        {
            var props = new ExitGames.Client.Photon.Hashtable();
            props["seed"] = MapSeed;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("seed", out object seed))
                MapSeed = (int)seed;
        }
    }

    // called when Arena scene loads
    public void OnArenaLoaded()
    {
        SpawnLocalPlayer();
        BotManager.Instance?.SpawnBots(MapSeed, PhotonNetwork.CurrentRoom.PlayerCount);
    }


    private void SpawnLocalPlayer()
    {
        Vector3 spawnPos = SpawnManager.Instance.GetRandomSpawnPoint();
        PhotonNetwork.Instantiate(
            _playerPrefab.name,
            spawnPos,
            Quaternion.identity
        );
    }
    public void OnJoinRoomFailed(short code, string msg)
        => Debug.LogError($"[PhotonNetworkManager] Join failed: {msg}");

    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short code, string msg) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> list) { }
    
    public void OnRegionListReceived(RegionHandler rh) { }
    public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> d) { }
    public void OnCustomAuthenticationFailed(string msg) { }

    protected override void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}