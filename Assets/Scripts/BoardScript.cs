using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Piece;

public class BoardScript : MonoBehaviour
{
    public const float BOARD_SIZE = 8;
    public const float SQUARE_SIZE = 1;

    public GameObject squareTemplate;
    public GameObject pieceTemplate;

    public GameObject squareHolder;
    public GameObject pieceHolder;

    public List<SquareScript> squares;
    public List<PieceScript> pieces;

    public int hoverX;
    public int hoverY;

    public Board board;

    public List<uint> moveList;

    public List<uint> enemyMoveList;

    public PieceScript selectedPiece;

    public ChessAI chessAI;
    public bool whiteIsAI;
    public bool blackIsAI = true;

    public bool controlZoneHighlight;

    public bool gameOver;
    public bool drawError;
    public Piece.PieceAlignment winnerPA;

    public TMPro.TextMeshPro turnText;
    public TMPro.TextMeshPro scoreText;

    public float moveThinkTime = 1f;
    public float moveDelayValue = 0.05f;
    public float moveDelay = 0;

    public List<Board> historyList;
    public int historyIndex;

    public bool awaitingMove = false;

    //offset by SQUARE_SIZE
    //so this is the center of each square
    public Vector3 GetSpritePositionFromCoordinates(int x, int y, float z)
    {
        return new Vector3(-(BOARD_SIZE / 2 - SQUARE_SIZE / 2) + x * SQUARE_SIZE, -(BOARD_SIZE / 2 - SQUARE_SIZE / 2) + y * SQUARE_SIZE, z);
    }
    //Opposite of the above
    //but it can take any arbitrary position and force it into the grid
    public (int, int) GetCoordinatesFromPosition(Vector3 position)
    {
        float x = position.x;
        float y = position.y;

        x += BOARD_SIZE / 2;
        y += BOARD_SIZE / 2;
        x *= SQUARE_SIZE;
        y *= SQUARE_SIZE;

        //reverse the rounding?
        if (x < 0)
        {
            x -= 1;
        }
        if (y < 0)
        {
            y -= 1;
        }

        return ((int)x, (int)y);
    }

    public void Start()
    {
        MakeBoard();
        InitializeAI();
    }

    public void ResetBoard(Board.BoardPreset bp)
    {
        whiteIsAI = false;  //so you can make your own move
        ResetSelected();
        board = new Board();
        board.Setup(bp);
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].czhWhite = false;
            squares[i].czhBlack = false;
            squares[i].Setup(i & 7, i >> 3, board.GetSquareAtCoordinate(i & 7, i >> 3));
        }
        RegenerateMoveList();
        FixBoardBasedOnPosition();
        historyList = new List<Board>();
        historyList.Add(new Board(board));
        historyIndex = 0;
        awaitingMove = false;
        gameOver = false;
        drawError = false;
        chessAI.InitAI();
    }
    public void ResetBoard(Piece.PieceType[] army, Board.EnemyModifier em)
    {
        whiteIsAI = false;  //so you can make your own move
        ResetSelected();
        board = new Board();
        board.Setup(army, em);
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].czhWhite = false;
            squares[i].czhBlack = false;
            squares[i].Setup(i & 7, i >> 3, board.GetSquareAtCoordinate(i & 7, i >> 3));
        }
        RegenerateMoveList();
        FixBoardBasedOnPosition();
        historyList = new List<Board>();
        historyList.Add(new Board(board));
        historyIndex = 0;
        awaitingMove = false;
        gameOver = false;
        drawError = false;
        chessAI.InitAI();
    }

    public void MakeBoard()
    {
        squares = new List<SquareScript>();
        historyList = new List<Board>();
        historyIndex = -1;

        board = new Board();
        board.Setup();
        historyList.Add(new Board(board));
        historyIndex = 0;

        for (int i = 0; i < 64; i++)
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

        RegenerateMoveList();

        //make pieces
        pieces = new List<PieceScript>();
        for (int i = 0; i < 64; i++)
        {
            pieces.Add(null);
        }
        FixBoardBasedOnPosition();
    }

    public void InitializeAI()
    {
        chessAI = new ChessAI();
        chessAI.InitAI();
    }

    public void SelectPiece(PieceScript piece)
    {
        ResetSelected(false);

        ulong bitboard = 0;
        ulong legalBitboard = 0;
        ulong specialBitboard = 0;
        ulong enemybitboard = 0;
        ulong enemylegalBitboard = 0;
        ulong enemySpecialBitboard = 0;

        //bool isEnemy = Piece.GetPieceAlignment(piece.piece) != board.AlignmentToMove();

        //Get a list of psuedolegal moves of the piece
        //uint[] possibleMoves = new uint[64];
        //int moveStartIndex = 0;

        //GenerateMoves(uint[] moves, int moveStartIndex, ref Board b, uint piece, int x, int y, MoveBitTable mbt)
        //moveStartIndex = MoveGeneratorInfoEntry.GenerateMoves(possibleMoves, moveStartIndex, ref board, piece.piece, piece.x, piece.y, null);


        for (int i = 0; i < moveList.Count; i++)
        {
            if (Move.GetFromX(moveList[i]) != piece.x || Move.GetFromY(moveList[i]) != piece.y)
            {
                continue;
            }

            int index = Move.GetToX(moveList[i]) + 8 * Move.GetToY(moveList[i]);

            bitboard |= 1uL << index;

            if (Board.IsMoveLegal(ref board, moveList[i], board.blackToMove))
            {
                legalBitboard |= 1uL << index;
            }

            if (Move.SpecialMoveHighlighted(Move.GetSpecialType(moveList[i])))
            {
                specialBitboard |= 1uL << index;
            }
        }

        for (int i = 0; i < enemyMoveList.Count; i++)
        {
            if (Move.GetFromX(enemyMoveList[i]) != piece.x || Move.GetFromY(enemyMoveList[i]) != piece.y)
            {
                continue;
            }

            int index = Move.GetToX(enemyMoveList[i]) + 8 * Move.GetToY(enemyMoveList[i]);

            enemybitboard |= 1uL << index;

            if (Board.IsMoveLegal(ref board, enemyMoveList[i], board.blackToMove))
            {
                enemylegalBitboard |= 1uL << index;
            }

            if (Move.SpecialMoveHighlighted(Move.GetSpecialType(enemyMoveList[i])))
            {
                enemySpecialBitboard |= 1uL << index;
            }
        }

        //Then check legality of all of them to create the legal move bitboard

        //I'm going to show you both so you have a better clue of how pieces move
        //You can also click on enemy pieces to see how they move too

        for (int i = 0; i < 64; i++)
        {
            //squares[i].showEnemyMove = isEnemy;

            ulong bitIndex = 1uL << i;
            if ((bitIndex & legalBitboard) != 0)
            {
                squares[i].showEnemyMove = false;
                squares[i].HighlightLegal((bitIndex & specialBitboard) != 0);
                continue;
            }
            if ((bitIndex & (bitboard)) != 0)
            {
                squares[i].showEnemyMove = false;
                squares[i].HighlightIllegal((bitIndex & specialBitboard) != 0);
                continue;
            }

            if ((bitIndex & enemylegalBitboard) != 0)
            {
                squares[i].showEnemyMove = true;
                squares[i].HighlightLegal((bitIndex & enemySpecialBitboard) != 0);
                continue;
            }
            if ((bitIndex & (enemybitboard)) != 0)
            {
                squares[i].showEnemyMove = true;
                squares[i].HighlightIllegal((bitIndex & enemySpecialBitboard) != 0);
                continue;
            }
        }

        selectedPiece = piece;
    }
    public void ResetSelected(bool forceDeselect = true)
    {
        for (int i = 0; i < 64; i++)
        {
            squares[i].ResetColor();
        }        

        if (board.GetLastMove() != 0)
        {
            int fromX = Move.GetFromX(board.GetLastMove());
            int fromY = Move.GetFromY(board.GetLastMove());
            int toX = Move.GetToX(board.GetLastMove());
            int toY = Move.GetToY(board.GetLastMove());

            squares[fromX + fromY * 8].SetLastMovedSquare();
            squares[toX + toY * 8].SetLastMovedSquare();
        }

        if (selectedPiece != null && forceDeselect)
        {
            selectedPiece.ForceDeselect();
        }
        selectedPiece = null;
    }
    public void SetControlZoneHighlight()
    {
        controlZoneHighlight = true;
    }
    public void UnsetControlZoneHighlight()
    {
        controlZoneHighlight = false;
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].czhWhite = false;
            squares[i].czhBlack = false;
            squares[i].ResetColor();
        }        
    }

    public void TryMove(PieceScript ps, Piece.PieceAlignment pa, int x, int y, int newX, int newY)
    {
        //Ai makes moves
        if (whiteIsAI && blackIsAI)
        {
            FixBoardBasedOnPosition();
            ResetSelected();
            return;
        }

        //If you are not allowed to move then no
        
        if (pa != board.AlignmentToMove())
        {
            return;
        }

        if (x < 0 || x > 7 || newX < 0 || newX > 7)
        {
            return;
        }

        if (y < 0 || y > 7 || newY < 0 || newY > 7)
        {
            return;
        }

        if (x == newX && y == newY)
        {
            return;
        }

        uint move = FindMoveInMoveList(x, y, newX, newY);

        if (move == 0)
        {
            ResetSelected();
            FixBoardBasedOnPosition();
            return;
            //debug setup       
            //Lets you make any move (but the legality check can still fail)
            //move = Move.PackMove((byte)x, (byte)y, (byte)newX, (byte)newY, Move.DeltaToDir(newX - x, newY - y), Move.SpecialType.Normal);
        }

        if (Board.IsMoveLegal(ref board, move, board.blackToMove))
        {
            /*
            if (!whiteIsAI)
            {
                Debug.Log("Apply " + Move.ConvertToString(move));
            }
            */
            Debug.Log("Apply " + Move.ConvertToString(move));

            board.ApplyMove(move);
            awaitingMove = false;
            chessAI.moveFound = false;
            while (historyList.Count > historyIndex + 1)
            {
                historyList.RemoveAt(historyIndex + 1);
            }
            historyList.Add(new Board(board));
            historyIndex++;
            chessAI.history.Add(chessAI.HashFromScratch(ref board));
            ResetSelected();
            FixBoardBasedOnPosition();

            RegenerateMoveList();

            if (board.GetVictoryCondition() != PieceAlignment.Null)
            {
                if (board.GetVictoryCondition() == PieceAlignment.White)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White wins with special condition");
                    gameOver = true;
                }
                if (board.GetVictoryCondition() == PieceAlignment.Black)
                {
                    winnerPA = PieceAlignment.Black;
                    Debug.Log("Black wins with special condition");
                    gameOver = true;
                }
                return;
            }

            if (!blackIsAI)
            {
                bool checkAI = Board.PositionIsCheck(ref board);
                bool stalemateAI = Board.PositionIsStalemate(ref board);

                if (checkAI && stalemateAI)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White win");
                    gameOver = true;
                }
                else if (stalemateAI)
                {
                    winnerPA = PieceAlignment.Null;
                    Debug.Log("Draw (Black stalemated)");
                    gameOver = true;
                }
            }
        } else
        {
            Debug.Log("Move is illegal because of " + Move.ConvertToString(Board.MoveIllegalByCheckFindRefutation(ref board, move)));

            FixBoardBasedOnPosition();
        }
    }

    public void ApplyAIMove()
    {
        //Get a move from the AI
        //Async method

        uint aiMove;

        //aiMove = chessAI.GetBestMove(ref board);
        aiMove = chessAI.bestMove;

        if (aiMove == 0)
        {
            bool checkAI = Board.PositionIsCheck(ref board);
            bool stalemateAI = Board.PositionIsStalemate(ref board);

            if (checkAI && stalemateAI)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("White win");
                gameOver = true;
            }
            else if (stalemateAI)
            {
                winnerPA = PieceAlignment.Null;
                Debug.Log("Draw (Black stalemated)");
                gameOver = true;
            }
            else
            {
                winnerPA = PieceAlignment.Null;
                Debug.LogError("AI failed to move for some reason");
                gameOver = true;
                drawError = true;
            }
            return;
        }

        board.ApplyMove(aiMove);
        awaitingMove = false;
        chessAI.moveFound = false;
        while (historyList.Count > historyIndex + 1)
        {
            historyList.RemoveAt(historyIndex + 1);
        }
        historyList.Add(new Board(board));
        historyIndex++;
        chessAI.history.Add(chessAI.HashFromScratch(ref board));
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();

        bool check = Board.PositionIsCheck(ref board);
        bool stalemate = Board.PositionIsStalemate(ref board);

        if (board.GetVictoryCondition() != PieceAlignment.Null)
        {
            if (board.GetVictoryCondition() == PieceAlignment.White)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("White wins with special condition");
                gameOver = true;
            }
            if (board.GetVictoryCondition() == PieceAlignment.Black)
            {
                winnerPA = PieceAlignment.Black;
                Debug.Log("Black wins with special condition");
                gameOver = true;
            }
            return;
        }

        if (check && stalemate)
        {
            winnerPA = PieceAlignment.Black;
            Debug.Log("Black win");
            gameOver = true;
        }
        else if (stalemate)
        {
            winnerPA = PieceAlignment.Null;
            Debug.Log("Draw (White stalemated)");
            gameOver = true;
        }
    }

    public void Undo()
    {
        if (historyIndex == 0)
        {
            return;
        }

        whiteIsAI = false;
        blackIsAI = false;
        gameOver = false;
        drawError = false;
        historyIndex--;
        board.CopyOverwrite(historyList[historyIndex]);
        //Destroy the future history
        chessAI.history.Remove(chessAI.HashFromScratch(historyList[historyIndex + 1]));
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();
    }
    public void Redo()
    {
        if (historyIndex >= historyList.Count - 1)
        {
            return;
        }

        whiteIsAI = false;
        blackIsAI = false;
        gameOver = false;
        drawError = false;
        historyIndex++;
        board.CopyOverwrite(historyList[historyIndex]);
        chessAI.history.Add(chessAI.HashFromScratch(historyList[historyIndex]));
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();
    }

    public void FixBoardBasedOnPosition()
    {
        //fix the pieces to match the board state
        for (int i = 0; i < pieces.Count; i++)
        {
            //also fix squares
            squares[i].sq = board.globalData.squares[i];
            squares[i].ResetSquareColor();

            bool needRecreate = false;

            int checkX = i % 8;
            int checkY = i / 8;

            //Create a new piece
            if (board.pieces[i] != 0 && pieces[i] == null)
            {
                needRecreate = true;
            }

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

            if (needRecreate)
            {
                if (pieces[i] != null)
                {
                    Destroy(pieces[i].gameObject);
                    pieces[i] = null;
                }

                GameObject go = Instantiate(pieceTemplate, pieceHolder.transform);
                go.name = "Piece " + i%8 + " " + i/8;
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
    }

    public void UpdateControlHighlight()
    {
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].czhHighlight = controlZoneHighlight;
            squares[i].ResetDotColor();
        }
    }

    public void RegenerateMoveList()
    {
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].czhWhite = false;
            squares[i].czhBlack = false;
            squares[i].czhHighlight = false;
        }

        moveList = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(moveList, ref board, board.blackToMove ? PieceAlignment.Black : PieceAlignment.White);
        //MoveGeneratorInfoEntry.GenerateMovesForPlayer(moveList, ref board, PieceAlignment.White);

        board.globalData.mbtactiveInverse.MakeInverse(board.globalData.mbtactive);
        for (int i = 0; i < squares.Count; i++)
        {
            if (board.globalData.mbtactiveInverse.Get(i % 8, i / 8) != 0)
            {
                if (board.blackToMove)
                {
                    squares[i].czhBlack = true;
                }
                else
                {
                    squares[i].czhWhite = true;
                }
            }

            if (controlZoneHighlight)
            {
                squares[i].czhHighlight = true;
            }
        }

        enemyMoveList = new List<uint>();
        MoveGeneratorInfoEntry.GenerateMovesForPlayer(enemyMoveList, ref board, !board.blackToMove ? PieceAlignment.Black : PieceAlignment.White);
        //MoveGeneratorInfoEntry.GenerateMovesForPlayer(enemyMoveList, ref board, PieceAlignment.Black);

        board.globalData.mbtactiveInverse.MakeInverse(board.globalData.mbtactive);
        for (int i = 0; i < squares.Count; i++)
        {
            if (board.globalData.mbtactiveInverse.Get(i % 8, i / 8) != 0)
            {
                if (board.blackToMove)
                {
                    squares[i].czhWhite = true;
                }
                else
                {
                    squares[i].czhBlack = true;
                }
            }

            if (controlZoneHighlight)
            {
                squares[i].czhHighlight = true;
            }
        }
    }

    public uint FindMoveInMoveList(int x, int y, int newX, int newY)
    {
        int foundIndex = -1;
        for (int i = 0; i < moveList.Count; i++)
        {
            int fx = Move.GetFromX(moveList[i]);
            int fy = Move.GetFromY(moveList[i]);
            int tx = Move.GetToX(moveList[i]);
            int ty = Move.GetToY(moveList[i]);

            if (x == fx && y == fy && newX == tx && newY == ty)
            {
                foundIndex = i;
                return moveList[i];
            }
        }

        return 0;
    }

    public void Update()
    {
        chessAI.searchDuration = moveThinkTime;

        turnText.text = "Turn " + (board.turn + (board.blackToMove ? 0.5f : 0)) + " <size=50%>" + (board.blackToMove ? "Black" : "White") + " to move</size>";
        if (board.bonusPly > 0)
        {
            turnText.text += "<size=50%> " + board.bonusPly + " bonus</size>";
        }

        float kingValue = (GlobalPieceManager.Instance.GetPieceTableEntry(PieceType.King).pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE);
        scoreText.text = "Pieces\n" + (board.whitePerPlayerInfo.pieceCount) + "\n<color=#000000>" + (board.blackPerPlayerInfo.pieceCount) + "</color>";
        scoreText.text += "\n\nValues\n" + (((board.whitePerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - kingValue)/2f) + "\n" + (board.whitePerPlayerInfo.pieceValueSumX2 / GlobalPieceManager.KING_VALUE_BONUS) + " king(s)\n<color=#000000>" + (((board.blackPerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - kingValue) / 2f) + "\n" + (board.blackPerPlayerInfo.pieceValueSumX2 / GlobalPieceManager.KING_VALUE_BONUS) + " king(s)\n</color>";
        if (gameOver)
        {
            if (winnerPA == PieceAlignment.Null)
            {
                turnText.text = "Draw";
            } else
            {
                turnText.text += "\n" + winnerPA + " Wins";
            }
        }

        if (!gameOver && moveDelay <= 0)
        {
            if (whiteIsAI && !board.blackToMove && !awaitingMove)
            {
                awaitingMove = true;
                chessAI.board = board;
                chessAI.moveFound = false;
                chessAI.searchTime = 0;
                StartCoroutine(chessAI.BestMoveCoroutine());
            }

            if (whiteIsAI && !board.blackToMove && chessAI.moveFound)
            {
                uint bestMove = chessAI.bestMove;
                //uint bestMove = chessAI.GetBestMove(ref board);

                int bestX = Move.GetFromX(bestMove);
                int bestY = Move.GetFromY(bestMove);
                int bestToX = Move.GetToX(bestMove);
                int bestToY = Move.GetToY(bestMove);
                //Debug.Log(bestX + " " + bestY + " " + bestToX + " " + bestToY);

                if (bestMove == 0)
                {
                    bool check = Board.PositionIsCheck(ref board);
                    bool stalemate = Board.PositionIsStalemate(ref board);

                    if (board.GetVictoryCondition() != PieceAlignment.Null)
                    {
                        if (board.GetVictoryCondition() == PieceAlignment.White)
                        {
                            winnerPA = PieceAlignment.White;
                            Debug.Log("White wins with special condition");
                            gameOver = true;
                        }
                        if (board.GetVictoryCondition() == PieceAlignment.Black)
                        {
                            winnerPA = PieceAlignment.Black;
                            Debug.Log("Black wins with special condition");
                            gameOver = true;
                        }
                        return;
                    }

                    if (check && stalemate)
                    {
                        winnerPA = PieceAlignment.Black;
                        Debug.Log("Black win");
                        gameOver = true;
                        return;
                    }
                    else if (stalemate)
                    {
                        winnerPA = PieceAlignment.Null;
                        Debug.Log("Draw (White stalemated)");
                        gameOver = true;
                        return;
                    }

                    whiteIsAI = false;
                    Debug.Log("Self play ended");
                    drawError = true;
                    gameOver = true;
                    return;
                }

                whiteIsAI = false;
                TryMove(pieces[bestX + bestY * 8], PieceAlignment.White, bestX, bestY, bestToX, bestToY);
                FixBoardBasedOnPosition();
                moveDelay = moveDelayValue;
                whiteIsAI = true;
                return;
            }

            if (board.blackToMove && blackIsAI && !awaitingMove)
            {
                awaitingMove = true;
                chessAI.board = board;
                chessAI.moveFound = false;
                chessAI.searchTime = 0;
                StartCoroutine(chessAI.BestMoveCoroutine());
            }

            if (board.blackToMove && blackIsAI && chessAI.moveFound)
            {
                ApplyAIMove();
                moveDelay = moveDelayValue;
            }
        }

        if (moveDelay > 0)
        {
            moveDelay -= Time.deltaTime;
        }
    }
}
