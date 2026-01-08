using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Move;
using static MoveGeneratorInfoEntry;
using static Piece;

public sealed class GlobalPieceManager : MonoBehaviour
{
    private static GlobalPieceManager intInstance;
    public static GlobalPieceManager Instance
    {
        get
        {
            //optimization?
            /*
            if (intInstance == null)
            {
                intInstance = FindAnyObjectByType<GlobalPieceManager>();
            }
            */
            return intInstance;
        }
    }

    //static for optimization?
    public static PieceTableEntry[] pieceTable;

    public PieceClassEntry[] pieceClassTable;

    public static float[] pieceSquareTableCenter;  //for most pieces
    public static float[] pieceSquareTableCorner;   //for king
    public static float[] pieceSquareTableTopCenter;   //for promotable things
    public static float[] pieceSquareTableTopCenterKing;   //for king in endgame (extreme top values because of the top victory condition)

    public static int[][] orbiterDeltas;
    public static Dir[][] orbiterDirections;
    public static Dir[][] orbiterDirectionsRadial;

    public static int[][] orbiterDeltas2;
    public static Dir[][] orbiterDirections2;
    public static Dir[][] orbiterDirectionsRadial2;

    public static ulong[] raysRight;
    public static ulong[] raysLeft;
    public static ulong[] raysUp;
    public static ulong[] raysDown;
    public static ulong[] raysUR;
    public static ulong[] raysUL;
    public static ulong[] raysDR;
    public static ulong[] raysDL;

    //King value is given a big number as a flag to easily get a king count
    //I don't necessarily want to put this in a separate variable as that would be a lot of "if type == king" checks everywhere which is annoying and potentially slows things down
    //This is an easier way
    //Note that the game will break if you put too many kings on the board at the same time as it will overflow the short counter
    //1024 might also be reachable if the board is filled with highly valued pieces? So I chose 2048 instead
    public const short KING_VALUE_BONUS = 1 << 11;  //2048
    public const short KING_VALUE_BONUS_MINUS_ONE = (1 << 11) - 1;  //2047

    public MoveGeneratorInfoEntry defensiveModifierMove;
    public MoveGeneratorInfoEntry recallModifierMove;
    public MoveGeneratorInfoEntry mirrorModifierMove;
    public MoveGeneratorInfoEntry seafaringModifierMove;
    public MoveGeneratorInfoEntry backdoorModifierMove;
    public MoveGeneratorInfoEntry forestModifierMove;
    public MoveGeneratorInfoEntry flyingGeneralModifierMove;

    public Material[] pieceMaterials;
    public Material[] guiPieceMaterials;

    public void Awake()
    {
        intInstance = this;
        LoadPieceTable();
        LoadPieceClassTable();
        LoadMaterials();

        LoadPieceSquareTables();
        LoadOrbiterDirections();
        LoadRays();
        defensiveModifierMove = new MoveGeneratorInfoEntry();
        defensiveModifierMove.atom = MoveGeneratorAtom.R;
        defensiveModifierMove.modifier |= MoveGeneratorPreModifier.m;
        defensiveModifierMove.modifier |= MoveGeneratorPreModifier.b;

        recallModifierMove = new MoveGeneratorInfoEntry();
        recallModifierMove.atom = MoveGeneratorAtom.Recall;

        mirrorModifierMove = new MoveGeneratorInfoEntry();
        mirrorModifierMove.atom = MoveGeneratorAtom.MirrorTeleport;

        seafaringModifierMove = new MoveGeneratorInfoEntry();
        seafaringModifierMove.atom = MoveGeneratorAtom.MirrorTeleport;

        backdoorModifierMove = new MoveGeneratorInfoEntry();
        backdoorModifierMove.atom = MoveGeneratorAtom.VerticalMirrorTeleport;

        forestModifierMove = new MoveGeneratorInfoEntry();
        forestModifierMove.atom = MoveGeneratorAtom.ForestTeleport;

        flyingGeneralModifierMove = new MoveGeneratorInfoEntry();
        flyingGeneralModifierMove.atom = MoveGeneratorAtom.R;
        flyingGeneralModifierMove.modifier = MoveGeneratorPreModifier.k;
    }

    public void LoadMaterials()
    {
        pieceMaterials = new Material[10];
        pieceMaterials[0] = Resources.Load<Material>("Materials/Piece/Normal");
        pieceMaterials[1] = Resources.Load<Material>("Materials/Piece/Vengeful");
        pieceMaterials[2] = Resources.Load<Material>("Materials/Piece/Phoenix");
        pieceMaterials[3] = Resources.Load<Material>("Materials/Piece/Radiant");
        pieceMaterials[4] = Resources.Load<Material>("Materials/Piece/Winged");
        pieceMaterials[5] = Resources.Load<Material>("Materials/Piece/Spectral");
        pieceMaterials[6] = Resources.Load<Material>("Materials/Piece/Immune");
        pieceMaterials[7] = Resources.Load<Material>("Materials/Piece/Warped");
        pieceMaterials[8] = Resources.Load<Material>("Materials/Piece/Shielded");
        pieceMaterials[9] = Resources.Load<Material>("Materials/Piece/HalfShielded");

        guiPieceMaterials = new Material[10];
        guiPieceMaterials[0] = Resources.Load<Material>("Materials/Piece/Normal_GUI");
        guiPieceMaterials[1] = Resources.Load<Material>("Materials/Piece/Vengeful_GUI");
        guiPieceMaterials[2] = Resources.Load<Material>("Materials/Piece/Phoenix_GUI");
        guiPieceMaterials[3] = Resources.Load<Material>("Materials/Piece/Radiant_GUI");
        guiPieceMaterials[4] = Resources.Load<Material>("Materials/Piece/Winged_GUI");
        guiPieceMaterials[5] = Resources.Load<Material>("Materials/Piece/Spectral_GUI");
        guiPieceMaterials[6] = Resources.Load<Material>("Materials/Piece/Immune_GUI");
        guiPieceMaterials[7] = Resources.Load<Material>("Materials/Piece/Warped_GUI");
        guiPieceMaterials[8] = Resources.Load<Material>("Materials/Piece/Shielded_GUI");
        guiPieceMaterials[9] = Resources.Load<Material>("Materials/Piece/HalfShielded_GUI");
    }

    public void LoadRays()
    {
        raysRight = new ulong[64];
        raysLeft = new ulong[64];
        raysUp = new ulong[64];
        raysDown = new ulong[64];
        raysUR = new ulong[64];
        raysUL = new ulong[64];
        raysDR = new ulong[64];
        raysDL = new ulong[64];

        for (int xy = 0; xy < 64; xy++)
        {
            int x = xy & 7;
            int y = xy >> 3;
            ulong bitindex = 1uL << xy;

            if (x != 7)
            {
                //xy < 63
                //note: x == 7 is true at position 63 so that check is unnecessary
                raysRight[xy] = (MoveGenerator.BITBOARD_PATTERN_RANK1 << (xy + 1)) & MainManager.GetWraparoundCutoff(x + 1);
            }
            if (x != 0)
            {
                if (xy >= 8)
                {
                    raysLeft[xy] = (MoveGenerator.BITBOARD_PATTERN_RANK1 << (xy - 8)) & MainManager.GetWraparoundCutoff(x - 8);
                }
                else
                {
                    raysLeft[xy] = (MoveGenerator.BITBOARD_PATTERN_RANK1 >> (8 - xy)) & MainManager.GetWraparoundCutoff(x - 8);
                }
            }
            if (y < 7)
            {
                raysUp[xy] = MoveGenerator.BITBOARD_PATTERN_AFILE << (xy + 8);
            }
            if (y > 0)
            {
                raysDown[xy] = MoveGenerator.BITBOARD_PATTERN_AFILE >> (64 - xy);
            }

            raysUR[xy] = ((MoveGenerator.BITBOARD_PATTERN_DIAGONAL << (xy)) & MainManager.GetWraparoundCutoff(x)) & ~bitindex;

            raysDL[xy] = ((MoveGenerator.BITBOARD_PATTERN_DIAGONAL >> (63 - xy)) & MainManager.GetWraparoundCutoff(x - 7)) & ~bitindex;

            if (xy <= 56)
            {
                raysDR[xy] = ((MoveGenerator.BITBOARD_PATTERN_ANTIDIAGONAL >> (56 - xy)) & MainManager.GetWraparoundCutoff(x)) & ~bitindex;
            }
            else
            {
                raysDR[xy] = ((MoveGenerator.BITBOARD_PATTERN_ANTIDIAGONAL << (xy - 56)) & MainManager.GetWraparoundCutoff(x)) & ~bitindex;
            }
            if (xy <= 7)
            {
                raysUL[xy] = ((MoveGenerator.BITBOARD_PATTERN_ANTIDIAGONAL >> (7 - xy)) & MainManager.GetWraparoundCutoff(x - 7)) & ~bitindex;
            }
            else
            {
                raysUL[xy] = ((MoveGenerator.BITBOARD_PATTERN_ANTIDIAGONAL << (xy - 7)) & MainManager.GetWraparoundCutoff(x - 7)) & ~bitindex;
            }
        }

        /*
                                        //ray right
                                ulong rayBitboard = 0;
                                ulong hitBitboard = 0;
                                if (x != 7)
                                {
                                    //xy < 63
                                    //note: x == 7 is true at position 63 so that check is unnecessary
                                    rayBitboard = (BITBOARD_PATTERN_RANK1 << (xy + 1)) & MainManager.GetWraparoundCutoff(x + 1);
                                    hitBitboard = rayBitboard & blockerBitboard;
                                    int hit = MainManager.PopBitboardLSB1(hitBitboard);
                                }

                                //ray left
                                if (x != 0)
                                {
                                    if (xy >= 8)
                                    {
                                        rayBitboard = (BITBOARD_PATTERN_RANK1 << (xy - 8)) & MainManager.GetWraparoundCutoff(x - 8);
                                        hitBitboard = rayBitboard & blockerBitboard;
                                    }
                                    else
                                    {
                                        rayBitboard = (BITBOARD_PATTERN_RANK1 >> (8 - xy)) & MainManager.GetWraparoundCutoff(x - 8);
                                        hitBitboard = rayBitboard & blockerBitboard;
                                    }
                                }

                                //ray up
                                if (y < 7)
                                {
                                    rayBitboard = BITBOARD_PATTERN_AFILE << (xy + 8);
                                    hitBitboard = rayBitboard & blockerBitboard;
                                }

                                //ray down
                                if (y > 0)
                                {
                                    rayBitboard = BITBOARD_PATTERN_AFILE >> (64 - xy);
                                    hitBitboard = rayBitboard & blockerBitboard;
                                }

                                /*
                                ulong lustfulBitboard = 0;
                                lustfulBitboard |= BITBOARD_PATTERN_AFILE << (subX);
                                lustfulBitboard |= BITBOARD_PATTERN_RANK1 << (subY << 3);
                                lustfulBitboard |= (BITBOARD_PATTERN_DIAGONAL << (i)) & MainManager.GetWraparoundCutoff(subX);
                                lustfulBitboard |= (BITBOARD_PATTERN_DIAGONAL >> (63 - i)) & MainManager.GetWraparoundCutoff(subX - 7);
                                if (i <= 56)
                                {
                                    lustfulBitboard |= (BITBOARD_PATTERN_ANTIDIAGONAL >> (56 - i)) & MainManager.GetWraparoundCutoff(subX);
                                }
                                else
                                {
                                    lustfulBitboard |= (BITBOARD_PATTERN_ANTIDIAGONAL << (i - 56)) & MainManager.GetWraparoundCutoff(subX);
                                }
                                if (i <= 7)
                                {
                                    lustfulBitboard |= (BITBOARD_PATTERN_ANTIDIAGONAL >> (7 - i)) & MainManager.GetWraparoundCutoff(subX - 7);
                                }
                                else
                                {
                                    lustfulBitboard |= (BITBOARD_PATTERN_ANTIDIAGONAL << (i - 7)) & MainManager.GetWraparoundCutoff(subX - 7);
                                }
                                */
    }

    public void LoadOrbiterDirections()
    {
        orbiterDeltas = new int[8][];
        orbiterDeltas[0] = new int[2] { 0, 1 };
        orbiterDeltas[1] = new int[2] { 1, 1 };
        orbiterDeltas[2] = new int[2] { 1, 0 };
        orbiterDeltas[3] = new int[2] { 1, -1 };
        orbiterDeltas[4] = new int[2] { 0, -1 };
        orbiterDeltas[5] = new int[2] { -1, -1 };
        orbiterDeltas[6] = new int[2] { -1, 0 };
        orbiterDeltas[7] = new int[2] { -1, 1 };

        orbiterDeltas2 = new int[12][];
        orbiterDeltas2[0] = new int[2] { 0, 2 };
        orbiterDeltas2[1] = new int[2] { 1, 2 };
        orbiterDeltas2[2] = new int[2] { 2, 1 };
        orbiterDeltas2[3] = new int[2] { 2, 0 };
        orbiterDeltas2[4] = new int[2] { 2, -1 };
        orbiterDeltas2[5] = new int[2] { 1, -2 };
        orbiterDeltas2[6] = new int[2] { 0, -2 };
        orbiterDeltas2[7] = new int[2] { -1, -2 };
        orbiterDeltas2[8] = new int[2] { -2, -1 };
        orbiterDeltas2[9] = new int[2] { -2, 0 };
        orbiterDeltas2[10] = new int[2] { -2, 1 };
        orbiterDeltas2[11] = new int[2] { -1, 2 };

        orbiterDirections = new Dir[8][];
        orbiterDirections[0] = new Dir[2] { Dir.Right, Dir.Left };
        orbiterDirections[1] = new Dir[2] { Dir.DownRight, Dir.UpLeft };
        orbiterDirections[2] = new Dir[2] { Dir.Down, Dir.Up };
        orbiterDirections[3] = new Dir[2] { Dir.DownLeft, Dir.UpRight };
        orbiterDirections[4] = new Dir[2] { Dir.Left, Dir.Right };
        orbiterDirections[5] = new Dir[2] { Dir.UpLeft, Dir.DownRight };
        orbiterDirections[6] = new Dir[2] { Dir.Up, Dir.Down };
        orbiterDirections[7] = new Dir[2] { Dir.UpRight, Dir.DownLeft };

        orbiterDirections2 = new Dir[12][];
        orbiterDirections2[0] = new Dir[2] { Dir.Right, Dir.Left };
        orbiterDirections2[1] = new Dir[2] { Dir.DownRight, Dir.UpLeft };
        orbiterDirections2[2] = new Dir[2] { Dir.DownRight, Dir.UpLeft };
        orbiterDirections2[3] = new Dir[2] { Dir.Down, Dir.Up };
        orbiterDirections2[4] = new Dir[2] { Dir.DownLeft, Dir.UpRight };
        orbiterDirections2[5] = new Dir[2] { Dir.DownLeft, Dir.UpRight };
        orbiterDirections2[6] = new Dir[2] { Dir.Left, Dir.Right };
        orbiterDirections2[7] = new Dir[2] { Dir.UpLeft, Dir.DownRight };
        orbiterDirections2[8] = new Dir[2] { Dir.UpLeft, Dir.DownRight };
        orbiterDirections2[9] = new Dir[2] { Dir.Up, Dir.Down };
        orbiterDirections2[10] = new Dir[2] { Dir.UpRight, Dir.DownLeft };
        orbiterDirections2[11] = new Dir[2] { Dir.UpRight, Dir.DownLeft };

        orbiterDirectionsRadial = new Dir[8][];
        orbiterDirectionsRadial2 = new Dir[8][];
        for (int i = 0; i < 8; i++)
        {
            orbiterDirectionsRadial[i] = new Dir[2];
            orbiterDirectionsRadial[i][0] = Move.DeltaToDir(orbiterDeltas[i][0], orbiterDeltas[i][1]);
            orbiterDirectionsRadial[i][1] = Move.ReverseDir(orbiterDirectionsRadial[i][0]);

            orbiterDirectionsRadial2[i] = new Dir[2];
            orbiterDirectionsRadial2[i][0] = Move.DeltaToDirSoft(orbiterDeltas[i][0], orbiterDeltas[i][1]);
            orbiterDirectionsRadial2[i][1] = Move.ReverseDir(orbiterDirectionsRadial[i][0]);
        }
    }

    public void LoadPieceTable()
    {
        string[][] rawTable = MainManager.CSVParse(Resources.Load<TextAsset>("Data/PieceData").text);

        pieceTable = new PieceTableEntry[rawTable.Length - 2];

        for (int i = 1; i < rawTable.Length - 1; i++)
        {
            pieceTable[i - 1] = PieceTableEntry.Parse((Piece.PieceType)(i), rawTable[i]);

            //Debug.Log((Piece.PieceType)i);
        }
    }

    public void LoadPieceClassTable()
    {
        string[][] rawTable = MainManager.CSVParse(Resources.Load<TextAsset>("Data/PieceClassInfo").text);

        pieceClassTable = new PieceClassEntry[rawTable.Length - 2];
        for (int i = 1; i < rawTable.Length - 1; i++)
        {
            pieceClassTable[i - 1] = PieceClassEntry.Parse((Piece.PieceClass)(i - 1), rawTable[i]);
        }
    }

    public static PieceClassEntry GetPieceClassEntry(Piece.PieceClass pc)
    {
        return Instance.pieceClassTable[(int)pc];
    }

    public static PieceTableEntry GetPieceTableEntry(uint piece)
    {
        /*
        if (pieceTable == null || pieceTable.Length < 2)
        {
            LoadPieceTable();
        }
        */

        Piece.PieceType pt = Piece.GetPieceType(piece);

        if (pt == Piece.PieceType.Null)
        {
            return null;
        }

        return pieceTable[((int)pt - 1)];
        //return Instance.GetPieceTableEntry(Piece.GetPieceType(piece));
    }
    /*
    public static PieceTableEntry GetPieceTableEntry(Piece.PieceType pieceType)
    {
        return Instance.GetPieceTableEntry(pieceType);
    }
    */
    /*
    public static PieceTableEntry GetPieceTableEntry(Piece.PieceType pieceType)
    {
        return Instance.GetPieceTableEntry(pieceType);
    }
    */
    public static PieceTableEntry GetPieceTableEntry(Piece.PieceType pieceType)
    {
        /*
        if (pieceTable == null || pieceTable.Length < 2)
        {
            LoadPieceTable();
        }
        */

        if (pieceType == Piece.PieceType.Null)
        {
            return null;
        }

        return pieceTable[((int)pieceType - 1)];
    }

    public void LoadPieceSquareTables()
    {
        pieceSquareTableCenter = new float[64];
        pieceSquareTableCorner = new float[64];
        pieceSquareTableTopCenter = new float[64];
        pieceSquareTableTopCenterKing = new float[64];

        int tempX = 0;
        int tempY = 0;
        int symX = 0;
        int symY = 0;
        int symC = 0;
        for (int i = 0; i < 64; i++)
        {
            tempX = i & 7;
            tempY = i >> 3;

            symX = tempX;
            if (tempX > 3)
            {
                symX = 7 - symX;
            }
            symY = tempY;
            if (tempY > 3)
            {
                symY = 7 - symY;
            }

            symC = symX;
            if (symC > symY)
            {
                symC = symY;
            }

            //bias towards x center?
            pieceSquareTableCenter[i] = symX * symX * 0.005f + symY * symY * 0.005f + symC * symC * 0.02f;
            pieceSquareTableCorner[i] = (3 - symX) * (3 - symX) * (3 - symX) * 0.02f + (3 - symY) * (3 - symY) * (3 - symY) * 0.035f;

            pieceSquareTableTopCenter[i] = pieceSquareTableCenter[i];
            pieceSquareTableTopCenterKing[i] = pieceSquareTableCenter[i];
            if (tempY > 4)
            {
                pieceSquareTableTopCenter[i] = pieceSquareTableCenter[i] + (tempY - 3) * 0.2f;
                pieceSquareTableTopCenterKing[i] = pieceSquareTableCenter[i] + (tempY - 3) * 0.3f;
            }
            //Get a fat bonus
            if (tempY == 7 || tempY == 6)
            {
                pieceSquareTableTopCenter[i] = pieceSquareTableCenter[i] + 0.6f;
                pieceSquareTableTopCenterKing[i] = pieceSquareTableCenter[i] + 1f;
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadPSTCenter(Piece.PieceAlignment pa, int index)
    {
        if (pa == Piece.PieceAlignment.Black)
        {
            return pieceSquareTableCenter[63 - index];
        } else
        {
            return pieceSquareTableCenter[index];
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadPSTEdge(Piece.PieceAlignment pa, int index)
    {
        if (pa == Piece.PieceAlignment.Black)
        {
            return pieceSquareTableCorner[63 - index];
        }
        else
        {
            return pieceSquareTableCorner[index];
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadPSTTopCenter(Piece.PieceAlignment pa, int index)
    {
        if (pa == Piece.PieceAlignment.Black)
        {
            return pieceSquareTableTopCenter[63 - index];
        }
        else
        {
            return pieceSquareTableTopCenter[index];
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadPSTTopCenterKing(Piece.PieceAlignment pa, int index)
    {
        if (pa == Piece.PieceAlignment.Black)
        {
            return pieceSquareTableTopCenterKing[63 - index];
        }
        else
        {
            return pieceSquareTableTopCenterKing[index];
        }
    }
}

[System.Serializable]
public sealed class PieceTableEntry
{
    //Redundant but potentially useful to save a few Piece.GetPieceType calls?
    public Piece.PieceType type;

    public short pieceValueX2;

    //Some move generator data
    public MoveGeneratorInfoEntry[] moveInfo;
    public MoveGeneratorInfoEntry[] enhancedMoveInfo;

    public Piece.EnhancedMoveType enhancedMoveType;
    public Piece.BonusMoveType bonusMoveType;
    public Piece.ReplacerMoveType replacerMoveType;

    [HideInInspector]
    public Piece.PieceProperty pieceProperty;
    [HideInInspector]
    public Piece.PiecePropertyB piecePropertyB;
    public Piece.PieceType promotionType;   //Null or Rock = no promotion

    public bool hasAdvancedMagic;

    public bool immobile;           //Naturally immobile: avoid making the army have too many of these (quota = 1/3 piece population?)
    public bool nonattacking;           //likewise
    public bool wingedCompatible;
    public Piece.PieceClass pieceClass;

    public float complexityLevel;
    public bool canIndirectCapture; //Pieces with this marked as false can speed up Q search a lot

    public bool neutralIncompatible;

    public static PieceTableEntry Parse(Piece.PieceType target, string[] tableRow)
    {
        //Debug.Log(target);
        PieceTableEntry output = new PieceTableEntry();

        Piece.PieceType pt;
        if (tableRow.Length > 0)
        {
            Enum.TryParse(tableRow[0], out pt);
            if (pt != target)
            {
                Debug.LogWarning("Mismatched piece table entry got " + tableRow[0] + " parsed as " + pt + " when should be " + target);
            }

            output.type = pt;
        }

        if (tableRow.Length > 1)
        {
            float.TryParse(tableRow[1], out float floatValue);

            output.pieceValueX2 = (byte)(floatValue * 2);

            if (output.type == Piece.PieceType.King)
            {
                output.pieceValueX2 += GlobalPieceManager.KING_VALUE_BONUS;
            }
        }

        if (tableRow.Length > 2)
        {
            //Parse piece movement
            output.moveInfo = MoveGeneratorInfoEntry.ParseMovement(tableRow[2]);
        }

        if (tableRow.Length > 3)
        {
            //Parse piece movement
            output.enhancedMoveInfo = MoveGeneratorInfoEntry.ParseMovement(tableRow[3]);
        }

        if (tableRow.Length > 4)
        {
            //Parse piece flags
            string[] pieceFlags = tableRow[4].Split("|");

            for (int i = 0; i < pieceFlags.Length; i++)
            {
                if (pieceFlags[i].Length < 1)
                {
                    continue;
                }

                Piece.BonusMoveType bmt = BonusMoveType.None;
                Enum.TryParse(pieceFlags[i], out bmt);
                //Piece.EnhancedMoveType emt = EnhancedMoveType.None;
                //Enum.TryParse(pieceFlags[i], out emt);
                Piece.ReplacerMoveType rmt = ReplacerMoveType.None;
                Enum.TryParse(pieceFlags[i], out rmt);

                if (bmt != BonusMoveType.None)
                {
                    output.bonusMoveType = bmt;
                }
                //output.enhancedMoveType = emt;
                if (rmt != ReplacerMoveType.None)
                {
                    output.replacerMoveType = rmt;
                }

                Piece.PieceProperty pp = Piece.PieceProperty.None;
                Enum.TryParse(pieceFlags[i], out pp);

                if (pp == Piece.PieceProperty.None)
                {
                    Piece.PiecePropertyB ppb = Piece.PiecePropertyB.None;
                    Enum.TryParse(pieceFlags[i], out ppb);
                    if (ppb == Piece.PiecePropertyB.None)
                    {
                        MoveGeneratorAtom mga;
                        if (pieceFlags[i].Length > 1)
                        {
                            Enum.TryParse(pieceFlags[i], out mga);

                            if (mga == MoveGeneratorAtom.Null)
                            {
                                Piece.EnhancedMoveType emt;
                                Enum.TryParse(pieceFlags[i], out emt);
                                if (emt == Piece.EnhancedMoveType.None && bmt == BonusMoveType.None && rmt == ReplacerMoveType.None)
                                {
                                    Debug.LogError("Error parsing property " + pieceFlags[i]);
                                } else
                                {
                                    output.enhancedMoveType = emt;
                                }
                                //Debug.LogError("Error parsing property " + pieceFlags[i]);
                            } else
                            {
                                if (SpecialMoveShouldBeFirst(mga))
                                {
                                    //output.moveInfo.Insert(0, new MoveGeneratorInfoEntry(mga));
                                    //Remake the array I guess :P
                                    MoveGeneratorInfoEntry[] newArray = new MoveGeneratorInfoEntry[output.moveInfo.Length + 1];
                                    for (int na = 0; na < output.moveInfo.Length; na++)
                                    {
                                        newArray[na + 1] = output.moveInfo[na];
                                    }
                                    newArray[0] = new MoveGeneratorInfoEntry(mga);
                                    output.moveInfo = newArray;
                                }
                                else
                                {
                                    //output.moveInfo.Add(new MoveGeneratorInfoEntry(mga));
                                    //Remake the array I guess :P
                                    MoveGeneratorInfoEntry[] newArray = new MoveGeneratorInfoEntry[output.moveInfo.Length + 1];
                                    for (int na = 0; na < output.moveInfo.Length; na++)
                                    {
                                        newArray[na] = output.moveInfo[na];
                                    }
                                    newArray[newArray.Length - 1] = new MoveGeneratorInfoEntry(mga);
                                    output.moveInfo = newArray;
                                }
                            }
                        }
                    }
                    else
                    {
                        output.piecePropertyB |= (ppb);
                    }
                } else
                {
                    output.pieceProperty |= (pp);
                }

                /*
                if (MoveGeneratorInfoEntry.PropertyHasSpecialMove(pp))
                {
                    if (MoveGeneratorInfoEntry.PropertySpecialMoveShouldBeFirst(pp))
                    {
                        output.moveInfo.Insert(0, MoveGeneratorInfoEntry.PropertySpecialMove(pp));
                    }
                    else
                    {
                        output.moveInfo.Add(MoveGeneratorInfoEntry.PropertySpecialMove(pp));
                    }
                }
                */
            }
        }

        bool allowWinged = false;
        bool forbidWinged = false;
        switch (output.type)
        {
            case Piece.PieceType.LocustQueen:
                forbidWinged = true;
                break;
        }
        for (int i = 0; i < output.moveInfo.Length; i++)
        {
            if ((output.moveInfo[i].range > 1 || output.moveInfo[i].range == 0) && (output.moveInfo[i].modifier & MoveGeneratorPreModifier.i) == 0 && (output.moveInfo[i].atom < MoveGeneratorAtom.SpecialMoveDivider))
            {
                allowWinged = true;
            }

            switch (output.moveInfo[i].atom)
            {
                case MoveGeneratorAtom.C:
                case MoveGeneratorAtom.Z:
                    forbidWinged = true;
                    break;
            }
        }
        for (int i = 0; i < output.enhancedMoveInfo.Length; i++)
        {
            if ((output.enhancedMoveInfo[i].range > 1 || output.enhancedMoveInfo[i].range == 0) && (output.enhancedMoveInfo[i].modifier & MoveGeneratorPreModifier.i) == 0 && (output.enhancedMoveInfo[i].atom < MoveGeneratorAtom.SpecialMoveDivider))
            {
                allowWinged = true;
            }

            switch (output.enhancedMoveInfo[i].atom)
            {
                case MoveGeneratorAtom.C:
                case MoveGeneratorAtom.Z:
                    forbidWinged = true;
                    break;
            }
        }
        if ((output.piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
        {
            forbidWinged = true;
        }

        if (allowWinged && !forbidWinged)
        {
            output.wingedCompatible = true;
        }

        bool immobile = true;
        for (int i = 0; i < output.moveInfo.Length; i++)
        {
            if ((output.moveInfo[i].modifier & (MoveGeneratorPreModifier.c | MoveGeneratorPreModifier.a | MoveGeneratorPreModifier.n | MoveGeneratorPreModifier.e)) != 0)
            {
                continue;
            }

            //probably legal move
            immobile = false;
            break;
        }
        for (int i = 0; i < output.enhancedMoveInfo.Length; i++)
        {
            if ((output.enhancedMoveInfo[i].modifier & (MoveGeneratorPreModifier.m | MoveGeneratorPreModifier.a | MoveGeneratorPreModifier.n | MoveGeneratorPreModifier.e)) != 0)
            {
                continue;
            }

            //probably legal move
            immobile = false;
            break;
        }
        bool nonattacking = true;
        for (int i = 0; i < output.moveInfo.Length; i++)
        {
            if ((output.moveInfo[i].modifier & (MoveGeneratorPreModifier.m | MoveGeneratorPreModifier.a | MoveGeneratorPreModifier.n)) != 0)
            {
                continue;
            }

            //probably legal move
            nonattacking = false;
            break;
        }
        for (int i = 0; i < output.enhancedMoveInfo.Length; i++)
        {
            if ((output.enhancedMoveInfo[i].modifier & (MoveGeneratorPreModifier.m | MoveGeneratorPreModifier.a | MoveGeneratorPreModifier.n)) != 0)
            {
                continue;
            }

            //probably legal move
            nonattacking = false;
            break;
        }
        switch (output.type)
        {
            case PieceType.Phantom:
                nonattacking = true;
                break;
        }
        output.immobile = immobile;
        output.nonattacking = nonattacking;

        //Has advanced magic
        //Stuff I didn't make properties for
        //(i.e. special moves or area of effect stuff)
        if (tableRow.Length > 5 && tableRow[5].Length > 1)
        {
            bool.TryParse(tableRow[5], out output.hasAdvancedMagic);
        }

        if (tableRow.Length > 6 && tableRow[6].Length > 1)
        {
            Enum.TryParse(tableRow[6], out Piece.PieceType proType);

            if (proType == Piece.PieceType.Null && tableRow[6].Length > 1)
            {
                Debug.LogWarning("Failed to parse promotion type " + tableRow[6] + " parsed as " + proType);
            }

            output.promotionType = proType;
        }

        if (tableRow.Length > 7 && tableRow[7].Length > 1)
        {
            Enum.TryParse(tableRow[7], out Piece.PieceClass pclass);

            if (pclass == Piece.PieceClass.None && tableRow[7].Length > 1 && !tableRow[7].Equals("None"))
            {
                Debug.LogWarning("Failed to parse piece class " + tableRow[7] + " parsed as " + pclass);
            }

            output.pieceClass = pclass;
        }

        if (tableRow.Length > 8 && tableRow[8].Length > 1)
        {
            float.TryParse(tableRow[8], out float complexity);

            output.complexityLevel = complexity;
        }

        switch (output.type)
        {
            case PieceType.FlankingBishop:
            case PieceType.FlankingKnight:
            case PieceType.FlankingPawn:
            case PieceType.Duelist:
            case PieceType.Fencer:
            case PieceType.PythonQueen:
            case PieceType.Pincer:               
            case PieceType.Shocker:
            case PieceType.Bolter:
            case PieceType.LightningElemental:
                output.canIndirectCapture = true;
                break;
        }

        //blanket ban?
        output.neutralIncompatible |= output.hasAdvancedMagic;

        if ((output.pieceProperty & PieceProperty.ConvertCapture) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.pieceProperty & PieceProperty.Relay) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.pieceProperty & PieceProperty.WeakConvertCapture) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.piecePropertyB & PiecePropertyB.AnyMomentum) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.piecePropertyB & PiecePropertyB.Amoeba) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.piecePropertyB & PiecePropertyB.PieceCarry) != 0)
        {
            output.neutralIncompatible = true;
        }
        if ((output.piecePropertyB & PiecePropertyB.TrueShiftImmune) != 0)
        {
            output.neutralIncompatible = true;
        }
        switch (output.type)
        {
            case PieceType.RoyalDouble:
            case PieceType.RoyalCastle:
            case PieceType.RoyalGuard:
            case PieceType.RoyalMaid:
            case PieceType.RoyalRecruit:
            case PieceType.Pride:
            case PieceType.Patience:
            case PieceType.ArcanaJustice:
            case PieceType.ArcanaLovers:
            case PieceType.ArcanaTower:
            case PieceType.ArcanaStar:
            case PieceType.ArcanaJudgement:
            case PieceType.SummerPawn:
            case PieceType.SummerQueen:
            case PieceType.SummerRook:
            case PieceType.WinterBishop:
            case PieceType.WinterPawn:
            case PieceType.WinterQueen:
            case PieceType.SpringKnight:
            case PieceType.SpringPawn:
            case PieceType.FallPawn:
            case PieceType.FallKnight:
            case PieceType.DayBishop:
            case PieceType.DayPawn:
            case PieceType.DayQueen:
            case PieceType.NightKnight:
            case PieceType.NightPawn:
            case PieceType.NightQueen:
                output.neutralIncompatible = true;
                break;
        }
        //whitelist a few things anyways
        switch (output.type)
        {
            case PieceType.Mercenary:
            case PieceType.EdgeRook:
            case PieceType.CenterQueen:
            case PieceType.CornerlessBishop:
                output.neutralIncompatible = false;
                break;
        }

        return output;
    }
}

[System.Serializable]
public sealed class MoveGeneratorInfoEntry
{
    public enum MoveGeneratorAtom
    {
        Null,   //something to placeholder for 0 I guess

        W,  //wazir
        R,  //rook
        F,  //ferz
        B,  //bishop

        K,  //King shorthand (= WF)
        Q,  //Queen shorthand (= RB)

        Z,  //crooked bishop (Not using the "Z" modifier because it is hard to do)
        C,  //crooked rook
        I,  //rhino

        G, //gryphon
        M, //manticore

        H, //Wheel movement

        O, //Orbiter
        P,  //Range 2 orbiter

        A,  //skip bishop (A -> B)
        D,  //skip rook (D -> R)

        J,  //skip bishop alternate (A leap then can turn 90 degrees)
        E,  //skip rook alternate (D leap but then can turn 90 degrees)
        S,  //rose leaper

        Leaper,  //(leaper / rider placeholder)

        SpecialMoveDivider,     //Used to check for special moves

        //Special kinds of movement
        Castling,
        AllyKingTeleport,
        EnemyKingTeleport,
        PawnSwapTeleport,
        AllySwapTeleport,
        AllyBehindTeleport,
        EnemyBehindTeleport,
        AnywhereTeleport,
        AnywhereAdjacentTeleport,
        AnywhereNonAdjacentTeleport,
        AnywhereSameColorTeleport,
        AnywhereOppositeColorTeleport,
        KingSwapTeleport,
        HomeRangeTeleport,
        MirrorTeleport,
        MirrorTeleportSwap,
        VerticalMirrorTeleport,
        ForestTeleport,         //4+ neighbors
        BlossomTeleport,        //3+ neighbors
        DiplomatTeleport,
        EchoTeleport,
        CoastTeleport,
        AimMover,

        LensRook,
        Recall,
    }

    //Not the range modifiers
    [Flags]
    public enum MoveGeneratorPreModifier 
    {
        None = 0,
        m = 1<<0,  //Move only
        c = 1<<1,  //Capture only

        f = 1<<2,  //Forward
        b = 1<<3,  //Back
        v = 1<<4,  //Vertical
        h = 1<<5,  //Horizontal

        fv = f | v,
        fh = f | h,
        bv = b | v,
        bh = b | h,
        vh = v | h,

        DirectionModifiers = f | b | v | h,     //All directions bitmask
        fb = f | b,

        i = 1<<6,  //Initial (first 2 rows only)
        r = 1<<7,  //rifle capture
        l = 1<<8,  //leaper capture
        j = 1<<9,   //forced leaper
        y = 1<<10,   //flying move only (unused)

        a = 1<<11,  //Ally ability
        n = 1<<12,  //Special ability that targets empty squares (Similar effect to "m")
        e = 1<<13,  //Special ability that targets enemy squares (Similar effect to "c")
        o = 1<<14,  //"bonus" movement flag (unit specific stuff to distinguish slip moves from regular moves?)
        p = 1<<15,  //passive ability (does not generate moves but does flag attack/defense so I can make range bitboards quickly)

        k = 1<<16,  //king attack

        ljanepo = l | j | a | n | e | o | p,    //to reduce the amount of conditions

        MoveLike = m | a,
        CaptureLike = c | e,
        TypeRestrictors = MoveLike | CaptureLike,
    }

    public enum RangeType
    {
        Normal, //0 = infinite,
        AntiRange,  //Move maximum distance (or maximum up to X range)  (to make this less annoying to code I only let you move max distance?)
        Minimum,    //Move minimum X units
        Exact,      //Exactly X units
    }

    public MoveGeneratorAtom atom;
    public MoveGeneratorPreModifier modifier;

    public int x, y;    //For leapers (but sliders also use it)
    public int range;
    public RangeType rangeType;

    public MoveGeneratorInfoEntry()
    {

    }

    public MoveGeneratorInfoEntry(MoveGeneratorAtom atom)
    {
        this.atom = atom;
    }

    public MoveGeneratorInfoEntry(MoveGeneratorInfoEntry mgie)
    {
        atom = mgie.atom;
        modifier = mgie.modifier;
        x = mgie.x;
        y = mgie.y;
        range = mgie.range;
        rangeType = mgie.rangeType;
    }

    public static MoveGeneratorInfoEntry[] ParseMovement(string movement)
    {
        int parseIndex = 0;
        List<MoveGeneratorInfoEntry> output = new List<MoveGeneratorInfoEntry>();

        //Parse loop
        while (parseIndex < movement.Length)
        {
            //Scan index ahead until the current atom ends
            MoveGeneratorInfoEntry newEntry;
            (newEntry, parseIndex) = ScanOneAtom(movement, parseIndex);
            output.Add(newEntry);
        }

        //K = WF
        //Q = RB
        for (int i = 0; i < output.Count; i++)
        {
            if (output[i].atom == MoveGeneratorAtom.K)
            {
                MoveGeneratorInfoEntry newEntry = new MoveGeneratorInfoEntry(output[i]);
                output[i].atom = MoveGeneratorAtom.W;
                newEntry.atom = MoveGeneratorAtom.F;
                output.Insert(i, newEntry);
                continue;
            }
            if (output[i].atom == MoveGeneratorAtom.Q)
            {
                MoveGeneratorInfoEntry newEntry = new MoveGeneratorInfoEntry(output[i]);
                output[i].atom = MoveGeneratorAtom.R;
                newEntry.atom = MoveGeneratorAtom.B;
                output.Insert(i, newEntry);
                continue;
            }
        }

        return output.ToArray();
    }

    public static (MoveGeneratorInfoEntry, int) ScanOneAtom(string movement, int startIndex)
    {
        //Index returned is the start index of the next atom
        int index = startIndex;
        MoveGeneratorInfoEntry nextEntry = new MoveGeneratorInfoEntry();

        //State 0 = parsing modifiers
        //State 1 = parsing atom (Mandatory)
        //State 2 = parsing range number
        //State 3 = parsing range symbol (+ or -)
        int state = 0;

        //int iteration = 0;

        while (index < movement.Length)
        {
            /*
            iteration++;
            if (iteration > 10000)  //No real movement thing should reach this
            {
                Debug.LogError("Parse infinite loop at index " + index + " state " + state + " parsing " + movement);
                break;
            }
            */
            switch (state)
            {
                case 0:
                    //If a state 1 thing is seen: skip to state 1
                    if (movement[index] == '(')
                    {
                        state = 1;
                        continue;
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorAtom _))
                    {
                        state = 1;
                        continue;
                    }

                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorPreModifier m))
                    {
                        nextEntry.modifier |= m;
                        index++;
                        continue;
                    }

                    //Fallthrough: error
                    throw new ArgumentException("Failed to parse movement (modifier parsing failed, last character seen = " + movement[index] + ") string = " + movement);
                case 1:
                    //Try to parse a state 1 thing
                    if (movement[index] == '(')
                    {
                        int closeIndex = index;
                        while (closeIndex < movement.Length)
                        {
                            if (movement[closeIndex] == ')')
                            {
                                break;
                            }
                            closeIndex++;
                        }
                        if (closeIndex > movement.Length)
                        {
                            throw new ArgumentException("Failed to parse movement (leaper atom parsing failed, no matching ')' found)");
                        }

                        //Parse stuff between
                        string insideParenthesis = movement.Substring(index + 1, closeIndex - index - 1);

                        string[] parenthesisSplit = insideParenthesis.Split(",");

                        if (parenthesisSplit.Length != 2)
                        {
                            throw new ArgumentException("Failed to parse movement (leaper atom parsing failed, found + " + parenthesisSplit.Length + " elements parenthesized, expected 2)");
                        }

                        int x;
                        int y;
                        if (!int.TryParse(parenthesisSplit[0], out x))
                        {
                            throw new ArgumentException("Failed to parse movement (leaper atom parsing failed, found + " + parenthesisSplit[0] + " for element 1 when a number was expected)");
                        }
                        if (!int.TryParse(parenthesisSplit[1], out y))
                        {
                            throw new ArgumentException("Failed to parse movement (leaper atom parsing failed, found + " + parenthesisSplit[1] + " for element 1 when a number was expected)");
                        }

                        nextEntry.x = x;
                        nextEntry.y = y;
                        nextEntry.range = 1;
                        nextEntry.atom = MoveGeneratorAtom.Leaper;

                        index = closeIndex + 1;
                        state = 2;
                        continue;
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorAtom a))
                    {
                        nextEntry.atom = a;

                        switch (a)
                        {
                            case MoveGeneratorAtom.W:
                            case MoveGeneratorAtom.F:
                            case MoveGeneratorAtom.K:
                                nextEntry.range = 1;
                                break;
                        }

                        index++;
                        state = 2;
                        //Debug.Log("Move to phase 2 with atom = " + movement[index - 1]);
                        continue;
                    }

                    //Fallthrough: error
                    throw new ArgumentException("Failed to parse movement (atom parsing failed, last character seen = " + movement[index] + ")");
                case 2:
                    //If state 0 or state 1 thing is seen: you've gone past this current atom
                    if (movement[index] == '(')
                    {
                        state = 1;
                        return (nextEntry, index);
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorAtom _))
                    {
                        state = 1;
                        return (nextEntry, index);
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorPreModifier _))
                    {
                        return (nextEntry, index);
                    }

                    //state 3 -
                    if (movement[index] == '-')
                    {
                        state = 3;
                        continue;
                    }

                    //Parse a number
                    int numberEndIndex = index;
                    while (numberEndIndex < movement.Length)
                    {
                        numberEndIndex++;

                        if (numberEndIndex >= movement.Length)
                        {
                            break;
                        }
                        if (movement[numberEndIndex] < '0' || movement[numberEndIndex] > '9')
                        {
                            break;
                        }
                    }
                    string numberToParse = movement.Substring(index, numberEndIndex - index);
                    //Debug.Log("Parse number " + numberToParse);

                    int range;
                    if (!int.TryParse(numberToParse, out range))
                    {
                        throw new ArgumentException("Failed to parse movement (range parsing failed, number to parse = " + numberToParse + ")");
                    } else
                    {
                        nextEntry.range = range;
                        index = numberEndIndex;
                        state = 3;
                        continue;
                    }

                    //Fallthrough: error
                    throw new ArgumentException("Failed to parse movement (range parsing failed, last character seen = " + movement[index] + ")");
                case 3:
                    //If state 0 or state 1 thing is seen: you've gone past this current atom
                    if (movement[index] == '(')
                    {
                        state = 1;
                        return (nextEntry, index);
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorAtom _))
                    {
                        state = 1;
                        return (nextEntry, index);
                    }
                    if (!int.TryParse(movement[index].ToString(), out _) && Enum.TryParse(movement[index].ToString(), out MoveGeneratorPreModifier _))
                    {
                        return (nextEntry, index);
                    }

                    //Parse a symbol
                    if (movement[index] == '+')
                    {
                        nextEntry.rangeType = RangeType.Minimum;
                        index++;
                        continue;
                    }
                    if (movement[index] == '-')
                    {
                        nextEntry.rangeType = RangeType.AntiRange;
                        index++;
                        continue;
                    }
                    if (movement[index] == '=')
                    {
                        nextEntry.rangeType = RangeType.Exact;
                        index++;
                        continue;
                    }

                    //Fallthrough: error
                    throw new ArgumentException("Failed to parse movement (range symbol parsing failed, last character seen = " + movement[index] + ") movement = " + movement);
            }
        }


        return (nextEntry, index);
    }

    public bool IsSpecialMove()
    {
        return atom >= MoveGeneratorAtom.SpecialMoveDivider;
    }

    public static bool SpecialMoveShouldBeFirst(MoveGeneratorAtom m)
    {
        switch (m) {
            case MoveGeneratorAtom.PawnSwapTeleport:
            case MoveGeneratorAtom.AllySwapTeleport:
            case MoveGeneratorAtom.KingSwapTeleport:
            case MoveGeneratorAtom.AllyBehindTeleport:
            case MoveGeneratorAtom.EnemyBehindTeleport:
            case MoveGeneratorAtom.AnywhereTeleport:
            case MoveGeneratorAtom.AnywhereAdjacentTeleport:
            case MoveGeneratorAtom.AnywhereNonAdjacentTeleport:
            case MoveGeneratorAtom.AnywhereSameColorTeleport:
            case MoveGeneratorAtom.AnywhereOppositeColorTeleport:
            //case Piece.PieceProperty.AnywhereTeleport:  //let you teleport anywhere instead of blocking out the capture only range?
            case MoveGeneratorAtom.AimMover:
                return true;
        }

        return false;
    }

    public static string GetSpecialMoveText(MoveGeneratorAtom m)
    {
        switch (m)
        {
            case MoveGeneratorAtom.Null:
            case MoveGeneratorAtom.W:
            case MoveGeneratorAtom.R:
            case MoveGeneratorAtom.F:
            case MoveGeneratorAtom.B:
            case MoveGeneratorAtom.K:
            case MoveGeneratorAtom.Q:
            case MoveGeneratorAtom.Z:
            case MoveGeneratorAtom.C:
            case MoveGeneratorAtom.I:
            case MoveGeneratorAtom.G:
            case MoveGeneratorAtom.M:
            case MoveGeneratorAtom.A:
            case MoveGeneratorAtom.D:
            case MoveGeneratorAtom.J:
            case MoveGeneratorAtom.E:
            case MoveGeneratorAtom.S:
            case MoveGeneratorAtom.Leaper:
            case MoveGeneratorAtom.Castling:
                return null;
            case MoveGeneratorAtom.H:
                return "[Special] Move around the rings of the board.";
            case MoveGeneratorAtom.O:
                return "[Special] Orbit around any adjacent piece (moving in a circle around it).";
            case MoveGeneratorAtom.P:
                return "[Special] Orbit around a piece in a radius 2 circle (The radius 2 circle contains all distance 2 squares except the 4 corners).";
            case MoveGeneratorAtom.AllyKingTeleport:
                return "[Special] Teleport Move to a square adjacent to an ally King.";
            case MoveGeneratorAtom.EnemyKingTeleport:
                return "[Special] Teleport Move to a square adjacent to an enemy King.";
            case MoveGeneratorAtom.PawnSwapTeleport:
                return "[Special] Teleport Swap with an ally pawn.";
            case MoveGeneratorAtom.AllySwapTeleport:
                return "[Special] Teleport Swap with an ally piece.";
            case MoveGeneratorAtom.AllyBehindTeleport:
                return "[Special] Teleport Move to a square behind an ally piece.";
            case MoveGeneratorAtom.EnemyBehindTeleport:
                return "[Special] Teleport Move to a square in front of an enemy piece.";
            case MoveGeneratorAtom.AnywhereTeleport:
                break;
            case MoveGeneratorAtom.AnywhereAdjacentTeleport:
                return "[Special] Teleport Move to a square adjacent to any piece.";
            case MoveGeneratorAtom.AnywhereNonAdjacentTeleport:
                return "[Special] Teleport Move to a square not adjacent to any piece.";
            case MoveGeneratorAtom.AnywhereSameColorTeleport:
                break;
            case MoveGeneratorAtom.AnywhereOppositeColorTeleport:
                break;
            case MoveGeneratorAtom.KingSwapTeleport:
                return "[Special] Teleport Swap with an ally King.";
            case MoveGeneratorAtom.HomeRangeTeleport:
                return "[Special] Teleport Move to a square in the first two rows.";
            case MoveGeneratorAtom.MirrorTeleport:
                return "[Special] Teleport Move or Capture to the mirrored square.";
            case MoveGeneratorAtom.MirrorTeleportSwap:
                return "[Special] Teleport Move, Capture or Swap with an ally on the mirrored square.";
            case MoveGeneratorAtom.VerticalMirrorTeleport:
                return "[Special] Teleport Move, Capture or Swap with an ally on the vertical mirrored square.";
            case MoveGeneratorAtom.ForestTeleport:
                return "[Special] Teleport Move to a square with at least 4 ally pieces adjacent to it.";
            case MoveGeneratorAtom.BlossomTeleport:
                return "[Special] Teleport Move or Capture to a square with at least 3 ally pieces adjacent to it.";
            case MoveGeneratorAtom.DiplomatTeleport:
                return "[Special] Teleport Move to a square adjacent to an ally and an enemy.";
            case MoveGeneratorAtom.EchoTeleport:
                return "[Special] Teleport Move to the square that the enemy last moved from.";
            case MoveGeneratorAtom.CoastTeleport:
                return "[Special] Teleport Move to anywhere on the 4 edges of the board.";
            case MoveGeneratorAtom.AimMover:
                return "[Special] Move or Capture to the aimed at square.";
            case MoveGeneratorAtom.LensRook:
                return "[Special] Can teleport to the 180 degree rotated square or move like a Rook from that square.";
            case MoveGeneratorAtom.Recall:
                return "[Special] Teleport backwards as far as possible to an empty square or swap with allies.";
        }
        return null;
    }
}

[System.Serializable]
public sealed class PieceClassEntry
{
    public int index;
    public string name; //to do later: move this to a text only file
    public string description; //to do later: move this to a text only file

    public Color squareColorLight;
    public Color squareColorDark;
    public Color backgroundColorLight;
    public Color backgroundColorDark;

    public Piece.PieceType[] normalPieces;
    public Piece.PieceType[] extraPieces;

    public Square.SquareType[] terrainTypes;
    public int rockCount;
    public int neutralCount;
    public int crystalCount;

    public enum Climate
    {
        Temperate,
        Forest,
        Swamp,
        Islands,
        Savanna,
        RockyHot,
        Desert,
        RockyCold,
        Icy,
    }

    public Climate climate;
    public int tier;

    public Piece.PieceClass[] nearbyRealms;

    internal static PieceClassEntry Parse(PieceClass i, string[] tableRow)
    {
        PieceClassEntry output = new PieceClassEntry();

        if (tableRow.Length > 1)
        {
            Piece.PieceClass pc;
            Enum.TryParse(tableRow[1], out pc);
            if (pc != i)
            {
                Debug.LogWarning("Piece Class parse fail: see " + pc + " aka " + tableRow[1] + " when it should be " + i);
            }
        }

        if (tableRow.Length > 2)
        {
            int tempIndex;
            int.TryParse(tableRow[2], out tempIndex);
            output.index = tempIndex;
        }

        if (tableRow.Length > 3)
        {
            output.name = tableRow[3];
        }

        if (tableRow.Length > 4)
        {
            output.description = tableRow[4];
        }

        if (tableRow.Length > 5)
        {
            Climate c;
            Enum.TryParse(tableRow[5], out c);
            output.climate = c;
        }

        //Debug.Log(tableRow[5] + " " + tableRow[6] + " " + tableRow[7] + " " + tableRow[8]);
        if (tableRow.Length > 6)
        {
            Color c;
            c = MainManager.ParseColor(tableRow[6]).GetValueOrDefault();
            c.a = 1;
            output.squareColorLight = c;
        }

        if (tableRow.Length > 7)
        {
            Color c;
            c = MainManager.ParseColor(tableRow[7]).GetValueOrDefault();
            c.a = 1;
            output.squareColorDark = c;
        }

        if (tableRow.Length > 8)
        {
            Color c;
            c = MainManager.ParseColor(tableRow[8]).GetValueOrDefault();
            c.a = 1;
            output.backgroundColorLight = c;
        }

        if (tableRow.Length > 9)
        {
            Color c;
            c = MainManager.ParseColor(tableRow[9]).GetValueOrDefault();
            c.a = 1;
            output.backgroundColorDark = c;
        }

        if (tableRow.Length > 10)
        {
            int t;
            int.TryParse(tableRow[10], out t);
            output.tier = t;
        }

        if (tableRow.Length > 11)
        {
            string[] nearbyClasses = tableRow[11].Split("|");

            List<Piece.PieceClass> nearbyClassesList = new List<PieceClass>();
            for (int j = 0; j < nearbyClasses.Length; j++)
            {
                Piece.PieceClass pc = PieceClass.None;
                Enum.TryParse(nearbyClasses[j], out pc);
                if (pc != PieceClass.None)
                {
                    nearbyClassesList.Add(pc);
                } else if (nearbyClasses[j].Length > 0)
                {
                    Debug.LogWarning("Could not parse " + nearbyClasses[j] + " in parsing " + i);
                }
            }
            output.nearbyRealms = nearbyClassesList.ToArray();
        }

        List<Piece.PieceType> normalPieces = new List<PieceType>();
        for (int j = 0; j < GlobalPieceManager.pieceTable.Length; j++)
        {
            if (GlobalPieceManager.pieceTable[j] != null && GlobalPieceManager.pieceTable[j].pieceClass == i)
            {
                //forbidden
                if (GlobalPieceManager.pieceTable[j].type == PieceType.GeminiTwin || GlobalPieceManager.pieceTable[j].type == PieceType.MoonIllusion)
                {
                    continue;
                }
                if (GlobalPieceManager.pieceTable[j].pieceValueX2 == 0)
                {
                    continue;
                }
                normalPieces.Add(GlobalPieceManager.pieceTable[j].type);
            }
        }
        output.normalPieces = normalPieces.ToArray();

        if (tableRow.Length > 12)
        {
            string[] terrains = tableRow[12].Split("|");

            List<Square.SquareType> terrainList = new List<Square.SquareType>();
            for (int j = 0; j < terrains.Length; j++)
            {
                Square.SquareType st = Square.SquareType.Normal;
                Enum.TryParse(terrains[j], out st);
                if (st != Square.SquareType.Normal)
                {
                    terrainList.Add(st);
                }
                else if (terrains[j].Length > 0)
                {
                    if (terrains[j].Equals("Rock"))
                    {
                        output.rockCount++;
                    } else if (terrains[j].Equals("Neutral"))
                    {
                        output.neutralCount++;
                    } else if (terrains[j].Equals("Crystal"))
                    {
                        output.crystalCount++;
                    }
                    else {
                        Debug.LogWarning("Could not parse " + terrains[j] + " in parsing " + i);
                    }
                }
            }
            output.terrainTypes = terrainList.ToArray();
        }

        if (tableRow.Length > 13)
        {
            List<Piece.PieceType> extraPiecesList = new List<PieceType>();
            string[] extraPieces = tableRow[13].Split("|");
            for (int j = 0; j < extraPieces.Length; j++)
            {
                Piece.PieceType pt = PieceType.Null;
                Enum.TryParse(extraPieces[j], out pt);
                if (pt != PieceType.Null)
                {
                    extraPiecesList.Add(pt);
                } else if (extraPieces[j].Length > 0)
                {
                    Debug.LogWarning("Could not parse " + extraPieces[j] + " in parsing " + i);
                }
            }
            output.extraPieces = extraPiecesList.ToArray();
        }

        return output;
    }
}