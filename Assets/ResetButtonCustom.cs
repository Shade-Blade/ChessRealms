using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButtonCustom : MonoBehaviour
{
    public BoardScript bs;
    public Piece.PieceType[] army;
    public Board.EnemyModifier em;

    public void OnMouseDown()
    {
        bs.ResetBoard(army, em);
    }
}
