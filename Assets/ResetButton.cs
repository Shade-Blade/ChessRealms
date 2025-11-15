using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public BattleBoardScript bs;
    public Board.BoardPreset preset;

    public void OnMouseDown()
    {
        bs.ResetBoard(preset);
    }
}
