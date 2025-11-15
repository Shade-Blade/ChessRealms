using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackAIButton : MonoBehaviour
{
    public BattleBoardScript bs;

    public SpriteRenderer backSprite;

    public void OnMouseDown()
    {
        bs.blackIsAI = !bs.blackIsAI;
    }

    public void Update()
    {
        if (bs.blackIsAI)
        {
            backSprite.color = new Color(0, 0, 0, 1);
        }
        else
        {
            backSprite.color = new Color(0.25f, 0.25f, 0.25f, 1);
        }
    }
}
