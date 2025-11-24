using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeScript : MonoBehaviour
{
    public OverworldScript os;

    public enum MapNodeType
    {
        Start,  //free node, non selectable
        Battle,
        Shop,
        FreePiece,
        Event,
        BossBattle
    }
    public MapNodeType nodeType;

    public SpriteRenderer backSprite;
    public float truePieceValueTotal;
    public float pieceValueTotal;
    public int pieceTypes;
    public Piece.PieceClass pieceClass;
    public Piece.PieceType[] army;          //Reused for the free pieces or shop pieces
    public Board.EnemyModifier em;

    public bool active;
    public bool done;
    public int depth;
    public List<MapNodeScript> parents;
    public List<MapNodeScript> children;
    public List<LineRenderer> childrenLines;

    public bool debugRegenerate;

    public TMPro.TMP_Text text;

    public void Start()
    {
        os = FindObjectOfType<OverworldScript>();

        switch (nodeType)
        {
            case MapNodeType.Start:
                text.text = "Start";
                done = true;
                break;
            case MapNodeType.Battle:
            case MapNodeType.BossBattle:
                //generate an army
                GenerateArmy();
                break;
            case MapNodeType.Shop:
                text.text = "Shop";
                break;
        }

        for (int i = 0; i < children.Count; i++)
        {
            GameObject lineObject = Instantiate(Resources.Load<GameObject>("Map/MapTrail"), transform);
            lineObject.transform.position = transform.position + Vector3.forward * -0.1f;
            childrenLines.Add(lineObject.GetComponent<LineRenderer>());
            childrenLines[childrenLines.Count - 1].SetPositions(new Vector3[] { lineObject.transform.position, children[i].transform.position });
            childrenLines[childrenLines.Count - 1].startColor = new Color(0, 1, 0, 0.6f);
            childrenLines[childrenLines.Count - 1].endColor = new Color(0, 1, 0, 0.6f);

            children[i].AddParent(this);
        }
        UpdateColor();
    }

    public void AddParent(MapNodeScript mns)
    {
        parents.Add(mns);
        if (mns.done && !active && !done)
        {
            active = true;
        }
        UpdateColor();
    }

    public void UpdateChildren()
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].UpdateState();
        }
    }

    public void CompleteNode()
    {
        done = true;
        active = false;
        UpdateColor();
        for (int i = 0; i < children.Count; i++)
        {
            if (!children[i].done)
            {
                children[i].active = true;
            }
        }
        UpdateChildren();
    }

    public void UpdateColor()
    {
        if (done)
        {
            backSprite.color = Color.cyan;
        } else if (active)
        {
            backSprite.color = Color.green;
        } else
        {
            backSprite.color = new Color(0, 0.5f, 0, 0.75f);
        }
    }

    public void UpdateState()
    {
        UpdateColor();
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

        if (nodeType == MapNodeType.BossBattle)
        {
            int i = Random.Range(1, 26);

            em = (Board.EnemyModifier)(1 << i);
        }
    }

    private void OnMouseDown()
    {
        if (active && !done)
        {
            os.EnterNode(this);
        }
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
