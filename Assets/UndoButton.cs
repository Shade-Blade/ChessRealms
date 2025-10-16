using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoButton : MonoBehaviour
{
    public BoardScript bs;
    public void OnMouseDown()
    {
        bs.Undo();
    }
}
