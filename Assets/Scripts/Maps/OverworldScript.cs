using System;
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

    public Action awaitInvoke;

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
        if (awaitInvoke != null)
        {
            return;
        }

        Action afterWorld = () =>
        {
            worldMap.current = ms;
            worldMap.gameObject.SetActive(false);

            MainManager.Instance.currentSelected = null;

            UnityEngine.Random.InitState(ms.nodeSeed);
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
                    realmMap.baseDifficulty = ms.truePieceValueTotal;
                    realmMap.pieceClass = ms.pieceClass;
                    realmMap.os = this;
                    realmMap.Init(ms.em == Board.EnemyModifier.Zenith);
                    setupBoard.SetTheme(realmMap.pieceClass);
                    worldMapMode = false;

                    //so it isn't null but does nothing
                    awaitInvoke = () => { };
                    StartCoroutine(RealmMapFadeIn());
                    break;
            }
        };
        Action afterRealm = () =>
        {
            realmMap.current = ms;
            realmMap.gameObject.SetActive(false);

            MainManager.Instance.currentSelected = null;

            UnityEngine.Random.InitState(ms.nodeSeed);
            switch (ms.nodeType)
            {
                case MapNodeScript.MapNodeType.Battle:
                case MapNodeScript.MapNodeType.BossBattle:
                case MapNodeScript.MapNodeType.FinalBossBattle:
                    setupBoard.gameObject.SetActive(false);
                    mapNodeSubobject = BattleBoardScript.CreateBoard(ms.army, MainManager.Instance.playerData.GetPlayerModifier(), ms.em, ms.pieceClass).gameObject;
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
        };

        if (worldMapMode)
        {
            awaitInvoke = afterWorld;
            StartCoroutine(WorldMapFadeOut());
        }
        else
        {
            awaitInvoke = afterRealm;
            StartCoroutine(RealmMapFadeOut());
        }
    }

    public void ReturnFromNode(bool complete = true)
    {
        MainManager.Instance.playerData.ResetConsumablesUsed();
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

        //so it isn't null but does nothing
        awaitInvoke = () => { };
        StartCoroutine(RealmMapFadeIn());

        bus.turnText.text = "";
        //bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.pmps.ResetAll();
        bus.thinkingText.SetText("", true, true);
    }

    public void ReturnFromRealmMap()
    {
        MainManager.Instance.playerData.ResetConsumablesUsed();

        MainManager.Instance.playerData.realmsComplete++;
        MainManager.Instance.playerData.realmBattlesComplete = 0;

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

        //so it isn't null but does nothing
        awaitInvoke = () => { };
        StartCoroutine(WorldMapFadeIn());

        bus.turnText.text = "";
        //bus.scoreText.text = "";
        bus.pieceText.text = "";
        bus.pmps.ResetAll();
        bus.thinkingText.SetText("", true, true);
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

        MainManager.Instance.currentSelected = null;

        //so it isn't null but does nothing
        awaitInvoke = () => { };
        StartCoroutine(WorldMapFadeIn());

        worldMap.os = this;
        worldMap.Init();
        setupBoard.SetTheme(Piece.PieceClass.None);
    }

    public IEnumerator RealmMapFadeIn()
    {
        float animationDuration = 9f / MainManager.Instance.playerData.animationSpeed;
        if (animationDuration > 1)
        {
            animationDuration = 1;
        }
        float time = 0;

        while (time < animationDuration)
        {
            if (realmMap == null)
            {
                yield break;
            }
            realmMap.transform.localPosition = UnityEngine.Vector3.up * 10 * (1 - (MainManager.EasingQuadratic(time / animationDuration, 1)));
            yield return null;

            time += Time.deltaTime;
        }

        realmMap.transform.localPosition = UnityEngine.Vector3.zero;

        if (awaitInvoke != null)
        {
            awaitInvoke.Invoke();
            awaitInvoke = null;
        }
    }

    public IEnumerator RealmMapFadeOut()
    {
        float animationDuration = 9f / MainManager.Instance.playerData.animationSpeed;
        if (animationDuration > 1)
        {
            animationDuration = 1;
        }
        float time = 0;

        while (time < animationDuration)
        {
            if (realmMap == null)
            {
                yield break;
            }
            realmMap.transform.localPosition = UnityEngine.Vector3.up * 10 * (MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;

            time += Time.deltaTime;
        }

        realmMap.transform.localPosition = UnityEngine.Vector3.up * 10;

        if (awaitInvoke != null)
        {
            awaitInvoke.Invoke();
            awaitInvoke = null;
        }
    }

    public IEnumerator WorldMapFadeIn()
    {
        float animationDuration = 9f / MainManager.Instance.playerData.animationSpeed;
        if (animationDuration > 1)
        {
            animationDuration = 1;
        }
        float time = 0;

        while (time < animationDuration)
        {
            if (worldMap == null)
            {
                yield break;
            }
            worldMap.transform.localPosition = UnityEngine.Vector3.up * 10 * (1 - (MainManager.EasingQuadratic(time / animationDuration, 1)));
            yield return null;

            time += Time.deltaTime;
        }

        worldMap.transform.localPosition = UnityEngine.Vector3.zero;

        if (awaitInvoke != null)
        {
            awaitInvoke.Invoke();
            awaitInvoke = null;
        }
    }

    public IEnumerator WorldMapFadeOut()
    {
        float animationDuration = 9f / MainManager.Instance.playerData.animationSpeed;
        if (animationDuration > 1)
        {
            animationDuration = 1;
        }
        float time = 0;

        while (time < animationDuration)
        {
            if (worldMap == null)
            {
                yield break;
            }
            worldMap.transform.localPosition = UnityEngine.Vector3.up * 10 * (MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;

            time += Time.deltaTime;
        }

        worldMap.transform.localPosition = UnityEngine.Vector3.up * 10;

        if (awaitInvoke != null)
        {
            awaitInvoke.Invoke();
            awaitInvoke = null;
        }
    }
}