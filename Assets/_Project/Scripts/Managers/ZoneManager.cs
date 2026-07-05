using System.Collections;
using UnityEngine;

public class ZoneManager : SceneSingleton<ZoneManager>
{
    [Header("Map Settings")]
    [SerializeField] private float _mapSize = 800f;

    [Header("Zone Phases")]
    [SerializeField]
    private float[] _radiusSteps =
        { 400f, 300f, 200f, 120f, 70f, 30f };
    [SerializeField] private float _waitTime = 30f;
    [SerializeField] private float _shrinkTime = 25f;
    [SerializeField] private int _damagePerSecond = 5;
    public bool IsShrinking => _isShrinking;

    public Vector3 CurrentCenter { get; private set; }
    public float CurrentRadius { get; private set; }

    private Vector3 _nextCenter;
    private float _nextRadius;
    private bool _isShrinking;
    private float _shrinkTimer;
    private float _shrinkDuration;
    private float _damageTimer;
    public Vector3 NextCenter { get; private set; }
    public float NextRadius { get; private set; }

    void Start()
    {
        // zone starts covering the whole map
        CurrentCenter = Vector3.zero;
        CurrentRadius = _mapSize * 0.75f;

        StartCoroutine(ZoneRoutine());
    }

    private IEnumerator ZoneRoutine()
    {
        for (int phase = 0; phase < _radiusSteps.Length; phase++)
        {
            // wait before shrinking (players reposition)
            yield return new WaitForSeconds(_waitTime);

            // pick next zone
            NextRadius = _radiusSteps[phase];
            NextCenter = PickNextCenter(CurrentCenter, CurrentRadius, NextRadius, phase);
            _nextRadius = NextRadius;
            _nextCenter = NextCenter;

            EventBus.ZonePhaseChanged(phase + 1);
            EventBus.ZoneShrinkStarted(CurrentCenter, CurrentRadius,
                                        _nextCenter, _nextRadius, _shrinkTime);

            // shrink over time
            _isShrinking = true;
            _shrinkTimer = 0f;
            _shrinkDuration = _shrinkTime;

            Vector3 startCenter = CurrentCenter;
            float startRadius = CurrentRadius;

            while (_shrinkTimer < _shrinkDuration)
            {
                _shrinkTimer += Time.deltaTime;
                float t = _shrinkTimer / _shrinkDuration;

                CurrentCenter = Vector3.Lerp(startCenter, _nextCenter, t);
                CurrentRadius = Mathf.Lerp(startRadius, _nextRadius, t);

                yield return null;
            }

            CurrentCenter = _nextCenter;
            CurrentRadius = _nextRadius;
            _isShrinking = false;
        }
    }

    void Update()
    {
        if (_isShrinking || CurrentRadius <= 0) return;
        CheckPlayerZoneDamage();
    }

    private void CheckPlayerZoneDamage()
    {
        _damageTimer += Time.deltaTime;
        if (_damageTimer < 1f) return;
        _damageTimer = 0f;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(
            new Vector3(player.transform.position.x, 0, player.transform.position.z),
            new Vector3(CurrentCenter.x, 0, CurrentCenter.z)
        );

        if (dist > CurrentRadius)
        {
           

            EventBus.ZoneDamageTick(_damagePerSecond);
        }
    }

    private Vector3 PickNextCenter(Vector3 currentCenter, float currentRadius, float nextRadius, int phase)
    {
        float maxOffset = Mathf.Max(0, currentRadius - nextRadius);

        int seed = 12345; // fallback seed
        if (PhotonNetworkManager.Instance != null)
            seed = PhotonNetworkManager.Instance.MapSeed;

        var rng = new System.Random(seed + phase);
        float x = (float)(rng.NextDouble() * 2 - 1) * maxOffset;
        float z = (float)(rng.NextDouble() * 2 - 1) * maxOffset;
        return currentCenter + new Vector3(x, 0, z);
    }
}