using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Move;

public class SquareScript : MonoBehaviour
{
    public int x;
    public int y;

    public BoardScript bs;

    public GameObject auraTemplate;
    public List<GameObject> auraList;
    public List<SpriteRenderer> auraListSprites;

    public SpriteRenderer image;
    public SpriteRenderer lastMovedHighlight;
    public SpriteRenderer moveHighlight;
    public SpriteRenderer giantMoveHighlight;
    public GameObject windDelta;
    public SpriteRenderer windArrow;
    public SpriteRenderer squareEffect;

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
    public Piece.Aura wAura;
    public Piece.Aura bAura;

    public float lifetime;

    //ehh
    public List<Sprite> effectSprites;
    public List<Material> effectMaterials;

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
        lifetime += Time.deltaTime;
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
        if (isHover)
        {
            HoverTextMasterScript.Instance.SetHoverText(GetHoverText());
        }

        lastIsHover = isHover;
        isHover = false;

        //debug
        if (sq.type == Square.SquareType.Normal)
        {
            image.color = isBlack ? bs.squareColorBlack : bs.squareColorWhite;
        }

        for (int i = 0; i < auraListSprites.Count; i++)
        {
            Color c = auraListSprites[i].color;
            //add in a bit of offset so multiple auras can be distinguished from one that happens to be the same color
            //Future thing to add is aura textures but this will also make those stand out better
            c.a = 0.7f + 0.2f * Mathf.Sin(lifetime + i);
            auraListSprites[i].color = c;
        }
    }

    public void HoverStart()
    {
        if (sq.type != Square.SquareType.Normal)
        {
            Debug.Log("Hover start " + sq.type);
        }
    }
    public void HoverStay()
    {

    }
    public void HoverStop()
    {

    }

    public void ResetAura()
    {
        for (int i = 0; i < auraList.Count; i++)
        {
            Destroy(auraList[i]);
        }
        auraList = new List<GameObject>();
        auraListSprites = new List<SpriteRenderer>();

        for (int i = 0; i < 32; i++)
        {
            int bitIndex = 1 << i;

            if (((int)wAura & bitIndex) != 0)
            {
                GameObject auraObject = Instantiate(auraTemplate, transform);
                Color c = Piece.GetAuraColor((Piece.Aura)bitIndex);
                c = Color.Lerp(c, new Color(0.8f, 1, 1, 1), 0.4f);
                SpriteRenderer auraSprite = auraObject.GetComponent<SpriteRenderer>();
                c.a = 0.45f;
                auraSprite.sprite = GetAuraSprite((Piece.Aura)bitIndex);
                auraSprite.material = GetAuraMaterial((Piece.Aura)bitIndex);
                auraSprite.color = c;
                auraObject.transform.localPosition = new Vector3(0, 0, -0.15f + (0.001f) * auraList.Count);
                auraListSprites.Add(auraSprite);
                auraList.Add(auraObject);
            }
            if (((int)bAura & bitIndex) != 0)
            {
                GameObject auraObject = Instantiate(auraTemplate, transform);
                Color c = Piece.GetAuraColor((Piece.Aura)bitIndex);
                c = Color.Lerp(c, new Color(0.2f, 0, 0, 1), 0.4f);
                SpriteRenderer auraSprite = auraObject.GetComponent<SpriteRenderer>();
                c.a = 0.45f;
                auraSprite.sprite = GetAuraSprite((Piece.Aura)bitIndex);
                auraSprite.material = GetAuraMaterial((Piece.Aura)bitIndex);
                auraSprite.color = c;
                auraObject.transform.localPosition = new Vector3(0, 0, -0.15f + (0.001f) * auraList.Count);
                auraListSprites.Add(auraSprite);
                auraList.Add(auraObject);
            }
        }

        for (int i = 0; i < auraListSprites.Count; i++)
        {
            Color c = auraListSprites[i].color;
            c.a = 0.5f + 0.1f * Mathf.Sin(lifetime);
            auraListSprites[i].color = c;
        }
    }
    public void SetAura(Piece.Aura wAura, Piece.Aura bAura) 
    {
        //no change
        if (this.wAura == wAura && this.bAura == bAura)
        {
            return;
        }

        this.wAura = wAura;
        this.bAura = bAura;
        ResetAura();
    }

    public Sprite GetSquareSprite(Square.SquareType square)
    {
        switch (square)
        {
            case Square.SquareType.Hole:
            case Square.SquareType.Normal:
                break;
            case Square.SquareType.Fire:
                return effectSprites[6];
            case Square.SquareType.Water:
                return effectSprites[22];
            case Square.SquareType.Rough:
                return effectSprites[15];
            case Square.SquareType.WindUp:
                return effectSprites[24];
            case Square.SquareType.WindDown:
                return effectSprites[24];
            case Square.SquareType.WindLeft:
                return effectSprites[23];
            case Square.SquareType.WindRight:
                return effectSprites[23];
            case Square.SquareType.Slippery:
                return effectSprites[16];
            case Square.SquareType.Bouncy:
                return effectSprites[2];
            case Square.SquareType.Bright:
                return effectSprites[3];
            case Square.SquareType.Promotion:
                return effectSprites[13];
            case Square.SquareType.Cursed:
                return effectSprites[4];
            case Square.SquareType.CaptureOnly:
                return effectSprites[10];
            case Square.SquareType.Frost:
                return effectSprites[7];
            case Square.SquareType.BronzeTreasure:
                return effectSprites[18];
            case Square.SquareType.SilverTreasure:
                return effectSprites[19];
            case Square.SquareType.GoldTreasure:
                return effectSprites[20];
        }
        return null;
    }
    public Material GetSquareMaterial(Square.SquareType square)
    {
        switch (square)
        {
            case Square.SquareType.WindUp:
                return effectMaterials[4];
            case Square.SquareType.WindDown:
                return effectMaterials[1];
            case Square.SquareType.WindLeft:
                return effectMaterials[2];
            case Square.SquareType.WindRight:
                return effectMaterials[3];
        }
        return effectMaterials[0];
    }
    public Sprite GetAuraSprite(Piece.Aura aura)
    {
        switch (aura)
        {
            case Piece.Aura.None:
                break;
            case Piece.Aura.Nullify:
                return effectSprites[12];
            case Piece.Aura.Banshee:
                return effectSprites[1];
            case Piece.Aura.Immobilizer:
                return effectSprites[11];
            case Piece.Aura.Attractor:
                return effectSprites[0];
            case Piece.Aura.Repulser:
                return effectSprites[14];
            case Piece.Aura.Harpy:
                return effectSprites[10];
            case Piece.Aura.Hag:
                return effectSprites[8];
            case Piece.Aura.Sloth:
                return effectSprites[17];
            case Piece.Aura.Watchtower:
                return effectSprites[21];
            case Piece.Aura.Fan:
                return effectSprites[5];
            case Piece.Aura.Hanged:
                return effectSprites[9];
            case Piece.Aura.Rough:
                return effectSprites[15];
            case Piece.Aura.Water:
                return effectSprites[22];
            case Piece.Aura.Immune:
                return effectSprites[25];
        }
        return null;
    }
    public Material GetAuraMaterial(Piece.Aura aura)
    {
        return effectMaterials[5];
    }

    public void ResetSquareColor()
    {
        //ResetAura();

        squareEffect.sprite = GetSquareSprite(sq.type);
        squareEffect.color = new Color(0, 0, 0, 0);
        squareEffect.material = GetSquareMaterial(sq.type);

        switch (sq.type)
        {
            case Square.SquareType.Hole:
                //Lack of a square :)
                image.color = new Color(0, 0, 0, 0);
                break;
            case Square.SquareType.Normal:
                image.color = isBlack ? bs.squareColorBlack : bs.squareColorWhite;
                //image.color = isBlack ? new Color(0.4f, 0.3f, 0.15f) : new Color(1, 0.9f, 0.7f);
                break;
            case Square.SquareType.Fire:
                image.color = isBlack ? new Color(0.5f, 0.3f, 0.1f) : new Color(1, 0.5f, 0.1f);
                squareEffect.color = new Color(1, 0.75f, 0.5f, 1);
                break;
            case Square.SquareType.Water:
                image.color = isBlack ? new Color(0.1f, 0.1f, 0.5f) : new Color(0.1f, 0.1f, 1f);
                squareEffect.color = new Color(0.5f, 0.5f, 1f, 1);
                break;
            case Square.SquareType.Rough:
                image.color = isBlack ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.6f, 0.6f, 0.6f);
                squareEffect.color = new Color(0.85f, 0.85f, 0.85f, 1);
                break;
            case Square.SquareType.WindUp:
                windDelta.transform.localEulerAngles = Vector3.back * 0;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                squareEffect.color = new Color(0.8f, 1f, 0.8f, 1);
                break;
            case Square.SquareType.WindDown:
                windDelta.transform.localEulerAngles = Vector3.back * 180;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                squareEffect.color = new Color(0.8f, 1f, 0.8f, 1);
                break;
            case Square.SquareType.WindLeft:
                windDelta.transform.localEulerAngles = Vector3.back * 270;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                squareEffect.color = new Color(0.8f, 1f, 0.8f, 1);
                break;
            case Square.SquareType.WindRight:
                windDelta.transform.localEulerAngles = Vector3.back * 90;
                windArrow.color = new Color(1, 1f, 1f);
                image.color = isBlack ? new Color(0.3f, 0.6f, 0.3f) : new Color(0.5f, 0.85f, 0.5f);
                squareEffect.color = new Color(0.8f, 1f, 0.8f, 1);
                break;
            case Square.SquareType.Slippery:
                image.color = isBlack ? new Color(0.1f, 0.5f, 0.5f) : new Color(0.1f, 1f, 1f);
                squareEffect.color = new Color(0.8f, 1f, 1f, 1);
                break;
            case Square.SquareType.Bouncy:
                image.color = isBlack ? new Color(0.5f, 0.1f, 0.5f) : new Color(1f, 0.1f, 1f);
                squareEffect.color = new Color(1f, 0.5f, 1f, 1);
                break;
            case Square.SquareType.Bright:
                image.color = isBlack ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.85f, 0.85f, 0.85f);
                squareEffect.color = new Color(0.85f, 0.85f, 1f, 1);
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
                squareEffect.color = new Color(0.75f, 0.5f, 1f, 1);
                break;
            case Square.SquareType.Cursed:
                image.color = isBlack ? new Color(0.15f, 0.1f, 0.2f) : new Color(0.25f, 0.2f, 0.3f);
                squareEffect.color = new Color(0.45f, 0.4f, 0.5f, 1);
                break;
            case Square.SquareType.CaptureOnly:
                image.color = isBlack ? new Color(0.5f, 0.1f, 0.1f) : new Color(1, 0.1f, 0.1f);
                squareEffect.color = new Color(1f, 0.5f, 0.5f, 1);
                break;
            case Square.SquareType.Frost:
                image.color = isBlack ? new Color(0.5f, 0.5f, 0.6f) : new Color(0.8f, 0.8f, 1f);
                squareEffect.color = new Color(0.8f, 1f, 1f, 1);
                break;
            case Square.SquareType.BronzeTreasure:
                image.color = isBlack ? new Color(0.3f, 0.2f, 0.1f) : new Color(0.6f, 0.3f, 0.1f);
                squareEffect.color = new Color(1f, 0.75f, 0.5f, 1);
                break;
            case Square.SquareType.SilverTreasure:
                image.color = isBlack ? new Color(0.5f, 0.5f, 0.6f) : new Color(0.9f, 0.9f, 1f);
                squareEffect.color = new Color(1f, 1f, 1f, 1);
                break;
            case Square.SquareType.GoldTreasure:
                image.color = isBlack ? new Color(0.5f, 0.4f, 0.1f) : new Color(1, 0.9f, 0.1f);
                squareEffect.color = new Color(1f, 0.6f, 0f, 1);
                break;
        }
    }

    public string GetHoverText()
    {
        string hoverText = "";
        if (Board.GetSquareTypeDescription(sq.type).Length > 0)
        {
            hoverText += "<outlinecolor,#c0c0c0>[S]</color></font> " + Board.GetSquareTypeDescription(sq.type);
        }

        //auras
        if (wAura != 0 && hoverText.Length > 0)
        {
            hoverText += "\n";
        }
        for (int i = 0; i < 32; i++)
        {
            int bitIndex = 1 << i;

            if (((int)wAura & bitIndex) != 0)
            {
                Piece.Aura a = (Piece.Aura)bitIndex;
                Color c = Piece.GetAuraColor(a);
                hoverText += "\n" + "<outlinecolor,#ffffff>[W]</color> <color," + MainManager.ColorToString(c) + ">" + Piece.GetAuraName(a) + "</color></font>: " + Piece.GetAuraDescription(a);
            }
        }
        if (bAura != 0 && hoverText.Length > 0)
        {
            hoverText += "\n";
        }
        for (int i = 0; i < 32; i++)
        {
            int bitIndex = 1 << i;
            if (((int)bAura & bitIndex) != 0)
            {
                Piece.Aura a = (Piece.Aura)bitIndex;
                Color c = Piece.GetAuraColor(a);
                hoverText += "\n" + "<outlinecolor,#808080>[B]</color> <color," + MainManager.ColorToString(c) + ">" + Piece.GetAuraName(a) + "</color></font>: " + Piece.GetAuraDescription(a);
            }
        }

        //hacky setup because I'm lazy but it works
        if (hoverText.Length > 0 && hoverText[0] == '\n')
        {
            return hoverText.Substring(1);
        }

        return hoverText;
    }

    public void HighlightTargetted()
    {
        Color oldColor = moveHighlight.color;

        if (oldColor.a == 0)
        {
            moveHighlight.color = new Color(1, 1, 0.8f, 0.8f);
            return;
        }

        moveHighlight.color = new Color(1 - oldColor.r / 2, 1 - oldColor.g / 4, 1 - oldColor.b / 2, 0.8f);
    }
    public void HighlightLegal(bool specialType, bool giant)
    {
        specialMoveType = specialType;

        SpriteRenderer moveHighlight = giant ? giantMoveHighlight : this.moveHighlight;
        if (showEnemyMove)
        {
            if (specialMoveType)
            {
                moveHighlight.color = new Color(0.6f, 0.55f, 0.4f, 0.8f);
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
    public void HighlightIllegal(bool specialType, bool giant)
    {
        specialMoveType = specialType;
        //"illegal" enemy moves = things that put you in check

        SpriteRenderer moveHighlight = giant ? giantMoveHighlight : this.moveHighlight;
        if (showEnemyMove)
        {
            if (specialMoveType)
            {
                moveHighlight.color = new Color(0.6f, 0.45f, 0.4f, 0.8f);
            }
            else
            {
                moveHighlight.color = new Color(0.6f, 0.4f, 0.4f, 0.8f);
            }
        }
        else
        {
            if (specialMoveType)
            {
                moveHighlight.color = new Color(1f, 0.5f, 0f, 0.8f);
            }
            else
            {
                moveHighlight.color = new Color(1f, 0f, 0f, 0.8f);
            }
        }
        isHighlightedIllegal = true;
    }
    public void ResetColor()
    {
        //image.color = isBlack ? blackColor : whiteColor;
        windArrow.color = new Color(0, 0, 0, 0);
        ResetSquareColor();
        moveHighlight.color = new Color(0, 0, 0, 0);
        giantMoveHighlight.color = new Color(0, 0, 0, 0);
        lastMovedHighlight.color = new Color(0, 0, 0, 0);
        isHighlightedLegal = false;
        isHighlightedIllegal = false;
        isLastMovedSquare = false;
        ResetDotColor();
    }
    public void ResetDotColor()
    {
        moveHighlight.color = new Color(0f, 0f, 0f, 0f);
        giantMoveHighlight.color = new Color(0, 0, 0, 0);
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
