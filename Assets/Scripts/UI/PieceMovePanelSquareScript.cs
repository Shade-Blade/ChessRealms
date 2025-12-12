using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceMovePanelSquareScript : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image imageA;
    public Image imageB;
    public Image imageC;
    public Image aura;
    public Color defaultColor;
    public Move.SpecialType specialTypeEmpty;
    public Move.SpecialType specialTypeEnemy;
    public Move.SpecialType specialTypeAlly;
    public SpecialIndication spi;
    public bool hybrid;
    public bool set;

    [Flags]
    public enum SpecialIndication
    {
        None = 0,
        Initial = 1,
        AntiRange = 1 << 1,
        Enhanced = 1 << 2,
    }

    public void Reset()
    {
        set = false;
        spi = SpecialIndication.None;
        hybrid = false;

        imageA.color = defaultColor;
        imageB.color = new Color(0, 0, 0, 0);
        imageB.rectTransform.localScale = Vector3.one;
        imageC.color = new Color(0, 0, 0, 0);
        specialTypeEmpty = Move.SpecialType.Null;
        specialTypeAlly = Move.SpecialType.Null;
        specialTypeEnemy = Move.SpecialType.Null;
        aura.color = new Color(0, 0, 0, 0);
    }

    public void SetAura()
    {
        //all auras have the same highlight color I guess
        this.aura.color = new Color(0.5f, 0.75f, 1f, 1);
    }

    public void Set(Move.SpecialType specialType, SpecialIndication spi)
    {
        //none -> set = hybrid
        if (set && spi != this.spi)
        {
            hybrid = true;
        }
        set = true;

        if (Move.SpecialMoveCanMoveOntoAllyGeneral(specialType))
        {
            specialTypeAlly = specialType;
        }
        //wacky thing to show the deposit ally moves correctly
        //They don't have capture so specialTypeEnemy is already null
        if ((Move.SpecialMoveCanMoveOntoEnemyGeneral(specialType) && this.specialTypeEnemy == Move.SpecialType.Null) || specialType == Move.SpecialType.DepositAlly || specialType == Move.SpecialType.DepositAllyPlantMove)
        {
            specialTypeEnemy = specialType;
        }
        if ((((!Move.SpecialMoveCantTargetEmpty(specialType) && this.specialTypeEmpty == Move.SpecialType.Null) || (hybrid && specialTypeEmpty != Move.SpecialType.MoveOnly))) && specialType != Move.SpecialType.DepositAlly && specialType != Move.SpecialType.DepositAllyPlantMove)
        {
            specialTypeEmpty = specialType;
        }
        //Debug.Log(specialType + " " + hybrid + " " + hybridAlt + " " + specialTypeAlly + " " + specialTypeEnemy + " " + specialTypeEmpty);

        //setup colors
        List<Move.SpecialType> types = new List<Move.SpecialType>();

        if (specialTypeAlly != Move.SpecialType.Null)
        {
            types.Add(specialTypeAlly);
        }

        //hacky setup
        if (specialTypeEmpty == Move.SpecialType.MoveOnly && hybrid)
        {
            if (specialTypeEmpty != Move.SpecialType.Null && !types.Contains(specialTypeEmpty))
            {
                types.Add(specialTypeEmpty);
            }
            if (specialTypeEnemy != Move.SpecialType.Null && !types.Contains(specialTypeEnemy))
            {
                types.Add(specialTypeEnemy);
            }
        }
        else
        {
            if (specialTypeEnemy != Move.SpecialType.Null && !types.Contains(specialTypeEnemy))
            {
                types.Add(specialTypeEnemy);
            }
            if (specialTypeEmpty != Move.SpecialType.Null && !types.Contains(specialTypeEmpty))
            {
                types.Add(specialTypeEmpty);
            }
        }

        if (spi == 0)
        {
            switch (types.Count)
            {
                case 1:
                    imageA.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    imageB.color = new Color(0, 0, 0, 0);
                    imageC.color = new Color(0, 0, 0, 0);
                    break;
                case 2:
                    imageA.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    imageB.color = PieceMovePanelScript.GetColorFromSpecialType(types[1]);
                    imageB.fillAmount = 0.5f;
                    imageC.color = new Color(0, 0, 0, 0);
                    break;
                case 3:
                    imageA.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    imageB.color = PieceMovePanelScript.GetColorFromSpecialType(types[1]);
                    imageB.fillAmount = 0.3f;
                    imageC.color = PieceMovePanelScript.GetColorFromSpecialType(types[2]);
                    imageC.fillAmount = 0.3f;
                    break;
            }
        }
        else
        {
            switch (types.Count)
            {
                case 1:
                    imageA.color = defaultColor;
                    imageB.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    imageB.rectTransform.localScale = Vector3.one * 0.5f;
                    imageB.fillAmount = 1;
                    imageC.color = new Color(0, 0, 0, 0);
                    break;
                case 2:
                    imageA.color = new Color(0, 0, 0, 0);
                    imageB.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    if (!hybrid)
                    {
                        imageB.rectTransform.localScale = Vector3.one * 0.5f;
                        imageB.fillAmount = 1;
                        imageC.color = PieceMovePanelScript.GetColorFromSpecialType(types[1]);
                        imageC.rectTransform.localScale = Vector3.one * 0.5f;
                        imageC.fillAmount = 0.5f;
                    }
                    else
                    {
                        imageB.fillAmount = 1;
                        imageC.color = PieceMovePanelScript.GetColorFromSpecialType(types[1]);
                        imageC.rectTransform.localScale = Vector3.one * 0.5f;
                        imageC.fillAmount = 1f;
                    }
                    break;
                case 3:
                    imageA.color = PieceMovePanelScript.GetColorFromSpecialType(types[0]);
                    imageB.color = PieceMovePanelScript.GetColorFromSpecialType(types[1]);
                    imageB.fillAmount = 0.3f;
                    imageC.color = PieceMovePanelScript.GetColorFromSpecialType(types[2]);
                    imageC.fillAmount = 0.3f;
                    break;
            }
        }

        this.spi = spi;
    }
}
