using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Move;
using static MoveGeneratorInfoEntry;
using static Piece;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PieceMovePanelScript : MonoBehaviour
{
    public Image backSprite;
    public Image pieceImageSprite;
    public TMPro.TMP_Text pieceNameText;
    public GameObject singleSquareTemplate;
    public GameObject trailTemplate;
    public TMPro.TMP_Text pieceValueText;
    public TMPro.TMP_Text pieceInfoText;
    public TMPro.TMP_Text pieceClassText;

    public GameObject centerHolder;
    public GameObject squareHolder;
    public GameObject trailHolder;

    public List<GameObject> squareLegend;

    public bool init;

    public List<List<PieceMovePanelSquareScript>> grid;
    public List<PieceMovePanelTrailScript> lines;

    public HashSet<Move.SpecialType> specialTypes;
    public PieceMovePanelSquareScript.SpecialIndication specialIndications;

    public void Start()
    {
        if (!init)
        {
            Init();
        }
    }

    public void Init()
    {
        init = true;

        grid = new List<List<PieceMovePanelSquareScript>>();
        lines = new List<PieceMovePanelTrailScript>();

        float panelSize = backSprite.rectTransform.sizeDelta.x;
        float squareSize = panelSize / 15f;
        float offset = (panelSize - squareSize) / 2;

        for (int y = 0; y < 15; y++)
        {
            grid.Add(new List<PieceMovePanelSquareScript>());
            for (int x = 0; x < 15; x++)
            {
                GameObject square = Instantiate(singleSquareTemplate, squareHolder.transform);
                PieceMovePanelSquareScript i = square.GetComponent<PieceMovePanelSquareScript>();

                i.rectTransform.anchoredPosition = new Vector3((x * squareSize) - offset, (y * squareSize) - offset, 0);
                grid[y].Add(i);

                if (((x + y) & 1) != 0)
                {
                    //white
                    i.defaultColor = new Color(0.9f, 0.9f, 0.9f, 1);
                }
                else
                {
                    //black
                    i.defaultColor = new Color(0.83f, 0.83f, 0.83f, 1);
                }
                i.Reset();
            }
        }

        //center (square in question)
        grid[7][7].defaultColor = new Color(0, 0, 0, 1);
        //make it show up above trails
        grid[7][7].transform.parent = centerHolder.transform;
        grid[7][7].Reset();

        ResetAll();
    }

    public void ResetAll()
    {
        pieceNameText.text = "";
        pieceInfoText.text = "";
        pieceValueText.text = "";
        pieceClassText.text = "";
        pieceImageSprite.sprite = null;
        pieceImageSprite.color = new Color(0, 0, 0, 0);
        ResetGridColor();
    }

    public void SetText(string text)
    {
        ResetAll();
        pieceInfoText.text = text;
    }

    public void SetBadge(Board.PlayerModifier pm)
    {
        ResetAll();
        pieceImageSprite.sprite = Text_BadgeSprite.GetBadgeSprite(pm);
        //don't do the color change thing
        pieceImageSprite.material = null;
        pieceImageSprite.color = new Color(1, 1, 1, 1);
        pieceNameText.text = Board.GetPlayerModifierName(pm);
        pieceInfoText.text = Board.GetPlayerModifierDescription(pm);        
    }

    public void SetConsumable(Move.ConsumableMoveType cmt)
    {
        ResetAll();
        pieceImageSprite.sprite = Text_ConsumableSprite.GetConsumableSprite(cmt);
        //don't do the color change thing
        pieceImageSprite.material = null;
        pieceImageSprite.color = new Color(1, 1, 1, 1);
        pieceNameText.text = Board.GetConsumableName(cmt);
        pieceInfoText.text = Board.GetConsumableDescription(cmt);
    }

    public void ResetGridColor()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            Destroy(lines[i].gameObject);
        }
        lines = new List<PieceMovePanelTrailScript>();

        for (int i = 0; i < squareLegend.Count; i++)
        {
            Destroy(squareLegend[i]);
        }
        //pieceInfoText.rectTransform.localPosition = Vector3.down * 150;

        for (int y = 0; y < 15; y++)
        {
            for (int x = 0; x < 15; x++)
            {
                PieceMovePanelSquareScript i = grid[y][x];
                if (((x + y) & 1) != 0)
                {
                    //white
                    i.defaultColor = new Color(0.9f, 0.9f, 0.9f, 1);
                }
                else
                {
                    //black
                    i.defaultColor = new Color(0.83f, 0.83f, 0.83f, 1);
                }
                i.Reset();
            }
        }

        //center (square in question)
        grid[7][7].defaultColor = new Color(0, 0, 0, 1);
        grid[7][7].Reset();
    }

    public void SetAura(ulong pattern, int dx, int dy)
    {
        while (pattern != 0)
        {
            int i = MainManager.PopBitboardLSB1(pattern, out pattern);
            int x = (i & 7);
            int y = (i >> 3);
            GetSquare(x + dx, y + dy).SetAura();
        }
    }
    public Piece.Aura SetAura(PieceTableEntry pte)
    {
        switch (pte.type)
        {
            case Piece.PieceType.Attractor:
                SetAura(MoveGenerator.BITBOARD_PATTERN_QUEEN3, -3, -3);
                return Aura.Attractor;
            case Piece.PieceType.Repulser:
                SetAura(MoveGenerator.BITBOARD_PATTERN_QUEEN3, -3, -3);
                return Aura.Repulser;
            case Piece.PieceType.Immobilizer:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Immobilizer;
            case Piece.PieceType.DivineApothecary:
            case Piece.PieceType.Virgo:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Nullify;
            case Piece.PieceType.Entrancer:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP2, -2, -2);
                return Aura.Water;
            case Piece.PieceType.Charmer:
                GetSquare(0, 1).SetAura();
                return Aura.Immobilizer;
            case Piece.PieceType.Sloth:
                SetAura(MoveGenerator.BITBOARD_PATTERN_RANGE2, -2, -2);
                return Aura.Sloth;
            case Piece.PieceType.ArcanaHierophant:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK2, -2, -2);
                return Aura.Rough;
            case Piece.PieceType.ArcanaHanged:
                SetAura(MoveGenerator.BITBOARD_PATTERN_DIAMOND, -2, -2);
                return Aura.Hanged;
            case Piece.PieceType.AceOfPentacles:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK1, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.AceOfCups:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP1, -1, -1);
                return Aura.Water;
            case Piece.PieceType.PageOfPentacles:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK1, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.PageOfCups:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP1, -1, -1);
                return Aura.Water;
            case Piece.PieceType.QueenOfPentacles:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK2, -2, -2);
                return Aura.Rough;
            case Piece.PieceType.QueenOfCups:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP2, -2, -2);
                return Aura.Water;
            case Piece.PieceType.Earth:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.Saturn:
                SetAura(MoveGenerator.BITBOARD_PATTERN_SATURN, -2, -2);
                return Aura.Rough;
            case Piece.PieceType.Ganymede:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.Taurus:
                SetAura(MoveGenerator.BITBOARD_PATTERN_TAURUS_WHITE, -1, 0);
                return Aura.Rough;
            case Piece.PieceType.Aquarius:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK2, -2, -2);
                return Aura.Water;
            case Piece.PieceType.EarthElemental:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.WaterElemental:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Water;
            case Piece.PieceType.EarthWisp:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK1, -1, -1);
                return Aura.Rough;
            case Piece.PieceType.WaterWisp:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP1, -1, -1);
                return Aura.Water;
            case Piece.PieceType.Banshee:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Banshee;
            case Piece.PieceType.Harpy:
                SetAura(MoveGenerator.BITBOARD_PATTERN_BISHOP2, -2, -2);
                return Aura.Harpy;
            case Piece.PieceType.Hag:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Hag;
            case PieceType.RabbitDiplomat:
            case PieceType.Diplomat:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK1, -1, -1);
                return Aura.Water;
            case Piece.PieceType.Enforcer:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ROOK1, -1, -1);
                return Aura.Immobilizer;
            case Piece.PieceType.Watchtower:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Watchtower;
            case Piece.PieceType.Fan:
                SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
                return Aura.Fan;
        }

        if ((pte.pieceProperty & PieceProperty.RelayImmune) != 0)
        {
            SetAura(MoveGenerator.BITBOARD_PATTERN_ADJACENT, -1, -1);
            return Aura.Immune;
        }
        return Aura.None;
    }

    public void SetMove(uint piece)
    {
        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(piece);
        Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);
        Piece.PieceModifier pm = Piece.GetPieceModifier(piece);

        pieceImageSprite.sprite = Text_PieceSprite.GetPieceSprite(pte.type);
        pieceImageSprite.material = Text_PieceSprite.GetMaterialGUI(pm);
        pieceImageSprite.color = Piece.GetPieceColor(Piece.GetPieceAlignment(piece));

        ResetGridColor();
        pieceNameText.text = Piece.GetPieceName(pte.type) + "";
        specialTypes = new HashSet<SpecialType>();
        specialIndications = 0;
        string moveText = "";

        foreach (MoveGeneratorInfoEntry mgie in pte.moveInfo)
        {
            if (MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom) != null && MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom).Length > 0)
            {
                moveText += MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom) + "\n";
            }
            SetSingleEntry(mgie, pte, false);
        }
        foreach (MoveGeneratorInfoEntry mgie in pte.enhancedMoveInfo)
        {
            if (MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom) != null && MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom).Length > 0)
            {
                moveText += MoveGeneratorInfoEntry.GetSpecialMoveText(mgie.atom) + "\n";
            }
            SetSingleEntry(mgie, pte, true);
        }

        List<Move.SpecialType> moveTypes = new List<SpecialType>();
        foreach (Move.SpecialType st in specialTypes)
        {
            moveTypes.Add(st);
        }
        //should sort by low first?
        moveTypes.Sort();

        if (specialIndications != 0)
        {
            moveText += "▪ ";
        }
        for (int i = 0; i < 32; i++)
        {
            int bitindex = 1 << i;

            if ((bitindex & (int)specialIndications) != 0)
            {
                PieceMovePanelSquareScript.SpecialIndication spi = (PieceMovePanelSquareScript.SpecialIndication)bitindex;

                switch (spi)
                {
                    case PieceMovePanelSquareScript.SpecialIndication.Initial:
                        moveText += "Initial: Only usable from the first 2 rows.\n";
                        break;
                    case PieceMovePanelSquareScript.SpecialIndication.AntiRange:
                        moveText += "Anti Range: Must go as far as possible.\n";
                        break;
                    case PieceMovePanelSquareScript.SpecialIndication.Enhanced:
                        switch (pte.enhancedMoveType)
                        {
                            case EnhancedMoveType.PartialForcedMoves:
                                //implementation quirk means you guaranteed get this when stuff is non capturing
                                moveText += "Enhanced moves are only possible if no normal moves are possible or the piece can't capture.\n";
                                //moveText += "▪ Enhanced: Moves only possible under certain conditions.\n";
                                break;
                            case EnhancedMoveType.InverseForcedMoves:
                                //implementation quirk means you guaranteed get this when stuff is non capturing
                                moveText += "Enhanced moves are only possible if normal moves are possible and the piece is not prevented from capturing by terrain or status effects.\n";
                                //moveText += "▪ Enhanced: Moves only possible under certain conditions.\n";
                                break;
                            case EnhancedMoveType.PartialForcedCapture:
                                moveText += "Enhanced moves are only possible if no capturing moves are possible.\n";
                                break;
                            case EnhancedMoveType.SwitchMover:
                                moveText += "Moveset replaced with Enhanced moveset when on dark squares.\n";
                                //moveText += "▪ Enhanced: Moves only possible under certain conditions.\n";
                                break;
                            case EnhancedMoveType.WarMover:
                                moveText += "Can only use Enhanced moves when adjacent to enemies.\n";
                                break;
                            case EnhancedMoveType.ShyMover:
                                moveText += "Can only use Enhanced moves when not adjacent to enemies.\n";
                                break;
                            case EnhancedMoveType.NoAllyMover:
                                moveText += "Moveset replaced with Enhanced moveset when no allies adjacent.\n";
                                break;
                            case EnhancedMoveType.AllyMover:
                                moveText += "Moveset replaced with Enhanced moveset when adjacent to allies.\n";
                                break;
                            case EnhancedMoveType.JusticeMover:
                                moveText += "Can only use Enhanced moves when the enemy captured last turn.\n";
                                break;
                            case EnhancedMoveType.DiligenceMover:
                                moveText += "Moveset replaced with Enhanced moveset when this piece was moved last turn.\n";
                                break;
                            case EnhancedMoveType.VampireMover:
                                moveText += "Can only use Enhanced moves after a capture occurred last turn.\n";
                                break;
                            case EnhancedMoveType.FearfulMover:
                                moveText += "Can only use Enhanced moves after a capture didn't occur last turn.\n";
                                break;
                            case EnhancedMoveType.FarHalfMover:
                                moveText += "Can only use Enhanced moves on the far half of the board.\n";
                                break;
                            case EnhancedMoveType.CloseHalfMover:
                                moveText += "Can only use Enhanced moves on the close half of the board.\n";
                                break;
                        }
                        switch (pte.type)
                        {
                            case PieceType.Envy:
                            case PieceType.Kindness:
                                moveText += "Enhanced moves only possible if not overwritten by moves copied from other pieces.\n";
                                break;
                            case PieceType.ArcanaLovers:
                                moveText += "Can only use Enhanced moves when adjacent to allies.\n";
                                break;
                        }
                        break;
                }
            }
        }

        Piece.Aura aura = SetAura(pte);
        if (aura != 0)
        {
            moveText += "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=#80c0ff>□</color></font> Aura: " + GetAuraDescription(aura) + "\n";
        }

        for (int i = 0; i < moveTypes.Count; i++)
        {
            //populate some square legend
            moveText += "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=" + MainManager.ColorToString(PieceMovePanelScript.GetColorFromSpecialType(moveTypes[i])) + "><size=120%>■</size></color></font> " + Move.GetSpecialTypeDescription(moveTypes[i], pte) + "\n";
        }
        pieceInfoText.text = moveText;

        if (pse != PieceStatusEffect.None)
        {
            pieceInfoText.text += "\n" + "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=" + MainManager.ColorToString(Piece.GetStatusEffectColor(pse)) + ">" + Piece.GetStatusEffectName(pse) + "</color></font>: " + Piece.GetStatusEffectDescription(pse) + "\n";
        }
        if (pm != PieceModifier.None)
        {
            pieceInfoText.text += "\n" + "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=" + MainManager.ColorToString(Piece.GetModifierColor(pm)) + ">" + Piece.GetModifierName(pm) + "</color></font>: " + Piece.GetModifierDescription(pm) + "\n";
        }

        if (Piece.GetPieceSpecialDescription(pte.type).Length > 0)
        {
            pieceInfoText.text += "\n" + Piece.GetPieceSpecialDescription(pte.type) + "\n";
        }
        if (moveText.Length > 0)
        {
            pieceInfoText.text += "\n";
        }

        //pieceInfoText.text = pte.type + "\n";
        pieceValueText.text = "Value: " + ((pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) / 2f) + "";
        if ((pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS) != 0)
        {
            pieceValueText.text += " <size=50%>(Royal)</size>";
        }

        if (pte.promotionType != 0)
        {
            pieceInfoText.text += "Promotes to " + Piece.GetPieceName(pte.promotionType) + "\n\n";
        }

        //string propertyText = "";
        if (pte.pieceProperty != 0 || pte.piecePropertyB != 0)
        {
            //pieceInfoText.text += "Properties:\n" + propertyText;
            ulong propertiesA = (ulong)pte.pieceProperty;
            ulong propertiesB = (ulong)pte.piecePropertyB;

            while (propertiesA != 0)
            {
                int index = MainManager.PopBitboardLSB1(propertiesA, out propertiesA);
                //pieceInfoText.text += (Piece.PieceProperty)(1uL << index) + "\n";
                if (Piece.GetPropertyDescription((Piece.PieceProperty)(1uL << index), pte.type).Length > 0)
                {
                    //<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=#404040>
                    pieceInfoText.text += "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=#ffffff>" + Piece.GetPropertyName((Piece.PieceProperty)(1uL << index)) + "</color></font>: " + Piece.GetPropertyDescription((Piece.PieceProperty)(1uL << index), pte.type) + "\n\n";
                }
            }
            while (propertiesB != 0)
            {
                int index = MainManager.PopBitboardLSB1(propertiesB, out propertiesB);
                //pieceInfoText.text += (Piece.PiecePropertyB)(1uL << index) + "\n";

                if (Piece.GetPropertyDescription((Piece.PiecePropertyB)(1uL << index), pte.type).Length > 0)
                {
                    pieceInfoText.text += "<font=\"Rubik-SemiBold SDF\" material=\"Rubik-SemiBold Atlas Material OutlineColorMatch\"><color=#ffffff>" + Piece.GetPropertyName((Piece.PiecePropertyB)(1uL << index)) + "</color></font>: " + Piece.GetPropertyDescription((Piece.PiecePropertyB)(1uL << index), pte.type) + "\n\n";
                }
            }
        }

        string bottomText = "";
        if (pte.pieceClass == PieceClass.None)
        {
            bottomText = "[None]";
        } else
        {
            bottomText = "[" + GlobalPieceManager.GetPieceClassEntry(pte.pieceClass).name + "]";
        }
        bottomText += " - [" + (pte.promotionType == PieceType.Null ? "NonPawn" : "Pawnlike") + "]";
        pieceClassText.text = bottomText;
    }

    //draw stuff based on given piece
    public void SetSingleEntry(MoveGeneratorInfoEntry mgie, PieceTableEntry pte, bool enhanced)
    {
        //determine move type
        Move.SpecialType specialType = Move.SpecialType.Normal;

        Piece.PieceType pt = pte.type;

        bool directionRestricted = (mgie.modifier & MoveGeneratorPreModifier.DirectionModifiers) != 0;

        PieceMovePanelSquareScript.SpecialIndication spi = PieceMovePanelSquareScript.SpecialIndication.None;

        if (enhanced)
        {
            spi |= PieceMovePanelSquareScript.SpecialIndication.Enhanced;
        }

        if (mgie.rangeType == RangeType.AntiRange)
        {
            spi |= PieceMovePanelSquareScript.SpecialIndication.AntiRange;
        }

        switch (mgie.modifier)
        {
            case MoveGeneratorPreModifier.m:
                specialType = Move.SpecialType.MoveOnly;
                break;
            case MoveGeneratorPreModifier.c:
                specialType = Move.SpecialType.CaptureOnly;
                break;
            default:
                if ((mgie.modifier & MoveGeneratorPreModifier.m) != 0)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
                if ((mgie.modifier & MoveGeneratorPreModifier.c) != 0)
                {
                    specialType = Move.SpecialType.CaptureOnly;
                }
                break;
        }

        if ((mgie.modifier & MoveGeneratorPreModifier.r) != 0)
        {
            specialType = Move.SpecialType.FireCaptureOnly;
        }

        switch (pte.replacerMoveType)
        {
            case ReplacerMoveType.FireCapture:
                if ((specialType == SpecialType.CaptureOnly))
                {
                    specialType = Move.SpecialType.FireCaptureOnly;
                }
                if (specialType != SpecialType.MoveOnly && specialType != SpecialType.FireCaptureOnly)
                {
                    specialType = Move.SpecialType.FireCapture;
                }
                break;
            case ReplacerMoveType.WrathCapturer:
                if (specialType != SpecialType.MoveOnly)
                {
                    specialType = Move.SpecialType.WrathCapturer;
                }
                break;
            case ReplacerMoveType.Push:
                if (specialType != SpecialType.CaptureOnly)
                {
                    specialType = SpecialType.PushMove;
                }
                break;
            case ReplacerMoveType.Pull:
                if (specialType != SpecialType.CaptureOnly)
                {
                    specialType = SpecialType.PullMove;
                }
                break;
            case ReplacerMoveType.SwapCapture:
                specialType = SpecialType.AnyoneSwap;
                break;
            case ReplacerMoveType.ConvertCapture:
                specialType = SpecialType.Convert;
                break;
            case ReplacerMoveType.WeakConvertCapture:
                specialType = SpecialType.ConvertPawn;
                break;
            case ReplacerMoveType.FlankingCapture:
                specialType = SpecialType.FlankingCapturer;
                break;
            case ReplacerMoveType.ConsumeAllies:
                if (specialType == SpecialType.Normal)
                {
                    specialType = SpecialType.ConsumeAllies;
                }
                if (specialType == SpecialType.CaptureOnly)
                {
                    specialType = SpecialType.ConsumeAlliesCaptureOnly;
                }
                break;
            case ReplacerMoveType.Inflict:
                if (specialType != SpecialType.MoveOnly)
                {
                    if (specialType == SpecialType.CaptureOnly)
                    {
                        specialType = SpecialType.InflictCaptureOnly;
                    }
                    else
                    {
                        specialType = SpecialType.Inflict;
                    }
                }
                break;
            case ReplacerMoveType.InflictFreeze:
                if (specialType != SpecialType.MoveOnly)
                {
                    if (specialType == SpecialType.CaptureOnly)
                    {
                        specialType = SpecialType.InflictFreezeCaptureOnly;
                    }
                    else
                    {
                        specialType = SpecialType.InflictFreeze;
                    }
                }
                break;
        }

        if ((mgie.modifier & MoveGeneratorPreModifier.ljanepo) != 0)
        {
            if ((mgie.modifier & MoveGeneratorPreModifier.l) != 0)
            {
                specialType = Move.SpecialType.LongLeaper;
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.j) != 0)
            {
                specialType = Move.SpecialType.LongLeaperCaptureOnly;
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.a) != 0)
            {
                specialType = Move.SpecialType.AllyAbility;

                switch (pt)
                {
                    case Piece.PieceType.ArcanaPriestess:
                        if (mgie.atom == MoveGeneratorAtom.B)
                        {
                            specialType = SpecialType.RangedPushAllyOnly;
                        }
                        else
                        {
                            specialType = SpecialType.RangedPullAllyOnly;
                        }
                        break;
                    case Piece.PieceType.HornSpirit:
                    case Piece.PieceType.TorchSpirit:
                    case Piece.PieceType.RingSpirit:
                    case Piece.PieceType.FeatherSpirit:
                    case Piece.PieceType.GlassSpirit:
                    case Piece.PieceType.BottleSpirit:
                    case Piece.PieceType.CapSpirit:
                    case Piece.PieceType.ShieldSpirit:
                        specialType = SpecialType.ImbueModifier;
                        break;
                    case Piece.PieceType.GrailSpirit:
                        specialType = SpecialType.ImbuePromote;
                        break;
                    case Piece.PieceType.WarpMage:
                    case Piece.PieceType.Flipper:
                        specialType = SpecialType.TeleportOpposite;
                        break;
                    case Piece.PieceType.Mirrorer:
                        specialType = SpecialType.TeleportMirror;
                        break;
                    case Piece.PieceType.Recaller:
                        specialType = SpecialType.TeleportRecall;
                        break;
                    case PieceType.AmoebaCitadel:
                    case PieceType.AmoebaGryphon:
                    case PieceType.AmoebaRaven:
                    case PieceType.AmoebaArchbishop:
                    case PieceType.AmoebaKnight:
                    case PieceType.AmoebaPawn:
                        specialType = SpecialType.AmoebaCombine;
                        break;
                    case Piece.PieceType.Bunker:
                    case Piece.PieceType.Train:
                    case Piece.PieceType.Carrier:
                    case Piece.PieceType.Airship:
                    case Piece.PieceType.Tunnel:
                        specialType = SpecialType.CarryAlly;
                        break;
                }
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.n) != 0)
            {
                specialType = Move.SpecialType.EmptyAbility;

                if ((pte.pieceProperty & Piece.PieceProperty.Splitter) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.Amoeba) != 0)
                {
                    specialType = SpecialType.Spawn;
                }
                if ((pte.piecePropertyB & Piece.PiecePropertyB.PieceCarry) != 0)
                {
                    specialType = SpecialType.DepositAlly;
                    if (pt == Piece.PieceType.Tunnel)
                    {
                        specialType = SpecialType.DepositAllyPlantMove;
                    }
                }
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.e) != 0)
            {
                specialType = Move.SpecialType.EnemyAbility;

                if (pt == Piece.PieceType.Lust)
                {
                    specialType = SpecialType.RangedPull;
                }
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.p) != 0)
            {
                specialType = Move.SpecialType.PassiveAbility;
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.o) != 0)
            {
                specialType = SpecialType.Normal;

                //(b.globalData.enemyModifier & Board.EnemyModifier.Xyloid) != 0
                //currently there are no other ways for a king to have bonusmove so I'm removing it for now
                if (pte.type == Piece.PieceType.King)
                {
                    specialType = Move.SpecialType.PlantMove;
                }
                switch (pte.bonusMoveType)
                {
                    case BonusMoveType.SlipMove:
                        specialType = Move.SpecialType.SlipMove;
                        break;
                    case BonusMoveType.PlantMove:
                        specialType = Move.SpecialType.PlantMove;
                        break;
                    case BonusMoveType.GliderMove:
                        specialType = Move.SpecialType.GliderMove;
                        break;
                    case BonusMoveType.CoastMove:
                        specialType = Move.SpecialType.CoastMove;
                        break;
                    case BonusMoveType.ShadowMove:
                        specialType = Move.SpecialType.ShadowMove;
                        break;
                }
                if ((pte.pieceProperty & Piece.PieceProperty.ChargeEnhance) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ChargeEnhanceStack) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ChargeEnhanceStackReset) != 0)
                {
                    switch (pt)
                    {
                        case Piece.PieceType.QueenLeech:
                            specialType = SpecialType.Spawn;
                            break;
                        case Piece.PieceType.SoulDevourer:
                            specialType = Move.SpecialType.ChargeMove;
                            break;
                        case Piece.PieceType.SoulCannon:
                        case Piece.PieceType.ChargeCannon:
                            specialType = Move.SpecialType.FireCaptureOnly;
                            break;
                        case Piece.PieceType.DivineArtisan:
                        case Piece.PieceType.DivineApprentice:
                            specialType = Move.SpecialType.ChargeApplyModifier;
                            break;
                        case Piece.PieceType.ChargeWarper:
                            specialType = Move.SpecialType.TeleportOpposite;
                            break;
                        default:
                            specialType = Move.SpecialType.ChargeMove;
                            break;
                    }
                }

                //extra special pieces
                switch (pt)
                {
                    case PieceType.WarpWeaver:
                        specialType = SpecialType.AimOccupied;
                        break;
                    case PieceType.Cannon:
                        specialType = SpecialType.AimEnemy;
                        break;
                    case PieceType.MegaCannon:
                        specialType = SpecialType.AimEnemy;
                        break;
                    case PieceType.SteelGolem:
                        specialType = SpecialType.AimEnemy;
                        break;
                    case PieceType.SteelPuppet:
                        specialType = SpecialType.AimEnemy;
                        break;
                    case PieceType.MetalFox:
                        specialType = SpecialType.AimAny;
                        break;
                }
            }
        }

        if ((mgie.modifier & MoveGeneratorPreModifier.k) != 0)
        {
            specialType = SpecialType.KingAttack;
        }

        //special stuff
        switch (pte.type)
        {
            case Piece.PieceType.Disguiser:
                specialType = SpecialType.MorphIntoTarget;
                break;
            case Piece.PieceType.Bolter:
                specialType = SpecialType.Withdrawer;
                break;
            case Piece.PieceType.Shocker:
                specialType = SpecialType.Advancer;
                break;
            case Piece.PieceType.Aries:
                specialType = SpecialType.AdvancerPush;
                break;
            case Piece.PieceType.LightningElemental:
                specialType = SpecialType.AdvancerWithdrawer;
                break;
            case Piece.PieceType.PythonQueen:
                if (specialType == SpecialType.CaptureOnly)
                {
                    specialType = SpecialType.InflictCaptureOnly;
                }
                else
                {
                    specialType = SpecialType.PoisonFlankingAdvancer;
                }
                break;
            case PieceType.FloatMage:
            case PieceType.GravityMage:
                if (specialType == SpecialType.Inflict)
                {
                    specialType = SpecialType.InflictShift;
                }
                if (specialType == SpecialType.InflictCaptureOnly)
                {
                    specialType = SpecialType.InflictShiftCaptureOnly;
                }
                break;
            case PieceType.RabbitQueen:
                if (specialType == SpecialType.EnemyAbility)
                {
                    specialType = SpecialType.ConvertRabbit;
                }
                else if (specialType == SpecialType.CaptureOnly)
                {
                    specialType = SpecialType.MorphRabbit;
                }
                break;
            case PieceType.RabbitDiplomat:
            case PieceType.RabbitKnight:
                if (specialType == SpecialType.EnemyAbility)
                {
                    specialType = SpecialType.MorphRabbit;
                }
                break;
        }

        //code these as extra move types?
        if ((mgie.modifier & MoveGeneratorPreModifier.i) != 0)
        {
            spi |= PieceMovePanelSquareScript.SpecialIndication.Initial;
        }
        specialIndications |= spi;
        specialTypes.Add(specialType);

        int deltaX = 0;
        int deltaY = 0;

        //then do the same thing the move generator does mostly
        switch (mgie.atom)
        {
            case MoveGeneratorAtom.R:
            case MoveGeneratorAtom.W:
                if (!directionRestricted)
                {
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, 1);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, -1);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, 0);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, 0);
                }
                else
                {
                    if ((mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, 1);
                    }
                    if ((mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, -1);
                    }
                    if ((mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, 0);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, 0);
                    }
                }
                break;
            case MoveGeneratorAtom.D:
                if (!directionRestricted)
                {
                    MakeTrail(0, 0, 0, 2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 1, 0, 1, 0, false);
                    MakeTrail(0, 0, 0, -2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, 0, -1, 0, -1, 0, false);
                    MakeTrail(0, 0, 2, 0, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, 1, 0, 1, 0, 0, false);
                    MakeTrail(0, 0, -2, 0, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, -1, 0, -1, 0, 0, false);
                }
                else
                {
                    //ray up
                    //Up is allowed if F is set or V is set
                    if ((mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        MakeTrail(0, 0, 0, 2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 1, 0, 1, 0, false);
                    }
                    //ray down
                    //Down is allowed if B is set or V is set
                    if ((mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        MakeTrail(0, 0, 0, -2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, 0, -1, 0, -1, 0, false);
                    }
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if ((mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        MakeTrail(0, 0, -2, 0, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, -1, 0, -1, 0, 0, false);
                        MakeTrail(0, 0, 2, 0, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, 1, 0, 1, 0, 0, false);
                    }
                }
                break;
            case MoveGeneratorAtom.B:
            case MoveGeneratorAtom.F:
                if (!directionRestricted)
                {
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, 1);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, 1);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, -1);
                    GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, -1);
                }
                else
                {
                    if ((mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, 1);
                    }
                    if ((mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 1, -1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -1, -1);
                    }
                }
                break;
            case MoveGeneratorAtom.A:
                if (!directionRestricted)
                {
                    MakeTrail(0, 0, 2, 2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, 1, 1, 1, 1, 0, false);
                    MakeTrail(0, 0, -2, 2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, -1, 1, -1, 1, 0, false);
                    MakeTrail(0, 0, 2, -2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, 1, -1, 1, -1, 0, false);
                    MakeTrail(0, 0, -2, -2, specialType);
                    GenerateOffsetRay(mgie, specialType, spi, -1, -1, -1, -1, 0, false);
                }
                else
                {
                    if ((mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        MakeTrail(0, 0, 2, 2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, 1, 1, 1, 1, 0, false);
                        MakeTrail(0, 0, -2, 2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, -1, 1, -1, 1, 0, false);
                    }
                    if ((mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        MakeTrail(0, 0, 2, -2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, 1, -1, 1, -1, 0, false);
                        MakeTrail(0, 0, -2, -2, specialType);
                        GenerateOffsetRay(mgie, specialType, spi, -1, -1, -1, -1, 0, false);
                    }
                }
                break;
            case MoveGeneratorAtom.Z:
                //Note: incompatible with Winged modifier because the mixed move types are hard to keep track of
                //crooked bishop
                //needs a custom ray thing
                //2 different steps
                //Problem: Duplicate steps
                //I can try to remove those by combining 2 different rays into the same function
                //(i.e. the up then right and the right then up are combined)
                //(The first step will have overlaps but that is less bad than overlaps every 2 steps)
                //ray up
                //Up is allowed if F is set or V is set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 1, -1, 1);
                }
                //ray down
                //Down is allowed if B is set or V is set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, -1, -1, -1);
                }
                //ray left
                //Left is allowed if H is set
                //Note that currently there is no left right asymmetry allowed in movesets
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, -1, 1, -1, -1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 1, 1, -1);
                }
                break;
            case MoveGeneratorAtom.C:   //crooked rook
                                        //Note: incompatible with Winged modifier because the mixed move types are hard to keep track of
                                        //needs a custom ray thing
                                        //2 different steps
                                        //Problem: Duplicate steps
                                        //I can try to remove those by combining 2 different rays into the same function
                                        //(i.e. the up then right and the right then up are combined)
                                        //(The first step will have overlaps but that is less bad than overlaps every 2 steps)
                                        //ray up right
                                        //Allowed if F set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 0, 0, 1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, -1, 0, 0, 1);
                }
                //ray down right
                //Allowed if B set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 0, 0, -1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, -1, 0, 0, -1);
                }
                break;
            case MoveGeneratorAtom.I:
                //Note: incompatible with Winged modifier because the mixed move types are hard to keep track of
                //8 dual rays :P
                //Logic is slightly different for directions
                //f = front 4
                //v = vertical 4 
                //fv = front vertical 2

                //Inverted conditions

                //(1,0), (1,1) steps

                //Vertical 4
                //ray up right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 0, 1, 1, 1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 0, 1, -1, 1);
                }
                //ray down right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 0, -1, 1, -1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 0, -1, -1, -1);
                }

                //Horizontal 4
                //ray up right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 0, 1, 1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, -1, 0, -1, 1);
                }
                //ray down right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                {
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, 1, 0, 1, -1);
                    GenerateOffsetRayDual(mgie, specialType, spi, 0, 0, -1, 0, -1, -1);
                }
                break;
            case MoveGeneratorAtom.G:   //gryphon
                                        //ray up right
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                {
                    GetSquare(1, 1).Set(specialType, spi);
                    MakeTrail(0, 0, 1, 1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, 1, 0, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, 1, 1, 0, 1);
                    }
                }
                //ray up left
                //Allowed if F set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                {
                    GetSquare(-1, 1).Set(specialType, spi);
                    MakeTrail(0, 0, -1, 1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, 1, 0, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, 1, -1, 0, 1);
                    }
                }
                //ray down right
                //Allowed if B set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                {
                    GetSquare(1, -1).Set(specialType, spi);
                    MakeTrail(0, 0, 1, -1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, -1, 0, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, -1, 1, 0, 1);
                    }
                }
                //ray down left
                //Allowed if B set
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                {
                    GetSquare(-1, -1).Set(specialType, spi);
                    MakeTrail(0, 0, -1, -1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, -1, 0, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, -1, -1, 0, 1);
                    }
                }
                break;
            case MoveGeneratorAtom.M:   //manticore
                                        //Uses the offset ray logic
                                        //ray up
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                {
                    GetSquare(0, 1).Set(specialType, spi);
                    MakeTrail(0, 0, 0, 1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 1, 1, 1, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 1, -1, 1, 1);
                    }
                }
                //ray down
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                {
                    GetSquare(0, -1).Set(specialType, spi);
                    MakeTrail(0, 0, 0, -1, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, -1, 1, -1, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, -1, -1, -1, 1);
                    }
                }
                //ray left
                //Note that currently there is no left right asymmetry allowed in movesets
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                {
                    GetSquare(-1, 0).Set(specialType, spi);
                    MakeTrail(0, 0, -1, 0, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, 0, -1, 1, 1);
                    }
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -1, 0, -1, -1, 1);
                    }
                }
                //ray right
                //Note that currently there is no left right asymmetry allowed in movesets
                if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                {
                    GetSquare(1, 0).Set(specialType, spi);
                    MakeTrail(0, 0, 1, 0, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, 0, 1, 1, 1);
                    }
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 1, 0, 1, -1, 1);
                    }
                }
                break;
            case MoveGeneratorAtom.E:   //(0,2) then rook not backwards
                                        //up
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                {
                    GetSquare(0, 2).Set(specialType, spi);
                    MakeTrail(0, 0, 0, 2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 2, -1, 0, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 2, 1, 0, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 2, 0, 1, 1);
                    }
                }
                //down
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                {
                    GetSquare(0, -2).Set(specialType, spi);
                    MakeTrail(0, 0, 0, -2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, -2, -1, 0, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 0, -2, 1, 0, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, -2, 0, -1, 1);
                    }
                }
                //left
                //actually no condition
                //This looks very cursed
                {
                    GetSquare(-2, 0).Set(specialType, spi);
                    MakeTrail(0, 0, -2, 0, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, 0, 0, 1, 1);
                        GenerateOffsetRay(mgie, specialType, spi, -2, 0, 0, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, 0, -1, 0, 1);
                    }
                }
                //right
                {
                    GetSquare(2, 0).Set(specialType, spi);
                    MakeTrail(0, 0, 2, 0, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, 0, 0, 1, 1);
                        GenerateOffsetRay(mgie, specialType, spi, 2, 0, 0, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, 0, 1, 0, 1);
                    }
                }
                break;
            case MoveGeneratorAtom.J:   //(2,2) then bishop not backwards
                                        //V allows the weird ricochet things in the vertical directions (<> shape paths
                                        //H allows the weird ricochet things in the horizontal directions (V^ shape paths)

                //up left
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                {
                    GetSquare(-2, 2).Set(specialType, spi);
                    MakeTrail(0, 0, -2, 2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, 2, 1, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.f)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, 2, -1, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.h)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, 2, -1, 1, 1);
                    }
                }
                //up right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                {
                    GetSquare(2, 2).Set(specialType, spi);
                    MakeTrail(0, 0, 2, 2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, 2, -1, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.fv)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, 2, 1, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.vh)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, 2, 1, 1, 1);
                    }
                }
                //down left
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                {
                    GetSquare(-2, -2).Set(specialType, spi);
                    MakeTrail(0, 0, -2, -2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, -2, 1, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.bv)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, -2, -1, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.vh)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, -2, -2, -1, -1, 1);
                    }
                }
                //down right
                if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                {
                    GetSquare(2, -2).Set(specialType, spi);
                    MakeTrail(0, 0, 2, -2, specialType);

                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, -2, -1, -1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.bv)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, -2, 1, 1, 1);
                    }

                    if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.vh)) == 0)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 2, -2, 1, -1, 1);
                    }
                }
                break;
            case MoveGeneratorAtom.H:   //wheel
                                        //Uses the between points ray logic
                break;
            case MoveGeneratorAtom.O:   //orbiter
                                        //Need to detect orbit targets nearby
                break;
            case MoveGeneratorAtom.P:   //orbiter
                                        //Need to detect orbit targets nearby
                break;
            case MoveGeneratorAtom.S:   //Rose knight
                                        //Hardcoded
                                        //the current parsing doesn't let you put in other numbers
                                        //And also roses of other types are less interesting (board too small to see them mostly)
                GenerateRoseMoves(mgie, specialType, spi);
                break;
            case MoveGeneratorAtom.Leaper:  //leaper
                                            //Is this a 4 leaper

                deltaX = mgie.x;
                deltaY = mgie.y;

                //for symmetry I will make the X coord larger
                int ch = deltaX > deltaY ? deltaX : deltaY;
                int cl = deltaX > deltaY ? deltaY : deltaX;

                if (deltaX == 0 || deltaY == 0)
                {
                    //4 ortho leaper
                    //up
                    if (!directionRestricted)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, -ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, 0);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, 0);
                    }
                    else
                    {
                        if ((mgie.modifier & MoveGeneratorPreModifier.f) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, ch);
                        }
                        //down
                        if ((mgie.modifier & MoveGeneratorPreModifier.b) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, 0, -ch);
                        }
                        //right
                        if ((mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, 0);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, 0);
                        }
                    }
                }
                else if (deltaX == deltaY || deltaX == -deltaY)
                {
                    if (!directionRestricted)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, -ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, -ch);
                    }
                    else
                    {
                        //4 diagonal leaper
                        //ray up right
                        if ((mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, ch);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, ch);
                        }
                        //ray down right
                        if ((mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, -ch);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, -ch);
                        }
                    }
                }
                else
                {
                    if (!directionRestricted)
                    {
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, cl, ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -cl, ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, cl, -ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -cl, -ch);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, cl);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, cl);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, -cl);
                        GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, -cl);
                    }
                    else
                    {
                        if (((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, cl, ch);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -cl, ch);
                        }
                        //ray down right
                        if (((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, cl, -ch);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -cl, -ch);
                        }

                        //Horizontal 4
                        //ray up right
                        if (((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, cl);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, cl);
                        }
                        //ray down right
                        if (((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                        {
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, ch, -cl);
                            GenerateOffsetRay(mgie, specialType, spi, 0, 0, -ch, -cl);
                        }
                    }
                }
                break;
            case MoveGeneratorAtom.Castling:
                //spawns moves
                specialTypes.Add(SpecialType.Castling);
                GetSquare(2, 0).Set(SpecialType.Castling, spi);
                GetSquare(-2, 0).Set(SpecialType.Castling, spi);
                MakeTrail(0, 0, 2, 0, SpecialType.Castling);
                MakeTrail(0, 0, -2, 0, SpecialType.Castling);
                break;
            case MoveGeneratorAtom.AllyKingTeleport:
                break;
            case MoveGeneratorAtom.EnemyKingTeleport:
                break;
            case MoveGeneratorAtom.PawnSwapTeleport:
                break;
            case MoveGeneratorAtom.AllySwapTeleport:
                break;
            case MoveGeneratorAtom.AllyBehindTeleport:
                break;
            case MoveGeneratorAtom.EnemyBehindTeleport:
                break;
            case MoveGeneratorAtom.AnywhereTeleport:
                specialTypes.Add(SpecialType.MoveOnly);
                //I can actually show this now
                for (int y = -7; y <= 7; y++)
                {
                    for (int x = -7; x <= 7; x++)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                        GetSquare(x, y).Set(SpecialType.MoveOnly, spi);
                    }
                }
                break;
            case MoveGeneratorAtom.AnywhereAdjacentTeleport:
                break;
            case MoveGeneratorAtom.AnywhereNonAdjacentTeleport:
                break;
            case MoveGeneratorAtom.AnywhereSameColorTeleport:
                specialTypes.Add(SpecialType.MoveOnly);
                for (int y = -7; y <= 7; y++)
                {
                    for (int x = -7; x <= 7; x++)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        if ((14 + (x + y)) % 2 != 0)
                        {
                            continue;
                        }
                        GetSquare(x, y).Set(SpecialType.MoveOnly, spi);
                    }
                }
                break;
            case MoveGeneratorAtom.AnywhereOppositeColorTeleport:
                specialTypes.Add(SpecialType.MoveOnly);
                for (int y = -7; y <= 7; y++)
                {
                    for (int x = -7; x <= 7; x++)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        if ((14 + (x + y)) % 2 == 0)
                        {
                            continue;
                        }
                        GetSquare(x, y).Set(SpecialType.MoveOnly, spi);
                    }
                }
                break;
            case MoveGeneratorAtom.HomeRangeTeleport:
                break;
            case MoveGeneratorAtom.KingSwapTeleport:
                break;
            case MoveGeneratorAtom.MirrorTeleport:
                break;
            case MoveGeneratorAtom.MirrorTeleportSwap:
                break;
            case MoveGeneratorAtom.VerticalMirrorTeleport:
                break;
            case MoveGeneratorAtom.BlossomTeleport:
                break;
            case MoveGeneratorAtom.ForestTeleport:
                break;
            case MoveGeneratorAtom.DiplomatTeleport:
                break;
            case MoveGeneratorAtom.EchoTeleport:
                break;
            case MoveGeneratorAtom.CoastTeleport:
                break;
            case MoveGeneratorAtom.AimMover:
                break;
            case MoveGeneratorAtom.LensRook:
                break;
            case MoveGeneratorAtom.Recall:
                break;
        }
    }

    private void GenerateRoseMoves(MoveGeneratorInfoEntry mgie, SpecialType specialType, PieceMovePanelSquareScript.SpecialIndication spi)
    {
        int deltaX = 1;
        int deltaY = 2;
        int[][] coordList = new int[8][];

        coordList[0] = new int[2];
        coordList[1] = new int[2];
        coordList[2] = new int[2];
        coordList[3] = new int[2];
        coordList[4] = new int[2];
        coordList[5] = new int[2];
        coordList[6] = new int[2];
        coordList[7] = new int[2];

        coordList[0][0] = deltaX;
        coordList[0][1] = deltaY;
        coordList[1][0] = deltaY;
        coordList[1][1] = deltaX;

        coordList[2][0] = deltaY;
        coordList[2][1] = -deltaX;
        coordList[3][0] = deltaX;
        coordList[3][1] = -deltaY;

        coordList[4][0] = -deltaX;
        coordList[4][1] = -deltaY;
        coordList[5][0] = -deltaY;
        coordList[5][1] = -deltaX;

        coordList[6][0] = -deltaY;
        coordList[6][1] = deltaX;
        coordList[7][0] = -deltaX;
        coordList[7][1] = deltaY;

        GenerateRoseRay(mgie, specialType, spi, coordList, 0);
        GenerateRoseRay(mgie, specialType, spi, coordList, 1);
        GenerateRoseRay(mgie, specialType, spi, coordList, 2);
        GenerateRoseRay(mgie, specialType, spi, coordList, 3);
        GenerateRoseRay(mgie, specialType, spi, coordList, 4);
        GenerateRoseRay(mgie, specialType, spi, coordList, 5);
        GenerateRoseRay(mgie, specialType, spi, coordList, 6);
        GenerateRoseRay(mgie, specialType, spi, coordList, 7);
    }

    private void GenerateRoseRay(MoveGeneratorInfoEntry mgie, SpecialType specialType, PieceMovePanelSquareScript.SpecialIndication spi, int[][] coordList, int index)
    {
        int iterations = 0;
        int tx = 0;
        int ty = 0;

        int lastX = 0;
        int lastY = 0;

        while (iterations < 8)
        {
            iterations++;
            if (iterations >= 8)
            {
                break;
            }

            tx += coordList[index][0];
            ty += coordList[index][1];
            index++;
            if (index >= 8)
            {
                index -= 8;
            }

            if (tx < -7 || tx > 7 || ty < -7 || ty > 7)
            {
                break;
            }
            GetSquare(tx, ty).Set(specialType, spi);

            MakeTrail(lastX, lastY, tx, ty, specialType);
            lastX = tx;
            lastY = ty;
        }
    }

    private void GenerateOffsetRay(MoveGeneratorInfoEntry mgie, SpecialType specialType, PieceMovePanelSquareScript.SpecialIndication spi, int x, int y, int dx, int dy, int lostRange = 0, bool startTrail = true)
    {
        int effectiveRange = mgie.range;
        if (effectiveRange == 0)
        {
            effectiveRange = 16;
        }
        effectiveRange -= lostRange;
        if (effectiveRange <= 0)
        {
            return;
        }

        int lastX = x;
        int lastY = y;
        int tx = 0;
        int ty = 0;
        switch (mgie.rangeType)
        {
            case RangeType.Normal:
            case RangeType.AntiRange:
                for (int i = 0; i < effectiveRange; i++)
                {
                    tx = x + dx * (i + 1);
                    ty = y + (dy * (i + 1));
                    if (tx < -7 || tx > 7 || ty < -7 || ty > 7)
                    {
                        continue;
                    }
                    GetSquare(tx, ty).Set(specialType, spi);
                    if (startTrail || (lastX != x || lastY != y))
                    {
                        MakeTrail(lastX, lastY, tx, ty, specialType);
                    }
                    lastX = tx;
                    lastY = ty;
                }
                break;
            case RangeType.Exact:
                for (int i = 0; i < effectiveRange; i++)
                {
                    tx = x + dx * (i + 1);
                    ty = y + (dy * (i + 1));
                    if (tx < -7 || tx > 7 || ty < -7 || ty > 7)
                    {
                        continue;
                    }
                    if (i + 1 == effectiveRange)
                    {
                        GetSquare(tx, ty).Set(specialType, spi);
                    }
                    if (startTrail || (lastX != x || lastY != y))
                    {
                        MakeTrail(lastX, lastY, tx, ty, specialType);
                    }
                    lastX = tx;
                    lastY = ty;
                }
                break;
            case RangeType.Minimum:
                for (int i = 0; i < 7; i++)
                {
                    tx = x + dx * (i + 1);
                    ty = y + (dy * (i + 1));
                    if (tx < -7 || tx > 7 || ty < -7 || ty > 7)
                    {
                        continue;
                    }
                    if (startTrail || (lastX != x || lastY != y))
                    {
                        MakeTrail(lastX, lastY, tx, ty, specialType);
                    }
                    lastX = tx;
                    lastY = ty;
                    if (i < effectiveRange - 1)
                    {
                        continue;
                    }
                    GetSquare(tx, ty).Set(specialType, spi);
                }
                break;
        }
    }
    private void GenerateOffsetRayDual(MoveGeneratorInfoEntry mgie, SpecialType specialType, PieceMovePanelSquareScript.SpecialIndication spi, int x, int y, int dxA, int dyA, int dxB, int dyB)
    {
        int effectiveRange = mgie.range;
        if (effectiveRange == 0)
        {
            effectiveRange = 16;
        }

        int lastX = x;
        int lastY = y;
        int tx = 0;
        int ty = 0;
        switch (mgie.rangeType)
        {
            case RangeType.Normal:
            case RangeType.AntiRange:
                int r = 0;
                for (int i = 0; i < effectiveRange; i++)
                {
                    r++;
                    if (r > effectiveRange)
                    {
                        break;
                    }
                    tx = x + (dxA + dxB) * (i) + dxA;
                    ty = y + (dyA + dyB) * (i) + dyA;
                    if (tx >= -7 && tx <= 7 && ty >= -7 && ty <= 7)
                    {
                        GetSquare(tx, ty).Set(specialType, spi);
                        MakeTrail(lastX, lastY, tx, ty, specialType);
                    }

                    tx = x + (dxA + dxB) * (i) + dxB;
                    ty = y + (dyA + dyB) * (i) + dyB;
                    if (tx >= -7 && tx <= 7 && ty >= -7 && ty <= 7)
                    {
                        GetSquare(tx, ty).Set(specialType, spi);
                        MakeTrail(lastX, lastY, tx, ty, specialType);
                    }

                    r++;
                    if (r > effectiveRange)
                    {
                        break;
                    }
                    tx = x + (dxA + dxB) * (i + 1);
                    ty = y + (dyA + dyB) * (i + 1);
                    if (tx < -7 || tx > 7 || ty < -7 || ty > 7)
                    {
                        break;
                    }
                    GetSquare(tx, ty).Set(specialType, spi);
                    MakeTrail(x + (dxA + dxB) * (i) + dxA, y + (dyA + dyB) * (i) + dyA, tx, ty, specialType);
                    MakeTrail(x + (dxA + dxB) * (i) + dxB, y + (dyA + dyB) * (i) + dyB, tx, ty, specialType);
                    lastX = tx;
                    lastY = ty;
                }
                break;
        }
    }

    public void MakeTrail(int xa, int ya, int xb, int yb, Move.SpecialType sp)
    {
        GameObject go = Instantiate(trailTemplate, trailHolder.transform);
        PieceMovePanelTrailScript pmpts = go.GetComponent<PieceMovePanelTrailScript>();

        bool leapTrail = false;
        if (xa - xb > 1 || xa - xb < -1 || ya - yb > 1 || ya - yb < -1)
        {
            leapTrail = true;
        }

        Color c = GetColorFromSpecialType(sp);
        c = Color.Lerp(c, Color.white, 0.2f); //new Color(0.2f + (c.r) * 0.8f, 0.2f + (c.g) * 0.8f, 0.2f + (c.b) * 0.8f, 0.5f);
        if (leapTrail)
        {
            Vector3 cross = Vector3.Cross(Vector3.forward, GetSquare(xa, ya).rectTransform.localPosition - GetSquare(xb, yb).rectTransform.localPosition);
            cross = cross.normalized;
            float distance = (GetSquare(xa, ya).rectTransform.localPosition - GetSquare(xb, yb).rectTransform.localPosition).magnitude;
            Vector3 midpoint = Vector3.Lerp(GetSquare(xa, ya).rectTransform.localPosition, GetSquare(xb, yb).rectTransform.localPosition, 0.5f);
            midpoint += cross * distance * 0.1f;
            pmpts.Setup(GetSquare(xa, ya).rectTransform.localPosition, midpoint, c);
            lines.Add(pmpts);

            go = Instantiate(trailTemplate, trailHolder.transform);
            PieceMovePanelTrailScript pmptsB = go.GetComponent<PieceMovePanelTrailScript>();
            pmptsB.Setup(midpoint, GetSquare(xb, yb).rectTransform.localPosition, c);
            lines.Add(pmptsB);
        }
        else
        {
            pmpts.Setup(GetSquare(xa, ya).rectTransform.localPosition, GetSquare(xb, yb).rectTransform.localPosition, c);
            lines.Add(pmpts);
        }
    }
    public PieceMovePanelSquareScript GetSquare(int x, int y)
    {
        return grid[y + 7][x + 7];
    }

    //todo: clean up and make these distinct
    //todo also: make unique icons for the weirder move types
    public static Color GetColorFromSpecialType(Move.SpecialType sp)
    {
        switch (sp)
        {
            case Move.SpecialType.Normal:
                return new Color(0.4f, 0.4f, 0.4f, 1);
            case Move.SpecialType.MoveOnly:
            case Move.SpecialType.FlyingMoveOnly:
                return new Color(0.4f, 0.4f, 1f, 1);
            case Move.SpecialType.CaptureOnly:
                return new Color(1f, 0.4f, 0.4f, 1);
            case Move.SpecialType.ConsumeAllies:
                return new Color(0.6f, 0.5f, 0.4f, 1);
            case Move.SpecialType.ConsumeAlliesCaptureOnly:
                return new Color(1f, 0.6f, 0.4f, 1);
            case Move.SpecialType.SelfMove:
            case Move.SpecialType.EnemyAbility:
            case Move.SpecialType.EmptyAbility:
            case Move.SpecialType.AllyAbility:
            case Move.SpecialType.ChargeMove:
            case Move.SpecialType.ChargeMoveReset:
            case Move.SpecialType.RangedPushAllyOnly:
            case Move.SpecialType.RangedPush:
            case Move.SpecialType.ImbueModifier:
            case Move.SpecialType.ImbuePromote:
            case Move.SpecialType.InflictFreeze:
            case Move.SpecialType.Inflict:
            case Move.SpecialType.InflictShift:
            case Move.SpecialType.CarryAlly:
            case Move.SpecialType.ChargeApplyModifier:
                return new Color(0.4f, 0.9f, 0.9f, 1);
            case Move.SpecialType.MorphIntoTarget:
            case Move.SpecialType.MorphRabbit:
            case Move.SpecialType.RangedPull:
            case Move.SpecialType.RangedPullAllyOnly:
                return new Color(0.25f, 0.7f, 0.7f, 1);
            case Move.SpecialType.AllySwap:
            case Move.SpecialType.AnyoneSwap:
            case Move.SpecialType.TeleportOpposite:
            case Move.SpecialType.TeleportRecall:
            case Move.SpecialType.TeleportMirror:
                return new Color(0.9f, 0.4f, 0.9f, 1);
            case Move.SpecialType.InflictShiftCaptureOnly:
            case Move.SpecialType.InflictFreezeCaptureOnly:
            case Move.SpecialType.InflictCaptureOnly:
                return new Color(0.7f, 0.9f, 0.9f, 1);
            case Move.SpecialType.AimEnemy:
            case Move.SpecialType.AimAny:
            case Move.SpecialType.AimOccupied:
            case Move.SpecialType.KingAttack:
                return new Color(1f, 0.6f, 0.6f, 1);
            case Move.SpecialType.PassiveAbility:
                return new Color(0.4f, 1f, 1f, 1);
            case Move.SpecialType.Castling:
                return new Color(0.8f, 0.8f, 0f, 1);
            case Move.SpecialType.ConvertRabbit:
            case Move.SpecialType.Convert:
            case Move.SpecialType.ConvertCaptureOnly:
            case Move.SpecialType.ConvertPawn:
                return new Color(1f, 1f, 0.4f, 1);
            case Move.SpecialType.FireCapture:
                return new Color(0.7f, 0.5f, 0.4f, 1);
            case Move.SpecialType.FireCaptureOnly:
                return new Color(1f, 0.7f, 0.4f, 1);
            case Move.SpecialType.AmoebaCombine:
                return new Color(0.4f, 1f, 0.8f, 1);
            case Move.SpecialType.DepositAlly:
            case Move.SpecialType.Spawn:
                return new Color(0.4f, 1f, 0.4f, 1);
            case Move.SpecialType.LongLeaper:
                return new Color(0.6f, 0.5f, 0.4f, 1);
            case Move.SpecialType.LongLeaperCaptureOnly:
                return new Color(1f, 0.5f, 0.4f, 1);
            case Move.SpecialType.FireCapturePush:
                return new Color(0.7f, 0.6f, 0.4f, 1);
            case Move.SpecialType.PullMove:
            case Move.SpecialType.PushMove:
                return new Color(0.5f, 0.5f, 0.4f, 1);
            case Move.SpecialType.AdvancerPush:
            case Move.SpecialType.Advancer:
            case Move.SpecialType.Withdrawer:
            case Move.SpecialType.AdvancerWithdrawer:
            case Move.SpecialType.FlankingCapturer:
            case Move.SpecialType.PoisonFlankingAdvancer:
                return new Color(0.6f, 0.5f, 0.4f, 1);
            case Move.SpecialType.WrathCapturer:
                return new Color(0.6f, 0.4f, 0.4f, 1);
            case Move.SpecialType.DepositAllyPlantMove:
            case Move.SpecialType.PlantMove:
                return new Color(0.4f, 0.5f, 0.4f, 1);
            case Move.SpecialType.SlipMove:
                return new Color(0.3f, 0.3f, 0.3f, 1);
            case Move.SpecialType.GliderMove:
                return new Color(1f, 0.8f, 0.6f, 1);
            case Move.SpecialType.ShadowMove:
                return new Color(0.2f, 0.2f, 0.2f, 1);
            case Move.SpecialType.CoastMove:
                return new Color(0.4f, 0.7f, 1f, 1);
        }
        return new Color(0.4f, 0.4f, 0.4f, 1);
    }
}
