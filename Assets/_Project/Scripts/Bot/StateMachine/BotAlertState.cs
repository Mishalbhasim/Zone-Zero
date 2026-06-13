using UnityEngine;

public class BotAlertState : IState
{
    private readonly BotStateMachine _sm;

    public BotAlertState(BotStateMachine sm)
    {
        _sm = sm;
    }

    public void Enter()
    {
        if (_sm.CurrentTarget != null)
            _sm.Agent.SetDestination(_sm.CurrentTarget.position);
    }

    public void Tick(float dt)
    {
        float speed = _sm.Agent.velocity.magnitude;
        _sm.BotAnimator?.SetFloat(_sm.SpeedHash, speed);

        if (_sm.CurrentTarget == null)
        {
            _sm.TransitionTo(_sm.PatrolState);
            return;
        }

        float dist = Vector3.Distance(_sm.transform.position,
                                       _sm.CurrentTarget.position);

        // close enough to shoot
        if (dist <= _sm.ShootRange)
        {
            _sm.TransitionTo(_sm.ShootState);
            return;
        }

        // chase target
        _sm.Agent.SetDestination(_sm.CurrentTarget.position);
    }

    public void Exit() { }
}