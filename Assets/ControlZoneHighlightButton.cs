using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlZoneHighlightButton : MonoBehaviour
{
    public BattleBoardScript bs;

    public SpriteRenderer backSprite;

    public void OnMouseDown()
    {
        bs.controlZoneHighlight = !bs.controlZoneHighlight;
        bs.UpdateControlHighlight();
    }

    public void Update()
    {
        if (bs.controlZoneHighlight)
        {
            backSprite.color = new Color(0, 1, 1, 1);
        }
        else
        {
            backSprite.color = new Color(0f, 0.5f, 0.5f, 1);
        }
    }
}
