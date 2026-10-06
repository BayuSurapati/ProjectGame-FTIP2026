using System.Collections.Generic;
using UnityEngine;

// "OTAK" papan.
// Class C# biasa (bukan MonoBehaviour): tidak butuh Scene, tidak punya tampilan.
// Tugasnya hanya menyimpan isi grid dan menjawab pertanyaan tentang grid.
public class GridModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2Int PlayerStart { get; private set; }

    // PAPAN (statis) dan BIDAK (dinamis) disimpan terpisah.
    private Cell[,] cells;
    private readonly Dictionary<Vector2Int, WasteCategory> trash =
        new Dictionary<Vector2Int, WasteCategory>();

    public IReadOnlyDictionary<Vector2Int, WasteCategory> Trash => trash;

    public GridModel(string layout)
    {
        Parse(layout);
    }

    // ---------------- Membaca teks layout ----------------

    private void Parse(string layout)
    {
        // Rapikan akhir baris gaya Windows (\r\n),
        // lalu buang baris kosong HANYA di awal/akhir teks.
        string[] rows = layout.Replace("\r", "").Trim('\n').Split('\n');

        Height = rows.Length;
        Width = 0;
        foreach (string row in rows)
            Width = Mathf.Max(Width, row.Length);

        cells = new Cell[Width, Height];
        int playerCount = 0;

        for (int r = 0; r < Height; r++)
        {
            // Teks dibaca dari ATAS ke bawah, tapi di grid kita y = 0 ada di BAWAH.
            // Tanpa pembalikan ini, level tampil terbalik atas-bawah.
            int y = Height - 1 - r;

            for (int x = 0; x < Width; x++)
            {
                // Baris yang lebih pendek dianggap diisi spasi (kosong).
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
                        WasteCategory cat = CharToCategory(c);
                        if (cat == WasteCategory.None)
                        {
                            Debug.LogWarning($"Simbol tidak dikenal '{c}' di {pos}. Dianggap lantai.");
                            cells[x, y] = new Cell(TileType.Floor);
                        }
                        else if (char.IsUpper(c))
                        {
                            cells[x, y] = new Cell(TileType.Bin, cat); // huruf BESAR = tong
                        }
                        else
                        {
                            cells[x, y] = new Cell(TileType.Floor);    // sampah berdiri di atas lantai
                            trash[pos] = cat;                          // huruf kecil = sampah
                        }
                        break;
                }
            }
        }

        // Validasi: kesalahan kecil di layout lebih baik ketahuan sekarang.
        if (playerCount != 1)
            Debug.LogError($"Level harus punya tepat 1 pemain 'P', ditemukan {playerCount}.");
    }

    private static WasteCategory CharToCategory(char c)
    {
        switch (char.ToLower(c))
        {
            case 'o': return WasteCategory.Organik;
            case 'a': return WasteCategory.Anorganik;
            case 'b': return WasteCategory.B3;
            default: return WasteCategory.None;
        }
    }

    // ---------------- Pertanyaan tentang papan ----------------

    public bool IsInside(Vector2Int p) =>
        p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;

    // Di luar papan selalu dianggap Void, jadi pemanggil tidak perlu cek batas sendiri.
    public Cell GetCell(Vector2Int p) =>
        IsInside(p) ? cells[p.x, p.y] : new Cell(TileType.Void);

    // Hanya melihat "tanah". Aturan sampah (menghalangi atau diambil)
    // sengaja BELUM dimasukkan — itu keputusan desain untuk langkah meluncur.
    public bool IsWalkable(Vector2Int p) => GetCell(p).Type == TileType.Floor;

    public bool HasTrash(Vector2Int p) => trash.ContainsKey(p);

    public WasteCategory GetTrash(Vector2Int p) =>
        trash.TryGetValue(p, out var cat) ? cat : WasteCategory.None;

    public void RemoveTrash(Vector2Int p) => trash.Remove(p);

    public Dictionary<Vector2Int, WasteCategory> CopyTrash() =>
        new Dictionary<Vector2Int, WasteCategory>(trash);

    public void RestoreTrash(Dictionary<Vector2Int, WasteCategory> snapshot)
    {
        trash.Clear();
        foreach (var pair in snapshot)
            trash[pair.Key] = pair.Value;
    }
}