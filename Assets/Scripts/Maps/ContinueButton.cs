using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventContinueButton : MonoBehaviour
{
    public GameObject eventParent;
    public IEvent eventParentInterface;
    public bool active;

    public void Start()
    {
        eventParentInterface = eventParent.GetComponent<IEvent>();
    }

    public void OnMouseDown()
    {
        if (active)
        {
            return;
        }
        active = true;
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(eventParentInterface.FadeOut());
        FindObjectOfType<OverworldScript>().ReturnFromNode();
        Destroy(gameObject);
    }
}

public interface IEvent
{
    public IEnumerator FadeOut();
}