using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Screens")]
    [SerializeField] private GameObject _bootScreen;
    [SerializeField] private GameObject _loginScreen;
    [SerializeField] private GameObject _mainMenuScreen;
    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private GameObject _hudScreen;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private GameObject _scoreboardScreen;
    [SerializeField] private GameObject _leaderboardScreen;
    [SerializeField] private GameObject _profileScreen;
    [SerializeField] private GameObject _settingsScreen;

    private GameObject _currentScreen;

    public void Show(string screenName)
    {
        if (_currentScreen != null)
            _currentScreen.SetActive(false);

        _currentScreen = GetScreen(screenName);

        if (_currentScreen != null)
            _currentScreen.SetActive(true);
    }

    private GameObject GetScreen(string name)
    {
        switch (name)
        {
            case "Boot": return _bootScreen;
            case "Login": return _loginScreen;
            case "MainMenu": return _mainMenuScreen;
            case "Lobby": return _lobbyScreen;
            case "HUD": return _hudScreen;
            case "Death": return _deathScreen;
            case "Scoreboard": return _scoreboardScreen;
            case "Leaderboard": return _leaderboardScreen;
            case "Profile": return _profileScreen;
            case "Settings": return _settingsScreen;
            default:
                Debug.LogWarning($"[UIManager] Screen not found: {name}");
                return null;
        }
    }
}