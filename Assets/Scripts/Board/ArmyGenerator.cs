using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece;

public static class ArmyGenerator
{
    public static Piece.PieceType[] GenerateArmy(float targetNonKingValue, int pieceTypes, float lowPieceBias, float lowComplexityBias, Piece.PieceClass pieceClass, Board.EnemyModifier em)
    {
        int typeValue = pieceTypes;
        float tryValue = targetNonKingValue;

        List<PieceTableEntry> piecePool = new List<PieceTableEntry>();

        for (int i = 0; i < GlobalPieceManager.pieceTable.Length; i++)
        {
            if (GlobalPieceManager.pieceTable[i] == null || GlobalPieceManager.pieceTable[i].type == Piece.PieceType.King || GlobalPieceManager.pieceTable[i].type == Piece.PieceType.Rock)
            {
                continue;
            }

            if (pieceClass == Piece.PieceClass.None || GlobalPieceManager.pieceTable[i].pieceClass == pieceClass)
            {
                piecePool.Add(GlobalPieceManager.pieceTable[i]);
            }
        }

        float highestValue = 0;
        for (int i = 0; i < piecePool.Count; i++)
        {
            if (piecePool[i].pieceValueX2 / 2f > highestValue)
            {
                highestValue = piecePool[i].pieceValueX2 / 2f;
            }
        }
        highestValue++;

        RandomTable<PieceTableEntry> pieceTable = GetPieceTable(pieceClass, typeValue, tryValue, lowPieceBias, lowComplexityBias);

        List<PieceTableEntry> subArmy = new List<PieceTableEntry>();

        float cumulativeValueCount = 0;
        int cumulativeCount = 0;
        int giantCount = 0;
        int immobileCount = 0;

        //force this not to be an infinite loop if you give it an impossible task
        //(i.e. try to get above 9 * 23 value with normal chess pieces because the queen is already 9 value)
        int iterations = 0;

        float lowerBound = 0;

        List<PieceTableEntry> hardcodeTable = pieceTable.GetAllOutput();

        List<Piece.PieceType> extraPieces = Board.EnemyModifierExtraPieces(em);

        /*
        float maxValue = tryValue / 2;
        for (int i = 0; i < extraPieces.Count; i++)
        {
            tryValue -= GlobalPieceManager.GetPieceTableEntry(extraPieces[i]).pieceValueX2 / 2f;
        }
        if (tryValue < maxValue)
        {
            tryValue = maxValue;
        }
        */
        float maxValue = tryValue / 2;
        if (em != 0)
        {
            float power = Board.GetEnemyModifierPower(em);
            tryValue -= power;
        }
        if (tryValue < maxValue)
        {
            tryValue = maxValue;
        }

        /*
        string hardcodeOutput = "";
        for (int i = 0; i < hardcodeTable.Count; i++)
        {
            hardcodeOutput += hardcodeTable[i].type + " ";
        }
        Debug.Log("Hardcode: " + hardcodeOutput);
        */

        while (cumulativeValueCount < tryValue)
        {
            iterations++;
            if (iterations > 10000)
            {
                break;
            }

            PieceTableEntry pteTarget = pieceTable.Output();

            if (pteTarget == null)
            {
                break;
            }

            //overpowered
            if (pteTarget.type == PieceType.Prince && (em & Board.EnemyModifier.KingSpecialModifiers) != 0)
            {
                continue;
            }

            //Attempt to increase variety
            //int subiterations = 0;
            List<PieceTableEntry> uniqueTable = new List<PieceTableEntry>();
            for (int i = 0; i < subArmy.Count; i++)
            {
                if (uniqueTable.Contains(subArmy[i]))
                {
                    continue;
                }
                uniqueTable.Add(subArmy[i]);
            }
            //Debug.Log(uniqueTable.Count);
            if (uniqueTable.Count < typeValue)
            {
                //Bias this towards low value pieces I guess
                if (hardcodeTable.Count > 0)
                {
                    if (hardcodeTable.Count == 1)
                    {
                        int hIndexA = UnityEngine.Random.Range(0, hardcodeTable.Count);
                        pteTarget = hardcodeTable[hIndexA];
                        hardcodeTable.RemoveAt(hIndexA);
                    }
                    else
                    {
                        int hIndexA = UnityEngine.Random.Range(0, hardcodeTable.Count);
                        int hIndexB = UnityEngine.Random.Range(0, hardcodeTable.Count - 1);
                        if (hIndexB >= hIndexA)
                        {
                            hIndexB++;
                        }

                        if (hardcodeTable[hIndexA].pieceValueX2 < hardcodeTable[hIndexB].pieceValueX2)
                        {
                            pteTarget = hardcodeTable[hIndexA];
                            hardcodeTable.RemoveAt(hIndexA);
                        }
                        else
                        {
                            pteTarget = hardcodeTable[hIndexB];
                            hardcodeTable.RemoveAt(hIndexB);
                        }
                    }
                }

                /*
                while (subiterations < 5 && subArmy.Contains(pteTarget))
                {
                    pteTarget = pieceTable.Output();
                    subiterations++;
                }
                */
            }

            int EffectiveValue(PieceTableEntry pte)
            {
                if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
                {
                    return pte.pieceValueX2 / 4;
                }

                return pte.pieceValueX2;
            }

            if (EffectiveValue(pteTarget) < (lowerBound))    //Too much value concentration
            {
                continue;
            }
            if (lowerBound == 0)
            {
                if ((pteTarget.pieceProperty & Piece.PieceProperty.Unique) != 0 && subArmy.FindAll((e) => (e.type == pteTarget.type)).Count > 0)
                {
                    continue;
                }

                if (pteTarget.immobile && immobileCount > Mathf.CeilToInt(cumulativeCount / 3f))
                {
                    continue;
                }
            }

            cumulativeValueCount += pteTarget.pieceValueX2 / 2f;

            if ((pteTarget.piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                cumulativeCount += 4;
                giantCount++;
            }
            else
            {
                cumulativeCount++;
            }

            if (pteTarget.immobile)
            {
                immobileCount++;
            }

            subArmy.Add(pteTarget);

            while ((cumulativeCount >= 23 || giantCount > 4 || immobileCount > Mathf.CeilToInt(cumulativeCount / 3f)) && cumulativeValueCount < tryValue)
            {
                subArmy.Sort((a, b) => (EffectiveValue(a) - EffectiveValue(b)));
                //remove the bottom 10 valued pieces
                while (cumulativeCount >= 10 || giantCount > 4 || immobileCount > Mathf.CeilToInt(cumulativeCount / 3f))
                {
                    cumulativeValueCount -= subArmy[0].pieceValueX2 / 2f;
                    if (EffectiveValue(subArmy[0]) > lowerBound)
                    {
                        lowerBound = EffectiveValue(subArmy[0]);
                    }

                    if ((subArmy[0].piecePropertyB & PiecePropertyB.Giant) != 0)
                    {
                        cumulativeCount -= 4;
                        giantCount--;
                    }
                    else
                    {
                        cumulativeCount--;
                    }

                    if (pteTarget.immobile)
                    {
                        immobileCount--;
                    }

                    subArmy.RemoveAt(0);
                }
                //add in some of the highest valued things
                while (cumulativeCount <= 20 && giantCount < 4 && immobileCount < Mathf.CeilToInt(cumulativeCount / 3f) && cumulativeValueCount < tryValue)
                {
                    int index = subArmy.Count - 1 - UnityEngine.Random.Range(0, 7);
                    if (index < 0)
                    {
                        index = subArmy.Count - 1 - UnityEngine.Random.Range(0, subArmy.Count);
                    }
                    cumulativeValueCount += subArmy[index].pieceValueX2 / 2f;

                    if ((subArmy[index].piecePropertyB & PiecePropertyB.Giant) != 0)
                    {
                        cumulativeCount += 4;
                        giantCount++;
                    }
                    else
                    {
                        cumulativeCount++;
                    }

                    if (pteTarget.immobile)
                    {
                        immobileCount++;
                    }

                    subArmy.Insert(0, subArmy[index]);
                }
            }
        }

        int GiantScore(PieceTableEntry pte)
        {
            int score = pte.pieceValueX2;

            if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
            {
                score += 1000;
            }
            return score;
        }

        subArmy.Sort((a, b) => (a.pieceValueX2 - b.pieceValueX2));
        //subArmy.Reverse();

        //try to get it closer to the target
        if (cumulativeValueCount > tryValue)
        {
            for (int i = 0; i < subArmy.Count; i++)
            {
                if (i < 0)
                {
                    continue;
                }

                PieceTableEntry pte = subArmy[i];
                if (pte.pieceValueX2 / 2f < 2 * (cumulativeValueCount - tryValue))
                {
                    cumulativeValueCount -= pte.pieceValueX2 / 2f;
                    subArmy.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

        int rowSize = 8;
        if (subArmy.Count < 15)
        {
            rowSize = (subArmy.Count + 2) / 2;
        }
        if (rowSize < 2)
        {
            rowSize = 2;
        }

        //List<Piece.PieceType> extraPieces = Board.EnemyModifierExtraPieces(em);

        for (int i = 0; i < extraPieces.Count; i++)
        {
            subArmy.Add(GlobalPieceManager.GetPieceTableEntry(extraPieces[i]));
        }

        string debug = "";
        //could make this a hash set but ehh
        //this is just debug info so it doesn't have to be fast
        List<PieceTableEntry> uniqueTableB = new List<PieceTableEntry>();
        for (int i = 0; i < subArmy.Count; i++)
        {
            debug += subArmy[i].type.ToString();
            debug += " ";

            if (uniqueTableB.Contains(subArmy[i]))
            {
                continue;
            }
            uniqueTableB.Add(subArmy[i]);
        }

        //Debug.Log(debug);
        //Debug.Log(uniqueTableB.Count);
        //Debug.Log(cumulativeValueCount);

        subArmy.Sort((a, b) => (GiantScore(a) - GiantScore(b)));
        subArmy.Reverse();
        subArmy.Insert(0, GlobalPieceManager.GetPieceTableEntry(Piece.PieceType.King));
        subArmy = MainManager.ShuffleListSegments(subArmy, rowSize);

        //Don't put the king on the edge (especially bad for row size < 8, you can rush the king on the exposed side)
        iterations = 0;
        while (rowSize > 2 && ((em & (Board.EnemyModifier.Hidden | Board.EnemyModifier.Numerous)) == 0) && (subArmy[rowSize - 2].type == Piece.PieceType.King || subArmy[rowSize - 1].type == Piece.PieceType.King))
        {
            subArmy = MainManager.ShuffleListSegments(subArmy, rowSize);
            iterations++;
            if (iterations > 100)
            {
                break;
            }
        }

        /*

        army = new Piece.PieceType[32];
        int offset = 4 - (rowSize / 2);
        int topOffset = 4 - ((subArmy.Count % rowSize)/2);
        if (subArmy.Count % rowSize == 0)
        {
            topOffset = offset;
        }
        int numRows = Mathf.CeilToInt(subArmy.Count / (rowSize + 0f));
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < rowSize; j++)
            {
                if (subArmy.Count == 0)
                {
                    break;
                }

                if (i == numRows - 1)
                {
                    army[j + topOffset + i * 8] = subArmy[0].type;
                    subArmy.RemoveAt(0);
                }
                else
                {
                    army[j + offset + i * 8] = subArmy[0].type;
                    subArmy.RemoveAt(0);
                }
            }
        }
        */

        //New setup
        Piece.PieceType[] army = new Piece.PieceType[32];

        //int kingIndex = Random.Range(0, 8);
        //army[kingIndex] = PieceType.King;

        List<int> pushOrder = new List<int>
        {
            3,
            4,
            2,
            5,
            1,
            6,
            0,
            7
        };

        int yLevel = 0;
        int pushIndex = 0;
        int rowPieces = 0;
        bool retry = false;
        while (subArmy.Count > 0)
        {
            //plop stuff down on y level
            if (pushIndex > 7)
            {
                if (retry)
                {
                    retry = false;
                    pushIndex = 0;
                    rowPieces = 0;
                    yLevel++;

                    if (yLevel > 3)
                    {
                        break;
                    }
                    continue;
                }

                pushIndex = 0;
                retry = true;
            }

            //Try to place
            PieceTableEntry pte = subArmy[0];

            int targetPos = yLevel * 8 + pushOrder[pushIndex];

            if ((pte.piecePropertyB & PiecePropertyB.Giant) != 0)
            {
                //Need to fit the giants in the given space efficiently
                if (pushOrder[pushIndex] % 2 != 0 && giantCount >= 4)
                {
                    pushIndex++;
                    continue;
                }
                if (pushOrder[pushIndex] % 8 == 7)
                {
                    pushIndex++;
                    continue;
                }

                if ((targetPos < 32) && army[targetPos] != PieceType.Null)
                {
                    pushIndex++;
                    continue;
                }
                if ((targetPos + 1 < 32) && army[targetPos + 1] != PieceType.Null)
                {
                    pushIndex++;
                    continue;
                }
                if ((targetPos + 8 < 32) && army[targetPos + 8] != PieceType.Null)
                {
                    pushIndex++;
                    continue;
                }
                if ((targetPos + 9 < 32) && army[targetPos + 9] != PieceType.Null)
                {
                    pushIndex++;
                    continue;
                }

                //place
                army[targetPos] = pte.type;
                army[targetPos + 1] = PieceType.Rock;
                army[targetPos + 8] = PieceType.Rock;
                army[targetPos + 9] = PieceType.Rock;
                subArmy.RemoveAt(0);
                rowPieces++;
                pushIndex++;
            }
            else
            {
                if (army[yLevel * 8 + pushOrder[pushIndex]] != PieceType.Null)
                {
                    pushIndex++;
                    continue;
                }

                //place
                army[targetPos] = pte.type;
                subArmy.RemoveAt(0);
                rowPieces++;
                pushIndex++;
            }

            if (rowPieces >= rowSize)
            {
                retry = false;
                pushIndex = 0;
                rowPieces = 0;
                yLevel++;
            }
        }


        for (int i = 0; i < army.Length; i++)
        {
            if (army[i] == PieceType.Rock)
            {
                army[i] = PieceType.Null;
            }
        }

        return army;
    }

    public static RandomTable<PieceTableEntry> GetPieceTable(Piece.PieceClass pc, int types, float targetTotal, float lowPieceBias, float lowComplexityBias)
    {
        if (pc == PieceClass.None)
        {
            return GetPieceTableForClass(pc, types, targetTotal, lowPieceBias, lowComplexityBias);
        }

        RandomTable<PieceTableEntry> normalTable = GetPieceTable(GlobalPieceManager.GetPieceClassEntry(pc).normalPieces, types, targetTotal, lowPieceBias, lowComplexityBias);
        RandomTable<PieceTableEntry> extraTable = GetPieceTable(GlobalPieceManager.GetPieceClassEntry(pc).extraPieces, Mathf.CeilToInt(types * 0.5f), targetTotal * 0.5f, lowPieceBias, lowComplexityBias);
        normalTable.weight = 1;
        extraTable.weight = 1 / 16f;    //about 1 per battle (but is higher because of failed hits in the normal table?)

        RandomTable<PieceTableEntry> fusedTable = new RandomTable<PieceTableEntry>(normalTable, extraTable);
        return fusedTable;
    }
    public static RandomTable<PieceTableEntry> GetPieceTableForClass(Piece.PieceClass pc, int types, float targetTotal, float lowPieceBias, float lowComplexityBias)
    {
        if (types == 0)
        {
            types = 1;
        }
        int pawnCount = Mathf.CeilToInt(types / 4f);
        int maxCount = Mathf.CeilToInt(types / 6f);
        int maxCountB = maxCount;

        switch (types)
        {
            case 1:
                pawnCount = 0;
                maxCount = 1;
                maxCountB = 0;
                break;
            case 2:
            case 3:
                pawnCount = 1;
                maxCount = 1;
                maxCountB = 0;
                break;
        }

        List<PieceTableEntry> piecePool = new List<PieceTableEntry>();

        for (int i = 0; i < GlobalPieceManager.pieceTable.Length; i++)
        {
            if (GlobalPieceManager.pieceTable[i] == null || GlobalPieceManager.pieceTable[i].type == Piece.PieceType.King || GlobalPieceManager.pieceTable[i].type == Piece.PieceType.Rock)
            {
                continue;
            }
            if (GlobalPieceManager.pieceTable[i].type == Piece.PieceType.GeminiTwin || GlobalPieceManager.pieceTable[i].type == Piece.PieceType.MoonIllusion)
            {
                continue;
            }
            if (GlobalPieceManager.pieceTable[i].pieceValueX2 == 0)
            {
                continue;
            }

            //piece must be at least 1/36 the total to count
            //Or at least 10 value
            if (GlobalPieceManager.pieceTable[i].pieceValueX2 <= 20 && GlobalPieceManager.pieceTable[i].pieceValueX2 <= targetTotal / 18)
            {
                continue;
            }

            if (pc == Piece.PieceClass.None || GlobalPieceManager.pieceTable[i].pieceClass == pc)
            {
                piecePool.Add(GlobalPieceManager.pieceTable[i]);
            }
        }

        //wacky setup but C# complains about the parameterless constructor now because there is ambiguity
        RandomTable<PieceTableEntry> pieceTable = new RandomTable<PieceTableEntry>(new List<PieceTableEntry>());

        /*
        List<IRandomTableEntry<PieceTableEntry>> pieceTableEntries = new List<IRandomTableEntry<PieceTableEntry>>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 3f))    //Too much value concentration (In normal board of 39 this stops 13+ value pieces)
            {
                continue;
            }
            if ((piecePool[i].pieceValueX2 / 2f) < (targetTotal / 50f))    //Not enough value density (In a board of 50 this stops 0.5)
            {
                continue;
            }
            if ((piecePool[i].type == PieceType.GeminiTwin) || (piecePool[i].type == PieceType.MoonIllusion) || (piecePool[i].type == PieceType.King) || (piecePool[i].pieceValueX2 == 0))
            {
                continue;
            }

            float tableValue = Mathf.Min(1f / Mathf.Pow(piecePool[i].pieceValueX2, 1 + lowPieceBias), 1);
            tableValue *= Mathf.Min(1f / Mathf.Pow(piecePool[i].complexityLevel + 1, 1 + lowComplexityBias), 1);

            if ((piecePool[i].pieceProperty & Piece.PieceProperty.Giant) != 0)
            {
                tableValue *= 0.25f;
            }

            pieceTableEntries.Add(new RandomTableEntry<PieceTableEntry>(piecePool[i], tableValue));
        }
        */

        //Filter out most entries
        //Keep a minimum of 10
        //Attempt to find 3 pawnlikes and 4 that are "high value"
        List<PieceTableEntry> reducedPool = new List<PieceTableEntry>();
        List<PieceTableEntry> subsetTable = new List<PieceTableEntry>();

        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].promotionType != PieceType.Null) && !reducedPool.Contains(piecePool[i]))
            {
                subsetTable.Add(piecePool[i]);
            }
        }

        for (int i = 0; i < pawnCount; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias, lowComplexityBias + 1);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias, lowComplexityBias + 1);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        //Debug.Log("Upper limit = " + (targetTotal / 1.5f));
        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 4f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 1.5f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
            else
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 1.5f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        for (int i = 0; i < maxCountB; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias - 1);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias - 1);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 4f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
            else
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 14f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 8f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        if (maxCountB == 0 && subsetTable.Count == 0)
        {
            //add the higher limit stuff anyway
            for (int i = 0; i < piecePool.Count; i++)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) < (targetTotal / 2f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        for (int i = 0; i < maxCount; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            //Now just add anything
            if (!reducedPool.Contains(piecePool[i]))
            {
                //enforce this still so you don't get forced into high value concentration
                if ((piecePool[i].pieceValueX2 / 2f) <= Mathf.Max((targetTotal / 1.5f), 1.5f))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        if (reducedPool.Count == 0)
        {
            //failsafe: add lowest value piece pool piece
            PieceTableEntry pteF = piecePool[0];
            for (int i = 0; i < piecePool.Count; i++)
            {
                if (piecePool[i].pieceValueX2 < pteF.pieceValueX2)
                {
                    pteF = piecePool[i];
                }
            }
            subsetTable.Add(pteF);
        }

        while (reducedPool.Count < types)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }


        //build the table
        List<IRandomTableEntry<PieceTableEntry>> reducedTable = new List<IRandomTableEntry<PieceTableEntry>>();

        for (int i = 0; i < reducedPool.Count; i++)
        {
            float tableValue = GetPieceTableEntryWeight(reducedPool[i], lowPieceBias, lowComplexityBias);
            reducedTable.Add(new RandomTableEntry<PieceTableEntry>(reducedPool[i], tableValue));
        }

        string debug = "";
        for (int i = 0; i < reducedPool.Count; i++)
        {
            debug += reducedPool[i].type.ToString();
            debug += " ";
        }
        Debug.Log(debug);

        pieceTable = new RandomTable<PieceTableEntry>(reducedTable);
        return pieceTable;
    }
    public static RandomTable<PieceTableEntry> GetPieceTable(Piece.PieceType[] pieceList, int types, float targetTotal, float lowPieceBias, float lowComplexityBias)
    {
        if (types == 0)
        {
            types = 1;
        }
        int pawnCount = Mathf.CeilToInt(types / 4f);
        int maxCount = Mathf.CeilToInt(types / 6f);
        int maxCountB = maxCount;

        switch (types)
        {
            case 1:
                pawnCount = 0;
                maxCount = 1;
                maxCountB = 0;
                break;
            case 2:
            case 3:
                pawnCount = 1;
                maxCount = 1;
                maxCountB = 0;
                break;
        }

        List<PieceTableEntry> piecePool = new List<PieceTableEntry>();

        for (int i = 0; i < pieceList.Length; i++)
        {
            if (GlobalPieceManager.GetPieceTableEntry(pieceList[i]) == null || pieceList[i] == Piece.PieceType.King || pieceList[i] == Piece.PieceType.Rock)
            {
                continue;
            }
            if (pieceList[i] == Piece.PieceType.GeminiTwin || pieceList[i] == Piece.PieceType.MoonIllusion)
            {
                continue;
            }
            if (GlobalPieceManager.GetPieceTableEntry(pieceList[i]).pieceValueX2 == 0)
            {
                continue;
            }

            //piece must be at least 1/36 the total to count
            //Or at least 10 value
            if (GlobalPieceManager.GetPieceTableEntry(pieceList[i]).pieceValueX2 <= 20 && GlobalPieceManager.GetPieceTableEntry(pieceList[i]).pieceValueX2 <= targetTotal / 18)
            {
                continue;
            }

            /*
            if (pc == Piece.PieceClass.None || GlobalPieceManager.GetPieceTableEntry(pieceList[i]).pieceClass == pc)
            {
                piecePool.Add(GlobalPieceManager.GetPieceTableEntry(pieceList[i]));
            }
            */
            piecePool.Add(GlobalPieceManager.GetPieceTableEntry(pieceList[i]));
        }

        RandomTable<PieceTableEntry> pieceTable = new RandomTable<PieceTableEntry>(new List<PieceTableEntry>());
        /*
        List<IRandomTableEntry<PieceTableEntry>> pieceTableEntries = new List<IRandomTableEntry<PieceTableEntry>>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 3f))    //Too much value concentration (In normal board of 39 this stops 13+ value pieces)
            {
                continue;
            }
            if ((piecePool[i].pieceValueX2 / 2f) < (targetTotal / 50f))    //Not enough value density (In a board of 50 this stops 0.5)
            {
                continue;
            }
            if ((piecePool[i].type == PieceType.GeminiTwin) || (piecePool[i].type == PieceType.MoonIllusion) || (piecePool[i].type == PieceType.King) || (piecePool[i].pieceValueX2 == 0))
            {
                continue;
            }

            float tableValue = Mathf.Min(1f / Mathf.Pow(piecePool[i].pieceValueX2, 1 + lowPieceBias), 1);
            tableValue *= Mathf.Min(1f / Mathf.Pow(piecePool[i].complexityLevel + 1, 1 + lowComplexityBias), 1);

            if ((piecePool[i].pieceProperty & Piece.PieceProperty.Giant) != 0)
            {
                tableValue *= 0.25f;
            }

            pieceTableEntries.Add(new RandomTableEntry<PieceTableEntry>(piecePool[i], tableValue));
        }
        */

        //Filter out most entries
        //Keep a minimum of 10
        //Attempt to find 3 pawnlikes and 4 that are "high value"
        List<PieceTableEntry> reducedPool = new List<PieceTableEntry>();
        List<PieceTableEntry> subsetTable = new List<PieceTableEntry>();

        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].promotionType != PieceType.Null) && !reducedPool.Contains(piecePool[i]))
            {
                subsetTable.Add(piecePool[i]);
            }
        }

        for (int i = 0; i < pawnCount; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias, lowComplexityBias + 1);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias, lowComplexityBias + 1);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        //Debug.Log("Upper limit = " + (targetTotal / 1.5f));
        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 4f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 1.5f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
            else
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 1.5f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        for (int i = 0; i < maxCountB; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias - 1);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias - 1);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if ((piecePool[i].piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 4f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
            else
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 14f) && (piecePool[i].pieceValueX2 / 2f) <= (targetTotal / 8f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        if (maxCountB == 0 && subsetTable.Count == 0)
        {
            //add the higher limit stuff anyway
            for (int i = 0; i < piecePool.Count; i++)
            {
                if ((piecePool[i].pieceValueX2 / 2f) > (targetTotal / 8f) && (piecePool[i].pieceValueX2 / 2f) < (targetTotal / 2f) && !reducedPool.Contains(piecePool[i]))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        for (int i = 0; i < maxCount; i++)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }

        subsetTable = new List<PieceTableEntry>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            //Now just add anything
            if (!reducedPool.Contains(piecePool[i]))
            {
                //enforce this still so you don't get forced into high value concentration
                if ((piecePool[i].pieceValueX2 / 2f) <= Mathf.Max((targetTotal / 1.5f), 1.5f))
                {
                    subsetTable.Add(piecePool[i]);
                }
            }
        }

        if (reducedPool.Count == 0)
        {
            //failsafe: add lowest value piece pool piece
            PieceTableEntry pteF = piecePool[0];
            for (int i = 0; i < piecePool.Count; i++)
            {
                if (piecePool[i].pieceValueX2 < pteF.pieceValueX2)
                {
                    pteF = piecePool[i];
                }
            }
            subsetTable.Add(pteF);
        }

        while (reducedPool.Count < types)
        {
            if (subsetTable.Count == 0)
            {
                break;
            }

            int jA = UnityEngine.Random.Range(0, subsetTable.Count);

            if (subsetTable.Count > 1)
            {
                int jB = UnityEngine.Random.Range(0, subsetTable.Count - 1);
                if (jB >= jA)
                {
                    jB++;
                }

                float wA = GetPieceTableEntryWeight(subsetTable[jA], lowPieceBias - 2, lowComplexityBias);
                float wB = GetPieceTableEntryWeight(subsetTable[jB], lowPieceBias - 2, lowComplexityBias);
                //Debug.Log((wA / (wA + wB)));
                if (RandomGenerator.Get() < 0.2f + 0.6f * (wA / (wA + wB)))
                {
                    //Debug.Log(subsetTable[jA].type);
                    reducedPool.Add(subsetTable[jA]);
                    subsetTable.RemoveAt(jA);
                }
                else
                {
                    //Debug.Log(subsetTable[jB].type);
                    reducedPool.Add(subsetTable[jB]);
                    subsetTable.RemoveAt(jB);
                }
            }
            else
            {
                //Debug.Log(subsetTable[jA].type);
                reducedPool.Add(subsetTable[jA]);
                subsetTable.RemoveAt(jA);
            }
        }


        //build the table
        List<IRandomTableEntry<PieceTableEntry>> reducedTable = new List<IRandomTableEntry<PieceTableEntry>>();

        for (int i = 0; i < reducedPool.Count; i++)
        {
            float tableValue = GetPieceTableEntryWeight(reducedPool[i], lowPieceBias, lowComplexityBias);
            reducedTable.Add(new RandomTableEntry<PieceTableEntry>(reducedPool[i], tableValue));
        }

        string debug = "";
        for (int i = 0; i < reducedPool.Count; i++)
        {
            debug += reducedPool[i].type.ToString();
            debug += " ";
        }
        //Debug.Log(debug);

        pieceTable = new RandomTable<PieceTableEntry>(reducedTable);
        return pieceTable;
    }

    public static float GetPieceTableEntryWeight(PieceTableEntry pte, float lowPieceBias, float lowComplexityBias)
    {
        float tableValue = Mathf.Min(1f / Mathf.Pow(pte.pieceValueX2, 1 + lowPieceBias), 1);
        tableValue *= Mathf.Min(1f / Mathf.Pow(pte.complexityLevel + 1, 1 + lowComplexityBias), 1);

        if ((pte.piecePropertyB & Piece.PiecePropertyB.Giant) != 0)
        {
            tableValue *= 0.25f;
        }

        return tableValue;
    }
}
