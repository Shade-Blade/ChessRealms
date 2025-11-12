using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParticleScript : MonoBehaviour
{
    public SpriteRenderer sr;

    public void Setup(Piece.PieceAlignment pa)
    {
        switch (pa)
        {
            case Piece.PieceAlignment.White:
                sr.color = new Color(1, 1, 1, 0.7f);
                break;
            case Piece.PieceAlignment.Black:
                sr.color = new Color(0, 0, 0, 0.7f);
                break;
            case Piece.PieceAlignment.Neutral:
                sr.color = new Color(1, 1, 0, 0.7f);
                break;
            case Piece.PieceAlignment.Crystal:
                sr.color = new Color(0, 1, 1, 0.7f);
                break;
        }
    } 
}
