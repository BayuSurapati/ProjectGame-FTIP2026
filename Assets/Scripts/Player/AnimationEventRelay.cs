using System;
using UnityEngine;

// Penerus Animation Event.
//
// Unity memanggil fungsi Animation Event hanya pada komponen yang menempel di
// objek YANG SAMA dengan Animator. Skrip ini duduk di sana, menerima panggilan itu,
// lalu menyiarkannya supaya komponen lain (yang biasanya ada di akar prefab)
// bisa mendengarkan.
//
// Tempelkan ke objek yang punya komponen Animator, bukan ke akar prefab.
//
// Di klip animasi, buat Animation Event dengan:
//   Function : AnimEvent
//   String   : nama penandanya, misalnya release
public class AnimationEventRelay : MonoBehaviour
{
    [Tooltip("Untuk memeriksa: menulis ke Console setiap kali ada event masuk, " +
             "beserta nama penandanya. Matikan lagi setelah selesai.")]
    [SerializeField] private bool logEvents = false;

    public event Action<string> OnAnimationEvent;

    // Nama fungsi ini harus sama persis dengan yang kamu tulis di kolom Function.
    public void AnimEvent(string id)
    {
        if (logEvents)
            Debug.Log($"AnimationEventRelay: event masuk dengan String '{id}' " +
                      $"pada frame {Time.frameCount}.");

        OnAnimationEvent?.Invoke(id);
    }
}