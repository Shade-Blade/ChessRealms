using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleWinPanelScript : MonoBehaviour
{
    public BoardScript bs;
    public TMPro.TMP_Text text;

    public void Start()
    {
        if (bs.board.globalData.enemyModifier != 0)
        {
            text.text = "Win $" + Mathf.Max(6, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 5f));
        }
        else
        {
            text.text = "Win $" + Mathf.Max(4, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 8f));
        }
    }

    public void ContinueButton()
    {
        if (bs.board.globalData.enemyModifier != 0)
        {
            MainManager.Instance.playerData.coins += Mathf.Max(6, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 5f));
        }
        else
        {
            MainManager.Instance.playerData.coins += Mathf.Max(4, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 8f));
        }
        FindObjectOfType<OverworldScript>().ReturnFromNode();
        Destroy(gameObject);
    }
}
