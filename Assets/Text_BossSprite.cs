using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_BossSprite : Text_SpecialSprite
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
        GameObject obj = Instantiate(MainManager.Instance.text_BossSprite);
        Text_BossSprite its = obj.GetComponent<Text_BossSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.itemSprite = GetBossSprite(args[0]);
            its.baseBox.sprite = its.itemSprite;
        }
        else
        {
            its.itemSprite = null;
            its.baseBox.sprite = its.itemSprite;
        }

        return obj;
    }

    public static Sprite GetBossSprite(string boss)
    {
        //Debug.Log("Display " + badge);
        Board.EnemyModifier bossType;

        Enum.TryParse(boss, true, out bossType);

        if (MainManager.PopCount((ulong)bossType) > 1)
        {
            return null;
        }

        //random failsafe thing I guess
        if ((int)bossType < 1)
        {
            Debug.Log("Parse fail: " + boss);
            return MainManager.Instance.bossSprites[MainManager.Instance.bossSprites.Length - 1];
        }

        return MainManager.Instance.bossSprites[(int)(MainManager.PopBitboardLSB1((ulong)bossType)) - 1];
    }
}
