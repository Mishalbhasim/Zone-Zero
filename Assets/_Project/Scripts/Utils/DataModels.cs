using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string PlayerId;
    public string PlayerName;
    public int TotalScore;
    public int TotalKills;
    public int TotalDeaths;
    public int MatchesPlayed;
    public int MatchesWon;
}

[Serializable]
public class PlayerMatchData
{
    public string PlayerId;
    public string PlayerName;
    public int Kills;
    public int Deaths;
    public int Score;
    public bool IsWinner;
}

[Serializable]
public class LeaderboardEntry
{
    public string PlayerId;
    public string PlayerName;
    public int Score;
    public int TotalKills;
    public int Rank;
}