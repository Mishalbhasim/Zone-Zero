using UnityEngine;

public class RifleWeapon : WeaponBase
{
    [SerializeField] private GameObject _hitEffectPrefab;

    // Rifle default stats (override in Inspector)
    protected override void Start()
    {
        Damage = 20;
        FireRate = 8f;   // fast auto
        MagSize = 30;
        ReloadTime = 2f;
        Range = 150f;
        base.Start();
    }

    protected override void Shoot()
    {
        Ray ray = new Ray(_firePoint.position, _firePoint.forward);
        Debug.DrawRay(_firePoint.position, _firePoint.forward * Range, Color.yellow, 0.3f);
        EventBus.WeaponFired("Rifle");

        if (!Physics.Raycast(ray, out RaycastHit hit, Range)) return;

        if (_hitEffectPrefab != null)
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        var botSM = hit.collider.GetComponentInParent<BotStateMachine>();
        if (botSM != null) { botSM.TakeDamage(Damage); return; }

        var playerSM = hit.collider.GetComponent<PlayerStateMachine>();
        if (playerSM != null) playerSM.TakeDamage(Damage);
    }
}