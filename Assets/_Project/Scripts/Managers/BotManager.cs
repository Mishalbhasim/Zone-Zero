using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BotManager : SceneSingleton<BotManager>
{
    [Header("Bot Settings")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private int _totalSlots = 30;

    [Header("Testing Only — Remove when Photon connected")]
    [Tooltip("Only used if not connected to Photon")]
    [SerializeField] private int _testRealPlayerCount = 1;

    public int TotalBots { get; private set; }
    public int BotsRemaining { get; private set; }

    private List<GameObject> _bots = new List<GameObject>();

    [Header("LOD Settings")]
    [SerializeField] private float _activeRange = 60f;
    [SerializeField] private int _maxActiveBots = 12;
    [SerializeField] private float _lodUpdateInterval = 1f;
    private float _lodTimer;

    void Start()
    {
        EventBus.OnBotKilled += OnBotKilled;

        
    }

    void Update()
    {
        _lodTimer += Time.deltaTime;
        if (_lodTimer < _lodUpdateInterval) return;
        _lodTimer = 0f;
        UpdateBotLOD();
    }

    private void UpdateBotLOD()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;
        Vector3 playerPos = playerObj.transform.position;

        var sortedBots = new List<(GameObject bot, float dist)>();
        foreach (var bot in _bots)
        {
            if (bot == null || !bot.activeSelf) continue;
            float dist = Vector3.Distance(bot.transform.position, playerPos);
            sortedBots.Add((bot, dist));
        }
        sortedBots.Sort((a, b) => a.dist.CompareTo(b.dist));

        for (int i = 0; i < sortedBots.Count; i++)
        {
            var botSM = sortedBots[i].bot.GetComponent<BotStateMachine>();
            if (botSM == null) continue;
            bool shouldBeActive = i < _maxActiveBots &&
                                   sortedBots[i].dist <= _activeRange;
            SetBotActive(botSM, shouldBeActive);
        }
    }

    private void SetBotActive(BotStateMachine botSM, bool active)
    {
        if (botSM.IsActive == active) return;
        botSM.IsActive = active;
        botSM.Agent.enabled = active;
        if (!active)
            botSM.BotAnimator?.SetFloat(botSM.SpeedHash, 0f);
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

        // initialize match with total players including bot and players
        int totalAlive = botsToSpawn + realPlayerCount;
        MatchManager.Instance?.StartCountdown(totalAlive);
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