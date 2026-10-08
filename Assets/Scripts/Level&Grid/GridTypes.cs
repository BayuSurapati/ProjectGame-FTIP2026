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