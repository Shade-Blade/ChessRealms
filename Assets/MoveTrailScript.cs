using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveTrailScript : MonoBehaviour
{
    public LineRenderer lr;
    public List<MoveMetadata> trail;

    public void SetColorMove()
    {
        lr.startColor = new Color(0, 0, 0.6f, 0.5f);
        lr.endColor = new Color(0, 0, 1, 0.8f);
    }
    public void SetColorMoveIllegal()
    {
        lr.startColor = new Color(0.6f, 0.3f, 0, 0.5f);
        lr.endColor = new Color(1, 0.5f, 0, 0.8f);
        lr.widthMultiplier *= 1.25f;
    }
    public void SetColorMoveCheck()
    {
        lr.startColor = new Color(0.6f, 0, 0, 0.5f);
        lr.endColor = new Color(1, 0, 0, 0.8f);
        lr.widthMultiplier *= 0.8f;
    }
    public void SetColorMoveSecondary()
    {
        lr.startColor = new Color(0f, 0.6f, 0.6f, 0.5f);
        lr.endColor = new Color(0, 1, 1, 0.8f);
        lr.widthMultiplier *= 0.8f;
    }
    public void SetColorMoveStationary()
    {
        lr.startColor = new Color(0.45f, 0f, 0.6f, 0.5f);
        lr.endColor = new Color(0.75f, 0, 1, 0.8f);
    }

    public const float Z_COORD = -0.7f;

    public void Setup(int startX, int startY, List<MoveMetadata> trail)
    {
        this.trail = trail;

        List<Vector3> pointList = new List<Vector3>();

        int pastX = startX;
        int pastY = startY;

        Vector3 firstPos = BoardScript.GetSpritePositionFromCoordinates(startX, startY, Z_COORD);
        pointList.Add(firstPos);

        Vector3 pastPos = firstPos;

        for (int i = 0; i < trail.Count; i++)
        {
            Vector3 futurePos = BoardScript.GetSpritePositionFromCoordinates(trail[i].x, trail[i].y, Z_COORD);

            switch (trail[i].path)
            {
                case MoveMetadata.PathType.Slider:
                case MoveMetadata.PathType.SliderGiant:
                    if (pastX - trail[i].x < -4 || pastX - trail[i].x > 4)
                    {
                        CylinderWrapAroundSegment(pointList, pastPos, futurePos);
                    }
                    else if (pastY - trail[i].y < -4 || pastY - trail[i].y > 4)
                    {
                        TubularWrapAroundSegment(pointList, pastPos, futurePos);
                    }
                    break;
                case MoveMetadata.PathType.Teleport:
                case MoveMetadata.PathType.TeleportGiant:
                    if (pastX - trail[i].x > 1 || pastX - trail[i].x < -1 || pastY - trail[i].y > 1 || pastY - trail[i].y < -1)
                    {
                        AddTeleportSegment(pointList, pastPos, futurePos);
                    }
                    break;
                case MoveMetadata.PathType.Leaper:
                case MoveMetadata.PathType.LeaperGiant:
                    //length 1 leaper section = only slider segment
                    if (pastX - trail[i].x > 1 || pastX - trail[i].x < -1 || pastY - trail[i].y > 1 || pastY - trail[i].y < -1)
                    {
                        if (pastX - trail[i].x < -4 || pastX - trail[i].x > 4)
                        {
                            AddLeaperSegmentCylinder(pointList, pastPos, futurePos);
                        }
                        else if (pastY - trail[i].y < -4 || pastY - trail[i].y > 4)
                        {
                            AddLeaperSegmentTubular(pointList, pastPos, futurePos);
                        }
                        else
                        {
                            AddLeaperSegment(pointList, pastPos, futurePos);
                        }
                    }
                    break;
            }
            pointList.Add(futurePos);
            pastPos = futurePos;
            pastX = trail[i].x;
            pastY = trail[i].y;
        }

        switch (trail[0].path)
        {
            case MoveMetadata.PathType.SliderGiant:
            case MoveMetadata.PathType.LeaperGiant:
            case MoveMetadata.PathType.TeleportGiant:
                MakePathGiant(pointList);
                break;
        }

        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());
    }

    public void Setup(int startX, int startY, int endX, int endY)
    {
        Vector3[] pointList = new Vector3[2] { BoardScript.GetSpritePositionFromCoordinates(startX, startY, Z_COORD), BoardScript.GetSpritePositionFromCoordinates(endX, endY, Z_COORD) };

        lr.positionCount = pointList.Length;
        lr.SetPositions(pointList);
    }

    public void CylinderWrapAroundSegment(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        Vector3 pastPos = pointA;
        Vector3 futurePos = pointB;
        //cylindrical
        if (pointA.x < pointB.x)
        {
            //pastX = 0,1
            //trail[i].x = 6,7 or something
            Vector3 futureLeft = futurePos + Vector3.left * 8; //BoardScript.GetSpritePositionFromCoordinates(trail[i].x - 8, trail[i].y, -0.7f);
            Vector3 pastRight = pastPos + Vector3.right * 8; //BoardScript.GetSpritePositionFromCoordinates(pastX + 8, pastY, -0.7f);

            //lerp(oldy, newy, inverselerp(oldx, newx, (edge)))

            Vector3 midLeft = new Vector3(-4f, Mathf.Lerp(futureLeft.y, pastPos.y, Mathf.InverseLerp(futureLeft.x, pastPos.x, -4f)), -0.7f);
            Vector3 midRight = new Vector3(4f, Mathf.Lerp(futurePos.y, pastRight.y, Mathf.InverseLerp(futurePos.x, pastRight.x, 4f)), -0.7f);

            Debug.Log(futureLeft + " " + midLeft + " " + midRight + " " + pastRight);

            pointList.Add(midLeft);
            pointList.Add(midLeft + Vector3.back * 15);
            pointList.Add(midRight + Vector3.back * 15);
            pointList.Add(midRight);
        }
        else
        {
            //Opposite of above case
            Vector3 futureRight = futurePos + Vector3.right * 8; //BoardScript.GetSpritePositionFromCoordinates(trail[i].x + 8, trail[i].y, -0.7f);
            Vector3 pastLeft = pastPos + Vector3.left * 8; //BoardScript.GetSpritePositionFromCoordinates(pastX - 8, pastY, -0.7f);

            Vector3 midRight = new Vector3(4f, Mathf.Lerp(futureRight.y, pastPos.y, Mathf.InverseLerp(futureRight.x, pastPos.x, 4f)), -0.7f);
            Vector3 midLeft = new Vector3(-4f, Mathf.Lerp(futurePos.y, pastLeft.y, Mathf.InverseLerp(futurePos.x, pastLeft.x, -4f)), -0.7f);

            Debug.Log(pastLeft + " " + midLeft + " " + midRight + " " + futureRight);

            pointList.Add(midRight);
            pointList.Add(midRight + Vector3.back * 15);
            pointList.Add(midLeft + Vector3.back * 15);
            pointList.Add(midLeft);
        }
    }
    public void TubularWrapAroundSegment(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        Vector3 pastPos = pointA;
        Vector3 futurePos = pointB;
        if (pointA.y < pointB.y)
        {
            //pastY = 0,1
            //trail[i].y = 6,7 or something

            Vector3 futureDown = futurePos + Vector3.down * 8; //BoardScript.GetSpritePositionFromCoordinates(trail[i].x, trail[i].y - 8, -0.7f);
            Vector3 pastUp = pastPos + Vector3.up * 8; //BoardScript.GetSpritePositionFromCoordinates(pastX, pastY + 8, -0.7f);

            Vector3 midDown = new Vector3(Mathf.Lerp(futureDown.x, pastPos.x, Mathf.InverseLerp(futureDown.y, pastPos.y, -4f)), -4f, -0.7f);
            Vector3 midUp = new Vector3(Mathf.Lerp(futurePos.x, pastUp.x, Mathf.InverseLerp(futurePos.y, pastUp.y, 4f)), 4f, -0.7f);

            pointList.Add(midDown);
            pointList.Add(midDown + Vector3.back * 15);
            pointList.Add(midUp + Vector3.back * 15);
            pointList.Add(midUp);
        }
        else
        {
            Vector3 futureUp = futurePos + Vector3.up * 8; // BoardScript.GetSpritePositionFromCoordinates(trail[i].x, trail[i].y + 8, -0.7f);
            Vector3 pastDown = pastPos + Vector3.down * 8; //BoardScript.GetSpritePositionFromCoordinates(pastX, pastY - 8, -0.7f);

            Vector3 midUp = new Vector3(Mathf.Lerp(futureUp.x, pastPos.x, Mathf.InverseLerp(futureUp.y, pastPos.y, 4f)), 4f, -0.7f);
            Vector3 midDown = new Vector3(Mathf.Lerp(futurePos.x, pastDown.x, Mathf.InverseLerp(futurePos.y, pastDown.y, -4f)), -4f, -0.7f);

            pointList.Add(midUp);
            pointList.Add(midUp + Vector3.back * 15);
            pointList.Add(midDown + Vector3.back * 15);
            pointList.Add(midDown);
        }
    }

    public void AddTeleportSegment(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        int zigzags = Mathf.CeilToInt((pointA - pointB).magnitude / BoardScript.SQUARE_SIZE);

        zigzags *= 4;

        Vector3 perpendicular = Vector3.Cross(Vector3.forward, (pointA - pointB)).normalized * 0.6f;

        for (int i = 0; i < zigzags; i++)
        {
            if (i != 0)
            {
                pointList.Add((i % 2 == 0 ? 0 : (i % 4 > 1 ? 0.5f : -0.5f)) * perpendicular + Vector3.Lerp(pointA, pointB, i / (zigzags + 0f)));
            }
        }
    }

    public void AddLeaperSegment(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        int segments = Mathf.CeilToInt(5 * ((pointA - pointB).magnitude / BoardScript.SQUARE_SIZE));

        Vector3 perpendicular = Vector3.Cross(Vector3.forward, (pointA - pointB)).normalized * 0.6f;

        if (perpendicular.y < 0)
        {
            perpendicular = -perpendicular;
        }

        for (int i = 0; i < segments; i++)
        {
            float x = (i / (segments + 0f));
            float value = 4 * (-x * x + x);
            value *= (pointA - pointB).magnitude * 0.25f;

            if (i != 0)
            {
                pointList.Add(perpendicular * value + Vector3.Lerp(pointA, pointB, i / (segments + 0f)));
            }
        }
    }
    public void AddLeaperSegmentCylinder(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        int segments = Mathf.CeilToInt(5 * ((pointA - pointB).magnitude / BoardScript.SQUARE_SIZE));

        Vector3 perpendicular = Vector3.Cross(Vector3.forward, (pointA - pointB)).normalized * 0.6f;

        if (perpendicular.y < 0)
        {
            perpendicular = -perpendicular;
        }

        if (pointA.x > pointB.x)
        {
            pointB.x += 8;
        } else
        {
            pointB.x -= 8;
        }

        bool wrapAround = false;

        Vector3 pastPoint = pointA;

        for (int i = 0; i < segments; i++)
        {
            float x = (i / (segments + 0f));
            float value = 4 * (-x * x + x);
            value *= (pointA - pointB).magnitude * 0.25f;

            if (i != 0)
            {
                Vector3 subPoint = perpendicular * value + Vector3.Lerp(pointA, pointB, i / (segments + 0f));

                if (!wrapAround)
                {
                    if (subPoint.x > 4)
                    {
                        pointA.x -= 8;
                        pointB.x -= 8;
                        subPoint.x -= 8;
                        wrapAround = true;
                    }
                    if (subPoint.x < -4)
                    {
                        pointA.x += 8;
                        pointB.x += 8;
                        subPoint.x += 8;
                        wrapAround = true;
                    }

                    if (wrapAround)
                    {
                        CylinderWrapAroundSegment(pointList, pastPoint, subPoint);
                    }
                }

                pastPoint = subPoint;
                pointList.Add(subPoint);
            }
        }
    }

    public void AddLeaperSegmentTubular(List<Vector3> pointList, Vector3 pointA, Vector3 pointB)
    {
        int segments = Mathf.CeilToInt(5 * ((pointA - pointB).magnitude / BoardScript.SQUARE_SIZE));

        Vector3 perpendicular = Vector3.Cross(Vector3.forward, (pointA - pointB)).normalized * 0.6f;

        if (perpendicular.y < 0)
        {
            perpendicular = -perpendicular;
        }

        if (pointA.y > pointB.y)
        {
            pointB.y += 8;
        }
        else
        {
            pointB.y -= 8;
        }

        bool wrapAround = false;

        Vector3 pastPoint = pointA;

        for (int i = 0; i < segments; i++)
        {
            float x = (i / (segments + 0f));
            float value = 4 * (-x * x + x);
            value *= (pointA - pointB).magnitude * 0.25f;

            if (i != 0)
            {
                Vector3 subPoint = perpendicular * value + Vector3.Lerp(pointA, pointB, i / (segments + 0f));

                if (!wrapAround)
                {
                    if (subPoint.y > 4)
                    {
                        pointA.y -= 8;
                        pointB.y -= 8;
                        subPoint.y -= 8;
                        wrapAround = true;
                    }
                    if (subPoint.y < -4)
                    {
                        pointA.y += 8;
                        pointB.y += 8;
                        subPoint.y += 8;
                        wrapAround = true;
                    }

                    if (wrapAround)
                    {
                        TubularWrapAroundSegment(pointList, pastPoint, subPoint);
                    }
                }

                pastPoint = subPoint;
                pointList.Add(subPoint);
            }
        }
    }

    public void MakePathGiant(List<Vector3> pointList)
    {
        float offset = BoardScript.SQUARE_SIZE / 2f;

        lr.widthMultiplier *= 3f;

        for (int i = 0; i < pointList.Count; i++)
        {
            pointList[i] += new Vector3(offset, offset, 0);
        }
    }
}
