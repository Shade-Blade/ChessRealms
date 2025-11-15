using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedoButton : MonoBehaviour
{
    public BattleBoardScript bs;
    public void OnMouseDown()
    {
        bs.Redo();
    }
}
