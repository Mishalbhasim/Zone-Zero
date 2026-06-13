using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsText;
    private float _deltaTime;

    void Update()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        if (_fpsText != null)
            _fpsText.text = $"FPS: {Mathf.Round(fps)}";
    }
}