using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreManager : SceneSingleton<ScoreManager>
{
    // Score config
    private const int KILL_PLAYER = 50;
    private const int KILL_BOT = 10;
    private const int WIN_BONUS = 200;

    // placement points (30th=0, 1st=500)
    private static readonly int[] PLACEMENT_POINTS = {
        500, 300, 200, 150, 120, 100, 90, 80, 70, 60,
         55,  50,  45,  40,  35,  30, 25, 20, 18, 16,
         14,  12,  10,   8,   6,   4,  3,  2,  1,  0
    };

    private Dictionary<string, PlayerMatchData> _scores
        = new Dictionary<string, PlayerMatchData>();

    private int _eliminationOrder = 30;

    void Start()
    {
        EventBus.OnBotKilled += OnBotKilled;
        EventBus.OnPlayerDied += OnPlayerDied;
        EventBus.OnMatchStarted += OnMatchStarted;
    }

    public void RegisterPlayer(string playerId, string playerName)
    {
        if (_scores.ContainsKey(playerId)) return;
        _scores[playerId] = new PlayerMatchData
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Kills = 0,
            Deaths = 0,
            Score = 0
        };
    }

    public void PlayerKilledBot(string playerId)
    {
        if (!_scores.ContainsKey(playerId)) return;
        _scores[playerId].Kills++;
        AddScore(playerId, KILL_BOT);
    }

    public void PlayerKilledPlayer(string killerId, string victimId)
    {
        if (_scores.ContainsKey(killerId))
        {
            _scores[killerId].Kills++;
            AddScore(killerId, KILL_PLAYER);
        }

        if (_scores.ContainsKey(victimId))
        {
            _scores[victimId].Deaths++;
            int placement = _eliminationOrder--;
            int pts = PLACEMENT_POINTS[Mathf.Clamp(placement - 1, 0, 29)];
            AddScore(victimId, pts);
        }
    }

    public void PlayerWon(string playerId)
    {
        if (!_scores.ContainsKey(playerId)) return;
        AddScore(playerId, WIN_BONUS + PLACEMENT_POINTS[0]);
        Debug.Log($"[ScoreManager] Winner: {_scores[playerId].PlayerName}");
    }

    private void AddScore(string playerId, int delta)
    {
        if (!_scores.ContainsKey(playerId)) return;
        _scores[playerId].Score += delta;
        _scores[playerId].Score = Mathf.Max(0, _scores[playerId].Score);

        if (playerId == GameManager.Instance?.LocalPlayerId)
            EventBus.PlayerScoreChanged(_scores[playerId].Score, delta);
    }

    public List<PlayerMatchData> GetSortedScores()
        => _scores.Values.OrderByDescending(p => p.Score).ToList();

    public PlayerMatchData GetWinner()
        => _scores.Values.OrderByDescending(p => p.Score).FirstOrDefault();

    public int GetScore(string playerId)
        => _scores.ContainsKey(playerId) ? _scores[playerId].Score : 0;

    public void Reset()
    {
        _scores.Clear();
        _eliminationOrder = 30;
    }

    private void OnBotKilled(int botId)
    {
        string playerId = GameManager.Instance?.LocalPlayerId;
        if (string.IsNullOrEmpty(playerId)) return;
        PlayerKilledBot(playerId);
    }

    private void OnPlayerDied()
    {
        if (GameManager.Instance != null)
            PlayerKilledPlayer("", GameManager.Instance.LocalPlayerId);
    }

    private void OnMatchStarted(int total) => Reset();

    protected override void OnDestroy()
    {
        EventBus.OnBotKilled -= OnBotKilled;
        EventBus.OnPlayerDied -= OnPlayerDied;
        EventBus.OnMatchStarted -= OnMatchStarted;
    }
}