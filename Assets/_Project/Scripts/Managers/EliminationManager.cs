using UnityEngine;
using Photon.Pun;

/// <summary>
/// Day 18 — EliminationManager
/// Master client owns all elimination logic.
/// Reports kills → MatchManager → win condition.
/// Syncs remaining player count to all clients via RPC.
///
/// Setup: Add to "MatchManagers" GameObject in Arena_01.
///        Add PhotonView component to same GameObject.
/// </summary>
public class EliminationManager : SceneSingleton<EliminationManager>
{
    private PhotonView _pv;

    protected override void Awake()
    {
        base.Awake();
        _pv = GetComponent<PhotonView>();
    }

    // ── Public API (call only on master client) ───────────────────────────────

    /// <summary>
    /// Call when any player or bot is eliminated.
    /// Only master client should call this.
    /// </summary>
    public void ReportElimination(string eliminatedId, int placement)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[EliminationManager] ReportElimination called on non-master. Ignored.");
            return;
        }

        Debug.Log($"[EliminationManager] Eliminated: {eliminatedId} | Placement: {placement}");

        // tell MatchManager on master client
        MatchManager.Instance?.OnEliminationReported(eliminatedId, placement);

        // sync new PlayersAlive count to ALL clients
        int remaining = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 0;
        _pv.RPC("RPC_SyncElimination", RpcTarget.All, eliminatedId, placement, remaining);
    }

    // ── RPC (runs on ALL clients) ─────────────────────────────────────────────

    [PunRPC]
    private void RPC_SyncElimination(string eliminatedId, int placement, int playersRemaining)
    {
        Debug.Log($"[EliminationManager] RPC: {eliminatedId} eliminated | Remaining: {playersRemaining}");

        // update HUD on all clients
        EventBus.PlayersRemainingChanged(playersRemaining);

        // non-master clients sync their local count
        if (!PhotonNetwork.IsMasterClient)
        {
            MatchManager.Instance?.SyncPlayersAlive(playersRemaining);
        }

        // fire for kill feed / scoreboard
        EventBus.PlayerEliminated(eliminatedId, placement);
    }
}