using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static Piece;

public class DifficultyButton : MonoBehaviour
{
    public BattleBoardScript bs;
    public int difficulty;
    public SpriteRenderer backSprite;

    public void OnMouseDown()
    {
        bs.difficulty = difficulty;
    }

    public void Update()
    {
        if (bs.difficulty == difficulty)
        {
            if (bs.chessAI.difficulty == difficulty)
            {
                backSprite.color = new Color(1, 1, 0, 1);
            }
            else
            {
                backSprite.color = new Color(1, 0.5f, 0, 1);
            }
        }
        else
        {
            if (bs.chessAI.difficulty == difficulty)
            {
                backSprite.color = new Color(0.5f, 1, 0, 1);
            }
            else
            {
                backSprite.color = new Color(0.5f, 0.5f, 0, 1);
            }
        }
    }
}
