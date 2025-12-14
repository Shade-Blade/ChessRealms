using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLosePanelScript : MonoBehaviour
{
    public Image loseButton;
    public Image loseButtonBorder;
    public TMPro.TMP_Text loseButtonText;
    public TextDisplayer runStats;

    public float lifetime;
    Board.VictoryType vt;

    public void Setup(Board.VictoryType vt)
    {
        this.vt = vt;
    }


    public void Start()
    {
        transform.localPosition = Vector3.up * 500;

        loseButtonText.text = "Retry <size=50%>(" + MainManager.Instance.playerData.retriesLeft + " left)</size>";
        if (MainManager.Instance.playerData.retriesLeft == 0)
        {
            loseButton.color = new Color(0.5f, 0.5f, 0.5f);
            loseButtonBorder.color = new Color(0.75f, 0.75f, 0.75f);
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
    }

    public void RetryButton()
    {
        if (MainManager.Instance.playerData.retriesLeft == 0)
        {
            return;
        }

        MainManager.Instance.playerData.retriesLeft--;
        FindObjectOfType<OverworldScript>().ReturnFromNode(false);
        Destroy(gameObject);
    }

    public void GiveUpButton()
    {
        MainManager.Instance.EndRun();
    }
}
