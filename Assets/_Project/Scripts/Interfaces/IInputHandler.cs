using UnityEngine;

public interface IInputHandler
{
    Vector2 MoveInput { get; }
    Vector2 LookInput { get; }
    bool JumpPressed { get; }
    bool ShootPressed { get; }
    bool ReloadPressed { get; }
    bool AimHeld { get; }
}