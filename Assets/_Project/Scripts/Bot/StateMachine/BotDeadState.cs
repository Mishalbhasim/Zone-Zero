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

        EventBus.BotKilled(_sm.gameObject.GetInstanceID());

        // disable after short delay
        _sm.gameObject.SetActive(false);
    }

    public void Tick(float dt) { }
    public void Exit() { }
}