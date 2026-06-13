using UnityEngine;

public class SpawnManager : SceneSingleton<SpawnManager>
{
    [Header("Spawn Settings")]
    [SerializeField] private float _mapWidth = 800f;
    [SerializeField] private float _mapLength = 800f;
    [SerializeField] private float _minDistanceBetweenPlayers = 50f;
    [SerializeField] private LayerMask _terrainLayer;

    private System.Collections.Generic.List<Vector3> _usedSpawns
        = new System.Collections.Generic.List<Vector3>();

    public Vector3 GetRandomSpawnPoint()
    {
        int attempts = 0;
        Vector3 spawnPos;

        do
        {
            // random position on map
            float x = Random.Range(-_mapWidth * 0.5f, _mapWidth * 0.5f);
            float z = Random.Range(-_mapLength * 0.5f, _mapLength * 0.5f);

            // sample terrain height at that position
            float y = SampleTerrainHeight(x, z);
            spawnPos = new Vector3(x, y + 1f, z);

            attempts++;
            if (attempts > 50) break;
        }
        while (IsTooCloseToOtherSpawns(spawnPos));

        _usedSpawns.Add(spawnPos);
        return spawnPos;
    }

    public void ResetSpawns() => _usedSpawns.Clear();

    private float SampleTerrainHeight(float x, float z)
    {
        if (Terrain.activeTerrain == null) return 0f;
        return Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
    }

    private bool IsTooCloseToOtherSpawns(Vector3 pos)
    {
        foreach (var spawn in _usedSpawns)
            if (Vector3.Distance(pos, spawn) < _minDistanceBetweenPlayers)
                return true;
        return false;
    }

    //Determines where the bot gets spawned
    public Vector3 GetSeededSpawnPoint(int index, int seed)
    {
        var rng = new System.Random(seed + index);

        float x = (float)(rng.NextDouble() * _mapWidth) - _mapWidth * 0.5f;
        float z = (float)(rng.NextDouble() * _mapLength) - _mapLength * 0.5f;
        float y = SampleTerrainHeight(x, z);

        Vector3 rawPos = new Vector3(x, y + 1f, z);

        // snap to nearest valid NavMesh point
        if (UnityEngine.AI.NavMesh.SamplePosition(rawPos, out var hit, 50f, UnityEngine.AI.NavMesh.AllAreas))
            return hit.position;

        return rawPos;
    }
}