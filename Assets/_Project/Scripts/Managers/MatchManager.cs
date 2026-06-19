using System.Collections;
using UnityEngine;

public class MatchManager : SceneSingleton<MatchManager>
{
    public bool IsMatchActive { get; private set; }
    public float MatchStartTime { get; private set; }
    public int PlayersAlive { get; private set; }

    void Start()
    {
        // EliminationManager now drives eliminations — no direct EventBus sub needed
        // Keep for solo/offline fallback only
        EventBus.OnPlayerEliminated += OnPlayerEliminatedFallback;
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

    // ── Called by EliminationManager on master client ─────────────────────────

    /// <summary>
    /// Master client calls this when any player/bot is eliminated.
    /// </summary>
    public void OnEliminationReported(string eliminatedId, int placement)
    {
        PlayersAlive--;
        PlayersAlive = Mathf.Max(0, PlayersAlive);

        Debug.Log($"[MatchManager] Elimination reported: {eliminatedId} | Remaining: {PlayersAlive}");

        CheckWinCondition();
    }

    /// <summary>
    /// Non-master clients call this to sync PlayersAlive from RPC.
    /// </summary>
    public void SyncPlayersAlive(int count)
    {
        PlayersAlive = count;
    }

    // ── Win condition ──────────────────────────────────────────────────────────

    private void CheckWinCondition()
    {
        if (!IsMatchActive) return;
        if (PlayersAlive > 1) return;

        IsMatchActive = false;

        var winner = ScoreManager.Instance?.GetWinner();
        if (winner != null)
        {
            ScoreManager.Instance.PlayerWon(winner.PlayerId);
            EventBus.PlayerWon(winner.PlayerId);
            Debug.Log($"[MatchManager] Winner: {winner.PlayerId}");
        }
        else
        {
            // no winner found — last bot standing or edge case
            EventBus.PlayerWon("Unknown");
            Debug.LogWarning("[MatchManager] Win condition met but no winner found in ScoreManager.");
        }
    }

    public void EndMatch()
    {
        IsMatchActive = false;
        var winner = ScoreManager.Instance?.GetWinner();
        if (winner != null)
            EventBus.PlayerWon(winner.PlayerId);
    }

    // ── Solo/offline fallback ─────────────────────────────────────────────────

    // Only fires if EliminationManager is not present (solo play without Photon)
    private void OnPlayerEliminatedFallback(string playerId, int placement)
    {
        if (Photon.Pun.PhotonNetwork.IsConnected) return; // Photon handles it
        PlayersAlive--;
        PlayersAlive = Mathf.Max(0, PlayersAlive);
        EventBus.PlayersRemainingChanged(PlayersAlive);
        CheckWinCondition();
    }

    protected override void OnDestroy()
    {
        EventBus.OnPlayerEliminated -= OnPlayerEliminatedFallback;
        base.OnDestroy();
    }
}