using UnityEngine;
using Photon.Pun;

public class BotDeadState : IState
{
    private readonly BotStateMachine _sm;

    public BotDeadState(BotStateMachine sm)
    {
        _sm = sm;
    }

    public void Enter()
    {
        string killerId = _sm._lastKillerUserId; // empty if zone killed
        _sm.photonView.RPC("RPC_BotDied", RpcTarget.All, killerId);
    }

    public void Tick(float dt) { }
    public void Exit() { }
}