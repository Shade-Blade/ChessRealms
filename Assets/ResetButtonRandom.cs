using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece;

public class ResetButtonRandom : MonoBehaviour
{
    public BoardScript bs;
    public Piece.PieceType[] army;
    public Board.EnemyModifier em;
    public Piece.PieceClass pieceClass;
    public float targetNonkingValue;
    public float lowPieceBias;

    public void OnMouseDown()
    {
        List<PieceTableEntry> piecePool = new List<PieceTableEntry>();

        for (int i = 0; i < GlobalPieceManager.Instance.pieceTable.Length; i++)
        {
            if (GlobalPieceManager.Instance.pieceTable[i] == null || GlobalPieceManager.Instance.pieceTable[i].type == Piece.PieceType.King || GlobalPieceManager.Instance.pieceTable[i].type == Piece.PieceType.Rock)
            {
                continue;
            }

            if (pieceClass == Piece.PieceClass.None || GlobalPieceManager.Instance.pieceTable[i].pieceClass == pieceClass)
            {
                piecePool.Add(GlobalPieceManager.Instance.pieceTable[i]);
            }
        }

        float highestValue = 0;
        for (int i = 0; i < piecePool.Count; i++)
        {
            if (piecePool[i].pieceValueX2/2f > highestValue)
            {
                highestValue = piecePool[i].pieceValueX2/2f;
            }
        }
        highestValue++;

        RandomTable<PieceTableEntry> pieceTable = new RandomTable<PieceTableEntry>();
        List<IRandomTableEntry<PieceTableEntry>> pieceTableEntries = new List<IRandomTableEntry<PieceTableEntry>>();
        for (int i = 0; i < piecePool.Count; i++)
        {
            if (piecePool[i].pieceValueX2 > (targetNonkingValue * 1.33f))    //Too much value concentration
            {
                continue;
            }
            if (piecePool[i].pieceValueX2 < (targetNonkingValue / 64f))    //Too much value concentration
            {
                continue;
            }
            if ((piecePool[i].pieceProperty & Piece.PieceProperty.Giant) != 0)
            {
                continue;
            }

            if (lowPieceBias < 0)
            {
                pieceTableEntries.Add(new RandomTableEntry<PieceTableEntry>(piecePool[i], Mathf.Min(1f / Mathf.Pow(piecePool[i].pieceValueX2, 1 + lowPieceBias), 1)));
            }
            else
            {
                pieceTableEntries.Add(new RandomTableEntry<PieceTableEntry>(piecePool[i], Mathf.Min(1f / Mathf.Pow(piecePool[i].pieceValueX2, 1 + lowPieceBias), 1)));
            }
        }
        pieceTable = new RandomTable<PieceTableEntry>(pieceTableEntries);

        List<PieceTableEntry> subArmy = new List<PieceTableEntry>();

        float cumulativeCount = 0;

        //force this not to be an infinite loop if you give it an impossible task
        //(i.e. try to get above 9 * 23 value with normal chess pieces because the queen is already 9 value)
        int iterations = 0;

        float lowerBound = 0;

        while (cumulativeCount < targetNonkingValue && subArmy.Count < 23)
        {
            iterations++;
            if (iterations > 50000)
            {
                break;
            }

            PieceTableEntry pteTarget = pieceTable.Output();
            if (pteTarget == null)
            {
                Debug.Log(lowerBound);
                break;
            }
            if (pteTarget.pieceValueX2 < (lowerBound))    //Too much value concentration
            {
                continue;
            }
            if (lowerBound == 0 && (pteTarget.pieceProperty & Piece.PieceProperty.Unique) != 0)
            {
                if (subArmy.FindAll((e) => (e.type == pteTarget.type)).Count > 0)
                {
                    continue;
                }
            }

            cumulativeCount += pteTarget.pieceValueX2 / 2f;
            subArmy.Add(pteTarget);

            while (subArmy.Count >= 23)
            {
                subArmy.Sort((a, b) => (a.pieceValueX2 - b.pieceValueX2));
                //remove the bottom 10 valued pieces
                for (int i = 0; i < 10; i++)
                {
                    cumulativeCount -= subArmy[0].pieceValueX2 / 2f;
                    if (subArmy[0].pieceValueX2 > lowerBound)
                    {
                        lowerBound = subArmy[0].pieceValueX2;
                    }
                    subArmy.RemoveAt(0);
                }
                //add in some of the highest valued things
                for (int i = 0; i < 5; i++)
                {
                    int index = subArmy.Count - 1 - Random.Range(0, 7);
                    cumulativeCount += subArmy[index].pieceValueX2 / 2f;
                    subArmy.Insert(0, subArmy[index]);
                }
            }
        }

        subArmy.Sort((a, b) => (a.pieceValueX2 - b.pieceValueX2));
        //subArmy.Reverse();

        //try to get it closer to the target
        if (cumulativeCount > targetNonkingValue)
        {
            for (int i = 0; i < subArmy.Count; i++)
            {
                if (i < 0)
                {
                    continue;
                }

                PieceTableEntry pte = subArmy[i];
                if (pte.pieceValueX2 / 2f < 2 * (cumulativeCount - targetNonkingValue))
                {
                    cumulativeCount -= pte.pieceValueX2 / 2f;
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

        subArmy.Reverse();
        subArmy.Insert(0, GlobalPieceManager.Instance.GetPieceTableEntry(Piece.PieceType.King));
        subArmy = MainManager.ShuffleListSegments(subArmy, rowSize);

        //Don't put the king on the edge (especially bad for row size < 8, you can rush the king on the exposed side)
        while (rowSize > 2 && (subArmy[0].type == Piece.PieceType.King || subArmy[rowSize - 1].type == Piece.PieceType.King))
        {
            subArmy = MainManager.ShuffleListSegments(subArmy, rowSize);
        }

        army = new Piece.PieceType[32];
        int offset = 4 - (rowSize / 2);
        int topOffset = 4 - ((subArmy.Count % rowSize)/2);
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

        bs.ResetBoard(army, em);
    }
}
