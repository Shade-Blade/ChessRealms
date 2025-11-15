using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButtonCustom : MonoBehaviour
{
    public BattleBoardScript bs;
    public Piece.PieceType[] army;
    public Board.PlayerModifier pm;
    public Board.EnemyModifier em;

    public void OnMouseDown()
    {
        bs.ResetBoard(army, pm, em);
    }
}
