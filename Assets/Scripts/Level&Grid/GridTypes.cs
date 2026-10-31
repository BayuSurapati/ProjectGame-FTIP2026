// Arah di atas grid, sebagai pilihan siap pakai.
// Gunanya supaya Inspector menampilkan daftar pilihan, bukan dua angka
// yang artinya harus ditebak.
using UnityEngine;

public enum GridDirection
{
    Up,      // grid +y, menjauhi kamera
    Down,    // grid -y, menghadap kamera
    Left,    // grid -x
    Right    // grid +x
}

public static class GridDirectionExt
{
    public static Vector2Int ToVector(this GridDirection d)
    {
        switch (d)
        {
            case GridDirection.Up: return Vector2Int.up;
            case GridDirection.Down: return Vector2Int.down;
            case GridDirection.Left: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }
}

// Jenis "tanah" di setiap kotak.
// Ini bagian PAPAN: tidak pernah berubah selama level berjalan.
public enum TileType
{
    Void,   // di luar papan / lubang: tidak bisa diinjak
    Floor,  // lantai: bisa dilewati
    Wall,   // tembok: penghalang
    Bin     // tong sampah: penghalang yang punya kategori
}

// Satu kotak di papan.
// Catatan: enum WasteCategory sudah DIHAPUS. Kategori kini berupa aset
// (WasteCategoryData), supaya aturan tiap wilayah bisa berbeda tanpa mengubah kode.
public struct Cell
{
    public TileType Type;
    public WasteCategoryData BinCategory; // hanya diisi kalau Type == Bin

    public Cell(TileType type, WasteCategoryData binCategory = null)
    {
        Type = type;
        BinCategory = binCategory;
    }
}