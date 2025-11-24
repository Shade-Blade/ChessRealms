using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class ShopScript : MonoBehaviour
{
    public GameObject shopItemTemplate;
    public GameObject pieceTemplate;
    public GameObject consumableTemplate;
    public GameObject badgeTemplate;

    public GameObject pieceHolder;
    public GameObject consumableHolder;
    public GameObject badgeHolder;

    public Piece.PieceClass pieceClass;

    public int pieceCount;
    public int consumableCount;
    public int badgeCount;

    public BoardScript bs;

    public static ShopScript CreateShop(BoardScript bs, Piece.PieceClass pc, int pieceCount, int consumableCount, int badgeCount)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Events/Shop"));
        ShopScript ss = go.GetComponent<ShopScript>();
        ss.pieceClass = pc;
        ss.bs = bs;
        ss.pieceCount = pieceCount;
        ss.consumableCount = consumableCount;
        ss.badgeCount = badgeCount;
        return ss;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (bs != null)
        {
            bs = FindAnyObjectByType<BoardScript>();
        }
        List<Piece.PieceType> pieces = new List<Piece.PieceType>(GlobalPieceManager.GetPieceClassEntry(pieceClass).normalPieces);

        if (pieceClass == Piece.PieceClass.None)
        {
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

                pieces.Add(GlobalPieceManager.pieceTable[i].type);
            }
        }

        int normalCount = pieceCount - 1;

        if (GlobalPieceManager.GetPieceClassEntry(pieceClass).extraPieces.Length == 0)
        {
            normalCount++;
        }

        while (pieces.Count > normalCount)
        {
            pieces.RemoveAt(Random.Range(0, pieces.Count));
        }

        if (GlobalPieceManager.GetPieceClassEntry(pieceClass).extraPieces.Length > 0)
        {
            pieces.Add(GlobalPieceManager.GetPieceClassEntry(pieceClass).extraPieces[Random.Range(0, GlobalPieceManager.GetPieceClassEntry(pieceClass).extraPieces.Length)]);
        }

        pieces.Sort((a, b) => (GlobalPieceManager.GetPieceTableEntry(a).pieceValueX2 - GlobalPieceManager.GetPieceTableEntry(b).pieceValueX2));

        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject si = Instantiate(shopItemTemplate, pieceHolder.transform);
            GameObject go = Instantiate(pieceTemplate, si.transform);
            go.transform.parent = si.transform;

            si.transform.localPosition = Vector3.left * 3 + Vector3.right * ((6f * i) / (pieces.Count - 1)) + Vector3.forward * -0.5f;
            go.GetComponent<SetupPieceScript>().bs = bs;
            go.GetComponent<SetupPieceScript>().Setup(Piece.SetPieceType(pieces[i], 0));
        }

        List<Move.ConsumableMoveType> consumables = new List<Move.ConsumableMoveType>();
        for (int i = 1; i < (22 + 1); i++)
        {
            consumables.Add((Move.ConsumableMoveType)i);
        }
        while (consumables.Count > consumableCount)
        {
            consumables.RemoveAt(Random.Range(0, consumables.Count));
        }
        for (int i = 0; i < consumables.Count; i++)
        {
            GameObject si = Instantiate(shopItemTemplate, consumableHolder.transform);
            GameObject go = Instantiate(consumableTemplate, si.transform);
            go.transform.parent = si.transform;

            si.transform.localPosition = Vector3.left * 1f + Vector3.right * ((2f * i) / (consumables.Count - 1)) + Vector3.forward * -0.5f;
            go.GetComponent<ConsumableScript>().bs = bs;
            go.GetComponent<ConsumableScript>().Setup(consumables[i]);
        }

        List<Board.PlayerModifier> badges = new List<Board.PlayerModifier>();
        for (int i = 1; i < (28 + 1); i++)
        {
            if (!MainManager.Instance.playerData.HasBadge((Board.PlayerModifier)(1 << i)))
            {
                badges.Add((Board.PlayerModifier)(1 << i));
            }
        }
        while (badges.Count > badgeCount)
        {
            badges.RemoveAt(Random.Range(0, badges.Count));
        }
        for (int i = 0; i < badges.Count; i++)
        {
            GameObject si = Instantiate(shopItemTemplate, badgeHolder.transform);
            GameObject go = Instantiate(badgeTemplate, si.transform);
            go.transform.parent = si.transform;

            si.transform.localPosition = Vector3.left * 1f + Vector3.right * ((2f * i) / (badges.Count - 1)) + Vector3.forward * -0.5f;
            go.GetComponent<BadgeScript>().bs = bs;
            go.GetComponent<BadgeScript>().Setup(badges[i]);
        }
    }
}
