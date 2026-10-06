using System.Collections.Generic;
using UnityEngine;

// "TUBUH" papan: membaca LevelData, membuat GridModel,
// lalu membangun tampilan 3D berdasarkan isi GridModel.
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private LevelData level;
    [SerializeField] private float cellSize = 1f;

    [Header("Prefab (boleh dikosongkan saat greybox)")]
    [Tooltip("Pivot prefab sebaiknya di dasar objek (bagian bawah menyentuh lantai).")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject binPrefab;
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private GameObject playerPrefab;

    public GridModel Model { get; private set; }
    public GameObject PlayerObject { get; private set; }
    public Dictionary<Vector2Int, GameObject> TrashObjects { get; } =
        new Dictionary<Vector2Int, GameObject>();

    private Transform boardRoot;

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

        // Hapus papan lama (berguna nanti untuk tombol reset / ganti level).
        if (boardRoot != null) Destroy(boardRoot.gameObject);
        TrashObjects.Clear();

        boardRoot = new GameObject("Board").transform;
        boardRoot.SetParent(transform, false);

        // 1) Otak dibuat dulu...
        Model = new GridModel(level.layout);

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
                              0.4f, Vector3.one * 0.8f, CategoryColor(cell.BinCategory),
                              $"Bin {cell.BinCategory}");
                        break;

                        // TileType.Void: sengaja tidak dibuat apa-apa.
                }
            }
        }

        foreach (var pair in Model.Trash)
        {
            TrashObjects[pair.Key] = Spawn(trashPrefab, PrimitiveType.Sphere, pair.Key,
                                           0.2f, Vector3.one * 0.4f, CategoryColor(pair.Value),
                                           $"Trash {pair.Value}");
        }

        PlayerObject = Spawn(playerPrefab, PrimitiveType.Capsule, Model.PlayerStart,
                             0.5f, Vector3.one * 0.5f, new Color(0.3f, 0.5f, 0.9f), "Player");
    }

    // Mengubah koordinat grid (x, y) menjadi posisi dunia 3D (x, 0, z).
    // Y milik grid menjadi Z milik dunia, karena Y di Unity adalah arah ATAS.
    // Papan dibuat berpusat di posisi objek LevelLoader agar kamera mudah diarahkan.
    public Vector3 GridToWorld(Vector2Int p)
    {
        var center = new Vector3((Model.Width - 1) * 0.5f, 0f, (Model.Height - 1) * 0.5f);
        return transform.position + (new Vector3(p.x, 0f, p.y) - center) * cellSize;
    }

    // ---------------- Helper pembuat objek ----------------

    private void SpawnFloor(Vector2Int p)
    {
        // Sedikit lebih kecil dari 1 agar celah antar-kotak terlihat seperti garis grid.
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
            // Mode greybox: bentuk dasar Unity, cukup untuk menguji mekanik.
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

    // Warna KHUSUS greybox, untuk memudahkan pengembang — BUKAN keputusan visual final.
    private static Color CategoryColor(WasteCategory c)
    {
        switch (c)
        {
            case WasteCategory.Organik: return new Color(0.3f, 0.7f, 0.3f);
            case WasteCategory.Anorganik: return new Color(0.95f, 0.8f, 0.2f);
            case WasteCategory.B3: return new Color(0.85f, 0.25f, 0.25f);
            default: return Color.white;
        }
    }
}