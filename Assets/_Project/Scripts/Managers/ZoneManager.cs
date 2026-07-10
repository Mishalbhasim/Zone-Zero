using System.Collections;
using UnityEngine;
using Photon.Pun;

public class ZoneManager : SceneSingleton<ZoneManager>
{
    [Header("Map Settings")]
    [SerializeField] private float _mapSize = 800f;

    [Header("Zone Phases")]
    [SerializeField]
    private float[] _radiusSteps = { 400f, 300f, 200f, 120f, 70f, 30f };
    [SerializeField] private float _waitTime = 30f;
    [SerializeField] private float _shrinkTime = 25f;
    [SerializeField] private int _damagePerSecond = 5;

    public bool IsShrinking => _isShrinking;
    public Vector3 CurrentCenter { get; private set; }
    public float CurrentRadius { get; private set; }
    public Vector3 NextCenter { get; private set; }
    public float NextRadius { get; private set; }

    private bool _isShrinking;
    private float _damageTimer;

    void Start()
    {
        CurrentCenter = Vector3.zero;
        CurrentRadius = _mapSize * 0.75f;
        StartCoroutine(ZoneRoutine());
    }

    private IEnumerator ZoneRoutine()
    {
        for (int phase = 0; phase < _radiusSteps.Length; phase++)
        {
            // warn players before shrinking
            EventBus.ZoneWarning(_waitTime);
            yield return new WaitForSeconds(_waitTime);

            // pick next zone
            NextRadius = _radiusSteps[phase];
            NextCenter = PickNextCenter(CurrentCenter, CurrentRadius, NextRadius, phase);

            EventBus.ZonePhaseChanged(phase + 1);
            EventBus.ZoneShrinkStarted(CurrentCenter, CurrentRadius,
                                        NextCenter, NextRadius, _shrinkTime);

            // shrink over time
            _isShrinking = true;
            float shrinkTimer = 0f;
            Vector3 startCenter = CurrentCenter;
            float startRadius = CurrentRadius;

            while (shrinkTimer < _shrinkTime)
            {
                shrinkTimer += Time.deltaTime;
                float t = shrinkTimer / _shrinkTime;
                CurrentCenter = Vector3.Lerp(startCenter, NextCenter, t);
                CurrentRadius = Mathf.Lerp(startRadius, NextRadius, t);
                yield return null;
            }

            CurrentCenter = NextCenter;
            CurrentRadius = NextRadius;
            _isShrinking = false;
        }

        // final phase — shrink to zero
        EventBus.ZoneWarning(_waitTime);
        yield return new WaitForSeconds(_waitTime);

        NextRadius = 0f;
        NextCenter = CurrentCenter;
        EventBus.ZoneShrinkStarted(CurrentCenter, CurrentRadius, NextCenter, NextRadius, _shrinkTime);

        _isShrinking = true;
        float finalTimer = 0f;
        float finalStartRadius = CurrentRadius;
        Vector3 finalStartCenter = CurrentCenter;

        while (finalTimer < _shrinkTime)
        {
            finalTimer += Time.deltaTime;
            float t = finalTimer / _shrinkTime;
            CurrentRadius = Mathf.Lerp(finalStartRadius, 0f, t);
            yield return null;
        }

        CurrentRadius = 0f;
        _isShrinking = false;
    }

    void Update()
    {
        if (_isShrinking) return;
        CheckPlayerZoneDamage();
    }
    private void CheckPlayerZoneDamage()
    {
        _damageTimer += Time.deltaTime;
        if (_damageTimer < 1f) return;
        _damageTimer = 0f;

        // damage player
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(
                new Vector3(player.transform.position.x, 0, player.transform.position.z),
                new Vector3(CurrentCenter.x, 0, CurrentCenter.z)
            );
            if (dist > CurrentRadius)
                EventBus.ZoneDamageTick(_damagePerSecond);
        }

        // damage bots outside zone (master only)
        if (!PhotonNetwork.IsMasterClient) return;
        var bots = GameObject.FindGameObjectsWithTag("Bot");
        foreach (var bot in bots)
        {
            if (bot == null) continue;
            float dist = Vector3.Distance(
                new Vector3(bot.transform.position.x, 0, bot.transform.position.z),
                new Vector3(CurrentCenter.x, 0, CurrentCenter.z)
            );
            if (dist > CurrentRadius)
            {
                var botSM = bot.GetComponent<BotStateMachine>();
                botSM?.TakeDamage(_damagePerSecond);
            }
        }
    }

    private Vector3 PickNextCenter(Vector3 currentCenter, float currentRadius, float nextRadius, int phase)
    {
        float maxOffset = Mathf.Max(0, currentRadius - nextRadius);
        int seed = 12345;
        if (PhotonNetworkManager.Instance != null)
            seed = PhotonNetworkManager.Instance.MapSeed;
        var rng = new System.Random(seed + phase);
        float x = (float)(rng.NextDouble() * 2 - 1) * maxOffset;
        float z = (float)(rng.NextDouble() * 2 - 1) * maxOffset;
        return currentCenter + new Vector3(x, 0, z);
    }
}