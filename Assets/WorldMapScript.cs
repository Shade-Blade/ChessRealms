using System.Collections;
using System.Collections.Generic;
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
        MapNodeScript mns = MakeMapNodeScript(startPos, pc, Mathf.Min(500, baseDifficulty), 2, MapNodeScript.MapNodeType.Start);
        MapNodeScript pastMNS = mns;

        //todo: find better way to do stuff to have branching paths

        //randomly decide
        int oldTier = GlobalPieceManager.GetPieceClassEntry(pc).tier;
        localPool = new List<PieceClass>();
        for (int i = 0; i < pieceClasses.Count; i++)
        {
            int newTier = GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).tier;
            if (newTier < oldTier + 1 || newTier > oldTier + 1)
            {
                if (oldTier + 1 <= 7)
                {
                    continue;
                }
            }

            localPool.Add(pieceClasses[i]);
        }
        for (int i = 0; i < GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms.Length; i++)
        {
            localPool.Add(GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms[i]);
        }
        pc = RandomTable<Piece.PieceClass>.ChooseRandom(localPool);
        if (localPool.Count == 0)
        {
            pc = RandomTable<Piece.PieceClass>.ChooseRandom(pieceClasses);
        }
        pieceClasses.Remove(pc);
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.2f) + delta * Random.Range(-0.2f, 0.2f), pc, baseDifficulty, (int)Mathf.Clamp(4 + (baseDifficulty / 10f), 4, 7), MapNodeScript.MapNodeType.WorldNode);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        oldTier = GlobalPieceManager.GetPieceClassEntry(pc).tier;
        localPool = new List<PieceClass>();
        for (int i = 0; i < pieceClasses.Count; i++)
        {
            int newTier = GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).tier;
            if (newTier < oldTier + 1 || newTier > oldTier + 1)
            {
                if (oldTier + 1 <= 7)
                {
                    continue;
                }
            }

            localPool.Add(pieceClasses[i]);
        }
        for (int i = 0; i < GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms.Length; i++)
        {
            localPool.Add(GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms[i]);
        }
        pc = RandomTable<Piece.PieceClass>.ChooseRandom(localPool);
        if (localPool.Count == 0)
        {
            pc = RandomTable<Piece.PieceClass>.ChooseRandom(pieceClasses);
        }
        pieceClasses.Remove(pc);
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.4f) + delta * Random.Range(-0.4f, 0.4f), pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 2.5f)), 3, MapNodeScript.MapNodeType.WorldNode);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        oldTier = GlobalPieceManager.GetPieceClassEntry(pc).tier;
        localPool = new List<PieceClass>();
        for (int i = 0; i < pieceClasses.Count; i++)
        {
            int newTier = GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).tier;
            if (newTier < oldTier + 1 || newTier > oldTier + 2)
            {
                if (oldTier + 1 <= 7)
                {
                    continue;
                }
            }

            localPool.Add(pieceClasses[i]);
        }
        for (int i = 0; i < GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms.Length; i++)
        {
            localPool.Add(GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms[i]);
        }
        pc = RandomTable<Piece.PieceClass>.ChooseRandom(localPool);
        if (localPool.Count == 0)
        {
            pc = RandomTable<Piece.PieceClass>.ChooseRandom(pieceClasses);
        }
        pieceClasses.Remove(pc);
        mns = MakeMapNodeScript(Vector3.Lerp(startPos, endPos, 0.6f) + delta * Random.Range(-0.4f, 0.4f), pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 5f)), 4, MapNodeScript.MapNodeType.WorldNode);
        pastMNS.children.Add(mns);
        pastMNS = mns;

        oldTier = GlobalPieceManager.GetPieceClassEntry(pc).tier;
        localPool = new List<PieceClass>();
        for (int i = 0; i < pieceClasses.Count; i++)
        {
            int newTier = GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).tier;
            if (newTier < oldTier + 1 || newTier > oldTier + 3)
            {
                if (oldTier + 1 <= 7)
                {
                    continue;
                }
            }

            localPool.Add(pieceClasses[i]);
        }
        for (int i = 0; i < GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms.Length; i++)
        {
            localPool.Add(GlobalPieceManager.GetPieceClassEntry(pieceClasses[i]).nearbyRealms[i]);
        }
        pc = RandomTable<Piece.PieceClass>.ChooseRandom(localPool);
        if (localPool.Count == 0)
        {
            pc = RandomTable<Piece.PieceClass>.ChooseRandom(pieceClasses);
        }
        pieceClasses.Remove(pc);
        mns = MakeMapNodeScript(endPos, pc, Mathf.Min(500, baseDifficulty * Mathf.Pow(1.2f, 7.5f)), 5, MapNodeScript.MapNodeType.WorldNode);
        pastMNS.children.Add(mns);

        lastNode = mns;
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
        mns.GenerateArmy();
        return mns;
    }

    public void InitNodes()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            //mapNodes[i].pieceClass = pieceClass;

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
        os.ReturnFromWorldMap();
    }
}
