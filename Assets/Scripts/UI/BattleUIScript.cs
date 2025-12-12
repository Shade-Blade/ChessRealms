using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using static Piece;
using UnityEngine.UIElements;

//note: persists out of battle but some of the stuff is disabled
//Acts as an intermediary?
public class BattleUIScript : MonoBehaviour
{
    public BoardScript bs;
    public ConsumableBadgePanelScript cps;

    public GameObject battleOnlyUI;

    public TMPro.TMP_Text thinkingText;
    public TMPro.TMP_Text turnText;
    //public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text pieceText;
    public PieceMovePanelScript pmps;

    public TMPro.TMP_Text moneyText;
    public TMPro.TMP_Text whiteValueText;
    public TMPro.TMP_Text blackValueText;
    public TMPro.TMP_Text deltaValueText;

    public TMPro.TMP_Text undoButtonText;
    public TMPro.TMP_Text retryText;

    public TMPro.TMP_Text realmCountText;
    public TMPro.TMP_Text battleCountText;

    public SpriteRenderer backgroundA;
    public SpriteRenderer backgroundB;

    //debug panel values
    public TMPro.TMP_Text difficultyText;
    public ConsumableScript debugConsumable;
    public SetupPieceScript debugSetupPiece;
    public BadgeScript debugBadge;
    public Scrollbar difficultySlider;
    public TMPro.TMP_InputField valueField;
    public TMPro.TMP_InputField typeField;
    public TMPro.TMP_InputField classField;
    public Board.PlayerModifier pm;
    public Board.EnemyModifier em;

    public void SetBoard(BoardScript bs)
    {
        difficultyText.text = "Difficulty: " + MainManager.Instance.playerData.difficulty;
        difficultySlider.value = (MainManager.Instance.playerData.difficulty - 1) / 4f;
        this.bs = bs;
        cps.SetBoardScript(bs);
        debugConsumable.bs = bs;
        debugSetupPiece.bs = bs;
        debugBadge.bs = bs;
        if (bs is BattleBoardScript bbs)
        {
            bbs.thinkingText = thinkingText;
            bbs.turnText = turnText;
            //bbs.scoreText = scoreText;
            bbs.pmps = pmps;
            battleOnlyUI.SetActive(true);
        }
        if (bs is SetupBoardScript sbs)
        {
            sbs.pmps = pmps;
            battleOnlyUI.SetActive(false);
        }
        whiteValueText.text = "" + (((bs.board.whitePerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f) - 5);
    }

    public void Undo()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.Undo();
        }
    }

    public void Redo()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.Redo();
        }
    }

    public void DoubleUndo()
    {
        if (MainManager.Instance.playerData.undosLeft == 0)
        {
            return;
        }

        if (bs is BattleBoardScript bbs)
        {
            bbs.DoubleUndo();
        }
    }

    public void UndoReset()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.UndoReset();
        }
    }

    public void SetDifficulty(int difficulty)
    {
        MainManager.Instance.playerData.difficulty = difficulty;
        difficultyText.text = "Difficulty: " + MainManager.Instance.playerData.difficulty;
    }

    public void DifficultySlider()
    {        
        SetDifficulty(Mathf.RoundToInt(difficultySlider.value * 4 + 1));
    }

    public void ControlZoneToggle()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.controlZoneHighlight = !bbs.controlZoneHighlight;
            bbs.UpdateControlHighlight();
        }
    }

    public void SetupModeToggle()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.setupMoves = !bbs.setupMoves;
            bbs.UpdateControlHighlight();
        }
    }

    public void ResetNormal()
    {
        if (bs is BattleBoardScript bbs)
        {
            bbs.ResetBoard(Board.BoardPreset.Normal);
        }
    }

    public void ResetRandomA()
    {
        if (bs is BattleBoardScript bbs)
        {
            float tryValue;
            float.TryParse(valueField.text, out tryValue);

            int typeValue;
            int.TryParse(typeField.text, out typeValue);

            Piece.PieceClass classValue;
            Enum.TryParse(classField.text, out classValue);

            Piece.PieceType[] army = ArmyGenerator.GenerateArmy(tryValue, typeValue, 0.5f, 0.5f, classValue, em);

            MainManager.Instance.playerData.GenerateSeed();
            bbs.ResetBoard(MainManager.Instance.playerData.army, army, pm, em);
        }
    }

    public void ResetRandomB()
    {
        if (bs is BattleBoardScript bbs)
        {
            float tryValue;
            float.TryParse(valueField.text, out tryValue);

            int typeValue;
            int.TryParse(typeField.text, out typeValue);

            Piece.PieceClass classValue;
            Enum.TryParse(classField.text, out classValue);

            Piece.PieceType[] army = ArmyGenerator.GenerateArmy(tryValue, typeValue, 0.5f, 0.5f, classValue, em);

            MainManager.Instance.playerData.GenerateSeed();
            bbs.ResetBoard(army, pm, em);
        }
    }

    public void Update()
    {
        backgroundA.color = Color.Lerp(Color.black, Color.Lerp(bs.backgroundA.color, bs.backgroundB.color, 0.2f), 0.9f);
        backgroundB.color = Color.Lerp(Color.black, Color.Lerp(bs.backgroundA.color, bs.backgroundB.color, 0.8f), 0.9f);

        undoButtonText.text = "Undo\n<size=75%><color=#ff8080>(" + MainManager.Instance.playerData.undosLeft + ")</color></size>";

        if (MainManager.Instance.playerData.retriesLeft == 1)
        {
            retryText.text = MainManager.Instance.playerData.retriesLeft + " retry left.";
        }
        else
        {
            if (MainManager.Instance.playerData.retriesLeft == 0)
            {
                retryText.text = "<color=#ff8080>" + MainManager.Instance.playerData.retriesLeft + " retries left.</color>";
            }
            else
            {
                retryText.text = MainManager.Instance.playerData.retriesLeft + " retries left.";
            }
        }

        float timeValue = Time.time * 0.2f;
        backgroundB.transform.localPosition = Vector3.up * 0.25f * (timeValue - Mathf.Ceil(timeValue));

        moneyText.text = "$" + MainManager.Instance.playerData.coins;
        whiteValueText.text = "" + (((bs.board.whitePerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f) - 5);
        blackValueText.text = "" + (((bs.board.blackPerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f) - 5);
        float delta = (((bs.board.whitePerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f) - ((bs.board.blackPerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f));
        if (delta > 0)
        {
            deltaValueText.text = "(+" + delta + ")";
            deltaValueText.color = new Color(1, 1, 1, 1);
        }
        else
        {
            deltaValueText.text = "(" + delta + ")";
            if (delta == 0)
            {
                deltaValueText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            else
            {
                deltaValueText.color = new Color(0.4f, 0.4f, 0.4f, 1);
            }
        }
        realmCountText.text = "Realm " + (MainManager.Instance.playerData.realmsComplete + 1);
        battleCountText.text = "Battle " + (MainManager.Instance.playerData.realmBattlesComplete + 1);
    }
}
