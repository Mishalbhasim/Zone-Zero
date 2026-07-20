using UnityEngine;
using Photon.Pun;
using StarterAssets;
using UnityEngine.Animations.Rigging;

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
    [SerializeField] private Vector3 aimRotationOffset = new Vector3(0, 100f, 0);


    private RigBuilder _rigBuilder;


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

        

        // disable ragdoll on start
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;
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
        float pitch = Camera.main.transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f; 
        pitch = Mathf.Clamp(pitch, -40f, 40f); 

        _spine2.localRotation = _originalSpineRotation * Quaternion.Euler(pitch, 50, 0);
    }
    else
    {
        _spine2.localRotation = _originalSpineRotation;
    }
}

    void OnEnable()
    {
        if (photonView != null && !photonView.IsMine) return;
        EventBus.OnZoneDamageTick += TakeDamageFromZone;
    }

    void OnDisable()
    {
        EventBus.OnZoneDamageTick -= TakeDamageFromZone;
    }

    // zone tick passes only int damage — wrapper keeps that signature intact
    private void TakeDamageFromZone(int damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(int damage, string attackerId = null)
    {
        if (!photonView.IsMine) return;
        if (_isDead) return;

        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);

        if (CurrentHP <= 0) Die(attackerId);
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

    // called remotely by shooter (runs on shooter's client, targets victim's PhotonView)
    public void RequestDamage(int damage, string attackerId)
    {
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage, attackerId);
    }

    [PunRPC]
    private void RPC_TakeDamage(int damage, string attackerId)
    {
        if (!photonView.IsMine) return; // only actual owner applies it
        TakeDamage(damage, attackerId);
    }

    private System.Collections.IEnumerator ShootNextFrame()
    {
        yield return new WaitForSeconds(0.2f);
        CurrentWeapon?.TryShoot();
    }

    private void Die(string killerId = null)
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
        photonView.RPC("RPC_PlayerDied", RpcTarget.All, photonView.Owner.UserId, killerId);
    }

    [PunRPC]
    private void RPC_PlayerDied(string playerId, string killerId)
    {
        // play death anim on all clients
        PlayDeathAnim();

        // only master client reports elimination
        if (PhotonNetwork.IsMasterClient)
        {
            int placement = MatchManager.Instance.PlayersAlive;
            EliminationManager.Instance.ReportElimination(playerId, placement, killerId);
        }
    }

    private void PlayDeathAnim()
    {
        // disable animator
        var anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        // disable character controller
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // enable ragdoll
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;
    }

    public void TakeDamageFromBot(int damage)
    {
        if (!photonView.IsMine) return;
        TakeDamage(damage, null);
    }
}