using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Kategori_", menuName = "Pilah Sampah/Kategori Sampah")]
public class WasteCategoryData : ScriptableObject
{
    public string displayName = "New Waste Category";

    [Tooltip("Penjelasan kategori untuk ensiklopedia ada di menu")]
    [TextArea(2, 5)]
    public string description;

    [Tooltip("Warna tong. Sampah TIDAK ikut diwarnai menurut kategori, " +
             "supaya pemain tidak bisa sekadar mencocokkan warna.")]
    public Color color = new Color(0.6f, 0.6f, 0.6f);
}
