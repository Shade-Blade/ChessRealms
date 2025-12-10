using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeScript : MonoBehaviour, ISelectEventListener, IDragEventListener, IShopItem
{
    Vector3 homePos;

    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;
    public BoxCollider bc;

    public BoardScript bs;

    public DraggableObject dob;
    public SelectableObject seb;

    public Board.PlayerModifier pm;

    public int badgeIndex = -1;
    public ConsumableBadgePanelScript cps;
    public TrashCanScript trashCan;

    public bool canInteract;
    public ShopItemScript sis;

    public void SetShopItemScript(ShopItemScript sis)
    {
        this.sis = sis;
    }
    public virtual void Start()
    {
        homePos = transform.position;
        dob = GetComponent<DraggableObject>();
        dob.canDrag = true;
        canInteract = true;

        //bad architecture?
        //But I don't really know of a better way
        cps = FindObjectOfType<ConsumableBadgePanelScript>();
        trashCan = FindObjectOfType<TrashCanScript>();

        Setup(pm);
    }

    public void Setup(Board.PlayerModifier pm)
    {
        this.pm = pm;

        if (pm == 0)
        {
            text.enabled = false;
            backSprite.enabled = false;
            bc.enabled = false;
            return;
        }
        text.enabled = true;
        backSprite.enabled = true;
        bc.enabled = true;

        text.text = Board.GetPlayerModifierName(pm);
    }

    public virtual void ForceDeselect()
    {
        Debug.Log("Force unselect");
        seb.ForceDeselect();
        dob.isDragged = false;
        ResetPosition();
    }

    public void OnSelect()
    {
        backSprite.color = new Color(1, 1, 1, 1);
        backSprite.color = new Color(1 - backSprite.color.r, backSprite.color.g, backSprite.color.b, 1);

        if (bs != null)
        {
            bs.SelectBadge(this);
        }
    }

    public void OnDeselect()
    {
        backSprite.color = new Color(1, 1, 1, 1);
        if (bs != null && (bs.selectedBadge == null || bs.selectedBadge == this))
        {
            if (bs.selectedPiece == null && bs.selectedConsumable == null)
            {
                bs.ResetSelected(false);
            }
        }
    }

    public void ResetPosition()
    {
        transform.position = homePos;
    }

    public int GetCost()
    {
        return Board.GetPlayerModifierCost(pm);
    }

    public void OnDragStart()
    {
    }
    public void OnDragStay()
    {       
        if (trashCan != null && badgeIndex >= 0 && bs != null && bs.setupMoves)
        {
            trashCan.text.text = "Sell: $" + GetCost();
            trashCan.SetActive();
        }
        if (cps != null)
        {
            if (cps.QueryPositionBadge(transform.position) != -1)
            {
                cps.badgeSlots[cps.QueryPositionBadge(transform.position)].SetHighlight();
                return;
            }
        }

        if (trashCan != null && badgeIndex >= 0 && bs != null && bs.setupMoves)
        {
            if (trashCan.QueryPosition(transform.position))
            {
                trashCan.SetHighlight();
            }
        }

        if (bs is BattleBoardScript bbs && bbs.animating && seb.selected)
        {
            ForceDeselect();
        }
    }
    public virtual void OnDragStop()
    {
        if (cps.QueryPositionBadge(transform.position) != -1)
        {
            //Consumable panel rearranging
            if (cps.TryPlaceBadge(this, cps.QueryPositionBadge(transform.position)))
            {
                if (sis != null)
                {
                    sis.Purchase();
                }
            }
        }
        else if (trashCan.QueryPosition(transform.position) && badgeIndex != -1 && bs != null && bs.setupMoves)
        {
            //Trash can
            cps.TryDeleteBadge(this);
            bs.ResetSelected();
        }
        ResetPosition();
        cps.FixInventory();
    }

    public virtual void Update()
    {
        dob.canDrag = bs.CanSelectPieces() && canInteract;
        bc.enabled = bs.CanSelectPieces() && text.enabled;
    }

    public void EnableInteraction()
    {
        canInteract = true;
    }

    public void DisableInteraction()
    {
        canInteract = false;
    }
}
