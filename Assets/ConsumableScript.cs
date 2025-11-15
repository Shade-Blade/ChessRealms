using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableScript : MonoBehaviour, ISelectEventListener, IDragEventListener
{
    Vector3 homePos;

    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;
    public BoxCollider bc;

    public BattleBoardScript bs;

    public DraggableObject dob;
    public SelectableObject seb;

    public Move.ConsumableMoveType cmt;

    public virtual void Start()
    {
        homePos = transform.position;
        dob = GetComponent<DraggableObject>();
        dob.canDrag = true;
    }

    public void Setup(Move.ConsumableMoveType cmt)
    {
        this.cmt = cmt;

        if (cmt == Move.ConsumableMoveType.None)
        {
            text.enabled = false;
            backSprite.enabled = false;
            bc.enabled = false;
            return;
        }
        text.enabled = true;
        backSprite.enabled = true;
        bc.enabled = true;

        text.text = cmt.ToString();
    }

    public virtual void ForceDeselect()
    {
        seb.ForceDeselect();
        dob.isDragged = false;
        ResetPosition();
    }

    public void OnSelect()
    {
        backSprite.color = new Color(1, 1, 1, 1);
        backSprite.color = new Color(1 - backSprite.color.r, backSprite.color.g, backSprite.color.b, 1);
        bs.SelectConsumable(this);
    }

    public void OnDeselect()
    {
        backSprite.color = new Color(1, 1, 1, 1);
        if (bs.selectedPiece == null || bs.selectedPiece == this)
        {
            bs.ResetSelected(false);
        }
    }

    public void ResetPosition()
    {
        transform.position = homePos;
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
            //Consumable moves are not allowed in setup mode
            ResetPosition();
            return;
        }


        //debug: lets you play as black
        //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);

        if (bs.hoverX < 0 || bs.hoverX > 7)
        {
            ResetPosition();
            return;
        }

        if (bs.hoverX < 0 || bs.hoverX > 7)
        {
            ResetPosition();
            return;
        }

        bs.TryConsumableMove(this, (byte)bs.hoverX, (byte)bs.hoverY);
        ResetPosition();
    }
}
