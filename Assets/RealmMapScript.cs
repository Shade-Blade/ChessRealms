using System;
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

    public bool finalBoss;

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

        GenerateMap_LayoutA(startPos, endPos, delta);
    }

    public void GenerateMap_LayoutA(Vector3 startPos, Vector3 endPos, Vector3 delta)
    {
        MapNodeScript mns = MakeMapNodeScript(startPos, Mathf.Min(500, baseDifficulty), 2, MapNodeScript.MapNodeType.Start);
        MapNodeScript pastMNS = mns;

        //b1 is hard but leads to d early
        //b2, c is easier but longer and has 2 battles before the next shop
        //  (Better economy)

        //todo: find better way to do stuff to have branching paths
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(1, 51253));
        MapNodeScript a = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.2f) + delta * UnityEngine.Random.Range(-0.2f, 0.2f), baseDifficulty, (int)Mathf.Clamp(4 + (baseDifficulty / 10f), 4, 7), MapNodeScript.MapNodeType.Shop);
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(2, 51253));
        MapNodeScript b1 = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.4f) + delta * UnityEngine.Random.Range(-0.8f, -0.6f), Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 1.9f)), 3, MapNodeScript.MapNodeType.Battle);
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(3, 51253));
        MapNodeScript b2 = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.4f) + delta * UnityEngine.Random.Range(0.6f, 0.8f), Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 1)), 3, MapNodeScript.MapNodeType.Battle);
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(4, 51253));
        MapNodeScript c = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.6f) + delta * UnityEngine.Random.Range(0.6f, 0.8f), Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 2)), 3, MapNodeScript.MapNodeType.Battle);
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(5, 51253));
        MapNodeScript d = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.8f) + delta * UnityEngine.Random.Range(-0.2f, 0.2f), baseDifficulty, (int)Mathf.Clamp(4 + (baseDifficulty / 10f), 4, 7), MapNodeScript.MapNodeType.Shop);

        mns.children.Add(a);
        a.children.Add(b1);
        a.children.Add(b2);
        b1.children.Add(d);
        b2.children.Add(c);
        c.children.Add(d);

        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(6, 51253));
        mns = MakeMapNodeScript(endPos, Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 3)), 5, MapNodeScript.MapNodeType.BossBattle);
        d.children.Add(mns);

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
        //UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(5 ^ BitConverter.ToInt32(BitConverter.GetBytes(difficulty), 0)));
        //mns.GenerateArmy();
        return mns;
    }

    public void InitNodes()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            //note: avoiding the float conversion because I am suspicious of possible float rounding differences across platforms
            mapNodes[i].nodeSeed = MainManager.ConvertSeedNodeOffset(46579 ^ (int)(mapNodes[i].truePieceValueTotal * 10000));
            UnityEngine.Random.InitState(mapNodes[i].nodeSeed);
            mapNodes[i].pieceClass = pieceClass;

            switch (mapNodes[i].nodeType)
            {
                case MapNodeScript.MapNodeType.Start:
                    current = mapNodes[i];
                    break;
                case MapNodeScript.MapNodeType.Battle:
                case MapNodeScript.MapNodeType.BossBattle:
                case MapNodeScript.MapNodeType.FinalBossBattle:
                    mapNodes[i].GenerateArmy();
                    break;
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

        switch (current.nodeType)
        {
            case MapNodeScript.MapNodeType.FinalBossBattle:
            case MapNodeScript.MapNodeType.BossBattle:
            case MapNodeScript.MapNodeType.Battle:
                MainManager.Instance.playerData.battlesComplete++;
                MainManager.Instance.playerData.realmBattlesComplete++;
                break;
        }
        MainManager.Instance.playerData.nodesComplete++;

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
