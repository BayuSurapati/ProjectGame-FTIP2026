using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class LevelLoader : MonoBehaviour
{

#if UNITY_EDITOR
    [Header("Preview in Scene")]
    [Tooltip("Drawing in Scene")]
    [SerializeField] private bool drawPreview = true;

    [Tooltip("Level Name and Size")]
    [SerializeField] private bool showPreviewLabel = true;

    private GridModel previewModel;
    private string previewSignature;

    private void OnDrawGizmos()
    {
        if (!drawPreview || level == null) return;

        // Saat Play, gambar papan yang sebenarnya.
        // Di edit mode, bangun model sementara hanya untuk digambar.
        GridModel m = Model;
        if (m == null)
        {
            string sig = Signature();
            if (previewModel == null || previewSignature != sig)
            {
                previewSignature = sig;
                previewModel = new GridModel(level);
            }
            m = previewModel;
        }

        if (m.Width == 0 || m.Height == 0) return;

        float s = cellSize;

        for (int x = 0; x < m.Width; x++)
        {
            for (int y = 0; y < m.Height; y++)
            {
                var p = new Vector2Int(x, y);
                Vector3 c = CellToWorld(p, m);
                Cell cell = m.GetCell(p);

                switch (cell.Type)
                {
                    case TileType.Floor:
                        // Kotak datar: lantai yang bisa dilalui.
                        Gizmos.color = new Color(0.85f, 0.85f, 0.78f, 0.9f);
                        Gizmos.DrawWireCube(c, new Vector3(s * 0.94f, 0f, s * 0.94f));
                        break;

                    case TileType.Wall:
                        // Kubus abu tembus pandang: tembok, setinggi satu kotak.
                        Gizmos.color = new Color(0.45f, 0.45f, 0.45f, 0.45f);
                        Gizmos.DrawCube(c + Vector3.up * s * 0.5f, Vector3.one * s * 0.96f);
                        break;

                    case TileType.Bin:
                        // Kubus berwarna kategori tongnya.
                        Gizmos.color = cell.BinCategory != null
                            ? cell.BinCategory.color
                            : Color.magenta;   // magenta = kategori belum diisi
                        Gizmos.DrawCube(c + Vector3.up * s * 0.4f, Vector3.one * s * 0.78f);
                        break;

                        // TileType.Void sengaja tidak digambar: itu ruang kosong
                        // yang boleh kamu isi dekorasi.
                }

                if (m.HasTrash(p))
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(c + Vector3.up * s * 0.2f, s * 0.17f);
                }
            }
        }

        // Bola biru: posisi awal pemain. Jangan dihalangi dekorasi tinggi.
        Gizmos.color = new Color(0.3f, 0.55f, 0.95f);
        Gizmos.DrawWireSphere(CellToWorld(m.PlayerStart, m) + Vector3.up * s * 0.5f, s * 0.3f);

        // Kotak jingga: batas luar papan. Inilah patokan menempatkan dekorasi.
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(m.Width * s, 0f, m.Height * s));

        if (showPreviewLabel)
        {
            Vector3 labelPos = transform.position +
                new Vector3(-m.Width * s * 0.5f, 0f, m.Height * s * 0.5f + s * 0.5f);
            Handles.Label(labelPos, $"{level.levelName}    {m.Width} x {m.Height}");
        }
    }
    // Pratinjau dibangun ulang hanya kalau layout atau legendanya berubah,
    // bukan setiap kali Scene digambar ulang.
    private string Signature()
    {
        var sb = new System.Text.StringBuilder(level.layout);

        foreach (TrashSymbol t in level.trashLegend)
            sb.Append('|').Append(t?.symbol)
              .Append(t?.item == null ? 0 : t.item.GetInstanceID());

        foreach (BinSymbol b in level.binLegend)
            sb.Append('|').Append(b?.symbol)
              .Append(b?.category == null ? 0 : b.category.GetInstanceID());

        return sb.ToString();
    }

#endif
}

