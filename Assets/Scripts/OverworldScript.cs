using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldScript : MonoBehaviour
{
    public enum OverworldMode
    {
        WorldMap,   //map of the whole world (after beating a realm)
        RealmMap,   //map of the area
        Battle, //battles
        SetupEvent, //event that pulls up the setup board (shops and such)
    }

    //public BoardScript bs;
    public GameObject mapNodeSubobject;

    public GameObject worldMapTemplate;
    public GameObject realmMapTemplate;

    public GameObject worldMap;
    public RealmMapScript realmMap;

    public SetupBoardScript setupBoard;

    public BattleUIScript bus;

    public void Start()
    {
        //redundant?
        bus.SetBoard(setupBoard);
        realmMap.os = this;
    }

    public void EnterNode(MapNodeScript ms)
    {
        realmMap.current = ms;
        realmMap.gameObject.SetActive(false);

        MainManager.Instance.currentSelected = null;

        switch (ms.nodeType)
        {
            case MapNodeScript.MapNodeType.Battle:
            case MapNodeScript.MapNodeType.BossBattle:
                setupBoard.gameObject.SetActive(false);
                mapNodeSubobject = BattleBoardScript.CreateBoard(ms.army, MainManager.Instance.playerData.GetPlayerModifier(), ms.em).gameObject;
                break;
            case MapNodeScript.MapNodeType.Shop:
                mapNodeSubobject = ShopScript.CreateShop(FindObjectOfType<SetupBoardScript>(), ms.pieceClass, ms.pieceTypes, 3, 3).gameObject;
                break;
            case MapNodeScript.MapNodeType.FreePiece:
                break;
            case MapNodeScript.MapNodeType.Event:
                break;
        }
    }

    public void ReturnFromNode(bool complete = true)
    {
        //use ms to do stuff
        if (mapNodeSubobject != null)
        {
            Destroy(mapNodeSubobject);
        }
        //currentPosition = null;
        realmMap.gameObject.SetActive(true);
        if (complete)
        {
            realmMap.CompleteNode();
        }
        setupBoard.gameObject.SetActive(true);
        bus.SetBoard(setupBoard);

        MainManager.Instance.currentSelected = null;

        bus.turnText.text = "";
        bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.thinkingText.text = "";
    }

    public void ReturnFromRealmMap()
    {
        //todo: make a world map to choose the next realm from multiple options

        float oldDifficulty = realmMap.baseDifficulty;
        Piece.PieceClass oldClass = realmMap.pieceClass;

        Destroy(realmMap.gameObject);

        GameObject newMap = Instantiate(realmMapTemplate, transform);
        realmMap = newMap.GetComponent<RealmMapScript>();

        realmMap.baseDifficulty = oldDifficulty * Mathf.Pow(1.2f, 2.5f);

        bool realmInvalid = true;
        while (realmInvalid)
        {
            realmInvalid = true;
            realmMap.pieceClass = (Piece.PieceClass)(Random.Range(0, 71));

            if (GlobalPieceManager.GetPieceClassEntry(realmMap.pieceClass).index != -1)
            {
                realmInvalid = false;
            }

            if (realmMap.pieceClass == oldClass)
            {
                realmInvalid = true;
            }

            //tier check
            int oldTier = GlobalPieceManager.GetPieceClassEntry(oldClass).tier;
            int newTier = GlobalPieceManager.GetPieceClassEntry(realmMap.pieceClass).tier;

            if (oldTier < newTier - 1 || oldTier > newTier + 1)
            {
                realmInvalid = true;
            }
        }

        realmMap.os = this;
        realmMap.Init();
    }
}