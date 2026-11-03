using System.Collections;
using UnityEngine;

// "TUBUH" sampah yang sedang dipegang.
//
// Objek ini BUKAN sampah yang ada di papan. Sampah di papan tetap disembunyikan
// oleh PlayerController dan tidak pernah dipindahkan, supaya undo tetap sederhana.
// Yang ditampilkan di sini adalah salinan baru yang menjadi CERMIN dari HeldTrash:
// setiap frame ia menyesuaikan diri dengan apa yang sedang dipegang otak.
// Karena itu undo dan reset ikut benar tanpa penanganan khusus.
//
// Tempelkan ke AKAR prefab pemain, lalu isi kolom Hand Slot.
public class HeldItemView : MonoBehaviour
{
    [Tooltip("Objek kosong yang menjadi anak tulang tangan. Di sinilah sampah menempel.")]
    [SerializeField] private Transform handSlot;

    [Tooltip("Boleh dikosongkan: dicari sendiri di Scene.")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LevelLoader loader;

    [Header("Tampilan di tangan")]
    [Tooltip("Pengali ukuran. Pakai kalau model sampahnya terlalu besar untuk digenggam.")]
    [SerializeField] private float heldScale = 1f;

    [Header("Kapan sampah lepas dari tangan")]
    [Tooltip("Pakai penanda di frame animasi (Animation Event). Lebih tepat, dan tidak " +
             "bergeser kalau Speed animasinya diubah. Butuh AnimationEventRelay di objek Animator.")]
    [SerializeField] private bool useAnimationEvent = true;

    [Tooltip("Isi kolom String pada Animation Event di klip lempar.")]
    [SerializeField] private string releaseEventId = "release";

    [Tooltip("Pengaman. Kalau penandanya tidak pernah datang dalam waktu ini, " +
             "sampah dilepas juga supaya tidak nyangkut di tangan selamanya.")]
    [SerializeField] private float releaseTimeout = 1.5f;

    [Tooltip("Dipakai HANYA kalau penanda frame tidak tersedia: jeda sejak animasi " +
             "lempar mulai sampai sampah lepas dari tangan.")]
    [SerializeField] private float throwDelay = 0.2f;

    [Header("Terbang ke tong")]

    [Tooltip("Lama sampah terbang menuju tong.")]
    [SerializeField] private float throwDuration = 0.35f;

    [Tooltip("Tinggi puncak lengkungan lemparan.")]
    [SerializeField] private float arcHeight = 1.1f;

    [Tooltip("Tinggi titik jatuh di atas lantai tong, kira-kira setinggi mulut tong.")]
    [SerializeField] private float binMouthHeight = 0.8f;

    [Tooltip("Putaran sampah saat terbang, derajat per detik. 0 = tidak berputar.")]
    [SerializeField] private float spinSpeed = 320f;

    private WasteItemData shown;      // yang sedang ditampilkan di tangan
    private GameObject inHand;        // objeknya
    private GameObject flying;        // yang sedang terbang ke tong
    private Coroutine throwRoutine;
    private AnimationEventRelay relay;
    private bool releaseSignal;       // penanda frame sudah datang
    private bool ready;

    private void Start()
    {
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (loader == null) loader = FindFirstObjectByType<LevelLoader>();

        if (playerController == null || loader == null)
        {
            Debug.LogError("HeldItemView: PlayerController atau LevelLoader tidak ditemukan.");
            enabled = false;
            return;
        }

        if (handSlot == null) handSlot = FindHumanoidHand();

        if (handSlot == null)
        {
            Debug.LogError("HeldItemView: Hand Slot belum diisi, dan tulang tangan tidak " +
                           "bisa ditemukan otomatis. Buat objek kosong sebagai anak tulang " +
                           "tangan, lalu isikan ke kolom Hand Slot.");
            enabled = false;
            return;
        }

        relay = GetComponentInChildren<AnimationEventRelay>();

        if (useAnimationEvent && relay == null)
            Debug.LogWarning("HeldItemView: AnimationEventRelay tidak ditemukan, jadi lepasnya " +
                             "sampah memakai Throw Delay. Tempelkan AnimationEventRelay ke objek " +
                             "yang punya Animator kalau ingin sinkron per frame.");

        if (relay != null) relay.OnAnimationEvent += HandleAnimationEvent;

        playerController.OnBinResult += HandleBinResult;
        playerController.OnStateRestored += HandleStateRestored;
        ready = true;
    }

    private void OnDestroy()
    {
        if (relay != null) relay.OnAnimationEvent -= HandleAnimationEvent;

        if (playerController == null) return;
        playerController.OnBinResult -= HandleBinResult;
        playerController.OnStateRestored -= HandleStateRestored;
    }

    private void HandleAnimationEvent(string id)
    {
        if (id == releaseEventId) releaseSignal = true;
    }

    // Hanya berhasil untuk rig Humanoid. Untuk rig Generic, Hand Slot wajib diisi manual.
    private Transform FindHumanoidHand()
    {
        Animator a = GetComponentInChildren<Animator>();
        if (a == null || !a.isHuman) return null;
        return a.GetBoneTransform(HumanBodyBones.RightHand);
    }

    // ---------------- Cermin dari HeldTrash ----------------

    private void Update()
    {
        if (!ready) return;

        WasteItemData want = playerController.HeldTrash;
        if (want == shown) return;   // tidak ada yang berubah

        ClearHand();
        shown = want;
        if (want == null) return;

        inHand = BuildVisual(want);
    }

    private GameObject BuildVisual(WasteItemData item)
    {
        GameObject go;

        if (item.prefab != null)
        {
            go = Instantiate(item.prefab, handSlot);
            go.transform.localScale = item.prefab.transform.localScale * heldScale;
        }
        else
        {
            // Mode greybox: bentuk dasar, supaya sudah bisa diuji tanpa model 3D.
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(handSlot, false);
            go.transform.localScale = Vector3.one * 0.3f * heldScale;
            go.GetComponent<Renderer>().material.color = item.fallbackColor;
        }

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.name = "Dipegang: " + item.displayName;

        // Kita tidak memakai fisika sama sekali di game ini.
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) Destroy(c);

        return go;
    }

    private void ClearHand()
    {
        if (inHand != null) Destroy(inHand);
        inHand = null;
    }

    // ---------------- Lemparan ----------------

    private void HandleBinResult(bool correct, WasteItemData item, string note)
    {
        // Salah tong berarti sampah ditolak dan tetap di tangan, jadi tidak ada apa-apa.
        if (!correct || inHand == null) return;

        // Objek diserahkan ke coroutine. Dilepas dari pencatatan tangan supaya
        // Update tidak ikut menghancurkannya saat melihat HeldTrash sudah kosong.
        GameObject thrown = inHand;
        inHand = null;
        shown = null;

        CancelThrow();
        throwRoutine = StartCoroutine(ThrowRoutine(thrown, BinWorldPosition()));
    }

    // Tong yang baru ditabrak selalu berada tepat di depan karakter: posisi
    // sekarang ditambah arah hadap. Itu karena arah hadap selalu disetel ke arah
    // luncuran, dan tong itulah yang menghentikan luncurannya.
    private Vector3 BinWorldPosition()
    {
        Vector2Int binCell = playerController.Position + playerController.Facing;
        return loader.GridToWorld(binCell) + Vector3.up * binMouthHeight;
    }

    private IEnumerator ThrowRoutine(GameObject thrown, Vector3 target)
    {
        flying = thrown;
        releaseSignal = false;

        // Selama menunggu, sampah masih di tangan dan ikut animasi ancang-ancang.
        if (useAnimationEvent && relay != null)
        {
            // Menunggu penanda di frame, tapi tidak selamanya. Kalau penandanya
            // belum dipasang di klip, pengaman ini mencegah sampah nyangkut.
            float waited = 0f;
            while (!releaseSignal && waited < releaseTimeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (!releaseSignal)
                Debug.LogWarning($"HeldItemView: penanda '{releaseEventId}' tidak datang dalam " +
                                 $"{releaseTimeout} detik. Cek Animation Event di klip lemparmu: " +
                                 "Function harus AnimEvent dan String harus sama dengan Release Event Id.");
        }
        else if (throwDelay > 0f)
        {
            yield return new WaitForSeconds(throwDelay);
        }

        if (flying == null) yield break;

        // true: posisi dunia dipertahankan saat dilepas dari tangan.
        flying.transform.SetParent(null, true);
        Vector3 from = flying.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, throwDuration);
            float k = Mathf.Min(t, 1f);

            Vector3 p = Vector3.Lerp(from, target, k);
            p.y += arcHeight * Mathf.Sin(k * Mathf.PI);   // lengkungan sederhana
            flying.transform.position = p;

            if (spinSpeed != 0f)
                flying.transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.Self);

            yield return null;
        }

        Destroy(flying);
        flying = null;
        throwRoutine = null;
    }

    // Undo dan reset: batalkan lemparan yang sedang berjalan.
    // Isi tangan tidak perlu diurus di sini, karena Update akan membangunnya ulang
    // dari HeldTrash di frame berikutnya.
    private void HandleStateRestored()
    {
        CancelThrow();
    }

    private void CancelThrow()
    {
        if (throwRoutine != null)
        {
            StopCoroutine(throwRoutine);
            throwRoutine = null;
        }

        if (flying != null)
        {
            Destroy(flying);
            flying = null;
        }
    }
}