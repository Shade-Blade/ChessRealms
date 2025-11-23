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

    public BoardScript bs;

    public GameObject realmMap;
    public GameObject setupBoard;

    public BattleUIScript bus;

    public void EnterNode(MapNodeScript ms)
    {
        setupBoard.SetActive(false);
        realmMap.SetActive(false);

        MainManager.Instance.currentSelected = null;

        //currently all map nodes are battles
        bs = BattleBoardScript.CreateBoard(ms.army, MainManager.Instance.playerData.GetPlayerModifier(), ms.em);
    }

    public void ReturnFromNode()
    {
        Destroy(bs.gameObject);
        realmMap.SetActive(true);
        setupBoard.SetActive(true);

        MainManager.Instance.currentSelected = null;

        bus.turnText.text = "";
        bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.thinkingText.text = "";
    }
}