using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectEventListener
{
    public void OnSelect();
    public void OnDeselect();
}

public class SelectableObject : MonoBehaviour
{
    public List<ISelectEventListener> listeners;

    public bool selected;
    public bool lastSelected;

    public void Start()
    {
        listeners = new List<ISelectEventListener>(GetComponents<ISelectEventListener>());
    }

    public void OnMouseDown()
    {
        MainManager.Instance.currentSelected = this;
        MainManager.Instance.sameFrameSelected = true;
        selected = true;
    }

    public void Update()
    {
        if (lastSelected ^ selected)
        {
            if (selected)
            {
                OnSelect();
            } else
            {
                OnDeselect();
            }
        }

        lastSelected = selected;
        if (MainManager.Instance.currentSelected != this)
        {
            selected = false;
        }
    }

    public void ForceSelected()
    {
        if (!selected)
        {
            OnSelect();
            selected = true;
            lastSelected = true;
        }
    }
    public void ForceDeselect()
    {
        if (selected)
        {
            OnDeselect();
            selected = false;
            lastSelected = false;
        }
    }

    public void OnSelect()
    {
        foreach (ISelectEventListener l in listeners)
        {
            l.OnSelect();
        }
    }
    public void OnDeselect()
    {
        foreach (ISelectEventListener l in listeners)
        {
            l.OnDeselect();
        }
    }
}
