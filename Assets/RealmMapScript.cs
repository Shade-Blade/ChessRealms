using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealmMapScript : MonoBehaviour
{
    public List<MapNodeScript> mapNodes;

    public GameObject mapNodeTemplate;

    public MapNodeScript current;
    public MapNodeScript lastNode;

    public Piece.PieceClass pieceClass;

    public OverworldScript os;

    public TMPro.TMP_Text realmText;

    public float baseDifficulty;

    public bool init = false;

    public void Init()
    {
        init = true;
        realmText.text = GlobalPieceManager.GetPieceClassEntry(pieceClass).name + " Realm";
        GenerateMap();
        mapNodes = new List<MapNodeScript>(GetComponentsInChildren<MapNodeScript>());
        InitNodes();
    }

    public void Start()
    {
        if (!init)
        {
            Init();
        }
    }

    public void GenerateMap()
    {
        realmText.text = GlobalPieceManager.GetPieceClassEntry(pieceClass).name + " Realm";

        //-3, 3.5
        //3, 7.5

        Vector3 startPos = new Vector3(-3, 3.5f, 0);
        Vector3 endPos = new Vector3(3, 7.5f, 0);
        Vector3 delta = new Vector3(1, -1, 0);

        MapNodeScript mns = MakeMapNodeScript(startPos, Mathf.Min(500, baseDifficulty), 2, MapNodeScript.MapNodeType.Start);
        MapNodeScript pastMNS = mns;

        //todo: find better way to do stuff to have branching paths

        //hardcoded shop after start
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.2f) + delta * Random.Range(-0.2f, 0.2f), baseDifficulty, (int)Mathf.Clamp(4 + (baseDifficulty / 10f), 4, 7), MapNodeScript.MapNodeType.Shop);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(-0.4f, 0.4f), Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 1)), 3, MapNodeScript.MapNodeType.Battle);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        //2 battles on some branches
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.6f) + delta * Random.Range(-0.4f, 0.4f), Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 2)), 4, MapNodeScript.MapNodeType.Battle);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        //hardcoded shop before boss
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.8f) + delta * Random.Range(-0.2f, 0.2f), baseDifficulty, (int)Mathf.Clamp(4 + (baseDifficulty / 10f), 4, 7), MapNodeScript.MapNodeType.Shop);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        mns = MakeMapNodeScript(endPos, Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 3)), 5, MapNodeScript.MapNodeType.BossBattle);
        pastMNS.children.Add(mns);

        lastNode = mns;
    }

    MapNodeScript MakeMapNodeScript(Vector3 position, float difficulty, int types, MapNodeScript.MapNodeType nodeType)
    {
        GameObject go = Instantiate(mapNodeTemplate, transform);
        go.transform.localPosition = position;
        MapNodeScript mns = go.GetComponent<MapNodeScript>();
        mns.os = os;
        mns.pieceClass = pieceClass;
        mns.truePieceValueTotal = difficulty;
        mns.pieceTypes = types;
        mns.nodeType = nodeType;
        mns.GenerateArmy();
        return mns;
    }

    public void InitNodes()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            mapNodes[i].pieceClass = pieceClass;

            if (mapNodes[i].nodeType == MapNodeScript.MapNodeType.Start)
            {
                current = mapNodes[i];
            }

            if (mapNodes[i].nodeType == MapNodeScript.MapNodeType.Battle)
            {
                mapNodes[i].GenerateArmy();
            }
            if (mapNodes[i].nodeType == MapNodeScript.MapNodeType.BossBattle)
            {
                mapNodes[i].GenerateArmy();
            }
        }

        //set depth
        void DepthSet(MapNodeScript mns, int depth)
        {
            if (mns.depth == 0 || mns.depth > depth)
            {
                mns.depth = depth;
            }
            for (int i = 0; i < mns.children.Count; i++)
            {
                DepthSet(mns.children[i], depth + 1);
            }
        }

        DepthSet(current, 0);
    }

    public void CompleteNode()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            mapNodes[i].active = false;
            mapNodes[i].UpdateState();
            mapNodes[i].UpdateColor();
        }

        if (current == null)
        {
            return;
        }
        current.done = true;
        current.active = false;
        current.CompleteNode();

        if (current == lastNode)
        {
            CompleteMap();
        }
    }

    public void CompleteMap()
    {
        //something going up to overworld script to make a realm map
        os.ReturnFromRealmMap();
    }
}
