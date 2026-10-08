using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Penghubung antara input, otak (GridModel), dan tubuh (objek 3D).
// Alurnya selalu: HITUNG DULU semuanya, BARU animasikan.
public class PlayerController : MonoBehaviour
{
    [SerializeField] private LevelLoader loader;
    [SerializeField] private SwipeInput input;

    [Header("Gerakan")]
    [Tooltip("Berapa kotak yang dilewati per detik.")]
    [SerializeField] private float speed = 6f;

    [Header("Aturan")]
    [Tooltip("Kalau dicentang, karakter berhenti tepat di kotak sampah yang diambil.")]
    [SerializeField] private bool stopOnPickup = false;

    // Siaran untuk pihak lain (GameSession).
    public event Action OnBeforeAction;     // SEBELUM keadaan berubah -> waktunya memotret
    public event Action OnActionResolved;   // SETELAH langkah tuntas -> waktunya cek menang
    public event Action<bool, WasteItemData, string> OnBinResult;  // benar?, bendanya, fakta

    public Vector2Int Position { get; private set; }
    public WasteItemData HeldTrash { get; private set; }   // null = tangan kosong
    public int RemainingTrash => Model.Trash.Count;
    public bool InputLocked { get; set; }

    private bool isMoving;
    private Transform player;
    private GridModel Model => loader.Model;

    private void Start()
    {
        if (loader == null) loader = FindFirstObjectByType<LevelLoader>();
        if (input == null) input = FindFirstObjectByType<SwipeInput>();

        if (loader == null || input == null)
        {
            Debug.LogError("PlayerController: LevelLoader atau SwipeInput tidak ditemukan.");
            enabled = false;
            return;
        }

        if (loader.Model == null || loader.PlayerObject == null)
        {
            Debug.LogError("PlayerController: papan belum dibangun. Cek LevelLoader pakai Awake() " +
                           "dan kolom Level sudah diisi.");
            enabled = false;
            return;
        }

        player = loader.PlayerObject.transform;
        Position = Model.PlayerStart;
        input.OnDirection += HandleDirection;
    }

    private void OnDestroy()
    {
        if (input != null) input.OnDirection -= HandleDirection;
    }

    // ---------------- Gerakan ----------------

    private void HandleDirection(Vector2Int dir)
    {
        if (isMoving || InputLocked) return;

        // --- TAHAP HITUNG ---
        var path = new List<Vector2Int>();
        Vector2Int current = Position;
        Vector2Int next = current + dir;

        while (Model.IsWalkable(next))
        {
            current = next;
            path.Add(current);

            if (stopOnPickup && HeldTrash == null && Model.HasTrash(current))
                break;

            next = current + dir;
        }

        Vector2Int blocker = current + dir;
        bool willDispose = Model.GetCell(blocker).Type == TileType.Bin && HeldTrash != null;

        // Tidak ada yang berubah sama sekali: jangan catat langkah, jangan buang jatah undo.
        if (path.Count == 0 && !willDispose) return;

        OnBeforeAction?.Invoke();   // potret keadaan SEBELUM apa pun berubah

        if (path.Count == 0)        // menempel tong, membuang tanpa bergerak
        {
            CheckBin(blocker);
            OnActionResolved?.Invoke();
            return;
        }

        // --- TAHAP TAMPILKAN ---
        StartCoroutine(MoveRoutine(path, blocker));
    }

    private IEnumerator MoveRoutine(List<Vector2Int> path, Vector2Int blocker)
    {
        isMoving = true;
        float durationPerCell = 1f / speed;

        try
        {
            foreach (Vector2Int cell in path)
            {
                Vector3 from = player.position;
                Vector3 to = loader.GridToWorld(cell);
                to.y = from.y;

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / durationPerCell;
                    player.position = Vector3.Lerp(from, to, Mathf.Min(t, 1f));
                    yield return null;
                }

                Position = cell;
                TryPickup(cell);
            }
        }
        finally
        {
            isMoving = false;   // dijamin jalan, walau coroutine dihentikan di tengah
        }

        CheckBin(blocker);
        OnActionResolved?.Invoke();
    }

    private void TryPickup(Vector2Int cell)
    {
        if (HeldTrash != null) return;
        if (!Model.HasTrash(cell)) return;

        HeldTrash = Model.GetTrash(cell);
        Model.RemoveTrash(cell);

        // DISEMBUNYIKAN, bukan dihancurkan, supaya bisa dimunculkan lagi saat undo.
        if (loader.TrashObjects.TryGetValue(cell, out GameObject go))
            go.SetActive(false);
    }

    private void CheckBin(Vector2Int binPos)
    {
        Cell cell = Model.GetCell(binPos);
        if (cell.Type != TileType.Bin) return;
        if (HeldTrash == null) return;

        WasteRuleSet rules = loader.Rules;
        if (rules == null)
        {
            Debug.LogWarning("PlayerController: Aturan Wilayah kosong, tidak bisa menilai benar/salah.");
            return;
        }

        // Benar atau salah ditentukan oleh ATURAN WILAYAH, bukan oleh data sampahnya.
        WasteCategoryData shouldBe = rules.CategoryOf(HeldTrash);
        bool correct = shouldBe != null && shouldBe == cell.BinCategory;

        WasteItemData item = HeldTrash;          // disimpan dulu untuk dikirim lewat event
        string note = rules.NoteFor(item);

        if (correct) HeldTrash = null;           // salah = sampah ditolak, tetap di tangan

        OnBinResult?.Invoke(correct, item, note);
    }

    // ---------------- Untuk sistem undo ----------------

    public GameSnapshot Capture()
    {
        return new GameSnapshot
        {
            PlayerPos = Position,
            Held = HeldTrash,
            Trash = Model.CopyTrash()   // fotokopi, bukan alamat
        };
    }

    public void ApplySnapshot(GameSnapshot snap)
    {
        StopAllCoroutines();   // batalkan animasi yang mungkin sedang berjalan
        isMoving = false;

        Position = snap.PlayerPos;
        HeldTrash = snap.Held;
        Model.RestoreTrash(snap.Trash);

        // Samakan tubuh dengan otak.
        foreach (var pair in loader.TrashObjects)
            pair.Value.SetActive(Model.HasTrash(pair.Key));

        Vector3 pos = loader.GridToWorld(snap.PlayerPos);
        pos.y = player.position.y;
        player.position = pos;   // undo berpindah seketika, tanpa animasi
    }
}