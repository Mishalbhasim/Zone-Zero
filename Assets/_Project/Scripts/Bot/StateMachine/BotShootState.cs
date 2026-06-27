using UnityEngine;

public class BotShootState : IState
{
    private readonly BotStateMachine _sm;
    private float _nextFireTime;

    public BotShootState(BotStateMachine sm)
    {
        _sm = sm;
    }

    public void Enter()
    {
        _sm.Agent.ResetPath();
        _sm.BotAnimator?.SetFloat(_sm.SpeedHash, 0f);
        _sm.BotAnimator?.SetBool(_sm.AimingHash, true);
    }

    public void Tick(float dt)
    {
        if (_sm.CurrentTarget == null)
        {
            _sm.TransitionTo(_sm.PatrolState);
            return;
        }

        float dist = Vector3.Distance(_sm.transform.position,
                                       _sm.CurrentTarget.position);

        // target moved away
        if (dist > _sm.ShootRange * 1.2f)
        {
            _sm.TransitionTo(_sm.AlertState);
            return;
        }

        // face target
        Vector3 dir = (_sm.CurrentTarget.position - _sm.transform.position);
        dir.y = 0;
        if (dir.magnitude > 0.1f)
            _sm.transform.rotation = Quaternion.Slerp(
                _sm.transform.rotation,
                Quaternion.LookRotation(dir),
                10f * dt
            );

        // shoot
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + 1f / _sm.FireRate;
            Shoot();
        }
    }

    public void Exit() {
        _sm.BotAnimator?.SetBool(_sm.AimingHash, false);
    }

    private void Shoot()
    {
        // 65% accuracy
        if (Random.value > 0.65f) return;

        var playerSM = _sm.CurrentTarget.GetComponentInParent<PlayerStateMachine>();
        if (playerSM != null)
            playerSM.TakeDamage(_sm.Damage);

        EventBus.WeaponFired("Bot");
    }
}