using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Events Registry")]
public class GameEventsSO : ScriptableObject
{
    [Header("Game State")]
    public GameEvent OnMatchStarted;
    public GameEvent OnMatchEnded;
    public GameEvent OnGamePaused;
    public GameEvent OnGameResumed;

    [Header("Player")]
    public IntEvent OnPlayerDamaged;    // payload = damage amount
    public GameEvent OnPlayerDied;
    public GameEvent OnPlayerRespawned;
    public IntEvent OnHealthChanged;    // payload = current HP

    [Header("Score")]
    public StringEvent OnPlayerKill;       // payload = killer ID
    public StringEvent OnPlayerDeath;      // payload = victim ID

    [Header("UI")]
    public StringEvent OnShowScreen;       // payload = screen name / enum string
}