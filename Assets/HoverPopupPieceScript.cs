using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverPopupPieceScript : MonoBehaviour
{
    public TMPro.TMP_Text text;
    public Image backSprite;

    public void Setup(Piece.PieceType pieceType)
    {
        text.text = Piece.GetPieceName(pieceType);
        bool giant = (GlobalPieceManager.GetPieceTableEntry(pieceType).piecePropertyB & Piece.PiecePropertyB.Giant) != 0;

        if (giant)
        {
            backSprite.rectTransform.localPosition = new Vector3(15, 15, 0);
            backSprite.rectTransform.sizeDelta = new Vector2(48, 48);
            text.rectTransform.localPosition = new Vector3(15, 15, 0);
            text.rectTransform.sizeDelta = new Vector2(48, 48);
        } else
        {
            backSprite.rectTransform.localPosition = Vector3.zero;
            text.rectTransform.localPosition = Vector3.zero;
        }
    }
}
