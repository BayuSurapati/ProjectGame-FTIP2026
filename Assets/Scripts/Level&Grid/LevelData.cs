using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;


// Satu huruf kecil di layout = satu jenis sampah.
[Serializable]
public class TrashSymbol
{
    [Tooltip("Satu huruf. Kalau diisi lebih, hanya huruf pertama yang dipakai.")]
    public string symbol = "a";
    public WasteItemData item;
}

// Satu huruf besar di layout = satu tong.
[Serializable]
public class BinSymbol
{
    [Tooltip("Satu huruf. Kalau diisi lebih, hanya huruf pertama yang dipakai.")]
    public string symbol = "A";
    public WasteCategoryData category;

}

// Satu file aset = satu level.
// Buat lewat menu: Assets > Create > Pilah Sampah > Level Data
[CreateAssetMenu(fileName = "Level_", menuName = "Pilah Sampah/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level Baru";

    [Tooltip("# tembok | . lantai | spasi = kosong | P pemain\n" +
             "Huruf BESAR = tong, huruf kecil = sampah:\n" +
             "O/o organik, A/a anorganik, B/b B3")]
    [TextArea(8, 20)]
    public string layout =
        "#######\n" +
        "#P..b.#\n" +
        "#.###.#\n" +
        "#.....#\n" +
        "#O.A.B#\n" +
        "#######";

    [Header("Legenda")]
    public List<TrashSymbol> trashLegend = new List<TrashSymbol>();
    public List<BinSymbol> binLegend = new List<BinSymbol>();

    // Unity tidak bisa menyimpan tipe char di Inspector, jadi simbol ditulis
    // sebagai string lalu diambil huruf pertamanya di sini.

    public WasteItemData FindTrash(char c)
    {
        foreach (TrashSymbol ts in trashLegend)
        {
            if(ts != null && ts.item != null && !string.IsNullOrEmpty(ts.symbol) && ts.symbol[0] == c)
            {
                return ts.item;
            }   
        }
        return null;
    }

    public WasteCategoryData FindBin(char c)
    {
        foreach (BinSymbol bs in binLegend)
        {
            if(bs != null && bs.category != null && !string.IsNullOrEmpty(bs.symbol) && bs.symbol[0] == c)
            {
                return bs.category;
            }
        }
        return null;
    }
}