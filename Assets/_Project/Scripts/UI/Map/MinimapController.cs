using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _minimapCamera;
    [SerializeField] private RectTransform _playerArrow;
    [SerializeField] private RectTransform _minimapRect;
    private Transform _playerTransform;
    public bool IsFullMapOpen { get; set; } = false;

    [Header("Zone Rings - Mini")]
    [SerializeField] private RectTransform _currentZoneRing;
    [SerializeField] private RectTransform _nextZoneRing;
    [SerializeField] private float _minimapCameraSize = 100f;

    [Header("Full Map UI")]
    [SerializeField] private RectTransform _fullMapRect;
    [SerializeField] private RectTransform _playerArrowFull;
    [SerializeField] private RectTransform _currentZoneRingFull;
    [SerializeField] private RectTransform _nextZoneRingFull;

    // terrain goes from -400 to +400, size = 800
    private const float TERRAIN_HALF = 400f;
    private const float TERRAIN_SIZE = 800f;

    void Update()
    {
        if (_playerTransform == null)
        {
            FindLocalPlayer();
            return;
        }

        if (!IsFullMapOpen)
        {
            Vector3 camPos = _playerTransform.position;
            camPos.y = 200f;
            _minimapCamera.transform.position = camPos;
        }

        UpdateMiniMap();

        if (IsFullMapOpen)
            UpdateFullMap();

        if (IsFullMapOpen)
            Debug.Log($"ArrowFull anchoredPos: {_playerArrowFull.anchoredPosition}");
    }

    private void UpdateMiniMap()
    {
        _playerArrow.gameObject.SetActive(!IsFullMapOpen);
        _playerArrow.anchoredPosition = Vector2.zero;
        float angle = _playerTransform.eulerAngles.y;
        _playerArrow.localRotation = Quaternion.Euler(0, 0, -angle);

        if (ZoneManager.Instance == null) return;

        float minimapSize = _minimapRect.rect.width;
        float worldToMinimap = minimapSize / (_minimapCameraSize * 2f);

        UpdateRing(_currentZoneRing, ZoneManager.Instance.CurrentCenter,
                   ZoneManager.Instance.CurrentRadius, worldToMinimap, false, 0, 0);

        bool isShrinking = ZoneManager.Instance.IsShrinking;
        _nextZoneRing.gameObject.SetActive(isShrinking);
        if (isShrinking)
            UpdateRing(_nextZoneRing, ZoneManager.Instance.NextCenter,
                       ZoneManager.Instance.NextRadius, worldToMinimap, false, 0, 0);
    }

    private void UpdateFullMap()
    {
        if (ZoneManager.Instance == null) return;

        float fullMapWidth = _fullMapRect.rect.width;
        float fullMapHeight = _fullMapRect.rect.height;
        float worldToFullMap = fullMapWidth / TERRAIN_SIZE;

        // player arrow — terrain -400 to +400, center = 0,0
        Vector3 playerPos = _playerTransform.position;
        float px = (playerPos.x / TERRAIN_HALF) * (fullMapWidth / 2f);
        float py = (playerPos.z / TERRAIN_HALF) * (fullMapHeight / 2f);
        _playerArrowFull.anchoredPosition = new Vector2(px, py);

        Debug.Log($"Player world: {playerPos.x:F1}, {playerPos.z:F1} | Arrow UI: {px:F1}, {py:F1} | MapSize: {fullMapWidth:F1}, {fullMapHeight:F1}");
        float angle = _playerTransform.eulerAngles.y;
        _playerArrowFull.localRotation = Quaternion.Euler(0, 0, -angle);

        // current zone
        UpdateRing(_currentZoneRingFull, ZoneManager.Instance.CurrentCenter,
                   ZoneManager.Instance.CurrentRadius, worldToFullMap, true, fullMapWidth, fullMapHeight);

        // next zone
        bool isShrinking = ZoneManager.Instance.IsShrinking;
        _nextZoneRingFull.gameObject.SetActive(isShrinking);
        if (isShrinking)
            UpdateRing(_nextZoneRingFull, ZoneManager.Instance.NextCenter,
                       ZoneManager.Instance.NextRadius, worldToFullMap, true, fullMapWidth, fullMapHeight);
    }

    private void UpdateRing(RectTransform ring, Vector3 worldCenter,
                            float worldRadius, float worldToMinimap, bool isFullMap,
                            float fullMapWidth, float fullMapHeight)
    {
        if (ring == null) return;

        float x, y;
        if (isFullMap)
        {
            // terrain -400 to +400
            x = (worldCenter.x / TERRAIN_HALF) * (fullMapWidth / 2f);
            y = (worldCenter.z / TERRAIN_HALF) * (fullMapHeight / 2f);
        }
        else
        {
            Vector3 offset = worldCenter - _playerTransform.position;
            x = offset.x * worldToMinimap;
            y = offset.z * worldToMinimap;
        }

        ring.anchoredPosition = new Vector2(x, y);
        float size = worldRadius * 2f * worldToMinimap;
        ring.sizeDelta = new Vector2(size, size);
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsOfType<PhotonPlayerSetup>();
        foreach (var p in players)
        {
            if (p.photonView.IsMine)
            {
                _playerTransform = p.transform;
                break;
            }
        }
    }
}