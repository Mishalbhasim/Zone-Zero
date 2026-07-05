using UnityEngine;
using BigRookGames.Weapons;

public class RifleWeapon : WeaponBase
{
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private GunfireController _gunVFX;

    protected override void Start()
    {
        Damage = 20;
        FireRate = 8f;
        MagSize = 30;
        ReloadTime = 2f;
        Range = 150f;
        base.Start();
    }

    protected override void Shoot()
    {
        // trigger muzzle flash + sound
        _gunVFX?.FireWeapon();

        Transform cam = Camera.main.transform;
        Ray ray = new Ray(cam.position, cam.forward);
        int layerMask = ~LayerMask.GetMask("Player");

        Debug.DrawRay(_firePoint.position, cam.forward * Range, Color.yellow, 0.3f);
        EventBus.WeaponFired("Rifle");

        if (!Physics.Raycast(ray, out RaycastHit hit, Range, layerMask)) return;

        Debug.Log($"[Rifle] Hit: {hit.collider.gameObject.name}");

        if (hit.collider.transform.IsChildOf(transform.root)) return;

        if (_hitEffectPrefab != null)
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        var botSM = hit.collider.GetComponentInParent<BotStateMachine>();
        if (botSM != null)
        {
            Debug.Log($"[Rifle] Hit bot: {botSM.gameObject.name}, HP: {botSM.CurrentHP}");
            botSM.TakeDamage(Damage);
            return;
        }

        var playerSM = hit.collider.GetComponentInParent<PlayerStateMachine>();
        if (playerSM != null) playerSM.TakeDamage(Damage);
    }
}