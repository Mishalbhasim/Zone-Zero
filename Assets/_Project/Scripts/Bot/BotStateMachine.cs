using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BotStateMachine : MonoBehaviour
{
    public BotPatrolState PatrolState { get; private set; }
    public BotAlertState AlertState { get; private set; }
    public BotShootState ShootState { get; private set; }
    public BotDeadState DeadState { get; private set; }

    [Header("Movement")]
    public float PatrolSpeed = 2f;
    public float ChaseSpeed = 6f;

    [Header("LOD")]
    public bool IsActive = true;

    [Header("Detection")]
    public float DetectionRange = 40f;
    public float DetectionFOV = 120f;
    public float ShootRange = 20f;

    [Header("Combat")]
    public int Damage = 10;
    public float FireRate = 0.8f;

    public Transform CurrentTarget { get; set; }
    public UnityEngine.AI.NavMeshAgent Agent { get; private set; }
    public Animator BotAnimator { get; private set; }
    public int CurrentHP { get; set; } = 100;
    public int MaxHP = 100;
    public int SpeedHash { get; private set; }
    public int DeadHash { get; private set; }
    public int AimingHash { get; private set; }
    public int MotionSpeedHash { get; private set; }
    public PhotonView photonView { get; private set; }

    private Transform _spine;
    private Quaternion _originalSpineRotation;
    private IState _currentState;

    void Awake()
    {
        Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        BotAnimator = GetComponentInChildren<Animator>();
        photonView = GetComponent<PhotonView>();

        SpeedHash = Animator.StringToHash("Speed");
        DeadHash = Animator.StringToHash("Dead");
        AimingHash = Animator.StringToHash("Aiming");
        MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        _spine = BotAnimator?.GetBoneTransform(HumanBodyBones.Chest);
        if (_spine != null)
            _originalSpineRotation = _spine.localRotation;

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
        if (!PhotonNetwork.IsMasterClient) return; // only master runs AI
        if (!IsActive) return;

        _currentState?.Tick(Time.deltaTime);

        if (_currentState != ShootState && _currentState != DeadState)
            DetectPlayer();
    }

    void LateUpdate()
    {
        if (_spine == null) return;

        if (_currentState == ShootState && CurrentTarget != null)
        {
            Vector3 dir = (CurrentTarget.position - _spine.position).normalized;
            _spine.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 50, 0);
        }
        else
        {
            _spine.localRotation = _originalSpineRotation;
        }
    }

    [PunRPC]
    public void RPC_BotDied(string killerUserId)
    {
        BotAnimator?.SetTrigger(DeadHash);
        Agent.enabled = false;
        BotManager.Instance?.DecrementBotsRemaining();

        if (PhotonNetwork.IsMasterClient)
        {
            EliminationManager.Instance?.ReportElimination(
                killerUserId,
                MatchManager.Instance.PlayersAlive
            );
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.Destroy(gameObject);
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

    public void TakeDamage(int damage)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[Bot] Master TakeDamage called. HP before: {CurrentHP}, state: {_currentState?.GetType().Name}");
            if (_currentState == DeadState) return;
            CurrentHP -= damage;
            CurrentHP = Mathf.Max(0, CurrentHP);
            Debug.Log($"[Bot] HP after: {CurrentHP}");
            if (CurrentHP <= 0)
                TransitionTo(DeadState);
        }
        else
        {
            photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, damage);
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage)
    {
        TakeDamage(damage);
    }

    public void TransitionTo(IState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
}