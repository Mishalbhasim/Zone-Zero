using System.Collections;
using UnityEngine;

public class MatchManager : SceneSingleton<MatchManager>
{
    public bool IsMatchActive { get; private set; }
    public float MatchStartTime { get; private set; }
    public int PlayersAlive { get; private set; }

    void Start()
    {
        EventBus.OnPlayerEliminated += OnPlayerEliminated;
    }

    public void StartCountdown(int totalPlayers)
    {
        PlayersAlive = totalPlayers;
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"[MatchManager] Starting in {i}...");
            yield return new WaitForSeconds(1f);
        }
        StartMatch();
    }

    private void StartMatch()
    {
        IsMatchActive = true;
        MatchStartTime = Time.time;
        EventBus.MatchStarted(PlayersAlive);
        Debug.Log("[MatchManager] Match Started!");
    }

    private void OnPlayerEliminated(string playerId, int placement)
    {
        PlayersAlive--;
        EventBus.PlayersRemainingChanged(PlayersAlive);

        // last one standing wins
        if (PlayersAlive <= 1 && IsMatchActive)
        {
            IsMatchActive = false;
            var winner = ScoreManager.Instance.GetWinner();
            if (winner != null)
            {
                ScoreManager.Instance.PlayerWon(winner.PlayerId);
                EventBus.PlayerWon(winner.PlayerId);
            }
        }
    }

    public void EndMatch()
    {
        IsMatchActive = false;
        var winner = ScoreManager.Instance.GetWinner();
        if (winner != null)
            EventBus.PlayerWon(winner.PlayerId);
    }

    protected override void OnDestroy()
    {
        EventBus.OnPlayerEliminated -= OnPlayerEliminated;
    }
}