using System.Collections.Generic;
using UnityEngine;
using static Move;
using static MoveGeneratorInfoEntry;
using static Piece;

internal static class MoveGenerator
{

    public const ulong BITBOARD_PATTERN_AFILE = 0x0101010101010101;
    public const ulong BITBOARD_PATTERN_RANK1 = 0x00000000000000ff;
    public const ulong BITBOARD_PATTERN_RANK18 = 0xff000000000000ff;
    public const ulong BITBOARD_PATTERN_QUEEN3 = 0x00492a1c7f1c2a49;        //shift left 3 down 3
    public const ulong BITBOARD_PATTERN_QUEEN2 = 0x000000150e1f0e15;        //shift left 2 down 2 
    public const ulong BITBOARD_PATTERN_ROOK2 = 0x00000004041f0404;         //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_BISHOP2 = 0x000000110a040a11;       //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_DIAMOND = 0x000000040e1f0e04;       //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_ADJACENT = 0x0000000000070707;      //shift left 1 down 1
    public const ulong BITBOARD_PATTERN_ROOK1 = 0x0000000000020702;      //shift left 1 down 1
    public const ulong BITBOARD_PATTERN_BISHOP1 = 0x0000000000050205;      //shift left 1 down 1
    public const ulong BITBOARD_PATTERN_RANGE2 = 0x0000001f1f1f1f1f;        //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_RANGE2ONLY = 0x0000001f1111111f;        //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_SATURN = 0x0000001500110015;        //shift left 2 down 2
    public const ulong BITBOARD_PATTERN_TAURUS_WHITE = 0x0000000000050500;  //shift left 1
    public const ulong BITBOARD_PATTERN_TAURUS_BLACK = 0x0000000000000505;  //shift left 1 down 2

    public const ulong BITBOARD_PATTERN_DIAGONAL = 0x8040201008040201;
    public const ulong BITBOARD_PATTERN_ANTIDIAGONAL = 0x0102040810204080;

    public const ulong BITBOARD_PATTERN_MIDDLEFILES = 0x3c3c3c3c3c3c3c3c;
    public const ulong BITBOARD_PATTERN_NOCORNERS = 0x183c7effff7e3c18;
    public const ulong BITBOARD_PATTERN_NOCENTER = 0xffffc3c3c3c3ffff;
    public const ulong BITBOARD_PATTERN_HALFBOARD = 0x00000000ffffffff;

    public const ulong BITBOARD_PATTERN_WHITESQUARES = 0x55aa55aa55aa55aa;
    public const ulong BITBOARD_PATTERN_BLACKSQUARES = 0xaa55aa55aa55aa55;
    public const ulong BITBOARD_PATTERN_FULL = 0xffffffffffffffff;

    public const ulong BITBOARD_2SQUARE = 0x0000000000000303;

    public const ulong BITBOARD_PATTERN_ORBIT2 = 0x0000000e1111110e;
    public const ulong BITBOARD_PATTERN_OUTEREDGES = 0xff818181818181ff;
    public const ulong BITBOARD_PATTERN_SIDEEDGES = 0x8181818181818181;

    public static void FixArcanaMoon(Board b)
    {

        b.globalData.bitboard_tarotMoonWhite = 0;
        b.globalData.bitboard_tarotMoonBlack = 0;
        b.globalData.bitboard_tarotMoonIllusionWhite = 0;
        b.globalData.bitboard_tarotMoonIllusionBlack = 0;

        for (int i = 0; i < 64; i++)
        {
            if (b.pieces[i] == 0)
            {
                continue;
            }

            ulong bitIndex = 1uL << i;
            Piece.PieceAlignment pa = Piece.GetPieceAlignment(b.pieces[i]);
            Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);

            switch (pa)
            {
                case PieceAlignment.White:
                    switch (pt)
                    {
                        case PieceType.ArcanaMoon:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            break;
                        case PieceType.MoonIllusion:
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            break;
                    }
                    break;
                case PieceAlignment.Black:
                    switch (pt)
                    {
                        case PieceType.ArcanaMoon:
                            b.globalData.bitboard_tarotMoonBlack |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionBlack |= bitIndex;
                            break;
                        case PieceType.MoonIllusion:
                            b.globalData.bitboard_tarotMoonIllusionBlack |= bitIndex;
                            break;
                    }
                    break;
            }
        }

        b.globalData.arcanaMoonOutdated = false;
    }

    //Most areas of effect are for enemies of (PA)
    //i.e. you are immune to the effects of your own pieces
    //Since this doesn't generate moves there is no move list being touched
    private static void GenerateAreaBitboards(ref Board b)
    {
        GeneratePieceBitboards(b);

        //Virgo gets smeared
        b.globalData.bitboard_virgoAuraWhite = MainManager.SmearBitboard(b.globalData.bitboard_virgoWhite);
        b.globalData.bitboard_virgoAuraBlack = MainManager.SmearBitboard(b.globalData.bitboard_virgoBlack);

        //reset bad bitboards
        b.globalData.bitboard_bansheeWhite = 0;
        b.globalData.bitboard_attractorWhite = 0;
        b.globalData.bitboard_repulserWhite = 0;
        b.globalData.bitboard_immobilizerWhite = 0;
        b.globalData.bitboard_harpyWhite = 0;
        b.globalData.bitboard_hagWhite = 0;
        b.globalData.bitboard_slothWhite = 0;
        b.globalData.bitboard_watchTowerWhite = 0;
        b.globalData.bitboard_fanWhite = 0;
        b.globalData.bitboard_hangedWhite = 0;
        b.globalData.bitboard_roughWhite = 0;
        b.globalData.bitboard_waterWhite = 0;

        b.globalData.bitboard_bansheeBlack = 0;
        b.globalData.bitboard_attractorBlack = 0;
        b.globalData.bitboard_repulserBlack = 0;
        b.globalData.bitboard_immobilizerBlack = 0;
        b.globalData.bitboard_harpyBlack = 0;
        b.globalData.bitboard_hagBlack = 0;
        b.globalData.bitboard_slothBlack = 0;
        b.globalData.bitboard_watchTowerBlack = 0;
        b.globalData.bitboard_fanBlack = 0;
        b.globalData.bitboard_hangedBlack = 0;
        b.globalData.bitboard_roughBlack = 0;
        b.globalData.bitboard_waterBlack = 0;

        //Immune relayer
        b.globalData.bitboard_immuneWhite |= MainManager.SmearBitboard(b.globalData.bitboard_immuneRelayerWhite);
        b.globalData.bitboard_immuneBlack |= MainManager.SmearBitboard(b.globalData.bitboard_immuneRelayerBlack);

        //MainManager.PrintBitboard(b.globalData.bitboard_immuneWhite);

        bool slothful = (b.globalData.enemyModifier & Board.EnemyModifier.Slothful) != 0;

        bool rough = (b.globalData.playerModifier & Board.PlayerModifier.Rough) != 0;

        //Generate the area bitboards for other piece types
        ulong pieceBitboard = (b.globalData.bitboard_piecesWhite | b.globalData.bitboard_piecesBlack) & b.globalData.bitboard_aura;
        while (pieceBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(pieceBitboard, out pieceBitboard);

            Piece.PieceType pt = Piece.GetPieceType(b.pieces[index]);
            Piece.PieceAlignment ipa = Piece.GetPieceAlignment(b.pieces[index]);

            ulong pattern = 0;

            if (slothful && pt == Piece.PieceType.King && ipa == Piece.PieceAlignment.Black)
            {
                b.globalData.bitboard_immobilizerBlack |= BITBOARD_PATTERN_AFILE << (index & 7);
            }

            if (rough && ipa == Piece.PieceAlignment.White)
            {
                pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                if (ipa == Piece.PieceAlignment.White)
                {
                    b.globalData.bitboard_roughWhite |= pattern;
                }
            }

            if (Piece.GetPieceModifier(b.pieces[index]) == Piece.PieceModifier.Immune)
            {
                pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                if (ipa == Piece.PieceAlignment.White)
                {
                    b.globalData.bitboard_waterWhite |= pattern;
                }
                if (ipa == Piece.PieceAlignment.Black)
                {
                    b.globalData.bitboard_waterBlack |= pattern;
                }
            }

            switch (pt)
            {
                case Piece.PieceType.Attractor:
                    //Q3
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_QUEEN3, index, -3, -3);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_attractorWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_attractorBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Repulser:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_QUEEN3, index, -3, -3);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_repulserWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_repulserBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Immobilizer:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_immobilizerWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_immobilizerBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Entrancer:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Charmer:
                    //this one doesn't need any fancy method because it's just an up shift
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_immobilizerWhite |= (1uL << (index + 8));
                    }
                    if (ipa == Piece.PieceAlignment.Black && index >= 8)
                    {
                        b.globalData.bitboard_immobilizerBlack |= (1uL << (index - 8));
                    }
                    break;
                case Piece.PieceType.Sloth:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_RANGE2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_slothWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_slothBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.ArcanaHierophant:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.ArcanaHanged:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_DIAMOND, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_hangedWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_hangedBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.AceOfPentacles:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.AceOfCups:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.PageOfPentacles:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.PageOfCups:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.QueenOfPentacles:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.QueenOfCups:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Earth:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Saturn:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_SATURN, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Ganymede:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Taurus:
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_TAURUS_WHITE, index, -1, 0);
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_TAURUS_BLACK, index, -1, -2);
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Aquarius:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK2, index, -3, -3);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.EarthElemental:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.WaterElemental:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.EarthWisp:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_roughWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_roughBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.WaterWisp:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Banshee:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_bansheeWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_bansheeBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Harpy:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_BISHOP2, index, -2, -2);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_harpyWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_harpyBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Hag:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_hagWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_hagBlack |= pattern;
                    }
                    break;
                case PieceType.RabbitDiplomat:
                case PieceType.Diplomat:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_waterWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_waterBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Enforcer:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ROOK1, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_immobilizerWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_immobilizerBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Watchtower:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_watchTowerWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_watchTowerBlack |= pattern;
                    }
                    break;
                case Piece.PieceType.Fan:
                    pattern = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, index, -1, -1);
                    if (ipa == Piece.PieceAlignment.White)
                    {
                        b.globalData.bitboard_fanWhite |= pattern;
                    }
                    if (ipa == Piece.PieceAlignment.Black)
                    {
                        b.globalData.bitboard_fanBlack |= pattern;
                    }
                    break;
            }
        }

        //delete bad bitboard stuff with virgo
        b.globalData.bitboard_bansheeWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_attractorWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_repulserWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_immobilizerWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_harpyWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_hagWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_slothWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_watchTowerWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_fanWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_hangedWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_roughWhite &= ~b.globalData.bitboard_virgoAuraBlack;
        b.globalData.bitboard_waterWhite &= ~b.globalData.bitboard_virgoAuraBlack;

        if ((b.globalData.playerModifier & Board.PlayerModifier.ImmunityZone) != 0)
        {
            b.globalData.bitboard_virgoAuraWhite |= 0x000000000000003c;
            b.globalData.bitboard_immuneWhite |= 0x000000000000003c;
        }

        b.globalData.bitboard_bansheeBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_attractorBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_repulserBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_immobilizerBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_harpyBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_hagBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_slothBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_watchTowerBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_fanBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_hangedBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_roughBlack &= ~b.globalData.bitboard_virgoAuraWhite;
        b.globalData.bitboard_waterBlack &= ~b.globalData.bitboard_virgoAuraWhite;
    }

    //Add moves for the entire target alignment
    //(need to add neutrals and crystals later as well as special movers that gain moves based on defenders)
    private static void GenerateMovesForAlignment(List<uint> moves, ref Board b, Piece.PieceAlignment pa, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata)
    {
        //idea to optimize?
        //This assumes that the pop step is not like 4x slower than the normal loop (which is hopefully true)
        ulong pieceBitboard = pa == Piece.PieceAlignment.White ? b.globalData.bitboard_piecesWhite : b.globalData.bitboard_piecesBlack;

        while (pieceBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(pieceBitboard, out pieceBitboard);

            uint piece = b.pieces[index];
            if (piece != 0)
            {
                GenerateMovesForPiece(moves, ref b, pa, piece, index & 7, index >> 3, mbt, moveMetadata);
            }
        }

        return;
    }


    //Add moves for the target piece
    private static void GenerateMovesForPiece(List<uint> moves, ref Board b, Piece.PieceAlignment pa, uint piece, int x, int y, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata)
    {
        //unnecessary check
        //Null type pieces should not exist
        /*
        if (Piece.GetPieceType(piece) == Piece.PieceType.Null)
        {
            return;
            //return moveStartIndex;
        }
        */

        //Stunned = can't move
        Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);
        if (pse == Piece.PieceStatusEffect.Frozen)
        {
            return;
        }

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        int xy = (x + (y << 3));

        //Get all move generator info entries
        PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache((xy), piece); //GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece));

        //arcana fool trying to copy nothing?
        if (pte == null)
        {
            return;
        }

        //hardcoded thing
        if (pte.type == PieceType.MegaCannon && Piece.GetPieceSpecialData(piece) != 0)
        {
            return;
        }


        if (pa == Piece.PieceAlignment.White)
        {
            //Unpredictable
            if ((b.globalData.enemyModifier & Board.EnemyModifier.Unpredictable) != 0)
            {
                if (xy == b.whitePerPlayerInfo.lastPieceMovedLocation)
                {
                    return;
                }
            }

            //Isolated
            if ((b.globalData.enemyModifier & Board.EnemyModifier.Isolated) != 0)
            {
                ulong bitIndex = 1uL << xy;
                if ((bitIndex & b.globalData.bitboard_piecesBlackAdjacent) != 0 && (bitIndex & MainManager.SmearBitboard(b.globalData.bitboard_piecesWhite & ~bitIndex)) == 0)
                {
                    return;
                }
            }

            //Forbid moving the enemies last moved piece (This is a buff to Hypnotizer but is also there to prevent you from just undoing your opponents last move every time)
            //Note: Can't be a simple type check
            //  Because if you get bonus moves you can capture the last enemy moved thing and then your piece is on the enemy last moved location
            //There are edge cases where this check will fail (Mostly things that change piece type or alignment)
            //But this should not have false positives
            /*
            if (b.blackPerPlayerInfo.lastPieceMovedLocation == (xy) && (Piece.GetPieceAlignment(piece) == Piece.GetPieceAlignment(b.blackPerPlayerInfo.lastPieceMoved)) && (pte.type == b.blackPerPlayerInfo.lastPieceMovedType))
            {
                return;
            }
            */
            //alt check that is more severe
            if (b.blackPerPlayerInfo.lastPieceMovedLocation == (xy) && b.bonusPly == 0)
            {
                return;
            }

            if (((pte.pieceProperty & Piece.PieceProperty.EnchantImmune) == 0) && Piece.GetPieceModifier(piece) != PieceModifier.Immune && ((1uL << xy & b.globalData.bitboard_immobilizerBlack) != 0))
            {
                return;
            }
            if ((pte.pieceProperty & Piece.PieceProperty.SlowMove) != 0 && (b.bonusPly > 0 || (b.whitePerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null && (GlobalPieceManager.GetPieceTableEntry(b.whitePerPlayerInfo.lastPieceMovedType).pieceProperty & Piece.PieceProperty.SlowMove) != 0)))
            {
                return;
            }
        }
        else if (pa == Piece.PieceAlignment.Black)
        {
            //Forbid moving the enemies last moved piece (This is a buff to Hypnotizer but is also there to prevent you from just undoing your opponents last move every time)
            //There are edge cases where this check will fail (Mostly things that change piece type or alignment)
            //But this should not have false positives
            /*
            Debug.Log((b.whitePerPlayerInfo.lastPieceMovedLocation + " " + (xy)) + " " + (Piece.GetPieceAlignment(piece) + " " + Piece.GetPieceAlignment(b.whitePerPlayerInfo.lastPieceMoved)) + " " + (pte.type + " " + b.whitePerPlayerInfo.lastPieceMovedType));
            if (b.whitePerPlayerInfo.lastPieceMovedLocation == (xy) && (Piece.GetPieceAlignment(piece) == Piece.GetPieceAlignment(b.whitePerPlayerInfo.lastPieceMoved)) && (pte.type == b.whitePerPlayerInfo.lastPieceMovedType))
            {
                return;
            }
            */
            //alt check that is more severe
            if (b.whitePerPlayerInfo.lastPieceMovedLocation == (xy) && b.bonusPly == 0)
            {
                return;
            }

            if (((pte.pieceProperty & Piece.PieceProperty.EnchantImmune) == 0) && Piece.GetPieceModifier(piece) != PieceModifier.Immune && ((1uL << xy & b.globalData.bitboard_immobilizerWhite) != 0))
            {
                return;
            }
            if ((pte.pieceProperty & Piece.PieceProperty.SlowMove) != 0 && (b.bonusPly > 0 || (b.blackPerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null && (GlobalPieceManager.GetPieceTableEntry(b.blackPerPlayerInfo.lastPieceMovedType).pieceProperty & Piece.PieceProperty.SlowMove) != 0)))
            {
                return;
            }
        }

        //Giant piece wrong corners can't move (to simplify move generation)
        if (((pte.piecePropertyB & Piece.PiecePropertyB.Giant) != 0) && Piece.GetPieceSpecialData(piece) != 0)
        {
            return;
        }

        Piece.PieceModifier pm = Piece.GetPieceModifier(piece);

        if (pm == Piece.PieceModifier.NoSpecial)
        {
            for (int i = 0; i < pte.moveInfo.Length; i++)
            {
                if (pte.moveInfo[i].atom >= MoveGeneratorAtom.SpecialMoveDivider)
                {
                    continue;
                }
                GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
            }
            return;
        }

        uint targetPiece = piece;
        switch (pte.type)
        {
            case PieceType.ArcanaFool:
                if (!b.blackToMove && pa == Piece.PieceAlignment.White && b.whitePerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null)
                {
                    targetPiece = Piece.SetPieceType(b.whitePerPlayerInfo.lastPieceMovedType, targetPiece);
                    targetPiece = Piece.SetPieceModifier(Piece.PieceModifier.NoSpecial, targetPiece);
                }
                if (b.blackToMove && pa == Piece.PieceAlignment.Black && b.blackPerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null)
                {
                    targetPiece = Piece.SetPieceType(b.blackPerPlayerInfo.lastPieceMovedType, targetPiece);
                    targetPiece = Piece.SetPieceModifier(Piece.PieceModifier.NoSpecial, targetPiece);
                }

                //This causes infinite loops
                if (Piece.GetPieceType(targetPiece) == Piece.PieceType.ArcanaFool)
                {
                    targetPiece = Piece.SetPieceType(Piece.PieceType.Null, targetPiece);
                }

                if (Piece.GetPieceType(targetPiece) != PieceType.Null)
                {
                    //don't really want to deal with pieces reacting to Fool's movement so no MBT?
                    GenerateMovesForPiece(moves, ref b, pa, targetPiece, x, y, null, moveMetadata);
                }
                return;
            case PieceType.Imitator:
                //Imitator is like Arcana Fool but applies to the last enemy piece moved
                //So it is potentially stronger if the enemy has better stuff, but since it relies on the enemy they have a better idea of how to avoid it
                if (!b.blackToMove && pa == Piece.PieceAlignment.White && b.blackPerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null)
                {
                    targetPiece = Piece.SetPieceType(b.blackPerPlayerInfo.lastPieceMovedType, targetPiece);
                    targetPiece = Piece.SetPieceModifier(Piece.PieceModifier.NoSpecial, targetPiece);
                }
                if (b.blackToMove && pa == Piece.PieceAlignment.Black && b.whitePerPlayerInfo.lastPieceMovedType != Piece.PieceType.Null)
                {
                    targetPiece = Piece.SetPieceType(b.whitePerPlayerInfo.lastPieceMovedType, targetPiece);
                    targetPiece = Piece.SetPieceModifier(Piece.PieceModifier.NoSpecial, targetPiece);
                }

                //This causes infinite loops?

                //White last moved is imitator
                //Black last moved is fool
                //(white)imitator -> (black)fool -> (white)fool -> (white)imitator
                //Block imitator -> fool because a 2 step thing is already hard to think through in practice
                Piece.PieceType tI = Piece.GetPieceType(targetPiece);
                if (tI == Piece.PieceType.ArcanaFool || tI == Piece.PieceType.Imitator)
                {
                    targetPiece = Piece.SetPieceType(Piece.PieceType.Null, targetPiece);
                }

                if (Piece.GetPieceType(targetPiece) != PieceType.Null)
                {
                    //don't really want to deal with pieces reacting to Fool's movement so no MBT?
                    GenerateMovesForPiece(moves, ref b, pa, targetPiece, x, y, null, moveMetadata);
                }
                return;
            case PieceType.ArcanaLovers:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                //Better condition now :)
                ulong a2Bitboard = 0;
                if (pa == PieceAlignment.White)
                {
                    a2Bitboard = b.globalData.bitboard_piecesWhiteAdjacent2 | b.globalData.bitboard_piecesWhiteAdjacent4 | b.globalData.bitboard_piecesWhiteAdjacent8;
                }
                if (pa == PieceAlignment.Black)
                {
                    a2Bitboard = b.globalData.bitboard_piecesBlackAdjacent2 | b.globalData.bitboard_piecesBlackAdjacent4 | b.globalData.bitboard_piecesBlackAdjacent8;
                }

                if ((1uL << (xy) & a2Bitboard) != 0)  //3 because it counts Arcana Lovers itself as one of the neighbors
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
        }

        ulong allyBitboard = pa == Piece.PieceAlignment.Black ? b.globalData.bitboard_piecesBlack : b.globalData.bitboard_piecesWhite;
        ulong enemyBitboardAdjacent = pa == Piece.PieceAlignment.Black ? b.globalData.bitboard_piecesWhiteAdjacent : b.globalData.bitboard_piecesBlackAdjacent;
        ulong entry = 0;
        switch (pte.enhancedMoveType)
        {
            case Piece.EnhancedMoveType.PartialForcedMoves:
            case Piece.EnhancedMoveType.PartialForcedCapture:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (mbt == null)
                {
                    entry = 0;
                }
                else
                {
                    entry = mbt.Get(x, y);
                }

                ulong emptyBitboard = ~b.globalData.bitboard_pieces;

                if (pte.enhancedMoveType == Piece.EnhancedMoveType.PartialForcedCapture)
                {
                    allyBitboard |= emptyBitboard;
                }

                if ((entry & ~allyBitboard) == 0)
                {
                    //because this generates in a suboptimal order the PFC movers have to not use mbt
                    if (pte.enhancedMoveType == Piece.EnhancedMoveType.PartialForcedCapture)
                    {
                        for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], null, moveMetadata);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                        }
                    }
                }
                return;
            case Piece.EnhancedMoveType.InverseForcedMoves:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (mbt == null)
                {
                    entry = 0;
                }
                else
                {
                    entry = mbt.Get(x, y);
                }

                if ((entry & ~allyBitboard) != 0)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.SwitchMover:
                if (((x + y) & 1) == 0)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                else
                {
                    for (int i = 0; i < pte.moveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.WarMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (((1uL << xy) & enemyBitboardAdjacent) != 0)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.ShyMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (((1uL << xy) & enemyBitboardAdjacent) != 0)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.NoAllyMover:
                //problem: the piece itself is inside the bitboard so it would be considered adjacent to itself
                allyBitboard = MainManager.SmearBitboard(allyBitboard & ~(1uL << xy));

                //MainManager.PrintBitboard(allyBitboard);
                if ((allyBitboard & (1uL << xy)) == 0)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                else
                {
                    for (int i = 0; i < pte.moveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.AllyMover:
                //problem: the piece itself is inside the bitboard so it would be considered adjacent to itself
                allyBitboard = MainManager.SmearBitboard(allyBitboard & ~(1uL << xy));

                //MainManager.PrintBitboard(allyBitboard);
                if ((allyBitboard & (1uL << xy)) == 0)
                {
                    for (int i = 0; i < pte.moveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                    }
                }
                else
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.JusticeMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (pa == Piece.PieceAlignment.Black ? (b.whitePerPlayerInfo.capturedLastTurn) : b.blackPerPlayerInfo.capturedLastTurn)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.DiligenceMover:
                int lastPieceLocation = -1;
                if (b.blackToMove)
                {
                    lastPieceLocation = b.blackPerPlayerInfo.lastPieceMovedLocation;
                }
                else
                {
                    lastPieceLocation = b.whitePerPlayerInfo.lastPieceMovedLocation;
                }

                if (lastPieceLocation == xy)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                else
                {
                    for (int i = 0; i < pte.moveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.VampireMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (b.blackPerPlayerInfo.capturedLastTurn || b.whitePerPlayerInfo.capturedLastTurn)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.FearfulMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (!(b.blackPerPlayerInfo.capturedLastTurn || b.whitePerPlayerInfo.capturedLastTurn))
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.FarHalfMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (pa == Piece.PieceAlignment.Black ? y < 4 : y >= 4)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
            case Piece.EnhancedMoveType.CloseHalfMover:
                for (int i = 0; i < pte.moveInfo.Length; i++)
                {
                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
                }

                if (pa == Piece.PieceAlignment.White ? y < 4 : y >= 4)
                {
                    for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                    {
                        GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.enhancedMoveInfo[i], mbt, moveMetadata);
                    }
                }
                return;
        }

        for (int i = 0; i < pte.moveInfo.Length; i++)
        {
            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, x, y, pte.moveInfo[i], mbt, moveMetadata);
        }

        return;
    }

    //To do: Move these to a MoveGeneration class
    public static void GenerateMovesForPlayer(List<uint> moves, ref Board b, Piece.PieceAlignment pa, Dictionary<uint, MoveMetadata> moveMetadata)
    {
        //Debug.Log("Generate for " + pa);
        //Future thing to do: pieces need to cast ability ranges with a third move range pass before the first 2 (for passive ability ranges)
        GenerateAreaBitboards(ref b);

        //MoveBitTable mbt = new MoveBitTable();
        MoveBitTable mbt = b.globalData.mbtactive;
        if (b.globalData.mbtactive == null)
        {
            b.globalData.mbtactive = new MoveBitTable();
            mbt = b.globalData.mbtactive;
        }
        mbt.Reset();
        GenerateMovesForAlignment(moves, ref b, pa, mbt, moveMetadata);
        //mbt is populated with stuff
        GenerateSecondaryMoves(moves, ref b, pa, mbt, moveMetadata);

        //This is a weird thing to do
        //Will this cause me problems later?

        //Probably not
        //Currently this is only used to turn Shielded into Half Shielded

        //This is the function (generatemovesforplayer) a board uses to determine who is attacked
        //So this should not cause me problems

        //Note: needs special handling because BoardScript calls this to generate enemy moves to show you what the enemy can do

        if (!(pa == Piece.PieceAlignment.White ^ !b.blackToMove))
        {
            b.RunTurnStart(pa == Piece.PieceAlignment.Black);
        }
    }

    public static void GeneratePieceBitboardsFull(Board b)
    {
        b.globalData.bitboard_piecesWhite = 0;
        b.globalData.bitboard_piecesBlack = 0;
        b.globalData.bitboard_piecesNeutral = 0;
        b.globalData.bitboard_piecesCrystal = 0;
        b.globalData.bitboard_pawnsWhite = 0;
        b.globalData.bitboard_pawnsBlack = 0;
        b.globalData.bitboard_kingWhite = 0;
        b.globalData.bitboard_kingBlack = 0;

        b.globalData.bitboard_immuneWhite = 0;
        b.globalData.bitboard_immuneBlack = 0;
        b.globalData.bitboard_immuneRelayerWhite = 0;
        b.globalData.bitboard_immuneRelayerBlack = 0;

        b.bitboard_autoWhite = 0;
        b.bitboard_autoBlack = 0;
        b.globalData.bitboard_abominationWhite = 0;
        b.globalData.bitboard_abominationBlack = 0;
        b.globalData.bitboard_zombieWhite = 0;
        b.globalData.bitboard_zombieBlack = 0;
        b.globalData.bitboard_clockworksnapperWhite = 0;
        b.globalData.bitboard_clockworksnapperBlack = 0;
        b.globalData.bitboard_bladebeastWhite = 0;
        b.globalData.bitboard_bladebeastBlack = 0;
        b.globalData.bitboard_tarotMoonWhite = 0;
        b.globalData.bitboard_tarotMoonBlack = 0;
        b.globalData.bitboard_tarotMoonIllusionWhite = 0;
        b.globalData.bitboard_tarotMoonIllusionBlack = 0;
        b.globalData.bitboard_virgoWhite = 0;
        b.globalData.bitboard_virgoBlack = 0;
        b.globalData.bitboard_warpWeaverWhite = 0;
        b.globalData.bitboard_warpWeaverBlack = 0;
        b.globalData.bitboard_metalFoxWhite = 0;
        b.globalData.bitboard_metalFoxBlack = 0;
        b.globalData.bitboard_megacannonWhite = 0;
        b.globalData.bitboard_megacannonBlack = 0;
        b.globalData.bitboard_momentumWhite = 0;
        b.globalData.bitboard_momentumBlack = 0;
        b.globalData.bitboard_sludgeTrail = 0;
        b.globalData.bitboard_daySwapper = 0;
        b.globalData.bitboard_seasonSwapper = 0;
        b.globalData.bitboard_egg = 0;
        b.globalData.bitboard_noStatus = 0;
        b.globalData.bitboard_noallyblock = 0;
        b.globalData.bitboard_noenemyblock = 0;
        b.globalData.bitboard_rabbit = 0;
        b.globalData.bitboard_shielded = 0;
        b.globalData.bitboard_secondary = 0;
        b.globalData.bitboard_aura = 0;

        b.globalData.bitboard_updatedPieces = 0;

        //How good is parallel for?
        //93 ms without
        //1900 ms with
        //So it's garbage :(

        //does the compiler like b.pieces.Length better than 64?
        //Test is negligible
        for (int i = 0; i < 64; i++)
        {
            uint piece = b.pieces[i];
            if (piece == 0)
            {
                continue;
            }

            ulong bitIndex = 1uL << i;
            //Piece.PieceAlignment ppa = Piece.GetPieceAlignment(piece);
            uint ppaB = (piece & 0xc0000000);

            //b.globalData.GetPieceAlignmentFromCache(i, piece);

            //note: the cache thing is roughly equal to Piece.GetPieceType

            PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(i, piece); //GlobalPieceManager.GetPieceTableEntry(b.pieces[i]);
            //Piece.PieceType pt = pte.type; //Piece.GetPieceType(b.pieces[i]);
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceType pt = pte.type;

            //? ms
            //Very slightly worse
            //Negligible difference
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

            /*
            if (pte == null)
            {
                Debug.LogError("Type 0 piece detected: " + Piece.ConvertToString(piece));
                continue;
            }
            */

            switch (ppaB)
            {
                case 0: //Piece.PieceAlignment.White:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingWhite |= bitIndex;
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        b.globalData.bitboard_piecesWhite |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesWhite |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsWhite |= bitIndex;
                    }
                    break;
                case 0x40000000: //Piece.PieceAlignment.Black:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingBlack |= bitIndex;
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        b.globalData.bitboard_piecesBlack |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesBlack |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsBlack |= bitIndex;
                    }
                    break;
                case 0x80000000: //Piece.PieceAlignment.Neutral:
                    b.globalData.bitboard_piecesNeutral |= bitIndex;
                    break;
                case 0xc0000000: // Piece.PieceAlignment.Crystal:
                    b.globalData.bitboard_piecesCrystal |= bitIndex;
                    break;
            }

            switch (pt)
            {
                case Piece.PieceType.ArcanaMoon:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.MoonIllusion:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonIllusionBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.DivineApothecary:
                case Piece.PieceType.Virgo:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_virgoWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            b.globalData.bitboard_immuneRelayerWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_virgoBlack |= bitIndex;
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            b.globalData.bitboard_immuneRelayerBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Abomination:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_abominationWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_abominationBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Zombie:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_zombieWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_zombieBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.BladeBeast:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_bladebeastWhite |= bitIndex;
                            b.bitboard_autoWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_bladebeastBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.ClockworkSnapper:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_clockworksnapperWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_clockworksnapperBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.WarpWeaver:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_warpWeaverWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_warpWeaverBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MetalFox:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_metalFoxWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_metalFoxBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MegaCannon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_megacannonWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_megacannonBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.SludgeTrail:
                    b.globalData.bitboard_sludgeTrail |= bitIndex;
                    continue;
                case PieceType.DayPawn:
                case PieceType.DayBishop:
                case PieceType.DayQueen:
                case PieceType.NightPawn:
                case PieceType.NightKnight:
                case PieceType.NightQueen:
                    b.globalData.bitboard_daySwapper |= bitIndex;
                    continue;
                case PieceType.SummerQueen:
                case PieceType.SummerRook:
                case PieceType.SpringKnight:
                case PieceType.SummerPawn:
                case PieceType.SpringPawn:
                case PieceType.WinterQueen:
                case PieceType.WinterBishop:
                case PieceType.FallKnight:
                case PieceType.WinterPawn:
                case PieceType.FallPawn:
                    b.globalData.bitboard_seasonSwapper |= bitIndex;
                    continue;
                case PieceType.FlameEgg:
                case PieceType.WaveEgg:
                case PieceType.RockEgg:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            break;
                    }
                    b.globalData.bitboard_egg |= bitIndex;
                    continue;
                case PieceType.RollerQueen:
                case PieceType.Roller:
                case PieceType.BounceBishop:
                case PieceType.ReboundRook:
                case PieceType.Balloon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_momentumWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_momentumBlack |= bitIndex;
                            break;
                    }
                    break;
                case PieceType.RelayKnight:
                case PieceType.RelayBishop:
                case PieceType.RelayRook:
                case PieceType.RelayQueen:
                case PieceType.Hypnotist:
                case PieceType.Kindness:
                case PieceType.Envy:
                case PieceType.ArcanaEmpress:
                case PieceType.ArcanaWorld:
                case PieceType.ArcanaSun:
                case PieceType.Charity:
                case PieceType.DivineBeacon:
                case PieceType.DivineMusician:
                    b.globalData.bitboard_secondary |= bitIndex;
                    break;
                case PieceType.Attractor:
                case PieceType.Repulser:
                case PieceType.Immobilizer:
                case PieceType.Entrancer:
                case PieceType.Charmer:
                case PieceType.Sloth:
                case PieceType.ArcanaHierophant:
                case PieceType.ArcanaHanged:
                case PieceType.AceOfPentacles:
                case PieceType.AceOfCups:
                case PieceType.PageOfPentacles:
                case PieceType.PageOfCups:
                case PieceType.QueenOfPentacles:
                case PieceType.QueenOfCups:
                case PieceType.Earth:
                case PieceType.Saturn:
                case PieceType.Ganymede:
                case PieceType.Taurus:
                case PieceType.Aquarius:
                case PieceType.EarthElemental:
                case PieceType.WaterElemental:
                case PieceType.EarthWisp:
                case PieceType.WaterWisp:
                case PieceType.Banshee:
                case PieceType.Harpy:
                case PieceType.Hag:
                case PieceType.Diplomat:
                case PieceType.Enforcer:
                case PieceType.Watchtower:
                case PieceType.Fan:
                    b.globalData.bitboard_aura |= bitIndex;
                    break;
                case PieceType.RabbitDiplomat:
                    b.globalData.bitboard_aura |= bitIndex;
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
                case PieceType.RabbitQueen:
                case PieceType.RabbitCourier:
                case PieceType.RabbitKnight:
                case PieceType.Rabbit:
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
            }

            Piece.PieceModifier pm = Piece.GetPieceModifier(piece);

            //Put naturally immune pieces in the immunity bitboard
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);
            if ((pte.pieceProperty & Piece.PieceProperty.RelayImmune) != 0)
            {
                switch (ppaB)
                {
                    case 0:
                        b.globalData.bitboard_immuneRelayerWhite |= bitIndex;
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        break;
                    case 0x40000000:
                        b.globalData.bitboard_immuneRelayerBlack |= bitIndex;
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        break;
                }
            }
            else if ((pte.pieceProperty & Piece.PieceProperty.EnchantImmune) != 0 || pm== PieceModifier.Immune)
            {
                switch (ppaB)
                {
                    case 0:
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        break;
                    case 0x40000000:
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        break;
                }
            }

            switch (pm)
            {
                case PieceModifier.HalfShielded:
                    break;
                case PieceModifier.Immune:
                    b.globalData.bitboard_aura |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Shielded:
                    b.globalData.bitboard_shielded |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Spectral:
                    b.globalData.bitboard_noallyblock |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                default:
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
            }

            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingAlly) != 0 || ((i & 56) == 16 && ppaB == 0 && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
            {
                b.globalData.bitboard_noallyblock |= bitIndex;
            }
            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingEnemy) != 0 || (piece & 0x3C00000) == 0x1800000)
            {
                b.globalData.bitboard_noenemyblock |= bitIndex;
            }
        }

        if ((b.globalData.playerModifier & Board.PlayerModifier.FullArmyWhiteBadges) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite;
        }
        else
        {
            if ((b.globalData.playerModifier & Board.PlayerModifier.Recall) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & ~MoveGenerator.BITBOARD_PATTERN_RANK1;
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Seafaring) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (MoveGenerator.BITBOARD_PATTERN_SIDEEDGES);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Backdoor) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (0x6600000000000066);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.FlyingGeneral) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_kingWhite;
            }
        }

        if ((b.globalData.enemyModifier & Board.EnemyModifier.Defensive) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesBlack;
        }
        else if ((b.globalData.enemyModifier & Board.EnemyModifier.KingMoveModifiers) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_kingBlack;
        }

        b.globalData.bitboard_rabbitAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_rabbit);

        b.globalData.bitboard_EOTPieces = b.globalData.bitboard_daySwapper | b.globalData.bitboard_seasonSwapper | b.globalData.bitboard_egg | (b.globalData.bitboard_rabbit & MoveGenerator.BITBOARD_PATTERN_RANK18);


        //X -> X white and X black
        //slower actually???
        //???
        /*
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        //doing it for the others is slow?
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        b.globalData.bitboard_immuneWhite = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneBlack = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerWhite = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerBlack = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;

        b.globalData.bitboard_abominationWhite = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_abominationBlack = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieWhite = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieBlack = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperWhite = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperBlack = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastWhite = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastBlack = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonWhite = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonBlack = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionWhite = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionBlack = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoWhite = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoBlack = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverWhite = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverBlack = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxWhite = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxBlack = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonWhite = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonBlack = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumWhite = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumBlack = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        */

        b.globalData.bitboard_piecesWhiteAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesWhite);
        b.globalData.bitboard_piecesBlackAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesBlack);

        (b.globalData.bitboard_piecesWhiteAdjacent1, b.globalData.bitboard_piecesWhiteAdjacent2, b.globalData.bitboard_piecesWhiteAdjacent4, b.globalData.bitboard_piecesWhiteAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesWhite);
        (b.globalData.bitboard_piecesBlackAdjacent1, b.globalData.bitboard_piecesBlackAdjacent2, b.globalData.bitboard_piecesBlackAdjacent4, b.globalData.bitboard_piecesBlackAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesBlack);

        b.globalData.bitboard_pieces = b.globalData.bitboard_piecesWhite | b.globalData.bitboard_piecesBlack | b.globalData.bitboard_piecesNeutral | b.globalData.bitboard_piecesCrystal;
        b.globalData.bitboard_piecesMirrored = MainManager.MirrorBitboard(b.globalData.bitboard_pieces);

        b.globalData.arcanaMoonOutdated = false;
    }
    public static void GeneratePieceBitboards(Board b)
    {
        ulong antiUpdate = ~b.globalData.bitboard_updatedPieces;

        if (antiUpdate == 0)
        {
            //Faster to just update with a normal for loop
            //(this does stuff that isn't in postturn but ehh
            GeneratePieceBitboardsFull(b);
            return;
        }

        b.globalData.bitboard_piecesWhite &= antiUpdate;
        b.globalData.bitboard_piecesBlack &= antiUpdate;
        b.globalData.bitboard_piecesNeutral &= antiUpdate;
        b.globalData.bitboard_piecesCrystal &= antiUpdate;
        b.globalData.bitboard_pawnsWhite &= antiUpdate;
        b.globalData.bitboard_pawnsBlack &= antiUpdate;
        b.globalData.bitboard_kingWhite &= antiUpdate;
        b.globalData.bitboard_kingBlack &= antiUpdate;

        b.globalData.bitboard_immuneWhite &= antiUpdate;
        b.globalData.bitboard_immuneBlack &= antiUpdate;
        b.globalData.bitboard_immuneRelayerWhite &= antiUpdate;
        b.globalData.bitboard_immuneRelayerBlack &= antiUpdate;

        b.bitboard_autoWhite &= antiUpdate;
        b.bitboard_autoBlack &= antiUpdate;
        b.globalData.bitboard_abominationWhite &= antiUpdate;
        b.globalData.bitboard_abominationBlack &= antiUpdate;
        b.globalData.bitboard_zombieWhite &= antiUpdate;
        b.globalData.bitboard_zombieBlack &= antiUpdate;
        b.globalData.bitboard_clockworksnapperWhite &= antiUpdate;
        b.globalData.bitboard_clockworksnapperBlack &= antiUpdate;
        b.globalData.bitboard_bladebeastWhite &= antiUpdate;
        b.globalData.bitboard_bladebeastBlack &= antiUpdate;
        b.globalData.bitboard_tarotMoonWhite &= antiUpdate;
        b.globalData.bitboard_tarotMoonBlack &= antiUpdate;
        b.globalData.bitboard_tarotMoonIllusionWhite &= antiUpdate;
        b.globalData.bitboard_tarotMoonIllusionBlack &= antiUpdate;
        b.globalData.bitboard_virgoWhite &= antiUpdate;
        b.globalData.bitboard_virgoBlack &= antiUpdate;
        b.globalData.bitboard_warpWeaverWhite &= antiUpdate;
        b.globalData.bitboard_warpWeaverBlack &= antiUpdate;
        b.globalData.bitboard_metalFoxWhite &= antiUpdate;
        b.globalData.bitboard_metalFoxBlack &= antiUpdate;
        b.globalData.bitboard_megacannonWhite &= antiUpdate;
        b.globalData.bitboard_megacannonBlack &= antiUpdate;
        b.globalData.bitboard_momentumWhite &= antiUpdate;
        b.globalData.bitboard_momentumBlack &= antiUpdate;
        b.globalData.bitboard_sludgeTrail &= antiUpdate;
        b.globalData.bitboard_daySwapper &= antiUpdate;
        b.globalData.bitboard_seasonSwapper &= antiUpdate;
        b.globalData.bitboard_egg &= antiUpdate;
        b.globalData.bitboard_noStatus &= antiUpdate;
        b.globalData.bitboard_noallyblock &= antiUpdate;
        b.globalData.bitboard_noenemyblock &= antiUpdate;
        b.globalData.bitboard_rabbit &= antiUpdate;
        b.globalData.bitboard_shielded &= antiUpdate;
        b.globalData.bitboard_secondary &= antiUpdate;
        b.globalData.bitboard_aura &= antiUpdate;

        //How good is parallel for?
        //93 ms without
        //1900 ms with
        //So it's garbage :(

        //does the compiler like b.pieces.Length better than 64?
        //Test is negligible
        ulong update = b.globalData.bitboard_updatedPieces;
        //Debug.Log(update);
        //for (int i = 0; i < 64; i++)
        while (update != 0)
        {
            int i = MainManager.PopBitboardLSB1(update, out update);
            uint piece = b.pieces[i];
            if (piece == 0)
            {
                continue;
            }

            ulong bitIndex = 1uL << i;
            //Piece.PieceAlignment ppa = Piece.GetPieceAlignment(piece);
            uint ppaB = (piece & 0xc0000000);

            //b.globalData.GetPieceAlignmentFromCache(i, piece);

            //note: the cache thing is roughly equal to Piece.GetPieceType

            PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(i, piece); //GlobalPieceManager.GetPieceTableEntry(b.pieces[i]);
            //Piece.PieceType pt = pte.type; //Piece.GetPieceType(b.pieces[i]);
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceType pt = pte.type;

            //? ms
            //Very slightly worse
            //Negligible difference
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

            /*
            if (pte == null)
            {
                Debug.LogError("Type 0 piece detected: " + Piece.ConvertToString(piece));
                continue;
            }
            */

            switch (ppaB)
            {
                case 0: //Piece.PieceAlignment.White:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingWhite |= bitIndex;
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        b.globalData.bitboard_piecesWhite |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesWhite |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsWhite |= bitIndex;
                    }
                    break;
                case 0x40000000: //Piece.PieceAlignment.Black:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingBlack |= bitIndex;
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        b.globalData.bitboard_piecesBlack |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesBlack |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsBlack |= bitIndex;
                    }
                    break;
                case 0x80000000: //Piece.PieceAlignment.Neutral:
                    b.globalData.bitboard_piecesNeutral |= bitIndex;
                    break;
                case 0xc0000000: // Piece.PieceAlignment.Crystal:
                    b.globalData.bitboard_piecesCrystal |= bitIndex;
                    break;
            }

            switch (pt)
            {
                case Piece.PieceType.ArcanaMoon:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.MoonIllusion:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonIllusionBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.DivineApothecary:
                case Piece.PieceType.Virgo:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_virgoWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            b.globalData.bitboard_immuneRelayerWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_virgoBlack |= bitIndex;
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            b.globalData.bitboard_immuneRelayerBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Abomination:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_abominationWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_abominationBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Zombie:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_zombieWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_zombieBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.BladeBeast:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_bladebeastWhite |= bitIndex;
                            b.bitboard_autoWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_bladebeastBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.ClockworkSnapper:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_clockworksnapperWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_clockworksnapperBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.WarpWeaver:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_warpWeaverWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_warpWeaverBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MetalFox:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_metalFoxWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_metalFoxBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MegaCannon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_megacannonWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_megacannonBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.SludgeTrail:
                    b.globalData.bitboard_sludgeTrail |= bitIndex;
                    continue;
                case PieceType.DayPawn:
                case PieceType.DayBishop:
                case PieceType.DayQueen:
                case PieceType.NightPawn:
                case PieceType.NightKnight:
                case PieceType.NightQueen:
                    b.globalData.bitboard_daySwapper |= bitIndex;
                    continue;
                case PieceType.SummerQueen:
                case PieceType.SummerRook:
                case PieceType.SpringKnight:
                case PieceType.SummerPawn:
                case PieceType.SpringPawn:
                case PieceType.WinterQueen:
                case PieceType.WinterBishop:
                case PieceType.FallKnight:
                case PieceType.WinterPawn:
                case PieceType.FallPawn:
                    b.globalData.bitboard_seasonSwapper |= bitIndex;
                    continue;
                case PieceType.FlameEgg:
                case PieceType.WaveEgg:
                case PieceType.RockEgg:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            break;
                    }
                    b.globalData.bitboard_egg |= bitIndex;
                    continue;
                case PieceType.RollerQueen:
                case PieceType.Roller:
                case PieceType.BounceBishop:
                case PieceType.ReboundRook:
                case PieceType.Balloon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_momentumWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_momentumBlack |= bitIndex;
                            break;
                    }
                    break;
                case PieceType.RelayKnight:
                case PieceType.RelayBishop:
                case PieceType.RelayRook:
                case PieceType.RelayQueen:
                case PieceType.Hypnotist:
                case PieceType.Kindness:
                case PieceType.Envy:
                case PieceType.ArcanaEmpress:
                case PieceType.ArcanaWorld:
                case PieceType.ArcanaSun:
                case PieceType.Charity:
                case PieceType.DivineBeacon:
                case PieceType.DivineMusician:
                    b.globalData.bitboard_secondary |= bitIndex;
                    break;
                case PieceType.Attractor:
                case PieceType.Repulser:
                case PieceType.Immobilizer:
                case PieceType.Entrancer:
                case PieceType.Charmer:
                case PieceType.Sloth:
                case PieceType.ArcanaHierophant:
                case PieceType.ArcanaHanged:
                case PieceType.AceOfPentacles:
                case PieceType.AceOfCups:
                case PieceType.PageOfPentacles:
                case PieceType.PageOfCups:
                case PieceType.QueenOfPentacles:
                case PieceType.QueenOfCups:
                case PieceType.Earth:
                case PieceType.Saturn:
                case PieceType.Ganymede:
                case PieceType.Taurus:
                case PieceType.Aquarius:
                case PieceType.EarthElemental:
                case PieceType.WaterElemental:
                case PieceType.EarthWisp:
                case PieceType.WaterWisp:
                case PieceType.Banshee:
                case PieceType.Harpy:
                case PieceType.Hag:
                case PieceType.Diplomat:
                case PieceType.Enforcer:
                case PieceType.Watchtower:
                case PieceType.Fan:
                    b.globalData.bitboard_aura |= bitIndex;
                    break;
                case PieceType.RabbitDiplomat:
                    b.globalData.bitboard_aura |= bitIndex;
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
                case PieceType.RabbitQueen:
                case PieceType.RabbitCourier:
                case PieceType.RabbitKnight:
                case PieceType.Rabbit:
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
            }

            //Put naturally immune pieces in the immunity bitboard
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);
            Piece.PieceModifier pm = Piece.GetPieceModifier(piece);

            if ((pte.pieceProperty & Piece.PieceProperty.RelayImmune) != 0)
            {
                switch (ppaB)
                {
                    case 0:
                        b.globalData.bitboard_immuneRelayerWhite |= bitIndex;
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        break;
                    case 0x40000000:
                        b.globalData.bitboard_immuneRelayerBlack |= bitIndex;
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        break;
                }
            }
            else if ((pte.pieceProperty & Piece.PieceProperty.EnchantImmune) != 0 || pm == PieceModifier.Immune)
            {
                switch (ppaB)
                {
                    case 0:
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        break;
                    case 0x40000000:
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        break;
                }
            }

            switch (pm)
            {
                case PieceModifier.HalfShielded:
                    break;
                case PieceModifier.Immune:
                    b.globalData.bitboard_aura |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Shielded:
                    b.globalData.bitboard_shielded |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Spectral:
                    b.globalData.bitboard_noallyblock |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                default:
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
            }

            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingAlly) != 0 || ((i & 56) == 16 && ppaB == 0 && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
            {
                b.globalData.bitboard_noallyblock |= bitIndex;
            }
            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingEnemy) != 0 || (piece & 0x3C00000) == 0x1800000)
            {
                b.globalData.bitboard_noenemyblock |= bitIndex;
            }
        }

        if ((b.globalData.playerModifier & Board.PlayerModifier.FullArmyWhiteBadges) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite;
        }
        else
        {
            if ((b.globalData.playerModifier & Board.PlayerModifier.Recall) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & ~MoveGenerator.BITBOARD_PATTERN_RANK1;
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Seafaring) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (MoveGenerator.BITBOARD_PATTERN_SIDEEDGES);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Backdoor) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (0x6600000000000066);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.FlyingGeneral) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_kingWhite;
            }
        }

        if ((b.globalData.enemyModifier & Board.EnemyModifier.Defensive) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesBlack;
        }
        else if ((b.globalData.enemyModifier & Board.EnemyModifier.KingMoveModifiers) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_kingBlack;
        }

        b.globalData.bitboard_rabbitAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_rabbit);

        b.globalData.bitboard_EOTPieces = b.globalData.bitboard_daySwapper | b.globalData.bitboard_seasonSwapper | b.globalData.bitboard_egg | (b.globalData.bitboard_rabbit & MoveGenerator.BITBOARD_PATTERN_RANK18);


        //X -> X white and X black
        //slower actually???
        //???
        /*
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        //doing it for the others is slow?
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        b.globalData.bitboard_immuneWhite = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneBlack = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerWhite = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerBlack = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;

        b.globalData.bitboard_abominationWhite = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_abominationBlack = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieWhite = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieBlack = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperWhite = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperBlack = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastWhite = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastBlack = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonWhite = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonBlack = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionWhite = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionBlack = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoWhite = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoBlack = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverWhite = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverBlack = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxWhite = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxBlack = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonWhite = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonBlack = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumWhite = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumBlack = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        */

        b.globalData.bitboard_piecesWhiteAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesWhite);
        b.globalData.bitboard_piecesBlackAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesBlack);

        (b.globalData.bitboard_piecesWhiteAdjacent1, b.globalData.bitboard_piecesWhiteAdjacent2, b.globalData.bitboard_piecesWhiteAdjacent4, b.globalData.bitboard_piecesWhiteAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesWhite);
        (b.globalData.bitboard_piecesBlackAdjacent1, b.globalData.bitboard_piecesBlackAdjacent2, b.globalData.bitboard_piecesBlackAdjacent4, b.globalData.bitboard_piecesBlackAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesBlack);

        b.globalData.bitboard_pieces = b.globalData.bitboard_piecesWhite | b.globalData.bitboard_piecesBlack | b.globalData.bitboard_piecesNeutral | b.globalData.bitboard_piecesCrystal;
        b.globalData.bitboard_piecesMirrored = MainManager.MirrorBitboard(b.globalData.bitboard_pieces);

        b.globalData.arcanaMoonOutdated = false;
    }
    public static void GeneratePieceBitboardsPostTurn(Board b)
    {
        ulong antiUpdate = ~b.globalData.bitboard_updatedPieces;

        if (antiUpdate == 0)
        {
            //Faster to just update with a normal for loop
            //(this does stuff that isn't in postturn but ehh
            GeneratePieceBitboardsFull(b);
            return;
        }

        b.globalData.bitboard_piecesWhite &= antiUpdate;
        b.globalData.bitboard_piecesBlack &= antiUpdate;
        b.globalData.bitboard_piecesNeutral &= antiUpdate;
        b.globalData.bitboard_piecesCrystal &= antiUpdate;
        b.globalData.bitboard_pawnsWhite &= antiUpdate;
        b.globalData.bitboard_pawnsBlack &= antiUpdate;
        b.globalData.bitboard_kingWhite &= antiUpdate;
        b.globalData.bitboard_kingBlack &= antiUpdate;

        b.globalData.bitboard_immuneWhite &= antiUpdate;
        b.globalData.bitboard_immuneBlack &= antiUpdate;
        b.globalData.bitboard_immuneRelayerWhite &= antiUpdate;
        b.globalData.bitboard_immuneRelayerBlack &= antiUpdate;

        b.bitboard_autoWhite &= antiUpdate;
        b.bitboard_autoBlack &= antiUpdate;
        b.globalData.bitboard_abominationWhite &= antiUpdate;
        b.globalData.bitboard_abominationBlack &= antiUpdate;
        b.globalData.bitboard_zombieWhite &= antiUpdate;
        b.globalData.bitboard_zombieBlack &= antiUpdate;
        b.globalData.bitboard_clockworksnapperWhite &= antiUpdate;
        b.globalData.bitboard_clockworksnapperBlack &= antiUpdate;
        b.globalData.bitboard_bladebeastWhite &= antiUpdate;
        b.globalData.bitboard_bladebeastBlack &= antiUpdate;
        b.globalData.bitboard_tarotMoonWhite &= antiUpdate;
        b.globalData.bitboard_tarotMoonBlack &= antiUpdate;
        b.globalData.bitboard_tarotMoonIllusionWhite &= antiUpdate;
        b.globalData.bitboard_tarotMoonIllusionBlack &= antiUpdate;
        b.globalData.bitboard_virgoWhite &= antiUpdate;
        b.globalData.bitboard_virgoBlack &= antiUpdate;
        b.globalData.bitboard_warpWeaverWhite &= antiUpdate;
        b.globalData.bitboard_warpWeaverBlack &= antiUpdate;
        b.globalData.bitboard_metalFoxWhite &= antiUpdate;
        b.globalData.bitboard_metalFoxBlack &= antiUpdate;
        b.globalData.bitboard_megacannonWhite &= antiUpdate;
        b.globalData.bitboard_megacannonBlack &= antiUpdate;
        b.globalData.bitboard_momentumWhite &= antiUpdate;
        b.globalData.bitboard_momentumBlack &= antiUpdate;
        b.globalData.bitboard_sludgeTrail &= antiUpdate;
        b.globalData.bitboard_daySwapper &= antiUpdate;
        b.globalData.bitboard_seasonSwapper &= antiUpdate;
        b.globalData.bitboard_egg &= antiUpdate;
        b.globalData.bitboard_noStatus &= antiUpdate;
        b.globalData.bitboard_noallyblock &= antiUpdate;
        b.globalData.bitboard_noenemyblock &= antiUpdate;
        b.globalData.bitboard_rabbit &= antiUpdate;
        b.globalData.bitboard_shielded &= antiUpdate;
        b.globalData.bitboard_secondary &= antiUpdate;
        b.globalData.bitboard_aura &= antiUpdate;

        //How good is parallel for?
        //93 ms without
        //1900 ms with
        //So it's garbage :(

        //does the compiler like b.pieces.Length better than 64?
        //Test is negligible
        ulong update = b.globalData.bitboard_updatedPieces;
        //for (int i = 0; i < 64; i++)
        while (update != 0)
        {
            int i = MainManager.PopBitboardLSB1(update, out update);
            uint piece = b.pieces[i];
            if (piece == 0)
            {
                continue;
            }

            ulong bitIndex = 1uL << i;
            //Piece.PieceAlignment ppa = Piece.GetPieceAlignment(piece);
            uint ppaB = (piece & 0xc0000000);

            //b.globalData.GetPieceAlignmentFromCache(i, piece);

            //note: the cache thing is roughly equal to Piece.GetPieceType

            PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(i, piece); //GlobalPieceManager.GetPieceTableEntry(b.pieces[i]);
            //Piece.PieceType pt = pte.type; //Piece.GetPieceType(b.pieces[i]);
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceType pt = pte.type;

            //? ms
            //Very slightly worse
            //Negligible difference
            //Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

            /*
            if (pte == null)
            {
                Debug.LogError("Type 0 piece detected: " + Piece.ConvertToString(piece));
                continue;
            }
            */

            switch (ppaB)
            {
                case 0: //Piece.PieceAlignment.White:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingWhite |= bitIndex;
                        b.globalData.bitboard_immuneWhite |= bitIndex;
                        b.globalData.bitboard_piecesWhite |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesWhite |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsWhite |= bitIndex;
                    }
                    break;
                case 0x40000000: //Piece.PieceAlignment.Black:
                    if (pt == PieceType.King)
                    {
                        b.globalData.bitboard_kingBlack |= bitIndex;
                        b.globalData.bitboard_immuneBlack |= bitIndex;
                        b.globalData.bitboard_piecesBlack |= bitIndex;
                        b.globalData.bitboard_noStatus |= bitIndex;
                        continue;
                    }
                    b.globalData.bitboard_piecesBlack |= bitIndex;
                    if ((pte.promotionType != Piece.PieceType.Null))
                    {
                        b.globalData.bitboard_pawnsBlack |= bitIndex;
                    }
                    break;
                case 0x80000000: //Piece.PieceAlignment.Neutral:
                    b.globalData.bitboard_piecesNeutral |= bitIndex;
                    break;
                case 0xc0000000: // Piece.PieceAlignment.Crystal:
                    b.globalData.bitboard_piecesCrystal |= bitIndex;
                    break;
            }

            switch (pt)
            {
                case Piece.PieceType.ArcanaMoon:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonWhite |= bitIndex;
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.MoonIllusion:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_tarotMoonIllusionWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_tarotMoonIllusionBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.DivineApothecary:
                case Piece.PieceType.Virgo:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_virgoWhite |= bitIndex;
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            b.globalData.bitboard_immuneRelayerWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_virgoBlack |= bitIndex;
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            b.globalData.bitboard_immuneRelayerBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Abomination:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_abominationWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_abominationBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.Zombie:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_zombieWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_zombieBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.BladeBeast:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_bladebeastWhite |= bitIndex;
                            b.bitboard_autoWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_bladebeastBlack |= bitIndex;
                            break;
                    }
                    continue;
                case Piece.PieceType.ClockworkSnapper:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_clockworksnapperWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_clockworksnapperBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.WarpWeaver:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_warpWeaverWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_warpWeaverBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MetalFox:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_metalFoxWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_metalFoxBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.MegaCannon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_megacannonWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_megacannonBlack |= bitIndex;
                            break;
                    }
                    continue;
                case PieceType.SludgeTrail:
                    b.globalData.bitboard_sludgeTrail |= bitIndex;
                    continue;
                case PieceType.DayPawn:
                case PieceType.DayBishop:
                case PieceType.DayQueen:
                case PieceType.NightPawn:
                case PieceType.NightKnight:
                case PieceType.NightQueen:
                    b.globalData.bitboard_daySwapper |= bitIndex;
                    continue;
                case PieceType.SummerQueen:
                case PieceType.SummerRook:
                case PieceType.SpringKnight:
                case PieceType.SummerPawn:
                case PieceType.SpringPawn:
                case PieceType.WinterQueen:
                case PieceType.WinterBishop:
                case PieceType.FallKnight:
                case PieceType.WinterPawn:
                case PieceType.FallPawn:
                    b.globalData.bitboard_seasonSwapper |= bitIndex;
                    continue;
                case PieceType.FlameEgg:
                case PieceType.WaveEgg:
                case PieceType.RockEgg:
                    switch (ppaB)
                    {
                        case 0:
                            b.globalData.bitboard_immuneWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.globalData.bitboard_immuneBlack |= bitIndex;
                            break;
                    }
                    b.globalData.bitboard_egg |= bitIndex;
                    continue;
                case PieceType.RollerQueen:
                case PieceType.Roller:
                case PieceType.BounceBishop:
                case PieceType.ReboundRook:
                case PieceType.Balloon:
                    switch (ppaB)
                    {
                        case 0:
                            b.bitboard_autoWhite |= bitIndex;
                            b.globalData.bitboard_momentumWhite |= bitIndex;
                            break;
                        case 0x40000000:
                            b.bitboard_autoBlack |= bitIndex;
                            b.globalData.bitboard_momentumBlack |= bitIndex;
                            break;
                    }
                    break;
                case PieceType.RelayKnight:
                case PieceType.RelayBishop:
                case PieceType.RelayRook:
                case PieceType.RelayQueen:
                case PieceType.Hypnotist:
                case PieceType.Kindness:
                case PieceType.Envy:
                case PieceType.ArcanaEmpress:
                case PieceType.ArcanaWorld:
                case PieceType.ArcanaSun:
                case PieceType.Charity:
                case PieceType.DivineBeacon:
                case PieceType.DivineMusician:
                    b.globalData.bitboard_secondary |= bitIndex;
                    break;
                case PieceType.Attractor:
                case PieceType.Repulser:
                case PieceType.Immobilizer:
                case PieceType.Entrancer:
                case PieceType.Charmer:
                case PieceType.Sloth:
                case PieceType.ArcanaHierophant:
                case PieceType.ArcanaHanged:
                case PieceType.AceOfPentacles:
                case PieceType.AceOfCups:
                case PieceType.PageOfPentacles:
                case PieceType.PageOfCups:
                case PieceType.QueenOfPentacles:
                case PieceType.QueenOfCups:
                case PieceType.Earth:
                case PieceType.Saturn:
                case PieceType.Ganymede:
                case PieceType.Taurus:
                case PieceType.Aquarius:
                case PieceType.EarthElemental:
                case PieceType.WaterElemental:
                case PieceType.EarthWisp:
                case PieceType.WaterWisp:
                case PieceType.Banshee:
                case PieceType.Harpy:
                case PieceType.Hag:
                case PieceType.Diplomat:
                case PieceType.Enforcer:
                case PieceType.Watchtower:
                case PieceType.Fan:
                    b.globalData.bitboard_aura |= bitIndex;
                    break;
                case PieceType.RabbitDiplomat:
                    b.globalData.bitboard_aura |= bitIndex;
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
                case PieceType.RabbitQueen:
                case PieceType.RabbitCourier:
                case PieceType.RabbitKnight:
                    b.globalData.bitboard_rabbit |= bitIndex;
                    break;
                case PieceType.Rabbit:
                    b.globalData.bitboard_rabbit |= bitIndex;
                    continue;
            }


            switch (Piece.GetPieceModifier(piece))
            {
                case PieceModifier.HalfShielded:
                    break;
                case PieceModifier.Immune:
                    b.globalData.bitboard_aura |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Shielded:
                    b.globalData.bitboard_shielded |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                case PieceModifier.Spectral:
                    b.globalData.bitboard_noallyblock |= bitIndex;
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
                default:
                    if ((piece & 0x3C000000) == 0)
                    {
                        b.globalData.bitboard_noStatus |= bitIndex;
                    }
                    break;
            }

            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingAlly) != 0 || ((i & 56) == 16 && ppaB == 0 && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
            {
                b.globalData.bitboard_noallyblock |= bitIndex;
            }
            //ghostly
            if ((pte.piecePropertyB & PiecePropertyB.NonBlockingEnemy) != 0 || (piece & 0x3C00000) == 0x1800000)
            {
                b.globalData.bitboard_noenemyblock |= bitIndex;
            }

            /*
                (pmO == Piece.PieceModifier.Spectral || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0 || (tY == 2 && pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
                if (Piece.GetPieceStatusEffect(obstaclePiece) == Piece.PieceStatusEffect.Ghostly || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingEnemy) != 0)
             */
        }

        if ((b.globalData.playerModifier & Board.PlayerModifier.FullArmyWhiteBadges) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite;
        }
        else
        {
            if ((b.globalData.playerModifier & Board.PlayerModifier.Recall) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & ~MoveGenerator.BITBOARD_PATTERN_RANK1;
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Seafaring) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (MoveGenerator.BITBOARD_PATTERN_SIDEEDGES);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Backdoor) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesWhite & (0x6600000000000066);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.FlyingGeneral) != 0)
            {
                b.globalData.bitboard_secondary |= b.globalData.bitboard_kingWhite;
            }
        }

        if ((b.globalData.enemyModifier & Board.EnemyModifier.Defensive) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_piecesBlack;
        } else if ((b.globalData.enemyModifier & Board.EnemyModifier.KingMoveModifiers) != 0)
        {
            b.globalData.bitboard_secondary |= b.globalData.bitboard_kingBlack;
        }

        b.globalData.bitboard_rabbitAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_rabbit);

        b.globalData.bitboard_EOTPieces = b.globalData.bitboard_daySwapper | b.globalData.bitboard_seasonSwapper | b.globalData.bitboard_egg | (b.globalData.bitboard_rabbit & MoveGenerator.BITBOARD_PATTERN_RANK18);


        //X -> X white and X black
        //slower actually???
        //???
        /*
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        //doing it for the others is slow?
        b.globalData.bitboard_pawnsWhite = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_pawnsBlack = b.globalData.bitboard_pawns & b.globalData.bitboard_piecesBlack;
        b.globalData.bitboard_kingWhite = b.globalData.bitboard_king & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_kingBlack = b.globalData.bitboard_king & b.globalData.bitboard_piecesBlack;

        b.globalData.bitboard_immuneWhite = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneBlack = b.globalData.bitboard_immune & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerWhite = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_immuneRelayerBlack = b.globalData.bitboard_immuneRelayer & b.globalData.bitboard_piecesWhite;

        b.globalData.bitboard_abominationWhite = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_abominationBlack = b.globalData.bitboard_abomination & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieWhite = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_zombieBlack = b.globalData.bitboard_zombie & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperWhite = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_clockworksnapperBlack = b.globalData.bitboard_clockworksnapper & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastWhite = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_bladebeastBlack = b.globalData.bitboard_bladebeast & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonWhite = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonBlack = b.globalData.bitboard_tarotMoon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionWhite = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_tarotMoonIllusionBlack = b.globalData.bitboard_tarotMoonIllusion & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoWhite = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_virgoBlack = b.globalData.bitboard_virgo & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverWhite = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_warpWeaverBlack = b.globalData.bitboard_warpWeaver & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxWhite = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_metalFoxBlack = b.globalData.bitboard_metalFox & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonWhite = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_megacannonBlack = b.globalData.bitboard_megacannon & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumWhite = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        b.globalData.bitboard_momentumBlack = b.globalData.bitboard_momentum & b.globalData.bitboard_piecesWhite;
        */

        //b.globalData.bitboard_piecesWhiteAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesWhite);
        //b.globalData.bitboard_piecesBlackAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_piecesBlack);

        //(b.globalData.bitboard_piecesWhiteAdjacent1, b.globalData.bitboard_piecesWhiteAdjacent2, b.globalData.bitboard_piecesWhiteAdjacent4, b.globalData.bitboard_piecesWhiteAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesWhite);
        //(b.globalData.bitboard_piecesBlackAdjacent1, b.globalData.bitboard_piecesBlackAdjacent2, b.globalData.bitboard_piecesBlackAdjacent4, b.globalData.bitboard_piecesBlackAdjacent8) = MainManager.CountAdjacencyCardinality(b.globalData.bitboard_piecesBlack);

        b.globalData.bitboard_pieces = b.globalData.bitboard_piecesWhite | b.globalData.bitboard_piecesBlack | b.globalData.bitboard_piecesNeutral | b.globalData.bitboard_piecesCrystal;
        //b.globalData.bitboard_piecesMirrored = MainManager.MirrorBitboard(b.globalData.bitboard_pieces);

        b.globalData.arcanaMoonOutdated = false;
    }

    //Secondary moves (relay moves, neutral pieces, crystal pieces, Kindness, Fool, etc)
    //Neutrals end up here to specifically avoid messing with the other stuff
    //This version must use a MBT to see what the attacks are
    private static void GenerateSecondaryMoves(List<uint> moves, ref Board b, Piece.PieceAlignment pa, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata)
    {
        MoveBitTable antiTable = b.globalData.mbtactiveInverse;
        if (b.globalData.mbtactiveInverse == null)
        {
            b.globalData.mbtactiveInverse = new MoveBitTable();
            antiTable = b.globalData.mbtactiveInverse;
        }
        bool antiInit = false;
        //antiTable.MakeInverse(b.globalData.mbtactive);

        b.globalData.bitboard_crystalWhite = 0;
        b.globalData.bitboard_crystalBlack = 0;

        int blackKingIndex = MainManager.PopBitboardLSB1(b.globalData.bitboard_kingBlack, out _);
        ulong blackKingAdjacent = MainManager.SmearBitboard(b.globalData.bitboard_kingBlack);

        ulong pieceBitboard = ((pa == Piece.PieceAlignment.White ? b.globalData.bitboard_piecesWhite : b.globalData.bitboard_piecesBlack) & b.globalData.bitboard_secondary) | b.globalData.bitboard_piecesNeutral | b.globalData.bitboard_piecesCrystal;

        while (pieceBitboard != 0)
        {
            int i = MainManager.PopBitboardLSB1(pieceBitboard, out pieceBitboard);

            ulong bitIndex = 1uL << i;
            //(int subX, int subY) = Board.CoordinateConvertInverse(i);
            int subX = i & 7;
            int subY = i >> 3;

            uint piece = b.pieces[i];
            if (piece == 0)
            {
                continue;
            }

            Piece.PieceAlignment ppa = Piece.GetPieceAlignment(piece);

            //Friendly piece special movement

            switch (ppa)
            {
                case PieceAlignment.White:
                case PieceAlignment.Black:
                    //Piece.PieceType pt = Piece.GetPieceType(piece);

                    PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(i, piece); //GlobalPieceManager.GetPieceTableEntry(pt);
                    Piece.PieceType pt = pte.type;

                    if ((pte.pieceProperty & Piece.PieceProperty.Relay) != 0 || (pa == Piece.PieceAlignment.White && pt == Piece.PieceType.King && (b.globalData.playerModifier & Board.PlayerModifier.RelayKing) != 0))
                    {
                        ulong relayBitboard = b.globalData.mbtactive.Get(subX, subY);
                        if (pa == Piece.PieceAlignment.White)
                        {
                            relayBitboard &= b.globalData.bitboard_piecesWhite;
                        }
                        else if (pa == Piece.PieceAlignment.Black)
                        {
                            relayBitboard &= b.globalData.bitboard_piecesBlack;
                        }

                        while (relayBitboard != 0)
                        {
                            int index = MainManager.PopBitboardLSB1(relayBitboard, out relayBitboard);

                            PieceTableEntry pteT = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[index]));

                            if (pteT.type != Piece.PieceType.King && (pteT.promotionType == Piece.PieceType.Null && (pteT.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) == 0))
                            {
                                //generate stuff
                                //no MBT as that might lead to incorrect relaying
                                GenerateMovesForPiece(moves, ref b, pa, piece, index & 7, (index & 56) >> 3, null, moveMetadata);
                            }
                        }
                    }

                    if ((pte.pieceProperty & Piece.PieceProperty.RelayBishop) != 0)
                    {
                        ulong relayBitboard = b.globalData.mbtactive.Get(subX, subY);
                        if (pa == Piece.PieceAlignment.White)
                        {
                            relayBitboard &= b.globalData.bitboard_piecesWhite;
                        }
                        else if (pa == Piece.PieceAlignment.Black)
                        {
                            relayBitboard &= b.globalData.bitboard_piecesBlack;
                        }

                        while (relayBitboard != 0)
                        {
                            int index = MainManager.PopBitboardLSB1(relayBitboard, out relayBitboard);

                            //forbid relays except in bishop lines
                            int ix = index & 7;
                            int iy = index << 3;
                            if (subX - ix != subY - iy && subX - ix != iy - subY)
                            {
                                continue;
                            }

                            PieceTableEntry pteT = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[index]));

                            if (pteT.type != Piece.PieceType.King && (pteT.promotionType == Piece.PieceType.Null && (pteT.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) == 0))
                            {
                                //generate stuff
                                //no MBT as that might lead to incorrect relaying
                                GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceType(Piece.PieceType.Bishop, piece), index & 7, (index & 56) >> 3, null, moveMetadata);
                            }
                        }
                    }

                    switch (pte.type)
                    {
                        case Piece.PieceType.Hypnotist:
                            //Hypnotizer special movement
                            //moves as an enemy piece
                            //  Doesn't let you move as your own piece so you can't wreck the enemy too easily at range
                            //  Hypnotized pawns are pretty dangerous
                            //as a hacky fix I will move them as if they had Shielded which prevents own king capture
                            ulong hypnoBitboard = b.globalData.mbtactive.Get(subX, subY);
                            //int pastIndex = -1;
                            if (pa == Piece.PieceAlignment.White)
                            {
                                hypnoBitboard &= b.globalData.bitboard_piecesBlack & ~b.globalData.bitboard_immuneBlack;
                                //pastIndex = b.whitePerPlayerInfo.lastPieceMovedLocation;
                            }
                            else if (pa == Piece.PieceAlignment.Black)
                            {
                                hypnoBitboard &= b.globalData.bitboard_piecesWhite & ~b.globalData.bitboard_immuneWhite;
                                //pastIndex = b.blackPerPlayerInfo.lastPieceMovedLocation;
                            }

                            while (hypnoBitboard != 0)
                            {
                                int index = MainManager.PopBitboardLSB1(hypnoBitboard, out hypnoBitboard);

                                //Forbid you from hypnotizing the last moved piece?
                                //This should stop you from undoing your opponents last move
                                //Ehh this isn't a big problem
                                //New system now forbids this because it turns out this is a pretty big problem (Most pieces have symmetric moves so they can always undo anything you try to do)

                                if (Piece.GetPieceType(b.pieces[index]) != Piece.PieceType.King)
                                {
                                    PieceTableEntry pteH = GlobalPieceManager.GetPieceTableEntry(b.pieces[index]);

                                    //Sidenote: Arcana Moon is made Enchant Immune to fix arcana moon + hypnotizer bugs?
                                    if ((pteH.pieceProperty & Piece.PieceProperty.EnchantImmune) != 0 || Piece.GetPieceModifier(b.pieces[index]) == PieceModifier.Immune)
                                    {
                                        continue;
                                    }

                                    if ((pteH.piecePropertyB & Piece.PiecePropertyB.Giant) == 0)
                                    {
                                        //generate stuff at the target's location
                                        GenerateMovesForPiece(moves, ref b, Piece.GetPieceAlignment(b.pieces[index]), Piece.SetPieceModifier(Piece.PieceModifier.Shielded, b.pieces[index]), index & 7, index >> 3, null, moveMetadata);
                                    }
                                    else
                                    {
                                        int newIndex = index;
                                        //(int dx, int dy)
                                        (int dx, int dy) = Board.GetGiantDelta(b.pieces[index]);

                                        newIndex += dx;
                                        newIndex += dy * 8;

                                        //generate stuff at the target's location
                                        GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceModifier(Piece.PieceModifier.Shielded, b.pieces[index]), newIndex & 7, newIndex >> 3, null, moveMetadata);
                                    }
                                }
                            }
                            break;
                        case Piece.PieceType.Kindness:
                            //Use the anti bitboard
                            if (!antiInit)
                            {
                                antiInit = true;
                                antiTable.MakeInverse(b.globalData.mbtactive);
                            }
                            ulong kindnessBitboard = antiTable.Get(subX, subY);
                            if (pa == Piece.PieceAlignment.White)
                            {
                                kindnessBitboard &= b.globalData.bitboard_piecesWhite;
                            }
                            else if (pa == Piece.PieceAlignment.Black)
                            {
                                kindnessBitboard &= b.globalData.bitboard_piecesBlack;
                            }
                            while (kindnessBitboard != 0)
                            {
                                int index = MainManager.PopBitboardLSB1(kindnessBitboard, out kindnessBitboard);

                                //Relay moves to kindness
                                GenerateMovesForPiece(moves, ref b, pa, b.pieces[index], subX, subY, null, moveMetadata);
                            }

                            for (int ei = 0; ei < pte.enhancedMoveInfo.Length; ei++)
                            {
                                GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, Piece.GetPieceStatusEffect(piece), Piece.GetPieceModifier(piece), piece, subX, subY, pte.enhancedMoveInfo[ei], null, moveMetadata);
                            }
                            break;
                        case Piece.PieceType.Envy:
                            //Similar setup to Hypnotist but the extra moves get applied to the Envy
                            //No king exception so the Envy can copy King moves too
                            ulong envyBitboard = b.globalData.mbtactive.Get(subX, subY);
                            if (pa == Piece.PieceAlignment.White)
                            {
                                envyBitboard &= b.globalData.bitboard_piecesBlack;
                            }
                            else if (pa == Piece.PieceAlignment.Black)
                            {
                                envyBitboard &= b.globalData.bitboard_piecesWhite;
                            }

                            while (envyBitboard != 0)
                            {
                                int index = MainManager.PopBitboardLSB1(envyBitboard, out envyBitboard);

                                GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceModifier(PieceModifier.NoSpecial, Piece.SetPieceAlignment(pa, b.pieces[index])), subX, subY, null, moveMetadata);
                            }

                            //envy bonus moves
                            for (int ei = 0; ei < pte.enhancedMoveInfo.Length; ei++)
                            {
                                GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, Piece.GetPieceStatusEffect(piece), Piece.GetPieceModifier(piece), piece, i & 7, (i & 56) >> 3, pte.enhancedMoveInfo[ei], null, moveMetadata);
                            }
                            break;
                        case Piece.PieceType.ArcanaEmpress:
                            ulong smearBitboard = (MainManager.SmearBitboard(MainManager.SmearBitboard(bitIndex)));
                            if (pa == Piece.PieceAlignment.White)
                            {
                                smearBitboard &= b.globalData.bitboard_piecesWhite;
                            }
                            else if (pa == Piece.PieceAlignment.Black)
                            {
                                smearBitboard &= b.globalData.bitboard_piecesBlack;
                            }

                            //relays the F move only
                            //no MBT as that might lead to incorrect relaying
                            while (smearBitboard != 0)
                            {
                                int index = MainManager.PopBitboardLSB1(smearBitboard, out smearBitboard);

                                Piece.PieceType ptR = Piece.GetPieceType(b.pieces[index]);
                                if (ptR != Piece.PieceType.King)
                                {
                                    //generate stuff
                                    //no MBT as that might lead to incorrect relaying
                                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, b.globalData.GetPieceTableEntryFromCache(index, b.pieces[index]), Piece.GetPieceStatusEffect(b.pieces[index]), Piece.GetPieceModifier(b.pieces[index]), piece, index & 7, (index & 56) >> 3, pte.moveInfo[0], null, moveMetadata);
                                }
                            }
                            break;
                    }

                    if (pa == PieceAlignment.White)
                    {
                        Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);
                        Piece.PieceModifier pm = Piece.GetPieceModifier(piece);
                        //Debug.Log(i + " " + (i >> 3) + " " + pa + " " + Piece.GetPieceType(b.pieces[i]));
                        if ((b.globalData.playerModifier & Board.PlayerModifier.Defensive) != 0)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.defensiveModifierMove, null, moveMetadata);
                        }

                        //Debug.Log(((b.globalData.playerModifier & Board.PlayerModifier.Recall) != 0) + " " + (subY != 0));
                        //bugged: somehow it is appearing on rank 0
                        if ((b.globalData.playerModifier & Board.PlayerModifier.Recall) != 0 && (subY != 0))
                        {
                            //Debug.Log("Recall " + i + " " + (i >> 3) + " " + pa + " " + Piece.GetPieceType(b.pieces[i]));
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.recallModifierMove, null, moveMetadata);
                        }

                        if ((b.globalData.playerModifier & Board.PlayerModifier.Seafaring) != 0 && ((subX) == 0 || (subX) == 7))
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.seafaringModifierMove, null, moveMetadata);
                        }

                        //Debug.Log(((b.globalData.playerModifier & Board.PlayerModifier.Backdoor) != 0) + " " + ((i & 3) == 1 || (i & 3) == 2) + " " + (((subY) == 0 || (subY) == 7)));
                        //No king backdoor because that seems very cheesy?
                        if (pt != PieceType.King && ((b.globalData.playerModifier & Board.PlayerModifier.Backdoor) != 0 && ((i & 3) == 1 || (i & 3) == 2 && ((subY) == 0 || (subY) == 7))))
                        {
                            //Debug.Log("Backdoor " + i + " " + (i >> 3) + " " + pa + " " + Piece.GetPieceType(b.pieces[i]));
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.backdoorModifierMove, null, moveMetadata);
                        }

                        if ((b.globalData.playerModifier & Board.PlayerModifier.Mirror) != 0)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.mirrorModifierMove, null, moveMetadata);
                        }

                        if ((b.globalData.playerModifier & Board.PlayerModifier.Forest) != 0)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.forestModifierMove, null, moveMetadata);
                        }

                        if ((b.globalData.playerModifier & Board.PlayerModifier.FlyingGeneral) != 0 && pte.type == PieceType.King)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.flyingGeneralModifierMove, null, moveMetadata);
                        }
                    }
                    else if (pa == Piece.PieceAlignment.Black)
                    {
                        Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);
                        Piece.PieceModifier pm = Piece.GetPieceModifier(piece);
                        if ((b.globalData.enemyModifier & Board.EnemyModifier.Defensive) != 0)
                        {
                            GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pte, pse, pm, piece, subX, subY, GlobalPieceManager.Instance.defensiveModifierMove, null, moveMetadata);
                        }

                        if ((b.globalData.enemyModifier & Board.EnemyModifier.Fusion) != 0 && pte.type == PieceType.King)
                        {
                            //Need to change how this works because the secondary move now no longer touches things without secondary moves
                            ulong fusionBitboard = blackKingAdjacent & b.globalData.bitboard_piecesBlack & ~b.globalData.bitboard_kingBlack;

                            while (fusionBitboard != 0)
                            {
                                int index = MainManager.PopBitboardLSB1(fusionBitboard, out fusionBitboard);
                                //PieceTableEntry pteT = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[index]));
                                //generate stuff
                                //no MBT as that might lead to incorrect relaying
                                GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceModifier(PieceModifier.NoSpecial, b.pieces[index]), blackKingIndex & 7, (blackKingIndex >> 3), null, moveMetadata);
                            }
                        }

                        if (pt == Piece.PieceType.King)
                        {
                            if ((b.globalData.enemyModifier & Board.EnemyModifier.Envious) != 0)
                            {
                                //Debug.Log("Envy target = " + b.globalData.whiteHighestValuePiece + " " + b.globalData.whiteHighestValuedPieceValue);
                                PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(b.globalData.whitePerPlayerInfo.highestPieceType);
                                for (int r = 0; r < pteR.moveInfo.Length; r++)
                                {
                                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pteR, pse, PieceModifier.NoSpecial, Piece.SetPieceModifier(Piece.PieceModifier.NoSpecial, piece), subX, subY, pteR.moveInfo[r], null, moveMetadata);
                                }
                            }

                            if ((b.globalData.enemyModifier & Board.EnemyModifier.Lustful) != 0)
                            {
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
                                //MainManager.PrintBitboard(lustfulBitboard);

                                if (pa == Piece.PieceAlignment.Black)
                                {
                                    lustfulBitboard &= b.globalData.bitboard_piecesWhite;
                                    //pastIndex = b.blackPerPlayerInfo.lastPieceMovedLocation;
                                }

                                while (lustfulBitboard != 0)
                                {
                                    int index = MainManager.PopBitboardLSB1(lustfulBitboard, out lustfulBitboard);

                                    //Forbid you from hypnotizing the last moved piece?
                                    //This should stop you from undoing your opponents last move
                                    //Ehh this isn't a big problem

                                    if (Piece.GetPieceType(b.pieces[index]) != Piece.PieceType.King)
                                    {
                                        PieceTableEntry pteH = GlobalPieceManager.GetPieceTableEntry(b.pieces[index]);

                                        //No enchant immunity?
                                        if ((pteH.piecePropertyB & Piece.PiecePropertyB.Giant) == 0)
                                        {
                                            //generate stuff at the target's location
                                            GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceModifier(Piece.PieceModifier.Shielded, b.pieces[index]), index & 7, index >> 3, null, moveMetadata);
                                        }
                                        else
                                        {
                                            int newIndex = index;
                                            //(int dx, int dy)
                                            (int dx, int dy) = Board.GetGiantDelta(b.pieces[index]);

                                            newIndex += dx;
                                            newIndex += dy * 8;

                                            //generate stuff at the target's location
                                            GenerateMovesForPiece(moves, ref b, pa, Piece.SetPieceModifier(Piece.PieceModifier.Shielded, b.pieces[index]), newIndex & 7, newIndex >> 3, null, moveMetadata);
                                        }
                                    }
                                }
                            }

                            if ((b.globalData.enemyModifier & Board.EnemyModifier.Xyloid) != 0)
                            {
                                PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(Piece.PieceType.Rootwalker);
                                for (int r = 0; r < pteR.moveInfo.Length; r++)
                                {
                                    GenerateMovesForMoveGeneratorEntry(moves, ref b, pa, pteR, pse, pm, piece, subX, subY, pteR.moveInfo[r], null, moveMetadata);
                                }
                            }
                        }
                    }
                    break;
                case PieceAlignment.Neutral:
                    piece = Piece.SetPieceAlignment(pa, piece);
                    GenerateMovesForPiece(moves, ref b, pa, piece, subX, subY, null, moveMetadata);
                    break;
                case PieceAlignment.Crystal:
                    if (!antiInit)
                    {
                        antiInit = true;
                        antiTable.MakeInverse(b.globalData.mbtactive);
                    }

                    //Check the inverse bitboard
                    ulong subBitboard = antiTable.Get(subX, subY);

                    bool canMove = false;
                    while (subBitboard != 0)
                    {
                        int index = MainManager.PopBitboardLSB1(subBitboard, out subBitboard);
                        if (index == -1)
                        {
                            break;
                        }

                        Piece.PieceAlignment cpA = Piece.GetPieceAlignment(b.pieces[index]);

                        if (cpA == Piece.PieceAlignment.White)
                        {
                            b.globalData.bitboard_crystalWhite |= (1uL << i);
                        }
                        if (cpA == Piece.PieceAlignment.Black)
                        {
                            b.globalData.bitboard_crystalBlack |= (1uL << i);
                        }

                        //who is this
                        if (Piece.GetPieceAlignment(b.pieces[index]) == pa)
                        {
                            canMove = true;
                            break;
                        }
                    }

                    if (canMove)
                    {
                        piece = Piece.SetPieceAlignment(pa, piece);
                        GenerateMovesForPiece(moves, ref b, pa, piece, subX, subY, null, moveMetadata);
                    }
                    break;
            }
        }
    }

    private static void GenerateCastling(List<uint> moves, ref Board b, Piece.PieceAlignment pa, Piece.PieceType pt, uint piece, int x, int y, bool goLeft, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        int deltaX = goLeft ? -1 : 1;
        //int deltaY = 0;

        int tempX = x;
        int tempY = y;

        //Check 1 away from king
        //Must be empty
        tempX += deltaX;

        //Fail
        if (tempX < 0 || tempX > 7)
        {
            return;
            //return moveStartIndex;
        }

        //if (b.GetPieceAtCoordinate(tempX, tempY) != 0)
        if (b.pieces[tempX + (tempY << 3)] != 0)
        {
            return;
            //return moveStartIndex;
        }

        //Check 2 away from king
        //Must be empty
        tempX += deltaX;

        //Fail
        if (tempX < 0 || tempX > 7)
        {
            return;
            //return moveStartIndex;
        }

        if (b.GetPieceAtCoordinate(tempX, tempY) != 0)
        {
            return;
            //return moveStartIndex;
        }

        //Now check 3 away
        //Must be an ally non pawn
        tempX += deltaX;
        if (tempX < 0 || tempX > 7)
        {
            return;
            //return moveStartIndex;
        }

        uint targetPiece = b.GetPieceAtCoordinate(tempX, tempY);
        if (Piece.GetPieceAlignment(targetPiece) != pa)
        {
            return;
            //return moveStartIndex;
        }

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(targetPiece));

        ulong pawnBitboard = 0;
        if (pa == Piece.PieceAlignment.White)
        {
            pawnBitboard = b.globalData.bitboard_pawnsWhite;
        }
        else
        {
            pawnBitboard = b.globalData.bitboard_pawnsBlack;
        }

        //Giants are too big to castle with
        if (targetPiece == 0 || ((pawnBitboard & (1uL << tempX + tempY * 8)) != 0) || ((pte.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0))
        {
            return;
            //return moveStartIndex;
        }

        //Success: generate move to 2 away
        tempX -= deltaX;

        //moves[moveStartIndex] = Move.PackMove((byte)x, (byte)y, (byte)tempX, (byte)tempY, DeltaToDir(deltaX, 0), SpecialType.Castling);
        //moveStartIndex++;
        if (moves != null && (mbt == null || !mbt.Get(x, y, tempX, tempY)))
        {
            moves.Add(Move.PackMove((byte)x, (byte)y, (byte)tempX, (byte)tempY, DeltaToDir(deltaX, 0), SpecialType.Castling));

            if (moveMetadata != null)
            {
                uint key = Move.PackMove(x, y, tempX, tempY);
                if (moveMetadata.ContainsKey(key))
                {
                    moveMetadata[key].pathTags.Add(pathTag);
                }
                else
                {
                    moveMetadata.Add(key, new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, SpecialType.Castling, pathTag));
                }
            }
        }

        return;
        //return moveStartIndex;
    }


    //Giant rays
    private static void GenerateGiantOffsetRayMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, uint piece, int x, int y, int deltaX, int deltaY, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        int gx = 0;
        int gy = 0;
        int deltaXA = 0;
        int deltaYA = 0;
        int deltaXB = 0;
        int deltaYB = 0;

        Move.Dir dir = Move.DeltaToDir(deltaX, deltaY);

        if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
        {
            dir = Dir.Null;
        }

        switch (dir)
        {
            case Dir.DownLeft:
                deltaXA = 0;
                deltaYA = 1;
                deltaXB = 1;
                deltaYB = 0;
                break;
            case Dir.Down:
                deltaXA = 1;
                deltaYA = 0;
                break;
            case Dir.DownRight:
                gx = 1;
                gy = 0;
                deltaXA = 0;
                deltaYA = 1;
                deltaXB = -1;
                deltaYB = 0;
                break;
            case Dir.Left:
                deltaXA = 0;
                deltaYA = 1;
                break;
            case Dir.Right:
                gx = 1;
                gy = 0;
                deltaXA = 0;
                deltaYA = 1;
                break;
            case Dir.UpLeft:
                gx = 0;
                gy = 1;
                deltaXA = 0;
                deltaYA = -1;
                deltaXB = 1;
                deltaYB = 0;
                break;
            case Dir.Up:
                gx = 0;
                gy = 1;
                deltaXA = 1;
                deltaYA = 0;
                break;
            case Dir.UpRight:
                gx = 1;
                gy = 1;
                deltaXA = -1;
                deltaYA = 0;
                deltaXB = 0;
                deltaYB = -1;
                break;
            case Dir.Null:
                deltaXA = 1;
                deltaYA = 0;
                deltaXB = 0;
                deltaYB = 1;
                //So it checks all 4 squares
                //I can't do this same thing for the other directions as that would be double checking squares and also the other parts of the giant would break stuff
                //For leaps longer than distance 1 it is guaranteed that the giant doesn't overlap with its previous steps so this set of values is required every time
                break;
        }

        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, x + gx, y + gy, 0, dir, deltaX, deltaY, deltaXA, deltaYA, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, pathTag);
    }

    private static void GenerateGiantOffsetRayMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, uint piece, int x, int y, int startX, int startY, int lostRange, Move.Dir dir, int deltaX, int deltaY, int deltaXA, int deltaYA, int deltaXB, int deltaYB, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        bool deltaBOff = false;
        bool checkBack = true;
        if (deltaXB == 0 && deltaYB == 0)
        {
            deltaBOff = true;
            checkBack = false;
        }
        if ((deltaXA + deltaXB) == -deltaX && (deltaYA + deltaYB) == -deltaY)
        {
            checkBack = false;
        }

        //Need to block smashing your own pieces if they can be passed through
        //Smashing enemies is not a problem
        ulong allyPieces = 0;
        switch (pa)
        {
            case PieceAlignment.White:
                allyPieces = b.globalData.bitboard_piecesWhite;
                break;
            case PieceAlignment.Black:
                allyPieces = b.globalData.bitboard_piecesBlack;
                break;
        }


        //Move.Dir dir = Dir.Null;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        //dir = Move.DeltaToDir(deltaX, deltaY);

        //bool wasGenerated = true;
        bool keepGoing = true;

        int tempX = startX;
        int tempY = startY;
        int lastTempX;
        int lastTempY;

        int gx = (startX - x);
        int gy = (startY - y);
        //int gx = 0;
        //int gy = 0;

        int maxRange = mgie.range;
        if (maxRange == 0)
        {
            maxRange = 16;
        }

        int currentRange = lostRange;
        while (true)
        {
            lastTempX = tempX;
            lastTempY = tempY;
            tempX += deltaX;
            tempY += deltaY;
            currentRange++;

            //Going too far?
            if (currentRange > maxRange)
            {
                break;
            }

            //Out of bounds?
            if (tempX < 0 || tempX > 7)
            {
                keepGoing = false;
                break;
            }
            if (tempY < 0 || tempY > 7)
            {
                keepGoing = false;
                break;
            }

            //check the offsets too
            if (tempX < 0 || tempX > 7)
            {
                keepGoing = false;
                break;
            }
            if (tempY + deltaXA < 0 || tempY + deltaYA > 7)
            {
                keepGoing = false;
                break;
            }
            if (!deltaBOff && (tempY + deltaXB < 0 || tempY + deltaYB > 7))
            {
                keepGoing = false;
                break;
            }
            if (checkBack && (tempX + deltaXA + deltaXB < 0 || tempX + deltaXA + deltaXB > 7 || tempY + deltaYA + deltaYB < 0 || tempY + deltaYA + deltaYB > 7))
            {
                keepGoing = false;
                break;
            }

            bool checkO = false;
            bool checkA = false;
            bool checkB = deltaBOff;
            bool checkAB = !checkBack;

            bool moCheckO = false;
            bool moCheckA = false;
            bool moCheckB = checkB;
            bool moCheckAB = checkAB;

            bool coCheckO = false;
            bool coCheckA = false;
            bool coCheckB = false;
            bool coCheckAB = false;

            moCheckO = b.pieces[tempX + (tempY) * 8] == 0;
            moCheckA = b.pieces[tempX + deltaXA + (tempY + deltaYA) * 8] == 0;
            moCheckB = !checkB || b.pieces[tempX + deltaXB + (tempY + deltaYB) * 8] == 0;
            moCheckAB = !checkAB || b.pieces[tempX + deltaXA + deltaXB + (tempY + deltaYA + deltaYB) * 8] == 0;

            coCheckO = !moCheckO && Piece.GetPieceAlignment(b.pieces[tempX + (tempY) * 8]) != pa;
            coCheckA = !moCheckA && Piece.GetPieceAlignment(b.pieces[tempX + deltaXA + (tempY + deltaYA) * 8]) != pa;
            coCheckB = !checkB && (b.pieces[tempX + deltaXB + (tempY + deltaYB) * 8] != 0 && Piece.GetPieceAlignment(b.pieces[tempX + deltaXB + (tempY + deltaYB) * 8]) != pa);
            coCheckAB = !checkAB && (b.pieces[tempX + deltaXA + deltaXB + (tempY + deltaYA + deltaYB) * 8] != 0 && Piece.GetPieceAlignment(b.pieces[tempX + deltaXA + deltaXB + (tempY + deltaYA + deltaYB) * 8]) != pa);

            if (specialType == SpecialType.MoveOnly && !(moCheckO && moCheckA && moCheckB && moCheckAB))
            {
                break;
            }
            if (specialType == SpecialType.CaptureOnly && !(coCheckO || coCheckA || coCheckB || coCheckAB))
            {
                //Debug.Log(tempX + " " + tempY + " / " + (tempX + deltaXA) + " " + (tempY + deltaYA) + " / " + !checkB + " " + (tempX + deltaXB) + " " + (tempY + deltaYB) + " / " + !checkAB + " " + (tempX + deltaXA + deltaXB) + " " + (tempY + deltaYA + deltaYB));                    

                break;
            }

            bool wg = false;
            //landing on an enemy sets KeepGoing to false but WasGenerated to true
            //so I need the OR of the two

            bool kg = true;

            bool kgt = true;
            (kgt, wg) = GiantCheckGenerateSquareSingle(ref b, allowBitboard, piece, x, y, tempX, tempY, dir, pa, specialType, pte, mbt);
            checkO |= kgt;
            kg &= kgt;
            checkO |= wg;
            (kgt, wg) = GiantCheckGenerateSquareSingle(ref b, allowBitboard, piece, x, y, tempX + deltaXA, tempY + deltaYA, dir, pa, specialType, pte, mbt);
            checkA |= kgt;
            kg &= kgt;
            checkA |= wg;
            if (!checkB)
            {
                (kgt, wg) = GiantCheckGenerateSquareSingle(ref b, allowBitboard, piece, x, y, tempX + deltaXB, tempY + deltaYB, dir, pa, specialType, pte, mbt);
                checkB |= kgt;
                kg &= kgt;
                checkB |= wg;
            }
            if (!checkAB)
            {
                (kgt, wg) = GiantCheckGenerateSquareSingle(ref b, allowBitboard, piece, x, y, tempX + deltaXA + deltaXB, tempY + deltaYA + deltaYB, dir, pa, specialType, pte, mbt);
                checkAB |= kgt;
                kg &= kgt;
                checkAB |= wg;
            }

            keepGoing = kg;
            if (checkO && checkA && checkB && checkAB)
            {

                //Is an obstacle in the way?
                //(keepGoing, wasGenerated) = GenerateSquareSingle(moves, canMove, ref b, piece, x, y, tempX - gx, tempY - gy, dir, pa, specialType, pte, mgie, mbt);

                //Note: mbt check would cause this check to fail a lot erroneously
                //So I have to take special care not to mess up the giants move
                //(Because the Giant protects those squares even if it can't move up onto them)
                if (moves != null && specialType != SpecialType.PassiveAbility)// && (mbt == null || !mbt.Get(x, y, tempX - gx, tempY - gy)))
                {
                    //Double check
                    if (((BITBOARD_2SQUARE << (tempX - gx + ((tempY - gy) << 3))) & allyPieces & ~(BITBOARD_2SQUARE << (x + (y << 3)))) == 0)
                    {
                        moves.Add(Move.PackMove((byte)x, (byte)y, (byte)(tempX - gx), (byte)(tempY - gy), dir, specialType));
                    }

                    if (moveMetadata != null)
                    {
                        bool isRider = false;
                        if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
                        {
                            isRider = true;
                        }
                        MoveMetadata md;
                        uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(tempX - gx), (byte)(tempY - gy));
                        if (!moveMetadata.ContainsKey(mdKey))
                        {
                            if (isRider)
                            {
                                md = new MoveMetadata(piece, tempX - gx, tempY - gy, MoveMetadata.PathType.LeaperGiant, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                                if (lastTempX - gx != x || lastTempY - gy != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX - gx), (byte)(lastTempY - gy))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                            else
                            {
                                md = new MoveMetadata(piece, tempX - gx, tempY - gy, MoveMetadata.PathType.SliderGiant, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                                if (lastTempX - gx != x || lastTempY - gy != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX - gx), (byte)(lastTempY - gy))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                        }
                        else
                        {
                            md = moveMetadata[mdKey];
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathTag));
                            if (lastTempX - gx != x || lastTempY - gy != y)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX - gx), (byte)(lastTempY - gy))]);
                            }
                        }
                    }
                }
            }

            if (!keepGoing)
            {
                break;
            }
        }

        return;
    }


    //Populate the span with moves for the piece at x and y
    //Note: I can't do bitboards?
    private static void GenerateMovesForMoveGeneratorEntry(List<uint> moves, ref Board b, Piece.PieceAlignment pa, PieceTableEntry pte, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata)
    {
        int tempX = x;
        int tempY = y;

        int deltaX = 0;
        int deltaY = 0;

        int xy = (x + (y << 3));

        //move up to highest level to reduce calls to these
        /*
        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        Piece.PieceType pt = Piece.GetPieceType(piece);
        Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);    //may change by other stuff to do water tiles
        Piece.PieceModifier pm = Piece.GetPieceModifier(piece);
        */

        //Move.Dir dir;

        bool flip = false;

        ulong flipCheckBitboard = 0;

        bool symmetric = (mgie.modifier & MoveGeneratorPreModifier.Flippable) != 0;

        if (!symmetric)
        {
            if (pa == Piece.PieceAlignment.White)
            {
                flipCheckBitboard |= b.globalData.bitboard_hangedBlack;
            }
            else if (pa == Piece.PieceAlignment.Black)
            {
                flipCheckBitboard |= b.globalData.bitboard_hangedWhite;
            }
            else
            {
                flipCheckBitboard |= b.globalData.bitboard_hangedWhite;
                flipCheckBitboard |= b.globalData.bitboard_hangedBlack;
            }
        }


        //PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(xy, piece); //GlobalPieceManager.GetPieceTableEntry(pt);
        Piece.PieceType pt = pte.type;

        bool directionRestricted = (mgie.modifier & MoveGeneratorPreModifier.DirectionModifiers) != 0;

        Move.SpecialType specialType = Move.SpecialType.Normal;

        if (pm == Piece.PieceModifier.NoSpecial)
        {
            if (mgie.atom >= MoveGeneratorAtom.SpecialMoveDivider)
            {
                return;
            }
        }

        if ((mgie.modifier & MoveGeneratorPreModifier.m) != 0)
        {
            specialType = Move.SpecialType.MoveOnly;
        }
        if ((mgie.modifier & MoveGeneratorPreModifier.c) != 0)
        {
            specialType = Move.SpecialType.CaptureOnly;
        }

        if (pm != Piece.PieceModifier.NoSpecial)
        {
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
                            if (Piece.GetPieceSpecialData(piece) != 0)
                            {
                                return;
                            }
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
                        if (Piece.GetPieceSpecialData(piece) == 0)
                        {
                            return;
                        }

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

                    if ((b.globalData.enemyModifier & Board.EnemyModifier.Xyloid) != 0 && pte.type == Piece.PieceType.King)
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
                        if (Piece.GetPieceSpecialData(piece) == 0)
                        {
                            return;
                        }
                        else
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
                    } else if (specialType == SpecialType.CaptureOnly)
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
        }
        else
        {
            //r, e -> c
            if ((mgie.modifier & MoveGeneratorPreModifier.r) != 0)
            {
                specialType = Move.SpecialType.CaptureOnly;
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.e) != 0)
            {
                specialType = Move.SpecialType.CaptureOnly;
            }

            if ((mgie.modifier & MoveGeneratorPreModifier.n) != 0)
            {
                specialType = Move.SpecialType.MoveOnly;
            }

            //passive or ally only ability = No
            if ((mgie.modifier & MoveGeneratorPreModifier.a) != 0)
            {
                return;
            }
            if ((mgie.modifier & MoveGeneratorPreModifier.p) != 0)
            {
                return;
            }
            //no bonus moves
            if ((mgie.modifier & MoveGeneratorPreModifier.o) != 0)
            {
                return;
            }
        }

        if ((mgie.modifier & MoveGeneratorPreModifier.i) != 0)
        {
            //Initial: if not legal just return immediately
            //I made initial refer to anything on the first 2 rows because otherwise it is ambiguous
            switch (pa)
            {
                case Piece.PieceAlignment.White:
                    if (y > 1)
                    {
                        return;
                        //return moveStartIndex;
                    }
                    break;
                case Piece.PieceAlignment.Black:
                    if (y < 6)
                    {
                        return;
                        //return moveStartIndex;
                    }
                    break;
                case Piece.PieceAlignment.Neutral:  //Note: moves as if it was the same color as the one moving it
                case Piece.PieceAlignment.Crystal:  //Note: moves as if it was the same color as the one moving it
                    return;
                    //return moveStartIndex;
            }
        }

        if (pa == Piece.PieceAlignment.White)
        {
            //turn 1 no capture (To stop certain kinds of team building instant cheeses)
            if (b.turn == 0 && b.bonusPly == 0)
            {
                if (pse == Piece.PieceStatusEffect.Bloodlust)
                {
                    return;
                }
                pse = PieceStatusEffect.Soaked;
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Soaked, piece);
            }

            //Complacent boss: can't capture 2 turns in a row
            if ((b.globalData.enemyModifier & Board.EnemyModifier.Complacent) != 0 && b.whitePerPlayerInfo.capturedLastTurn)
            {
                if (pse == Piece.PieceStatusEffect.Bloodlust)
                {
                    return;
                }
                pse = PieceStatusEffect.Soaked;
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Soaked, piece);
            }

            //Prideful boss: Can't capture if White has more pieces
            if ((b.globalData.enemyModifier & Board.EnemyModifier.Prideful) != 0 && b.whitePerPlayerInfo.pieceCount > b.blackPerPlayerInfo.pieceCount)
            {
                if (pse == Piece.PieceStatusEffect.Bloodlust)
                {
                    return;
                }
                pse = PieceStatusEffect.Soaked;
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Soaked, piece);
            }

            if ((b.globalData.playerModifier & Board.PlayerModifier.Tempering) != 0)
            {
                switch (specialType)
                {
                    case SpecialType.CaptureOnly:
                        specialType = SpecialType.Normal;
                        break;
                    case SpecialType.ConsumeAlliesCaptureOnly:
                        specialType = SpecialType.ConsumeAllies;
                        break;
                    case SpecialType.ConvertCaptureOnly:
                        specialType = SpecialType.Convert;
                        break;
                    case SpecialType.FireCaptureOnly:
                        specialType = SpecialType.FireCapture;
                        break;
                    case SpecialType.LongLeaperCaptureOnly:
                        specialType = SpecialType.LongLeaper;
                        break;
                    case SpecialType.InflictFreezeCaptureOnly:
                        specialType = SpecialType.InflictFreeze;
                        break;
                    case SpecialType.InflictCaptureOnly:
                        specialType = SpecialType.Inflict;
                        break;
                    case SpecialType.InflictShiftCaptureOnly:
                        specialType = SpecialType.InflictShift;
                        break;
                }
            }
        }

        if (pa == Piece.PieceAlignment.Black)
        {
            /*
            //Greedy boss: first 3 captures are Convert
            if ((b.globalData.enemyModifier & Board.EnemyModifier.Greedy) != 0 && (b.globalData.whitePerPlayerInfo.startPieceCount - b.whitePerPlayerInfo.pieceCount) < 3)
            {
                //convert is a stationary thing so fire capture gets lumped in also
                if (specialType == SpecialType.Normal || specialType == SpecialType.FireCapture)
                {
                    specialType = SpecialType.Convert;
                }
                if (specialType == SpecialType.CaptureOnly || specialType == SpecialType.FireCaptureOnly)
                {
                    specialType = SpecialType.ConvertCaptureOnly;
                }
            }
            */
        }

        if ((pte.piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
        {
            ulong bitIndex = 1uL << xy;
            bitIndex |= 1uL << xy + 1;
            bitIndex |= 1uL << xy + 8;
            bitIndex |= 1uL << xy + 9;
            if (!symmetric)
            {
                flip = (flipCheckBitboard & (bitIndex)) != 0;
            }
            //(int aX, int aY) = Move.TransformBasedOnAlignment(pa, 1, 1, flip);

            int flipValue = flip ? -1 : 1;
            int aX = 1;
            int aY = 1;
            switch (pa)
            {
                case PieceAlignment.White:
                    aX = flipValue;
                    aY = flipValue;
                    break;
                case PieceAlignment.Black:
                    aX = -flipValue;
                    aY = -flipValue;
                    break;
            }

            //Trying to capture when capturing is restricted
            //note that non standard captures are not blocked because it would be kind of hard to stop them
            //This includes advancers, withdrawers, coordinators, leapers
            //Water effect
            //Water squares
            //Voided status effect
            ulong waterBitboard = b.globalData.bitboard_square_water;
            switch (pa)
            {
                case PieceAlignment.White:
                    waterBitboard = b.globalData.bitboard_waterBlack;
                    break;
                case PieceAlignment.Black:
                    waterBitboard = b.globalData.bitboard_waterWhite;
                    break;
            }

            if (pse == Piece.PieceStatusEffect.Soaked || (((bitIndex & waterBitboard) != 0) && ((pte.pieceProperty & (Piece.PieceProperty.WaterImmune | Piece.PieceProperty.NoTerrain)) == 0)))
            {
                if (pse == Piece.PieceStatusEffect.Bloodlust)
                {
                    //Bloodlust status effect might be on the piece already
                    //Bloodlust + Voided = can't move at all
                    return;
                }

                //lazy kind of check
                //later code forces this to only apply to things that target enemies
                pse = PieceStatusEffect.Soaked;
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Soaked, piece);
            }

            //Trying to not capture when non-captures are restricted
            //note that non standard captures are not blocked because it would be kind of hard to stop them
            //This includes advancers, withdrawers, coordinators, leapers
            //Harpy effect
            //Capture only zone
            ulong harpyBitboard = 0;
            switch (pa)
            {
                case PieceAlignment.White:
                    harpyBitboard = b.globalData.bitboard_harpyBlack;
                    break;
                case PieceAlignment.Black:
                    harpyBitboard = b.globalData.bitboard_harpyWhite;
                    break;
            }
            if (pse == Piece.PieceStatusEffect.Bloodlust || ((pte.pieceProperty & Piece.PieceProperty.NoTerrain) == 0 && b.globalData.squares[xy].type == Square.SquareType.CaptureOnly) || (bitIndex & harpyBitboard) != 0)
            {
                if (pse == Piece.PieceStatusEffect.Soaked)
                {
                    //Bloodlust + Voided = can't move at all
                    return;
                }

                //lazy kind of check
                //later code forces this to only apply to things that target enemies
                pse = PieceStatusEffect.Bloodlust;
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Bloodlust, piece);
            }

            ulong allowBitboard = GetAllowedSquares(ref b, piece, x, y, pa, Piece.PieceModifier.None, specialType, pte);
            allowBitboard |= GetAllowedSquares(ref b, piece, x + 1, y, pa, Piece.PieceModifier.None, specialType, pte);
            allowBitboard |= GetAllowedSquares(ref b, piece, x, y + 1, pa, Piece.PieceModifier.None, specialType, pte);
            allowBitboard |= GetAllowedSquares(ref b, piece, x + 1, y + 1, pa, Piece.PieceModifier.None, specialType, pte);

            switch (mgie.atom)
            {
                case MoveGeneratorAtom.R:
                case MoveGeneratorAtom.W:
                    //ray up
                    //Up is allowed if F is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        (deltaX, deltaY) = (0, aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray down
                    //Down is allowed if B is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        (deltaX, deltaY) = (0, -aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray left
                    //Left is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (-aX, 0);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray right
                    //Right is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (aX, 0);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                    }
                    break;
                case MoveGeneratorAtom.B:
                case MoveGeneratorAtom.F:
                    //ray up right
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (aX, aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray up left
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (-aX, aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray down right
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (aX, -aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray down left
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (-aX, -aY);
                        GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                    }
                    break;
                case MoveGeneratorAtom.Leaper:
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
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                        {
                            (deltaX, deltaY) = (0, ch * aY); //Move.TransformBasedOnAlignment(pa, cl, ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //down
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                        {
                            (deltaX, deltaY) = (0, -ch * aY); //Move.TransformBasedOnAlignment(pa, cl, -ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (ch * aX, 0); //Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (-ch * aX, 0); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }
                    }
                    else if (deltaX == deltaY || deltaX == -deltaY)
                    {
                        //4 diagonal leaper
                        //ray up right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                        {
                            (deltaX, deltaY) = (ch * aX, cl * aY); //Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //ray up left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                        {
                            (deltaX, deltaY) = (-ch * aX, cl * aY); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //ray down right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                        {
                            (deltaX, deltaY) = (ch * aX, -cl * aY); //Move.TransformBasedOnAlignment(pa, ch, -cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //ray down left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                        {
                            (deltaX, deltaY) = (-ch * aX, -cl * aY); //Move.TransformBasedOnAlignment(pa, -ch, -cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }
                    }
                    else
                    {
                        //8 leaper

                        //Logic is slightly different for directions
                        //f = front 4
                        //v = vertical 4 
                        //fv = front vertical 2

                        //Inverted conditions

                        //Vertical 4
                        //ray up right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                        {
                            (deltaX, deltaY) = (cl * aX, ch * aY); //Move.TransformBasedOnAlignment(pa, cl, ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //ray up left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                        {
                            (deltaX, deltaY) = (-cl * aX, ch * aY); //Move.TransformBasedOnAlignment(pa, -cl, ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //ray down right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                        {
                            (deltaX, deltaY) = (cl * aX, -ch * aY); //Move.TransformBasedOnAlignment(pa, cl, -ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //ray down left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                        {
                            (deltaX, deltaY) = (-cl * aX, -ch * aY); //Move.TransformBasedOnAlignment(pa, -cl, -ch, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }

                        //Horizontal 4
                        //ray up right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                        {
                            (deltaX, deltaY) = (ch * aX, cl * aY);//Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 4);
                        }
                        //ray up left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                        {
                            (deltaX, deltaY) = (-ch * aX, cl * aY); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 5);
                        }
                        //ray down right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                        {
                            (deltaX, deltaY) = (ch * aX, -cl * aY); //Move.TransformBasedOnAlignment(pa, ch, -cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 6);
                        }
                        //ray down left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                        {
                            (deltaX, deltaY) = (-ch * aX, -cl * aY); //Move.TransformBasedOnAlignment(pa, -ch, -cl, flip);
                            GenerateGiantOffsetRayMoves(moves, ref b, allowBitboard, pa, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 7);
                        }
                    }
                    break;
            }
        }
        else
        {
            if (!symmetric)
            {
                flip = (flipCheckBitboard & (1uL << xy)) != 0;
            }
            //(int aX, int aY) = Move.TransformBasedOnAlignment(pa, 1, 1, flip);
            int flipValue = flip ? -1 : 1;
            int aX = 1;
            int aY = 1;
            switch (pa)
            {
                case PieceAlignment.White:
                    aX = flipValue;
                    aY = flipValue;
                    break;
                case PieceAlignment.Black:
                    aX = -flipValue;
                    aY = -flipValue;
                    break;
            }

            //Trying to capture when capturing is restricted
            //note that non standard captures are not blocked because it would be kind of hard to stop them
            //This includes advancers, withdrawers, coordinators, leapers
            //Water effect
            //Water squares
            //Voided status effect
            ulong waterBitboard = b.globalData.bitboard_square_water;
            switch (pa)
            {
                case PieceAlignment.White:
                    waterBitboard = b.globalData.bitboard_waterBlack;
                    break;
                case PieceAlignment.Black:
                    waterBitboard = b.globalData.bitboard_waterWhite;
                    break;
            }
            if (pse == Piece.PieceStatusEffect.Soaked || ((((1uL << xy) & waterBitboard) != 0) && ((pte.pieceProperty & (Piece.PieceProperty.WaterImmune | Piece.PieceProperty.NoTerrain)) == 0)))
            {
                if (pse == Piece.PieceStatusEffect.Bloodlust)
                {
                    //Bloodlust status effect might be on the piece already
                    //Bloodlust + Voided = can't move at all
                    return;
                }

                //lazy kind of check
                //later code forces this to only apply to things that target enemies
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Soaked, piece);
                pse = PieceStatusEffect.Soaked;
            }

            //Trying to not capture when non-captures are restricted
            //note that non standard captures are not blocked because it would be kind of hard to stop them
            //This includes advancers, withdrawers, coordinators, leapers
            //Harpy effect
            //Capture only zone
            ulong harpyBitboard = 0;
            switch (pa)
            {
                case PieceAlignment.White:
                    harpyBitboard = b.globalData.bitboard_harpyBlack;
                    break;
                case PieceAlignment.Black:
                    harpyBitboard = b.globalData.bitboard_harpyWhite;
                    break;
            }
            if (pse == Piece.PieceStatusEffect.Bloodlust || ((pte.pieceProperty & Piece.PieceProperty.NoTerrain) == 0 && b.globalData.squares[xy].type == Square.SquareType.CaptureOnly) || (1uL << xy & harpyBitboard) != 0)
            {
                if (pse == Piece.PieceStatusEffect.Soaked)
                {
                    //Bloodlust + Voided = can't move at all
                    return;
                }

                //lazy kind of check
                //later code forces this to only apply to things that target enemies
                piece = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Bloodlust, piece);
                pse = PieceStatusEffect.Bloodlust;
            }

            ulong allowBitboard = GetAllowedSquares(ref b, piece, x, y, pa, Piece.GetPieceModifier(piece), specialType, pte);

            switch (mgie.atom)
            {
                case MoveGeneratorAtom.R:
                case MoveGeneratorAtom.W:
                    //ray up
                    //Up is allowed if F is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray down
                    //Down is allowed if B is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray left
                    //Left is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray right
                    //Right is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                    }
                    break;
                case MoveGeneratorAtom.D:
                    //ray up
                    //Up is allowed if F is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray down
                    //Down is allowed if B is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray left
                    //Left is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray right
                    //Right is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                    }
                    break;
                case MoveGeneratorAtom.B:
                case MoveGeneratorAtom.F:
                    //ray up right
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray up left
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray down right
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray down left
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                    }
                    break;
                case MoveGeneratorAtom.A:
                    //ray up right
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                    }
                    //ray up left
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                    }
                    //ray down right
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                    }
                    //ray down left
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x + deltaX, y + deltaY, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
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
                    int deltaXB;
                    int deltaYB;
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.fv) != 0)
                    {
                        (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        (deltaXB, deltaYB) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 0, 1);
                    }
                    //ray down
                    //Down is allowed if B is set or V is set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.bv) != 0)
                    {
                        (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        (deltaXB, deltaYB) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 2, 3);
                    }
                    //ray left
                    //Left is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        (deltaXB, deltaYB) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 4, 5);
                    }
                    //ray right
                    //Right is allowed if H is set
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                    {
                        (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        (deltaXB, deltaYB) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 6, 7);
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
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        (deltaXB, deltaYB) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 0, 1);
                    }
                    //ray up left
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        (deltaXB, deltaYB) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 2, 3);
                    }
                    //ray down right
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        (deltaXB, deltaYB) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 4, 5);
                    }
                    //ray down left
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        (deltaXB, deltaYB) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 6, 7);
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
                        (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        (deltaXB, deltaYB) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 0, 1);
                    }
                    //ray up left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                    {
                        (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        (deltaXB, deltaYB) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 2, 3);
                    }
                    //ray down right
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                    {
                        (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        (deltaXB, deltaYB) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 4, 5);
                    }
                    //ray down left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                    {
                        (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        (deltaXB, deltaYB) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 6, 7);
                    }

                    //Horizontal 4
                    //ray up right
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        (deltaXB, deltaYB) = (aX, aY);  //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 8, 9);
                    }
                    //ray up left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bv) == 0))
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        (deltaXB, deltaYB) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 10, 11);
                    }
                    //ray down right
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        (deltaXB, deltaYB) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 12, 13);
                    }
                    //ray down left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fv) == 0))
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        (deltaXB, deltaYB) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        GenerateRayMovesDual(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, deltaXB, deltaYB, specialType, pte, mgie, mbt, moveMetadata, 14, 15);
                    }
                    break;
                case MoveGeneratorAtom.G:   //gryphon
                                            //ray up right
                    int targetX;
                    int targetY;
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                    {
                        (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 0), MoveMetadata.MakePathTag(mgie.atom, 1) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 0));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 1));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up
                                (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //right
                                (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                            }
                        }
                    }
                    //ray up left
                    //Allowed if F set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                    {
                        (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 2), MoveMetadata.MakePathTag(mgie.atom, 3) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 2));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 3));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up
                                (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //left
                                (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                            }
                        }
                    }
                    //ray down right
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                    {
                        (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 4), MoveMetadata.MakePathTag(mgie.atom, 5) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 4));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 5));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up
                                (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 4);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //right
                                (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 5);
                            }
                        }
                    }
                    //ray down left
                    //Allowed if B set
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                    {
                        (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 6), MoveMetadata.MakePathTag(mgie.atom, 7) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 6));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 7));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //down
                                (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 6);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //left
                                (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 7);
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.M:   //manticore
                                            //Uses the offset ray logic
                                            //ray up
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.bh) == 0))
                    {
                        (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 0), MoveMetadata.MakePathTag(mgie.atom, 1) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 0));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 1));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up right
                                (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up left
                                (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                            }
                        }
                    }
                    //ray down
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.fh) == 0))
                    {
                        (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 2), MoveMetadata.MakePathTag(mgie.atom, 3) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 2));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 3));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //down right
                                (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //down left
                                (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                            }
                        }
                    }
                    //ray left
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 4), MoveMetadata.MakePathTag(mgie.atom, 5) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 4));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 5));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                            {
                                //left up
                                (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 4);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                            {
                                //left down
                                (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 5);
                            }
                        }
                    }
                    //ray right
                    //Note that currently there is no left right asymmetry allowed in movesets
                    if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                    {
                        (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Slider, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 6), MoveMetadata.MakePathTag(mgie.atom, 7) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 6));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 7));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) == 0)
                            {
                                //right up
                                (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 6);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) == 0)
                            {
                                //right down
                                (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 7);
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.E:   //(0,2) then rook not backwards
                    //up
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                    {
                        //left shift is weird with negatives?
                        (deltaX, deltaY) = (0, aY << 1);//Move.TransformBasedOnAlignment(pa, 0, 2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 0), MoveMetadata.MakePathTag(mgie.atom, 1), MoveMetadata.MakePathTag(mgie.atom, 2) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 0));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 1));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 2));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //up then left
                                (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //up then right
                                (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //up then up
                                (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                            }
                        }
                    }
                    //down
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                    {
                        (deltaX, deltaY) = (0, (-aY) << 1);//Move.TransformBasedOnAlignment(pa, 0, -2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 3), MoveMetadata.MakePathTag(mgie.atom, 4), MoveMetadata.MakePathTag(mgie.atom, 5) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 3));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 4));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 5));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //down then left
                                (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //down then right
                                (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 4);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //down then down
                                (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 5);
                            }
                        }
                    }
                    //left
                    //actually no condition
                    //This looks very cursed
                    {
                        (deltaX, deltaY) = ((-aX) << 1, 0); //Move.TransformBasedOnAlignment(pa, -2, 0, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 6), MoveMetadata.MakePathTag(mgie.atom, 7), MoveMetadata.MakePathTag(mgie.atom, 8) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 6));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 7));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 8));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //left then up
                                (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 6);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //left then down
                                (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 7);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //left then left
                                (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 8);
                            }
                        }
                    }
                    //right
                    {
                        (deltaX, deltaY) = ((aX) << 1, 0); //Move.TransformBasedOnAlignment(pa, 2, 0, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 9), MoveMetadata.MakePathTag(mgie.atom, 10), MoveMetadata.MakePathTag(mgie.atom, 11) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 9));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 10));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 11));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //right then up
                                (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 9);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //right then down
                                (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 10);
                            }

                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.v) == 0)
                            {
                                //right then right
                                (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 11);
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.J:   //(2,2) then bishop not backwards
                    //V allows the weird ricochet things in the vertical directions (<> shape paths
                    //H allows the weird ricochet things in the horizontal directions (V^ shape paths)

                    //up left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                    {
                        (deltaX, deltaY) = ((-aX) << 1, aY << 1); //Move.TransformBasedOnAlignment(pa, -2, 2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 0), MoveMetadata.MakePathTag(mgie.atom, 1), MoveMetadata.MakePathTag(mgie.atom, 2) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 0));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 1));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 2));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //UL then UR
                                (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 0);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.f)) == 0)
                            {
                                //UL then DL
                                (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 1);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.h)) == 0)
                            {
                                //UL then UL
                                (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 2);
                            }
                        }
                    }
                    //up right
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0))
                    {
                        (deltaX, deltaY) = (aX << 1, aY << 1); //Move.TransformBasedOnAlignment(pa, 2, 2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 3), MoveMetadata.MakePathTag(mgie.atom, 4), MoveMetadata.MakePathTag(mgie.atom, 5) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 3));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 4));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 5));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //UR then UL
                                (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 3);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.f)) == 0)
                            {
                                //UR then DR
                                (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 4);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.h)) == 0)
                            {
                                //UR then UR
                                (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 5);
                            }
                        }
                    }
                    //down left
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                    {
                        (deltaX, deltaY) = ((-aX) << 1, (-aY) << 1); //Move.TransformBasedOnAlignment(pa, -2, -2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 6), MoveMetadata.MakePathTag(mgie.atom, 7), MoveMetadata.MakePathTag(mgie.atom, 8) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 6));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 7));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 8));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //DL then DR
                                (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 6);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.b)) == 0)
                            {
                                //DL then UL
                                (deltaX, deltaY) = (-aX, aY); //Move.TransformBasedOnAlignment(pa, -1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 7);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.h)) == 0)
                            {
                                //DL then DL
                                (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 8);
                            }
                        }
                    }
                    //down right
                    if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0))
                    {
                        (deltaX, deltaY) = (aX << 1, (-aY) << 1); //Move.TransformBasedOnAlignment(pa, 2, -2, flip);
                        targetX = x + deltaX;
                        targetY = y + deltaY;

                        //3 rays
                        (bool canContinue, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, pa, specialType, pte, mgie, mbt);
                        if (moveMetadata != null && (canContinue || wasGenerated))
                        {
                            //It is possible for wasGenerated to be false but I still need to report the middling step
                            //(I.e. if you have a Winged piece you can fly over an obstacle but there is no move onto the obstacle)
                            //May cause problems for me later with overlapping movement ranges? (Need to order the moves properly in the table)
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(targetX), (byte)(targetY));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, targetX, targetY, MoveMetadata.PathType.Leaper, specialType, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 9), MoveMetadata.MakePathTag(mgie.atom, 10), MoveMetadata.MakePathTag(mgie.atom, 11) }));
                            }
                            else
                            {
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 9));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 10));
                                moveMetadata[key].pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, 11));
                            }
                        }

                        if (canContinue)
                        {
                            if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) == 0)
                            {
                                //DR then DL
                                (deltaX, deltaY) = (-aX, -aY); //Move.TransformBasedOnAlignment(pa, -1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 9);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.b)) == 0)
                            {
                                //DR then UR
                                (deltaX, deltaY) = (aX, aY); //Move.TransformBasedOnAlignment(pa, 1, 1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 10);
                            }

                            if (!directionRestricted || (mgie.modifier & (MoveGeneratorPreModifier.v | MoveGeneratorPreModifier.h)) == 0)
                            {
                                //DR then DR
                                (deltaX, deltaY) = (aX, -aY); //Move.TransformBasedOnAlignment(pa, 1, -1, flip);
                                GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, targetX, targetY, 1, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, 11);
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.H:   //wheel
                                            //Uses the between points ray logic
                    int cxA = x;
                    int cyA = x;
                    if (x > 3)
                    {
                        cxA = 7 - x;
                        cyA = 7 - x;
                    }
                    if (y < cxA)
                    {
                        cxA = y;
                        cyA = y;
                    }
                    if (7 - y < cxA)
                    {
                        cxA = 7 - y;
                        cyA = 7 - y;
                    }

                    //cxA and cxB = 0,0 or 1,1 or 2,2 or 3,3
                    //to make this easier I will also flip the corner around if the x or y is on the other side of that corner

                    if (x == 7 - cxA || y == 7 - cyA)
                    {
                        cxA = 7 - cxA;
                        cyA = 7 - cyA;
                    }

                    //The other corners
                    int cxB = cxA;
                    int cyB = 7 - cyA;
                    int cxC = 7 - cxA;
                    int cyC = cyB;
                    int cxD = cxC;
                    int cyD = cyA;

                    //Debug.Log(x + " " + y + ", " + cxA + " " + cyA + ", " + cxB + " " + cyB + ", " + cxC + " " + cyC + ", " + cxD + " " + cyD);

                    bool keepGoing = true;

                    //A  (pos)   D
                    //B         C

                    //Or
                    //A     D
                    //(pos)
                    //B     C

                    uint wtagA = MoveMetadata.MakePathTag(mgie.atom, 0);
                    uint wtagB = MoveMetadata.MakePathTag(mgie.atom, 1);

                    if (y == cxA)
                    {
                        //X direction first
                        //Move between x and A
                        keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, cxA, cyA, specialType, pte, mbt, moveMetadata, wtagA);
                        //Debug.Log(x + " " + y + " to " + cxA + " " + cyA);

                        if (keepGoing)
                        {
                            //A to B
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxA, cyA, cxB, cyB, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxA + " " + cyA + " to " + cxB + " " + cyB);
                        }
                        //B to C
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxB, cyB, cxC, cyC, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxB + " " + cyB + " to " + cxC + " " + cyC);
                        }
                        //C to D
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxC, cyC, cxD, cyD, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxC + " " + cyC + " to " + cxD + " " + cyD);
                        }
                        //D to x
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxD, cyD, x, y, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxD + " " + cyD + " to " + x + " " + y);
                        }

                        //Reverse
                        //x to D
                        keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, cxD, cyD, specialType, pte, mbt, moveMetadata, wtagB);
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxD, cyD, cxC, cyC, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxC, cyC, cxB, cyB, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxB, cyB, cxA, cyA, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxA, cyA, x, y, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                    }
                    else
                    {
                        //Y direction first
                        //Move between x and A
                        keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, cxA, cyA, specialType, pte, mbt, moveMetadata, wtagA);
                        //Debug.Log(x + " " + y + " to " + cxA + " " + cyA);
                        if (keepGoing)
                        {
                            //A to D
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxA, cyA, cxD, cyD, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxA + " " + cyA + " to " + cxD + " " + cyD);
                        }
                        //D to C
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxD, cyD, cxC, cyC, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxD + " " + cyD + " to " + cxC + " " + cyC);
                        }
                        //C to B
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxC, cyC, cxB, cyB, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxC + " " + cyC + " to " + cxB + " " + cyB);
                        }
                        //B to x
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxB, cyB, x, y, specialType, pte, mbt, moveMetadata, wtagA);
                            //Debug.Log(cxB + " " + cyB + " to " + x + " " + y);
                        }

                        //Reverse
                        keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, cxB, cyB, specialType, pte, mbt, moveMetadata, wtagB);
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxB, cyB, cxC, cyC, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxC, cyC, cxD, cyD, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxD, cyD, cxA, cyA, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                        if (keepGoing)
                        {
                            keepGoing = GenerateOffsetMovesBetweenPoints(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, cxA, cyA, x, y, specialType, pte, mbt, moveMetadata, wtagB);
                        }
                    }
                    break;
                case MoveGeneratorAtom.O:   //orbiter
                                            //Need to detect orbit targets nearby
                    ulong pieceBitboard = 1uL << (xy);
                    pieceBitboard = MainManager.SmearBitboard(pieceBitboard) & ~pieceBitboard;
                    pieceBitboard &= b.globalData.bitboard_pieces;

                    while (pieceBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(pieceBitboard, out pieceBitboard);

                        //Uses the offset ray logic
                        //Orbit around pieceIndex
                        int px = pieceIndex & 7;
                        int py = (pieceIndex & 56) >> 3;

                        //But there are 8 ways to orbit, I need to see which one it is

                        int index = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            if (px + GlobalPieceManager.Instance.orbiterDeltas[i][0] == x && py + GlobalPieceManager.Instance.orbiterDeltas[i][1] == y)
                            {
                                index = i;
                                break;
                            }
                        }

                        int pastX = x;
                        int pastY = y;
                        //Orbit
                        for (int i = 1; i < 8; i++)
                        {
                            int orbitX = px + GlobalPieceManager.Instance.orbiterDeltas[(i + index) % 8][0];
                            int orbitY = py + GlobalPieceManager.Instance.orbiterDeltas[(i + index) % 8][1];
                            (bool keepGoingO, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, orbitX, orbitY, GlobalPieceManager.Instance.orbiterDirections[(i + index) % 8][0], pa, specialType, pte, mgie, mbt);
                            if (moveMetadata != null && (keepGoingO || wasGenerated))
                            {
                                uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(orbitX), (byte)(orbitY));
                                uint otherKey = Move.PackMove((byte)x, (byte)y, (byte)(pastX), (byte)(pastY));
                                if (!moveMetadata.ContainsKey(mdKey))
                                {
                                    MoveMetadata md = new MoveMetadata(piece, orbitX, orbitY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 0));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                    moveMetadata.Add(mdKey, md);
                                }
                                else
                                {
                                    MoveMetadata md = moveMetadata[mdKey];
                                    md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 0));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                }
                            }
                            pastX = orbitX;
                            pastY = orbitY;
                            if (!keepGoingO)
                            {
                                break;
                            }
                        }
                        pastX = x;
                        pastY = y;
                        //Orbit opposite
                        for (int i = 7; i > 0; i--)
                        {
                            int orbitX = px + GlobalPieceManager.Instance.orbiterDeltas[(i + index) % 8][0];
                            int orbitY = py + GlobalPieceManager.Instance.orbiterDeltas[(i + index) % 8][1];
                            (bool keepGoingO, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, orbitX, orbitY, GlobalPieceManager.Instance.orbiterDirections[(i + index) % 8][1], pa, specialType, pte, mgie, mbt);
                            if (moveMetadata != null && (wasGenerated || keepGoingO))
                            {
                                uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(orbitX), (byte)(orbitY));
                                uint otherKey = Move.PackMove((byte)x, (byte)y, (byte)(pastX), (byte)(pastY));
                                if (!moveMetadata.ContainsKey(mdKey))
                                {
                                    MoveMetadata md = new MoveMetadata(piece, orbitX, orbitY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 1));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                    moveMetadata.Add(mdKey, md);
                                }
                                else
                                {
                                    MoveMetadata md = moveMetadata[mdKey];
                                    md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 1));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                }
                            }
                            pastX = orbitX;
                            pastY = orbitY;
                            if (!keepGoingO)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.P:   //orbiter
                                            //Need to detect orbit targets nearby
                    ulong O2pieceBitboard = 0;
                    O2pieceBitboard = MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ORBIT2, (x + (y << 3)), -2, -2);
                    //MainManager.PrintBitboard(O2pieceBitboard);
                    O2pieceBitboard &= b.globalData.bitboard_pieces;

                    while (O2pieceBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(O2pieceBitboard, out O2pieceBitboard);

                        //Uses the offset ray logic
                        //Orbit around pieceIndex
                        int px = pieceIndex & 7;
                        int py = (pieceIndex & 56) >> 3;

                        //But there are 8 ways to orbit, I need to see which one it is

                        int index = 0;
                        for (int i = 0; i < 12; i++)
                        {
                            if (px + GlobalPieceManager.Instance.orbiterDeltas2[i][0] == x && py + GlobalPieceManager.Instance.orbiterDeltas2[i][1] == y)
                            {
                                index = i;
                                break;
                            }
                        }

                        int pastX = x;
                        int pastY = y;
                        //Orbit
                        for (int i = 1; i < 12; i++)
                        {
                            int orbitX = px + GlobalPieceManager.Instance.orbiterDeltas2[(i + index) % 12][0];
                            int orbitY = py + GlobalPieceManager.Instance.orbiterDeltas2[(i + index) % 12][1];
                            (bool keepGoingO, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, orbitX, orbitY, GlobalPieceManager.Instance.orbiterDirections2[(i + index) % 12][0], pa, specialType, pte, mgie, mbt);
                            if (moveMetadata != null && (keepGoingO || wasGenerated))
                            {
                                uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(orbitX), (byte)(orbitY));
                                uint otherKey = Move.PackMove((byte)x, (byte)y, (byte)(pastX), (byte)(pastY));
                                if (!moveMetadata.ContainsKey(mdKey))
                                {
                                    MoveMetadata md = new MoveMetadata(piece, orbitX, orbitY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 0));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                    moveMetadata.Add(mdKey, md);
                                }
                                else
                                {
                                    MoveMetadata md = moveMetadata[mdKey];
                                    md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 0));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                }
                            }
                            pastX = orbitX;
                            pastY = orbitY;
                            if (!keepGoingO)
                            {
                                break;
                            }
                        }
                        pastX = x;
                        pastY = y;
                        //Orbit opposite
                        for (int i = 11; i > 0; i--)
                        {
                            int orbitX = px + GlobalPieceManager.Instance.orbiterDeltas2[(i + index) % 12][0];
                            int orbitY = py + GlobalPieceManager.Instance.orbiterDeltas2[(i + index) % 12][1];
                            (bool keepGoingO, bool wasGenerated) = TryGenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, orbitX, orbitY, GlobalPieceManager.Instance.orbiterDirections2[(i + index) % 12][1], pa, specialType, pte, mgie, mbt);
                            if (moveMetadata != null && (wasGenerated || keepGoingO))
                            {
                                uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(orbitX), (byte)(orbitY));
                                uint otherKey = Move.PackMove((byte)x, (byte)y, (byte)(pastX), (byte)(pastY));
                                if (!moveMetadata.ContainsKey(mdKey))
                                {
                                    MoveMetadata md = new MoveMetadata(piece, orbitX, orbitY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 1));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                    moveMetadata.Add(mdKey, md);
                                }
                                else
                                {
                                    MoveMetadata md = moveMetadata[mdKey];
                                    md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, (uint)(px + (py << 3)), 1));
                                    if (moveMetadata.ContainsKey(otherKey))
                                    {
                                        md.AddPredecessor(moveMetadata[otherKey]);
                                    }
                                    if (pastX == x && pastY == y)
                                    {
                                        md.terminalNode = true;
                                    }
                                }
                            }
                            pastX = orbitX;
                            pastY = orbitY;
                            if (!keepGoingO)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.S:   //Rose knight
                    //Hardcoded
                    deltaX = 1;
                    deltaY = 2;
                    GenerateRoseMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata);
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
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            (deltaX, deltaY) = (0, aY * ch);//Move.TransformBasedOnAlignment(pa, cl, ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //down
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            (deltaX, deltaY) = (0, aY * -ch); //Move.TransformBasedOnAlignment(pa, cl, -ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (aX * ch, 0); //Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (aX * -ch, 0); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }
                    }
                    else if (deltaX == deltaY || deltaX == -deltaY)
                    {
                        //4 diagonal leaper
                        //ray up right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                        {
                            (deltaX, deltaY) = (aX * ch, aY * cl); //Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //ray up left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0)
                        {
                            (deltaX, deltaY) = (aX * -ch, aY * cl); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //ray down right
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                        {
                            (deltaX, deltaY) = (aX * ch, aY * -cl); //Move.TransformBasedOnAlignment(pa, ch, -cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //ray down left
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0)
                        {
                            (deltaX, deltaY) = (aX * -ch, aY * -cl); //Move.TransformBasedOnAlignment(pa, -ch, -cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }
                    }
                    else
                    {
                        //8 leaper

                        //Logic is slightly different for directions
                        //f = front 4
                        //v = vertical 4 
                        //fv = front vertical 2

                        //Inverted conditions

                        //Vertical 4
                        //ray up right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0 && (mgie.modifier & MoveGeneratorPreModifier.h) == 0))
                        {
                            (deltaX, deltaY) = (aX * cl, aY * ch); //Move.TransformBasedOnAlignment(pa, cl, ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8));
                        }
                        //ray up left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0 && (mgie.modifier & MoveGeneratorPreModifier.h) == 0))
                        {
                            (deltaX, deltaY) = (aX * -cl, aY * ch); //Move.TransformBasedOnAlignment(pa, -cl, ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 1);
                        }
                        //ray down right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0 && (mgie.modifier & MoveGeneratorPreModifier.h) == 0))
                        {
                            (deltaX, deltaY) = (aX * cl, aY * -ch); //Move.TransformBasedOnAlignment(pa, cl, -ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 2);
                        }
                        //ray down left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0 && (mgie.modifier & MoveGeneratorPreModifier.h) == 0))
                        {
                            (deltaX, deltaY) = (aX * -cl, aY * -ch); //Move.TransformBasedOnAlignment(pa, -cl, -ch, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 3);
                        }

                        //Horizontal 4
                        //ray up right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0 && (mgie.modifier & MoveGeneratorPreModifier.v) == 0))
                        {
                            (deltaX, deltaY) = (aX * ch, aY * cl); //Move.TransformBasedOnAlignment(pa, ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 4);
                        }
                        //ray up left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.b) == 0 && (mgie.modifier & MoveGeneratorPreModifier.v) == 0))
                        {
                            (deltaX, deltaY) = (aX * -ch, aY * cl); //Move.TransformBasedOnAlignment(pa, -ch, cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 5);
                        }
                        //ray down right
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0 && (mgie.modifier & MoveGeneratorPreModifier.v) == 0))
                        {
                            (deltaX, deltaY) = (aX * ch, aY * -cl); //Move.TransformBasedOnAlignment(pa, ch, -cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 6);
                        }
                        //ray down left
                        if (!directionRestricted || ((mgie.modifier & MoveGeneratorPreModifier.f) == 0 && (mgie.modifier & MoveGeneratorPreModifier.v) == 0))
                        {
                            (deltaX, deltaY) = (aX * -ch, aY * -cl); //Move.TransformBasedOnAlignment(pa, -ch, -cl, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, (uint)((ch + (cl << 3)) << 8) + 7);
                        }
                    }
                    break;
                case MoveGeneratorAtom.Castling:
                    //spawns moves
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        if (!b.whitePerPlayerInfo.canCastle)
                        {
                            return;
                            //return moveStartIndex;
                        }
                    }
                    else
                    {
                        if (!b.blackPerPlayerInfo.canCastle)
                        {
                            return;
                            //return moveStartIndex;
                        }
                    }

                    GenerateCastling(moves, ref b, pa, pt, piece, x, y, true, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 0));
                    GenerateCastling(moves, ref b, pa, pt, piece, x, y, false, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 1));
                    break;
                case MoveGeneratorAtom.AllyKingTeleport:
                    //Teleport to empty spaces
                    ulong allyKingBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        allyKingBitboard = MainManager.SmearBitboard(b.globalData.bitboard_kingWhite);
                    }
                    else
                    {
                        allyKingBitboard = MainManager.SmearBitboard(b.globalData.bitboard_kingBlack);
                    }
                    allyKingBitboard &= ~b.globalData.bitboard_pieces;
                    while (allyKingBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(allyKingBitboard, out allyKingBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.EnemyKingTeleport:
                    //Teleport to empty spaces
                    ulong enemyKingBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        enemyKingBitboard = MainManager.SmearBitboard(b.globalData.bitboard_kingBlack);
                    }
                    else
                    {
                        enemyKingBitboard = MainManager.SmearBitboard(b.globalData.bitboard_kingWhite);
                    }
                    enemyKingBitboard &= ~b.globalData.bitboard_pieces;
                    while (enemyKingBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(enemyKingBitboard, out enemyKingBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.PawnSwapTeleport:
                    ulong allyPawnsBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        allyPawnsBitboard = b.globalData.bitboard_pawnsWhite;
                    }
                    else
                    {
                        allyPawnsBitboard = b.globalData.bitboard_pawnsBlack;
                    }
                    while (allyPawnsBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(allyPawnsBitboard, out allyPawnsBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.AllySwap, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AllySwapTeleport:
                    ulong allyBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        allyBitboard = b.globalData.bitboard_piecesWhite;
                    }
                    else
                    {
                        allyBitboard = b.globalData.bitboard_piecesBlack;
                    }
                    while (allyBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(allyBitboard, out allyBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.AllySwap, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AllyBehindTeleport:
                    ulong allyBBitboard = 0;
                    //behind is subjective so 2 separate code lines
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        allyBBitboard = b.globalData.bitboard_piecesWhite;
                        //down 1 but not occupied
                        allyBBitboard = allyBBitboard >> 8 & ~allyBBitboard;
                    }
                    else
                    {
                        allyBBitboard = b.globalData.bitboard_piecesBlack;
                        allyBBitboard = allyBBitboard << 8 & ~allyBBitboard;
                    }

                    while (allyBBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(allyBBitboard, out allyBBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.EnemyBehindTeleport:
                    ulong enemyBBitboard = 0;
                    //behind is subjective so 2 separate code lines
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        enemyBBitboard = b.globalData.bitboard_piecesBlack;
                        //down 1 but not occupied
                        enemyBBitboard = enemyBBitboard << 8 & ~enemyBBitboard;
                    }
                    else
                    {
                        enemyBBitboard = b.globalData.bitboard_piecesWhite;
                        enemyBBitboard = enemyBBitboard >> 8 & ~enemyBBitboard;
                    }

                    while (enemyBBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(enemyBBitboard, out enemyBBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AnywhereTeleport:
                    ulong anywhereBitboard = 0;
                    anywhereBitboard = ~b.globalData.bitboard_pieces;
                    while (anywhereBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(anywhereBitboard, out anywhereBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AnywhereAdjacentTeleport:
                    ulong anywhereAdjacentBitboard = 0;
                    anywhereAdjacentBitboard = (MainManager.SmearBitboard(b.globalData.bitboard_pieces));
                    while (anywhereAdjacentBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(anywhereAdjacentBitboard, out anywhereAdjacentBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AnywhereNonAdjacentTeleport:
                    ulong anywhereNonAdjacentBitboard = 0;
                    anywhereNonAdjacentBitboard = ~(MainManager.SmearBitboard(b.globalData.bitboard_pieces));
                    while (anywhereNonAdjacentBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(anywhereNonAdjacentBitboard, out anywhereNonAdjacentBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AnywhereSameColorTeleport:
                    ulong anywhereSameBitboard = 0;
                    anywhereSameBitboard = (BITBOARD_PATTERN_FULL) & ((((x + y) & 1) == 0) ? BITBOARD_PATTERN_BLACKSQUARES : BITBOARD_PATTERN_WHITESQUARES);
                    while (anywhereSameBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(anywhereSameBitboard, out anywhereSameBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AnywhereOppositeColorTeleport:
                    ulong anywhereOppositeBitboard = 0;
                    anywhereOppositeBitboard = (BITBOARD_PATTERN_FULL) & ((((x + y) & 1) != 0) ? BITBOARD_PATTERN_BLACKSQUARES : BITBOARD_PATTERN_WHITESQUARES);
                    while (anywhereOppositeBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(anywhereOppositeBitboard, out anywhereOppositeBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.HomeRangeTeleport:
                    ulong homeRangeBitboard = 0xffff;

                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.Black)
                    {
                        //6 rows upwards
                        homeRangeBitboard <<= 48;
                    }

                    homeRangeBitboard &= ~b.globalData.bitboard_pieces;
                    while (homeRangeBitboard != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(homeRangeBitboard, out homeRangeBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.KingSwapTeleport:
                    ulong allyKingBitboardB = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        allyKingBitboardB = b.globalData.bitboard_kingWhite;
                    }
                    else
                    {
                        allyKingBitboardB = b.globalData.bitboard_kingBlack;
                    }
                    while (allyKingBitboardB != 0)
                    {
                        int pieceIndex = MainManager.PopBitboardLSB1(allyKingBitboardB, out allyKingBitboardB);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, pieceIndex & 7, (pieceIndex & 56) >> 3, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(pieceIndex & 7), (byte)(pieceIndex >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, pieceIndex & 7, pieceIndex >> 3, MoveMetadata.PathType.Teleport, SpecialType.AllySwap, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.MirrorTeleport:
                    GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, 7 - x, y, Dir.Null, pa, SpecialType.Normal, pte, mbt);
                    break;
                case MoveGeneratorAtom.MirrorTeleportSwap:
                    GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, 7 - x, y, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                    break;
                case MoveGeneratorAtom.VerticalMirrorTeleport:
                    GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, x, 7 - y, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                    break;
                case MoveGeneratorAtom.BlossomTeleport:
                    ulong blossomTeleportBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        blossomTeleportBitboard = b.globalData.bitboard_piecesWhiteAdjacent4 | b.globalData.bitboard_piecesWhiteAdjacent8;
                        blossomTeleportBitboard |= (b.globalData.bitboard_piecesWhiteAdjacent2 & b.globalData.bitboard_piecesWhiteAdjacent1);
                        blossomTeleportBitboard &= ~b.globalData.bitboard_piecesWhite;
                    }
                    else
                    {
                        blossomTeleportBitboard = b.globalData.bitboard_piecesBlackAdjacent4 | b.globalData.bitboard_piecesBlackAdjacent8;
                        blossomTeleportBitboard |= (b.globalData.bitboard_piecesBlackAdjacent2 & b.globalData.bitboard_piecesBlackAdjacent1);
                        blossomTeleportBitboard &= ~b.globalData.bitboard_piecesBlack;
                    }
                    while (blossomTeleportBitboard != 0)
                    {
                        int index = MainManager.PopBitboardLSB1(blossomTeleportBitboard, out blossomTeleportBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, index & 7, (index & 56) >> 3, Dir.Null, pa, SpecialType.Normal, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(index & 7), (byte)(index >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, index & 7, index >> 3, MoveMetadata.PathType.Teleport, SpecialType.Normal, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.ForestTeleport:
                    ulong forestTeleportBitboard = 0;
                    if (Piece.GetPieceAlignment(piece) == Piece.PieceAlignment.White)
                    {
                        forestTeleportBitboard = b.globalData.bitboard_piecesWhiteAdjacent4 | b.globalData.bitboard_piecesWhiteAdjacent8;
                    }
                    else
                    {
                        forestTeleportBitboard = b.globalData.bitboard_piecesBlackAdjacent4 | b.globalData.bitboard_piecesBlackAdjacent8;
                    }
                    forestTeleportBitboard &= ~b.globalData.bitboard_pieces;
                    while (forestTeleportBitboard != 0)
                    {
                        int index = MainManager.PopBitboardLSB1(forestTeleportBitboard, out forestTeleportBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, index & 7, (index & 56) >> 3, Dir.Null, pa, SpecialType.Normal, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(index & 7), (byte)(index >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, index & 7, index >> 3, MoveMetadata.PathType.Teleport, SpecialType.Normal, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.DiplomatTeleport:
                    ulong diplomatBitboard = b.globalData.bitboard_piecesBlackAdjacent & b.globalData.bitboard_piecesWhiteAdjacent;
                    while (diplomatBitboard != 0)
                    {
                        int index = MainManager.PopBitboardLSB1(diplomatBitboard, out diplomatBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, index & 7, (index & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(index & 7), (byte)(index >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, index & 7, index >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.EchoTeleport:
                    int lastMovedLocation = -1;
                    if (pa == PieceAlignment.White)
                    {
                        if (b.blackPerPlayerInfo.lastMove != 0)
                        {
                            lastMovedLocation = Move.GetFromX(b.blackPerPlayerInfo.lastMove) + (Move.GetFromY(b.blackPerPlayerInfo.lastMove) << 3);
                        }
                    }
                    else if (pa == PieceAlignment.Black)
                    {
                        if (b.whitePerPlayerInfo.lastMove != 0)
                        {
                            lastMovedLocation = Move.GetFromX(b.whitePerPlayerInfo.lastMove) + (Move.GetFromY(b.whitePerPlayerInfo.lastMove) << 3);
                        }
                    }
                    if (lastMovedLocation != -1)
                    {
                        (_, bool wasGeneratedE) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, lastMovedLocation & 7, (lastMovedLocation & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGeneratedE)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(lastMovedLocation & 7), (byte)((lastMovedLocation & 56) >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, lastMovedLocation & 7, (lastMovedLocation & 56) >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.CoastTeleport:
                    ulong coastBitboard = BITBOARD_PATTERN_OUTEREDGES;
                    coastBitboard &= ~b.globalData.bitboard_pieces;
                    while (coastBitboard != 0)
                    {
                        int index = MainManager.PopBitboardLSB1(coastBitboard, out coastBitboard);

                        //Plop a move down
                        (_, bool wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, index & 7, (index & 56) >> 3, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                        if (moveMetadata != null && wasGenerated)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(index & 7), (byte)(index >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, index & 7, index >> 3, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.AimMover:
                    ushort value = Piece.GetPieceSpecialData(piece);

                    switch (pt)
                    {
                        case PieceType.SteelGolem:
                        case PieceType.SteelPuppet:
                            specialType = SpecialType.CaptureOnly;
                            break;
                        case PieceType.Cannon:
                            specialType = SpecialType.FireCaptureOnly;
                            break;
                    }

                    //Debug.Log(value & 63);
                    if (value != 0)
                    {
                        (_, bool wasGeneratedE) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, value & 7, (value & 56) >> 3, Dir.Null, pa, specialType, pte, mbt);
                        if (moveMetadata != null && wasGeneratedE)
                        {
                            uint key = Move.PackMove((byte)x, (byte)y, (byte)(value & 7), (byte)((value & 56) >> 3));
                            if (!moveMetadata.ContainsKey(key))
                            {
                                moveMetadata.Add(key, new MoveMetadata(piece, value & 7, (value & 56) >> 3, MoveMetadata.PathType.Teleport, specialType, MoveMetadata.MakePathTag(mgie.atom, 0)));
                            }
                        }
                    }
                    break;
                case MoveGeneratorAtom.LensRook:
                    //plop a move down?
                    (bool keepGoingLR, bool wasGeneratedB) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, 7 - x, 7 - y, Dir.Null, pa, SpecialType.MoveOnly, pte, mbt);
                    if (moveMetadata != null && wasGeneratedB)
                    {
                        moveMetadata.Add(Move.PackMove((byte)x, (byte)y, (byte)(7 - x), (byte)(7 - y), Dir.Null, SpecialType.MoveOnly), new MoveMetadata(piece, 7 - x, 7 - y, MoveMetadata.PathType.Teleport, SpecialType.MoveOnly, new List<uint> { MoveMetadata.MakePathTag(mgie.atom, 0), MoveMetadata.MakePathTag(mgie.atom, 1), MoveMetadata.MakePathTag(mgie.atom, 2), MoveMetadata.MakePathTag(mgie.atom, 3) }));
                    }

                    if (keepGoingLR)
                    {
                        //ray up
                        //Up is allowed if F is set or V is set
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.f) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            (deltaX, deltaY) = (0, aY); //Move.TransformBasedOnAlignment(pa, 0, 1, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, 7 - x, 7 - y, 0, deltaX, deltaY, SpecialType.MoveOnly, pte, mgie, mbt, moveMetadata, 0);
                        }
                        //ray down
                        //Down is allowed if B is set or V is set
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.b) != 0 || (mgie.modifier & MoveGeneratorPreModifier.v) != 0)
                        {
                            (deltaX, deltaY) = (0, -aY); //Move.TransformBasedOnAlignment(pa, 0, -1, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, 7 - x, 7 - y, 0, deltaX, deltaY, SpecialType.MoveOnly, pte, mgie, mbt, moveMetadata, 1);
                        }
                        //ray left
                        //Left is allowed if H is set
                        //Note that currently there is no left right asymmetry allowed in movesets
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (-aX, 0); //Move.TransformBasedOnAlignment(pa, -1, 0, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, 7 - x, 7 - y, 0, deltaX, deltaY, SpecialType.MoveOnly, pte, mgie, mbt, moveMetadata, 2);
                        }
                        //ray right
                        //Right is allowed if H is set
                        //Note that currently there is no left right asymmetry allowed in movesets
                        if (!directionRestricted || (mgie.modifier & MoveGeneratorPreModifier.h) != 0)
                        {
                            (deltaX, deltaY) = (aX, 0); //Move.TransformBasedOnAlignment(pa, 1, 0, flip);
                            GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, 7 - x, 7 - y, 0, deltaX, deltaY, SpecialType.MoveOnly, pte, mgie, mbt, moveMetadata, 3);
                        }
                    }
                    break;
                case MoveGeneratorAtom.Recall:
                    switch (pa)
                    {
                        case Piece.PieceAlignment.White:
                            if (y != 0)
                            {
                                (_, bool wgW) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, x, 0, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                                if (moveMetadata != null && wgW)
                                {
                                    uint key = Move.PackMove((byte)x, (byte)y, (byte)(x), (byte)0);
                                    if (!moveMetadata.ContainsKey(key))
                                    {
                                        moveMetadata.Add(key, new MoveMetadata(piece, x, 0, MoveMetadata.PathType.Teleport, specialType, MoveMetadata.MakePathTag(mgie.atom, 0)));
                                    }
                                }
                            }
                            break;
                        case Piece.PieceAlignment.Black:
                            if (y != 7)
                            {
                                (_, bool wgB) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, x, 7, Dir.Null, pa, SpecialType.AllySwap, pte, mbt);
                                if (moveMetadata != null && wgB)
                                {
                                    uint key = Move.PackMove((byte)x, (byte)y, (byte)(x), (byte)0);
                                    if (!moveMetadata.ContainsKey(key))
                                    {
                                        moveMetadata.Add(key, new MoveMetadata(piece, x, 0, MoveMetadata.PathType.Teleport, specialType, MoveMetadata.MakePathTag(mgie.atom, 0)));
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }
        }
        return;
    }

    private static bool GenerateOffsetMovesBetweenPoints(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int startX, int startY, int endX, int endY, Move.SpecialType specialType, PieceTableEntry pte, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        //try to go
        int dist = 0;

        int dx = endX - startX;
        int dy = endY - startY;

        dist = dx;
        if (dx < 0)
        {
            dist = -dx;
        }
        if (dy > dist)
        {
            dist = dy;
        }
        else if (-dy > dist)
        {
            dist = -dy;
        }

        bool invalidDirection = true;
        if (startX == endX || startY == endY)
        {
            invalidDirection = false;
        }
        if ((endX - startX) == (endY - startY))
        {
            invalidDirection = false;
        }
        if ((endX - startX) == -(endY - startY))
        {
            invalidDirection = false;
        }

        if (invalidDirection)
        {
            return false;
        }

        if (dx > 0)
        {
            dx = 1;
        }
        else if (dx < 0)
        {
            dx = -1;
        }
        if (dy > 0)
        {
            dy = 1;
        }
        else if (dy < 0)
        {
            dy = -1;
        }

        return GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, startX, startY, dx, dy, specialType, pte, dist, mbt, moveMetadata, pathTag);
    }

    private static bool GenerateOffsetRayMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int startX, int startY, int lostRange, int deltaX, int deltaY, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        //I can precompute these in the above loop but that adds more arguments

        //new idea: only compute them on demand to assume it is less likely to hit these?
        //bool cylindrical = (pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0;
        bool tubular = (pte.pieceProperty & Piece.PieceProperty.Sneaky) != 0;
        //bool reflecter = (pte.pieceProperty & Piece.PieceProperty.Reflecter) != 0;

        if (specialType == SpecialType.Spawn)
        {
            tubular = false;
        }

        int rangeMultiplier = 1;
        bool specialRange = (pte.pieceProperty & Piece.PieceProperty.RangeChange) != 0;

        if ((pte.piecePropertyB & (Piece.PiecePropertyB.ChargeEnhanceStack | Piece.PiecePropertyB.ChargeEnhanceStackReset)) != 0 && (specialType == SpecialType.ChargeMove || specialType == SpecialType.ChargeMoveReset || specialType == SpecialType.FireCaptureOnly))
        {
            rangeMultiplier = Piece.GetPieceSpecialData(piece);
        }

        Move.Dir dir = Dir.Null;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        if (pa == Piece.PieceAlignment.Black && (b.globalData.enemyModifier & Board.EnemyModifier.Knave) != 0)
        {
            tubular = true;
        }

        dir = Move.DeltaToDir(deltaX, deltaY);

        int generatedMoves = 0;

        if (specialRange)
        {
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_MissingPieces) != 0)
            {
                if (pa == Piece.PieceAlignment.Neutral || pa == Piece.PieceAlignment.Crystal)
                {
                    rangeMultiplier = 1;
                }
                else
                {
                    rangeMultiplier = 1 + b.GetMissingPieces(pa == Piece.PieceAlignment.Black);
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = y + 1;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = (8 - y);
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_NearRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = 8 - y;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = y + 1;
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeDecrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = (8 - 2 * y);
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = 2 * y - 7;
                        break;
                }
            }
            if (rangeMultiplier < 1)
            {
                rangeMultiplier = 1;
            }
        }

        int tempX = startX;
        int tempY = startY;

        //int lastTempX = tempX;
        //int lastTempY = tempY;

        /*
        if (moveMetadata != null)
        {
            bool isRider = false;
            if ((x - tempX) > 1 || (x - tempX) < -1 || (y - tempY) > 1 || (y - tempY) < -1)
            {
                isRider = true;
            }
            MoveMetadata md;
            uint mdKey = Move.PackMove(x, y, tempX, tempY);
            if ((lastTempX != x || lastTempY != y) && !moveMetadata.ContainsKey(mdKey))
            {
                if (isRider)
                {
                    md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, pathTag);
                    moveMetadata.Add(mdKey, md);
                }
                else
                {
                    md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, pathTag);
                    moveMetadata.Add(mdKey, md);
                }
            }
        }
        */

        int effectiveMaxRange = (mgie.range - 1) + rangeMultiplier; //mgie.range * rangeMultiplier;
        if (effectiveMaxRange == 0)
        {
            effectiveMaxRange = 8;
        }

        if ((pte.pieceProperty & Piece.PieceProperty.RangeDecrease_FurtherRows) != 0)
        {
            if (mgie.range == 0)
            {
                effectiveMaxRange = 1 * rangeMultiplier;
            }
            if (mgie.range == 1)
            {
                effectiveMaxRange = 1;
            }
        }

        switch (pte.type)
        {
            case PieceType.RabbitCourier:
                ulong bitIndex = 1uL << (x + (y << 3));
                effectiveMaxRange = 1;
                switch (pa)
                {
                    case PieceAlignment.White:
                        if ((bitIndex & b.globalData.bitboard_piecesWhiteAdjacent8) != 0)
                        {
                            effectiveMaxRange += 8;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesWhiteAdjacent4) != 0)
                        {
                            effectiveMaxRange += 4;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesWhiteAdjacent2) != 0)
                        {
                            effectiveMaxRange += 2;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesWhiteAdjacent1) != 0)
                        {
                            effectiveMaxRange += 1;
                        }
                        break;
                    case PieceAlignment.Black:
                        if ((bitIndex & b.globalData.bitboard_piecesBlackAdjacent8) != 0)
                        {
                            effectiveMaxRange += 8;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesBlackAdjacent4) != 0)
                        {
                            effectiveMaxRange += 4;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesBlackAdjacent2) != 0)
                        {
                            effectiveMaxRange += 2;
                        }
                        if ((bitIndex & b.globalData.bitboard_piecesBlackAdjacent1) != 0)
                        {
                            effectiveMaxRange += 1;
                        }
                        break;
                }
                break;
            case PieceType.Gluttony:
                effectiveMaxRange += Piece.GetPieceSpecialData(piece);
                break;
            case PieceType.King:
                if (pa == Piece.PieceAlignment.Black && (b.globalData.enemyModifier & Board.EnemyModifier.Voracious) != 0)
                {
                    effectiveMaxRange = 1 + (b.GetMissingPieces(false) >> 2);
                }
                break;
        }

        bool wasGenerated = true;
        bool keepGoing = true;

        bool canMove = true;
        int currentRange = lostRange;
        while (true)
        {
            //lastTempX = tempX;
            //lastTempY = tempY;
            tempX += deltaX;
            tempY += deltaY;
            currentRange++;

            switch (mgie.rangeType)
            {
                case RangeType.Exact:
                    //Going too far?
                    if (currentRange > effectiveMaxRange)
                    {
                        return keepGoing;
                    }
                    canMove = (currentRange == mgie.range);
                    break;
                case RangeType.Minimum:
                    canMove = (currentRange >= mgie.range);
                    break;
                case RangeType.AntiRange:
                    canMove = false;
                    break;
                default:
                    //Going too far?
                    if (currentRange > effectiveMaxRange)
                    {
                        return keepGoing;
                    }
                    break;
            }


            //Out of bounds?
            //if (tempX < 0 || tempX > 7)
            if ((tempX & -8) != 0)  //is 1 conditional + 1 operation better than 2 conditionals?
            {
                if ((pte.pieceProperty & (Piece.PieceProperty.Cylindrical | Piece.PieceProperty.Reflecter)) == 0)
                {
                    if (mgie.rangeType == RangeType.AntiRange && !(tempX - deltaX == x && tempY - deltaY == y))
                    {
                        (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX - deltaX, tempY - deltaY, dir, pa, specialType, pte, mgie, null);
                    }

                    return false;
                }

                if ((pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0)
                {
                    if (tempX < 0)
                    {
                        tempX += 8;
                    }
                    else
                    {
                        tempX -= 8;
                    }
                }
                if ((pte.pieceProperty & Piece.PieceProperty.Reflecter) != 0)
                {
                    if (deltaY == 0)
                    {
                        return false;
                    }
                    if (tempX < 0)
                    {
                        tempX = -tempX;
                        deltaX = -deltaX;
                    }
                    else
                    {
                        tempX = 14 - tempX;
                        deltaX = -deltaX;
                    }
                }
            }
            if ((tempY & -8) != 0)
            //if (tempY < 0 || tempY > 7)
            {
                if (!tubular)
                {
                    if (mgie.rangeType == RangeType.AntiRange && !(tempX - deltaX == x && tempY - deltaY == y))
                    {
                        (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX - deltaX, tempY - deltaY, dir, pa, specialType, pte, mgie, null);
                    }

                    return false;
                }

                if (tempY < 0)
                {
                    tempY += 8;
                }
                else
                {
                    tempY -= 8;
                }
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }

            //Is an obstacle in the way?
            (keepGoing, wasGenerated) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dir, pa, specialType, pte, mgie, mbt);

            if (moveMetadata != null && (keepGoing || wasGenerated))
            {
                int lastTempX = (tempX - deltaX);
                int lastTempY = (tempY - deltaY);
                if (lastTempX < 0)
                {
                    if ((pte.pieceProperty & (Piece.PieceProperty.Reflecter)) != 0)
                    {
                        lastTempX = -lastTempX;
                    }
                    if ((pte.pieceProperty & (Piece.PieceProperty.Cylindrical)) != 0)
                    {
                        lastTempX += 8;
                    }
                }
                if (lastTempX > 7)
                {
                    if ((pte.pieceProperty & (Piece.PieceProperty.Reflecter)) != 0)
                    {
                        lastTempX = 14 - lastTempX;
                    }
                    if ((pte.pieceProperty & (Piece.PieceProperty.Cylindrical)) != 0)
                    {
                        lastTempX -= 8;
                    }
                }
                if (lastTempY < 0)
                {
                    lastTempY += 8;
                }
                if (lastTempY > 7)
                {
                    lastTempY -= 8;
                }

                bool isRider = false;
                if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
                {
                    isRider = true;
                }
                MoveMetadata md;
                uint mdKey = Move.PackMove(x, y, tempX, tempY);
                uint nkey = Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY));
                if (!moveMetadata.ContainsKey(mdKey))
                {
                    if (isRider)
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                        if ((lastTempX != x || lastTempY != y) && moveMetadata.ContainsKey(nkey))
                        {
                            md.AddPredecessor(moveMetadata[nkey]);
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                    else
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                        if ((lastTempX != x || lastTempY != y) && moveMetadata.ContainsKey(nkey))
                        {
                            md.AddPredecessor(moveMetadata[nkey]);
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                }
                else
                {
                    md = moveMetadata[mdKey];
                    md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathTag));
                    if ((lastTempX != x || lastTempY != y) && moveMetadata.ContainsKey(nkey))
                    {
                        md.AddPredecessor(moveMetadata[nkey]);
                    }
                }
            }

            if (!keepGoing || specialType == SpecialType.LongLeaperCaptureOnly)
            {
                if (specialType == SpecialType.LongLeaperCaptureOnly)
                {
                    uint obstaclePiece = b.pieces[tempX + (tempY << 3)];
                    if (obstaclePiece == 0 || Piece.GetPieceAlignment(obstaclePiece) == pa)
                    {
                        return false;
                    }
                }

                //For antirange: if nothing was generated you have to generate something on the last square
                //That means you hit an ally piece
                //No mbt because the bit would already be set
                if (mgie.rangeType == RangeType.AntiRange && !wasGenerated)
                {
                    (keepGoing, wasGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX - deltaX, tempY - deltaY, dir, pa, specialType, pte, mgie, null);
                    if (wasGenerated)
                    {
                        generatedMoves++;
                    }
                }

                //For flying: change type to flying
                bool canFly = Move.CanFlyOverObstacles(specialType);
                if ((canFly || pm == Piece.PieceModifier.Winged || ((pte.piecePropertyB & PiecePropertyB.NaturalWinged) != 0) || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SideWings) != 0) && (x < 2 || x > 5))) && specialType != SpecialType.FlyingMoveOnly)
                {
                    if (!canFly)
                    {
                        switch (specialType)
                        {
                            case SpecialType.AllyAbility:
                            case SpecialType.EmptyAbility:
                            case SpecialType.EnemyAbility:
                            case SpecialType.PassiveAbility:
                                break;
                            default:
                                specialType = SpecialType.FlyingMoveOnly;
                                break;
                        }
                    }

                    if (moveMetadata != null && (!wasGenerated) && moveMetadata != null)
                    {
                        int lastTempX = (tempX - deltaX);
                        int lastTempY = (tempY - deltaY);
                        if (lastTempX < 0)
                        {
                            if ((pte.pieceProperty & (Piece.PieceProperty.Reflecter)) != 0)
                            {
                                lastTempX = -lastTempX;
                            }
                            if ((pte.pieceProperty & (Piece.PieceProperty.Cylindrical)) != 0)
                            {
                                lastTempX += 8;
                            }
                        }
                        if (lastTempX > 7)
                        {
                            if ((pte.pieceProperty & (Piece.PieceProperty.Reflecter)) != 0)
                            {
                                lastTempX = 14 - lastTempX;
                            }
                            if ((pte.pieceProperty & (Piece.PieceProperty.Cylindrical)) != 0)
                            {
                                lastTempX -= 8;
                            }
                        }
                        if (lastTempY < 0)
                        {
                            lastTempX += 8;
                        }
                        if (lastTempY > 7)
                        {
                            lastTempX -= 8;
                        }


                        bool isRider = false;
                        if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
                        {
                            isRider = true;
                        }
                        MoveMetadata md;
                        uint mdKey = Move.PackMove(x, y, tempX, tempY);
                        if (!moveMetadata.ContainsKey(mdKey))
                        {
                            if (isRider)
                            {
                                md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                                if (lastTempX != x || lastTempY != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                            else
                            {
                                md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, pathTag));
                                if (lastTempX != x || lastTempY != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                        }
                        else
                        {
                            md = moveMetadata[mdKey];
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathTag));
                            if (lastTempX != x || lastTempY != y)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        //moved up a level
        //return keepGoing;

        //return generatedMoves > 0;
        //return moveStartIndex;
    }

    private static bool GenerateOffsetRayMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int startX, int startY, int deltaX, int deltaY, Move.SpecialType specialType, PieceTableEntry pte, int maxRange, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        //null
        //returns true because it didn't even check any obstacles
        if (maxRange == 0)
        {
            return true;
        }

        //I can precompute these in the above loop but that adds more arguments
        bool cylindrical = (pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0;
        bool tubular = (pte.pieceProperty & Piece.PieceProperty.Sneaky) != 0;
        bool reflecter = (pte.pieceProperty & Piece.PieceProperty.Reflecter) != 0;

        if (specialType == SpecialType.Spawn)
        {
            tubular = false;
        }

        int rangeMultiplier = 1;
        bool specialRange = (pte.pieceProperty & Piece.PieceProperty.RangeChange) != 0;

        Move.Dir dir = Dir.Null;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        //king can't to stop instant cheeses in early game with less pieces
        if (pa == Piece.PieceAlignment.Black && (b.globalData.enemyModifier & Board.EnemyModifier.Knave) != 0 && pte.type != PieceType.King)
        {
            tubular = true;
        }

        dir = Move.DeltaToDir(deltaX, deltaY);

        if (specialRange)
        {
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_MissingPieces) != 0)
            {
                if (pa == Piece.PieceAlignment.Neutral || pa == Piece.PieceAlignment.Crystal)
                {
                    rangeMultiplier = 1;
                }
                else
                {
                    rangeMultiplier = 1 + b.GetMissingPieces(pa == Piece.PieceAlignment.Black);
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = y + 1;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = (8 - y);
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_NearRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = 8 - y;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = y + 1;
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeDecrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = (8 - 2 * y);
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = 2 * y - 7;
                        break;
                }
            }
            if (rangeMultiplier < 1)
            {
                rangeMultiplier = 1;
            }
        }

        int tempX = startX;
        int tempY = startY;

        int lastTempX = tempX;
        int lastTempY = tempY;

        bool canMove = true;
        int currentRange = 0;

        bool keepGoing = true;
        bool wasGenerated = false;

        while (true)
        {
            lastTempX = tempX;
            lastTempY = tempY;
            tempX += deltaX;
            tempY += deltaY;
            currentRange++;

            //Going too far?
            if (currentRange > maxRange)
            {
                break;
            }

            //Out of bounds?
            //if (tempX < 0 || tempX > 7)
            if ((tempX & -8) != 0)
            {
                if (!cylindrical && !reflecter)
                {
                    if (!(lastTempX == x && lastTempY == y))
                    {
                        (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, lastTempX, lastTempY, dir, pa, specialType, pte, mbt);
                    }

                    keepGoing = false;
                    break;
                }

                if (cylindrical)
                {
                    if (tempX < 0)
                    {
                        tempX += 8;
                    }
                    else
                    {
                        tempX -= 8;
                    }
                }
                if (reflecter)
                {
                    if (deltaY == 0)
                    {
                        keepGoing = false;
                        break;
                    }
                    if (tempX < 0)
                    {
                        tempX = -tempX;
                        deltaX = -deltaX;
                    }
                    else
                    {
                        tempX = 14 - tempX;
                        deltaX = -deltaX;
                    }
                }
            }
            if ((tempY & -8) != 0)
            //if (tempY < 0 || tempY > 7)
            {
                if (!tubular)
                {
                    keepGoing = false;
                    break;
                }

                if (tempY < 0)
                {
                    tempY += 8;
                }
                else
                {
                    tempY -= 8;
                }
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }

            //Is an obstacle in the way?
            (keepGoing, wasGenerated) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dir, pa, specialType, pte, mbt);

            if (moveMetadata != null && (keepGoing || wasGenerated))
            {
                bool isRider = false;
                if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
                {
                    isRider = true;
                }
                MoveMetadata md;
                uint mdKey = Move.PackMove(x, y, tempX, tempY);
                if (!moveMetadata.ContainsKey(mdKey))
                {
                    if (isRider)
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, pathTag);
                        if (lastTempX != x || lastTempY != y)
                        {
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                        }
                        else
                        {
                            md.terminalNode = true;
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                    else
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, pathTag);
                        if (lastTempX != x || lastTempY != y)
                        {
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                        }
                        else
                        {
                            md.terminalNode = true;
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                }
                else
                {
                    md = moveMetadata[mdKey];
                    md.pathTags.Add(pathTag);
                    if (lastTempX != x || lastTempY != y)
                    {
                        md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                    }
                    else
                    {
                        md.terminalNode = true;
                    }
                }
            }

            if (specialType == SpecialType.LongLeaperCaptureOnly)
            {
                uint obstaclePiece = b.pieces[tempX + (tempY << 3)];
                if (obstaclePiece == 0 || Piece.GetPieceAlignment(obstaclePiece) == pa)
                {
                    return false;
                }
            }

            if (!keepGoing)
            {
                //For flying: change type to flying
                if ((Move.CanFlyOverObstacles(specialType) || pm == Piece.PieceModifier.Winged || ((pte.piecePropertyB & PiecePropertyB.NaturalWinged) != 0) || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SideWings) != 0) && (x < 2 || x > 5))) && specialType != SpecialType.FlyingMoveOnly)
                {
                    if (!Move.CanFlyOverObstacles(specialType))
                    {
                        specialType = SpecialType.FlyingMoveOnly;
                    }

                    //re-add the metadata if it would have failed the wasGenerated condition
                    if (moveMetadata != null && (!wasGenerated))
                    {
                        bool isRider = false;
                        if (deltaX > 1 || deltaX < -1 || deltaY > 1 || deltaY < -1)
                        {
                            isRider = true;
                        }
                        MoveMetadata md;
                        uint mdKey = Move.PackMove(x, y, tempX, tempY);
                        if (!moveMetadata.ContainsKey(mdKey))
                        {
                            if (isRider)
                            {
                                md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, pathTag);
                                if (lastTempX != x || lastTempY != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                            else
                            {
                                md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, pathTag);
                                if (lastTempX != x || lastTempY != y)
                                {
                                    md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                                }
                                moveMetadata.Add(mdKey, md);
                            }
                        }
                        else
                        {
                            md = moveMetadata[mdKey];
                            md.pathTags.Add(pathTag);
                            if (lastTempX != x || lastTempY != y)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                            }
                        }
                    }
                }
                else
                {
                    return keepGoing;
                }
            }
        }

        return keepGoing;
        //return generatedMoves > 0;
        //return moveStartIndex;
    }

    //This is a ton of arguments

    //Problem: somehow this has overhead >:(
    //This literally just calls 1 method again
    /*
    private static void GenerateRayMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int deltaX, int deltaY, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        //Offset of 0 :P
        GenerateOffsetRayMoves(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, x, y, 0, deltaX, deltaY, specialType, pte, mgie, mbt, moveMetadata, pathTag);
    }
    */



    private static void GenerateRayMovesDual(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int deltaXA, int deltaYA, int deltaXB, int deltaYB, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathtagA, uint pathtagB)
    {
        //I can precompute these in the above loop but that adds more arguments
        bool cylindrical = (pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0;
        bool tubular = (pte.pieceProperty & Piece.PieceProperty.Sneaky) != 0;
        bool reflecter = (pte.pieceProperty & Piece.PieceProperty.Reflecter) != 0;

        int rangeMultiplier = 1;
        bool specialRange = (pte.pieceProperty & Piece.PieceProperty.RangeChange) != 0;

        Move.Dir dirA = Dir.Null;
        Move.Dir dirB = Dir.Null;
        Move.Dir dirC = Dir.Null;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        if (pa == Piece.PieceAlignment.Black && (b.globalData.enemyModifier & Board.EnemyModifier.Knave) != 0)
        {
            tubular = true;
        }

        if ((specialType == SpecialType.ChargeMove || specialType == SpecialType.ChargeMoveReset) && (pte.piecePropertyB & (Piece.PiecePropertyB.ChargeEnhanceStack | Piece.PiecePropertyB.ChargeEnhanceStackReset)) != 0)
        {
            rangeMultiplier = Piece.GetPieceSpecialData(piece);
        }

        dirA = Move.DeltaToDir(deltaXA, deltaYA);
        dirB = Move.DeltaToDir(deltaXB, deltaYB);
        dirC = Move.DeltaToDir(deltaXA + deltaXB, deltaYA + deltaYB);
        if (dirC == Dir.Null)
        {
            dirC = dirA;
        }

        bool blockedA = false;
        bool blockedB = false;

        if (specialRange)
        {
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_MissingPieces) != 0)
            {
                if (pa == Piece.PieceAlignment.Neutral || pa == Piece.PieceAlignment.Crystal)
                {
                    rangeMultiplier = 1;
                }
                else
                {
                    rangeMultiplier = 1 + b.GetMissingPieces(pa == Piece.PieceAlignment.Black);
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = y + 1;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = (8 - y);
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeIncrease_NearRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = 8 - y;
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = y + 1;
                        break;
                }
            }
            if ((pte.pieceProperty & Piece.PieceProperty.RangeDecrease_FurtherRows) != 0)
            {
                switch (pa)
                {
                    case Piece.PieceAlignment.White:
                        rangeMultiplier = (8 - 2 * y);
                        break;
                    case Piece.PieceAlignment.Black:
                        rangeMultiplier = 2 * y - 7;
                        break;
                }
            }
            if (rangeMultiplier < 1)
            {
                rangeMultiplier = 1;
            }
        }

        int tempX = x;
        int tempY = y;

        int tempXA = x;
        int tempYA = y;
        int tempXB = x;
        int tempYB = y;

        int effectiveMaxRange = (mgie.range - 1) + rangeMultiplier;
        if (effectiveMaxRange == 0)
        {
            effectiveMaxRange = 20; //Note: 8 is not enough because crooked rook can take 16 steps from one corner to the other corner
        }

        bool canMove = true;
        int currentRange = 0;
        while (true)
        {
            //iteration part 1: the A side and B side
            if (!blockedA)
            {
                tempXA = tempX + deltaXA;
                tempYA = tempY + deltaYA;
            }
            if (!blockedB)
            {
                tempXB = tempX + deltaXB;
                tempYB = tempY + deltaYB;
            }
            currentRange++;

            switch (mgie.rangeType)
            {
                case RangeType.Exact:
                    canMove = (currentRange == mgie.range);
                    break;
                case RangeType.Minimum:
                    canMove = (currentRange >= mgie.range);
                    break;
                case RangeType.AntiRange:
                    canMove = false;
                    break;
            }

            //Going too far?
            if (currentRange > effectiveMaxRange && mgie.rangeType != RangeType.Minimum)
            {
                break;
            }

            //Out of bounds?
            if (!blockedA)
            {
                if (tempXA < 0 || tempXA > 7)
                {
                    if (!cylindrical && !reflecter)
                    {
                        if (mgie.rangeType == RangeType.AntiRange && !(tempX == x && tempY == y))
                        {
                            (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                        }

                        blockedA = true;
                    }

                    if (cylindrical)
                    {
                        if (tempXA < 0)
                        {
                            tempXA += 8;
                        }
                        else
                        {
                            tempXA -= 8;
                        }
                    }
                    if (reflecter)
                    {
                        if (tempXA < 0)
                        {
                            tempXA = -tempXA;
                            deltaXA = -deltaXA;
                        }
                        else
                        {
                            tempXA = 14 - tempXA;
                            deltaXA = -deltaXA;
                        }
                    }
                }
                if (tempYA < 0 || tempYA > 7)
                {
                    if (!tubular)
                    {
                        if (mgie.rangeType == RangeType.AntiRange && !(tempX == x && tempY == y))
                        {
                            (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                        }

                        blockedA = true;
                    }

                    if (tempYA < 0)
                    {
                        tempYA += 8;
                    }
                    else
                    {
                        tempYA -= 8;
                    }
                    if (specialType != SpecialType.FlyingMoveOnly)
                    {
                        specialType = Move.SpecialType.MoveOnly;
                    }
                }
            }
            if (!blockedB)
            {
                if (tempXB < 0 || tempXB > 7)
                {
                    if (!cylindrical && !reflecter)
                    {
                        if (mgie.rangeType == RangeType.AntiRange && !(tempX == x && tempY == y))
                        {
                            (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                        }

                        blockedB = true;
                    }

                    if (cylindrical)
                    {
                        if (tempXB < 0)
                        {
                            tempXB += 8;
                        }
                        else
                        {
                            tempXB -= 8;
                        }
                    }
                    if (reflecter)
                    {
                        if (tempXB < 0)
                        {
                            tempXB = -tempXB;
                            deltaXB = -deltaXB;
                        }
                        else
                        {
                            tempXB = 14 - tempXB;
                            deltaXB = -deltaXB;
                        }
                    }
                }
                if (tempYB < 0 || tempYB > 7)
                {
                    if (!tubular)
                    {
                        if (mgie.rangeType == RangeType.AntiRange && !(tempX == x && tempY == y))
                        {
                            (_, bool moveGenerated) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                        }

                        blockedB = true;
                    }

                    if (tempYB < 0)
                    {
                        tempYB += 8;
                    }
                    else
                    {
                        tempYB -= 8;
                    }
                    if (specialType != SpecialType.FlyingMoveOnly)
                    {
                        specialType = Move.SpecialType.MoveOnly;
                    }
                }
            }

            if (!blockedA)
            {
                //Is an obstacle in the way?
                (bool keepGoingA, bool wasGeneratedA) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempXA, tempYA, dirA, pa, specialType, pte, mgie, mbt);

                if (moveMetadata != null && (keepGoingA || wasGeneratedA))
                {
                    bool isRider = false;
                    if (deltaXA > 1 || deltaXA < -1 || deltaYA > 1 || deltaYA < -1)
                    {
                        isRider = true;
                    }
                    MoveMetadata md;
                    uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(tempXA), (byte)(tempYA));
                    if (!moveMetadata.ContainsKey(mdKey))
                    {
                        if (isRider)
                        {
                            md = new MoveMetadata(piece, tempXA, tempYA, MoveMetadata.PathType.Leaper, specialType, MoveMetadata.MakePathTag(mgie.atom, pathtagA));
                            if (x != tempX || y != tempY)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                            }
                            moveMetadata.Add(mdKey, md);
                        }
                        else
                        {
                            md = new MoveMetadata(piece, tempXA, tempYA, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, pathtagA));
                            if (x != tempX || y != tempY)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                            }
                            moveMetadata.Add(mdKey, md);
                        }
                    }
                    else
                    {
                        md = moveMetadata[mdKey];
                        md.pathTags.Add(pathtagA);
                        if (x != tempX || y != tempY)
                        {
                            md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                        }
                    }
                }

                if (!keepGoingA)
                {
                    //For antirange: if nothing was generated you have to generate something on the last square
                    //That means you hit an ally piece
                    //No bit table as it would already be set
                    if (mgie.rangeType == RangeType.AntiRange && !wasGeneratedA)
                    {
                        (keepGoingA, wasGeneratedA) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                    }

                    //For flying: change type to flying
                    //No flying check because it would be a real headache to generate those moves correctly with my current setup
                    blockedA = true;
                }
            }
            if (!blockedB)
            {
                //Is an obstacle in the way?
                (bool keepGoingB, bool wasGeneratedB) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempXB, tempYB, dirB, pa, specialType, pte, mgie, null);

                if (moveMetadata != null && (keepGoingB || wasGeneratedB))
                {
                    bool isRider = false;
                    if (deltaXB > 1 || deltaXB < -1 || deltaYB > 1 || deltaYB < -1)
                    {
                        isRider = true;
                    }
                    MoveMetadata md;
                    uint mdKey = Move.PackMove((byte)x, (byte)y, (byte)(tempXB), (byte)(tempYB));
                    if (!moveMetadata.ContainsKey(mdKey))
                    {
                        if (isRider)
                        {
                            md = new MoveMetadata(piece, tempXB, tempYB, MoveMetadata.PathType.Leaper, specialType, MoveMetadata.MakePathTag(mgie.atom, pathtagB));
                            if (x != tempX || y != tempY)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                            }
                            moveMetadata.Add(mdKey, md);
                        }
                        else
                        {
                            md = new MoveMetadata(piece, tempXB, tempYB, MoveMetadata.PathType.Slider, specialType, MoveMetadata.MakePathTag(mgie.atom, pathtagB));
                            if (x != tempX || y != tempY)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                            }
                            moveMetadata.Add(mdKey, md);
                        }
                    }
                    else
                    {
                        md = moveMetadata[mdKey];
                        md.pathTags.Add(pathtagB);
                        if (x != tempX || y != tempY)
                        {
                            md.AddPredecessor(moveMetadata[Move.PackMove(x, y, tempX, tempY)]);
                        }
                    }
                }

                if (!keepGoingB)
                {
                    //For antirange: if nothing was generated you have to generate something on the last square
                    //That means you hit an ally piece
                    if (mgie.rangeType == RangeType.AntiRange && !wasGeneratedB)
                    {
                        (keepGoingB, wasGeneratedB) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirC, pa, specialType, pte, mgie, null);
                    }

                    //For flying: change type to flying
                    //No flying check because it would be a real headache to generate those moves correctly with my current setup
                    blockedB = true;
                }
            }

            //Both sides are blocked so you can't go forward
            if (blockedA && blockedB)
            {
                return;
                //return moveStartIndex;
            }

            tempX += deltaXA;
            tempY += deltaYA;
            tempX += deltaXB;
            tempY += deltaYB;

            //iteration part 2: the C side
            currentRange++;

            switch (mgie.rangeType)
            {
                case RangeType.Exact:
                    canMove = (currentRange == mgie.range);
                    break;
                case RangeType.Minimum:
                    canMove = (currentRange >= mgie.range);
                    break;
                case RangeType.AntiRange:
                    canMove = false;
                    break;
            }

            //Going too far?
            if (currentRange > effectiveMaxRange && mgie.rangeType != RangeType.Minimum)
            {
                break;
            }

            if (tempX < 0 || tempX > 7)
            {
                if (!cylindrical)
                {
                    if (mgie.rangeType == RangeType.AntiRange)
                    {
                        if (!blockedA)
                        {
                            (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXA, tempYA, dirA, pa, specialType, pte, mgie, null);
                            /*
                            if (moveGenerated)
                            {
                                moveStartIndex++;
                            }
                            if (moveStartIndex >= moves.Length)
                            {
                                return moveStartIndex;
                            }
                            */
                        }
                        if (!blockedB)
                        {
                            (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXB, tempYB, dirB, pa, specialType, pte, mgie, null);
                            /*
                            if (moveGenerated)
                            {
                                moveStartIndex++;
                            }
                            if (moveStartIndex >= moves.Length)
                            {
                                return moveStartIndex;
                            }
                            */
                        }
                    }

                    break;
                }

                if (tempX < 0)
                {
                    tempX += 8;
                }
                else
                {
                    tempX -= 8;
                }
            }
            if (tempY < 0 || tempY > 7)
            {
                if (!tubular)
                {
                    if (mgie.rangeType == RangeType.AntiRange)
                    {
                        if (!blockedA)
                        {
                            (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXA, tempYA, dirA, pa, specialType, pte, mgie, null);
                            /*
                            if (moveGenerated)
                            {
                                moveStartIndex++;
                            }
                            if (moveStartIndex >= moves.Length)
                            {
                                return moveStartIndex;
                            }
                            */
                        }
                        if (!blockedB)
                        {
                            (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXB, tempYB, dirB, pa, specialType, pte, mgie, null);
                            /*
                            if (moveGenerated)
                            {
                                moveStartIndex++;
                            }
                            if (moveStartIndex >= moves.Length)
                            {
                                return moveStartIndex;
                            }
                            */
                        }
                    }

                    break;
                }

                if (tempY < 0)
                {
                    tempY += 8;
                }
                else
                {
                    tempY -= 8;
                }
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }

            //Is an obstacle in the way?
            (bool keepGoing, bool wasGenerated) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dirA, pa, specialType, pte, mgie, mbt);

            if (moveMetadata != null && (keepGoing || wasGenerated))
            {
                bool isRider = false;
                if (deltaXB > 1 || deltaXB < -1 || deltaYB > 1 || deltaYB < -1)
                {
                    isRider = true;
                }
                MoveMetadata md;
                uint mdKey = Move.PackMove(x, y, tempX, tempY);
                if (!moveMetadata.ContainsKey(mdKey))
                {
                    if (isRider)
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, new List<uint>());
                        if (!blockedA)
                        {
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagA));
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXA), (byte)(tempYA))]);
                        }
                        if (!blockedB)
                        {
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagB));
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXB), (byte)(tempYB))]);
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                    else
                    {
                        md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Slider, specialType, new List<uint>());
                        if (!blockedA)
                        {
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagA));
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXA), (byte)(tempYA))]);
                        }
                        if (!blockedB)
                        {
                            md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagB));
                            md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXB), (byte)(tempYB))]);
                        }
                        moveMetadata.Add(mdKey, md);
                    }
                }
                else
                {
                    md = moveMetadata[mdKey];
                    if (!blockedA)
                    {
                        md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagA));
                    }
                    if (!blockedB)
                    {
                        md.pathTags.Add(MoveMetadata.MakePathTag(mgie.atom, pathtagB));
                    }
                    if (!blockedA)
                    {
                        md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXA), (byte)(tempYA))]);
                    }
                    if (!blockedB)
                    {
                        md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(tempXB), (byte)(tempYB))]);
                    }
                }
            }

            if (!keepGoing)
            {
                //For antirange: if nothing was generated you have to generate something on the last square
                //That means you hit an ally piece
                if (mgie.rangeType == RangeType.AntiRange && !wasGenerated)
                {
                    if (!blockedA)
                    {
                        (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXA, tempYA, dirA, pa, specialType, pte, mgie, null);
                        /*
                        if (moveGenerated)
                        {
                            moveStartIndex++;
                        }
                        if (moveStartIndex >= moves.Length)
                        {
                            return moveStartIndex;
                        }
                        */
                    }
                    if (!blockedB)
                    {
                        (_, _) = GenerateSquareSingle(moves, true, allowBitboard, ref b, pse, piece, x, y, tempXB, tempYB, dirB, pa, specialType, pte, mgie, null);
                        /*
                        if (moveGenerated)
                        {
                            moveStartIndex++;
                        }
                        if (moveStartIndex >= moves.Length)
                        {
                            return moveStartIndex;
                        }
                        */
                    }
                }

                //For flying: change type to flying
                //No flying check because it would be a real headache to generate those moves correctly with my current setup
                return;
            }
        }
        return;
        //return moveStartIndex;
    }

    private static void GenerateRoseMoves(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int deltaX, int deltaY, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata)
    {
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

        //1, 2
        //2, 1
        //2, -1
        //1, -2
        //-1, -2
        //-2, -1
        //-2, 1
        //-1, 2

        //MoveMetadata.MakePathTag(mgie.atom, deltaX + (deltaY << 3), 0)
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 0, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 0));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 1, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 1));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 2, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 2));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 3, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 3));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 4, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 4));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 5, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 5));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 6, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 6));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 7, true, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 7));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 0, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 8));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 1, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 9));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 2, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 10));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 3, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 11));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 4, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 12));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 5, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 13));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 6, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 14));
        GenerateRoseRay(moves, ref b, allowBitboard, pa, pse, pm, piece, x, y, coordList, 7, false, specialType, pte, mbt, moveMetadata, MoveMetadata.MakePathTag(mgie.atom, 15));
    }
    private static void GenerateRoseRay(List<uint> moves, ref Board b, ulong allowBitboard, Piece.PieceAlignment pa, Piece.PieceStatusEffect pse, Piece.PieceModifier pm, uint piece, int x, int y, int[][] coordList, int index, bool forward, Move.SpecialType specialType, PieceTableEntry pte, MoveBitTable mbt, Dictionary<uint, MoveMetadata> moveMetadata, uint pathTag)
    {
        //I can precompute these in the above loop but that adds more arguments
        bool cylindrical = (pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0;
        bool tubular = (pte.pieceProperty & Piece.PieceProperty.Sneaky) != 0;

        if (specialType == SpecialType.Spawn)
        {
            tubular = false;
        }

        Move.Dir dir = Dir.Null;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);

        if (pa == Piece.PieceAlignment.Black && (b.globalData.enemyModifier & Board.EnemyModifier.Knave) != 0)
        {
            tubular = true;
        }

        int tempX = x;
        int tempY = y;

        int lastTempX = tempX;
        int lastTempY = tempY;

        bool wasGenerated = true;
        bool keepGoing = true;

        bool canMove = true;
        int moveIndex = index;
        while (true)
        {
            lastTempX = tempX;
            lastTempY = tempY;
            //tempX += deltaX;
            //tempY += deltaY;
            tempX += coordList[index][0];
            tempY += coordList[index][1];

            //Debug.Log(tempX + " " + tempY + " " + index + " " + forward + " " + coordList[index][0] + " " + coordList[index][1]);
            if (forward)
            {
                index++;
                if (index >= 8)
                {
                    index -= 8;
                }
            }
            else
            {
                index--;
                if (index < 0)
                {
                    index += 8;
                }
            }

            //Out of bounds?
            //if (tempX < 0 || tempX > 7)
            if ((tempX & -8) != 0)
            {
                if (!cylindrical)
                {
                    return;
                }

                if (cylindrical)
                {
                    if (tempX < 0)
                    {
                        tempX += 8;
                    }
                    else
                    {
                        tempX -= 8;
                    }
                }
            }
            //if (tempY < 0 || tempY > 7)
            if ((tempY & -8) != 0)
            {
                if (!tubular)
                {
                    return;
                }

                if (tempY < 0)
                {
                    tempY += 8;
                }
                else
                {
                    tempY -= 8;
                }
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }

            //Is an obstacle in the way?
            (keepGoing, wasGenerated) = GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, tempX, tempY, dir, pa, specialType, pte, mbt);

            if (moveMetadata != null && (keepGoing || wasGenerated))
            {
                MoveMetadata md;
                uint mdKey = Move.PackMove(x, y, tempX, tempY);
                uint nkey = Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY));
                if (!moveMetadata.ContainsKey(mdKey))
                {
                    md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, pathTag);
                    if ((lastTempX != x || lastTempY != y) && moveMetadata.ContainsKey(nkey))
                    {
                        md.AddPredecessor(moveMetadata[nkey]);
                    }
                    moveMetadata.Add(mdKey, md);
                }
                else
                {
                    md = moveMetadata[mdKey];
                    md.pathTags.Add(pathTag);
                    if ((lastTempX != x || lastTempY != y) && moveMetadata.ContainsKey(nkey))
                    {
                        md.AddPredecessor(moveMetadata[nkey]);
                    }
                }
            }

            if (specialType == SpecialType.LongLeaperCaptureOnly)
            {
                uint obstaclePiece = b.pieces[(tempX) + (tempY << 3)]; //b.GetPieceAtCoordinate(tempX, tempY);
                if ((obstaclePiece == 0 || Piece.GetPieceAlignment(obstaclePiece) == pa))
                {
                    return;
                }
            }

            if (!keepGoing)
            {
                //For flying: change type to flying
                if ((Move.CanFlyOverObstacles(specialType) || pm == Piece.PieceModifier.Winged || ((pte.piecePropertyB & PiecePropertyB.NaturalWinged) != 0) || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SideWings) != 0) && (x < 2 || x > 5))) && specialType != SpecialType.FlyingMoveOnly)
                {
                    if (!Move.CanFlyOverObstacles(specialType))
                    {
                        //No conversion of capture only to move only
                        if (Move.SpecialMoveCantTargetEmpty(specialType))
                        {
                            return;
                        }
                        switch (specialType)
                        {
                            case SpecialType.AllyAbility:
                            case SpecialType.EmptyAbility:
                            case SpecialType.EnemyAbility:
                            case SpecialType.PassiveAbility:
                                break;
                            default:
                                specialType = SpecialType.FlyingMoveOnly;
                                break;
                        }
                    }

                    if (moveMetadata != null && (!wasGenerated))
                    {
                        MoveMetadata md;
                        uint mdKey = Move.PackMove(x, y, tempX, tempY);
                        if (!moveMetadata.ContainsKey(mdKey))
                        {
                            md = new MoveMetadata(piece, tempX, tempY, MoveMetadata.PathType.Leaper, specialType, pathTag);
                            if (lastTempX != x || lastTempY != y)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                            }
                            moveMetadata.Add(mdKey, md);
                        }
                        else
                        {
                            md = moveMetadata[mdKey];
                            md.pathTags.Add(pathTag);
                            if (lastTempX != x || lastTempY != y)
                            {
                                md.AddPredecessor(moveMetadata[Move.PackMove((byte)x, (byte)y, (byte)(lastTempX), (byte)(lastTempY))]);
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

    //false if obstacle
    //this has a ridiculous amount of arguments :P
    //returns (keep going, was a move generated)
    private static (bool, bool) GenerateSquareSingle(List<uint> moves, bool canMove, ulong allowBitboard, ref Board b, Piece.PieceStatusEffect pse, uint piece, int x, int y, int tX, int tY, Dir dir, Piece.PieceAlignment pa, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt)
    {
        bool wasGenerated = false;

        //Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);

        int txy = tX + (tY << 3);

        //canMove &= specialAllowed;
        ulong bitIndexT = 1uL << txy;

        if ((bitIndexT & b.globalData.bitboard_square_hole) != 0)
        {
            return (false, false);
        }

        canMove &= (bitIndexT & allowBitboard) != 0;

        //MainManager.PrintBitboard(b.globalData.bitboard_pieces);
        if ((b.globalData.bitboard_pieces & bitIndexT) != 0)
        {
            uint obstaclePiece = b.pieces[txy];
            PieceTableEntry pteO = b.globalData.GetPieceTableEntryFromCache(txy, obstaclePiece); //GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(obstaclePiece));            

            if (Piece.GetPieceAlignment(obstaclePiece) == pa)
            {
                Piece.PieceModifier pmO = Piece.GetPieceModifier(obstaclePiece);

                //Ally piece
                if ((canMove || mgie.rangeType == RangeType.AntiRange) && pse != Piece.PieceStatusEffect.Bloodlust && specialType != SpecialType.PassiveAbility)
                {
                    if (Move.SpecialMoveCanMoveOntoAlly(specialType, ref b, x, y, tX, tY, dir))
                    {
                        if (moves != null && (mbt == null || !mbt.Get(x, y, bitIndexT)))
                        {
                            moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                            wasGenerated = true;
                        }
                    }
                    else if ((pmO == Piece.PieceModifier.Warped || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.WarpZone) != 0) && (tX >= 2 && tX <= 5 && tY >= 2 && tY <= 5))) && Move.SpecialMoveCanMoveOntoAlly(SpecialType.AllySwap, ref b, x, y, tX, tY, dir))
                    {
                        if (moves != null && (mbt == null || !mbt.Get(x, y, bitIndexT)))
                        {
                            moves.Add(Move.PackMove(x, y, tX, tY, dir, SpecialType.AllySwap));
                            wasGenerated = true;
                        }
                    }
                }                

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    mbt.Set(x, y, bitIndexT);
                }

                //Ally spectral pieces can be ignored :)
                //if (pmO == Piece.PieceModifier.Spectral || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0 || (tY == 2 && pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
                if ((b.globalData.bitboard_noallyblock & bitIndexT) != 0)
                {
                    return (true, wasGenerated);
                }
                else
                {
                    return (false, wasGenerated);
                }
            }
            else
            {
                //Enemy piece: Can capture
                if ((canMove || mgie.rangeType == RangeType.AntiRange) && Move.SpecialMoveCanMoveOntoEnemy(specialType, ref b, tX, tY, dir) && !Piece.IsPieceInvincible(b, obstaclePiece, tX, tY, piece, x, y, specialType, pte, pteO) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    if (moves != null && specialType != SpecialType.PassiveAbility && (mbt == null || !mbt.Get(x, y, bitIndexT)))
                    {
                        moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                        wasGenerated = true;
                    }
                }

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    mbt.Set(x, y, bitIndexT);
                }

                //if (Piece.GetPieceStatusEffect(obstaclePiece) == Piece.PieceStatusEffect.Ghostly || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingEnemy) != 0)
                if ((b.globalData.bitboard_noenemyblock & bitIndexT) != 0)
                {
                    return (true, wasGenerated);
                }
                else
                {
                    return (false, wasGenerated);
                }
            }
        }

        //No obstacle: add the move
        if (canMove)
        {
            if (!Move.SpecialMoveCantTargetEmpty(specialType) && pse != Piece.PieceStatusEffect.Bloodlust)
            {
                if (moves != null && specialType != SpecialType.PassiveAbility && (mbt == null || !mbt.Get(x, y, tX, tY)))
                {
                    moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                    wasGenerated = true;
                }
            }
        }

        //Anti range still causes the attack range to be set
        //Because any enemy piece trying to cross that line will get attacked
        //Note that minimum range and 
        if (mgie.rangeType == RangeType.AntiRange || canMove)
        {
            if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
            {
                mbt.Set(x, y, tX, tY);
            }
        }

        //Rough terrain: don't keep going
        ulong roughTest = b.globalData.bitboard_square_rough;
        switch (pa)
        {
            case PieceAlignment.White:
                roughTest = b.globalData.bitboard_roughBlack;
                break;
            case PieceAlignment.Black:
                roughTest = b.globalData.bitboard_roughWhite;
                break;
        }

        if ((pte.pieceProperty & Piece.PieceProperty.NoTerrain) == 0 && (((bitIndexT) & roughTest) != 0))
        {
            return (false, wasGenerated);
        }

        return (true, wasGenerated);
    }

    private static (bool, bool) GenerateSquareSingle(List<uint> moves, bool canMove, ulong allowBitboard, ref Board b, Piece.PieceStatusEffect pse, uint piece, int x, int y, int tX, int tY, Dir dir, Piece.PieceAlignment pa, Move.SpecialType specialType, PieceTableEntry pte, MoveBitTable mbt)
    {
        bool wasGenerated = false;
        //Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);

        int txy = (tX + (tY << 3));

        ulong bitIndexT = 1uL << txy;

        if ((bitIndexT & b.globalData.bitboard_square_hole) != 0)
        {
            return (false, false);
        }

        //canMove &= specialAllowed;
        canMove &= (bitIndexT & allowBitboard) != 0;

        if ((b.globalData.bitboard_pieces & bitIndexT) != 0)
        {
            uint obstaclePiece = b.pieces[txy];
            PieceTableEntry pteO = b.globalData.GetPieceTableEntryFromCache(txy, obstaclePiece); // GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(obstaclePiece));

            if (Piece.GetPieceAlignment(obstaclePiece) == pa)
            {
                //Ally piece
                Piece.PieceModifier opm = Piece.GetPieceModifier(obstaclePiece);
                if ((canMove) && pse != Piece.PieceStatusEffect.Bloodlust)
                {
                    if (Move.SpecialMoveCanMoveOntoAlly(specialType, ref b, x, y, tX, tY, dir))
                    {
                        if (moves != null && (mbt == null || !mbt.Get(x, y, tX, tY)))
                        {
                            moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                            wasGenerated = true;
                        }
                    }
                    else if ((opm == Piece.PieceModifier.Warped || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.WarpZone) != 0) && (tX >= 2 && tX <= 5 && tY >= 2 && tY <= 5))) && Move.SpecialMoveCanMoveOntoAlly(SpecialType.AllySwap, ref b, x, y, tX, tY, dir))
                    {
                        if (moves != null && (mbt == null || !mbt.Get(x, y, tX, tY)))
                        {
                            moves.Add(Move.PackMove(x, y, tX, tY, dir, SpecialType.AllySwap));
                            wasGenerated = true;
                        }
                    }


                    /*
                    moves[moveStartIndex] = Move.PackMove(x, y, tX, tY, dir, specialType);
                    moveStartIndex++;
                    wasGenerated = true;
                    */
                }

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    mbt.Set(x, y, tX, tY);
                }

                //Ally spectral pieces can be ignored :)
                //if (opm == Piece.PieceModifier.Spectral || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0 || (tY == 2 && pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
                if ((b.globalData.bitboard_noallyblock & bitIndexT) != 0)
                {
                    return (true, wasGenerated);
                }
                else
                {
                    return (false, wasGenerated);
                }
            }
            else
            {
                //Enemy piece: Can capture
                if ((canMove) && Move.SpecialMoveCanMoveOntoEnemy(specialType, ref b, tX, tY, dir) && !Piece.IsPieceInvincible(b, obstaclePiece, tX, tY, piece, x, y, specialType, pte, pteO) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    if (moves != null && (mbt == null || !mbt.Get(x, y, tX, tY)))
                    {
                        moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                        wasGenerated = true;
                    }
                    /*
                    moves[moveStartIndex] = Move.PackMove(x, y, tX, tY, dir, specialType);
                    moveStartIndex++;
                    wasGenerated = true;
                    */
                }

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
                {
                    mbt.Set(x, y, tX, tY);
                }

                //if (Piece.GetPieceStatusEffect(obstaclePiece) == Piece.PieceStatusEffect.Ghostly || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0)
                if ((b.globalData.bitboard_noenemyblock & bitIndexT) != 0)
                {
                    return (true, wasGenerated);
                }
                else
                {
                    return (false, wasGenerated);
                }
            }
        }

        //No obstacle: add the move
        if (canMove)
        {
            if (!Move.SpecialMoveCantTargetEmpty(specialType) && pse != Piece.PieceStatusEffect.Bloodlust)
            {
                if (moves != null && specialType != SpecialType.PassiveAbility && (mbt == null || !mbt.Get(x, y, tX, tY)))
                {
                    moves.Add(Move.PackMove(x, y, tX, tY, dir, specialType));
                    wasGenerated = true;
                }
                /*
                moves[moveStartIndex] = Move.PackMove(x, y, tX, tY, dir, specialType);
                moveStartIndex++;
                wasGenerated = true;
                */
            }

            //Anti range still causes the attack range to be set
            //Because any enemy piece trying to cross that line will get attacked

            if (mbt != null && Move.SpecialMoveCaptureLike(specialType) && pse != Piece.PieceStatusEffect.Soaked)
            {
                mbt.Set(x, y, tX, tY);
            }
        }

        //Rough terrain: don't keep going
        ulong roughTest = b.globalData.bitboard_square_rough;
        switch (pa)
        {
            case PieceAlignment.White:
                roughTest = b.globalData.bitboard_roughBlack;
                break;
            case PieceAlignment.Black:
                roughTest = b.globalData.bitboard_roughWhite;
                break;
        }

        if ((pte.pieceProperty & Piece.PieceProperty.NoTerrain) == 0 && ((bitIndexT & roughTest) != 0))
        {
            return (false, wasGenerated);
        }

        return (true, wasGenerated);
    }

    private static ulong GetAllowedSquares(ref Board b, uint piece, int x, int y, Piece.PieceAlignment pa, Piece.PieceModifier pm, Move.SpecialType specialType, PieceTableEntry pte)
    {
        ulong bitboard = BITBOARD_PATTERN_FULL;

        ulong allyAdjacentBitboard = 0;
        ulong enemyAdjacentBitboard = 0;
        ulong enemyBitboard = 0;
        //ulong bitindexT = 1uL << tX + (tY << 3);

        //Unfortunately I have to check these per square
        ulong bansheeBitboard = 0;
        ulong attractorBitboard = 0;
        ulong repulserBitboard = 0;
        ulong slothBitboard = 0;
        ulong watchtowerBitboard = 0;

        ulong immuneBitboard = 0;

        switch (pa)
        {
            case PieceAlignment.White:
                allyAdjacentBitboard = b.globalData.bitboard_piecesWhiteAdjacent;
                enemyAdjacentBitboard = b.globalData.bitboard_piecesBlackAdjacent & ~b.globalData.bitboard_piecesBlack;
                enemyBitboard = b.globalData.bitboard_piecesBlack;

                immuneBitboard = b.globalData.bitboard_immuneWhite;

                bansheeBitboard = b.globalData.bitboard_bansheeBlack & ~immuneBitboard;
                attractorBitboard = b.globalData.bitboard_attractorBlack & ~immuneBitboard;
                repulserBitboard = b.globalData.bitboard_repulserBlack & ~immuneBitboard;
                slothBitboard = b.globalData.bitboard_slothBlack & ~immuneBitboard;
                watchtowerBitboard = b.globalData.bitboard_watchTowerBlack & ~immuneBitboard;

                if ((b.globalData.enemyModifier & Board.EnemyModifier.Blinking) != 0)
                {
                    int tX = Move.GetToX(b.whitePerPlayerInfo.lastMove);
                    int tY = Move.GetToY(b.whitePerPlayerInfo.lastMove);

                    if (((tX + tY) & 1) == 0)   //= was on black
                    {
                        bitboard &= BITBOARD_PATTERN_WHITESQUARES;
                    }
                    else
                    {
                        bitboard &= ~BITBOARD_PATTERN_WHITESQUARES;
                    }
                }
                if ((b.globalData.enemyModifier & Board.EnemyModifier.Terror) != 0)
                {
                    bansheeBitboard |= MainManager.SmearBitboard(MainManager.SmearBitboard(b.globalData.bitboard_kingBlack));
                    //MainManager.PrintBitboard(b.globalData.bitboard_kingBlack);
                    //MainManager.PrintBitboard(MainManager.SmearBitboard(b.globalData.bitboard_kingBlack));
                    //MainManager.PrintBitboard(MainManager.SmearBitboard(MainManager.SmearBitboard(b.globalData.bitboard_kingBlack)));
                    //MainManager.PrintBitboard(bansheeBitboard);
                }
                break;
            case PieceAlignment.Black:
                allyAdjacentBitboard = b.globalData.bitboard_piecesBlackAdjacent;
                enemyAdjacentBitboard = b.globalData.bitboard_piecesWhiteAdjacent & ~b.globalData.bitboard_piecesWhite;
                enemyBitboard = b.globalData.bitboard_piecesWhite;

                immuneBitboard = b.globalData.bitboard_immuneBlack;

                bansheeBitboard = b.globalData.bitboard_bansheeWhite & ~immuneBitboard;
                attractorBitboard = b.globalData.bitboard_attractorWhite & ~immuneBitboard;
                repulserBitboard = b.globalData.bitboard_repulserWhite & ~immuneBitboard;
                slothBitboard = b.globalData.bitboard_slothWhite & ~immuneBitboard;
                watchtowerBitboard = b.globalData.bitboard_watchTowerWhite & ~immuneBitboard;
                break;
        }

        ulong bitindex = 1uL << x + (y << 3);

        if ((pte.pieceProperty & Piece.PieceProperty.EnchantImmune) == 0 && pm != PieceModifier.Immune)
        {
            bitboard &= (~bansheeBitboard | enemyBitboard);

            if ((bitindex & attractorBitboard) != 0)
            {
                switch (pa)
                {
                    case PieceAlignment.White:
                        //delete tY <= y

                        //only tY > y

                        //No forward
                        if (y == 7)
                        {
                            return 0;
                        }
                        else
                        {
                            bitboard &= BITBOARD_PATTERN_FULL << (8 + (y << 3));
                        }
                        break;
                    case PieceAlignment.Black:
                        //delete tY >= y

                        //No back
                        if (y == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            bitboard &= BITBOARD_PATTERN_FULL >> (64 - (y << 3));
                        }
                        break;
                }
            }
            if ((bitindex & repulserBitboard) != 0)
            {
                switch (pa)
                {
                    case PieceAlignment.White:
                        //tY >= y

                        //No back
                        if (y == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            bitboard &= BITBOARD_PATTERN_FULL >> (64 - (y << 3));
                        }
                        break;
                    case PieceAlignment.Black:
                        //tY <= y

                        //No forward
                        if (y == 7)
                        {
                            return 0;
                        }
                        else
                        {
                            bitboard &= BITBOARD_PATTERN_FULL << (8 + (y << 3));
                        }
                        break;
                }
            }
            if ((bitindex & slothBitboard) != 0)
            {
                bitboard &= MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, (x + (y << 3)), -1, -1);
            }
            //MainManager.PrintBitboard(watchtowerBitboard);
            /*
            if ((bitindexT & watchtowerBitboard) != 0)
            {
                bitboard &= MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, (x + (y << 3)), -1, -1);
            }
            */
            //watchtower logic is a bit different
            //mini optimization?
            //might be against my rules (don't make optimizations that rely on a piece not existing) but ehh?
            if (watchtowerBitboard != 0)
            {
                bitboard &= (~watchtowerBitboard) | (MainManager.ShiftBitboardPattern(BITBOARD_PATTERN_ADJACENT, (x + (y << 3)), -1, -1));
            }
        }

        switch (pte.type)
        {
            case Piece.PieceType.EliteMilitia:
            case Piece.PieceType.Militia:
                if (pa == Piece.PieceAlignment.White)// && (bitindexT & MoveGeneratorInfoEntry.BITBOARD_PATTERN_HALFBOARD) == 0)
                {
                    bitboard &= MoveGenerator.BITBOARD_PATTERN_HALFBOARD;
                }
                if (pa == Piece.PieceAlignment.Black)// && (bitindexT & MoveGeneratorInfoEntry.BITBOARD_PATTERN_HALFBOARD) != 0)
                {
                    bitboard &= ~MoveGenerator.BITBOARD_PATTERN_HALFBOARD;
                }
                break;
            case Piece.PieceType.EdgeRook:
                /*
                if ((bitindexT & MoveGeneratorInfoEntry.BITBOARD_PATTERN_NOCENTER) == 0)
                {
                    canMove = false;
                }
                */
                bitboard &= MoveGenerator.BITBOARD_PATTERN_NOCENTER;
                break;
            case Piece.PieceType.CornerlessBishop:
                /*
                if ((bitindexT & MoveGeneratorInfoEntry.BITBOARD_PATTERN_NOCORNERS) == 0)
                {
                    canMove = false;
                }
                */
                bitboard &= MoveGenerator.BITBOARD_PATTERN_NOCORNERS;
                break;
            case Piece.PieceType.CenterQueen:
                /*
                if ((bitindexT & MoveGeneratorInfoEntry.BITBOARD_PATTERN_MIDDLEFILES) == 0)
                {
                    canMove = false;
                }
                */
                bitboard &= MoveGenerator.BITBOARD_PATTERN_MIDDLEFILES;
                break;
        }

        switch (specialType)
        {
            case SpecialType.SlipMove:
                //specialAllowed = (bitindexT & enemyBitboard) != 0;
                bitboard &= enemyAdjacentBitboard;
                break;
            case SpecialType.GliderMove:
                //specialAllowed = (bitindexT & enemyBitboard) == 0;
                bitboard &= ~enemyAdjacentBitboard;
                break;
            case SpecialType.CoastMove:
                //specialAllowed = (bitindexT & BITBOARD_PATTERN_EDGES) != 0;
                bitboard &= BITBOARD_PATTERN_OUTEREDGES;
                break;
            case SpecialType.PlantMove:
            case SpecialType.DepositAllyPlantMove:
                //specialAllowed = (bitindexT & allyBitboard) != 0;
                bitboard &= allyAdjacentBitboard;
                break;
            case SpecialType.ShadowMove:
                //specialAllowed = (bitindexT & b.globalData.bitboard_piecesMirrored) != 0;
                //MainManager.PrintBitboard(b.globalData.bitboard_piecesMirrored);
                bitboard &= b.globalData.bitboard_piecesMirrored;
                break;
        }

        return bitboard;
    }

    //keep going, can generate
    private static (bool, bool) GiantCheckGenerateSquareSingle(ref Board b, ulong allowBitboard, uint piece, int x, int y, int tX, int tY, Dir dir, Piece.PieceAlignment pa, Move.SpecialType specialType, PieceTableEntry pte, MoveBitTable mbt)
    {
        //if (tX > 7 || tX < 0 || tY > 7 || tY < 0)
        if (((tX | tY) & -8) != 0)
        {
            return (false, false);
        }

        //Forbid null moves
        if (tX == x && tY == y)
        {
            return (false, false);
        }

        int txy = (tX + (tY << 3));

        bool canMove = true;
        bool canGenerate = false;
        //Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);

        //canMove &= specialAllowed;
        ulong bitIndexT = 1uL << txy;
        //bitIndexT |= 1uL << (tX + 1) + tY * 8;
        //bitIndexT |= 1uL << (tX) + (tY + 1) * 8;
        //bitIndexT |= 1uL << (tX + 1) + (tY + 1) * 8;
        bitIndexT |= bitIndexT << 1;
        bitIndexT |= bitIndexT << 8;

        //No overlap with any part of hole
        if ((bitIndexT & b.globalData.bitboard_square_hole) != 0)
        {
            return (false, false);
        }

        canMove &= (bitIndexT & allowBitboard) != 0;

        //if ((b.globalData.bitboard_pieces & (1uL << txy)) != 0)
        if (b.pieces[txy] != 0)
        {
            uint obstaclePiece = b.pieces[txy];
            PieceTableEntry pteO = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(obstaclePiece));

            if (Piece.GetPieceAlignment(obstaclePiece) == pa)
            {
                //Ally piece
                if ((canMove) && Move.SpecialMoveCanMoveOntoAlly(specialType, ref b, x, y, tX, tY, dir))
                {
                    canGenerate = true;
                }

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType))
                {
                    mbt.Set(x, y, tX, tY);
                }

                //Ally spectral pieces can be ignored :)
                //if (Piece.GetPieceModifier(obstaclePiece) == Piece.PieceModifier.Spectral || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0 || (tY == 2 && pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.SpectralWall) != 0)))
                if ((b.globalData.bitboard_noallyblock & bitIndexT) != 0)
                {
                    return (true, canGenerate);
                }
                else
                {
                    return (false, canGenerate);
                }
            }
            else
            {
                //Enemy piece: Can capture
                if ((canMove) && Move.SpecialMoveCanMoveOntoEnemy(specialType, ref b, tX, tY, dir) && !Piece.IsPieceInvincible(b, obstaclePiece, tX, tY, piece, x, y, specialType, pte, pteO))
                {
                    canGenerate = true;
                }

                //no matter what it gets set anyway
                if (mbt != null && Move.SpecialMoveCaptureLike(specialType))
                {
                    mbt.Set(x, y, tX, tY);
                }

                //if (Piece.GetPieceStatusEffect(obstaclePiece) == Piece.PieceStatusEffect.Ghostly || (pteO.piecePropertyB & Piece.PiecePropertyB.NonBlockingAlly) != 0)
                if ((b.globalData.bitboard_noenemyblock & bitIndexT) != 0)
                {
                    return (true, canGenerate);
                }
                else
                {
                    return (false, canGenerate);
                }
            }
        }

        //No obstacle: add the move
        if (canMove)
        {
            if (!Move.SpecialMoveCantTargetEmpty(specialType))
            {
                canGenerate = true;
            }

            //Anti range still causes the attack range to be set
            //Because any enemy piece trying to cross that line will get attacked
            if (mbt != null && Move.SpecialMoveCaptureLike(specialType))
            {
                mbt.Set(x, y, tX, tY);
            }
        }

        //Rough terrain: don't keep going
        ulong roughTest = b.globalData.bitboard_square_rough;
        switch (pa)
        {
            case PieceAlignment.White:
                roughTest = b.globalData.bitboard_roughBlack;
                break;
            case PieceAlignment.Black:
                roughTest = b.globalData.bitboard_roughWhite;
                break;
        }

        if ((pte.pieceProperty & Piece.PieceProperty.NoTerrain) == 0 && ((bitIndexT) & roughTest) != 0)
        {
            return (false, canGenerate);
        }

        return (true, canGenerate);
    }

    //keep going, wasGenerated
    private static (bool, bool) TryGenerateSquareSingle(List<uint> moves, bool canMove, ulong allowBitboard, ref Board b, Piece.PieceStatusEffect pse, uint piece, int x, int y, int targetX, int targetY, Dir dir, Piece.PieceAlignment pa, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt)
    {
        //if (targetX > 7 || targetX < 0 || targetY > 7 || targetY < 0)
        //2 operations and 1 condition is probably better than 4 conditions
        if (((targetX | targetY) & -8) != 0)
        {
            return (false, false);
        }

        //Forbid null moves
        if (targetX == x && targetY == y)
        {
            return (false, false);
        }

        return GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, dir, pa, specialType, pte, mgie, mbt);
    }
    private static (bool, bool) TryGenerateSquareSingle(List<uint> moves, bool canMove, ulong allowBitboard, ref Board b, Piece.PieceStatusEffect pse, uint piece, int x, int y, int targetX, int targetY, Piece.PieceAlignment pa, Move.SpecialType specialType, PieceTableEntry pte, MoveGeneratorInfoEntry mgie, MoveBitTable mbt)
    {
        bool cylindrical = (pte.pieceProperty & Piece.PieceProperty.Cylindrical) != 0;
        bool sneaky = (pte.pieceProperty & Piece.PieceProperty.Sneaky) != 0;
        bool reflecter = (pte.pieceProperty & Piece.PieceProperty.Reflecter) != 0;

        if ((!cylindrical && !reflecter) && ((targetX & -8) != 0))
        {
            return (false, false);
        }
        if (!sneaky && ((targetY & -8) != 0))
        {
            return (false, false);
        }

        if (cylindrical)
        {
            if (targetX < 0)
            {
                targetX = 8 + targetX;
            }
            if (targetX > 7)
            {
                targetX = targetX - 8;
            }
        }
        if (reflecter)
        {
            if (targetX < 0)
            {
                targetX = -targetX;
            }
            if (targetX > 7)
            {
                targetX = 7 - targetX;
            }
        }
        if (sneaky)
        {
            if (targetY < 0)
            {
                targetY = 8 + targetY;
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }
            if (targetY > 7)
            {
                targetY = targetY - 8;
                if (specialType != SpecialType.FlyingMoveOnly)
                {
                    specialType = Move.SpecialType.MoveOnly;
                }
            }
        }

        //Forbid null moves
        if (targetX == x && targetY == y)
        {
            return (false, false);
        }

        Dir dir = DeltaToDir(targetX - x, targetY - y);

        return GenerateSquareSingle(moves, canMove, allowBitboard, ref b, pse, piece, x, y, targetX, targetY, dir, pa, specialType, pte, mgie, mbt);
    }
}