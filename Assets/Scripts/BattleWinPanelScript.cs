using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleWinPanelScript : MonoBehaviour
{
    public BoardScript bs;
    public TMPro.TMP_Text text;

    public void Start()
    {
        text.text = "Win $" + Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 8f);
    }

    public void ContinueButton()
    {
        MainManager.Instance.playerData.coins += Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 8f);
        FindObjectOfType<OverworldScript>().ReturnFromNode();
        Destroy(gameObject);
    }
}
