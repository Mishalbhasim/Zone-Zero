using UnityEngine;
using StarterAssets;

public class CanvasInputWatcher : MonoBehaviour
{
    private UICanvasControllerInput _canvasInput;

    void Awake()
    {
        _canvasInput = GetComponent<UICanvasControllerInput>();
    }

    void Update()
    {
        if (_canvasInput.starterAssetsInputs != null) return;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var inputs = player.GetComponent<StarterAssetsInputs>();
        if (inputs != null)
        {
            _canvasInput.starterAssetsInputs = inputs;
            enabled = false; // stop checking once found
        }
    }
}