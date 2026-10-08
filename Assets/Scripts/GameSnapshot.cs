using System.Collections.Generic;
using UnityEngine;

// "Foto" keadaan game pada satu titik waktu.
// Berisi SEMUA hal yang bisa berubah saat pemain melangkah.
// Setiap kali kamu menambah mekanik baru yang menyimpan sesuatu, tambahkan juga di sini.
public struct GameSnapshot
{
    public Vector2Int PlayerPos;
    public WasteItemData Held;   // null = tangan kosong

    // WAJIB berupa salinan, bukan alamat daftar aslinya.
    public Dictionary<Vector2Int, WasteItemData> Trash;
}