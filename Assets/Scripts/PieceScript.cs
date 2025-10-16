using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceScript : MonoBehaviour, ISelectEventListener, IDragEventListener
{
    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;

    public BoardScript bs;

    public DraggableObject dob;
    public SelectableObject seb;

    public int x;
    public int y;

    public uint piece;

    public bool isSelected;

    public void Start()
    {
        dob = GetComponent<DraggableObject>();
        dob.canDrag = true;
    }

    public void ForceDeselect()
    {
        seb.ForceDeselect();
    }

    public void Setup(uint piece, int x, int y)
    {
        this.piece = piece;

        text.text = Piece.GetPieceType(piece).ToString();

        if (Piece.GetPieceModifier(piece) != 0)
        {
            text.text = text.text + "\n" + Piece.GetPieceModifier(piece);
        }

        if (Piece.GetPieceStatusEffect(piece) != 0)
        {
            text.text = text.text + "\n" + Piece.GetPieceStatusEffect(piece) + " " + (Piece.GetPieceStatusDuration(piece));
        }

        if (Piece.GetPieceSpecialData(piece) != 0)
        {
            text.text = text.text + "\n(" + Piece.GetPieceSpecialData(piece) + ")";
        }

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        Color color = Piece.GetPieceColor(pa);

        backSprite.color = color;
        text.color = color;

        this.x = x;
        this.y = y;
        transform.position = bs.GetSpritePositionFromCoordinates(x, y, -0.5f);
    }

    public void Setup(string pieceType, Color color)
    {
        text.text = pieceType;
        backSprite.color = color;
        text.color = color;
    }

    public void OnSelect()
    {
        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        backSprite.color = new Color(1 - backSprite.color.r, backSprite.color.g, backSprite.color.b, 1);

        bs.SelectPiece(this);
        isSelected = true;
    }

    public void OnDeselect()
    {
        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        if (bs.selectedPiece == null || bs.selectedPiece == this)
        {
            bs.ResetSelected(false);
        }
        isSelected = false;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void OnDragStart()
    {
    }
    public void OnDragStay()
    {
        (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position);            
    }
    public void OnDragStop()
    {
        //debug: lets you play as black
        //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
        if (!bs.blackIsAI)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, bs.board.blackToMove ? Piece.PieceAlignment.Black : Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
                //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
            }
        }
        else
        {
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
            }
        }
        x = bs.hoverX;
        y = bs.hoverY;
        transform.position = bs.GetSpritePositionFromCoordinates(bs.hoverX, bs.hoverY, -0.5f);
        bs.FixBoardBasedOnPosition();
    }

    public void SetPosition(int nx, int ny)
    {
        x = nx;
        y = ny;
        transform.position = bs.GetSpritePositionFromCoordinates(nx, ny, -0.5f);
    }

    public void ResetColor()
    {
        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        if (bs.selectedPiece == null)
        {
            bs.ResetSelected();
        }
        isSelected = false;
    }
}
