using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UndoButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private bool isHeld = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Prevents the button from being "stuck" down if the player drags their finger/mouse away
        isHeld = false;
    }

    public bool GetHeld()
    {
        return isHeld;
    }
}
