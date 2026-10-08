using System.Collections.Generic;
using UnityEngine;

// "OTAK" papan.
// Class C# biasa (bukan MonoBehaviour): tidak butuh Scene, tidak punya tampilan.
public class GridModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2Int PlayerStart { get; private set; }

    // PAPAN (statis) dan BIDAK (dinamis) disimpan terpisah.
    private Cell[,] cells;
    private readonly Dictionary<Vector2Int, WasteItemData> trash =
        new Dictionary<Vector2Int, WasteItemData>();

    public IReadOnlyDictionary<Vector2Int, WasteItemData> Trash => trash;

    // Sekarang menerima LevelData, bukan string, karena legenda simbol ada di sana.
    public GridModel(LevelData level)
    {
        Parse(level);
    }

    // ---------------- Membaca teks layout ----------------

    private void Parse(LevelData level)
    {
        // Rapikan akhir baris gaya Windows (\r\n),
        // lalu buang baris kosong HANYA di awal/akhir teks.
        string[] rows = level.layout.Replace("\r", "").Trim('\n').Split('\n');

        Height = rows.Length;
        Width = 0;
        foreach (string row in rows)
            Width = Mathf.Max(Width, row.Length);

        cells = new Cell[Width, Height];
        int playerCount = 0;

        for (int r = 0; r < Height; r++)
        {
            // Teks dibaca dari ATAS ke bawah, tapi di grid kita y = 0 ada di BAWAH.
            int y = Height - 1 - r;

            for (int x = 0; x < Width; x++)
            {
                char c = x < rows[r].Length ? rows[r][x] : ' ';
                var pos = new Vector2Int(x, y);

                switch (c)
                {
                    case ' ': cells[x, y] = new Cell(TileType.Void); break;
                    case '.': cells[x, y] = new Cell(TileType.Floor); break;
                    case '#': cells[x, y] = new Cell(TileType.Wall); break;

                    case 'P':
                        cells[x, y] = new Cell(TileType.Floor); // pemain berdiri di atas lantai
                        PlayerStart = pos;
                        playerCount++;
                        break;

                    default:
                        // Tong dicek lebih dulu, lalu sampah.
                        WasteCategoryData bin = level.FindBin(c);
                        if (bin != null)
                        {
                            cells[x, y] = new Cell(TileType.Bin, bin);
                            break;
                        }

                        WasteItemData item = level.FindTrash(c);
                        if (item != null)
                        {
                            cells[x, y] = new Cell(TileType.Floor); // sampah berdiri di atas lantai
                            trash[pos] = item;
                            break;
                        }

                        Debug.LogWarning($"Simbol '{c}' di {pos} tidak ada di legenda level " +
                                         $"'{level.name}'. Kotaknya dianggap lantai.");
                        cells[x, y] = new Cell(TileType.Floor);
                        break;
                }
            }
        }

        if (playerCount != 1)
            Debug.LogError($"Level harus punya tepat 1 pemain 'P', ditemukan {playerCount}.");
    }

    // ---------------- Pertanyaan tentang papan ----------------

    public bool IsInside(Vector2Int p) =>
        p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;

    // Di luar papan selalu dianggap Void, jadi pemanggil tidak perlu cek batas sendiri.
    public Cell GetCell(Vector2Int p) =>
        IsInside(p) ? cells[p.x, p.y] : new Cell(TileType.Void);

    public bool IsWalkable(Vector2Int p) => GetCell(p).Type == TileType.Floor;

    public bool HasTrash(Vector2Int p) => trash.ContainsKey(p);

    // ---------------- Daftar sampah ----------------

    public WasteItemData GetTrash(Vector2Int p) =>
        trash.TryGetValue(p, out WasteItemData item) ? item : null;

    public void RemoveTrash(Vector2Int p) => trash.Remove(p);

    // Fotokopi daftar sampah, bukan sekadar alamatnya.
    public Dictionary<Vector2Int, WasteItemData> CopyTrash() =>
        new Dictionary<Vector2Int, WasteItemData>(trash);

    public void RestoreTrash(Dictionary<Vector2Int, WasteItemData> snapshot)
    {
        trash.Clear();
        foreach (var pair in snapshot) trash[pair.Key] = pair.Value;
    }
}