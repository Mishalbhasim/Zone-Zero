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
        _cancelButton?.onClick.AddListener(OnCancelClicked);
        _timer = _countdownSeconds;

        if (PhotonNetwork.InRoom)
            OnJoinedRoom();
        else if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetworkManager.Instance?.JoinOrCreateRoom();
        else
            UpdateUI();
    }
    void Update()
    {
        if (!_countingDown || _matchStarting) return;

        _timer -= Time.deltaTime;
        _timerText.text = Mathf.CeilToInt(_timer).ToString();

        // change color when urgent
        if (_timer <= 5f)
            _timerText.color = new Color(1f, 0.24f, 0.24f); // red
        else
            _timerText.color = new Color(0f, 0.898f, 1f); // cyan

        if (_timer <= 0f)
            StartMatch();
    }

    // Photon Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log($"[LobbyManager] Joined room. Players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdateUI();
        RefreshPlayerList();
        TryStartCountdown();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[LobbyManager] Player joined: {newPlayer.NickName}");
        UpdateUI();
        RefreshPlayerList();

        // room full → start immediately
        if (PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers)
            StartMatch();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUI();
        RefreshPlayerList();
    }

    // UI Updates

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

        // clear existing
        for (int i = _playerListContent.childCount - 1; i >= 0; i--)
            Destroy(_playerListContent.GetChild(i).gameObject);

        // add player entries
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

    private void TryStartCountdown()
    {
        if (_countingDown) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= _minPlayers)
        {
            _countingDown = true;
            _timerLabel.text = "MATCH STARTING IN";
            Debug.Log("[LobbyManager] Countdown started");
        }
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
        {
            PhotonNetwork.AutomaticallySyncScene = true; 
            PhotonNetwork.LoadLevel(_arenaScene);
        }
    }

    // Buttons

    private void OnCancelClicked()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(_mainMenuScene);
    }
}