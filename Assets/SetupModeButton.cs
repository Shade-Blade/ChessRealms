using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupModeButton : MonoBehaviour
{
    public BattleBoardScript bs;

    public SpriteRenderer backSprite;
    public TMPro.TMP_InputField setupModeTypeField;
    public Piece.PieceType oldType;
    public List<SetupPieceScript> sps;
    public int alignment;
    int pastAlignment;

    public void Start()
    {
        for (int i = 0; i < sps.Count; i++)
        {
            sps[i].Setup(0);
        }
    }

    public void OnMouseDown()
    {
        bs.setupMoves = !bs.setupMoves;
        bs.UpdateControlHighlight();
    }

    public void Update()
    {
        if (bs.setupMoves)
        {
            backSprite.color = new Color(0, 0, 1, 1);
        }
        else
        {
            backSprite.color = new Color(0f, 0, 0.5f, 1);
        }

        Enum.TryParse(setupModeTypeField.text, out Piece.PieceType newType);

        if (newType != oldType || pastAlignment != alignment)
        {
            for (int i = 0; i < sps.Count; i++)
            {
                sps[i].Setup(Piece.SetPieceType(newType, Piece.SetPieceAlignment((Piece.PieceAlignment)alignment, 0)));
            }
            oldType = newType;
        }
        pastAlignment = alignment;
    }
}
