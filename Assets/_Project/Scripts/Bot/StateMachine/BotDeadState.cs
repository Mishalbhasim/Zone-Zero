using UnityEngine;

public class BotDeadState : IState
{
    private readonly BotStateMachine _sm;

    public BotDeadState(BotStateMachine sm)
    {
        _sm = sm;
    }

    public void Enter()
    {
        _sm.Agent.enabled = false;
        _sm.BotAnimator?.SetBool(_sm.DeadHash, true);

        // fire legacy event (HUD kill feed etc still use this)
        EventBus.BotKilled(_sm.gameObject.GetInstanceID());

        // report elimination to MatchManager via EliminationManager
        // master client owns elimination count
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            string botId = "Bot_" + _sm.gameObject.GetInstanceID();
            int placement = MatchManager.Instance != null ? MatchManager.Instance.PlayersAlive : 1;
            EliminationManager.Instance?.ReportElimination(botId, placement);
        }

        _sm.gameObject.SetActive(false);
    }

    public void Tick(float dt) { }

    public void Exit() { }
}