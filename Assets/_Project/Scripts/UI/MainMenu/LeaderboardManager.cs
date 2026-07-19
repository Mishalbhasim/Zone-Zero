using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _rowPrefab;

    [Header("Settings")]
    [SerializeField] private string _statisticName = "TotalScore";
    [SerializeField] private int _maxResults = 20;


    [Header("Your Rank Row")]
    [SerializeField] private GameObject _yourRankRow;
    [SerializeField] private TMP_Text _yourRankRankText;
    [SerializeField] private TMP_Text _yourRankNameText;
    [SerializeField] private TMP_Text _yourRankScoreText;

    

    private List<GameObject> _spawnedRows = new List<GameObject>();

    public void FetchAndDisplay()
    {
        ClearRows();
        var request = new GetLeaderboardRequest
        {
            StatisticName = _statisticName,
            StartPosition = 0,
            MaxResultsCount = _maxResults
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardReceived, OnError);

        FetchYourRank();
    }

    void FetchYourRank()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = _statisticName,
            MaxResultsCount = 1
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnYourRankReceived, OnError);
    }

    void OnYourRankReceived(GetLeaderboardAroundPlayerResult result)
    {
        if (result.Leaderboard == null || result.Leaderboard.Count == 0)
        {
            _yourRankRow.SetActive(false);
            return;
        }

        _yourRankRow.SetActive(true);
        var entry = result.Leaderboard[0];
        _yourRankRankText.text = (entry.Position + 1).ToString();
        _yourRankNameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Player" : entry.DisplayName;
        _yourRankScoreText.text = entry.StatValue.ToString();
    }

    private void OnLeaderboardReceived(GetLeaderboardResult result)
    {
        foreach (var entry in result.Leaderboard)
        {
            GameObject row = Instantiate(_rowPrefab, _content);
            row.SetActive(true);

            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var t in texts)
            {
                if (t.gameObject.name == "RankText")
                    t.text = (entry.Position + 1).ToString();
                else if (t.gameObject.name == "PlayerNameText")
                    t.text = string.IsNullOrEmpty(entry.DisplayName) ? "Player" : entry.DisplayName;
                else if (t.gameObject.name == "ScoreText")
                    t.text = entry.StatValue.ToString();
            }

            _spawnedRows.Add(row);
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"[Leaderboard] Failed to fetch: {error.GenerateErrorReport()}");
    }

    private void ClearRows()
    {
        foreach (var row in _spawnedRows)
            Destroy(row);
        _spawnedRows.Clear();
    }
}