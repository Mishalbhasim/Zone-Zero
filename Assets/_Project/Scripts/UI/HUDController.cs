using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _healthBar;

    [Header("Death Screen")]
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private TextMeshProUGUI _respawnText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI _ammoText;

    void OnEnable()
    {
        EventBus.OnPlayerHealthChanged += UpdateHealth;
        EventBus.OnPlayerDied += ShowDeathScreen;
        EventBus.OnPlayerRespawned += HideDeathScreen;
        EventBus.OnRespawnTimerTick += UpdateRespawnTimer;
        EventBus.OnAmmoChanged += UpdateAmmo;
    }

    void OnDisable()
    {
        EventBus.OnPlayerHealthChanged -= UpdateHealth;
        EventBus.OnPlayerDied -= ShowDeathScreen;
        EventBus.OnPlayerRespawned -= HideDeathScreen;
        EventBus.OnRespawnTimerTick -= UpdateRespawnTimer;
        EventBus.OnAmmoChanged -= UpdateAmmo;
    }

    private void UpdateHealth(int current, int max)
    {
        if (_healthBar == null) return;
        _healthBar.value = current;
    }

    private void ShowDeathScreen()
    {
        if (_deathScreen != null)
            _deathScreen.SetActive(true);
    }

    private void HideDeathScreen(Vector3 pos)
    {
        if (_deathScreen != null)
            _deathScreen.SetActive(false);
    }

    private void UpdateRespawnTimer(int seconds)
    {
        if (_respawnText != null)
            _respawnText.text = $"Respawning in {seconds}...";
    }

    private void UpdateAmmo(int current, int max)
    {
        if (_ammoText != null)
            _ammoText.text = $"{current}/{max}";
    }
}