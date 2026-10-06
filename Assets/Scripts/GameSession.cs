using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameSession : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private bool showDebugUI = true;

    public int Moves { get; private set; }
    public int Mistakes { get; private set; }
    public bool isWon { get; private set; }

    //Stacking untuk menyimpan snapshot permainan sebelumnya
    private readonly Stack<GameSnapshot> history = new Stack<GameSnapshot>();
    private GameSnapshot initialState;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogError("GameSession: PlayerController tidak ditemukan di scene. Pastikan ada GameObject dengan PlayerController.");
            enabled = false;
            return;
        }

        initialState = player.Capture();

        player.OnBeforeAction += RecordStep;
        player.OnWrongBin += CountMistake;
        player.OnActionResolved += CheckWin;
    }

    private void OnDestroy()
    {
        if (player == null) return;
        player.OnBeforeAction -= RecordStep;
        player.OnWrongBin -= CountMistake;
        player.OnActionResolved -= CheckWin;
    }

    //-----------------History-----------------

    private void RecordStep()
    {
        history.Push(player.Capture());
        Moves++;
    }

    public void Undo()
    {
        if (isWon || history.Count == 0) return;

        player.ApplySnapshots(history.Pop());
        Moves = Mathf.Max(0, Moves - 1);
    }

    public void ResetLevel()
    {
        history.Clear();
        player.ApplySnapshots(initialState);

        Moves = 0;
        Mistakes = 0;
        isWon = false;
        player.InputLocked = false;
    }

    private void CountMistake() => Mistakes++;

    //---------------Win-----------------

    private void CheckWin()
    {
        if (isWon) return;
        if(player.HeldTrash != WasteCategory.None) return;
        if(player.RemainingTrash > 0) return;

        isWon = true;
        player.InputLocked = true;
        Debug.Log($"MENANG! Langkah: {Moves}, Kesalahan: {Mistakes}, Bintang: {Stars}");
    }

    public int Stars => Mistakes == 0 ? 3 : (Mistakes == 1 ? 2 : 1);


    // Update is called once per frame
    void Update()
    {
        Keyboard k = Keyboard.current;
        if(k == null)
        {
            return;
        }

        if (k.zKey.wasPressedThisFrame) Undo();
        if (k.rKey.wasPressedThisFrame) ResetLevel();

    }

    private void OnGUI()
    {
        if(!showDebugUI) return;
        
        GUI.Label(new Rect(10,10,500,20), 
            $"Langkah: {Moves}   Kesalahan: {Mistakes}   Membawa: {player.HeldTrash}   " +
            $"Sisa sampah: {player.RemainingTrash}");

        if (isWon)
        {
            GUI.Label(new Rect(10, 32, 500, 20), $"MENANG — bintang {Stars}");
        }

        if (GUI.Button(new Rect(10, 58, 110, 30), "Undo (Z)")) Undo();
        if (GUI.Button(new Rect(130, 58, 110, 30), "Reset (R)")) ResetLevel();
    }
}
