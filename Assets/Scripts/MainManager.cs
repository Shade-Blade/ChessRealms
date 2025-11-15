using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Bmi1;
using static Unity.Burst.Intrinsics.X86.Popcnt;

[System.Serializable]
public class PlayerData
{
    public Piece.PieceType[] army;

    public Move.ConsumableMoveType[] consumables;

    public PlayerData()
    {
        army = new Piece.PieceType[16];

        consumables = new Move.ConsumableMoveType[4];

        army[0] = Piece.PieceType.King;
    }
} 

public class MainManager : MonoBehaviour
{
    private static MainManager intInstance;
    public static MainManager Instance
    {
        get {
            //optimization
            /*
            if (intInstance == null)
            {
                intInstance = FindObjectOfType<MainManager>();
            }
            */
            return intInstance;
        }
    }

    public DraggableObject currentDragged;
    public SelectableObject currentSelected;
    public bool sameFrameSelected;

    public const ulong NO_A_FILE = 0xfefefefefefefefe;
    public const ulong NO_AB_FILE = 0xfcfcfcfcfcfcfcfc;
    public const ulong NO_ABC_FILE = 0xf8f8f8f8f8f8f8f8;
    public const ulong NO_ABCD_FILE = 0xf0f0f0f0f0f0f0f0;
    public const ulong NO_ABCDE_FILE = 0xe0e0e0e0e0e0e0e0;
    public const ulong NO_ABCDEF_FILE = 0xc0c0c0c0c0c0c0c0;
    public const ulong NO_ABCDEFG_FILE = 0x8080808080808080;
    public const ulong NO_H_FILE = 0x7f7f7f7f7f7f7f7f;
    public const ulong NO_GH_FILE = 0x3f3f3f3f3f3f3f3f;
    public const ulong NO_FGH_FILE = 0x1f1f1f1f1f1f1f1f;
    public const ulong NO_EFGH_FILE = 0x0f0f0f0f0f0f0f0f;
    public const ulong NO_DEFGH_FILE = 0x0707070707070707;
    public const ulong NO_CDEFGH_FILE = 0x0303030303030303;
    public const ulong NO_BCDEFGH_FILE = 0x0101010101010101;

    public PlayerData playerData;

    public string bitboardTest = "";

    private static int[] debrujin_index64 = new int[64]{
        0,  1, 48,  2, 57, 49, 28,  3,
       61, 58, 50, 42, 38, 29, 17,  4,
       62, 55, 59, 36, 53, 51, 43, 22,
       45, 39, 33, 30, 24, 18, 12,  5,
       63, 47, 56, 27, 60, 41, 37, 16,
       54, 35, 52, 21, 44, 32, 23, 11,
       46, 26, 40, 15, 34, 20, 31, 10,
       25, 14, 19,  9, 13,  8,  7,  6
    };
    //multiply single bit by 0x03f79d71b4cb0a89

    private void Awake()
    {
        intInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        //GlobalPieceManager.GetPieceTableEntry(Piece.PieceType.King);

        //currently 1.7 mil per second
        //"Good" speed = 100 mil?
        //But that might not be possible with C# code in any way?
        //Or it requires magic bitboards and other stuff I can't do

        //0.6 million after adding the loops to remove status effects and shielded
        //Just adding the status effect tick down loop reduced it from 800k to 600k
        //  = 30% time increase just by adding that one loop
        //:(

        //It is now 250k :((
        //Well at least I got depth 6 AI working?
        //But if it isn't possible to make that good enough then I guess I'll have to scrap this game
        
        //optimization: now it is 350k again

        //~500k normal after optimizing TickDownStatusEffects to reduce PieceTableEntry checks

        //220 ish seconds for depth 6 normal
        for (int i = 0; i <= 3; i++) 
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();

            Board board = new Board();
            board.Setup(Board.BoardPreset.Normal);

            ulong perftResult = Board.PerftTest(ref board, i);

            currentTime = DateTime.UtcNow;
            long unixTimeEnd = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            Debug.Log("Perft took " + ((unixTimeEnd - unixTime)/ 1000d) + " seconds for " + perftResult + " positions at depth + " + i + " = " + "(" + (perftResult / ((unixTimeEnd - unixTime) / 1000d)) + " pos/sec) (" + (((unixTimeEnd - unixTime) / 1000d) / perftResult) + " s per pos)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (bitboardTest.Length > 0)
        {
            ulong test = 0;
            int index = 0;

            for (int i = 0; i < bitboardTest.Length; i++)
            {
                index = i;
                ulong value = 0;

                switch (bitboardTest[i])
                {
                    case '0':
                        value = 0;
                        break;
                    case '1':
                        value = 1;
                        break;
                    case '2':
                        value = 2;
                        break;
                    case '3':
                        value = 3;
                        break;
                    case '4':
                        value = 4;
                        break;
                    case '5':
                        value = 5;
                        break;
                    case '6':
                        value = 6;
                        break;
                    case '7':
                        value = 7;
                        break;
                    case '8':
                        value = 8;
                        break;
                    case '9':
                        value = 9;
                        break;
                    case 'a':
                        value = 10;
                        break;
                    case 'b':
                        value = 11;
                        break;
                    case 'c':
                        value = 12;
                        break;
                    case 'd':
                        value = 13;
                        break;
                    case 'e':
                        value = 14;
                        break;
                    case 'f':
                        value = 15;
                        break;
                }

                test |= value << (15 - index) * 4;
            }

            Debug.Log(test);
            PrintBitboard(test);
        }
        */

        if (currentDragged != null && !currentDragged.isDragged)
        {
            currentDragged = null;
        }

        if (Input.GetMouseButtonDown(0) && !sameFrameSelected && currentSelected != null)
        {
            currentSelected.OnDeselect();
            currentSelected = null;
        }

        sameFrameSelected = false;
    }



    public static Vector2 XYProject(UnityEngine.Vector3 v)
    {
        return v.x * Vector2.right + v.y * Vector2.up;
    }

    public static Vector3 XYProjectPreserve(Vector3 v)
    {
        return v.x * Vector3.right + v.y * Vector3.up;
    }

    public static Vector3 XYProjectReverse(Vector2 v)
    {
        return v.x * Vector3.right + v.y * Vector3.up;
    }

    public static int PopBitboardLSB1(ulong bitboard, out ulong output)
    {
        if (bitboard == 0)
        {
            output = bitboard;
            return -1;
        }

        //Intrinsics
        /*
        if (IsBmi1Supported)
        {
            //trailing zero count u64
            ulong specialOutput = tzcnt_u64(bitboard);

            //bit lowest set reset
            output = blsr_u64(bitboard);

            return (int)specialOutput;
        }
        */

        ulong isolated = (bitboard) ^ (bitboard - 1);
        isolated -= isolated >> 1;

        //1 bit left
        output = bitboard - isolated;
        int index = debrujin_index64[(isolated * 0x03f79d71b4cb0a89) >> 58];

        return index;
    }
    //Slower than LSB1
    //but at least it looks simpler than the wacky magic number stuff
    public static int PopBitboardMSB1(ulong bitboard, out ulong output)
    {
        ulong test = bitboard;
        int index = 0;

        output = bitboard;

        if (test > 0xffffffff)
        {
            index += 32;
            test >>= 32;
        }
        if (test > 0xffff)
        {
            index += 16;
            test >>= 16;
        }
        if (test > 0xff)
        {
            index += 8;
            test >>= 8;
        }
        if (test > 0xf)
        {
            index += 4;
            test >>= 4;
        }
        if (test > 0x3)
        {
            index += 2;
            test >>= 2;
        }
        if (test > 0x1)
        {
            index += 1;
            test >>= 1;
        }

        output &= ~(1uL << index);
        return index;
    }
    public static int PopCount(ulong bitboard)
    {
        if (IsPopcntSupported)
        {
            return popcnt_u64(bitboard);
        }

        int output = 0;
        while (bitboard != 0)
        {
            PopBitboardLSB1(bitboard, out bitboard);
            output++;
        }
        return output;
    }

    //https://www.chessprogramming.org/Flipping_Mirroring_and_Rotating
    public static ulong MirrorBitboard(ulong x)
    {
        const ulong k1 = (0x5555555555555555);
        const ulong k2 = (0x3333333333333333);
        const ulong k4 = (0x0f0f0f0f0f0f0f0f);
        x = ((x >> 1) & k1) | ((x & k1) << 1);
        x = ((x >> 2) & k2) | ((x & k2) << 2);
        x = ((x >> 4) & k4) | ((x & k4) << 4);
        return x;
    }
    public ulong Majority(ulong a, ulong b, ulong c)
    {
        return (a & b) | (c & (a | b));
    }
    public ulong Odd(ulong a, ulong b, ulong c)
    {
        return a ^ b ^ c;
    }

    //https://www.chessprogramming.org/Population_Count#CardinalityofMultipleSets

    //This is just a chain of 3 bit adders?
    //oddMaj is one 3 bit adder (returns odd = 1s digit, maj = 2s digit)

    /*
    one1,two1  := oddMaj(x1,x2,x3)
    one2,two2  := oddMaj(x4,x5,x6)
    one3,two3  := oddMaj(x7,x8,x9)
    one4,two4  := oddMaj(x10,x11,x12)
    one5,two5  := oddMaj(x13,x14,x15)
    one6,two6  := oddMaj(one1,one2,one3)
    ones,two7  := oddMaj(one4,one5,one6)
    two8,four1 := oddMaj(two1,two2,two3)
    two9,four2 := oddMaj(two4,two5,two6)
    twos,four3 := oddMaj(two7,two8,two9)
    four,eight := oddMaj(four1,four2,four3)

     Version for 8 bitboards
    one1,two1  := oddMaj(x1,x2,x3)
    one2,two2  := oddMaj(x4,x5,x6)
    one3  := x7 ^ x8
    two3 = x7 & x8

    one6,two6  := oddMaj(one1,one2,one3)
    ones = one6

    two8,four1 := oddMaj(two1,two2,two3)

    twos = two8 ^ two6
    four = four1 ^ (two8 & two6)
    eight = four1 & (two8 & two6)
     */
    public static (ulong, ulong, ulong, ulong) BitboardCardinality(ulong x1, ulong x2, ulong x3, ulong x4, ulong x5, ulong x6, ulong x7, ulong x8)
    {
        ulong one1 = x1 ^ x2 ^ x3;
        ulong one2 = x4 ^ x5 ^ x6;
        ulong one3 = x7 ^ x8;

        ulong two1 = (x1 & x2) | (x3 & (x1 | x2));
        ulong two2 = (x4 & x5) | (x6 & (x4 | x5));
        ulong two3 = (x7 & x8);

        ulong ones = one1 ^ one2 ^ one3;
        ulong two6 = (one1 & one2) | (one3 & (one1 | one2));

        ulong two8 = two1 ^ two2 ^ two3;
        ulong four1 = (two1 & two2) | (two3 & (two1 | two2));

        ulong twos = two8 ^ two6;
        ulong four = four1 ^ (two8 & two6);
        ulong eight = four1 & (two8 & two6);

        return (ones, twos, four, eight);
    }
    public static (ulong, ulong, ulong, ulong) CountAdjacencyCardinality(ulong x)
    {
        ulong al = (x & NO_A_FILE) >> 1;
        ulong ar = (x & NO_H_FILE) << 1;
        return BitboardCardinality(x >> 8, x << 8, al, al >> 8, al << 8, ar, ar >> 8, ar << 8);
    }

    public static ulong SmearBitboard(ulong bitboard)
    {
        //does this save time overall?
        //I assume the prevalence of empty bitboards for most of the area of effect pieces will make this better to have
        if (bitboard == 0)
        {
            return bitboard;
        }

        ulong distBitboard = bitboard;
        distBitboard |= distBitboard << 8;
        distBitboard |= distBitboard >> 8;
        distBitboard |= (distBitboard & NO_A_FILE) >> 1;
        distBitboard |= (distBitboard & NO_H_FILE) << 1;

        return distBitboard;
    }
    public static void PrintBitboard(ulong bb)
    {
        string output = "";
        for (int y = 7; y >= 0; y--)
        {
            for (int x = 0; x < 8; x++)
            {
                output += ((1uL << x + y * 8) & bb) != 0 ? "X" : "O";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
    public static ulong GetWraparoundCutoff(int wrapAround)
    {
        ulong wrapCutoff = 0xffffffffffffffff;
        switch (wrapAround)
        {
            case -7:
                wrapCutoff = NO_BCDEFGH_FILE;
                break;
            case -6:
                wrapCutoff = NO_CDEFGH_FILE;
                break;
            case -5:
                wrapCutoff = NO_DEFGH_FILE;
                break;
            case -4:
                wrapCutoff = NO_EFGH_FILE;
                break;
            case -3:
                wrapCutoff = NO_FGH_FILE;
                break;
            case -2:
                wrapCutoff = NO_GH_FILE;
                break;
            case -1:
                wrapCutoff = NO_H_FILE;
                break;
            case 1:
                wrapCutoff = NO_A_FILE;
                break;
            case 2:
                wrapCutoff = NO_AB_FILE;
                break;
            case 3:
                wrapCutoff = NO_ABC_FILE;
                break;
            case 4:
                wrapCutoff = NO_ABCD_FILE;
                break;
            case 5:
                wrapCutoff = NO_ABCDE_FILE;
                break;
            case 6:
                wrapCutoff = NO_ABCDEF_FILE;
                break;
            case 7:
                wrapCutoff = NO_ABCDEFG_FILE;
                break;
        }
        return wrapCutoff;
    }
    public static ulong ShiftBitboardPattern(ulong pattern, int xy, int dx, int dy)
    {
        ulong shiftedPattern = pattern;

        //Debug.Log("Shift pattern by " + xy + " " + dx + " " + dy + " ");
        //PrintBitboard(pattern);
        //Problem: this will wrap around left to right

        int wrapAround = (xy & 7) + dx;
        //Debug.Log("Pre wraparound " + wrapAround);

        if (wrapAround > 0)
        {
            //fix

            //-2 -1 0 1 2 3 4 5 (6 -> 1) (7 -> 2)
            //xy & 7
            //0 1 2 3 4 5 6 7
            //xy & 7 - dx
            //2 3 4 5 6 7(0) 8(1) 9(2)
            wrapAround -= dx * 2;

            if (wrapAround > 7)
            {
                wrapAround -= 7;
            } else
            {
                wrapAround = 0;
            }
        }

        ulong wrapCutoff = 0xffffffffffffffff;

        //8 wrap around 
        wrapCutoff = GetWraparoundCutoff(wrapAround);

        //Debug.Log("Wrap cutoff " + wrapAround);
        //PrintBitboard(wrapCutoff);

        //negative left shift is not a right shift
        //thanks C#
        //Debug.Log("Shift by " + (xy + dy * 8 + dx));
        if ((xy + dy * 8 + dx) < 0)
        {
            shiftedPattern >>= -(xy + dy * 8 + dx);
        }
        else
        {
            shiftedPattern <<= (xy + dy * 8 + dx);
        }
        //PrintBitboard(shiftedPattern);

        shiftedPattern &= wrapCutoff;
        //PrintBitboard(shiftedPattern);

        return shiftedPattern;
    }

    public static List<T> ShuffleList<T>(List<T> list)
    {
        List<T> output = new List<T>();

        for (int i = 0; i < list.Count; i++)
        {
            output.Insert(UnityEngine.Random.Range(0, output.Count), list[i]);
        }

        return output;
    }
    public static List<T> ShuffleListSegments<T>(List<T> list, int segmentSize)
    {
        List<List<T>> subLists = new List<List<T>>();

        List<T> output = new List<T>();
        for (int i = 0; i < Mathf.CeilToInt(list.Count / (segmentSize + 0f)); i++)
        {
            subLists.Add(new List<T>());
            for (int j = 0; j < segmentSize; j++)
            {
                if (j + i * segmentSize >= list.Count)
                {
                    break;
                }

                subLists[i].Add(list[j + i * segmentSize]);
            }
            subLists[i] = ShuffleList<T>(subLists[i]);
            for (int j = 0; j < segmentSize; j++)
            {
                if (j >= subLists[i].Count)
                {
                    break;
                }
                output.Add(subLists[i][j]);
            }
        }

        return output;
    }

    public static void PrintMoveSet(HashSet<uint> moveSet)
    {
        if (moveSet == null || moveSet.Count == 0)
        {
            Debug.Log("Empty set");
            return;
        }
        string output = "";
        foreach (uint m in moveSet)
        {
            if (output.Length > 0)
            {
                output += " ";
            }
            output += Move.ConvertToStringMinimal(m);
        }
        Debug.Log(output);
    }
    public static void PrintMoveList(List<uint> moveList)
    {
        if (moveList == null || moveList.Count == 0)
        {
            Debug.Log("Empty list");
            return;
        }
        string output = "";
        foreach (uint m in moveList)
        {
            if (output.Length > 0)
            {
                output += " ";
            }
            output += Move.ConvertToStringMinimal(m);
        }
        Debug.Log(output);
    }


    //this uses the excel formatting so I can use excel to edit files
    //Excel formatting:
    //\n is used to delineate line breaks
    //Entries may or may not have "" around them
    //Within quotes, commas are treated literally
    //"" is " when within quotes
    //  Note that """ is difficult to resolve (use quotes as context)
    public static string[][] CSVParse(string s)
    {
        //split by line breaks (no way to escape them)
        string[] r = s.Split('\n');
        string[][] output = new string[r.Length][];

        bool inQuote = false;
        string temp;
        List<string> tempList = new List<string>();

        for (int i = 0; i < r.Length; i++)
        {
            tempList = new List<string>();
            //Parse the line of data
            temp = "";
            for (int j = 0; j < r[i].Length; j++)
            {
                if (r[i][j] == '"')
                {
                    if (inQuote && j < r[i].Length - 1 && r[i][j + 1] == '"')
                    {
                        temp += '"';
                        j++; //skip both "s (with the j++ below)
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                    j++; //Skip adding "
                }

                if (j >= r[i].Length)
                {
                    break;
                }

                if (inQuote)
                {
                    temp += r[i][j];
                }
                else
                {
                    if (r[i][j] == ',')
                    {
                        tempList.Add(temp.Replace("\r", ""));
                        temp = "";
                    }
                    else
                    {
                        temp += r[i][j];
                    }
                }
            }

            //one more
            tempList.Add(temp.Replace("\r", ""));

            output[i] = (string[])tempList.ToArray().Clone();
        }

        return output;
    }

    public static uint BitFilter(uint target, int startBit, int endBit) //Inclusive
    {
        //Intrinsics
        if (IsBmi1Supported)
        {
            //trailing zero count u32
            //might still lose some time due to uint casts but ehh
            return bextr_u32(target, (uint)startBit, (uint)(1 + endBit - startBit));
        }

        //Make a bit filter and then cut all the wrong bits
        uint endBitFilter = 1u << endBit;

        //Set all lower bits
        endBitFilter = endBitFilter + (endBitFilter - 1);

        uint startBitFilter = (1u << startBit) - 1;

        uint bitFilter = endBitFilter - startBitFilter;

        return (target & bitFilter) >> startBit;
    }
    public static uint BitFilterSet(uint target, uint set, int startBit, int endBit)
    {
        //unfortunately there's not a magic intrinsic that does the opposite of bit field extract above :(

        //Make a bit filter and then cut all the wrong bits
        uint endBitFilter = 1u << endBit;

        //Set all lower bits
        endBitFilter = endBitFilter + (endBitFilter - 1);

        uint startBitFilter = (1u << startBit) - 1;

        uint bitFilter = endBitFilter - startBitFilter;

        bitFilter = ~bitFilter;

        set <<= startBit;

        return (target & bitFilter) + set;
    }
    public static ulong BitFilter(ulong target, int startBit, int endBit)
    {
        //Make a bit filter and then cut all the wrong bits
        ulong endBitFilter = 1UL << endBit;

        //Set all lower bits
        endBitFilter = endBitFilter + (endBitFilter - 1);

        ulong startBitFilter = (1UL << startBit) - 1;

        ulong bitFilter = endBitFilter - startBitFilter;

        return (target & bitFilter) >> startBit;
    }
    public static ulong BitFilterSet(ulong target, ulong set, int startBit, int endBit)
    {
        //Make a bit filter and then cut all the wrong bits
        ulong endBitFilter = 1UL << endBit;

        //Set all lower bits
        endBitFilter = endBitFilter + (endBitFilter - 1);

        ulong startBitFilter = (1UL << startBit) - 1;

        ulong bitFilter = endBitFilter - startBitFilter;

        bitFilter = ~bitFilter;

        set <<= startBit;

        return (target & bitFilter) + set;
    }
}
