using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Lobby,
        LoadingMatch,
        InMatch,
        PostMatch
    }

    public GameState CurrentState { get; private set; }

    // local player informatin
    public string LocalPlayerId { get; set; }
    public string LocalPlayerName { get; set; }

    void Start()
    {
        TransitionTo(GameState.Boot);
    }

    public void TransitionTo(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] → {newState}");
    }
}