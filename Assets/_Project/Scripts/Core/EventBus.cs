using System;
using UnityEngine;

public static class EventBus
{
    //weapon
    public static event Action<int, int> OnAmmoChanged;
    public static void AmmoChanged(int current, int max)
        => OnAmmoChanged?.Invoke(current, max);


    public static event Action<string> OnWeaponFired;
    public static void WeaponFired(string weaponType)
        => OnWeaponFired?.Invoke(weaponType);


    //Health
    public static event Action<int, int> OnPlayerHealthChanged;
    public static void PlayerHealthChanged(int current, int max)
        => OnPlayerHealthChanged?.Invoke(current, max);

    public static event Action OnPlayerDied;
    public static void PlayerDied()
        => OnPlayerDied?.Invoke();

    public static event Action<Vector3> OnPlayerRespawned;
    public static void PlayerRespawned(Vector3 position)
        => OnPlayerRespawned?.Invoke(position);

    //Death
    public static event Action<int> OnRespawnTimerTick;
    public static void RespawnTimerTick(int secondsLeft)
        => OnRespawnTimerTick?.Invoke(secondsLeft);




    public static event Action<string, int> OnPlayerEliminated;
    public static void PlayerEliminated(string playerId, int placement)
        => OnPlayerEliminated?.Invoke(playerId, placement);

    public static event Action<string> OnPlayerWon;
    public static void PlayerWon(string playerId)
        => OnPlayerWon?.Invoke(playerId);

    public static event Action<int> OnPlayersRemainingChanged;
    public static void PlayersRemainingChanged(int remaining)
        => OnPlayersRemainingChanged?.Invoke(remaining);

    public static event Action<int, int> OnPlayerScoreChanged;
    public static void PlayerScoreChanged(int newScore, int delta)
        => OnPlayerScoreChanged?.Invoke(newScore, delta);

    public static event Action<int> OnMatchStarted;
    public static void MatchStarted(int totalPlayers)
        => OnMatchStarted?.Invoke(totalPlayers);


    public static event Action<int> OnBotKilled;
    public static void BotKilled(int botId)
        => OnBotKilled?.Invoke(botId);


    //Zone
    public static event Action<Vector3, float, Vector3, float, float> OnZoneShrinkStarted;
    public static void ZoneShrinkStarted(Vector3 curCenter, float curRadius,
                                          Vector3 nextCenter, float nextRadius, float duration)
        => OnZoneShrinkStarted?.Invoke(curCenter, curRadius, nextCenter, nextRadius, duration);

    public static event Action<int> OnZoneDamageTick;
    public static void ZoneDamageTick(int damage)
        => OnZoneDamageTick?.Invoke(damage);

    public static event Action<int> OnZonePhaseChanged;
    public static void ZonePhaseChanged(int phase)
        => OnZonePhaseChanged?.Invoke(phase);
}