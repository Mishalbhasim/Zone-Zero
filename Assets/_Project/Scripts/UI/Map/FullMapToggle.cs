using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.InputSystem;
#endif

public class FullMapToggle : MonoBehaviour
{
    [SerializeField] private GameObject _fullMapPanel;
    [SerializeField] private Camera _minimapCamera;
    [SerializeField] private RectTransform _playerArrow;
    [SerializeField] private float _miniOrthoSize = 100f;
    [SerializeField] private float _fullOrthoSize = 400f;
    [SerializeField] private float _terrainCenter = 400f;

    private bool _isFullMap = false;
    private MinimapController _minimapController;

    void Start()
    {
        _minimapController = GetComponent<MinimapController>();
        // click minimap to open full map
        var minimapPanel = GetComponent<UnityEngine.UI.RawImage>();
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current.rightButton.wasPressedThisFrame)
            ToggleFullMap();
#endif
    }

    public void ToggleFullMap()
    {
        _isFullMap = !_isFullMap; // toggle FIRST

        var minimapController = GetComponent<MinimapController>();
        minimapController.IsFullMapOpen = _isFullMap; // then set

        _fullMapPanel.SetActive(_isFullMap);

        if (_isFullMap)
        {
            _minimapCamera.orthographicSize = _fullOrthoSize;
            _minimapCamera.transform.position = new Vector3(0f, 200f, 0f);
            _playerArrow.localScale = Vector3.one * 0.3f;
        }
        else
        {
            _minimapCamera.orthographicSize = _miniOrthoSize;
            _playerArrow.localScale = Vector3.one;
        }
    }
}