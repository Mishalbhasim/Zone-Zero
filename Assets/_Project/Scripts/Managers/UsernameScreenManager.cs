using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class UsernameScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private TextMeshProUGUI _charCountText;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _confirmPasswordInput;
    [SerializeField] private Button _toggleModeButton;
    [SerializeField] private TextMeshProUGUI _toggleModeText;
    [SerializeField] private TextMeshProUGUI _confirmButtonText;

    private bool _isRegisterMode = true;

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

        _toggleModeButton?.onClick.AddListener(OnToggleModeClicked);
        SetMode(false); //start in login mode by default

        // hide error on start
        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

        UpdateCharCount("");
    }

    private void SetMode(bool isRegister)
    {
        _isRegisterMode = isRegister;

        if (_confirmPasswordInput != null)
            _confirmPasswordInput.gameObject.SetActive(_isRegisterMode);

        if (_confirmButtonText != null)
            _confirmButtonText.text = _isRegisterMode ? "REGISTER" : "LOGIN";

        if (_toggleModeText != null)
            _toggleModeText.text = _isRegisterMode
                ? "Already have an account? Login here"
                : "New player? Register here";

        if (_titleText != null)
            _titleText.text = _isRegisterMode ? "SIGN UP" : "LOGIN";

        if (_errorText != null)
            _errorText.gameObject.SetActive(false);
    }

    private void OnToggleModeClicked()
    {
        SetMode(!_isRegisterMode);
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
        string password = _passwordInput != null ? _passwordInput.text : "";

        // shared validation
        if (string.IsNullOrEmpty(username))
        {
            ShowError("Username cannot be empty");
            return;
        }

        if (username.Length < _minLength)
        {
            ShowError($"Username must be at least {_minLength} characters");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Password cannot be empty");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters");
            return;
        }

        if (_isRegisterMode)
        {
            string confirmPassword = _confirmPasswordInput != null ? _confirmPasswordInput.text : "";
            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            DoRegister(username, password);
        }
        else
        {
            DoLogin(username, password);
        }
    }

    private void DoRegister(string username, string password)
    {
        _confirmButton.interactable = false;

        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = $"{username.ToLower()}@zonezero.internal",
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnPlayFabError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        
        _confirmButton.interactable = true;
        CompleteLogin(_usernameInput.text.Trim());
    }

    private void DoLogin(string username, string password)
    {
        _confirmButton.interactable = false;

        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnPlayFabError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
       
        _confirmButton.interactable = true;
        CompleteLogin(_usernameInput.text.Trim());
    }

    private void OnPlayFabError(PlayFabError error)
    {
        _confirmButton.interactable = true;
        Debug.LogError($"[Auth] PlayFab error: {error.GenerateErrorReport()}");

        switch (error.Error)
        {
            case PlayFabErrorCode.UsernameNotAvailable:
                ShowError("That username is already taken");
                break;
            case PlayFabErrorCode.InvalidUsernameOrPassword:
            case PlayFabErrorCode.AccountNotFound:
                ShowError("Incorrect username or password");
                break;
            case PlayFabErrorCode.InvalidPassword:
                ShowError("Password does not meet requirements");
                break;
            default:
                ShowError("Something went wrong. Please try again");
                break;
        }
    }

    private void CompleteLogin(string username)
    {
       
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = username
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => Debug.Log("[Auth] Display name set successfully"),
            error => Debug.LogError($"[Auth] Failed to set display name: {error.GenerateErrorReport()}")
        );

        
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