using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanScript : MonoBehaviour
{
    public SpriteRenderer sprite;
    public BoxCollider bc;
    public TMPro.TMP_Text text;

    public bool active;

    public bool highlighted;
    public bool forbidden;

    //is this position inside the trash can bounds
    public bool QueryPosition(Vector3 position)
    {
        return Mathf.Abs(transform.position.x - position.x) <= bc.size.x / 2 && Mathf.Abs(transform.position.y - position.y) <= bc.size.y / 2;
    }

    public void SetActive()
    {
        active = true;
    }
    public void SetHighlight()
    {
        highlighted = true;
    }
    public void SetForbidden()
    {
        forbidden = true;
    }

    public void Update()
    {
        if (!active)
        {
            sprite.color = new Color(1, 0.4f, 0.4f, 0);
            text.text = "";
        } else
        {
            if (forbidden)
            {
                sprite.color = new Color(0.6f, 0.4f, 0.4f, 1);
                text.text = "Can't Sell";
            }
            else if (highlighted)
            {
                sprite.color = new Color(1, 0f, 0f, 1);
            }
            else
            {
                sprite.color = new Color(0.75f, 0.2f, 0.2f, 1);
            }
        }
        active = false;
        highlighted = false;
        forbidden = false;
    }
}
