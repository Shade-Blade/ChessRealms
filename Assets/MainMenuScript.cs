using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public SpriteRenderer backgroundA;
    public SpriteRenderer backgroundB;

    // Update is called once per frame
    void Update()
    {
        float timeValue = Time.time * 0.2f;
        backgroundB.transform.localPosition = Vector3.up * 0.25f * (timeValue - Mathf.Ceil(timeValue));
    }
}
