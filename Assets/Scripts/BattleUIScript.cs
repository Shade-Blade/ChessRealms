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
    public TMPro.TMP_Text difficultyText;
    public ConsumableBadgePanelScript cps;

    public ConsumableScript debugConsumable;
    public SetupPieceScript debugSetupPiece;
    public BadgeScript debugBadge;

    public Scrollbar difficultySlider;

    public TMPro.TMP_Text thinkingText;
    public TMPro.TMP_Text turnText;
    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text pieceText;
    public TMPro.TMP_Text moneyText;
    public TMPro.TMP_Text valueText;

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
            bbs.scoreText = scoreText;
            bbs.pieceInfoText = pieceText;
        }
        valueText.text = "Value: " + ((bs.board.whitePerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f);
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

            bbs.ResetBoard(army, pm, em);
        }
    }

    public void Update()
    {
        moneyText.text = "$" + MainManager.Instance.playerData.coins;
        valueText.text = "Value: " + ((bs.board.whitePerPlayerInfo.pieceValueSumX2 & (GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f);
    }
}
