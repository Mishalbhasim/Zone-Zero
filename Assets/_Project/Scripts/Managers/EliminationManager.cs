using UnityEngine;
using Photon.Pun;

public class EliminationManager : SceneSingleton<EliminationManager>
{
    private PhotonView _pv;

    protected override void Awake()
    {
        base.Awake();
        _pv = GetComponent<PhotonView>();
    }

    // Player Elimination
    public void ReportElimination(string eliminatedId, int placement, string killerId = null)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[EliminationManager] ReportElimination called on non-master. Ignored.");
            return;
        }
        Debug.Log($"[EliminationManager] Eliminated: {eliminatedId} | Placement: {placement} | Killer: {killerId}");
        // tell MatchManager — it calculates actual placement and calls SyncPlacement
        MatchManager.Instance?.OnEliminationReported(eliminatedId, placement, killerId);
    }

    // Called from MatchManager after calculating actual placement
    public void SyncPlacement(string eliminatedId, int actualPlacement, int remaining, string killerId = null)
    {
        _pv.RPC("RPC_SyncElimination", RpcTarget.All, eliminatedId, actualPlacement, remaining, killerId);
    }

    public void BroadcastWinner(string winnerId)
    {
        _pv.RPC("RPC_PlayerWon", RpcTarget.All, winnerId);
    }

    [PunRPC]
    private void RPC_PlayerWon(string winnerId)
    {
        EventBus.PlayerWon(winnerId);
    }

    [PunRPC]
    private void RPC_SyncElimination(string eliminatedId, int placement, int playersRemaining, string killerId)
    {
        Debug.Log($"[EliminationManager] RPC: {eliminatedId} eliminated | Placement: {placement} | Remaining: {playersRemaining} | Killer: {killerId}");

        EventBus.PlayersAliveChanged(playersRemaining);

        if (!PhotonNetwork.IsMasterClient)
            MatchManager.Instance?.SyncPlayersAlive(playersRemaining);

        EventBus.PlayerEliminated(eliminatedId, placement);

        // Bot eliminations are already credited via ScoreManager.PlayerKilledBot
        // in BotStateMachine.RPC_BotDied — skip here to avoid double-counting.
        bool isBotElimination = eliminatedId != null && eliminatedId.StartsWith("BOT_");
        if (!isBotElimination && !string.IsNullOrEmpty(killerId))
            ScoreManager.Instance?.PlayerKilledPlayer(killerId, eliminatedId);
    }
}