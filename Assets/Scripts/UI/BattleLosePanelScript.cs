using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLosePanelScript : MonoBehaviour
{
    public BattleBoardScript bs;
    public Image retryButton;
    public Image retryButtonBorder;
    public TMPro.TMP_Text retryButtonText;
    public Image undoButton;
    public Image undoButtonBorder;
    public TMPro.TMP_Text undoButtonText;
    public TextDisplayer runStats;

    public GameObject subobject;

    public float lifetime;
    Board.VictoryType vt;

    public Action awaitAction;

    public void Setup(Board.VictoryType vt)
    {
        this.vt = vt;
    }


    public void Start()
    {
        transform.localPosition = Vector3.up * 500;

        retryButtonText.text = "Retry <size=50%>(" + MainManager.Instance.playerData.retriesLeft + " left)</size>";
        if (MainManager.Instance.playerData.retriesLeft == 0)
        {
            retryButton.color = new Color(0.5f, 0.5f, 0.5f);
            retryButtonBorder.color = new Color(0.75f, 0.75f, 0.75f);
        }

        undoButtonText.text = "Undo <size=50%>(" + MainManager.Instance.playerData.undosLeft + " left)</size>";
        if (MainManager.Instance.playerData.undosLeft == 0)
        {
            undoButton.color = new Color(0.5f, 0.5f, 0.5f);
            undoButtonBorder.color = new Color(0.75f, 0.75f, 0.75f);
        }

        runStats.SetText("Lost by " + vt.ToString() + " Victory.\nMade it to Realm " + (MainManager.Instance.playerData.realmsComplete + 1) + ", Battle " + (MainManager.Instance.playerData.realmBattlesComplete + 1), true, true);
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

    public void RetryButton()
    {
        if (MainManager.Instance.playerData.retriesLeft == 0)
        {
            return;
        }

        //block input while awaiting something
        if (awaitAction != null)
        {
            return;
        }

        bs.StartAnimtingEndAnimation();
        subobject.SetActive(false);
        awaitAction = () => ExecuteRetry();
    }

    public void ExecuteRetry()
    {
        MainManager.Instance.playerData.retriesLeft--;
        FindObjectOfType<OverworldScript>().ReturnFromNode(false);
        Destroy(gameObject);
    }

    public void GiveUpButton()
    {
        //block input while awaiting something
        if (awaitAction != null)
        {
            return;
        }

        bs.StartAnimtingEndAnimation();
        //subobject.SetActive(false);
        //awaitAction = () => ExecuteGiveUp();
        ExecuteGiveUp();
    }

    public void ExecuteGiveUp()
    {
        MainManager.Instance.EndRun();
    }

    public void UndoButton()
    {
        if (MainManager.Instance.playerData.undosLeft == 0)
        {
            return;
        }

        bs.DoubleUndo();
        Destroy(gameObject);
    }
}
