using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoverEventListener
{
    public void OnHoverStart();
    public void OnHoverStop();

    //less necessary
    public void OnHoverStay() { }
}

public class HoverableObject : MonoBehaviour
{
    public bool isHover;
    public bool lastIsHover;

    public List<IHoverEventListener> listeners;

    public void Start()
    {
        listeners = new List<IHoverEventListener>(GetComponents<IHoverEventListener>());
    }

    public void OnMouseOver()
    {
        isHover = true;
    }

    public void Update()
    {
        if (isHover && !lastIsHover)
        {
            OnHoverStart();
        }
        if (isHover)
        {
            OnHoverStay();
        }
        if (!isHover && lastIsHover)
        {
            OnHoverStop();
        }
        lastIsHover = isHover;
        isHover = false;
    }

    public void OnHoverStart()
    {
        foreach (IHoverEventListener l in listeners)
        {
            l.OnHoverStart();
        }
    }
    public void OnHoverStay()
    {
        foreach (IHoverEventListener l in listeners)
        {
            l.OnHoverStay();
        }
    }
    public void OnHoverStop()
    {
        foreach (IHoverEventListener l in listeners)
        {
            l.OnHoverStop();
        }
    }
}
