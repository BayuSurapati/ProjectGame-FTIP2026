using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sampah_", menuName = "Pilah Sampah/Jenis Sampah")]
public class WasteItemData : ScriptableObject
{
    public string displayName = "Sampah Baru";

    [Tooltip("Keterangan netral tentang bendanya, bukan aturan pemilahannya.")]
    [TextArea(2, 5)]
    public string description;

    [Tooltip("Model 3D. Kalau kosong, dipakai prefab umum atau bola greybox.")]
    public GameObject prefab;

    [Tooltip("Warna greybox. Jangan diwarnai menurut kategori: biarkan pemain berpikir.")]
    public Color fallbackColor = new Color(0.85f, 0.85f, 0.85f);
}
