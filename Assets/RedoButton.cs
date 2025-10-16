using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedoButton : MonoBehaviour
{
    public BoardScript bs;
    public void OnMouseDown()
    {
        bs.Redo();
    }
}
