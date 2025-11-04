using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupModeAlignmentButton : MonoBehaviour
{
    public SetupModeButton smb;
    public SpriteRenderer backSprite;

    public void OnMouseDown()
    {
        smb.alignment++;
        smb.alignment %= 4;
    }

    public void Update()
    {
        switch (smb.alignment)
        {
            case 0:
                backSprite.color = new Color(1, 1, 1, 1);
                break;
            case 1:
                backSprite.color = new Color(0, 0, 0, 1);
                break;
            case 2:
                backSprite.color = new Color(1, 1, 0, 1);
                break;
            case 3:
                backSprite.color = new Color(1, 0, 1, 1);
                break;
        }
    }
}
