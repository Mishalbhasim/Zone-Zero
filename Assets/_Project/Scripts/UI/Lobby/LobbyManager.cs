using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _timerLabel;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Transform _playerListContent;

    [Header("Player List Item")]
    [SerializeField] private GameObject _playerListItemPrefab;

    [Header("Settings")]
    [SerializeField] private int _countdownSeconds = 20;
    [SerializeField] private int _minPlayers = 1;
    [SerializeField] private string _arenaScene = "Arena_01";
    [SerializeField] private string _mainMenuScene = "MainMenu";

    private float _timer;
    private bool _countingDown = false;
    private bool _matchStarting = false;

    void Start()
    {
        _countingDown = false;
        _matchStarting = false;
        _timer = _countdownSeconds;
        _cancelButton?.onClick.AddListener(OnCancelClicked);

        if (!PhotonNetwork.IsConnectedAndReady)
            UpdateUI();
        else if (!PhotonNetwork.InRoom)
            PhotonNetworkManager.Instance?.JoinOrCreateRoom();
    }

    void Update()
    {
        if (!_countingDown || _matchStarting) return;

        _timer -= Time.deltaTime;
        _timerText.text = Mathf.CeilToInt(_timer).ToString();

        if (_timer <= 5f)
            _timerText.color = new Color(1f, 0.24f, 0.24f);
        else
            _timerText.color = new Color(0f, 0.898f, 1f);

        if (_timer <= 0f)
            StartMatch();
    }

    // Photon Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log($"[LobbyManager] Joined room. Players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdateUI();
        RefreshPlayerList();
        Invoke(nameof(TryStartCountdown), 0.5f);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[LobbyManager] Player joined: {newPlayer.NickName}");
        UpdateUI();
        RefreshPlayerList();

        if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            StartMatch();

        TryStartCountdown();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        RefreshPlayerList();
    }

    // UI

    private void UpdateUI()
    {
        if (!PhotonNetwork.InRoom) return;
        int current = PhotonNetwork.CurrentRoom.PlayerCount;
        int max = PhotonNetwork.CurrentRoom.MaxPlayers;
        _playerCountText.text = $"{current}/{max}";
    }

    private void RefreshPlayerList()
    {
        if (_playerListContent == null) return;

        for (int i = _playerListContent.childCount - 1; i >= 0; i--)
            Destroy(_playerListContent.GetChild(i).gameObject);

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (_playerListItemPrefab != null)
            {
                var item = Instantiate(_playerListItemPrefab, _playerListContent);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string name = string.IsNullOrEmpty(player.NickName)
                        ? $"Player_{player.ActorNumber}"
                        : player.NickName;
                    text.text = player.IsMasterClient ? $"★ {name}" : name;
                }
            }
        }
    }

    // Countdown

    private void TryStartCountdown()
    {
        Debug.Log($"[LobbyManager] TryStartCountdown called. IsMaster: {PhotonNetwork.IsMasterClient}");
        if (_countingDown) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= _minPlayers)
        {
            Debug.Log("[LobbyManager] Master sending RPC_StartCountdown");
            photonView.RPC("RPC_StartCountdown", RpcTarget.AllBuffered, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    private void RPC_StartCountdown(double startTimestamp)
    {
        Debug.Log("[LobbyManager] RPC_StartCountdown received");
        if (_countingDown) return;
        _countingDown = true;
        double elapsed = PhotonNetwork.Time - startTimestamp;
        _timer = _countdownSeconds - (float)elapsed;
        _timerLabel.text = "MATCH STARTING IN";
    }

    // Match Start

    private void StartMatch()
    {
        if (_matchStarting) return;
        _matchStarting = true;

        Debug.Log("[LobbyManager] Starting match!");
        _timerText.text = "GO!";
        _timerLabel.text = "";

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(_arenaScene);
    }

    // Buttons

    private void OnCancelClicked()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(_mainMenuScene);
    }
}