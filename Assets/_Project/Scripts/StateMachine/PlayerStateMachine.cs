using UnityEngine;
using Photon.Pun;
using StarterAssets;

public class PlayerStateMachine : MonoBehaviourPun
{
    [Header("Stats")]
    public int MaxHP = 100;
    public int CurrentHP { get; private set; }

    [Header("Weapon")]
    public WeaponBase CurrentWeapon;

    private bool _isDead;

    // aim
    private Animator _animator;
    private ThirdPersonController _tpc;
    private float _aimTimer;
    private const float AIM_DURATION = 0.5f;
    private AimTargetController _aimTargetController;
    private Transform _spine2;
    private Quaternion _originalSpineRotation;
    [SerializeField] private Vector3 aimRotationOffset = new Vector3(0, 50f, 0);

    void Start()
    {
        if (!photonView.IsMine) return;

        CurrentHP = MaxHP;
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);

        _animator = GetComponentInChildren<Animator>();

        _spine2 = _animator.GetBoneTransform(HumanBodyBones.Chest);

        if (_spine2 != null)
        {
            _originalSpineRotation = _spine2.localRotation;
        }

        _animator = GetComponentInChildren<Animator>();
        _tpc = GetComponent<ThirdPersonController>();
        _aimTargetController = GetComponent<AimTargetController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (_isDead) return;

        // reset aim after shooting stops
        if (_aimTimer > 0)
        {
            _aimTimer -= Time.deltaTime;
            if (_aimTimer <= 0)
            {
                _animator?.SetBool("Aiming", false);
                if (_tpc != null) _tpc.strafe = false;
            }
        }
        if (_aimTimer <= 0)
        {
            _animator?.SetBool("Aiming", false);
            if (_tpc != null) _tpc.strafe = false;
            //_aimTargetController?.SetAiming(false);
        }
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (_spine2 == null) return;

        if (_aimTimer > 0)
        {
            _spine2.localRotation = _originalSpineRotation * Quaternion.Euler(aimRotationOffset);
        }
        else
        {
            _spine2.localRotation = _originalSpineRotation;
        }
    }

    void OnEnable()
    {
        if (photonView != null && !photonView.IsMine) return;
        EventBus.OnZoneDamageTick += TakeDamage;
    }

    void OnDisable()
    {
        EventBus.OnZoneDamageTick -= TakeDamage;
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;
        if (_isDead) return;

        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);

        if (CurrentHP <= 0) Die();
    }

    public void TryShoot()
    {
        if (!photonView.IsMine) return;
        if (_isDead) return;

        _animator?.SetBool("Aiming", true);
        if (_tpc != null) _tpc.strafe = true;
        _aimTargetController?.SetAiming(true);
        _aimTimer = AIM_DURATION;

        StartCoroutine(ShootNextFrame());
    }

    private System.Collections.IEnumerator ShootNextFrame()
    {
        yield return new WaitForSeconds(0.2f);
        CurrentWeapon?.TryShoot();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // disable movement
        if (_tpc != null) _tpc.enabled = false;

        // play death anim locally
        PlayDeathAnim();

        // fire local event for HUD death screen
        if (photonView.IsMine)
            EventBus.PlayerDied();

        // notify all clients via RPC
        photonView.RPC("RPC_PlayerDied", RpcTarget.All, photonView.Owner.UserId);
    }

    [PunRPC]
    private void RPC_PlayerDied(string playerId)
    {
        // play death anim on all clients
        PlayDeathAnim();

        // only master client reports elimination
        if (PhotonNetwork.IsMasterClient)
        {
            int placement = MatchManager.Instance.PlayersAlive;
            EliminationManager.Instance.ReportElimination(playerId, placement);
        }
    }

    private void PlayDeathAnim()
    {
        var anim = GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (var p in anim.parameters)
            {
                if (p.name == "Dead")
                {
                    anim.SetTrigger("Dead");
                    break;
                }
            }
        }
    }

    public void TakeDamageFromBot(int damage)
    {
        if (!photonView.IsMine) return;
        TakeDamage(damage);
    }
}