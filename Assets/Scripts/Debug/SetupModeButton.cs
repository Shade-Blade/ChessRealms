using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupModeButton : MonoBehaviour
{
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

    public void AlignmentButton()
    {
        alignment++;
        alignment %= 4;
    }

    public void Update()
    {
        Enum.TryParse(setupModeTypeField.text, out Piece.PieceType newType);

        if (newType != oldType || pastAlignment != alignment)
        {
            for (int i = 0; i < sps.Count; i++)
            {
                sps[i].Setup(Piece.SetPieceType(newType, Piece.SetPieceAlignment((Piece.PieceAlignment)(alignment << 30), 0)));
            }
            oldType = newType;
        }
        pastAlignment = alignment;
    }
}
