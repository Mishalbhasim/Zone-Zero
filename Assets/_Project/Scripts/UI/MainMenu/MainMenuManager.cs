using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
        Debug.Log("[MainMenu] Play clicked → loading Lobby");
        // TODO Day 23: load Lobby scene
        // For now → go straight to Arena
        SceneManager.LoadScene("Arena_01");
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