using UnityEngine;

public class SniperWeapon : WeaponBase
{
    [Header("Sniper")]
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private float _scopedFOV = 20f;
    private float _normalFOV = 60f;
    private bool _isScoped;
    private Camera _cam;

    protected override void Start()
    {
        Damage = 90;
        FireRate = 0.5f;  // slow, bolt-action feel
        MagSize = 5;
        ReloadTime = 3f;
        Range = 500f;
        base.Start();
        _cam = Camera.main;
        _normalFOV = _cam.fieldOfView;
    }

    // call from PlayerStateMachine on right-click / scope button
    public void SetScope(bool scoped)
    {
        _isScoped = scoped;
        _cam.fieldOfView = scoped ? _scopedFOV : _normalFOV;
    }

    protected override void Shoot()
    {
        Ray ray = new Ray(_firePoint.position, Camera.main.transform.forward);
        Debug.DrawRay(_firePoint.position, _firePoint.forward * Range, Color.blue, 0.5f);
        EventBus.WeaponFired("Sniper");

        if (!Physics.Raycast(ray, out RaycastHit hit, Range)) return;

        if (_hitEffectPrefab != null)
            Instantiate(_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        var botSM = hit.collider.GetComponentInParent<BotStateMachine>();
        if (botSM != null) { botSM.TakeDamage(Damage); return; }

        var playerSM = hit.collider.GetComponent<PlayerStateMachine>();
        if (playerSM != null) playerSM.TakeDamage(Damage);
    }

    void OnDestroy()
    {
        // reset FOV if sniper destroyed mid-scope
        if (_cam != null) _cam.fieldOfView = _normalFOV;
    }
}