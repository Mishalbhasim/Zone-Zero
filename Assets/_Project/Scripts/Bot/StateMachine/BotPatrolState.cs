using UnityEngine;
using UnityEngine.AI;

public class BotPatrolState : IState
{
    private readonly BotStateMachine _sm;
    private Vector3 _targetPoint;
    private float _patrolRadius = 30f;
    private float _waitTimer;

    public BotPatrolState(BotStateMachine sm)
    {
        _sm = sm;
    }

    public void Enter()
    {
        _sm.Agent.speed = _sm.PatrolSpeed;
        PickNewDestination();
    }

    public void Tick(float dt)
    {
        if (!_sm.Agent.isOnNavMesh) return;
        // animator speed
        float speed = _sm.Agent.velocity.magnitude;
        _sm.BotAnimator?.SetFloat(_sm.SpeedHash, speed);
        _sm.BotAnimator?.SetFloat(_sm.MotionSpeedHash, speed / _sm.PatrolSpeed);

        // reached destination
        if (!_sm.Agent.pathPending &&
            _sm.Agent.remainingDistance < 0.5f)
        {
            _waitTimer += dt;
            if (_waitTimer > 2f)
            {
                _waitTimer = 0f;
                PickNewDestination();
            }
        }
    }

    public void Exit() { }

    private void PickNewDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * _patrolRadius;
        randomDir += _sm.transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit,
                                    _patrolRadius, NavMesh.AllAreas))
        {
            _sm.Agent.SetDestination(hit.position);
        }
    }
}