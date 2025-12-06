using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
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

    public bool worldMapMode;
    public WorldMapScript worldMap;
    public RealmMapScript realmMap;

    public SetupBoardScript setupBoard;

    public BattleUIScript bus;

    public void Start()
    {
        //redundant?
        bus.SetBoard(setupBoard);
        if (realmMap != null)
        {
            realmMap.os = this;
        }
        if (worldMap != null)
        {
            worldMap.os = this;
        }
        setupBoard.SetTheme(worldMap.lastRealm);
        worldMapMode = true;
    }

    public void EnterNode(MapNodeScript ms)
    {
        if (worldMapMode)
        {
            worldMap.current = ms;
            worldMap.gameObject.SetActive(false);

            MainManager.Instance.currentSelected = null;

            switch (ms.nodeType)
            {
                case MapNodeScript.MapNodeType.WorldNode:
                    //Make a realm map
                    if (realmMap != null)
                    {
                        Destroy(realmMap.gameObject);
                    }

                    GameObject newMap = Instantiate(realmMapTemplate, transform);
                    realmMap = newMap.GetComponent<RealmMapScript>();
                    realmMap.baseDifficulty = ms.pieceValueTotal;
                    realmMap.pieceClass = ms.pieceClass;
                    realmMap.os = this;
                    realmMap.Init();
                    setupBoard.SetTheme(realmMap.pieceClass);
                    worldMapMode = false;
                    break;
            }
        }
        else
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
                    mapNodeSubobject.GetComponent<BattleBoardScript>().SetTheme(realmMap.pieceClass);
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
        //reopen world map
        //use ms to do stuff
        if (mapNodeSubobject != null)
        {
            Destroy(mapNodeSubobject);
        }
        //currentPosition = null;
        Destroy(realmMap.gameObject);
        worldMap.gameObject.SetActive(true);
        worldMap.CompleteNode();
        worldMapMode = true;
        setupBoard.gameObject.SetActive(true);
        bus.SetBoard(setupBoard);

        MainManager.Instance.currentSelected = null;

        bus.turnText.text = "";
        bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.thinkingText.text = "";
    }

    public void ReturnFromWorldMap()
    {
        //Build another world map
        float oldDifficulty = worldMap.baseDifficulty;
        Piece.PieceClass oldClass = worldMap.lastNode.pieceClass;

        Destroy(worldMap.gameObject);

        GameObject newMap = Instantiate(worldMapTemplate, transform);
        worldMap = newMap.GetComponent<WorldMapScript>();

        worldMap.baseDifficulty = oldDifficulty * Mathf.Pow(1.2f, 11f);

        worldMap.lastRealm = oldClass;

        worldMap.os = this;
        worldMap.Init();
        setupBoard.SetTheme(Piece.PieceClass.None);
    }
}