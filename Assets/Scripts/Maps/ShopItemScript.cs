using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemScript : MonoBehaviour
{
    public int cost;
    public IShopItem shopItem;
    public TMPro.TMP_Text text;
    public bool canInteract;

    public void Start()
    {
        if (shopItem == null)
        {
            shopItem = GetComponentInChildren<IShopItem>();
            shopItem.SetShopItemScript(this);
            ResetHomePosition();
        }

        if (shopItem == null)
        {
            Destroy(gameObject);
            return;
        }

        cost = shopItem.GetCost();
        text.text = "$" + cost;
    }

    public void Update()
    {
        cost = shopItem.GetCost();
        text.text = "$" + cost;
        if (canInteract)
        {
            if (MainManager.Instance.playerData.coins >= cost)
            {
                shopItem.EnableInteraction();
                text.color = new Color(1, 1, 0, 1);
            }
            else
            {
                shopItem.DisableInteraction();
                text.color = new Color(1, 0.25f, 0, 1);
            }
        }
        else
        {
            shopItem.DisableInteraction();
        }
    }

    public void Purchase()
    {
        MainManager.Instance.playerData.coins -= cost;
        Destroy(gameObject);
    }

    public void ResetHomePosition()
    {
        shopItem.ResetHomePosition(transform.position);
    }
}

public interface IShopItem
{
    public void EnableInteraction();
    public void DisableInteraction();

    public int GetCost();

    public void SetShopItemScript(ShopItemScript sis);

    public void ResetHomePosition(Vector3 homePosition);
}