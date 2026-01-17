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
        BossBattle,
        FinalBossBattle,
        WorldNode,  //world map node
    }
    public MapNodeType nodeType;

    public SpriteRenderer backSprite;
    public SpriteRenderer borderSprite;
    public float truePieceValueTotal;
    public float pieceValueTotal;
    public int pieceTypes;
    public Piece.PieceClass pieceClass;
    public uint[] army;          //Reused for the free pieces or shop pieces
    public Board.EnemyModifier em;

    public bool active;
    public bool done;
    public int depth;
    public List<MapNodeScript> parents;
    public List<MapNodeScript> children;
    public List<LineRenderer> childrenLines;
    public List<LineRenderer> parentLines;

    public bool debugRegenerate;

    public TMPro.TMP_Text text;

    public int nodeSeed;

    public bool isHover;

    public void Start()
    {
        if (parents == null)
        {
            parents = new List<MapNodeScript>();
        }
        if (children == null)
        {
            children = new List<MapNodeScript>();
        }
        if (childrenLines == null)
        {
            childrenLines = new List<LineRenderer>();
        }
        if (parentLines == null)
        {
            parentLines = new List<LineRenderer>();
        }

        if (os == null)
        {
            os = FindObjectOfType<OverworldScript>();
        }

        switch (nodeType)
        {
            case MapNodeType.Start:
                text.text = "Start";
                done = true;
                break;
            case MapNodeType.Battle:
            case MapNodeType.BossBattle:
            case MapNodeType.FinalBossBattle:
                //generate an army
                GenerateArmy();
                break;
            case MapNodeType.Shop:
                text.text = "Shop";
                //Make the army contain the shop pieces?
                break;
            case MapNodeType.WorldNode:
                text.text = GlobalPieceManager.GetPieceClassEntry(pieceClass).name + "\nRealm\n" + truePieceValueTotal;
                break;
        }

        for (int i = 0; i < children.Count; i++)
        {
            GameObject lineObject = Instantiate(Resources.Load<GameObject>("Map/MapTrail"), transform);
            lineObject.transform.position = transform.position + Vector3.forward * 0.2f;
            childrenLines.Add(lineObject.GetComponent<LineRenderer>());
            childrenLines[childrenLines.Count - 1].SetPositions(new Vector3[] { lineObject.transform.position, children[i].transform.position + Vector3.forward * 0.2f });
            childrenLines[childrenLines.Count - 1].startColor = new Color(0, 0, 0, 0.6f);
            childrenLines[childrenLines.Count - 1].endColor = new Color(0, 0, 0, 0.6f);
            children[i].parentLines.Add(childrenLines[childrenLines.Count - 1]);
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
        for (int i = 0; i < children.Count; i++)
        {
            if (!children[i].done)
            {
                children[i].active = true;
            }
        }
        UpdateChildren();
        UpdateColor();
        for (int i = 0; i < parents.Count; i++)
        {
            parents[i].UpdateState();
        }
    }

    public void UpdateColor()
    {
        if (done)
        {
            backSprite.color = Color.cyan;
            borderSprite.color = new Color(1,1,1, 1);

            for (int i = 0; i < parentLines.Count; i++)
            {
                if (parents[i].done)
                {
                    parentLines[i].startColor = new Color(0, 1, 1, 0.6f);
                    parentLines[i].endColor = new Color(0, 1, 1, 0.6f);
                }
            }

            for (int i = 0; i < childrenLines.Count; i++)
            {
                if (children[i].done)
                {
                    childrenLines[i].startColor = new Color(0, 1, 1, 0.6f);
                    childrenLines[i].endColor = new Color(0, 1, 1, 0.6f);
                }
                else
                {
                    if (children[i].active)
                    {
                        childrenLines[i].startColor = new Color(0, 1, 0, 0.6f);
                        childrenLines[i].endColor = new Color(0, 1, 0, 0.6f);
                    }
                    else
                    {
                        childrenLines[i].startColor = new Color(0, 0, 0, 0.6f);
                        childrenLines[i].endColor = new Color(0, 0, 0, 0.6f);
                    }
                }
            }
        } else if (active)
        {
            backSprite.color = Color.green;
            borderSprite.color = Color.white;
            if (nodeType == MapNodeType.WorldNode)
            {
                backSprite.color = GlobalPieceManager.GetPieceClassEntry(pieceClass).backgroundColorLight;
            }

            for (int i = 0; i < childrenLines.Count; i++)
            {
                childrenLines[i].startColor = new Color(0, 0, 0, 0.6f);
                childrenLines[i].endColor = new Color(0, 0, 0, 0.6f);
            }
        }
        else
        {
            backSprite.color = new Color(0, 0.5f, 0, 1f);
            borderSprite.color = new Color(0.5f, 0.5f, 0.5f, 1);
            if (nodeType == MapNodeType.WorldNode)
            {
                backSprite.color = GlobalPieceManager.GetPieceClassEntry(pieceClass).squareColorDark;
            }

            for (int i = 0; i < childrenLines.Count; i++)
            {
                childrenLines[i].startColor = new Color(0, 0, 0, 0.6f);
                childrenLines[i].endColor = new Color(0, 0, 0, 0.6f);
            }
        }
    }

    public void UpdateState()
    {
        UpdateColor();
    }

    public void GenerateArmy()
    {
        if (nodeType == MapNodeType.BossBattle)
        {
            int i = Random.Range(1, 24);

            em = (Board.EnemyModifier)(1 << i);
        }
        if (nodeType == MapNodeType.FinalBossBattle)
        {
            em = Board.EnemyModifier.Zenith;
        }
        army = ArmyGenerator.ConvertPieceTypeArray(ArmyGenerator.GenerateArmy(truePieceValueTotal, pieceTypes, truePieceValueTotal > 15f ? 0.5f + ((10f * 0.25f) / (truePieceValueTotal - 5)) : 0.75f, 0.5f, pieceClass, em));
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
        if (active && !done)
        {
            os.EnterNode(this);
        }
    }
    public void OnMouseOver()
    {
        isHover = true;
    }

    public void Update()
    {
        //make the fade in work properly :P
        for (int i = 0; i < childrenLines.Count; i++)
        {
            childrenLines[i].SetPositions(new Vector3[] { transform.position + Vector3.forward * 0.2f, children[i].transform.position + Vector3.forward * 0.2f });
        }

        if (isHover)
        {
            switch (nodeType) {
                case MapNodeType.Battle:
                    HoverTextMasterScript.Instance.MakeHoverPopup(army, "Army Value: " + pieceValueTotal);
                    break;
                case MapNodeType.BossBattle:
                case MapNodeType.FinalBossBattle:
                    HoverTextMasterScript.Instance.MakeHoverPopup(army, "Army Value: " + pieceValueTotal + "<line><boss," + em.ToString() + "> Boss: " + em.ToString() + "\n" + Board.GetEnemyModifierDescription(null, em));
                    break;
                case MapNodeType.WorldNode:
                    HoverTextMasterScript.Instance.SetHoverText("<outlinecolordark," + MainManager.ColorToString(GlobalPieceManager.GetPieceClassEntry(pieceClass).squareColorLight) + ">" + GlobalPieceManager.GetPieceClassEntry(pieceClass).name + " Realm</color></font><line>" + GlobalPieceManager.GetPieceClassEntry(pieceClass).description + "<line>Base Power: " + truePieceValueTotal);
                    break;
                default:
                    HoverTextMasterScript.Instance.SetHoverText(nodeType.ToString());
                    break;
            }
        }
        isHover = false;

        if (debugRegenerate)
        {
            debugRegenerate = false;
            GenerateArmy();
        }
    }
}
