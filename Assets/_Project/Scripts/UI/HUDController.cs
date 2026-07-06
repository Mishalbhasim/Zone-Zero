using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Zone Timer")]
    [SerializeField] private TextMeshProUGUI _zoneTimerText;
    private float _zoneTimer;
    private bool _zoneTimerActive;


    [Header("Death Screen")]
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private Button _deathReturnButton;

    [Header("Victory Screen")]
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private TextMeshProUGUI _winnerNameText;
    [SerializeField] private TextMeshProUGUI _winnerStatsText;
    [SerializeField] private Button _victoryReturnButton;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI _ammoText;

    [Header("Players Alive")]
    [SerializeField] private TextMeshProUGUI _playersAliveText;

    [Header("Zone Warning")]
    [SerializeField] private TextMeshProUGUI _zoneWarningText;
    private float _zoneWarningTimer;
    private bool _zoneWarningActive;

    private int _myKills = 0;
    private int _myPlacement = 0;

    void Start()
    {
        // hide both screens on start
        _deathScreen?.SetActive(false);
        _victoryScreen?.SetActive(false);

        // wire buttons
        _deathReturnButton?.onClick.AddListener(ReturnToLobby);
        _victoryReturnButton?.onClick.AddListener(ReturnToLobby);
    }

    void OnEnable()
    {
        EventBus.OnPlayerHealthChanged += UpdateHealth;
        EventBus.OnPlayerDied += ShowDeathScreen;
        EventBus.OnPlayerRespawned += HideDeathScreen;
        EventBus.OnRespawnTimerTick += UpdateRespawnTimer;
        EventBus.OnAmmoChanged += UpdateAmmo;
        EventBus.OnPlayerWon += ShowVictoryScreen;
        EventBus.OnBotKilled += OnBotKilled;
        EventBus.OnPlayerEliminated += OnPlayerEliminated;
        EventBus.OnPlayersAliveChanged += UpdatePlayersAlive;
        EventBus.OnZoneShrinkStarted += OnZoneShrinkStarted;
        EventBus.OnZoneWarning += OnZoneWarning;
    }

    void OnDisable()
    {
        EventBus.OnPlayerHealthChanged -= UpdateHealth;
        EventBus.OnPlayerDied -= ShowDeathScreen;
        EventBus.OnPlayerRespawned -= HideDeathScreen;
        EventBus.OnRespawnTimerTick -= UpdateRespawnTimer;
        EventBus.OnAmmoChanged -= UpdateAmmo;
        EventBus.OnPlayerWon -= ShowVictoryScreen;
        EventBus.OnBotKilled -= OnBotKilled;
        EventBus.OnPlayerEliminated -= OnPlayerEliminated;
        EventBus.OnPlayersAliveChanged -= UpdatePlayersAlive;
        EventBus.OnZoneShrinkStarted -= OnZoneShrinkStarted;
        EventBus.OnZoneWarning -= OnZoneWarning;
    }


    void Update()
    {
        UpdateZoneTimer();
        UpdateZoneWarning();
    }

    //Health

    private void UpdateHealth(int current, int max)
    {
        if (_healthBar == null) return;
        _healthBar.value = current;

        // get fill image
        var fill = _healthBar.fillRect?.GetComponent<Image>();
        if (fill == null) return;

        float percent = (float)current / max;
        if (percent > 0.6f)
            fill.color = new Color(0f, 1f, 0.53f);      // green
        else if (percent > 0.3f)
            fill.color = new Color(1f, 0.85f, 0f);       // yellow
        else
            fill.color = new Color(1f, 0.24f, 0.24f);    // red

        // after fill color change — add:
        if (_healthText != null)
            _healthText.text = $"{current}";
    }
    //Ammo

    private void UpdateAmmo(int current, int max)
    {
        if (_ammoText != null)
            _ammoText.text = $"{current}/{max}";
    }

    //Kill tracking

    private void OnBotKilled(int botId)
    {
        _myKills++;
    }

    private void OnPlayerEliminated(string playerId, int placement)
    {
        // track our own placement
        if (playerId == GameManager.Instance?.LocalPlayerId)
            _myPlacement = placement;
    }

    //Death Screen

    private void ShowDeathScreen()
    {

        Debug.Log("[HUD] ShowDeathScreen called");
        if (_deathScreen == null)
        {
            Debug.LogError("[HUD] _deathScreen is null!");
            return;
        }


        _deathScreen.SetActive(true);

        int score = ScoreManager.Instance?.GetScore(
            GameManager.Instance?.LocalPlayerId ?? "") ?? 0;

        if (_statsText != null)
            _statsText.text = $"#{_myPlacement} out of 30  |  Kills: {_myKills}  |  Score: {score}";
    }

    private void HideDeathScreen(Vector3 pos)
    {
        _deathScreen?.SetActive(false);
    }

    private void UpdateRespawnTimer(int seconds)
    {
        // kept for compatibility — not used in BR mode
    }

    //Victory Screen

    private void ShowVictoryScreen(string winnerId)
    {
        if (_victoryScreen == null) return;
        _victoryScreen.SetActive(true);

        string localId = GameManager.Instance?.LocalPlayerId ?? "";
        bool isLocalWinner = winnerId == localId;

        if (_winnerNameText != null)
        {
            _winnerNameText.text = isLocalWinner
                ? GameManager.Instance?.LocalPlayerName ?? "You"
                : winnerId;
        }

        int score = ScoreManager.Instance?.GetScore(winnerId) ?? 0;
        var winner = ScoreManager.Instance?.GetWinner();

        if (_winnerStatsText != null)
            _winnerStatsText.text = $"Kills: {winner?.Kills ?? 0}  |  Score: {score}";
    }

    //Return to Lobby

    private void ReturnToLobby()
    {
        Debug.Log("[HUD] Returning to lobby");

        // disable auto sync before leaving
        PhotonNetwork.AutomaticallySyncScene = false;

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("MainMenu");
    }

    //update alive players
    private void UpdatePlayersAlive(int count)
    {
        if (_playersAliveText != null)
            _playersAliveText.text = $"{count} ALIVE";
    }


    private void OnZoneShrinkStarted(Vector3 currentCenter, float currentRadius,
                                  Vector3 nextCenter, float nextRadius, float shrinkTime)
    {
        _zoneTimer = shrinkTime;
        _zoneTimerActive = true;
        _zoneTimerText?.gameObject.SetActive(true);

    }

    private void UpdateZoneTimer()
    {
        if (!_zoneTimerActive) return;
        _zoneTimer -= Time.deltaTime;

        if (_zoneTimerText != null)
        {
            _zoneTimerText.text = $"ZONE {Mathf.CeilToInt(_zoneTimer)}s";
            _zoneTimerText.color = _zoneTimer <= 10f
                ? new Color(1f, 0.24f, 0.24f)   // red
                : new Color(0f, 0.898f, 1f);     // cyan
        }

        if (_zoneTimer <= 0f)
        {
            _zoneTimerActive = false;
            _zoneTimerText?.gameObject.SetActive(false);
        }
    }


    private void OnZoneWarning(float waitTime)
    {
        _zoneWarningTimer = waitTime;
        _zoneWarningActive = true;
        _zoneWarningText?.gameObject.SetActive(true);
    }


    private void UpdateZoneWarning()
    {
        if (!_zoneWarningActive) return;
        _zoneWarningTimer -= Time.deltaTime;

        if (_zoneWarningText != null)
        {
            _zoneWarningText.text = $"ZONE CLOSING IN {Mathf.CeilToInt(_zoneWarningTimer)}s";
            _zoneWarningText.color = _zoneWarningTimer <= 10f
                ? new Color(1f, 0.24f, 0.24f)
                : new Color(1f, 0.85f, 0f); // yellow warning
        }

        if (_zoneWarningTimer <= 0f)
        {
            _zoneWarningActive = false;
            _zoneWarningText?.gameObject.SetActive(false);
        }
    }
}