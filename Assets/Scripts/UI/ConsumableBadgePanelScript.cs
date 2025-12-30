using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableBadgePanelScript : MonoBehaviour
{
    public List<ConsumableSlotScript> consumableSlots;
    public List<BadgeSlotScript> badgeSlots;
    public List<ConsumableScript> consumables;
    public List<BadgeScript> badges;

    public GameObject consumableTemplate;
    public GameObject badgeTemplate;

    public BoardScript bs;

    public void SetBoardScript(BoardScript bs)
    {
        this.bs = bs;
        for (int i = 0; i < consumables.Count; i++)
        {
            if (consumables[i] != null)
            {
                consumables[i].bs = bs;
            }
        }
        for (int i = 0; i < badges.Count; i++)
        {
            if (badges[i] != null)
            {
                badges[i].bs = bs;
            }
        }
    }

    public void Start()
    {
        for (int i = 0; i < consumableSlots.Count; i++)
        {
            consumables.Add(null);
        }
        for (int i = 0; i < badgeSlots.Count; i++)
        {
            badges.Add(null);
        }
        FixInventory();
    }

    public int QueryPositionConsumable(Vector3 position)
    {
        for (int i = 0; i < consumableSlots.Count; i++)
        {
            if (consumableSlots[i].QueryPosition(position))
            {
                return i;
            }
        }

        return -1;
    }
    public int QueryPositionBadge(Vector3 position)
    {
        for (int i = 0; i < badgeSlots.Count; i++)
        {
            if (badgeSlots[i].QueryPosition(position))
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryPlaceConsumable(ConsumableScript cs, int i)
    {
        if (MainManager.Instance.playerData.consumables[i] == Move.ConsumableMoveType.None)
        {
            //Place it in the slot
            if (cs.consumableIndex >= 0)
            {
                MainManager.Instance.playerData.consumables[cs.consumableIndex] = Move.ConsumableMoveType.None;
                MainManager.Instance.playerData.consumablesDisabled[i] = MainManager.Instance.playerData.consumablesDisabled[cs.consumableIndex];
                MainManager.Instance.playerData.consumablesDisabled[cs.consumableIndex] = false;
            } else
            {
                MainManager.Instance.playerData.consumablesDisabled[i] = false;
            }
            MainManager.Instance.playerData.consumables[i] = cs.cmt;
            return true;
        }
        else
        {
            //try to swap?
            if (cs.consumableIndex >= 0)
            {
                MainManager.Instance.playerData.consumables[cs.consumableIndex] = MainManager.Instance.playerData.consumables[i];
                bool csdisabled = MainManager.Instance.playerData.consumablesDisabled[cs.consumableIndex];
                MainManager.Instance.playerData.consumablesDisabled[cs.consumableIndex] = MainManager.Instance.playerData.consumablesDisabled[i];
                MainManager.Instance.playerData.consumablesDisabled[i] = csdisabled;
                MainManager.Instance.playerData.consumables[i] = cs.cmt;
                return true;
            } else
            {
                //give up
                return false;
            }
        }
    }
    public bool TryPlaceBadge(BadgeScript bs, int i)
    {
        if (MainManager.Instance.playerData.badges[i] == 0)
        {
            //Place it in the slot
            if (bs.badgeIndex >= 0)
            {
                MainManager.Instance.playerData.badges[bs.badgeIndex] = 0;
            }
            MainManager.Instance.playerData.badges[i] = bs.pm;
            return true;
        }
        else
        {
            //try to swap?
            if (bs.badgeIndex >= 0)
            {
                MainManager.Instance.playerData.badges[bs.badgeIndex] = MainManager.Instance.playerData.badges[i];
                MainManager.Instance.playerData.badges[i] = bs.pm;
                return true;
            }
            else
            {
                //give up
                return false;
            }
        }
    }

    public void TryDeleteConsumable(ConsumableScript cs)
    {
        MainManager.Instance.playerData.consumables[cs.consumableIndex] = Move.ConsumableMoveType.None;
        MainManager.Instance.playerData.consumablesDisabled[cs.consumableIndex] = false;

        //Todo: Consumable data table to determine what consumables cost
        MainManager.Instance.playerData.coins += Board.GetConsumableCost(cs.cmt);
    }
    public void TryDeleteBadge(BadgeScript bs)
    {
        MainManager.Instance.playerData.badges[bs.badgeIndex] = 0;

        //Todo: Badge data table to determine what badges cost
        MainManager.Instance.playerData.coins += Board.GetPlayerModifierCost(bs.pm);
    }

    public void FixInventory()
    {
        for (int i = 0; i < consumables.Count; i++)
        {
            if (consumables[i] != null)
            {
                Destroy(consumables[i].gameObject);
            }
        }
        for (int i = 0; i < badges.Count; i++)
        {
            if (badges[i] != null)
            {
                Destroy(badges[i].gameObject);
            }
        }

        //make the ones in the correct slots
        for (int i = 0; i < MainManager.Instance.playerData.consumables.Length; i++)
        {
            if (MainManager.Instance.playerData.consumables[i] != Move.ConsumableMoveType.None)
            {
                //make a consumable
                GameObject go = Instantiate(consumableTemplate, consumableSlots[i].transform);
                consumables[i] = go.GetComponent<ConsumableScript>();
                consumables[i].disabled = MainManager.Instance.playerData.consumablesDisabled[i];
                consumables[i].Setup(MainManager.Instance.playerData.consumables[i]);
                consumables[i].consumableIndex = i;
                consumables[i].bs = bs;
            }
        }

        //make the ones in the correct slots
        for (int i = 0; i < MainManager.Instance.playerData.badges.Length; i++)
        {
            if (MainManager.Instance.playerData.badges[i] != 0)
            {
                //make a consumable
                GameObject go = Instantiate(badgeTemplate, badgeSlots[i].transform);
                badges[i] = go.GetComponent<BadgeScript>();
                badges[i].Setup(MainManager.Instance.playerData.badges[i]);
                badges[i].badgeIndex = i;
                badges[i].bs = bs;
            }
        }
    }
}
