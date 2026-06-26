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

    [Header("Panels")]
    [SerializeField] private GameObject _settingsPanel;

    [Header("Version")]
    [SerializeField] private TextMeshProUGUI _versionText;

    void Start()
    {
        // set version text
        if (_versionText != null)
            _versionText.text = $"v{Application.version}";

        // wire buttons
        _playButton?.onClick.AddListener(OnPlayClicked);
        _friendsButton?.onClick.AddListener(OnFriendsClicked);
        _settingsButton?.onClick.AddListener(OnSettingsClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);

        // hide panels on start
        if (_settingsPanel != null)
            _settingsPanel.SetActive(false);
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenu] Play clicked → joining room");

        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetworkManager.Instance?.JoinOrCreateRoom();
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Not connected yet — waiting");
            StartCoroutine(WaitAndJoin());
        }
    }

    private System.Collections.IEnumerator WaitAndJoin()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
            yield return null;

        PhotonNetworkManager.Instance?.JoinOrCreateRoom();
        SceneManager.LoadScene("Lobby");
    }
    private void OnFriendsClicked()
    {
        Debug.Log("[MainMenu] Friends clicked");
        // TODO Day 23: open friends/lobby panel
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings clicked");
        if (_settingsPanel != null)
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quit clicked");
        Application.Quit();
    }
}