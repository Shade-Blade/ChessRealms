using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public SpriteRenderer arrowhead;
    public SpriteRenderer circle;
    public LineRenderer line;

    public int fx;
    public int fy;
    public int tx;
    public int ty;

    public void Set(int fx, int fy, int tx, int ty)
    {
        this.fx = fx;
        this.fy = fy;
        this.tx = tx;
        this.ty = ty;

        if (fx == tx && fy == ty)
        {
            circle.enabled = true;
            arrowhead.enabled = false;
            line.enabled = false;
            transform.localPosition = BoardScript.GetSpritePositionFromCoordinates(tx, ty, -0.8f);
        } else
        {
            circle.enabled = false;
            arrowhead.enabled = true;
            line.enabled = true;
            Vector3[] positions = new Vector3[2];
            positions[0] = BoardScript.GetSpritePositionFromCoordinates(fx, fy, -0.8f);
            positions[1] = BoardScript.GetSpritePositionFromCoordinates(tx, ty, -0.8f);
            Vector3 delta = positions[1] - positions[0];
            positions[1] -= delta.normalized * 0.5f;
            line.SetPositions(positions);
            line.positionCount = 2;
            arrowhead.transform.localEulerAngles = Vector3.forward * (180 / Mathf.PI) * Mathf.Atan2(-delta.x, delta.y);
            transform.localPosition = positions[1];
        }
    }

    public void Set(int tx, int ty)
    {
        this.tx = tx;
        this.ty = ty;

        if (fx == tx && fy == ty)
        {
            circle.enabled = true;
            arrowhead.enabled = false;
            line.enabled = false;
            transform.localPosition = BoardScript.GetSpritePositionFromCoordinates(tx, ty, -0.8f);
        }
        else
        {
            circle.enabled = false;
            arrowhead.enabled = true;
            line.enabled = true;
            Vector3[] positions = new Vector3[2];
            positions[0] = BoardScript.GetSpritePositionFromCoordinates(fx, fy, -0.8f);
            positions[1] = BoardScript.GetSpritePositionFromCoordinates(tx, ty, -0.8f);
            Vector3 delta = positions[1] - positions[0];
            positions[1] -= delta.normalized * 0.5f;
            line.SetPositions(positions);
            line.positionCount = 2;
            arrowhead.transform.localEulerAngles = Vector3.forward * (180 / Mathf.PI) * Mathf.Atan2(-delta.x, delta.y);
            transform.localPosition = positions[1];
        }
    }
}
