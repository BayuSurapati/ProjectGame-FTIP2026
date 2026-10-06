using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class GameSnapshot : MonoBehaviour
{
    public Vector2Int playerPos;
    public WasteCategory Held;

    //Berupa salinan
    public Dictionary<Vector2Int, WasteCategory> Trash;
}
