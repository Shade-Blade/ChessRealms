using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece;

public class SetupPieceScript : PieceScript, IShopItem
{
    Vector3 homePos;
    public bool canInteract;

    public ShopItemScript sis;

    public void SetShopItemScript(ShopItemScript sis)
    {
        this.sis = sis;
    }
    public void ResetHomePosition(Vector3 homePos)
    {
        this.homePos = homePos;
    }


    public override void Start()
    {
        base.Start();

        homePos = transform.position;
        canInteract = true;

        //Debug
        Setup(piece);
    }

    public override void Update()
    {
        if (bs == null || piece == 0)
        {
            backSprite.enabled = false;
            text.enabled = false;
            dob.canDrag = false;
            bc.enabled = false;
            return;
        }
        dob.canDrag = bs.CanSelectPieces() && bs.setupMoves && canInteract;
        bc.enabled = bs.CanSelectPieces() && text.enabled && bs.setupMoves; //Become intangible while animating (Shop item version is still selectable so you can see descriptions)

        if (isHover)
        {
            HoverTextMasterScript.Instance.SetHoverText(GetHoverText());
        }
        lastIsHover = isHover;
        isHover = false;

        if (!bs.setupMoves)
        {
            backSprite.enabled = false;
            text.enabled = false;
        } else
        {
            backSprite.enabled = true;
            text.enabled = true;
        }
    }

    public override void OnSelect()
    {
        canDelete = false;

        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        //backSprite.color = new Color(1 - backSprite.color.r, backSprite.color.g, backSprite.color.b, 1);
        selectObject.SetActive(true);

        bs.SelectPiece(this);
        isSelected = true;
    }

    public override void OnDragStay()
    {
        //giant is offset by 0.5
        if (isGiant)
        {
            (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position + Vector3.left * 0.5f);
        }
        else
        {
            (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position);
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

            //Different offset: The SetupPiece is centered on the x level it's placed on
            //Vector3 offset = (Vector3.up + Vector3.right) * 0.5f;
            Vector3 offset = (Vector3.up) * 0.5f;
            text.transform.localPosition = offset + Vector3.back * 0.05f;
            backSprite.transform.localPosition = offset;
            text.transform.localScale = Vector3.one * 2;
            backSprite.transform.localScale = Vector3.one * 2f;
            selectObject.transform.localPosition = offset + Vector3.forward * 0.2f;
            selectObject.transform.localScale = Vector3.one * 2f;
            bc.center = offset;
            bc.size = new Vector3(2, 2, 0.1f);
        }
        else
        {
            text.transform.localPosition = Vector3.back * 0.05f;
            backSprite.transform.localPosition = Vector3.zero;
            text.transform.localScale = Vector3.one;
            backSprite.transform.localScale = Vector3.one;
            selectObject.transform.localPosition = Vector3.forward * 0.2f;
            selectObject.transform.localScale = Vector3.one;
            bc.center = Vector3.zero;
            bc.size = new Vector3(1, 1, 0.1f);
        }

        backSprite.sprite = Text_PieceSprite.GetPieceSprite(Piece.GetPieceType(piece));
        backSprite.material = Text_PieceSprite.GetMaterial(Piece.GetPieceModifier(piece));
        MaterialPropertyBlock mpb = Text_PieceSprite.SetupMaterialProperties(Piece.GetPieceStatusEffect(piece), null);
        mpb.SetTexture("_MainTex", backSprite.sprite.texture);
        backSprite.SetPropertyBlock(mpb);

        if (MainManager.Instance.pieceTextVisible)
        {
            text.text = Piece.GetPieceName(Piece.GetPieceType(piece)); //Piece.GetPieceType(piece).ToString();
        }
        else
        {
            text.text = ""; //Piece.GetPieceType(piece).ToString();
        }

        /*
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
        */

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        Color color = Piece.GetPieceColor(pa);

        backSprite.color = color;
        text.color = color;

        if (pa == PieceAlignment.Black)
        {
            text.color = Color.black;
        }
    }

    public override void OnDragStop()
    {
        if (bs.setupMoves)
        {
            if (bs.hoverX < 0 || bs.hoverX > 7)
            {
                ResetPosition();
                return;
            }

            if (bs.hoverY < 0 || bs.hoverY > 7)
            {
                ResetPosition();
                return;
            }

            if (bs is SetupBoardScript)
            {
                if (bs.hoverY < 0 || bs.hoverY > 1)
                {
                    ResetPosition();
                    return;
                }
            }

            if (bs.TrySetupMove(this, Move.MakeSetupCreateMove(Piece.GetPieceType(piece), Piece.GetPieceAlignment(piece), (byte)bs.hoverX, (byte)bs.hoverY)))
            {
                if (sis != null)
                {
                    sis.Purchase();
                }
            }

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

    public void EnableInteraction()
    {
        canInteract = true;
    }

    public void DisableInteraction()
    {
        canInteract = false;
    }
}
