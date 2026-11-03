using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private string isMovingParam, isCarryingParam, disposeParam, rejectParam;

    int isMovingHash, isCarryingHash, disposeHash, rejectHash;

    private readonly HashSet<int> available = new HashSet<int>();
    private bool ready;

    // Start is called before the first frame update
    void Start()
    {
        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if(playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if(animator == null)
        {
            Debug.LogError("Animator is not assigned and could not be found in children.");
            enabled = false;
            return;
        }

        //Parameter diganti menjadi angka
        isMovingHash = Animator.StringToHash(isMovingParam);
        isCarryingHash = Animator.StringToHash(isCarryingParam);
        disposeHash = Animator.StringToHash(disposeParam);
        rejectHash = Animator.StringToHash(rejectParam);

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            available.Add(p.nameHash);
        }

        playerController.OnBinResult += HandleBinResult;
        ready = true;
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnBinResult -= HandleBinResult;
        }
    }

    private void Update()
    {
        if (!ready)
        {
            return;
        }

        //dibaca ulang setiap frame, undo dan reset ikut benar dengan sendirinya
        SetBool(isMovingHash, playerController.IsMoving);
        SetBool(isCarryingHash, playerController.HeldTrash != null);
    }

    private void HandleBinResult(bool correct, WasteItemData item, string note)
    {
        SetTrigger(correct ? disposeHash : rejectHash);
    }

    private void SetBool(int hash, bool value)
    {
        if(available.Contains(hash))
        {
            animator.SetBool(hash, value);
        }
    }

    private void SetTrigger(int hash) 
    {
        if(available.Contains(hash))
        {
            animator.SetTrigger(hash);
        }
    }
}
