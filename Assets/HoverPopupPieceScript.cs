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
        backSprite.sprite = Text_PieceSprite.GetPieceSprite(pieceType);
        backSprite.material = Text_PieceSprite.GetMaterialGUI(0);
        backSprite.color = Piece.GetPieceColor(Piece.PieceAlignment.Black);

        if (MainManager.Instance.pieceTextVisible)
        {
            text.text = Piece.GetPieceName(pieceType);
        }
        else
        {
            text.text = "";
        }
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
