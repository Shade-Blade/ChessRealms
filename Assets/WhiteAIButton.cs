using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteAIButton : MonoBehaviour
{
    public BoardScript bs;

    public SpriteRenderer backSprite;

    public void OnMouseDown()
    {
        bs.whiteIsAI = !bs.whiteIsAI;
    }

    public void Update()
    {
        if (bs.whiteIsAI)
        {
            backSprite.color = new Color(1, 1, 1, 1);
        } else
        {
            backSprite.color = new Color(0.75f, 0.75f, 0.75f, 1);
        }
    }
}
