using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _friendsButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _closeFriendsPanelButton;
    [SerializeField] private Button _leaderboardButton;

    [Header("Panels")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _friendsPanel;
    [SerializeField] private GameObject _leaderboardPanel;

    [Header("Version")]
    [SerializeField] private TextMeshProUGUI _versionText;

    [Header("Region")]
    [SerializeField] private TMP_Dropdown _regionDropdown;

    [Header("Join Room Panel")]
    [SerializeField] private GameObject _joinRoomPanel;
    [SerializeField] private TMP_InputField _roomCodeInputField;
    [SerializeField] private Button _confirmJoinButton;
    [SerializeField] private Button _closeJoinRoomPanelButton;

    void Start()
    {
        // set version text
        if (_versionText != null)
            _versionText.text = $"v{Application.version}";

        // populate region dropdown
        if (_regionDropdown != null)
        {
            _regionDropdown.ClearOptions();
            _regionDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "India", "Asia", "Japan", "Europe", "US West"
            });
            _regionDropdown.value = 0; // default: India
            _regionDropdown.onValueChanged.AddListener(OnRegionChanged);
        }

        // wire buttons
        _playButton?.onClick.AddListener(OnPlayClicked);
        _friendsButton?.onClick.AddListener(OnFriendsClicked);
        _settingsButton?.onClick.AddListener(OnSettingsClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);
        _createRoomButton?.onClick.AddListener(OnCreateRoomClicked);
        _joinRoomButton?.onClick.AddListener(OnJoinRoomClicked);
        _closeFriendsPanelButton?.onClick.AddListener(OnCloseFriendsPanelClicked);
        _confirmJoinButton?.onClick.AddListener(OnConfirmJoinClicked);
        _closeJoinRoomPanelButton?.onClick.AddListener(OnCloseJoinRoomPanelClicked);
        _leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);

        // hide panels on start
        if (_settingsPanel != null)
            _settingsPanel.SetActive(false);

        if (_friendsPanel != null)
            _friendsPanel.SetActive(false);

        if (_joinRoomPanel != null)
            _joinRoomPanel.SetActive(false);

        if (_leaderboardPanel != null)
            _leaderboardPanel.SetActive(false);
    }

    private void ShowOnlyPanel(GameObject panelToShow)
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(panelToShow == _settingsPanel);
        if (_friendsPanel != null) _friendsPanel.SetActive(panelToShow == _friendsPanel);
        if (_joinRoomPanel != null) _joinRoomPanel.SetActive(panelToShow == _joinRoomPanel);
        if (_leaderboardPanel != null) _leaderboardPanel.SetActive(panelToShow == _leaderboardPanel);
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenu] Play clicked → joining room");

        PhotonNetworkManager.Instance?.Connect();

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetworkManager.Instance?.JoinOrCreateRoom();
          
        }
        else
        {
            Debug.LogWarning("[MainMenu] Not connected yet — waiting");
            StartCoroutine(WaitAndJoin());
        }
    }

    private void OnRegionChanged(int index)
    {
        string[] regionCodes = { "in", "asia", "jp", "eu", "usw" };
        if (index >= 0 && index < regionCodes.Length)
        {
            PhotonNetworkManager.Instance.SelectedRegion = regionCodes[index];
            Debug.Log($"[MainMenu] Region set to: {regionCodes[index]}");
        }
    }

    private System.Collections.IEnumerator WaitAndJoin()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
            yield return null;

        PhotonNetworkManager.Instance?.JoinOrCreateRoom();
        // Scene load handled by PhotonNetworkManager.OnJoinedRoom().
    }

    private void OnFriendsClicked()
    {
        Debug.Log("[MainMenu] Friends clicked");
        ShowOnlyPanel(_friendsPanel);
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings clicked");
        ShowOnlyPanel(_settingsPanel);
    }

    private void OnCloseFriendsPanelClicked()
    {
        if (_friendsPanel != null)
            _friendsPanel.SetActive(false);
    }

    private void OnCreateRoomClicked()
    {
        Debug.Log("[MainMenu] Create Room clicked");
        PhotonNetworkManager.Instance?.Connect();
        StartCoroutine(WaitAndCreateRoom());
    }

    private System.Collections.IEnumerator WaitAndCreateRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
                yield return null;
        }

        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
            yield return null;

        PhotonNetworkManager.Instance?.CreatePrivateRoom();
        // Scene load handled by PhotonNetworkManager.OnJoinedRoom()
        // once Photon confirms the room was actually created and joined.
    }

    private void OnJoinRoomClicked()
    {
        Debug.Log("[MainMenu] Join Room clicked");
        ShowOnlyPanel(_joinRoomPanel);
    }

    private void OnConfirmJoinClicked()
    {
        string code = _roomCodeInputField != null ? _roomCodeInputField.text.Trim().ToUpper() : "";

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[MainMenu] No room code entered");
            return;
        }

        Debug.Log($"[MainMenu] Attempting to join room with code: {code}");
        PhotonNetworkManager.Instance?.Connect();
        StartCoroutine(WaitAndJoinByCode(code));
    }

    private System.Collections.IEnumerator WaitAndJoinByCode(string code)
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
                yield return null;
        }

        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
            yield return null;

        PhotonNetworkManager.Instance?.JoinRoomByCode(code);
        // Scene load handled by PhotonNetworkManager.OnJoinedRoom().
    }

    private void OnCloseJoinRoomPanelClicked()
    {
        if (_joinRoomPanel != null)
            _joinRoomPanel.SetActive(false);
    }

    public void OnLeaderboardClicked()
    {
        Debug.Log("[MainMenu] Leaderboard clicked");
        ShowOnlyPanel(_leaderboardPanel);
        _leaderboardPanel?.GetComponent<LeaderboardManager>()?.FetchAndDisplay();
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quit clicked");
        Application.Quit();
    }
}