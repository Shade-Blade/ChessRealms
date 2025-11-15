using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupPieceScript : PieceScript
{
    Vector3 homePos;

    public override void Start()
    {
        base.Start();

        homePos = transform.position;

        //Debug
        if (piece != 0)
        {
            Setup(piece);
        }
    }

    public override void ForceDeselect()
    {
        seb.ForceDeselect();
        dob.isDragged = false;
        transform.position = homePos;
    }

    public void Setup(uint piece)
    {
        this.piece = piece;

        if (piece == 0 || Piece.GetPieceType(piece) == Piece.PieceType.Null)
        {
            text.enabled = false;
            backSprite.enabled = false;
            bc.enabled = false;
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
                return;
            }

            Vector3 offset = (Vector3.up + Vector3.right) * 0.5f;
            text.transform.localPosition = offset;
            backSprite.transform.localPosition = offset;
            text.transform.localScale = Vector3.one * 2;
            backSprite.transform.localScale = Vector3.one * 1.6f;
            bc.center = offset;
            bc.size = new Vector3(2, 2, 0.1f);
        }
        else
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
    }

    public override void OnDragStop()
    {
        if (bs.setupMoves)
        {
            if (bs.hoverX < 0 || bs.hoverX > 7)
            {
                return;
            }

            if (bs.hoverY < 0 || bs.hoverY > 7)
            {
                return;
            }

            bs.TrySetupMove(this, Move.MakeSetupCreateMove(Piece.GetPieceType(piece), Piece.GetPieceAlignment(piece), (byte)bs.hoverX, (byte)bs.hoverY));

            ResetPosition();
            bs.FixBoardBasedOnPosition();
            return;
        }

        ResetPosition();
        //bs.FixBoardBasedOnPosition();
    }

    public void ResetPosition()
    {
        transform.position = homePos;
    }
}
