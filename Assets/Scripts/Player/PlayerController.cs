using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LevelLoader loader;
    [SerializeField] private SwipeInput swipeInput;

    [Header("Movement")]
    [Tooltip("Kotak yang dilewati per detik")]
    [SerializeField] private float moveSpeed = 1f;


    [Header("Aturan")]
    [Tooltip("Kalau dicentang, player menuju tong sampah yang benar")]
    [SerializeField] private bool stopOnPickup = false;

    public event Action OnBeforeAction;
    public event Action OnActionResolved;
    public event Action OnWrongBin;


    public Vector2Int Position { get; private set; }
    public WasteCategory HeldTrash { get; private set; } = WasteCategory.None;
    public int RemainingTrash => Model.Trash.Count;
    public bool InputLocked { get; set; }

    private bool isMoving;
    private Transform player;
    private GridModel Model => loader.Model;

    // Start is called before the first frame update
    void Start()
    {
        if(loader == null)
        {
            FindFirstObjectByType<LevelLoader>();
        }

        if(swipeInput == null)
        {
            FindFirstObjectByType<SwipeInput>();
        }

        player = loader.PlayerObject.transform;
        Position = Model.PlayerStart;

        swipeInput.OnDirection += HandleDirection;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Destroy event saat tidak digunakan lagi
    private void OnDestroy()
    {
        if(swipeInput != null)
        {
            swipeInput.OnDirection -= HandleDirection;
        }
    }

    private void HandleDirection(Vector2Int dir)
    {
        Debug.Log($"Arah diterima: {dir}");   // SEMENTARA

        if (isMoving || InputLocked) return;
        
        var path = new List<Vector2Int>();
        Vector2Int currentPos = Position;
        Vector2Int next = currentPos + dir;

        while (Model.IsWalkable(next))
        {
            
            currentPos = next;
            path.Add(currentPos);

            //Berhenti kalau memungut sampah
            if(stopOnPickup && HeldTrash == WasteCategory.None && Model.HasTrash(currentPos))
            {
                break;
            }
            next = currentPos + dir;
        }
        // Kotak yang MENGHENTIKAN luncuran. Bisa tembok, tong, atau luar papan.
        Vector2Int blocker = currentPos + dir;
        bool willDispose = Model.GetCell(blocker).Type == TileType.Bin
                           && HeldTrash != WasteCategory.None;

        if (path.Count == 0 && !willDispose) return;

        OnBeforeAction?.Invoke();

        if (path.Count == 0)
        {
            CheckBin(blocker);
            OnActionResolved?.Invoke();
            return;
        }

        StartCoroutine(MoveRoutine(path, blocker));
    }

    private IEnumerator MoveRoutine(List<Vector2Int> path, Vector2Int blocker)
    {
        isMoving = true;
        float durationPerCell = 1f / moveSpeed;

        foreach (Vector2Int cell in path)
        {
            Vector3 from = player.position;
            Vector3 to = loader.GridToWorld(cell);
            to.y = from.y;

            float t = 0f;
            while(t < 1f)
            {
                t += Time.deltaTime / durationPerCell;
                player.position = Vector3.Lerp(from, to, Mathf.Min(t, 1f));
                yield return null; // tunggu satu frame
            }
            Position = cell;
            TryPickup(cell);
        }
        isMoving = false;
        CheckBin(blocker);
    }

    private void TryPickup(Vector2Int cell)
    {
        if(HeldTrash != WasteCategory.None) return;
        if(!Model.HasTrash(cell)) return;

        HeldTrash = Model.GetTrash(cell);
        Model.RemoveTrash(cell);

        if(loader.TrashObjects.TryGetValue(cell, out GameObject go))
        {
            Destroy(go);
            loader.TrashObjects.Remove(cell);
        }

        Destroy(go);
        Debug.Log($"Mengambil Sampah: {HeldTrash}");
    }

    //SEMENTARA: Hasil di cetak di Konsol
    //Nanti bisa dikasih efek visual, skor, dan kartu fakta edukasi
    private void CheckBin(Vector2Int binPos)
    {
        Cell cell = Model.GetCell(binPos);
        if(cell.Type != TileType.Bin) return;

        if(HeldTrash == WasteCategory.None)
        {
            Debug.Log("Tidak ada sampah yang dipegang");
            return;
        }
        if(cell.BinCategory == HeldTrash)
        {
            Debug.Log($"Sampah {HeldTrash} dibuang ke tong yang benar!");
            HeldTrash = WasteCategory.None;
        }
        else
        {
            Debug.Log($"Sampah {HeldTrash} dibuang ke tong yang salah! Seharusnya {cell.BinCategory}");
        }
    }

    public GameSnapshot Capture()
    {
        return new GameSnapshot
        {
            playerPos = Position,
            Held = HeldTrash,
            Trash = Model.CopyTrash()   // fotokopi, bukan alamat
        };
    }

    public void ApplySnapshots(GameSnapshot snap)
    {
        StopAllCoroutines();   // batalkan animasi yang mungkin sedang berjalan
        isMoving = false;

        Position = snap.playerPos;
        HeldTrash = snap.Held;
        Model.RestoreTrash(snap.Trash);

        // Samakan tubuh dengan otak: sampah yang ada di daftar ditampilkan, sisanya disembunyikan.
        foreach (var pair in loader.TrashObjects)
            pair.Value.SetActive(Model.HasTrash(pair.Key));

        Vector3 pos = loader.GridToWorld(snap.playerPos);
        pos.y = player.position.y;
        player.position = pos;   // undo berpindah seketika, tanpa animasi
    }


    // SEMENTARA: hapus setelah testing selesai.
    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 400, 20), $"Posisi: {Position}   Membawa: {HeldTrash}");
    }
}
