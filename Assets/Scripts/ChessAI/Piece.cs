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
        Courtesan,
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