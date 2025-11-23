using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLosePanelScript : MonoBehaviour
{
    public void ContinueButton()
    {
        FindObjectOfType<OverworldScript>().ReturnFromNode();
        Destroy(gameObject);
    }
}
