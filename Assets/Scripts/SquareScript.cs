using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Move;

public class SquareScript : MonoBehaviour
{
    public int x;
    public int y;

    public BoardScript bs;

    public SpriteRenderer image;
    public SpriteRenderer lastMovedHighlight;
    public SpriteRenderer moveHighlight;
    public GameObject windDelta;
    public SpriteRenderer windArrow;

    public Color whiteColor;
    public Color blackColor;

    public bool isBlack;

    public bool isHover;
    public bool lastIsHover;

    //Todo: make an enum based solution for all these things
    public bool showEnemyMove;
    public bool isHighlightedLegal;
    public bool isHighlightedIllegal;
    public bool specialMoveType;

    public bool isLastMovedSquare;

    public bool czhHighlight;   //The czh data is hard to obtain mid turn so I will keep the on or off separately
    public bool czhWhite;
    public bool czhBlack;

    public Square sq;

    public void Setup(int x, int y, Square sq)
    {
        isBlack = (x + y) % 2 == 0;
        this.x = x;
        this.y = y;

        image.color = isBlack ? blackColor : whiteColor;
        this.sq = sq;
    }

    public void OnMouseOver()
    {
        isHover = true;
    }

    public void Update()
    {
        if (lastIsHover ^ isHover)
        {
            if (isHover)
            {
                HoverStart();
            } else
            {
                HoverStop();
            }
        }

        lastIsHover = isHover;
        isHover = false;
    }

    public void HoverStart()
    {
    }

    public void HoverStop()
    {

    }

    public void ResetSquareColor()
    {
        switch (sq.type)
        {
            case Square.SquareType.Hole:
                //Pure black
                image.color = new Color(0, 0, 0, 1);
                break;
            case Square.SquareType.Normal:
                image.color = isBlack ? new Color(0.4f, 0.3f, 0.15f) : new Color(1, 0.9f, 0.7f);
                break;
            case Square.SquareType.Fire:
                image.color = isBlack ? new Color(0.5f, 0.3f, 0.1f) : new Color(1, 0.5f, 0.1f);
                break;
            case Square.SquareType.Water:
                image.color = isBlack ? new Color(0.1f, 0.1f, 0.5f) : new Color(0.1f, 0.1f, 1f);
                break;
            case Square.SquareType.Rough:
                image.color = isBlack ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.6f, 0.6f, 0.6f);
                break;
            case Square.SquareType.WindUp:
                windDelta.transform.localEulerAngles = Vector3.back * 0;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                break;
            case Square.SquareType.WindDown:
                windDelta.transform.localEulerAngles = Vector3.back * 180;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                break;
            case Square.SquareType.WindLeft:
                windDelta.transform.localEulerAngles = Vector3.back * 270;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                break;
            case Square.SquareType.WindRight:
                windDelta.transform.localEulerAngles = Vector3.back * 90;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                break;
            case Square.SquareType.Slippery:
                image.color = isBlack ? new Color(0.1f, 0.5f, 0.5f) : new Color(0.1f, 1f, 1f);
                break;
            case Square.SquareType.Bouncy:
                image.color = isBlack ? new Color(0.5f, 0.1f, 0.5f) : new Color(1f, 0.1f, 1f);
                break;
            case Square.SquareType.Bright:
                image.color = isBlack ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.85f, 0.85f, 0.85f);
                break;
            case Square.SquareType.Promotion:
                //white side promotion square vs black side promotion square
                if (y > 3)
                {
                    image.color = isBlack ? new Color(0.6f, 0.4f, 0.8f) : new Color(0.8f, 0.6f, 1f);
                }
                else
                {
                    image.color = isBlack ? new Color(0.2f, 0.1f, 0.3f) : new Color(0.4f, 0.3f, 0.5f);
                }
                break;
            case Square.SquareType.Cursed:
                image.color = isBlack ? new Color(0.15f, 0.1f, 0.2f) : new Color(0.25f, 0.2f, 0.3f);
                break;
            case Square.SquareType.CaptureOnly:
                image.color = isBlack ? new Color(0.5f, 0.1f, 0.1f) : new Color(1, 0.1f, 0.1f);
                break;
            case Square.SquareType.Frost:
                image.color = isBlack ? new Color(0.5f, 0.5f, 0.1f) : new Color(1, 1f, 0.1f);
                break;
            case Square.SquareType.BronzeTreasure:
                image.color = isBlack ? new Color(0.3f, 0.2f, 0.1f) : new Color(0.6f, 0.3f, 0.1f);
                break;
            case Square.SquareType.SilverTreasure:
                image.color = isBlack ? new Color(0.5f, 0.5f, 0.6f) : new Color(0.9f, 0.9f, 1f);
                break;
            case Square.SquareType.GoldTreasure:
                image.color = isBlack ? new Color(0.5f, 0.4f, 0.1f) : new Color(1, 0.9f, 0.1f);
                break;
        }
    }

    public void HighlightLegal(bool specialType)
    {
        specialMoveType = specialType;
        if (showEnemyMove)
        {
            if (specialMoveType)
            {
                moveHighlight.color = new Color(0.6f, 0.5f, 0.4f, 0.8f);
            }
            else
            {
                moveHighlight.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }
        }
        else
        {
            if (specialMoveType)
            {
                moveHighlight.color = new Color(0f, 1f, 1f, 0.8f);
            }
            else
            {
                moveHighlight.color = new Color(0f, 0f, 1f, 0.8f);
            }
        }
        isHighlightedLegal = true;
    }
    public void HighlightIllegal(bool specialType)
    {
        specialMoveType = specialType;
        //"illegal" enemy moves = things that put you in check
        if (showEnemyMove)
        {
            moveHighlight.color = new Color(0.6f, 0.4f, 0.4f, 0.8f);
        }
        else
        {
            moveHighlight.color = new Color(1f, 0f, 0f, 0.8f);
        }
        isHighlightedIllegal = true;
    }
    public void ResetColor()
    {
        //image.color = isBlack ? blackColor : whiteColor;
        windArrow.color = new Color(0, 0, 0, 0);
        ResetSquareColor();
        moveHighlight.color = new Color(0, 0, 0, 0);
        lastMovedHighlight.color = new Color(0, 0, 0, 0);
        isHighlightedLegal = false;
        isHighlightedIllegal = false;
        isLastMovedSquare = false;
        ResetDotColor();
    }
    public void ResetDotColor()
    {
        moveHighlight.color = new Color(0f, 0f, 0f, 0f);
        if (czhHighlight)
        {
            if (czhWhite)
            {
                moveHighlight.color = new Color(0.6f, 0.6f, 1f, 0.5f);
            }

            if (czhBlack)
            {
                moveHighlight.color = new Color(1f, 0.6f, 0.6f, 0.5f);
                if (czhWhite)
                {
                    moveHighlight.color = new Color(1f, 0.6f, 1f, 0.5f);
                }
            }
        }

        if (isLastMovedSquare)
        {
            lastMovedHighlight.color = new Color(0, 0, 1f, 0.8f);
        }
    }

    public void SetLastMovedSquare()
    {
        isLastMovedSquare = true;
        lastMovedHighlight.color = new Color(0, 0, 1f, 0.6f);
    }
}
