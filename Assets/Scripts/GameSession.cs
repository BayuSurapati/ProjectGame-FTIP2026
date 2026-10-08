using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Pencatat jalannya satu sesi permainan: riwayat langkah, undo, reset,
// jumlah langkah & kesalahan, kondisi menang, dan kartu fakta edukasi.
[DefaultExecutionOrder(100)]
public class GameSession : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool showDebugUI = true;

    public int Moves { get; private set; }
    public int Mistakes { get; private set; }
    public bool IsWon { get; private set; }

    // Stack = tumpukan piring: yang terakhir ditaruh, itu yang pertama diambil.
    private readonly Stack<GameSnapshot> history = new Stack<GameSnapshot>();
    private GameSnapshot initialState;
    private string lastMessage = "";

    private void Start()
    {
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("GameSession: PlayerController tidak ditemukan.");
            enabled = false;
            return;
        }

        initialState = playerController.Capture();

        playerController.OnBeforeAction += RecordStep;
        playerController.OnBinResult += HandleBinResult;
        playerController.OnActionResolved += CheckWin;
    }

    private void OnDestroy()
    {
        if (playerController == null) return;
        playerController.OnBeforeAction -= RecordStep;
        playerController.OnBinResult -= HandleBinResult;
        playerController.OnActionResolved -= CheckWin;
    }

    // ---------------- Riwayat ----------------

    private void RecordStep()
    {
        history.Push(playerController.Capture());
        Moves++;
    }

    public void Undo()
    {
        if (IsWon || history.Count == 0) return;

        playerController.ApplySnapshot(history.Pop());
        Moves = Mathf.Max(0, Moves - 1);
        lastMessage = "";

        // Mistakes SENGAJA tidak dikurangi.
        // Undo memperbaiki posisi, tapi tidak menghapus fakta bahwa pemain salah memilah.
    }

    public void ResetLevel()
    {
        history.Clear();
        playerController.ApplySnapshot(initialState);

        Moves = 0;
        Mistakes = 0;
        IsWon = false;
        lastMessage = "";
        playerController.InputLocked = false;
    }

    // ---------------- Hasil membuang ----------------

    private void HandleBinResult(bool correct, WasteItemData item, string note)
    {
        if (!correct) Mistakes++;

        string head = correct ? "BENAR" : "SALAH";
        lastMessage = $"{head} — {item.displayName}. {note}";
        Debug.Log(lastMessage);
    }

    // ---------------- Menang ----------------

    private void CheckWin()
    {
        if (IsWon) return;
        if (playerController.HeldTrash != null) return;       // masih memegang sampah
        if (playerController.RemainingTrash > 0) return;      // masih ada sampah di papan

        IsWon = true;
        playerController.InputLocked = true;
        Debug.Log($"MENANG! Langkah: {Moves}, Kesalahan: {Mistakes}, Bintang: {Stars}");
    }

    // SEMENTARA: rumus bintang masih kasar, nanti disesuaikan per level.
    public int Stars => Mistakes == 0 ? 3 : (Mistakes == 1 ? 2 : 1);

    // ---------------- Kontrol & tampilan sementara ----------------

    private void Update()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return;

        if (k.zKey.wasPressedThisFrame) Undo();
        if (k.rKey.wasPressedThisFrame) ResetLevel();
    }

    // SEMENTARA: diganti UI asli nanti.
    private void OnGUI()
    {
        if (!showDebugUI) return;

        string held = playerController.HeldTrash != null
            ? playerController.HeldTrash.displayName
            : "-";

        GUI.Label(new Rect(10, 10, 600, 20),
            $"Langkah: {Moves}   Kesalahan: {Mistakes}   Membawa: {held}   " +
            $"Sisa sampah: {playerController.RemainingTrash}");

        if (IsWon)
            GUI.Label(new Rect(10, 32, 600, 20), $"MENANG — bintang {Stars}");

        if (GUI.Button(new Rect(10, 58, 110, 30), "Undo (Z)")) Undo();
        if (GUI.Button(new Rect(130, 58, 110, 30), "Reset (R)")) ResetLevel();

        if (!string.IsNullOrEmpty(lastMessage))
            GUI.Label(new Rect(10, 96, 420, 120), lastMessage);
    }
}