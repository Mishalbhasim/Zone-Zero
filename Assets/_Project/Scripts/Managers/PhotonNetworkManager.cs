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
    public string SelectedRegion { get; set; } = "in"; // defaultis India

    public string CurrentRoomCode { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected) return;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = SelectedRegion;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        
    }

    public void JoinOrCreateRoom()
    {
        MapSeed = Random.Range(0, 99999);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnJoinRandomFailed(short code, string msg)
    {
        // if no room then create new room
        var options = new RoomOptions { MaxPlayers = 30 };
        string roomName = $"ZZ_{Random.Range(1000, 9999)}";
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    //private room with code

    public void CreatePrivateRoom()
    {
        MapSeed = Random.Range(0, 99999);
        CurrentRoomCode = GenerateRoomCode();

        var options = new RoomOptions
        {
            MaxPlayers = 30,
            IsVisible = false,   // hidden from public random matchmaking
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(CurrentRoomCode, options, TypedLobby.Default);
        
    }

    public void JoinRoomByCode(string code)
    {
        PhotonNetwork.JoinRoom(code);
    }

    private string GenerateRoomCode()
    {
        
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new System.Text.StringBuilder();
        for (int i = 0; i < 6; i++)
            code.Append(chars[Random.Range(0, chars.Length)]);
        return code.ToString();
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    //Callbacks
    public void OnConnected() { }

    public void OnConnectedToMaster()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        
    }

    public void OnJoinedRoom()
    {
      
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

        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
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

    public void OnCreatedRoom()
    {
        Debug.Log("[PhotonNetworkManager] Room created successfully.");
        
    }

    public void OnCreateRoomFailed(short code, string msg)
        => Debug.LogError($"[PhotonNetworkManager] Create room failed: {msg}");

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