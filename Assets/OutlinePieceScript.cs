using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece;

public class OutlinePieceScript : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;
    
    public BoardScript bs;

    public uint piece;
    public int x;
    public int y;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Setup(uint piece, int x, int y, Color? color, float offset)
    {
        this.piece = piece;

        //should not be possible
        if (piece == 0)
        {
            Destroy(gameObject);
            return;
        }

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        text.enabled = true;
        backSprite.enabled = true;

        backSprite.sprite = Text_PieceSprite.GetPieceSprite(Piece.GetPieceType(piece));

        if (MainManager.Instance.pieceTextVisible)
        {
            text.text = Piece.GetPieceName(Piece.GetPieceType(piece)); //Piece.GetPieceType(piece).ToString();
        }
        else
        {
            text.text = ""; //Piece.GetPieceType(piece).ToString();
        }

        if (!color.HasValue)
        {
            switch (pa)
            {
                case PieceAlignment.White:
                    color = new Color(0.25f, 0.9f, 0.9f, 0.5f);
                    break;
                case PieceAlignment.Black:
                    color = new Color(0.75f, 0.1f, 0.1f, 0.5f);
                    break;
                case PieceAlignment.Neutral:
                    color = new Color(1, 1, 0, 0.5f);
                    break;
                case PieceAlignment.Crystal:
                    color = new Color(1,0,1, 0.5f);
                    break;
            }
        }

        backSprite.color = color.GetValueOrDefault();
        text.color = color.GetValueOrDefault();

        if (pa == PieceAlignment.Black)
        {
            text.color = new Color(0, 0, 0, 0.5f);
        }

        this.x = x;
        this.y = y;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f + offset);
    }

    public void SetColor(Color c)
    {
        backSprite.color = c;
    }
}
