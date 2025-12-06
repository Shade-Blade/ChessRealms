using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupBoardScript : BoardScript
{
    public TMPro.TMP_Text pieceInfoText;
    public override void Start()
    {
        BattleUIScript bus = FindObjectOfType<BattleUIScript>();
        bus.SetBoard(this);

        MakeBoard();
        setupMoves = true;
    }

    public override void MakeBoard()
    {
        squares = new List<SquareScript>();

        board = new Board();

        Piece.PieceType[] oldArmy = new Piece.PieceType[16];

        for (int i = 0; i < 16; i++)
        {
            oldArmy[i] = MainManager.Instance.playerData.army[i];
        }
        board.Setup(oldArmy, new Piece.PieceType[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, Board.PlayerModifier.None, Board.EnemyModifier.NoKing);

        for (int i = 0; i < 16; i++)
        {
            int subX = i % 8;
            int subY = i / 8;

            GameObject go = Instantiate(squareTemplate, squareHolder.transform);
            go.name = "Square " + subX + " " + subY;
            SquareScript sc = go.GetComponent<SquareScript>();
            sc.bs = this;
            squares.Add(sc);

            sc.Setup(subX, subY, board.GetSquareAtCoordinate(subX, subY));
            sc.transform.position = GetSpritePositionFromCoordinates(subX, subY, 0);
        }

        for (int i = 0; i < 48; i++)
        {
            board.globalData.squares[i + 16] = new Square(Square.SquareType.Hole);
        }

        //make pieces
        pieces = new List<PieceScript>();
        for (int i = 0; i < 64; i++)
        {
            pieces.Add(null);
        }
        FixBoardBasedOnPosition();
    }

    public override void SelectConsumable(ConsumableScript cs)
    {
        ResetSelected(false);
        return;
    }

    public override void FixBoardBasedOnPosition()
    {
        //To fix wack bugs I should just refresh the entire piece list anyway?
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i] != null)
            {
                Destroy(pieces[i].gameObject);
            }
            pieces[i] = null;
        }

        //fix the pieces to match the board state
        for (int i = 0; i < 16; i++)
        {
            //also fix squares
            squares[i].sq = board.globalData.squares[i];
            squares[i].ResetSquareColor();

            bool needRecreate = false;

            int checkX = i % 8;
            int checkY = i / 8;

            //Create a new piece
            //if (board.pieces[i] != 0 && pieces[i] == null)
            if (board.pieces[i] != 0)
            {
                needRecreate = true;
            }

            /*
            //Destroy a piece that is on an empty
            if (board.pieces[i] == 0 && pieces[i] != null)
            {
                Destroy(pieces[i].gameObject);
                pieces[i] = null;
            }

            //Wrong coordinates = just move it back to where it should be?
            if (pieces[i] != null && (pieces[i].x != checkX || pieces[i].y != checkY))
            {
                //needRecreate = true;
                pieces[i].SetPosition(checkX, checkY);
            }

            //Wrong type = setup again?
            if (pieces[i] != null && pieces[i].piece != board.pieces[i])
            {
                pieces[i].Setup(board.pieces[i], checkX, checkY);
            }
            */

            if (needRecreate)
            {
                if (pieces[i] != null)
                {
                    Destroy(pieces[i].gameObject);
                    pieces[i] = null;
                }

                GameObject go = Instantiate(pieceTemplate, pieceHolder.transform);
                go.name = "Piece " + i % 8 + " " + i / 8;
                PieceScript ps = go.GetComponent<PieceScript>();
                ps.bs = this;
                pieces[i] = ps;
                ps.Setup(board.pieces[i], checkX, checkY);
            }

            //reset color anyway
            if (pieces[i] != null)
            {
                pieces[i].ResetColor();
            }
        }

        //find weird pieces
        HashSet<GameObject> pieceSet = new HashSet<GameObject>();
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i] != null)
            {
                pieceSet.Add(pieces[i].gameObject);
            }
        }

        PieceScript[] psList = pieceHolder.GetComponentsInChildren<PieceScript>();
        for (int i = 0; i < psList.Length; i++)
        {
            if (!pieceSet.Contains(psList[i].gameObject))
            {
                Destroy(psList[i].gameObject);
            }
        }

        for (int i = 0; i < 16; i++)
        {
            if (board.pieces[i] == 0 || ((GlobalPieceManager.GetPieceTableEntry(board.pieces[i]).piecePropertyB & Piece.PiecePropertyB.Giant) != 0 && (Piece.GetPieceSpecialData(board.pieces[i]) != 0)))
            {
                MainManager.Instance.playerData.army[i] = 0;
                continue;
            }
            MainManager.Instance.playerData.army[i] = Piece.GetPieceType(board.pieces[i]);
        }
    }

    public override bool TrySetupMove(PieceScript ps, int x, int y, int newX, int newY)
    {
        if (x < 0 || x > 7 || newX < 0 || newX > 7)
        {
            return false;
        }

        if (y < 0 || y > 1 || newY < 0 || newY > 7)
        {
            return false;
        }

        if (x == newX && y == newY)
        {
            return false;
        }

        uint move = Move.PackMove((byte)x, (byte)y, (byte)newX, (byte)newY);
        return TrySetupMove(ps, move);
    }

    public override bool TrySetupMove(PieceScript ps, uint move)
    {
        if (Board.IsSetupMoveLegal(ref board, move))
        {
            /*
            if (!whiteIsAI)
            {
                Debug.Log("Apply " + Move.ConvertToString(move));
            }
            */
            //Debug.Log("Apply " + Move.ConvertToString(move));

            board.MakeSetupMove(move);

            ResetSelected();

            FixBoardBasedOnPosition();
            return true;
        }

        ResetSelected();
        FixBoardBasedOnPosition();
        return false;
    }

    public override void Update()
    {
        backgroundA.color = backgroundColorWhite;
        backgroundB.color = backgroundColorBlack;

        pieceInfoText.text = "";
        string propertyText = "";
        string moveText = "";
        if (selectedPiece != null)
        {
            PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(selectedPiece.piece);

            pieceInfoText.text += pte.type + "\n";
            pieceInfoText.text += "Value: " + (pte.pieceValueX2 / 2f) + "\n";
            pieceInfoText.text += "Move: ";
            for (int i = 0; i < pte.moveInfo.Length; i++)
            {
                MoveGeneratorInfoEntry mgie = pte.moveInfo[i];
                if (mgie.atom > MoveGeneratorInfoEntry.MoveGeneratorAtom.SpecialMoveDivider)
                {
                    propertyText += mgie.atom + "\n";
                    continue;
                }
                for (int j = 0; j < 15; j++)
                {
                    if (((int)mgie.modifier & (1 << j)) != 0)
                    {
                        moveText += (MoveGeneratorInfoEntry.MoveGeneratorPreModifier)(1 << j);
                    }
                }

                if (mgie.atom == MoveGeneratorInfoEntry.MoveGeneratorAtom.Leaper)
                {
                    moveText += "(" + mgie.x + ", " + mgie.y + ")";
                }
                else
                {
                    moveText += mgie.atom;
                }

                if (mgie.range > 1)
                {
                    moveText += mgie.range;
                }

                switch (mgie.rangeType)
                {
                    case MoveGeneratorInfoEntry.RangeType.Exact:
                        moveText += "=";
                        break;
                    case MoveGeneratorInfoEntry.RangeType.AntiRange:
                        moveText += "-";
                        break;
                    case MoveGeneratorInfoEntry.RangeType.Minimum:
                        moveText += "+";
                        break;
                }

                moveText += " ";
            }
            pieceInfoText.text += moveText + "\n";
            moveText = "";
            if (pte.enhancedMoveInfo.Length > 0)
            {
                pieceInfoText.text += "Bonus Type: " + pte.enhancedMoveType + "\n";

                pieceInfoText.text += "Bonus Move: ";
                for (int i = 0; i < pte.enhancedMoveInfo.Length; i++)
                {
                    MoveGeneratorInfoEntry mgie = pte.enhancedMoveInfo[i];
                    if (mgie.atom > MoveGeneratorInfoEntry.MoveGeneratorAtom.SpecialMoveDivider)
                    {
                        propertyText += mgie.atom + "\n";
                        continue;
                    }
                    for (int j = 0; j < 15; j++)
                    {
                        if (((int)mgie.modifier & (1 << j)) != 0)
                        {
                            moveText += (MoveGeneratorInfoEntry.MoveGeneratorPreModifier)(1 << j);
                        }
                    }

                    if (mgie.atom == MoveGeneratorInfoEntry.MoveGeneratorAtom.Leaper)
                    {
                        moveText += "(" + mgie.x + ", " + mgie.y + ")";
                    }
                    else
                    {
                        moveText += mgie.atom;
                    }

                    if (mgie.range > 1)
                    {
                        moveText += mgie.range;
                    }

                    switch (mgie.rangeType)
                    {
                        case MoveGeneratorInfoEntry.RangeType.Exact:
                            moveText += "=";
                            break;
                        case MoveGeneratorInfoEntry.RangeType.AntiRange:
                            moveText += "-";
                            break;
                        case MoveGeneratorInfoEntry.RangeType.Minimum:
                            moveText += "+";
                            break;
                    }

                    moveText += " ";
                }
                pieceInfoText.text += moveText + "\n";
            }

            if (pte.promotionType != 0)
            {
                pieceInfoText.text += "Promotes to " + pte.promotionType + "\n";
            }

            if (pte.pieceProperty != 0 || pte.piecePropertyB != 0 || propertyText.Length > 0)
            {
                pieceInfoText.text += "Properties:\n" + propertyText;
                ulong propertiesA = (ulong)pte.pieceProperty;

                while (propertiesA != 0)
                {
                    int index = MainManager.PopBitboardLSB1(propertiesA, out propertiesA);
                    pieceInfoText.text += (Piece.PieceProperty)(1uL << index) + "\n";
                }
                propertiesA = (ulong)pte.piecePropertyB;
                while (propertiesA != 0)
                {
                    int index = MainManager.PopBitboardLSB1(propertiesA, out propertiesA);
                    pieceInfoText.text += (Piece.PiecePropertyB)(1uL << index) + "\n";
                }
            }
        }
    }
}
