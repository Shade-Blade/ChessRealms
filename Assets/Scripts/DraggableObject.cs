using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDragEventListener
{
    public void OnDragStart();
    public void OnDragStay();
    public void OnDragStop();
}

public class DraggableObject : MonoBehaviour
{
    public bool isDragged;
    public byte dragOut;
    public Vector3 dragOffset;
    public float dragZ;
    public float normalZ;

    public bool canDrag;

    public List<IDragEventListener> listeners;

    public void Start()
    {
        listeners = new List<IDragEventListener>(GetComponents<IDragEventListener>());
        canDrag = true;
    }

    public void OnMouseDown()
    {
        if (canDrag)
        {
            isDragged = true;
            MainManager.Instance.currentDragged = this;
            dragOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            OnDragStart();
            dragOut = 2;
        }
    }

    public void OnMouseDrag()
    {
        dragOut = 2;
    }

    public void Update()
    {
        if (dragOut <= 0 || MainManager.Instance.currentDragged != this || !canDrag)
        {
            if (isDragged)
            {
                OnDragStop();
            }
            isDragged = false;
            dragOut = 0;
        } else
        {
            dragOut--;
        }

        if (isDragged)
        {
            transform.position = MainManager.XYProjectPreserve(Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragOffset) + Vector3.forward * dragZ;
            OnDragStay();
        } else
        {
            transform.position = transform.position + Vector3.back * transform.position.z + Vector3.forward * normalZ;
        }
    }

    public void OnDragStart()
    {
        foreach (IDragEventListener l in listeners)
        {
            l.OnDragStart();
        }
    }
    public void OnDragStay()
    {
        foreach (IDragEventListener l in listeners)
        {
            l.OnDragStay();
        }
    }
    public void OnDragStop()
    {
        foreach (IDragEventListener l in listeners)
        {
            l.OnDragStop();
        }
    }
}
