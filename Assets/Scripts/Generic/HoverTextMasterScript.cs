using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTextMasterScript : MonoBehaviour
{
    public static HoverTextMasterScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HoverTextMasterScript>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    private static HoverTextMasterScript instance;

    public GameObject hoverPopupPrototype;
    public GameObject hoverPopupArmyPrototype;

    public HoverPopupScript hoverPopup;

    public string hoverText;
    public string pastHoverText;

    //process: script sets hover text every frame
    public void Update()
    {
        if (hoverText == null)
        {
            if (hoverPopup != null)
            {
                Destroy(hoverPopup.gameObject);
            }
        } else
        {
            if (!hoverText.Equals(pastHoverText))
            {
                //Hover popup must be updated
                if (hoverPopup == null)
                {
                    MakeHoverPopup(hoverText);
                } else
                {
                    Destroy(hoverPopup.gameObject);
                    MakeHoverPopup(hoverText);
                    //hoverPopup.SetText(hoverText, true, true);
                }
            }
        }

        pastHoverText = hoverText;
        hoverText = null;
    }

    public void MakeHoverPopup(string s)
    {
        GameObject o = Instantiate(hoverPopupPrototype, MainManager.Instance.Canvas.transform);
        HoverPopupScript hps = o.GetComponent<HoverPopupScript>();
        hps.SetText(s, true, true);
        hps.PositionUpdate();

        hoverPopup = hps;
    }

    public string ArmyToString(Piece.PieceType[] army)
    {
        string output = "*";

        for (int i = 0; i < army.Length; i++)
        {
            if (i != 0)
            {
                output += ", ";
            }
            output += army[i].ToString();
        }
        return output;
    }

    public void MakeHoverPopup(Piece.PieceType[] army, string text)
    {
        //there already is a army popup
        if (hoverText != null && hoverText.Equals(ArmyToString(army)))
        {
            return;
        }

        if (hoverPopup != null)
        {
            Destroy(hoverPopup.gameObject);
        }

        GameObject o = Instantiate(hoverPopupArmyPrototype, MainManager.Instance.Canvas.transform);
        HoverArmyPopupScript hps = o.GetComponent<HoverArmyPopupScript>();
        hoverText = ArmyToString(army);
        hps.SetArmy(army);
        hps.SetText(text, true, true);
        hps.PositionUpdate();

        hoverPopup = hps;
    }

    public void SetHoverText(string p_hoverText)
    {
        hoverText = p_hoverText;
    }
}
