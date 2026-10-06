using UnityEngine;

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
        "#P..o.#\n" +
        "#.#...#\n" +
        "#..a..#\n" +
        "#O...A#\n" +
        "#######";
}