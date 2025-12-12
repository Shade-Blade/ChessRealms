using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86.Bmi1;
using static Unity.Burst.Intrinsics.X86.Popcnt;

[System.Serializable]
public class PlayerData
{
    public Piece.PieceType[] army;

    public Move.ConsumableMoveType[] consumables;
    public Board.PlayerModifier[] badges;

    public int difficulty;

    public int coins;
    public int realmsComplete;
    public int realmBattlesComplete;
    public int battlesComplete;
    public int nodesComplete;
    public List<int> realmRouteChoices;
    public ulong seed;

    public int undosLeft;
    public int retriesLeft;

    public PlayerData()
    {
        army = new Piece.PieceType[16];

        consumables = new Move.ConsumableMoveType[4];
        badges = new Board.PlayerModifier[4];

        difficulty = 3;
        coins = 5;

        //the normal king position to start with
        army[5] = Piece.PieceType.King;

        coins = 0;
        realmsComplete = 0;
        realmBattlesComplete = 0;
        battlesComplete = 0;
        nodesComplete = 0;
        realmRouteChoices = new List<int>();

        undosLeft = 3;
        retriesLeft = 1;
    }

    public void GenerateSeed()
    {
        //arbitrary thing I guess
        UnityEngine.Random.InitState(DateTime.UtcNow.ToString().GetHashCode());

        uint randomA = (uint)UnityEngine.Random.Range(0, 256);
        ulong newRandom = 0;

        //is this good for avoiding correlation?
        for (int k = 0; k < 8; k++)
        {
            newRandom |= randomA;
            newRandom <<= 8;
            randomA = (uint)UnityEngine.Random.Range(0, 256);
        }
        seed = newRandom;
    }

    public bool HasBadge(Board.PlayerModifier pm)
    {
        for (int i = 0; i < badges.Length; i++)
        {
            if ((badges[i] & pm) != 0)
            {
                return true;
            }
        }
        return false;
    }
    public Board.PlayerModifier GetPlayerModifier()
    {
        Board.PlayerModifier output = 0;
        for (int i = 0; i < badges.Length; i++)
        {
            output |= badges[i];
        }
        return output;
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

    public Canvas Canvas
    {
        get
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }
            return canvas;
        }
    }
    public Canvas canvas;

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
    public string lastTextboxMenuResult;

    public enum GameConst
    {
        PieceCount,
        PieceValue,
        ResetsLeft,
        UndosLeft
    }

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
        playerData.GenerateSeed();
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

        //Now 680k ish with more random small optimizations
        //Now 900k with more aggressive stuff to avoid updating piece bitboards as much
        //Now 1.2m with inlining annotations for tiny methods
        //1.5m with other random tiny things

        //90 ish seconds for depth 6 normal

        //Note that this does not match normal perft test because the perft test does not check for checks
        //1
        //20
        //400
        //8902
        //197742
        //4896537
        //120882519

        //Bugs fixed now
        /*
        for (int i = 0; i <= 4; i++)
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();

            Board board = new Board();
            board.Setup(Board.BoardPreset.Normal);

            ulong perftResult = Board.PerftTest(ref board, i);

            currentTime = DateTime.UtcNow;
            long unixTimeEnd = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            string output = ("Perft took " + ((unixTimeEnd - unixTime)/ 1000d) + " seconds for " + perftResult + " positions at depth + " + i + " = " + "(" + (perftResult / ((unixTimeEnd - unixTime) / 1000d)) + " pos/sec) (" + (((unixTimeEnd - unixTime) / 1000d) / perftResult) + " s per pos)");

            //BattleBoardScript bbs = FindObjectOfType<BattleBoardScript>();
            //bbs.thinkingText.text = output;
            Debug.Log(output);
        } 
        */

        //AI testing
        /*
        ChessAI cai = new ChessAI();
        Board board = new Board();
        board.Setup(Board.BoardPreset.Normal);
        cai.board = board;
        cai.InitAI(3);
        cai.SetDifficulty(3);
        cai.maxDepth = 4;
        cai.keepSearching = true;
        cai.searchTime = 0;
        cai.moveFound = false;
        cai.AlphaBetaAI(null);
        */
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


    public Vector2 RealMousePos()
    {
        Vector2 mousePos = (Vector2)Input.mousePosition;

        mousePos.x = (mousePos.x) * (CanvasWidth() / Screen.width);
        mousePos.y = (mousePos.y) * (CanvasHeight() / Screen.height);
        return mousePos;
    }
    public static float CanvasHeight()
    {
        return 450;
    }
    public static float CanvasWidth()
    {
        return (Screen.width / (0.0f + Screen.height)) * 450;
    }


    public void EndRun()
    {
        //go back to main menu
        SceneManager.LoadScene("MainMenuScene");
    }

    public static string FixCapCase(string input)
    {
        string output = "";

        for (int i = 0; i < input.Length; i++)
        {
            if (output.Length > 0 && ((input[i] + "").ToLowerInvariant() != (input[i] + "")))
            {
                output += " ";
            }
            output += input[i];
        }
        return output;
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

    public static Vector2 LineCollision(Vector2 a, Vector2 b, Vector2 a2, Vector2 b2)
    {
        //some determinants of some matrixes
        float t = ((a.x - a2.x) * (a2.y - b2.y) - (a.y - a2.y) * (a2.x - b2.x)) / ((a.x - b.x) * (a2.y - b2.y) - (a.y - b.y) * (a2.x - b2.x));
        float u = ((a.x - b.x) * (a.y - a2.y) - (a.y - b.y) * (a.x - a2.x)) / ((a.x - b.x) * (a2.y - b2.y) - (a.y - b.y) * (a2.x - b2.x));

        if (t < 0 || t > 1 || u < 0 || u > 1)
        {
            return Vector2.positiveInfinity;
        }

        return (a.x + t) * Vector2.right + (a.y + t * (b.y - a.y)) * Vector2.up;
    }
    public static Vector2 RectangleCollision(Vector2 a, Vector2 b, float bottom, float top, float left, float right)
    {
        Vector2 leftI = LineCollision(a, b, new Vector2(left, bottom), new Vector2(left, top));
        Vector2 rightI = LineCollision(a, b, new Vector2(right, bottom), new Vector2(right, top));
        Vector2 topI = LineCollision(a, b, new Vector2(left, top), new Vector2(right, top));
        Vector2 bottomI = LineCollision(a, b, new Vector2(left, bottom), new Vector2(right, bottom));

        if (leftI != Vector2.positiveInfinity)
        {
            return leftI;
        }
        if (rightI != Vector2.positiveInfinity)
        {
            return rightI;
        }
        if (topI != Vector2.positiveInfinity)
        {
            return topI;
        }
        if (bottomI != Vector2.positiveInfinity)
        {
            return bottomI;
        }
        return Vector2.positiveInfinity;
    }

    public static int ParseHex2Byte(string parse)
    {
        //Debug.Log(parse + " " + parse.Length);
        if (parse.Length != 2)
        {
            return 0;
        }

        int CharToInt(char a)
        {
            //Debug.Log("ith " + a);
            if (int.TryParse(a + "", out int a2))
            {
                return a2;
            }
            else
            {
                return (a - 'a' + 10);
            }
        }

        char a = parse[0];
        char b = parse[1];

        int a2 = CharToInt(a);
        int b2 = CharToInt(b);

        return a2 * 16 + b2;
    }
    public static Color? ParseColor(string parse)
    {
        parse = parse.ToLower();

        int CharToInt(char a)
        {
            //Debug.Log("ith " + a);
            if (int.TryParse(a + "", out int a2))
            {
                return a2;
            }
            else
            {
                return (a - 'a' + 10);
            }
        }

        //v2 version
        if (parse.Length > 1 && parse[0] == '#')
        {
            //Next up is a hex code thing
            string hexValue = parse.Substring(1);

            //something like FFFFFFFF might not work with ToInt32 since that returns an int not a uint
            //long colorVal = Convert.ToInt64("0x" + hexValue, 16);
            //uint realcolor = (uint)colorVal;

            //check correctness
            if (hexValue.Length != 4 && hexValue.Length != 6 && hexValue.Length != 8)
            {
                //Wrong length
                //Debug.Log("Wrong length " + hexValue + " has length " + hexValue.Length);
                return null;
            }

            float red = 0;
            float green = 0;
            float blue = 0;
            float alpha = 0;
            switch (hexValue.Length)
            {
                /*
                case 4: //weird 16 bit format (5 bits each for rgb, 1 bit for a) (
                    red = ((((realcolor) % (1 << 16)) >> 11) / 31f);
                    green = ((((realcolor) % (1 << 11)) >> 6) / 31f);
                    blue = ((((realcolor) % (1 << 6)) >> 1) / 31f);
                    alpha = ((((realcolor) % (1 << 1)) >> 0) / 1f);
                    break;
                */
                case 4: //rgba
                    red = CharToInt(hexValue[0]) / 15f;
                    green = CharToInt(hexValue[0]) / 15f;
                    blue = CharToInt(hexValue[0]) / 15f;
                    alpha = CharToInt(hexValue[0]) / 15f;
                    //red = ((((realcolor) % (1 << 16)) >> 12) / 15f);
                    //green = ((((realcolor) % (1 << 12)) >> 8) / 15f);
                    //blue = ((((realcolor) % (1 << 8)) >> 4) / 15f);
                    //alpha = ((((realcolor) % (1 << 4)) >> 0) / 15f);
                    break;
                case 6: //rrggbb
                    red = ParseHex2Byte(hexValue[0] + "" + hexValue[1]) / 255f;
                    green = ParseHex2Byte(hexValue[2] + "" + hexValue[3]) / 255f;
                    blue = ParseHex2Byte(hexValue[4] + "" + hexValue[5]) / 255f;
                    alpha = 1;
                    //red = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                    //green = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                    //blue = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                    //alpha = 1;
                    break;
                case 8: //rrggbbaa
                    red = ParseHex2Byte(hexValue[0] + "" + hexValue[1]) / 255f;
                    green = ParseHex2Byte(hexValue[2] + "" + hexValue[3]) / 255f;
                    blue = ParseHex2Byte(hexValue[4] + "" + hexValue[5]) / 255f;
                    alpha = ParseHex2Byte(hexValue[6] + "" + hexValue[7]) / 255f;
                    //red = ((((realcolor) % (1L << 32)) >> 24) / 255f);
                    //green = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                    //blue = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                    //alpha = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                    break;
            }

            //Debug.Log(parse + " Colors: " + red + " " + green + " " + blue + " " + alpha);
            return new Color(red, green, blue, alpha);
        }
        else
        {
            //Debug.Log("Invalid start " + parse);
            return null;
        }

        /*
        //fail parse: null
        try
        {
            if (parse.Length > 1 && parse[0] == '#')
            {
                //Next up is a hex code thing
                string hexValue = parse.Substring(1);

                //something like FFFFFFFF might not work with ToInt32 since that returns an int not a uint
                long colorVal = Convert.ToInt64("0x" + hexValue, 16);
                uint realcolor = (uint)colorVal;

                //check correctness
                if (hexValue.Length != 4 && hexValue.Length != 6 && hexValue.Length != 8)
                {
                    //Wrong length
                    //Debug.Log("Wrong length " + hexValue + " has length " + hexValue.Length);
                    return null;
                }

                float red = 0;
                float green = 0;
                float blue = 0;
                float alpha = 0;
                switch (hexValue.Length)
                {
                    
                    case 4: //weird 16 bit format (5 bits each for rgb, 1 bit for a) (
                        red = ((((realcolor) % (1 << 16)) >> 11) / 31f);
                        green = ((((realcolor) % (1 << 11)) >> 6) / 31f);
                        blue = ((((realcolor) % (1 << 6)) >> 1) / 31f);
                        alpha = ((((realcolor) % (1 << 1)) >> 0) / 1f);
                        break;
                    
                    case 4: //rgba
                        red = ((((realcolor) % (1 << 16)) >> 12) / 15f);
                        green = ((((realcolor) % (1 << 12)) >> 8) / 15f);
                        blue = ((((realcolor) % (1 << 8)) >> 4) / 15f);
                        alpha = ((((realcolor) % (1 << 4)) >> 0) / 15f);
                        break;
                    case 6: //rrggbb
                        red = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                        green = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                        blue = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                        alpha = 1;
                        break;
                    case 8: //rrggbbaa
                        red = ((((realcolor) % (1L << 32)) >> 24) / 255f);
                        green = ((((realcolor) % (1 << 24)) >> 16) / 255f);
                        blue = ((((realcolor) % (1 << 16)) >> 8) / 255f);
                        alpha = ((((realcolor) % (1 << 8)) >> 0) / 255f);
                        break;
                }

                //Debug.Log("Colors: " + red + " " + green + " " + blue + " " + alpha);
                return new Color(red, green, blue, alpha);
            }
            else
            {
                //Debug.Log("Invalid start " + parse);
                return null;
            }

        } catch (FormatException)
        {
            Debug.Log("General parse failure: " + parse);
            return null;
        }
        */
    }
    public static string ColorToString(Color a)
    {
        //inverse of parse color
        string output = "#";

        int intVal = (int)(255 * a.r + 0.5f);

        string IntToHex(int a)
        {
            //Debug.Log("ith " + a);
            if (a < 10)
            {
                return a.ToString();
            }
            else
            {
                return ((char)('a' + (char)(a - 10))).ToString();
            }
        }

        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);
        intVal = (int)(255 * (a.g) + 0.5f);
        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);
        intVal = (int)(255 * (a.b) + 0.5f);
        output += IntToHex(intVal / 16);
        output += IntToHex(intVal % 16);

        if (a.a != 1)
        {
            intVal = (int)(255 * a.a + 0.5f);
            output += IntToHex(intVal / 16);
            output += IntToHex(intVal % 16);
        }

        //Debug.Log(a + " " + output);

        return output;
    }

    public static int ConvertSeed(ulong seed, int value)
    {
        ulong subseed = seed << 1;
        seed = subseed ^ seed;

        //arbitrary number
        value *= 731371737;

        int output = 0;
        for (int i = 0; i < 32; i++)
        {
            ulong bitIndex = 1ul << (i * 2);

            //seed xor
            if ((seed ^ bitIndex) != 0)
            {
                output += 1 << i;
            }
        }

        return output ^ value;
    }
    public static int ConvertSeed(ulong seed, int valueA, int valueB)
    {
        ulong subseed = seed << 1;
        seed = subseed ^ seed;

        //arbitrary numbers
        valueA += valueB * 213232531;
        valueA *= 731371737;

        seed ^= 0xabcdabcdabcdabcd;
        seed = seed * (ulong)valueA * 73137173;

        int output = 0;
        for (int i = 0; i < 32; i++)
        {
            ulong bitIndex = 1ul << (i * 2);

            //seed xor
            if ((seed ^ bitIndex) != 0)
            {
                output += 1 << i;
            }
        }

        return output ^ valueA;
    }
    public static int ConvertSeed(int valueA, int valueB)
    {
        ulong seed = Instance.playerData.seed;
        ulong subseed = seed << 1;
        seed = subseed ^ seed;

        //arbitrary numbers
        valueA += valueB * 213232531;
        valueA *= 731371737;

        seed ^= 0xabcdabcdabcdabcd;
        seed = seed * (ulong)valueA * 73137173;

        int output = 0;
        for (int i = 0; i < 32; i++)
        {
            ulong bitIndex = 1ul << (i * 2);

            //seed xor
            if ((seed & bitIndex) != 0)
            {
                output += 1 << i;
            }
        }

        return output ^ valueA;
    }
    public static int ConvertSeedNode(int index)
    {
        return ConvertSeed(MainManager.Instance.playerData.realmsComplete, index);
    }
    public static int ConvertSeedNodeOffset(int index, int offset)
    {
        return ConvertSeed(offset, offset) ^ ConvertSeed(MainManager.Instance.playerData.realmsComplete, index);
    }
    public static int ConvertSeedNodeOffset(int offset)
    {
        return ConvertSeed(offset, offset) ^ ConvertSeed(MainManager.Instance.playerData.realmsComplete, MainManager.Instance.playerData.battlesComplete);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        //2 casts = bad?
        //But the assembly might not do anything with those casts
        ulong isolated = ((bitboard) & (ulong)(-(long)bitboard));

        //1 bit left
        output = bitboard - isolated;
        int index = debrujin_index64[(isolated * 0x03f79d71b4cb0a89) >> 58];

        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopBitboardLSB1(ulong bitboard)
    {
        if (bitboard == 0)
        {
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

        /*
        ulong isolated = (bitboard) ^ (bitboard - 1);
        isolated -= isolated >> 1;
        */

        //-x = (~x + 1)

        //2 casts = bad?
        //But the assembly might not do anything with those casts
        //ulong isolated = ((bitboard) & (ulong)(-(long)bitboard));

        //1 bit left
        int index = debrujin_index64[((((bitboard) & (ulong)(-(long)bitboard))) * 0x03f79d71b4cb0a89) >> 58];

        return index;
    }

    //Slower than LSB1
    //but at least it looks simpler than the wacky magic number stuff
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopBitboardMSB1(ulong bitboard)
    {
        ulong test = bitboard;
        int index = 0;

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

    //for completeness I'll add in the full 15 checker here
    //Use this to compute row cardinality?
    public static (ulong, ulong, ulong, ulong) BitboardCardinality(ulong x1, ulong x2, ulong x3, ulong x4, ulong x5, ulong x6, ulong x7, ulong x8, ulong x9, ulong x10, ulong x11, ulong x12, ulong x13, ulong x14, ulong x15)
    {
        ulong one1 = x1 ^ x2 ^ x3;
        ulong one2 = x4 ^ x5 ^ x6;
        ulong one3 = x7 ^ x8 ^ x9;
        ulong one4 = x10 ^ x11 ^ x12;
        ulong one5 = x13 ^ x14 ^ x15;
        ulong one6 = one1 ^ one2 ^ one3;
        ulong ones = one4 ^ one5 ^ one6;

        ulong two1 = (x1 & x2) | (x3 & (x1 | x2));
        ulong two2 = (x4 & x5) | (x6 & (x4 | x5));
        ulong two3 = (x7 & x8) | (x9 & (x7 | x8));
        ulong two4 = (x10 & x11) | (x12 & (x10 | x11));
        ulong two5 = (x13 & x14) | (x15 & (x13 | x14));
        ulong two6 = (one1 & one2) | (one3 & (one1 | one2));
        ulong two7 = (one4 & one5) | (one6 & (one4 | one5));
        ulong two8 = (two1 ^ two2 ^ two3);
        ulong two9 = (two4 ^ two5 ^ two6);
        ulong twos = (two7 ^ two8 ^ two9);

        ulong four1 = (two1 & two2) | (two3 & (two1 | two2));
        ulong four2 = (two4 & two5) | (two6 & (two4 | two5));
        ulong four3 = (two7 & two8) | (two9 & (two7 | two8));

        ulong four = four1 ^ four2 ^ four3;
        ulong eight = (four1 & four2) | (four3 & (four1 | four2));

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetWraparoundCutoff(int wrapAround)
    {
        switch (wrapAround)
        {
            case -8:
                return 0;
            case -7:
                return NO_BCDEFGH_FILE;
            case -6:
                return NO_CDEFGH_FILE;
            case -5:
                return NO_DEFGH_FILE;
            case -4:
                return NO_EFGH_FILE;
            case -3:
                return NO_FGH_FILE;
            case -2:
                return NO_GH_FILE;
            case -1:
                return NO_H_FILE;
            default:
                return MoveGenerator.BITBOARD_PATTERN_FULL;
            case 1:
                return NO_A_FILE;
            case 2:
                return NO_AB_FILE;
            case 3:
                return NO_ABC_FILE;
            case 4:
                return NO_ABCD_FILE;
            case 5:
                return NO_ABCDE_FILE;
            case 6:
                return NO_ABCDEF_FILE;
            case 7:
                return NO_ABCDEFG_FILE;
            case 8:
                return 0;
        }
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
    public string GetTextFromFile(string filePath, int y, int x = 0)
    {
        string s = Resources.Load<TextAsset>(filePath).text;
        if (s == null)
        {
            Debug.LogError("[GetTextFromFile] Null file");
            return "<color,red>Invalid File</color>";
        }

        string[][] parse = CSVParse(s);
        if (parse == null)
        {
            Debug.LogError("[GetTextFromFile] CSV failure");
            return "<color,red>CSV Parsing Failure</color>";
        }
        if (y >= parse.Length)
        {
            Debug.LogError("[GetTextFromFile] Index " + y + " >= length " + parse.Length);
            return "<color,red>Invalid Line: Index " + y + " >= length " + parse.Length + "</color>";
        }
        if (x >= parse[y].Length)
        {
            Debug.LogError("[GetTextFromFile] subline: Index " + x + " >= length " + parse[y].Length);
            return "<color,red>Invalid Subline: Index " + x + " >= length " + parse[y].Length + "</color>";
        }

        return parse[y][x];
    }
    public string GetTextFromFile(string[][] file, int y, int x = 0)
    {
        if (file == null)
        {
            Debug.LogError("[GetTextFromFile] Null file");
            return "<color,red>File is null</color>";
        }
        if (y >= file.Length)
        {
            Debug.LogError("[GetTextFromFile] Index " + y + " >= length " + file.Length);
            return "<color,red>Invalid Line: Index " + y + " >= length " + file.Length + "</color>";
        }
        if (x >= file[y].Length)
        {
            Debug.LogError("[GetTextFromFile] subline: Index " + x + " >= length " + file[y].Length);
            return "<color,red>Invalid Subline: Index " + x + " >= length " + file[y].Length + "</color>";
        }

        return file[y][x];
    }
    public static string[][] GetAllTextFromFile(string filePath)
    {
        TextAsset ta = Resources.Load<TextAsset>(filePath);
        if (ta == null)
        {
            return null;
        }
        string s = ta.text;
        if (s == null)
        {
            string[][] output = new string[1][];
            output[0] = new string[1];
            output[0][0] = "<color,red>Invalid File</color>";
            Debug.LogError("[GetAllTextFromFile] File path " + filePath + " could not be read");
            return output;
        }

        string[][] parse = CSVParse(s);
        return parse;
    }

    public string GetConst(int index)
    {
        return GetConst((GameConst)index);
    }
    public string GetConst(GameConst con)
    {
        switch (con)
        {
            //todo: implement using some kind of count of the playerdata army stuff
            case GameConst.PieceCount:
                break;
            case GameConst.PieceValue:
                break;
            case GameConst.ResetsLeft:
                return playerData.retriesLeft + "";
            case GameConst.UndosLeft:
                return playerData.undosLeft + "";
        }

        return "";
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
    public static uint BitFilterSetB(uint target, uint set, int startBit, int endBit)
    {
        //unfortunately there's not a magic intrinsic that does the opposite of bit field extract above :(

        //Make a bit filter and then cut all the wrong bits
        uint endBitFilter = 1u << endBit;

        //Set all lower bits
        endBitFilter = endBitFilter + (endBitFilter - 1);

        uint startBitFilter = (1u << startBit) - 1;

        uint bitFilter = endBitFilter - startBitFilter;

        bitFilter = ~bitFilter;

        //set <<= startBit;

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

    //quadratic easing (positive = fast at start, slow at end, negative = slow at start, fast at end. Heaviness values outside [-1, 1] will return values outside [0,1] near the slow end but will rebound)
    //(graph out the formula for more specifics)
    public static float EasingQuadratic(float input, float heaviness)
    {
        return input * (1 + heaviness) - input * input * heaviness;
    }

    //Unlike the exponential easings, this one has a set time it will reach the end
    //Time to target = sqrt(abs(input - target) / force)
    //Inverse (force) = (abs(input - target) / time^2)
    //Note: calculating a force from a time value would need to use the starting point
    //(and my easing functions are specifically designed so that the starting point is unnecessary)
    public static float EasingQuadraticTime(float input, float target, float force)
    {
        force = Mathf.Abs(force);

        //x - offset
        //the x value is the next value the formula should take (the next input to feed back into this)
        if (input - target < 0)
        {
            force = -force;
        }
        float formulaInput = Time.smoothDeltaTime - Mathf.Sqrt(Mathf.Abs((input - target) / force));

        //point where you reach the target is at x = 0
        if (formulaInput > 0)
        {
            return target;
        }

        float output = force * formulaInput * formulaInput + target;

        return output;
    }
    public static float EasingQuadraticForce(float distance, float time)
    {
        return distance / (time * time);
    }
    public static Vector3 EasingQuadraticTime(Vector3 input, Vector3 target, float force)
    {
        //sus code
        //return new Vector3(EasingQuadraticTime(input.x, target.x, force), EasingQuadraticTime(input.y, target.y, force), EasingQuadraticTime(input.z, target.z, force));
        //^ Not straight: want something that does straight line stuff

        float distance = (input - target).magnitude;
        float newDistance = EasingQuadraticTime(distance, 0, force);

        return (target) - (target - input).normalized * newDistance;
    }
    public static Quaternion EasingQuadraticTime(Quaternion input, Quaternion target, float force)
    {
        //why is the description for the w part different from the x,y,z parts? sus
        //well interpolating like this works fine anyway
        return new Quaternion(EasingQuadraticTime(input.x, target.x, force), EasingQuadraticTime(input.y, target.y, force), EasingQuadraticTime(input.z, target.z, force), EasingQuadraticTime(input.w, target.w, force));
    }
}
