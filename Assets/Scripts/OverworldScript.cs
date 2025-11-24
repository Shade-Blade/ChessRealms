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
                mapNodeSubobject = ShopScript.CreateShop(FindObjectOfType<SetupBoardScript>(), ms.pieceClass, 5, 3, 3).gameObject;
                break;
            case MapNodeScript.MapNodeType.FreePiece:
                break;
            case MapNodeScript.MapNodeType.Event:
                break;
        }
    }

    public void ReturnFromNode()
    {
        //use ms to do stuff
        if (mapNodeSubobject != null)
        {
            Destroy(mapNodeSubobject);
        }
        //currentPosition = null;
        realmMap.gameObject.SetActive(true);
        realmMap.CompleteNode();
        setupBoard.gameObject.SetActive(true);
        bus.SetBoard(setupBoard);

        MainManager.Instance.currentSelected = null;

        bus.turnText.text = "";
        bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.thinkingText.text = "";
    }
}