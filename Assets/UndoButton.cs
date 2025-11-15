using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoButton : MonoBehaviour
{
    public BattleBoardScript bs;
    public void OnMouseDown()
    {
        bs.Undo();
    }
}
