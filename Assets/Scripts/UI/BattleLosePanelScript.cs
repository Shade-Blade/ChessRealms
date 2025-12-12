using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLosePanelScript : MonoBehaviour
{
    public Image loseButton;
    public Image loseButtonBorder;
    public TMPro.TMP_Text loseButtonText;
    public TMPro.TMP_Text runStats;

    public float lifetime;
    public void Start()
    {
        transform.localPosition = Vector3.up * 500;

        loseButtonText.text = "Retry <size=50%>(" + MainManager.Instance.playerData.retriesLeft + " left)</size>";
        if (MainManager.Instance.playerData.retriesLeft == 0)
        {
            loseButton.color = new Color(0.5f, 0.5f, 0.5f);
            loseButtonBorder.color = new Color(0.75f, 0.75f, 0.75f);
        }

        runStats.text = "Made it to Realm " + MainManager.Instance.playerData.realmsComplete + " Battle " + MainManager.Instance.playerData.realmBattlesComplete;
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
