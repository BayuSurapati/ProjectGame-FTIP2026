using System.Collections.Generic;
using UnityEngine;

// "TUBUH" papan: membaca LevelData, membuat GridModel,
// lalu membangun tampilan 3D berdasarkan isi GridModel.
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelData level;

    [Tooltip("Aturan wilayah yang dipakai level ini: Indonesia, Taiwan, dst.")]
    [SerializeField] private WasteRuleSet rules;

    [SerializeField] private float cellSize = 1f;

    [Header("Prefab umum (boleh dikosongkan saat greybox)")]
    [Tooltip("Pivot prefab sebaiknya di dasar objek (bagian bawah menyentuh lantai).")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject binPrefab;
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private GameObject playerPrefab;

    public GridModel Model { get; private set; }
    public WasteRuleSet Rules => rules;
    public GameObject PlayerObject { get; private set; }
    public Dictionary<Vector2Int, GameObject> TrashObjects { get; } =
        new Dictionary<Vector2Int, GameObject>();

    private Transform boardRoot;

    // Awake, bukan Start: papan harus berdiri sebelum script lain mencarinya.
    private void Awake()
    {
        Build();
    }

    public void Build()
    {
        if (level == null)
        {
            Debug.LogError("LevelLoader: LevelData belum diisi di Inspector.");
            return;
        }

        if (rules == null)
            Debug.LogWarning("LevelLoader: Aturan Wilayah belum diisi. " +
                             "Pengecekan benar/salah tidak akan bekerja.");

        if (boardRoot != null) Destroy(boardRoot.gameObject);
        TrashObjects.Clear();

        boardRoot = new GameObject("Board").transform;
        boardRoot.SetParent(transform, false);

        // 1) Otak dibuat dulu...
        Model = new GridModel(level);

        // 2) ...lalu tubuh dibangun berdasarkan isi otak.
        for (int x = 0; x < Model.Width; x++)
        {
            for (int y = 0; y < Model.Height; y++)
            {
                var p = new Vector2Int(x, y);
                Cell cell = Model.GetCell(p);

                switch (cell.Type)
                {
                    case TileType.Floor:
                        SpawnFloor(p);
                        break;

                    case TileType.Wall:
                        Spawn(wallPrefab, PrimitiveType.Cube, p,
                              0.5f, Vector3.one, Color.gray, "Wall");
                        break;

                    case TileType.Bin:
                        SpawnFloor(p);
                        Spawn(binPrefab, PrimitiveType.Cube, p,
                              0.4f, Vector3.one * 0.8f, cell.BinCategory.color,
                              $"Tong {cell.BinCategory.displayName}");
                        break;

                        // TileType.Void: sengaja tidak dibuat apa-apa.
                }
            }
        }

        foreach (var pair in Model.Trash)
        {
            WasteItemData item = pair.Value;

            // Prefab milik jenis sampah itu lebih diutamakan daripada prefab umum.
            GameObject prefab = item.prefab != null ? item.prefab : trashPrefab;

            TrashObjects[pair.Key] = Spawn(prefab, PrimitiveType.Sphere, pair.Key,
                                           0.2f, Vector3.one * 0.4f, item.fallbackColor,
                                           item.displayName);
        }

        PlayerObject = Spawn(playerPrefab, PrimitiveType.Capsule, Model.PlayerStart,
                             0.5f, Vector3.one * 0.5f, new Color(0.3f, 0.5f, 0.9f), "Player");
    }

    // Mengubah koordinat grid (x, y) menjadi posisi dunia 3D (x, 0, z).
    public Vector3 GridToWorld(Vector2Int p)
    {
        var center = new Vector3((Model.Width - 1) * 0.5f, 0f, (Model.Height - 1) * 0.5f);
        return transform.position + (new Vector3(p.x, 0f, p.y) - center) * cellSize;
    }

    // ---------------- Helper pembuat objek ----------------

    private void SpawnFloor(Vector2Int p)
    {
        Spawn(floorPrefab, PrimitiveType.Cube, p,
              -0.05f, new Vector3(0.95f, 0.1f, 0.95f), new Color(0.85f, 0.85f, 0.8f), "Floor");
    }

    private GameObject Spawn(GameObject prefab, PrimitiveType fallback, Vector2Int p,
                             float greyboxHeight, Vector3 greyboxScale, Color greyboxColor,
                             string label)
    {
        GameObject go;
        Vector3 pos = GridToWorld(p);

        if (prefab != null)
        {
            go = Instantiate(prefab, boardRoot);
        }
        else
        {
            go = GameObject.CreatePrimitive(fallback);
            go.transform.SetParent(boardRoot, false);
            go.transform.localScale = greyboxScale * cellSize;
            go.GetComponent<Renderer>().material.color = greyboxColor;
            Destroy(go.GetComponent<Collider>()); // kita tidak memakai fisika
            pos += Vector3.up * greyboxHeight * cellSize;
        }

        go.transform.position = pos;
        go.name = $"{label} ({p.x},{p.y})";
        return go;
    }
}