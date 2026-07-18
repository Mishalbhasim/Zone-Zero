using UnityEngine;
using System.Collections.Generic;

public class TreeScatterWithColliders : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] treePrefabs;
    public int treeCount = 2000;
    public bool alignTreesToSlope = false;

    public GameObject[] rockPrefabs;
    public int rockCount = 1000;
    public bool alignRocksToSlope = true;

    public Terrain terrain;

    [Header("Slope")]
    public float maxSlopeAngle = 40f;

    [Header("Exclusion Zones (spawn points, POIs, roads etc.)")]
    public Transform[] exclusionZones;
    public float exclusionRadius = 15f;

    [Header("Spacing (avoid overlap between placed objects)")]
    public float minSpacing = 3f;

    private List<Vector3> placedPositions = new List<Vector3>();

    [ContextMenu("Scatter Now")]
    void Scatter()
    {
        if (terrain == null)
        {
            Debug.LogError("Assign terrain first.");
            return;
        }

        placedPositions.Clear();

        // clear previous children if re-running
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

        int treesPlaced = ScatterType(treePrefabs, treeCount, alignTreesToSlope);
        int rocksPlaced = ScatterType(rockPrefabs, rockCount, alignRocksToSlope);

        Debug.Log($"Scattered trees {treesPlaced}/{treeCount}, rocks {rocksPlaced}/{rockCount}.");
    }

    int ScatterType(GameObject[] prefabSet, int targetCount, bool alignToSlope)
    {
        if (prefabSet == null || prefabSet.Length == 0 || targetCount <= 0) return 0;

        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 60;

        while (placed < targetCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(0f, data.size.x);
            float z = Random.Range(0f, data.size.z);
            Vector3 worldXZ = new Vector3(x, 0, z) + terrainPos;

            // slope check
            float normX = x / data.size.x;
            float normZ = z / data.size.z;
            Vector3 normal = data.GetInterpolatedNormal(normX, normZ);
            float slopeAngle = Vector3.Angle(normal, Vector3.up);
            if (slopeAngle > maxSlopeAngle) continue;

            float y = terrain.SampleHeight(worldXZ) + terrainPos.y;
            Vector3 pos = new Vector3(worldXZ.x, y, worldXZ.z);

            // exclusion zones
            bool excluded = false;
            if (exclusionZones != null)
            {
                foreach (var zone in exclusionZones)
                {
                    if (zone == null) continue;
                    if (Vector3.Distance(pos, zone.position) < exclusionRadius)
                    {
                        excluded = true;
                        break;
                    }
                }
            }
            if (excluded) continue;

            // spacing check vs already placed
            bool tooClose = false;
            foreach (var p in placedPositions)
            {
                if (Vector3.Distance(pos, p) < minSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            GameObject prefab = prefabSet[Random.Range(0, prefabSet.Length)];
            GameObject instance =
#if UNITY_EDITOR
                (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
#else
                Instantiate(prefab, transform);
#endif
            instance.transform.position = pos;

            float yRot = Random.Range(0, 360f);
            if (alignToSlope)
            {
                // align up-axis to terrain normal, keep random yaw
                Quaternion slopeRot = Quaternion.FromToRotation(Vector3.up, normal);
                instance.transform.rotation = slopeRot * Quaternion.Euler(0, yRot, 0);
            }
            else
            {
                instance.transform.rotation = Quaternion.Euler(0, yRot, 0);
            }

#if UNITY_EDITOR
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(instance,
                UnityEditor.StaticEditorFlags.BatchingStatic);
#else
            // runtime: leave isStatic off, NavMeshObstacle handles carving
#endif

            placedPositions.Add(pos);
            placed++;
        }

        return placed;
    }

    [ContextMenu("Clear Scattered")]
    void ClearScattered()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }
        placedPositions.Clear();
    }
}