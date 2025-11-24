using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public void OnMouseDown()
    {
        FindObjectOfType<OverworldScript>().ReturnFromNode();
        Destroy(gameObject);
    }
}
