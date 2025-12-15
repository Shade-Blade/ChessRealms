using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_ConsumableSprite : Text_SpecialSprite
{
    //text width number
    public static string GetWidth(string b)
    {
        //use em
        return (WIDTH) + "em";
    }

    //public string[] args;
    //public int index;
    public bool visible;

    public const float WIDTH = 1.5f;

    public Sprite itemSprite;

    //build an item sprite with given args
    //position is handled by the thing that makes the item sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_ConsumableSprite);
        Text_ConsumableSprite its = obj.GetComponent<Text_ConsumableSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.itemSprite = GetConsumableSprite(args[0]);
            its.baseBox.sprite = its.itemSprite;
        }
        else
        {
            its.itemSprite = null;
            its.baseBox.sprite = its.itemSprite;
        }

        return obj;
    }

    public static Sprite GetConsumableSprite(string consumable)
    {
        //Debug.Log("Display " + badge);
        Move.ConsumableMoveType consumableType;

        Enum.TryParse(consumable, true, out consumableType);

        //random failsafe thing I guess
        if ((int)consumableType < 1)
        {
            Debug.Log("Parse fail: " + consumable);
            return MainManager.Instance.consumableSprites[MainManager.Instance.consumableSprites.Length - 1];
        }

        return GetConsumableSprite(consumableType);
    }

    public static Sprite GetConsumableSprite(Move.ConsumableMoveType cmt)
    {
        return MainManager.Instance.consumableSprites[(int)(cmt) - 1];
    }
}