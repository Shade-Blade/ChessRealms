using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemScript : MonoBehaviour
{
    public int cost;
    public IShopItem shopItem;
    public TMPro.TMP_Text text;

    public void Start()
    {
        if (shopItem == null)
        {
            shopItem = GetComponentInChildren<IShopItem>();
            shopItem.SetShopItemScript(this);
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
        if (MainManager.Instance.playerData.coins >= cost)
        {
            shopItem.EnableInteraction();
        } else
        {
            shopItem.DisableInteraction();
        }
    }

    public void Purchase()
    {
        MainManager.Instance.playerData.coins -= cost;
        Destroy(gameObject);
    }
}

public interface IShopItem
{
    public void EnableInteraction();
    public void DisableInteraction();

    public int GetCost();

    public void SetShopItemScript(ShopItemScript sis);
}