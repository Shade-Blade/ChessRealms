using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static Move;
using static Piece;

//Cache the many many GetPieceTableEntry calls?
//1 cache entry per square
public struct PieceTableCacheEntry
{
    public uint piece;  //if check fails: check type (if type check passes: change piece num) (if type check fails: refresh)
    public Piece.PieceType type;
    public PieceTableEntry pte;

    public PieceTableCacheEntry(uint piece, PieceType type, PieceTableEntry pte)
    {
        this.piece = piece;
        this.type = type;
        this.pte = pte;
    }

    /*
    public PieceTableCacheEntry()
    {
        piece = 0;
        type = PieceType.Null;
        pte = null;
    }
    */
}

//is it worth caching the other piece attributes?
//no
/*
public struct PieceAlignmentCacheEntry
{
    public uint piece;  //if check fails: check type (if type check passes: change piece num) (if type check fails: refresh)
    public Piece.PieceAlignment pa;

    public PieceAlignmentCacheEntry(uint piece, Piece.PieceAlignment pa)
    {
        this.piece = piece;
        this.pa = pa;
    }
}
*/

//Data that remains static over the course of the game
//Being space efficient with this is not very important?
[Serializable]
//Idea: make it a struct?
//How much performance is lost by making the global stuff non global? This struct is some multiple of the board class size
//Maybe C# is good at struct copying?
public struct BoardGlobalData
{
    //Board layout (special squares)
    public Square[] squares;

    public Board.PlayerModifier playerModifier;
    public Board.EnemyModifier enemyModifier;

    public BoardGlobalPlayerInfo whitePerPlayerInfo;
    public BoardGlobalPlayerInfo blackPerPlayerInfo;

    public bool noLastMoveHash;

    //idea: static
    //doesn't really help?
    public PieceTableCacheEntry[] pteCache;

    //caching this is not very helpful
    //because Piece.GetPieceAlignment is only a few operations
    //cache return would have a hard time being faster than that
    //public PieceAlignmentCacheEntry[] paCache;

    //scratch data
    //to not have to allocate a bunch of move bit tables and creating a lot of garbage
    //Move generation populates these globally

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
    public ulong bitboard_square_water;
    public ulong bitboard_square_rough;
    public ulong bitboard_square_normal;

    public ulong bitboard_crystalWhite;
    public ulong bitboard_crystalBlack;

    public ulong bitboard_pieces;
    public ulong bitboard_piecesWhite;
    public ulong bitboard_piecesBlack;
    public ulong bitboard_piecesNeutral;
    public ulong bitboard_piecesCrystal;

    public ulong bitboard_enhancedWhite;
    public ulong bitboard_enhancedBlack;

    public ulong bitboard_piecesMirrored;

    public ulong bitboard_piecesWhiteAdjacent;
    public ulong bitboard_piecesBlackAdjacent;

    public ulong bitboard_piecesWhiteAdjacent1;
    public ulong bitboard_piecesBlackAdjacent1;
    public ulong bitboard_piecesWhiteAdjacent2;
    public ulong bitboard_piecesBlackAdjacent2;
    public ulong bitboard_piecesWhiteAdjacent4;
    public ulong bitboard_piecesBlackAdjacent4;
    public ulong bitboard_piecesWhiteAdjacent8;
    public ulong bitboard_piecesBlackAdjacent8;

    public ulong bitboard_pawns;
    //public ulong bitboard_pawnsWhite;
    //public ulong bitboard_pawnsBlack;

    public ulong bitboard_king;
    //public ulong bitboard_kingWhite;
    //public ulong bitboard_kingBlack;

    //A lot of pieces have rough square auras or water auras
    public ulong bitboard_roughWhite;
    public ulong bitboard_roughBlack;
    public ulong bitboard_waterWhite;
    public ulong bitboard_waterBlack;

    //relay immunity to allies (or pieces with natural immunity)
    //These bitboards negate the other bitboards
    //I'm going to make relay immune just a basic adjacency because relay to protected would be slow to code
    //public ulong bitboard_immuneRelayer;
    public ulong bitboard_immuneRelayerWhite;
    public ulong bitboard_immuneRelayerBlack;
    //public ulong bitboard_immune;
    public ulong bitboard_immuneNaturalWhite;
    public ulong bitboard_immuneNaturalBlack;
    public ulong bitboard_immuneWhite;
    public ulong bitboard_immuneBlack;

    //A bunch of piece specific bitboards :/
    //Well at least holding all of these in memory is faster than manually checking proximity every time
    //These are all auras
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
    public ulong bitboard_watchTowerWhite;
    public ulong bitboard_watchTowerBlack;
    public ulong bitboard_fanWhite;
    public ulong bitboard_fanBlack;
    public ulong bitboard_hangedWhite;
    public ulong bitboard_hangedBlack;
    public ulong bitboard_virgoWhite;
    public ulong bitboard_virgoAuraWhite;
    public ulong bitboard_virgoBlack;
    public ulong bitboard_virgoAuraBlack;

    //Piece placements
    public bool arcanaMoonOutdated;
    public ulong bitboard_tarotMoon;
    //public ulong bitboard_tarotMoonWhite;
    //public ulong bitboard_tarotMoonBlack;
    public ulong bitboard_tarotMoonIllusion;
    //public ulong bitboard_tarotMoonIllusionWhite;
    //public ulong bitboard_tarotMoonIllusionBlack;

    public ulong bitboard_zombie;
    //public ulong bitboard_zombieWhite;
    //public ulong bitboard_zombieBlack;
    public ulong bitboard_abomination;
    //public ulong bitboard_abominationWhite;
    //public ulong bitboard_abominationBlack;
    public ulong bitboard_clockworksnapper;
    //public ulong bitboard_clockworksnapperWhite;
    //public ulong bitboard_clockworksnapperBlack;
    public ulong bitboard_bladebeast;
    //public ulong bitboard_bladebeastWhite;
    //public ulong bitboard_bladebeastBlack;
    public ulong bitboard_warpWeaver;
    //public ulong bitboard_warpWeaverWhite;
    //public ulong bitboard_warpWeaverBlack;
    public ulong bitboard_metalFox;
    //public ulong bitboard_metalFoxWhite;
    //public ulong bitboard_metalFoxBlack;
    public ulong bitboard_megacannon;
    //public ulong bitboard_megacannonWhite;
    //public ulong bitboard_megacannonBlack;
    public ulong bitboard_momentum;
    //public ulong bitboard_momentumWhite;
    //public ulong bitboard_momentumBlack;

    public ulong bitboard_sludgeTrail;
    public ulong bitboard_daySwapper;
    public ulong bitboard_seasonSwapper;
    public ulong bitboard_egg;
    public ulong bitboard_EOTPieces;
    public ulong bitboard_noStatus; //note that HalfShielded is off this list

    //Precompute this stuff?
    public ulong bitboard_noallyblock;
    public ulong bitboard_noenemyblock;

    public ulong bitboard_shielded;
    public ulong bitboard_warped;

    public ulong bitboard_rabbit;
    public ulong bitboard_rabbitAdjacent;

    //Secondary movers
    public ulong bitboard_secondary;
    //Aura pieces
    public ulong bitboard_aura;

    //to update
    public ulong bitboard_updatedPieces;
    public ulong bitboard_updatedEmpty;

    /*
    public BoardGlobalData()
    {
        squares = new Square[64];
        pteCache = new PieceTableCacheEntry[64];
        for (int i = 0; i < 64; i++)
        {
            pteCache[i] = new PieceTableCacheEntry();
        }
    }
    */

    public void Init()
    {
        squares = new Square[64];
        if (pteCache == null)
        {
            pteCache = new PieceTableCacheEntry[64];
        }
        /*
        for (int i = 0; i < 64; i++)
        {
            pteCache[i] = new PieceTableCacheEntry();
        }
        */
        bitboard_updatedPieces = MoveGenerator.BITBOARD_PATTERN_FULL;
        bitboard_updatedEmpty = 0;
    }

    public PieceTableEntry GetPieceTableEntryFromCache(int xy, uint piece)
    {
        //No cache layer
        //~81 ms
        //return GlobalPieceManager.GetPieceTableEntry(piece);

        //Cache layer
        //~50 ms (this array access is slow somehow?)
        //return pteCache[xy].GetFromCacheEntry(piece);

        //Removing the struct method call
        //~15 ms!!
        //Wow

        //It is slow or I need to find some optimizations
        //idea: what if I don't have a local copy
        //PieceTableCacheEntry ptce = pteCache[xy];
        //
        if (pteCache[xy].piece == piece)
        {
            return pteCache[xy].pte;
        }

        Piece.PieceType pt = Piece.GetPieceType(piece);
        if (pteCache[xy].type == pt)
        {
            pteCache[xy].piece = piece;
            return pteCache[xy].pte;
        }

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);
        //pteCache[xy] = new PieceTableCacheEntry(piece, pt, pte);
        pteCache[xy].piece = piece;
        pteCache[xy].pte = pte;
        pteCache[xy].type = pt;
        return pte;
    }

    /*
    public Piece.PieceAlignment GetPieceAlignmentFromCache(int xy, uint piece)
    {
        //No cache layer = 15 ms

        //With cache layer = ?

        PieceAlignmentCacheEntry pace = paCache[xy];
        //
        if (pace.piece == piece)
        {
            return pace.pa;
        }

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        paCache[xy] = new PieceAlignmentCacheEntry(piece, pa);
        return pa;
    }
    */
}

[Serializable]
public struct BoardGlobalPlayerInfo
{
    public short startPieceValueSumX2;
    public byte startPieceCount;

    public int highestPieceValue;
    public Piece.PieceType highestPieceType;
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
    public byte piecesLost;
    public bool capturedLastTurn;
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

    //Auto movers
    //GlobalData is a very heavy struct so I probably want to split it out of the data
    public ulong bitboard_auto;

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
        GiantTest,

        SoulMages,  //no button for it
    }

    //Badges
    //These can be added together?
    [Flags]
    public enum PlayerModifier : uint
    {
        None = 0,
        NoKing = 1,             //Equivalent to Hidden enemy property
        RelayKing = 1 << 1,     //King has Relay property
        Slippery = 1 << 2,      //All normal squares act as Slippery (for you only)
        Push = 1 << 3,          //Passive push on every piece
        Vortex = 1 << 4,        //Passive pull on every piece
        Sprint = 1 << 5,        //Double move on the first 3 turns
        Rough = 1 << 6,         //All white non pawns create rough squares in wazir range
        Defensive = 1 << 7,     //Can move only backwards infinitely
        Recall = 1 << 8,        //Can swap move with home row
        Tempering = 1 << 9,     //Capture Only -> Normal, FireCaptureOnly -> FireCapture (so you can make passive moves with Capture Only) (there are a few other CaptureOnly variants of stuff)

        //Start badges

        Rockfall = 1 << 10,     //Spawn up to 16 rocks
        Promoter = 1 << 11,     //Promotion squares on rank 6
        BronzeTreasure = 1 << 12,   //rank 6
        SilverTreasure = 1 << 13,   //rank 7
        GoldenTreasure = 1 << 14,   //rank 8

        TimeBurst = 1 << 15,    //Every 8 turns, get a double move

        FinalVengeance = 1 << 16,   //At 6 pieces or less your pieces are all Vengeful (DestroyCapturer)
        PhoenixWing = 1 << 17,     //The first piece you lose respawns like Revenant (if compatible, i.e. not a Giant)
        FirstRadiant = 1 << 18,      //The first piece you capture spawns an extra pawn for you and turns your piece Golden if possible
        SideWings = 1 << 19,         //Your pieces on the outer 4 files act like Winged if compatible
        SpectralWall = 1 << 20,     //Pieces on rank 3 act as Spectral for you
        ImmunityZone = 1 << 21,     //Middle 4 squares of home row cure your pieces effects and apply StatusImmune
        WarpZone = 1 << 22,     //Center 4x4 your pieces act like they are Warped (for non giants)
        ShieldZone = 1 << 23,      //Bishop + Knight pawn squares apply InvincibleFar2 (so they are invincible from long range)

        Seafaring = 1 << 24,    //All pieces on edge files get a special move to the opposite side (Mirror move) (Cylindrical like)
        Backdoor = 1 << 25,     //Pieces on king / queen normal start squares get a special move to the opposite top side (Sneaky like)
        Mirror = 1 << 26,       //Mirror teleport

        Forest = 1 << 27,       //Teleport to squares with at least 4 allies adjacent

        FlyingGeneral = 1 << 28,   //king attacks kings in rook range

        FullArmyWhiteBadges = Defensive | Forest | Mirror,
        SecondaryBadges = Recall | Seafaring | Backdoor | FlyingGeneral,
        PassiveShiftBadges = Push | Vortex
    }
    //These can be added together?
    [Flags]
    public enum EnemyModifier : uint
    {
        None = 0,

        Blinking = 1u << 1,    //Pieces you move must alternate starting on black and white squares
        Complacent = 1u << 2,  //Can't capture 2 turns in a row
        Defensive = 1u << 3,   //All black pieces get infinite backwards Rook / Bishop movement (move only)
        Envious = 1u << 4, //King copies the movement of your highest valued piece (Movement copied stays the same over the course of the game)
        Fusion = 1u << 5,  //King has the normal movement of all ally pieces
        Greedy = 1u << 6,  //First X captures the enemy gets Convert instead of capture
        Hidden = 1,     //~14 are compatible with no king (about half) (so about half are king affecting and half aren't)
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

        KingMoveModifiers = Fusion | Envious | Lustful | Xyloid,
        KingSpecialModifiers = KingMoveModifiers | Slothful | Obelisk | Voracious | Terror | Zenith,
        SecondaryModifiers = Defensive | KingMoveModifiers
    }

    public void Init()
    {
        blackToMove = false;
        pieces = new uint[64];
        globalData = new BoardGlobalData();
        globalData.Init();

        whitePerPlayerInfo.canCastle = true;
        blackPerPlayerInfo.canCastle = true;
        whitePerPlayerInfo.lastPieceMovedLocation = -1;
        blackPerPlayerInfo.lastPieceMovedLocation = -1;
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
            case BoardPreset.SoulMages:
                SetupSoulMages();
                break;
        }
    }
    public void Setup(Piece.PieceType[] army, PlayerModifier pm, EnemyModifier em)
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

            if ((GlobalPieceManager.GetPieceTableEntry(pt).piecePropertyB & PiecePropertyB.Giant) != 0)
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

        globalData.playerModifier = pm;

        PostSetupInit();
    }
    public void Setup(Piece.PieceType[] warmy, Piece.PieceType[] barmy, PlayerModifier pm, EnemyModifier em)
    {
        Init();

        Piece.PieceType[] ptList = warmy;

        for (int i = 0; i < pieces.Length; i++)
        {
            int indexToRead = i;
            if (indexToRead > 31)
            {
                ptList = barmy;
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

            if (GlobalPieceManager.GetPieceTableEntry(pt) == null)
            {
                Debug.Log(pt);
            }

            pieces[i] = Piece.PackPieceData(pt, i < 32 ? PieceAlignment.White : PieceAlignment.Black);

            if ((GlobalPieceManager.GetPieceTableEntry(pt).piecePropertyB & PiecePropertyB.Giant) != 0)
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

        globalData.playerModifier = pm;

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
                    modifier = PieceModifier.Radiant;
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
            PieceType.CrookedPawn, PieceType.ArcanaFortune, PieceType.Tardigrade, PieceType.Ranger, PieceType.Moon, PieceType.FastPawn, PieceType.Runner, PieceType.Canoe,
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
            PieceType.Snake, PieceType.GravityMage, PieceType.Leafling, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
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

    public void SetupSoulMages()
    {
        Init();

        Piece.PieceType[] ptList = new Piece.PieceType[]
        {
            PieceType.QueenLeech, PieceType.SoulDevourer, PieceType.QueenLeech, PieceType.Lich, PieceType.King, 0, 0, PieceType.SoulDevourer,
            PieceType.Leech, PieceType.SoulCannon, PieceType.Leech, PieceType.Lich, PieceType.Pawn, 0, 0, 0,
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

    public void PostSetupInit()
    {
        ModifierSetup();

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
                if (pte.type == PieceType.ArcanaFool)
                {
                    globalData.noLastMoveHash = false;
                }
                if (pte.type == PieceType.Imitator)
                {
                    globalData.noLastMoveHash = false;
                }
            }
        }
    }

    public static List<Piece.PieceType> EnemyModifierExtraPieces(EnemyModifier em)
    {
        List<PieceType> newpieces = new List<PieceType>();

        if ((em & EnemyModifier.Jester) != 0)
        {
            newpieces.Add(PieceType.Jester);
            newpieces.Add(PieceType.Jester);
            newpieces.Add(PieceType.Jester);
        }
        if ((em & EnemyModifier.Numerous) != 0)
        {
            newpieces.Add(PieceType.King);
            newpieces.Add(PieceType.King);
        }
        if ((em & EnemyModifier.Queenly) != 0)
        {
            newpieces.Add(PieceType.Queen);
            newpieces.Add(PieceType.Princess);
        }

        return newpieces;
    }

    public void ModifierSetup()
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

        if ((globalData.playerModifier & PlayerModifier.NoKing) != 0)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White && Piece.GetPieceType(pieces[i]) == PieceType.King)
                {
                    pieces[i] = 0;
                }
            }
        }

        if ((globalData.playerModifier & PlayerModifier.Rockfall) != 0)
        {
            int attempts = 0;
            int rocks = 0;
            while (attempts < 1000)
            {
                int randPosition = UnityEngine.Random.Range(0, 64);

                if (pieces[randPosition] == 0)
                {
                    pieces[randPosition] = Piece.SetPieceAlignment(PieceAlignment.Neutral, Piece.SetPieceType(PieceType.Rock, 0));
                    rocks++;
                    if (rocks >= 16)
                    {
                        break;
                    }
                }
                attempts++;
            }
        }

        if ((globalData.playerModifier & PlayerModifier.Promoter) != 0)
        {
            int attempts = 0;
            int promotionSquares = 0;
            while (attempts < 1000)
            {
                int randPosition = UnityEngine.Random.Range(0, 8);

                if (globalData.squares[randPosition + 40].type == Square.SquareType.Normal)
                {
                    globalData.squares[randPosition + 40].type = Square.SquareType.Promotion;
                    promotionSquares++;
                    if (promotionSquares >= 2)
                    {
                        break;
                    }
                }
                attempts++;
            }
            //globalData.squares[50] = new Square(Square.SquareType.Promotion);
            //globalData.squares[53] = new Square(Square.SquareType.Promotion);
        }

        if ((globalData.playerModifier & PlayerModifier.BronzeTreasure) != 0)
        {
            int attempts = 0;
            while (attempts < 1000)
            {
                int randPosition = UnityEngine.Random.Range(0, 8);

                if (globalData.squares[randPosition + 40].type == Square.SquareType.Normal)
                {
                    globalData.squares[randPosition + 40].type = Square.SquareType.BronzeTreasure;
                    break;
                }
                attempts++;
            }
        }

        if ((globalData.playerModifier & PlayerModifier.SilverTreasure) != 0)
        {
            int attempts = 0;
            while (attempts < 1000)
            {
                int randPosition = UnityEngine.Random.Range(0, 8);

                if (globalData.squares[randPosition + 48].type == Square.SquareType.Normal)
                {
                    globalData.squares[randPosition + 48].type = Square.SquareType.SilverTreasure;
                    break;
                }
                attempts++;
            }
        }

        if ((globalData.playerModifier & PlayerModifier.GoldenTreasure) != 0)
        {
            int attempts = 0;
            int goldSquares = 0;
            while (attempts < 1000)
            {
                int randPosition = UnityEngine.Random.Range(0, 8);

                if (globalData.squares[randPosition + 56].type == Square.SquareType.Normal)
                {
                    globalData.squares[randPosition + 56].type = Square.SquareType.GoldTreasure;
                    goldSquares++;
                    if (goldSquares >= 2)
                    {
                        break;
                    }
                }
                attempts++;
            }
        }
    }

    public Piece.Aura[] GetAuraBitboards(bool black)
    {
        Piece.Aura[] output = new Aura[64];
        for (int i = 0; i < 64; i++)
        {
            ulong bitIndex = 1uL << i;

            if (black)
            {
                if ((bitIndex & globalData.bitboard_virgoAuraBlack) != 0)
                {
                    output[i] |= Aura.Nullify;
                }
                if ((bitIndex & globalData.bitboard_bansheeBlack) != 0)
                {
                    output[i] |= Aura.Banshee;
                }
                if ((bitIndex & globalData.bitboard_attractorBlack) != 0)
                {
                    output[i] |= Aura.Attractor;
                }
                if ((bitIndex & globalData.bitboard_repulserBlack) != 0)
                {
                    output[i] |= Aura.Repulser;
                }
                if ((bitIndex & globalData.bitboard_harpyBlack) != 0)
                {
                    output[i] |= Aura.Harpy;
                }
                if ((bitIndex & globalData.bitboard_hagBlack) != 0)
                {
                    output[i] |= Aura.Hag;
                }
                if ((bitIndex & globalData.bitboard_slothBlack) != 0)
                {
                    output[i] |= Aura.Sloth;
                }
                if ((bitIndex & globalData.bitboard_watchTowerBlack) != 0)
                {
                    output[i] |= Aura.Watchtower;
                }
                if ((bitIndex & globalData.bitboard_fanBlack) != 0)
                {
                    output[i] |= Aura.Fan;
                }
                if ((bitIndex & globalData.bitboard_hangedBlack) != 0)
                {
                    output[i] |= Aura.Hanged;
                }
                if ((bitIndex & globalData.bitboard_roughBlack) != 0)
                {
                    output[i] |= Aura.Rough;
                }
                if ((bitIndex & globalData.bitboard_waterBlack) != 0)
                {
                    output[i] |= Aura.Water;
                }
                if ((bitIndex & MainManager.SmearBitboard(globalData.bitboard_immuneRelayerBlack)) != 0)
                {
                    output[i] |= Aura.Immune;
                }
            }
            else
            {
                if ((bitIndex & globalData.bitboard_virgoAuraWhite) != 0)
                {
                    output[i] |= Aura.Nullify;
                }
                if ((bitIndex & globalData.bitboard_bansheeWhite) != 0)
                {
                    output[i] |= Aura.Banshee;
                }
                if ((bitIndex & globalData.bitboard_attractorWhite) != 0)
                {
                    output[i] |= Aura.Attractor;
                }
                if ((bitIndex & globalData.bitboard_repulserWhite) != 0)
                {
                    output[i] |= Aura.Repulser;
                }
                if ((bitIndex & globalData.bitboard_harpyWhite) != 0)
                {
                    output[i] |= Aura.Harpy;
                }
                if ((bitIndex & globalData.bitboard_hagWhite) != 0)
                {
                    output[i] |= Aura.Hag;
                }
                if ((bitIndex & globalData.bitboard_slothWhite) != 0)
                {
                    output[i] |= Aura.Sloth;
                }
                if ((bitIndex & globalData.bitboard_watchTowerWhite) != 0)
                {
                    output[i] |= Aura.Watchtower;
                }
                if ((bitIndex & globalData.bitboard_fanWhite) != 0)
                {
                    output[i] |= Aura.Fan;
                }
                if ((bitIndex & globalData.bitboard_hangedWhite) != 0)
                {
                    output[i] |= Aura.Hanged;
                }
                if ((bitIndex & globalData.bitboard_roughWhite) != 0)
                {
                    output[i] |= Aura.Rough;
                }
                if ((bitIndex & globalData.bitboard_waterWhite) != 0)
                {
                    output[i] |= Aura.Water;
                }
                if ((bitIndex & MainManager.SmearBitboard(globalData.bitboard_immuneRelayerWhite)) != 0)
                {
                    output[i] |= Aura.Immune;
                }
            }
        }

        return output;
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

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(pieces[i]));

            if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
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

                if (pte.type == PieceType.King)
                {
                    continue;
                }
                if (globalData.whitePerPlayerInfo.highestPieceValue <= pte.pieceValueX2 || (globalData.whitePerPlayerInfo.highestPieceValue == pte.pieceValueX2 && globalData.whitePerPlayerInfo.highestPieceType < Piece.GetPieceType(pieces[i])))
                {
                    globalData.whitePerPlayerInfo.highestPieceValue = pte.pieceValueX2;
                    globalData.whitePerPlayerInfo.highestPieceType = Piece.GetPieceType(pieces[i]);
                }
            } else if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
            {
                globalData.blackPerPlayerInfo.startPieceValueSumX2 += pte.pieceValueX2;
                globalData.blackPerPlayerInfo.startPieceCount++;

                if (pte.type == PieceType.King)
                {
                    continue;
                }
                if (globalData.blackPerPlayerInfo.highestPieceValue <= pte.pieceValueX2 || (globalData.blackPerPlayerInfo.highestPieceValue == pte.pieceValueX2 && globalData.blackPerPlayerInfo.highestPieceType < Piece.GetPieceType(pieces[i])))
                {
                    globalData.blackPerPlayerInfo.highestPieceValue = pte.pieceValueX2;
                    globalData.blackPerPlayerInfo.highestPieceType = Piece.GetPieceType(pieces[i]);
                }
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

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(pieces[i]));

            if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
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
                    whitePerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.GetPieceTableEntry((PieceType)Piece.GetPieceSpecialData(pieces[i])).pieceValueX2;
                }
            }
            if (Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                blackPerPlayerInfo.pieceCount++;

                if ((pte.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(pieces[i]) != 0)
                {
                    blackPerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.GetPieceTableEntry((PieceType)Piece.GetPieceSpecialData(pieces[i])).pieceValueX2;
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
        globalData.Init();
    }
    public Board(Board b)
    {
        /*
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
        */
        CopyOverwrite(b);
    }

    public void SplitGlobalData()
    {
        BoardGlobalData bgd = new BoardGlobalData();
        bgd.Init();

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
        globalData.bitboard_square_rough = 0;
        globalData.bitboard_square_normal = 0;
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
                case Square.SquareType.Water:
                    globalData.bitboard_square_water |= bitIndex;
                    break;
                case Square.SquareType.Rough:
                    globalData.bitboard_square_rough |= bitIndex;
                    break;
                case Square.SquareType.Normal:
                    globalData.bitboard_square_normal |= bitIndex;
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

        bitboard_auto = b.bitboard_auto;
        whitePerPlayerInfo = b.whitePerPlayerInfo;
        blackPerPlayerInfo = b.blackPerPlayerInfo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetPieceAtCoordinate(int x, int y)
    {
        return pieces[x + (y << 3)];
        //return pieces[CoordinateConvert(x, y)];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPieceAtCoordinate(int x, int y, uint set)
    {
        /*
        //will slow things down?
        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            return;
        }
        */

        //pieces[CoordinateConvert(x, y)] = set;
        pieces[x + (y << 3)] = set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square GetSquareAtCoordinate(int x, int y)
    {
        return globalData.squares[x + (y << 3)];
        //return globalData.squares[CoordinateConvert(x, y)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetMissingPieces(bool isBlack)
    {
        if (isBlack)
        {
            return (byte)(blackPerPlayerInfo.piecesLost);
        } else
        {
            return (byte)(whitePerPlayerInfo.piecesLost);
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

        //Currently removed because it is too easy?
        //Or at least the AI is not good enough to look far enough ahead to stop you
        //Some of those endgames might be hard to stop anyway (opposition)
        //But in theory one side can optimally stop your king
        //Idea: maybe I can encourage the AI to maintain opposition if the king would run past?

        //Overtaken victory
        int ylevel = 7;
        if ((globalData.enemyModifier & EnemyModifier.Zenith) == 0)
        {
            //Not possible with Zenith active (would be dumb if you could cheese the final boss early)
            for (int i = 0; i < 8; i++)
            {
                uint target = pieces[i + (ylevel << 3)];
                if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.White)
                {
                    return PieceAlignment.White;
                }
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

    public void ApplyNullMove(bool applyTurnEnd = true)
    {
        if (applyTurnEnd)
        {
            RunTurnEnd(blackToMove, false, null);
        }

        //to correctly check for check instead of running into the "no moving the same piece the enemy did" thing
        if (blackToMove)
        {
            blackPerPlayerInfo.lastPieceMovedLocation = -1;
        }
        else
        {
            whitePerPlayerInfo.lastPieceMovedLocation = -1;
        }

        bonusPly = 0;
        blackToMove = !blackToMove;
    }
    public void ApplyMove(uint move)
    {
        ApplyMove(move, null);
    }
    public void ApplyMove(uint move, List<BoardUpdateMetadata> boardUpdateMetadata, bool applyTurnEnd = true)
    {
        //bad
        //Only really exists to fix Arcana Moon
        //Profiling says this isn't the biggest thing though?
        //(The problem is the big slow loop in TickDownStatusEffects?)
        //MoveGeneratorInfoEntry.GeneratePieceBitboards(this);

        //new: specialized method
        //MoveGeneratorInfoEntry.FixArcanaMoon(this);

        //new 2: only update if necessary
        globalData.arcanaMoonOutdated = true;

        //I can do this because the board update metadata being present means it isn't the ai looking through the game tree
        //So this being slow is not a problem
        if (boardUpdateMetadata != null)
        {
            //Unset to debug incorrectly tracked values
            //This being unset should not change anything but it's mostly here in case of rare problems
            //ResetPieceValueCount();
        }

        //Note: Setup moves bypass this system entirely (it doesn't run in the normal turn cycle)
        if (Move.IsConsumableMove(move))
        {
            ApplyConsumableMove(move, boardUpdateMetadata);

            if (applyTurnEnd)
            {
                RunTurnEnd(blackToMove, false, boardUpdateMetadata);
            }
            else
            {
                ApplyZenithEffect(boardUpdateMetadata);
            }
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
        int fxy = (fx + (fy << 3));
        int tx = Move.GetToX(move);
        int ty = Move.GetToY(move);
        int txy = (tx + (ty << 3));

        //Very safe assumption that the move you do is going to touch the from square and the to square somehow
        //There are cases where this assumption is wrong but those are rare
        globalData.bitboard_updatedPieces |= (1uL << fxy);
        globalData.bitboard_updatedPieces |= (1uL << txy);

        Move.Dir dir = Move.GetDir(move);

        Move.SpecialType specialType = Move.GetSpecialType(move);

        uint oldPiece = pieces[fxy]; //GetPieceAtCoordinate(fx, fy);
        uint targetPiece = pieces[txy]; //GetPieceAtCoordinate(tx, ty);

        PieceTableEntry pteO = globalData.GetPieceTableEntryFromCache(fxy, oldPiece); //GlobalPieceManager.GetPieceTableEntry(opt);
        Piece.PieceType opt = pteO.type; //Piece.GetPieceType(oldPiece);

        if (opt == PieceType.Null)
        {
            //not a legal
            if (applyTurnEnd)
            {
                RunTurnEnd(blackToMove, false, boardUpdateMetadata);
            }
            else
            {
                ApplyZenithEffect(boardUpdateMetadata);
            }

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

        bool lastMoveStationary = Move.SpecialMoveStationary(specialType);
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

        Piece.PieceAlignment opa = Piece.GetPieceAlignment(oldPiece);

        int blackPremovePiecesLost = blackPerPlayerInfo.piecesLost;

        if ((pteO.piecePropertyB & PiecePropertyB.Giant) != 0)
        {
            ApplyGiantMove(oldPiece, opa, fx, fy, tx, ty, pteO, boardUpdateMetadata);
            if (applyTurnEnd)
            {
                RunTurnEnd(blackToMove, false, boardUpdateMetadata);
            }
            else
            {
                ApplyZenithEffect(boardUpdateMetadata);
            }

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
            pteT = globalData.GetPieceTableEntryFromCache(txy, targetPiece); //GlobalPieceManager.GetPieceTableEntry(targetPiece);
        }
        Piece.PieceAlignment tpa = Piece.GetPieceAlignment(targetPiece);
        Piece.PieceStatusEffect pseO = Piece.GetPieceStatusEffect(oldPiece);

        bool enemyMove = tpa != opa;

        switch (pseO)
        {
            case PieceStatusEffect.Sparked:
                oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                break;
            case PieceStatusEffect.Bloodlust:
                //bloodlust
                //Some special cases have to be handled separately
                if (!passiveMove && enemyMove && Move.SpecialMoveCaptureLike(specialType))
                {
                    oldPiece = Piece.SetPieceStatusEffect(0, oldPiece);
                    oldPiece = Piece.SetPieceStatusDuration(0, oldPiece);
                }
                break;
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
            case SpecialType.KingAttack:
            case SpecialType.ChargeMoveReset:
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case Move.SpecialType.FlyingMoveOnly:
            case Move.SpecialType.PullMove:
            case Move.SpecialType.SlipMove:
            case SpecialType.GliderMove:
            case SpecialType.CoastMove:
            case SpecialType.ShadowMove:
            case Move.SpecialType.PlantMove:
                if (!passiveMove && blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Greedy) != 0 && (whitePerPlayerInfo.piecesLost) < 2 && enemyMove)
                {
                    //set to stationary
                    lastMoveStationary = true;
                    blackPerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
                    goto case SpecialType.Convert;
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
                        whitePerPlayerInfo.lastMove = Move.SetSpecialType(Move.SpecialType.Normal, whitePerPlayerInfo.lastMove);
                        whitePerPlayerInfo.lastPieceMovedLocation = (tx + ty * 8);
                    }
                    lastMoveStationary = false;
                }

                //no check for ChargeMoveReset because the reset happens later?
                //Minor inefficiency but ehh
                if ((pteO.piecePropertyB & PiecePropertyB.ChargeByMoving) != 0 && specialType != SpecialType.ChargeMove)
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
                        if (globalData.arcanaMoonOutdated)
                        {
                            MoveGenerator.FixArcanaMoon(this);
                        }
                        if (opa == PieceAlignment.White)
                        {
                            moonBitboard = globalData.bitboard_tarotMoonIllusion & globalData.bitboard_piecesWhite;
                            whitePerPlayerInfo.pieceCount++;
                            //MainManager.PrintBitboard(moonBitboard);
                        }
                        else if (opa == PieceAlignment.Black)
                        {
                            moonBitboard = globalData.bitboard_tarotMoonIllusion & globalData.bitboard_piecesBlack;
                            blackPerPlayerInfo.pieceCount++;
                            //MainManager.PrintBitboard(moonBitboard);
                        }
                        while (moonBitboard != 0)
                        {
                            int index = MainManager.PopBitboardLSB1(moonBitboard, out moonBitboard);

                            //the bitboard will get filled with stuff for parallel moves and become corrupted
                            //but the original data will exist within so this is still guaranteed to hit all of them?
                            Piece.PieceType spt = Piece.GetPieceType(pieces[index]);
                            if (spt == PieceType.ArcanaMoon) // || spt == PieceType.MoonIllusion)
                            {
                                globalData.bitboard_updatedPieces |= (1uL << index);
                                //was commented before, not sure why I did that? Some bug?
                                if (boardUpdateMetadata != null && spt != PieceType.MoonIllusion)
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, pieces[index], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                                }
                                pieces[index] = Piece.SetPieceType(Piece.PieceType.MoonIllusion, pieces[index]);
                            }
                        }
                        break;
                    case PieceType.Necromancer:
                        if (!passiveMove)
                        {
                            residuePiece = Piece.SetPieceType(PieceType.Skeleton, oldPiece);

                            //Add the value of the skeleton
                            PieceTableEntry pteS = GlobalPieceManager.GetPieceTableEntry(PieceType.Skeleton);

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
                    case PieceType.SteelGolem:
                    case PieceType.SteelPuppet:
                    case PieceType.MetalFox:
                    case PieceType.Cannon:
                        oldPiece = Piece.SetPieceSpecialData(0, oldPiece);
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

                    Piece.PieceType tpt = pteT.type; //Piece.GetPieceType(targetPiece);

                    bool pieceChange = false;

                    //King is immutable
                    //(Note that the way king capture is detected means that any time where you have a move that destroys your own king that counts against your opponent)
                    if (pteO.type == PieceType.King)
                    {
                        pieceChange = true;
                    }

                    switch (tpa)
                    {
                        case PieceAlignment.White:
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                            break;
                        case PieceAlignment.Black:
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                            break;
                    }

                    //note: you can consume enemies but allies is easier for you to do
                    //Leech doesn't because it promotes to Queen Leech
                    //(I don't want Leeches to be more powerful)
                    //Don't let you get power from eating rocks (Rocks are possibly very plentiful so it would be very overpowered)
                    if ((specialType == SpecialType.ConsumeAllies || specialType == SpecialType.ConsumeAlliesCaptureOnly) && opt != PieceType.Leech && pteT.pieceValueX2 > 0)
                    {
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) + 1), oldPiece);
                    }

                    ulong hagBitboard = 0;
                    switch (opa)
                    {
                        case PieceAlignment.White:
                            hagBitboard = globalData.bitboard_hagBlack;
                            break;
                        case PieceAlignment.Black:
                            hagBitboard = globalData.bitboard_hagWhite;
                            break;
                    }

                    //Debug.Log("Hag check" + opa);
                    //MainManager.PrintBitboard(hagBitboard);

                    //destroy capturer

                    bool wrathDestruction = false;
                    if (!blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Wrathful) != 0 && (whitePerPlayerInfo.piecesLost) < 2)
                    {
                        wrathDestruction = true;
                    }

                    if (!pieceChange && opt != PieceType.King && (wrathDestruction || (pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & (PieceProperty.DestroyOnCapture)) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((1uL << (fx + fy * 8) & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful) || (tpa == PieceAlignment.White && ((globalData.playerModifier & PlayerModifier.FinalVengeance) != 0) && whitePerPlayerInfo.pieceCount <= 6))
                    {
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
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
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
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

                                if (j == 0 || i == 0)
                                {
                                    continue;
                                }

                                PieceTableEntry pteE = globalData.GetPieceTableEntryFromCache(tx + i + 8 * (ty + j), pieces[tx + i + 8 * (ty + j)]);

                                //delete the enemies on the delta
                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + i, ty + j));
                                if (epa != opa && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.FireCapture, pteO, pteE))
                                {
                                    if (epa == PieceAlignment.White)
                                    {
                                        whitePerPlayerInfo.pieceCount--;
                                        whitePerPlayerInfo.piecesLost++;
                                        whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }
                                    if (epa == PieceAlignment.Black)
                                    {
                                        blackPerPlayerInfo.pieceCount--;
                                        blackPerPlayerInfo.piecesLost++;
                                        blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }

                                    DeletePieceAtCoordinate(tx + i, ty + j, pteE, epa, boardUpdateMetadata);
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
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
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

                                PieceTableEntry pteE = globalData.GetPieceTableEntryFromCache(tx + i + 8 * (ty + j), pieces[tx + i + 8 * (ty + j)]);

                                //delete the enemies on the delta
                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + i, ty + j));
                                if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.FireCapture, pteO, pteE)) {
                                    if (epa == PieceAlignment.White)
                                    {
                                        whitePerPlayerInfo.pieceCount--;
                                        whitePerPlayerInfo.piecesLost++;
                                        whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }
                                    if (epa == PieceAlignment.Black)
                                    {
                                        blackPerPlayerInfo.pieceCount--;
                                        blackPerPlayerInfo.piecesLost++;
                                        blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                    }

                                    DeletePieceAtCoordinate(tx + i, ty + j, pteE, epa, boardUpdateMetadata);
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
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
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

                                int txyij = tx + i + ((ty + j) << 3);

                                uint tempPiece = pieces[txyij];
                                PieceTableEntry pteE = globalData.GetPieceTableEntryFromCache(txyij, tempPiece); // GlobalPieceManager.GetPieceTableEntry(pieces[tx + i + 8 * (ty + j)]);

                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(tempPiece);
                                if (epa != opa && pteE != null && !Piece.IsPieceInvincible(this, tempPiece, tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.InflictFreeze, pteO, pteE))
                                {
                                    pieces[txyij] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, tempPiece));
                                    globalData.bitboard_updatedPieces |= (1uL << txyij);
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pieces[txyij], BoardUpdateMetadata.BoardUpdateType.StatusApply));
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
                            whitePerPlayerInfo.piecesLost++;
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

                                int txyij = tx + i + ((ty + j) << 3);

                                uint tempPiece = pieces[txyij];
                                PieceTableEntry pteE = globalData.GetPieceTableEntryFromCache(txyij, tempPiece); //GlobalPieceManager.GetPieceTableEntry(tempPiece);

                                Piece.PieceAlignment epa = Piece.GetPieceAlignment(tempPiece);
                                if (epa != opa && pteE != null && !Piece.IsPieceInvincible(this, tempPiece, tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.Inflict, pteO, pteE))
                                {
                                    pieces[txyij] = Piece.SetPieceStatusEffect(PieceStatusEffect.Poisoned, Piece.SetPieceStatusDuration(3, tempPiece));
                                    globalData.bitboard_updatedPieces |= (1uL << txyij);
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pieces[txyij], BoardUpdateMetadata.BoardUpdateType.StatusApply));
                                    }
                                }
                            }
                        }
                        pieceChange = true;
                    }
                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.HoneyExplode) != 0)
                    {
                        //Also explodes self
                        oldPiece = 0;
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        }
                        pieceChange = true;

                        //Slap honey puddles around
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

                                int txyij = tx + i + ((ty + j) << 3);
                                if ((i == 0 || j == 0) && pieces[txyij] == 0)
                                {
                                    pieces[txyij] = Piece.SetPieceType(PieceType.HoneyPuddle, Piece.SetPieceAlignment(PieceAlignment.Neutral, 0));
                                    globalData.bitboard_updatedPieces |= (1uL << txyij);
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pieces[txyij], BoardUpdateMetadata.BoardUpdateType.Spawn));
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
                        PieceTableEntry pteP = GlobalPieceManager.GetPieceTableEntry(pteO.promotionType);
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
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            blackPerPlayerInfo.pieceCount++;
                            blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            whitePerPlayerInfo.pieceCount++;
                            whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                        }
                        pieceChange = true;
                    }

                    //Neutral converting to neutral doesn't make sense
                    if (!pieceChange && opa != PieceAlignment.Neutral && (pteO.piecePropertyB & PiecePropertyB.NeutralOnCapture) != 0)
                    {
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Neutral, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            oldPiece = (Piece.SetPieceAlignment(PieceAlignment.Neutral, oldPiece));
                        }
                        pieceChange = true;
                    }

                    if (!pieceChange && (pteO.piecePropertyB & PiecePropertyB.StatusImmune) == 0 && Piece.GetPieceModifier(oldPiece) != PieceModifier.Immune)
                    {
                        if ((pteT.piecePropertyB & PiecePropertyB.FreezeCapturer) != 0)
                        {
                            oldPiece = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, oldPiece));
                            //txy already gets put on the update bitboard no matter what
                            //globalData.bitboard_updatedPieces |= (1uL << txy);

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.StatusApply, true));
                            }
                            //pieceChange = true;
                        }
                        if ((pteT.piecePropertyB & PiecePropertyB.PoisonCapturer) != 0)
                        {
                            oldPiece = Piece.SetPieceStatusEffect(PieceStatusEffect.Poisoned, Piece.SetPieceStatusDuration(3, oldPiece));
                            //globalData.bitboard_updatedPieces |= (1uL << txy);

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.StatusApply, true));
                            }
                            //pieceChange = true;
                        }
                    }
                }

                if (boardUpdateMetadata != null && pteO.type != PieceType.King)
                {
                    if (oldPiece == 0)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Capture, true));
                    }
                    else
                    {
                        if (opt == Piece.GetPieceType(oldPiece))
                        {
                            if (opa != Piece.GetPieceAlignment(oldPiece))
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.AlignmentChange, true));
                            }
                        }
                        else
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange, true));
                        }
                    }

                    if (residuePiece != 0)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, fx, fy, residuePiece, BoardUpdateMetadata.BoardUpdateType.Spawn, false));
                    }
                }

                if ((pteO.piecePropertyB & (PiecePropertyB.AnyMomentum)) != 0)
                {
                    if ((pteO.piecePropertyB & (PiecePropertyB.ForwardMomentum)) != 0)
                    {
                        oldPiece = Piece.SetPieceSpecialData((ushort)(dir), oldPiece);
                    }
                    if ((pteO.piecePropertyB & (PiecePropertyB.ReverseMomentum)) != 0)
                    {
                        oldPiece = Piece.SetPieceSpecialData((ushort)(Move.ReverseDir(dir)), oldPiece);
                    }
                }

                //pull moves over here
                switch (specialType) {
                    case Move.SpecialType.PullMove:
                        //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        pieces[(fx + (fy << 3))] = residuePiece;
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                            pieces[tx + (ty << 3)] = oldPiece;
                        }

                        if (oldPiece == 0)
                        {
                            DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
                        }

                        //pull move is just normal with an extra step
                        (int pulldx, int pulldy) = Move.DirToDelta(dir);
                        pulldx = -pulldx;
                        pulldy = -pulldy;

                        if (((((fx + pulldx) | (fy + pulldy)) & -8) != 0))
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
                        if ((pteP.piecePropertyB & PiecePropertyB.TrueShiftImmune) != 0)
                        {
                            //goto case Move.SpecialType.Normal;
                            break;
                        }

                        //Rare edge case with Knave + (pull mover)
                        if (((((tx + pulldx) | (ty + pulldy)) & -8) != 0))
                        {
                            break;
                        }

                        //There is another edge case :P
                        //Pulling pieces through a piece you can pass through (need to forbid this)
                        if (GetPieceAtCoordinate(tx + pulldx, ty + pulldy) != 0)
                        {
                            break;
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx + pulldx, fy + pulldy, tx + pulldx, ty + pulldy, pullPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (tx + pulldx + ((ty + pulldy) << 3)));
                        globalData.bitboard_updatedPieces |= (1uL << (fx + pulldx + ((fy + pulldy) << 3)));
                        SetPieceAtCoordinate(tx + pulldx, ty + pulldy, pullPiece);
                        SetPieceAtCoordinate(fx + pulldx, fy + pulldy, 0);
                        break;
                    case SpecialType.ChargeMoveReset:
                        oldPiece = Piece.SetPieceSpecialData(0, oldPiece);
                        //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        pieces[(fx + (fy << 3))] = residuePiece;
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            //To avoid problems with radiant pieces and such I'll only do stuff to passive moves?
                            //This should still save a lot of updates
                            if (residuePiece == 0)
                            {
                                globalData.bitboard_updatedEmpty |= (1uL << (fx + (fy << 3)));
                            }
                            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                            pieces[tx + (ty << 3)] = oldPiece;
                        }

                        if (oldPiece == 0)
                        {
                            DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
                        }
                        break;
                    case SpecialType.ChargeMove:
                        if (Piece.GetPieceSpecialData(oldPiece) > 0)
                        {
                            oldPiece = Piece.SetPieceSpecialData((ushort)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                        }
                        //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        pieces[(fx + (fy << 3))] = residuePiece;
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            //To avoid problems with radiant pieces and such I'll only do stuff to passive moves?
                            //This should still save a lot of updates
                            if (residuePiece == 0)
                            {
                                globalData.bitboard_updatedEmpty |= (1uL << (fx + (fy << 3)));
                            }
                            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                            pieces[tx + (ty << 3)] = oldPiece;
                        }

                        if (oldPiece == 0)
                        {
                            DeletePieceAtCoordinate(tx, ty, pteO, opa, boardUpdateMetadata);
                        }
                        break;
                    default:
                        //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, residuePiece);
                        pieces[(fx + (fy << 3))] = residuePiece;
                        if (!passiveMove)
                        {
                            CapturePieceAtCoordinate(tx, ty, oldPiece, pteO, opa, pteT, tpa, boardUpdateMetadata);
                        }
                        else
                        {
                            //To avoid problems with radiant pieces and such I'll only do stuff to passive moves?
                            //This should still save a lot of updates
                            if (residuePiece == 0)
                            {
                                globalData.bitboard_updatedEmpty |= (1uL << (fx + (fy << 3)));
                            }
                            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
                            pieces[tx + (ty << 3)] = oldPiece;
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
            case SpecialType.ConvertRabbit:
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
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                }
                if (opa == PieceAlignment.White)
                {
                    //I get a piece
                    whitePerPlayerInfo.pieceCount++;
                    whitePerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    //I get a piece
                    blackPerPlayerInfo.pieceCount++;
                    blackPerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
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
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, Piece.SetPieceAlignment(opa, targetPiece), BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                }

                if ((pteO.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
                {
                    if (opa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        blackPerPlayerInfo.pieceCount++;
                        blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                        SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                    }
                    if (opa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
                        blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                        whitePerPlayerInfo.pieceCount++;
                        whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                        SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                    }

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, GetPieceAtCoordinate(fx, fy), BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                    }
                }

                if ((pteT.piecePropertyB & PiecePropertyB.Giant) != 0)
                {
                    ushort pieceValue = Piece.GetPieceSpecialData(targetPiece);

                    int gdx = pieceValue & 1;
                    int gdy = (pieceValue & 2) >> 1;

                    //make this a +1 or -1
                    gdx *= 2;
                    gdx = 1 - gdx;
                    gdy *= 2;
                    gdy = 1 - gdy;

                    globalData.bitboard_updatedPieces |= (1uL << (txy));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + gdx));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + (gdy << 3)));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + gdx + (gdy << 3)));

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
                    PieceTableEntry pteL = globalData.GetPieceTableEntryFromCache(tempX + (tempY << 3), leapTarget); //GlobalPieceManager.GetPieceTableEntry(leapTarget);
                    if (leapTarget != 0 && (Piece.GetPieceAlignment(leapTarget) != opa && !Piece.IsPieceInvincible(this, leapTarget, tempX, tempY, oldPiece, fx, fy, Move.SpecialType.LongLeaper, pteO, pteL)))
                    {
                        //Debug.Log("Delete at " + tempX + " " + tempY);
                        didCapture = true;
                        if (Piece.GetPieceAlignment(leapTarget) == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteL.pieceValueX2;
                        }
                        if (Piece.GetPieceAlignment(leapTarget) == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
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
                    if (((((tempX) | (tempY)) & -8) != 0))
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
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;

                    //I get a piece
                    blackPerPlayerInfo.pieceCount++;
                    blackPerPlayerInfo.pieceValueSumX2 += pteT.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
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

                if ((pteT.piecePropertyB & PiecePropertyB.Giant) != 0)
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

                    globalData.bitboard_updatedPieces |= (1uL << (txy));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + gdx));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + (gdy << 3)));
                    globalData.bitboard_updatedPieces |= (1uL << (txy + gdx + (gdy << 3)));

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

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, GetPieceAtCoordinate(tx, ty), BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
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
                    if (blackToMove && (globalData.enemyModifier & Board.EnemyModifier.Greedy) != 0 && (whitePerPlayerInfo.piecesLost) < 2)
                    {
                        if (enemyMove)
                        {
                            //set to stationary
                            blackPerPlayerInfo.lastPieceMovedLocation = (fx + fy * 8);
                            goto case SpecialType.Convert;
                        }
                    }

                    if (tpa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (tpa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
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
                                oldPiece = Piece.SetPieceSpecialData((ushort)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                                SetPieceAtCoordinate(fx, fy, oldPiece);
                            }
                        }
                        else
                        {
                            if (pteT.pieceValueX2 > 0)
                            {
                                //Get point for fire capture
                                oldPiece = Piece.SetPieceSpecialData((ushort)(Piece.GetPieceSpecialData(oldPiece) + 1), oldPiece);
                                SetPieceAtCoordinate(fx, fy, oldPiece);
                            }
                        }
                    }
                    if ((pteO.piecePropertyB & (PiecePropertyB.ChargeEnhanceStackReset)) != 0)
                    {
                        if (Piece.GetPieceSpecialData(oldPiece) > 0)
                        {
                            oldPiece = Piece.SetPieceSpecialData(0, oldPiece);
                            SetPieceAtCoordinate(fx, fy, oldPiece);
                        }
                    }

                    switch (opt)
                    {
                        case PieceType.SteelGolem:
                        case PieceType.SteelPuppet:
                        case PieceType.MetalFox:
                        case PieceType.Cannon:
                            oldPiece = Piece.SetPieceSpecialData(0, oldPiece);
                            break;
                    }

                    //Outlaw switching sides
                    if ((pteO.piecePropertyB & PiecePropertyB.EnemyOnCapture) != 0)
                    {
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount--;
                            whitePerPlayerInfo.piecesLost++;
                            whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            blackPerPlayerInfo.pieceCount++;
                            blackPerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.Black, oldPiece));
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount--;
                            blackPerPlayerInfo.piecesLost++;
                            blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                            whitePerPlayerInfo.pieceCount++;
                            whitePerPlayerInfo.pieceValueSumX2 += pteO.pieceValueX2;
                            SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(PieceAlignment.White, oldPiece));
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, GetPieceAtCoordinate(fx, fy), BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
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

                //weird rare bug is causing illegal pushes, double check I guess
                if ((((tx + pushdx) | (ty + pushdy)) & -8) != 0 || GetPieceAtCoordinate(tx + pushdx, ty + pushdy) != 0)
                {
                    goto case Move.SpecialType.Normal;
                }

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tx + pushdx, ty + pushdy, targetPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                }

                globalData.bitboard_updatedPieces |= (1uL << (txy + pushdx + (pushdy << 3)));

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
                if (((((tx + a2dx) | (ty + a2dy)) & -8) != 0))
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
                PieceTableEntry pteA2 = globalData.GetPieceTableEntryFromCache(tx + a2dx + ((ty + a2dy) << 3), a2Piece); //GlobalPieceManager.GetPieceTableEntry(a2Piece);
                if (Piece.IsPieceInvincible(this, a2Piece, tx + a2dx, ty + a2dy, oldPiece, fx, fy, Move.SpecialType.Advancer, pteO, pteA2))
                {
                    goto case Move.SpecialType.Withdrawer;
                }


                globalData.bitboard_updatedPieces |= (1uL << ((tx + a2dx) + ((ty + a2dy) << 3)));
                if (Piece.GetPieceAlignment(a2Piece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteA2.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(a2Piece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
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
                if (((((tx + advdx) | (ty + advdy)) & -8) != 0))
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
                PieceTableEntry pteA = globalData.GetPieceTableEntryFromCache(tx + advdx + ((ty + advdy) << 3), advPiece);// GlobalPieceManager.GetPieceTableEntry(advPiece);
                if (Piece.IsPieceInvincible(this, advPiece, tx + advdx, ty + advdy, oldPiece, fx, fy, Move.SpecialType.Advancer, pteO, pteA))
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                globalData.bitboard_updatedPieces |= (1uL << ((tx + advdx) + ((ty + advdy) << 3)));
                if (specialType == SpecialType.PoisonFlankingAdvancer)
                {
                    //toxic the advanced thing
                    if ((pteA.piecePropertyB & PiecePropertyB.StatusImmune) != 0 || Piece.GetPieceModifier(oldPiece) == PieceModifier.Immune)
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
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteA.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(advPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
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
                if (((((fx + withdx) | (fy + withdy)) & -8) != 0))
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
                PieceTableEntry pteW = globalData.GetPieceTableEntryFromCache(fx + withdx + ((fy + withdy) << 3), withPiece);// GlobalPieceManager.GetPieceTableEntry(withPiece);
                if (Piece.IsPieceInvincible(this, withPiece, tx + withdx, ty + withdy, oldPiece, fx, fy, Move.SpecialType.Withdrawer, pteO, pteW))
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                globalData.bitboard_updatedPieces |= (1uL << ((tx + withdx) + ((ty + withdy) << 3)));
                //Do withdraw capture
                if (Piece.GetPieceAlignment(withPiece) == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteW.pieceValueX2;
                }
                if (Piece.GetPieceAlignment(withPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
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

                    PieceTableEntry pteF = globalData.GetPieceTableEntryFromCache(flankX + (flankY << 3), flankPiece); //GlobalPieceManager.GetPieceTableEntry(flankPiece);
                    if (Piece.IsPieceInvincible(this, flankPiece, flankX, flankY, oldPiece, fx, fy, Move.SpecialType.Withdrawer, pteO, pteF))
                    {
                        goto case Move.SpecialType.MoveOnly;
                    }

                    globalData.bitboard_updatedPieces |= (1uL << index);

                    if (Piece.GetPieceAlignment(flankPiece) == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteF.pieceValueX2;
                    }
                    if (Piece.GetPieceAlignment(flankPiece) == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
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
                //ally swap vs non ally = capture
                if (opa != tpa && specialType == SpecialType.AllySwap)
                {
                    goto case Move.SpecialType.Normal;
                }

                if (boardUpdateMetadata != null)
                {
                    if (pieces[tx + (ty << 3)] != 0)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, fx, fy, targetPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                    }
                }
                //Very straightforward
                SetPieceAtCoordinate(tx, ty, oldPiece);
                SetPieceAtCoordinate(fx, fy, targetPiece);
                break;
            case Move.SpecialType.ImbueModifier:
                if (passiveMove)
                {
                    goto case Move.SpecialType.MoveOnly;
                }
                oldPiece = 0;
                SetPieceAtCoordinate(fx, fy, oldPiece);
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                switch (opt)
                {
                    case PieceType.HornSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Vengeful, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.TorchSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Phoenix, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.RingSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Radiant, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.FeatherSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Winged, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.GlassSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Spectral, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.BottleSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Immune, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.CapSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Warped, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                    case PieceType.ShieldSpirit:
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceModifier(PieceModifier.Shielded, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, targetPiece))));
                        break;
                }
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, GetPieceAtCoordinate(tx, ty), BoardUpdateMetadata.BoardUpdateType.ImbueModifier));
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
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, GetPieceAtCoordinate(tx, ty), BoardUpdateMetadata.BoardUpdateType.ImbueModifier));
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
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }

                if (pteT.type == PieceType.Rabbit && Piece.GetPieceSpecialData(targetPiece) != 0)
                {
                    //Change rabbit back to normal
                    pieces[(tx + (ty << 3))] = Piece.SetPieceType((PieceType)(Piece.GetPieceSpecialData(targetPiece)), Piece.SetPieceSpecialData(0, targetPiece));
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, targetPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }
                    break;
                }

                PieceTableEntry pteIP = GlobalPieceManager.GetPieceTableEntry(pteT.promotionType);
                if (tpa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceValueSumX2 += (short)(pteIP.pieceValueX2 - pteO.pieceValueX2);
                }
                if (tpa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 += (short)(pteIP.pieceValueX2 - pteO.pieceValueX2);
                }

                SetPieceAtCoordinate(tx, ty, Piece.SetPieceStatusDuration(0, Piece.SetPieceStatusEffect(PieceStatusEffect.None, Piece.SetPieceType(pteT.promotionType, targetPiece))));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, GetPieceAtCoordinate(tx, ty), BoardUpdateMetadata.BoardUpdateType.TypeChange));
                }
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

                //not a capture like so it just turns you into the target
                SetPieceAtCoordinate(fx, fy, Piece.SetPieceAlignment(opa, targetPiece));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, GetPieceAtCoordinate(fx, fy), BoardUpdateMetadata.BoardUpdateType.TypeChange));
                }
                break;
            case SpecialType.MorphRabbit:
                if (targetPiece == 0)
                {
                    goto case Move.SpecialType.MoveOnly;
                }

                //PieceTableEntry pteRabbit = GlobalPieceManager.GetPieceTableEntry(PieceType.Rabbit);

                /*
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteT.pieceValueX2 - pteRabbit.pieceValueX2);
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteT.pieceValueX2 - pteRabbit.pieceValueX2);
                }
                */

                //not a capture like so it just turns you into the target
                SetPieceAtCoordinate(tx, ty, Piece.SetPieceType(PieceType.Rabbit, Piece.SetPieceSpecialData((ushort)(Piece.GetPieceType(targetPiece)), targetPiece)));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, GetPieceAtCoordinate(tx, ty), BoardUpdateMetadata.BoardUpdateType.TypeChange));
                }
                break;
            case Move.SpecialType.Spawn:
                switch (opt)
                {
                    case PieceType.Gemini:
                        oldPiece = Piece.SetPieceType(PieceType.GeminiTwin, oldPiece);
                        PieceTableEntry pteGT = GlobalPieceManager.GetPieceTableEntry(PieceType.GeminiTwin);   //Todo: make this a global?

                        //Modify the piece values
                        //This is a small minus to the piece value so the AI only splits when it can make some play that gets more material
                        //(but having 2 pieces means you get a bigger piece table bonus also)
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteGT.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteGT.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                        break;
                    case PieceType.Triknight:
                        oldPiece = Piece.SetPieceType(PieceType.Knight, oldPiece);
                        PieceTableEntry pteKnight = GlobalPieceManager.GetPieceTableEntry(PieceType.Knight);   //Todo: make this a global?

                        //since you targetted an empty it is guaranteed you can spawn at least 2
                        int knightsMade = 2;

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                        //try to spawn on the opposite side too
                        int ox = (fx - tx) + fx;
                        int oy = (fy - ty) + fy;

                        bool dospawn = true;
                        if (((((ox) | (oy)) & -8) != 0))
                        {
                            dospawn = false;
                        }
                        if (dospawn && pieces[ox + (oy << 3)] != 0)
                        {
                            dospawn = false;
                        }
                        if (dospawn)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (ox + (oy << 3)));
                            knightsMade++;
                            SetPieceAtCoordinate(ox, oy, oldPiece);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (knightsMade == 3)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, ox, oy, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - knightsMade * pteKnight.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                            if (knightsMade == 3)
                            {
                                whitePerPlayerInfo.pieceCount++;
                            }
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - knightsMade * pteKnight.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                            if (knightsMade == 3)
                            {
                                blackPerPlayerInfo.pieceCount++;
                            }
                        }
                        break;
                    case PieceType.Tribishop:
                        //try to spawn on the opposite side too
                        oldPiece = Piece.SetPieceType(PieceType.Bishop, oldPiece);
                        PieceTableEntry pteBishop = GlobalPieceManager.GetPieceTableEntry(PieceType.Bishop);   //Todo: make this a global?

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
                            globalData.bitboard_updatedPieces |= (1uL << (oxB + (oyB << 3)));
                            SetPieceAtCoordinate(oxB, oyB, oldPiece);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (bishopsMade == 3)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oxB, oyB, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - bishopsMade * pteBishop.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                            if (bishopsMade == 3)
                            {
                                whitePerPlayerInfo.pieceCount++;
                            }
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - bishopsMade * pteBishop.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                            if (bishopsMade == 3)
                            {
                                blackPerPlayerInfo.pieceCount++;
                            }
                        }
                        break;
                    case PieceType.Birook:
                        oldPiece = Piece.SetPieceType(PieceType.Rook, oldPiece);
                        PieceTableEntry pteRook = GlobalPieceManager.GetPieceTableEntry(PieceType.Rook);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRook.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRook.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                    case PieceType.TrojanHorse:
                        //Spawn 3 pawns
                        PieceTableEntry ptePawn = GlobalPieceManager.GetPieceTableEntry(PieceType.Pawn);   //Todo: make this a global?
                        int pawnCount = 0;

                        uint newPawn = Piece.SetPieceType(PieceType.Pawn, oldPiece);
                        for (int i = -1; i <= 1; i++)
                        {
                            if (fx + i < 0 || fx + i > 7 || pieces[(fx + i) + (ty << 3)] != 0)
                            {
                                continue;
                            }
                            pawnCount++;
                            globalData.bitboard_updatedPieces |= (1uL << ((fx + i) + (ty << 3)));
                            SetPieceAtCoordinate(fx + i, ty, newPawn);

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, fx + i, ty, newPawn, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pawnCount * ptePawn.pieceValueX2 - pteO.pieceValueX2);
                            whitePerPlayerInfo.pieceCount += (byte)(pawnCount - 1);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pawnCount * ptePawn.pieceValueX2 - pteO.pieceValueX2);
                            blackPerPlayerInfo.pieceCount += (byte)(pawnCount - 1);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.Capture));
                        }
                        oldPiece = 0;
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        break;
                    case PieceType.QueenLeech:
                        //try to spawn on the opposite side too (x mirrored only)
                        uint newLeech = Piece.SetPieceType(PieceType.Leech, oldPiece);
                        newLeech = Piece.SetPieceSpecialData(0, newLeech);
                        PieceTableEntry pteLeech = GlobalPieceManager.GetPieceTableEntry(PieceType.Leech);   //Todo: make this a global?
                        SetPieceAtCoordinate(tx, ty, newLeech);
                        int leechCount = 1;

                        int leechX = (fx - tx) + fx;
                        if (leechX >= 0 && leechX <= 7 && pieces[leechX + (ty << 3)] == 0)
                        {
                            leechCount = 2;
                            globalData.bitboard_updatedPieces |= (1uL << (leechX + (ty << 3)));
                            SetPieceAtCoordinate(leechX, ty, newLeech);
                        }

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, newLeech, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            if (leechCount == 2)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, leechX, ty, newLeech, BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(leechCount * pteLeech.pieceValueX2);
                            whitePerPlayerInfo.pieceCount += (byte)(leechCount);
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(leechCount * pteLeech.pieceValueX2);
                            blackPerPlayerInfo.pieceCount += (byte)(leechCount);
                        }

                        //lose 1 charge
                        oldPiece = Piece.SetPieceSpecialData((byte)(Piece.GetPieceSpecialData(oldPiece) - 1), oldPiece);
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        break;
                    case PieceType.AmoebaCitadel:
                        //Splits off an Archbishop (3)
                        //Becomes Archbishop
                        oldPiece = Piece.SetPieceType(PieceType.AmoebaArchbishop, oldPiece);
                        PieceTableEntry pteCitadelSplit = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaArchbishop);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteCitadelSplit.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteCitadelSplit.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                    case PieceType.AmoebaGryphon:
                        //Splits off a Knight (2)
                        //Becomes Archbishop
                        oldPiece = Piece.SetPieceType(PieceType.AmoebaArchbishop, oldPiece);
                        PieceTableEntry pteGryphonSplitA = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaArchbishop);   //Todo: make this a global?
                        PieceTableEntry pteGryphonSplitB = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaKnight);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceType(PieceType.AmoebaKnight, oldPiece));

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, Piece.SetPieceType(PieceType.AmoebaKnight, oldPiece), BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteGryphonSplitA.pieceValueX2 - pteGryphonSplitB.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteGryphonSplitA.pieceValueX2 - pteGryphonSplitB.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                    case PieceType.AmoebaRaven:
                        //Splits off a Knight (2)
                        //Becomes Knight
                        oldPiece = Piece.SetPieceType(PieceType.AmoebaKnight, oldPiece);
                        PieceTableEntry pteRavenSplit = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaKnight);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRavenSplit.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteRavenSplit.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                    case PieceType.AmoebaArchbishop:
                        //Splits off a Pawn (1)
                        //Becomes Knight
                        oldPiece = Piece.SetPieceType(PieceType.AmoebaKnight, oldPiece);
                        PieceTableEntry pteArchbishopSplitA = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaKnight);   //Todo: make this a global?
                        PieceTableEntry pteArchbishopSplitB = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaPawn);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, Piece.SetPieceType(PieceType.AmoebaPawn, oldPiece));

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, Piece.SetPieceType(PieceType.AmoebaPawn, oldPiece), BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteArchbishopSplitA.pieceValueX2 - pteArchbishopSplitB.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - pteArchbishopSplitA.pieceValueX2 - pteArchbishopSplitB.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                    case PieceType.AmoebaKnight:
                        //Splits off a Pawn (1)
                        //Becomes Pawn
                        oldPiece = Piece.SetPieceType(PieceType.AmoebaPawn, oldPiece);
                        PieceTableEntry pteKnightSplit = GlobalPieceManager.GetPieceTableEntry(PieceType.AmoebaPawn);   //Todo: make this a global?

                        SetPieceAtCoordinate(fx, fy, oldPiece);
                        SetPieceAtCoordinate(tx, ty, oldPiece);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }

                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteKnightSplit.pieceValueX2);
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 - 2 * pteKnightSplit.pieceValueX2);
                            blackPerPlayerInfo.pieceCount++;
                        }
                        break;
                }
                break;
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
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
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, targetPiece, BoardUpdateMetadata.BoardUpdateType.StatusApply));
                }
                break;
            case SpecialType.TeleportMirror:
                int tmx = 7 - tx;
                int tmy = ty;
                SetPieceAtCoordinate(tx, ty, 0);
                globalData.bitboard_updatedPieces |= (1uL << (tmx + (ty << 3)));
                SetPieceAtCoordinate(tmx, ty, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tmx, ty, targetPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
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
                globalData.bitboard_updatedPieces |= (1uL << (trex + (trey << 3)));
                SetPieceAtCoordinate(trex, trey, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, trex, trey, targetPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
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
                globalData.bitboard_updatedPieces |= (1uL << (tox + (toy << 3)));
                SetPieceAtCoordinate(tox, toy, targetPiece);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, tox, toy, targetPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case SpecialType.CarryAlly:
                //Absorb ally into yourself
                SetPieceAtCoordinate(tx, ty, 0);
                if (tpa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    //No loss because it is not permanent
                }
                if (tpa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    //No loss because it is not permanent
                }
                oldPiece = Piece.SetPieceSpecialData((ushort)Piece.GetPieceType(targetPiece), oldPiece);
                SetPieceAtCoordinate(fx, fy, oldPiece);
                break;
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
                //Spawn an ally piece at the position
                //+1 piece count
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
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, Piece.SetPieceAlignment(opa, Piece.SetPieceType((Piece.PieceType)Piece.GetPieceSpecialData(oldPiece), 0)),  BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                SetPieceAtCoordinate(tx, ty, Piece.SetPieceAlignment(opa, Piece.SetPieceType((Piece.PieceType)Piece.GetPieceSpecialData(oldPiece), 0)));
                //remove it from inside the carry piece
                SetPieceAtCoordinate(fx, fy, Piece.SetPieceSpecialData(0, oldPiece));
                break;
            case SpecialType.AimAny:
            case SpecialType.AimEnemy:
            case SpecialType.AimOccupied:
                //Add 64 so that square 0 is targettable
                SetPieceAtCoordinate(fx, fy, Piece.SetPieceSpecialData((ushort)(64 + (tx + (ty << 3))), oldPiece));
                break;
            case SpecialType.AmoebaCombine:
                //Donate some power
                int donorPower = 0;
                int targetPower = 0;
                switch (opt)
                {
                    case PieceType.AmoebaCitadel:
                        donorPower = 6;
                        break;
                    case PieceType.AmoebaGryphon:
                        donorPower = 5;
                        break;
                    case PieceType.AmoebaRaven:
                        donorPower = 4;
                        break;
                    case PieceType.AmoebaArchbishop:
                        donorPower = 3;
                        break;
                    case PieceType.AmoebaKnight:
                        donorPower = 2;
                        break;
                    case PieceType.AmoebaPawn:
                        donorPower = 1;
                        break;
                }
                switch (Piece.GetPieceType(targetPiece))
                {
                    case PieceType.AmoebaCitadel:
                        targetPower = 6;
                        break;
                    case PieceType.AmoebaGryphon:
                        targetPower = 5;
                        break;
                    case PieceType.AmoebaRaven:
                        targetPower = 4;
                        break;
                    case PieceType.AmoebaArchbishop:
                        targetPower = 3;
                        break;
                    case PieceType.AmoebaKnight:
                        targetPower = 2;
                        break;
                    case PieceType.AmoebaPawn:
                        targetPower = 1;
                        break;
                }
                int sumPower = donorPower + targetPower;

                int newDonorPower = 0;
                int newTargetPower = sumPower;
                if (sumPower > 6)
                {
                    newTargetPower = 6;
                    newDonorPower = sumPower - 6;
                }

                Piece.PieceType newTargetType = PieceType.Null;
                Piece.PieceType newDonorType = PieceType.Null;
                switch (newDonorPower)
                {
                    case 6:
                        newDonorType = PieceType.AmoebaCitadel;
                        break;
                    case 5:
                        newDonorType = PieceType.AmoebaGryphon;
                        break;
                    case 4:
                        newDonorType = PieceType.AmoebaRaven;
                        break;
                    case 3:
                        newDonorType = PieceType.AmoebaArchbishop;
                        break;
                    case 2:
                        newDonorType = PieceType.AmoebaKnight;
                        break;
                    case 1:
                        newDonorType = PieceType.AmoebaPawn;
                        break;
                }
                switch (newTargetPower)
                {
                    case 6:
                        newTargetType = PieceType.AmoebaCitadel;
                        break;
                    case 5:
                        newTargetType = PieceType.AmoebaGryphon;
                        break;
                    case 4:
                        newTargetType = PieceType.AmoebaRaven;
                        break;
                    case 3:
                        newTargetType = PieceType.AmoebaArchbishop;
                        break;
                    case 2:
                        newTargetType = PieceType.AmoebaKnight;
                        break;
                    case 1:
                        newTargetType = PieceType.AmoebaPawn;
                        break;
                }

                oldPiece = Piece.SetPieceType(PieceType.AmoebaArchbishop, oldPiece);
                PieceTableEntry pteAC_O = GlobalPieceManager.GetPieceTableEntry(newDonorType);   //Todo: make this a global?
                PieceTableEntry pteAC_T = GlobalPieceManager.GetPieceTableEntry(newTargetType);   //Todo: make this a global?

                if (newDonorType == PieceType.Null)
                {
                    SetPieceAtCoordinate(fx, fy, 0);
                } else
                {
                    SetPieceAtCoordinate(fx, fy, Piece.SetPieceType(newDonorType, oldPiece));
                }
                SetPieceAtCoordinate(tx, ty, Piece.SetPieceType(newTargetType, targetPiece));

                if (boardUpdateMetadata != null)
                {
                    if (newDonorType == PieceType.Null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, Piece.SetPieceType(newDonorType, oldPiece), BoardUpdateMetadata.BoardUpdateType.Capture));
                    }
                    else
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, Piece.SetPieceType(newDonorType, oldPiece), BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, Piece.SetPieceType(newTargetType, targetPiece), BoardUpdateMetadata.BoardUpdateType.Spawn));
                }

                if (opa == PieceAlignment.White)
                {
                    if (newDonorType == PieceType.Null)
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 + pteT.pieceValueX2 - pteAC_T.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                    } else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 + pteT.pieceValueX2 - pteAC_O.pieceValueX2 - pteAC_T.pieceValueX2);
                    }
                }
                if (opa == PieceAlignment.Black)
                {
                    if (newDonorType == PieceType.Null)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 + pteT.pieceValueX2 - pteAC_T.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                    }
                    else
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (short)(pteO.pieceValueX2 + pteT.pieceValueX2 - pteAC_O.pieceValueX2 - pteAC_T.pieceValueX2);
                    }
                }
                break;
            case Move.SpecialType.Castling:
                //We have to move the piece 3 away to the middle spot
                int dx = (tx - fx) >> 1;

                int hopX = (tx - dx);
                int allyX = (tx + dx);

                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(allyX, ty, hopX, ty, GetPieceAtCoordinate(allyX, ty), BoardUpdateMetadata.BoardUpdateType.Shift));
                }

                //move king
                //SetPieceAtCoordinate(fx, fy, 0);
                SetPieceAtCoordinate(fx, fy, 0);
                //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa);
                SetPieceAtCoordinate(tx, ty, oldPiece);

                //move other thing
                globalData.bitboard_updatedPieces |= (1uL << (hopX + (ty << 3)));
                SetPieceAtCoordinate(hopX, ty, GetPieceAtCoordinate(allyX, ty));
                globalData.bitboard_updatedPieces |= (1uL << (allyX + (ty << 3)));
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

        if (!passiveMove && Move.SpecialMoveCaptureLike(specialType) && enemyMove)
        {
            if (Piece.GetPieceModifier(oldPiece) == PieceModifier.Radiant)
            {
                SpawnGoldenPawn(px, py, opa, boardUpdateMetadata);
            } else if (opa == PieceAlignment.White && tpa == PieceAlignment.Black && ((globalData.playerModifier & PlayerModifier.FirstRadiant) != 0) && blackPremovePiecesLost == 0)
            {
                SpawnGoldenPawn(px, py, opa, boardUpdateMetadata);

                if (oldPiece != 0 && ((pteO.piecePropertyB & PiecePropertyB.Giant) == 0))
                {
                    oldPiece = Piece.SetPieceModifier(PieceModifier.Radiant, oldPiece);
                    //Make it golden
                    if (lastMoveStationary)
                    {
                        SetPieceAtCoordinate(fx, fy, oldPiece);
                    } else
                    {
                        SetPieceAtCoordinate(tx, ty, oldPiece);
                    }
                }
            }
        }

        //I can shift these into the below switch case to optimize things slightly
        //but it is less data driven

        if ((pteO.pieceProperty & PieceProperty.PassiveShift) != 0 || (opa == PieceAlignment.White && (globalData.playerModifier & PlayerModifier.PassiveShiftBadges) != 0))
        {
            if ((pteO.pieceProperty & PieceProperty.PassivePush) != 0 || (opa == PieceAlignment.White && (globalData.playerModifier & PlayerModifier.Push) != 0))
            {
                DoPassivePush(px, py, opa, boardUpdateMetadata);
            }
            if ((pteO.pieceProperty & PieceProperty.PassivePushStrong) != 0)
            {
                DoPassivePushStrong(px, py, opa, boardUpdateMetadata);
            }
            if ((pteO.pieceProperty & PieceProperty.PassivePull) != 0 || (opa == PieceAlignment.White && (globalData.playerModifier & PlayerModifier.Vortex) != 0))
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
        }

        if ((pteO.piecePropertyB & PiecePropertyB.SpreadCure) != 0)
        {
            DoSpreadCure(px, py, opa, boardUpdateMetadata);
        }

        if ((pteO.piecePropertyB & PiecePropertyB.AllTandemMovers) != 0)
        {
            if ((pteO.piecePropertyB & PiecePropertyB.TandemMover) != 0)
            {
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
            if ((pteO.piecePropertyB & PiecePropertyB.TandemMoverDiag) != 0)
            {
                switch (dir)
                {
                    case Dir.DownLeft:
                        //order by down and left first
                        //TryPiecePushAlly(fx - 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.DownRight:
                        //TryPiecePushAlly(fx + 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.Null:
                        break;
                    case Dir.UpLeft:
                        //TryPiecePushAlly(fx - 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        break;
                    case Dir.UpRight:
                        //TryPiecePushAlly(fx + 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        break;
                }
            }
            if ((pteO.piecePropertyB & PiecePropertyB.TandemMoverOrtho) != 0)
            {
                switch (dir)
                {
                    case Move.Dir.Down:
                        //TryPiecePushAlly(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Left:
                        //TryPiecePushAlly(fx - 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, -1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Right:
                        //TryPiecePushAlly(fx + 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Up:
                        //TryPiecePushAlly(fx, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx - 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx + 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushAlly(fx, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        break;
                }
            }
            if ((pteO.piecePropertyB & PiecePropertyB.EnemyTandemMover) != 0)
            {
                switch (dir)
                {
                    case Move.Dir.Down:
                        TryPiecePushEnemy(fx - 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        //TryPiecePushEnemy(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Left:
                        TryPiecePushEnemy(fx - 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        //TryPiecePushEnemy(fx - 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Right:
                        TryPiecePushEnemy(fx + 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        //TryPiecePushEnemy(fx + 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Up:
                        TryPiecePushEnemy(fx - 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        //TryPiecePushEnemy(fx, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        break;
                    case Dir.DownLeft:
                        //order by down and left first
                        //TryPiecePushEnemy(fx - 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, -1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.DownRight:
                        //TryPiecePushEnemy(fx + 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy + 1, 1, -1, opa, boardUpdateMetadata);
                        break;
                    case Dir.Null:
                        break;
                    case Dir.UpLeft:
                        //TryPiecePushEnemy(fx - 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy + 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, -1, 1, opa, boardUpdateMetadata);
                        break;
                    case Dir.UpRight:
                        //TryPiecePushEnemy(fx + 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy + 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy - 1, 1, 1, opa, boardUpdateMetadata);
                        break;
                }
            }
            if ((pteO.piecePropertyB & PiecePropertyB.EnemyTandemMoverOrtho) != 0)
            {
                switch (dir)
                {
                    case Move.Dir.Down:
                        //TryPiecePushEnemy(fx, fy - 1, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 0, -1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 0, -1, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Left:
                        //TryPiecePushEnemy(fx - 1, fy, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, -1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, -1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Right:
                        //TryPiecePushEnemy(fx + 1, fy, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy + 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 1, 0, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 1, 0, opa, boardUpdateMetadata);
                        break;
                    case Move.Dir.Up:
                        //TryPiecePushEnemy(fx, fy + 1, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx - 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx + 1, fy, 0, 1, opa, boardUpdateMetadata);
                        TryPiecePushEnemy(fx, fy - 1, 0, 1, opa, boardUpdateMetadata);
                        break;
                }
            }
            if ((pteO.piecePropertyB & PiecePropertyB.AnyTandemMover) != 0)
            {
                switch (dir)
                {
                    case Move.Dir.Down:
                        TryPiecePush(fx - 1, fy - 1, 0, -1, boardUpdateMetadata);
                        //TryPiecePush(fx, fy - 1, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy + 1, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, 0, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, 0, -1, boardUpdateMetadata);
                        break;
                    case Move.Dir.Left:
                        TryPiecePush(fx - 1, fy + 1, -1, 0, boardUpdateMetadata);
                        //TryPiecePush(fx - 1, fy, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, -1, 0, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, -1, 0, boardUpdateMetadata);
                        break;
                    case Move.Dir.Right:
                        TryPiecePush(fx + 1, fy + 1, 1, 0, boardUpdateMetadata);
                        //TryPiecePush(fx + 1, fy, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy + 1, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, 1, 0, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, 1, 0, boardUpdateMetadata);
                        break;
                    case Move.Dir.Up:
                        TryPiecePush(fx - 1, fy + 1, 0, 1, boardUpdateMetadata);
                        //TryPiecePush(fx, fy + 1, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, 0, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, 0, 1, boardUpdateMetadata);
                        break;
                    case Dir.DownLeft:
                        //order by down and left first
                        //TryPiecePush(fx - 1, fy - 1, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy + 1, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, -1, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, -1, -1, boardUpdateMetadata);
                        break;
                    case Dir.DownRight:
                        //TryPiecePush(fx + 1, fy - 1, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, 1, -1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy + 1, 1, -1, boardUpdateMetadata);
                        break;
                    case Dir.Null:
                        break;
                    case Dir.UpLeft:
                        //TryPiecePush(fx - 1, fy + 1, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy + 1, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, -1, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, -1, 1, boardUpdateMetadata);
                        break;
                    case Dir.UpRight:
                        //TryPiecePush(fx + 1, fy + 1, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx, fy + 1, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy + 1, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx + 1, fy - 1, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx, fy - 1, 1, 1, boardUpdateMetadata);
                        TryPiecePush(fx - 1, fy - 1, 1, 1, boardUpdateMetadata);
                        break;
                }
            }
        }

        //special extra stuff
        switch (opt)
        {
            case PieceType.King:
                if (blackToMove && ((globalData.enemyModifier & EnemyModifier.Obelisk) != 0))
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
                globalData.bitboard_updatedEmpty &= ~(1uL << (fx + (fy << 3)));
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
                        //Neutral so it doesn't inflate your piece count
                        globalData.bitboard_updatedPieces |= (1uL << (tempX + (tempY << 3)));
                        pieces[tempX + (tempY << 3)] = Piece.SetPieceType(PieceType.SludgeTrail, Piece.SetPieceAlignment(Piece.PieceAlignment.Neutral, oldPiece));

                        /*
                        if (opa == PieceAlignment.White)
                        {
                            whitePerPlayerInfo.pieceCount++;
                        }
                        if (opa == PieceAlignment.Black)
                        {
                            blackPerPlayerInfo.pieceCount++;
                        }
                        */

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(tempX, tempY, pieces[tempX + (tempY << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                        }
                    }

                    tempX += sdx;
                    tempY += sdy;
                    //it is a bug if this happens
                    if (((((tempX) | (tempY)) & -8) != 0))
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
            case PieceType.LavaGolem:
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
                        //only in + shape (otherwise it gets pretty ridiculous)
                        if (i != 0 && j != 0)
                        {
                            continue;
                        }

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
                        if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.FireCapture, pteO, pteE))
                        {
                            if (epa == PieceAlignment.White)
                            {
                                whitePerPlayerInfo.pieceCount--;
                                whitePerPlayerInfo.piecesLost++;
                                whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                            }
                            if (epa == PieceAlignment.Black)
                            {
                                blackPerPlayerInfo.pieceCount--;
                                blackPerPlayerInfo.piecesLost++;
                                blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                            }

                            DeletePieceAtCoordinate(tx + i, ty + j, pteE, opa, boardUpdateMetadata);
                            //SetPieceAtCoordinate(tx + i, ty + j, 0);
                        }
                    }
                }
                break;
            case PieceType.IceGolem:
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
                        //only in + shape (otherwise it gets pretty ridiculous)
                        if (i != 0 && j != 0)
                        {
                            continue;
                        }

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
                        if (epa != opa && (i != 0 || j != 0) && pteE != null && !Piece.IsPieceInvincible(this, pieces[tx + i + ((ty + j) << 3)], tx + i, ty + j, oldPiece, fx, fy, Move.SpecialType.InflictFreeze, pteO, pteE))
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (tx + i + ((ty + j) << 3)));
                            pieces[tx + i + ((ty + j) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, pieces[tx + i + ((ty + j) << 3)]));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, pieces[tx + i + ((ty + j) << 3)], BoardUpdateMetadata.BoardUpdateType.StatusApply));
                            }
                        }
                    }
                }
                break;
            case PieceType.RabbitDiplomat:
                //Convert rabbits in range >:)
                //But only normal rabbits
                if (!lastMoveStationary)
                {
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

                            uint rtarget = GetPieceAtCoordinate(tx + i, ty + j);

                            PieceTableEntry pteE = GlobalPieceManager.GetPieceTableEntry(rtarget);

                            //delete the enemies on the delta
                            Piece.PieceAlignment epa = Piece.GetPieceAlignment(rtarget);
                            if (pteE != null && pteE.type == PieceType.Rabbit && epa != opa)
                            {
                                //convert

                                //Convert target to my side
                                if (epa == PieceAlignment.White)
                                {
                                    whitePerPlayerInfo.pieceCount--;
                                    whitePerPlayerInfo.piecesLost++;
                                    whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                }
                                if (epa == PieceAlignment.Black)
                                {
                                    blackPerPlayerInfo.pieceCount--;
                                    blackPerPlayerInfo.piecesLost++;
                                    blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                }
                                if (opa == PieceAlignment.White)
                                {
                                    //I get a piece
                                    whitePerPlayerInfo.pieceCount++;
                                    whitePerPlayerInfo.pieceValueSumX2 += pteE.pieceValueX2;
                                }
                                if (opa == PieceAlignment.Black)
                                {
                                    //I get a piece
                                    blackPerPlayerInfo.pieceCount++;
                                    blackPerPlayerInfo.pieceValueSumX2 += pteE.pieceValueX2;
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
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(tx + i, ty + j, Piece.SetPieceAlignment(opa, rtarget), BoardUpdateMetadata.BoardUpdateType.AlignmentChange));
                                }
                                DeletePieceAtCoordinate(tx + i, ty + j, pteE, epa, boardUpdateMetadata);
                                SetPieceAtCoordinate(tx + i, ty + j, Piece.SetPieceAlignment(opa, rtarget));
                            }
                        }
                    }
                }
                break;
        }



        bool bonusMove = false;


        //you only get 1 bonus ply from bonus movers
        if (bonusPly < 1)
        {
            if ((pteO.pieceProperty & PieceProperty.BonusMove) != 0)
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

            if (blackToMove)
            {
                if (turn < 5 && (globalData.enemyModifier & EnemyModifier.Youthful) != 0)
                {
                    bonusMove = true;
                }
            }
            else
            {
                if (turn < 3 && (globalData.playerModifier & PlayerModifier.Sprint) != 0)
                {
                    bonusMove = true;
                }
                if ((turn > 0 && ((turn & 7) == 0)) && (globalData.playerModifier & PlayerModifier.TimeBurst) != 0)
                {
                    bonusMove = true;
                }
            }
        }

        if (applyTurnEnd)
        {
            RunTurnEnd(blackToMove, bonusMove, boardUpdateMetadata);
        }
        else
        {
            ApplyZenithEffect(boardUpdateMetadata);
        }
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

        int fxy = (fx + (fy << 3));
        ulong bitindex = 1uL << fxy;
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
                    PieceTableEntry pteT = GlobalPieceManager.GetPieceTableEntry(tpt);
                    if (tpa == PieceAlignment.White)
                    {
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                        whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
                    }
                    if (tpa == PieceAlignment.Black)
                    {
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
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
                    if (pieceChange < 2 && ((pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & PieceProperty.DestroyOnCapture) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((bitindex & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful || (tpa == PieceAlignment.White && ((globalData.playerModifier & PlayerModifier.FinalVengeance) != 0) && whitePerPlayerInfo.pieceCount <= 6)))
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

        DeleteGiant(pieces[fxy], fx, fy);
        //DeleteGiantPieceMovedFromCoordinate(fx, fy, 0);

        if (boardUpdateMetadata != null)
        {
            //update for the giant
            if (pieceChange == 2)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Capture, true));
            }

            if (pieceChange == 1)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.TypeChange, true));
            }
        }

        if (oldPiece == 0)
        {
            if (opa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceCount--;
                whitePerPlayerInfo.piecesLost++;
                whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
            }
            if (opa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceCount--;
                blackPerPlayerInfo.piecesLost++;
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
            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
            pieces[tx + (ty << 3)] = oldPiece;
        }
        else
        {
            //PlaceMovedPiece(oldPiece, tx, ty, pteO, opa);
            PlaceGiant(oldPiece, tx, ty);
            //pieces[tx + (ty << 3)] = oldPiece;
        }
    }
    public void ApplyConsumableMove(uint move, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        (Move.ConsumableMoveType cmt, int x, int y) = Move.DecodeConsumableMove(move);

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[x + (y << 3)]);
        uint targetPiece = pieces[x + (y << 3)];
        globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));
        switch (cmt)
        {
            case ConsumableMoveType.None:
                break;
            case ConsumableMoveType.PocketRock:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Rock, Piece.SetPieceAlignment(PieceAlignment.Neutral, 0));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.PocketRockslide:
                for (int i = -1; i <= 1; i++)
                {
                    if (x + i < 0 || x + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (y + j < 0 || y + j > 7)
                        {
                            continue;
                        }

                        uint piece = pieces[(x + i) + ((y + j) << 3)];
                        globalData.bitboard_updatedPieces |= (1uL << ((x + i) + ((y + j) << 3)));
                        if (piece == 0)
                        {
                            pieces[x + i + ((y + j) << 3)] = Piece.SetPieceType(PieceType.Rock, Piece.SetPieceAlignment(PieceAlignment.Neutral, 0));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(x + i, y + j, pieces[x + i + ((y + j) << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                            }
                        }
                    }
                }
                break;
            case ConsumableMoveType.PocketPawn:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Pawn, Piece.SetPieceAlignment(PieceAlignment.White, 0));
                whitePerPlayerInfo.pieceCount++;
                pte = GlobalPieceManager.GetPieceTableEntry(PieceType.Pawn);
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.PocketKnight:
                pieces[x + (y << 3)] = Piece.SetPieceType(PieceType.Knight, Piece.SetPieceAlignment(PieceAlignment.White, 0));
                whitePerPlayerInfo.pieceCount++;
                pte = GlobalPieceManager.GetPieceTableEntry(PieceType.Knight);
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.Horns:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Vengeful, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Torch:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Phoenix, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Ring:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Radiant, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Wings:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Winged, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Glass:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Spectral, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Bottle:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Immune, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Cap:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Warped, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.Shield:
                pieces[x + (y << 3)] = Piece.SetPieceModifier(PieceModifier.Shielded, pieces[x + (y << 3)]);
                break;
            case ConsumableMoveType.WarpBack:
                int tx = x;
                int ty = 0;
                int dy = 1;
                int dx = 0;

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
                        break;
                    }
                }

                globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));
                pieces[tx + ty * 8] = GetPieceAtCoordinate(x, y);
                pieces[x + y * 8] = 0;

                if (boardUpdateMetadata != null)
                {
                    //shift
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pieces[tx + ty * 8], BoardUpdateMetadata.BoardUpdateType.Shift));
                }
                break;
            case ConsumableMoveType.SplashFreeze:
                for (int i = -1; i <= 1; i++)
                {
                    if (x + i < 0 || x + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (y + j < 0 || y + j > 7)
                        {
                            continue;
                        }

                        uint piece = pieces[(x + i) + ((y + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << ((x + i) + ((y + j) << 3)));
                            pieces[(x + i) + ((y + j) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, Piece.SetPieceStatusDuration(3, pieces[(x + i) + ((y + j) << 3)]));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(x + i, y + j, pieces[(x + i) + ((y + j) << 3)], BoardUpdateMetadata.BoardUpdateType.StatusApply));
                            }
                        }
                    }
                }
                break;
            case ConsumableMoveType.SplashPhantom:
                for (int i = -1; i <= 1; i++)
                {
                    if (x + i < 0 || x + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (y + j < 0 || y + j > 7)
                        {
                            continue;
                        }

                        uint piece = pieces[(x + i) + ((y + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << ((x + i) + ((y + j) << 3)));
                            pieces[(x + i) + ((y + j) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.Ghostly, Piece.SetPieceStatusDuration(3, pieces[(x + i) + ((y + j) << 3)]));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(x + i, y + j, pieces[(x + i) + ((y + j) << 3)], BoardUpdateMetadata.BoardUpdateType.StatusApply));
                            }
                        }
                    }
                }
                break;
            case ConsumableMoveType.SplashAir:
                DoPassivePush(x, y, Piece.PieceAlignment.White, boardUpdateMetadata);
                break;
            case ConsumableMoveType.SplashVortex:
                for (int i = 0; i < 8; i++)
                {
                    int vdx = GlobalPieceManager.orbiterDeltas[i][0];
                    int vdy = GlobalPieceManager.orbiterDeltas[i][1];

                    if (((((x + vdx) | (y + vdy)) & -8) != 0))
                    {
                        continue;
                    }

                    if (Piece.GetPieceAlignment(pieces[x + vdx + (y + vdy) * 8]) != Piece.PieceAlignment.White)
                    {
                        TryPiecePull(x + vdx, y + vdy, vdx, vdy, GlobalPieceManager.GetPieceTableEntry(pieces[x + vdx + (y + vdy) * 8]), boardUpdateMetadata);
                    }

                    if (((((x + vdx * 2) | (y + vdy * 2)) & -8) != 0))
                    {
                        continue;
                    }

                    if (Piece.GetPieceAlignment(pieces[x + vdx * 2 + (y + vdy * 2) * 8]) == Piece.PieceAlignment.White)
                    {
                        continue;
                    }

                    TryPiecePull(x + vdx * 2, y + vdy * 2, vdx, vdy, GlobalPieceManager.GetPieceTableEntry(pieces[x + vdx * 2 + (y + vdy * 2) * 8]), boardUpdateMetadata);
                }
                break;
            case ConsumableMoveType.Fan:
                for (int fdy = 1; fdy >= -1; fdy--)
                {
                    for (int fdx = -1; fdx <= 1; fdx++)
                    {
                        if (((((x + fdx) | (y + fdy)) & -8) != 0))
                        {
                            continue;
                        }
                        if (Piece.GetPieceAlignment(pieces[x + fdx + (y + fdy) * 8]) == Piece.PieceAlignment.White)
                        {
                            continue;
                        }
                        TryPiecePush(x + fdx, y + fdy, 0, 1, boardUpdateMetadata);
                    }
                }
                break;
            case ConsumableMoveType.MegaFan:
                for (int i = 7; i >= 0; i--)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (Piece.GetPieceAlignment(pieces[j + (i) * 8]) == Piece.PieceAlignment.White)
                        {
                            continue;
                        }
                        TryPiecePush(j, i, 0, 1, boardUpdateMetadata);
                    }
                }
                break;
            case ConsumableMoveType.Grail:
                pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(pieces[x + (y << 3)]));
                pieces[x + (y << 3)] = Piece.SetPieceType(pte.promotionType, pieces[x + (y << 3)]);
                whitePerPlayerInfo.pieceValueSumX2 += (short)(GlobalPieceManager.GetPieceTableEntry(pte.promotionType).pieceValueX2 - pte.pieceValueX2);
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Spawn));
                }
                break;
            case ConsumableMoveType.SplashCure:
                DoSpreadCure(x, y, PieceAlignment.White, boardUpdateMetadata);
                break;
            case ConsumableMoveType.Bag:
                //Convert target to my side
                if (Piece.GetPieceAlignment(targetPiece) == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
                    blackPerPlayerInfo.pieceValueSumX2 -= pte.pieceValueX2;
                }
                //I get a piece
                whitePerPlayerInfo.pieceCount++;
                whitePerPlayerInfo.pieceValueSumX2 += pte.pieceValueX2;

                whitePerPlayerInfo.capturedLastTurn = true;

                if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
                {
                    ushort pieceValue = Piece.GetPieceSpecialData(targetPiece);

                    int gdx = pieceValue & 1;
                    int gdy = (pieceValue & 2) >> 1;

                    //make this a +1 or -1
                    gdx *= 2;
                    gdx = 1 - gdx;
                    gdy *= 2;
                    gdy = 1 - gdy;

                    SetPieceAtCoordinate(x, y, Piece.SetPieceAlignment(Piece.PieceAlignment.White, targetPiece));
                    SetPieceAtCoordinate(x + gdx, y, Piece.SetPieceAlignment(Piece.PieceAlignment.White, GetPieceAtCoordinate(x + gdx, y)));
                    SetPieceAtCoordinate(x, y + gdy, Piece.SetPieceAlignment(Piece.PieceAlignment.White, GetPieceAtCoordinate(x, y + gdy)));
                    SetPieceAtCoordinate(x + gdx, y + gdy, Piece.SetPieceAlignment(Piece.PieceAlignment.White, GetPieceAtCoordinate(x + gdx, y + gdy)));
                }
                else
                {
                    DeletePieceAtCoordinate(x, y, pte, Piece.PieceAlignment.Black, boardUpdateMetadata);
                    SetPieceAtCoordinate(x, y, Piece.SetPieceAlignment(Piece.PieceAlignment.White, targetPiece));
                }
                break;
        }
    }

    public void DeletePieceAtCoordinate(int x, int y, PieceTableEntry pte, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(GetPieceAtCoordinate(x, y)));
        if (pte == null)
        {
            return;
        }

        globalData.bitboard_updatedPieces |= (1uL << (x + ((y) << 3)));

        uint piece = GetPieceAtCoordinate(x, y);

        if (boardUpdateMetadata != null)
        {
            //ArcanaMoon makes a lot more updates
            if (pte.type != PieceType.ArcanaMoon && pte.type != PieceType.MoonIllusion)
            {
                PieceTableEntry realPTE = GlobalPieceManager.GetPieceTableEntry(piece);
                //check to not add a double update for the piece carry special case below
                if (realPTE == pte)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, piece, BoardUpdateMetadata.BoardUpdateType.Capture));
                }
            }
        }

        if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
        {
            DeleteGiant(piece, x, y);
            return;
        }

        if ((pte.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(pieces[x + (y << 3)]) != 0)
        {
            PieceTableEntry newPte = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)Piece.GetPieceSpecialData(pieces[x + (y << 3)]));

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

        if (pte.type == PieceType.Rabbit && Piece.GetPieceSpecialData(pieces[x + (y << 3)]) != 0)
        {
            //rabbit value is already deducted
            PieceTableEntry newPte = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)Piece.GetPieceSpecialData(pieces[x + (y << 3)]));
            PieceTableEntry newPteR = GlobalPieceManager.GetPieceTableEntry(Piece.PieceType.Rabbit);

            if (pa == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= (short)(newPte.pieceValueX2 - newPteR.pieceValueX2);
            }
            if (pa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= (short)(newPte.pieceValueX2 - newPteR.pieceValueX2);
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
        if (!specialPiece)
        {
            if (Piece.GetPieceModifier(piece) == PieceModifier.Phoenix)
            {
                TryReplacePhoenix(x, y, piece, boardUpdateMetadata);
            } else if ((pa == PieceAlignment.White && whitePerPlayerInfo.piecesLost == 1 && ((globalData.playerModifier & PlayerModifier.PhoenixWing) != 0)))
            {
                //this acts after the lost count is incremented to 1
                TryReplace(x, y, piece, boardUpdateMetadata);
            }
        }

        //SetPieceAtCoordinate(x, y, 0);
        pieces[(x + (y << 3))] = 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CapturePieceAtCoordinate(int x, int y, uint newPiece, PieceTableEntry pte, Piece.PieceAlignment pa, PieceTableEntry pteT, Piece.PieceAlignment paT, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        DeletePieceAtCoordinate(x, y, pteT, paT, boardUpdateMetadata);
        pieces[x + (y << 3)] = newPiece;
        //PlaceMovedPiece(newPiece, x, y, pte, pa);
    }
    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteGiantPieceMovedFromCoordinate(int x, int y, uint residuePiece = 0)
    {
        DeleteGiant(GetPieceAtCoordinate(x, y), x, y);
        return;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeletePieceMovedFromCoordinate(int x, int y, uint residuePiece = 0)
    {
        pieces[(x + (y << 3))] = residuePiece;
    }
    */
    public void DeleteArcanaMoon(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (globalData.arcanaMoonOutdated)
        {
            MoveGenerator.FixArcanaMoon(this);
        }
        //Note: the capture check reduces point score by one of them
        //This is why I made Moon Illusion the same value as Arcana Moon (it doesn't increase when it spawns stuff)
        ulong bitboard = 0;
        if (black)
        {
            bitboard = globalData.bitboard_tarotMoonIllusion & globalData.bitboard_piecesBlack;
        } else
        {
            bitboard = globalData.bitboard_tarotMoonIllusion & globalData.bitboard_piecesWhite;
        }
        //MainManager.PrintBitboard(bitboard);

        while (bitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard, out bitboard);

            if (black)
            {
                blackPerPlayerInfo.pieceCount--;
                blackPerPlayerInfo.piecesLost++;
            }
            else
            {
                whitePerPlayerInfo.pieceCount--;
                whitePerPlayerInfo.piecesLost++;
            }
            globalData.bitboard_updatedPieces |= (1uL << index);

            if (boardUpdateMetadata != null)
            {
                //Double check is here
                if (pieces[index] != 0)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, pieces[index], BoardUpdateMetadata.BoardUpdateType.Capture));
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
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, pieces[tx + ty * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(PieceType.Pawn);
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
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, piece, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece));
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
    public void TryReplace(int x, int y, uint piece, List<BoardUpdateMetadata> boardUpdateMetadata)
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
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, piece, BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(piece));
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
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        if (boardUpdateMetadata != null)
        {
            //Spawn a new lich
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pieces[tx + ty * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(PieceType.Lich);
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
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        if (boardUpdateMetadata != null)
        {
            //Spawn a new lich
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pieces[tx + ty * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }

        PieceTableEntry pteR = GlobalPieceManager.GetPieceTableEntry(PieceType.Revenant);
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

        if (((((x + dx) | (y + dy)) & -8) != 0))
        {
            return;
        }
        if (pieces[(x + dx) + (y + dy) * 8] != 0)
        {
            return;
        }

        PieceTableEntry pteS = GlobalPieceManager.GetPieceTableEntry(PieceType.Slime);

        //Spawn a thing
        pieces[(x + dx) + (y + dy) * 8] = Piece.SetPieceType(PieceType.Slime, GetPieceAtCoordinate(x, y));
        globalData.bitboard_updatedPieces |= (1uL << (x + dx + ((y + dy) << 3)));
        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx, y + dy, pieces[(x + dx) + (y + dy) * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }
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

        if ((((x + dx * 2) | (y + dy * 2)) & -8) != 0)
        {
            return;
        }
        if (pieces[(x + dx * 2) + (y + dy * 2) * 8] != 0)
        {
            return;
        }

        //Spawn a thing
        pieces[(x + dx * 2) + (y + dy * 2) * 8] = Piece.SetPieceType(PieceType.Slime, GetPieceAtCoordinate(x, y));
        globalData.bitboard_updatedPieces |= (1uL << (x + dx + dx + ((y + dy + dy) << 3)));
        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx * 2, y + dy * 2, pieces[(x + dx * 2) + (y + dy * 2) * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }
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

        if (((((x + dx) | (y + dy)) & -8) != 0))
        {
            return;
        }
        if (pieces[(x + dx) + (y + dy) * 8] != 0)
        {
            return;
        }

        PieceTableEntry pteS = GlobalPieceManager.GetPieceTableEntry(newPiece);

        //Spawn a thing
        pieces[(x + dx) + (y + dy) * 8] = Piece.SetPieceType(newPiece, GetPieceAtCoordinate(x, y));
        globalData.bitboard_updatedPieces |= (1uL << (x + dx + ((y + dy) << 3)));
        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x + dx, y + dy, pieces[(x + dx) + (y + dy) * 8], BoardUpdateMetadata.BoardUpdateType.Spawn));
        }
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

    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PlaceMovedPiece(uint piece, int x, int y, PieceTableEntry pte, Piece.PieceAlignment pa)
    {
        if (pte == null)
        {
            return;
        }

        if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
        {
            PlaceGiant(piece, x, y);
            return;
        }

        pieces[x + (y << 3)] = piece;
    }
    */
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
        //Since the above forces value = 0 this is useless code
        //dx *= 2;
        //dx = 1 - dx;
        //dy *= 2;
        //dy = 1 - dy;
        dx = 1;
        dy = 1;

        int xy = (x + (y << 3));
        globalData.bitboard_updatedPieces |= (1uL << xy);
        globalData.bitboard_updatedPieces |= (1uL << (xy + 1));
        globalData.bitboard_updatedPieces |= (1uL << (xy + 8));
        globalData.bitboard_updatedPieces |= (1uL << (xy + 9));

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

        if (pieceValue != 0)
        {
            //force you to use 0
            PlaceGiant(Piece.SetPieceSpecialData(0, piece), x - dx, y - dy);
            return;
        }

        //make this a +1 or -1
        //Since the above forces value = 0 this is useless code
        //dx *= 2;
        //dx = 1 - dx;
        //dy *= 2;
        //dy = 1 - dy;
        dx = 1;
        dy = 1;

        int xy = (x + (y << 3));
        globalData.bitboard_updatedPieces |= (1uL << xy);
        globalData.bitboard_updatedPieces |= (1uL << (xy + 1));
        globalData.bitboard_updatedPieces |= (1uL << (xy + 8));
        globalData.bitboard_updatedPieces |= (1uL << (xy + 9));

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
        MoveGenerator.GeneratePieceBitboardsPostTurn(this);

        ApplyAutoMovers(!black, boardUpdateMetadata);
        ApplyPromotion(black, boardUpdateMetadata);

        if (!bonusMove)
        {
            TickDownStatusEffects(black, boardUpdateMetadata);
        }

        ApplySquareEffectsTurnEnd(black, boardUpdateMetadata);
        ApplyZenithEffect(boardUpdateMetadata);
        
        if (boardUpdateMetadata != null)
        {
            ApplyGoalSquares(boardUpdateMetadata);
        }
    }

    //this should be idempotent and also not have immediate effect
    //Currently set to be after move gen
    public void RunTurnStart(bool black)
    {
        //you
        Piece.PieceAlignment pa = black ? PieceAlignment.Black : PieceAlignment.White;

        ulong enemyBitboard = 0;
        if (pa == PieceAlignment.Black)
        {
            enemyBitboard = globalData.bitboard_piecesWhite & globalData.bitboard_shielded;
        } else
        {
            enemyBitboard = globalData.bitboard_piecesBlack & globalData.bitboard_shielded;
        }

        if (enemyBitboard == 0)
        {
            return;
        }

        MoveBitTable antiTable = globalData.mbtactiveInverse;
        if (globalData.mbtactiveInverse == null)
        {
            globalData.mbtactiveInverse = new MoveBitTable();
            antiTable = globalData.mbtactiveInverse;
        }
        antiTable.MakeInverse(globalData.mbtactive);

        while (enemyBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(enemyBitboard, out enemyBitboard);

            int subX = index & 7;
            int subY = index >> 3;

            uint piece = pieces[index];

            //shielded piece that isn't you
            if (Piece.GetPieceAlignment(piece) != pa)
            {
                //Check the inverse bitboard
                ulong subBitboard = antiTable.Get(index);

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
                        globalData.bitboard_updatedPieces |= (1uL << index);
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
        //hope the memcpy stuff isn't bad
        /*
        if (black)
        {
            if ((bitboard_auto & globalData.bitboard_piecesBlack) == 0)
            {
                return;
            }
        } else
        {
            if ((bitboard_auto & globalData.bitboard_piecesWhite) == 0)
            {
                return;
            }
        }
        */
        if (bitboard_auto == 0)
        {
            return;
        }

        //MainManager.PrintBitboard(bitboard_auto);
        //MainManager.PrintBitboard(globalData.bitboard_piecesBlack);

        /*
        globalData.bitboard_zombieBlack;
        globalData.bitboard_bladebeastBlack;
        globalData.bitboard_clockworksnapperBlack;
        globalData.bitboard_abominationBlack;
        */

        //ulong enemyBitboard = 0;
        //ulong enemySmeared = 0;
        //ulong allyBitboard = 0;
        bool moveZombies = false;
        ulong emptyBitboard = ~globalData.bitboard_pieces;

        ulong clockworksnapper = 0;
        ulong bladebeast = 0;

        ulong metalFox = 0;
        ulong warpWeaver = 0;

        ulong megacannon = 0;
        ulong momentum = 0;

        //Problem: accessing globaldata.bitboard creates overhead
        if (black)
        {
            //allyBitboard = globalData.bitboard_piecesBlack;
            //enemyBitboard = (globalData.bitboard_pieces & ~allyBitboard);
            //enemySmeared = MainManager.SmearBitboard(enemyBitboard);

            moveZombies = whitePerPlayerInfo.capturedLastTurn;
            clockworksnapper = globalData.bitboard_clockworksnapper & globalData.bitboard_piecesBlack;// & enemySmeared;     //Should work but it doesn't, why???
            bladebeast = globalData.bitboard_bladebeast & globalData.bitboard_piecesBlack;// & enemySmeared;

            metalFox = globalData.bitboard_metalFox & globalData.bitboard_piecesBlack;
            warpWeaver = globalData.bitboard_warpWeaver & globalData.bitboard_piecesBlack;
            megacannon = globalData.bitboard_megacannon & globalData.bitboard_piecesWhite;       //Mega Cannon ticks up on your turn so it can destroy Kings without wrong King Capture attribution
            momentum = globalData.bitboard_momentum & globalData.bitboard_piecesBlack;

            if (!moveZombies && (clockworksnapper | bladebeast | metalFox | warpWeaver | megacannon | momentum) == 0)
            {
                return;
            }
        }
        else
        {
            //allyBitboard = globalData.bitboard_piecesWhite;
            //enemyBitboard = (globalData.bitboard_pieces & ~allyBitboard);
            //enemySmeared = MainManager.SmearBitboard(enemyBitboard);
            moveZombies = blackPerPlayerInfo.capturedLastTurn;
            clockworksnapper = globalData.bitboard_clockworksnapper & globalData.bitboard_piecesWhite;// & enemySmeared;
            bladebeast = globalData.bitboard_bladebeast & globalData.bitboard_piecesWhite;// & enemySmeared;

            /*
            MainManager.PrintBitboard(clockworksnapper);
            MainManager.PrintBitboard(bladebeast);
            MainManager.PrintBitboard(enemySmeared);
            MainManager.PrintBitboard(clockworksnapper & enemySmeared);
            MainManager.PrintBitboard(bladebeast & enemySmeared);
            */

            metalFox = globalData.bitboard_metalFox & globalData.bitboard_piecesWhite;
            warpWeaver = globalData.bitboard_warpWeaver & globalData.bitboard_piecesWhite;
            megacannon = globalData.bitboard_megacannon & globalData.bitboard_piecesBlack;
            momentum = globalData.bitboard_momentum & globalData.bitboard_piecesWhite;

            if (!moveZombies && (clockworksnapper | bladebeast | metalFox | warpWeaver | megacannon | momentum) == 0)
            {
                return;
            }
        }

        PieceTableEntry pteO;// = GlobalPieceManager.GetPieceTableEntry(PieceType.ClockworkSnapper);
        Piece.PieceAlignment opa = black ? PieceAlignment.Black : PieceAlignment.White;
        Piece.PieceAlignment opaB = !black ? PieceAlignment.Black : PieceAlignment.White;

        //note: compiler doesn't see that it can't enter the while loop without running this
        if (clockworksnapper != 0)
        {
            pteO = GlobalPieceManager.GetPieceTableEntry(PieceType.ClockworkSnapper);
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
        }
        if (bladebeast != 0)
        {
            pteO = GlobalPieceManager.GetPieceTableEntry(PieceType.BladeBeast);
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
                if (((((tx) | (ty)) & -8) != 0))
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
                if (((((tx) | (ty)) & -8) != 0))
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
                if (((((tx) | (ty)) & -8) != 0))
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
                if (((((tx) | (ty)) & -8) != 0))
                {
                    continue;
                }
                targetPiece = pieces[tx + ty * 8];
                tpa = Piece.GetPieceAlignment(targetPiece);
                (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
            }
        }

        if (moveZombies)
        {
            ulong zombieBitboard = 0;
            ulong abominationBitboard = 0;
            int kingIndex = 0;
            if (black)
            {
                zombieBitboard = globalData.bitboard_zombie & globalData.bitboard_piecesBlack;
                abominationBitboard = globalData.bitboard_abomination & globalData.bitboard_piecesBlack;
                if (abominationBitboard != 0)
                {
                    kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_king & globalData.bitboard_piecesBlack);
                }
            }
            else
            {
                zombieBitboard = globalData.bitboard_zombie & globalData.bitboard_piecesWhite;
                abominationBitboard = globalData.bitboard_abomination & globalData.bitboard_piecesWhite;
                if (abominationBitboard != 0)
                {
                    kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_king & globalData.bitboard_piecesWhite);
                }
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
                    globalData.bitboard_updatedPieces |= bitindex;
                    globalData.bitboard_updatedPieces |= bitindexOld;

                    pieces[newindex] = pieces[index];
                    pieces[index] = 0;
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, newindex & 7, newindex >> 3, pieces[newindex], BoardUpdateMetadata.BoardUpdateType.Shift));
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
                    globalData.bitboard_updatedPieces |= bitindex;
                    globalData.bitboard_updatedPieces |= bitindexOld;

                    pieces[newindex] = pieces[index];
                    pieces[index] = 0;
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, newindex & 7, newindex >> 3, pieces[newindex], BoardUpdateMetadata.BoardUpdateType.Shift));
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

            Piece.PieceType tpt = Piece.GetPieceType(targetPiece);
            PieceTableEntry pteT = globalData.GetPieceTableEntryFromCache((tx + (ty << 3)), targetPiece); // GlobalPieceManager.GetPieceTableEntry(tpt);

            //Piece carrier with a King inside
            if ((pteT.piecePropertyB & PiecePropertyB.PieceCarry) != 0 && Piece.GetPieceSpecialData(targetPiece) == (int)(Piece.PieceType.King))
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
                whitePerPlayerInfo.piecesLost++;
                whitePerPlayerInfo.pieceValueSumX2 -= pteT.pieceValueX2;
            }
            if (tpa == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceCount--;
                blackPerPlayerInfo.piecesLost++;
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
            if (!pieceChange && ((pteT.pieceProperty & PieceProperty.DestroyCapturer) != 0 || (pteO.pieceProperty & PieceProperty.DestroyOnCapture) != 0 || Piece.GetPieceStatusEffect(oldPiece) == PieceStatusEffect.Fragile || ((1uL << fx + fy * 8 & hagBitboard) != 0) || Piece.GetPieceModifier(targetPiece) == PieceModifier.Vengeful || (tpa == PieceAlignment.White && ((globalData.playerModifier & PlayerModifier.FinalVengeance) != 0) && whitePerPlayerInfo.pieceCount <= 6)))
            {
                oldPiece = 0;
                if (opa == PieceAlignment.White)
                {
                    whitePerPlayerInfo.pieceCount--;
                    whitePerPlayerInfo.piecesLost++;
                    whitePerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                if (opa == PieceAlignment.Black)
                {
                    blackPerPlayerInfo.pieceCount--;
                    blackPerPlayerInfo.piecesLost++;
                    blackPerPlayerInfo.pieceValueSumX2 -= pteO.pieceValueX2;
                }
                pieceChange = true;
            }

            //conversion
            //Destroy Capturer / Destroy on Capture has precedence
            if (!pieceChange && ((pteO.piecePropertyB & PiecePropertyB.MorphImmune) == 0 && (((pteT.pieceProperty & PieceProperty.MorphCapturer) != 0) ||
                ((pteT.pieceProperty & PieceProperty.MorphCapturerNonPawn) != 0 && pteO.promotionType == PieceType.Null) ||
                ((pteT.pieceProperty & PieceProperty.MorphCapturerPawn) != 0 && pteO.promotionType != PieceType.Null))))
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
                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
            }

            globalData.bitboard_updatedPieces |= (1uL << (fx + (fy << 3)));
            globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

            //DeletePieceMovedFromCoordinate(fx, fy, pteO, opa, 0);
            pieces[(fx + (fy << 3))] = 0;
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

        if (metalFox != 0)
        {
            pteO = GlobalPieceManager.GetPieceTableEntry(PieceType.MetalFox);
            while (metalFox != 0)
            {
                int index = MainManager.PopBitboardLSB1(metalFox, out metalFox);

                //Acts like ClockworkSnapper but only against the target square
                //If the attack doesn't work it doesn't do anything

                uint oldPiece = pieces[index];
                if (oldPiece == 0 || Piece.GetPieceType(oldPiece) != PieceType.MetalFox)
                {
                    continue;
                }

                ushort data = Piece.GetPieceSpecialData(pieces[index]);
                if (data == 0)
                {
                    continue;
                }

                int fx = index & 7;
                int fy = index >> 3;
                int tx = data & 7;
                int ty = (data & 56) >> 3;

                if (((((tx) | (ty)) & -8) != 0))
                {
                    continue;
                }

                //target
                uint targetPiece = pieces[tx + ty * 8];
                if (targetPiece == 0)
                {
                    continue;
                }

                oldPiece = Piece.SetPieceSpecialData(0, oldPiece);

                Piece.PieceAlignment tpa = Piece.GetPieceAlignment(targetPiece);

                bool attackSuccessful = false;
                (oldPiece, attackSuccessful) = AutoMoverAttack(black, pteO, opa, fx, fy, tx, ty, targetPiece, oldPiece, tpa, boardUpdateMetadata);
            }
        }

        while (warpWeaver != 0)
        {
            int index = MainManager.PopBitboardLSB1(warpWeaver, out warpWeaver);

            uint oldPiece = pieces[index];
            if (oldPiece == 0 || Piece.GetPieceType(oldPiece) != PieceType.WarpWeaver)
            {
                continue;
            }

            ushort data = Piece.GetPieceSpecialData(pieces[index]);
            if (data == 0)
            {
                continue;
            }

            int fx = index & 7;
            int fy = index >> 3;
            int tx = data & 7;
            int ty = (data & 56) >> 3;
            //Debug.Log(fx + " " + fy + " " + tx + " " + ty);

            if (((((tx) | (ty)) & -8) != 0))
            {
                continue;
            }

            //target
            uint targetPiece = pieces[tx + (ty << 3)];
            if (targetPiece != 0)
            {
                continue;
            }

            globalData.bitboard_updatedPieces |= (1uL << (fx + (fy << 3)));
            globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, tx, ty, oldPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
            }

            PieceTableEntry pte = globalData.GetPieceTableEntryFromCache(index, oldPiece);
            Piece.PieceAlignment pa = Piece.GetPieceAlignment(oldPiece);

            oldPiece = Piece.SetPieceSpecialData(0, oldPiece);

            //DeletePieceMovedFromCoordinate(fx, fy, pte, pa);
            //PlaceMovedPiece(oldPiece, tx, ty, pte, pa);
            pieces[(fx + (fy << 3))] = 0;
            pieces[tx + (ty << 3)] = oldPiece;
        }

        while (megacannon != 0)
        {
            int index = MainManager.PopBitboardLSB1(megacannon, out megacannon);

            uint oldPiece = pieces[index];
            PieceTableEntry pteMC = globalData.GetPieceTableEntryFromCache(index, oldPiece);
            if (oldPiece == 0 || Piece.GetPieceType(oldPiece) != PieceType.MegaCannon)
            {
                continue;
            }

            globalData.bitboard_updatedPieces |= (1uL << index);

            ushort data = Piece.GetPieceSpecialData(oldPiece);
            if (data > 0)
            {
                data += 64;

                //Time to shoot
                //Note: because I don't have many bits it's a 7 turn delay
                //448 + 63 = 511 (maximum storable value right now)
                //change: 
                if (data >= 512)
                {
                    int fx = index & 7;
                    int fy = index >> 3;

                    int tx = data & 7;
                    int ty = (data & 56) >> 3;

                    pieces[index] = Piece.SetPieceSpecialData(0, oldPiece);

                    //Explode all the enemy pieces in the 3x3 area around you
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (tx + dx < 0)
                        {
                            continue;
                        }
                        if (tx + dx > 7)
                        {
                            continue;
                        }

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (ty + dy < 0)
                            {
                                continue;
                            }
                            if (ty + dy > 7)
                            {
                                continue;
                            }

                            if (dx != 0 && dy != 0)
                            {
                                continue;
                            }
                            //Debug.Log("Blow up " + (tx + dx) + " " + (ty + dy));

                            uint enemyPiece = pieces[tx + dx + 8 * (ty + dy)];
                            PieceTableEntry pteE = globalData.GetPieceTableEntryFromCache(tx + dx + 8 * (ty + dy), enemyPiece);

                            //delete the enemies on the delta
                            Piece.PieceAlignment epa = Piece.GetPieceAlignment(GetPieceAtCoordinate(tx + dx, ty + dy));
                            if (epa != opaB && pteE != null && !Piece.IsPieceInvincible(this, enemyPiece, tx + dx, ty + dy, pieces[index], fx, fy, Move.SpecialType.FireCapture, pteMC, pteE))
                            {
                                if (epa == PieceAlignment.White)
                                {
                                    whitePerPlayerInfo.pieceCount--;
                                    whitePerPlayerInfo.piecesLost++;
                                    whitePerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                }
                                if (epa == PieceAlignment.Black)
                                {
                                    blackPerPlayerInfo.pieceCount--;
                                    blackPerPlayerInfo.piecesLost++;
                                    blackPerPlayerInfo.pieceValueSumX2 -= pteE.pieceValueX2;
                                }

                                DeletePieceAtCoordinate(tx + dx, ty + dy, pteE, epa, boardUpdateMetadata);
                                //SetPieceAtCoordinate(tx + i, ty + j, 0);
                            }
                        }
                    }
                } else
                {
                    pieces[index] = Piece.SetPieceSpecialData(data, oldPiece);
                }
            }
        }

        while (momentum != 0)
        {
            int index = MainManager.PopBitboardLSB1(momentum, out momentum);
            uint oldPiece = pieces[index];
            if (oldPiece == 0)
            {
                continue;
            }

            ushort data = Piece.GetPieceSpecialData(pieces[index]);
            if (data == 0)
            {
                continue;
            }

            PieceTableEntry pteM = globalData.GetPieceTableEntryFromCache(index, oldPiece);

            //weird edge case but if you somehow invalidate the bitboard and put some other data piece in that position you need to double check
            if ((pteM.piecePropertyB & (PiecePropertyB.Momentum | PiecePropertyB.BounceMomentum | PiecePropertyB.ReverseMomentum)) == 0)
            {
                continue;
            }

            (int dx, int dy) = Move.DirToDelta((Dir)data);
            int fx = index & 7;
            int fy = index >> 3;

            //Attempt to move
            //If bonks: bounce if bounce mode
            bool bounce = (pteM.piecePropertyB & PiecePropertyB.BounceMomentum) != 0;

            globalData.bitboard_updatedPieces |= (1uL << (index));

            if (((((fx + dx) | (fy + dy)) & -8) != 0) || (pieces[(fx + dx) + ((fy + dy) << 3)] != 0))
            {
                //Bonk
                if (bounce)
                {
                    //Try to move in opposite dir
                    if (((((fx - dx) | (fy - dy)) & -8) != 0) || (pieces[(fx - dx) + ((fy - dy) << 3)] != 0))
                    {
                        //Bonk again
                        pieces[index] = Piece.SetPieceSpecialData(0, oldPiece);
                    }
                    else
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, fx - dx, fy - dy, oldPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (index - dx - (dy << 3)));
                        pieces[(fx - dx) + ((fy - dy) << 3)] = Piece.SetPieceSpecialData((ushort)(Move.ReverseDir((Dir)data)), oldPiece);
                        pieces[(fx) + ((fy) << 3)] = 0;
                    }
                } else
                {
                    pieces[index] = Piece.SetPieceSpecialData(0, oldPiece);
                }
            } else
            {
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(fx, fy, fx + dx, fy + dy, oldPiece, BoardUpdateMetadata.BoardUpdateType.Shift));
                }

                globalData.bitboard_updatedPieces |= (1uL << (index + dx + (dy << 3)));
                //Go
                pieces[(fx + dx) + ((fy + dy) << 3)] = oldPiece;
                pieces[(fx) + ((fy) << 3)] = 0;
            }   
        }
    }
    public void ApplyPromotion(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int yLevel = 7;

        Piece.PieceAlignment targetAlignment = PieceAlignment.White;

        ulong toSearch = MoveGenerator.BITBOARD_PATTERN_RANK1;
        if (black)
        {
            targetAlignment = PieceAlignment.Black;
            yLevel = 0;
            toSearch &= globalData.bitboard_piecesBlack;
        } else
        {
            toSearch <<= 56;
            toSearch &= globalData.bitboard_piecesWhite;
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
                PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

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

                    PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pte.promotionType);

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

        /*
        for (int i = 0; i < 8; i++)
        {
            uint piece = pieces[i + yLevel * 8];
            if (piece != 0 && Piece.GetPieceAlignment(piece) == targetAlignment)
            {
                Piece.PieceType pt = Piece.GetPieceType(piece);
                PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

                if (pte.promotionType != PieceType.Null)
                {
                    pieces[i + yLevel * 8] = Piece.SetPieceType(pte.promotionType, piece);
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i, yLevel, pieces[i + yLevel * 8], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
                    {
                        PlaceGiant(pieces[i + yLevel * 8], i, yLevel);
                    }

                    PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pte.promotionType);

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
        */

        while (toSearch != 0)
        {
            int i = MainManager.PopBitboardLSB1(toSearch, out toSearch);

            uint piece = pieces[i];
            if (piece != 0 && Piece.GetPieceAlignment(piece) == targetAlignment)
            {
                Piece.PieceType pt = Piece.GetPieceType(piece);
                PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

                //transmuted rabbit reverting to normal
                if (pt == PieceType.Rabbit)
                {
                    if (Piece.GetPieceSpecialData(piece) != 0)
                    {
                        //convert to the actual piece again
                        pieces[i] = Piece.SetPieceType((PieceType)(Piece.GetPieceSpecialData(piece)), Piece.SetPieceSpecialData(0, pieces[i]));
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, yLevel, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                        }
                        continue;
                    }
                }

                if (pte.promotionType != PieceType.Null)
                {
                    globalData.bitboard_updatedPieces |= (1uL << (i));
                    pieces[i] = Piece.SetPieceType(pte.promotionType, piece);
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, yLevel, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
                    {
                        PlaceGiant(pieces[i], i, yLevel);
                    }

                    PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pte.promotionType);

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
    }

    //This is the bottleneck function (The slowest part of ApplyMove?)
    //It loops over every single piece which is pretty slow when it is going through a lot of iterations
    //May need to pay attention to ways to reduce this
    //The other big one is GeneratePieceBitboards which is another full board loop
    public void TickDownStatusEffects(bool black, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //Turns tick down at the end of the other player's turn
        //This is so that things applied to a King don't cause problems
        //(But in general I won't normally allow modified kings)
        PieceAlignment pa = black ? PieceAlignment.White : PieceAlignment.Black;

        ulong piecesToCheck = black ? globalData.bitboard_piecesWhite : globalData.bitboard_piecesBlack;

        //Neutrals only really get checked here for special cases (Sludge Trail and whatever other kinds of space filling pieces I make up)
        piecesToCheck |= globalData.bitboard_piecesNeutral;

        bool immuneZone = (globalData.playerModifier & PlayerModifier.ImmunityZone) != 0;

        piecesToCheck &= (~globalData.bitboard_noStatus | globalData.bitboard_EOTPieces);

        //MainManager.PrintBitboard(~globalData.bitboard_noStatus);

        ulong processedSquares = 0;
        while (piecesToCheck != 0)
        {
            int i = MainManager.PopBitboardLSB1(piecesToCheck, out piecesToCheck);
            uint piece = pieces[i];

            ulong bitIndex = 1uL << i;

            if (piece == 0)
            {
                continue;
            }
            if ((bitIndex & processedSquares) != 0)
            {
                continue;
            }

            PieceTableEntry pte = null;
            Piece.PieceType pt;// = Piece.GetPieceType(piece);

            //PieceTableEntry pte = globalData.GetPieceTableEntryFromCache(i, piece); //GlobalPieceManager.GetPieceTableEntry(piece);
            //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(piece);
            if ((bitIndex & globalData.bitboard_sludgeTrail) != 0)
            //if (pt == PieceType.SludgeTrail)  //survives for 2 ply (1 ply from being spawned, 1 to block black for 1 turn)
            {
                globalData.bitboard_updatedPieces |= (1uL << (i));
                if (Piece.GetPieceSpecialData(piece) == 0)
                {
                    pieces[i] = Piece.SetPieceSpecialData(1, piece);
                } else
                {
                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, piece, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }
                    pieces[i] = 0;
                }
                continue;
            }

            //assume the piece table build fixes this?
            /*
            if (Piece.GetPieceAlignment(pieces[i]) != pa)
            {
                continue;
            }
            */

            /*
            if (Piece.GetPieceModifier(piece) == PieceModifier.HalfShielded)
            {
                pieces[i] = Piece.SetPieceModifier(0, piece);
            }
            */

            if ((piece & 0x3C0000) == 0x240000)
            {
                pieces[i] = Piece.SetPieceModifier(0, piece);
                globalData.bitboard_updatedPieces |= (1uL << (i));
            }

            PieceTableEntry pteB;

            ulong checkBitboard = 0;
            if ((bitIndex & globalData.bitboard_EOTPieces) != 0)
            {
                pt = Piece.GetPieceType(piece);

                switch (pt)
                {
                    case PieceType.Rabbit:
                        if (Piece.GetPieceSpecialData(piece) != 0)
                        {
                            //rabbit reverting to normal
                            //convert to the actual piece again
                            pieces[i] = Piece.SetPieceType((PieceType)(Piece.GetPieceSpecialData(piece)), Piece.SetPieceSpecialData(0, pieces[i]));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.SummerQueen:
                    case PieceType.SummerRook:
                    case PieceType.SpringKnight:
                    case PieceType.SummerPawn:
                    case PieceType.SpringPawn:
                        if (turn != 0 && bonusPly == 0 && turn % 5 == 0)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt + 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }

                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }

                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                        }
                        break;
                    case PieceType.WinterQueen:
                    case PieceType.WinterBishop:
                    case PieceType.FallKnight:
                    case PieceType.WinterPawn:
                    case PieceType.FallPawn:
                        if (turn != 0 && bonusPly == 0 && turn % 5 == 0)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt - 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }

                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.DayQueen:
                    case PieceType.DayBishop:
                    case PieceType.DayPawn:
                        if (bonusPly == 0)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt + 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }

                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.NightQueen:
                    case PieceType.NightKnight:
                    case PieceType.NightPawn:
                        if (bonusPly == 0)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt - 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }

                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.FlameEgg:
                        //File free of enemies
                        if (!black)
                        {
                            checkBitboard = globalData.bitboard_piecesWhite;
                        }
                        else
                        {
                            checkBitboard = globalData.bitboard_piecesBlack;
                        }

                        if (((MoveGenerator.BITBOARD_PATTERN_AFILE << (i & 7)) & checkBitboard) == 0)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt + 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.WaveEgg:
                        bool waveCheck = true;
                        if (!black)
                        {
                            waveCheck = ((1uL << i) & (globalData.bitboard_piecesBlackAdjacent1 | globalData.bitboard_piecesBlackAdjacent2 | globalData.bitboard_piecesBlackAdjacent4 | globalData.bitboard_piecesBlackAdjacent8)) == 0;
                        }
                        else
                        {
                            waveCheck = ((1uL << i) & (globalData.bitboard_piecesWhiteAdjacent1 | globalData.bitboard_piecesWhiteAdjacent2 | globalData.bitboard_piecesWhiteAdjacent4 | globalData.bitboard_piecesWhiteAdjacent8)) == 0;
                        }

                        if (waveCheck)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt + 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                    case PieceType.RockEgg:
                        bool rockCheck = true;
                        if (!black)
                        {
                            rockCheck = ((1uL << i) & ((globalData.bitboard_piecesBlackAdjacent8) | (globalData.bitboard_piecesWhiteAdjacent8 | globalData.bitboard_piecesWhiteAdjacent4 | globalData.bitboard_piecesWhiteAdjacent2))) != 0;
                        }
                        else
                        {
                            rockCheck = ((1uL << i) & ((globalData.bitboard_piecesWhiteAdjacent8) | (globalData.bitboard_piecesBlackAdjacent8 | globalData.bitboard_piecesBlackAdjacent4 | globalData.bitboard_piecesBlackAdjacent2))) != 0;
                        }

                        /*
                        if (!black)
                        {
                            checkBitboard = globalData.bitboard_piecesBlack;
                        }
                        else
                        {
                            checkBitboard = globalData.bitboard_piecesWhite;
                        }
                        //would overlap with self
                        checkBitboard &= ~(1uL << i);
                        */

                        //Old condition: empty row
                        //Hard to get which isn't very fitting
                        //Instead I'll make it 8 neighbors
                        //look at all those shift arrows :P

                        //if (((MoveGeneratorInfoEntry.BITBOARD_PATTERN_RANK1 << ((i >> 3) << 3)) & checkBitboard) == 0)

                        if (rockCheck)
                        {
                            globalData.bitboard_updatedPieces |= (1uL << (i));
                            if (pte == null)
                            {
                                pte = globalData.GetPieceTableEntryFromCache(i, pieces[i]);
                            }
                            pteB = GlobalPieceManager.GetPieceTableEntry((Piece.PieceType)(pt + 1));
                            if (!black)
                            {
                                blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            else
                            {
                                whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pte.pieceValueX2);
                            }
                            pieces[i] = Piece.SetPieceType(pteB.type, pieces[i]);
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, pieces[i], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                            }
                        }
                        break;
                }
            }

            //status duration = 0
            if ((piece & 0x3C000000) == 0)
            {
                continue;
            }

            //assume some update is going to happen
            globalData.bitboard_updatedPieces |= (1uL << (i));

            byte psed = Piece.GetPieceStatusDuration(piece);
            /*
            if (psed == 0)
            {
                continue;
            }
            */

            if (black && (i >= 2 && i <= 5) && immuneZone)
            {
                pieces[i] = Piece.SetPieceStatusEffect(0, Piece.SetPieceStatusDuration(0, piece));
                continue;
            }

            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(piece);

            psed--;
            if (psed == 0)
            {

                //Rip
                if (pse <= PieceStatusEffect.Poisoned)
                {
                    if (pte == null)
                    {
                        pte = globalData.GetPieceTableEntryFromCache(i, piece);
                    }
                    if (!black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (short)(pte.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                    }

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, piece, BoardUpdateMetadata.BoardUpdateType.Capture));
                    }

                    DeletePieceAtCoordinate(i & 7, i >> 3, pte, pa, boardUpdateMetadata);
                    //pieces[i] = 0;
                    continue;
                }

                pieces[i] = Piece.SetPieceStatusEffect(0, piece);
            }
            pieces[i] = Piece.SetPieceStatusDuration(psed, piece);

            switch (pse)
            {
                case PieceStatusEffect.Light:
                    if (pte == null)
                    {
                        pte = globalData.GetPieceTableEntryFromCache(i, piece);
                    }
                    if ((pte.piecePropertyB & PiecePropertyB.TrueShiftImmune) != 0)
                    {
                        break;
                    }
                    if (!black)
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = piece;
                            pieces[i] = 0;
                            globalData.bitboard_updatedPieces |= (1uL << (i - 8));
                            globalData.bitboard_piecesBlack |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pieces[i - 8], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    else
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = piece;
                            pieces[i] = 0;
                            globalData.bitboard_updatedPieces |= (1uL << (i + 8));
                            globalData.bitboard_piecesWhite |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pieces[i + 8], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
                case PieceStatusEffect.Heavy:
                    if (pte == null)
                    {
                        pte = globalData.GetPieceTableEntryFromCache(i, piece);
                    }
                    if ((pte.piecePropertyB & PiecePropertyB.TrueShiftImmune) != 0)
                    {
                        break;
                    }
                    //directions opposite of Light
                    if (!black)
                    {
                        //i += 8
                        if (i + 8 < 64 && pieces[i + 8] == 0)
                        {
                            pieces[i + 8] = piece;
                            pieces[i] = 0;
                            globalData.bitboard_updatedPieces |= (1uL << (i + 8));
                            globalData.bitboard_piecesBlack |= (1uL << (i + 8));
                            processedSquares |= (1uL << (i + 8));
                            globalData.bitboard_piecesBlack &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) + 1, pieces[i + 8], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    else
                    {
                        //i -= 8
                        if (i - 8 >= 0 && pieces[i - 8] == 0)
                        {
                            pieces[i - 8] = piece;
                            pieces[i] = 0;
                            globalData.bitboard_updatedPieces |= (1uL << (i - 8));
                            globalData.bitboard_piecesWhite |= (1uL << (i - 8));
                            processedSquares |= (1uL << (i - 8));
                            globalData.bitboard_piecesWhite &= ~(1uL << (i));
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(i & 7, i >> 3, i & 7, (i >> 3) - 1, pieces[i - 8], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }
                        }
                    }
                    break;
            }
        }
    }
    public bool TryPiecePushAlly(int x, int y, int dx, int dy, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((((x | y) & -8) != 0) || Piece.GetPieceAlignment(pieces[x + y * 8]) != pa)
        {
            return false;
        }

        return TryPiecePush(x, y, dx, dy, GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]), boardUpdateMetadata);
    }
    public bool TryPiecePushEnemy(int x, int y, int dx, int dy, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((((x | y) & -8) != 0) || Piece.GetPieceAlignment(pieces[x + y * 8]) == pa)
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

        if ((((x | y) & -8) != 0))
        {
            return;
        }

        int px = x + dx;
        int py = y + dy;

        bool pincer = false;
        if ((((px | y) & -8) != 0))
        {
            pincer = true;
        } else
        {
            if (pieces[px + (py << 3)] != 0 && Piece.GetPieceAlignment(pieces[px + (py << 3)]) == pa)
            {
                pincer = true;
            }
        }

        //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        PieceTableEntry pte = globalData.GetPieceTableEntryFromCache((x + (y << 3)), pieces[(x + (y << 3))]);
        if (pincer && pieces[x + y * 8] != 0 && Piece.GetPieceAlignment(pieces[x + y * 8]) != pa && !Piece.IsPieceInvincible(this, pieces[x + y * 8], x, y, pieces[ox + oy * 8], ox, oy, Move.SpecialType.Advancer, pteO, pte))
        {
            //Destroy x, y
            if (pte == null)
            {
                return;
            }

            globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));

            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Capture));
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
        if ((((x | y) & -8) != 0) || pieces[x + (y << 3)] == 0)
        {
            return false;
        }
        return TryPiecePush(x, y, dx, dy, globalData.GetPieceTableEntryFromCache(x + (y << 3), pieces[x + (y << 3)]), boardUpdateMetadata);
    }
    public bool TryPiecePush(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int tx = x + dx;
        int ty = y + dy;

        if (pte == null)
        {
            return false;
        }

        if ((((tx | ty) & -8) != 0))
        {
            return false;
        }

        //Debug.Log("Push weak");

        //Nothing to push
        if (pte == null)
        {
            return false;
        }

        if (pieces[tx + ty * 8] == 0 && (pte.pieceProperty & (PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) == 0)
        {
            if (boardUpdateMetadata != null)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Shift));
            }

            globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));
            globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

            //blow wind towards ty
            pieces[tx + ty * 8] = pieces[x + y * 8];
            pieces[x + y * 8] = 0;
            return true;
        }

        return false;
    }
    public bool TryPieceSwapEnemy(int x, int y, int x2, int y2, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((((x | y) & -8) != 0))
        {
            return false;
        }
        if ((((x2 | y2) & -8) != 0))
        {
            return false;
        }
        if ((pieces[x + y * 8] != 0 && Piece.GetPieceAlignment(pieces[x + y * 8]) != pa) || (pieces[x2 + y2 * 8] != 0 && Piece.GetPieceAlignment(pieces[x2 + y2 * 8]) != pa))
        {
            return false;
        }

        PieceTableEntry pteA = globalData.GetPieceTableEntryFromCache(x + (y << 3), pieces[x + (y << 3)]); // GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        PieceTableEntry pteB = globalData.GetPieceTableEntryFromCache(x2 + (y2 << 3), pieces[x2 + (y2 << 3)]); // GlobalPieceManager.GetPieceTableEntry(pieces[x2 + y2 * 8]);

        if ((pteA != null && (((pteA.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0)) || (pteB != null && (pteB.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            if (pieces[x + (y << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }
        if (boardUpdateMetadata != null)
        {
            if (pieces[x2 + (y2 << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x2, y2, x, y, pieces[x2 + (y2 << 3)], BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }

        globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));
        globalData.bitboard_updatedPieces |= (1uL << (x2 + (y2 << 3)));

        uint temp = pieces[x + y * 8];
        pieces[x + y * 8] = pieces[x2 + y2 * 8];
        pieces[x2 + y2 * 8] = temp;
        return true;
    }
    public bool TryPieceSwap(int x, int y, int x2, int y2, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((((x | y) & -8) != 0))
        {
            return false;
        }
        if ((((x2 | y2) & -8) != 0))
        {
            return false;
        }

        PieceTableEntry pteA = globalData.GetPieceTableEntryFromCache(x + (y << 3), pieces[x + (y << 3)]); // GlobalPieceManager.GetPieceTableEntry(pieces[x + y * 8]);
        PieceTableEntry pteB = globalData.GetPieceTableEntryFromCache(x2 + (y2 << 3), pieces[x2 + (y2 << 3)]); // GlobalPieceManager.GetPieceTableEntry(pieces[x2 + y2 * 8]);

        if ((pteA != null && ((pteA.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0)) || (pteB != null && ((pteB.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            if (pieces[x + (y << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pieces[x + (y << 3)], BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }
        if (boardUpdateMetadata != null)
        {
            if (pieces[x2 + (y2 << 3)] != 0)
            {
                boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, x2, y2, pieces[x2 + (y2 << 3)], BoardUpdateMetadata.BoardUpdateType.Shift));
            }
        }

        globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));
        globalData.bitboard_updatedPieces |= (1uL << (x2 + (y2 << 3)));

        uint temp = pieces[x + y * 8];
        pieces[x + y * 8] = pieces[x2 + y2 * 8];
        pieces[x2 + y2 * 8] = temp;
        return true;
    }
    public bool TryPiecePull(int x, int y, int dx, int dy, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if ((((x | y) & -8) != 0) || pieces[x + y * 8] == 0)
        {
            return false;
        }

        return TryPiecePull(x, y, dx, dy, globalData.GetPieceTableEntryFromCache(x + (y << 3), pieces[x + (y << 3)]), boardUpdateMetadata);
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
        if ((((tx | ty) & -8) != 0))
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
            if ((((tx | ty) & -8) != 0))
            {
                return false;
            }
        }

        PieceTableEntry pte = globalData.GetPieceTableEntryFromCache(tx + (ty << 3), pieces[tx + (ty << 3)]);
        if (Piece.GetPieceAlignment(pieces[tx + ty * 8]) == target || (pte != null && ((pte.pieceProperty & (PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0)))
        {
            return false;
        }

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(tx, ty, (tx - dx), (ty - dy), pieces[tx + ty * 8], BoardUpdateMetadata.BoardUpdateType.Shift));
        }

        globalData.bitboard_updatedPieces |= (1uL << ((tx - dx) + ((ty - dy) << 3)));
        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));

        //obstacle at tx, ty, pull to 1 step earlier
        pieces[(tx - dx) + (ty - dy) * 8] = pieces[tx + ty * 8];
        pieces[tx + ty * 8] = 0;

        return true;
    }
    public bool TryPiecePushStrong(int x, int y, int dx, int dy, PieceTableEntry pte, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        if (pte != null && ((pte.pieceProperty & (PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & Piece.PiecePropertyB.TrueShiftImmune) != 0))
        {
            return false;
        }

        int tx = x;
        int ty = y;

        tx += dx;
        ty += dy;

        if ((((tx | ty) & -8) != 0))
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
            if ((((tx | ty) & -8) != 0))
            {
                break;
            }
        }

        //back 1 step
        tx -= dx;
        ty -= dy;

        if (boardUpdateMetadata != null)
        {
            boardUpdateMetadata.Add(new BoardUpdateMetadata(x, y, tx, ty, pieces[x + y * 8], BoardUpdateMetadata.BoardUpdateType.Shift));
        }

        globalData.bitboard_updatedPieces |= (1uL << (tx + (ty << 3)));
        globalData.bitboard_updatedPieces |= (1uL << (x + (y << 3)));

        //Debug.Log("L: " + x + " " + y + " " + dx + " " + dy + " " + tx + " " + ty + " " + Piece.GetPieceType(pieces[tx + ty * 8]));
        pieces[tx + ty * 8] = pieces[x + y * 8];
        pieces[x + y * 8] = 0;

        return true;
    }
    public void DoPassivePush(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.orbiterDeltas[i][1];

            if (((((x + dx) | (y + dy)) & -8) != 0))
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
            {
                continue;
            }

            TryPiecePush(x + dx, y + dy, dx, dy, globalData.GetPieceTableEntryFromCache(x + dx + ((y + dy) << 3), pieces[x + dx + ((y + dy) << 3)]), boardUpdateMetadata);
        }
    }
    public void DoPassivePushStrong(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.orbiterDeltas[i][1];

            if (((((x + dx) | (y + dy)) & -8) != 0))
            {
                continue;
            }

            if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
            {
                continue;
            }

            //Debug.Log("Passive strong");
            TryPiecePushStrong(x + dx, y + dy, dx, dy, globalData.GetPieceTableEntryFromCache(x + dx + ((y + dy) << 3), pieces[x + dx + ((y + dy) << 3)]), boardUpdateMetadata);
        }
    }
    public void DoPassivePull(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.orbiterDeltas[i][1];

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

            TryPiecePull(x + dx * 2, y + dy * 2, dx, dy, globalData.GetPieceTableEntryFromCache(x + (dx << 1) + ((y + (dy << 1)) << 3), pieces[x + (dx << 1) + ((y + (dy << 1)) << 3)]), boardUpdateMetadata);
        }
    }
    public void DoPassivePullStrong(int x, int y, Piece.PieceAlignment pa, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        for (int i = 0; i < 8; i++)
        {
            int dx = GlobalPieceManager.orbiterDeltas[i][0];
            int dy = GlobalPieceManager.orbiterDeltas[i][1];

            if (((((x + dx) | (y + dy)) & -8) != 0))
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

                if (((((x + dx) | (y + dy)) & -8) != 0))
                {
                    continue;
                }

                if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
                {
                    continue;
                }

                TryPiecePush(x + dx, y + dy, dx, dy, globalData.GetPieceTableEntryFromCache(x + dx + ((y + dy) << 3), pieces[x + dx + ((y + dy) << 3)]), boardUpdateMetadata);
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

                if (((((x + dx) | (y + dy)) & -8) != 0))
                {
                    continue;
                }

                if (Piece.GetPieceAlignment(pieces[x + dx + (y + dy) * 8]) == pa)
                {
                    continue;
                }

                //Debug.Log("Passive strong");
                TryPiecePushStrong(x + dx, y + dy, dx, dy, globalData.GetPieceTableEntryFromCache(x + dx + ((y + dy) << 3), pieces[x + dx + ((y + dy) << 3)]), boardUpdateMetadata);
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

                if (((((x + dx) | (y + dy)) & -8) != 0))
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


                globalData.bitboard_updatedPieces |= (1uL << ((x + dx) + ((y + dy) << 3)));
                pieces[x + dx + ((y + dy) << 3)] = Piece.SetPieceStatusEffect(PieceStatusEffect.None, Piece.SetPieceStatusDuration(0, piece));
                if (boardUpdateMetadata != null)
                {
                    boardUpdateMetadata.Add(new BoardUpdateMetadata(x + dx, y + dy, piece, BoardUpdateMetadata.BoardUpdateType.StatusCure));
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
        int lastMoveIndex;// = Move.GetToX(lastMove) + 8 * Move.GetToY(lastMove);
        if (black)
        {
            lastMove = blackPerPlayerInfo.lastMove;
            lastMoveIndex = blackPerPlayerInfo.lastPieceMovedLocation;
        }
        else
        {
            lastMove = whitePerPlayerInfo.lastMove;
            lastMoveIndex = whitePerPlayerInfo.lastPieceMovedLocation;
        }

        ulong fanBitboard = 0;
        if (!black)
        {
            fanBitboard = globalData.bitboard_fanBlack;
        }
        else
        {
            fanBitboard = globalData.bitboard_fanWhite;
        }
        PieceTableEntry pte;
        //where are the bugs happening
        //there are legit ways this can happen though? (DestroyCapturer)
        if (lastMoveIndex >= 0 && pieces[lastMoveIndex] != 0)
        {
            processedSquares |= 1uL << lastMoveIndex;

            pte = globalData.GetPieceTableEntryFromCache(lastMoveIndex, pieces[lastMoveIndex]); // GlobalPieceManager.GetPieceTableEntry(pieces[lastMoveIndex]);

            bool modifierMovement = false;

            //since most of the time these are false this is in front?

            //How is pte null sometimes
            //Feels like a bug
            /*
            if (pte != null)
            {

            }
            */
            if ((globalData.enemyModifier & (EnemyModifier.Mesmerizing | EnemyModifier.Rifter)) != 0 && (turn & 1) == 1 && ((pte.pieceProperty & (PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & PiecePropertyB.TrueShiftImmune) == 0))
            {
                if (!blackToMove && (globalData.enemyModifier & EnemyModifier.Mesmerizing) != 0)
                {
                    //Up push
                    if (lastMoveIndex < 56 && pieces[lastMoveIndex + 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) + 1, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        //blow wind upwards
                        pieces[lastMoveIndex + 8] = pieces[lastMoveIndex];
                        pieces[lastMoveIndex] = 0;
                        processedSquares |= 1uL << lastMoveIndex + 8;
                        globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                        globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 8);
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
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) - 1, (lastMoveIndex >> 3), pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            pieces[lastMoveIndex - 1] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                            processedSquares |= 1uL << lastMoveIndex - 1;
                            globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                            globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex - 1);
                            modifierMovement = true;
                        }
                    }
                    else
                    {
                        //right
                        if ((lastMoveIndex & 7) < 7 && pieces[lastMoveIndex + 1] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) + 1, (lastMoveIndex >> 3), pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            pieces[lastMoveIndex + 1] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                            processedSquares |= 1uL << lastMoveIndex + 1;
                            globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                            globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 1);
                            modifierMovement = true;
                        }
                    }
                }
            }

            if (!modifierMovement && (fanBitboard != 0 || ((1uL << lastMoveIndex) & globalData.bitboard_square_normal) != 0) && (((pte.pieceProperty & (PieceProperty.NoTerrain)) == 0 && (pte.piecePropertyB & PiecePropertyB.TrueShiftImmune) == 0)))
            {
                bool fan = false;

                if (((1uL << lastMoveIndex) & fanBitboard) != 0)
                {
                    if (!black)
                    {
                        if (lastMoveIndex > 7 && pieces[lastMoveIndex - 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) - 1, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            pieces[lastMoveIndex - 8] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                            globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                            globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex - 8);
                            processedSquares |= 1uL << lastMoveIndex - 8;
                            fan = true;
                        }
                    }
                    else
                    {
                        if (lastMoveIndex < 56 && pieces[lastMoveIndex + 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) + 1, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            //blow wind upwards
                            pieces[lastMoveIndex + 8] = pieces[lastMoveIndex];
                            pieces[lastMoveIndex] = 0;
                            globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                            globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 8);
                            processedSquares |= 1uL << lastMoveIndex + 8;
                            fan = true;
                        }
                    }
                }

                if (!fan)
                {
                    switch (globalData.squares[lastMoveIndex].type)
                    {
                        case Square.SquareType.WindUp:
                            if (lastMoveIndex < 56 && pieces[lastMoveIndex + 8] == 0)
                            {
                                if (boardUpdateMetadata != null)
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) + 1, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                }

                                //blow wind upwards
                                pieces[lastMoveIndex + 8] = pieces[lastMoveIndex];
                                pieces[lastMoveIndex] = 0;
                                processedSquares |= 1uL << lastMoveIndex + 8;
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 8);
                            }
                            break;
                        case Square.SquareType.WindDown:
                            if (lastMoveIndex > 7 && pieces[lastMoveIndex - 8] == 0)
                            {
                                if (boardUpdateMetadata != null)
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7), (lastMoveIndex >> 3) - 1, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                }

                                pieces[lastMoveIndex - 8] = pieces[lastMoveIndex];
                                pieces[lastMoveIndex] = 0;
                                processedSquares |= 1uL << lastMoveIndex - 8;
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex - 8);
                            }
                            break;
                        case Square.SquareType.WindLeft:
                            if ((lastMoveIndex & 7) > 0 && pieces[lastMoveIndex - 1] == 0)
                            {
                                if (boardUpdateMetadata != null)
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) - 1, (lastMoveIndex >> 3), pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                }

                                pieces[lastMoveIndex - 1] = pieces[lastMoveIndex];
                                pieces[lastMoveIndex] = 0;
                                processedSquares |= 1uL << lastMoveIndex - 1;
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex - 1);
                            }
                            break;
                        case Square.SquareType.WindRight:
                            if ((lastMoveIndex & 7) < 7 && pieces[lastMoveIndex + 1] == 0)
                            {
                                if (boardUpdateMetadata != null)
                                {
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, (lastMoveIndex & 7) + 1, (lastMoveIndex >> 3), pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                }

                                pieces[lastMoveIndex + 1] = pieces[lastMoveIndex];
                                pieces[lastMoveIndex] = 0;
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 1);
                                processedSquares |= 1uL << lastMoveIndex + 1;
                            }
                            break;
                        case Square.SquareType.Slippery:
                            int iceDx = 0;
                            int iceDy = 0;
                            if (Move.GetDir(lastMove) != Move.Dir.Null)
                            {
                                (iceDx, iceDy) = Move.DirToDelta(Move.GetDir(lastMove));

                                iceDx += lastMoveIndex & 7;
                                iceDy += (lastMoveIndex & 56) >> 3;

                                //check ice move legality
                                if (iceDx >= 0 && iceDx <= 7 && iceDy >= 0 && iceDy <= 7 && pieces[iceDx + iceDy * 8] == 0)
                                {
                                    if (boardUpdateMetadata != null)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, iceDx, iceDy, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                    }

                                    processedSquares |= 1uL << (iceDx + iceDy * 8);
                                    pieces[iceDx + iceDy * 8] = pieces[lastMoveIndex];
                                    pieces[lastMoveIndex] = 0;
                                    globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                    globalData.bitboard_updatedPieces |= 1uL << (iceDx + (iceDy << 3));
                                }
                            }
                            break;
                        case Square.SquareType.Bouncy:
                            int bouncyDx = 0;
                            int bouncyDy = 0;
                            if (Move.GetDir(lastMove) != Move.Dir.Null)
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
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, bouncyDx, bouncyDy, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                    }

                                    processedSquares |= 1uL << (bouncyDx + bouncyDy * 8);
                                    pieces[bouncyDx + bouncyDy * 8] = pieces[lastMoveIndex];
                                    pieces[lastMoveIndex] = 0;
                                    globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                    globalData.bitboard_updatedPieces |= 1uL << (bouncyDx + (bouncyDy << 3));
                                }
                            }
                            break;
                        case Square.SquareType.Promotion:
                            if (pte.promotionType != PieceType.Null)
                            {
                                switch (Piece.GetPieceAlignment(pieces[lastMoveIndex]))
                                {
                                    case PieceAlignment.White:
                                        if (((lastMoveIndex & 56) >> 3 <= 3))
                                        {
                                            break;
                                        }
                                        break;
                                    case PieceAlignment.Black:
                                        if (((lastMoveIndex & 56) >> 3 > 3))
                                        {
                                            break;
                                        }
                                        break;
                                }

                                pieces[lastMoveIndex] = Piece.SetPieceType(pte.promotionType, pieces[lastMoveIndex]);
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;

                                if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
                                {
                                    PlaceGiant(pieces[lastMoveIndex], lastMoveIndex & 7, (lastMoveIndex & 56) >> 3);
                                    globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 1);
                                    globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 8);
                                    globalData.bitboard_updatedPieces |= 1uL << (lastMoveIndex + 9);
                                }

                                PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pte.promotionType);

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
                                    boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                                }
                            }
                            break;
                        case Square.SquareType.Frost:
                            //Zap the piece
                            if (Piece.GetPieceStatusEffect(pieces[lastMoveIndex]) == PieceStatusEffect.None && ((pte.piecePropertyB & PiecePropertyB.StatusImmune) == 0) && Piece.GetPieceModifier(pieces[lastMoveIndex]) != PieceModifier.Immune)
                            {
                                pieces[lastMoveIndex] = Piece.SetPieceStatusEffect(PieceStatusEffect.Frozen, pieces[lastMoveIndex]);
                                pieces[lastMoveIndex] = Piece.SetPieceStatusDuration(2, pieces[lastMoveIndex]);
                                globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;

                                if (boardUpdateMetadata != null)
                                {
                                    if (pieces[lastMoveIndex] != 0)
                                    {
                                        boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.StatusApply));
                                    }
                                }
                            }
                            break;
                        default:
                            if ((globalData.playerModifier & PlayerModifier.Slippery) != 0 && Piece.GetPieceAlignment(pieces[lastMoveIndex]) == PieceAlignment.White)
                            {
                                int piceDx = 0;
                                int piceDy = 0;
                                if (Move.GetDir(lastMove) != Move.Dir.Null)
                                {
                                    (piceDx, piceDy) = Move.DirToDelta(Move.GetDir(lastMove));

                                    piceDx += lastMoveIndex & 7;
                                    piceDy += (lastMoveIndex & 56) >> 3;

                                    //check ice move legality
                                    if (piceDx >= 0 && piceDx <= 7 && piceDy >= 0 && piceDy <= 7 && pieces[piceDx + piceDy * 8] == 0)
                                    {
                                        if (boardUpdateMetadata != null)
                                        {
                                            boardUpdateMetadata.Add(new BoardUpdateMetadata(lastMoveIndex & 7, lastMoveIndex >> 3, piceDx, piceDy, pieces[lastMoveIndex], BoardUpdateMetadata.BoardUpdateType.Shift));
                                        }

                                        processedSquares |= 1uL << (piceDx + piceDy * 8);
                                        pieces[piceDx + piceDy * 8] = pieces[lastMoveIndex];
                                        pieces[lastMoveIndex] = 0;

                                        globalData.bitboard_updatedPieces |= 1uL << (piceDx + piceDy * 8);
                                        globalData.bitboard_updatedPieces |= 1uL << lastMoveIndex;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }


        ulong squaresToSearch;
        //Only the side that just moved is affected
        //(so wind squares only push your pieces after your turn)
        //This is so that wind tiles are slow enough to react to better near the start of the game
        //(Otherwise you can get extremely fast rush strategies if the wind blows towards the opponent?)
        if (black)
        {
            squaresToSearch = (blackPieces) & ~processedSquares;
        }
        else
        {
            squaresToSearch = (whitePieces) & ~processedSquares;
        }

        //skip searching normals
        //But the fan aura is not normal
        squaresToSearch &= (~globalData.bitboard_square_normal) | fanBitboard;

        if (squaresToSearch == 0)
        {
            return;
        }


        //Fix these bitboards
        if (globalData.bitboard_square_cursed != 0)
        {
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
                    pte = globalData.GetPieceTableEntryFromCache(index, pieces[index]); //GlobalPieceManager.GetPieceTableEntry(pieces[index]);
                    if (pte == null || (pte.pieceProperty & (PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & (PiecePropertyB.Giant)) != 0)
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
                    pte = globalData.GetPieceTableEntryFromCache(index, pieces[index]); //GlobalPieceManager.GetPieceTableEntry(pieces[index]);
                    if (pte == null || (pte.pieceProperty & (PieceProperty.NoTerrain)) != 0 || (pte.piecePropertyB & (PiecePropertyB.Giant)) != 0)
                    {
                        continue;
                    }

                    DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pte, Piece.PieceAlignment.Black, boardUpdateMetadata);
                    //pieces[index] = 0;
                    blackPerPlayerInfo.pieceValueSumX2 -= (pte.pieceValueX2);
                }
            }
        }

        //Do the other types




        while (squaresToSearch != 0)
        {
            int index = MainManager.PopBitboardLSB1(squaresToSearch, out squaresToSearch);

            ulong bitIndex = 1uL << index;

            if (pieces[index] == 0 || (bitIndex & processedSquares) != 0)
            {
                continue;
            }
            processedSquares |= bitIndex;

            PieceTableEntry pteI = globalData.GetPieceTableEntryFromCache(index, pieces[index]); //GlobalPieceManager.GetPieceTableEntry(pieces[index]);

            if (globalData.squares[index].type != Square.SquareType.Promotion && ((pteI.pieceProperty & (PieceProperty.NoTerrain)) != 0 || (pteI.piecePropertyB & (PiecePropertyB.Giant)) != 0))
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
            } else
            {
                if (((1uL << index) & fanBitboard) != 0)
                {
                    if (black)
                    {
                        if (index < 56 && pieces[index + 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) + 1, pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            //blow wind upwards
                            globalData.bitboard_updatedPieces |= (1uL << (index + 8));
                            globalData.bitboard_updatedPieces |= 1uL << index;
                            pieces[index + 8] = pieces[index];
                            pieces[index] = 0;
                            processedSquares |= 1uL << index + 8;
                            continue;
                        }
                    } else
                    {
                        if (index > 7 && pieces[index - 8] == 0)
                        {
                            if (boardUpdateMetadata != null)
                            {
                                boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) - 1, pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                            }

                            globalData.bitboard_updatedPieces |= (1uL << (index - 8));
                            globalData.bitboard_updatedPieces |= 1uL << index;
                            pieces[index - 8] = pieces[index];
                            pieces[index] = 0;
                            processedSquares |= 1uL << index - 8;
                            continue;
                        }
                    }
                }
            }

            switch (globalData.squares[index].type)
            {
                case Square.SquareType.Fire:
                    //Burn that piece
                    if (black)
                    {
                        blackPerPlayerInfo.pieceValueSumX2 -= (pteI.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
                    }
                    else
                    {
                        whitePerPlayerInfo.pieceValueSumX2 -= (pteI.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                    }

                    DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pteI, black ? Piece.PieceAlignment.Black : Piece.PieceAlignment.White, boardUpdateMetadata);
                    break;
                case Square.SquareType.WindUp:
                    if (index < 56 && pieces[index + 8] == 0)
                    {
                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) + 1, pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        //blow wind upwards
                        globalData.bitboard_updatedPieces |= (1uL << (index + 8));
                        globalData.bitboard_updatedPieces |= 1uL << index;
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
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7), (index >> 3) - 1, pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (index - 8));
                        globalData.bitboard_updatedPieces |= 1uL << index;
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
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7) - 1, (index >> 3), pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (index - 1));
                        globalData.bitboard_updatedPieces |= 1uL << index;
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
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, (index & 7) + 1, (index >> 3), pieces[index], BoardUpdateMetadata.BoardUpdateType.Shift));
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (index + 1));
                        globalData.bitboard_updatedPieces |= 1uL << index;
                        pieces[index + 1] = pieces[index];
                        pieces[index] = 0;
                        processedSquares |= 1uL << index + 1;
                    }
                    break;
                case Square.SquareType.Promotion:
                    if (pteI.promotionType != PieceType.Null)
                    {
                        if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.White && ((index & 56) >> 3 <= 3))
                        {
                            break;
                        }
                        if (Piece.GetPieceAlignment(pieces[index]) == PieceAlignment.Black && ((index & 56) >> 3 > 3))
                        {
                            break;
                        }

                        globalData.bitboard_updatedPieces |= (1uL << (index));
                        pieces[index] = Piece.SetPieceType(pteI.promotionType, pieces[index]);

                        if ((pteI.piecePropertyB & PiecePropertyB.Giant) != 0)
                        {
                            PlaceGiant(pieces[index], index & 7, (index & 56) >> 3);
                        }

                        PieceTableEntry pteB = GlobalPieceManager.GetPieceTableEntry(pteI.promotionType);

                        if (boardUpdateMetadata != null)
                        {
                            boardUpdateMetadata.Add(new BoardUpdateMetadata(index & 7, index >> 3, pieces[index], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                        }

                        if (black)
                        {
                            blackPerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pteI.pieceValueX2);
                        }
                        else
                        {
                            whitePerPlayerInfo.pieceValueSumX2 += (short)(pteB.pieceValueX2 - pteI.pieceValueX2);
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

            PieceTableEntry pteH = globalData.GetPieceTableEntryFromCache(index, pieces[index]); //GlobalPieceManager.GetPieceTableEntry(pieces[index]);

            //only being a giant saves you from the hole
            if ((pteH.piecePropertyB & PiecePropertyB.Giant) == 0)
            {
                switch (Piece.GetPieceAlignment(pieces[index]))
                {
                    case PieceAlignment.White:
                        DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pteH, Piece.PieceAlignment.White, boardUpdateMetadata);
                        whitePerPlayerInfo.pieceValueSumX2 -= (pteH.pieceValueX2);
                        whitePerPlayerInfo.pieceCount--;
                        whitePerPlayerInfo.piecesLost++;
                        break;
                    case PieceAlignment.Black:
                        DeletePieceAtCoordinate(index & 7, (index & 56) >> 3, pteH, Piece.PieceAlignment.Black, boardUpdateMetadata);
                        blackPerPlayerInfo.pieceValueSumX2 -= (pteH.pieceValueX2);
                        blackPerPlayerInfo.pieceCount--;
                        blackPerPlayerInfo.piecesLost++;
                        break;
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
                    PieceTableEntry pteO = globalData.GetPieceTableEntryFromCache(subindex, pieces[subindex]); //GlobalPieceManager.GetPieceTableEntry(pieces[subindex]);
                    PieceTableEntry pteK = GlobalPieceManager.GetPieceTableEntry(PieceType.King);

                    blackPerPlayerInfo.pieceValueSumX2 += (short)(pteK.pieceValueX2 - pteO.pieceValueX2);

                    if (boardUpdateMetadata != null)
                    {
                        boardUpdateMetadata.Add(new BoardUpdateMetadata(subindex & 7, subindex >> 3, pieces[subindex], BoardUpdateMetadata.BoardUpdateType.TypeChange));
                    }

                    globalData.bitboard_updatedPieces |= (1uL << (subindex));
                    pieces[subindex] = Piece.SetPieceType(PieceType.King, pieces[subindex]);
                    break;
                }
            }
        }
    }

    public void ApplyGoalSquares(List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        bool goldExists = false;
        bool goldActive = true;

        for (int i = 0; i < 64; i++)
        {
            switch (globalData.squares[i].type)
            {
                case Square.SquareType.BronzeTreasure:
                    if (pieces[i] != 0 && Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
                    {
                        globalData.squares[i].type = Square.SquareType.Normal;
                        //todo: better architectured way of doing this
                        MainManager.Instance.playerData.coins += 2;
                    }
                    break;
                case Square.SquareType.SilverTreasure:
                    if (pieces[i] != 0 && Piece.GetPieceAlignment(pieces[i]) == PieceAlignment.White)
                    {
                        globalData.squares[i].type = Square.SquareType.Normal;
                        MainManager.Instance.playerData.coins += 4;
                    }
                    break;
                case Square.SquareType.GoldTreasure:
                    goldExists = true;
                    if (pieces[i] == 0 || Piece.GetPieceAlignment(pieces[i]) != PieceAlignment.White)
                    {
                        goldActive = false;
                    }
                    break;
            }
        }

        if (goldActive && goldExists)
        {
            for (int i = 0; i < 64; i++)
            {
                if (globalData.squares[i].type == Square.SquareType.GoldTreasure)
                {
                    globalData.squares[i].type = Square.SquareType.Normal;
                    MainManager.Instance.playerData.coins += 6;
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
        return IsConsumableMoveValid(ref b, move) && IsMoveLegal(ref b, move, false);
    }

    public static bool IsConsumableMoveValid(ref Board b, uint move)
    {
        (ConsumableMoveType cmt, int tx, int ty) = DecodeConsumableMove(move);
        //Debug.Log(cmt);

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[tx + (ty << 3)]));
        uint targetPiece = b.pieces[tx + (ty << 3)];

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
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.PocketRockslide:
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece == 0)
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.Horns:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Vengeful, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Torch:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Phoenix, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Ring:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Radiant, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Wings:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Winged, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Glass:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Spectral, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Bottle:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Immune, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Shield:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Shielded, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.Cap:
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || !Move.IsModifierCompatible(PieceModifier.Warped, pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.WarpBack:
                int wx = tx;
                int wy = 0;
                int dy = 1;
                int dx = 0;

                Piece.PieceAlignment pa = Piece.GetPieceAlignment(b.GetPieceAtCoordinate(tx, ty));
                switch (pa)
                {
                    case PieceAlignment.White:
                        wy = 0;
                        dy = 1;
                        dx = 0;
                        break;
                    case PieceAlignment.Black:
                        wy = 7;
                        dy = -1;
                        dx = 0;
                        break;
                    case PieceAlignment.Neutral:
                        wx = 0;
                        dy = 0;
                        dx = 1;
                        break;
                    case PieceAlignment.Crystal:
                        wx = 7;
                        dy = 0;
                        dx = -1;
                        break;
                }

                //try to search
                while (b.pieces[wx + wy * 8] != 0)
                {
                    wx += dx;
                    wy += dy;
                    if (wx == tx && wy == ty)
                    {
                        return false;
                    }
                }
                return true;
            case ConsumableMoveType.Grail:
                //it must have a promotion
                if (pte == null || Piece.GetPieceAlignment(targetPiece) != PieceAlignment.White || pte.promotionType == PieceType.Null)
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
            case ConsumableMoveType.SplashFreeze:
            case ConsumableMoveType.SplashPhantom:
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White)
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.SplashAir:
                //Need to check if any of the pushes are legal
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White && Move.PushLegal(ref b, tx + i, ty + j, Move.DeltaToDir(i, j)))
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.SplashVortex:
                //Need to check if any of the pushes are legal
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White && Move.PushLegal(ref b, tx + i, ty + j, Move.DeltaToDir(-i, -j)))
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }

                        //Range 2 pull
                        if (tx + (2 * i) < 0 || tx + (2 * i) > 7 || ty + (2 * j) < 0 || ty + (2 * j) > 7)
                        {
                            continue;
                        }
                        piece = b.pieces[(tx + (2 * i)) + ((ty + (2 * j)) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White && Move.PushLegal(ref b, tx + (2 * i), ty + (2 * j), Move.DeltaToDir(-i, -j)))
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.Fan:
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White && Move.PushLegal(ref b, tx + i, ty + j, Dir.Up))
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.MegaFan:
                for (int i = 0; i < 64; i++)
                {
                    uint piece = b.pieces[i];
                    if (piece != 0 && Piece.GetPieceAlignment(piece) != PieceAlignment.White && Move.PushLegal(ref b, i & 7, i >> 3, Dir.Up))
                    {
                        return true; //IsMoveLegal(ref b, move, false);
                    }
                }
                break;
            case ConsumableMoveType.SplashCure:
                //Don't let you waste it (so it only works if something nearby is splash curable
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) == PieceAlignment.White && Piece.GetPieceStatusEffect(piece) != PieceStatusEffect.None)
                        {
                            return true; //IsMoveLegal(ref b, move, false);
                        }
                    }
                }
                return false;
            case ConsumableMoveType.Bag:
                bool allyNear = false;
                for (int i = -1; i <= 1; i++)
                {
                    if (tx + i < 0 || tx + i > 7)
                    {
                        continue;
                    }
                    for (int j = -1; j <= 1; j++)
                    {
                        if (ty + j < 0 || ty + j > 7)
                        {
                            continue;
                        }

                        uint piece = b.pieces[(tx + i) + ((ty + j) << 3)];
                        if (piece != 0 && Piece.GetPieceAlignment(piece) == PieceAlignment.White)
                        {
                            allyNear = true;
                            break;
                        }
                    }
                    if (allyNear)
                    {
                        break;
                    }
                }

                if (!allyNear)
                {
                    return false;
                }
                if (pte == null || Piece.GetPieceAlignment(b.pieces[tx + (ty << 3)]) == PieceAlignment.White || pte.type == PieceType.King || Piece.IsPieceInvincible(b, b.pieces[tx + (ty << 3)], tx, ty, Piece.SetPieceType(PieceType.Rock, 0), tx, ty, SpecialType.Convert, GlobalPieceManager.GetPieceTableEntry(PieceType.Rock), pte))
                {
                    return false;
                }
                return true; //IsMoveLegal(ref b, move, false);
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
            giant = ((GlobalPieceManager.GetPieceTableEntry(pt).piecePropertyB & PiecePropertyB.Giant) != 0);

            //Spawn a piece at position

            if (giant)
            {
                PlaceGiant(Piece.SetPieceType(pt, Piece.SetPieceAlignment((PieceAlignment)(MainManager.BitFilter(move, 25, 26) << 30), 0)), Move.GetToX(move), Move.GetToY(move));
            }
            else
            {
                globalData.bitboard_updatedPieces |= (1uL << Move.GetToXYInt(move));
                pieces[Move.GetToXYInt(move)] = Piece.SetPieceType(pt, Piece.SetPieceAlignment((PieceAlignment)(MainManager.BitFilter(move, 25, 26) << 30), 0));
            }

            if (Piece.GetPieceAlignment(pieces[Move.GetToXYInt(move)]) == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.GetPieceTableEntry(pt).pieceValueX2;
                whitePerPlayerInfo.pieceCount++;
            }
            if (Piece.GetPieceAlignment(pieces[Move.GetToXYInt(move)]) == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 += GlobalPieceManager.GetPieceTableEntry(pt).pieceValueX2;
                blackPerPlayerInfo.pieceCount++;
            }

            return;
        }

        giant = ((GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).piecePropertyB & PiecePropertyB.Giant) != 0);
        if (Move.GetToY(move) == 15)
        {
            //Erase the piece

            if (Piece.GetPieceAlignment(pieces[Move.GetFromXYInt(move)]) == PieceAlignment.White)
            {
                whitePerPlayerInfo.pieceValueSumX2 -= GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).pieceValueX2;
                whitePerPlayerInfo.pieceCount--;
            }
            if (Piece.GetPieceAlignment(pieces[Move.GetFromXYInt(move)]) == PieceAlignment.Black)
            {
                blackPerPlayerInfo.pieceValueSumX2 -= GlobalPieceManager.GetPieceTableEntry(pieces[Move.GetFromXYInt(move)]).pieceValueX2;
                blackPerPlayerInfo.pieceCount--;
            }

            if (giant)
            {
                globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move)));
                globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move) + 1));
                globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move) + 8));
                globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move) + 9));
                pieces[Move.GetFromXYInt(move)] = 0;
                pieces[Move.GetFromXYInt(move) + 1] = 0;
                pieces[Move.GetFromXYInt(move) + 8] = 0;
                pieces[Move.GetFromXYInt(move) + 9] = 0;
            }
            else
            {
                globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move)));
                pieces[Move.GetFromXYInt(move)] = 0;
            }
            return;
        }

        //Execute the move

        globalData.bitboard_updatedPieces |= (1uL << (Move.GetFromXYInt(move)));
        globalData.bitboard_updatedPieces |= (1uL << (Move.GetToXYInt(move)));
        uint newPiece = pieces[Move.GetFromXYInt(move)];

        //swap the 2 places

        //Very straightforward
        uint oldPiece = pieces[(Move.GetFromX(move)) + ((Move.GetFromY(move) << 3))];
        SetPieceAtCoordinate(Move.GetFromX(move), Move.GetFromY(move), pieces[(Move.GetToX(move)) + ((Move.GetToY(move) << 3))]);
        SetPieceAtCoordinate(Move.GetToX(move), Move.GetToY(move), oldPiece);

        /*
        //Move the thing
        DeletePieceMovedFromCoordinate(Move.GetFromX(move), Move.GetFromY(move), GlobalPieceManager.GetPieceTableEntry(newPiece), Piece.GetPieceAlignment(newPiece));
        PlaceMovedPiece(newPiece, Move.GetToX(move), Move.GetToY(move), GlobalPieceManager.GetPieceTableEntry(newPiece), Piece.GetPieceAlignment(newPiece));
        */
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
            if ((GlobalPieceManager.GetPieceTableEntry(pt).piecePropertyB & PiecePropertyB.Giant) != 0)
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
        if ((GlobalPieceManager.GetPieceTableEntry(b.pieces[GetFromXYInt(move)]).piecePropertyB & PiecePropertyB.Giant) != 0)
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
            /*
            if (b.pieces[GetToXYInt(move)] != 0)
            {
                return false;
            }
            */

            //New: now you swap the 2 pieces (but target must not be giant)
            //Can probably assume 
            if (b.pieces[GetToXYInt(move)] != 0 && ((GlobalPieceManager.GetPieceTableEntry(b.pieces[GetToXYInt(move)]).piecePropertyB & PiecePropertyB.Giant) != 0))
            {
                return false;
            }
            return true;
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
        if ((globalData.enemyModifier & EnemyModifier.Hidden) != 0)
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
        if ((globalData.enemyModifier & EnemyModifier.Hidden) != 0)
        {
            blackKing = true;
        }

        int kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_king & globalData.bitboard_piecesBlack);
        uint target = 0;
        if (kingIndex != -1)
        {
            target = pieces[kingIndex];
            if (Piece.GetPieceType(target) == PieceType.King && Piece.GetPieceAlignment(target) == PieceAlignment.Black)
            {
                blackKing = true;
            }
        }
        kingIndex = MainManager.PopBitboardLSB1(globalData.bitboard_king & globalData.bitboard_piecesWhite);
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
        if ((globalData.enemyModifier & EnemyModifier.Hidden) != 0)
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
        MoveGenerator.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

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
        MoveGenerator.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

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
        MoveGenerator.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

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
        MoveGenerator.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, moveDict);

        //b.BoardPrint();

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

        if (!b.CheckForKings())
        {
            return 0;
        }

        if (depth <= 0)
        {
            return 1;
        }

        //Faster to pre populate the capacity
        //Because less resizes required?
        //Test is inconsistent for some reason though?
        List<uint> moves = new List<uint>(30);
        MoveGenerator.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        //debug
        /*
        List<uint> movesAlt = new List<uint>(30);
        MoveGenerator.GenerateMovesForPlayer(movesAlt, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, new Dictionary<uint, MoveMetadata>());

        if (moves.Count != movesAlt.Count)
        {
            Debug.Log(moves.Count + " actual vs expected " + movesAlt.Count);
            b.BoardPrint();
        }
        */

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


    public void BoardPrint()
    {
        string toPrint = "";

        for (int i = 7; i >= 0; i--)
        {
            for (int j = 0; j < 8; j++)
            {
                if (j != 0)
                {
                    toPrint += " ";
                }
                toPrint += Piece.ConvertToString(pieces[(i << 3) + j]);
            }
            toPrint += "\n";
        }

        Debug.Log(toPrint);
    }

    public static int GetConsumableCost(ConsumableMoveType cmt)
    {
        switch (cmt)
        {
            case ConsumableMoveType.PocketRock:
                return 1;
            case ConsumableMoveType.PocketRockslide:
                return 7;
            case ConsumableMoveType.PocketPawn:
                return 1;
            case ConsumableMoveType.PocketKnight:
                return 4;
            case ConsumableMoveType.Horns:
                return 4;
            case ConsumableMoveType.Torch:
                return 6;
            case ConsumableMoveType.Ring:
                return 4;
            case ConsumableMoveType.Wings:
                return 6;
            case ConsumableMoveType.Glass:
                return 6;
            case ConsumableMoveType.Bottle:
                return 3;
            case ConsumableMoveType.Shield:
                return 5;
            case ConsumableMoveType.Cap:
                return 3;
            case ConsumableMoveType.Grail:
                return 5;
            case ConsumableMoveType.WarpBack:
                return 2;
            case ConsumableMoveType.SplashFreeze:
                return 3;
            case ConsumableMoveType.SplashPhantom:
                return 2;
            case ConsumableMoveType.SplashCure:
                return 1;
            case ConsumableMoveType.SplashAir:
                return 1;
            case ConsumableMoveType.SplashVortex:
                return 1;
            case ConsumableMoveType.Fan:
                return 4;
            case ConsumableMoveType.MegaFan:
                return 6;
            case ConsumableMoveType.Bag:
                return 6;
        }
        return 5;
    }
    public static string GetConsumableName(ConsumableMoveType cmt)
    {
        switch (cmt)
        {
            case ConsumableMoveType.Horns:
            case ConsumableMoveType.Torch:
            case ConsumableMoveType.Ring:
            case ConsumableMoveType.Wings:
            case ConsumableMoveType.Glass:
            case ConsumableMoveType.Bottle:
            case ConsumableMoveType.Shield:
            case ConsumableMoveType.Cap:
            case ConsumableMoveType.Grail:
            case ConsumableMoveType.Fan:
            case ConsumableMoveType.Bag:
                return cmt.ToString();
            case ConsumableMoveType.PocketRock:
                return "Pocket Rock";
            case ConsumableMoveType.PocketRockslide:
                return "Pocket Rockslide";
            case ConsumableMoveType.PocketPawn:
                return "Pocket Pawn";
            case ConsumableMoveType.PocketKnight:
                return "Pocket Knight";
            case ConsumableMoveType.WarpBack:
                return "Warp Back";
            case ConsumableMoveType.SplashFreeze:
                return "Splash Freeze";
            case ConsumableMoveType.SplashPhantom:
                return "Splash Phantom";
            case ConsumableMoveType.SplashCure:
                return "Splash Cure";
            case ConsumableMoveType.SplashAir:
                return "Splash Wind";
            case ConsumableMoveType.SplashVortex:
                return "Splash Vortex";
            case ConsumableMoveType.MegaFan:
                return "Mega Fan";
        }
        return "";
    }
    public static string GetConsumableDescription(ConsumableMoveType cmt)
    {
        switch (cmt)
        {
            case ConsumableMoveType.PocketRock:
                return "Drop a Rock on the target empty square.";
            case ConsumableMoveType.PocketRockslide:
                return "Drop a Rock on the target empty square and all adjacent empty squares.";
            case ConsumableMoveType.PocketPawn:
                return "Drop an ally Pawn on the target empty square.";
            case ConsumableMoveType.PocketKnight:
                return "Drop an ally Knight on the target empty square.";
            case ConsumableMoveType.Horns:
                return "Imbue the target ally with Vengeful for the rest of the battle. (Vengeful: When captured, the capturer is destroyed if it is not a King.)";
            case ConsumableMoveType.Torch:
                return "Imbue the target ally with Phoenix for the rest of the battle. (Phoenix: When destroyed, respawn as far back as possible and lose this Modifier.)";
            case ConsumableMoveType.Ring:
                return "Imbue the target ally with Radiant for the rest of the battle. (Radiant: When this piece captures, spawn a Pawn as far back as possible.)";
            case ConsumableMoveType.Wings:
                return "Imbue the target ally with Winged for the rest of the battle. (Winged: Ignore the first obstacle but can't capture after leaping over the obstacle.)";
            case ConsumableMoveType.Glass:
                return "Imbue the target ally with Spectral for the rest of the battle. (Spectral: Ally pieces are not blocked by this piece.)";
            case ConsumableMoveType.Bottle:
                return "Imbue the target ally with Immune for the rest of the battle. (Immune: Unaffected by status effects and enchantments. Enemy pieces that are orthogonally adjacent can't capture.)";
            case ConsumableMoveType.Shield:
                return "Imbue the target ally with Shielded for the rest of the battle. (Shielded: Invincible, but degrades to Half Shielded if an enemy piece threatens it at the start of their turn. Half Shielded degrades to nothing after 1 turn.)";
            case ConsumableMoveType.Cap:
                return "Imbue the target ally with Warped for the rest of the battle. (Warped: Ally pieces can swap places with this piece if they can move onto it.)";
            case ConsumableMoveType.Grail:
                return "Promote the target pawnlike ally piece.";
            case ConsumableMoveType.WarpBack:
                return "Move the target piece as far back as possible.";
            case ConsumableMoveType.SplashFreeze:
                return "Imbue the target enemy and all adjacent enemies with Freeze for 3 turns.";
            case ConsumableMoveType.SplashPhantom:
                return "Imbue the target enemy and all adjacent enemies with Phantom for 3 turns.";
            case ConsumableMoveType.SplashCure:
                return "Cure the target ally and all adjacent allies of status effects.";
            case ConsumableMoveType.SplashAir:
                return "Push all adjacent enemies away 1 square.";
            case ConsumableMoveType.SplashVortex:
                return "Pull enemies towards the target square (Enemies from range 2 are pulled towards range 1.)";
            case ConsumableMoveType.Fan:
                return "Push the target enemy and all adjacent enemies back 1 square.";
            case ConsumableMoveType.MegaFan:
                return "Push all enemies back 1 square.";
            case ConsumableMoveType.Bag:
                return "Convert target enemy to be your piece. Only usable if the target is adjacent to one of your pieces.";
        }
        return "";
    }
    public static string GetPlayerModifierName(PlayerModifier pm)
    {
        switch (pm)
        {
            case PlayerModifier.Push:
            case PlayerModifier.Vortex:
            case PlayerModifier.Sprint:
            case PlayerModifier.Rough:
            case PlayerModifier.Defensive:
            case PlayerModifier.Recall:
            case PlayerModifier.Tempering:
            case PlayerModifier.Rockfall:
            case PlayerModifier.Promoter:
            case PlayerModifier.Seafaring:
            case PlayerModifier.Backdoor:
            case PlayerModifier.Mirror:
            case PlayerModifier.Forest:
            case PlayerModifier.Slippery:
                return pm.ToString();
            case PlayerModifier.NoKing:
                return "No King";
            case PlayerModifier.RelayKing:
                return "Relay King";
            case PlayerModifier.BronzeTreasure:
                return "Bronze Treasure";
            case PlayerModifier.SilverTreasure:
                return "Silver Treasure";
            case PlayerModifier.GoldenTreasure:
                return "Golden Treasure";
            case PlayerModifier.TimeBurst:
                return "Time Burst";
            case PlayerModifier.FinalVengeance:
                return "Final Vengeance";
            case PlayerModifier.PhoenixWing:
                return "Phoenix Wing";
            case PlayerModifier.FirstRadiant:
                return "First Radiant";
            case PlayerModifier.SideWings:
                return "Side Wings";
            case PlayerModifier.SpectralWall:
                return "Spectral Wall";
            case PlayerModifier.ImmunityZone:
                return "Immunity Zone";
            case PlayerModifier.WarpZone:
                return "Warp Zone";
            case PlayerModifier.ShieldZone:
                return "Shield Zone";
            case PlayerModifier.FlyingGeneral:
                return "Flying General";
        }
        return "";
    }
    public static string GetPlayerModifierDescription(PlayerModifier pm)
    {
        switch (pm)
        {
            case PlayerModifier.NoKing:
                return "King disappears on battle start. You can't be Checkmated.";
            case PlayerModifier.RelayKing:
                return "King relays its moves to adjacent allies.";
            case PlayerModifier.Slippery:
                return "Pieces you move slip 1 square when they move (if they land on normal squares).";
            case PlayerModifier.Push:
                return "Pieces you move push enemies away by 1 square.";
            case PlayerModifier.Vortex:
                return "Pieces you move pull enemies towards it 1 square.";
            case PlayerModifier.Sprint:
                return "For the first 3 turns, you get 2 moves per turn.";
            case PlayerModifier.Rough:
                return "Your pieces create rough squares orthogonally adjacent to them. (Rough squares force enemies to stop on them).";
            case PlayerModifier.Defensive:
                return "Your pieces can move backwards infinitely.";
            case PlayerModifier.Recall:
                return "Your pieces can teleport move or swap back to the home row.";
            case PlayerModifier.Tempering:
                return "Capture Only moves now allow movement as well.";
            case PlayerModifier.Rockfall:
                return "Spawn 16 rocks on the board.";
            case PlayerModifier.Promoter:
                return "Spawn 2 Promotion Squares, which promote your pieces on them on the enemy half of the board.";
            case PlayerModifier.BronzeTreasure:
                return "Spawn a Bronze Treasure square, which gives you $2 when you land on it.";
            case PlayerModifier.SilverTreasure:
                return "Spawn a Silver Treasure square, which gives you $4 when you land on it.";
            case PlayerModifier.GoldenTreasure:
                return "Spawn two Golden Treasure squares, which gives you $6 when you have pieces on all of them simultaneously.";
            case PlayerModifier.TimeBurst:
                return "Every 8 turns, you can move twice in one turn.";
            case PlayerModifier.FinalVengeance:
                return "When you have 6 pieces or less, your pieces will destroy the pieces that capture them.";
            case PlayerModifier.PhoenixWing:
                return "The first piece you lose is respawned as far back as possible.";
            case PlayerModifier.FirstRadiant:
                return "The first piece you capture with spawns a Pawn and becomes Radiant.";
            case PlayerModifier.SideWings:
                return "Pieces on the outer 4 files can fly over one obstacle (but can't capture after flying over the obstacle).";
            case PlayerModifier.SpectralWall:
                return "Your pieces on row 3 do not block movement of ally pieces.";
            case PlayerModifier.ImmunityZone:
                return "The center 4 squares on your front row will cure ally pieces of status effects when you move pieces there.";
            case PlayerModifier.WarpZone:
                return "Ally pieces in the center 4x4 allow you to move your pieces onto them to swap with them.";
            case PlayerModifier.ShieldZone:
                return "Ally pieces on b2, c2, f2 and g2 are Invincible to attackers further than 2 squares away from them.";
            case PlayerModifier.Seafaring:
                return "Pieces on the a file and h file can teleport move to the opposite side of the board.";
            case PlayerModifier.Backdoor:
                return "Pieces on b1, b8, c1, c8, f1, f8, g1 and g8 can teleport move to the vertical opposite side of the board.";
            case PlayerModifier.Mirror:
                return "Pieces can teleport move to the mirrored side of the board.";
            case PlayerModifier.Forest:
                return "Pieces can teleport move to any square with at least 4 allies adjacent to them.";
            case PlayerModifier.FlyingGeneral:
                return "Your King checks enemy Kings on the same file or rank as it with no obstacles in between (Can capture if the enemy has multiple Kings).";
        }
        return "";
    }
    public static int GetPlayerModifierCost(PlayerModifier pm)
    {
        switch (pm)
        {
            case PlayerModifier.NoKing:
                return 5;
            case PlayerModifier.RelayKing:
                return 5;
            case PlayerModifier.Slippery:
                return 6;
            case PlayerModifier.Push:
                return 6;
            case PlayerModifier.Vortex:
                return 6;
            case PlayerModifier.Sprint:
                return 6;
            case PlayerModifier.Rough:
                return 7;
            case PlayerModifier.Defensive:
                return 6;
            case PlayerModifier.Recall:
                return 6;
            case PlayerModifier.Tempering:
                return 7;
            case PlayerModifier.Rockfall:
                return 6;
            case PlayerModifier.Promoter:
                return 5;
            case PlayerModifier.BronzeTreasure:
                return 6;
            case PlayerModifier.SilverTreasure:
                return 8;
            case PlayerModifier.GoldenTreasure:
                return 10;
            case PlayerModifier.TimeBurst:
                return 4;
            case PlayerModifier.FinalVengeance:
                return 6;
            case PlayerModifier.PhoenixWing:
                return 8;
            case PlayerModifier.FirstRadiant:
                return 7;
            case PlayerModifier.SideWings:
                return 5;
            case PlayerModifier.SpectralWall:
                return 4;
            case PlayerModifier.ImmunityZone:
                return 4;
            case PlayerModifier.WarpZone:
                return 4;
            case PlayerModifier.ShieldZone:
                return 6;
            case PlayerModifier.Seafaring:
                return 3;
            case PlayerModifier.Backdoor:
                return 3;
            case PlayerModifier.Mirror:
                return 6;
            case PlayerModifier.Forest:
                return 7;
            case PlayerModifier.FlyingGeneral:
                return 3;
        }
        return 8;
    }
    public static string GetEnemyModifierDescription(EnemyModifier em)
    {
        switch (em)
        {
            case EnemyModifier.Blinking:
                return "Blinking: You must alternate moving to light and dark squares for every turn.";
            case EnemyModifier.Complacent:
                return "Complacent: You can't capture twice in to turns.";
            case EnemyModifier.Defensive:
                return "Defensive: Enemy pieces can move backwards infinitely.";
            case EnemyModifier.Envious:
                return "Envious: Enemy King copies the movement of your highest valued piece at the start.";
            case EnemyModifier.Fusion:
                return "Fusion: Enemy King copies the movement of allies adjacent to it.";
            case EnemyModifier.Greedy:
                return "Greedy: Until you lose 2 pieces, your enemy can convert pieces to their side instead of capturing or burning your pieces.";
            case EnemyModifier.Hidden:
                return "Hidden: No enemy Kings, Immune to Checkmate.";
            case EnemyModifier.Isolated:
                return "Isolated: You can't move pieces that are adjacent to enemies and no allies.";
            case EnemyModifier.Jester:
                return "Jester: Spawn 3 Jesters.";
            case EnemyModifier.Knave:
                return "Knave: Enemy pieces can move between the top and bottom of the board (wwithout capturing).";
            case EnemyModifier.Lustful:
                return "Lustful: Pieces on the same rank, file or diagonals as the enemy King can be moved by the enemy";
            case EnemyModifier.Mesmerizing:
                return "Mesmerizing: Every 2 turns, the piece you move is pulled forwards after you move.";
            case EnemyModifier.Numerous:
                return "Numerous: Spawn 2 extra Kings.";
            case EnemyModifier.Obelisk:
                return "Obelisk: The enemy King pulls allies alongside it as it moves.";
            case EnemyModifier.Prideful:
                return "Prideful: You can't capture if you have more pieces than your opponent.";
            case EnemyModifier.Queenly:
                return "Queenly: Spawn a Queen and a Princess.";
            case EnemyModifier.Rifter:
                return "Rifter: Every 2 turns, the piece you move is pulled towards the sides of the board after you move.";
            case EnemyModifier.Slothful:
                return "Slothful: Pieces on the enemy King's file are immobilized.";
            case EnemyModifier.Terror:
                return "Terror: You can't move pieces within 2 squares of the enemy King unless you are capturing.";
            case EnemyModifier.Unpredictable:
                return "Unpredictable: You can't move the same piece twice in two turns.";
            case EnemyModifier.Voracious:
                return "Voracious: For every 4 pieces lost, the enemy King gains 1 square of movement range.";
            case EnemyModifier.Wrathful:
                return "wrathful: Until you lose 2 pieces, when you capture, your piece gets destroyed.";
            case EnemyModifier.Xyloid:
                return "Xyloid: The enemy King can move orthogonally infinitely ignoring obstacles to squares adjacent to its allies.";
            case EnemyModifier.Youthful:
                return "Youthful: For the first 5 turns, the enemy gets 2 moves per turn.";
            case EnemyModifier.Zenith:
                return "Zenith: Overtaken Victory is not possible. Capturing the enemy King causes an enemy piece to transform into a new enemy King.";
        }
        return "";
    }
    //estimation
    public static float GetEnemyModifierPower(EnemyModifier em)
    {
        switch (em)
        {
            case EnemyModifier.Hidden:
                return 5;
            case EnemyModifier.Blinking:
                return 5;
            case EnemyModifier.Complacent:
                return 5;
            case EnemyModifier.Defensive:
                return 5;
            case EnemyModifier.Envious:
                return 12;
            case EnemyModifier.Fusion:
                return 12;
            case EnemyModifier.Greedy:
                return 8;
            case EnemyModifier.Isolated:
                return 6;
            case EnemyModifier.Jester:
                return 6;
            case EnemyModifier.Knave:
                return 4;
            case EnemyModifier.Lustful:
                return 8;
            case EnemyModifier.Mesmerizing:
                return 5;
            case EnemyModifier.Numerous:
                return 12;
            case EnemyModifier.Obelisk:
                return 5;
            case EnemyModifier.Prideful:
                return 5;
            case EnemyModifier.Queenly:
                return 12;
            case EnemyModifier.Rifter:
                return 5;
            case EnemyModifier.Slothful:
                return 8;
            case EnemyModifier.Terror:
                return 8;
            case EnemyModifier.Unpredictable:
                return 5;
            case EnemyModifier.Voracious:
                return 5;
            case EnemyModifier.Wrathful:
                return 12;
            case EnemyModifier.Xyloid:
                return 7;
            case EnemyModifier.Youthful:
                return 12;
            case EnemyModifier.Zenith:
                return 12;
        }
        return 6;
    }

    public static string GetSquareTypeDescription(Square.SquareType st)
    {
        switch (st)
        {
            case Square.SquareType.Hole:
                return "Hole: Pieces can't land here and are destroyed if pushed here.";
            case Square.SquareType.Fire:
                return "Fire: Pieces here are destroyed unless they were moved last turn.";
            case Square.SquareType.Water:
                return "Water: Pieces here can't capture.";
            case Square.SquareType.Rough:
                return "Rough: Pieces must stop here instead of continuing on.";
            case Square.SquareType.WindUp:
                return "Wind (N): Wind that pushes pieces northwards.";
            case Square.SquareType.WindDown:
                return "Wind (S): Wind that pushes pieces southwards.";
            case Square.SquareType.WindLeft:
                return "Wind (W): Wind that pushes pieces westwards.";
            case Square.SquareType.WindRight:
                return "Wind (E): Wind that pushes pieces eastwards.";
            case Square.SquareType.Slippery:
                return "Slippery: Pieces that move here slip one square (Does not apply to moves that aren't aligned to the grid or most teleports).";
            case Square.SquareType.Bouncy:
                return "Bouncy: Pieces that move here bounce back one square (Does not apply to moves that aren't aligned to the grid or most teleports).";
            case Square.SquareType.Bright:
                return "Bright: Pieces here can't be captured by pieces that are more than 2 squares away.";
            case Square.SquareType.Promotion:
                return "Promotion: If on the enemy side, your pieces promote when they reach here. If on your side, enemy pieces promote when they reach here.";
            case Square.SquareType.Cursed:
                return "Cursed: Pieces here are destroyed if they have no allies adjacent.";
            case Square.SquareType.CaptureOnly:
                return "Bloodlust: Pieces here can only capture.";
            case Square.SquareType.Frost:
                return "Frost: Pieces here are Frozen for 1 turn.";
            case Square.SquareType.BronzeTreasure:
                return "Bronze Treasure: If one of your pieces lands here, you get $2 and this becomes a Normal square.";
            case Square.SquareType.SilverTreasure:
                return "Silver Treasure: If one of your pieces lands here, you get $4 and this becomes a Normal square.";
            case Square.SquareType.GoldTreasure:
                return "Golden Treasure: If all Golden Treasure squares have your pieces on them, you get $6 and this becomes a Normal square.";
        }
        return "";
    }
}

[Serializable]
public struct Square
{
    public enum SquareType : byte
    {
        Hole = 255,     //(missing)
        Normal = 0,     //
        Fire,           //(fire symbol) If on them: piece is deleted unless it is the piece last moved
        Water,          //(wave symbol) If on them: piece can't capture
        Rough,          //(square symbol) Stops ray movers
        WindUp,         //squiggly wave
        WindDown,       //squiggly wave
        WindLeft,       //squiggly wave
        WindRight,      //squiggly wave
        Slippery,       //(asterisk crystal symbol) If you move onto it: get Pushed 1 square in Dir
        Bouncy,         //(circle symbol) Opposite of Ice: pulls you back 1 when you move on them
        Bright,         //(hexagon symbol) Immune
        Promotion,      //(3 point crown symbol) Promote early
        Cursed,         //(hollow circle) If not adjacent to allies: get destroyed
        CaptureOnly,    //(^ symbol) If on them: piece can only leave by capturing
        Frost,    //Whatever moves onto them gets applied stun for 1 turn

        //Special objectives
        BronzeTreasure, //($ symbol) Spawns on rank 6
        SilverTreasure, //($$ symbol) Spawns on back rank
        GoldTreasure,   //($$$ symbol) Spawns in multiples (rank 6, 7, 8?), must occupy all to get treasure
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
    
    //This is kind of a bit flags thing so you can add perpendicular directions together
    public enum Dir : int //sbyte
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

    public enum SpecialType : int //byte
    {
        Null = -1,  //invalid
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
        CoastMove,          //only hits edges of board
        ShadowMove,         //move to mirror

        AllyAbility,
        ImbueModifier,
        //ImbueWinged,
        ImbuePromote,
        ChargeApplyModifier,
        RangedPullAllyOnly,
        RangedPushAllyOnly,

        InflictFreeze,
        InflictFreezeCaptureOnly,
        Inflict,
        InflictCaptureOnly,
        InflictShift,
        InflictShiftCaptureOnly,

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

        AimEnemy,
        AimAny,
        AimOccupied,

        AmoebaCombine,

        MorphRabbit,
        ConvertRabbit,

        KingAttack,     //capture only for king (i.e. only checks enemy kings)
    }

    //
    public enum ConsumableMoveType
    {
        None,
        PocketRock,
        PocketRockslide,    //3x3 rocks (but only in empty places)
        PocketPawn,
        PocketKnight,
        Horns,  //vengeful imbuer
        Torch,  //phoenix imbuer
        Ring,   //golden imbuer
        Wings,  //winged imbuer
        Glass,  //spectral imbuer
        Bottle, //immune imbuer
        Shield, //shielded imbuer
        Cap,   //warped imbuer  (jester hat)
        Grail,  //Promote imbuer
        WarpBack,
        SplashFreeze,
        SplashPhantom,
        SplashCure,
        SplashAir,  //Air blast bomb (enemy only)
        SplashVortex, //Pulls 2 away to be 1 away (and tries to pull 1 away to the center square)
        Fan,    //Pushes 3x3 area of enemies back
        MegaFan,    //Pushes every enemy back
        Bag,    //Convert an enemy piece, must be adjacent to one of yours and not a King
    }

    public static bool IsConsumableMove(uint move)
    {
        /*
        if (Move.GetFromX(move) > 7)
        {
            return true;
        }
        if (Move.GetFromY(move) > 7)
        {
            return true;
        }
        */

        return (move & 0x88) != 0;

        //return false;
    }
    public static (ConsumableMoveType, int, int) DecodeConsumableMove(uint move)
    {
        int fx = Move.GetFromX(move) & 7;
        int fy = Move.GetFromY(move) & 7;

        return ((ConsumableMoveType)(fx + (fy << 3)), Move.GetToX(move), (Move.GetToY(move)));
    }
    public static uint EncodeConsumableMove(ConsumableMoveType cmt, int tx, int ty)
    {
        int fx = 8 + ((int)cmt & 7);
        int fy = 8 + ((int)cmt >> 3);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFromX(uint moveInfo)
    {
        return (int)(0xf & (moveInfo));
        //return (byte)(MainManager.BitFilter(moveInfo, 0, 3));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetFromX(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 0, 3);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFromY(uint moveInfo)
    {
        return (int)((0xf0 & (moveInfo)) >> 4);
        //return (byte)(MainManager.BitFilter(moveInfo, 4, 7));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetFromY(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 4, 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetToX(uint moveInfo)
    {
        return (int)((0xf00 & (moveInfo)) >> 8);
        //return (byte)(MainManager.BitFilter(moveInfo, 8, 11));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetToX(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 8, 11);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetToY(uint moveInfo)
    {
        return (int)((0xf000 & (moveInfo)) >> 12);
        //return (byte)(MainManager.BitFilter(moveInfo, 12, 15));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetToY(byte toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 12, 15);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetToFrom(uint moveInfo)
    {
        return (uint)((0xffff & (moveInfo)));
        //return (ushort)(MainManager.BitFilter(moveInfo, 0, 15));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetToFrom(ushort toFrom, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)toFrom, 0, 15);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetToXYInt(uint moveInfo)
    {
        return GetToX(moveInfo) + (GetToY(moveInfo) << 3);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFromXYInt(uint moveInfo)
    {
        return GetFromX(moveInfo) + (GetFromY(moveInfo) << 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dir GetDir(uint moveInfo)
    {
        return (Dir)((0xff0000 & (moveInfo)) >> 16);
        //return (Dir)(MainManager.BitFilter(moveInfo, 16, 23));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetDir(Dir dir, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)dir, 16, 23);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpecialType GetSpecialType(uint moveInfo)
    {
        return (SpecialType)((0xff000000 & (moveInfo)) >> 24);
        //return (SpecialType)(MainManager.BitFilter(moveInfo, 24, 31));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetSpecialType(SpecialType specialType, uint moveInfo)
    {
        return MainManager.BitFilterSet(moveInfo, (uint)specialType, 24, 31);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RemoveNonLocation(uint moveInfo)
    {
        return GetToFrom(moveInfo);
    }

    public static uint MakeSetupCreateMove(Piece.PieceType type, Piece.PieceAlignment pa, byte toX, byte toY)
    {
        uint m = Move.PackMove((byte)15, (byte)15, toX, toY, 0, 0);
        m = MainManager.BitFilterSet(m, (uint)type, 16, 24);
        m = MainManager.BitFilterSet(m, (uint)pa >> 30, 25, 26);
        //Debug.Log(MainManager.BitFilter(m, 16, 24));
        return m;
    }

    //not sure if C# actually does the inlining because this looks like a lot of instructions?
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackMove(int fromX, int fromY, int toX, int toY, Dir dir, SpecialType specialType)
    {
        //uint output = 0;

        /*
        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);
        */

        /*
        output += (uint)((fromX));
        output += (uint)(((fromY)) << 4);
        output += (uint)(((toX)) << 8);
        output += (uint)(((toY)) << 12);
        output += (((uint)dir)<< 16);
        output += ((uint)specialType << 24);

        return output;
        */
        return (uint)(fromX) + (uint)(((fromY)) << 4) + (uint)(((toX)) << 8) + (uint)(((toY)) << 12) + (((uint)dir) << 16) + ((uint)specialType << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY, Dir dir, SpecialType specialType)
    {
        //uint output = 0;

        /*
        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);
        */

        /*
        output += (uint)((fromX));
        output += (uint)(((fromY)) << 4);
        output += (uint)(((toX)) << 8);
        output += (uint)(((toY)) << 12);
        output += (((uint)dir)<< 16);
        output += ((uint)specialType << 24);

        return output;
        */
        return (uint)(fromX) + (uint)(((fromY)) << 4) + (uint)(((toX)) << 8) + (uint)(((toY)) << 12) + (((uint)dir) << 16) + ((uint)specialType << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY, SpecialType specialType)
    {
        /*
        uint output = 0;
        */

        /*
        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        //output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);
        */

        /*
        output += (uint)((fromX));
        output += (uint)(((fromY)) << 4);
        output += (uint)(((toX)) << 8);
        output += (uint)(((toY)) << 12);
        //output += (((uint)dir) << 16);
        output += ((uint)specialType << 24);
        */

        //return output;
        return (uint)(fromX) + (uint)(((fromY)) << 4) + (uint)(((toX)) << 8) + (uint)(((toY)) << 12) + ((uint)specialType << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackMove(int fromX, int fromY, int toX, int toY)
    {
        //uint output = 0;

        /*
        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        output += (((uint)dir & 0x000000ff) << 16);
        output += ((uint)specialType << 24);
        */

        /*
        output += (uint)((fromX));
        output += (uint)(((fromY)) << 4);
        output += (uint)(((toX)) << 8);
        output += (uint)(((toY)) << 12);
        output += (((uint)dir)<< 16);
        output += ((uint)specialType << 24);

        return output;
        */
        return (uint)(fromX) + (uint)(((fromY)) << 4) + (uint)(((toX)) << 8) + (uint)(((toY)) << 12);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint PackMove(byte fromX, byte fromY, byte toX, byte toY)
    {
        //uint output = 0;

        /*
        output += (uint)((fromX) & (0x0000000f));
        output += (uint)(((fromY) & (0x0000000f)) << 4);
        output += (uint)(((toX) & (0x0000000f)) << 8);
        output += (uint)(((toY) & (0x0000000f)) << 12);
        //output += (((uint)dir & 0x000000ff) << 16);
        //output += ((uint)specialType << 24);
        */

        /*
        output += (uint)((fromX));
        output += (uint)(((fromY)) << 4);
        output += (uint)(((toX)) << 8);
        output += (uint)(((toY)) << 12);
        //output += (((uint)dir) << 16);
        //output += ((uint)specialType << 24);

        return output;
        */

        return (uint)(fromX) + (uint)(((fromY)) << 4) + (uint)(((toX)) << 8) + (uint)(((toY)) << 12);
    }

    //does this even work?
    //ehh probably, tuples don't need a method to be returned
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) DirToDelta(Dir d)
    {
        byte di = (byte)d;

        //Debug.Log(d + " is " + (((di & 1) - ((di & 2) >> 1)), (((di & 4) >> 2) - ((di & 8) >> 3))));

        return (((di & 1) - ((di & 2) >> 1)), (((di & 4) >> 2) - ((di & 8) >> 3)));
        //int dx = 0;
        //int dy = 0;
        //return (dx, dy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) DeltaToUnitDelta(int dx, int dy)
    {
        return ((dx > 0 ? 1 : (dx < 0 ? -1 : 0)), (dy > 0 ? 1 : (dy < 0 ? -1 : 0)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static Dir DeltaToDir(int dx, int dy)
    {
        //if exactly aligned: dir set
        if (dx == 0)
        {
            if (dy > 0)
            {
                return Dir.Up;
            }
            else if (dy < 0)
            {
                return Dir.Down;
            }
            return Dir.Null;
        }
        if (dy == 0)
        {
            if (dx > 0)
            {
                return Dir.Right;
            }
            else if (dx < 0)
            {
                return Dir.Left;
            }
        }
        if (dx == dy)
        {
            if (dx > 0)
            {
                return Dir.UpRight;
            }
            else if (dx < 0)
            {
                return Dir.DownLeft;
            }
        }
        if (dx == -dy)
        {
            if (dx > 0)
            {
                return Dir.DownRight;
            }
            else if (dx < 0)
            {
                return Dir.UpLeft;
            }
        }
        return Dir.Null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dir DeltaToDirSoft(int dx, int dy)
    {
        //if exactly aligned: dir set
        if (dx == 0)
        {
            if (dy > 0)
            {
                return Dir.Up;
            }
            else if (dy < 0)
            {
                return Dir.Down;
            }
            return Dir.Null;
        } else if (dx > 0)
        {
            if (dy > 0)
            {
                return Dir.UpRight;
            } else if (dy == 0)
            {
                return Dir.Right;
            } else
            {
                return Dir.DownRight;
            }
        } else
        {
            if (dy > 0)
            {
                return Dir.UpLeft;
            }
            else if (dy == 0)
            {
                return Dir.Left;
            }
            else
            {
                return Dir.DownLeft;
            }
        }
    }

    //may be too big to inline?
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            default:
                return Dir.Null;
        }
    }

    public static bool PushLegal(ref Board b, int x, int y, Dir dir)
    {
        if (dir == Dir.Null)
        {
            return false;
        }
        (int tempX, int tempY) = (DirToDelta(dir));
        tempX += x;
        tempY += y;

        //Target must be empty and legal
        if ((((tempX | tempY) & -8) != 0))
        {
            return false;
        }

        return b.pieces[tempX + (tempY << 3)] == 0;
    }
    public static bool PullLegal(ref Board b, int x, int y, Dir dir)
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
        if ((((tempX | tempY) & -8) != 0))
        {
            return false;
        }

        return b.pieces[tempX + (tempY << 3)] == 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SpecialMoveCanMoveOntoAllyGeneral(SpecialType st)
    {
        switch (st)
        {
            case SpecialType.AimOccupied:
            case SpecialType.AimAny:
            case SpecialType.FireCapturePush:
            case SpecialType.PushMove:
            case SpecialType.RangedPush:
            case SpecialType.RangedPushAllyOnly:
            case SpecialType.RangedPull:
            case SpecialType.RangedPullAllyOnly:
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
            case SpecialType.AllySwap:
            case SpecialType.AnyoneSwap:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:
            case SpecialType.AmoebaCombine:
            case SpecialType.ImbueModifier:
            case SpecialType.ImbuePromote:
            case SpecialType.ChargeApplyModifier:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
            case SpecialType.TeleportMirror:
            case SpecialType.CarryAlly:
            case SpecialType.MorphRabbit:
                return true;
            default:
                return false;
        }
    }

    //This requires all these extra arguments because some move types need extra checks
    //The enemy capture version uses the check invincible thing

    public static bool SpecialMoveCanMoveOntoAlly(SpecialType st, ref Board b, int x, int y, int tx, int ty, Dir dir)
    {
        //optimize stuff by blacklisting every non (move onto ally) move early?
        switch (st)
        {
            case SpecialType.Normal:
            case SpecialType.MoveOnly:
            case SpecialType.CaptureOnly:
            case SpecialType.FlyingMoveOnly:
            case SpecialType.ChargeMove:
            case SpecialType.ChargeMoveReset:
            case SpecialType.SelfMove:
            case SpecialType.Castling:
            case SpecialType.Convert:
            case SpecialType.ConvertCaptureOnly:
            case SpecialType.ConvertPawn:
            case SpecialType.Spawn:
            case SpecialType.FireCapture:
            case SpecialType.FireCaptureOnly:
            case SpecialType.LongLeaper:
            case SpecialType.LongLeaperCaptureOnly:
            case SpecialType.PullMove:
            case SpecialType.AdvancerPush:
            case SpecialType.Advancer:
            case SpecialType.Withdrawer:
            case SpecialType.AdvancerWithdrawer:
            case SpecialType.WrathCapturer:
            case SpecialType.FlankingCapturer:
            case SpecialType.PoisonFlankingAdvancer:
            case SpecialType.MorphIntoTarget:
            case SpecialType.SlipMove:
            case SpecialType.PlantMove:
            case SpecialType.GliderMove:
            case SpecialType.CoastMove:
            case SpecialType.ShadowMove:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
            case SpecialType.EnemyAbility:
            case SpecialType.EmptyAbility:
            case SpecialType.AimEnemy:
            case SpecialType.ConvertRabbit:
            case SpecialType.KingAttack:
                return false;
            case SpecialType.AimOccupied:   //whitelist instead :)
            case SpecialType.AimAny:
                return true;
        }


        PieceTableEntry opte = b.globalData.GetPieceTableEntryFromCache((x + (y << 3)), b.GetPieceAtCoordinate(x, y)); //GlobalPieceManager.GetPieceTableEntry(b.GetPieceAtCoordinate(x, y));

        //Whatever the real move is you can premove it no matter what
        /*
        if (st == SpecialType.AimAny || st == SpecialType.AimOccupied)
        {
            return true;
        }
        */

        //I'll just ban this unconditionally
        PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache((tx + (ty << 3)), b.GetPieceAtCoordinate(tx, ty)); //GlobalPieceManager.GetPieceTableEntry(b.GetPieceAtCoordinate(tx, ty));
        if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
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
                return PushLegal(ref b, tx, ty, dir);
            case SpecialType.RangedPull:
            case SpecialType.RangedPullAllyOnly:
                return PullLegal(ref b, tx, ty, dir);
            case SpecialType.ConsumeAllies:
            case SpecialType.ConsumeAlliesCaptureOnly:
                return (Piece.GetPieceType(b.GetPieceAtCoordinate(tx, ty)) != PieceType.King);
            case SpecialType.AllySwap:
            case SpecialType.AnyoneSwap:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:
                return true;
            case SpecialType.AmoebaCombine:
                return (pte.piecePropertyB & PiecePropertyB.Amoeba) != 0 && pte.type != PieceType.AmoebaCitadel;
            case SpecialType.ImbueModifier:
                if (Piece.GetPieceModifier(b.pieces[tx + (ty << 3)]) != PieceModifier.None)
                {
                    return false;
                }
                switch (opte.type)
                {
                    case PieceType.HornSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Vengeful, pte);
                    case PieceType.TorchSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Phoenix, pte);
                    case PieceType.RingSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Radiant, pte);
                    case PieceType.FeatherSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Winged, pte);
                    case PieceType.GlassSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Spectral, pte);
                    case PieceType.BottleSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Immune, pte);
                    case PieceType.CapSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Warped, pte);
                    case PieceType.ShieldSpirit:
                        return Move.IsModifierCompatible(PieceModifier.Shielded, pte);
                }
                return true;
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
            case SpecialType.MorphRabbit:
                return (pte.piecePropertyB & PiecePropertyB.MorphImmune) == 0;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SpecialMoveCanMoveOntoEnemyGeneral(SpecialType st)
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
            case SpecialType.AnyoneSwap:
            case SpecialType.MorphIntoTarget:
            case SpecialType.EnemyAbility:
            case SpecialType.PassiveAbility:
            case SpecialType.CoastMove:
            case SpecialType.PlantMove:
            case SpecialType.ShadowMove:
            case SpecialType.WrathCapturer:
            case SpecialType.MorphRabbit:
            case SpecialType.KingAttack:
            case SpecialType.ConvertRabbit:
            case SpecialType.ConvertPawn:
            case SpecialType.RangedPull:
            case SpecialType.RangedPush:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.AimOccupied:
            case SpecialType.AimEnemy:
            case SpecialType.AimAny:
                return true;
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            case SpecialType.CoastMove:
            case SpecialType.PlantMove:
            case SpecialType.ShadowMove:
            case SpecialType.WrathCapturer:
            case SpecialType.MorphRabbit:
            case SpecialType.KingAttack:
                return true;
            case SpecialType.ConvertRabbit:
                return b.globalData.GetPieceTableEntryFromCache(x + (y << 3), b.pieces[x + (y << 3)]).pieceClass == PieceClass.Rabbit;
            case SpecialType.ConvertPawn:
                return b.globalData.GetPieceTableEntryFromCache(x + (y << 3), b.pieces[x + (y << 3)]).promotionType != 0;
            case SpecialType.RangedPull:
                return PullLegal(ref b, x, y, dir);
            case SpecialType.RangedPush:
                return PushLegal(ref b, x, y, dir);
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.AimOccupied:
            case SpecialType.AimEnemy:
            case SpecialType.AimAny:
                return true;
            default:
                return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.EnemyAbility:
            case SpecialType.AllyAbility:
            case SpecialType.PassiveAbility:    //since this only targets allies or enemies I don't want to clog up the bitboard with extraneous info
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
            case SpecialType.AimOccupied:
            case SpecialType.AimEnemy:
            case SpecialType.AmoebaCombine:
            case SpecialType.ConvertRabbit:
            case SpecialType.MorphRabbit:
            case SpecialType.KingAttack:
                return true;
            default:
                return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.AimEnemy:
            case SpecialType.ConvertRabbit:
            case SpecialType.MorphRabbit:
            //case SpecialType.KingAttack:
                return true;
            default:
                return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
            case SpecialType.AimOccupied:
            case SpecialType.AimEnemy:
            case SpecialType.AimAny:
            case SpecialType.ConvertRabbit:
            case SpecialType.MorphRabbit:
                return true;
            default:
                return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            //case SpecialType.AimEnemy:
            case SpecialType.AimOccupied:
            case SpecialType.AimAny:
                return true;
            default:
                return false;
        }
    }

    //This is mostly for things that are weird and not captures (and not normal passive moves either)
    //Weird capturing types are not marked because they aren't very confusing mostly
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            case SpecialType.ImbueModifier:
            case SpecialType.ImbuePromote:
            case SpecialType.Castling:
            case SpecialType.ChargeMove:        //Also here because this has special consequences
            case SpecialType.ChargeMoveReset:
            case SpecialType.Inflict:
            case SpecialType.InflictCaptureOnly:
            case SpecialType.InflictFreeze:
            case SpecialType.InflictFreezeCaptureOnly:
            case SpecialType.InflictShift:
            case SpecialType.InflictShiftCaptureOnly:
            case SpecialType.ChargeApplyModifier:
            case SpecialType.CarryAlly:
            case SpecialType.DepositAlly:
            case SpecialType.DepositAllyPlantMove:
            case SpecialType.TeleportMirror:
            case SpecialType.TeleportOpposite:
            case SpecialType.TeleportRecall:
            case SpecialType.AimEnemy:
            case SpecialType.AimOccupied:
            case SpecialType.AimAny:
            case SpecialType.AmoebaCombine:
            case SpecialType.MorphRabbit:
            //case SpecialType.KingAttack:
                return true;
            default:
                return false;
        }
    }

    public static string GetSpecialTypeDescription(SpecialType st, PieceTableEntry pte)
    {
        //todo: text file
        switch (st)
        {
            case SpecialType.Normal:
                return "Move or Capture.";
            case SpecialType.MoveOnly:
                return "Move only.";
            case SpecialType.CaptureOnly:
                return "Capture only.";
            case SpecialType.FlyingMoveOnly:
                return "Move only.";
            case SpecialType.ConsumeAllies:
                return "Move or Capture. Can capture allies to gain charges.";
            case SpecialType.ConsumeAlliesCaptureOnly:
                return "Capture only. Can capture allies.";
            case SpecialType.ChargeMove:
                return "Move or Capture. Only possible if charged.";
            case SpecialType.ChargeMoveReset:
                return "Move or Capture. Only possible if charged. Resets charge to 0 when used.";
            case SpecialType.SelfMove:
                return "Stay in place";
            case SpecialType.Castling:
                return "Move towards an ally non-pawn piece beyond the square moved to, moving that ally behind you. Only usable once per battle.";
            case SpecialType.Convert:
                return "(Enchant) Move or Convert an enemy piece to your side.";
            case SpecialType.ConvertCaptureOnly:
                return "(Enchant) Convert an enemy piece to your side.";
            case SpecialType.ConvertPawn:
                return "Move or Convert an enemy pawn to your side. Against non pawns, this is a capture.";
            case SpecialType.Spawn:
                switch (pte.type)
                {
                    case PieceType.Gemini:
                        return "Split off a Gemini Twin (Become a Gemini Twin).";
                    case PieceType.Triknight:
                        return "Split off up to 2 Knights (Second Knight spawns on the opposite side) (Become a Knight).";
                    case PieceType.Tribishop:
                        return "Split off up to 2 Bishops (Second Bishop spawns on the opposite side) (Become a Bishop).";
                    case PieceType.Birook:
                        return "Split off a Rook (Become a Rook).";
                    case PieceType.TrojanHorse:
                        return "Spawn up to 3 pawns behind you and then disappear.";
                    case PieceType.QueenLeech:
                        return "(Charge) Spawn up to 2 Leeches (Second Leech spawns on the opposite side).";
                    case PieceType.AmoebaCitadel:
                        return "Split off an Amoeba Archbishop (Become an Amoeba Archbishop).";
                    case PieceType.AmoebaGryphon:
                        return "Split off an Amoeba Knight (Become an Amoeba Archbishop).";
                    case PieceType.AmoebaRaven:
                        return "Split off an Amoeba Knight (Become an Amoeba Knight).";
                    case PieceType.AmoebaArchbishop:
                        return "Split off an Amoeba Pawn (Become an Amoeba Knight).";
                    case PieceType.AmoebaKnight:
                        return "Split off an Amoeba Pawn (Become an Amoeba Pawn).";
                }
                return "Spawn a piece on this square.";
            case SpecialType.FireCapture:
                return "Move or Burn an enemy piece.";
            case SpecialType.FireCaptureOnly:
                return "Burn an enemy piece.";
            case SpecialType.LongLeaper:
                return "(Ignores Obstacles)(Indirect Capture) Leap over pieces, destroying all enemies between the start and endpoint.";
            case SpecialType.LongLeaperCaptureOnly:
                return "(Must Leap Over Enemy)(Indirect Capture) Leap over pieces, destroying all enemies between the start and endpoint.";
            case SpecialType.FireCapturePush:
                return "Move, Burn an enemy piece, or Push an ally piece.";
            case SpecialType.PullMove:
                return "Move or Capture, pulling an ally piece behind you.";
            case SpecialType.PushMove:
                return "Move, Capture, or Push an ally piece.";
            case SpecialType.AdvancerPush:
                return "(Indirect Capture) Move or Push an ally piece. Enemies 1 square beyond the square moved to are destroyed.";
            case SpecialType.Advancer:
                return "(Indirect Capture) Move only. Enemies 1 square beyond the square moved to are destroyed.";
            case SpecialType.Withdrawer:
                return "(Indirect Capture) Move only. Enemies 1 square behind the starting square are destroyed.";
            case SpecialType.AdvancerWithdrawer:
                return "(Indirect Capture) Move only. Enemies 1 square behind the starting square or 1 square beyond the square moved to are destroyed.";
            case SpecialType.WrathCapturer:
                return "(Indirect Capture) Move or Capture. Enemies 1 square beyond the square moved to are also destroyed.";
            case SpecialType.FlankingCapturer:
                return "(Indirect Capture) Move only. Enemies adjacent to both the starting and ending squares are destroyed.";
            case SpecialType.PoisonFlankingAdvancer:
                return "(Indirect Capture) Move only. Enemies adjacent to both the starting and ending squares are destroyed. Enemies 1 square beyond the square moved to are Poisoned for 3 turns.";
            case SpecialType.AllySwap:
                return "Move or Swap with an ally (non Shift Immune) piece.";
            case SpecialType.AnyoneSwap:
                return "Move or Swap with any (non Shift Immune) piece.";
            case SpecialType.MorphIntoTarget:
                return "Move or Transform into the target piece.";
            case SpecialType.SlipMove:
                return "Move to a square adjacent to enemy pieces.";
            case SpecialType.PlantMove:
                return "(Ignores Obstacles) Move or Capture underground to a square adjacent to ally pieces.";
            case SpecialType.GliderMove:
                return "Move to a square not adjacent to enemy pieces.";
            case SpecialType.CoastMove:
                return "Move or Capture to a square on the 4 edges of the board.";
            case SpecialType.ShadowMove:
                return "Move or Capture to a square that has a piece on it on the mirrored square";
            case SpecialType.AllyAbility:
                return "An ability that affects allies";
            case SpecialType.ImbueModifier:
                switch (pte.type)
                {
                    case PieceType.HornSpirit:
                        return "Imbue the target with Vengeful. (Vengeful: When captured, the capturer is destroyed if it is not a King.)";
                    case PieceType.TorchSpirit:
                        return "Imbue the target with Phoenix. (Phoenix: When destroyed, respawn as far back as possible and lose this Modifier.)";
                    case PieceType.RingSpirit:
                        return "Imbue the target with Radiant. (Radiant: When this piece captures, spawn a Pawn as far back as possible.)";
                    case PieceType.FeatherSpirit:
                        return "Imbue the target with Winged. (Winged: Ignore the first obstacle but can't capture after leaping over the obstacle.)";
                    case PieceType.GlassSpirit:
                        return "Imbue the target with Spectral. (Spectral: Ally pieces are not blocked by this piece.)";
                    case PieceType.BottleSpirit:
                        return "Imbue the target with Immune. (Immune: Unaffected by status effects and enchantments. Enemy pieces that are orthogonally adjacent can't capture.)";
                    case PieceType.CapSpirit:
                        return "Imbue the target with Warped. (Warped: Ally pieces can swap places with this piece if they can move onto it.)";
                    case PieceType.ShieldSpirit:
                        return "Imbue the target with Shielded. (Shielded: Invincible, but degrades to Half Shielded if an enemy piece threatens it at the start of their turn. Half Shielded degrades to nothing after 1 turn.)";
                }
                //todo: this also needs piece specifics
                return "Imbue a modifier, destroying this piece";
            case SpecialType.ImbuePromote:
                return "Promote a promotable piece, destroying this piece";
            case SpecialType.ChargeApplyModifier:
                switch (pte.type)
                {
                    case PieceType.DivineArtisan:
                    case PieceType.DivineApprentice:
                        return "Use one charge to apply Shielded to the target piece. (Shielded: Invincible, but degrades to Half Shielded if an enemy piece threatens it at the start of their turn. Half Shielded degrades to nothing after 1 turn.)";
                }
                //todo: this also needs piece specifics
                return "Use one charge to apply a modifier to the target piece.";
            case SpecialType.RangedPullAllyOnly:
                switch (pte.type)
                {
                    case PieceType.ArcanaPriestess:
                        return "Pull an ally piece as far as possible.";
                }
                //priestess needs a special description
                return "Pull an ally piece one square.";
            case SpecialType.RangedPushAllyOnly:
                switch (pte.type)
                {
                    case PieceType.ArcanaPriestess:
                        return "Push an ally piece as far as possible.";
                }
                //priestess needs a special description
                return "Push an ally piece one square.";
            case SpecialType.InflictFreeze:
                return "Move or Inflict Freeze for 3 turns on enemy pieces. (Freeze: Piece can't move.)";
            case SpecialType.InflictFreezeCaptureOnly:
                return "Inflict Freeze for 3 turns on enemy pieces. (Freeze: Piece can't move.)";
            case SpecialType.Inflict:
                switch (pte.type)
                {
                    case PieceType.Phantom:
                        return "Move or Inflict Ghostly for 3 turns on enemy pieces. (Ghostly: Enemy pieces are not blocked by this piece.)";
                    case PieceType.SparkMage:
                        return "Move or Inflict Sparked for 2 turns on enemy pieces. (Sparked: Piece will be destroyed if they do not move before the effect expires.)";
                    case PieceType.SplashMage:
                        return "Move or Inflict Soaked for 3 turns on enemy pieces. (Soaked: Piece can't capture.)";
                    default:
                        return "Move or Inflict Poisoned for 3 turns on enemy pieces. (Poisoned: Piece will be destroyed when the effect expires.)";
                }
            case SpecialType.InflictCaptureOnly:
                switch (pte.type)
                {
                    case PieceType.Phantom:
                        return "Inflict Ghostly for 3 turns on enemy pieces. (Ghostly: Enemy pieces are not blocked by this piece.)";
                    case PieceType.SparkMage:
                        return "Inflict Sparked for 2 turns on enemy pieces. (Sparked: Piece will be destroyed if they do not move before the effect expires.)";
                    case PieceType.SplashMage:
                        return "Inflict Soaked for 3 turns on enemy pieces. (Soaked: Piece can't capture.)";
                    default:
                        return "Inflict Poisoned for 3 turns on enemy pieces. (Poisoned: Piece will be destroyed when the effect expires.)";
                }
            case SpecialType.InflictShift:
                switch (pte.type)
                {
                    case PieceType.FloatMage:
                        return "Move or Inflict Light on an enemy for 3 turns. (Light: Piece automatically moves forward every turn.)";
                    case PieceType.GravityMage:
                        return "Move or Inflict Heavy on an enemy for 3 turns. (Heavy: Piece automatically moves backwards every turn.)";
                }
                return "Move or Inflict a status effect for 3 turns on enemy pieces.";
            case SpecialType.InflictShiftCaptureOnly:
                switch (pte.type)
                {
                    case PieceType.FloatMage:
                        return "Inflict Light on an enemy for 3 turns. (Light: Piece automatically moves forward every turn)";
                    case PieceType.GravityMage:
                        return "Inflict Heavy on an enemy for 3 turns. (Heavy: Piece automatically moves backwards every turn)";
                }
                return "Inflict a status effect for 3 turns on enemy pieces.";
            case SpecialType.TeleportOpposite:
                return "Teleport the target to the opposite side of yourself.";
            case SpecialType.TeleportRecall:
                return "Teleport the target back behind yourself if that square is empty.";
            case SpecialType.TeleportMirror:
                return "Teleport the target to its mirrored square.";
            case SpecialType.CarryAlly:
                return "Move an ally inside of yourself.";
            case SpecialType.DepositAlly:
                return "Deposit an ally inside of yourself to outside.";
            case SpecialType.DepositAllyPlantMove:
                return "(Ignores Obstacles) Deposit an ally inside of yourself to outside.";
            case SpecialType.EnemyAbility:
                return "(Enchant) An ability that affects enemy pieces.";
            case SpecialType.RangedPull:
                return "(Enchant) (Ignores Obstacles) Pull a piece one square.";
            case SpecialType.RangedPush:
                return "(Enchant) (Ignores Obstacles) Push a piece one square.";
            case SpecialType.EmptyAbility:
                return "An ability that affects empty squares.";
            case SpecialType.PassiveAbility:
                switch (pte.type)
                {
                    case PieceType.Hypnotist:
                        return "(Enchant) Move enemy pieces in this area.";
                    case PieceType.Envy:
                        return "Copy non special enemy movement in this area.";
                }
                return "A passive ability.";
            case SpecialType.AimEnemy:
                return "Aim yourself at an enemy piece.";
            case SpecialType.AimAny:
                return "Aim yourself at a square.";
            case SpecialType.AimOccupied:
                return "Aim yourself at an occupied square.";
            case SpecialType.AmoebaCombine:
                return "Combine with other Amoebas to fuse into a stronger piece.";
            case SpecialType.MorphRabbit:
                return "Turn a piece into a Rabbit. (Rabbits can transform back on their first or last row)";
            case SpecialType.ConvertRabbit:
                return "Convert a Rabbit to your side.";
            case SpecialType.KingAttack:
                return "Capture enemy Kings. (i.e. gives Check on these squares)";
        }
        return "";
    }

    public static bool IsModifierCompatible(PieceModifier pm, PieceTableEntry pte)
    {
        //Note that some are blocked for being redundant (i.e. Vengeful is already DestroyCapturer so combining them does nothing additional)
        //Winged checks WingedCompatible
        //Spectral is blocked from NonBlockingAlly
        //Warped is blocked by ShiftImmune

        if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
        {
            return false;
        }

        switch (pm)
        {
            case PieceModifier.Vengeful:
                return (pte.pieceProperty & PieceProperty.DestroyCapturer) == 0;
            case PieceModifier.Winged:
                //Precomputed because the condition is very complex (Requires a move that is blockable and no move types that are not programmed to work with Winged)
                return pte.wingedCompatible && ((pte.piecePropertyB & PiecePropertyB.NaturalWinged) == 0);
            case PieceModifier.Spectral:
                return (pte.piecePropertyB & PiecePropertyB.NonBlockingAlly) == 0;
            case PieceModifier.Warped:
                return (pte.piecePropertyB & PiecePropertyB.ShiftImmune) == 0;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) TransformBasedOnAlignment(PieceAlignment pa, int x, int y, bool flip)
    {
        int flipValue = flip ? -1 : 1;
        switch (pa)
        {
            case PieceAlignment.White:
                return (x * flipValue, y * flipValue);
            case PieceAlignment.Black:
                return (-x * flipValue, -y * flipValue);
            //unused, neutrals generate using the mover's alignment so this isn't even used
            //case PieceAlignment.Neutral:
                //return (y * flipValue, -x * flipValue);
        }

        return (x, y);
    }
}

//4096 bit table
[Serializable]
public class MoveBitTable
{
    public ulong[] tableElements;

    //I only ever use AttackDefense so this setup is unnecessary
    /*
    public enum BitTableType : byte
    {
        MoveOnly = 1,
        AttackDefense = 1 << 1,
        Magic = 1 << 2,

        Normal = MoveOnly | AttackDefense,
    }
    public BitTableType btType;
    */

    public ulong Get(int xy)
    {
        return tableElements[xy];
    }
    public ulong Get(int x, int y)
    {
        return tableElements[x + (y << 3)];
    }
    public bool Get(int x, int y, int toX, int toY)
    {
        return ((tableElements[x + (y << 3)]) & ((1uL << (toX + (toY << 3))))) != 0;
    }
    public bool Get(int x, int y, ulong bitindexT)
    {
        return ((tableElements[x + (y << 3)]) & (bitindexT)) != 0;
    }
    public bool Get(int xy, ulong bitindexT)
    {
        return ((tableElements[xy]) & bitindexT) != 0;
    }

    public void Set(int x, int y, int toX, int toY, bool set = true)
    {
        if (set)
        {
            tableElements[x + (y << 3)] |= ((1uL << (toX + (toY << 3))));
        }
        else
        {
            tableElements[x + (y << 3)] &= ~((1uL << (toX + (toY << 3))));
        }
    }
    public void Set(int x, int y, ulong bitindexT, bool set = true)
    {
        if (set)
        {
            tableElements[x + (y << 3)] |= ((bitindexT));
        }
        else
        {
            tableElements[x + (y << 3)] &= ~((bitindexT));
        }
    }

    public void Set(int xy, uint bitindexT, bool set = true)
    {
        if (set)
        {
            tableElements[xy] |= (bitindexT);
        }
        else
        {
            tableElements[xy] &= ~(bitindexT);
        }
    }

    public MoveBitTable()
    {
        tableElements = new ulong[64];
    }

    public ulong Flatten()
    {
        ulong output = 0;
        for (int i = 0; i < 64; i++)
        {
            output |= tableElements[i];
        }
        return output;
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

    public void Copy(MoveBitTable mbt)
    {
        for (int i = 0; i < 64; i++)
        {
            tableElements[i] = mbt.tableElements[i];
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