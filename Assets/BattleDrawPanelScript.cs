using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDrawPanelScript : MonoBehaviour
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

        retryButtonText.text = "Retry <size=50%>(Free)</size>";

        undoButtonText.text = "Undo <size=50%>(Free)</size>";

        runStats.SetText("You must Retry or Undo here.<line>Retries and Undos are free here.", true, true);
    }

    public void Update()
    {
        if (!bs.animating)
        {
            lifetime += Time.deltaTime;
        }

        if (lifetime > (0.8f / ((1 / 36f) * bs.animationSpeed)))
        {
            if (bs.animationSpeed >= 10000)
            {
                transform.localPosition = Vector3.zero;
            }
            else
            {
                transform.localPosition = MainManager.EasingQuadraticTime(transform.localPosition, Vector3.zero, MainManager.EasingQuadraticForce(500, (0.25f / ((1 / 36f) * bs.animationSpeed))));
            }
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
        bs.DoubleUndo();
        Destroy(gameObject);
    }
}
