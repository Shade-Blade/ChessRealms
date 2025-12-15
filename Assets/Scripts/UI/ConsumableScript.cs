using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableScript : MonoBehaviour, ISelectEventListener, IDragEventListener, IShopItem
{
    Vector3 homePos;

    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;
    public BoxCollider bc;

    public BoardScript bs;

    public DraggableObject dob;
    public SelectableObject seb;

    public Move.ConsumableMoveType cmt;

    public int consumableIndex = -1;
    public ConsumableBadgePanelScript cps;
    public TrashCanScript trashCan;

    public bool canInteract;

    public ShopItemScript sis;
    public GameObject selectObject;

    public void SetShopItemScript(ShopItemScript sis)
    {
        this.sis = sis;
    }
    public void ResetHomePosition(Vector3 homePos)
    {
        this.homePos = homePos;
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

        Setup(cmt);
    }

    public void Setup(Move.ConsumableMoveType cmt)
    {
        this.cmt = cmt;

        if (cmt == Move.ConsumableMoveType.None)
        {
            text.enabled = false;
            backSprite.enabled = false;
            bc.enabled = false;
            return;
        }
        text.enabled = true;
        backSprite.enabled = true;
        bc.enabled = true;

        backSprite.sprite = Text_ConsumableSprite.GetConsumableSprite(cmt);

        if (MainManager.Instance.pieceTextVisible)
        {
            text.text = Board.GetConsumableName(cmt);
        }
        else
        {
            text.text = "";
        }
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
        selectObject.SetActive(true);

        bs.SelectConsumable(this);
    }

    public void OnDeselect()
    {
        backSprite.color = new Color(1, 1, 1, 1);
        selectObject.SetActive(false);
        if (bs.selectedConsumable == null || bs.selectedConsumable == this)
        {
            if (bs.selectedPiece == null && bs.selectedBadge == null)
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
        return Board.GetConsumableCost(cmt);
    }

    public void OnDragStart()
    {
    }
    public void OnDragStay()
    {
        (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position);

        if (trashCan != null && consumableIndex >= 0)
        {
            trashCan.text.text = "Sell: $" + GetCost();
            trashCan.SetActive();
        }
        if (cps != null)
        {
            if (cps.QueryPositionConsumable(transform.position) != -1)
            {
                cps.consumableSlots[cps.QueryPositionConsumable(transform.position)].SetHighlight();
                return;
            }
        }

        if (trashCan != null && consumableIndex >= 0)
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
        if (cps.QueryPositionConsumable(transform.position) != -1)
        {
            //Consumable panel rearranging
            if (cps.TryPlaceConsumable(this, cps.QueryPositionConsumable(transform.position)))
            {
                if (sis != null)
                {
                    sis.Purchase();
                }
            }
        } else if (trashCan.QueryPosition(transform.position) && consumableIndex != -1)
        {
            //Trash can
            cps.TryDeleteConsumable(this);
            bs.ResetSelected();
        } else
        {
            if (bs == null)
            {
                return;
            }

            //Setup mode
            if (bs.setupMoves)
            {
                //Consumable moves are not allowed in setup mode
                ResetPosition();
                return;
            }

            if (bs.hoverX < 0 || bs.hoverX > 7)
            {
                ResetPosition();
                return;
            }

            if (bs.hoverY < 0 || bs.hoverY > 7)
            {
                ResetPosition();
                return;
            }

            if (bs is BattleBoardScript bbs)
            {
                bbs.TryConsumableMove(this, consumableIndex, (byte)bs.hoverX, (byte)bs.hoverY);
            }
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
