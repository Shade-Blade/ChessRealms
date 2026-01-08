using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static Move;
using static Piece;

public class PieceScript : MonoBehaviour, ISelectEventListener, IDragEventListener
{
    public TMPro.TMP_Text text;
    public SpriteRenderer backSprite;

    public BoardScript bs;

    public DraggableObject dob;
    public SelectableObject seb;

    public int x;
    public int y;

    public uint piece;

    public bool isSelected;

    public bool isHover;
    public bool lastIsHover;

    public bool isGiant;
    public BoxCollider bc;

    public TrashCanScript trashCan;
    public bool canDelete;

    //public TMPro.TMP_Text specialText;
    public TextDisplayer specialText;
    public TMPro.TMP_Text statusText;

    public SquareScript squareBelow;

    public ulong relayDefenders;
    public ulong relayAttacked;

    public GameObject relayTrailTemplate;
    public List<LineRenderer> relayDefendersList;   //Light part is the end point
    public List<LineRenderer> relayAttackedList;    //Light part is the start point

    public GameObject selectObject;

    public virtual void Start()
    {
        trashCan = FindObjectOfType<TrashCanScript>();

        dob = GetComponent<DraggableObject>();
        dob.canDrag = true;
    }

    public virtual void ForceDeselect()
    {
        seb.ForceDeselect();
        dob.isDragged = false;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
    }
    public void OnMouseOver()
    {
        isHover = true;
    }

    public string GetHoverText()
    {
        string hoverText = "";

        //piece name
        switch (Piece.GetPieceAlignment(piece))
        {
            case PieceAlignment.White:
                hoverText += "<outlinecolor,#f2f2f2>" + Piece.GetPieceName(Piece.GetPieceType(piece)) + "</color></font>\n";
                break;
            case PieceAlignment.Black:
                hoverText += "<outlinecolor,#808080>" + Piece.GetPieceName(Piece.GetPieceType(piece)) + "</color></font>\n";
                break;
            case PieceAlignment.Neutral:
                hoverText += "<outlinecolor,#f2f266>" + Piece.GetPieceName(Piece.GetPieceType(piece)) + "</color></font>\n";
                hoverText += "<outlinecolor,#f2f266>Neutral</color></font> pieces can be moved or captured by either side.<line>";
                hoverText += "Note that you can't move the exact same piece your enemy did last turn.<line>";
                break;
            case PieceAlignment.Crystal:
                hoverText += "<outlinecolor,#f266f2>" + Piece.GetPieceName(Piece.GetPieceType(piece)) + "</color></font>\n";
                hoverText += "<outlinecolor,#f266f2>Crystal</color></font> pieces can be moved by a side that is directly attacking this piece.<line>";
                hoverText += "(i.e. an attack shown by Control Zones.)<line>";
                hoverText += "Note that you can't move the exact same piece your enemy did last turn.<line>";
                break;
        }

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(piece);
        hoverText += "Value: " + ((pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 2f);
        if (pte.pieceValueX2 > GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)
        {
            hoverText += " (Royal)";
        }

        //modifiers
        if (Piece.GetPieceModifier(piece) != 0)
        {
            Piece.PieceModifier pm = Piece.GetPieceModifier(piece);
            hoverText += "\n" + "<outlinecolor," + MainManager.ColorToString(Piece.GetModifierColor(pm)) + ">" + Piece.GetModifierName(pm) + "</color></font>: " + Piece.GetModifierDescription(pm);
        }

        //statuses
        if (Piece.GetPieceStatusEffect(piece) != 0)
        {
            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);
            hoverText += "\n" + "<outlinecolor," + MainManager.ColorToString(Piece.GetStatusEffectColor(pse)) + ">" + Piece.GetStatusEffectName(pse) + "</color></font>: " + Piece.GetStatusEffectDescription(pse);
        }

        //Relay attackers
        if (relayAttacked != 0)
        {
            hoverText += "\n<outlinecolor,#ffff00>Relay</color></font> to: ";
            int relay = 0;
            ulong ra = relayAttacked;
            while (ra != 0)
            {
                int i = MainManager.PopBitboardLSB1(ra, out ra);

                if (relay != 0)
                {
                    hoverText += ", ";
                }

                if (bs.board.pieces[i] != 0)
                {
                    hoverText += Piece.GetPieceName(Piece.GetPieceType(bs.board.pieces[i]));
                }

                relay++;
            }
        }
        //Relay defenders
        if (relayDefenders != 0)
        {
            hoverText += "\n<outlinecolor,#ffff00>Relay</color></font> from: ";
            int relay = 0;
            ulong rd = relayDefenders;
            while (rd != 0)
            {
                int i = MainManager.PopBitboardLSB1(rd, out rd);

                if (relay != 0)
                {
                    hoverText += ", ";
                }

                if (bs.board.pieces[i] != 0)
                {
                    hoverText += Piece.GetPieceName(Piece.GetPieceType(bs.board.pieces[i]));
                }

                relay++;
            }
        }

        if (squareBelow != null)
        {
            if (hoverText.Length > 0)
            {
                hoverText += "\n\n";
            }
            hoverText += squareBelow.GetHoverText();
        }

        return hoverText;
    }

    public void Setup(uint piece, int x, int y)
    {
        this.piece = piece;

        //should not be possible
        if (piece == 0)
        {
            Destroy(gameObject);
            return;
        }

        isGiant = (GlobalPieceManager.GetPieceTableEntry(piece).piecePropertyB & Piece.PiecePropertyB.Giant) != 0;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        if (specialText != null)
        {
            specialText.SetText("", true, true); //text = "";

            Piece.PieceType pt = Piece.GetPieceType(piece);
            ushort specialData = Piece.GetPieceSpecialData(piece);
            //■
            //special types
            Board b = bs.board;

            if (((b.globalData.bitboard_enhancedWhite | b.globalData.bitboard_enhancedBlack) & (1uL << (x + (y << 3)))) != 0)
            {
                switch (GlobalPieceManager.GetPieceTableEntry(piece).enhancedMoveType)
                {
                    case EnhancedMoveType.PartialForcedMoves:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.InverseForcedMoves:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.PartialForcedCapture:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.SwitchMover:
                        specialText.SetText("□", true, true);
                        break;
                    case EnhancedMoveType.WarMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.ShyMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.NoAllyMover:
                        specialText.SetText("□", true, true);
                        break;
                    case EnhancedMoveType.AllyMover:
                        specialText.SetText("□", true, true);
                        break;
                    case EnhancedMoveType.JusticeMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.DiligenceMover:
                        specialText.SetText("□", true, true);
                        break;
                    case EnhancedMoveType.VampireMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.FearfulMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.FarHalfMover:
                        specialText.SetText("■", true, true);
                        break;
                    case EnhancedMoveType.CloseHalfMover:
                        specialText.SetText("■", true, true);
                        break;
                    default:
                        if (pt == PieceType.ArcanaLovers)
                        {
                            specialText.SetText("■", true, true);
                        }
                        break;
                }
            }

            switch (pt)
            {
                case PieceType.ArcanaFool:
                    Piece.PieceType last = PieceType.Null;
                    switch (pa)
                    {
                        case PieceAlignment.White:
                            last = b.whitePerPlayerInfo.lastPieceMovedType;
                            break;
                        case PieceAlignment.Black:
                            last = b.blackPerPlayerInfo.lastPieceMovedType;
                            break;
                    }
                    if (last != PieceType.ArcanaFool && last != PieceType.Null)
                    {
                        specialText.SetText(Piece.GetPieceName(last), true, true);
                        //specialText.SetText("<piece," + last + ",2>", true, true);
                        //specialText.text = Piece.GetPieceName(last);
                    }
                    break;
                case PieceType.Imitator:
                    Piece.PieceType elast = PieceType.Null;
                    switch (pa)
                    {
                        case PieceAlignment.White:
                            elast = b.blackPerPlayerInfo.lastPieceMovedType;
                            break;
                        case PieceAlignment.Black:
                            elast = b.whitePerPlayerInfo.lastPieceMovedType;
                            break;
                    }
                    if (elast != PieceType.Imitator && elast != PieceType.ArcanaFool && elast != PieceType.Null)
                    {
                        specialText.SetText(Piece.GetPieceName(elast), true, true);
                        //specialText.SetText("<piece," + elast + ",2>", true, true);
                        //specialText.text = Piece.GetPieceName(elast);
                    }
                    break;
                case PieceType.SludgeTrail:
                case Piece.PieceType.Revenant:
                    if (specialData != 0)
                    {
                        specialText.SetText("X", true, true);
                    }
                    break;
                case Piece.PieceType.ChargeWarper:
                case Piece.PieceType.ChargePawn:
                case Piece.PieceType.ChargeCannon:
                case Piece.PieceType.ChargeBeast:
                case Piece.PieceType.ChargeKnight:
                case Piece.PieceType.QueenLeech:
                case Piece.PieceType.SoulDevourer:
                case Piece.PieceType.SoulCannon:
                case Piece.PieceType.Lich:
                    if (specialData != 0)
                    {
                        specialText.SetText(specialData + "", true, true);
                    }
                    break;
                case Piece.PieceType.MegaCannon:
                    if (specialData != 0)
                    {
                        ushort squareData = (ushort)(specialData & 63);
                        specialText.SetText(Move.FileToLetter(squareData & 7) + "" + (((squareData) >> 3) + 1) + "," + (8 - (specialData >> 6)), true, true);
                    }
                    break;
                case PieceType.WarpWeaver:
                case Piece.PieceType.Cannon:
                case Piece.PieceType.SteelGolem:
                case Piece.PieceType.SteelPuppet:
                case Piece.PieceType.MetalFox:
                    if (specialData != 0)
                    {
                        specialText.SetText(Move.FileToLetter(specialData & 7) + "" + (((specialData & 63) >> 3) + 1), true, true);
                    }
                    break;
                case Piece.PieceType.Tunnel:
                case Piece.PieceType.Bunker:
                case Piece.PieceType.Carrier:
                case Piece.PieceType.Airship:
                case Piece.PieceType.Train:
                case Piece.PieceType.Rabbit:
                    if (specialData != 0)
                    {
                        specialText.SetText(Piece.GetPieceName((Piece.PieceType)specialData), true, true);
                        //specialText.SetText("<piece," + Piece.GetPieceName((Piece.PieceType)specialData) + ",2>", true, true);
                    }
                    break;
                case Piece.PieceType.Balloon:
                case Piece.PieceType.Roller:
                case Piece.PieceType.RollerQueen:
                case Piece.PieceType.BounceBishop:
                case Piece.PieceType.ReboundRook:
                    if (specialData != 0)
                    {
                        switch ((Dir)specialData)
                        {
                            case Dir.DownLeft:
                                specialText.SetText("<dlarrow>", true, true);
                                break;
                            case Dir.Down:
                                specialText.SetText("<darrow>", true, true);
                                break;
                            case Dir.DownRight:
                                specialText.SetText("<drarrow>", true, true);
                                break;
                            case Dir.Left:
                                specialText.SetText("<larrow>", true, true);
                                break;
                            case Dir.Right:
                                specialText.SetText("<rarrow>", true, true);
                                break;
                            case Dir.UpLeft:
                                specialText.SetText("<ularrow>", true, true);
                                break;
                            case Dir.Up:
                                specialText.SetText("<uarrow>", true, true);
                                break;
                            case Dir.UpRight:
                                specialText.SetText("<urarrow>", true, true);
                                break;
                        }
                        //specialText.text = (Dir)specialData + "";
                    }
                    break;
            }
        }

        if (statusText != null)
        {
            if (Piece.GetPieceStatusDuration(piece) != 0)
            {
                statusText.text = Piece.GetPieceStatusDuration(piece) + "";
            }
            else
            {
                statusText.text = "";
            }
        }

        text.enabled = true;
        backSprite.enabled = true;
        bc.enabled = true;
        if (isGiant)
        {
            if (Piece.GetPieceSpecialData(piece) != 0)
            {
                text.enabled = false;
                backSprite.enabled = false;
                //transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
                bc.enabled = false;
                this.x = x;
                this.y = y;
                return;
            }

            Vector3 offset = (Vector3.up + Vector3.right) * 0.5f;
            text.transform.localPosition = offset + Vector3.back * 0.05f;
            backSprite.transform.localPosition = offset;
            text.transform.localScale = Vector3.one * 2;
            backSprite.transform.localScale = Vector3.one * 1.8f;
            selectObject.transform.localPosition = offset + Vector3.forward * 0.2f;
            selectObject.transform.localScale = Vector3.one * 2f;
            bc.center = offset;
            bc.size = new Vector3(2,2,0.1f);
        } else
        {
            text.transform.localPosition = Vector3.back * 0.05f;
            backSprite.transform.localPosition = Vector3.zero;
            text.transform.localScale = Vector3.one;
            backSprite.transform.localScale = Vector3.one * 0.9f;
            selectObject.transform.localPosition = Vector3.forward * 0.2f;
            selectObject.transform.localScale = Vector3.one;
            bc.center = Vector3.zero;
            bc.size = new Vector3(1, 1, 0.1f);
        }

        backSprite.sprite = Text_PieceSprite.GetPieceSprite(Piece.GetPieceType(piece));
        backSprite.material = Text_PieceSprite.GetMaterial(Piece.GetPieceModifier(piece));
        MaterialPropertyBlock mpb = Text_PieceSprite.SetupMaterialProperties(Piece.GetPieceStatusEffect(piece), null);
        mpb.SetTexture("_MainTex", backSprite.sprite.texture);
        backSprite.SetPropertyBlock(mpb);

        if (MainManager.Instance.pieceTextVisible)
        {
            text.text = Piece.GetPieceName(Piece.GetPieceType(piece)); //Piece.GetPieceType(piece).ToString();
        }
        else
        {
            text.text = ""; //Piece.GetPieceType(piece).ToString();
        }

        /*
        if (Piece.GetPieceModifier(piece) != 0)
        {
            text.text = text.text + "\n" + Piece.GetPieceModifier(piece);
        }

        if (Piece.GetPieceStatusEffect(piece) != 0)
        {
            text.text = text.text + "\n" + Piece.GetPieceStatusEffect(piece); // + " " + (Piece.GetPieceStatusDuration(piece));
        }
        */

        /*
        if (Piece.GetPieceSpecialData(piece) != 0)
        {
            text.text = text.text + "\n(" + Piece.GetPieceSpecialData(piece) + ")";
        }
        */

        Color color = Piece.GetPieceColor(pa);

        backSprite.color = color;
        text.color = color;

        if (pa == PieceAlignment.Black)
        {
            text.color = Color.black;
        }

        this.x = x;
        this.y = y;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
    }

    public void ResetRelay()
    {
        for (int i = 0; i < relayAttackedList.Count; i++)
        {
            Destroy(relayAttackedList[i].gameObject);
        }
        relayAttackedList = new List<LineRenderer>();
        for (int i = 0; i < relayDefendersList.Count; i++)
        {
            Destroy(relayDefendersList[i].gameObject);
        }
        relayDefendersList = new List<LineRenderer>();
    }
    public void UpdateRelay()
    {
        ResetRelay();
        ulong test = relayDefenders;
        while (test != 0)
        {
            int i = MainManager.PopBitboardLSB1(test, out test);

            GameObject relayTrail = Instantiate(relayTrailTemplate, transform);
            LineRenderer relayTrailLine = relayTrail.GetComponent<LineRenderer>();

            Vector3 posA = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.8f);
            Vector3 posB = BoardScript.GetSpritePositionFromCoordinates(i & 7, i >> 3, -0.8f);
            List<Vector3> pointList = new List<Vector3>();
            pointList.Add(posA);
            MoveTrailScript.AddLeaperSegment(pointList, posA, posB);
            pointList.Add(posB);

            relayTrailLine.positionCount = pointList.Count;
            relayTrailLine.SetPositions(pointList.ToArray());
            relayDefendersList.Add(relayTrailLine);
        }
        test = relayAttacked;
        while (test != 0)
        {
            int i = MainManager.PopBitboardLSB1(test, out test);

            GameObject relayTrail = Instantiate(relayTrailTemplate, transform);
            LineRenderer relayTrailLine = relayTrail.GetComponent<LineRenderer>();

            //reversed
            Vector3 posA = BoardScript.GetSpritePositionFromCoordinates(i & 7, i >> 3, -0.8f);
            Vector3 posB = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.8f);
            List<Vector3> pointList = new List<Vector3>();
            pointList.Add(posA);
            MoveTrailScript.AddLeaperSegment(pointList, posA, posB);
            pointList.Add(posB);

            relayTrailLine.positionCount = pointList.Count;
            relayTrailLine.SetPositions(pointList.ToArray());
            relayAttackedList.Add(relayTrailLine);
        }
    }

    public virtual void Update()
    {
        dob.canDrag = bs.CanSelectPieces();
        bc.enabled = bs.CanSelectPieces() && text.enabled; //Become intangible while animating (Also be intangible if you are the wrong part of a giant)

        if (isHover && bs.CanSelectPieces())
        {
            HoverTextMasterScript.Instance.SetHoverText(GetHoverText());
        }

        lastIsHover = isHover;
        isHover = false;
    }

    public virtual void OnSelect()
    {
        if (bs.setupMoves)
        {
            if (x < 0 || x > 7 || y < 0 || y > 7)
            {
                canDelete = false;
            } else
            {
                canDelete = bs.IsSetupMoveLegal(this, Move.PackMove(x, y, 15, 15));
            }
        }

        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        //backSprite.color = new Color(1 - backSprite.color.r, backSprite.color.g, backSprite.color.b, 1);
        selectObject.SetActive(true);

        bs.SelectPiece(this);
        isSelected = true;
    }

    public void OnDeselect()
    {
        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        selectObject.SetActive(false);
        if (bs.selectedPiece == null || bs.selectedPiece == this)
        {
            bs.ResetSelected(false);
        }
        isSelected = false;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public int GetCost()
    {
        int cost = 0;
        if (GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece)) != null)
        {
            cost = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece)).pieceValueX2;
        }

        if (Piece.GetPieceModifier(piece) != 0)
        {
            cost += Piece.GetModifierValue(Piece.GetPieceModifier(piece));
        }

        cost &= GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE;

        return cost;
    }

    public void OnDragStart()
    {
        //ResetRelay();
    }
    public virtual void OnDragStay()
    {
        isHover = true;
        (bs.hoverX, bs.hoverY) = bs.GetCoordinatesFromPosition(transform.position);            
        if (trashCan != null && bs.setupMoves)
        {
            if (!canDelete)
            {
                trashCan.text.text = "Can't Sell";
            }
            else
            {
                trashCan.text.text = "Sell: $" + (GetCost() & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE);
            }
            trashCan.SetActive();
            if (!canDelete)
            {
                trashCan.SetForbidden();
            }
            if (isGiant)
            {
                if (trashCan.QueryPosition(transform.position + new Vector3(BoardScript.SQUARE_SIZE / 2, BoardScript.SQUARE_SIZE / 2)))
                {
                    if (canDelete)
                    {
                        trashCan.SetHighlight();
                    }
                }
            }
            else
            {
                if (trashCan.QueryPosition(transform.position))
                {
                    if (canDelete)
                    {
                        trashCan.SetHighlight();
                    }
                }
            }
        }
    }
    public virtual void OnDragStop()
    {
        //Setup mode
        if (bs.setupMoves)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                if ((bs is SetupBoardScript && bs.hoverX >= 0 && bs.hoverX <= 7 && bs.hoverY >= 0 && bs.hoverY <= 1) || (!(bs is SetupBoardScript) && bs.hoverX >= 0 && bs.hoverX <= 7 && bs.hoverY >= 0 && bs.hoverY <= 7))
                {
                    bs.TrySetupMove(this, x, y, bs.hoverX, bs.hoverY);
                } else if (trashCan != null && isGiant ? trashCan.QueryPosition(transform.position + new Vector3(BoardScript.SQUARE_SIZE / 2, BoardScript.SQUARE_SIZE / 2)) : trashCan.QueryPosition(transform.position))
                {
                    //does this make sense?
                    //This should not have problems (i.e. being able to get money and keep the piece)
                    bool value = bs.TrySetupMove(this, Move.PackMove(x, y, 15, 15));
                    if (value)
                    {
                        MainManager.Instance.playerData.coins += GlobalPieceManager.GetPieceTableEntry(piece).pieceValueX2;
                    }
                } else
                {
                    transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
                }
            } else
            {
                transform.position = BoardScript.GetSpritePositionFromCoordinates(x, y, -0.5f);
            }
            return;
        }


        //debug: lets you play as black
        //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
        if (bs is BattleBoardScript bbs && !bbs.blackIsAI)
        {
            //don't treat adjustments as moves
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, bs.board.blackToMove ? Piece.PieceAlignment.Black : Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
                //bs.TryMove(this, Piece.GetPieceAlignment(piece), x, y, bs.hoverX, bs.hoverY);
            } else
            {

            }
        }
        else
        {
            if (!(x == bs.hoverX && y == bs.hoverY))
            {
                bs.TryMove(this, Piece.PieceAlignment.White, x, y, bs.hoverX, bs.hoverY);
            } else
            {

            }
        }

        if (bs.CanSelectPieces())
        {
            x = bs.hoverX;
            y = bs.hoverY;
            transform.position = BoardScript.GetSpritePositionFromCoordinates(bs.hoverX, bs.hoverY, -0.5f);
            bs.FixBoardBasedOnPosition();
        }
    }

    public void SetPosition(int nx, int ny)
    {
        x = nx;
        y = ny;
        transform.position = BoardScript.GetSpritePositionFromCoordinates(nx, ny, -0.5f);
    }

    public void ResetColor()
    {
        backSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));
        if (bs.selectedPiece == null)
        {
            bs.ResetSelected();
        }
        isSelected = false;
    }
}
