using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeScript : MonoBehaviour
{
    public OverworldScript os;

    public enum MapNodeType
    {
        Battle,
        Shop,
        FreePiece,
        Event,
        BossBattle
    }
    public MapNodeType nodeType;

    public float truePieceValueTotal;
    public float pieceValueTotal;
    public int pieceTypes;
    public Piece.PieceClass pieceClass;
    public Piece.PieceType[] army;          //Reused for the free pieces or shop pieces
    public Board.EnemyModifier em;

    public int depth;
    public List<MapNodeScript> parents;
    public List<MapNodeScript> children;

    public bool debugRegenerate;

    public TMPro.TMP_Text text;

    public void Start()
    {
        os = FindObjectOfType<OverworldScript>();

        //generate an army
        GenerateArmy();

        if (nodeType == MapNodeType.BossBattle)
        {
            int i = Random.Range(1, 26);

            em = (Board.EnemyModifier)(1 << i);
        }
    }

    public void GenerateArmy()
    {
        army = ArmyGenerator.GenerateArmy(truePieceValueTotal, pieceTypes, 0.5f, 0.5f, pieceClass, em);
        //Recount piece value because it could be off by 1 or something
        pieceValueTotal = 0;
        for (int i = 0; i < army.Length; i++)
        {
            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(army[i]);

            if (pte == null || pte.type == Piece.PieceType.King)
            {
                continue;
            }

            pieceValueTotal += pte.pieceValueX2 / 2f;
        }
        text.text = pieceValueTotal + "";
    }

    private void OnMouseDown()
    {
        os.EnterNode(this);
    }

    public void Update()
    {
        if (debugRegenerate)
        {
            debugRegenerate = false;
            GenerateArmy();
        }
    }
}
