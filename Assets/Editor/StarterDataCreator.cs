#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Alat bantu sekali pakai: membuat data awal (kategori, jenis sampah, aturan, level contoh)
// supaya kamu tidak perlu membuat belasan aset satu per satu lewat menu.
// Aman dijalankan berkali-kali: aset yang sudah ada akan diisi ulang, bukan diduplikat.
// File ini WAJIB berada di folder bernama "Editor".
public static class StarterDataCreator
{
    private const string Root = "Assets/Data";

    [MenuItem("Pilah Sampah/Buat Data Awal")]
    public static void Create()
    {
        EnsureFolder(Root);
        EnsureFolder(Root + "/Kategori");
        EnsureFolder(Root + "/Sampah");
        EnsureFolder(Root + "/Aturan");
        EnsureFolder(Root + "/Level");

        // ---- Kategori (skema Indonesia) ----
        var organik = Category("Organik", new Color(0.33f, 0.68f, 0.38f),
            "Sampah yang bisa membusuk dan dikomposkan.");
        var anorganik = Category("Anorganik", new Color(0.93f, 0.78f, 0.25f),
            "Sampah kering: plastik, kertas, logam, kaca.");
        var b3 = Category("B3", new Color(0.86f, 0.30f, 0.28f),
            "Bahan berbahaya dan beracun, perlu penanganan khusus.");

        // ---- Jenis sampah ----
        var sisaMakanan = Item("Sisa makanan", "Nasi, sayur, dan sisa lauk dari piring.");
        var botolPlastik = Item("Botol plastik", "Botol minuman plastik bening.");
        var baterai = Item("Baterai bekas", "Baterai AA bekas dari remot atau mainan.");
        var tisuBekas = Item("Tisu bekas", "Tisu yang sudah dipakai mengelap.");
        var kotakSusu = Item("Kotak susu", "Karton minuman berlapis plastik dan aluminium.");
        var minyakJelantah = Item("Minyak jelantah", "Minyak goreng bekas dalam botol.");

        // ---- Aturan wilayah ----
        // CATATAN: teks fakta di bawah masih SEMENTARA. Periksa ulang dengan
        // peraturan setempat sebelum dipakai di versi yang dipamerkan.
        var indonesia = Rules("Indonesia",
            new List<WasteCategoryData> { organik, anorganik, b3 },
            new List<WasteRule>
            {
                Rule(sisaMakanan, organik,
                    "Sisa makanan bisa dikomposkan jadi pupuk, bukan dibuang bersama plastik."),
                Rule(botolPlastik, anorganik,
                    "Bilas dulu sebelum dibuang. Botol yang masih berisi sisa minuman sering ditolak pendaur ulang."),
                Rule(baterai, b3,
                    "Mengandung logam berat. Kalau tercampur sampah rumah tangga, logamnya bisa meresap ke tanah."),
                Rule(tisuBekas, anorganik,
                    "Tisu bekas pakai tidak bisa didaur ulang karena seratnya sudah pendek dan terkontaminasi."),
                Rule(kotakSusu, anorganik,
                    "Berlapis kertas, plastik, dan aluminium, jadi hanya bisa didaur ulang lewat program khusus."),
                Rule(minyakJelantah, b3,
                    "Jangan dibuang ke saluran air karena menyumbat dan mencemari. Kumpulkan untuk didaur ulang."),
            });

        // ---- Level contoh ----
        var level = Asset<LevelData>(Root + "/Level/Level_Contoh.asset");
        level.levelName = "Contoh: baterai";
        level.layout =
            "#######\n" +
            "#P..b.#\n" +
            "#.###.#\n" +
            "#.....#\n" +
            "#O.A.B#\n" +
            "#######";
        level.trashLegend = new List<TrashSymbol>
        {
            Sym("s", sisaMakanan), Sym("p", botolPlastik), Sym("b", baterai),
            Sym("t", tisuBekas),   Sym("k", kotakSusu),    Sym("m", minyakJelantah),
        };
        level.binLegend = new List<BinSymbol>
        {
            Bin("O", organik), Bin("A", anorganik), Bin("B", b3),
        };
        EditorUtility.SetDirty(level);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Data awal selesai dibuat di Assets/Data. " +
                  "Isi kolom Level dan Aturan Wilayah di LevelLoader, lalu tekan Play.");
        Selection.activeObject = indonesia;
    }

    // ---------------- Helper ----------------

    private static WasteCategoryData Category(string nama, Color warna, string keterangan)
    {
        var a = Asset<WasteCategoryData>($"{Root}/Kategori/Kategori_{nama}.asset");
        a.displayName = nama;
        a.color = warna;
        a.description = keterangan;
        EditorUtility.SetDirty(a);
        return a;
    }

    private static WasteItemData Item(string nama, string keterangan)
    {
        var a = Asset<WasteItemData>($"{Root}/Sampah/Sampah_{nama.Replace(" ", "")}.asset");
        a.displayName = nama;
        a.description = keterangan;
        EditorUtility.SetDirty(a);
        return a;
    }

    private static WasteRuleSet Rules(string wilayah, List<WasteCategoryData> kategori, List<WasteRule> aturan)
    {
        var a = Asset<WasteRuleSet>($"{Root}/Aturan/Aturan_{wilayah}.asset");
        a.regionName = wilayah;
        a.categories = kategori;
        a.rules = aturan;
        EditorUtility.SetDirty(a);
        return a;
    }

    private static WasteRule Rule(WasteItemData item, WasteCategoryData kategori, string fakta) =>
        new WasteRule { wasteItem = item, wasteCategory = kategori, note = fakta };

    private static TrashSymbol Sym(string simbol, WasteItemData item) =>
        new TrashSymbol { symbol = simbol, item = item };

    private static BinSymbol Bin(string simbol, WasteCategoryData kategori) =>
        new BinSymbol { symbol = simbol, category = kategori };

    private static T Asset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int i = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
    }
}
#endif