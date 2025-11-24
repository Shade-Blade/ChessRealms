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

    public TrashCanScript trashCan;
    public bool canDelete;

    public virtual void Start()
    {
        trashCan = FindObjectOfType<TrashCanScript>();

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

        //should not be possible
        if (piece == 0)
        {
            Destroy(gameObject);
            return;
        }

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

    public virtual void Update()
    {
        dob.canDrag = bs.CanSelectPieces();
        bc.enabled = bs.CanSelectPieces() && text.enabled; //Become intangible while animating (Also be intangible if you are the wrong part of a giant)
    }

    public virtual void OnSelect()
    {
        if (bs.setupMoves)
        {
            if (x < 0 || x > 7 || y < 0 || y > 7)
            {
                canDelete = false;
            } else
            {
                canDelete = bs.IsSetupMoveLegal(this, Move.PackMove(x, y, 15, 15));
            }
        }

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

    public int GetCost()
    {
        int cost = 0;
        if (GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece)) != null)
        {
            cost = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece)).pieceValueX2;
        }

        if (Piece.GetPieceModifier(piece) != 0)
        {
            cost += 5;
        }

        cost &= GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE;

        return cost;
    }

    public void OnDragStart()
    {
    }
    public virtual void OnDragStay()
    {
        (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position);            
        if (trashCan != null && bs.setupMoves)
        {
            if (!canDelete)
            {
                trashCan.text.text = "Can't Sell";
            }
            else
            {
                trashCan.text.text = "Sell: $" + (GetCost() & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE);
            }
            trashCan.SetActive();
            if (!canDelete)
            {
                trashCan.SetForbidden();
            }
            if (isGiant)
            {
                if (trashCan.QueryPosition(transform.position + new Vector3(BoardScript.SQUARE_SIZE / 2, BoardScript.SQUARE_SIZE / 2)))
                {
                    if (canDelete)
                    {
                        trashCan.SetHighlight();
                    }
                }
            }
            else
            {
                if (trashCan.QueryPosition(transform.position))
                {
                    if (canDelete)
                    {
                        trashCan.SetHighlight();
                    }
                }
            }
        }
    }
    public virtual void OnDragStop()
    {
        //Setup mode
        if (bs.setupMoves)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                if ((bs is SetupBoardScript && bs.hoverX >= 0 && bs.hoverX <= 7 && bs.hoverY >= 0 && bs.hoverY <= 1) || (!(bs is SetupBoardScript) && bs.hoverX >= 0 && bs.hoverX <= 7 && bs.hoverY >= 0 && bs.hoverY <= 7))
                {
                    bs.TrySetupMove(this, x, y, bs.hoverX, bs.hoverY);
                } else if (trashCan != null && isGiant ? trashCan.QueryPosition(transform.position + new Vector3(BoardScript.SQUARE_SIZE / 2, BoardScript.SQUARE_SIZE / 2)) : trashCan.QueryPosition(transform.position))
                {
                    //does this make sense?
                    //This should not have problems (i.e. being able to get money and keep the piece)
                    bool value = bs.TrySetupMove(this, Move.PackMove(x, y, 15, 15));
                    if (value)
                    {
                        MainManager.Instance.playerData.coins += GlobalPieceManager.GetPieceTableEntry(piece).pieceValueX2;
                    }
                } else
                {
                    transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
                }
            } else
            {
                transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
            }
            return;
        }


        //debug: lets you play as black
        //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
        if (bs is BattleBoardScript bbs && !bbs.blackIsAI)
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

        if (bs.CanSelectPieces())
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
