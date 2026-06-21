using UnityEngine;

public class ZoneVisual : MonoBehaviour
{
    [SerializeField] private Material _zoneMaterial;

    [Header("Wall Settings")]
    [SerializeField] private float _wallHeight = 150f;
    [SerializeField] private int _cylinderSegments = 64;

    [Header("Pulse Effect")]
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _pulseIntensity = 0.05f;
    [SerializeField] private Color _wallColor = new Color(0.1f, 0.4f, 1f, 0.35f);

    private GameObject _wallObject;
    private MeshRenderer _meshRenderer;
    private Material _wallMaterial;

    void Start()
    {
        if (_zoneMaterial == null)
        {
            Debug.LogError("[ZoneVisual] Zone Material not assigned in inspector!");
            return;
        }

        // create wall GameObject
        _wallObject = new GameObject("ZoneWall");
        _wallObject.transform.SetParent(transform);

        // add mesh components
        var meshFilter = _wallObject.AddComponent<MeshFilter>();
        _meshRenderer = _wallObject.AddComponent<MeshRenderer>();

        // build mesh
        meshFilter.mesh = BuildCylinderMesh(_cylinderSegments, 1f, _wallHeight);

        // create material from assigned material
        _wallMaterial = new Material(_zoneMaterial);
        _wallMaterial.SetFloat("_Cull", 0f); // both sides visible
        _meshRenderer.material = _wallMaterial;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
    }

    void Update()
    {
        if (_wallObject == null) return;
        if (ZoneManager.Instance == null) return;

        float radius = ZoneManager.Instance.CurrentRadius;
        Vector3 center = ZoneManager.Instance.CurrentCenter;

        // position wall at zone center
        _wallObject.transform.position = new Vector3(center.x, _wallHeight * 0.5f - 10f, center.z);

        // scale to zone radius
        _wallObject.transform.localScale = new Vector3(radius, 1f, radius);

        // pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseIntensity;
        Color pulsedColor = new Color(_wallColor.r, _wallColor.g, _wallColor.b, _wallColor.a * pulse);
        _wallMaterial.SetColor("_BaseColor", pulsedColor);
    }

    private Mesh BuildCylinderMesh(int segments, float radius, float height)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ZoneWallMesh";

        int vertCount = segments * 2;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] tris = new int[segments * 6];

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            verts[i] = new Vector3(x, 0, z);
            uvs[i] = new Vector2((float)i / segments, 0);

            verts[i + segments] = new Vector3(x, height, z);
            uvs[i + segments] = new Vector2((float)i / segments, 1);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int ti = i * 6;

            // flipped winding — visible from inside
            tris[ti + 0] = i;
            tris[ti + 1] = next;
            tris[ti + 2] = i + segments;

            tris[ti + 3] = next;
            tris[ti + 4] = next + segments;
            tris[ti + 5] = i + segments;
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        return mesh;
    }

    void OnDestroy()
    {
        if (_wallMaterial != null) Destroy(_wallMaterial);
    }
}