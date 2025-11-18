using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumablePanelScript : MonoBehaviour
{
    public List<ConsumableSlotScript> slots;
    public List<ConsumableScript> consumables;

    public GameObject consumableTemplate;

    public BoardScript bs;

    public void Start()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            consumables.Add(null);
        }
        FixInventory();
    }

    public int QueryPosition(Vector3 position)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].QueryPosition(position))
            {
                return i;
            }
        }

        return -1;
    }

    public void TryPlaceConsumable(ConsumableScript cs, int i)
    {
        if (MainManager.Instance.playerData.consumables[i] == Move.ConsumableMoveType.None)
        {
            //Place it in the slot
            if (cs.consumableIndex >= 0)
            {
                MainManager.Instance.playerData.consumables[cs.consumableIndex] = Move.ConsumableMoveType.None;
            }
            MainManager.Instance.playerData.consumables[i] = cs.cmt;
        }
        else
        {
            //try to swap?
            if (cs.consumableIndex >= 0)
            {
                MainManager.Instance.playerData.consumables[cs.consumableIndex] = MainManager.Instance.playerData.consumables[i];
                MainManager.Instance.playerData.consumables[i] = cs.cmt;
            } else
            {
                //give up
                return;
            }
        }
    }

    public void TryDeleteConsumable(ConsumableScript cs)
    {
        MainManager.Instance.playerData.consumables[cs.consumableIndex] = Move.ConsumableMoveType.None;
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

        //make the ones in the correct slots
        for (int i = 0; i < MainManager.Instance.playerData.consumables.Length; i++)
        {
            if (MainManager.Instance.playerData.consumables[i] != Move.ConsumableMoveType.None)
            {
                //make a consumable
                GameObject go = Instantiate(consumableTemplate, slots[i].transform);
                consumables[i] = go.GetComponent<ConsumableScript>();
                consumables[i].Setup(MainManager.Instance.playerData.consumables[i]);
                consumables[i].consumableIndex = i;
                consumables[i].bs = bs;
            }
        }
    }
}
