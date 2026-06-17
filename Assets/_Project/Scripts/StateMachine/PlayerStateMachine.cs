using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("Stats")]
    public int MaxHP = 100;
    public int CurrentHP { get; private set; }

    [Header("Weapon")]
    public WeaponBase CurrentWeapon;

    private bool _isDead;

    void Start()
    {
        CurrentHP = MaxHP;
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);
    }

    void OnEnable()
    {
        EventBus.OnZoneDamageTick += TakeDamage;
    }

    void OnDisable()
    {
        EventBus.OnZoneDamageTick -= TakeDamage;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);
        EventBus.PlayerHealthChanged(CurrentHP, MaxHP);
        if (CurrentHP <= 0) Die();
    }

    public void TryShoot()
    {
        if (_isDead) return;
        CurrentWeapon?.TryShoot();
    }

    private void Die()
    {
        _isDead = true;
        EventBus.PlayerDied();
        // disable player control
        var tpc = GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;
        // play death animation
        var anim = GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            bool hasParam = false;
            foreach (var p in anim.parameters)
                if (p.name == "Dead") { hasParam = true; break; }

            if (hasParam)
                anim.SetTrigger("Dead");
        }
    }
}