using System.Collections.Generic;
using UnityEngine;

public class BotManager : SceneSingleton<BotManager>
{
    [Header("Bot Settings")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private int _totalSlots = 30;

    [Header("Testing Only")]
    [Tooltip("Simulates real players in room - remove when Photon is connected")]
    [SerializeField] private int _testRealPlayerCount = 25;

    public int TotalBots { get; private set; }
    public int BotsRemaining { get; private set; }

    private List<GameObject> _bots = new List<GameObject>();

    void Start()
    {
        EventBus.OnBotKilled += OnBotKilled;

        // TODO Day 16: replace _testRealPlayerCount with
        // PhotonNetwork.CurrentRoom.PlayerCount
        // and seed with PhotonNetwork.CurrentRoom.CustomProperties["mapSeed"]
        int seed = 12345;
        SpawnBots(seed, _testRealPlayerCount);
    }

    public void SpawnBots(int seed, int realPlayerCount)
    {
        int botsToSpawn = Mathf.Max(0, _totalSlots - realPlayerCount);

        TotalBots = botsToSpawn;
        BotsRemaining = botsToSpawn;
        _bots.Clear();

        for (int i = 0; i < botsToSpawn; i++)
        {
            Vector3 spawnPos = SpawnManager.Instance.GetSeededSpawnPoint(i, seed);
            var bot = Instantiate(_botPrefab, spawnPos, Quaternion.identity);
            bot.name = $"Bot_{i}";
            _bots.Add(bot);
        }

        Debug.Log($"[BotManager] Spawned {botsToSpawn} bots (slots: {_totalSlots}, players: {realPlayerCount})");
    }

    private void OnBotKilled(int botId)
    {
        BotsRemaining--;
        Debug.Log($"[BotManager] Bot killed. Remaining: {BotsRemaining}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventBus.OnBotKilled -= OnBotKilled;
    }
}