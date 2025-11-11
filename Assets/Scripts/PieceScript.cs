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

    public bool isGiant;
    public BoxCollider bc;

    public virtual void Start()
    {
        dob = GetComponent<DraggableObject>();
        dob.canDrag = true;
    }

    public virtual void ForceDeselect()
    {
        seb.ForceDeselect();
        dob.isDragged = false;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
    }

    public void Setup(uint piece, int x, int y)
    {
        this.piece = piece;

        isGiant = (GlobalPieceManager.GetPieceTableEntry(piece).piecePropertyB & Piece.PiecePropertyB.Giant) != 0;

        text.enabled = true;
        backSprite.enabled = true;
        bc.enabled = true;
        if (isGiant)
        {
            if (Piece.GetPieceSpecialData(piece) != 0)
            {
                text.enabled = false;
                backSprite.enabled = false;
                bc.enabled = false;
                this.x = x;
                this.y = y;
                return;
            }

            Vector3 offset = (Vector3.up + Vector3.right) * 0.5f;
            text.transform.localPosition = offset;
            backSprite.transform.localPosition = offset;
            text.transform.localScale = Vector3.one * 2;
            backSprite.transform.localScale = Vector3.one * 1.6f;
            bc.center = offset;
            bc.size = new Vector3(2,2,0.1f);
        } else
        {
            text.transform.localPosition = Vector3.zero;
            backSprite.transform.localPosition = Vector3.zero;
            text.transform.localScale = Vector3.one;
            backSprite.transform.localScale = Vector3.one * 0.8f;
            bc.center = Vector3.zero;
            bc.size = new Vector3(1, 1, 0.1f);
        }


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
        transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
    }

    public void Setup(string pieceType, Color color)
    {
        text.text = pieceType;
        backSprite.color = color;
        text.color = color;
    }

    public void Update()
    {
        dob.canDrag = !bs.animating;
        bc.enabled = !bs.animating && text.enabled; //Become intangible while animating
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
    public virtual void OnDragStop()
    {
        //Setup mode
        if (bs.setupMoves)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                if (bs.hoverX < 0 || bs.hoverX > 7 || bs.hoverY < 0 || bs.hoverY > 7)
                {
                    bs.TrySetupMove(this, Move.PackMove((byte)x, (byte)y, 15, 15));
                }
                else
                {
                    bs.TrySetupMove(this, x, y, bs.hoverX, bs.hoverY);
                }
            } else
            {
                transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
            }
            return;
        }


        //debug: lets you play as black
        //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
        if (!bs.blackIsAI)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, bs.board.blackToMove ? Piece.PieceAlignment.Black : Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
                //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
            } else
            {

            }
        }
        else
        {
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
            } else
            {

            }
        }

        if (!bs.animating)
        {
            x = bs.hoverX;
            y = bs.hoverY;
            transform.position = BoardScript.GetSpritePositionFromCoordinates(bs.hoverX, bs.hoverY, -0.5f);
            bs.FixBoardBasedOnPosition();
        }
    }

    public void SetPosition(int nx, int ny)
    {
        x = nx;
        y = ny;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(nx, ny, -0.5f);
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
