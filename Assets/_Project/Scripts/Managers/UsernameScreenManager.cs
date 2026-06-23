using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class UsernameScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private TextMeshProUGUI _charCountText;

    [Header("Settings")]
    [SerializeField] private int _minLength = 3;
    [SerializeField] private int _maxLength = 12;
    private const string MAINMENU_SCENE = "MainMenu";

    void Start()
    {
        // setup input field
        _usernameInput.characterLimit = _maxLength;
        _usernameInput.onValueChanged.AddListener(OnInputChanged);
        _confirmButton.onClick.AddListener(OnConfirmClicked);

        // hide error on start
        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

        UpdateCharCount("");
    }

    private void OnInputChanged(string value)
    {
        UpdateCharCount(value);

        // hide error while typing
        if (_errorText != null)
            _errorText.gameObject.SetActive(false);
    }

    private void UpdateCharCount(string value)
    {
        if (_charCountText != null)
            _charCountText.text = $"{value.Length}/{_maxLength}";
    }

    private void OnConfirmClicked()
    {
        string username = _usernameInput.text.Trim();

        // validate
        if (string.IsNullOrEmpty(username))
        {
            ShowError("Name cannot be empty");
            return;
        }

        if (username.Length < _minLength)
        {
            ShowError($"Name must be at least {_minLength} characters");
            return;
        }

        // save and go to MainMenu
        BootManager.SaveUsername(username);
        GameManager.Instance.TransitionTo(GameManager.GameState.MainMenu);
        SceneManager.LoadScene(MAINMENU_SCENE);
    }

    private void ShowError(string message)
    {
        if (_errorText != null)
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }
    }
}