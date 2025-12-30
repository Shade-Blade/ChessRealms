using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceMovePanelTrailScript : MonoBehaviour
{
    public RectTransform trailBase;
    public List<Image> subImages;

    public void Setup(Vector3 start, Vector3 end, Color c)
    {
        trailBase.localPosition = (start + end) / 2;

        //atan(1) = 45

        //0 = up
        //45 = up left
        trailBase.localEulerAngles = Vector3.forward * (180 / Mathf.PI) * Mathf.Atan2(start.x - end.x, end.y - start.y);
        trailBase.sizeDelta = new Vector3(1, (end - start).magnitude, 1);
        for (int i = 0; i < subImages.Count; i++)
        {
            subImages[i].color = c;
        }
    }
}
