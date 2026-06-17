using UnityEngine;

public class PistolWeapon : WeaponBase
{
    [SerializeField] private GameObject _hitEffectPrefab;

    protected override void Shoot()
    {
        Transform cam = Camera.main.transform;
        Ray ray = new Ray(_firePoint.position, cam.forward);

        Debug.DrawRay(_firePoint.position, cam.forward * Range, Color.red, 0.5f);
        EventBus.WeaponFired("Pistol");

        if (!Physics.Raycast(ray, out RaycastHit hit, Range)) return;

        Debug.Log($"[Pistol] Hit: {hit.collider.gameObject.name}");

        // ignore self
        if (hit.collider.transform.IsChildOf(transform.root)) return;

        if (_hitEffectPrefab != null)
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        var botSM = hit.collider.GetComponentInParent<BotStateMachine>();
        if (botSM != null) { botSM.TakeDamage(Damage); return; }

        var playerSM = hit.collider.GetComponentInParent<PlayerStateMachine>();
        if (playerSM != null) playerSM.TakeDamage(Damage);
    }
}