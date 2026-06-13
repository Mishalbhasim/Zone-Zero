using UnityEngine;
using UnityEngine.AI;

public class BotStateMachine : MonoBehaviour
{
    public BotPatrolState PatrolState { get; private set; }
    public BotAlertState AlertState { get; private set; }
    public BotShootState ShootState { get; private set; }
    public BotDeadState DeadState { get; private set; }

    [Header("Detection")]
    public float DetectionRange = 40f;
    public float DetectionFOV = 120f;
    public float ShootRange = 20f;

    [Header("Combat")]
    public int Damage = 15;
    public float FireRate = 1.5f;

    public Transform CurrentTarget { get; set; }


    public NavMeshAgent Agent { get; private set; }
    public Animator BotAnimator { get; private set; }

    public int CurrentHP { get; set; } = 100;
    public int MaxHP = 100;

    public int SpeedHash { get; private set; }
    public int DeadHash { get; private set; }

    private IState _currentState;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        BotAnimator = GetComponentInChildren<Animator>();

        SpeedHash = Animator.StringToHash("Speed");
        DeadHash = Animator.StringToHash("Dead");

        PatrolState = new BotPatrolState(this);
        AlertState = new BotAlertState(this);
        ShootState = new BotShootState(this);
        DeadState = new BotDeadState(this);
    }

    void Start()
    {
        TransitionTo(PatrolState);
    }

    void Update()
    {
        _currentState?.Tick(Time.deltaTime);

        // detection 
        if (_currentState != ShootState && _currentState != DeadState)
            DetectPlayer();
    }

    private void DetectPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        

        foreach (var player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
           
            if (dist > DetectionRange) continue;

            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            

            if (angle <= DetectionFOV * 0.5f)
            {
                // line of sight check
                if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer,
                                     out RaycastHit hit, DetectionRange))
                {
                    
                    if (hit.collider.GetComponentInParent<PlayerStateMachine>() != null)
                    {
                        CurrentTarget = player.transform;
                        if (_currentState == PatrolState)
                            TransitionTo(AlertState);
                        return;
                    }
                }
            }
        }

        CurrentTarget = null;
    }

    //Takes Damage
    public void TakeDamage(int damage)
    {
        if (_currentState == DeadState) return;

        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);

        if (CurrentHP <= 0)
            TransitionTo(DeadState);
    }

    public void TransitionTo(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
}