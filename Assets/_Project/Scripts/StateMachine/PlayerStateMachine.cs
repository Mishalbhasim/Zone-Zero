using UnityEngine;
using Photon.Pun;

public class PlayerStateMachine : MonoBehaviourPun
{
    [Header("Stats")]
    public int MaxHP = 100;
    public int CurrentHP { get; private set; }

    [Header("Weapon")]
    public WeaponBase CurrentWeapon;

    private bool _isDead;

    void Start()
    {
        // only init health for local player
        if (!photonView.IsMine) return;

        CurrentHP = MaxHP;
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);
    }

    void OnEnable()
    {
        // only local player takes zone damage
        if (photonView != null && !photonView.IsMine) return;
        EventBus.OnZoneDamageTick += TakeDamage;
    }

    void OnDisable()
    {
        EventBus.OnZoneDamageTick -= TakeDamage;
    }

    public void TakeDamage(int damage)
    {
        // only process damage on owner
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
        CurrentWeapon?.TryShoot();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // disable movement
        var tpc = GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;

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
        // play death anim on all clients for this player
        PlayDeathAnim();

        // only master client reports elimination to MatchManager
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

    // called by bot raycast hit on this player
    public void TakeDamageFromBot(int damage)
    {
        if (!photonView.IsMine) return;
        TakeDamage(damage);
    }
}