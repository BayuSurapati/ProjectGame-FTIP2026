using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Membaca arah gerakan pemain
/// Hanya membaca input dari dari pemain saja.
/// </summary>
public class SwipeInput : MonoBehaviour
{
    public event System.Action<Vector2Int> OnDirection;

    [Header("Swipe")]
    [Tooltip("Jarak minimal swipe, dalam persen tinggi layar. 0.04 = 4% tinggi layar.")]
    [SerializeField, Range(0.01f, 0.2f)] private float swipeThreshold = 0.04f;

    [Header("Keyboard")]
    [SerializeField] private bool useKeyboard = true;

    private Vector2 pressStart;
    private bool isDragging;
    private bool swipeFired; // Satu tarikan hanya menghasilkan satu arah
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ReadPointer();
        if (useKeyboard) ReadKeyboard();
    }

    private void ReadPointer()
    {
        Pointer p = Pointer.current;
        if (p == null) return;

        if (p.press.wasPressedThisFrame)
        {
            pressStart = p.position.ReadValue();
            isDragging = true;
            swipeFired = false;
        }
        else if (isDragging && p.press.isPressed && !swipeFired)
        {
            Vector2 delta = p.position.ReadValue() - pressStart;

            // Ambang diukur relatif terhadap tinggi layar, supaya terasa sama
            // di layar HP kecil maupun monitor besar.
            if (delta.magnitude >= Screen.height * swipeThreshold)
            {
                swipeFired = true;   // arah sudah dikirim, tunggu jari dilepas
                OnDirection?.Invoke(ToGridDirection(delta));
            }
        }
        else if (p.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void ReadKeyboard()
    {
        Keyboard k = Keyboard.current;
        if (k == null) { Debug.LogError("Keyboard.current NULL"); return; }   // SEMENTARA

        if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame)
            OnDirection?.Invoke(Vector2Int.up);
        else if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame)
            OnDirection?.Invoke(Vector2Int.down);
        else if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame)
            OnDirection?.Invoke(Vector2Int.left);
        else if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame)
            OnDirection?.Invoke(Vector2Int.right);


    }

    //Swipe miring dibulatkan ke sumbu yang paling dominan
    //Tidak ada gerakan diagonal, hanya horizontal atau vertikal
    private static Vector2Int ToGridDirection(Vector2 delta)
    {
        if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }

        return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
    }
}
