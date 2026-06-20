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

    // Bot Death Sync

    public void ReportBotDeath(string botName)
    {
        Debug.Log($"[EliminationManager] Bot died: {botName}");

        // report elimination to match manager if it is master clinet only
        if (PhotonNetwork.IsMasterClient)
        {
            int placement = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 1;
            MatchManager.Instance?.OnEliminationReported(botName, placement);
            int remaining = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 0;
            _pv.RPC("RPC_SyncBotDeath", RpcTarget.All, botName, remaining);
        }
        else
        {
            // non-master client killed a bot — tell master client
            _pv.RPC("RPC_RequestBotDeath", RpcTarget.MasterClient, botName);
        }
    }

    [PunRPC]
    private void RPC_RequestBotDeath(string botName)
    {
        // master client received bot death from another client
        // process and broadcast to all
        int placement = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 1;
        MatchManager.Instance?.OnEliminationReported(botName, placement);
        int remaining = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 0;
        _pv.RPC("RPC_SyncBotDeath", RpcTarget.All, botName, remaining);
    }

    [PunRPC]
    private void RPC_SyncBotDeath(string botName, int playersRemaining)
    {
        Debug.Log($"[EliminationManager] RPC: Bot {botName} died | Remaining: {playersRemaining}");

        // find bot by name and disable on all clients
        var bot = GameObject.Find(botName);
        if (bot != null)
            bot.SetActive(false);

        // update HUD
        EventBus.PlayersRemainingChanged(playersRemaining);
        BotManager.Instance?.DecrementBotsRemaining();

        // non-master clients sync count
        if (!PhotonNetwork.IsMasterClient)
            MatchManager.Instance?.SyncPlayersAlive(playersRemaining);
    }

    //Player Elimination RPC

    [PunRPC]
    private void RPC_SyncElimination(string eliminatedId, int placement, int playersRemaining)
    {
        Debug.Log($"[EliminationManager] RPC: {eliminatedId} eliminated | Remaining: {playersRemaining}");

        // update HUD on all clients
        EventBus.PlayersRemainingChanged(playersRemaining);

        // non-master clients sync their local count
        if (!PhotonNetwork.IsMasterClient)
            MatchManager.Instance?.SyncPlayersAlive(playersRemaining);

        // fire for kill feed / scoreboard
        EventBus.PlayerEliminated(eliminatedId, placement);
    }
}