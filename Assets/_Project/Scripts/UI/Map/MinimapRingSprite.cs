using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MinimapRingSprite : MonoBehaviour
{
    [SerializeField] private Color _color = Color.white;

    void Awake()
    {
        GetComponent<Image>().sprite = CreateRingSprite();
        GetComponent<Image>().color = _color;
    }

    private Sprite CreateRingSprite()
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float outerRadius = size / 2f;
        float innerRadius = outerRadius - 6f; // 6 pixel thin line

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist >= innerRadius && dist <= outerRadius)
                    pixels[y * size + x] = Color.white;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}