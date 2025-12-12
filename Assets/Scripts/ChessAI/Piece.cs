using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//Use ints
//(If I made a struct it would be forced to be 8 bytes?)
//Which is a big waste of space?
//So instead of that this is a static class that helps convert those ints into the real data you want
public static class Piece
{
    //format
    //0-9 = piece type
    //10-16 = piece special data byte
    //18-21 = modifier type
    //22-25 = status type
    //26-29 = status duration
    //30-31 = alignment


    //Actually more like 10 bits
    //I don't think my piece numbers are going to reach 1024
    public enum PieceType : uint
    {
        Null = 0,
        Rock = 1,   //rock = neutral aligned Null   (I decided not to make it 0 because I don't want to deal with that headache)
        King = 2,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn,
        Squire,
        Dragoon,
        Lancer,
        Cataphract,
        Marshall,
        Priest,
        Missionary,
        Inquisitor,
        Archbishop,
        Cardinal,
        Anchorite,
        Rammer,
        Turret,
        Trebuchet,
        Fortress,
        Shielder,
        Amazon,
        Guard,
        Ranger,
        Warden,
        Princess,
        Pegasus,
        Pontiff,
        Stronghold,
        Empress,
        SuperPawn,
        DivineProgenitor,
        DivineProtector,
        DivineAvenger,
        DivineBeacon,
        DivineApothecary,
        DivineCourier,
        DivineArtisan,
        DivineSquire,
        DivinePage,
        DivineMusician,
        DivineMaid,
        DivineUsurper,
        DivineGardener,
        DivineApprentice,
        DivineHerald,
        SnailQueen,
        Crab,
        Lobster,
        Shrimp,
        Tardigrade,
        BigBomb,
        Bomb,
        XBomb,
        PiercingCharge,
        MiniBomb,
        RelayKnight,
        RelayBishop,
        RelayRook,
        RelayQueen,
        Runner,
        SwiftBishop,
        SwiftRook,
        SwiftQueen,
        FastPawn,
        CoastGuard,
        Boat,
        Skipper,
        Raft,
        Warship,
        Canoe,
        Submarine,
        Pusher,
        PushBishop,
        PushRook,
        PushQueen,
        PushPawn,
        StickyMan,
        StickyBishop,
        StickyRook,
        StickyQueen,
        StickyPawn,
        Lanterner,
        Candler,
        Torcher,
        Vestal,
        MatchPawn,
        Hypnotist,
        Attractor,
        Repulser,
        Immobilizer,
        Entrancer,
        Charmer,
        Wrath,
        Greed,
        Gluttony,
        Sloth,
        Lust,
        Envy,
        Pride,
        Imp,
        Patience,
        Charity,
        Temperance,
        Diligence,
        Chastity,
        Kindness,
        Humility,
        Monk,
        ArcanaFool,
        ArcanaMagician,
        ArcanaPriestess,
        ArcanaEmpress,
        ArcanaEmperor,
        ArcanaHierophant,
        ArcanaLovers,
        ArcanaChariot,
        ArcanaJustice,
        ArcanaHermit,
        ArcanaFortune,
        ArcanaStrength,
        ArcanaHanged,
        ArcanaDeath,
        ArcanaTemperance,
        ArcanaDevil,
        ArcanaTower,
        ArcanaStar,
        ArcanaMoon,
        MoonIllusion,
        ArcanaSun,
        ArcanaJudgement,
        ArcanaWorld,
        AceOfWands,
        AceOfSwords,
        AceOfPentacles,
        AceOfCups,
        PageOfWands,
        PageOfSwords,
        PageOfPentacles,
        PageOfCups,
        QueenOfWands,
        QueenOfSwords,
        QueenOfPentacles,
        QueenOfCups,
        Mercury,
        Venus,
        Earth,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Moon,
        Asteroid,
        Comet,
        Io,
        Europa,
        Ganymede,
        Callisto,
        Aries,
        Taurus,
        Gemini,
        GeminiTwin,
        Cancer,
        Leo,
        Virgo,
        Libra,
        Scorpio,
        Sagittarius,
        Capricorn,
        Aquarius,
        Pisces,
        AirElemental,
        FireElemental,
        EarthElemental,
        WaterElemental,
        VoidElemental,
        LightElemental,
        AirWisp,
        FireWisp,
        EarthWisp,
        WaterWisp,
        VoidWisp,
        LightWisp,
        Vulture,
        Falcon,
        Hawk,
        Eagle,
        Fledgling,
        SwitchPaladin,
        SwitchTower,
        SwitchFrog,
        SwitchKnight,
        SwitchSquire,
        WarQueen,
        WarRook,
        WarBishop,
        WarKnight,
        BerserkerPawn,
        Hopper,
        Locust,
        KingHopper,
        Checker,
        Revenant,
        Abomination,
        Necromancer,
        Vampire,
        Skeleton,
        Zombie,
        Bat,
        Banshee,
        Harpy,
        Hag,
        Frog,
        Toad,
        Sludge,
        SludgeTrail,
        Plaguebearer,
        Werewolf,
        Werebear,
        Werechimera,
        Werefox,
        ElephantCalf,
        Elephant,
        ElephantCrusher,
        ElephantCharger,
        ElephantQueen,
        ElephantBulwark,
        RabbitQueen,
        RabbitCourier,
        RabbitDiplomat,
        RabbitKnight,
        Rabbit,
        Blackguard,
        Spy,
        Infiltrator,
        Disguiser,
        Assassin,
        SlipPawn,
        Diplomat,
        RoyalDouble,
        RoyalGuard,
        RoyalMaid,
        RoyalCastle,
        RoyalRecruit,
        ShadowQueen,
        EchoSoldier,
        MirrorQueen,
        LensRook,
        ReflectionBishop,
        MirrorKnight,
        MirrorPawn,
        Pincer,
        Bolter,
        Shocker,
        Magnet,
        LightningElemental,
        ElectroPawn,
        HornSpirit,
        TorchSpirit,
        RingSpirit,
        BottleSpirit,
        CapSpirit,
        GlassSpirit,
        FeatherSpirit,
        SwordSpirit,
        ShieldSpirit,
        GrailSpirit,
        DaggerSpirit,
        Paladin,
        CrookedBishop,
        CrookedRook,
        CrookedQueen,
        CrookedPawn,
        ClockworkTurtle,
        ClockworkTowerB,
        ClockworkTowerR,
        ClockworkLeaper,
        ClockworkWalker,
        BladeBeast,
        ClockworkSnapper,
        Leafling,
        Gardener,
        Rootwalker,
        Burrower,
        Blossom,
        FlowerPawn,
        Gargoyle,
        Pillar,
        Arch,
        Wall,
        Statue,
        EliteMilitia,
        EdgeRook,
        CornerlessBishop,
        CenterQueen,
        Militia,
        KangarooQueen,
        KangarooPrincess,
        Triknight,
        Tribishop,
        Birook,
        TrojanHorse,
        PawnStack,
        BigSlime,
        Slime,
        Duelist,
        Fencer,
        FlankingKnight,
        FlankingBishop,
        FlankingPawn,
        FrontQueen,
        FrontRook,
        FrontBishop,
        FrontKnight,
        FrontPawn,
        Marshqueen,
        Brook,
        Bishight,
        Knishop,
        TailPawn,
        NarrowQueen,
        NarrowRook,
        NarrowBishop,
        NarrowKnight,
        NarrowPawn,
        FlatQueen,
        FlatRook,
        FlatBishop,
        FlatKnight,
        AxePawn,
        GlassQueen,
        GlassRook,
        GlassBishop,
        GlassKnight,
        GlassPawn,
        QuickKnight,
        QuickHog,
        Hummingbird,
        QuickSlug,
        QuickPawn,
        LavaGolem,
        IceGolem,
        SlothQueen,
        VolcanoTower,
        FlameObelisk,
        HeavyFrog,
        HeavyPawn,
        SoulCannon,
        Lich,
        SoulDevourer,
        QueenLeech,
        Leech,
        ChargeBeast,
        ChargeCannon,
        ChargeWarper,
        ChargeKnight,
        ChargePawn,
        Ghoul,
        GhoulMarshall,
        GhoulAmazon,
        GhoulPawn,
        PufferfishQueen,
        PufferfishPawn,
        Robber,
        Enforcer,
        Smuggler,
        Mercenary,
        Outlaw,
        WarpWeaver,
        WarpMage,
        Mirrorer,
        Recaller,
        Flipper,
        Conductor,
        Harlequin,
        Jester,
        Dancer,
        Penguin,
        IceElemental,
        EmperorPenguin,
        SnowBomb,
        SnowQueen,
        Snowman,
        PythonQueen,
        PoisonElemental,
        PoisonBomb,
        PoisonFrog,
        Snake,
        Phantom,
        HealerMage,
        PoisonMage,
        FreezeMage,
        SparkMage,
        SplashMage,
        FloatMage,
        GravityMage,
        SkipQueen,
        SkipRook,
        SkipBishop,
        SkipGuard,
        SkipPawn,
        SummerQueen,
        WinterQueen,
        SummerRook,
        WinterBishop,
        SpringKnight,
        FallKnight,
        SummerPawn,
        WinterPawn,
        SpringPawn,
        FallPawn,

        DayQueen,
        NightQueen,
        DayBishop,
        NightKnight,
        DayPawn,
        NightPawn,
        HoneybeeQueen,
        HoneybeeLieutenant,
        Honeybee,
        WaxQueen,
        WaxSoldier,
        WaxPawn,
        HoneyBomb,
        HoneyPuddle,
        SteelGolem,
        MegaCannon,
        Cannon,
        MetalFox,
        SteelPuppet,
        RollerQueen,
        ReboundRook,
        BounceBishop,
        Roller,
        Balloon,
        GliderQueen,
        Capybara,
        Beaver,
        Squirrel,
        GliderPawn,
        AmoebaCitadel,
        AmoebaGryphon,
        AmoebaRaven,
        AmoebaArchbishop,
        AmoebaKnight,
        AmoebaPawn,
        RockEgg,
        MountainTurtle,
        WaveEgg,
        OceanSerpent,
        FlameEgg,
        Dragon,
        Suprema,
        Bastion,
        Augur,
        RoseKnight,
        Traverser,

        Bunker,
        Tunnel,
        Fan,
        Watchtower,
        Train,
        Carrier,
        Airship,
        Imitator,

        Peasant,

        Prince,
        Yeoman,

        EndOfTable
    }

    public enum PieceClass
    {
        None,
        Normal,
        Knight,
        Bishop,
        Rook,
        Amazons,
        Ultimate,
        Divine,        //Realm 65 (i.e. they are not part of the normal world)
        Armored,
        Explosive,
        Relay,
        Swift,
        Boat,
        Push,
        Sticky,
        Fire,
        Enchanter,
        DeadlySins,
        HeavenlyVirtues,
        TarotMajor,
        TarotMinor,     //realm is merged with tarot major
        Planets,
        Zodiac,
        Elemental,
        Birds,
        Switchers,
        Warlike,
        Hoppers,
        Undead,
        Monster,
        Lycanthropes,
        Rabbit,
        Giant,
        Dark,
        Royalist,
        Mirror,
        Electro,
        ObjectSpirit,   //No realm of its own because they don't play well together (It doesn't work as an army) (They only really work as part of other armies, an army made out of them is hard to use?)
        Crooked,
        Clockwork,
        Plants,
        Statue,
        ZoneRestricted,
        Splitters,
        Flankers,
        FrontBias,
        FrontBack,
        Narrow,
        Flat,
        Glass,
        Quick,
        Slow,
        SoulMages,
        ChargeMovers,
        ForcedMovers,
        Criminals,          //Has no realm of their own or their realm isn't recognized (Merge with Dark I guess?) (They don't play together as a single army very well)
        Troupe,             //No realm of their own because they don't work as an army (because not enough capture power)
        Teleporters,        //No realm of their own because they teleport around
        Icy,
        Poison,
        StatusMages,
        Skip,
        Seasonal,
        DayNight,
        Honeybees,
        Targetters,
        Rollers,
        Gliders,
        Amoebas,
        GreatBeasts,
        Lost,
    }

    //2 bits = 4 possible
    public enum PieceAlignment : uint
    {
        //Not possible for a piece to have Null alignment because it doesn't fit
        Null = 255,

        White = 0,
        Black = 1 << 30,
        Neutral = 2u << 30,        //Neutral pieces have a lot of balance problems (What material value do they give? Zero? If it's zero then the AI will just sacrifice them willy nilly which might just be the best use case for them)
            //This hypothesis is kind of wrong at least for the current AI: in practice the AI just ignores the neutral pieces most of the time
            //This is because moving your own pieces increases piece table values which gives a more immediate benefit from the AI's perspective
        Crystal = 3u << 30         //Crystal pieces have this problem a lot less as keeping them around you can use them for defense (you get positive value from them so just sacrificing them immediately isn't the best play)
    }

    //4 bits = 15 possible
    //Modifiers are infinite duration things
    public enum PieceModifier : uint
    {
        None,
        Vengeful = 1 << 18,      //DestroyCapturer   (Red)
        Phoenix = 2 << 18,       //Revenant power    (Orange)

        Radiant = 3 << 18,         //Spawn pawns like Revenant on capture (Yellow)
                            //Problem: needs limits so you can't clog the board with infinite pawns

        Winged = 4 << 18,         //Gets move only hop over anything in its ranged moves  (Green)
        Spectral = 5 << 18,       //Does not block ally pieces from moving through them   (Cyan)
        Immune = 6 << 18,         //Immune to enemy negative effects (= NoTerrain, StatusImmune, EnchantImmune)   (Blue)
            //The balance problem and why I didn't implement this before is that there are a lot of cases where none of those three exist and then the modifier becomes useless)
            //It needs extra stuff to do?
            //  Now it is rook range 1 water (i.e. enemy can't capture)
        Warped = 7 << 18,         //You can swap move onto them (unlike spectral you can't pass through) (Purple)

        Shielded = 8 << 18,       //If user ends turn when it is attacked: Shielded becomes Half Shielded (Gray)
        HalfShielded = 9 << 18,   //Half shielded is removed on enemy turn end (So a shielded king works properly, if it was removed on own turn end it would lead to stalemates where the shielded king is not capturable by the enemy but any move would remove the shield)

        NoSpecial = 10 << 18,      //Special thing for move copying pieces (Blocks all special moves)

        //?: on capture: spawn pawn like Revenant (Problem: why normal pawns? Pawn spawn may clog up your formation? If it is just the attacker's class pawnlike it would be somewhat unpredictable and unbalanced, if it was the victim's pawnlike that balance problem is removed but it causes weird stuff)
            //One point in favor is that this gives space for more piece spawners
            //It also gives something that can be more "economy"
            //It might not be that bad it spawns normal pawns only?
            //Need some way of making the spawned pawns not annoying
        //?: on capture: give your King special data stacking (Problem: What if you have no king or many kings)
        //
        //Warped: Allies can Swap Move onto them (the idea is that it forces CanMoveOntoAlly to true for most moves) (This is basically an alternate Spectral)
            //Problem: Overlaps spectral a lot? Very weird move generation wise?
            //It probably looks like a good idea?
            //Has some different possibility space than Spectral


        //Reserved 9
        //Reserved 10
        //Reserved 11
        //Reserved 12
        //Reserved 13
        //Reserved 14
        //Reserved 15
    }

    //4 bits = 15 possible
    //Status effects have a temporary counter (3 bits) (decremented on enemy turn end?) (So poison for 1 turn is basically a weaker ranged capture)
    public enum PieceStatusEffect : uint
    {
        None,

        Bloodlust = 1 << 22,  //Die in X turns, removed on capture, can only capture
        Sparked = 2 << 22, //Die in X turns, removed on move
        Poisoned = 3 << 22,   //die after X turns
        //Logic to speed up status effect checks is that every status effect at or before Poisoned destroys pieces on effect end

        Frozen = 4 << 22,    //can't move for X turns
        Soaked = 5 << 22,   //No capturing and no enemy targetting abilities (and generates no auras)
        Ghostly = 6 << 22, //Enemy version of Spectral (enemy pieces can pass through)
        Fragile = 7 << 22,    //Acts as "destroy on capture"
        Heavy = 8 << 22,  //Falls down by 1 every turn
        Light = 9 << 22,  //Floats forward by 1 every turn

        //Reserved 10
        //Reserved 11
        //Reserved 12
        //Reserved 13
        //Reserved 14
        //Reserved 15
    }

    //Special properties (may generate teleports)
    [Flags]
    public enum PieceProperty : ulong
    {   
        //63
        //so I am officially out of flags
        //I have to rearrange the existing ones to make more space
        //Ideally I should remove flags that are only used by 1 piece type and change it to a single piece condition (This won't increase the overhead because 1 conditional is 1 conditional? Difference between them is negligible or the piece type check might even be better than the flag check?)

        //Tags to change by priority
        //Castling: can be made into a forced King only thing
        //Convert and Weak Convert can be combined
        //

        None = 0,

        //Move generating things
        //Idea: since they only exist to add stuff to the moveInfo I can just overload the thing so it can parse these as special move atoms also
        /*
        //Castling = 1uL,   //treated as a special property of the king     //x
        AllyKingTeleport = 1uL << 1,    //x
        EnemyKingTeleport = 1uL << 2,   //x
        PawnSwapTeleport = 1uL << 3,    //x
        AllySwapTeleport = 1uL << 4,    //x
        AllyBehindTeleport = 1uL << 0,  //x
        AnywhereTeleport = 1uL << 5,    //x
        HomeRangeTeleport = 1uL << 58,  //x
        KingSwapTeleport = 1uL << 59,   //x
        */
        BonusMove = 1uL << 2,
        SlowMove = 1uL << 4,
        ConsumeAllies = 1uL << 5,
        ChargeEnhance = 1uL << 58,
        ExplodeCaptureX = 1uL << 59,

        //Range modifiers under conditions
        RangeIncrease_MissingPieces = 1uL << 6,
        RangeIncrease_FurtherRows = 1uL << 7,
        RangeIncrease_NearRows = 1uL << 23,
        RangeDecrease_FurtherRows = 1uL << 8,

        RangeChange = RangeIncrease_MissingPieces | RangeIncrease_FurtherRows | RangeIncrease_NearRows | RangeDecrease_FurtherRows,

        //Special capture types
        ConvertCapture = 1uL << 9,
        WeakConvertCapture = 1uL << 10,
        SwapCapture = 1uL << 11,

        PromoteCapture = 1uL << 12,   //this piece promotes by capturing
        PromoteCaptureNonPawn = 1uL << 13,   //this piece promotes by capturing

        EnchantImmune = 1uL << 14,
        Deadly = 1uL << 15,
        FireImmune = 1uL << 16,
        WaterImmune = 1uL << 17,
        //DisplacementImmune = 1uL << 18,       //NoTerrain subsumed this

        Cylindrical = 1uL << 19,
        Sneaky = 1uL << 20,         //Top bottom non captures
        Reflecter = 1uL << 21,        

        BoundaryProperties = Cylindrical | Sneaky | Reflecter,
        BitboardIncompatible = RangeChange | BoundaryProperties,

        Unique = 1uL << 22,             //this is mostly a out of battle restriction, make into a boolean in the PTE?

        //Invincibility of various forms
        Invincible = 1uL << 24,
        InvincibleWrongColor = 1uL << 25,
        InvincibleFront = 1uL << 26,
        InvinciblePawns = 1uL << 27,
        InvincibleNonPawns = 1uL << 28,
        InvincibleClose = 1uL << 29,
        InvincibleClose2 = 1uL << 30,
        InvinciblePride = 1uL << 31,      //Only most costly piece or piece of more cost
        InvincibleJustice = 1uL << 32,      //Invincible if ally captured last turn

        Relay = 1uL << 33,
        RelayBishop = 1uL << 34,      //this is only on a special piece that doesn't match its movement
        RelayImmune = 1uL << 35,        //area in range is immune (so I just paste the attack/defense range to the immune range minus any enemy pieces in the way)
        WrathCapture = 1uL << 36,
        Splitter = 1uL << 37,

        FlankingCapture = 1uL << 3,

        /*
        RelayKnight = 1uL << 33,  
        RelayBishop = 1uL << 34,
        RelayBishopImmune = 1uL << 35,
        RelayRook = 1uL << 36,
        RelayMan = 1uL << 37,
        */

        OnlyCapturePawns = 1uL << 38,

        DestroyCapturer = 1uL << 39,
        DestroyOnCapture = 1uL << 40,
        ExplodeCapture = 1uL << 41,

        FireCapture = 1uL << 42,

        //SwitchMover = 1uL << 43,    //use enhanced only if on black
        //WarMover = 1uL << 44,       //enhanced if enemy near
        //NoAllyMover = 1uL << 45,    //enhanced if no ally near
        //AllyMover = 1uL << 0,    //enhanced if ally near
        //JusticeMover = 1uL << 46,   //enhanced if enemy captured last turn
        //DiligenceMover = 1uL << 47, //enhanced if moved last turn
        //VampireMover = 1uL << 48, //enhanced if you or enemy captured last turn

        SlipMover = 1uL << 49, //enhanced target squares can move next to enemies
        PlantMover = 1uL << 50, //enhanced target squares can move next to allies

        NoTerrain = 1uL << 51,

        //morph capturer to be this victim's type
        MorphCapturer = 1uL << 52,
        MorphCapturerPawn = 1uL << 53,
        MorphCapturerNonPawn = 1uL << 54,

        ClockworkSwapper = 1uL << 55,

        Push = 1uL << 56,
        Pull = 1uL << 57,

        PassivePull = 1uL << 60,
        PassivePushDiag = 1uL << 18,
        PassivePullStrong = 1uL << 61,
        PassivePushStrongDiag = 1uL << 1,
        PassivePush = 1uL << 62,
        PassivePushStrong = 1uL << 63,

        PassiveShift = PassivePull | PassivePushDiag | PassivePullStrong | PassivePushStrongDiag | PassivePush | PassivePushStrong
    }

    //You know what? I can make as many bits as I want for properties
    //128 bits go brr
    //64 bit limit isn't even a problem
    //There is no problem that mandates I only use 1 flag (outside of avoiding a problem of knowing which variable to check)
    //(Though I have to check B-properties against the B property flag value)
    [Flags]
    public enum PiecePropertyB : ulong
    {
        None = 0,

        ChargeEnhanceStack = 1uL,   //ChargeEnhance but the range of the enhanced moves are increased (additively, so 2 base gets +1 per charge so I can put in 2 range stuff when you get 1 charge, then 3 with 2 charge...) by the charges
        ChargeEnhanceStackReset = 1uL << 1,   //ChargeEnhanceStack but the charges reset to 0 on charge move
        //PartialForcedMoves = 1uL << 2,  //Check bittable for piece: if empty only then get secondary moves (So the first later moves get priority)
        //InverseForcedMoves = 1uL << 3,  //Inverse of partial forced moves: only get secondary moves when the primary moves exist
        //PartialForcedCapture = 1uL << 21,  //Check bittable for piece: if empty only then get secondary moves (So the first later moves get priority)

        //PromoteWarp = 1uL << 4,     //Warp on promotion (except capturing)

        NonBlockingAlly = 1uL << 5, //like Spectral
        NonBlockingEnemy = 1uL << 6,    //like Ghostly

        ChargeByMoving = 1uL << 7,
        EnemyOnCapture = 1uL << 8,
        NeutralOnCapture = 1uL << 9,

        FreezeCapturer = 1uL << 10,
        PoisonCapturer = 1uL << 11,
        StatusImmune = 1uL << 12,

        PieceCarry = 1uL << 13,
        SpreadCure = 1uL << 14,

        PoisonExplode = 1uL << 15,
        IceExplode = 1uL << 16,

        InflictFreeze = 1uL << 17,
        Inflict = 1uL << 18,

        NotCarriable = 1uL << 19,

        InvincibleNoEnemyAdjacent = 1uL << 20,

        //Made into hardcoded things for optimization per turn
        //SeasonalSwapper = 1uL << 22,
        //SeasonalSwapperB = 1uL << 23,

        ClockworkSwapperB = 1uL << 24,

        //left unimplemented because it is difficult to give a balanced value
        //Many fading pieces reach an asymptote of value because you lose them after some time
        //If you can't win in 10 turns they lose all value (So if I underprice them you can basically go from having little value to having too much as you can instantly use the real material differential to win)
        //Fading = 1uL << 25,

        ShiftImmune = 1uL << 26,

        //NoCount = 1uL << 27,        //not counted in piece count    (Note that for implementation reasons ArcanaMoon and MoonIllusion do not do this)
        Giant = 1uL << 27,

        //Made into hardcoded things for optimization per turn
        //DaySwapper = 1uL << 28,
        //DaySwapperB = 1uL << 29,

        InvincibleFar = 1uL << 30,  //inverse of close (i.e. invincible from range 2+)
        InvincibleFar2 = 1uL << 31, //invincible from range 3+ (So knights can attack it)

        //FarHalfMover = 1uL << 4,
        //CloseHalfMover = 1uL << 32,

        GliderMover = 1uL << 32,
        CoastMover = 1uL << 33,
        ShadowMover = 1uL << 34,

        //AimMover = 1uL << 35,
        Amoeba = 1uL << 36,

        Momentum = 1uL << 37,
        ReverseMomentum = 1uL << 38,
        BounceMomentum = 1uL << 39,

        ForwardMomentum = Momentum | BounceMomentum,
        AnyMomentum = Momentum | ReverseMomentum | BounceMomentum,

        TandemMover = 1uL << 40,
        TandemMoverDiag = 1uL << 41,
        TandemMoverOrtho = 1uL << 42,
        EnemyTandemMover = 1uL << 43,
        EnemyTandemMoverOrtho = 1uL << 44,

        AllTandemMovers = TandemMover | TandemMoverDiag | TandemMoverOrtho | EnemyTandemMover | EnemyTandemMoverOrtho | AnyTandemMover,

        HoneyExplode = 1uL << 45,

        NaturalWinged = 1uL << 46,
        AnyTandemMover = 1uL << 47,
        MorphImmune = 1uL << 48,

        TrueShiftImmune = ShiftImmune | Giant,

    }

    //how worth it is it to implement this?
    public enum EnhancedMoveType
    {
        None,
        PartialForcedMoves,
        InverseForcedMoves,
        PartialForcedCapture,
        SwitchMover,    //use enhanced only if on black
        WarMover,       //enhanced if enemy near
        ShyMover,     //enhanced if enemy not near
        NoAllyMover,    //enhanced if no ally near
        AllyMover,    //enhanced if ally near
        JusticeMover,   //enhanced if enemy captured last turn
        DiligenceMover, //enhanced if moved last turn
        VampireMover, //enhanced if you or enemy captured last turn
        FearfulMover, //enhanced if you or enemy didn't capture last turn
        FarHalfMover,   //enhanced on enemy half of the board
        CloseHalfMover, //enhanced on ally half of the board
    }
    public enum BonusMoveType
    {
        None,
        SlipMove,
        PlantMove,
        GliderMove,
        CoastMove,
        ShadowMove,
    }
    public enum ReplacerMoveType
    {
        None,
        FireCapture,
        WrathCapturer,
        Push,
        Pull,
        SwapCapture,
        ConvertCapture,
        WeakConvertCapture,
        FlankingCapture,
        ConsumeAllies,
        Inflict,
        InflictFreeze,

    }

    [Flags]
    public enum Aura
    {
        None = 0,
        Nullify = 1,            //empty set symbol
        Banshee = 1 << 1,       //x symbol
        Immobilizer = 1 << 2,   //hollow square symbol
        Attractor = 1 << 3,     //-><- symbol
        Repulser = 1 << 4,      //<-> symbol
        Harpy = 1 << 5,         //spear ^ symbols
        Hag = 1 << 6,           //circle dots
        Sloth = 1 << 7,         //spiral symbols
        Watchtower = 1 << 8,    //eyes
        Fan = 1 << 9,           //pinwheels
        Hanged = 1 << 10,       //spin arrows
        Rough = 1 << 11,        //square dots
        Water = 1 << 12         //water waves (same symbol for water tiles?)
    }

    public static string GetAuraName(Aura aura)
    {
        return aura.ToString();
    }
    public static string GetAuraDescription(Aura aura)
    {
        switch (aura)
        {
            case Aura.Nullify:
                return "Nullifies the effects of enemy Auras.";
            case Aura.Banshee:
                return "Enemies can't move here unless they are capturing.";
            case Aura.Immobilizer:
                return "Enemies in range can't move.";
            case Aura.Attractor:
                return "Enemies in range can only move forwards.";
            case Aura.Repulser:
                return "Enemies in range can only move backwards.";
            case Aura.Harpy:
                return "Enemies in range can only capture.";
            case Aura.Hag:
                return "Enemies in range that capture here are Destroyed.";
            case Aura.Sloth:
                return "Enemies in range can only move 1 space at a time.";
            case Aura.Watchtower:
                return "Enemies can't move onto these squares unless they are moving 1 square at a time.";
            case Aura.Fan:
                return "Enemies in range are pushed forwards.";
            case Aura.Hanged:
                return "Enemies in range move upside down.";
            case Aura.Rough:
                return "Enemies must stop on rough squares.";
            case Aura.Water:
                return "Enemies in range can't capture.";
        }
        return null;
    }
    public static Color GetAuraColor(Aura aura)
    {
        switch (aura)
        {
            case Aura.Nullify:
                return new Color(0, 1, 1, 1);
            case Aura.Banshee:
                return new Color(1f, 0.25f, 0, 1f);
            case Aura.Immobilizer:
                return new Color(0.5f, 0.5f, 0.5f, 1);
            case Aura.Attractor:
                return new Color(1f, 0.75f, 0, 1f);
            case Aura.Repulser:
                return new Color(0, 0.5f, 1f, 1f);
            case Aura.Harpy:
                return new Color(1, 0, 0, 1);
            case Aura.Hag:
                return new Color(0.5f, 1f, 0, 0);
            case Aura.Sloth:
                return new Color(1f, 0, 0.5f, 1f);
            case Aura.Watchtower:
                return new Color(0, 1f, 0.5f, 0);
            case Aura.Fan:
                return new Color(0, 1f, 0, 1f);
            case Aura.Hanged:
                return new Color(0.4f, 0f, 1f, 1f);
            case Aura.Rough:
                return new Color(1, 0.9f, 0, 0);
            case Aura.Water:
                return new Color(0, 0, 1, 1);
        }
        return new Color(0,0,0,0);
    }
    public static string GetPropertyName(PieceProperty pp)
    {
        switch (pp)
        {
            case PieceProperty.Push:
            case PieceProperty.Pull:
            case PieceProperty.Splitter:
            case PieceProperty.ConsumeAllies:
            case PieceProperty.ConvertCapture:
            case PieceProperty.WeakConvertCapture:
            case PieceProperty.SwapCapture:
            case PieceProperty.WrathCapture:
            case PieceProperty.FlankingCapture:
            case PieceProperty.FireCapture:
            case PieceProperty.SlipMover:
            case PieceProperty.PlantMover:
            case PieceProperty.ClockworkSwapper:
                break;
            case PieceProperty.BonusMove:
                return "Bonus Move";
            case PieceProperty.SlowMove:
                return "Slow";
            case PieceProperty.ChargeEnhance:
                return "Charge Enhance";
            case PieceProperty.ExplodeCaptureX:
                return "Explode X";
            case PieceProperty.RangeIncrease_MissingPieces:
                return "Range Boost - Pieces";
            case PieceProperty.RangeIncrease_FurtherRows:
                return "Range Boost - Far";
            case PieceProperty.RangeIncrease_NearRows:
                return "Range Boost - Close";
            case PieceProperty.RangeDecrease_FurtherRows:
                return "Range Reduction - Far";
            case PieceProperty.PromoteCapture:
                return "Promote Capture";
            case PieceProperty.PromoteCaptureNonPawn:
                return "Promote Capture";
            case PieceProperty.EnchantImmune:
                return "Enchant Immune";
            case PieceProperty.Deadly:
                return "Deadly";
            case PieceProperty.FireImmune:
                return "Fire Immune";
            case PieceProperty.WaterImmune:
                return "Water Immune";
            case PieceProperty.Cylindrical:
                return "Cylindrical";
            case PieceProperty.Sneaky:
                return "Sneaky";
            case PieceProperty.Reflecter:
                return "Reflecter";
            case PieceProperty.Unique:
                return "Unique";
            case PieceProperty.Invincible:
                return "Invincible";
            case PieceProperty.InvincibleWrongColor:
                return "Invincible - Wrong Color";
            case PieceProperty.InvincibleFront:
                return "Invincible - Front";
            case PieceProperty.InvinciblePawns:
                return "Invincible - Pawns";
            case PieceProperty.InvincibleNonPawns:
                return "Invincible - Non Pawns";
            case PieceProperty.InvincibleClose:
                return "Invincible - Close";
            case PieceProperty.InvincibleClose2:
                return "Invincible - Close 2";
            case PieceProperty.InvinciblePride:
                return "Invincible - Pride";
            case PieceProperty.InvincibleJustice:
                return "Invincible - Justice";
            case PieceProperty.Relay:
                return "Relay";
            case PieceProperty.RelayBishop:
                return "Relay Bishop";
            case PieceProperty.RelayImmune:
                return "Shared Immunity";
            case PieceProperty.OnlyCapturePawns:
                return "Only Capture Pawns";
            case PieceProperty.DestroyCapturer:
                return "Destroy Capturer";
            case PieceProperty.DestroyOnCapture:
                return "Destroy On Capture";
            case PieceProperty.ExplodeCapture:
                return "Explode";
            case PieceProperty.NoTerrain:
                return "Terrain Immune";
            case PieceProperty.MorphCapturer:
                return "Morph Capturer";
            case PieceProperty.MorphCapturerPawn:
                return "Morph Capturer Pawn";
            case PieceProperty.MorphCapturerNonPawn:
                return "Morph Capturer Non Pawn";
            case PieceProperty.PassivePull:
                return "Passive Pull";
            case PieceProperty.PassivePushDiag:
                return "Passive Pull Diagonal";
            case PieceProperty.PassivePullStrong:
                return "Passive Pull Strong";
            case PieceProperty.PassivePushStrongDiag:
                return "Passive Push Strong Diagonal";
            case PieceProperty.PassivePush:
                return "Passive Push";
            case PieceProperty.PassivePushStrong:
                return "Passive Push Strong";
        }
        return "";
    }
    public static string GetPropertyDescription(PieceProperty pp, PieceType pt)
    {
        switch (pp)
        {
            case PieceProperty.Push:
            case PieceProperty.Pull:
            case PieceProperty.Splitter:
            case PieceProperty.ConsumeAllies:
            case PieceProperty.ConvertCapture:
            case PieceProperty.WeakConvertCapture:
            case PieceProperty.SwapCapture:
            case PieceProperty.WrathCapture:
            case PieceProperty.FlankingCapture:
            case PieceProperty.FireCapture:
            case PieceProperty.SlipMover:
            case PieceProperty.PlantMover:
            case PieceProperty.ClockworkSwapper:
                break;
            case PieceProperty.BonusMove:
                return "Moving this piece gives you a bonus move (Limit of 1 bonus move per turn.).";
            case PieceProperty.SlowMove:
                return "Can't move a Slow piece if you moved a Slow piece last turn.";
            case PieceProperty.ChargeEnhance:
                switch (pt) {
                    case PieceType.ChargeWarper:
                        return "Certain moves are only possible when charged (Teleport Opposite).";
                    case PieceType.SoulCannon:
                        return "Certain moves are only possible when charged (Fire Capture).";
                }
                return "Certain moves are only possible when charged.";
            case PieceProperty.ExplodeCaptureX:
                return "On capture, explodes, destroying pieces in an X shape.";
            case PieceProperty.RangeIncrease_MissingPieces:
                return "Gain range for each piece lost.";
            case PieceProperty.RangeIncrease_FurtherRows:
                return " Gain range the further ahead you are.";
            case PieceProperty.RangeIncrease_NearRows:
                return "Gain range the further back you are.";
            case PieceProperty.RangeDecrease_FurtherRows:
                return "Lose range the further forward you are. (2 squares per square ahead)";
            case PieceProperty.PromoteCapture:
                return "Promotes when capturing a piece.";
            case PieceProperty.PromoteCaptureNonPawn:
                return "Promotes when capturing a non pawn piece.";
            case PieceProperty.EnchantImmune:
                return "Immune to enchantment effects.";
            case PieceProperty.Deadly:
                return "Ignores all forms of Invincibility.";
            case PieceProperty.FireImmune:
                return "Can't be Burned.";
            case PieceProperty.WaterImmune:
                return "Ignores water effects.";
            case PieceProperty.Cylindrical:
                return "Moves can wrap around the sides of the board.";
            case PieceProperty.Sneaky:
                return "Moves can wrap around the top and bottom edges of the board. Can't capture when crossing over.";
            case PieceProperty.Reflecter:
                return "Moves reflect off the sides of the board.";
            case PieceProperty.Unique:
                return "Normally impossible to obtain multiple copies of this piece.";
            case PieceProperty.Invincible:
                return "Can't be destroyed.";
            case PieceProperty.InvincibleWrongColor:
                return "Wrong Color: Can't be destroyed by attackers on the opposite color squares.";
            case PieceProperty.InvincibleFront:
                return "Can't be destroyed by attackers ahead of it.";
            case PieceProperty.InvinciblePawns:
                return "Can't be destroyed by pawns.";
            case PieceProperty.InvincibleNonPawns:
                return "Can't be destroyed by non pawns.";
            case PieceProperty.InvincibleClose:
                return "Can't be destroyed by adjacent attackers.";
            case PieceProperty.InvincibleClose2:
                return "Can't be destroyed by attackers in range 2.";
            case PieceProperty.InvinciblePride:
                return "Can't attack or be attacked by pieces of lesser value except for the piece the enemy last moved.";
            case PieceProperty.InvincibleJustice:
                return "Can't be attacked if the enemy captured last turn.";
            case PieceProperty.Relay:
                return "Allies defended by this piece gain its movement power.";
            case PieceProperty.RelayBishop:
                return "Allies in Bishop range gain Bishop movement.";
            case PieceProperty.RelayImmune:
                return "Allies adjacent to this piece are immune to negative effects.";
            case PieceProperty.OnlyCapturePawns:
                return "Can only capture pawns.";
            case PieceProperty.DestroyCapturer:
                return "When captured, the capturer is destroyed. (Kings are immune.)";
            case PieceProperty.DestroyOnCapture:
                return "When capturing, this piece is destroyed.";
            case PieceProperty.ExplodeCapture:
                return "When capturing, explode to destroy non fire immune pieces adjacent to where it captured.";
            case PieceProperty.NoTerrain:
                return "Immune to square effects and terrain auras (Rough aura and Water aura.)";
            case PieceProperty.MorphCapturer:
                return "The piece that captures this one changes type to this piece.";
            case PieceProperty.MorphCapturerPawn:
                return "The pawn that captures this one changes type to this piece.";
            case PieceProperty.MorphCapturerNonPawn:
                return "The non pawn piece that captures this one changes type to this piece.";
            case PieceProperty.PassivePull:
                return "Pull enemies from range 2 to be range 1.";
            case PieceProperty.PassivePushDiag:
                return "Pull enemies diagonally away from range 2 to be range 1.";
            case PieceProperty.PassivePullStrong:
                return "Pull enemies from any distance 1 square towards this one.";
            case PieceProperty.PassivePushStrongDiag:
                return "Push enemies diagonally adjacent to the target square as far as possible.";
            case PieceProperty.PassivePush:
                return "Push enemies adjacent to the target square away 1 square.";
            case PieceProperty.PassivePushStrong:
                return "Push enemies adjacent to the target square away as far as possible.";
        }
        return "";
    }
    public static string GetPropertyName(PiecePropertyB pp)
    {
        switch (pp)
        {
            case PiecePropertyB.GliderMover:
            case PiecePropertyB.CoastMover:
            case PiecePropertyB.ShadowMover:
            case PiecePropertyB.PieceCarry:
            case PiecePropertyB.InflictFreeze:
            case PiecePropertyB.Inflict:
                break;
            case PiecePropertyB.ChargeEnhanceStack:
                return "Charge Enhance Stack";
            case PiecePropertyB.ChargeEnhanceStackReset:
                return "Charge Enhance Stack Reset:";
            case PiecePropertyB.NonBlockingAlly:
                return "Non Blocking Ally";
            case PiecePropertyB.NonBlockingEnemy:
                return "Non Blocking Enemy";
            case PiecePropertyB.ChargeByMoving:
                return "Charge By Moving";
            case PiecePropertyB.EnemyOnCapture:
                return "Enemy On Capture";
            case PiecePropertyB.NeutralOnCapture:
                return "Neutral On Capture";
            case PiecePropertyB.FreezeCapturer:
                return "Freeze Capturer";
            case PiecePropertyB.PoisonCapturer:
                return "Poison Capturer";
            case PiecePropertyB.StatusImmune:
                return "Status Immune";
            case PiecePropertyB.SpreadCure:
                return "Spread Cure";
            case PiecePropertyB.PoisonExplode:
                return "Poison Explode";
            case PiecePropertyB.IceExplode:
                return "Ice Explode";
            case PiecePropertyB.NotCarriable:
                return "Not Carriable";
            case PiecePropertyB.InvincibleNoEnemyAdjacent:
                return "Invincible - No Enemy Adjacent";
            case PiecePropertyB.ShiftImmune:
                return "Shift Immune";
            case PiecePropertyB.Giant:
                return "Giant";
            case PiecePropertyB.InvincibleFar:
                return "Invincible - Far";
            case PiecePropertyB.InvincibleFar2:
                return "Invincible - Far 2";
            case PiecePropertyB.Momentum:
                return "Momentum";
            case PiecePropertyB.ReverseMomentum:
                return "Reverse Momentum";
            case PiecePropertyB.BounceMomentum:
                return "Bounce Momentum";
            case PiecePropertyB.TandemMover:
                return "Tandem Mover";
            case PiecePropertyB.TandemMoverDiag:
                return "Tandem Mover Diagonal";
            case PiecePropertyB.TandemMoverOrtho:
                return "Tandem Mover Orthogonal";
            case PiecePropertyB.EnemyTandemMover:
                return "Enemy Tandem Mover";
            case PiecePropertyB.EnemyTandemMoverOrtho:
                return "Enemy Tandem Mover Orthogonal";
            case PiecePropertyB.AnyTandemMover:
                return "Any Tandem Mover";
            case PiecePropertyB.HoneyExplode:
                return "Honey Explode";
            case PiecePropertyB.NaturalWinged:
                return "Natural Winged";
            case PiecePropertyB.MorphImmune:
                return "Morph Immune";
        }
        return "";
    }
    public static string GetPropertyDescription(PiecePropertyB pp, PieceType pt)
    {
        switch (pp)
        {
            case PiecePropertyB.GliderMover:
            case PiecePropertyB.CoastMover:
            case PiecePropertyB.ShadowMover:
            case PiecePropertyB.PieceCarry:
            case PiecePropertyB.InflictFreeze:
            case PiecePropertyB.Inflict:
                break;
            case PiecePropertyB.ChargeEnhanceStack:
                switch (pt)
                {
                    case PieceType.ChargeWarper:
                        return "Certain moves are only possible when charged (Teleport Opposite). Range increases with charges.";
                    case PieceType.SoulCannon:
                        return "Certain moves are only possible when charged (Fire Capture). Range increases with charges.";
                }
                return "Certain moves are only possible when charged. Range increases with charges.";
            case PiecePropertyB.ChargeEnhanceStackReset:
                switch (pt)
                {
                    case PieceType.ChargeWarper:
                        return "Certain moves are only possible when charged (Teleport Opposite). Range increases with charges. Resets on move.";
                    case PieceType.SoulCannon:
                        return "Certain moves are only possible when charged (Fire Capture). Range increases with charges. Resets on move.";
                }
                return "Certain moves are only possible when charged. Range increases with charges. Resets on move.";
            case PiecePropertyB.NonBlockingAlly:
                return "Ally pieces are not blocked by this piece.";
            case PiecePropertyB.NonBlockingEnemy: 
                return "Enemy pieces are not blocked by this piece.";
            case PiecePropertyB.ChargeByMoving:
                return "Gain charges whenever using a non charge move.";
            case PiecePropertyB.EnemyOnCapture:
                return "After attacking, this piece switches alignment.";
            case PiecePropertyB.NeutralOnCapture:
                return "After attacking, this piece becomes Neutral if it isn't already.";
            case PiecePropertyB.FreezeCapturer:
                return "When captured, the capturer is Frozen for 3 turns.";
            case PiecePropertyB.PoisonCapturer:
                return "When captured, the capturer is Poisoned for 3 turns.";
            case PiecePropertyB.StatusImmune:
                return "Immune to status effects.";
            case PiecePropertyB.SpreadCure:
                return "After Move: Cure all adjacent allies of status effects.";
            case PiecePropertyB.PoisonExplode:
                return "Explode to Poison adjacent enemy pieces for 3 turns.";
            case PiecePropertyB.IceExplode:
                return "Explode to Freeze adjacent enemy pieces for 3 turns.";
            case PiecePropertyB.NotCarriable:
                return "Can't be carried by other pieces.";
            case PiecePropertyB.InvincibleNoEnemyAdjacent:
                return "Invincible if no enemy is adjacent (not including the attacker).";
            case PiecePropertyB.ShiftImmune:
                return "Can't be displaced by any effects.";
            case PiecePropertyB.Giant:
                return "Takes up 4 squares. Can capture up to 4 enemies at once but can be captured on any of its 4 squares.";
            case PiecePropertyB.InvincibleFar:
                return "Can only be destroyed by adjacent attackers.";
            case PiecePropertyB.InvincibleFar2:
                return "Can only be destroyed by attackers in range 2.";
            case PiecePropertyB.Momentum:
                return "Move 1 square along after it moves. (Momentum is preserved until it hits an obstacle.).";
            case PiecePropertyB.ReverseMomentum:
                return "Move 1 square backwards after it moves. (Momentum is preserved until it hits an obstacle.).";
            case PiecePropertyB.BounceMomentum:
                return "Move 1 square along after it moves. (Momentum is inverted when it hits an obstacle.).";
            case PiecePropertyB.TandemMover:
                return "Move adjacent allies along itself.";
            case PiecePropertyB.TandemMoverDiag:
                return "Move diagonally adjacent allies along itself.";
            case PiecePropertyB.TandemMoverOrtho:
                return "Move orthogonally adjacent allies along itself.";
            case PiecePropertyB.EnemyTandemMover:
                return "Move adjacent enemies along itself.";
            case PiecePropertyB.EnemyTandemMoverOrtho:
                return "Move orthogonally adjacent enemies along itself.";
            case PiecePropertyB.AnyTandemMover:
                return "Move adjacent pieces along itself.";
            case PiecePropertyB.HoneyExplode:
                return "Explode to fill all adjacent squares with Honey Puddles.";
            case PiecePropertyB.NaturalWinged:
                return "Ignores the first obstacle it meets but can't capture past that obstacle.";
            case PiecePropertyB.MorphImmune:
                return "Can't change type.";
        }
        return "";
    }
    public static string GetPieceSpecialDescription(PieceType pt)
    {
        switch (pt) {
            case PieceType.DivineArtisan:
                return "Burn enemy pieces to gain Charge.";
            case PieceType.DivineApprentice:
                return "Burn enemy pieces to gain Charge.";
            case PieceType.Gluttony:
                return "Gain range by capturing enemy pieces.";
            case PieceType.Kindness:
                return "Copies the non special moves of ally pieces defending this.";
            case PieceType.ArcanaFool:
                return "Copies the non special moves of the last ally piece moved (Can't copy Arcana Fool.).";
            case PieceType.ArcanaEmpress:
                return "Ally pieces in range 2 can move diagonally 1 square.";
            case PieceType.ArcanaMoon:
                return "After Move: Spawn a Moon Illusion.\nMoon Illusions can be moved like Arcana Moon. When an ally Arcana Moon or an ally Moon Illusion is destroyed, all of them are destroyed.";
            case PieceType.MoonIllusion:
                return "After Move: Become Arcana Moon and spawn a Moon Illusion.\nMoon Illusions can be moved like Arcana Moon. When an ally Arcana Moon or an ally Moon Illusion is destroyed, all of them are destroyed.";
            case PieceType.Revenant:
                return "When destroyed, respawns as far back as possible (Single Use).";
            case PieceType.Necromancer:
                return "When it captures, spawns a Skeleton on the starting square.";
            case PieceType.Sludge:
                return "Creates a Sludge Trail as it moves.";
            case PieceType.RabbitCourier:
                return "Gains range for each ally piece adjacent to it.";
            case PieceType.RabbitDiplomat:
                return "When it moves, adjacent normal Rabbits are converted to your side.";
            case PieceType.RabbitKnight:
                return "If a rabbit piece is adjacent, this is Invincible unless an enemy is also adjacent.";
            case PieceType.Pincer:
                return "After Move: Enemies sandwiched between the Pincer and an ally piece or the edge of the board are destroyed.";
            case PieceType.ClockworkTurtle:
                return "If no allies are adjacent, become Invincible.";
            case PieceType.EliteMilitia:
            case PieceType.Militia:
                return "Can't move to the far half of the board.";
            case PieceType.EdgeRook:
                return "Can't move to the center 4 files.";
            case PieceType.CornerlessBishop:
                return "Can't move to within 2 orthogonal steps from the corners of the board.";
            case PieceType.CenterQueen:
                return "Can't move to the outer 4 files.";
            case PieceType.KangarooQueen:
                return "When captured, spawn a Kangaroo Princess behind it if possible.";
            case PieceType.PawnStack:
                return "When captured, spawn a Pawn behind it if possible.";
            case PieceType.BigSlime:
                return "When captured, spawn up to 2 Slimes behind it if possible.";
            case PieceType.LavaGolem:
                return "After Move: Burn all adjacent enemy pieces.";
            case PieceType.IceGolem:
                return "After Move: Freeze all adjacent enemy pieces for 3 turns.";
            case PieceType.Lich:
                return "When destroyed while it has charges, respawns as far back as possible and consume one charge.";
            case PieceType.WarpWeaver:
                return "After Enemy Move: When the aimed at square becomes empty, teleport to that square.";
            case PieceType.ElectroPawn:
                return "Pull enemies that are 2 forwards to be 1 forwards.";
            case PieceType.Uranus:
                return "After Move: Swap poisitions of the pieces adjacent.";
            case PieceType.Abomination:
                return "After Enemy Move: When an ally piece is captured, move towards the enemy king.";
            case PieceType.Zombie:
                return "After Enemy Move: When an ally piece is captured, move forwards one square.";
            case PieceType.ClockworkSnapper:
                return "After Enemy Move: When an enemy is in front of this piece, capture that piece. (Can't capture Kings.)";
            case PieceType.BladeBeast:
                return "After Enemy Move: When an enemy is orthogonally adjacent to this piece, capture that piece. (Can't capture Kings.)";
            case PieceType.Temperance:
                return "Can't capture pieces of 4.5 value or less. Threshold decreases by 0.5 for each ally piece lost.";
            case PieceType.ClockworkTowerB:
                return "After Move: Transforms into Clockwork Tower R";
            case PieceType.ClockworkTowerR:
                return "After Move: Transforms into Clockwork Tower B.";
            case PieceType.ClockworkLeaper:
                return "After Move: Transforms into Clockwork Walker.";
            case PieceType.ClockworkWalker:
                return "After Move: Transforms into Clockwork Leaper.";
            case PieceType.MegaCannon:
                return "Aim at a target to charge an attack to destroy 5 squares surrounding the target. Charging takes 7 turns and ticks down after your turn. While charging, Mega Cannon is Invincible but can't move.";
            case PieceType.MetalFox:
                return "After Enemy Move: When an enemy appears in the targetted square, capture that enemy.";
            case PieceType.ChargeCannon:
                return "Fire Capture gains range for each charge.";
            case PieceType.DayPawn:
                return "After Turn: Transforms into Night Pawn.";
            case PieceType.DayBishop:
                return "After Turn: Transforms into Night Knight.";
            case PieceType.DayQueen:
                return "After Turn: Transforms into Night Queen.";
            case PieceType.NightPawn:
                return "After Turn: Transforms into Day Pawn.";
            case PieceType.NightKnight:
                return "After Turn: Transforms into Day Bishop.";
            case PieceType.NightQueen:
                return "After Turn: Transforms into Day Queen.";
            case PieceType.SummerPawn:
                return "After 5 Turns: Transforms into Winter Pawn.";
            case PieceType.SummerRook:
                return "After 5 Turns: Transforms into Winter Bishop.";
            case PieceType.SummerQueen:
                return "After 5 Turns: Transforms into Winter Queen.";
            case PieceType.WinterPawn:
                return "After 5 Turns: Transforms into Summer Pawn.";
            case PieceType.WinterBishop:
                return "After 5 Turns: Transforms into Summer Rook.";
            case PieceType.WinterQueen:
                return "After 5 Turns: Transforms into Summer Queen.";
            case PieceType.SpringKnight:
                return "After 5 Turns: Transforms into Fall Knight.";
            case PieceType.SpringPawn:
                return "After 5 Turns: Transforms into Fall Pawn.";
            case PieceType.FallKnight:
                return "After 5 Turns: Transforms into Spring Knight.";
            case PieceType.FallPawn:
                return "After 5 Turns: Transforms into Spring Pawn.";
            case PieceType.RockEgg:
                return "After Enemy Move: Hatches into Mountain Tortoise when it has 8 allies adjacent.";
            case PieceType.WaveEgg:
                return "After Enemy Move: Hatches into Ocean Serpent when it has 0 allies adjacent.";
            case PieceType.FlameEgg:
                return "After Enemy Move: Hatches into Dragon when its file has no enemies in it.";
            case PieceType.Imitator:
                return "Copy the non special movement of the last moved enemy piece. (Can't copy Arcana Fool or Imitator.).";
        }
        return "";
    }

    public static uint PackPieceData(PieceType pt, byte pspd, PieceModifier pm, PieceStatusEffect pse, byte psed, PieceAlignment pa)
    {
        uint output = 0;
        output = SetPieceType(pt, output);
        output = SetPieceSpecialData(pspd, output);
        output = SetPieceModifier(pm, output);
        output = SetPieceStatusEffect(pse, output);
        output = SetPieceStatusDuration(psed, output);
        output = SetPieceAlignment(pa, output);
        return output;
    }
    public static uint PackPieceData(PieceType pt, byte pspd, PieceModifier pm, PieceAlignment pa)
    {
        uint output = 0;
        output = SetPieceType(pt, output);
        output = SetPieceSpecialData(pspd, output);
        output = SetPieceModifier(pm, output);
        output = SetPieceAlignment(pa, output);
        return output;
    }
    public static uint PackPieceData(PieceType pt, PieceModifier pm, PieceAlignment pa)
    {
        uint output = 0;
        output = SetPieceType(pt, output);
        output = SetPieceModifier(pm, output);
        output = SetPieceAlignment(pa, output);
        return output;
    }
    public static uint PackPieceData(PieceType pt, PieceAlignment pa)
    {
        uint output = 0;
        output = SetPieceType(pt, output);
        output = SetPieceAlignment(pa, output);
        return output;
    }

    public static string ConvertToString(uint pieceInfo)
    {
        string output = GetPieceType(pieceInfo).ToString();

        ushort pieceData = GetPieceSpecialData(pieceInfo);
        if (pieceData != 0)
        {
            output += " (" + pieceData + ")";
        }

        PieceModifier modifier = GetPieceModifier(pieceInfo);
        if (modifier != PieceModifier.None)
        {
            output += " (" + modifier + ")";
        }

        PieceStatusEffect pse = GetPieceStatusEffect(pieceInfo);
        if (pse != PieceStatusEffect.None)
        {
            output += " (" + pse + " " + GetPieceStatusDuration(pieceInfo) + ")";
        }

        return output;
    }

    //Bit layout
    //0-8 = piece Type (9 bits)
    //9-17 = special data (9 bits)
    //18-21 = modifier (4 bits)
    //22-25 = status effect (4 bits)
    //26-29 = status duration (4 bits)
    //30-31 = alignment (2 bits)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType GetPieceType(uint pieceInfo)
    {
        return (PieceType)(pieceInfo & 0x1ff);
        //return (PieceType)(MainManager.BitFilter(pieceInfo, 0, 8));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceType(PieceType pt, uint pieceInfo)
    {
        return MainManager.BitFilterSetB(pieceInfo, (uint)pt, 0, 8);
    }

    //9 bits of special data
    //This is quite a lot
    //I enlarged it from 8 to 9 so I can fit a whole entire piece type into it :)
    //Currently uses 2 bits for giants
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetPieceSpecialData(uint pieceInfo)
    {
        return (ushort)((pieceInfo & 0x3fe00) >> 9);
        //return (ushort)(MainManager.BitFilter(pieceInfo, 9, 17));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceSpecialData(ushort psd, uint pieceInfo)
    {
        return MainManager.BitFilterSet(pieceInfo, psd, 9, 17);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceModifier GetPieceModifier(uint pieceInfo)
    {
        return (PieceModifier)((pieceInfo & 0x3C0000));
        //return (PieceModifier)((pieceInfo & 0x3C0000) >> 18);
        //return (PieceModifier)(MainManager.BitFilter(pieceInfo, 18, 21));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceModifier(PieceModifier pm, uint pieceInfo)
    {
        return MainManager.BitFilterSetB(pieceInfo, (uint)pm, 18, 21);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceStatusEffect GetPieceStatusEffect(uint pieceInfo)
    {
        return (PieceStatusEffect)((pieceInfo & 0x3C00000));
        //return (PieceStatusEffect)((pieceInfo & 0x3C00000) >> 22);
        //return (PieceStatusEffect)(MainManager.BitFilter(pieceInfo, 22, 25));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceStatusEffect(PieceStatusEffect pm, uint pieceInfo)
    {
        return MainManager.BitFilterSetB(pieceInfo, (uint)pm, 22, 25);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetPieceStatusDuration(uint pieceInfo)
    {
        return (byte)((pieceInfo & 0x3C000000) >> 26);
        //return (byte)(MainManager.BitFilter(pieceInfo, 26, 29));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceStatusDuration(byte psd, uint pieceInfo)
    {
        return MainManager.BitFilterSet(pieceInfo, psd, 26, 29);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceAlignment GetPieceAlignment(uint pieceInfo)
    {
        return (PieceAlignment)((pieceInfo & 0xc0000000));
        //return (PieceAlignment)((pieceInfo & 0xc0000000) >> 30);
        //return (PieceAlignment)(MainManager.BitFilter(pieceInfo, 30, 31));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SetPieceAlignment(PieceAlignment pa, uint pieceInfo)
    {
        //return ((pieceInfo & 0x3fffffff) + ((uint)pa << 30));
        return MainManager.BitFilterSetB(pieceInfo, (uint)pa, 30, 31);
    }

    public static Color GetPieceColor(PieceAlignment pa)
    {
        switch (pa)
        {
            case PieceAlignment.White:
                return Color.white;
            case PieceAlignment.Black:
                return Color.black;
            case PieceAlignment.Neutral:
                return Color.yellow;
            case PieceAlignment.Crystal:
                return Color.magenta;
        }

        return Color.blue;
    }

    public static string GetPieceName(PieceType pt)
    {
        return MainManager.FixCapCase(pt.ToString());
    }

    public static string GetModifierName(PieceModifier pm)
    {
        return MainManager.FixCapCase(pm.ToString());
    }
    public static Color GetModifierColor(PieceModifier pm)
    {
        switch (pm)
        {
            case PieceModifier.Vengeful:
                return new Color(1, 0, 0, 1);
            case PieceModifier.Phoenix:
                return new Color(1, 0.5f, 0, 1);
            case PieceModifier.Radiant:
                return new Color(1, 1, 0, 1);
            case PieceModifier.Winged:
                return new Color(0, 1, 0, 1);
            case PieceModifier.Spectral:
                return new Color(0, 1, 1, 1);
            case PieceModifier.Immune:
                return new Color(0, 0, 1, 1);
            case PieceModifier.Warped:
                return new Color(0.5f, 0, 1, 1);
            case PieceModifier.Shielded:
                return new Color(1, 1, 1, 1);
            case PieceModifier.HalfShielded:
                return new Color(0.5f, 0.5f, 0.5f, 1);
            case PieceModifier.NoSpecial:
                return new Color(0, 0, 0, 1);
        }
        return new Color(0, 0, 0, 1);
    }
    public static string GetModifierDescription(PieceModifier pm)
    {
        switch (pm)
        {
            case PieceModifier.Vengeful:
                return "Vengeful: When captured, the capturer is destroyed if it is not a King.";
            case PieceModifier.Phoenix:
                return "Phoenix: When destroyed, respawn as far back as possible and lose this Modifier.";
            case PieceModifier.Radiant:
                return "Radiant: When this piece captures, spawn a Pawn as far back as possible.";
            case PieceModifier.Winged:
                return "Winged: Ignore the first obstacle but can't capture after leaping over the obstacle.";
            case PieceModifier.Spectral:
                return "Spectral: Ally pieces are not blocked by this piece.";
            case PieceModifier.Immune:
                return "Immune: Unaffected by status effects and enchantments. Enemy pieces that are orthogonally adjacent can't capture.";
            case PieceModifier.Warped:
                return "Warped: Ally pieces can swap places with this piece if they can move onto it.";
            case PieceModifier.Shielded:
                return "Shielded: Invincible, but degrades to Half Shielded if an enemy piece threatens it at the start of their turn. Half Shielded degrades to nothing after 1 turn.";
            case PieceModifier.HalfShielded:
                return "Invincible, but Half Shielded degrades to nothing after 1 turn.";
            case PieceModifier.NoSpecial:
                return "No special movement allowed.";
        }
        return "";
    }
    public static string GetStatusEffectName(PieceStatusEffect pse)
    {
        return pse.ToString();
    }
    public static Color GetStatusEffectColor(PieceStatusEffect pse)
    {
        switch (pse)
        {
            case PieceStatusEffect.Bloodlust:
                return new Color(1, 0, 0, 1);
            case PieceStatusEffect.Sparked:
                return new Color(1, 0.5f, 0, 1);
            case PieceStatusEffect.Poisoned:
                return new Color(0, 0.5f, 0, 1);
            case PieceStatusEffect.Frozen:
                return new Color(0.5f, 1, 1, 1);
            case PieceStatusEffect.Soaked:
                return new Color(0, 0, 1, 1);
            case PieceStatusEffect.Ghostly:
                return new Color(0.75f, 0.5f, 1f, 1);
            case PieceStatusEffect.Fragile:
                return new Color(1, 1, 0.5f, 1);
            case PieceStatusEffect.Heavy:
                return new Color(0.25f, 0.25f, 0.25f, 1);
            case PieceStatusEffect.Light:
                return new Color(0.5f, 1, 0.5f, 1);
        }
        return new Color(0, 0, 0, 1);
    }
    public static string GetStatusEffectDescription(PieceStatusEffect pse)
    {
        switch (pse)
        {
            case PieceStatusEffect.Bloodlust:
                return "Piece will be destroyed if they do not capture before the effect expires. Only capturing moves are possible.";
            case PieceStatusEffect.Sparked:
                return "Piece will be destroyed if they do not move before the effect expires.";
            case PieceStatusEffect.Poisoned:
                return "Piece will be destroyed when the effect expires.";
            case PieceStatusEffect.Frozen:
                return "Piece can't move.";
            case PieceStatusEffect.Soaked:
                return "Piece can't capture.";
            case PieceStatusEffect.Ghostly:
                return "Piece pieces are not blocked by this piece.";
            case PieceStatusEffect.Fragile:
                return "If this piece captures, it is destroyed.";
            case PieceStatusEffect.Heavy:
                return "Piece automatically moves backwards every turn";
            case PieceStatusEffect.Light:
                return "Piece automatically moves forward every turn.";
        }
        return "";
    }


    public static bool IsPieceInvincible(Board b, uint piece, int x, int y, uint attackerPiece, int attackerX, int attackerY, Move.SpecialType specialType, PieceTableEntry pteA, PieceTableEntry pteV)
    {
        Piece.PieceType ptA = Piece.GetPieceType(attackerPiece);
        Piece.PieceType ptV = Piece.GetPieceType(piece);

        Piece.PieceAlignment pa = Piece.GetPieceAlignment(piece);
        Piece.PieceAlignment paA = Piece.GetPieceAlignment(attackerPiece);

        //PieceTableEntry pteA = GlobalPieceManager.GetPieceTableEntry(ptA);
        //PieceTableEntry pteV = GlobalPieceManager.GetPieceTableEntry(ptV);        

        //Voided can't capture
        if (Piece.GetPieceStatusEffect(attackerPiece) == PieceStatusEffect.Soaked)
        {
            return true;
        }

        //Shielded, Protected can't attack kings
        if (Piece.GetPieceType(piece) == PieceType.King)
        {
            PieceModifier pmA = GetPieceModifier(attackerPiece);

            if (pmA == PieceModifier.Shielded || pmA == PieceModifier.HalfShielded)
            {
                return true;
            }
        } else
        {
            //King attack only attacks kings
            if (specialType == Move.SpecialType.KingAttack)
            {
                return true;
            }
        }

        //Morph immune
        if ((specialType == Move.SpecialType.MorphRabbit) && ((pteV.piecePropertyB & PiecePropertyB.MorphImmune) != 0))
        {
            return true;
        }

        //Fire immune
        if ((specialType == Move.SpecialType.FireCapture || specialType == Move.SpecialType.FireCaptureOnly || specialType == Move.SpecialType.FireCapturePush) && ((pteV.pieceProperty & PieceProperty.FireImmune) != 0))
        {
            return true;
        }

        //Water/Ice immune
        if ((specialType == Move.SpecialType.InflictFreeze || specialType == Move.SpecialType.InflictFreezeCaptureOnly) && (((pteV.pieceProperty & PieceProperty.WaterImmune) != 0) || ((pteV.piecePropertyB & PiecePropertyB.StatusImmune) != 0) || Piece.GetPieceModifier(piece) == PieceModifier.Immune))
        {
            return true;
        }

        //Shift immune status
        if ((specialType == Move.SpecialType.InflictShift || specialType == Move.SpecialType.InflictShiftCaptureOnly) && ((((pteV.piecePropertyB & (PiecePropertyB.StatusImmune | PiecePropertyB.ShiftImmune)) != 0) || Piece.GetPieceModifier(piece) == PieceModifier.Immune)))
        {
            return true;
        }
        //Other status immune
        if (((pteV.piecePropertyB & PiecePropertyB.StatusImmune) != 0) || Piece.GetPieceModifier(piece) == PieceModifier.Immune)
        {
            switch (specialType)
            {
                case Move.SpecialType.Inflict:
                case Move.SpecialType.InflictCaptureOnly:
                    return true;
            }
        }

        //blanket ban stuff with immune
        ulong immunityBitboard = 0;
        if (pa == PieceAlignment.White)
        {
            immunityBitboard = b.globalData.bitboard_immuneWhite;
        }
        if (pa == PieceAlignment.Black)
        {
            immunityBitboard = b.globalData.bitboard_immuneBlack;
        }

        if ((pteV.pieceProperty & PieceProperty.EnchantImmune) != 0 || (1uL << x + y * 8 & immunityBitboard) != 0 || Piece.GetPieceModifier(piece) == PieceModifier.Immune)
        {
            switch (specialType)
            {
                case Move.SpecialType.EnemyAbility:
                case Move.SpecialType.RangedPull:
                case Move.SpecialType.RangedPush:
                case Move.SpecialType.Convert:
                case Move.SpecialType.ConvertPawn:
                    return true;
            }
        }

        bool victimGiant = ((pteV.piecePropertyB & PiecePropertyB.Giant) != 0);
        bool attackerGiant = ((pteA.piecePropertyB & PiecePropertyB.Giant) != 0);

        //Giants are not compatible with most stuff
        if (victimGiant)
        {
            switch (specialType)
            {
                //things that are fine
                /*
                case Move.SpecialType.Normal:
                case Move.SpecialType.MoveOnly:
                case Move.SpecialType.CaptureOnly:
                case Move.SpecialType.FlyingMoveOnly:
                case Move.SpecialType.SelfMove:
                case Move.SpecialType.FireCapture:
                case Move.SpecialType.FireCaptureOnly:
                case Move.SpecialType.LongLeaper:
                case Move.SpecialType.FireCapturePush:
                case Move.SpecialType.PullMove:
                case Move.SpecialType.PushMove:
                case Move.SpecialType.Advancer:
                case Move.SpecialType.Withdrawer:
                case Move.SpecialType.AdvancerWithdrawer:
                case Move.SpecialType.TandemMovementPawns:
                case Move.SpecialType.TandemMovementNonPawns:
                case Move.SpecialType.SlipMove:
                case Move.SpecialType.PlantMove:
                case Move.SpecialType.AllyAbility:
                case Move.SpecialType.EnemyAbility:
                case Move.SpecialType.EmptyAbility:
                case Move.SpecialType.PassiveAbility:
                    break;
                */
                case Move.SpecialType.Castling:
                //case Move.SpecialType.Convert:        I don't want Giants to just have EnchantImmune and NoTerrain for free so I'll make them weak to some stuff still
                //case Move.SpecialType.ConvertPawn:
                case Move.SpecialType.Spawn:
                case Move.SpecialType.RangedPull:
                case Move.SpecialType.RangedPullAllyOnly:
                case Move.SpecialType.RangedPush:
                case Move.SpecialType.RangedPushAllyOnly:
                case Move.SpecialType.AllySwap:
                case Move.SpecialType.AnyoneSwap:
                case Move.SpecialType.MorphIntoTarget:
                case Move.SpecialType.MorphRabbit:
                    return true;
            }
        }

        //Shift immune
        if (((pteV.piecePropertyB & PiecePropertyB.ShiftImmune) != 0))
        {
            switch (specialType)
            {
                //things that are fine
                /*
                case Move.SpecialType.Normal:
                case Move.SpecialType.MoveOnly:
                case Move.SpecialType.CaptureOnly:
                case Move.SpecialType.FlyingMoveOnly:
                case Move.SpecialType.SelfMove:
                case Move.SpecialType.FireCapture:
                case Move.SpecialType.FireCaptureOnly:
                case Move.SpecialType.LongLeaper:
                case Move.SpecialType.FireCapturePush:
                case Move.SpecialType.PullMove:
                case Move.SpecialType.PushMove:
                case Move.SpecialType.Advancer:
                case Move.SpecialType.Withdrawer:
                case Move.SpecialType.AdvancerWithdrawer:
                case Move.SpecialType.TandemMovementPawns:
                case Move.SpecialType.TandemMovementNonPawns:
                case Move.SpecialType.SlipMove:
                case Move.SpecialType.PlantMove:
                case Move.SpecialType.AllyAbility:
                case Move.SpecialType.EnemyAbility:
                case Move.SpecialType.EmptyAbility:
                case Move.SpecialType.PassiveAbility:
                    break;
                */
                case Move.SpecialType.Castling:
                case Move.SpecialType.RangedPull:
                case Move.SpecialType.RangedPullAllyOnly:
                case Move.SpecialType.RangedPush:
                case Move.SpecialType.RangedPushAllyOnly:
                case Move.SpecialType.AllySwap:
                case Move.SpecialType.AnyoneSwap:
                    return true;
            }
        }

        PieceModifier pm = GetPieceModifier(piece);        

        //Deadly overrides all of this (Except for stuff above, so it doesn't let you break the game or ignore other immunities)
        if ((pteA.pieceProperty & PieceProperty.Deadly) != 0)
        {
            return false;
        }

        if (pm == PieceModifier.Shielded || pm == PieceModifier.HalfShielded)
        {
            return true;
        }

        if ((pteV.pieceProperty & PieceProperty.Invincible) != 0)
        {
            return true;
        }

        if (pteV.type == PieceType.MegaCannon && Piece.GetPieceSpecialData(piece) != 0)
        {
            return true;
        }

        if ((pteV.piecePropertyB & PiecePropertyB.InvincibleNoEnemyAdjacent) != 0)
        {
            ulong enemy = 0;
            ulong adjacentBitboard = MainManager.SmearBitboard(1uL << (x + (y << 3)));

            if (victimGiant)
            {
                (int dx, int dy) = Board.GetGiantDelta(piece);
                adjacentBitboard = 1uL << ((x) + ((y) << 3));
                adjacentBitboard |= 1uL << ((x) + ((y + dy) << 3));
                adjacentBitboard |= 1uL << ((x + dx) + ((y) << 3));
                adjacentBitboard |= 1uL << ((x + dx) + ((y + dy) << 3));
                adjacentBitboard = MainManager.SmearBitboard(adjacentBitboard);
            }

            switch (paA)
            {
                case PieceAlignment.White:
                    enemy = b.globalData.bitboard_piecesWhite;
                    break;
                case PieceAlignment.Black:
                    enemy = b.globalData.bitboard_piecesBlack;
                    break;
            }
            enemy &= ~(1uL << (attackerX + (attackerY << 3)));
            enemy &= adjacentBitboard;

            if (enemy == 0)
            {
                return true;
            }
        }

        if (ptV == PieceType.RabbitKnight)
        {
            ulong enemy = 0;
            ulong rabbit = b.globalData.bitboard_rabbit;
            ulong adjacentBitboard = MainManager.SmearBitboard(1uL << (x + (y << 3)));

            switch (paA)
            {
                case PieceAlignment.White:
                    enemy = b.globalData.bitboard_piecesWhite & ~rabbit;
                    break;
                case PieceAlignment.Black:
                    enemy = b.globalData.bitboard_piecesBlack & ~rabbit;
                    break;
            }
            enemy &= ~(1uL << (attackerX + (attackerY << 3)));
            enemy &= adjacentBitboard;

            if (enemy == 0 && (adjacentBitboard & rabbit & ~(1uL << (x + (y << 3)))) != 0)
            {
                return true;
            }
        }


        //invincible from range 1
        if ((pteV.pieceProperty & PieceProperty.InvincibleClose) != 0)
        {
            int dx = attackerX - x;
            int dy = attackerY - y;

            if (attackerGiant)
            {
                //dx can be -2
                if (dx >= -2 && dx <= 1 && dy >= -2 && dy <= 1)
                {
                    return true;
                }
            } else
            {
                //-1 -> 1
                //faster than Math.Abs?
                dx *= dx;
                dy *= dy;
                if (dx <= 1 && dy <= 1)
                {
                    return true;
                }
            }
        }

        //range 2 invincible
        if ((pteV.pieceProperty & PieceProperty.InvincibleClose2) != 0)
        {
            int dx = attackerX - x;
            int dy = attackerY - y;

            if (attackerGiant)
            {
                //dx can be -3
                if (dx >= -3 && dx <= 2 && dy >= -3 && dy <= 2)
                {
                    return true;
                }
            } else
            {
                //-1 -> 1
                //faster than Math.Abs?
                dx *= dx;
                dy *= dy;
                if (dx <= 4 && dy <= 4)
                {
                    return true;
                }
            }
        }

        //invincible from range 2+
        //Inverse of InvincibleClose
        if ((pteV.piecePropertyB & PiecePropertyB.InvincibleFar) != 0)
        {
            int dx = attackerX - x;
            int dy = attackerY - y;

            if (attackerGiant)
            {
                //dx can be -2
                if (!(dx >= -2 && dx <= 1 && dy >= -2 && dy <= 1))
                {
                    return true;
                }
            }
            else
            {
                //-1 -> 1
                //faster than Math.Abs?
                dx *= dx;
                dy *= dy;
                if (!(dx <= 1 && dy <= 1))
                {
                    return true;
                }
            }
        }

        //range 3+ invincible
        //Inverse of InvincibleClose2
        if ((pteV.piecePropertyB & PiecePropertyB.InvincibleFar2) != 0 || ((1uL << (x + (y << 3)) & b.globalData.bitboard_square_bright) != 0) || (pa == PieceAlignment.White && ((b.globalData.playerModifier & Board.PlayerModifier.ShieldZone) != 0) && (y == 1 && (((x & 3) == 1) || ((x & 3) == 2)))))
        {
            int dx = attackerX - x;
            int dy = attackerY - y;

            if (attackerGiant)
            {
                //dx can be -3
                if (!(dx >= -3 && dx <= 2 && dy >= -3 && dy <= 2))
                {
                    return true;
                }
            }
            else
            {
                //-1 -> 1
                //faster than Math.Abs?
                dx *= dx;
                dy *= dy;
                if (!(dx <= 4 && dy <= 4))
                {
                    return true;
                }
            }
        }

        //invincible from front
        if ((pteV.pieceProperty & PieceProperty.InvincibleFront) != 0)
        {
            if (Piece.GetPieceAlignment(piece) == PieceAlignment.Black)
            {
                if (attackerY - y < 0)
                {
                    return true;
                }
            }
            else
            {
                if (attackerY - y > 0)
                {
                    return true;
                }
            }

        }

        //invincible from non pawns
        if ((pteV.pieceProperty & PieceProperty.InvincibleNonPawns) != 0 && pteA.promotionType == PieceType.Null)
        {
            return true;
        }

        //invincible from pawns
        if ((pteV.pieceProperty & PieceProperty.InvinciblePawns) != 0 && pteA.promotionType != PieceType.Null)
        {
            return true;
        }

        //can only attack Pride if attacker is >= value (or deadly condition above)
        if ((pteV.pieceProperty & PieceProperty.InvinciblePride) != 0)
        {
            if ((pteA.pieceValueX2 < pteV.pieceValueX2))
            {
                //new: last moved piece can attack Pride
                //So Pride has more possible targets but can get hit by a few more things
                int lastMovedLocation = 0;
                if (paA == PieceAlignment.White)
                {
                    lastMovedLocation = b.whitePerPlayerInfo.lastPieceMovedLocation;
                }
                if (paA == PieceAlignment.Black)
                {
                    lastMovedLocation = b.blackPerPlayerInfo.lastPieceMovedLocation;
                }
                if ((attackerX + (attackerY << 3) != lastMovedLocation))
                {
                    return true;
                }
            }
        }

        if ((pteV.pieceProperty & PieceProperty.InvincibleWrongColor) != 0 && (((x + y + attackerX + attackerY) & 1) != 0))
        {
            return true;
        }

        //capture restrictions
        //Debug.Log(ptA + " vs " + ptV + " and pteV promotion is " + pteV.promotionType);
        if ((pteA.pieceProperty & PieceProperty.OnlyCapturePawns) != 0 && pteV.promotionType == PieceType.Null)
        {
            return true;
        }

        //pride can capture deadly pieces (includes Kings)
        if ((pteA.pieceProperty & PieceProperty.InvinciblePride) != 0)
        {
            if ((pteV.pieceValueX2 < pteA.pieceValueX2) && (pteV.pieceProperty & PieceProperty.Deadly) == 0)
            {
                //new: last moved piece can attack Pride
                //So Pride has more possible targets but can get hit by a few more things
                int lastMovedLocation = 0;
                if (pa == PieceAlignment.White)
                {
                    lastMovedLocation = b.whitePerPlayerInfo.lastPieceMovedLocation;
                }
                if (pa == PieceAlignment.Black)
                {
                    lastMovedLocation = b.blackPerPlayerInfo.lastPieceMovedLocation;
                }
                if ((x + (y << 3) != lastMovedLocation))
                {
                    return true;
                }
            }
        }

        //Clockwork Turtle
        if (ptV == PieceType.ClockworkTurtle)
        {
            ulong allyBitboard = pa == Piece.PieceAlignment.Black ? b.globalData.bitboard_piecesBlack : b.globalData.bitboard_piecesWhite;
            allyBitboard = MainManager.SmearBitboard(allyBitboard & ~(1uL << x + y * 8));

            //MainManager.PrintBitboard(allyBitboard);

            if (((1uL << x + y * 8) & allyBitboard) == 0)
            {
                return true;
            }
        }

        //Temperance restriction
        //currently hardcoded number
        if (ptA == PieceType.Temperance && (pteV.pieceValueX2 <= 9 - b.GetMissingPieces(Piece.GetPieceAlignment(attackerPiece) == PieceAlignment.Black)))
        {
            return true;
        }

        return false;
    }
}