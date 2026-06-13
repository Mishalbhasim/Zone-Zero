using System;
using System.Collections.Generic;

public interface ILeaderboardService
{
    void GetTopPlayers(int count,
                       Action<List<PlayerData>> onSuccess,
                       Action<string> onFail);

    void UpdateEntry(string playerId, string playerName, int score,
                       Action onSuccess, Action<string> onFail);
}