using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static Move;
using static Piece;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.GraphicsBuffer;

//Data that remains static over the course of the game
//Being space efficient with this is not very important?
[Serializable]
public class BoardGlobalData
{
    //Board layout (special squares)
    public Square[] squares;

    public Board.PlayerModifier playerModifier;
    public Board.EnemyModifier enemyModifier;

    public BoardGlobalPlayerInfo whitePerPlayerInfo;
    public BoardGlobalPlayerInfo blackPerPlayerInfo;

    public bool noLastMoveHash;

    //scratch data
    //to not have to allocate a bunch of move bit tables and creating a lot of garbage
    //Move generation populates these globally

    public int whiteHighestValuedPieceValue;
    public Piece.PieceType whiteHighestValuePiece;
    public int blackHighestValuedPieceValue;
    public Piece.PieceType blackHighestValuePiece;

    public MoveBitTable mbtpassive;
    public MoveBitTable mbtpassiveInverse;
    public MoveBitTable mbtactive;
    public MoveBitTable mbtactiveInverse;

    //square bitboards for easier updates
    public ulong bitboard_square_hole;      //Hole deletes stuff on it
    public ulong bitboard_square_fire;
    public ulong bitboard_square_windUp;
    public ulong bitboard_square_windDown;
    public ulong bitboard_square_windLeft;
    public ulong bitboard_square_windRight;
    public ulong bitboard_square_bright;
    public ulong bitboard_square_promotion;
    public ulong bitboard_square_cursed;

    public ulong bitboard_crystalWhite;
    public ulong bitboard_crystalBlack;

    public ulong bitboard_pieces;
    public ulong bitboard_piecesWhite;
    public ulong bitboard_piecesBlack;
    public ulong bitboard_piecesNeutral;
    public ulong bitboard_piecesCrystal;

    public ulong bitboard_piecesWhiteAdjacent;
    public ulong bitboard_piecesBlackAdjacent;

    public ulong bitboard_pawnsWhite;
    public ulong bitboard_pawnsBlack;

    public ulong bitboard_kingWhite;
    public ulong bitboard_kingBlack;

    //A lot of pieces have rough square auras or water auras
    public ulong bitboard_roughWhite;
    public ulong bitboard_roughBlack;
    public ulong bitboard_waterWhite;
    public ulong bitboard_waterBlack;

    //relay immunity to allies (or pieces with natural immunity)
    //These bitboards negate the other bitboards
    //I'm going to make relay immune just a basic adjacency because relay to protected would be slow to code
    public ulong bitboard_immuneRelayerWhite;
    public ulong bitboard_immuneRelayerBlack;
    public ulong bitboard_immuneWhite;
    public ulong bitboard_immuneBlack;

    //A bunch of piece specific bitboards :/
    //Well at least holding all of these in memory is faster than manually checking proximity every time
    public ulong bitboard_bansheeWhite;
    public ulong bitboard_bansheeBlack;
    public ulong bitboard_attractorWhite;
    public ulong bitboard_attractorBlack;
    public ulong bitboard_immobilizerWhite;
    public ulong bitboard_immobilizerBlack;
    public ulong bitboard_repulserWhite;
    public ulong bitboard_repulserBlack;
    public ulong bitboard_harpyWhite;
    public ulong bitboard_harpyBlack;
    public ulong bitboard_hagWhite;
    public ulong bitboard_hagBlack;
    public ulong bitboard_slothWhite;
    public ulong bitboard_slothBlack;
    public ulong bitboard_hangedWhite;
    public ulong bitboard_hangedBlack;
    public ulong bitboard_virgoWhite;
    public ulong bitboard_virgoBlack;

    //Piece placements
    public ulong bitboard_tarotMoonWhite;
    public ulong bitboard_tarotMoonBlack;
    public ulong bitboard_tarotMoonIllusionWhite;
    public ulong bitboard_tarotMoonIllusionBlack;
    public ulong bitboard_zombieWhite;
    public ulong bitboard_zombieBlack;
    public ulong bitboard_abominationWhite;
    public ulong bitboard_abominationBlack;
    public ulong bitboard_clockworksnapperWhite;
    public ulong bitboard_clockworksnapperBlack;
    public ulong bitboard_bladebeastWhite;
    public ulong bitboard_bladebeastBlack;

    public BoardGlobalData()
    {
        squares = new Square[64];
    }
}

[Serializable]
public struct BoardGlobalPlayerInfo
{
    public short startPieceValueSumX2;
    public byte startPieceCount;
}
[Serializable]

//this handily fits in the 8 bytes a struct is sized at minimum?
public struct BoardPlayerInfo
{
    public bool canCastle;

    public uint lastMove;  //= -1 if null move?
    public uint lastPieceMoved;                     //Note: lastPieceMovedType is a relic of the old time where I didn't have the full data set (Potentially still saves a tiny bit of time to have 2 separate variables?)
    public Piece.PieceType lastPieceMovedType;
    public int lastPieceMovedLocation;     //
    public short pieceValueSumX2;
    public byte pieceCount;
    public bool capturedLastTurn;

    public BoardPlayerInfo(uint[] pieces, Piece.PieceAlignment pa)
    {
        canCastle = true;
        lastMove = 0;
        pieceCount = 0;
        capturedLastTurn = false;
        pieceValueSumX2 = 0;
        lastPieceMovedType = Piece.PieceType.Null;
        lastPieceMoved = 0;
        lastPieceMovedLocation = -1;

        for (int i = 0; i < pieces.Length; i++)
        {
            Piece.PieceType pt = Piece.GetPieceType(pieces[i]);
            Piece.PieceAlignment ppa = Piece.GetPieceAlignment(pieces[i]);

            if ((short)(pt) > 0 && pa == ppa)
            {
                //Increment piece count and value sum
                pieceCount++;

                PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(pt);
                pieceValueSumX2 += pte.pieceValueX2;
            }
        }
    }
}

[System.Serializable]
//public struct Board
public class Board
{
    public bool blackToMove;
    public int ply;
    public int bonusPly;    //bonus turn tracker (reset to 0 when the side to move flips back)
    public int turn;

    public BoardGlobalData globalData;

    //I want this not to go on the heap
    public uint[] pieces;
    public BoardPlayerInfo whitePerPlayerInfo;
    public BoardPlayerInfo blackPerPlayerInfo;

    public enum BoardPreset
    {
        Normal,
        PerfectModifierTest,    //Perfect chess + modifiers
        BishopArmy,
        KnightArmy,
        RookArmy,
        BirdArmy,
        TripunchArmy,
        CrystalTest,
        WeirdPieces,
        Lycanthropes,
        Fiery,
        PushAndPull,
        Royalists,
        WarPieces,
        SwitchPieces,
        EndgameTest,

        RelayPieces,
        TerrainTest,
        GiantTest
    }

    //basically run difficulty things?
    //These can be added together?
    [Flags]
    public enum PlayerModifier : uint
    {
        None = 0,
        NoKing = 1,
        InfiniteCastling = 1 << 1,
        CharityKing = 1 << 2,
        Slippery = 1 << 3,
        Push = 1 << 4,
        Vortex = 1 << 5,

    }
    //These can be added together?
    [Flags]
    public enum EnemyModifier : uint
    {
        None = 0,
        NoKing = 1,

        Blinking = 1u << 1,    //Pieces you move must alternate starting on black and white squares
        Complacent = 1u << 2,  //Can't capture 2 turns in a row
        Defensive = 1u << 3,   //All black pieces get infinite backwards Rook / Bishop movement (move only)
        Envious = 1u << 4, //Spawns 4 Envies on the black side
        Fusion = 1u << 5,  //King has the normal movement of all ally pieces
        Greedy = 1u << 6,  //First X captures the enemy gets Convert instead of capture
        Hidden = NoKing,    //No king
        Isolated = 1u << 7, //Pieces with enemies and no allies next to them can't move
        Jester = 1u << 8, //Spawn 4 Jesters on the black side
        Knave = 1u << 9, //All enemy pieces are Sneaky (can cross bottom to top without capturing)
        Lustful = 1u << 10, //King acts as Hypnotist without being blockable
        Mesmerizing = 1u << 11, //Pieces white moves get pulled upwards 1
        Numerous = 1u << 12, //Spawn 3 more kings
        Obelisk = 1u << 13, //King is a tandem mover
        Prideful = 1u << 14, //You can't capture if you have more pieces than your enemy
        Queenly = 1u << 15, //Spawn 1 Queen and 3 Princesses
        Rifter = 1u << 16, //Pieces you move are pulled towards the sides of the board
        Slothful = 1u << 17, //Immobilize everything in the black king file
        Terror = 1u << 18, //Can't move within 2 of the black king except to capture
        Unpredictable = 1u << 19, //Can't move same piece twice in a row
        Voracious = 1u << 20, //For every 2 white pieces lost, the enemy king gains 1 range
        Wrathful = 1u << 21, //The next 2 pieces White captures with are destroyed on capture
        Xyloid = 1u << 22, //King can move only teleport to any ally adjacent squares
        Youthful = 1u << 23, //Black can double move for the first 5 turns (Can't check with the first move but that is more an implementation quirk as Black can't move such that Black can king capture on the second move)
        Zenith = 1u << 24, //When king is captured, it replaces the highest position index ally piece (note that position index is y*8 + x)
    }

    public void Init()
    {
        blackToMove = false;
        pieces = new uint[64];
        globalData = new BoardGlobalData();

        whitePerPlayerInfo.canCastle = true;
        blackPerPlayerInfo.canCastle = true;
    }

    public void Setup(BoardPreset bp = BoardPreset.Normal)
    {
        switch (bp)
        {
            default:
            case BoardPreset.Normal:
                SetupNormal();
                break;
            case BoardPreset.PerfectModifierTest:
                SetupPerfectModifierTest();
                break;
            case BoardPreset.BishopArmy:
                SetupBishopArmy();
                break;
            case BoardPreset.KnightArmy:
                SetupKnightArmy();
                break;
            case BoardPreset.RookArmy:
                SetupRookArmy();
                break;
            case BoardPreset.BirdArmy:
                SetupBirdArmy();
                break;
            case BoardPreset.TripunchArmy:
                SetupTripunchArmy();
                break;
            case BoardPreset.CrystalTest:
                SetupCrystalTest();
                break;
            case BoardPreset.WeirdPieces:
                SetupWeirdArmy();
                break;
            case BoardPreset.Lycanthropes:
                SetupLycanthropeArmy();
                break;
            case BoardPreset.Fiery:
                SetupFieryArmy();
                break;
            case BoardPreset.PushAndPull:
                SetupPushPullArmy();
                break;
            case BoardPreset.Royalists:
                SetupRoyalistArmy();
                break;
            case BoardPreset.WarPieces:
                SetupWarArmy();
                break;
            case BoardPreset.SwitchPieces:
                SetupSwitchArmy();
                break;
            case BoardPreset.EndgameTest:
                SetupEndgameTest();
                break;
            case BoardPreset.TerrainTest:
                SetupTerrainTest();
                break;
            case BoardPreset.RelayPieces:
                SetupRelayArmy();
                break;
            case BoardPreset.GiantTest:
                SetupGiantArmy();
                break;
                //SetupGiantArmy();
        }
    }
    public void Setup(Piece.PieceType[] army, EnemyModifier em)
    {
        Init();

        Piece.PieceType[] ptList = army;

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            
            if (indexToRead >= ptList.Length)
            {
                continue;
            }

            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);

            if ((GlobalPieceManager.Instance.GetPieceTableEntry(pt).pieceProperty & PieceProperty.Giant) != 0)
            {
                //Based on how the giants are oriented to get the giants to appear the same way on both sides the bottom right corner becomes the top right
                //So on the black side the bottom corner is down 1
                if (i > 32)
                {
                    PlaceGiant(pieces[i], i & 7, (i >> 3) - 1);
                }
                else
                {
                    PlaceGiant(pieces[i], i & 7, i >> 3);
                }
            }
        }

        globalData.enemyModifier = em;

        PostSetupInit();
    }

    public void SetupNormal()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook,
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }

    public void SetupPerfectModifierTest()
    {
        Init();

        //lazy ish idea
        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Marshall, PieceType.Cardinal, PieceType.Queen, PieceType.Amazon, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook,
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        /*
        new Piece.PieceType[]
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook,
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };
         */

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;

            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }

            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            Piece.PieceModifier modifier = PieceModifier.None;

            switch (i % 8)
            {
                case 0:
                    modifier = PieceModifier.Vengeful;
                    break;
                case 1:
                    modifier = PieceModifier.Phoenix;
                    break;
                case 2:
                    modifier = PieceModifier.Golden;
                    break;
                case 3:
                    modifier = PieceModifier.Winged;
                    break;
                case 4:
                    modifier = PieceModifier.Spectral;
                    break;
                case 5:
                    modifier = PieceModifier.Immune;
                    break;
                case 6:
                    modifier = PieceModifier.Warped;
                    break;
                case 7:
                    modifier = PieceModifier.Shielded;
                    break;
            }

            pieces[i] = Piece.PackPieceData(pt, modifier, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        //pieces[32] = Piece.PackPieceData(PieceType.Rook, PieceAlignment.Crystal);
        //pieces[24] = Piece.PackPieceData(PieceType.Rook, PieceAlignment.Crystal);
        //pieces[40] = Piece.PackPieceData(PieceType.Rook, PieceAlignment.Neutral);
        PostSetupInit();
    }
    public void SetupBishopArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Archbishop, PieceType.Missionary, PieceType.Inquisitor, PieceType.Cardinal, PieceType.King, PieceType.Inquisitor, PieceType.Missionary, PieceType.Archbishop,
            PieceType.Priest, PieceType.Priest, PieceType.Priest, PieceType.Priest, PieceType.Priest, PieceType.Priest, PieceType.Priest, PieceType.Priest,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }

    public void SetupKnightArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Cataphract, PieceType.Lancer, PieceType.Dragoon, PieceType.Marshall, PieceType.King, PieceType.Dragoon, PieceType.Lancer, PieceType.Cataphract,
            PieceType.Squire, PieceType.Squire, PieceType.Squire, PieceType.Squire, PieceType.Squire, PieceType.Squire, PieceType.Squire, PieceType.Squire,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupRookArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Trebuchet, PieceType.Turret, PieceType.Rammer, PieceType.Fortress, PieceType.King, PieceType.Rammer, PieceType.Turret, PieceType.Trebuchet,
            PieceType.Shielder, PieceType.Shielder, PieceType.Shielder, PieceType.Shielder, PieceType.Shielder, PieceType.Shielder, PieceType.Shielder, PieceType.Shielder,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupBirdArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Falcon, PieceType.Vulture, PieceType.Hawk, PieceType.Eagle, PieceType.King, PieceType.Hawk, PieceType.Vulture, PieceType.Falcon,
            PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling, PieceType.Fledgling,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupTripunchArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Stronghold, PieceType.Pegasus, PieceType.Pontiff, PieceType.Empress, PieceType.King, PieceType.Pontiff, PieceType.Pegasus, PieceType.Stronghold,
            PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn, PieceType.SuperPawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupCrystalTest()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook,
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Knight,PieceType.Knight,0,0,0,0,PieceType.Knight,PieceType.Knight,
            PieceType.Guard,0,0,0,0,0,0,PieceType.Guard
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            Piece.PieceAlignment pa = i < 32 ? PieceAlignment.White : PieceAlignment.Black;

            int row = (i & 56) >> 3;
            if (row == 2 || row == 5)
            {
                pa = PieceAlignment.Crystal;
            }
            if (row == 3 || row == 4)
            {
                pa = PieceAlignment.Neutral;
            }

            pieces[i] = Piece.PackPieceData(pt, pa);
        }

        PostSetupInit();
    }
    public void SetupWeirdArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.CrookedBishop, PieceType.CrookedRook, PieceType.Lobster, PieceType.Shrimp, PieceType.King, PieceType.Stronghold, PieceType.Spy, PieceType.Warship,
            PieceType.CrookedPawn, PieceType.ArcanaFortune, PieceType.Tardigrade, PieceType.Courtesan, PieceType.Moon, PieceType.FastPawn, PieceType.Runner, PieceType.Canoe,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupLycanthropeArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Werebear, PieceType.Werewolf, PieceType.Plaguebearer, PieceType.Werechimera, PieceType.King, PieceType.Queen, PieceType.Bishop, PieceType.Rook,
            PieceType.Werefox, PieceType.Werefox, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Werefox, PieceType.Werefox,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupFieryArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Lanterner, PieceType.Candler, PieceType.Torcher, PieceType.Vestal, PieceType.King, PieceType.Torcher, PieceType.Candler, PieceType.Lanterner,
            PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn, PieceType.MatchPawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupPushPullArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.PushRook, PieceType.Pusher, PieceType.PushBishop, PieceType.PushQueen, PieceType.King, PieceType.StickyQueen, PieceType.StickyBishop, PieceType.StickyRook,
            PieceType.PushPawn, PieceType.PushPawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.StickyMan, PieceType.StickyPawn,
            PieceType.Pawn,PieceType.Pawn,0,0,0,0,PieceType.StickyPawn,PieceType.StickyPawn,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupGiantArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Elephant, PieceType.Elephant, PieceType.ElephantQueen, PieceType.ElephantQueen, PieceType.King, PieceType.Bishop, PieceType.ElephantCharger, PieceType.ElephantCharger,
            PieceType.Elephant, PieceType.Elephant, PieceType.ElephantQueen, PieceType.ElephantQueen, PieceType.ElephantCalf, PieceType.ElephantCalf, PieceType.ElephantCharger, PieceType.ElephantCharger,
            PieceType.Pawn,PieceType.Pawn,PieceType.Pawn,PieceType.Pawn,PieceType.ElephantCalf,PieceType.ElephantCalf,PieceType.Pawn,PieceType.Pawn,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        SetupGiant(0, 0);
        SetupGiant(2, 0);
        SetupGiant(4, 1);
        SetupGiant(6, 0);
        SetupGiant(0, 6);
        SetupGiant(2, 6);
        SetupGiant(4, 5);
        SetupGiant(6, 6);

        PostSetupInit();
    }
    public void SetupRoyalistArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.RoyalCastle, PieceType.RoyalGuard, PieceType.RoyalMaid, PieceType.RoyalDouble, PieceType.King, PieceType.RoyalMaid, PieceType.RoyalGuard, PieceType.RoyalCastle,
            PieceType.Infiltrator, PieceType.Disguiser, PieceType.Jester, PieceType.RoyalRecruit, PieceType.RoyalRecruit, PieceType.Jester, PieceType.Disguiser, PieceType.Spy,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupWarArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.WarRook, PieceType.WarKnight, PieceType.WarBishop, PieceType.WarQueen, PieceType.King, PieceType.WarBishop, PieceType.Bomb, PieceType.BigBomb,
            PieceType.BerserkerPawn, PieceType.BerserkerPawn, PieceType.BerserkerPawn, PieceType.BerserkerPawn, PieceType.BerserkerPawn, PieceType.BerserkerPawn, PieceType.MiniBomb, PieceType.MiniBomb,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupSwitchArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.SwitchTower, PieceType.SwitchFrog, PieceType.SwitchKnight, PieceType.SwitchPaladin, PieceType.King, PieceType.SwitchKnight, PieceType.SwitchFrog, PieceType.SwitchTower,
            PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire, PieceType.SwitchSquire,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupEndgameTest()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Rook, 0, 0, 0, PieceType.King, 0, 0, 0,
            PieceType.Pawn,0,0,0,0,0,0,PieceType.Pawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupTerrainTest()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.Rook, PieceType.ArcanaMoon, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook,
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            //type 0 is normal so if I want the center 4 rows to be special I subtract by 1 less
            if (i > 16 && i <= 16 + 16)
            {
                //Debug.Log(i + " " + (Square.SquareType)(i - 15));
                globalData.squares[i] = new Square((Square.SquareType)(i - 15));
            }
            globalData.squares[16] = new Square(Square.SquareType.Hole);
            globalData.squares[31] = new Square(Square.SquareType.Fire);
            if (i > 33 && i <= 33 + 14)
            {
                //Debug.Log(i + " " + (Square.SquareType)(i - 32));
                globalData.squares[i] = new Square((Square.SquareType)(i - 32));
            }

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }
    public void SetupRelayArmy()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.RelayRook, PieceType.RelayKnight, PieceType.RelayBishop, PieceType.RelayQueen, PieceType.King, PieceType.Charity, PieceType.ArcanaSun, PieceType.ArcanaWorld,
            PieceType.ArcanaEmpress, PieceType.Hypnotist, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Kindness, PieceType.Envy,
            PieceType.Pawn,PieceType.Pawn,PieceType.ArcanaFool,0,0,0,PieceType.Pawn,PieceType.Pawn,
            0,0,0,0,0,0,0,0
        };

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                indexToRead = 63 - indexToRead;
                indexToRead = indexToRead - (indexToRead % 8) + (7 - (indexToRead % 8));
            }
            Piece.PieceType pt = ptList[indexToRead];

            if (pt == PieceType.Null)
            {
                continue;
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);
        }

        PostSetupInit();
    }

    public void PostSetupInit()
    {
        EnemyModifierSetup();

        SetInitialPieceValueCount();
        ResetPieceValueCount();
        BuildSquareBitboards();

        globalData.noLastMoveHash = true;

        if ((globalData.enemyModifier & EnemyModifier.Blinking) != 0)
        {
            globalData.noLastMoveHash = false;
        }

        for (int i = 0; i < 64; i++)
        {
            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[i]);
            if (pte != null)
            {
                if ((pte.pieceProperty & PieceProperty.SlowMove) != 0)
                {
                    globalData.noLastMoveHash = false;
                }
                if ((pte.pieceProperty & PieceProperty.InvinciblePride) != 0)
                {
                    globalData.noLastMoveHash = false;
                }
            }
        }
    }

    public static List<Piece.PieceType> EnemyModifierExtraPieces(EnemyModifier em)
    {
        List<PieceType> newpieces = new List<PieceType>();

        //todo: whenever I finally make the random enemy preset generator I will move this there
        if ((em & EnemyModifier.Jester) != 0)
        {
            newpieces.Add(PieceType.Jester);
            newpieces.Add(PieceType.Jester);
            newpieces.Add(PieceType.Jester);
            newpieces.Add(PieceType.Jester);
        }
        if ((em & EnemyModifier.Numerous) != 0)
        {
            newpieces.Add(PieceType.King);
            newpieces.Add(PieceType.King);
            newpieces.Add(PieceType.King);
        }
        if ((em & EnemyModifier.Queenly) != 0)
        {
            newpieces.Add(PieceType.Queen);
            newpieces.Add(PieceType.Princess);
            newpieces.Add(PieceType.Princess);
        }

        return newpieces;
    }

    public void EnemyModifierSetup()
    {
        if ((globalData.enemyModifier & EnemyModifier.Hidden) != 0)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black && Piece.GetPieceType(pieces[i]) == PieceType.King)
                {
                    pieces[i] = 0;
                }
            }
        }
    }

    public uint GetLastMove()
    {
        if (bonusPly > 0)
        {
            if (blackToMove)
            {
                return blackPerPlayerInfo.lastMove;
            }
            else
            {
                return whitePerPlayerInfo.lastMove;
            }
        }
        if (blackToMove)
        {
            return whitePerPlayerInfo.lastMove;
        } else
        {
            return blackPerPlayerInfo.lastMove;
        }
    }
    public bool GetLastMoveStationary()
    {
        if (bonusPly > 0)
        {
            if (blackToMove)
            {
                return Move.GetFromXYInt(blackPerPlayerInfo.lastMove) == blackPerPlayerInfo.lastPieceMovedLocation;
            }
            else
            {
                return Move.GetFromXYInt(whitePerPlayerInfo.lastMove) == whitePerPlayerInfo.lastPieceMovedLocation;
            }
        }
        if (blackToMove)
        {
            return Move.GetFromXYInt(whitePerPlayerInfo.lastMove) == whitePerPlayerInfo.lastPieceMovedLocation;
        }
        else
        {
            return Move.GetFromXYInt(blackPerPlayerInfo.lastMove) == blackPerPlayerInfo.lastPieceMovedLocation;
        }
    }
    public bool GetLastMoveCapture()
    {
        if (bonusPly > 0)
        {
            if (blackToMove)
            {
                return blackPerPlayerInfo.capturedLastTurn;
            }
            else
            {
                return whitePerPlayerInfo.capturedLastTurn;
            }
        }
        if (blackToMove)
        {
            return whitePerPlayerInfo.capturedLastTurn;
        }
        else
        {
            return blackPerPlayerInfo.capturedLastTurn;
        }
    }
    public uint GetLastMovedPiece()
    {
        if (bonusPly > 0)
        {
            if (blackToMove)
            {
                return blackPerPlayerInfo.lastPieceMoved;
            }
            else
            {
                return whitePerPlayerInfo.lastPieceMoved;
            }
        }
        if (blackToMove)
        {
            return whitePerPlayerInfo.lastPieceMoved;
        }
        else
        {
            return blackPerPlayerInfo.lastPieceMoved;
        }
    }

    public void SetInitialPieceValueCount()
    {
        for (int i = 0; i < 64; i++)
        {
            if (pieces[i] == 0)
            {
                continue;
            }

            PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(pieces[i]));

            if ((pte.pieceProperty & PieceProperty.Giant) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
            {
                continue;
            }

            if (pte.type == PieceType.MoonIllusion)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
            {
                globalData.whitePerPlayerInfo.startPieceValueSumX2 += pte.pieceValueX2;
                globalData.whitePerPlayerInfo.startPieceCount++;
            }
            if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
            {
                globalData.blackPerPlayerInfo.startPieceValueSumX2 += pte.pieceValueX2;
                globalData.blackPerPlayerInfo.startPieceCount++;
            }
        }
    }
    public void ResetPieceValueCount()
    {
        whitePerPlayerInfo.pieceCount = 0;
        whitePerPlayerInfo.pieceValueSumX2 = 0;
        blackPerPlayerInfo.pieceCount = 0;
        blackPerPlayerInfo.pieceValueSumX2 = 0;
        for (int i = 0; i < 64; i++)
        {
            if (pieces[i] == 0)
            {
                continue;
            }

            PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(pieces[i]));

            if ((pte.pieceProperty & PieceProperty.Giant) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
            {
                continue;
            }

            if (pte.type == PieceType.MoonIllusion)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                whitePerPlayerInfo.pieceCount++;

                if ((pte.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
                {
                    whitePerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.Instance.GetPieceTableEntry((PieceType)Piece.GetPieceSpecialData(pieces[i])).pieceValueX2;
                }
            }
            if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                blackPerPlayerInfo.pieceCount++;

                if ((pte.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
                {
                    blackPerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.Instance.GetPieceTableEntry((PieceType)Piece.GetPieceSpecialData(pieces[i])).pieceValueX2;
                }
            }
        }
    }

    public Board()
    {
        ply = 0;
        bonusPly = 0;
        turn = 0;
        pieces = new uint[64];
        globalData = new BoardGlobalData();
    }
    public Board(Board b)
    {
        ply = b.ply;
        bonusPly = b.bonusPly;
        turn = b.turn;
        pieces = new uint[64];
        for (int i = 0; i < 64; i++)
        {
            pieces[i] = b.pieces[i];
        }

        blackToMove = b.blackToMove;
        
        globalData = b.globalData;

        whitePerPlayerInfo = b.whitePerPlayerInfo;
        blackPerPlayerInfo = b.blackPerPlayerInfo;
    }

    public void SplitGlobalData()
    {
        BoardGlobalData bgd = new BoardGlobalData();

        for (int i = 0; i < 64; i++)
        {
            bgd.squares[i] = globalData.squares[i];
        }

        bgd.playerModifier = globalData.playerModifier;
        bgd.enemyModifier = globalData.enemyModifier;

        bgd.whitePerPlayerInfo = globalData.whitePerPlayerInfo;
        bgd.blackPerPlayerInfo = globalData.blackPerPlayerInfo;

        //Bit tables and bitboards can be generated later
        globalData = bgd;
    }

    public void BuildSquareBitboards()
    {
        /*
            public ulong bitboard_square_fire;
            public ulong bitboard_square_windUp;
            public ulong bitboard_square_windDown;
            public ulong bitboard_square_windLeft;
            public ulong bitboard_square_windRight;
            public ulong bitboard_square_bright;
            public ulong bitboard_square_promotion;
            public ulong bitboard_square_cursed;
         */

        globalData.bitboard_square_hole = 0;
        globalData.bitboard_square_fire = 0;
        globalData.bitboard_square_windUp = 0;
        globalData.bitboard_square_windDown = 0;
        globalData.bitboard_square_windLeft = 0;
        globalData.bitboard_square_windRight = 0;
        globalData.bitboard_square_bright = 0;
        globalData.bitboard_square_promotion = 0;
        globalData.bitboard_square_cursed = 0;
        for (int i = 0; i < 64; i++)
        {
            ulong bitIndex = 1uL << i;
            switch (globalData.squares[i].type)
            {
                case Square.SquareType.Hole:
                    globalData.bitboard_square_hole |= bitIndex;
                    break;
                case Square.SquareType.Fire:
                    globalData.bitboard_square_fire |= bitIndex;
                    break;
                case Square.SquareType.WindUp:
                    globalData.bitboard_square_windUp |= bitIndex;
                    break;
                case Square.SquareType.WindDown:
                    globalData.bitboard_square_windDown |= bitIndex;
                    break;
                case Square.SquareType.WindLeft:
                    globalData.bitboard_square_windLeft |= bitIndex;
                    break;
                case Square.SquareType.WindRight:
                    globalData.bitboard_square_windRight |= bitIndex;
                    break;
                case Square.SquareType.Bright:
                    globalData.bitboard_square_bright |= bitIndex;
                    break;
                case Square.SquareType.Promotion:
                    globalData.bitboard_square_promotion |= bitIndex;
                    break;
                case Square.SquareType.Cursed:
                    globalData.bitboard_square_cursed |= bitIndex;
                    break;
            }
        }
    }

    //Doesn't allocate more memory = better performance?
    public void CopyOverwrite(Board b)
    {
        ply = b.ply;
        bonusPly = b.bonusPly;
        turn = b.turn;
        //pieces = new uint[64];
        if (pieces == null)
        {
            pieces = new uint[64];
        }
        for (int i = 0; i < 64; i++)
        {
            pieces[i] = b.pieces[i];
        }

        blackToMove = b.blackToMove;

        globalData = b.globalData;

        whitePerPlayerInfo = b.whitePerPlayerInfo;
        blackPerPlayerInfo = b.blackPerPlayerInfo;
    }

    public static bool CoordinateIsBlack(int x, int y)
    {
        return ((x + y) & 1) == 0;
    }
    public static int CoordinateConvert(int x, int y)
    {
        return y * 8 + x;
    }
    public static (int, int) CoordinateConvertInverse(int index)
    {
        //the fast way of doing this
        return (index & 7, index >> 3);
    }

    public uint GetPieceAtCoordinate(int x, int y)
    {
        return pieces[CoordinateConvert(x, y)];
    }
    public void SetPieceAtCoordinate(int x, int y, uint set)
    {
        /*
        //will slow things down?
        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            return;
        }
        */

        pieces[CoordinateConvert(x, y)] = set;
    }

    public Square GetSquareAtCoordinate(int x, int y)
    {
        return globalData.squares[CoordinateConvert(x, y)];
    }

    public byte GetMissingPieces(bool isBlack)
    {
        if (isBlack)
        {
            return (byte)(globalData.blackPerPlayerInfo.startPieceCount - blackPerPlayerInfo.pieceCount);
        } else
        {
            return (byte)(globalData.whitePerPlayerInfo.startPieceCount - whitePerPlayerInfo.pieceCount);
        }
    }

    public ulong MakeZobristHashFromScratch(ulong[][] ztable, ulong[] supplementZTable)
    {
        ulong zhash = 0;
        uint testIndex = 1;

        for (int i = 0; i < pieces.Length; i++)
        {
            testIndex = 1;
            for (int j = 0; j < 32; j++)
            {
                if ((testIndex & pieces[i]) != 0)
                {
                    zhash ^= ztable[i][j];
                }

                testIndex <<= 1;
            }
        }

        //Add in the extra z table stuff
        //black to move bool (1 bit)

        if (blackToMove)
        {
            zhash ^= supplementZTable[0];
        }

        //Per player
        //castling rights (1 bit)
        if (whitePerPlayerInfo.canCastle)
        {
            zhash ^= supplementZTable[1];
        }


        int zindex = 2;
        
        //last piece moved type (10 bits)
        testIndex = 1;
        for (int i = 0; i < 10; i++)
        {
            if (!globalData.noLastMoveHash)
            {
                if ((testIndex & (uint)whitePerPlayerInfo.lastPieceMovedType) != 0)
                {
                    zhash ^= supplementZTable[zindex];
                }
            }

            zindex++;
            testIndex >>= 1;
        }

        if (blackPerPlayerInfo.canCastle)
        {
            zhash ^= supplementZTable[12];
        }

        zindex = 13;
        testIndex = 1;
        for (int i = 0; i < 10; i++)
        {
            if (!globalData.noLastMoveHash)
            {
                if ((testIndex & (uint)blackPerPlayerInfo.lastPieceMovedType) != 0)
                {
                    zhash ^= supplementZTable[zindex];
                }
            }

            zindex++;
            testIndex >>= 1;
        }

        //23
        if ((bonusPly & 1) != 0)
        {
            zhash ^= supplementZTable[23];
        }

        return zhash;
    }

    //Make a hash from the delta
    public ulong MakeZobristHashFromDelta(ulong[][] ztable, ulong[] supplementZTable, ref Board oldBoard, ulong oldHash)
    {
        ulong output = oldHash;

        uint testIndex = 1;
        for (int i = 0; i < pieces.Length; i++)
        {
            uint xor = pieces[i] ^ oldBoard.pieces[i];
            if (xor != 0)
            {
                //these could be in either order technically
                //Remove old and add new
                testIndex = 1;
                for (int j = 0; j < 32; j++)
                {
                    //Only hash in if there is a difference
                    //Equivalent to the below separate thing
                    if (((testIndex & (xor))) != 0)
                    {
                        output ^= ztable[i][j];
                    }
                    testIndex <<= 1;
                }
            }
        }

        if (blackToMove ^ oldBoard.blackToMove)
        {
            output ^= supplementZTable[0];
        }

        if (whitePerPlayerInfo.canCastle ^ oldBoard.whitePerPlayerInfo.canCastle)
        {
            output ^= supplementZTable[1];
        }

        int zindex = 2;
        testIndex = 1;
        //2 to 11
        if (!globalData.noLastMoveHash)
        {
            uint lpmt = (uint)whitePerPlayerInfo.lastPieceMovedType ^ (uint)oldBoard.whitePerPlayerInfo.lastPieceMovedType;
            for (int i = 0; i < 10; i++)
            {
                if ((testIndex & (lpmt)) != 0)
                {
                    output ^= supplementZTable[zindex];
                }

                zindex++;
                testIndex >>= 1;
            }
        }

        if (blackPerPlayerInfo.canCastle ^ oldBoard.blackPerPlayerInfo.canCastle)
        {
            output ^= supplementZTable[12];
        }

        zindex = 13;
        testIndex = 1;
        //13 to 22
        if (!globalData.noLastMoveHash)
        {
            uint lpmt = (uint)blackPerPlayerInfo.lastPieceMovedType ^ (uint)oldBoard.blackPerPlayerInfo.lastPieceMovedType;
            for (int i = 0; i < 10; i++)
            {
                if ((testIndex & (lpmt)) != 0)
                {
                    output ^= supplementZTable[zindex];
                }

                zindex++;
                testIndex >>= 1;
            }
        }

        //23
        if (((bonusPly ^ oldBoard.bonusPly) & 1) != 0)
        {
            output ^= supplementZTable[23];
        }


        return output;
    }

    public Piece.PieceAlignment GetVictoryCondition()
    {
        //To add bare king victory I just change these numbers to 1
        if (whitePerPlayerInfo.pieceCount == 1)
        {
            if (blackPerPlayerInfo.pieceCount == 1)
            {
                //???
                //"Neutral" victory = draw
                return PieceAlignment.Neutral;
            }
            return PieceAlignment.Black;
        }
        if (blackPerPlayerInfo.pieceCount == 1)
        {
            return PieceAlignment.White;
        }

        //Overtaken victory
        int ylevel = 7;
        for (int i = 0; i < 8; i++)
        {
            uint target = pieces[i + (ylevel << 3)];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.White)
            {
                return PieceAlignment.White;
            }
        }
        ylevel = 0;
        for (int i = 0; i < 8; i++)
        {
            uint target = pieces[i + (ylevel << 3)];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.Black)
            {
                return PieceAlignment.Black;
            }
        }

        return PieceAlignment.Null;
    }

    public Piece.PieceAlignment AlignmentToMove()
    {
        if (blackToMove)
        {
            return PieceAlignment.Black;
        } else
        {
            return PieceAlignment.White;
        }
    }
    public Piece.PieceAlignment AlignmentNotToMove()
    {
        if (blackToMove)
        {
            return PieceAlignment.White;
        }
        else
        {
            return PieceAlignment.Black;
        }
    }

    public void ApplyNullMove()
    {
        RunTurnEnd(blackToMove, false, null);
        bonusPly = 0;
        blackToMove = !blackToMove;
    }
    public void ApplyMove(uint move)
    {
        ApplyMove(move, null);
    }
    public void ApplyMove(uint move, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //bad
        //Only really exists to fix Arcana Moon
        MoveGeneratorInfoEntry.GeneratePieceBitboards(this);

        //I can do this because the board update metadata being present means it isn't the ai looking through the game tree
        //So this being slow is not a problem
        if (boardUpdateMetadata != null)
        {
            ResetPieceValueCount();
        }

        //Note: Setup moves bypass this system entirely (it doesn't run in the normal turn cycle)
        if (Move.IsConsumableMove(move))
        {
            ApplyConsumableMove(move, boardUpdateMetadata);

            ply++;
            if (blackToMove)
            {
                turn++;
            }

            //RunTurnStart(!blackToMove);
            blackToMove = !blackToMove;
            return;
        }

        int fx = Move.GetFromX(move);
        int fy = Move.GetFromY(move);
        int tx = Move.GetToX(move);
        int ty = Move.GetToY(move);

        Move.Dir dir = Move.GetDir(move);

        Move.SpecialType specialType = Move.GetSpecialType(move);

        uint oldPiece = GetPieceAtCoordinate(fx, fy);
        uint targetPiece = GetPieceAtCoordinate(tx, ty);

        Piece.PieceType opt = Piece.GetPieceType(oldPiece);

        if (opt == PieceType.Null)
        {
            //not a legal
            RunTurnEnd(blackToMove, false, boardUpdateMetadata);

            ply++;
            if (blackToMove)
            {
                turn++;
            }

            //RunTurnStart(!blackToMove);
            blackToMove = !blackToMove;
            return;
        }

        bool passiveMove = targetPiece == 0;

        bool lastMoveStationary = Move.SpecialMoveStationary(Move.GetSpecialType(move));
        if (blackToMove)
        {
            blackPerPlayerInfo.lastMove = move;
            blackPerPlayerInfo.lastPieceMoved = oldPiece;
            blackPerPlayerInfo.lastPieceMovedType = opt;
            blackPerPlayerInfo.capturedLastTurn = false;
            if (lastMoveStationary)
            {
                blackPerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
            }
            else
            {
                blackPerPlayerInfo.lastPieceMovedLocation = (tx + ty * 8);
            }
        }
        else
        {
            whitePerPlayerInfo.lastMove = move;
            whitePerPlayerInfo.lastPieceMoved = oldPiece;
            whitePerPlayerInfo.lastPieceMovedType = opt;
            whitePerPlayerInfo.capturedLastTurn = false;
            if (lastMoveStationary)
            {
                whitePerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
            }
            else
            {
                whitePerPlayerInfo.lastPieceMovedLocation = (tx + ty * 8);
            }
        }

        PieceTableEntry pteO = GlobalPieceManager.Instance.GetPieceTableEntry(opt);
        Piece.PieceAlignment opa = Piece.GetPieceAlignment(oldPiece);

        if ((pteO.pieceProperty & PieceProperty.Giant) != 0)
        {
            ApplyGiantMove(oldPiece, opa, fx, fy, tx, ty, pteO, boardUpdateMetadata);
            RunTurnEnd(blackToMove, false, boardUpdateMetadata);

            ply++;
            if (blackToMove)
            {
                turn++;
            }

            //RunTurnStart(!blackToMove);
            blackToMove = !blackToMove;
            return;
        }

        PieceTableEntry pteT = null;
        if (!passiveMove)
        {
            pteT = GlobalPieceManager.GetPieceTableEntry(targetPiece);
        }
        Piece.PieceAlignment tpa = Piece.GetPieceAlignment(targetPiece);

        //zapped
        if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Sparked)
        {
            oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
            oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
        }

        //bloodlust
        //Some special cases have to be handled separately
        if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust && !passiveMove && tpa != opa && Move.SpecialMoveCaptureLike(specialType))
        {
            oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
            oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
        }

        if ((pteO.pieceProperty & PieceProperty.ClockworkSwapper) != 0)
        {
            oldPiece = Piece.SetPieceType(opt + 1, oldPiece);
        }
        if ((pteO.piecePropertyB & PiecePropertyB.ClockworkSwapperB) != 0)
        {
            oldPiece = Piece.SetPieceType(opt - 1, oldPiece);
        }

        switch (specialType)
        {
            case Move.SpecialType.Normal:
            case Move.SpecialType.MoveOnly:     //note: a Move Only onto an enemy piece acts like a capture still
            case Move.SpecialType.CaptureOnly:
            case Move.SpecialType.ChargeMove:
            case SpecialType.ChargeMoveReset:
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case Move.SpecialType.FlyingMoveOnly:
            case Move.SpecialType.PullMove:
            case Move.SpecialType.SlipMove:
            case SpecialType.GliderMove:
            case Move.SpecialType.PlantMove:
                if (blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Greedy) != 0 && (globalData.whitePerPlayerInfo.startPieceCount - whitePerPlayerInfo.pieceCount) < 2)
                {
                    if (!passiveMove && tpa != opa)
                    {
                        //set to stationary
                        blackPerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
                        goto case SpecialType.Convert;
                    }
                }
                //need to fix this information for later
                if (lastMoveStationary)
                {
                    if (blackToMove)
                    {
                        blackPerPlayerInfo.lastMove = Move.SetSpecialType(Move.SpecialType.Normal, blackPerPlayerInfo.lastMove);
                        blackPerPlayerInfo.lastPieceMovedLocation = (tx + ty * 8);
                    }
                    else
                    {
                        blackPerPlayerInfo.lastMove = Move.SetSpecialType(Move.SpecialType.Normal, whitePerPlayerInfo.lastMove);
                        whitePerPlayerInfo.lastPieceMovedLocation = (tx + ty * 8);
                    }
                }
                lastMoveStationary = false;

                //no check for ChargeMoveReset because the reset happens later?
                //Minor inefficiency but ehh
                if (specialType != SpecialType.ChargeMove && (pteO.piecePropertyB & PiecePropertyB.ChargeByMoving) != 0)
                {
                    //Get a charge
                    oldPiece = Piece.SetPieceSpecialData((ushort)(Piece.GetPieceSpecialData(oldPiece) + 1), oldPiece);
                }

                //Normal movement
                //Move the piece to the spot
                //Delete old piece
                uint residuePiece = 0;
                switch (opt)
                {
                    case PieceType.Gluttony:
                        if (!passiveMove)
                        {
                            ushort newPower = Piece.GetPieceSpecialData(oldPiece);
                            if (newPower < 7)
                            {
                                newPower++;
                                oldPiece = Piece.SetPieceSpecialData(newPower, oldPiece);
                            }
                        }
                        break;
                    case PieceType.ArcanaMoon:
                    case PieceType.MoonIllusion:
                        if (opt == PieceType.ArcanaMoon)
                        {
                            residuePiece = Piece.SetPieceType(PieceType.MoonIllusion, oldPiece);
                            oldPiece = Piece.SetPieceType(PieceType.ArcanaMoon, oldPiece);
                        }
                        else
                        {
                            //Moving the illusion leaves no residue (so you can move illusions away from attack or clear them from your back row)
                            //residuePiece = Piece.SetPieceType(PieceType.MoonIllusion, oldPiece);
                            oldPiece = Piece.SetPieceType(PieceType.ArcanaMoon, oldPiece);
                        }
                        ulong moonBitboard = 0;
                        if (opa == PieceAlignment.White)
                        {
                            moonBitboard = globalData.bitboard_tarotMoonIllusionWhite;
                            whitePerPlayerInfo.pieceCount++;
                            //MainManager.PrintBitboard(moonBitboard);
                        }
                        else if (opa == PieceAlignment.Black)
                        {
                            moonBitboard = globalData.bitboard_tarotMoonIllusionBlack;
                            blackPerPlayerInfo.pieceCount++;
                            //MainManager.PrintBitboard(moonBitboard);
                        }
                        while (moonBitboard != 0)
                        {
                            int index = MainManager.PopBitboardLSB1(moonBitboard, out moonBitboard);

                            //the bitboard will get filled with stuff for parallel moves and become corrupted
                            //but the original data will exist within so this is still guaranteed to hit all of them?
                            Piece.PieceType spt = Piece.GetPieceType(pieces[index]);
                            if (spt == PieceType.ArcanaMoon || spt == PieceType.MoonIllusion)
                            {
                                pieces[index] = Piece.SetPieceType(Piece.PieceType.MoonIllusion, pieces[index]);
                            }
                        }
                        break;
                    case PieceType.Necromancer:
                        if (!passiveMove)
                        {
                            residuePiece = Piece.SetPieceType(PieceType.Skeleton, oldPiece);

                            //Add the value of the skeleton
                            PieceTableEntry pteS = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Skeleton);

                            if (opa == PieceAlignment.White)
                            {
                                whitePerPlayerInfo.pieceCount++;
                                whitePerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
                            } else
                            {
                                blackPerPlayerInfo.pieceCount++;
                                blackPerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
                            }
                        }
                        break;
                }

                if (!passiveMove)
                {
                    if (blackToMove)
                    {
                        blackPerPlayerInfo.capturedLastTurn = true;
                    } else
                    {
                        whitePerPlayerInfo.capturedLastTurn = true;
                    }

                    Piece.PieceType tpt = Piece.GetPieceType(targetPiece);

                    bool pieceChange = false;

                    //King is immutable
                    //(Note that the way king capture is detected means that any time where you have a move that destroys your own king that counts against your opponent)
                    if (pteO.type == PieceType.King)
                    {
                        pieceChange = true;
                    }

                    if (tpa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (tpa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }

                    //note: you can consume enemies but allies is easier for you to do
                    //Leech doesn't because it promotes to Queen Leech
                    //(I don't want Leeches to be more powerful)
                    if ((specialType == SpecialType.ConsumeAllies || specialType == SpecialType.ConsumeAlliesCaptureOnly) && opt != PieceType.Leech)
                    {
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) + 1), oldPiece);
                    }

                    ulong hagBitboard = 0;
                    if (opa == PieceAlignment.White)
                    {
                        hagBitboard = globalData.bitboard_hagBlack;
                    } else if (opa == PieceAlignment.Black)
                    {
                        hagBitboard = globalData.bitboard_hagWhite;
                    }

                    //Debug.Log("Hag check" + opa);
                    //MainManager.PrintBitboard(hagBitboard);

                    //destroy capturer

                    bool wrathDestruction = false;
                    if (!blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Wrathful) != 0 && (globalData.whitePerPlayerInfo.startPieceCount - whitePerPlayerInfo.pieceCount) < 3)
                    {
                        wrathDestruction = true;
                    }

                    if (!pieceChange && (wrathDestruction || (pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & (PieceProperty.DestroyOnCapture)) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((1uL << (fx + fy * 8) & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful))
                    {
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;
                    }

                    //X capture
                    if (!pieceChange && (pteO.pieceProperty & PieceProperty.ExplodeCaptureX) != 0)
                    {
                        //Also explodes self
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;

                        //Explode all the enemy pieces in the 3x3 area around you
                        for (int i = -1; i <= 1; i++)
                        {
                            if (tx + i < 0)
                            {
                                continue;
                            }
                            if (tx + i > 7)
                            {
                                continue;
                            }

                            for (int j = -1; j <= 1; j++)
                            {
                                if (ty + j < 0)
                                {
                                    continue;
                                }
                                if (ty + j > 7)
                                {
                                    continue;
                                }

                                PieceTableEntry pteE = GlobalPieceManager.GetPieceTableEntry(pieces[tx + i + 8 * (ty + j)]);

                                //delete the enemies on the delta
                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + i, ty + j));
                                if (epa != opa && (i != 0 && j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.FireCapture, pteO, pteE))
                                {
                                    if (epa == PieceAlignment.White)
                                    {
                                        whitePerPlayerInfo.pieceCount--;
                                        whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }
                                    if (epa == PieceAlignment.Black)
                                    {
                                        blackPerPlayerInfo.pieceCount--;
                                        blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }

                                    DeletePieceAtCoordinate(tx + i, ty + j, pteE, opa, boardUpdateMetadata);
                                    //SetPieceAtCoordinate(tx + i, ty + j, 0);
                                }
                            }
                        }
                        pieceChange = true;
                    }

                    //Explode on capture
                    if (!pieceChange && (pteO.pieceProperty & PieceProperty.ExplodeCapture) != 0)
                    {
                        //Also explodes self
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;

                        //Explode all the enemy pieces in the 3x3 area around you
                        for (int i = -1; i <= 1; i++)
                        {
                            if (tx + i < 0)
                            {
                                continue;
                            }
                            if (tx + i > 7)
                            {
                                continue;
                            }

                            for (int j = -1; j <= 1; j++)
                            {
                                if (ty + j < 0)
                                {
                                    continue;
                                }
                                if (ty + j > 7)
                                {
                                    continue;
                                }

                                PieceTableEntry pteE = GlobalPieceManager.GetPieceTableEntry(pieces[tx + i + 8 * (ty + j)]);

                                //delete the enemies on the delta
                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + i, ty + j));
                                if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.FireCapture, pteO, pteE)) {
                                    if (epa == PieceAlignment.White)
                                    {
                                        whitePerPlayerInfo.pieceCount--;
                                        whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }
                                    if (epa == PieceAlignment.Black)
                                    {
                                        blackPerPlayerInfo.pieceCount--;
                                        blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }

                                    DeletePieceAtCoordinate(tx + i, ty + j, pteE, opa, boardUpdateMetadata);
                                    //SetPieceAtCoordinate(tx + i, ty + j, 0);
                                }
                            }
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.IceExplode) != 0)
                    {
                        //Also explodes self
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;

                        //Explode all the enemy pieces in the 3x3 area around you
                        for (int i = -1; i <= 1; i++)
                        {
                            if (tx + i < 0)
                            {
                                continue;
                            }
                            if (tx + i > 7)
                            {
                                continue;
                            }

                            for (int j = -1; j <= 1; j++)
                            {
                                if (ty + j < 0)
                                {
                                    continue;
                                }
                                if (ty + j > 7)
                                {
                                    continue;
                                }

                                PieceTableEntry pteE = GlobalPieceManager.GetPieceTableEntry(pieces[tx + i + 8 * (ty + j)]);

                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + i, ty + j));
                                if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.InflictFreeze, pteO, pteE))
                                {
                                    pieces[tx + i + ((ty + j) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, pieces[tx + i + ((ty + j) << 3)]));
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pteE.type, BoardUpdateMetadata.BoardUpdateType.StatusApply));
                                    }
                                }
                            }
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.PoisonExplode) != 0)
                    {
                        //Also explodes self
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;

                        //Explode all the enemy pieces in the 3x3 area around you
                        for (int i = -1; i <= 1; i++)
                        {
                            if (tx + i < 0)
                            {
                                continue;
                            }
                            if (tx + i > 7)
                            {
                                continue;
                            }

                            for (int j = -1; j <= 1; j++)
                            {
                                if (ty + j < 0)
                                {
                                    continue;
                                }
                                if (ty + j > 7)
                                {
                                    continue;
                                }

                                uint tempPiece = pieces[tx + i + ((ty + j) << 3)];
                                PieceTableEntry pteE = GlobalPieceManager.GetPieceTableEntry(tempPiece);

                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(tempPiece);
                                if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, tempPiece, tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.Inflict, pteO, pteE))
                                {
                                    pieces[tx + i + ((ty + j) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Poisoned, Piece.SetPieceStatusDuration(3, tempPiece));
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pteE.type, BoardUpdateMetadata.BoardUpdateType.StatusApply));
                                    }
                                }
                            }
                        }
                        pieceChange = true;
                    }

                    //conversion
                    //Destroy Capturer / Destroy on Capture has precedence
                    if (!pieceChange && (((pteT.pieceProperty & PieceProperty.MorphCapturer) != 0) || 
                        ((pteT.pieceProperty & PieceProperty.MorphCapturerNonPawn) != 0 && pteO.promotionType == PieceType.Null) || 
                        ((pteT.pieceProperty & PieceProperty.MorphCapturerPawn) != 0 && pteO.promotionType != PieceType.Null)))
                    {
                        oldPiece = Piece.SetPieceType(tpt, oldPiece);
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
                        }
                        pieceChange = true;
                    }

                    //promote on capture
                    //conversion has precedence over it
                    if (!pieceChange && ((pteO.pieceProperty & PieceProperty.PromoteCapture) != 0 && pteO.promotionType != PieceType.Null)
                        || (((pteO.pieceProperty & PieceProperty.PromoteCaptureNonPawn) != 0 && pteO.promotionType != PieceType.Null && pteO.promotionType == PieceType.Null)))
                    {
                        oldPiece = Piece.SetPieceType(pteO.promotionType, oldPiece);
                        PieceTableEntry pteP = GlobalPieceManager.Instance.GetPieceTableEntry(pteO.promotionType);
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pteP.pieceValueX2 - pteO.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pteP.pieceValueX2 - pteO.pieceValueX2);
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
                    {
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            blackPerPlayerInfo.pieceCount++;
                            blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            whitePerPlayerInfo.pieceCount++;
                            whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.NeutralOnCapture) != 0)
                    {
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Neutral, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Neutral, oldPiece));
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.StatusImmune) == 0)
                    {
                        if ((pteT.piecePropertyB & PiecePropertyB.FreezeCapturer) != 0)
                        {
                            oldPiece = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, oldPiece));

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, pteO.type, BoardUpdateMetadata.BoardUpdateType.StatusApply));
                            }
                            pieceChange = true;
                        }
                        if ((pteT.piecePropertyB & PiecePropertyB.PoisonCapturer) != 0)
                        {
                            oldPiece = Piece.SetPieceStatusEffect(PieceStatusEffect.Poisoned, Piece.SetPieceStatusDuration(3, oldPiece));

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, pteO.type, BoardUpdateMetadata.BoardUpdateType.StatusApply));
                            }
                            pieceChange = true;
                        }
                    }

                    if (boardUpdateMetadata != null)
                    {
                        if (pieceChange)
                        {
                            if (oldPiece == 0)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, opt, BoardUpdateMetadata.BoardUpdateType.Capture, true));
                            }
                            else
                            {
                                if (opt == Piece.GetPieceType(oldPiece))
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.GetPieceType(oldPiece), BoardUpdateMetadata.BoardUpdateType.TypeChange, true));
                                }
                                else
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.GetPieceType(oldPiece), BoardUpdateMetadata.BoardUpdateType.AlignmentChange, true));
                                }
                            }
                        }

                        if (residuePiece != 0)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, Piece.GetPieceType(residuePiece), BoardUpdateMetadata.BoardUpdateType.Spawn, false));
                        }
                    }
                }

                if (specialType == SpecialType.ChargeMove)
                {
                    if (Piece.GetPieceSpecialData(oldPiece) > 0)
                    {
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                    }
                }
                if (specialType == SpecialType.ChargeMoveReset)
                {
                    oldPiece = Piece.SetPieceSpecialData((byte)(0), oldPiece);
                }

                //pull moves over here
                switch (specialType) {
                    case Move.SpecialType.PullMove:
                        DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                        }

                        if (oldPiece == 0)
                        {
                            DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
                        }

                        //pull move is just normal with an extra step
                        (int pulldx, int pulldy) = Move.DirToDelta(dir);
                        pulldx = -pulldx;
                        pulldy = -pulldy;

                        if (fx + pulldx < 0 || fx + pulldx > 7 || fy + pulldy < 0 || fy + pulldy > 7)
                        {
                            //Illegal pull
                            //goto case Move.SpecialType.Normal;
                            break;
                        }

                        uint pullPiece = GetPieceAtCoordinate(fx + pulldx, fy + pulldy);

                        if (!(pullPiece != 0 && Piece.GetPieceAlignment(pullPiece) == opa))
                        {
                            //Illegal pull
                            //goto case Move.SpecialType.Normal;
                            break;
                        }

                        //Can't pull giants
                        PieceTableEntry pteP = GlobalPieceManager.GetPieceTableEntry(pullPiece);
                        if ((pteP.pieceProperty & PieceProperty.Giant) != 0 || (pteP.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)
                        {
                            //goto case Move.SpecialType.Normal;
                            break;
                        }

                        //Rare edge case with Knave + (pull mover)
                        if (tx + pulldx > 7 || tx + pulldx < 0 || ty + pulldy > 7 || ty + pulldy < 0)
                        {
                            break;
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx + pulldx, fy + pulldy, tx + pulldx, ty + pulldy, Piece.GetPieceType(pullPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        SetPieceAtCoordinate(tx + pulldx, ty + pulldy, pullPiece);
                        SetPieceAtCoordinate(fx + pulldx, fy + pulldy, 0);
                        break;
                    default:
                        DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                        }

                        if (oldPiece == 0)
                        {
                            DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
                        }
                        break;
                }
                break;
            case Move.SpecialType.Convert:
            case SpecialType.ConvertCaptureOnly:
                //
                if (passiveMove)
                {
                    goto case SpecialType.MoveOnly;
                    //SetPieceAtCoordinate(fx, fy, 0);
                    //SetPieceAtCoordinate(tx, ty, oldPiece);
                    //break;
                }

                //Convert target to my side
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;

                    //I get a piece
                    blackPerPlayerInfo.pieceCount++;
                    blackPerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;

                    //I get a piece
                    whitePerPlayerInfo.pieceCount++;
                    whitePerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }

                if (blackToMove)
                {
                    blackPerPlayerInfo.capturedLastTurn = true;
                }
                else
                {
                    whitePerPlayerInfo.capturedLastTurn = true;
                }

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, opt, BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                }

                if ((pteO.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, opt, BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                    }

                    if (opa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        blackPerPlayerInfo.pieceCount++;
                        blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                        SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                    }
                    if (opa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        whitePerPlayerInfo.pieceCount++;
                        whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                        SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                    }
                }

                if ((pteT.pieceProperty & PieceProperty.Giant) != 0)
                {
                    ushort pieceValue = Piece.GetPieceSpecialData(targetPiece);

                    int gdx = pieceValue & 1;
                    int gdy = (pieceValue & 2) >> 1;

                    //make this a +1 or -1
                    gdx *= 2;
                    gdx = 1 - gdx;
                    gdy *= 2;
                    gdy = 1 - gdy;

                    SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, targetPiece));
                    SetPieceAtCoordinate(tx + gdx, ty, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx + gdx, ty)));
                    SetPieceAtCoordinate(tx, ty + gdy, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx, ty + gdy)));
                    SetPieceAtCoordinate(tx + gdx, ty + gdy, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx + gdx, ty + gdy)));
                } else
                {
                    DeletePieceAtCoordinate(tx, ty, pteT, tpa, boardUpdateMetadata);
                    SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, targetPiece));
                }
                break;
            case Move.SpecialType.LongLeaper:
            case Move.SpecialType.LongLeaperCaptureOnly:
                (int lldx, int lldy) = Move.DirToDelta(dir);
                int tempX = fx;
                int tempY = fy;
                bool didCapture = false;
                while (tempX != tx || tempY != ty)
                {
                    if (dir == Dir.Null)
                    {
                        break;
                    }

                    tempX += lldx;
                    tempY += lldy;
                    uint leapTarget = GetPieceAtCoordinate(tempX, tempY);

                    //Debug.Log(fx + " " + fy + " " + tempX + " " + tempY + " " + opa + " " + Piece.GetPieceType(leapTarget) + " " + Piece.GetPieceAlignment(leapTarget) + " " + tx + " " + ty);

                    //delete if enemy
                    PieceTableEntry pteL = GlobalPieceManager.GetPieceTableEntry(leapTarget);
                    if (leapTarget != 0 && (Piece.GetPieceAlignment(leapTarget) != opa && !Piece.IsPieceInvincible(this, leapTarget, tempX, tempY, oldPiece, fx, fy, Move.SpecialType.LongLeaper, pteO, pteL)))
                    {
                        //Debug.Log("Delete at " + tempX + " " + tempY);
                        didCapture = true;
                        if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteL.pieceValueX2;
                        }
                        if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteL.pieceValueX2;
                        }
                        if (blackToMove)
                        {
                            blackPerPlayerInfo.capturedLastTurn = true;
                        }
                        else
                        {
                            whitePerPlayerInfo.capturedLastTurn = true;
                        }

                        //Delete piece at target
                        DeletePieceAtCoordinate(tempX, tempY, GlobalPieceManager.GetPieceTableEntry(leapTarget), Piece.GetPieceAlignment(leapTarget), boardUpdateMetadata);
                    }

                    //it is a bug if this happens
                    if (tempX < 0 || tempX > 7 || tempY < 0 || tempY > 7)
                    {
                        break;
                    }
                }
                if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust && didCapture)
                {
                    oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                    oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                }
                //move piece then
                SetPieceAtCoordinate(fx, fy, 0);
                SetPieceAtCoordinate(tx, ty, oldPiece);
                break;
            case Move.SpecialType.ConvertPawn:
                //
                if (passiveMove)
                {
                    goto case SpecialType.MoveOnly;
                    //SetPieceAtCoordinate(fx, fy, 0);
                    //SetPieceAtCoordinate(tx, ty, oldPiece);
                    //break;
                }

                //illegal conversion?
                //turns into normal move
                if (pteT.promotionType == PieceType.Null)
                {
                    goto case Move.SpecialType.Normal;
                }

                //Convert target to my side
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;

                    //I get a piece
                    blackPerPlayerInfo.pieceCount++;
                    blackPerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;

                    //I get a piece
                    whitePerPlayerInfo.pieceCount++;
                    whitePerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }

                if (blackToMove)
                {
                    blackPerPlayerInfo.capturedLastTurn = true;
                }
                else
                {
                    whitePerPlayerInfo.capturedLastTurn = true;
                }

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, opt, BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                }

                if ((pteT.pieceProperty & PieceProperty.Giant) != 0)
                {
                    ushort pieceValue = Piece.GetPieceSpecialData(targetPiece);

                    int gdx = pieceValue & 1;
                    int gdy = (pieceValue & 2) >> 1;

                    //make this a +1 or -1
                    gdx *= 2;
                    gdx = 1 - gdx;
                    gdy *= 2;
                    gdy = 1 - gdy;

                    if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                    {
                        oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                        oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                    }

                    SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, targetPiece));
                    SetPieceAtCoordinate(tx + gdx, ty, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx + gdx, ty)));
                    SetPieceAtCoordinate(tx, ty + gdy, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx, ty + gdy)));
                    SetPieceAtCoordinate(tx + gdx, ty + gdy, Piece.SetPieceAlignment(opa, GetPieceAtCoordinate(tx + gdx, ty + gdy)));
                }
                else
                {
                    if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                    {
                        oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                        oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                    }

                    DeletePieceAtCoordinate(tx, ty, pteT, tpa, boardUpdateMetadata);
                    SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, targetPiece));
                }
                break;
            case Move.SpecialType.FireCapture:
            case Move.SpecialType.FireCaptureOnly:
                if (passiveMove)
                {
                    //Move the piece to the spot
                    //Delete old piece
                    //SetPieceAtCoordinate(fx, fy, 0);
                    //Add new piece
                    //SetPieceAtCoordinate(tx, ty, oldPiece);
                    goto case SpecialType.MoveOnly;
                } else
                {
                    if (blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Greedy) != 0 && (globalData.whitePerPlayerInfo.startPieceCount - whitePerPlayerInfo.pieceCount) < 3)
                    {
                        if (tpa != opa)
                        {
                            //set to stationary
                            blackPerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
                            goto case SpecialType.Convert;
                        }
                    }

                    if (tpa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (tpa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (blackToMove)
                    {
                        blackPerPlayerInfo.capturedLastTurn = true;
                    }
                    else
                    {
                        whitePerPlayerInfo.capturedLastTurn = true;
                    }

                    /*
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tx + pushdx, ty + pushdy, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                    }
                     */

                    //fire capturers get fire capture free instead of using charges
                    //(fire capture + charge enhance means the enhanced thing is something better)
                    if (((pteO.pieceProperty & PieceProperty.ChargeEnhance) != 0 || (pteO.piecePropertyB & (PiecePropertyB.ChargeEnhanceStack)) != 0))
                    {
                        if ((pteO.pieceProperty & PieceProperty.FireCapture) == 0)
                        {
                            //Pay for fire capture
                            if (Piece.GetPieceSpecialData(oldPiece) > 0)
                            {
                                oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                                SetPieceAtCoordinate(fx, fy, oldPiece);
                            }
                        }
                        else
                        {
                            //Get point for fire capture
                            oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) + 1), oldPiece);
                            SetPieceAtCoordinate(fx, fy, oldPiece);
                        }
                    }
                    if ((pteO.piecePropertyB & (PiecePropertyB.ChargeEnhanceStackReset)) != 0)
                    {
                        if (Piece.GetPieceSpecialData(oldPiece) > 0)
                        {
                            oldPiece = Piece.SetPieceSpecialData((byte)(0), oldPiece);
                            SetPieceAtCoordinate(fx, fy, oldPiece);
                        }
                    }

                    //Outlaw switching sides
                    if ((pteO.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, opt, BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            blackPerPlayerInfo.pieceCount++;
                            blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            whitePerPlayerInfo.pieceCount++;
                            whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                        }
                    }

                    //Delete piece at target
                    //SetPieceAtCoordinate(tx, ty, 0);
                    DeletePieceAtCoordinate(tx, ty, pteT, tpa, boardUpdateMetadata);
                }
                break;
            case Move.SpecialType.FireCapturePush:
            case Move.SpecialType.PushMove:
                //push move with no push is just Normal
                if (!(targetPiece != 0 && tpa == opa))
                {
                    if (specialType == Move.SpecialType.FireCapturePush)
                    {
                        goto case Move.SpecialType.FireCapture;
                    }
                    else
                    {
                        goto case Move.SpecialType.Normal;
                    }
                }

                //Precondition means it is known what is to be pushed
                (int pushdx, int pushdy) = Move.DirToDelta(dir);

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tx + pushdx, ty + pushdy, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                }

                //Push piece towards delta
                SetPieceAtCoordinate(tx + pushdx, ty + pushdy, GetPieceAtCoordinate(tx, ty));

                //Delete old piece
                SetPieceAtCoordinate(fx, fy, 0);
                //Add new piece
                SetPieceAtCoordinate(tx, ty, oldPiece);
                break;
            case Move.SpecialType.RangedPush:
            case Move.SpecialType.RangedPushAllyOnly:
                //Assume that the move generator pre checked this so the push is valid
                (int rpushdx, int rpushdy) = Move.DirToDelta(dir);
                //priestess gets a bigger push
                if (opt == PieceType.ArcanaPriestess)
                {
                    TryPiecePushStrong(tx, ty, rpushdx, rpushdy, pteT, boardUpdateMetadata);
                }
                else
                {
                    TryPiecePush(tx, ty, rpushdx, rpushdy, pteT, boardUpdateMetadata);
                    //SetPieceAtCoordinate(tx + rpushdx, ty + rpushdy, GetPieceAtCoordinate(tx, ty));
                    //SetPieceAtCoordinate(tx, ty, 0);
                }
                break;
            case Move.SpecialType.RangedPull:
            case Move.SpecialType.RangedPullAllyOnly:
                //Assume that the move generator pre checked this so the pull is valid
                (int rpulldx, int rpulldy) = Move.DirToDelta(dir);
                if (opt == PieceType.ArcanaPriestess)
                {
                    TryPiecePullStrong(tx, ty, rpulldx, rpulldy, pteT, boardUpdateMetadata);
                } else
                {
                    TryPiecePull(tx, ty, rpulldx, rpulldy, pteT, boardUpdateMetadata);
                    //SetPieceAtCoordinate(tx + rpulldx, ty + rpulldy, GetPieceAtCoordinate(tx, ty));
                    //SetPieceAtCoordinate(tx, ty, 0);
                }
                break;
            case Move.SpecialType.AdvancerWithdrawer:
                //Similar coordinate logic to push
                (int a2dx, int a2dy) = Move.DirToDelta(dir);
                if (tx + a2dx < 0 || tx + a2dx > 7 || ty + a2dy < 0 || ty + a2dy > 7)
                {
                    //No advancer capture
                    goto case Move.SpecialType.Withdrawer;
                }
                uint a2Piece = GetPieceAtCoordinate(tx + a2dx, ty + a2dy);
                if (!(a2Piece != 0 && Piece.GetPieceAlignment(a2Piece) != opa))
                {
                    //No advancer capture
                    goto case Move.SpecialType.Withdrawer;
                }
                //Do advancer capture
                PieceTableEntry pteA2 = GlobalPieceManager.GetPieceTableEntry(a2Piece);
                if (Piece.IsPieceInvincible(this, a2Piece, tx + a2dx, ty + a2dy, oldPiece, fx, fy, Move.SpecialType.Advancer, pteO, pteA2))
                {
                    goto case Move.SpecialType.Withdrawer;
                }

                if (Piece.GetPieceAlignment(a2Piece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteA2.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(a2Piece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteA2.pieceValueX2;
                }
                if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                {
                    oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                    oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                }
                DeletePieceAtCoordinate(tx + a2dx, ty + a2dy, GlobalPieceManager.GetPieceTableEntry(a2Piece), opa, boardUpdateMetadata);
                goto case Move.SpecialType.Withdrawer;
            case Move.SpecialType.AdvancerPush:
            case Move.SpecialType.Advancer:
            case Move.SpecialType.WrathCapturer:
                //Advancer Push becomes Push if it's pushing
                if (specialType == SpecialType.AdvancerPush && (targetPiece != 0 && tpa == opa))
                {
                    goto case Move.SpecialType.PushMove;
                }
                if (specialType == Move.SpecialType.WrathCapturer && passiveMove)
                {
                    goto case Move.SpecialType.MoveOnly;
                }
                //Similar coordinate logic to push
                (int advdx, int advdy) = Move.DirToDelta(dir);
                if (tx + advdx < 0 || tx + advdx > 7 || ty + advdy < 0 || ty + advdy > 7)
                {
                    //No advancer capture
                    goto case Move.SpecialType.MoveOnly;
                }
                uint advPiece = GetPieceAtCoordinate(tx + advdx, ty + advdy);
                if (!(advPiece != 0 && Piece.GetPieceAlignment(advPiece) != opa))
                {
                    //No advancer capture
                    goto case Move.SpecialType.MoveOnly;
                }
                PieceTableEntry pteA = GlobalPieceManager.GetPieceTableEntry(advPiece);
                if (Piece.IsPieceInvincible(this, advPiece, tx + advdx, ty + advdy, oldPiece, fx, fy, Move.SpecialType.Advancer, pteO, pteA))
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                if (specialType == SpecialType.PoisonFlankingAdvancer)
                {
                    //toxic the advanced thing
                    if ((pteA.piecePropertyB & PiecePropertyB.StatusImmune) != 0)
                    {
                        goto case Move.SpecialType.MoveOnly;
                    }

                    pieces[tx + advdx + ((ty + advdy) << 3)] = Piece.SetPieceStatusEffect(Piece.PieceStatusEffect.Poisoned, Piece.SetPieceStatusDuration(3, pieces[tx + advdx + ((ty + advdy) << 3)]));

                    goto case Move.SpecialType.MoveOnly;
                }
                //Do advancer capture
                if (Piece.GetPieceAlignment(advPiece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteA.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(advPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteA.pieceValueX2;
                }
                if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                {
                    oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                    oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                }
                DeletePieceAtCoordinate(tx + advdx, ty + advdy, GlobalPieceManager.GetPieceTableEntry(advPiece), opa, boardUpdateMetadata);
                if (specialType == Move.SpecialType.WrathCapturer)
                {
                    goto case Move.SpecialType.Normal;
                } else
                {
                    goto case Move.SpecialType.MoveOnly;
                }
            case Move.SpecialType.Withdrawer:
                //Similar coordinate logic to pull
                (int withdx, int withdy) = Move.DirToDelta(dir);
                withdx = -withdx;
                withdy = -withdy;
                if (fx + withdx < 0 || fx + withdx > 7 || fy + withdy < 0 || fy + withdy > 7)
                {
                    //No withdraw capture
                    goto case Move.SpecialType.MoveOnly;
                }
                uint withPiece = GetPieceAtCoordinate(fx + withdx, fy + withdy);
                if (!(withPiece != 0 && Piece.GetPieceAlignment(withPiece) != opa))
                {
                    //No withdraw capture
                    goto case Move.SpecialType.MoveOnly;
                }
                PieceTableEntry pteW = GlobalPieceManager.GetPieceTableEntry(withPiece);
                if (Piece.IsPieceInvincible(this, withPiece, tx + withdx, ty + withdy, oldPiece, fx, fy, Move.SpecialType.Withdrawer, pteO, pteW))
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                //Do withdraw capture
                if (Piece.GetPieceAlignment(withPiece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteW.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(withPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteW.pieceValueX2;
                }
                if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                {
                    oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                    oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                }
                DeletePieceAtCoordinate(fx + withdx, fy + withdy, pteW, opa, boardUpdateMetadata);
                goto case Move.SpecialType.MoveOnly;
            case SpecialType.FlankingCapturer:
            case SpecialType.PoisonFlankingAdvancer:
                ulong bitindexF = MainManager.SmearBitboard(1uL << fx + (fy << 3));
                ulong bitindexT = MainManager.SmearBitboard(1uL << tx + (ty << 3));
                ulong flankBitboard = bitindexF & bitindexT;
                while (flankBitboard != 0)
                {
                    int index = MainManager.PopBitboardLSB1(flankBitboard, out flankBitboard);

                    int flankX = index & 7;
                    int flankY = index >> 3;

                    uint flankPiece = GetPieceAtCoordinate(flankX, flankY);

                    if (flankPiece == 0 || Piece.GetPieceAlignment(flankPiece) == opa)
                    {
                        continue;
                    }

                    PieceTableEntry pteF = GlobalPieceManager.GetPieceTableEntry(flankPiece);
                    if (Piece.IsPieceInvincible(this, flankPiece, flankX, flankY, oldPiece, fx, fy, Move.SpecialType.Withdrawer, pteO, pteF))
                    {
                        goto case Move.SpecialType.MoveOnly;
                    }

                    if (Piece.GetPieceAlignment(flankPiece) == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteF.pieceValueX2;
                    }
                    if (Piece.GetPieceAlignment(flankPiece) == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteF.pieceValueX2;
                    }

                    /*
                    if (Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Bloodlust)
                    {
                        oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                        oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                    }
                    */
                    DeletePieceAtCoordinate(flankX, flankY, pteF, opa, boardUpdateMetadata);
                }
                if (specialType == SpecialType.PoisonFlankingAdvancer)
                {
                    goto case Move.SpecialType.Advancer;
                }
                goto case Move.SpecialType.MoveOnly;
            case Move.SpecialType.AllySwap:
            case Move.SpecialType.AnyoneSwap:
                if (boardUpdateMetadata != null)
                {
                    if (pieces[tx + (ty << 3)] != 0)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, fx, fy, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                    }
                }
                //Very straightforward
                SetPieceAtCoordinate(tx, ty, oldPiece);
                SetPieceAtCoordinate(fx, fy, targetPiece);
                break;
            case Move.SpecialType.Imbue:
            case Move.SpecialType.ImbueWinged:
                if (passiveMove)
                {
                    goto case Move.SpecialType.MoveOnly;
                }
                oldPiece = 0;
                SetPieceAtCoordinate(fx, fy, oldPiece);
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                switch (opt)
                {
                    case PieceType.GlassSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Spectral, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.ShieldSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Shielded, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.FeatherSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Winged, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                }
                break;
            case SpecialType.ChargeApplyModifier:
                switch (opt)
                {
                    case PieceType.DivineArtisan:
                    case PieceType.DivineApprentice:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Shielded, targetPiece));
                        break;
                }
                if (Piece.GetPieceSpecialData(oldPiece) > 0)
                {
                    oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                }
                SetPieceAtCoordinate(fx, fy, oldPiece);
                break;
            case SpecialType.ImbuePromote:
                if (passiveMove)
                {
                    goto case Move.SpecialType.MoveOnly;
                }
                oldPiece = 0;
                SetPieceAtCoordinate(fx, fy, oldPiece);
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                PieceTableEntry pteIP = GlobalPieceManager.Instance.GetPieceTableEntry(pteT.promotionType);
                if (tpa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceValueSumX2 += (short)(pteIP.pieceValueX2 - pteO.pieceValueX2);
                }
                if (tpa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 += (short)(pteIP.pieceValueX2 - pteO.pieceValueX2);
                }

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, pteT.promotionType, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                }

                SetPieceAtCoordinate(tx, ty, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, Piece.SetPieceType(pteT.promotionType, targetPiece))));
                break;
            case Move.SpecialType.MorphIntoTarget:
                if (targetPiece == 0)
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceValueSumX2 += (short)(pteT.pieceValueX2 - pteO.pieceValueX2);
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 += (short)(pteT.pieceValueX2 - pteO.pieceValueX2);
                }

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, opt, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                }

                //not a capture like so it just turns you into the target
                SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(opa, targetPiece));
                break;
            case Move.SpecialType.Spawn:
                switch (opt)
                {
                    case PieceType.Gemini:
                        oldPiece = Piece.SetPieceType(PieceType.GeminiTwin, oldPiece);
                        PieceTableEntry pteGT = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.GeminiTwin);   //Todo: make this a global?

                        //Modify the piece values
                        //This is a small minus to the piece value so the AI only splits when it can make some play that gets more material
                        //(but having 2 pieces means you get a bigger piece table bonus also)
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteGT.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteGT.pieceValueX2);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, PieceType.GeminiTwin, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, PieceType.GeminiTwin, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                        break;
                    case PieceType.Triknight:
                        oldPiece = Piece.SetPieceType(PieceType.Knight, oldPiece);
                        PieceTableEntry pteKnight = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Knight);   //Todo: make this a global?

                        //since you targetted an empty it is guaranteed you can spawn at least 2
                        int knightsMade = 2;

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                        //try to spawn on the opposite side too
                        int ox = (fx - tx) + fx;
                        int oy = (fy - ty) + fy;

                        bool dospawn = true;
                        if (ox < 0 || ox > 7 || oy < 0 || oy > 7)
                        {
                            dospawn = false;
                        }
                        if (dospawn && pieces[ox + (oy << 3)] != 0)
                        {
                            dospawn = false;
                        }
                        if (dospawn)
                        {
                            knightsMade++;
                            SetPieceAtCoordinate(ox, oy, oldPiece);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, PieceType.Knight, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, PieceType.Knight, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (knightsMade == 3)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, ox, oy, PieceType.Knight, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - knightsMade * pteKnight.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - knightsMade * pteKnight.pieceValueX2);
                        }
                        break;
                    case PieceType.Tribishop:
                        //try to spawn on the opposite side too
                        oldPiece = Piece.SetPieceType(PieceType.Bishop, oldPiece);
                        PieceTableEntry pteBishop = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Bishop);   //Todo: make this a global?

                        //since you targetted an empty it is guaranteed you can spawn at least 2
                        int bishopsMade = 2;

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                        //try to spawn on the opposite side too
                        int oxB = (fx - tx) + fx;
                        int oyB = (fy - ty) + fy;

                        bool dospawnB = true;
                        if (oxB < 0 || oxB > 7 || oyB < 0 || oyB > 7)
                        {
                            dospawnB = false;
                        }
                        if (dospawnB && pieces[oxB + (oyB << 3)] != 0)
                        {
                            dospawnB = false;
                        }
                        if (dospawnB)
                        {
                            bishopsMade++;
                            SetPieceAtCoordinate(oxB, oyB, oldPiece);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, PieceType.Bishop, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, PieceType.Bishop, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (bishopsMade == 3)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oxB, oyB, PieceType.Bishop, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - bishopsMade * pteBishop.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - bishopsMade * pteBishop.pieceValueX2);
                        }
                        break;
                    case PieceType.Birook:
                        oldPiece = Piece.SetPieceType(PieceType.Rook, oldPiece);
                        PieceTableEntry pteRook = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Rook);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, PieceType.Rook, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, PieceType.Rook, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRook.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRook.pieceValueX2);
                        }
                        break;
                    case PieceType.TrojanHorse:
                        //Spawn 3 pawns
                        PieceTableEntry ptePawn = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Pawn);   //Todo: make this a global?
                        int pawnCount = 0;

                        uint newPawn = Piece.SetPieceType(PieceType.Pawn, oldPiece);
                        for (int i = -1; i <= 1; i++)
                        {
                            if (fx + i < 0 || fx + i > 7 || pieces[(fx + i) + (ty << 3)] != 0)
                            {
                                continue;
                            }
                            pawnCount++;
                            SetPieceAtCoordinate(fx + i, ty, newPawn);

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, fx + i, ty, PieceType.Pawn, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pawnCount * ptePawn.pieceValueX2 - pteO.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pawnCount * ptePawn.pieceValueX2 - pteO.pieceValueX2);
                        }

                        oldPiece = 0;
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        break;
                    case PieceType.QueenLeech:
                        //try to spawn on the opposite side too (x mirrored only)
                        uint newLeech = Piece.SetPieceType(PieceType.Leech, oldPiece);
                        newLeech = Piece.SetPieceSpecialData(0, newLeech);
                        PieceTableEntry pteLeech = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Leech);   //Todo: make this a global?
                        SetPieceAtCoordinate(tx, ty, newLeech);
                        int leechCount = 1;

                        int leechX = (fx - tx) + fx;
                        if (leechX >= 0 && leechX <= 7 && pieces[leechX + (ty << 3)] == 0)
                        {
                            leechCount = 2;
                            SetPieceAtCoordinate(leechX, ty, newLeech);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, PieceType.Leech, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (leechCount == 2)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, leechX, ty, PieceType.Leech, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(leechCount * pteLeech.pieceValueX2);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(leechCount * pteLeech.pieceValueX2);
                        }

                        //lose 1 charge
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        break;
                }
                break;
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
                if (passiveMove)
                {
                    goto case SpecialType.MoveOnly;
                }
                Piece.PieceStatusEffect pse = PieceStatusEffect.Poisoned;
                if (specialType == SpecialType.InflictFreeze || specialType == SpecialType.InflictFreezeCaptureOnly) {
                    pse = PieceStatusEffect.Frozen;
                }
                byte pd = 4;
                switch (opt)
                {
                    case PieceType.Phantom:
                        pse = PieceStatusEffect.Ghostly;
                        break;
                    case PieceType.SparkMage:
                        pse = PieceStatusEffect.Sparked;
                        pd = 2; //needs a big buff, a forced move is probably strong enough
                        break;
                    case PieceType.SplashMage:
                        pse = PieceStatusEffect.Soaked;
                        break;
                    case PieceType.FloatMage:
                        pse = PieceStatusEffect.Light;
                        break;
                    case PieceType.GravityMage:
                        pse = PieceStatusEffect.Heavy;
                        break;
                }
                SetPieceAtCoordinate(tx, ty, Piece.SetPieceStatusDuration(pd, Piece.SetPieceStatusEffect(pse, targetPiece)));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.StatusApply));
                }
                break;
            case SpecialType.TeleportMirror:
                int tmx = 7 - tx;
                int tmy = ty;
                SetPieceAtCoordinate(tx, ty, 0);
                SetPieceAtCoordinate(tmx, ty, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tmx, ty, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case SpecialType.TeleportRecall:
                int trex = fx;
                int trey = fy;
                switch (opa)
                {
                    case PieceAlignment.White:
                        trey = fy - 1;
                        break;
                    case PieceAlignment.Black:
                        trey = fy + 1;
                        break;
                }
                SetPieceAtCoordinate(tx, ty, 0);
                SetPieceAtCoordinate(trex, trey, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, trex, trey, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case SpecialType.TeleportOpposite:
                if ((pteO.pieceProperty & PieceProperty.ChargeEnhance) != 0)
                {
                    if (Piece.GetPieceSpecialData(oldPiece) > 0)
                    {
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                    }
                }

                //move tx to opposite side
                //fx - (tx - fx)
                //(fx << 1) - tx
                int tox = (fx << 1) - tx;
                int toy = (fy << 1) - ty;
                SetPieceAtCoordinate(tx, ty, 0);
                SetPieceAtCoordinate(tox, toy, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tox, toy, Piece.GetPieceType(targetPiece), BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case SpecialType.CarryAlly:
                //Absorb ally into yourself
                SetPieceAtCoordinate(tx, ty, 0);
                if (tpa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                }
                if (tpa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                }
                oldPiece = Piece.SetPieceSpecialData((ushort)Piece.GetPieceType(targetPiece), oldPiece);
                SetPieceAtCoordinate(fx, fy, oldPiece);
                break;
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
                //Spawn an ally piece at the position
                //+1 piece count
                if (tpa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount++;
                }
                if (tpa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount++;
                }
                SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, Piece.SetPieceType((Piece.PieceType)Piece.GetPieceSpecialData(oldPiece), 0)));
                //remove it from inside the carry piece
                SetPieceAtCoordinate(fx, fy, Piece.SetPieceSpecialData(0, oldPiece));
                break;
            case Move.SpecialType.Castling:
                //We have to move the piece 3 away to the middle spot
                int dx = (tx - fx) >> 1;

                int hopX = (tx - dx);
                int allyX = (tx + dx);

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(allyX, ty, hopX, ty, Piece.GetPieceType(GetPieceAtCoordinate(allyX, ty)), BoardUpdateMetadata.BoardUpdateType.Shift));
                }

                //move king
                //SetPieceAtCoordinate(fx, fy, 0);
                SetPieceAtCoordinate(fx, fy, 0);
                //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa);
                SetPieceAtCoordinate(tx, ty, oldPiece);

                //move other thing
                SetPieceAtCoordinate(hopX, ty, GetPieceAtCoordinate(allyX, ty));
                SetPieceAtCoordinate(allyX, ty, 0);

                if (oldPiece != 0)
                {
                    if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.canCastle = false;
                    }
                    if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.canCastle = false;
                    }
                }
                break;
        }

        int px = tx;
        int py = ty;
        if (lastMoveStationary)
        {
            px = fx;
            py = fy;
        }

        if (!passiveMove && Move.SpecialMoveCaptureLike(specialType) && tpa != opa && Piece.GetPieceModifier(oldPiece) == PieceModifier.Golden)
        {
            SpawnGoldenPawn(px, py, opa, boardUpdateMetadata);
        }

        //I can shift these into the below switch case to optimize things slightly
        //but it is less data driven

        if ((pteO.pieceProperty & PieceProperty.PassivePush) != 0)
        {
            DoPassivePush(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.pieceProperty & PieceProperty.PassivePushStrong) != 0)
        {
            DoPassivePushStrong(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.pieceProperty & PieceProperty.PassivePull) != 0)
        {
            DoPassivePull(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.pieceProperty & PieceProperty.PassivePushDiag) != 0)
        {
            DoPassivePushDiag(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.pieceProperty & PieceProperty.PassivePullStrong) != 0)
        {
            DoPassivePullStrong(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.pieceProperty & PieceProperty.PassivePushStrongDiag) != 0)
        {
            DoPassivePushStrongDiag(px, py, opa, boardUpdateMetadata);
        }
        if ((pteO.piecePropertyB & PiecePropertyB.SpreadCure) != 0)
        {
            DoSpreadCure(px, py, opa, boardUpdateMetadata);
        }

        //special extra stuff
        switch (opt)
        {
            case PieceType.ArcanaEmperor:
                //no push if not right kind of movement
                //AE only moves orthogonally by default
                bool doAEMove = false;
                if (tx - fx == 1 && ty - fy == 0)
                {
                    doAEMove = true;
                }
                if (tx - fx == -1 && ty - fy == 0)
                {
                    doAEMove = true;
                }
                if (tx - fx == 0 && ty - fy == 1)
                {
                    doAEMove = true;
                }
                if (tx - fx == 0 && ty - fy == -1)
                {
                    doAEMove = true;
                }

                if (doAEMove)
                {
                    //There is probably a better way to do this but I need to do the pushes in a specific order
                    //No diagonal movement right now because it would be pretty overpowered?
                    switch (dir)
                    {
                        case Move.Dir.Down:
                            TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            //TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            break;
                        case Move.Dir.Left:
                            TryPiecePushAlly(fx - 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                            //TryPiecePushAlly(fx - 1, fy, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, -1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                            break;
                        case Move.Dir.Right:
                            TryPiecePushAlly(fx + 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                            //TryPiecePushAlly(fx + 1, fy, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 1, 0, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                            break;
                        case Move.Dir.Up:
                            TryPiecePushAlly(fx - 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                            //TryPiecePushAlly(fx, fy + 1, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 0, 1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                            break;
                        case Dir.DownLeft:
                            //order by down and left first
                            TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            break;
                        case Dir.DownRight:
                            TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            break;
                        case Dir.Null:
                            break;
                        case Dir.UpLeft:
                            TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            break;
                        case Dir.UpRight:
                            TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                            break;
                    }
                }
                break;
            case PieceType.Uranus:
                //4 piece swap seems overpowered?
                //It would probably be fine if it was a "both sides must not be empty" but that still seems exploitable
                //No vertical swappage because that seems overpowered? (Pull the enemy king up 2 squares if you sacrifice a Uranus on the piece in front of the king)
                //  But you lose point value in the process
                //ehh, why not, Uranuses are worth 7 points so a 7 point sacrifice is very high risk
                TryPieceSwapEnemy(px, py - 1, px, py + 1, opa, boardUpdateMetadata);
                TryPieceSwapEnemy(px - 1, py, px + 1, py, opa, boardUpdateMetadata);
                break;
            case PieceType.Libra:
                int uy = opa == PieceAlignment.Black ? -1 : 1;
                //Being able to rearrange 4 enemy pieces with 1 piece move is pretty overpowered
                //So nerfing this to 2 pieces
                //TryPiecePushEnemy(px - 2, py + uy, 0, -uy, opa, boardUpdateMetadata);
                TryPiecePushEnemy(px - 1, py + uy, 0, -uy, opa, boardUpdateMetadata);
                TryPiecePushEnemy(px + 1, py + uy, 0, -uy, opa, boardUpdateMetadata);
                //TryPiecePushEnemy(px + 2, py + uy, 0, -uy, opa, boardUpdateMetadata);
                //TryPiecePushEnemy(px - 2, py - uy, 0, uy, opa, boardUpdateMetadata);
                TryPiecePushEnemy(px - 1, py - uy, 0, uy, opa, boardUpdateMetadata);
                TryPiecePushEnemy(px + 1, py - uy, 0, uy, opa, boardUpdateMetadata);
                //TryPiecePushEnemy(px + 2, py - uy, 0, uy, opa, boardUpdateMetadata);
                break;
            case PieceType.Sludge:
                (int sdx, int sdy) = Move.DirToDelta(dir);
                int tempX = fx;
                int tempY = fy;
                while (tempX != tx || tempY != ty)
                {
                    if (dir == Dir.Null)
                    {
                        break;
                    }

                    uint leapTarget = GetPieceAtCoordinate(tempX, tempY);

                    if (leapTarget == 0)
                    {
                        //Spawn a Sludge Trail
                        pieces[tempX + (tempY << 3)] = Piece.SetPieceType(PieceType.SludgeTrail, oldPiece);

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount++;
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(tempX, tempY, Piece.PieceType.SludgeTrail, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }
                    }

                    tempX += sdx;
                    tempY += sdy;
                    //it is a bug if this happens
                    if (tempX < 0 || tempX > 7 || tempY < 0 || tempY > 7)
                    {
                        break;
                    }
                }
                break;
            case PieceType.Pincer:
                DoPincerCheck(px, py, opa, pteO, boardUpdateMetadata);
                break;
            case PieceType.ElectroPawn:
                int ey = opa == PieceAlignment.Black ? -1 : 1;
                TryPiecePushEnemy(px, py + 2 * ey, 0, -ey, opa, boardUpdateMetadata);
                break;
        }

        if (opt == PieceType.King && blackToMove && ((globalData.enemyModifier & EnemyModifier.Obelisk) != 0))
        {
            //no push if not right kind of movement
            bool doOMove = true;
            if (tx - fx > 1 || tx - fx < -1)
            {
                doOMove = false;
            }
            if (ty - fy > 1 || ty - fy < -1)
            {
                doOMove = false;
            }
            if (tx - fx == 0 && ty - fy == 0)
            {
                doOMove = false;
            }

            if (doOMove)
            {
                //There is probably a better way to do this but I need to do the pushes in a specific order
                //No diagonal movement right now because it would be pretty overpowered?
                switch (dir)
                {
                    case Move.Dir.Down:
                        TryPiecePushAlly(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        //TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Left:
                        TryPiecePushAlly(fx - 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        //TryPiecePushAlly(fx - 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Right:
                        TryPiecePushAlly(fx + 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        //TryPiecePushAlly(fx + 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Up:
                        TryPiecePushAlly(fx - 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        //TryPiecePushAlly(fx, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        break;
                    case Dir.DownLeft:
                        //order by down and left first
                        //TryPiecePushAlly(fx - 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.DownRight:
                        //TryPiecePushAlly(fx + 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.Null:
                        break;
                    case Dir.UpLeft:
                        //TryPiecePushAlly(fx - 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        break;
                    case Dir.UpRight:
                        //TryPiecePushAlly(fx + 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        break;
                }
            }
        }



        bool bonusMove = false;

        //you only get 1 bonus ply from bonus movers
        //So you get 
        if ((pteO.pieceProperty & PieceProperty.BonusMove) != 0 && bonusPly < 1)
        {
            if (blackToMove && opa == PieceAlignment.Black)
            {
                bonusMove = true;
            }
            if (!blackToMove && opa == PieceAlignment.White)
            {
                bonusMove = true;
            }
        }

        if (blackToMove && bonusPly < 1 && turn < 5 && (globalData.enemyModifier & EnemyModifier.Youthful) != 0)
        {
            bonusMove = true;
        }

        RunTurnEnd(blackToMove, bonusMove, boardUpdateMetadata);
        ply++;

        //Note: quiescence search will kind of break if the side to move got a bonus move
        //But that is probably fine, if you have 1 extra move you usually have enough time to undo any threats that come up (so rushing in with a queen valued thing you can get the queen out with your next move)
        if (!bonusMove)
        {
            bonusPly = 0;
            if (blackToMove)
            {
                turn++;
            }

            //RunTurnStart(!blackToMove);
            blackToMove = !blackToMove;
        } else
        {
            bonusPly++;
        }
    }
    public void ApplyGiantMove(uint oldPiece, Piece.PieceAlignment opa, int fx, int fy, int tx, int ty, PieceTableEntry pteO, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //First entry will be moving from f to t (but only for non stationary moves)
        //ehhh I decided against this
        /*
        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, pteO.type, BoardUpdateMetadata.BoardUpdateType.Move));
        }
        */

        //More complex than normal movement

        ulong bitindex = 1uL << fx + fy * 8;
        bitindex |= bitindex << 1;
        bitindex |= bitindex << 8;

        int pieceChange = 0;

        for (int i = 0; i <= 1; i++)
        {
            for (int j = 0; j <= 1; j++)
            {
                int targetX = tx + i;
                int targetY = ty + j;
                uint targetPiece = pieces[targetX + targetY * 8];
                //
                if (targetPiece != 0)
                {
                    //Enemy piece?
                    Piece.PieceType tpt = Piece.GetPieceType(targetPiece);
                    Piece.PieceAlignment tpa = Piece.GetPieceAlignment(targetPiece);
                    if (opa == tpa)
                    {
                        continue;
                    }

                    if (blackToMove)
                    {
                        blackPerPlayerInfo.capturedLastTurn = true;
                    }
                    else
                    {
                        whitePerPlayerInfo.capturedLastTurn = true;
                    }
                    PieceTableEntry pteT = GlobalPieceManager.Instance.GetPieceTableEntry(tpt);
                    if (tpa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (tpa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }

                    ulong hagBitboard = 0;
                    if (opa == PieceAlignment.White)
                    {
                        hagBitboard = globalData.bitboard_hagBlack;
                    }
                    else if (opa == PieceAlignment.Black)
                    {
                        hagBitboard = globalData.bitboard_hagWhite;
                    }

                    //Debug.Log("Hag check" + opa);
                    //MainManager.PrintBitboard(hagBitboard);

                    //destroy capturer
                    if (pieceChange < 2 && ((pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & PieceProperty.DestroyOnCapture) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((bitindex & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful))
                    {
                        oldPiece = 0;
                        pieceChange = 2;
                    }

                    //conversion
                    //Destroy Capturer / Destroy on Capture has precedence
                    if (pieceChange < 1 && (((pteT.pieceProperty & PieceProperty.MorphCapturer) != 0) ||
                        ((pteT.pieceProperty & PieceProperty.MorphCapturerNonPawn) != 0 && pteO.promotionType == PieceType.Null) ||
                        ((pteT.pieceProperty & PieceProperty.MorphCapturerPawn) != 0 && pteO.promotionType != PieceType.Null)))
                    {
                        oldPiece = Piece.SetPieceType(tpt, oldPiece);
                        pieceChange = 1;
                    }
                    //destroy target
                    DeletePieceAtCoordinate(targetX, targetY, pteT, tpa, boardUpdateMetadata);
                }
            }
        }

        //out of move loop: now do the move
        DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, 0);

        if (boardUpdateMetadata != null)
        {
            //update for the giant
            if (pieceChange == 2)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, pteO.type, BoardUpdateMetadata.BoardUpdateType.Capture, true));
            }

            if (pieceChange == 1)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.GetPieceType(oldPiece), BoardUpdateMetadata.BoardUpdateType.TypeChange, true));
            }
        }

        if (oldPiece == 0)
        {
            if (opa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceCount--;
                whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
            }
            if (opa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceCount--;
                blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
            }
        } else if (pieceChange == 1)
        {
            PieceTableEntry pteT = GlobalPieceManager.GetPieceTableEntry(oldPiece);
            if (opa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
            }
            if (opa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
            }
            PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
        }
        else
        {
            PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
        }
    }
    public void ApplyConsumableMove(uint move, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        (Move.ConsumableMoveType cmt, int x, int y) = Move.DecodeConsumableMove(move);

        PieceTableEntry pte;
        switch (cmt)
        {
            case ConsumableMoveType.None:
                break;
            case ConsumableMoveType.PocketRock:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Rock, Piece.SetPieceAlignment(PieceAlignment.Neutral, 0));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, PieceType.Rock, BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.PocketPawn:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Pawn, Piece.SetPieceAlignment(PieceAlignment.White, 0));
                whitePerPlayerInfo.pieceCount++;
                pte = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Pawn);
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, PieceType.Pawn, BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.PocketKnight:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Knight, Piece.SetPieceAlignment(PieceAlignment.White, 0));
                whitePerPlayerInfo.pieceCount++;
                pte = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Knight);
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, PieceType.Knight, BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.Shield:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Shielded, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Glass:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Spectral, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Wings:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Winged, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.WarpBack:
                int tx = x;
                int ty = 0;
                int dy = 1;
                int dx = 0;

                //try to search
                while (pieces[tx + ty * 8] != 0)
                {
                    tx += dx;
                    ty += dy;
                    if (tx == x && ty == y)
                    {
                        break;
                    }
                }

                pieces[tx + ty * 8] = GetPieceAtCoordinate(x, y);

                if (boardUpdateMetadata != null)
                {
                    //shift
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, Piece.GetPieceType(pieces[tx + ty * 8]), BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case ConsumableMoveType.Freeze:
                pieces[x + (y << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, pieces[x + (y << 3)]));
                break;
            case ConsumableMoveType.Phantom:
                pieces[x + (y << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Ghostly, Piece.SetPieceStatusDuration(3, pieces[x + (y << 3)]));
                break;
            case ConsumableMoveType.Promote:
                pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(pieces[x + (y << 3)]));
                pieces[x + (y << 3)] = Piece.SetPieceType(pte.promotionType, pieces[x + (y << 3)]);
                whitePerPlayerInfo.pieceValueSumX2 += (short)(GlobalPieceManager.Instance.GetPieceTableEntry(pte.promotionType).pieceValueX2 - pte.pieceValueX2);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, PieceType.Knight, BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.SplashCure:
                DoSpreadCure(x, y, PieceAlignment.White, boardUpdateMetadata);
                break;
        }
    }

    public void DeletePieceAtCoordinate(int x, int y, PieceTableEntry pte, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(GetPieceAtCoordinate(x, y)));
        if (pte == null)
        {
            return;
        }

        uint piece = GetPieceAtCoordinate(x, y);

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
        }

        if ((pte.pieceProperty & PieceProperty.Giant) != 0)
        {
            DeleteGiant(piece, x, y);
            return;
        }

        if ((pte.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(pieces[x + (y << 3)]) != 0)
        {
            PieceTableEntry newPte = GlobalPieceManager.Instance.GetPieceTableEntry((Piece.PieceType)Piece.GetPieceSpecialData(pieces[x + (y << 3)]));

            if (pa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= newPte.pieceValueX2;
            }
            if (pa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= newPte.pieceValueX2;
            }
            DeletePieceAtCoordinate(x, y, newPte, pa, boardUpdateMetadata);
            return;
        }

        bool specialPiece = true;
        switch (pte.type) {
            case PieceType.ArcanaMoon:
            case PieceType.MoonIllusion:
                DeleteArcanaMoon(pa == PieceAlignment.Black, boardUpdateMetadata);
                break;
            case PieceType.Revenant:
                //is the revenant power depleted
                if (Piece.GetPieceSpecialData(piece) != 0)
                {
                    specialPiece = false;
                    break;
                }
                TryReplaceRevenant(x, y, boardUpdateMetadata);
                break;
            case PieceType.Lich:
                //Lich is a rechargeable revenant (but starts without a charge)
                if (Piece.GetPieceSpecialData(piece) == 0)
                {
                    specialPiece = false;
                    break;
                }
                TryReplaceLich(x, y, piece, boardUpdateMetadata);
                break;
            case PieceType.BigSlime:
                specialPiece = false;
                SpawnSplitSlimes(x, y, boardUpdateMetadata);
                break;
            case PieceType.PawnStack:
                specialPiece = false;
                SpawnSplitPiece(x, y, PieceType.Pawn, boardUpdateMetadata);
                break;
            case PieceType.KangarooQueen:
                specialPiece = false;
                SpawnSplitPiece(x, y, PieceType.KangarooPrincess, boardUpdateMetadata);
                break;
            default:
                specialPiece = false;
                break;
        }
        if (!specialPiece && Piece.GetPieceModifier(piece) == PieceModifier.Phoenix)
        {
            TryReplacePhoenix(x, y, piece, boardUpdateMetadata);
        }

        SetPieceAtCoordinate(x, y, 0);
    }
    public void CapturePieceAtCoordinate(int x, int y, uint newPiece, PieceTableEntry pte, Piece.PieceAlignment pa, PieceTableEntry pteT, Piece.PieceAlignment paT, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        DeletePieceAtCoordinate(x, y, pteT, paT, boardUpdateMetadata);
        PlaceMovedPiece(newPiece, x, y, pte, pa);
    }
    public void DeletePieceMovedFromCoordinate(int x, int y, PieceTableEntry pte, Piece.PieceAlignment pa, uint residuePiece = 0)
    {
        //PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(GetPieceAtCoordinate(x, y)));

        if (pte != null && (pte.pieceProperty & PieceProperty.Giant) != 0)
        {
            DeleteGiant(GetPieceAtCoordinate(x, y), x, y);
            return;
        }

        SetPieceAtCoordinate(x, y, residuePiece);
    }
    public void DeleteArcanaMoon(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //Note: the capture check reduces point score by one of them
        //This is why I made Moon Illusion the same value as Arcana Moon (it doesn't increase when it spawns stuff)
        ulong bitboard = 0;
        if (black)
        {
            bitboard = globalData.bitboard_tarotMoonIllusionBlack;
        } else
        {
            bitboard = globalData.bitboard_tarotMoonIllusionWhite;
        }
        //MainManager.PrintBitboard(bitboard);

        while (bitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard, out bitboard);

            if (black)
            {
                blackPerPlayerInfo.pieceCount--;
            }
            else
            {
                whitePerPlayerInfo.pieceCount--;
            }

            if (boardUpdateMetadata != null)
            {
                //Double check is here
                if (pieces[index] != 0)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, Piece.PieceType.MoonIllusion, BoardUpdateMetadata.BoardUpdateType.Capture));
                }
            }

            //no double check
            //I'm just going to assume the bitboard is correct I guess
            pieces[index] = 0;
        }
    }

    public void SpawnGoldenPawn(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x;
        int ty = y;
        int dx = 0;
        int dy = 0;

        //Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                ty = 0;
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                ty = 7;
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                tx = 0;
                dy = 0;
                dx = 1;
                break;
            case PieceAlignment.Crystal:
                tx = 7;
                dy = 0;
                dx = -1;
                break;
        }

        //try to search
        while (pieces[tx + ty * 8] != 0)
        {
            tx += dx;
            ty += dy;
            if (tx == x && ty == y)
            {
                return;
            }
        }

        pieces[tx + ty * 8] = Piece.SetPieceType(PieceType.Pawn, Piece.SetPieceAlignment(pa, 0));

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, PieceType.Pawn, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Pawn);
        //bonus value
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    public void TryReplacePhoenix(int x, int y, uint piece, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x;
        int ty = y;
        int dx = 0;
        int dy = 0;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                ty = 0;
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                ty = 7;
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                tx = 0;
                dy = 0;
                dx = 1;
                break;
            case PieceAlignment.Crystal:
                tx = 7;
                dy = 0;
                dx = -1;
                break;
        }

        //try to search
        while (pieces[tx + ty * 8] != 0)
        {
            tx += dx;
            ty += dy;
            if (tx == x && ty == y)
            {
                return;
            }
        }

        pieces[tx + ty * 8] = GetPieceAtCoordinate(x, y);
        pieces[tx + ty * 8] = Piece.SetPieceModifier(PieceModifier.None, pieces[tx + ty * 8]);

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.GetPieceType(piece), BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(piece));
        //code will undo the piece value decrease
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    public void TryReplaceLich(int x, int y, uint piece, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x;
        int ty = y;
        int dx = 0;
        int dy = 0;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                ty = 0;
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                ty = 7;
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                tx = 0;
                dy = 0;
                dx = 1;
                break;
            case PieceAlignment.Crystal:
                tx = 7;
                dy = 0;
                dx = -1;
                break;
        }

        //try to search
        while (pieces[tx + ty * 8] != 0)
        {
            tx += dx;
            ty += dy;
            if (tx == x && ty == y)
            {
                return;
            }
        }

        pieces[tx + ty * 8] = GetPieceAtCoordinate(x, y);
        pieces[tx + ty * 8] = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(piece) - 1), pieces[tx + ty * 8]);

        if (boardUpdateMetadata != null)
        {
            //Spawn a new lich
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.PieceType.Lich, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Lich);
        //code will undo the piece value decrease
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    public void TryReplaceRevenant(int x, int y, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x;
        int ty = y;
        int dx = 0;
        int dy = 0;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                ty = 0;
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                ty = 7;
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                tx = 0;
                dy = 0;
                dx = 1;
                break;
            case PieceAlignment.Crystal:
                tx = 7;
                dy = 0;
                dx = -1;
                break;
        }

        //try to search
        while (pieces[tx + ty * 8] != 0)
        {
            tx += dx;
            ty += dy;
            if (tx == x && ty == y)
            {
                return;
            }
        }

        pieces[tx + ty * 8] = GetPieceAtCoordinate(x, y);
        pieces[tx + ty * 8] = Piece.SetPieceSpecialData(1, pieces[tx + ty * 8]);

        if (boardUpdateMetadata != null)
        {
            //Spawn a new lich
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, Piece.PieceType.Revenant, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Revenant);
        //code will undo the piece value decrease
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteR.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    //kill a big slime, spawn slimes behind 2
    public void SpawnSplitSlimes(int x, int y, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int dx = 0;
        int dy = -1;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                dy = 0;
                dx = -1;
                break;
            case PieceAlignment.Crystal:
                dy = 0;
                dx = 1;
                break;
        }

        if (y + dy < 0 || y + dy > 7 || x + dx < 0 || x + dx > 7)
        {
            return;
        }
        if (pieces[(x + dx) + (y + dy) * 8] != 0)
        {
            return;
        }

        PieceTableEntry pteS = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.Slime);

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx, y + dy, Piece.PieceType.Slime, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        //Spawn a thing
        pieces[(x + dx) + (y + dy) * 8] = Piece.SetPieceType(PieceType.Slime, GetPieceAtCoordinate(x, y));
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }

        if (y + dy * 2 < 0 || y + dy * 2 > 7 || x + dx * 2 < 0 || x + dx * 2 > 7)
        {
            return;
        }
        if (pieces[(x + dx * 2) + (y + dy * 2) * 8] != 0)
        {
            return;
        }

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx * 2, y + dy * 2, Piece.PieceType.Slime, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        //Spawn a thing
        pieces[(x + dx * 2) + (y + dy * 2) * 8] = Piece.SetPieceType(PieceType.Slime, GetPieceAtCoordinate(x, y));
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    public void SpawnSplitPiece(int x, int y, PieceType newPiece, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int dx = 0;
        int dy = -1;

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(GetPieceAtCoordinate(x, y));
        switch (pa)
        {
            case PieceAlignment.White:
                dy = -1;
                dx = 0;
                break;
            case PieceAlignment.Black:
                dy = 1;
                dx = 0;
                break;
            case PieceAlignment.Neutral:
                dy = 0;
                dx = -1;
                break;
            case PieceAlignment.Crystal:
                dy = 0;
                dx = 1;
                break;
        }

        if (y + dy < 0 || y + dy > 7 || x + dx < 0 || x + dx > 7)
        {
            return;
        }
        if (pieces[(x + dx) + (y + dy) * 8] != 0)
        {
            return;
        }

        PieceTableEntry pteS = GlobalPieceManager.Instance.GetPieceTableEntry(newPiece);

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx, y + dy, newPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        //Spawn a thing
        pieces[(x + dx) + (y + dy) * 8] = Piece.SetPieceType(newPiece, GetPieceAtCoordinate(x, y));
        if (pa == PieceAlignment.White)
        {
            whitePerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            whitePerPlayerInfo.pieceCount++;
        }
        if (pa == PieceAlignment.Black)
        {
            blackPerPlayerInfo.pieceValueSumX2 += pteS.pieceValueX2;
            blackPerPlayerInfo.pieceCount++;
        }
    }
    public void PlaceMovedPiece(uint piece, int x, int y, PieceTableEntry pte, Piece.PieceAlignment pa)
    {
        if (pte == null)
        {
            return;
        }

        if ((pte.pieceProperty & PieceProperty.Giant) != 0)
        {
            PlaceGiant(piece, x, y);
            return;
        }

        SetPieceAtCoordinate(x, y, piece);
    }
    public void PlaceGiant(uint piece, int x, int y)
    {
        //which corner of the giant is this
        ushort pieceValue = Piece.GetPieceSpecialData(piece);

        int dx = pieceValue & 1;
        int dy = (pieceValue & 2) >> 1;

        if (pieceValue != 0)
        {
            //force you to use 0
            PlaceGiant(Piece.SetPieceSpecialData(0, piece), x - dx, y - dy);
            return;
        }

        //make this a +1 or -1
        dx *= 2;
        dx = 1 - dx;
        dy *= 2;
        dy = 1 - dy;

        SetPieceAtCoordinate(x, y, Piece.SetPieceSpecialData(0, piece));
        SetPieceAtCoordinate(x + dx, y, Piece.SetPieceSpecialData(1, piece));
        SetPieceAtCoordinate(x, y + dy, Piece.SetPieceSpecialData(2, piece));
        SetPieceAtCoordinate(x + dx, y + dy, Piece.SetPieceSpecialData(3, piece));
    }

    public static (int, int) GetGiantDelta(uint piece)
    {
        ushort pieceValue = Piece.GetPieceSpecialData(piece);

        int gdx = pieceValue & 1;
        int gdy = (pieceValue & 2) >> 1;

        //make this a +1 or -1
        gdx *= 2;
        gdx = 1 - gdx;
        gdy *= 2;
        gdy = 1 - gdy;

        //The 4 corners are given by
        //x,y
        //x+gdx,y
        //x,y+gdy
        //x+gdx,y+gdy
        return (gdx, gdy);
    }
    public void DeleteGiant(uint piece, int x, int y)
    {
        //which corner of the giant is this
        ushort pieceValue = Piece.GetPieceSpecialData(piece);

        int dx = pieceValue & 1;
        int dy = (pieceValue & 2) >> 1;

        //make this a +1 or -1
        dx *= 2;
        dx = 1 - dx;
        dy *= 2;
        dy = 1 - dy;

        SetPieceAtCoordinate(x, y, 0);
        SetPieceAtCoordinate(x + dx, y, 0);
        SetPieceAtCoordinate(x, y + dy, 0);
        SetPieceAtCoordinate(x + dx, y + dy, 0);
    }
    public void SetupGiant(int x, int y)    //Coordinates of lower left corner (lowest index)
    {
        pieces[x + y * 8] = Piece.SetPieceSpecialData(0, pieces[x + y * 8]);
        pieces[x + y * 8 + 1] = Piece.SetPieceSpecialData(1, pieces[x + y * 8 + 1]);
        pieces[x + y * 8 + 8] = Piece.SetPieceSpecialData(2, pieces[x + y * 8 + 8]);
        pieces[x + y * 8 + 9] = Piece.SetPieceSpecialData(3, pieces[x + y * 8 + 9]);
    }


    public void RunTurnEnd(bool black, bool bonusMove, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //note: piece bitboards will be out of date as this is after ApplyMove

        //info required to fix stuff
        //Inefficient
        //How much of a save is it to make ApplyMove fix the bitboards in itself?
        //It would be a lot of conditionals to place stuff and fix the bitboards
        MoveGeneratorInfoEntry.GeneratePieceBitboards(this);

        ApplyAutoMovers(!black, boardUpdateMetadata);
        ApplyPromotion(black, boardUpdateMetadata);

        if (!bonusMove)
        {
            TickDownStatusEffects(black, boardUpdateMetadata);
        }

        ApplySquareEffectsTurnEnd(black, boardUpdateMetadata);
        ApplyZenithEffect(boardUpdateMetadata);
    }

    //this should be idempotent and also not have immediate effect
    //Currently set to be after move gen
    public void RunTurnStart(bool black)
    {
        MoveBitTable antiTable = globalData.mbtactiveInverse;
        if (globalData.mbtactiveInverse == null)
        {
            globalData.mbtactiveInverse = new MoveBitTable();
            antiTable = globalData.mbtactiveInverse;
        }
        antiTable.MakeInverse(globalData.mbtactive);

        //you
        Piece.PieceAlignment pa = black ? PieceAlignment.Black : PieceAlignment.White;

        ulong enemyBitboard = 0;
        if (pa == PieceAlignment.Black)
        {
            enemyBitboard = globalData.bitboard_piecesWhite;
        } else
        {
            enemyBitboard = globalData.bitboard_piecesBlack;
        }

        while (enemyBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(enemyBitboard, out enemyBitboard);

            (int subX, int subY) = Board.CoordinateConvertInverse(index);

            uint piece = pieces[index];

            //shielded piece that isn't you
            if (Piece.GetPieceModifier(piece) == PieceModifier.Shielded && Piece.GetPieceAlignment(piece) != pa)
            {
                //Check the inverse bitboard
                ulong subBitboard = antiTable.Get(subX, subY);

                //Debug.Log("Enemy shielded piece " + i + " targetters");
                //MainManager.PrintBitboard(subBitboard);

                while (subBitboard != 0)
                {
                    int indexB = MainManager.PopBitboardLSB1(subBitboard, out subBitboard);
                    if (indexB == -1)
                    {
                        break;
                    }

                    Piece.PieceAlignment cpA = Piece.GetPieceAlignment(pieces[indexB]);

                    //demote shielded to half shielded
                    //if one of your pieces attacks it
                    if (cpA == pa)
                    {
                        pieces[index] = Piece.SetPieceModifier(PieceModifier.HalfShielded, piece);
                    }
                }
            }
        }

        //Use table and anti table to generate stuff
        /*
        for (int i = 0; i < 64; i++)
        {

        }
        */
    }

    public void ApplyAutoMovers(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        
        /*
        globalData.bitboard_zombieBlack;
        globalData.bitboard_bladebeastBlack;
        globalData.bitboard_clockworksnapperBlack;
        globalData.bitboard_abominationBlack;
        */

        ulong enemyBitboard = 0;
        ulong enemySmeared = 0;
        ulong allyBitboard = 0;
        bool moveZombies = false;
        ulong emptyBitboard = ~globalData.bitboard_pieces;

        ulong clockworksnapper = 0;
        ulong bladebeast = 0;

        int kingIndex = 0;
        if (black)
        {
            allyBitboard = globalData.bitboard_piecesBlack;
            enemyBitboard = (globalData.bitboard_pieces & ~allyBitboard);
            enemySmeared = MainManager.SmearBitboard(enemyBitboard);

            moveZombies = whitePerPlayerInfo.capturedLastTurn;
            kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingWhite, out _);
            clockworksnapper = globalData.bitboard_clockworksnapperBlack;// & enemySmeared;     //Should work but it doesn't, why???
            bladebeast = globalData.bitboard_bladebeastBlack;// & enemySmeared;
        }
        else
        {
            allyBitboard = globalData.bitboard_piecesWhite;
            enemyBitboard = (globalData.bitboard_pieces & ~allyBitboard);
            enemySmeared = MainManager.SmearBitboard(enemyBitboard);

            moveZombies = blackPerPlayerInfo.capturedLastTurn;
            kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingBlack, out _);
            clockworksnapper = globalData.bitboard_clockworksnapperWhite;// & enemySmeared;
            bladebeast = globalData.bitboard_bladebeastWhite;// & enemySmeared;

            /*
            MainManager.PrintBitboard(clockworksnapper);
            MainManager.PrintBitboard(bladebeast);
            MainManager.PrintBitboard(enemySmeared);
            MainManager.PrintBitboard(clockworksnapper & enemySmeared);
            MainManager.PrintBitboard(bladebeast & enemySmeared);
            */
        }

        PieceTableEntry pteO = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.ClockworkSnapper);
        Piece.PieceAlignment opa = black ? PieceAlignment.Black : PieceAlignment.White;
        while (clockworksnapper != 0)
        {
            int index = MainManager.PopBitboardLSB1(clockworksnapper, out clockworksnapper);

            int fx = index & 7;
            int fy = index >> 3;
            int tx = fx;
            int ty = fy + (black ? -1 : 1);

            if (ty > 7 || ty < 0)
            {
                continue;
            }

            //target
            uint targetPiece = pieces[tx + ty * 8];
            if (targetPiece == 0)
            {
                continue;
            }

            uint oldPiece = pieces[fx + fy * 8];
            if (oldPiece == 0 || Piece.GetPieceType(oldPiece) != PieceType.ClockworkSnapper)
            {
                continue;
            }
            Piece.PieceAlignment tpa = Piece.GetPieceAlignment(targetPiece);

            bool attackSuccessful = false;
            (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
        }
        while (bladebeast != 0)
        {
            int index = MainManager.PopBitboardLSB1(bladebeast, out bladebeast);

            int fx = index & 7;
            int fy = index >> 3;
            int tx = fx;
            int ty = fy;

            //target
            /*
            uint targetPiece = pieces[tx + ty * 8];
            if (targetPiece == 0)
            {
                continue;
            }
            */
            uint targetPiece;

            uint oldPiece = pieces[fx + fy * 8];
            if (oldPiece == 0 || Piece.GetPieceType(oldPiece) != PieceType.BladeBeast)
            {
                continue;
            }
            Piece.PieceAlignment tpa;

            bool attackSuccessful = false;
            tx = fx;
            ty = fy + 1;
            if (tx > 7 || tx < 0 || ty > 7 || ty < 0)
            {
                continue;
            }
            targetPiece = pieces[tx + ty * 8];
            tpa = Piece.GetPieceAlignment(targetPiece);
            (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
            if (attackSuccessful)
            {
                continue;
            }
            tx = fx + 1;
            ty = fy;
            if (tx > 7 || tx < 0 || ty > 7 || ty < 0)
            {
                continue;
            }
            targetPiece = pieces[tx + ty * 8];
            tpa = Piece.GetPieceAlignment(targetPiece);
            (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
            if (attackSuccessful)
            {
                continue;
            }
            tx = fx;
            ty = fy - 1;
            if (tx > 7 || tx < 0 || ty > 7 || ty < 0)
            {
                continue;
            }
            targetPiece = pieces[tx + ty * 8];
            tpa = Piece.GetPieceAlignment(targetPiece);
            (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
            if (attackSuccessful)
            {
                continue;
            }
            tx = fx - 1;
            ty = fy;
            if (tx > 7 || tx < 0 || ty > 7 || ty < 0)
            {
                continue;
            }
            targetPiece = pieces[tx + ty * 8];
            tpa = Piece.GetPieceAlignment(targetPiece);
            (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
        }

        if (black)
        {
            moveZombies = whitePerPlayerInfo.capturedLastTurn;
        }
        else
        {
            moveZombies = blackPerPlayerInfo.capturedLastTurn;
        }

        if (moveZombies)
        {
            ulong zombieBitboard = 0;
            ulong abominationBitboard = 0;
            if (black)
            {
                zombieBitboard = globalData.bitboard_zombieBlack;
                abominationBitboard = globalData.bitboard_abominationBlack;
            }
            else
            {
                zombieBitboard = globalData.bitboard_zombieWhite;
                abominationBitboard = globalData.bitboard_abominationWhite;
            }

            while (zombieBitboard != 0)
            {
                int index = MainManager.PopBitboardLSB1(zombieBitboard, out zombieBitboard);

                int newindex = index + (black ? -8 : 8);
                if (newindex < 64 && newindex >= 0 && pieces[newindex] == 0)
                {
                    ulong bitindexOld = 1uL << newindex;
                    ulong bitindex = 1uL << index;
                    if (black) {
                        globalData.bitboard_pieces &= ~bitindexOld;
                        globalData.bitboard_piecesBlack &= ~bitindexOld;
                        globalData.bitboard_pieces |= bitindex;
                        globalData.bitboard_piecesBlack |= bitindex;
                    }
                    else
                    {
                        globalData.bitboard_pieces &= ~bitindexOld;
                        globalData.bitboard_piecesWhite &= ~bitindexOld;
                        globalData.bitboard_pieces |= bitindex;
                        globalData.bitboard_piecesWhite |= bitindex;
                    }

                    pieces[newindex] = pieces[index];
                    pieces[index] = 0;
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, newindex & 7, newindex >> 3, Piece.PieceType.Zombie, BoardUpdateMetadata.BoardUpdateType.Shift));
                    }
                }
            }
            while (abominationBitboard != 0)
            {
                int index = MainManager.PopBitboardLSB1(abominationBitboard, out abominationBitboard);

                Move.Dir dir = Move.Dir.Null;

                int kdx = (kingIndex & 7) - (index & 7);
                int kdy = (kingIndex >> 3) - (index >> 3);

                dir = Move.DeltaToDirSoft(kdx, kdy);

                int dxy = 0;
                switch (dir)
                {
                    case Move.Dir.DownLeft:
                        dxy = -9;
                        break;
                    case Move.Dir.Down:
                        dxy = -8;
                        break;
                    case Move.Dir.DownRight:
                        dxy = -7;
                        break;
                    case Move.Dir.Left:
                        dxy = -1;
                        break;
                    case Move.Dir.Null:
                        break;
                    case Move.Dir.Right:
                        dxy = 1;
                        break;
                    case Move.Dir.UpLeft:
                        dxy = 7;
                        break;
                    case Move.Dir.Up:
                        dxy = 8;
                        break;
                    case Move.Dir.UpRight:
                        dxy = 9;
                        break;
                }

                int newindex = index + dxy;

                //newindex is guaranteed to be in bounds?
                if (pieces[newindex] == 0)
                {
                    ulong bitindexOld = 1uL << newindex;
                    ulong bitindex = 1uL << index;
                    if (black)
                    {
                        globalData.bitboard_pieces &= ~bitindexOld;
                        globalData.bitboard_piecesBlack &= ~bitindexOld;
                        globalData.bitboard_pieces |= bitindex;
                        globalData.bitboard_piecesBlack |= bitindex;
                    }
                    else
                    {
                        globalData.bitboard_pieces &= ~bitindexOld;
                        globalData.bitboard_piecesWhite &= ~bitindexOld;
                        globalData.bitboard_pieces |= bitindex;
                        globalData.bitboard_piecesWhite |= bitindex;
                    }

                    pieces[newindex] = pieces[index];
                    pieces[index] = 0;
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, newindex & 7, newindex >> 3, Piece.PieceType.Abomination, BoardUpdateMetadata.BoardUpdateType.Shift));
                    }
                }
            }
        }

        (uint, bool) AutoMoverAttack(bool black, PieceTableEntry pteO, PieceAlignment opa, int fx, int fy, int tx, int ty, uint targetPiece, uint oldPiece, PieceAlignment tpa, List<BoardUpdateMetadata> boardUpdateMetadata)
        {
            //Does the attack bonk?
            //Note that this is specifically auto mover "attack" so no passive moves are allowed
            //Hardcoded not to work on the King so you can't do King Kamikaze strats
            if (opa == tpa || targetPiece == 0 || Piece.GetPieceType(targetPiece) == PieceType.King || Piece.IsPieceInvincible(this, targetPiece, tx, ty, oldPiece, fx, fy, SpecialType.CaptureOnly, pteO, GlobalPieceManager.GetPieceTableEntry(targetPiece)))
            {
                return (oldPiece, false);
            }

            //Try to attack now
            if (black)
            {
                blackPerPlayerInfo.capturedLastTurn = true;
            }
            else
            {
                whitePerPlayerInfo.capturedLastTurn = true;
            }

            Piece.PieceType tpt = Piece.GetPieceType(targetPiece);
            PieceTableEntry pteT = GlobalPieceManager.Instance.GetPieceTableEntry(tpt);

            bool pieceChange = false;

            //King is immutable
            //(Note that the way king capture is detected means that any time where you have a move that destroys your own king that counts against your opponent)
            if (pteO.type == PieceType.King)
            {
                pieceChange = true;
            }

            if (tpa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceCount--;
                whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
            }
            if (tpa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceCount--;
                blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
            }

            ulong hagBitboard = 0;
            if (opa == PieceAlignment.White)
            {
                hagBitboard = globalData.bitboard_hagBlack;
            }
            else if (opa == PieceAlignment.Black)
            {
                hagBitboard = globalData.bitboard_hagWhite;
            }

            //Debug.Log("Hag check" + opa);
            //MainManager.PrintBitboard(hagBitboard);

            //destroy capturer
            if (!pieceChange && ((pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & PieceProperty.DestroyOnCapture) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((1uL << fx + fy * 8 & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful))
            {
                oldPiece = 0;
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                pieceChange = true;
            }

            //conversion
            //Destroy Capturer / Destroy on Capture has precedence
            if (!pieceChange && (((pteT.pieceProperty & PieceProperty.MorphCapturer) != 0) ||
                ((pteT.pieceProperty & PieceProperty.MorphCapturerNonPawn) != 0 && pteO.promotionType == PieceType.Null) ||
                ((pteT.pieceProperty & PieceProperty.MorphCapturerPawn) != 0 && pteO.promotionType != PieceType.Null)))
            {
                oldPiece = Piece.SetPieceType(tpt, oldPiece);
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteT.pieceValueX2);
                }
                pieceChange = true;
            }

            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, pteO.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }

            DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, 0);
            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);

            if (oldPiece == 0)
            {
                DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
            }

            ulong bitindexOld = 1uL << (fx + fy * 8);
            ulong bitindex = 1uL << (tx + ty * 8);
            if (black)
            {
                globalData.bitboard_pieces &= ~bitindexOld;
                globalData.bitboard_piecesBlack &= ~bitindexOld;
                globalData.bitboard_piecesWhite &= ~bitindex;
                if (oldPiece == 0)
                {
                    globalData.bitboard_piecesBlack |= bitindex;
                    globalData.bitboard_pieces |= bitindex;
                }
            }
            else
            {
                globalData.bitboard_pieces &= ~bitindexOld;
                globalData.bitboard_piecesWhite &= ~bitindexOld;
                globalData.bitboard_piecesBlack &= ~bitindex;
                if (oldPiece == 0)
                {
                    globalData.bitboard_pieces |= bitindex;
                    globalData.bitboard_piecesWhite |= bitindex;
                }
            }

            return (oldPiece, true);
        }
    }
    public void ApplyPromotion(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int yLevel = 7;

        Piece.PieceAlignment targetAlignment = PieceAlignment.White;
        if (black)
        {
            targetAlignment = PieceAlignment.Black;
            yLevel = 0;
        }

        /*
        ulong checkBitboard = (targetAlignment == PieceAlignment.White ? globalData.bitboard_piecesWhite : globalData.bitboard_piecesBlack) & (MoveGeneratorInfoEntry.BITBOARD_PATTERN_RANK1 << (yLevel << 3));

        while (checkBitboard != 0)
        {
            int i = MainManager.PopBitboardLSB1(checkBitboard, out checkBitboard);

            uint piece = pieces[i + yLevel * 8];
            if (piece != 0 && Piece.GetPieceAlignment(piece) == targetAlignment)
            {
                Piece.PieceType pt = Piece.GetPieceType(piece);
                PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(pt);

                if (pte.promotionType != PieceType.Null)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i, yLevel, pte.type, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    pieces[i + yLevel * 8] = Piece.SetPieceType(pte.promotionType, piece);

                    if ((pte.pieceProperty & PieceProperty.Giant) != 0)
                    {
                        PlaceGiant(pieces[i + yLevel * 8], i, yLevel);
                    }

                    PieceTableEntry pteB = GlobalPieceManager.Instance.GetPieceTableEntry(pte.promotionType);

                    if (black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                }
            }
        }
        */

        for (int i = 0; i < 8; i++)
        {
            uint piece = pieces[i + yLevel * 8];
            if (piece != 0 && Piece.GetPieceAlignment(piece) == targetAlignment)
            {
                Piece.PieceType pt = Piece.GetPieceType(piece);
                PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(pt);

                if (pte.promotionType != PieceType.Null)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i, yLevel, pte.type, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    pieces[i + yLevel * 8] = Piece.SetPieceType(pte.promotionType, piece);

                    if ((pte.pieceProperty & PieceProperty.Giant) != 0)
                    {
                        PlaceGiant(pieces[i + yLevel * 8], i, yLevel);
                    }

                    PieceTableEntry pteB = GlobalPieceManager.Instance.GetPieceTableEntry(pte.promotionType);

                    if (black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    } else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                }
            }
        }
    }

    public void TickDownStatusEffects(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //Turns tick down at the end of the other player's turn
        //This is so that things applied to a King don't cause problems
        //(But in general I won't normally allow modified kings)
        PieceAlignment pa = black ? PieceAlignment.White : PieceAlignment.Black;

        ulong piecesToCheck = pa == PieceAlignment.White ? globalData.bitboard_piecesWhite : globalData.bitboard_piecesBlack;

        ulong processedSquares = 0;
        while (piecesToCheck != 0)
        {
            int i = MainManager.PopBitboardLSB1(piecesToCheck, out piecesToCheck);

            if (pieces[i] == 0)
            {
                continue;
            }
            if (((1uL << (i)) & processedSquares) != 0)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[i]) != pa)
            {
                continue;
            }

            if (Piece.GetPieceModifier(pieces[i]) == PieceModifier.HalfShielded)
            {
                pieces[i] = Piece.SetPieceModifier(0, pieces[i]);
            }

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[i]);
            if (pte.type == PieceType.SludgeTrail)
            {
                if (!black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                    blackPerPlayerInfo.pieceCount--;
                }
                else
                {
                    whitePerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                    whitePerPlayerInfo.pieceCount--;
                }
                pieces[i] = 0;
                continue;
            }

            if (turn != 0 && bonusPly == 0 && turn % 5 == 0)
            {
                PieceTableEntry pteB;
                if ((pte.piecePropertyB & PiecePropertyB.SeasonalSwapper) != 0)
                {
                    pteB = GlobalPieceManager.Instance.GetPieceTableEntry((Piece.PieceType)(pte.type + 1));
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                }
                if ((pte.piecePropertyB & PiecePropertyB.SeasonalSwapperB) != 0)
                {
                    pteB = GlobalPieceManager.Instance.GetPieceTableEntry((Piece.PieceType)(pte.type - 1));
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                }
            }

            if (turn >= 10)
            {
                if ((pte.piecePropertyB & PiecePropertyB.Fading) != 0)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }

                    DeletePieceAtCoordinate(i & 7, i >> 3, pte, pa, boardUpdateMetadata);
                    continue;
                }
            }

            byte psed = Piece.GetPieceStatusDuration(pieces[i]);
            if (psed == 0)
            {
                continue;
            }

            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(pieces[i]);

            psed--;
            if (psed == 0)
            {

                //Rip
                if (pse == PieceStatusEffect.Poisoned || pse == PieceStatusEffect.Sparked || pse == PieceStatusEffect.Bloodlust)
                {
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                    }

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }

                    DeletePieceAtCoordinate(i & 7, i >> 3, pte, pa, boardUpdateMetadata);
                    //pieces[i] = 0;
                    continue;
                }

                pieces[i] = Piece.SetPieceStatusEffect(0, pieces[i]);
            }
            pieces[i] = Piece.SetPieceStatusDuration(psed, pieces[i]);

            switch (pse)
            {
                case PieceStatusEffect.Light:
                    if ((pte.pieceProperty & PieceProperty.Giant) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)
                    {
                        break;
                    }
                    if (!black)
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesBlack |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    else
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesWhite |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
                case PieceStatusEffect.Heavy:
                    if ((pte.pieceProperty & PieceProperty.Giant) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)
                    {
                        break;
                    }
                    //directions opposite of Light
                    if (!black)
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesBlack |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    else
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesWhite |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
            }
        }

        /*
        for (int i = 0; i < 64; i++)
        {
            if (pieces[i] == 0)
            {
                continue;
            }
            if (((1uL << (i)) & processedSquares) != 0)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[i]) != pa)
            {
                continue;
            }

            if (Piece.GetPieceModifier(pieces[i]) == PieceModifier.HalfShielded)
            {
                pieces[i] = Piece.SetPieceModifier(0, pieces[i]);
            }

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[i]);
            if (pte.type == PieceType.SludgeTrail)
            {
                if (!black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                    blackPerPlayerInfo.pieceCount--;
                }
                else
                {
                    whitePerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                    whitePerPlayerInfo.pieceCount--;
                }
                pieces[i] = 0;
                continue;
            }

            if (turn != 0 && bonusPly == 0 && turn % 5 == 0)
            {
                PieceTableEntry pteB;
                if ((pte.piecePropertyB & PiecePropertyB.SeasonalSwapper) != 0)
                {
                    pteB = GlobalPieceManager.Instance.GetPieceTableEntry((Piece.PieceType)(pte.type + 1));
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                }
                if ((pte.piecePropertyB & PiecePropertyB.SeasonalSwapperB) != 0)
                {
                    pteB = GlobalPieceManager.Instance.GetPieceTableEntry((Piece.PieceType)(pte.type - 1));
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                    }
                    pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                }
            }

            if (turn >= 10)
            {
                if ((pte.piecePropertyB & PiecePropertyB.Fading) != 0)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }

                    DeletePieceAtCoordinate(i & 7, i >> 3, pte, pa, boardUpdateMetadata);
                    continue;
                }
            }

            byte psed = Piece.GetPieceStatusDuration(pieces[i]);
            if (psed == 0)
            {
                continue;
            }

            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(pieces[i]);

            psed--;
            if (psed == 0)
            {

                //Rip
                if (pse == PieceStatusEffect.Poisoned || pse == PieceStatusEffect.Sparked || pse == PieceStatusEffect.Bloodlust)
                {
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                    }

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }

                    DeletePieceAtCoordinate(i & 7, i >> 3, pte, pa, boardUpdateMetadata);
                    //pieces[i] = 0;
                    continue;
                }

                pieces[i] = Piece.SetPieceStatusEffect(0, pieces[i]);
            }
            pieces[i] = Piece.SetPieceStatusDuration(psed, pieces[i]);

            switch (pse)
            {
                case PieceStatusEffect.Light:
                    if ((pte.pieceProperty & PieceProperty.Giant) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)
                    {
                        break;
                    }
                    if (!black)
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesBlack |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    } else
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesWhite |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
                case PieceStatusEffect.Heavy:
                    if ((pte.pieceProperty & PieceProperty.Giant) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)
                    {
                        break;
                    }
                    //directions opposite of Light
                    if (!black)
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesBlack |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    else
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = pieces[i];
                            pieces[i] = 0;
                            globalData.bitboard_piecesWhite |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
            }
        }
        */
    }
    public bool TryPiecePushAlly(int x, int y, int dx, int dy, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7 || Piece.GetPieceAlignment(pieces[x + y * 8]) != pa)
        {
            return false;
        }

        return TryPiecePush(x, y, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]), boardUpdateMetadata);
    }
    public bool TryPiecePushEnemy(int x, int y, int dx, int dy, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7 || Piece.GetPieceAlignment(pieces[x + y * 8]) == pa)
        {
            return false;
        }

        return TryPiecePush(x, y, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]), boardUpdateMetadata);
    }
    public void DoPincerCheck(int x, int y, Piece.PieceAlignment pa, PieceTableEntry pteO, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        TryPincerCapture(x, y, 1, 0, pa, pteO, boardUpdateMetadata);
        TryPincerCapture(x, y, -1, 0, pa, pteO, boardUpdateMetadata);
        TryPincerCapture(x, y, 0, 1, pa, pteO, boardUpdateMetadata);
        TryPincerCapture(x, y, 0, -1, pa, pteO, boardUpdateMetadata);
    }
    public void TryPincerCapture(int x, int y, int dx, int dy, Piece.PieceAlignment pa, PieceTableEntry pteO, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int ox = x;
        int oy = y;

        x += dx;
        y += dy;

        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            return;
        }

        int px = x + dx;
        int py = y + dy;

        bool pincer = false;
        if (px < 0 || px > 7 || py < 0 || py > 7)
        {
            pincer = true;
        } else
        {
            if (pieces[x + dx + (y + dy) * 8] != 0 && Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
            {
                pincer = true;
            }
        }

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        if (pincer && pieces[x + y * 8] != 0 && Piece.GetPieceAlignment(pieces[x + y * 8]) != pa && !Piece.IsPieceInvincible(this, pieces[x + y * 8], x, y, pieces[ox + oy * 8], ox, oy, Move.SpecialType.Advancer, pteO, pte))
        {
            //Destroy x, y
            if (pte == null)
            {
                return;
            }

            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pte.type, BoardUpdateMetadata.BoardUpdateType.Capture));
            }

            Piece.PieceAlignment targetPA = Piece.GetPieceAlignment(pieces[x + y * 8]);
            if (targetPA == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
            }
            if (targetPA == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
            }

            DeletePieceAtCoordinate(x, y, pte, targetPA, boardUpdateMetadata);
        }
    }

    public bool TryPiecePush(int x, int y, int dx, int dy, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        return TryPiecePush(x, y, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]), boardUpdateMetadata);
    }
    public bool TryPiecePush(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x + dx;
        int ty = y + dy;

        if (pte == null)
        {
            return false;
        }

        if (tx < 0 || tx > 7)
        {
            return false;
        }
        if (ty < 0 || ty > 7)
        {
            return false;
        }

        //Debug.Log("Push weak");

        //Nothing to push
        if (pte == null)
        {
            return false;
        }

        if (pieces[tx + ty * 8] == 0 && (pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) == 0)
        {
            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }

            //blow wind towards ty
            pieces[tx + ty * 8] = pieces[x + y * 8];
            pieces[x + y * 8] = 0;
            return true;
        }

        return false;
    }
    public bool TryPieceSwapEnemy(int x, int y, int x2, int y2, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            return false;
        }
        if (x2 < 0 || x2 > 7 || y2 < 0 || y2 > 7)
        {
            return false;
        }
        if ((pieces[x + y * 8] != 0 && Piece.GetPieceAlignment(pieces[x + y * 8]) != pa) || (pieces[x2 + y2 * 8] != 0 && Piece.GetPieceAlignment(pieces[x2 + y2 * 8]) != pa))
        {
            return false;
        }

        PieceTableEntry pteA = GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pieces[x2 + y2 * 8]);

        if ((pteA != null && ((pteA.pieceProperty & PieceProperty.Giant) != 0 || (pteA.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)) || (pteB != null && ((pteB.pieceProperty & PieceProperty.Giant) != 0 || (pteB.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            if (pieces[x + (y << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pteA.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }
        if (boardUpdateMetadata != null)
        {
            if (pieces[x2 + (y2 << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x2, y2, x, y, pteB.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }

        uint temp = pieces[x + y * 8];
        pieces[x + y * 8] = pieces[x2 + y2 * 8];
        pieces[x2 + y2 * 8] = temp;
        return true;
    }
    public bool TryPieceSwap(int x, int y, int x2, int y2, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            return false;
        }
        if (x2 < 0 || x2 > 7 || y2 < 0 || y2 > 7)
        {
            return false;
        }

        PieceTableEntry pteA = GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pieces[x2 + y2 * 8]);

        if ((pteA != null && ((pteA.pieceProperty & PieceProperty.Giant) != 0 || (pteA.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)) || (pteB != null && ((pteB.pieceProperty & PieceProperty.Giant) != 0 || (pteB.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            if (pieces[x + (y << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pteA.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }
        if (boardUpdateMetadata != null)
        {
            if (pieces[x2 + (y2 << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pteA.type, BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }

        uint temp = pieces[x + y * 8];
        pieces[x + y * 8] = pieces[x2 + y2 * 8];
        pieces[x2 + y2 * 8] = temp;
        return true;
    }
    public bool TryPiecePull(int x, int y, int dx, int dy, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7 || pieces[x + y * 8] == 0)
        {
            return false;
        }

        return TryPiecePull(x, y, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]), boardUpdateMetadata);
    }
    public bool TryPiecePull(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //inverse push
        return TryPiecePush(x, y, -dx, -dy, pte, boardUpdateMetadata);
    }
    public bool TryPiecePullStrong(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //inverse push
        return TryPiecePushStrong(x, y, -dx, -dy, pte, boardUpdateMetadata);
    }
    public bool TryPiecePullRanged(int x, int y, int dx, int dy, Piece.PieceAlignment target, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x;
        int ty = y;

        tx += dx;
        ty += dy;
        if (tx < 0 || tx > 7 || ty < 0 || ty > 7)
        {
            return false;
        }

        //already an obstacle after 1 step
        if (pieces[tx + ty * 8] != 0)
        {
            return false;
        }

        while (pieces[tx + ty * 8] == 0)
        {
            tx += dx;
            ty += dy;
            if (tx < 0 || tx > 7)
            {
                return false;
            }
            if (ty < 0 || ty > 7)
            {
                return false;
            }
        }

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[tx + ty * 8]);
        if (Piece.GetPieceAlignment(pieces[tx + ty * 8]) == target || (pte != null && ((pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, (tx - dx), (ty - dy), pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
        }

        //obstacle at tx, ty, pull to 1 step earlier
        pieces[(tx - dx) + (ty - dy) * 8] = pieces[tx + ty * 8];
        pieces[tx + ty * 8] = 0;

        return true;
    }
    public bool TryPiecePushStrong(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (pte != null && ((pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.ShiftImmune) != 0))
        {
            return false;
        }

        int tx = x;
        int ty = y;

        tx += dx;
        ty += dy;

        if (tx < 0 || tx > 7 || ty < 0 || ty > 7)
        {
            return false;
        }

        //Nothing to push
        if (pieces[x + y * 8] == 0)
        {
            return false;
        }

        //Debug.Log("Push strong");

        //there's already an obstacle after 1 step
        if (pieces[tx + ty * 8] != 0)
        {
            return false;
        }

        while (pieces[tx + ty * 8] == 0)
        {
            //Debug.Log(x + " " + y + " " + dx + " " + dy + " " + tx + " " + ty + " " + Piece.GetPieceType(pieces[tx + ty * 8]));
            tx += dx;
            ty += dy;
            if (tx < 0 || tx > 7)
            {
                break;
            }
            if (ty < 0 || ty > 7)
            {
                break;
            }
        }

        //back 1 step
        tx -= dx;
        ty -= dy;

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
        }

        //Debug.Log("L: " + x + " " + y + " " + dx + " " + dy + " " + tx + " " + ty + " " + Piece.GetPieceType(pieces[tx + ty * 8]));
        pieces[tx + ty * 8] = pieces[x + y * 8];
        pieces[x + y * 8] = 0;

        return true;
    }
    public void DoPassivePush(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.Instance.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.Instance.orbiterDeltas[i][1];

            if (x + dx < 0 || x + dx > 7)
            {
                continue;
            }
            if (y + dy < 0 || y + dy > 7)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
            {
                continue;
            }

            TryPiecePush(x + dx, y + dy, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + dx + (y + dy) * 8]), boardUpdateMetadata);
        }
    }
    public void DoPassivePushStrong(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.Instance.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.Instance.orbiterDeltas[i][1];

            if (x + dx < 0 || x + dx > 7)
            {
                continue;
            }
            if (y + dy < 0 || y + dy > 7)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
            {
                continue;
            }

            //Debug.Log("Passive strong");
            TryPiecePushStrong(x + dx, y + dy, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + dx + (y + dy) * 8]), boardUpdateMetadata);
        }
    }
    public void DoPassivePull(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.Instance.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.Instance.orbiterDeltas[i][1];

            if (x + dx * 2 < 0 || x + dx * 2 > 7)
            {
                continue;
            }
            if (y + dy * 2 < 0 || y + dy * 2 > 7)
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[x + dx * 2 + (y + dy * 2) * 8]) == pa)
            {
                continue;
            }

            TryPiecePull(x + dx * 2, y + dy * 2, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + dx * 2 + (y + dy * 2) * 8]), boardUpdateMetadata);
        }
    }
    public void DoPassivePullStrong(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.Instance.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.Instance.orbiterDeltas[i][1];

            if (x + dx < 0 || x + dx > 7)
            {
                continue;
            }
            if (y + dy < 0 || y + dy > 7)
            {
                continue;
            }

            TryPiecePullRanged(x, y, dx, dy, pa, boardUpdateMetadata);
        }
    }
    public void DoPassivePushDiag(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int dx = i * 2 - 1;
                int dy = j * 2 - 1;

                if (x + dx < 0 || x + dx > 7)
                {
                    continue;
                }
                if (y + dy < 0 || y + dy > 7)
                {
                    continue;
                }

                if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
                {
                    continue;
                }

                TryPiecePush(x + dx, y + dy, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + dx + (y + dy) * 8]), boardUpdateMetadata);
            }
        }
    }
    public void DoPassivePushStrongDiag(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int dx = i * 2 - 1;
                int dy = j * 2 - 1;

                if (x + dx < 0 || x + dx > 7)
                {
                    continue;
                }
                if (y + dy < 0 || y + dy > 7)
                {
                    continue;
                }

                if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
                {
                    continue;
                }

                //Debug.Log("Passive strong");
                TryPiecePushStrong(x + dx, y + dy, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + dx + (y + dy) * 8]), boardUpdateMetadata);
            }
        }
    }
    public void DoSpreadCure(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int dx = i;
                int dy = j;
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                if (x + dx < 0 || x + dx > 7)
                {
                    continue;
                }
                if (y + dy < 0 || y + dy > 7)
                {
                    continue;
                }

                uint piece = pieces[x + dx + ((y + dy) << 3)];
                if (Piece.GetPieceAlignment(piece) != pa)
                {
                    continue;
                }

                //Debug.Log("Splash cure " + (x + dx) + " " + (y + dy));

                if (Piece.GetPieceStatusEffect(piece) == PieceStatusEffect.None)
                {
                    continue;
                }

                pieces[x + dx + ((y + dy) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.None, Piece.SetPieceStatusDuration(0, piece));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x + dx, y + dy, Piece.GetPieceType(piece), BoardUpdateMetadata.BoardUpdateType.StatusCure));
                }
            }
        }
    }

    public void ApplySquareEffectsTurnEnd(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //Rebuild the piece bitboards
        //MoveGeneratorInfoEntry.GeneratePieceBitboards(this);

        ulong whitePieces = 0;
        ulong blackPieces = 0;

        whitePieces = globalData.bitboard_piecesWhite;
        blackPieces = globalData.bitboard_piecesBlack;

        //To prevent a piece from being affected by multiple squares (possible scan order shenanigans) I will mark squares with pieces moved around
        //Otherwise a piece might be pushed onto a square that gets processed later which pushes the piece again and so on
        ulong processedSquares = 0;


        uint lastMove = 0;
        if (black)
        {
            lastMove = blackPerPlayerInfo.lastMove;
        }
        else
        {
            lastMove = whitePerPlayerInfo.lastMove;
        }

        int lastMoveIndex = Move.GetToX(lastMove) + 8 * Move.GetToY(lastMove);
        bool lastMoveStationary = Move.SpecialMoveStationary(Move.GetSpecialType(lastMove));
        if (lastMoveStationary)
        {
            //last moved piece is on lastMoveIndex
            lastMoveIndex = Move.GetFromX(lastMove) + 8 * Move.GetFromY(lastMove);
        }

        processedSquares |= 1uL << lastMoveIndex;

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[lastMoveIndex]);

        bool modifierMovement = false;

        if ((turn & 1) == 1 && pte != null && ((pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & PiecePropertyB.ShiftImmune) == 0))
        {
            if (!blackToMove && (globalData.enemyModifier & EnemyModifier.Mesmerizing) != 0)
            {
                //Up push
                if (lastMoveIndex < 56 && pieces[lastMoveIndex + 8] == 0)
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                    }

                    //blow wind upwards
                    pieces[lastMoveIndex + 8] = pieces[lastMoveIndex];
                    pieces[lastMoveIndex] = 0;
                    processedSquares |= 1uL << lastMoveIndex + 8;
                    modifierMovement = true;
                }
            }

            if (!modifierMovement && !blackToMove && (globalData.enemyModifier & EnemyModifier.Rifter) != 0)
            {
                modifierMovement = true;

                if ((lastMoveIndex & 7) < 4)
                {
                    //left 
                    if ((lastMoveIndex & 7) > 0 && pieces[lastMoveIndex - 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) - 1, (lastMoveIndex >> 3), pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[lastMoveIndex - 1] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex - 1;
                        modifierMovement = true;
                    }
                } else
                {
                    //right
                    if ((lastMoveIndex & 7) < 7 && pieces[lastMoveIndex + 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) + 1, (lastMoveIndex >> 3), pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[lastMoveIndex + 1] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex + 1;
                        modifierMovement = true;
                    }
                }
            }
        }

        if (!modifierMovement && pte != null && (globalData.squares[lastMoveIndex].type == Square.SquareType.Promotion || ((pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & PiecePropertyB.ShiftImmune) == 0)))
        {
            switch (globalData.squares[lastMoveIndex].type)
            {
                case Square.SquareType.WindUp:
                    if (lastMoveIndex < 56 && pieces[lastMoveIndex + 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) + 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        //blow wind upwards
                        pieces[lastMoveIndex + 8] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex + 8;
                    }
                    break;
                case Square.SquareType.WindDown:
                    if (lastMoveIndex > 7 && pieces[lastMoveIndex - 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) - 1, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[lastMoveIndex - 8] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex - 8;
                    }
                    break;
                case Square.SquareType.WindLeft:
                    if ((lastMoveIndex & 7) > 0 && pieces[lastMoveIndex - 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) - 1, (lastMoveIndex >> 3), pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[lastMoveIndex - 1] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex - 1;
                    }
                    break;
                case Square.SquareType.WindRight:
                    if ((lastMoveIndex & 7) < 7 && pieces[lastMoveIndex + 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) + 1, (lastMoveIndex >> 3), pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[lastMoveIndex + 1] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex + 1;
                    }
                    break;
                case Square.SquareType.Slippery:
                    int iceDx = 0;
                    int iceDy = 0;
                    if (!lastMoveStationary && Move.GetDir(lastMove) != Move.Dir.Null)
                    {
                        (iceDx, iceDy) = Move.DirToDelta(Move.GetDir(lastMove));

                        iceDx += lastMoveIndex & 7;
                        iceDy += (lastMoveIndex & 56) >> 3;

                        //check ice move legality
                        if (iceDx >= 0 && iceDx <= 7 && iceDy >= 0 && iceDy <= 7 && pieces[iceDx + iceDy * 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, iceDx, iceDy, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            processedSquares |= 1uL << (iceDx + iceDy * 8);
                            pieces[iceDx + iceDy * 8] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                        }
                    }
                    break;
                case Square.SquareType.Bouncy:
                    int bouncyDx = 0;
                    int bouncyDy = 0;
                    if (!lastMoveStationary && Move.GetDir(lastMove) != Move.Dir.Null)
                    {
                        (bouncyDx, bouncyDy) = Move.DirToDelta(Move.GetDir(lastMove));
                        bouncyDx = -bouncyDx;
                        bouncyDy = -bouncyDy;
                        bouncyDx += lastMoveIndex & 7;
                        bouncyDy += (lastMoveIndex & 56) >> 3;

                        //bouncy move legality
                        if (bouncyDx >= 0 && bouncyDx <= 7 && bouncyDy >= 0 && bouncyDy <= 7 && pieces[bouncyDx + bouncyDy * 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, bouncyDx, bouncyDy, pte.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            processedSquares |= 1uL << (bouncyDx + bouncyDy * 8);
                            pieces[bouncyDx + bouncyDy * 8] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                        }
                    }
                    break;
                case Square.SquareType.Promotion:
                    if (pte.promotionType != PieceType.Null)
                    {
                        if (Piece.GetPieceAlignment(pieces[lastMoveIndex]) == PieceAlignment.White && ((lastMoveIndex & 56) >> 3 <= 3))
                        {
                            break;
                        }
                        if (Piece.GetPieceAlignment(pieces[lastMoveIndex]) == PieceAlignment.Black && ((lastMoveIndex & 56) >> 3 > 3))
                        {
                            break;
                        }

                        pieces[lastMoveIndex] = Piece.SetPieceType(pte.promotionType, pieces[lastMoveIndex]);

                        if ((pte.pieceProperty & PieceProperty.Giant) != 0)
                        {
                            PlaceGiant(pieces[lastMoveIndex], lastMoveIndex & 7, (lastMoveIndex & 56) >> 3);
                        }

                        PieceTableEntry pteB = GlobalPieceManager.Instance.GetPieceTableEntry(pte.promotionType);

                        if (black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                        }
                        else
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, pteB.type, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                        }
                    }
                    break;
                case Square.SquareType.Frost:
                    //Zap the piece
                    if (Piece.GetPieceStatusEffect(pieces[lastMoveIndex]) == PieceStatusEffect.None && ((pte.piecePropertyB & PiecePropertyB.StatusImmune) == 0))
                    {
                        pieces[lastMoveIndex] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, pieces[lastMoveIndex]);
                        pieces[lastMoveIndex] = Piece.SetPieceStatusDuration(2, pieces[lastMoveIndex]);
                    }
                    break;
            }
        }



        //Fix these bitboards
        ulong whitePiecePartial = whitePieces & globalData.bitboard_square_cursed & ~processedSquares;
        ulong blackPiecePartial = blackPieces & globalData.bitboard_square_cursed & ~processedSquares;

        //Cursed squares get an earlier pass as fire and wind squares can change the piece layout
        //So this keeps things consistent and more clear?

        while (whitePiecePartial != 0)
        {
            int index = MainManager.PopBitboardLSB1(whitePiecePartial, out whitePiecePartial);

            ulong bitIndex = 1uL << index;

            ulong searchArea = MainManager.SmearBitboard(bitIndex) & ~bitIndex;
            processedSquares |= bitIndex;
            if ((whitePieces & searchArea) == 0)
            {
                //Kill the piece(?)
                pte = GlobalPieceManager.GetPieceTableEntry(pieces[index]);
                if (pte == null || (pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) != 0)
                {
                    continue;
                }

                DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pte, Piece.PieceAlignment.White, boardUpdateMetadata);
                //pieces[index] = 0;
                whitePerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
            }
        }

        while (blackPiecePartial != 0)
        {
            int index = MainManager.PopBitboardLSB1(blackPiecePartial, out blackPiecePartial);

            ulong bitIndex = 1uL << index;
            ulong searchArea = MainManager.SmearBitboard(bitIndex) & ~bitIndex;
            processedSquares |= bitIndex;
            if ((blackPieces & searchArea) == 0)
            {
                //Kill the piece(?)
                pte = GlobalPieceManager.GetPieceTableEntry(pieces[index]);
                if (pte == null || (pte.pieceProperty & (PieceProperty.Giant | PieceProperty.NoTerrain)) != 0)
                {
                    continue;
                }

                DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pte, Piece.PieceAlignment.Black, boardUpdateMetadata);
                //pieces[index] = 0;
                blackPerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
            }
        }

        //Do the other types


        ulong squaresToSearch;
        //Only the side that just moved is affected
        //(so wind squares only push your pieces after your turn)
        //This is so that wind tiles are slow enough to react to better near the start of the game
        //(Otherwise you can get extremely fast rush strategies if the wind blows towards the opponent?)
        if (black)
        {
            squaresToSearch = (blackPieces) & ~processedSquares;
        } else
        {
            squaresToSearch = (whitePieces) & ~processedSquares;
        }

        while (squaresToSearch != 0)
        {
            int index = MainManager.PopBitboardLSB1(squaresToSearch, out squaresToSearch);

            ulong bitIndex = 1uL << index;

            if (pieces[index] == 0 || globalData.squares[index].type == Square.SquareType.Normal || (bitIndex & processedSquares) != 0)
            {
                continue;
            }
            processedSquares |= bitIndex;

            PieceTableEntry pteI = GlobalPieceManager.GetPieceTableEntry(pieces[index]);

            if (globalData.squares[index].type != Square.SquareType.Promotion && ((pteI.pieceProperty & (PieceProperty.NoTerrain | PieceProperty.Giant)) != 0))
            {
                continue;
            }
            
            if ((pteI.piecePropertyB & PiecePropertyB.ShiftImmune) != 0) {
                switch (globalData.squares[index].type)
                {
                    case Square.SquareType.WindUp:
                    case Square.SquareType.WindDown:
                    case Square.SquareType.WindLeft:
                    case Square.SquareType.WindRight:
                        continue;
                }
            }

            switch (globalData.squares[index].type)
            {
                case Square.SquareType.Fire:
                    //Burn that piece
                    if (black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (pteI.pieceValueX2);
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (pteI.pieceValueX2);
                    }

                    DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pte, black ? Piece.PieceAlignment.Black : Piece.PieceAlignment.White, boardUpdateMetadata);
                    break;
                case Square.SquareType.WindUp:
                    if (index < 56 && pieces[index + 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) + 1, pteI.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        //blow wind upwards
                        pieces[index + 8] = pieces[index];
                        pieces[index] = 0;
                        processedSquares |= 1uL << index + 8;
                    }
                    break;
                case Square.SquareType.WindDown:
                    if (index > 7 && pieces[index - 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) - 1, pteI.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[index - 8] = pieces[index];
                        pieces[index] = 0;
                        processedSquares |= 1uL << index - 8;
                    }
                    break;
                case Square.SquareType.WindLeft:
                    if ((index & 7) > 0 && pieces[index - 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7) - 1, (index >> 3), pteI.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[index - 1] = pieces[index];
                        pieces[index] = 0;
                        processedSquares |= 1uL << index - 1;
                    }
                    break;
                case Square.SquareType.WindRight:
                    if ((index & 7) < 7 && pieces[index + 1] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7) + 1, (index >> 3), pteI.type, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        pieces[index + 1] = pieces[index];
                        pieces[index] = 0;
                        processedSquares |= 1uL << index + 1;
                    }
                    break;
                case Square.SquareType.Promotion:
                    if (pte.promotionType != PieceType.Null)
                    {
                        if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.White && ((index & 56) >> 3 <= 3))
                        {
                            break;
                        }
                        if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.Black && ((index & 56) >> 3 > 3))
                        {
                            break;
                        }

                        pieces[index] = Piece.SetPieceType(pte.promotionType, pieces[index]);

                        if ((pte.pieceProperty & PieceProperty.Giant) != 0)
                        {
                            PlaceGiant(pieces[index], index & 7, (index & 56) >> 3);
                        }

                        PieceTableEntry pteB = GlobalPieceManager.Instance.GetPieceTableEntry(pte.promotionType);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, pteB.type, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                        }

                        if (black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                        }
                        else
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                        }
                    }
                    break;
            }
        }

        //Holes get a late pass in the rare case where stuff can pull them off the holes?
        ulong holePieces = (whitePieces | blackPieces) & globalData.bitboard_square_hole;
        while (holePieces != 0)
        {
            int index = MainManager.PopBitboardLSB1(holePieces, out holePieces);

            PieceTableEntry pteH = GlobalPieceManager.GetPieceTableEntry(pieces[index]);

            //only being a giant saves you from the hole
            if ((pteH.pieceProperty & PieceProperty.Giant) == 0)
            {
                if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.White)
                {
                    DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pteH, Piece.PieceAlignment.White, boardUpdateMetadata);
                    whitePerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
                }
                if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.Black)
                {
                    DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pteH, Piece.PieceAlignment.Black, boardUpdateMetadata);
                    blackPerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
                }
            }
        }
    }

    public void ApplyZenithEffect(List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((globalData.enemyModifier & Board.EnemyModifier.Zenith) != 0 && blackPerPlayerInfo.pieceValueSumX2 < GlobalPieceManager.KING_VALUE_BONUS)
        {
            //Try to transmogrify one of black's other pieces into another King
            for (int i = 0; i < 64; i++)
            {
                int subindex = 63 - i;
                if (pieces[subindex] != 0 && Piece.GetPieceAlignment(pieces[subindex]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteO = GlobalPieceManager.GetPieceTableEntry(pieces[subindex]);
                    PieceTableEntry pteK = GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.King);

                    blackPerPlayerInfo.pieceValueSumX2 += (short)(pteK.pieceValueX2 - pteO.pieceValueX2);

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(subindex & 7, subindex >> 3, PieceType.King, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    pieces[subindex] = Piece.SetPieceType(PieceType.King, pieces[subindex]);
                    break;
                }
            }
        }
    }

    //assumes that the side to move is the mover
    public static bool IsMoveLegal(ref Board b, uint move, bool black)
    {
        //New setup: if you somehow get into KC position it lets you do the capture
        return !MoveIllegalByCheck(ref b, move, black) || MoveIsKingCapture(ref b, move, black);
    }

    public static bool IsConsumableMoveLegal(ref Board b, uint move)
    {
        (ConsumableMoveType cmt, int tx, int ty) = DecodeConsumableMove(move);

        PieceTableEntry pte = GlobalPieceManager.Instance.GetPieceTableEntry(Piece.GetPieceType(b.pieces[tx + (ty << 3)]));

        switch (cmt)
        {
            case ConsumableMoveType.PocketRock:
            case ConsumableMoveType.PocketPawn:
            case ConsumableMoveType.PocketKnight:
                //Only on empty
                if (b.pieces[tx + (ty << 3)] != 0)
                {
                    return false;
                }
                return true;
            case ConsumableMoveType.Horns:
                break;
            case ConsumableMoveType.Torch:
                break;
            case ConsumableMoveType.Ring:
                break;
            case ConsumableMoveType.Wings:
                break;
            case ConsumableMoveType.Glass:
                break;
            case ConsumableMoveType.Bottle:
                break;
            case ConsumableMoveType.Shield:
                break;
            case ConsumableMoveType.WarpBack:
                break;
            case ConsumableMoveType.Freeze:
                break;
            case ConsumableMoveType.Phantom:
                break;
            case ConsumableMoveType.Promote:
                //it must have a promotion
                if (pte == null || pte.promotionType == PieceType.Null)
                {
                    return false;
                }
                return true;
            case ConsumableMoveType.SplashCure:
                //Don't let you waste it (so it only works if something nearby is splash curable
                break;
        }

        return false;
    }

    public void MakeSetupMove(uint move)
    {
        bool giant;
        if (Move.GetFromY(move) == 15)
        {
            PieceType pt = (PieceType)MainManager.BitFilter(move, 16, 24);
            //Debug.Log(pt);
            giant = ((GlobalPieceManager.Instance.GetPieceTableEntry(pt).pieceProperty & PieceProperty.Giant) != 0);

            //Spawn a piece at position

            if (giant)
            {
                PlaceGiant(Piece.SetPieceType(pt, Piece.SetPieceAlignment((PieceAlignment)MainManager.BitFilter(move, 25, 26), 0)), Move.GetToX(move), Move.GetToY(move));
            }
            else
            {
                pieces[Move.GetToXYInt(move)] = Piece.SetPieceType(pt, Piece.SetPieceAlignment((PieceAlignment)MainManager.BitFilter(move, 25, 26), 0));
            }

            whitePerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.Instance.GetPieceTableEntry(pt).pieceValueX2;

            return;
        }

        giant = ((GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).pieceProperty & PieceProperty.Giant) != 0);
        if (Move.GetToY(move) == 15)
        {
            //Erase the piece

            if (Piece.GetPieceAlignment(pieces[Move.GetFromXYInt(move)]) == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).pieceValueX2;
            }
            if (Piece.GetPieceAlignment(pieces[Move.GetFromXYInt(move)]) == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).pieceValueX2;
            }

            if (giant)
            {
                pieces[Move.GetFromXYInt(move)] = 0;
                pieces[Move.GetFromXYInt(move) + 1] = 0;
                pieces[Move.GetFromXYInt(move) + 8] = 0;
                pieces[Move.GetFromXYInt(move) + 9] = 0;
            }
            else
            {
                pieces[Move.GetFromXYInt(move)] = 0;
            }
            return;
        }

        //Execute the move

        uint newPiece = pieces[Move.GetFromXYInt(move)];

        //Move the thing
        DeletePieceMovedFromCoordinate(Move.GetFromX(move), Move.GetFromY(move), GlobalPieceManager.GetPieceTableEntry(newPiece), Piece.GetPieceAlignment(newPiece));
        PlaceMovedPiece(newPiece, Move.GetToX(move), Move.GetToY(move), GlobalPieceManager.GetPieceTableEntry(newPiece), Piece.GetPieceAlignment(newPiece));
    }

    public static bool SetupMoveIsKingCapture(ref Board b, uint move)
    {
        Board copy = new Board(b);
        copy.MakeSetupMove(move);

        return !copy.CheckForKings();
    }

    public static bool IsSetupMoveLegal(ref Board b, uint move)
    {
        if (Move.GetFromY(move) == 15)
        {
            PieceType pt = (PieceType)MainManager.BitFilter(move, 16, 24);

            //Spawn a piece at position

            //Does it fit in the space
            if ((GlobalPieceManager.Instance.GetPieceTableEntry(pt).pieceProperty & PieceProperty.Giant) != 0)
            {
                if (GetToX(move) >= 7)
                {
                    return false;
                }
                if (GetToY(move) >= 7)
                {
                    return false;
                }

                if (b.pieces[GetToXYInt(move)] != 0)
                {
                    return false;
                }
                if (b.pieces[GetToXYInt(move) + 1] != 0)
                {
                    return false;
                }
                if (b.pieces[GetToXYInt(move) + 8] != 0)
                {
                    return false;
                }
                if (b.pieces[GetToXYInt(move) + 9] != 0)
                {
                    return false;
                }
            }
            else
            {
                if (b.pieces[GetToXYInt(move)] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        if (Move.GetToY(move) == 15)
        {
            //Erase the piece
            //this only needs the king capture check because it just can get deleted no problem

            return !SetupMoveIsKingCapture(ref b, move);
        }

        //Can move be executed
        //Debug.Log(Move.GetFromY(move) + " " + Move.GetFromX(move));
        if (b.pieces[GetFromXYInt(move)] == 0)
        {
            return false;
        }

        //Does it fit in the space
        if ((GlobalPieceManager.GetPieceTableEntry(b.pieces[GetFromXYInt(move)]).pieceProperty & PieceProperty.Giant) != 0)
        {
            if (GetToX(move) >= 7)
            {
                return false;
            }
            if (GetToY(move) >= 7)
            {
                return false;
            }

            int fx = GetFromX(move);
            int fy = GetFromY(move);
            int tx = GetToX(move);
            int ty = GetToY(move);

            //Note: exclude positions that overlap the giant itself because it can move out of its own way
            if ((tx > fx + 1 || ty > fy + 1 || tx < fx || ty < fy) && b.pieces[GetToXYInt(move)] != 0)
            {
                return false;
            }
            if ((tx + 1 > fx + 1 || ty > fy + 1 || tx + 1 < fx || ty < fy) && b.pieces[GetToXYInt(move) + 1] != 0)
            {
                return false;
            }
            if ((tx > fx + 1 || ty + 1 > fy + 1 || tx < fx || ty + 1 < fy) && b.pieces[GetToXYInt(move) + 8] != 0)
            {
                return false;
            }
            if ((tx + 1 > fx + 1 || ty + 1 > fy + 1 || tx + 1 < fx || ty + 1 < fy) && b.pieces[GetToXYInt(move) + 9] != 0)
            {
                return false;
            }
        }
        else
        {
            if (b.pieces[GetToXYInt(move)] != 0)
            {
                return false;
            }
        }

        return !SetupMoveIsKingCapture(ref b, move);
    }

    public bool CheckForKings()
    {
        bool whiteKing = false;
        bool blackKing = false;

        if ((globalData.playerModifier & PlayerModifier.NoKing) != 0)
        {
            whiteKing = true;
        }
        if ((globalData.enemyModifier & EnemyModifier.NoKing) != 0)
        {
            blackKing = true;
        }

        //Piece value check
        if (whitePerPlayerInfo.pieceValueSumX2 >= GlobalPieceManager.KING_VALUE_BONUS)
        {
            whiteKing = true;
        }
        if (blackPerPlayerInfo.pieceValueSumX2 >= GlobalPieceManager.KING_VALUE_BONUS)
        {
            blackKing = true;
        }

        return whiteKing && blackKing;

        /*
        //Fast check
        int kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingBlack, out _);
        uint target = 0;
        if (kingIndex != -1)
        {
            target = pieces[kingIndex];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.Black)
            {
                blackKing = true;
            }
        }
        kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingWhite, out _);
        if (kingIndex != -1)
        {
            target = pieces[kingIndex];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.White)
            {
                whiteKing = true;
            }
        }

        if (whiteKing && blackKing)
        {
            return true;
        }

        for (int i = 0; i < 64; i++)
        {
            if (Piece.GetPieceType(pieces[i]) == PieceType.King)
            {
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
                {
                    whiteKing = true;
                }
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
                {
                    blackKing = true;
                }
            }

            if (whiteKing && blackKing)
            {
                return true;
            }
        }

        return false;
        */
    }

    public bool CheckForKingsSlow()
    {
        bool whiteKing = false;
        bool blackKing = false;

        if ((globalData.playerModifier & PlayerModifier.NoKing) != 0)
        {
            whiteKing = true;
        }
        if ((globalData.enemyModifier & EnemyModifier.NoKing) != 0)
        {
            blackKing = true;
        }

        int kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingBlack, out _);
        uint target = 0;
        if (kingIndex != -1)
        {
            target = pieces[kingIndex];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.Black)
            {
                blackKing = true;
            }
        }
        kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_kingWhite, out _);
        if (kingIndex != -1)
        {
            target = pieces[kingIndex];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.White)
            {
                whiteKing = true;
            }
        }

        if (whiteKing && blackKing)
        {
            return true;
        }

        for (int i = 0; i < 64; i++)
        {
            if (Piece.GetPieceType(pieces[i]) == PieceType.King)
            {
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
                {
                    whiteKing = true;
                }
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
                {
                    blackKing = true;
                }
            }

            if (whiteKing && blackKing)
            {
                return true;
            }
        }

        return false;
    }


    public Piece.PieceAlignment GetKingCaptureWinner()
    {
        bool whiteKing = false;
        bool blackKing = false;

        if ((globalData.playerModifier & PlayerModifier.NoKing) != 0)
        {
            whiteKing = true;
        }
        if ((globalData.enemyModifier & EnemyModifier.NoKing) != 0)
        {
            blackKing = true;
        }

        //Piece value check
        if (whitePerPlayerInfo.pieceValueSumX2 >= GlobalPieceManager.KING_VALUE_BONUS)
        {
            whiteKing = true;
        }
        if (blackPerPlayerInfo.pieceValueSumX2 >= GlobalPieceManager.KING_VALUE_BONUS)
        {
            blackKing = true;
        }

        if (whiteKing && blackKing)
        {
            return PieceAlignment.Null;
        }
        if (whiteKing && !blackKing)
        {
            return PieceAlignment.White;
        }
        if (!whiteKing && blackKing)
        {
            return PieceAlignment.Black;
        }
        return PieceAlignment.Neutral;
    }

    public static bool PositionIsCheck(ref Board b)
    {
        Board copy = new Board(b);
        copy.ApplyNullMove();
        return IsKingCapturePossible(ref copy);
    }

    //This is probably something I show in the UI
    public static uint PositionIsCheckFindThreat(ref Board b)
    {
        Board copy = new Board(b);
        copy.ApplyNullMove();
        return FindKingCaptureMove(ref copy);
    }

    //Stalemate + in check
    public static bool PositionIsCheckmate(ref Board b)
    {
        return PositionIsCheck(ref b) && PositionIsStalemate(ref b);
    }

    //The side to move has no legal moves
    public static bool PositionIsStalemate(ref Board b)
    {
        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        for (int i = 0; i < moves.Count; i++)
        {
            if (IsMoveLegal(ref b, moves[i], b.blackToMove))
            {
                return false;
            }   
        }

        return true;
    }

    public static bool MoveIllegalByCheck(ref Board b, uint move, bool black)
    {
        Board copy = new Board(b);
        copy.ApplyMove(move);

        /*
        PieceAlignment winner = copy.GetVictoryCondition();
        if (winner == PieceAlignment.Black && black)
        {
            return false;
        }
        if (winner == PieceAlignment.White && !black)
        {
            return false;
        }
        */

        return IsKingCapturePossible(ref copy);
    }
    public static bool MoveIsKingCapture(ref Board b, uint move, bool black)
    {
        Board copy = new Board(b);
        copy.ApplyMove(move);
        return !copy.CheckForKings();
    }

    //Probably something to show in the UI for clarity
    public static uint MoveIllegalByCheckFindRefutation(ref Board b, uint move)
    {
        Board copy = new Board(b);
        copy.ApplyMove(move);

        return FindKingCaptureMove(ref copy);
    }
    public static (uint, List<MoveMetadata>) MoveIllegalByCheckFindRefutationPath(ref Board b, uint move)
    {
        Board copy = new Board(b);
        copy.ApplyMove(move);

        return FindKingCaptureMovePath(ref copy);
    }

    public static bool IsKingCapturePossible(ref Board b)
    {
        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        return IsKingCapturePossible(ref b, moves);
    }
    //Version the AI uses because it usually checks this after getting a move list
    public static bool IsKingCapturePossible(ref Board b, List<uint> moves)
    {
        if (moves == null)
        {
            return IsKingCapturePossible(ref b);
        }

        //List<uint> moves = new List<uint>();
        //MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White);

        Board copy = new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            //MoveGeneratorInfoEntry.GeneratePieceBitboards(b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White);
            copy.CopyOverwrite(b);
            //Debug.Log("Check KC " + Move.ConvertToString(moves[i]));
            copy.ApplyMove(moves[i]);

            if (!copy.CheckForKings())
            {
                return true;
            }
        }

        return false;
    }
    public static uint FindKingCaptureMove(ref Board b)
    {
        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        Board copy = new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);
            if (!copy.CheckForKings())
            {
                return moves[i];
            }
        }

        return 0;
    }
    public static uint FindKingCaptureMove(ref Board b, List<uint> moves)
    {
        //List<uint> moves = new List<uint>();
        //MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        Board copy = new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);
            if (!copy.CheckForKings())
            {
                return moves[i];
            }
        }

        return 0;
    }
    public static (uint, List<MoveMetadata>) FindKingCaptureMovePath(ref Board b)
    {
        List<uint> moves = new List<uint>();
        Dictionary<uint, MoveMetadata> moveDict = new Dictionary<uint, MoveMetadata>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, moveDict);

        Board copy = new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);
            if (!copy.CheckForKings())
            {
                return (moves[i], moveDict[Move.RemoveNonLocation(moves[i])].TracePath(Move.GetFromX(moves[i]), Move.GetFromY(moves[i]), Move.GetDir(moves[i])));
            }
        }

        return (0, null);
    }


    public static ulong PerftTest(ref Board b, int depth)
    {
        ulong number = 0;
        if (depth <= 0)
        {
            return 1;
        }

        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        Board copy = new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);

            if (!copy.CheckForKings())
            {
                continue;
            }

            number += PerftTest(ref copy, depth - 1);
        }

        return number;
    }
}

[Serializable]
public struct Square
{
    public enum SquareType : byte
    {
        Hole = 255,
        Normal = 0,
        Fire,           //If on them: piece is deleted unless it is the piece last moved
        Water,          //If on them: piece can't capture
        Rough,          //Stops ray movers
        WindUp,
        WindDown,
        WindLeft,
        WindRight,
        Slippery,       //If you move onto it: get Pushed 1 square in Dir
        Bouncy,         //Opposite of Ice: pulls you back 1 when you move on them
        Bright,         //Immune
        Promotion,      //Promote early
        Cursed,         //If not adjacent to allies: get destroyed
        CaptureOnly,    //If on them: piece can only leave by capturing
        Frost,    //Whatever moves onto them gets applied stun for 1 turn

        //Special objectives
        BronzeTreasure, //Spawns on rank 6
        SilverTreasure, //Spawns on back rank
        GoldTreasure,   //Spawns in multiples (rank 6, 7, 8?), must occupy all to get treasure
    }

    public SquareType type;

    public Square(SquareType st)
    {
        type = st;
    }
}

//4 bytes
//Slightly wasteful of space but making it 3 bytes doesn't really work
public static class Move
{
    //public ushort toFrom;    //= 65535 if null move?
    //public Dir dir;
    //public SpecialType specialType;
    
    public enum Dir : sbyte
    {
        DownLeft = 10,
        Down = 8,
        DownRight = 9,
        Left = 2,
        Null = 0,   //Used for anything that isn't directly in one of the 8 directions (all leaps, teleports, but weird sliders usually go in those directions)
        Right = 1,
        UpLeft = 6,
        Up = 4,
        UpRight = 5,
    }

    public enum SpecialType : byte
    {
        Normal,
        MoveOnly,
        CaptureOnly,
        FlyingMoveOnly,
        ConsumeAllies,
        ConsumeAlliesCaptureOnly,
        ChargeMove,
        ChargeMoveReset,
        SelfMove,
        Castling,
        Convert,
        ConvertCaptureOnly,
        ConvertPawn,
        Spawn,
        FireCapture,
        FireCaptureOnly,
        LongLeaper,     //can be combined with a capturing one
        LongLeaperCaptureOnly,     //can be combined with a capturing one
        FireCapturePush,    //fire capture and push
        PullMove,
        PushMove,
        AdvancerPush,       //push + advancer
        Advancer,
        Withdrawer,
        AdvancerWithdrawer,
        WrathCapturer,      //apply Advancer if it is a capture
        FlankingCapturer,
        PoisonFlankingAdvancer,
        //TandemMovementPawns,
        //TandemMovementNonPawns,
        AllySwap,
        AnyoneSwap,
        MorphIntoTarget,    //turn into target

        SlipMove,           //Move that leaves you adjacent to an enemy
        PlantMove,           //Move that leaves you adjacent to an ally, also ignores blockers

        GliderMove,         //anti-slip but can cross obstacles like plant (fly over them)

        AllyAbility,
        Imbue,
        ImbueWinged,
        ImbuePromote,
        ChargeApplyModifier,
        RangedPullAllyOnly,
        RangedPushAllyOnly,

        InflictFreeze,
        InflictFreezeCaptureOnly,
        Inflict,
        InflictCaptureOnly,

        TeleportOpposite,
        TeleportRecall,
        TeleportMirror,

        CarryAlly,
        DepositAlly,
        DepositAllyPlantMove,

        EnemyAbility,
        RangedPull,
        RangedPush,

        EmptyAbility,

        PassiveAbility,     //generic passive ability
    }

    //
    public enum ConsumableMoveType
    {
        None,
        PocketRock,
        PocketPawn,
        PocketKnight,
        Horns,  //vengeful imbuer
        Torch,  //phoenix imbuer
        Ring,   //golden imbuer
        Wings,  //winged imbuer
        Glass,  //spectral imbuer
        Bottle, //immune imbuer
        Shield, //shielded imbuer
        WarpBack,
        Freeze,
        Phantom,
        Promote,
        SplashCure,

    }

    public static bool IsConsumableMove(uint move)
    {
        if (Move.GetToX(move) > 7)
        {
            return true;
        }
        if (Move.GetToY(move) > 7)
        {
            return true;
        }
        if (Move.GetFromX(move) > 7)
        {
            return true;
        }
        if (Move.GetFromY(move) > 7)
        {
            return true;
        }
        return false;
    }
    public static (ConsumableMoveType, int, int) DecodeConsumableMove(uint move)
    {
        int fx = Move.GetFromX(move) & 7;
        int fy = Move.GetFromX(move) & 7;

        return ((ConsumableMoveType)(fx + (fy << 3)), Move.GetToX(move), (Move.GetToY(move)));
    }
    public static uint EncodeConsumableMove(ConsumableMoveType cmt, int tx, int ty)
    {
        int fx = 8 + ((int)cmt >> 3);
        int fy = 8 + ((int)cmt & 7);

        return Move.PackMove((byte)fx, (byte)fy, (byte)tx, (byte)ty);
    }

    public static string PositionToString(int x, int y)
    {
        int fx = x;
        int fy = y;
        return FileToLetter(fx) + "" + (fy + 1);
    }
    public static string PositionToString(int index)
    {
        int fx = index & 7;
        int fy = index >> 3;
        return FileToLetter(fx) + "" + (fy + 1);
    }
    public static string ConvertToString(uint moveInfo)
    {
        int fx = GetFromX(moveInfo);
        int fy = GetFromY(moveInfo);
        int tx = GetToX(moveInfo);
        int ty = GetToY(moveInfo);

        return (FileToLetter(fx) + "" + (fy + 1) + "" + FileToLetter(tx) + "" + (ty + 1) + " " + Move.GetDir(moveInfo) + " " + Move.GetSpecialType(moveInfo));
    }
    public static string ConvertToStringMinimal(uint moveInfo)
    {
        int fx = GetFromX(moveInfo);
        int fy = GetFromY(moveInfo);
        int tx = GetToX(moveInfo);
        int ty = GetToY(moveInfo);

        return (FileToLetter(fx) + "" + (fy + 1) + "" + FileToLetter(tx) + "" + (ty + 1));
    }


    public static char FileToLetter(int file)
    {
        return (char)('a' + file);
    }

    public static byte GetFromX(uint moveInfo)
    {
        return (byte)(MainManager.BitFilter(moveInfo, 0, 3));
    }
    public static uint SetFromX(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 0, 3);
    }
    public static byte GetFromY(uint moveInfo)
    {
        return (byte)(MainManager.BitFilter(moveInfo, 4, 7));
    }
    public static uint SetFromY(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 4, 7);
    }

    public static byte GetToX(uint moveInfo)
    {
        return (byte)(MainManager.BitFilter(moveInfo, 8, 11));
    }
    public static uint SetToX(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 8, 11);
    }
    public static byte GetToY(uint moveInfo)
    {
        return (byte)(MainManager.BitFilter(moveInfo, 12, 15));
    }
    public static uint SetToY(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 12, 15);
    }

    public static ushort GetToFrom(uint moveInfo)
    {
        return (ushort)(MainManager.BitFilter(moveInfo, 0, 15));
    }
    public static uint SetToFrom(ushort toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 0, 15);
    }
    public static int GetToXYInt(uint moveInfo)
    {
        return GetToX(moveInfo) + (GetToY(moveInfo) << 3);
    }
    public static int GetFromXYInt(uint moveInfo)
    {
        return GetFromX(moveInfo) + (GetFromY(moveInfo) << 3);
    }

    public static Dir GetDir(uint moveInfo)
    {
        return (Dir)(MainManager.BitFilter(moveInfo, 16, 23));
    }
    public static uint SetDir(Dir dir, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)dir, 16, 23);
    }

    public static SpecialType GetSpecialType(uint moveInfo)
    {
        return (SpecialType)(MainManager.BitFilter(moveInfo, 24, 31));
    }
    public static uint SetSpecialType(SpecialType specialType, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)specialType, 24, 31);
    }

    public static uint RemoveNonLocation(uint moveInfo)
    {
        return GetToFrom(moveInfo);
    }

    public static uint MakeSetupCreateMove(Piece.PieceType type, Piece.PieceAlignment pa, byte toX, byte toY)
    {
        uint m = Move.PackMove(15, 15, toX, toY, 0, 0);
        m = MainManager.BitFilterSet(m, (uint)type, 16, 24);
        m = MainManager.BitFilterSet(m, (uint)pa, 25, 26);
        //Debug.Log(MainManager.BitFilter(m, 16, 24));
        return m;
    }

    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY, Dir dir, SpecialType specialType)
    {
        uint output = 0;

        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);

        return output;
    }
    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY, SpecialType specialType)
    {
        uint output = 0;

        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        //output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);

        return output;
    }
    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY)
    {
        uint output = 0;

        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        //output += (((uint)dir & 0x000000ff) << 16);
        //output += ((uint)specialType << 24);

        return output;
    }

    public static (int, int) DirToDelta(Dir d)
    {
        byte di = (byte)d;

        //Debug.Log(d + " is " + (((di & 1) - ((di & 2) >> 1)), (((di & 4) >> 2) - ((di & 8) >> 3))));

        return (((di & 1) - ((di & 2) >> 1)), (((di & 4) >> 2) - ((di & 8) >> 3)));
        //int dx = 0;
        //int dy = 0;
        //return (dx, dy);
    }

    public static (int, int) DeltaToUnitDelta(int dx, int dy)
    {
        return ((dx > 0 ? 1 : (dx < 0 ? -1 : 0)), (dy > 0 ? 1 : (dy < 0 ? -1 : 0)));
    }

    public static Dir DeltaToDir(int dx, int dy)
    {
        Dir dir = Dir.Null;
        //if exactly aligned: dir set
        if (dx == 0)
        {
            if (dy > 0)
            {
                dir = Dir.Up;
            }
            else if (dy < 0)
            {
                dir = Dir.Down;
            }
        }
        if (dy == 0)
        {
            if (dx > 0)
            {
                dir = Dir.Right;
            }
            else if (dx < 0)
            {
                dir = Dir.Left;
            }
        }
        if (dx == dy)
        {
            if (dx > 0)
            {
                dir = Dir.UpRight;
            }
            else if (dx < 0)
            {
                dir = Dir.DownLeft;
            }
        }
        if (dx == -dy)
        {
            if (dx > 0)
            {
                dir = Dir.DownRight;
            }
            else if (dx < 0)
            {
                dir = Dir.UpLeft;
            }
        }
        return dir;
    }
    public static Dir DeltaToDirSoft(int dx, int dy)
    {
        Dir dir = Dir.Null;
        //if exactly aligned: dir set
        if (dx == 0)
        {
            if (dy > 0)
            {
                dir = Dir.Up;
            }
            else if (dy < 0)
            {
                dir = Dir.Down;
            }
            return dir;
        } else if (dx > 0)
        {
            if (dy > 0)
            {
                dir = Dir.UpRight;
            } else if (dy == 0)
            {
                dir = Dir.Right;
            } else
            {
                dir = Dir.DownRight;
            }
            return dir;
        } else
        {
            if (dy > 0)
            {
                dir = Dir.UpLeft;
            }
            else if (dy == 0)
            {
                dir = Dir.Left;
            }
            else
            {
                dir = Dir.DownLeft;
            }
            return dir;
        }
    }

    public static Dir ReverseDir(Dir d)
    {
        switch (d)
        {
            case Dir.DownLeft:
                return Dir.UpRight;
            case Dir.Down:
                return Dir.Up;
            case Dir.DownRight:
                return Dir.UpLeft;
            case Dir.Left:
                return Dir.Right;
            case Dir.Null:
                return Dir.Null;
            case Dir.Right:
                return Dir.Left;
            case Dir.UpLeft:
                return Dir.DownRight;
            case Dir.Up:
                return Dir.Down;
            case Dir.UpRight:
                return Dir.DownLeft;
        }
        return Dir.Null;
    }

    public static bool PushLegal(ref Board b, int x, int y, Dir dir, bool ally)
    {
        if (dir == Dir.Null)
        {
            return false;
        }
        (int tempX, int tempY) = (DirToDelta(dir));
        tempX += x;
        tempY += y;

        //Target must be empty and legal
        if (tempX < 0 || tempX > 7 || tempY < 0 || tempY > 7)
        {
            return false;
        }

        return b.GetPieceAtCoordinate(tempX, tempY) == 0;
    }
    public static bool PullLegal(ref Board b, int x, int y, Dir dir, bool ally)
    {
        if (dir == Dir.Null)
        {
            return false;
        }
        (int tempX, int tempY) = (DirToDelta(dir));
        tempX = -tempX;
        tempY = -tempY;
        tempX += x;
        tempY += y;

        //Target must be empty and legal
        if (tempX < 0 || tempX > 7 || tempY < 0 || tempY > 7)
        {
            return false;
        }

        return b.GetPieceAtCoordinate(tempX, tempY) == 0;
    }

    //This requires all these extra arguments because some move types need extra checks
    //The enemy capture version uses the check invincible thing
    public static bool SpecialMoveCanMoveOntoAlly(SpecialType st, ref Board b, int x, int y, int tx, int ty, Dir dir)
    {
        //I'll just ban this unconditionally
        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(b.GetPieceAtCoordinate(tx, ty));
        if ((pte.pieceProperty & PieceProperty.Giant) != 0)
        {
            return false;
        }

        if ((pte.piecePropertyB & PiecePropertyB.ShiftImmune) != 0)
        {
            switch (st)
            {
                case SpecialType.FireCapturePush:
                case SpecialType.PushMove:
                case SpecialType.RangedPush:
                case SpecialType.RangedPushAllyOnly:
                case SpecialType.RangedPull:
                case SpecialType.RangedPullAllyOnly:
                case SpecialType.AllySwap:
                case SpecialType.AnyoneSwap:
                case SpecialType.TeleportOpposite:
                case SpecialType.TeleportMirror:
                case SpecialType.TeleportRecall:
                case SpecialType.CarryAlly:
                    return false;
            }
        }

        switch (st)
        {
            case SpecialType.FireCapturePush:
            case SpecialType.PushMove:
            case SpecialType.RangedPush:
            case SpecialType.RangedPushAllyOnly:
                return PushLegal(ref b, tx, ty, dir, true);
            case SpecialType.RangedPull:
            case SpecialType.RangedPullAllyOnly:
                return PullLegal(ref b, tx, ty, dir, true);
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
                return (Piece.GetPieceType(b.GetPieceAtCoordinate(tx, ty)) != PieceType.King);
            case SpecialType.AllySwap:
            case SpecialType.AnyoneSwap:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:
                return true;
            case SpecialType.Imbue:
                return Piece.GetPieceModifier(b.pieces[tx + (ty << 3)]) == PieceModifier.None;
            case SpecialType.ImbueWinged:
                //Debug.Log(Piece.GetPieceType(b.GetPieceAtCoordinate(x, y)) + " " + GlobalPieceManager.GetPieceTableEntry(b.GetPieceAtCoordinate(x, y)).wingedCompatible);
                return pte.wingedCompatible && Piece.GetPieceModifier(b.pieces[tx + (ty << 3)]) == PieceModifier.None;
            case SpecialType.ImbuePromote:
                return pte.promotionType != PieceType.Null;
            case SpecialType.ChargeApplyModifier:
                return Piece.GetPieceModifier(b.pieces[tx + (ty << 3)]) == PieceModifier.None;
            case SpecialType.TeleportOpposite:
                //x - (tx - x)
                //(x << 1) - tx
                //same for y ((y << 1) - ty)
                int mx = ((x << 1) - tx);
                int my = ((y << 1) - ty);
                return mx >= 0 && mx <= 7 && my >= 0 && my <= 7 && b.pieces[mx + (my << 3)] == 0;
            case SpecialType.TeleportRecall:
                switch (Piece.GetPieceAlignment(b.pieces[x + (y << 3)]))
                {
                    case PieceAlignment.White:
                        return (y > 0) && (b.pieces[x + (y << 3) - 8] == 0);
                    case PieceAlignment.Black:
                        return (y < 7) && (b.pieces[x + (y << 3) + 8] == 0);
                }
                break;
            case SpecialType.TeleportMirror:
                return b.pieces[(7 - tx) + (ty << 3)] == 0;
            case SpecialType.CarryAlly:
                return (pte.piecePropertyB & PiecePropertyB.NotCarriable) == 0;
        }

        return false;
    }
    public static bool SpecialMoveCanMoveOntoEnemy(SpecialType st, ref Board b, int x, int y, Dir dir)
    {
        /*
        Normal,
        MoveOnly,
        CaptureOnly,
        FlyingMoveOnly,
        SelfMove,
        Castling,
        Convert,
        Spawn,
        FireCapture,
        LongLeaper,     //can be combined with a capturing one
        FireCapturePush,    //fire capture and push
        PullMove,
        RangedPull,
        PushMove,
        RangedPush,
        Advancer,
        Withdrawer,
        TandemMovementPawns,
        TandemMovementNonPawns,
         */

        switch (st)
        {
            case SpecialType.Normal:
            case SpecialType.CaptureOnly:
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case SpecialType.ChargeMove:
            case SpecialType.ChargeMoveReset:
            case SpecialType.FireCapture:
            case SpecialType.FireCaptureOnly:
            case SpecialType.FireCapturePush:
            case SpecialType.PullMove:
            case SpecialType.PushMove:
            //case SpecialType.TandemMovementPawns:
            //case SpecialType.TandemMovementNonPawns:
            case SpecialType.Convert:
            case SpecialType.ConvertCaptureOnly:
            case SpecialType.AnyoneSwap:
            case SpecialType.MorphIntoTarget:
            case SpecialType.EnemyAbility:
            case SpecialType.PassiveAbility:
            case SpecialType.PlantMove:
            case SpecialType.WrathCapturer:
                return true;
            case SpecialType.ConvertPawn:
                return GlobalPieceManager.GetPieceTableEntry(b.pieces[x + y * 8]).promotionType != 0;
            case SpecialType.RangedPull:
                return PullLegal(ref b, x, y, dir, false);
            case SpecialType.RangedPush:
                return PushLegal(ref b, x, y, dir, false);
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
                return true;
        }

        return false;
    }
    public static bool SpecialMoveCantTargetEmpty(SpecialType st)
    {
        switch (st)
        {
            case SpecialType.FireCaptureOnly:
            case SpecialType.CaptureOnly:
            case SpecialType.ConvertCaptureOnly:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case SpecialType.RangedPush:
            case SpecialType.RangedPull:
            case SpecialType.RangedPullAllyOnly:
            case SpecialType.RangedPushAllyOnly:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.EnemyAbility:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:    //since this only targets allies or enemies I don't want to clog up the bitboard with extraneous info
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
                return true;
        }
        return false;
    }
    public static bool SpecialMoveCaptureLike(SpecialType st)   //Things that show an attack highlight
    {
        switch (st)
        {
            case SpecialType.Normal:
            case SpecialType.CaptureOnly:
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case SpecialType.ChargeMove:
            case SpecialType.ChargeMoveReset:
            case SpecialType.FireCapture:
            case SpecialType.FireCaptureOnly:
            case SpecialType.FireCapturePush:
            case SpecialType.PullMove:
            case SpecialType.PushMove:
            //case SpecialType.TandemMovementPawns:
            //case SpecialType.TandemMovementNonPawns:
            case SpecialType.Convert:
            case SpecialType.ConvertCaptureOnly:
            case SpecialType.ConvertPawn:
            case SpecialType.EnemyAbility:
            case SpecialType.PassiveAbility:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
                return true;
        }

        return false;
    }
    public static bool SpecialMoveStationary(SpecialType st)
    {
        switch (st)
        {
            case SpecialType.SelfMove:
            case SpecialType.Convert:
            case SpecialType.ConvertCaptureOnly:
            case SpecialType.ConvertPawn:
            case SpecialType.Spawn:
            case SpecialType.FireCapture:
            case SpecialType.FireCaptureOnly:
            case SpecialType.FireCapturePush:
            case SpecialType.RangedPull:
            case SpecialType.RangedPullAllyOnly:
            case SpecialType.RangedPush:
            case SpecialType.RangedPushAllyOnly:
            case SpecialType.MorphIntoTarget:
            case SpecialType.AllyAbility:
            case SpecialType.EnemyAbility:
            case SpecialType.EmptyAbility:
            case SpecialType.PassiveAbility:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
                return true;
        }
        return false;
    }
    public static bool CanFlyOverObstacles(SpecialType st)
    {
        switch (st)
        {
            case SpecialType.RangedPullAllyOnly:
            case SpecialType.RangedPushAllyOnly:
            case SpecialType.LongLeaper:
            case SpecialType.LongLeaperCaptureOnly:
            case SpecialType.RangedPush:
            case SpecialType.RangedPull:
            case SpecialType.PlantMove:
            case SpecialType.GliderMove:
            case SpecialType.DepositAllyPlantMove:
                return true;
        }

        return false;
    }

    //This is mostly for things that are weird and not captures (and not normal passive moves either)
    //Weird capturing types are not marked because they aren't very confusing mostly
    public static bool SpecialMoveHighlighted(SpecialType st)
    {
        switch (st)
        {
            case SpecialType.RangedPullAllyOnly:
            case SpecialType.RangedPushAllyOnly:
            case SpecialType.RangedPush:
            case SpecialType.RangedPull:
            case SpecialType.Spawn:
            case SpecialType.AnyoneSwap:
            case SpecialType.AllySwap:
            case SpecialType.MorphIntoTarget:
            case SpecialType.EnemyAbility:
            case SpecialType.EmptyAbility:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:    //shouldn't need this
            case SpecialType.Imbue:
            case SpecialType.ImbueWinged:
            case SpecialType.ImbuePromote:
            case SpecialType.Castling:
            case SpecialType.ChargeMove:        //Also here because this has special consequences
            case SpecialType.ChargeMoveReset:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
                return true;
        }

        return false;
    }

    public static (int, int) TransformBasedOnAlignment(PieceAlignment pa, int x, int y, bool flip)
    {
        int flipValue = flip ? -1 : 1;
        switch (pa)
        {
            case PieceAlignment.White:
                return (x * flipValue, y * flipValue);
            case PieceAlignment.Black:
                return (-x * flipValue, -y * flipValue);
            case PieceAlignment.Neutral:
                return (y * flipValue, -x * flipValue);
        }

        return (x, y);
    }
}

//4096 bit table
[Serializable]
public class MoveBitTable
{
    public ulong[] tableElements;

    public enum BitTableType : byte
    {
        MoveOnly = 1,
        AttackDefense = 1 << 1,
        Magic = 1 << 2,

        Normal = MoveOnly | AttackDefense,
    }
    public BitTableType btType;

    public ulong Get(int x, int y)
    {
        return tableElements[x + y * 8];
    }
    public bool Get(int x, int y, int toX, int toY)
    {
        return ((tableElements[x + y * 8]) & ((1uL << (toX + 8 * toY)))) != 0;
    }        

    public void Set(int x, int y, int toX, int toY, bool set = true)
    {
        if (set)
        {
            tableElements[x + y * 8] |= ((1uL << (toX + 8 * toY)));
        }
        else
        {
            tableElements[x + y * 8] &= ~((1uL << (toX + 8 * toY)));
        }
    }

    public MoveBitTable()
    {
        tableElements = new ulong[64];
    }

    public void Reset()
    {
        if (tableElements == null)
        {
            tableElements = new ulong[64];
        }
        for (int i = 0; i < 64; i++)
        {
            tableElements[i] = 0;
        }
    }

    public void FillMoveBitTable(List<uint> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            uint movesI = moves[i];
            int movefX = Move.GetFromX(movesI);
            int movefY = Move.GetFromY(movesI);
            int movetX = Move.GetToX(movesI);
            int movetY = Move.GetToY(movesI);

            if (Get(movefX, movefY, movetX, movetY))
            {
                Debug.LogWarning("Duplicate move");
            }
            Set(movefX, movefY, movetX, movetY, true);
        }
    }

    public MoveBitTable CopyInverse()
    {
        MoveBitTable newTable = new MoveBitTable();
        newTable.MakeInverse(this);
        return newTable;
    }

    public void MakeInverse(MoveBitTable toCopy)
    {
        Reset();

        btType = toCopy.btType;

        if (tableElements == null)
        {
            tableElements = new ulong[64];
        }
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                ulong bit = 1uL << j;

                if ((toCopy.tableElements[i] & bit) != 0)
                {
                    //old table element i, bit j
                    //new table element j, bit i
                    tableElements[j] |= 1uL << i;
                }
            }
        }
    }
}