using System;

public interface IDataService
{
    void SavePlayerData(PlayerData data,
                          Action onSuccess, Action<string> onFail);

    void LoadPlayerData(string playerId,
                          Action<PlayerData> onSuccess, Action<string> onFail);

    void UpdateScoreAfterMatch(string playerId, int kills,
                               int deaths, int score, bool won,
                               Action onSuccess, Action<string> onFail);
}