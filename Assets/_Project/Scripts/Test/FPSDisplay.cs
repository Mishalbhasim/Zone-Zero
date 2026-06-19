using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    float timer;
    int frames;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= 0.5f)
        {
            int fps = Mathf.RoundToInt(frames / timer);
            fpsText.text = fps + " FPS";

            frames = 0;
            timer = 0;
        }
    }
}
