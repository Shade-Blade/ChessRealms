using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleWinPanelScript : MonoBehaviour
{
    public BattleBoardScript bs;
    public TextDisplayer text;
    public float lifetime;
    Board.VictoryType vt;

    public GameObject subobject;

    public Action awaitAction;

    public void Setup(Board.VictoryType vt)
    {
        this.vt = vt;
    }

    public void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime > 0.2f)
        {
            transform.localPosition = MainManager.EasingQuadraticTime(transform.localPosition, Vector3.zero, MainManager.EasingQuadraticForce(500, 0.25f));
        }

        if (awaitAction != null && !bs.animating)
        {
            //do the action
            awaitAction.Invoke();
            awaitAction = null;
        }
    }

    public void Start()
    {
        transform.localPosition = Vector3.up * 500;
        int value = Mathf.Max(6, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 5f));
        if (bs.board.globalData.enemyModifier != 0)
        {
            value = Mathf.Max(6, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 5f));
        }
        else
        {
            value = Mathf.Max(4, Mathf.CeilToInt((bs.board.globalData.blackPerPlayerInfo.startPieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 8f));
        }
        text.SetText("Won by " + vt.ToString() + " Victory on Turn " + (bs.board.GetTurn()) + ".\nYou won <outlinecolor,#ffff00>$" + value + "</color>", true, true);
    }

    public void ContinueButton()
    {
        bs.StartAnimtingEndAnimation();
        subobject.SetActive(false);
        awaitAction = () => ExecuteContinue();
    }

    public void ExecuteContinue()
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
