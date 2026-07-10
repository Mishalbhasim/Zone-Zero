using System.Collections;
using Photon.Pun;
using UnityEngine;

public class MatchManager : SceneSingleton<MatchManager>
{
    public bool IsMatchActive { get; private set; }
    public float MatchStartTime { get; private set; }
    public int PlayersAlive { get; private set; }

    void Start()
    {


        EventBus.PlayersAliveChanged(PlayersAlive);
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





    // Master client calls this when any player or bot is eliminated.
    public void OnEliminationReported(string eliminatedId, int placement, string killerId = null)
    {

        Debug.Log($"[MatchManager] BEFORE decrement: PlayersAlive={PlayersAlive}, eliminatedId={eliminatedId}, killerId={killerId}");
        PlayersAlive--;
        PlayersAlive = Mathf.Max(0, PlayersAlive);

        // placement = players still alive + 1
        int actualPlacement = PlayersAlive + 1;

        Debug.Log($"[MatchManager] Elimination reported: {eliminatedId} | Killer: {killerId} | Remaining: {PlayersAlive}");
        EventBus.PlayersAliveChanged(PlayersAlive);

        // pass actual placement + killer to elimination manager
        EliminationManager.Instance?.SyncPlacement(eliminatedId, actualPlacement, PlayersAlive, killerId);
        CheckWinCondition();
    }



    // Non-master clients call this to sync PlayersAlive from RPC.
    public void SyncPlayersAlive(int count)
    {
        PlayersAlive = count;
    }

    //Win condition

    private void CheckWinCondition()
    {
        if (!IsMatchActive) return;
        if (PlayersAlive > 1) return;
        IsMatchActive = false;

        // find last surviving player
        var survivors = GameObject.FindGameObjectsWithTag("Player");
        if (survivors.Length > 0)
        {
            var survivor = survivors[0].GetComponent<PhotonView>();
            string winnerId = survivor != null ? survivor.Owner.UserId : "Unknown";
            EliminationManager.Instance?.BroadcastWinner(winnerId);
            Debug.Log($"[MatchManager] Winner: {winnerId}");
        }
        else
        {
            // no players alive — last bot won or edge case
            EliminationManager.Instance?.BroadcastWinner("Unknown");
        }
    }

    public void EndMatch()
    {
        IsMatchActive = false;
        var winner = ScoreManager.Instance?.GetWinner();
        if (winner != null)
            EventBus.PlayerWon(winner.PlayerId);
    }
 

    protected override void OnDestroy()
    {
       
        base.OnDestroy();
    }
}