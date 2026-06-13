using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    public int Damage = 25;
    public float FireRate = 2f;
    public int MagSize = 12;
    public float ReloadTime = 1.5f;
    public float Range = 100f;

    [Header("References")]
    [SerializeField] protected Transform _firePoint;
    [SerializeField] protected ParticleSystem _muzzleFlash;

    protected int _currentAmmo;
    protected float _nextFireTime;
    protected bool _isReloading;

    public int CurrentAmmo => _currentAmmo;
    public bool IsReloading => _isReloading;

    protected virtual void Start()
    {
        _currentAmmo = MagSize;
    }

    public virtual void TryShoot()
    {
        if (_isReloading) return;
        if (_currentAmmo <= 0) { StartReload(); return; }
        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + 1f / FireRate;
        _currentAmmo--;

        Shoot();

        if (_muzzleFlash != null)
            _muzzleFlash.Play();

        EventBus.AmmoChanged(_currentAmmo, MagSize);
    }

    protected abstract void Shoot();

    public virtual void StartReload()
    {
        if (_isReloading || _currentAmmo == MagSize) return;
        StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        yield return new WaitForSeconds(ReloadTime);
        _currentAmmo = MagSize;
        _isReloading = false;
        EventBus.AmmoChanged(_currentAmmo, MagSize);
    }
}