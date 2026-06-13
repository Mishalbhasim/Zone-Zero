using UnityEngine;

public class PistolWeapon : WeaponBase
{
    [SerializeField] private GameObject _hitEffectPrefab;
    protected override void Shoot()
    {
        // raycast from fire point forward
        Ray ray = new Ray(_firePoint.position, _firePoint.forward);
        RaycastHit hit;

        // debug line — visible in Scene view
        Debug.DrawRay(_firePoint.position,
                      _firePoint.forward * Range,
                      Color.red, 0.5f);

        EventBus.WeaponFired("Pistol");

        if (Physics.Raycast(ray, out hit, Range))
        {
            Debug.Log($"[Pistol] Hit: {hit.collider.gameObject.name}");

            // spawn hit effect
            if (_hitEffectPrefab != null)
                GameObject.Instantiate(
                    _hitEffectPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );

            // check bot
            var botSM = hit.collider.GetComponentInParent<BotStateMachine>();
            if (botSM != null)
            {
                botSM.TakeDamage(Damage);
                return;
            }

            // check player
            
            var playerSM = hit.collider.GetComponent<PlayerStateMachine>();
            if (playerSM != null)
            {
                playerSM.TakeDamage(Damage);
                return;
            }
        }
    }
}