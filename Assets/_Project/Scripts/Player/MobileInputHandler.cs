using UnityEngine;

public class MobileInputHandler : MonoBehaviour, IInputHandler
{
    [SerializeField] private VirtualJoystick _moveJoystick;

    public Vector2 MoveInput => _moveJoystick != null
                                 ? _moveJoystick.InputDirection
                                 : Vector2.zero;
    public Vector2 LookInput => Vector2.zero;
    public bool AimHeld { get; private set; }

    // jump
    private bool _jumpPressed;
    private bool _jumpConsumed;
    public bool JumpPressed
    {
        get
        {
            if (_jumpPressed && !_jumpConsumed)
            {
                _jumpConsumed = true;
                return true;
            }
            return false;
        }
    }

    // shoot
    private bool _shootPressed;
    private bool _shootConsumed;
    public bool ShootPressed
    {
        get
        {
            if (_shootPressed && !_shootConsumed)
            {
                _shootConsumed = true;
                return true;
            }
            return false;
        }
    }

    // reload
    private bool _reloadPressed;
    private bool _reloadConsumed;
    public bool ReloadPressed
    {
        get
        {
            if (_reloadPressed && !_reloadConsumed)
            {
                _reloadConsumed = true;
                return true;
            }
            return false;
        }
    }

    public void OnJumpButtonDown()
    {
        _jumpPressed = true;
        _jumpConsumed = false;
    }

    public void OnShootButtonDown()
    {
        _shootPressed = true;
        _shootConsumed = false;
    }

    public void OnReloadButtonDown()
    {
        _reloadPressed = true;
        _reloadConsumed = false;
    }

    void LateUpdate()
    {
        _jumpPressed = false;
        _shootPressed = false;
        _reloadPressed = false;
    }
}