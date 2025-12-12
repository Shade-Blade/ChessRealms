using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeSlotScript : MonoBehaviour
{
    public SpriteRenderer sprite;
    public BoxCollider bc;
    public ConsumableBadgePanelScript cps;

    public bool highlighted;

    public ShopItemScript sis;

    public void SetShopItemScript(ShopItemScript sis)
    {
        this.sis = sis;
    }


    public void Start()
    {
        cps = FindAnyObjectByType<ConsumableBadgePanelScript>();
    }

    //is this position inside the trash can bounds
    public bool QueryPosition(Vector3 position)
    {
        return Mathf.Abs(transform.position.x - position.x) <= bc.size.x / 2 && Mathf.Abs(transform.position.y - position.y) <= bc.size.y / 2;
    }

    public void SetHighlight()
    {
        highlighted = true;
    }

    public void Update()
    {
        if (!highlighted)
        {
            sprite.color = new Color(0.4f, 0.4f, 0.7f, 0.75f);
        }
        else
        {
            sprite.color = new Color(1, 1f, 1f, 1);
        }
        highlighted = false;
    }
}
