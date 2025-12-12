using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceMovePanelTrailScript : MonoBehaviour
{
    public Image image;

    public void Setup(Vector3 start, Vector3 end, Color c)
    {
        image.rectTransform.localPosition = (start + end) / 2;

        //atan(1) = 45

        //0 = up
        //45 = up left
        image.rectTransform.localEulerAngles = Vector3.forward * (180 / Mathf.PI) * Mathf.Atan2(start.x - end.x, end.y - start.y);
        image.rectTransform.sizeDelta = new Vector3(1, (end - start).magnitude, 1);
        image.color = c;
    }
}
