// Jenis "tanah" di setiap kotak.
// Ini bagian PAPAN: tidak pernah berubah selama level berjalan.
public enum TileType
{
    Void,   // di luar papan / lubang: tidak bisa diinjak
    Floor,  // lantai: bisa dilewati
    Wall,   // tembok: penghalang
    Bin     // tong sampah: penghalang yang punya kategori
}

// Kategori sampah. SEMENTARA — nanti disesuaikan dengan aturan Taiwan & Indonesia.
public enum WasteCategory
{
    None,
    Organik,
    Anorganik,
    B3
}

// Satu kotak di papan.
public struct Cell
{
    public TileType Type;
    public WasteCategory BinCategory; // hanya dipakai kalau Type == Bin

    public Cell(TileType type, WasteCategory binCategory = WasteCategory.None)
    {
        Type = type;
        BinCategory = binCategory;
    }
}