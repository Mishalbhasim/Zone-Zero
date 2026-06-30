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
        // RPC to all clients — handles death everywhere
        _sm.photonView.RPC("RPC_BotDied", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId);
    }

    public void Tick(float dt) { }
    public void Exit() { }
}