using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealmMapScript : MonoBehaviour
{
    public List<MapNodeScript> mapNodes;

    public MapNodeScript current;
    public MapNodeScript lastNode;

    public Piece.PieceClass pieceClass;

    public OverworldScript os;

    public TMPro.TMP_Text realmText;

    public void Start()
    {
        realmText.text = GlobalPieceManager.GetPieceClassEntry(pieceClass).name + " Realm";
        mapNodes = new List<MapNodeScript>(GetComponentsInChildren<MapNodeScript>());

        //todo: follow path using playerdata

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
    }

    public void CompleteNode()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            mapNodes[i].active = false;
            mapNodes[i].UpdateState();
            mapNodes[i].UpdateColor();
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
    }
}
