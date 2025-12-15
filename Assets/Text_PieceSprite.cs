using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_PieceSprite : Text_SpecialSprite
{
    //text width number
    public static string GetWidth(string b)
    {
        //use em
        return (WIDTH) + "em";
    }

    //public string[] args;
    //public int index;
    public bool visible;

    public const float WIDTH = 1.5f;

    public Sprite itemSprite;

    //build an item sprite with given args
    //position is handled by the thing that makes the item sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_PieceSprite);
        Text_PieceSprite its = obj.GetComponent<Text_PieceSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.itemSprite = GetPieceSprite(args[0]);
            its.baseBox.sprite = its.itemSprite;
        }
        else
        {
            its.itemSprite = null;
            its.baseBox.sprite = its.itemSprite;
        }

        return obj;
    }

    public static MaterialPropertyBlock SetupMaterialProperties(Piece.PieceStatusEffect pse, MaterialPropertyBlock mpb)
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        float leak = 1;
        Color blackColor = new Color(0, 0, 0, 1);
        Color grayColor = new Color(0, 0, 0, 1);
        Color whiteColor = new Color(0, 0, 0, 1);

        switch (pse)
        {
            case Piece.PieceStatusEffect.None:
                break;
            case Piece.PieceStatusEffect.Bloodlust:
                blackColor = new Color(0.5f, 0f, 0f, 1f);
                grayColor = new Color(1f, 0.2f, 0f, 1f);
                whiteColor = new Color(1f, 0.6f, 0.3f, 1f);
                leak = 0.2f;
                break;
            case Piece.PieceStatusEffect.Sparked:
                blackColor = new Color(0.35f, 0.05f, 0f, 1f);
                grayColor = new Color(1f, 0.25f, 0.05f, 1f);
                whiteColor = new Color(1f, 0.8f, 0.45f, 1f);
                leak = 0.2f;
                break;
            case Piece.PieceStatusEffect.Poisoned:
                blackColor = new Color(0f, 0.2f, 0f, 1f);
                grayColor = new Color(0.2f, 1f, 0.1f, 1f);
                whiteColor = new Color(0.3f, 1f, 0.15f, 1f);
                leak = 0.4f;
                break;
            case Piece.PieceStatusEffect.Frozen:
                blackColor = new Color(0.24f, 0.48f, 0.8f, 1f);
                grayColor = new Color(0.5f, 0.7f, 1f, 1f);
                whiteColor = new Color(0.63f, 0.78f, 0.96f, 1f);
                leak = 0.15f;
                break;
            case Piece.PieceStatusEffect.Soaked:
                blackColor = new Color(0.1f, 0.2f, 0.4f, 1f);
                grayColor = new Color(0.3f, 0.45f, 0.9f, 1f);
                whiteColor = new Color(0.7f, 0.9f, 1f, 1f);
                leak = 0.2f;
                break;
            case Piece.PieceStatusEffect.Ghostly:
                blackColor = new Color(0.2f, 0.05f, 0.3f, 1f);
                grayColor = new Color(0.5f, 0f, 1f, 0.8f);
                whiteColor = new Color(0.75f, 0.5f, 1f, 0.6f);
                leak = 0.2f;
                break;
            case Piece.PieceStatusEffect.Fragile:
                blackColor = new Color(0f, 0.2f, 0.1f, 1f);
                grayColor = new Color(0.2f, 1f, 0.4f, 1f);
                whiteColor = new Color(0.3f, 1f, 0.5f, 1f);
                leak = 0.4f;
                break;
            case Piece.PieceStatusEffect.Heavy:
                blackColor = new Color(0.2f, 0.2f, 0f, 1f);
                grayColor = new Color(0.4f, 0.4f, 1f, 1f);
                whiteColor = new Color(0.6f, 0.6f, 0.15f, 1f);
                leak = 0.4f;
                break;
            case Piece.PieceStatusEffect.Light:
                blackColor = new Color(0f, 0.3f, 0.4f, 1f);
                grayColor = new Color(0.1f, 0.65f, 0.75f, 1f);
                whiteColor = new Color(0.5f, 0.85f, 1f, 1f);
                leak = 0.4f;
                break;
        }

        mpb.SetFloat("_Leak", leak);
        mpb.SetVector("_BlackColor", blackColor);
        mpb.SetVector("_GrayColor", grayColor);
        mpb.SetVector("_WhiteColor", whiteColor);
        return mpb;
    }

    public static Material GetMaterial(Piece.PieceModifier pm)
    {
        switch (pm)
        {
            case Piece.PieceModifier.Vengeful:
                return GlobalPieceManager.Instance.pieceMaterials[1];
            case Piece.PieceModifier.Phoenix:
                return GlobalPieceManager.Instance.pieceMaterials[2];
            case Piece.PieceModifier.Radiant:
                return GlobalPieceManager.Instance.pieceMaterials[3];
            case Piece.PieceModifier.Winged:
                return GlobalPieceManager.Instance.pieceMaterials[4];
            case Piece.PieceModifier.Spectral:
                return GlobalPieceManager.Instance.pieceMaterials[5];
            case Piece.PieceModifier.Immune:
                return GlobalPieceManager.Instance.pieceMaterials[6];
            case Piece.PieceModifier.Warped:
                return GlobalPieceManager.Instance.pieceMaterials[7];
            case Piece.PieceModifier.Shielded:
                return GlobalPieceManager.Instance.pieceMaterials[8];
            case Piece.PieceModifier.HalfShielded:
                return GlobalPieceManager.Instance.pieceMaterials[9];
        }

        return GlobalPieceManager.Instance.pieceMaterials[0];
    }
    public static Material GetMaterialGUI(Piece.PieceModifier pm)
    {
        switch (pm)
        {
            case Piece.PieceModifier.Vengeful:
                return GlobalPieceManager.Instance.guiPieceMaterials[1];
            case Piece.PieceModifier.Phoenix:
                return GlobalPieceManager.Instance.guiPieceMaterials[2];
            case Piece.PieceModifier.Radiant:
                return GlobalPieceManager.Instance.guiPieceMaterials[3];
            case Piece.PieceModifier.Winged:
                return GlobalPieceManager.Instance.guiPieceMaterials[4];
            case Piece.PieceModifier.Spectral:
                return GlobalPieceManager.Instance.guiPieceMaterials[5];
            case Piece.PieceModifier.Immune:
                return GlobalPieceManager.Instance.guiPieceMaterials[6];
            case Piece.PieceModifier.Warped:
                return GlobalPieceManager.Instance.guiPieceMaterials[7];
            case Piece.PieceModifier.Shielded:
                return GlobalPieceManager.Instance.guiPieceMaterials[8];
            case Piece.PieceModifier.HalfShielded:
                return GlobalPieceManager.Instance.guiPieceMaterials[9];
        }

        return GlobalPieceManager.Instance.guiPieceMaterials[0];
    }

    public static Sprite GetPieceSprite(string piece)
    {
        //Debug.Log("Display " + badge);
        Piece.PieceType pieceType;

        Enum.TryParse(piece, true, out pieceType);

        //random failsafe thing I guess
        if ((int)pieceType < 1)
        {
            Debug.Log("Parse fail: " + piece);
            return MainManager.Instance.pieceSprites[MainManager.Instance.pieceSprites.Length - 1];
        }

        return GetPieceSprite(pieceType);
    }

    public static Sprite GetPieceSprite(Piece.PieceType pieceType)
    {
        return MainManager.Instance.pieceSprites[(int)(pieceType) - 1];
    }
}
