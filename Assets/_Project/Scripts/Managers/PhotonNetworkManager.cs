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
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[PhotonNetworkManager] Connecting...");
    }

    public void JoinOrCreateRoom()
    {
        MapSeed = Random.Range(0, 99999);

        var options = new RoomOptions
        {
            MaxPlayers = 30
        };

        PhotonNetwork.JoinOrCreateRoom(
            "ZoneZero_Dev",
            options,
            TypedLobby.Default
        );
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    // ── Callbacks ────────────────────────────
    public void OnConnected() { }

    public void OnConnectedToMaster()
    {
        Debug.Log("[PhotonNetworkManager] Connected.");
        JoinOrCreateRoom();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonNetworkManager] Disconnected: {cause}");
    }

    public void OnJoinedRoom()
    {
        Debug.Log($"[PhotonNetworkManager] Joined room. Seed: {MapSeed}");

        // master client sets the seed
        if (PhotonNetwork.IsMasterClient)
        {
            var props = new ExitGames.Client.Photon.Hashtable();
            props["seed"] = MapSeed;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            // other players read seed from room
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("seed", out object seed))
                MapSeed = (int)seed;
        }

        // spawn player
        SpawnLocalPlayer();

        //spawn bots ( every client use same seed but localy)
        
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
    public void OnJoinRandomFailed(short code, string msg) { }
    public void OnRegionListReceived(RegionHandler rh) { }
    public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> d) { }
    public void OnCustomAuthenticationFailed(string msg) { }

    protected override void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}