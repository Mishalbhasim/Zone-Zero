using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ZoneRingUI — draws animated rotating dashed ring on Main Menu.
/// Uses raw GL drawing on a RawImage component.
/// 
/// Setup:
/// 1. Create empty GameObject under LeftPanel named "ZoneRing"
/// 2. Add RawImage component to it
/// 3. Attach this script
/// 4. Set Width + Height to 400x400 in Rect Transform
/// </summary>
[RequireComponent(typeof(RawImage))]
public class ZoneRingUI : MonoBehaviour
{
    [Header("Ring Settings")]
    [SerializeField] private int _textureSize = 512;
    [SerializeField] private int _segments = 24;
    [SerializeField] private float _ringRadius = 180f;
    [SerializeField] private float _ringThickness = 3f;
    [SerializeField] private Color _ringColor = new Color(0f, 0.898f, 1f, 0.8f);

    [Header("Animation")]
    [SerializeField] private float _rotationSpeed = 15f;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _pulseMin = 0.3f;
    [SerializeField] private float _pulseMax = 0.9f;

    private RawImage _rawImage;
    private Texture2D _texture;
    private float _currentAngle = 0f;

    void Start()
    {
        _rawImage = GetComponent<RawImage>();
        _texture = new Texture2D(_textureSize, _textureSize, TextureFormat.RGBA32, false);
        _rawImage.texture = _texture;
        _rawImage.color = Color.white;
    }

    void Update()
    {
        _currentAngle += _rotationSpeed * Time.deltaTime;

        float pulse = Mathf.Lerp(_pulseMin, _pulseMax,
            (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f);

        DrawRing(_currentAngle, pulse);
    }

    private void DrawRing(float rotationDeg, float alpha)
    {
        // clear texture
        Color[] pixels = new Color[_textureSize * _textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Vector2 center = new Vector2(_textureSize * 0.5f, _textureSize * 0.5f);
        float gapAngle = 360f / _segments * 0.2f;

        for (int s = 0; s < _segments; s++)
        {
            float startAngle = (360f / _segments * s) + rotationDeg + gapAngle;
            float endAngle = (360f / _segments * (s + 1)) + rotationDeg - gapAngle;

            DrawArc(pixels, center, _ringRadius, _ringThickness,
                startAngle, endAngle,
                new Color(_ringColor.r, _ringColor.g, _ringColor.b, alpha));
        }

        // inner ring (faint)
        DrawArc(pixels, center, _ringRadius * 0.8f, 1f,
            0, 360,
            new Color(_ringColor.r, _ringColor.g, _ringColor.b, alpha * 0.2f));

        _texture.SetPixels(pixels);
        _texture.Apply();
    }

    private void DrawArc(Color[] pixels, Vector2 center, float radius,
        float thickness, float startDeg, float endDeg, Color color)
    {
        int steps = Mathf.RoundToInt((endDeg - startDeg) * radius * 0.05f);
        steps = Mathf.Max(steps, 20);

        for (int i = 0; i <= steps; i++)
        {
            float angle = Mathf.Lerp(startDeg, endDeg, (float)i / steps) * Mathf.Deg2Rad;

            for (float r = radius - thickness; r <= radius + thickness; r += 0.5f)
            {
                int px = Mathf.RoundToInt(center.x + Mathf.Cos(angle) * r);
                int py = Mathf.RoundToInt(center.y + Mathf.Sin(angle) * r);

                if (px >= 0 && px < _textureSize && py >= 0 && py < _textureSize)
                    pixels[py * _textureSize + px] = color;
            }
        }
    }

    void OnDestroy()
    {
        if (_texture != null)
            Destroy(_texture);
    }
}