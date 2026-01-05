using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static Piece;

public class WorldMapScript : MonoBehaviour
{
    public List<MapNodeScript> mapNodes;
    public GameObject mapNodeTemplate;
    public OverworldScript os; 
    
    public bool init = false;
    public float baseDifficulty;
    public Piece.PieceClass lastRealm = Piece.PieceClass.None;

    public MapNodeScript current;
    public MapNodeScript lastNode;

    public void Init()
    {
        init = true;
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
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(1253));
        //realmText.text = GlobalPieceManager.GetPieceClassEntry(pieceClass).name + " Realm";

        //-3, 3.5
        //3, 7.5

        Vector3 startPos = new Vector3(-3, 3.5f, 0);
        Vector3 endPos = new Vector3(3, 7.5f, 0);
        Vector3 delta = new Vector3(1, -1, 0);

        List<Piece.PieceClass> pieceClasses = new List<PieceClass>();
        List<Piece.PieceClass> localPool = new List<PieceClass>();
        for (int i = 0; i < 71; i++)
        {
            /*
                    bool realmInvalid = true;
        while (realmInvalid)
        {
            realmInvalid = true;
            worldMap.lastRealm = (Piece.PieceClass)(Random.Range(0, 71));

            if (GlobalPieceManager.GetPieceClassEntry(worldMap.lastRealm).index != -1)
            {
                realmInvalid = false;
            }

            if (worldMap.lastRealm == oldClass)
            {
                realmInvalid = true;
            }

            //tier check
            int oldTier = GlobalPieceManager.GetPieceClassEntry(oldClass).tier;
            int newTier = GlobalPieceManager.GetPieceClassEntry(worldMap.lastRealm).tier;

            if (oldTier < newTier - 1 || oldTier > newTier + 1)
            {
                realmInvalid = true;
            }
        }
             */

            Piece.PieceClass ipc = (Piece.PieceClass)i;
            if (GlobalPieceManager.GetPieceClassEntry(ipc).index == -1)
            {
                continue;
            }

            if (ipc == lastRealm)
            {
                continue;
            }
            pieceClasses.Add(ipc);
        }

        Piece.PieceClass pc = lastRealm;
        MapNodeScript mns = MakeMapNodeScript(startPos + Vector3.up * 0.1f, pc, Mathf.Min(500, baseDifficulty), 2, MapNodeScript.MapNodeType.Start);
        //todo: find better way to do stuff to have branching paths and generated layouts

        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(1, 71253));
        MapNodeScript a;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            a = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.25f) + delta * Random.Range(-0.3f, 0.3f), 0, 3, pc, baseDifficulty, MapNodeScript.MapNodeType.WorldNode);
        } else
        {
            a = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.25f) + delta * Random.Range(-0.3f, 0.3f), 0, 0, pc, baseDifficulty, MapNodeScript.MapNodeType.WorldNode);
        }
        pc = a.pieceClass;
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(2, 71253));
        MapNodeScript b1;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            b1 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(-1.2f, -1f), 0, 4, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 2f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            b1 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(-1.2f, -1f), 1, 1, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 2f)), MapNodeScript.MapNodeType.WorldNode);
        }
        pc = a.pieceClass;
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(3, 71253));
        MapNodeScript b2;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            b2 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.6f) + delta * Random.Range(-0.2f, 0.2f), 3, 5, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 3.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            b2 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.6f) + delta * Random.Range(-0.2f, 0.2f), 2, 2, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 3.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        pc = a.pieceClass;
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(4, 71253));
        MapNodeScript b3;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            b3 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(1f, 1.2f), 1, 4, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 2.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            b3 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(1f, 1.2f), 1, 2, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 2.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        pc = b1.pieceClass;
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(5, 71253));
        MapNodeScript c1;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            c1 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.8f) + delta * Random.Range(-0.8f, -0.7f), 4, 6, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 4.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            c1 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.8f) + delta * Random.Range(-0.8f, -0.7f), 3, 4, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 4.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        pc = b3.pieceClass;
        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(6, 71253));
        MapNodeScript c2;
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            c2 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.8f) + delta * Random.Range(0.7f, 0.8f), 5, 7, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 5.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            c2 = MakeWorldNode(pieceClasses, Vector3.Lerp(startPos, endPos, 0.8f) + delta * Random.Range(0.7f, 0.8f), 3, 4, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 5.5f)), MapNodeScript.MapNodeType.WorldNode);
        }

        mns.children.Add(a);

        a.children.Add(b1);
        a.children.Add(b2);
        a.children.Add(b3);

        b1.children.Add(c1);
        b2.children.Add(c1);
        b2.children.Add(c2);
        b3.children.Add(c2);

        UnityEngine.Random.InitState(MainManager.ConvertSeedNodeOffset(7, 71253));
        if (MainManager.Instance.playerData.realmsComplete > 2)
        {
            mns = MakeWorldNode(pieceClasses, endPos, 7, 8, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 7.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        else
        {
            mns = MakeWorldNode(pieceClasses, endPos, 4, 8, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(MainManager.Instance.scalingFactor, 7.5f)), MapNodeScript.MapNodeType.WorldNode);
        }
        mns.em = Board.EnemyModifier.Zenith;
        c1.children.Add(mns);
        c2.children.Add(mns);
        lastNode = mns;
    }

    MapNodeScript MakeWorldNode(List<PieceClass> pieceClasses, Vector3 position, int tierLow, int tierHigh, PieceClass pc, float difficulty, MapNodeScript.MapNodeType nodeType)
    {
        int oldTier = GlobalPieceManager.GetPieceClassEntry(pc).tier;
        List<PieceClass> localPool = new List<PieceClass>();
        for (int i = 0; i < pieceClasses.Count; i++)
        {
            int newTier = GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).tier;
            if (newTier < tierLow || newTier > tierHigh)
            {
                continue;
            }

            localPool.Add(pieceClasses[i]);
        }
        PieceClass[] nearby = GlobalPieceManager.GetPieceClassEntry(pc).nearbyRealms;
        for (int i = 0; i < nearby.Length; i++)
        {
            if (!pieceClasses.Contains(nearby[i]))
            {
                continue;
            }
            localPool.Add(nearby[i]);
        }
        pc = RandomTable<Piece.PieceClass>.ChooseRandom(localPool);
        if (localPool.Count == 0)
        {
            pc = RandomTable<Piece.PieceClass>.ChooseRandom(pieceClasses);
        }
        pieceClasses.Remove(pc);

        difficulty = (int)(10 * difficulty) / 10f;

        return MakeMapNodeScript(position, pc, difficulty, 0, MapNodeScript.MapNodeType.WorldNode);
    }

    MapNodeScript MakeMapNodeScript(Vector3 position, Piece.PieceClass pc, float difficulty, int types, MapNodeScript.MapNodeType nodeType)
    {
        GameObject go = Instantiate(mapNodeTemplate, transform);
        go.transform.localPosition = position;
        MapNodeScript mns = go.GetComponent<MapNodeScript>();
        mns.os = os;
        mns.pieceClass = pc;
        //mns.pieceClass = pieceClass;
        mns.truePieceValueTotal = difficulty;
        mns.pieceTypes = types;
        mns.nodeType = nodeType;
        return mns;
    }

    public void InitNodes()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            //note: avoiding the float conversion because I am suspicious of possible float rounding differences across platforms
            mapNodes[i].nodeSeed = MainManager.ConvertSeedNodeOffset(776579 ^ (int)(mapNodes[i].truePieceValueTotal * 10000));
            UnityEngine.Random.InitState(mapNodes[i].nodeSeed);
            //mapNodes[i].pieceClass = pieceClass;

            if (mapNodes[i].nodeType == MapNodeScript.MapNodeType.Start)
            {
                current = mapNodes[i];
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
        os.ReturnFromWorldMap();
    }
}
