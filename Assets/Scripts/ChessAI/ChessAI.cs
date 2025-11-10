using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms.Impl;
using System.Threading;

public struct ZTableEntry   //= 32? bytes per entry
{
    public ulong hash;         //8 bytes
    public short turnCount;    //2 bytes
    public byte depth;     //1 byte
    public byte flags;     //1 byte
    public uint move;       //4 bytes
    public float score;		//4 bytes

    public const byte BOUND_EXACT = 0;
    public const byte BOUND_ALPHA = 1;  //lower bound (Known to be above this)
    public const byte BOUND_BETA = 2;   //upper bound (Known to be below this)

    public ZTableEntry(ulong hash, short turnCount, byte depth, byte flags, uint move, float score)
    {
        this.hash = hash;
        this.turnCount = turnCount;
        this.depth = depth;
        this.flags = flags;
        this.move = move;
        this.score = score;
    }
}

//Scrapped because 5% hit rate is pretty garbage
//(This is mostly because the Q nodes create a bunch of extra moves to figure out what are captures)
public struct MoveBoardCacheEntry
{
    public ulong hash;
    public uint move;
    public Board board;

    public MoveBoardCacheEntry(ulong hash, uint move, Board board)
    {
        this.hash = hash;
        this.move = move;
        this.board = board;
    }
}

public class ChessAI
{
    ZTableEntry[] zobristTable;
    public ulong zTableSize = (1 << 18);
    public ulong moveBoardCacheTableSize = (1 << 14);   //since these entries are very large I shouldn't make this too big (Boards are a lot less lightweight)

    ulong[][] zobristHashes;
    ulong[] zobristSupplemental;

    ulong[] moveHashes;

    public const float KING_CAPTURE = 160000000000; //(1 << 30);        //illegal position: invalidates any position that leads to it (Can't move into check)
    public const float WHITE_VICTORY = 100000; //(1 << 24);     //White wins (mate in X = this - X)
    public const float BLACK_VICTORY = -100000; //-(1 << 24);    //Black wins (mate in X = this + X)

    ulong nodesSearched;
    ulong quiNodeSearched;
    ulong nodesTransposed;
    ulong prunes;

    public HashSet<ulong> history;

    public Dictionary<int, Board> boardCache;

    public MoveBoardCacheEntry[] moveBoardCache;

    //Set to 1 for normal
    //Lower values make it bad (easy mode?)
    public float valueMult;
    public float randomDeviation;
    public float openingDeviation;
    public float blunderDeviation;

    public bool keepSearching = false;
    public float searchDuration;        //How long to search maximum (making it more like other engines where it's fixed time instead of fixed depth)
    public float searchTime;
    public bool errorState = false;

    public int maxDepth;
    public bool moveFound = false;
    public uint bestMove;
    public float bestEvaluation;
    public int currentDepth;
    public Board board;

    public int difficulty;

    //to stop double coroutines or threads from appearing if you stop and start the AI
    public int version = 0;

    public void InitAI(int difficulty)
    {
        Debug.Log("AI init");
        boardCache = new Dictionary<int, Board>();
        history = new HashSet<ulong>();
        ZobristTableInit();
        //MoveBoardCacheTableInit();
        nodesSearched = 0;
        quiNodeSearched = 0;
        nodesTransposed = 0;
        prunes = 0;
        version++;

        //0.2 = too easy?

        this.difficulty = difficulty;
        switch (difficulty)
        {
            case -1:
                valueMult = 0.8f;
                openingDeviation = 0.2f;   //Opening value roughly shifts by +- this (bigger on turn 1)
                randomDeviation = 0.5f;    //Value roughly shifts by +- this
                blunderDeviation = 3f;
                maxDepth = 2;   //very fast
                break;
            case 0:
                valueMult = 0.9f;
                openingDeviation = 0.1f;   //Opening value roughly shifts by +- this (bigger on turn 1)
                randomDeviation = 0.3f;    //Value roughly shifts by +- this
                blunderDeviation = 2f;
                maxDepth = 3;   //very fast
                break;
            case 1:
                valueMult = 1f;
                openingDeviation = 0.05f;
                randomDeviation = 0.1f;
                blunderDeviation = 1f;
                maxDepth = 4;   //pretty fast
                break;
            case 2:
                valueMult = 1;
                openingDeviation = 0.05f;
                randomDeviation = 0.05f;
                blunderDeviation = 0f;
                maxDepth = 8;  //this is impossible to get without running this on a supercomputer probably :P
                break;
            case 3:
                valueMult = 1;
                openingDeviation = 0.04f;
                randomDeviation = 0.04f;
                blunderDeviation = 0f;
                maxDepth = 16;  //this is impossible to get without running this on a supercomputer probably :P
                break;
        }

        //easy mode
        //0.6 value mult
        //0.1 opening deviance
        //0.1 random deviance
        //3 blunder deviance
        //but even "max difficulty" is still too easy because of problems
        /*
        valueMult = 0.6f;
        openingDeviation = 0.1f;   //Opening value roughly shifts by +- this (bigger on turn 1)
        randomDeviation = 0.3f;    //Value roughly shifts by +- this
        blunderDeviation = 3f;
        */

        //mid mode
        //Normal with max depth = 3?
        //Maybe 0.2 blunder deviance and 0.1 random deviance

        //normal
        //Probably still very easy for a chess master to win because the engine is not very good
        //1 value mult
        //0.05 opening deviance
        //0.05 random deviance
        //0 blunder deviance
        /*
        valueMult = 1;
        openingDeviation = 0.05f;
        randomDeviation = 0.05f;
        blunderDeviation = 0;
        maxDepth = 12;  //this is impossible to get without running this on a supercomputer probably :P
        */

        moveFound = false;
    }

    public void ZobristTableInit()
    {
        zobristTable = new ZTableEntry[zTableSize];

        zobristHashes = new ulong[64][];
        zobristSupplemental = new ulong[24];

        //Extra thing for the board cache
        moveHashes = new ulong[32];

        //32 bit depth?
        for (int i = 0; i < zobristHashes.Length; i++)
        {
            zobristHashes[i] = new ulong[32];

            for (int j = 0; j < zobristHashes[i].Length; j++)
            {
                uint randomA = (uint)UnityEngine.Random.Range(0, 256);
                ulong newRandom = 0;

                //is this good for avoiding correlation?
                for (int k = 0; k < 8; k++)
                {
                    newRandom |= randomA;
                    newRandom <<= 8;
                    randomA = (uint)UnityEngine.Random.Range(0, 256);
                }

                zobristHashes[i][j] = newRandom;
            }
        }

        for (int i = 0; i < zobristSupplemental.Length; i++)
        {
            uint randomA = (uint)UnityEngine.Random.Range(0, 256);
            ulong newRandom = 0;

            //is this good for avoiding correlation?
            for (int k = 0; k < 8; k++)
            {
                newRandom |= randomA;
                newRandom <<= 8;
                randomA = (uint)UnityEngine.Random.Range(0, 256);
            }

            zobristSupplemental[i] = newRandom;
        }
    }
    /*
    public void MoveBoardCacheTableInit()
    {
        moveBoardCache = new MoveBoardCacheEntry[moveBoardCacheTableSize];

        for (int i = 0; i < moveBoardCache.Length; i++)
        {
            moveBoardCache[i] = new MoveBoardCacheEntry(0, 0, new Board());
        }
    }
    */
    public void SetZTableEntry(ulong hash, ZTableEntry newEntry)
    {
        zobristTable[(uint)hash & (zobristTable.Length - 1)] = newEntry;
    }
    public ZTableEntry GetZTableEntry(ulong hash)
    {
        //return default;
        return zobristTable[(uint)hash & (zobristTable.Length - 1)];
    }

    /*
    public void SetMoveBoardCacheEntry(ulong hash, uint move, MoveBoardCacheEntry newEntry)
    {
        moveBoardCache[((uint)hash ^ move) & (moveBoardCache.Length - 1)] = newEntry;
    }
    public MoveBoardCacheEntry GetMoveBoardCacheEntry(ulong hash, uint move)
    {
        return moveBoardCache[((uint)hash ^ move) & (moveBoardCache.Length - 1)];
    }
    */


    public float GetEndgameValue(ref Board b)
    {
        /*
        int pvsum = b.whitePerPlayerInfo.pieceValueSumX2 + b.blackPerPlayerInfo.pieceValueSumX2;

        //normal = 39 + 39
        //still middlegame = 24 + 24?
        //fully endgame = 12 + 12?

        //Need to add 5 because I made the king worth 5 in case you have multiple

        float output = pvsum - 34;
        output /= (48);
        output = 1 - output;

        if (output > 1)
        {
            output = 1;
        }
        if (output < 0)
        {
            output = 0;
        }
        */

        //V2 formula
        //Based on the weaker of the 2 sides
        //Because if one side is weak then the king can start to be aggressive
        float output = 0;

        int whiteValue = b.whitePerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE;
        int blackValue = b.blackPerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE;

        int pvmin = whiteValue < blackValue ? whiteValue : blackValue;

        output = pvmin / 2f;

        //subtract out the king
        output -= 5;

        //If side left has 10 or less material left = 100% endgamey
        //If side left has 24 or more then 0% endgamey?
        //Normal army is 39 base

        output = (output - 10) / 14f;
        if (output > 1)
        {
            output = 1;
        }
        if (output < 0)
        {
            output = 0;
        }

        return 1 - output;
    }

    public float EvaluateBoard(ref Board b, ulong boardHash, List<uint> moves)
    {
        return EvaluateBoardFunction(ref b, boardHash, moves);
    }

    public float EvaluateBoardFunction(ref Board b, ulong boardHash, List<uint> moves)
    {
        Piece.PieceAlignment winner = b.GetVictoryCondition();
        if (winner != PieceAlignment.Null)
        {
            //Has precedence
            //because moving into check for a win condition feels very contrary to normal chess logic
            //It seems fine for multi king because you can intuitively understand why you can ignore check in that case
            if (!b.CheckForKings() || Board.IsKingCapturePossible(ref b, moves))
            {
                return KING_CAPTURE;
            }

            switch (winner)
            {
                case PieceAlignment.White:
                    return WHITE_VICTORY;
                case PieceAlignment.Black:
                    return BLACK_VICTORY;
                case PieceAlignment.Neutral:
                    return 0;
            }
        }

        //Debug.Log("Start eval");
        //Note that this is immediately after a GenerateMoves call so the bitboards for the board are properly set up
        //So you can count value of crystal pieces

        float value = 0;
        float endgameValue = GetEndgameValue(ref b);

        //Piece value sum
        float pieceValueSum = ((b.whitePerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - (b.blackPerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f;

        //Easy mode thing
        pieceValueSum *= valueMult;

        value += pieceValueSum;

        //Crystal pieces
        float crystalMaterial = EvaluateCrystalPieceMaterial(ref b);
        value += crystalMaterial;

        //Pieces with modifiers
        //should incentivize keeping Shielded instead of wasting it
        float modifierMaterial = EvaluateModifierMaterial(ref b);
        value += modifierMaterial;

        //Castling value
        //Small bonus for castling
        if (!b.whitePerPlayerInfo.canCastle)
        {
            value += 0.2f;
        }
        if (!b.blackPerPlayerInfo.canCastle)
        {
            value -= 0.2f;
        }

        //Piece table values
        float pstDelta = GetPSTDelta(ref b, endgameValue);
        value += pstDelta;

        float kingSafety = EvaluateKingSafety(ref b, endgameValue);
        value += kingSafety * valueMult * valueMult;    //Bad AI undervalues king safety

        float passedPawns = GetPassedPawns(ref b, endgameValue);
        value += passedPawns;

        float endgameKingCloseness = EvaluateEndgameKingCloseness(ref b, endgameValue);
        //hacky setup
        //the side that's winning in the endgame wants the kings to be close, the other side wants to run away
        //Should encourage the winning side to get closer to a checkmating situation
        value *= endgameKingCloseness;

        //Random delta
        value += (((((boardHash >> 4) & 15)) / 16f) - ((((boardHash >> 20) & 15)) / 16f)) * randomDeviation;
        if ((boardHash & 511) <= 2)   //6/512 (since this shows up in the search tree this will be worse than it looks?)
        {
            //this gives a triangle distribution?
            value += blunderDeviation * (((((boardHash >> 8) & 15)) / 16f) - ((((boardHash >> 16) & 15)) / 16f));
        }

        //Debug.Log("End eval");

        //Debug.Log(b.whitePerPlayerInfo.pieceValueSumX2 + " " + b.blackPerPlayerInfo.pieceValueSumX2);
        //Debug.Log(pieceValueSum + " " + modifierMaterial + " " + pstDelta + " " + kingSafety + " endgame = " + endgameValue + " Total Material = " + ((b.whitePerPlayerInfo.pieceValueSumX2 + b.blackPerPlayerInfo.pieceValueSumX2) & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE));
        //Debug.Log(value);
        return value;
    }

    public float EvaluateCrystalPieceMaterial(ref Board b)
    {
        ulong bitboard_crystalWhite = b.globalData.bitboard_crystalWhite;
        ulong bitboard_crystalBlack = b.globalData.bitboard_crystalBlack;

        //Relatively low so you don't sacrifice stuff to get a crystal piece for 1 turn
        //But it needs to be high enough that the AI gets steered towards using them
        float crystalMultiplier = 0.2f;
        float value = 0;

        while (bitboard_crystalWhite != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard_crystalWhite, out bitboard_crystalWhite);

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[index]));
            value += crystalMultiplier * (pte.pieceValueX2) * 0.5f;
        }
        while (bitboard_crystalBlack != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard_crystalBlack, out bitboard_crystalBlack);

            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(Piece.GetPieceType(b.pieces[index]));
            value -= crystalMultiplier * (pte.pieceValueX2) * 0.5f;
        }

        return value;
    }

    //Modifiers are pretty good
    //But they are only so good?
    //Status effects are bad
    //But they vary in how bad they are
    //And the badness varies on duration a bit
    //      Poison and the like are worse if they are low turn count
    //      The non poison ones are better at low turn count
    public float EvaluateModifierMaterial(ref Board b)
    {
        ulong bitboard_white = b.globalData.bitboard_piecesWhite;
        ulong bitboard_black = b.globalData.bitboard_piecesBlack;

        float output = 0;
        while (bitboard_white != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard_white, out bitboard_white);

            PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(index, b.pieces[index]); //GlobalPieceManager.GetPieceTableEntry(b.pieces[index]);

            if (pte == null)
            {
                continue;
            }

            switch (Piece.GetPieceModifier(b.pieces[index]))
            {
                case PieceModifier.Phoenix:
                    output += pte.pieceValueX2 * 0.75f;
                    break;
                case PieceModifier.Winged:
                    output += pte.pieceValueX2 * 0.25f;
                    break;
                case PieceModifier.Vengeful:
                case PieceModifier.Radiant:
                case PieceModifier.Spectral:
                case PieceModifier.Warped:
                case PieceModifier.Shielded:
                    output += 2.5f;
                    break;
                case PieceModifier.Immune:
                case PieceModifier.HalfShielded:
                    output += 1.5f;
                    break;
            }

            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(b.pieces[index]);
            byte pd = Piece.GetPieceStatusDuration(b.pieces[index]);

            float valueMult = 1f;

            //effects must be halved because it gives PieceValue value normally
            switch (pse)
            {
                case PieceStatusEffect.Frozen:
                    valueMult *= 1 - (0.15f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Heavy:
                case PieceStatusEffect.Fragile:
                case PieceStatusEffect.Soaked:
                    valueMult *= 1 - (0.1f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Light:
                case PieceStatusEffect.Ghostly:
                    valueMult *= 1 - (0.05f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Poisoned:    //not normally curable so it is worse
                    valueMult *= 1 - (0.175f * (1 + 1 / pd));
                    break;
                case PieceStatusEffect.Sparked:
                case PieceStatusEffect.Bloodlust:
                    valueMult *= 1 - (0.25f * (1 / pd));
                    break;
            }

            if (pte.type == PieceType.Revenant)
            {
                if (Piece.GetPieceSpecialData(b.pieces[index]) != 0)
                {
                    valueMult *= 0.5f;
                } else
                {
                    valueMult *= 0.5f;
                    valueMult += 0.5f;
                }
            }

            if (pte.type == PieceType.MegaCannon)
            {
                ushort data = Piece.GetPieceSpecialData(b.pieces[index]);

                int targetX = data & 7;
                int targetY = (data & 56) >> 3;
                int timer = data >> 6;

                if (b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output += (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output += (timer - 4);
                    }
                }

                targetX += 1;
                if (targetX <= 7 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output += (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output += (timer - 4);
                    }
                }

                targetX -= 2;
                if (targetX >= 0 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output += (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output += (timer - 4);
                    }
                }

                targetX += 1;
                targetY += 1;
                if (targetY <= 7 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output += (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output += (timer - 4);
                    }
                }

                targetY -= 2;
                if (targetY >= 0 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.Black)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output += (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output += (timer - 4);
                    }
                }
            }

            output -= this.valueMult * ((1 - valueMult) * pte.pieceValueX2);

            if ((pte.pieceProperty & PieceProperty.ConsumeAllies) != 0)
            {
                float value = pte.pieceValueX2 * 0.25f;
                if (value > 1.5f)
                {
                    value = 1.5f;
                }
                value *= Piece.GetPieceSpecialData(b.pieces[index]);
                output += value;
            }
            if ((pte.piecePropertyB & PiecePropertyB.ChargeByMoving) != 0)
            {
                float value = pte.pieceValueX2 * 0.05f;
                if (value > 1.5f)
                {
                    value = 1.5f;
                }
                value *= Piece.GetPieceSpecialData(b.pieces[index]);
                output += value;
            }
        }
        while (bitboard_black != 0)
        {
            int index = MainManager.PopBitboardLSB1(bitboard_black, out bitboard_black);

            PieceTableEntry pte = b.globalData.GetPieceTableEntryFromCache(index, b.pieces[index]); //GlobalPieceManager.GetPieceTableEntry(b.pieces[index]);

            if (pte == null)
            {
                continue;
            }

            switch (Piece.GetPieceModifier(b.pieces[index]))
            {
                case PieceModifier.Phoenix:
                    output -= pte.pieceValueX2 * 0.75f;
                    break;
                case PieceModifier.Winged:
                    output -= pte.pieceValueX2 * 0.25f;
                    break;
                case PieceModifier.Vengeful:
                case PieceModifier.Radiant:
                case PieceModifier.Spectral:
                case PieceModifier.Warped:
                case PieceModifier.Shielded:
                    output -= 2.5f;
                    break;
                case PieceModifier.Immune:
                case PieceModifier.HalfShielded:
                    output -= 1.5f;
                    break;
            }

            Piece.PieceStatusEffect pse = Piece.GetPieceStatusEffect(b.pieces[index]);
            byte pd = Piece.GetPieceStatusDuration(b.pieces[index]);
            float valueMult = 1f;

            //effects must be halved because it gives PieceValue value normally
            switch (pse)
            {
                case PieceStatusEffect.Frozen:
                    valueMult *= 1 - (0.15f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Heavy:
                case PieceStatusEffect.Fragile:
                case PieceStatusEffect.Soaked:
                    valueMult *= 1 - (0.1f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Light:
                case PieceStatusEffect.Ghostly:
                    valueMult *= 1 - (0.05f + 0.03125f * (pd));
                    break;
                case PieceStatusEffect.Poisoned:    //not normally curable so it is worse
                    valueMult *= 1 - (0.175f * (1 + 1 / pd));
                    break;
                case PieceStatusEffect.Sparked:
                case PieceStatusEffect.Bloodlust:
                    valueMult *= 1 - (0.25f * (1 / pd));
                    break;
            }

            if (pte.type == PieceType.Revenant)
            {
                if (Piece.GetPieceSpecialData(b.pieces[index]) != 0)
                {
                    valueMult *= 0.5f;
                }
                else
                {
                    valueMult *= 0.5f;
                    valueMult += 0.5f;
                }
            }


            if (pte.type == PieceType.MegaCannon)
            {
                ushort data = Piece.GetPieceSpecialData(b.pieces[index]);

                int targetX = data & 7;
                int targetY = (data & 56) >> 3;
                int timer = data >> 6;

                if (b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.White)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output -= (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output -= (timer - 4);
                    }
                }

                targetX += 1;
                if (targetX <= 7 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.White)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output -= (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output -= (timer - 4);
                    }
                }

                targetX -= 2;
                if (targetX >= 0 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.White)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output -= (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output -= (timer - 4);
                    }
                }

                targetX += 1;
                targetY += 1;
                if (targetY <= 7 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.White)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output -= (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output -= (timer - 4);
                    }
                }

                targetY -= 2;
                if (targetY >= 0 && b.pieces[targetX + (targetY << 3)] != 0 && Piece.GetPieceAlignment(b.pieces[targetX + (targetY << 3)]) == PieceAlignment.White)
                {
                    PieceTableEntry pteC = b.globalData.GetPieceTableEntryFromCache((targetX + (targetY << 3)), b.pieces[targetX + (targetY << 3)]);
                    output -= (timer / 30f) * (pteC.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * valueMult;
                    if (pteC.type == PieceType.King && timer >= 5)
                    {
                        output -= (timer - 4);
                    }
                }
            }

            output += this.valueMult * ((1 - valueMult) * pte.pieceValueX2);

            if ((pte.pieceProperty & PieceProperty.ConsumeAllies) != 0)
            {
                float value = pte.pieceValueX2 * 0.25f;
                if (value > 1.5f)
                {
                    value = 1.5f;
                }
                value *= Piece.GetPieceSpecialData(b.pieces[index]);
                output -= value;
            }
            if ((pte.piecePropertyB & PiecePropertyB.ChargeByMoving) != 0)
            {
                float value = pte.pieceValueX2 * 0.05f;
                if (value > 1.5f)
                {
                    value = 1.5f;
                }
                value *= Piece.GetPieceSpecialData(b.pieces[index]);
                output -= value;
            }
        }
        return output;
    }

    public float GetPSTDelta(ref Board b, float endgameValue)
    {
        float value = 0;
        float psqt = 0;
        for (int i = 0; i < b.pieces.Length; i++)
        {
            psqt = 0;
            if (b.pieces[i] == 0)
            {
                continue;
            }

            Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceAlignment pa = Piece.GetPieceAlignment(b.pieces[i]);
            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

            /*
            if (pt == Piece.PieceType.Null)
            {
                continue;
            }
            */

            if (pt == Piece.PieceType.King)
            {
                //Make this more aggressive
                psqt += 2 * (endgameValue * GlobalPieceManager.Instance.ReadPSTTopCenter(pa, i) + (1 - endgameValue) * GlobalPieceManager.Instance.ReadPSTEdge(pa, i));
            }
            else if (pte.promotionType != Piece.PieceType.Null)
            {
                //Get top center a little bit even outside of endgame
                psqt += (0.85f * endgameValue + 0.15f) * GlobalPieceManager.Instance.ReadPSTTopCenterKing(pa, i) + (0.85f - 0.85f * endgameValue) * GlobalPieceManager.Instance.ReadPSTCenter(pa, i);
            }
            else
            {
                psqt += GlobalPieceManager.Instance.ReadPSTCenter(pa, i);
            }

            if (pa == Piece.PieceAlignment.White)
            {
                value += psqt;
            } else if (pa == Piece.PieceAlignment.Black)
            {
                value -= psqt;
            }
        }

        return value;
    }

    public float EvaluateKingSafety(ref Board b, float endgameValue)
    {
        float value = 0;

        //prefer pawns near king vs stronger stuff
        float strongPieceNearKing = 0.01f;
        float weakPieceNearKing = 0.15f;

        //King safety
        ulong wBitboard = 0;
        ulong bBitboard = 0;
        ulong kingBitboardW = 0;
        int kingPopW = 0;           //if you have multi kings these factors get massively reduced (*0.2?) (because you can afford to lose one of them to an attack?) (they still appear so the AI still wants to chase them down)
        float kingPopPenaltyW = 1;
        ulong kingBitboardB = 0;
        int kingPopB = 0;
        float kingPopPenaltyB = 1;

        float midPenaltyW = 0.5f;
        float midPenaltyB = 0.5f;

        //Populate these bitboards
        for (int i = 0; i < b.pieces.Length; i++)
        {
            if (b.pieces[i] == 0)
            {
                continue;
            }

            Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceAlignment pa = Piece.GetPieceAlignment(b.pieces[i]);

            /*
            if (pt == PieceType.Null)
            {
                continue;
            }
            */

            if (pa == Piece.PieceAlignment.White)
            {
                wBitboard |= 1uL << i;
            }
            if (pa == Piece.PieceAlignment.Black)
            {
                bBitboard |= 1uL << i;
            }

            if (pt == Piece.PieceType.King && pa == Piece.PieceAlignment.White)
            {
                kingBitboardW |= 1uL << i;
                kingPopW++;
                if ((i & 7) > 1 && (i & 7) < 6)   //care even less about kings in mid
                {
                    midPenaltyW *= 0.4f;
                }
            }
            if (pt == Piece.PieceType.King && pa == Piece.PieceAlignment.Black)
            {
                kingBitboardB |= 1uL << i;
                kingPopB++;
                if ((i & 7) > 1 && (i & 7) < 6)   //care even less about kings in mid
                {
                    midPenaltyB *= 0.4f;
                }
            }
        }

        if (kingPopW > 1)
        {
            kingPopPenaltyW *= 0.2f;
        }
        if (kingPopB > 1)
        {
            kingPopPenaltyB *= 0.2f;
        }

        ulong kingDist1BitboardW = MainManager.SmearBitboard(kingBitboardW);
        ulong kingDist2BitboardW = MainManager.SmearBitboard(kingDist1BitboardW);
        kingDist1BitboardW &= ~kingBitboardW;
        kingDist2BitboardW &= ~kingDist1BitboardW;

        ulong kingDist1BitboardB = MainManager.SmearBitboard(kingBitboardB);
        ulong kingDist2BitboardB = MainManager.SmearBitboard(kingDist1BitboardB);
        kingDist1BitboardB &= ~kingBitboardB;
        kingDist2BitboardB &= ~kingDist1BitboardB;

        //now do stuff
        for (int i = 0; i < 64; i++)
        {
            if (b.pieces[i] == 0)
            {
                continue;
            }

            Piece.PieceType pt = Piece.GetPieceType(b.pieces[i]);
            Piece.PieceAlignment pa = Piece.GetPieceAlignment(b.pieces[i]);
            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);
            ulong bitboardIndex = 1uL << i;

            if (pt == PieceType.Null)
            {
                continue;
            }

            //King safety: ally pieces
            //King tropism: enemy pieces cause penalties based on their value
            if ((bitboardIndex & kingDist1BitboardW) != 0)
            {
                if (pa == Piece.PieceAlignment.White)
                {
                    if (pte.promotionType == Piece.PieceType.Null)
                    {
                        value += weakPieceNearKing * kingPopPenaltyW * midPenaltyW;
                    } else
                    {
                        value += strongPieceNearKing * kingPopPenaltyW * midPenaltyW;
                    }
                } else
                {
                    float tropismValue = (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * 0.02f + 0.2f;
                    if (tropismValue > 0.4f)
                    {
                        tropismValue = 0.4f;
                    }
                    value -= tropismValue * kingPopPenaltyW * midPenaltyW;
                }
            }

            //weaker boost for dist 2
            if ((bitboardIndex & kingDist2BitboardW) != 0)
            {
                if (pa == Piece.PieceAlignment.White)
                {
                    if (pte.promotionType == Piece.PieceType.Null)
                    {
                        value += weakPieceNearKing * 0.5f * kingPopPenaltyW * midPenaltyW;
                    }
                    else
                    {
                        value += strongPieceNearKing * 0.5f * kingPopPenaltyW * midPenaltyW;
                    }
                }
                else
                {
                    float tropismValue = (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * 0.01f + 0.1f;
                    if (tropismValue > 0.2f)
                    {
                        tropismValue = 0.2f;
                    }
                    value -= tropismValue * kingPopPenaltyW * midPenaltyW;
                }
            }

            //the same for black
            if ((bitboardIndex & kingDist1BitboardB) != 0)
            {
                if (pa == Piece.PieceAlignment.Black)
                {
                    if (pte.promotionType == Piece.PieceType.Null)
                    {
                        value -= weakPieceNearKing * kingPopPenaltyB * midPenaltyB;
                    }
                    else
                    {
                        value -= strongPieceNearKing * kingPopPenaltyB * midPenaltyB;
                    }
                }
                else
                {
                    //a pretty big floor
                    //Even a pawn next to the king is a bad thing to allow (It lets stronger pieces worm their way in)
                    float tropismValue = (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * 0.02f + 0.2f;
                    if (tropismValue > 0.4f)
                    {
                        tropismValue = 0.4f;
                    }
                    value += tropismValue * kingPopPenaltyB * midPenaltyB;
                }
            }

            //weaker boost for dist 2
            if ((bitboardIndex & kingDist2BitboardB) != 0)
            {
                if (pa == Piece.PieceAlignment.White)
                {
                    if (pte.promotionType == Piece.PieceType.Null)
                    {
                        value -= weakPieceNearKing * 0.5f * kingPopPenaltyB * midPenaltyB;
                    }
                    else
                    {
                        value -= strongPieceNearKing * 0.5f * kingPopPenaltyB * midPenaltyB;
                    }
                }
                else
                {
                    float tropismValue = (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) * 0.01f + 0.1f;
                    if (tropismValue > 0.2f)
                    {
                        tropismValue = 0.2f;
                    }
                    value += tropismValue * kingPopPenaltyB * midPenaltyB;
                }
            }
        }

        //care less about king safety in endgame
        return value * (1.1f - endgameValue);
    }

    public float GetPassedPawns(ref Board b, float endgameValue)
    {
        float output = 0;

        //Check all pawns
        ulong whitePawnBitboard = b.globalData.bitboard_pawnsWhite;
        ulong blackPawnBitboard = b.globalData.bitboard_pawnsBlack;

        ulong afilebitboard = 0x0101010101010101;

        while (whitePawnBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(whitePawnBitboard, out whitePawnBitboard);

            //The file ahead of you
            ulong newBitboard = afilebitboard << index + 8;
            if ((index & 7) != 0)
            {
                newBitboard |= afilebitboard << index + 7;
            }
            if ((index & 7) != 7)
            {
                newBitboard |= afilebitboard << index + 9;
            }

            //Debug.Log("Pawn at " + (index & 7) + " " + ((index & 56) >> 3));
            //MainManager.PrintBitboard(newBitboard);

            if ((b.globalData.bitboard_pawnsBlack & newBitboard) == 0)
            {
                output += 0.15f;
                output += ((index & 56) >> 3) * 0.15f;
            }
        }
        while (blackPawnBitboard != 0)
        {
            int index = MainManager.PopBitboardLSB1(blackPawnBitboard, out blackPawnBitboard);

            //The file below
            ulong newBitboard = afilebitboard >> (64 - index);
            if ((index & 7) != 7)
            {
                newBitboard |= afilebitboard >> (63 - index);
            }
            if ((index & 7) != 0)
            {
                newBitboard |= afilebitboard >> (65 - index);
            }

            //Debug.Log("Pawn at " + (index & 7) + " " + ((index & 56) >> 3));
            //MainManager.PrintBitboard(newBitboard);

            if ((b.globalData.bitboard_pawnsWhite & newBitboard) == 0)
            {
                output -= 0.15f;
                output -= (7 - ((index & 56) >> 3)) * 0.15f;
            }
        }

        //int index = MainManager.PopBitboardLSB1(subBitboard, out subBitboard);

        //+0.15 for each passed pawn
        //+0.05 for each rank ahead it is

        //The PST and this should sum to +1 or +2 which should incentivize you to promote stuff because I made the promoted units have higher value?

        //Passed pawns have less value with more things that can stop them
        return output * (0.2f + 0.8f * endgameValue);
    }

    public float EvaluateEndgameKingCloseness(ref Board b, float endgameValue)
    {
        if (endgameValue < 0.7f)
        {
            return 1;
        }

        ulong kingBitboard = b.globalData.bitboard_kingWhite;

        int distance = 0;
        while (distance < 8)
        {
            kingBitboard = MainManager.SmearBitboard(kingBitboard);
            distance++;

            if ((kingBitboard & b.globalData.bitboard_kingBlack) != 0) 
            {
                break;
            }
        }

        //the distance can never be 0 because I don't check for distance 0
        float extraDelta = (0.13f * (1f / distance)) * ((endgameValue - 0.7f) / 0.3f);
        //Debug.Log(distance);
        return (1.0f + extraDelta);
    }

    /*
    public uint GetBestMove(ref Board b)
    {
        board = b;
        keepSearching = true;
        searchTime = 0;
        moveFound = false;

        Thread searchThread = new Thread(new ParameterizedThreadStart(AlphaBetaAI));
        searchThread.Start();

        //return AlphaBetaAI(ref b, 2);
        //return NaiveAI(ref b);
        return bestMove;
    }
    */

    //coroutine
    public IEnumerator BestMoveCoroutine(bool errorState)
    {
        int localVersion = version;

        //board = b;
        keepSearching = true;
        searchTime = 0;
        moveFound = false;
        //this.errorState = errorState;

        //ehh
        if (errorState || valueMult == 0)
        {
            NaiveAI();
            yield break;
        }

        //debug testing in profiler
        /*
        if (board != null)
        {
            maxDepth = 3;
            AlphaBetaAI(null);
        }
        */

        Thread searchThread = new Thread(new ParameterizedThreadStart(AlphaBetaAI));
        searchThread.Start();

        bool timeUp = false;
        while (!moveFound)
        {
            //Something weird happened
            if (version != localVersion)
            {
                searchThread.Abort();
                yield break;
            }

            //Uh oh
            if (!moveFound && searchThread.ThreadState == ThreadState.Stopped && keepSearching && !timeUp)
            {
                //successful search ends it early
                if (localVersion == version)
                {
                    Debug.LogError("Search thread stopped for some reason");
                }
                yield break;
            }

            if (!timeUp)
            {
                if (searchTime < searchDuration)
                {
                    searchTime += Time.deltaTime;
                }
                else
                {
                    timeUp = true;
                    //Time's up
                    //but the search thread might be wrapping things up
                    keepSearching = false;
                    searchTime = 0;
                }
            }
            yield return null;
        }
        //yield return bestMove;
    }

    public string TranslateEval(float eval)
    {
        if (eval == KING_CAPTURE)
        {
            return "KC";
        }

        if (eval > WHITE_VICTORY / 2)
        {
            eval -= WHITE_VICTORY;
            return "#" + -eval;
        }
        if (eval < BLACK_VICTORY / 2)
        {
            eval -= BLACK_VICTORY;
            return "-#" + eval;
        }

        return eval + "";
    }

    public ulong HashFromScratch(ref Board b)
    {
        return b.MakeZobristHashFromScratch(zobristHashes, zobristSupplemental);
    }
    public ulong HashFromScratch(Board b)
    {
        return b.MakeZobristHashFromScratch(zobristHashes, zobristSupplemental);
    }

    public Board GetBoardFromHistoryCache(int turn)
    {
        //lazy idea
        if (!boardCache.ContainsKey(turn))
        {
            boardCache[turn] = new Board();
        }
        return boardCache[turn];
    }

    public void TryPrintBestLine()
    {
        int ply = board.ply;

        string output = "Turn " + board.turn + ": ";

        while (true)
        {
            ply++;
            if (boardCache.ContainsKey(ply))
            {
                output += " " + Move.ConvertToStringMinimal(boardCache[ply].GetLastMove());
            } else
            {
                break;
            }
        }

        Debug.Log(output);
    }

    //Very quick depth 1 search
    //Just choose whatever makes number go up (while still being legal)
    public void NaiveAI()
    {
        Board b = board;

        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        float bestEvaluation = WHITE_VICTORY;
        uint bestMove = 0;

        Board copy = GetBoardFromHistoryCache(b.ply); //new Board();
        for (int i = 0; i < moves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);
            if (!Board.IsKingCapturePossible(ref copy))
            {
                float evaluation = EvaluateBoard(ref copy, HashFromScratch(ref copy), null);

                if (evaluation < bestEvaluation)
                {
                    bestEvaluation = evaluation;
                    bestMove = moves[i];
                }
            }
        }

        moveFound = true;
        this.bestMove = bestMove;
        //return bestMove;
    }

    //Random choice of moves
    public void RandomAI()
    {
        Board b = board;

        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        uint bestMove = 0;

        List<uint> shuffledMoves = new List<uint>();
        while (moves.Count > 0)
        {
            uint newMove = moves[UnityEngine.Random.Range(0, moves.Count)];
            moves.Remove(newMove);
            shuffledMoves.Add(newMove);
        }

        Board copy = GetBoardFromHistoryCache(b.ply); //new Board();
        for (int i = 0; i < shuffledMoves.Count; i++)
        {
            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);
            if (!Board.IsKingCapturePossible(ref copy))
            {
                moveFound = true;
                this.bestMove = shuffledMoves[i];
            }
        }

        moveFound = true;
        this.bestMove = bestMove;
    }

    public void AlphaBetaAI(object o)
    {
        //I can make this as big as I want now :)
        int maxDepth = this.maxDepth;
        currentDepth = 0;

        //there might be some thread unsafe data manipulation causing weird blunders?
        Board b = new Board(board);
        b.SplitGlobalData();

        uint bestMove = 0;
        this.bestEvaluation = 0;
        float bestEvaluation = 0;
        //Debug.Log("Start alpha beta as " + (b.blackToMove ? "Black" : "White"));
        long dt = 0;
        int maxDepthReached = 0;
        float realBestEvaluation = 0;
        for (int i = 0; i <= maxDepth; i++)
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();

            maxDepthReached = i;
            nodesSearched = 0;
            nodesTransposed = 0;
            quiNodeSearched = 0;
            prunes = 0;
            (bestMove, bestEvaluation) = AlphaBetaSearch(ref b, HashFromScratch(ref b), i, 0, 0, float.MinValue, float.MaxValue);
            currentDepth = i;

            /*
            ulong lastHash = HashFromScratch(ref b);
            ZTableEntry zte = GetZTableEntry(lastHash);
            if (zte.hash == lastHash)
            {
                Debug.Log("Last hash says best move is " + Move.ConvertToStringMinimal(zte.move));
            } else
            {
                Debug.Log("No last hash");
            }
            */

            //(i == maxDepth || !keepSearching)
            /*
            if (!float.IsNaN(bestEvaluation))
            {
                Debug.Log("AB searched eval " + bestEvaluation + " depth = " + i + " " + nodesSearched + " nodes searched, " + nodesTransposed + " nodes saved by transposition, " + quiNodeSearched + " qui nodes, " + prunes + " prunes");
            }
            */

            currentTime = DateTime.UtcNow;
            long unixTimeEnd = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            dt = (unixTimeEnd - unixTime);
            
            /*
            if (!float.IsNaN(bestEvaluation))
            {
                Debug.Log("Bestmove = " + (Move.ConvertToString(bestMove)) + " Eval = " + TranslateEval(bestEvaluation) + " Search took " + ((unixTimeEnd - unixTime) / 1000d) + " seconds for " + (nodesSearched + nodesTransposed + quiNodeSearched) + " positions at depth + " + i + " = " + "(" + ((nodesSearched + nodesTransposed + quiNodeSearched) / ((unixTimeEnd - unixTime) / 1000d)) + " nodes/sec)");
            }
            */

            if (!float.IsNaN(bestEvaluation) && bestMove != 0)
            {
                realBestEvaluation = bestEvaluation;
                //Debug.Log("Found move " + bestMove);
                this.bestMove = bestMove;
                this.bestEvaluation = bestEvaluation;
            }

            //The game is already over so there is no point to searching any moves?
            if (bestEvaluation == KING_CAPTURE || bestEvaluation == WHITE_VICTORY || bestEvaluation == BLACK_VICTORY)
            {
                realBestEvaluation = bestEvaluation;
                //this.bestMove = 0;
                this.bestMove = bestMove;   //? need to fix it breaking when its about to win as white?
                this.bestEvaluation = bestEvaluation;
                break;
            }

            //Mate in X is kind of pointless to search further?
            //Problem: there are bugs where the mate evaluations are not fully stable (i.e. it evaluates #X but a higher depth evaluates a normal number or #(> X))
            //Hopefully these are not that bad?
            if (bestEvaluation > WHITE_VICTORY / 2 || bestEvaluation < BLACK_VICTORY / 2)
            {
                realBestEvaluation = bestEvaluation;
                //this.bestMove = 0;
                this.bestMove = bestMove;   //? need to fix it breaking when its about to win as white?
                this.bestEvaluation = bestEvaluation;
                break;
            }

            if (!keepSearching)
            {
                break;
            }
        }

        Debug.Log((b.blackToMove ? "Black" : "White") + " Bestmove = " + (Move.ConvertToString(this.bestMove)) + " Eval = " + TranslateEval(realBestEvaluation) + " Search took " + (dt / 1000d) + " seconds for " + (nodesSearched + nodesTransposed + quiNodeSearched) + " positions with " + prunes + " prunes, " + nodesTransposed + " transposes, " + quiNodeSearched + " qui nodes at depth + " + maxDepthReached + " = " + "(" + ((nodesSearched + nodesTransposed + quiNodeSearched) / (dt / 1000d)) + " nodes/sec)");

        if (this.bestMove == 0)
        {
            //error: need to failsafe I guess?
            Debug.LogError("Null move failsafe");
            NaiveAI();
        }


        //TryPrintBestLine();

        moveFound = true;
        //this.bestMove = bestMove;

        //return bestMove;

        //invalidate the thread looper thing
        version++;
    }

    public float MoveScore(ref Board b, ref Board copy, ulong oldHash, uint move, Dictionary<uint, bool> nonpassiveDict)
    {
        float score = 0;

        //New setup: transposition table moves have priority
        //They get a big bonus
        //Then mvv lva

        //some terms are not multiplied (i.e. the mvv lva terms)
        int mult = b.blackToMove ? -1 : 1;

        //try to use board cache
        /*
        MoveBoardCacheEntry mbce = GetMoveBoardCacheEntry(oldHash, move);

        if (mbce.hash == oldHash && mbce.move == move)
        {
            //Cache hit (Saves 1 ApplyMove call)
            //copy = mbce.board;
            moveboardcachehits++;
            copy.CopyOverwrite(mbce.board);
        } else
        {
            moveboardcachemisses++;
            //Cache miss
            copy.CopyOverwrite(b);
            copy.ApplyMove(move);

            mbce.hash = oldHash;
            mbce.move = move;
            mbce.board.CopyOverwrite(copy);
            SetMoveBoardCacheEntry(oldHash, move, mbce);
        }
        */
        copy.CopyOverwrite(b);
        copy.ApplyMove(move);

        ulong newHash = copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, oldHash);
        ZTableEntry zte = GetZTableEntry(newHash);

        if (!copy.CheckForKings())
        {
            return KING_CAPTURE;
        }

        //Consult transposition table
        //If it is M1 or something just use that immediately because you can't really do better than that with any other kind of move
        if (zte.hash == newHash)
        {
            if (zte.score > WHITE_VICTORY / 2 || zte.score < BLACK_VICTORY / 2)
            {
                return zte.score * mult;
            }

            score += zte.score * mult * 1 + 1000;
            //return score;
        }

        //MVV LVA
        //Instead of MVV I will just detect the lost material
        //And LVA is based on piece value of attacker

        int lostMaterial = 0;
        if (b.blackToMove)
        {
            lostMaterial = b.whitePerPlayerInfo.pieceValueSumX2 - copy.whitePerPlayerInfo.pieceValueSumX2;
        } else
        {
            lostMaterial = b.blackPerPlayerInfo.pieceValueSumX2 - copy.blackPerPlayerInfo.pieceValueSumX2;
        }

        if (score != 0)
        {
            if (lostMaterial > 0 && !nonpassiveDict.ContainsKey(move))
            {
                nonpassiveDict.Add(move, true);
            }
            return score;
        }

        score += lostMaterial * 1f;

        //LVA
        int x = Move.GetFromX(move);
        int y = Move.GetFromY(move);

        Piece.PieceType pt = Piece.GetPieceType(b.pieces[x + y * 8]);

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

        if (lostMaterial > 0)
        {
            score += 0.1f - (0.001f * (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) + 500;
        }

        return score;

    }
    public float QMoveScore(ref Board b, ref Board copy, ulong oldHash, uint move, bool precondition)
    {
        float score = 0;

        int fxy = Move.GetFromXYInt(move);
        int txy = Move.GetToXYInt(move);
        bool passPrecondition = Piece.GetPieceAlignment(b.pieces[fxy]) != Piece.GetPieceAlignment(b.pieces[txy]) && b.pieces[txy] != 0;

        if (precondition)
        {
            if (passPrecondition)
            {
                return -1;
            }
        }

        //New setup: transposition table moves have priority
        //They get a big bonus
        //Then mvv lva

        //some terms are not multiplied (i.e. the mvv lva terms)
        int mult = b.blackToMove ? -1 : 1;

        //try to use board cache
        /*
        MoveBoardCacheEntry mbce = GetMoveBoardCacheEntry(oldHash, move);

        if (mbce.hash == oldHash && mbce.move == move)
        {
            //Cache hit (Saves 1 ApplyMove call)
            //copy = mbce.board;
            moveboardcachehits++;
            copy.CopyOverwrite(mbce.board);
        }
        else
        {
            moveboardcachemisses++;
            //Cache miss
            copy.CopyOverwrite(b);
            copy.ApplyMove(move);

            mbce.hash = oldHash;
            mbce.move = move;
            mbce.board.CopyOverwrite(copy);
            SetMoveBoardCacheEntry(oldHash, move, mbce);
        }
        */
        copy.CopyOverwrite(b);
        copy.ApplyMove(move, null, false);

        //zte check will often fail because it's missing the turn end stuff?
        //But with no status effects, terrains, etc it will still pass
        ulong newHash = copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, oldHash);
        ZTableEntry zte = GetZTableEntry(newHash);

        //Somehow this is causing bugs I don't know why
        if (!copy.CheckForKings())
        {
            return KING_CAPTURE;
        }

        //Consult transposition table
        //If it is M1 or something just use that immediately because you can't really do better than that with any other kind of move
        if (zte.hash == newHash)
        {
            if (zte.score > WHITE_VICTORY / 2 || zte.score < BLACK_VICTORY / 2)
            {
                return zte.score * mult;
            }
            //score += zte.score * mult * 1 + 1000;
        }

        //MVV LVA
        //Instead of MVV I will just detect the lost material
        //And LVA is based on piece value of attacker

        int lostMaterial = 0;
        if (b.blackToMove)
        {
            lostMaterial = b.whitePerPlayerInfo.pieceValueSumX2 - copy.whitePerPlayerInfo.pieceValueSumX2;
        }
        else
        {
            lostMaterial = b.blackPerPlayerInfo.pieceValueSumX2 - copy.blackPerPlayerInfo.pieceValueSumX2;
        }

        //legit captures get some boost
        if (copy.GetLastMoveCapture())
        {
            score += 1000;
        }

        score += lostMaterial * 1f;

        //LVA
        int x = Move.GetFromX(move);
        int y = Move.GetFromY(move);

        Piece.PieceType pt = Piece.GetPieceType(b.pieces[x + y * 8]);

        PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

        if (lostMaterial > 0)
        {
            score += 0.1f - (0.001f * (pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE));

            if ((pte.pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) >= lostMaterial)
            {
                return 0;
            }
        }

        /*
        if (lostMaterial > 0 ^ passPrecondition)
        {
            Debug.Log(passPrecondition + " " + lostMaterial + " " + Move.ConvertToString(move) + " " + Piece.ConvertToString(b.pieces[fxy]) + " " + Piece.ConvertToString(b.pieces[txy]));
        }
        */

        return score;

    }

    public (uint, float) AlphaBetaSearch(ref Board b, ulong boardOldHash, int depth, int ext, int red, float alpha, float beta, HashSet<uint> killerMoves = null)
    {
        nodesSearched++;

        //Try to find it in the Z table
        ZTableEntry zte = GetZTableEntry(boardOldHash);

        HashSet<uint> currentKillerMoves = new HashSet<uint>();

        //Usable?
        //Triggers instant cutoff?

        uint bestMove = 0;
        float bestEvaluation = float.NaN;

        if (zte.hash == boardOldHash)
        {
            bestMove = zte.move;
            //Root node is given by killerMoves == null
            if (zte.depth >= depth && !history.Contains(boardOldHash) && killerMoves != null)
            {
                //Exact number: can use immediately
                if (zte.flags == ZTableEntry.BOUND_EXACT)
                {
                    nodesTransposed++;
                    return (zte.move, zte.score);
                }

                if (b.blackToMove)
                {
                    //Reduce the score (check for < alpha)
                    if (zte.flags == ZTableEntry.BOUND_ALPHA)
                    {
                        if (zte.score <= alpha)
                        {
                            nodesTransposed++;
                            return (zte.move, zte.score);
                        }
                    }
                }
                else
                {
                    //Increase the score (check for > beta)
                    if (zte.flags == ZTableEntry.BOUND_BETA)
                    {
                        if (zte.score >= beta)
                        {
                            nodesTransposed++;
                            return (zte.move, zte.score);
                        }
                    }
                }
            }
        }

        //Max depth: use Q search
        if (depth <= 0)
        {
            uint oldMove = b.GetLastMove();
            (uint qmove, float qevaluation) = QSearch(ref b, boardOldHash, Move.GetToX(oldMove), Move.GetToY(oldMove), alpha, beta, 0);
            return (qmove, qevaluation);
        }

        //Do a search


        Board copy = GetBoardFromHistoryCache(b.ply); //new Board();
        copy.CopyOverwrite(b);

        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);

        //If you already won then just return that immediately
        PieceAlignment winner = b.GetVictoryCondition();
        if (winner != PieceAlignment.Null)
        {
            //Has precedence
            //because moving into check for a win condition feels very contrary to normal chess logic
            //It seems fine for multi king because you can intuitively understand why you can ignore check in that case
            uint kingcapture = Board.FindKingCaptureMove(ref b, moves);
            if (kingcapture != 0)
            {
                //return (0, KING_CAPTURE);
                return (kingcapture, KING_CAPTURE);
            }

            if (winner == PieceAlignment.White)
            {
                SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, 0, WHITE_VICTORY));
                return (0, WHITE_VICTORY);
            }
            if (winner == PieceAlignment.Black)
            {
                SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, 0, BLACK_VICTORY));
                return (0, BLACK_VICTORY);
            }
        }

        //new setup means that the hash may change
        boardOldHash = b.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref copy, boardOldHash);
        //copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash)

        //score moves
        //Catastrophically bad idea(?)
        //Removing it didn't make any difference?
        //My engine is still glacially slow :(
        //I think this will kill my performance but idk how else to do this
        Dictionary<uint, float> scoreDict = new Dictionary<uint, float>();
        Dictionary<uint, bool> nonpassiveDict = new Dictionary<uint, bool>();


        int newExt = ext;

        
        //Null move pruning is buggy right now so I turned it off
        //It also isn't helping me at all?
        /*
        bool allowNullEvaluation = depth > 1;
        allowNullEvaluation = false;

        if (b.whitePerPlayerInfo.pieceValueSumX2 < 10 || b.blackPerPlayerInfo.pieceValueSumX2 < 10)
        {
            allowNullEvaluation = false;
        }
        if (alpha == float.MinValue || alpha == beta)    //Root node should not try null move, don't use multi null moves
        {
            allowNullEvaluation = false;
        }

        uint nullMoveBest;
        float nullEvaluation;
        //This also generates possible killer moves to try
        if (allowNullEvaluation)
        {
            if (b.blackToMove)
            {
                copy.CopyOverwrite(b);
                copy.ApplyNullMove();
                //0 window search
                (nullMoveBest, nullEvaluation) = AlphaBetaSearch(ref copy, copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash), (depth - 2), ext, red, alpha, alpha, currentKillerMoves);

                if (nullEvaluation != KING_CAPTURE && nullEvaluation <= alpha)
                {
                    return (0, nullEvaluation);
                }
            }
            else
            {
                copy.CopyOverwrite(b);
                copy.ApplyNullMove();
                //0 window search
                (nullMoveBest, nullEvaluation) = AlphaBetaSearch(ref copy, copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash), (depth - 2), ext, red, beta, beta, currentKillerMoves);

                if (nullEvaluation != KING_CAPTURE && nullEvaluation >= beta)
                {
                    return (0, nullEvaluation);
                }
            }
            //if you are in check, null move returns King Capture
            //  (Note: maybe not if it gets pruned out early)
            //  (But this scenario 
            //Check extension
            if (nullEvaluation == KING_CAPTURE)
            {
                if (ext < 1)
                {
                    newExt++;
                }
            }
        }
        */

        /*
        if (killerMoves != null && killerMoves.Count > 0)
        {
            MainManager.PrintMoveList(moves);
            MainManager.PrintMoveSet(killerMoves);
        }
        */
        for (int i = 0; i < moves.Count; i++)
        {
            //Ordering problem: MVA LVV ordering can put some idiotic moves in front (If the queen gets pulled out early the extremely bad "kamikaze queen" captures get searched first)
            //I guess it also removes all the benefit of iterative deepening? :P
            //So I should make the transposition table take precedence over MVA LVV
            //If a move is better according to the table the idiotic moves should get pruned out

            //King capture is always sorted to the front
            scoreDict[moves[i]] = MoveScore(ref b, ref copy, boardOldHash, moves[i], nonpassiveDict);

            if (scoreDict[moves[i]] == KING_CAPTURE)
            {
                //Bad
                //return (0, KING_CAPTURE);
                return (moves[i], KING_CAPTURE);
            }

            //Don't touch hash move scores with this?
            //Maybe touching MVVLVA scores is fine
            if (killerMoves != null && scoreDict[moves[i]] < 900 && killerMoves.Contains(Move.RemoveNonLocation(moves[i])))
            {
                //A small ish bonus
                scoreDict[moves[i]] += 25;
            }
        }

        //force the best move to be searched first
        if (bestMove != 0 && scoreDict.ContainsKey(bestMove))
        {
            scoreDict[bestMove] = 50000;
        }

        int FloatCompare(float a, float b)
        {
            if (a == b)
            {
                return 0;
            }
            if (a > b)
            {
                return 1;
            } else
            {
                return -1;
            }
        }

        //sort moves
        //put the best ones first
        moves.Sort((a, b) => -FloatCompare(scoreDict[a], scoreDict[b]));
        //moves.OrderBy((e) => -scoreDict[e]);

        //Debug.Log("Post order");

        int passiveMoves = 0;
        int lateReductionThreshold = moves.Count / 3;
        if (lateReductionThreshold < 3)
        {
            lateReductionThreshold = 3;
        }
        bool doReduction = false;

        /*
        if (alpha == float.MinValue && beta == float.MaxValue)
        {
            Debug.Log("Last best: " + Move.ConvertToStringMinimal(bestMove) + " " + (scoreDict.ContainsKey(bestMove) ? scoreDict[bestMove] : "X"));
        }
        */
        //MoveBoardCacheEntry mbce;

        for (int i = 0; i < moves.Count; i++)
        {
            /*
            if (alpha == float.MinValue && beta == float.MaxValue)
            {
                Debug.Log(Move.ConvertToStringMinimal(moves[i]) + " " + scoreDict[moves[i]] + " " + depth);
            }
            */

            /*
            if (depth == 3)
            {
                Debug.Log("Score " + scoreDict[moves[i]] + " " + Move.ConvertToString(moves[i]));
            }
            */

            /*
            float ms = MoveScore(ref b, ref copy, boardOldHash, moves[i]);
            if (ms != 0)
            {
                Debug.Log("ms " + ms);
            }
            */

            //try to use board cache
            /*
            mbce = GetMoveBoardCacheEntry(boardOldHash, moves[i]);

            if (mbce.hash == boardOldHash && mbce.move == moves[i])
            {
                //Cache hit (Saves 1 ApplyMove call)
                //copy = mbce.board;
                moveboardcachehits++;
                copy.CopyOverwrite(mbce.board);
            }
            else
            {
                moveboardcachemisses++;
                //Cache miss
                copy.CopyOverwrite(b);
                copy.ApplyMove(moves[i]);

                mbce.hash = boardOldHash;
                mbce.move = moves[i];
                mbce.board.CopyOverwrite(copy);
                SetMoveBoardCacheEntry(boardOldHash, moves[i], mbce);
            }
            */

            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);


            //scorer finds this early
            /*
            if (!copy.CheckForKings())
            {
                //King capture move is possible in this position therefore this position is illegal
                return (moves[i], KING_CAPTURE);
            }
            */

            /*
            if (copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash) != copy.MakeZobristHashFromScratch(zobristHashes, zobristSupplemental))
            {
                Debug.Log(copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash) + " vs " + copy.MakeZobristHashFromScratch(zobristHashes, zobristSupplemental));
            }
            */

            int newRed = red;
            /*
            if (newRed < 0 + (depth * 0.333f))
            {
                newRed = (int)(0 + (depth * 0.333f));
            }
            */
            if (newRed < 1 + (depth * 0.15f))
            {
                newRed = (int)(1 + (depth * 0.15f));
            }
            if (!nonpassiveDict.ContainsKey(moves[i]))
            {
                passiveMoves++;
                if (depth > 2 && passiveMoves >= lateReductionThreshold) // && scoreDict[moves[i]] == 0)
                {
                    doReduction = true;
                }
            }

            uint candidateMove;
            float candidateEval;

            ulong chash = copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash);
            if (doReduction)
            {
                (candidateMove, candidateEval) = AlphaBetaSearch(ref copy, chash, (depth - 1 - (newRed - red) + (newExt - ext)), newExt, newRed, alpha, beta, currentKillerMoves);
            }
            else
            {
                (candidateMove, candidateEval) = AlphaBetaSearch(ref copy, chash, (depth - 1 + (newExt - ext)), newExt, red, alpha, beta, currentKillerMoves);
            }

            if (history.Contains(chash))
            {
                PenalizeMove(candidateEval, 0.25f);
            }

            if (killerMoves == null)
            {
                if (((chash >> 32) & 127) <= 7)   //10/128 chance of a blunder I guess?
                {
                    //shift right so the check bits and the bits that determine value are not the same
                    candidateEval += blunderDeviation * (((((chash >> 24) & 15)) / 16f) - ((((chash >> 28) & 15)) / 16f));
                }
            }


            if (!keepSearching)
            {
                return (bestMove, bestEvaluation);
            }

            if (candidateEval == KING_CAPTURE || float.IsNaN(candidateEval))
            {
                //That move is illegal
                continue;
            }

            if (b.turn < 3)
            {
                candidateEval += ((((boardOldHash & 15) - 8f) / 16f) * openingDeviation * (3 - b.turn));
            }

            if (b.blackToMove)
            {
                //Reduce the score (beta cutoff)
                if (float.IsNaN(bestEvaluation) || candidateEval < bestEvaluation)
                {
                    if (doReduction && newRed != red)
                    {
                        //re-search
                        //Wastes time but is unavoidable
                        (candidateMove, candidateEval) = AlphaBetaSearch(ref copy, copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash), (depth - 1), ext, red, alpha, beta, currentKillerMoves);
                    }

                    bestEvaluation = candidateEval;
                    bestMove = moves[i];
                    /*
                    if (depth == 3)
                    {
                        Debug.Log(Move.ConvertToString(bestMove) + " " + bestEvaluation);
                    } 
                    */
                }

                if (bestEvaluation < beta)
                {
                    beta = bestEvaluation;
                }

                if (bestEvaluation <= alpha)
                {
                    prunes++;
                    if (killerMoves != null)
                    {
                        uint newKiller = Move.RemoveNonLocation(bestMove);
                        killerMoves.Add(newKiller);
                    }

                    //double check this isn't an illegal position?
                    if (Board.IsKingCapturePossible(ref b, moves))
                    {
                        //King capture move is possible in this position therefore this position is illegal
                        return (moves[i], KING_CAPTURE);
                    }

                    //Populate the Z table
                    ulong newHash = copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash);
                    SetZTableEntry(newHash, new ZTableEntry(newHash, (byte)copy.turn, (byte)depth, ZTableEntry.BOUND_ALPHA, bestMove, bestEvaluation));
                    return (bestMove, bestEvaluation);
                }
            }
            else
            {
                //Increase the score (alpha cutoff)
                if (float.IsNaN(bestEvaluation) || candidateEval > bestEvaluation)
                {
                    if (doReduction && newRed != red)
                    {
                        //re-search
                        //Wastes time but is unavoidable
                        (candidateMove, candidateEval) = AlphaBetaSearch(ref copy, copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash), (depth - 1), ext, red, alpha, beta, currentKillerMoves);
                    }

                    bestEvaluation = candidateEval;
                    bestMove = moves[i];
                }

                if (bestEvaluation > alpha)
                {
                    alpha = bestEvaluation;
                }

                if (bestEvaluation >= beta)
                {
                    prunes++;
                    if (killerMoves != null)
                    {
                        uint newKiller = Move.RemoveNonLocation(bestMove);
                        killerMoves.Add(newKiller);
                    }

                    //double check this isn't an illegal position?
                    if (Board.IsKingCapturePossible(ref b, moves))
                    {
                        //King capture move is possible in this position therefore this position is illegal
                        return (moves[i], KING_CAPTURE);
                    }

                    //Populate the Z table
                    ulong newHash = copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash);
                    SetZTableEntry(newHash, new ZTableEntry(newHash, (byte)copy.turn, (byte)depth, ZTableEntry.BOUND_BETA, bestMove, bestEvaluation));
                    return (bestMove, bestEvaluation);
                }
            }
        }

        //Successful in search

        //Every move leads to king capture = stalemate or checkmate
        if (float.IsNaN(bestEvaluation) || bestEvaluation > KING_CAPTURE / 2)
        {
            if (Board.PositionIsCheck(ref b))
            {
                //Checkmate :(
                if (b.blackToMove)
                {
                    SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, 0, WHITE_VICTORY));
                    return (0, WHITE_VICTORY);
                } else
                {
                    SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, 0, BLACK_VICTORY));
                    return (0, BLACK_VICTORY);
                }
            } else
            {
                //Stalemate :|
                //Populate the Z table still
                SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, 0, 0));
                return (0, 0);
            }
        }

        //Mate in X
        if (bestEvaluation > WHITE_VICTORY / 2)
        {
            bestEvaluation = bestEvaluation - 1;
        }
        if (bestEvaluation < BLACK_VICTORY / 2)
        {
            bestEvaluation = bestEvaluation + 1;
        }

        //Populate the Z table
        SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, (byte)depth, ZTableEntry.BOUND_EXACT, bestMove, bestEvaluation));
        return (bestMove, bestEvaluation);
    }

    public float PenalizeMove(float score, float multPenalty)
    {
        if (score > WHITE_VICTORY / 2 || score < BLACK_VICTORY / 2)
        {
            //still get penalized slightly
            if (multPenalty == 1)
            {
                return score;
            } else
            {
                if (score > 0)
                {
                    return score - 0.5f;
                }
                else
                {
                    return score + 0.5f;
                }
            }
        }

        return score * multPenalty;
    }

    //Reduced search: check for things that can capture last moved
    //This does similar minimax stuff
    //Does not check transposition table?
    public (uint, float) QSearch(ref Board b, ulong boardOldHash, int x, int y, float alpha, float beta, int qdepth, HashSet<uint> killerMoves = null)
    {
        quiNodeSearched++;
        HashSet<uint> currentKillerMoves = new HashSet<uint>();

        //Something has gone catastrophically wrong
        //need to limit it so it doesn't stack overflow
        //Unfortunately for me a stack overflow in the AI thread is invisible and doesn't show me any error messages >:(
        if (qdepth >= 10)
        {
            //Debug.LogError("Too much quiescence, last move = " + Move.ConvertToString(b.GetLastMove()));
            return (0, float.NaN);
        }

        //Try to find it in the Z table
        ZTableEntry zte = GetZTableEntry(boardOldHash);

        float repetitionPenalty = 0.25f;
        if (!history.Contains(boardOldHash))
        {
            repetitionPenalty = 1;
        }

        uint bestMove = 0;
        float evaluation = float.NaN;

        //Usable?
        //Triggers instant cutoff?
        if (zte.hash == boardOldHash)
        {
            bestMove = zte.move;

            //Don't use the 0 depth entries because you might be considering a different square Q search?
            if (zte.depth > 0)
            {
                //Exact number: can use immediately
                if (zte.flags == ZTableEntry.BOUND_EXACT)
                {
                    nodesTransposed++;
                    return (zte.move, PenalizeMove(zte.score, repetitionPenalty));
                }

                if (b.blackToMove)
                {
                    //Reduce the score (check for < alpha)
                    if (zte.flags == ZTableEntry.BOUND_ALPHA)
                    {
                        if (zte.score < beta)
                        {
                            nodesTransposed++;
                            return (zte.move, PenalizeMove(zte.score, repetitionPenalty));
                        }
                    }
                }
                else
                {
                    //Increase the score (check for > beta)
                    if (zte.flags == ZTableEntry.BOUND_BETA)
                    {
                        if (zte.score > alpha)
                        {
                            nodesTransposed++;
                            return (zte.move, PenalizeMove(zte.score, repetitionPenalty));
                        }
                    }
                }
            }
        }

        Board copy = GetBoardFromHistoryCache(b.ply);
        copy.CopyOverwrite(b);

        List<uint> moves = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moves, ref b, b.blackToMove ? PieceAlignment.Black : PieceAlignment.White, null);
        boardOldHash = b.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref copy, boardOldHash);

        //Null move might actually better?
        //You are not forced to do all these captures, so not doing them might be better
        float nullEvaluation = EvaluateBoard(ref b, boardOldHash, moves);

        if (nullEvaluation == KING_CAPTURE)
        {
            return (0, KING_CAPTURE);
        }
        if (nullEvaluation == WHITE_VICTORY)
        {
            return (0, WHITE_VICTORY);
        }
        if (nullEvaluation == BLACK_VICTORY)
        {
            return (0, BLACK_VICTORY);
        }


        //?
        //If you have a good capture available you should do better than the null evaluation
        //The alpha and beta cutoffs mean that a bad capture will be avoided as the null evaluation is better for the player than them so the cutoff would be avoided
        //

        evaluation = nullEvaluation;
        if (b.blackToMove)
        {
            if (nullEvaluation < beta)
            {
                beta = nullEvaluation;
            }
        } else
        {
            if (nullEvaluation > alpha)
            {
                alpha = nullEvaluation;
            }
        }       

        //this seems like an impossible scenario
        /*
        if (alpha > beta)
        {
            return (0, float.NaN);
        }
        */


        //I want to sort the moves still because there can still be good pruning
        //score moves
        //in alpha beta search this is below the win condition check
        //but this is the king capture check also so it has to be before
        Dictionary<uint, float> scoreDict = new Dictionary<uint, float>();

        for (int i = 0; i < moves.Count; i++)
        {
            scoreDict[moves[i]] = QMoveScore(ref b, ref copy, boardOldHash, moves[i], qdepth > 0);

            //If you see a KING CAPTURE you can just stop immediately
            if (scoreDict[moves[i]] == KING_CAPTURE)
            {
                //Debug.Log(Move.ConvertToString(moves[i]) + " is kingcapture");
                //Is this the cause of the bugs?
                //SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_EXACT, bestMove, KING_CAPTURE));
                return (moves[i], KING_CAPTURE);
            }
        }

        //inefficient but important to have?
        //Do I need this
        //I can just make future Q searches include king captures
        //No? Maybe it finds a move that blunders a queen and prunes early but doesn't even see that king capture is possible
        //no: move score setup now forces king captures to the front
        /*
        if (Board.IsKingCapturePossible(ref b, moves))
        {
            //Populate the Z table
            SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (byte)b.turn, 0, ZTableEntry.BOUND_EXACT, bestMove, KING_CAPTURE));
            return (0, KING_CAPTURE);
        }
        */
        //If you already won then just return that immediately
        //redundant: null evaluation finds these values first

        /*
        PieceAlignment winner = b.GetVictoryCondition();
        if (winner != PieceAlignment.Null)
        {
            //this check is superceded by the above check
            //Has precedence
            //because moving into check for a win condition feels very contrary to normal chess logic
            //It seems fine for multi king because you can intuitively understand why you can ignore check in that case
            if (Board.IsKingCapturePossible(ref b))
            {
                return (0, KING_CAPTURE);
            }

            if (winner == PieceAlignment.White)
            {
                SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_EXACT, 0, WHITE_VICTORY));
                return (0, WHITE_VICTORY);
            }
            if (winner == PieceAlignment.Black)
            {
                SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_EXACT, 0, BLACK_VICTORY));
                return (0, BLACK_VICTORY);
            }
        }
        */

        int FloatCompare(float a, float b)
        {
            if (a == b)
            {
                return 0;
            }
            if (a > b)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        //sort moves
        //put the best ones first

        //only consider the quiescence stuff that has a chance of getting seen
        //List<uint> moveSubset = moves.FindAll((e) => (scoreDict[e] > 0 || (Move.GetToX(e) == x && Move.GetToY(e) == y)));
        List<uint> moveSubset = new List<uint>();

        for (int i = 0; i < moves.Count; i++)
        {
            //removed the toX and toY check because it would end up in the score dict if it was capture
            //int txy = Move.GetToX(moves[i]) + Move.GetToY(moves[i]) * 8;
            //int fxy = Move.GetFromX(moves[i]) + Move.GetFromY(moves[i]) * 8;

            if (qdepth > 5)
            {
                //only legit captures get in
                if (scoreDict[moves[i]] > 1000)    // || ((txy & 7) == x && (txy >> 3) == y) && b.pieces[txy] != 0 && Piece.GetPieceAlignment(b.pieces[txy]) != Piece.GetPieceAlignment(b.pieces[fxy]))
                {
                    moveSubset.Add(moves[i]);
                }
            }
            else
            {
                if (scoreDict[moves[i]] > 0)    // || ((txy & 7) == x && (txy >> 3) == y) && b.pieces[txy] != 0 && Piece.GetPieceAlignment(b.pieces[txy]) != Piece.GetPieceAlignment(b.pieces[fxy]))
                {
                    moveSubset.Add(moves[i]);
                }
            }
        }


        moveSubset.Sort((a, b) => -FloatCompare(scoreDict[a], scoreDict[b]));

        for (int i = 0; i < moveSubset.Count; i++)
        {
            /*
            if (Move.GetToX(moves[i]) == x && Move.GetToY(moves[i]) == y)
            {
                //double check this is a capture and not some exotic other kind of move?
                //Push moves would cause infinite quiescence recursion?

                if (Move.GetSpecialType(moves[i]) == Move.SpecialType.PushMove || Move.GetSpecialType(moves[i]) == Move.SpecialType.PullMove)
                {
                    int tx = Move.GetToX(moves[i]);
                    int ty = Move.GetToY(moves[i]);

                    //Target must be non zero and different alignment
                    if (b.pieces[tx + ty * 8] == 0 || Piece.GetPieceAlignment(b.pieces[tx + ty * 8]) == Piece.GetPieceAlignment(copy.pieces[tx + ty * 8]))
                    {
                        continue;
                    }
                }
            }
            */

            /*
            bool considerMove = false;

            copy.CopyOverwrite(b);
            copy.ApplyMove(moves[i]);

            //int condition = 0;
            if (Move.GetToX(moves[i]) == x && Move.GetToY(moves[i]) == y)
            {
                //double check this is a capture and not some exotic other kind of move?
                //Push moves would cause infinite quiescence recursion :(
                //2 pushers push each other back and forth which satisfies the position check infinitely while the enemy mops up your pieces
                //Can also do the same thing with pullers
                //Maybe I can just change the capture X,Y check to fix this?
                //No it didn't work
                considerMove = true;
                //condition = 1;

                if (Move.GetSpecialType(moves[i]) == Move.SpecialType.PushMove || Move.GetSpecialType(moves[i]) == Move.SpecialType.PullMove)
                {
                    int tx = Move.GetToX(moves[i]);
                    int ty = Move.GetToY(moves[i]);

                    //Target must be non zero and different alignment
                    if (b.pieces[tx + ty * 8] == 0 || Piece.GetPieceAlignment(b.pieces[tx + ty * 8]) == Piece.GetPieceAlignment(copy.pieces[tx + ty * 8]))
                    {
                        considerMove = false;
                    }
                }
            }

            //Consider all weak takes strong captures also
            //These moves are also rare enough to not cause branching factor problems
            //(This helps fix problems with pins against strong pieces)
            //Problem: what about the tree branch where 

            if (!considerMove)
            {
                //condition = 2;
                int lostMaterial = 0;
                if (b.blackToMove)
                {
                    lostMaterial = b.whitePerPlayerInfo.pieceValueSumX2 - copy.whitePerPlayerInfo.pieceValueSumX2;
                }
                else
                {
                    lostMaterial = b.blackPerPlayerInfo.pieceValueSumX2 - copy.blackPerPlayerInfo.pieceValueSumX2;
                }

                //LVA
                int fx = Move.GetFromX(moves[i]);
                int fy = Move.GetFromY(moves[i]);

                Piece.PieceType pt = Piece.GetPieceType(b.pieces[fx + fy * 8]);

                PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(pt);

                if (pte.pieceValueX2 < lostMaterial)
                {
                    considerMove = true;
                }
            }
            */

            //if (considerMove)
            //{
            //Debug.Log("Condition " + condition + " consider " + Move.ConvertToString(moves[i]));
            //Debug.Log(Move.ConvertToString(moveSubset[i]) + " " + scoreDict[moveSubset[i]] + " with qui target square " + x + " " + y);

            //?
            //how are score 0 moves getting in
            /*
            if (scoreDict[moveSubset[i]] == 0)
            {
                break;
            }
            */

            //try to use board cache
            /*
            mbce = GetMoveBoardCacheEntry(boardOldHash, moveSubset[i]);

            if (mbce.hash == boardOldHash && mbce.move == moveSubset[i])
            {
                //Cache hit (Saves 1 ApplyMove call)
                //copy = mbce.board;
                moveboardcachehits++;
                copy.CopyOverwrite(mbce.board);
            }
            else
            {
                moveboardcachemisses++;
                //Cache miss
                copy.CopyOverwrite(b);
                copy.ApplyMove(moveSubset[i]);

                mbce.hash = boardOldHash;
                mbce.move = moveSubset[i];
                mbce.board.CopyOverwrite(copy);
                SetMoveBoardCacheEntry(boardOldHash, moveSubset[i], mbce);
            }
            */

            copy.CopyOverwrite(b);
            copy.ApplyMove(moveSubset[i]);

            (uint candidateMove, float candidateEval) = QSearch(ref copy, copy.MakeZobristHashFromDelta(zobristHashes, zobristSupplemental, ref b, boardOldHash), Move.GetToX(moveSubset[i]), Move.GetToY(moveSubset[i]), alpha, beta, qdepth + 1);

            //not safe to return a value here as the position may not be quiet?
            if (!keepSearching)
            {
                return (bestMove, float.NaN);
            }

            if (candidateEval == KING_CAPTURE)
            {
                continue;
            }

            if (b.blackToMove)                    
            {
                //Reduce the score (beta cutoff)
                if (float.IsNaN(evaluation) || candidateEval < evaluation)
                {
                    evaluation = candidateEval;
                    bestMove = moveSubset[i];
                }

                if (evaluation < beta)
                {
                    beta = evaluation;
                }

                if (evaluation <= alpha)
                {
                    prunes++;
                    //Populate the Z table
                    SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_ALPHA, bestMove, evaluation));
                    return (bestMove, PenalizeMove(evaluation, repetitionPenalty));
                }
            }
            else
            {
                //Increase the score (alpha cutoff)
                if (float.IsNaN(evaluation) || candidateEval > evaluation)
                {
                    evaluation = candidateEval;
                    bestMove = moveSubset[i];
                }

                if (evaluation > alpha)
                {
                    alpha = evaluation;
                }

                if (evaluation >= beta)
                {
                    prunes++;
                    //Populate the Z table
                    SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_BETA, bestMove, evaluation));
                    return (bestMove, PenalizeMove(evaluation, repetitionPenalty));
                }
            }
            //}
        }

        /*
        if (float.IsNaN(evaluation))
        {
            //give up on finding move?
            //Need to handle this specially later?
            return (0, EvaluateBoard(ref b));
        } else
        {
        }
        */

        //Above case is no longer necessary as the board was evaluated once before
        //Populate the Z table
        SetZTableEntry(boardOldHash, new ZTableEntry(boardOldHash, (short)b.ply, 0, ZTableEntry.BOUND_EXACT, bestMove, evaluation));
        return (bestMove, PenalizeMove(evaluation, repetitionPenalty));
    }
}
