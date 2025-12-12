using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Piece;

public class BattleBoardScript : BoardScript
{

    public GameObject moveTrailTemplate;
    public GameObject moveParticleTemplate;
    public GameObject battleWinPanelTemplate;
    public GameObject battleLosePanelTemplate;

    public List<uint> moveList;
    public Dictionary<uint, MoveMetadata> moveMetadata;
    public MoveTrailScript lastMoveTrail;

    public List<uint> enemyMoveList;
    public Dictionary<uint, MoveMetadata> enemyMoveMetadata;
    public MoveTrailScript illegalMoveTrail;

    //
    public MoveTrailScript checkMoveTrail;

    public List<MoveTrailScript> extraMoveTrails;

    public Coroutine animCoroutine;
    public bool animating;
    public PieceScript lastMovedPiece;
    public float animationSpeed = 5;            //speeds: 6 (slow), 12 (normal), 36 (fast), 500 (very fast), 10000 (instant)

    public ChessAI chessAI;
    public bool whiteIsAI;
    public bool blackIsAI = true;

    public bool controlZoneHighlight;

    public bool gameOver;
    public bool drawError;
    public Piece.PieceAlignment winnerPA;

    public TMPro.TMP_Text thinkingText;
    public TMPro.TMP_Text turnText;
    //public TMPro.TMP_Text scoreText;
    //public TMPro.TMP_Text pieceInfoText;

    public float moveThinkTime = 1f;
    public float moveDelayValue = 0.05f;
    public float moveDelay = 0;

    public List<Board> historyList;
    public int historyIndex;

    public int difficulty = 2;

    public bool awaitingMove = false;
    public bool errorMove = false;

    public static BattleBoardScript CreateBoard(Piece.PieceType[] army, Board.PlayerModifier pm, Board.EnemyModifier em)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Board/BattleBoardTemplate"));
        BattleBoardScript bbs = go.GetComponent<BattleBoardScript>();

        bbs.MakeBoard();
        bbs.InitializeAI();
        bbs.ResetBoard(MainManager.Instance.playerData.army, army, pm, em);
        return bbs;
    }

    public override void Start()
    {
        BattleUIScript bus = FindObjectOfType<BattleUIScript>();
        bus.SetBoard(this);
        if (chessAI != null)
        {
            return;
        }

        setupMoves = false;
        MakeBoard();
        InitializeAI();
    }

    public void ResetBoard(Board.BoardPreset bp)
    {
        animating = false;
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }
        DestroyLastMovedTrail();
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

        switch (difficulty)
        {
            case -1:
                moveThinkTime = 0.5f;
                break;
            case 0:
                moveThinkTime = 1;
                break;
            case 1:
                moveThinkTime = 2;
                break;
            case 2:
                moveThinkTime = 4;
                break;
            case 3:
                moveThinkTime = 8;
                break;
        }
        chessAI.InitAI(difficulty);
    }
    public void ResetBoard(Piece.PieceType[] army, Board.PlayerModifier pm, Board.EnemyModifier em)
    {
        DestroyLastMovedTrail();
        whiteIsAI = false;  //so you can make your own move
        ResetSelected();
        board = new Board();
        board.Setup(army, pm, em);
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

        switch (difficulty)
        {
            case -1:
            case 0:
            case 1:
            case 2:
                moveThinkTime = 4;
                break;
            case 3:
                moveThinkTime = 8;
                break;
        }
        chessAI.InitAI(difficulty);
    }
    public void ResetBoard(Piece.PieceType[] warmy, Piece.PieceType[] barmy, Board.PlayerModifier pm, Board.EnemyModifier em)
    {
        Debug.Log("Make board");
        DestroyLastMovedTrail();
        whiteIsAI = false;  //so you can make your own move
        ResetSelected();
        board = new Board();
        board.Setup(warmy, barmy, pm, em);
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

        switch (difficulty)
        {
            case -1:
            case 0:
            case 1:
            case 2:
                moveThinkTime = 4;
                break;
            case 3:
                moveThinkTime = 8;
                break;
        }
        chessAI.InitAI(difficulty);
    }

    public void SetDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case -1:
                moveThinkTime = 0.5f;
                break;
            case 0:
                moveThinkTime = 1;
                break;
            case 1:
                moveThinkTime = 2;
                break;
            case 2:
                moveThinkTime = 4;
                break;
            case 3:
                moveThinkTime = 8;
                break;
        }
        chessAI.SetDifficulty(difficulty);
    }

    public override void MakeBoard()
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

        switch (difficulty)
        {
            case -1:
            case 0:
            case 1:
            case 2:
                moveThinkTime = 4;
                break;
            case 3:
                moveThinkTime = 8;
                break;
        }
        chessAI.InitAI(difficulty);
    }

    public override void SelectConsumable(ConsumableScript cs)
    {
        ResetSelected(false);
        DestroyIllegalTrail();

        if (board.blackToMove)
        {
            return;
        }

        bool specialColor = true;
        if (cs.cmt == Move.ConsumableMoveType.Bag)
        {
            specialColor = false;
        }

        ulong bitboard = 0;
        ulong legalBitboard = 0;
        for (int i = 0; i < 64; i++)
        {
            ulong bitIndex = 1uL << i;

            if (Board.IsConsumableMoveValid(ref board, Move.EncodeConsumableMove(cs.cmt, i & 7, i >> 3)))
            {
                bitboard |= bitIndex;

                if (Board.IsMoveLegal(ref board, Move.EncodeConsumableMove(cs.cmt, i & 7, i >> 3), false))
                {
                    legalBitboard |= bitIndex;
                }
            }
        }

        for (int i = 0; i < 64; i++)
        {
            //squares[i].showEnemyMove = isEnemy;

            ulong bitIndex = 1uL << i;
            if ((bitIndex & legalBitboard) != 0)
            {
                squares[i].showEnemyMove = false;
                squares[i].HighlightLegal(specialColor, false);
                continue;
            }
            if ((bitIndex & (bitboard)) != 0)
            {
                squares[i].showEnemyMove = false;
                squares[i].HighlightIllegal(specialColor, false);
                continue;
            }
        }

        selectedPiece = null;
        selectedConsumable = cs;

        if (pmps != null)
        {
            pmps.SetText(selectedConsumable.text.text);
        }
    }
    public override void SelectPiece(PieceScript piece)
    {
        ResetSelected(false);

        DestroyIllegalTrail();

        if (piece is SetupPieceScript)
        {
            //It isn't a piece that has legal moves
            selectedBadge = null;
            selectedConsumable = null;
            selectedPiece = piece;
            return;
        }

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
                squares[i].HighlightLegal((bitIndex & specialBitboard) != 0, piece.isGiant);
                continue;
            }
            if ((bitIndex & (bitboard)) != 0)
            {
                squares[i].showEnemyMove = false;
                squares[i].HighlightIllegal((bitIndex & specialBitboard) != 0, piece.isGiant);
                continue;
            }

            if ((bitIndex & enemylegalBitboard) != 0)
            {
                squares[i].showEnemyMove = true;
                squares[i].HighlightLegal((bitIndex & enemySpecialBitboard) != 0, piece.isGiant);
                continue;
            }
            if ((bitIndex & (enemybitboard)) != 0)
            {
                squares[i].showEnemyMove = true;
                squares[i].HighlightIllegal((bitIndex & enemySpecialBitboard) != 0, piece.isGiant);
                continue;
            }
        }

        //targetted squares
        switch (Piece.GetPieceType(board.pieces[piece.x + (piece.y << 3)]))
        {
            case PieceType.MegaCannon:
                int targetted = Piece.GetPieceSpecialData(board.pieces[piece.x + (piece.y << 3)]);
                if (targetted != 0)
                {
                    int targetIndex = targetted & 63;

                    squares[targetIndex].HighlightTargetted();

                    if ((targetIndex & 7) < 7)
                    {
                        squares[targetIndex + 1].HighlightTargetted();
                    }
                    if ((targetIndex & 7) > 0)
                    {
                        squares[targetIndex - 1].HighlightTargetted();
                    }
                    if ((targetIndex) < 56)
                    {
                        squares[targetIndex + 8].HighlightTargetted();
                    }
                    if ((targetIndex) >= 8)
                    {
                        squares[targetIndex - 8].HighlightTargetted();
                    }
                }
                break;
            case PieceType.SteelGolem:
            case PieceType.SteelPuppet:
            case PieceType.Cannon:
            case PieceType.MetalFox:
                int targettedB = Piece.GetPieceSpecialData(board.pieces[piece.x + (piece.y << 3)]);
                if (targettedB != 0)
                {
                    int targetIndex = targettedB & 63;
                    squares[targetIndex].HighlightTargetted();
                }
                break;
        }

        selectedPiece = piece;
        //PieceTableEntry pte = GlobalPieceManager.GetPieceTableEntry(selectedPiece.piece);

        pmps.SetMove(selectedPiece.piece);
    }
    public void DestroyLastMovedTrail()
    {
        DestroyIllegalTrail();

        if (lastMoveTrail != null)
        {
            Destroy(lastMoveTrail.gameObject);
        }

        if (checkMoveTrail != null)
        {
            Destroy(checkMoveTrail.gameObject);
        }

        if (extraMoveTrails != null)
        {
            for (int i = 0; i < extraMoveTrails.Count; i++)
            {
                if (extraMoveTrails[i] != null)
                {
                    Destroy(extraMoveTrails[i].gameObject);
                }
            }
            extraMoveTrails = null;
        }
    }
    public void DestroyIllegalTrail()
    {
        //destroy when you click on a piece
        if (illegalMoveTrail != null)
        {
            Destroy(illegalMoveTrail.gameObject);
        }
    }
    public override void ResetSelected(bool forceDeselect = true)
    {
        for (int i = 0; i < 64; i++)
        {
            squares[i].ResetColor();
        }
        /*
        if (pieceInfoText != null)
        {
            pieceInfoText.text = "";
        }
        if (pmps != null)
        {
            pmps.ResetAll();
        }
        */

        if (board.GetLastMove() != 0)
        {
            bool isGiant = (GlobalPieceManager.GetPieceTableEntry(board.GetLastMovedPiece()).piecePropertyB & PiecePropertyB.Giant) != 0;

            int fromX = Move.GetFromX(board.GetLastMove());
            int fromY = Move.GetFromY(board.GetLastMove());
            int toX = Move.GetToX(board.GetLastMove());
            int toY = Move.GetToY(board.GetLastMove());

            squares[fromX + fromY * 8].SetLastMovedSquare();
            squares[toX + toY * 8].SetLastMovedSquare();

            if (isGiant)
            {
                squares[fromX + 1 + fromY * 8].SetLastMovedSquare();
                squares[toX + 1 + toY * 8].SetLastMovedSquare();
                squares[fromX + (fromY + 1) * 8].SetLastMovedSquare();
                squares[toX + (toY + 1) * 8].SetLastMovedSquare();
                squares[fromX + 1 + (fromY + 1) * 8].SetLastMovedSquare();
                squares[toX + 1 + (toY + 1) * 8].SetLastMovedSquare();
            }
        }

        /*
        if (selectedPiece == null || forceDeselect)
        {
            DestroyIllegalTrail();
        }
        */
        if (selectedPiece != null && forceDeselect)
        {
            selectedPiece.ForceDeselect();
        }
        if (selectedConsumable != null && forceDeselect)
        {
            selectedConsumable.ForceDeselect();
        }
        if (selectedBadge != null && forceDeselect)
        {
            selectedBadge.ForceDeselect();
        }
        selectedPiece = null;
        selectedConsumable = null;
        selectedBadge = null;
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

    public void TryConsumableMove(ConsumableScript cs, int index, int x, int y)
    {
        uint move = Move.EncodeConsumableMove(cs.cmt, x, y);

        Debug.Log(Move.ConvertToString(move));

        if (gameOver || board.blackToMove)
        {
            return;
        }

        if (Board.IsConsumableMoveLegal(ref board, move))
        {
            Debug.Log("Apply " + Move.ConvertToString(move));

            //Consume the consumable
            if (index >= 0)
            {
                MainManager.Instance.playerData.consumables[index] = Move.ConsumableMoveType.None;
            }

            List<BoardUpdateMetadata> boardUpdateMetadata = new List<BoardUpdateMetadata>();

            board.ApplyMove(move, boardUpdateMetadata);

            awaitingMove = false;
            chessAI.moveFound = false;
            if (difficulty != MainManager.Instance.playerData.difficulty)
            {
                difficulty = MainManager.Instance.playerData.difficulty;
                SetDifficulty(difficulty);
            }
            while (historyList.Count > historyIndex + 1)
            {
                historyList.RemoveAt(historyIndex + 1);
            }
            historyList.Add(new Board(board));
            historyIndex++;
            RegenerateMoveList();
            chessAI.history.Add(chessAI.HashFromScratch(ref board));
            ResetSelected(true);

            if (lastMoveTrail != null)
            {
                Destroy(lastMoveTrail.gameObject);
            }
            if (illegalMoveTrail != null)
            {
                Destroy(illegalMoveTrail.gameObject);
            }
            if (checkMoveTrail != null)
            {
                Destroy(checkMoveTrail.gameObject);
            }
            Board checkCopy = new Board(board);
            checkCopy.ApplyNullMove();
            (uint checkMove, List<MoveMetadata> moveTrailCheck) = Board.FindKingCaptureMovePath(ref checkCopy);
            if (checkMove != 0)
            {
                checkMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                checkMoveTrail.Setup(Move.GetFromX(checkMove), Move.GetFromY(checkMove), moveTrailCheck);
                checkMoveTrail.SetColorMoveCheck();
                if (!checkCopy.blackToMove)
                {
                    checkMoveTrail.SetColorLight();
                }
                else
                {
                    checkMoveTrail.SetColorDark();
                }
            }

            if (extraMoveTrails != null)
            {
                for (int i = 0; i < extraMoveTrails.Count; i++)
                {
                    if (extraMoveTrails[i] != null)
                    {
                        Destroy(extraMoveTrails[i].gameObject);
                    }
                }
                extraMoveTrails = null;
            }
            extraMoveTrails = new List<MoveTrailScript>();
            for (int i = 0; i < boardUpdateMetadata.Count; i++)
            {
                if (boardUpdateMetadata[i].tx != -1)
                {
                    MoveTrailScript mtsE = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                    extraMoveTrails.Add(mtsE);
                    mtsE.SetColorMoveSecondary();
                    mtsE.Setup(boardUpdateMetadata[i].fx, boardUpdateMetadata[i].fy, boardUpdateMetadata[i].tx, boardUpdateMetadata[i].ty);
                }
            }

            StartAnimatingBoardUpdate(move, board.GetLastMoveStationary(), null, boardUpdateMetadata);
            //FixBoardBasedOnPosition();

            if (board.GetVictoryCondition() != PieceAlignment.Null)
            {
                if (board.GetVictoryCondition() == PieceAlignment.White)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White wins with special condition");
                    gameOver = true;
                    WinBattle();
                }
                if (board.GetVictoryCondition() == PieceAlignment.Black)
                {
                    winnerPA = PieceAlignment.Black;
                    Debug.Log("Black wins with special condition");
                    gameOver = true;
                    LoseBattle();
                }
                return;
            }

            if (!board.CheckForKings())
            {
                //Which side has no kings?
                if (board.GetKingCaptureWinner() == PieceAlignment.White)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White wins with special condition");
                    gameOver = true;
                    WinBattle();
                }
                if (board.GetKingCaptureWinner() == PieceAlignment.Black)
                {
                    winnerPA = PieceAlignment.Black;
                    Debug.Log("Black wins with special condition");
                    gameOver = true;
                    LoseBattle();
                }
                if (board.GetKingCaptureWinner() == PieceAlignment.Neutral)
                {
                    winnerPA = PieceAlignment.Null;
                    Debug.Log("Draw with special condition");
                    gameOver = true;
                    LoseBattle();
                }
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
                    WinBattle();
                }
                else if (stalemateAI)
                {
                    //new: stalemate is loss
                    winnerPA = PieceAlignment.White;
                    Debug.Log("Draw (Black stalemated)");
                    gameOver = true;
                    WinBattle();
                }
            }
        }
        else
        {
            if (Board.IsConsumableMoveValid(ref board, move))
            {
                //Illegal move failsafe?
                awaitingMove = false;

                (uint refMove, List<MoveMetadata> illegalPath) = Board.MoveIllegalByCheckFindRefutationPath(ref board, move);
                Debug.Log("Move is illegal because of " + Move.ConvertToString(refMove));

                if (illegalMoveTrail != null)
                {
                    Destroy(illegalMoveTrail.gameObject);
                }
                if (illegalPath != null)
                {
                    illegalMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                    illegalMoveTrail.SetColorMoveIllegal();
                    illegalMoveTrail.Setup(Move.GetFromX(refMove), Move.GetFromY(refMove), illegalPath);
                    //Debug.Log("Make illegal path");

                    //this doesn't trigger bonus move so this doesn't need a wacky check
                    illegalMoveTrail.SetColorDark();
                }

                /*
                string pathString = Move.PositionToString(Move.GetFromX(refMove), Move.GetFromY(refMove));
                for (int i = 0; i < illegalPath.Count; i++)
                {
                    if (illegalPath[i] == null)
                    {
                        pathString += " X";
                    }
                    else
                    {
                        pathString += " " + Move.PositionToString(illegalPath[i].x, illegalPath[i].y);
                    }
                }
                Debug.Log("Refutation " + Move.ConvertToString(move) + " move path: " + pathString);
                */

                FixBoardBasedOnPosition();
            }
        }
    }
    public override bool TrySetupMove(PieceScript ps, int x, int y, int newX, int newY)
    {
        if (x < 0 || x > 7 || newX < 0 || newX > 7)
        {
            return false;
        }

        if (y < 0 || y > 7 || newY < 0 || newY > 7)
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
            Debug.Log("Apply " + Move.ConvertToString(move));

            List<BoardUpdateMetadata> boardUpdateMetadata = new List<BoardUpdateMetadata>();

            board.MakeSetupMove(move);

            historyList[historyIndex] = (new Board(board));

            awaitingMove = false;
            chessAI.moveFound = false;
            if (difficulty != MainManager.Instance.playerData.difficulty)
            {
                difficulty = MainManager.Instance.playerData.difficulty;
                SetDifficulty(difficulty);
            }
            RegenerateMoveList();
            ResetSelected();

            if (lastMoveTrail != null)
            {
                Destroy(lastMoveTrail.gameObject);
            }
            if (illegalMoveTrail != null)
            {
                Destroy(illegalMoveTrail.gameObject);
            }
            if (checkMoveTrail != null)
            {
                Destroy(checkMoveTrail.gameObject);
            }

            if (extraMoveTrails != null)
            {
                for (int i = 0; i < extraMoveTrails.Count; i++)
                {
                    if (extraMoveTrails[i] != null)
                    {
                        Destroy(extraMoveTrails[i].gameObject);
                    }
                }
                extraMoveTrails = null;
            }
            extraMoveTrails = new List<MoveTrailScript>();

            FixBoardBasedOnPosition();
            return true;
        }

        ResetSelected();
        FixBoardBasedOnPosition();
        return false;
    }

    public override void TryMove(PieceScript ps, Piece.PieceAlignment pa, int x, int y, int newX, int newY)
    {
        if (gameOver)
        {
            return;
        }

        //No
        if (animating)
        {
            return;
        }

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

        //null move or move not in the list
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
            List<MoveMetadata> moveTrail = null;
            if (moveMetadata.ContainsKey(Move.RemoveNonLocation(move)))
            {
                moveTrail = moveMetadata[Move.RemoveNonLocation(move)].TracePath(Move.GetFromX(move), Move.GetFromY(move), Move.GetDir(move));
                //Debug.Log(MoveMetadata.PathTagToString(moveTrail[0].pathTags[0]));
            }

            /*
            string pathString = Move.PositionToString(Move.GetFromX(move), Move.GetFromY(move));
            for (int i = 0; i < moveTrail.Count; i++)
            {
                if (moveTrail[i] == null)
                {
                    pathString += " X";
                }
                else
                {
                    pathString += " " + Move.PositionToString(moveTrail[i].x, moveTrail[i].y) + "(" + moveTrail[i].pathTags[0] + ")";
                }
            }
            Debug.Log("Move Path " + Move.ConvertToString(move) + " move path: " + pathString);
            */

            /*
            if (!whiteIsAI)
            {
                Debug.Log("Apply " + Move.ConvertToString(move));
            }
            */
            Debug.Log("Apply " + Move.ConvertToString(move));

            List<BoardUpdateMetadata> boardUpdateMetadata = new List<BoardUpdateMetadata>();

            board.ApplyMove(move, boardUpdateMetadata);
            //Debug.Log(chessAI.HashFromScratch(board));

            awaitingMove = false;
            chessAI.moveFound = false;
            if (difficulty != MainManager.Instance.playerData.difficulty)
            {
                difficulty = MainManager.Instance.playerData.difficulty;
                SetDifficulty(difficulty);
            }
            while (historyList.Count > historyIndex + 1)
            {
                historyList.RemoveAt(historyIndex + 1);
            }
            historyList.Add(new Board(board));
            historyIndex++;
            RegenerateMoveList();
            if (chessAI.history.Contains(chessAI.HashFromScratch(ref board)))
            {
                Debug.Log("Repeat position?");
            }
            chessAI.history.Add(chessAI.HashFromScratch(ref board));

            ResetSelected(true);

            if (lastMoveTrail != null)
            {
                Destroy(lastMoveTrail.gameObject);
            }
            lastMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();

            if (board.GetLastMoveStationary())
            {
                lastMoveTrail.SetColorMoveStationary();
            }
            else
            {
                lastMoveTrail.SetColorMove();
            }

            if (moveTrail == null)
            {
                Debug.LogError("Failsafe move trail");
                lastMoveTrail.Setup(Move.GetFromX(move), Move.GetFromY(move), Move.GetToX(move), Move.GetToY(move));
                if (board.blackToMove ^ (board.bonusPly != 0))
                {
                    lastMoveTrail.SetColorLight();
                }
                else
                {
                    lastMoveTrail.SetColorDark();
                }
            }
            else
            {
                lastMoveTrail.Setup(Move.GetFromX(move), Move.GetFromY(move), moveTrail);
                if (board.blackToMove ^ (board.bonusPly != 0))
                {
                    lastMoveTrail.SetColorLight();
                }
                else
                {
                    lastMoveTrail.SetColorDark();
                }
            }

            if (illegalMoveTrail != null)
            {
                Destroy(illegalMoveTrail.gameObject);
            }
            if (checkMoveTrail != null)
            {
                Destroy(checkMoveTrail.gameObject);
            }
            Board checkCopy = new Board(board);
            checkCopy.ApplyNullMove();
            (uint checkMove, List<MoveMetadata> moveTrailCheck) = Board.FindKingCaptureMovePath(ref checkCopy);
            if (checkMove != 0)
            {
                checkMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                checkMoveTrail.Setup(Move.GetFromX(checkMove), Move.GetFromY(checkMove), moveTrailCheck);
                checkMoveTrail.SetColorMoveCheck();
                if (!checkCopy.blackToMove)
                {
                    checkMoveTrail.SetColorLight();
                }
                else
                {
                    checkMoveTrail.SetColorDark();
                }
            }

            if (extraMoveTrails != null)
            {
                for (int i = 0; i < extraMoveTrails.Count; i++)
                {
                    if (extraMoveTrails[i] != null)
                    {
                        Destroy(extraMoveTrails[i].gameObject);
                    }
                }
                extraMoveTrails = null;
            }
            extraMoveTrails = new List<MoveTrailScript>();
            for (int i = 0; i < boardUpdateMetadata.Count; i++)
            {
                if (boardUpdateMetadata[i].tx != -1)
                {
                    MoveTrailScript mtsE = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                    extraMoveTrails.Add(mtsE);
                    mtsE.SetColorMoveSecondary();
                    mtsE.Setup(boardUpdateMetadata[i].fx, boardUpdateMetadata[i].fy, boardUpdateMetadata[i].tx, boardUpdateMetadata[i].ty);
                }
            }

            StartAnimatingBoardUpdate(move, board.GetLastMoveStationary(), moveTrail, boardUpdateMetadata);
            //FixBoardBasedOnPosition();

            if (board.GetVictoryCondition() != PieceAlignment.Null)
            {
                if (board.GetVictoryCondition() == PieceAlignment.White)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White wins with special condition");
                    gameOver = true;
                    WinBattle();
                }
                if (board.GetVictoryCondition() == PieceAlignment.Black)
                {
                    winnerPA = PieceAlignment.Black;
                    Debug.Log("Black wins with special condition");
                    gameOver = true;
                    LoseBattle();
                }
                return;
            }

            if (!board.CheckForKings())
            {
                //Which side has no kings?
                if (board.GetKingCaptureWinner() == PieceAlignment.White)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("White wins with special condition");
                    gameOver = true;
                    WinBattle();
                }
                if (board.GetKingCaptureWinner() == PieceAlignment.Black)
                {
                    winnerPA = PieceAlignment.Black;
                    Debug.Log("Black wins with special condition");
                    gameOver = true;
                    LoseBattle();
                }
                if (board.GetKingCaptureWinner() == PieceAlignment.Neutral)
                {
                    winnerPA = PieceAlignment.Null;
                    Debug.Log("Draw with special condition");
                    gameOver = true;
                    LoseBattle();
                }
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
                    WinBattle();
                }
                else if (stalemateAI)
                {
                    winnerPA = PieceAlignment.White;
                    Debug.Log("Draw (Black stalemated)");
                    gameOver = true;
                    WinBattle();
                }
            }
        }
        else
        {
            //Illegal move failsafe?
            awaitingMove = false;

            (uint refMove, List<MoveMetadata> illegalPath) = Board.MoveIllegalByCheckFindRefutationPath(ref board, move);
            //Debug.Log("Move is illegal because of " + Move.ConvertToString(move));

            string pathString = Move.PositionToString(Move.GetFromX(refMove), Move.GetFromY(refMove));
            for (int i = 0; i < illegalPath.Count; i++)
            {
                if (illegalPath[i] == null)
                {
                    pathString += " X";
                }
                else
                {
                    pathString += " " + Move.PositionToString(illegalPath[i].x, illegalPath[i].y);
                }
            }
            Debug.Log("Refutation " + Move.ConvertToString(move) + " move path: " + pathString);

            if (illegalMoveTrail != null)
            {
                Destroy(illegalMoveTrail.gameObject);
            }
            illegalMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
            illegalMoveTrail.SetColorMoveIllegal();

            //dumb way to check stuff but ehh
            Board checkRefutationCopy = new Board(board);
            checkRefutationCopy.ApplyMove(move);
            if (!checkRefutationCopy.blackToMove)
            {
                illegalMoveTrail.SetColorLight();
            }
            else
            {
                illegalMoveTrail.SetColorDark();
            }
            illegalMoveTrail.Setup(Move.GetFromX(refMove), Move.GetFromY(refMove), illegalPath);

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
        errorMove = false;

        if (aiMove == 0)
        {
            bool checkAI = Board.PositionIsCheck(ref board);
            bool stalemateAI = Board.PositionIsStalemate(ref board);

            if (checkAI && stalemateAI)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("White win");
                gameOver = true;
                WinBattle();
            }
            else if (stalemateAI)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("Black stalemated");
                gameOver = true;
                WinBattle();
            }
            else
            {
                winnerPA = PieceAlignment.Null;
                Debug.LogError("AI failed to move for some reason");
                gameOver = true;
                drawError = true;
                LoseBattle();
            }
            return;
        }

        uint checkMove = FindMoveInMoveList(Move.GetFromX(aiMove), Move.GetFromY(aiMove), Move.GetToX(aiMove), Move.GetToY(aiMove));
        if (checkMove == 0 || !Board.IsMoveLegal(ref board, checkMove, true))
        {
            //Error
            Debug.LogError("Black AI attempted an illegal move, retrying");
            //MainManager.PrintBitboard(board.globalData.bitboard_piecesBlack);
            //MainManager.PrintBitboard(board.globalData.bitboard_piecesBlackAdjacent);
            //Debug.LogError(board.globalData.bitboard_piecesBlack + " " + board.globalData.bitboard_piecesBlackAdjacent);
            awaitingMove = false;
            chessAI.moveFound = false;
            errorMove = true;
            return;
        }

        //lastmove for AI
        Dictionary<uint, MoveMetadata> moveDict = new Dictionary<uint, MoveMetadata>();
        List<uint> moveList = new List<uint>();
        MoveGenerator.GenerateMovesForPlayer(moveList, ref board, PieceAlignment.Black, moveDict);

        List<MoveMetadata> moveTrail = null;
        List<BoardUpdateMetadata> boardUpdateMetadata = new List<BoardUpdateMetadata>();

        if (!moveMetadata.ContainsKey(Move.RemoveNonLocation(aiMove)))
        {
            RegenerateMoveList();
        }
        else
        {
            moveTrail = moveMetadata[Move.RemoveNonLocation(aiMove)].TracePath(Move.GetFromX(aiMove), Move.GetFromY(aiMove), Move.GetDir(aiMove));
        }

        ResetSelected();
        board.ApplyMove(aiMove, boardUpdateMetadata);
        awaitingMove = false;
        chessAI.moveFound = false;
        if (difficulty != MainManager.Instance.playerData.difficulty)
        {
            difficulty = MainManager.Instance.playerData.difficulty;
            SetDifficulty(difficulty);
        }
        while (historyList.Count > historyIndex + 1)
        {
            historyList.RemoveAt(historyIndex + 1);
        }
        historyList.Add(new Board(board));
        historyIndex++;
        RegenerateMoveList();
        if (chessAI.history.Contains(chessAI.HashFromScratch(ref board)))
        {
            Debug.Log("Repeat position?");
        }
        chessAI.history.Add(chessAI.HashFromScratch(ref board));
        ResetSelected(true);

        if (lastMoveTrail != null)
        {
            Destroy(lastMoveTrail.gameObject);
        }
        lastMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
        if (board.GetLastMoveStationary())
        {
            lastMoveTrail.SetColorMoveStationary();
        }
        else
        {
            lastMoveTrail.SetColorMove();
        }
        if (moveTrail == null)
        {
            Debug.LogWarning("Failsafe move trail");
            lastMoveTrail.Setup(Move.GetFromX(aiMove), Move.GetFromY(aiMove), Move.GetToX(aiMove), Move.GetToY(aiMove));
            lastMoveTrail.SetColorDark();
        }
        else
        {
            lastMoveTrail.Setup(Move.GetFromX(aiMove), Move.GetFromY(aiMove), moveTrail);
            lastMoveTrail.SetColorDark();
        }

        if (illegalMoveTrail != null)
        {
            Destroy(illegalMoveTrail.gameObject);
        }
        if (checkMoveTrail != null)
        {
            Destroy(checkMoveTrail.gameObject);
        }
        Board checkCopy = new Board(board);
        checkCopy.ApplyNullMove();
        (uint checkMoveB, List<MoveMetadata> moveTrailCheck) = Board.FindKingCaptureMovePath(ref checkCopy);
        if (checkMoveB != 0)
        {
            checkMoveTrail = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
            checkMoveTrail.Setup(Move.GetFromX(checkMoveB), Move.GetFromY(checkMoveB), moveTrailCheck);
            checkMoveTrail.SetColorMoveCheck();
            if (!checkCopy.blackToMove)
            {
                checkMoveTrail.SetColorLight();
            }
            else
            {
                checkMoveTrail.SetColorDark();
            }
        }

        if (extraMoveTrails != null)
        {
            for (int i = 0; i < extraMoveTrails.Count; i++)
            {
                if (extraMoveTrails[i] != null)
                {
                    Destroy(extraMoveTrails[i].gameObject);
                }
            }
            extraMoveTrails = null;
        }
        extraMoveTrails = new List<MoveTrailScript>();
        for (int i = 0; i < boardUpdateMetadata.Count; i++)
        {
            if (boardUpdateMetadata[i].tx != -1)
            {
                MoveTrailScript mtsE = Instantiate(moveTrailTemplate, transform).GetComponent<MoveTrailScript>();
                extraMoveTrails.Add(mtsE);
                mtsE.SetColorMoveSecondary();
                mtsE.Setup(boardUpdateMetadata[i].fx, boardUpdateMetadata[i].fy, boardUpdateMetadata[i].tx, boardUpdateMetadata[i].ty);
            }
        }

        StartAnimatingBoardUpdate(aiMove, board.GetLastMoveStationary(), moveTrail, boardUpdateMetadata);
        //FixBoardBasedOnPosition();

        bool check = Board.PositionIsCheck(ref board);
        bool stalemate = Board.PositionIsStalemate(ref board);

        if (board.GetVictoryCondition() != PieceAlignment.Null)
        {
            if (board.GetVictoryCondition() == PieceAlignment.White)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("White wins with special condition");
                gameOver = true;
                WinBattle();
            }
            if (board.GetVictoryCondition() == PieceAlignment.Black)
            {
                winnerPA = PieceAlignment.Black;
                Debug.Log("Black wins with special condition");
                gameOver = true;
                LoseBattle();
            }
            return;
        }

        if (!board.CheckForKings())
        {
            //Which side has no kings?
            if (board.GetKingCaptureWinner() == PieceAlignment.White)
            {
                winnerPA = PieceAlignment.White;
                Debug.Log("White wins with special condition");
                gameOver = true;
                WinBattle();
                return;
            }
            if (board.GetKingCaptureWinner() == PieceAlignment.Black)
            {
                winnerPA = PieceAlignment.Black;
                Debug.Log("Black wins with special condition");
                gameOver = true;
                LoseBattle();
                return;
            }
            if (board.GetKingCaptureWinner() == PieceAlignment.Neutral)
            {
                winnerPA = PieceAlignment.Null;
                Debug.Log("Draw with special condition");
                gameOver = true;
                LoseBattle();
                return;
            }
        }

        if (check && stalemate)
        {
            winnerPA = PieceAlignment.Black;
            Debug.Log("Black win");
            gameOver = true;
            LoseBattle();
        }
        else if (stalemate)
        {
            //stalemate is loss
            winnerPA = PieceAlignment.Black;
            Debug.Log("White stalemated");
            gameOver = true;
            LoseBattle();
        }
    }

    public void UndoReset()
    {
        if (historyIndex == 0)
        {
            return;
        }

        gameOver = false;
        drawError = false;
        historyIndex = 0;
        board.CopyOverwrite(historyList[0]);
        //Destroy the future history
        chessAI.history = new HashSet<ulong>
        {
            chessAI.HashFromScratch(historyList[0])
        };
        DestroyLastMovedTrail();    //it is annoying to get the correct trail so I'll just destroy it 
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();
    }
    public void DoubleUndo()
    {
        if (historyIndex <= 0)
        {
            return;
        }
        MainManager.Instance.playerData.undosLeft--;
        /*
        if (historyIndex <= 1)
        {
            return;
        }

        gameOver = false;
        drawError = false;
        historyIndex -= 2;
        board.CopyOverwrite(historyList[historyIndex]);
        //Destroy the future history
        chessAI.history.Remove(chessAI.HashFromScratch(historyList[historyIndex + 1]));
        chessAI.history.Remove(chessAI.HashFromScratch(historyList[historyIndex + 2]));
        DestroyLastMovedTrail();    //it is annoying to get the correct trail so I'll just destroy it 
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();
        */
        bool blackToMove = board.blackToMove;
        int turn = board.turn;
        while (board.blackToMove != blackToMove || turn == board.turn || board.bonusPly > 0)
        {
            Undo();
            if (historyIndex == 0)
            {
                break;
            }
        }
        blackIsAI = true;
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
        DestroyLastMovedTrail();    //it is annoying to get the correct trail so I'll just destroy it 
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
        DestroyLastMovedTrail();    //it is annoying to get the correct trail so I'll just destroy it 
        RegenerateMoveList();
        ResetSelected();
        FixBoardBasedOnPosition();
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

        Piece.Aura[] wAura = board.GetAuraBitboards(false);
        Piece.Aura[] bAura = board.GetAuraBitboards(true);

        //fix the pieces to match the board state
        for (int i = 0; i < pieces.Count; i++)
        {
            //also fix squares
            squares[i].sq = board.globalData.squares[i];
            squares[i].ResetSquareColor();
            squares[i].SetAura(wAura[i], bAura[i]);

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
                ps.squareBelow = squares[i];
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

    public void WinBattle()
    {
        GameObject go = Instantiate(battleWinPanelTemplate, MainManager.Instance.canvas.transform);
        go.GetComponent<BattleWinPanelScript>().bs = this;
    }
    public void LoseBattle()
    {
        Instantiate(battleLosePanelTemplate, MainManager.Instance.canvas.transform);
    }

    public IEnumerator AnimatePieceSpawn(uint piece, int ofx, int ofy, int otx, int oty)
    {
        //Spawn a piece

        int i = otx + (oty << 3);
        //for residue piece leavers the piece here is the last piece moved so don't destroy it yet
        /*
        if (pieces[i] != null)
        {
            Destroy(pieces[i].gameObject);
        }
        */

        GameObject go = Instantiate(pieceTemplate, pieceHolder.transform);
        go.name = "Piece " + i % 8 + " " + i / 8;
        PieceScript ps = go.GetComponent<PieceScript>();
        ps.bs = this;
        pieces[i] = ps;
        ps.Setup(piece, otx, oty);
        ps.squareBelow = squares[(otx + (oty << 3))];

        float duration = 0;
        float animationDuration = 3.6f / animationSpeed;

        while (duration < 1)
        {
            ps.transform.localScale = Vector3.one * duration;

            duration += Time.deltaTime / animationDuration;
            yield return null;
        }

        ps.transform.localScale = Vector3.one;
        pieces[i] = ps;
    }
    public IEnumerator AnimatePieceSpin(PieceScript ps, int otx, int oty)
    {
        PieceScript targetPiece = ps;
        if (targetPiece == null)
        {
            yield break;
        }

        Transform pivot = targetPiece.transform;
        if (targetPiece.isGiant)
        {
            int dx = Piece.GetPieceSpecialData(targetPiece.piece) & 1;
            int dy = (Piece.GetPieceSpecialData(targetPiece.piece) & 2) >> 1;

            ps = pieces[ps.x - dx + ((ps.y - dy) << 3)];
            if (targetPiece == null)
            {
                yield break;
            }

            //make a different pivot
            pivot = new GameObject().transform;
            pivot.parent = pieceHolder.transform;
            pivot.transform.position = GetSpritePositionFromCoordinates(otx + 0.5f, oty + 0.5f, -0.5f);
            targetPiece.transform.parent = pivot;
        }

        Vector3 targetPos = GetSpritePositionFromCoordinates(otx, oty, -0.5f);

        float duration = 0;
        float animationDuration = 3.6f / animationSpeed;

        while (duration < 1)
        {
            pivot.transform.localEulerAngles = Vector3.forward * duration * 360;
            duration += Time.deltaTime / animationDuration;
            yield return null;
        }

        if (targetPiece.isGiant)
        {
            ps.transform.parent = pieceHolder.transform;
            Destroy(pivot.gameObject);
        }

        ps.transform.localEulerAngles = Vector3.zero;
    }
    public IEnumerator AnimatePieceCapture(PieceScript ps, int otx, int oty)
    {
        PieceScript targetPiece = ps;
        if (targetPiece == null)
        {
            yield break;
        }

        if (targetPiece.isGiant)
        {
            int dx = Piece.GetPieceSpecialData(targetPiece.piece) & 1;
            int dy = (Piece.GetPieceSpecialData(targetPiece.piece) & 2) >> 1;

            ps = pieces[ps.x - dx + ((ps.y - dy) << 3)];
            if (targetPiece == null)
            {
                yield break;
            }
        }

        Vector3 targetPos = GetSpritePositionFromCoordinates(otx, oty, targetPiece.transform.position.z + 0.1f);

        float duration = 0;
        float animationDuration = 3.6f / animationSpeed;

        Vector3 startPos = ps.transform.position;

        while (duration < 1)
        {
            if (targetPiece.isGiant)
            {
                ps.transform.position = targetPos + new Vector3(1, 1, 0) * (SQUARE_SIZE / 2) * (duration);
            }
            else
            {
                ps.transform.position = targetPos;
            }
            ps.transform.localScale = Vector3.one * (1 - duration);

            duration += Time.deltaTime / animationDuration;
            yield return null;
        }

        ps.transform.localScale = Vector3.zero;
        Destroy(ps.gameObject);
        if (ps != lastMovedPiece)
        {
            pieces[(otx + (oty << 3))] = null;
        }
    }
    public IEnumerator AnimatePieceShift(PieceScript ps, int otx, int oty)
    {
        PieceScript targetPiece = ps;

        if (targetPiece == null)
        {
            yield break;
        }

        Vector3 startPos = ps.transform.position;
        startPos.z = -1.1f;
        Vector3 targetPos = GetSpritePositionFromCoordinates(otx, oty, -1.1f);

        float animationDuration = 3.6f / animationSpeed;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            targetPiece.transform.position = Vector3.Lerp(startPos, targetPos, MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;
        }
        targetPiece.transform.position = targetPos;

        if (pieces[ps.x + (ps.y << 3)] != null)
        {
            pieces[ps.x + (ps.y << 3)] = null;
        }
        pieces[(otx + (oty << 3))] = targetPiece;
        targetPiece.x = otx;
        targetPiece.y = oty;
    }

    public IEnumerator AnimatePieceMove(MoveParticleScript mps, int ofx, int ofy, int otx, int oty)
    {
        if (ofx < 0 || ofx > 7 || ofy < 0 || ofy > 7)
        {
            yield break;
        }

        PieceScript targetPiece = pieces[ofx + (ofy << 3)];
        if (targetPiece == null)
        {
            yield break;
        }

        mps.transform.position = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);

        Vector3 startPos = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);
        Vector3 targetPos = GetSpritePositionFromCoordinates(otx, oty, -1.1f);

        float animationDuration = 3.6f / animationSpeed;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            mps.transform.position = Vector3.Lerp(startPos, targetPos, MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;
        }
        mps.transform.position = targetPos;
    }
    public IEnumerator AnimatePieceMove(MoveParticleScript mps, int ofx, int ofy, List<MoveMetadata> moveTrail)
    {
        PieceScript targetPiece = pieces[ofx + (ofy << 3)];
        if (targetPiece == null)
        {
            yield break;
        }
        mps.transform.position = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);

        int sx = ofx;
        int sy = ofy;

        List<Vector3> pathList = MoveTrailScript.MakeTrailPoints(ofx, ofy, moveTrail);

        float animationDuration = 3.6f / animationSpeed;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            mps.transform.position = MoveTrailScript.LerpList(pathList, MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;
        }
        mps.transform.position = pathList[pathList.Count - 1];

        /*
        foreach (MoveMetadata m in moveTrail)
        {
            int mx = m.x;
            int my = m.y;

            if (m.path != MoveMetadata.PathType.Teleport && m.path != MoveMetadata.PathType.TeleportGiant)
            {
                if (mx - sx > 4)
                {
                    mx -= 8;
                }
                if (sx - mx > 4)
                {
                    mx += 8;
                }
                if (my - sy > 4)
                {
                    my -= 8;
                }
                if (sy - my > 4)
                {
                    my += 8;
                }
            }
            targetPos = GetSpritePositionFromCoordinates(mx, my, -1.1f);
            sx = m.x;
            sy = m.y;

            while (mps.transform.position != targetPos)
            {
                Vector3 delta = (targetPos - mps.transform.position).normalized * animationSpeed * Time.deltaTime;

                if ((targetPos - mps.transform.position).magnitude < animationSpeed * Time.deltaTime)
                {
                    mps.transform.position = targetPos;
                    break;
                }

                mps.transform.position += delta;

                if (mps.transform.position.x > SQUARE_SIZE * 4)
                {
                    mps.transform.position -= Vector3.right * SQUARE_SIZE * 8;
                    targetPos -= Vector3.right * SQUARE_SIZE * 8;
                }
                if (mps.transform.position.x < -SQUARE_SIZE * 4)
                {
                    mps.transform.position += Vector3.right * SQUARE_SIZE * 8;
                    targetPos += Vector3.right * SQUARE_SIZE * 8;
                }
                if (mps.transform.position.y > SQUARE_SIZE * 4)
                {
                    mps.transform.position -= Vector3.up * SQUARE_SIZE * 8;
                    targetPos -= Vector3.up * SQUARE_SIZE * 8;
                }
                if (mps.transform.position.y < -SQUARE_SIZE * 4)
                {
                    mps.transform.position += Vector3.up * SQUARE_SIZE * 8;
                    targetPos += Vector3.up * SQUARE_SIZE * 8;
                }

                yield return null;
            }
            yield return null;
        }
        */
    }

    public IEnumerator AnimatePieceMove(int ofx, int ofy, int otx, int oty)
    {
        if (ofx < 0 || ofx > 7 || ofy < 0 || ofy > 7)
        {
            yield break;
        }

        PieceScript targetPiece = pieces[ofx + (ofy << 3)];
        if (targetPiece == null)
        {
            yield break;
        }

        targetPiece.transform.position = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);

        Vector3 startPos = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);
        Vector3 targetPos = GetSpritePositionFromCoordinates(otx, oty, -1.1f);

        float animationDuration = 3.6f / animationSpeed;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            targetPiece.transform.position = Vector3.Lerp(startPos, targetPos, MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;
        }
        targetPiece.transform.position = targetPos;

        //other functions reset the position so fix it here?
        //      (Resets because it calls Setup which sets piece positions)
        //I could remove the position reset (make a second Setup) but ehh
        targetPiece.x = otx;
        targetPiece.y = oty;
    }
    public IEnumerator AnimatePieceMove(int ofx, int ofy, List<MoveMetadata> moveTrail)
    {
        PieceScript targetPiece = pieces[ofx + (ofy << 3)];
        if (targetPiece == null)
        {
            yield break;
        }
        targetPiece.transform.position = GetSpritePositionFromCoordinates(ofx, ofy, -1.1f);

        int sx = ofx;
        int sy = ofy;

        List<Vector3> pathList = MoveTrailScript.MakeTrailPoints(ofx, ofy, moveTrail);

        float animationDuration = 3.6f / animationSpeed;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            targetPiece.transform.position = MoveTrailScript.LerpList(pathList, MainManager.EasingQuadratic(time / animationDuration, 1));
            yield return null;
        }
        targetPiece.transform.position = pathList[pathList.Count - 1];

        //other functions reset the position so fix it here?
        //      (Resets because it calls Setup which sets piece positions)
        //I could remove the position reset (make a second Setup) but ehh
        targetPiece.x = moveTrail[moveTrail.Count - 1].x;
        targetPiece.y = moveTrail[moveTrail.Count - 1].y;
    }

    public void StartAnimatingBoardUpdate(uint move, bool lastMoveStationary, List<MoveMetadata> moveTrail, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        //"instant animations"
        //It basically doesn't even play them at all and just uses FixBoardBasedOnPosition to fix the position
        if (animationSpeed < 10000)
        {
            animating = true;
            animCoroutine = StartCoroutine(AnimateBoardUpdate(move, lastMoveStationary, moveTrail, boardUpdateMetadata));
        }
    }

    public IEnumerator AnimateBoardUpdate(uint move, bool lastMoveStationary, List<MoveMetadata> moveTrail, List<BoardUpdateMetadata> boardUpdateMetadata)
    {
        int ofx = Move.GetFromX(move);
        int ofy = Move.GetFromY(move);

        int otx = Move.GetToX(move);
        int oty = Move.GetToY(move);

        if (ofx >= 0 && ofx <= 7 && ofy >= 0 && ofy <= 7)
        {
            lastMovedPiece = pieces[ofx + (ofy << 3)];
        }
        if (!lastMoveStationary)
        {
            //Animate trail
            if (moveTrail == null)
            {
                yield return StartCoroutine(AnimatePieceMove(ofx, ofy, otx, oty));
            }
            else
            {
                yield return StartCoroutine(AnimatePieceMove(ofx, ofy, moveTrail));
            }
        }
        else
        {
            //Spawn a particle that moves along the path
            switch (Move.GetSpecialType(move))
            {
                case Move.SpecialType.CarryAlly:
                    yield return StartCoroutine(AnimatePieceMove(otx, oty, ofx, ofy));
                    if (pieces[otx + (oty << 3)] != null)
                    {
                        Destroy(pieces[otx + (oty << 3)].gameObject);
                    }
                    break;
                default:
                    //for now it's just a generic dot for most stuff
                    MoveParticleScript mps = Instantiate(moveParticleTemplate, pieces[ofx + (ofy << 3)].transform).GetComponent<MoveParticleScript>();
                    mps.Setup(Piece.GetPieceAlignment(pieces[ofx + (ofy << 3)].piece));
                    if (moveTrail == null)
                    {
                        yield return StartCoroutine(AnimatePieceMove(mps, ofx, ofy, otx, oty));
                    }
                    else
                    {
                        yield return StartCoroutine(AnimatePieceMove(mps, ofx, ofy, moveTrail));
                    }
                    Destroy(mps.gameObject);
                    break;
            }
        }

        //to do later: animation for stationary moves (some particle travels along the path instead of the piece moving)

        //To do later: some kind of parallel data structure that can handle mixed ordering and parallel components

        for (int i = 0; i < boardUpdateMetadata.Count; i++)
        {
            int fx = boardUpdateMetadata[i].fx;
            int fy = boardUpdateMetadata[i].fy;
            int tx = boardUpdateMetadata[i].tx;
            int ty = boardUpdateMetadata[i].ty;
            if (tx == -1 && ty == -1)
            {
                tx = fx;
                ty = fy;
            }

            if (boardUpdateMetadata[i].wasLastMovedPiece)
            {
                switch (boardUpdateMetadata[i].type)
                {
                    case BoardUpdateMetadata.BoardUpdateType.Move:
                    case BoardUpdateMetadata.BoardUpdateType.Shift:
                        yield return StartCoroutine(AnimatePieceShift(lastMovedPiece, tx, ty));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.Capture:
                        yield return StartCoroutine(AnimatePieceCapture(lastMovedPiece, fx, fy));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.Spawn:
                        yield return StartCoroutine(AnimatePieceSpawn(boardUpdateMetadata[i].piece, fx, fy, tx, ty));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.TypeChange:
                        yield return StartCoroutine(AnimatePieceSpin(lastMovedPiece, fx, fy));
                        lastMovedPiece.Setup(boardUpdateMetadata[i].piece, lastMovedPiece.x, lastMovedPiece.y);
                        lastMovedPiece.squareBelow = squares[(lastMovedPiece.x + (lastMovedPiece.y << 3))];
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.AlignmentChange:
                        yield return StartCoroutine(AnimatePieceSpin(lastMovedPiece, fx, fy));
                        lastMovedPiece.Setup(boardUpdateMetadata[i].piece, lastMovedPiece.x, lastMovedPiece.y);
                        lastMovedPiece.squareBelow = squares[(lastMovedPiece.x + (lastMovedPiece.y << 3))];
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.ImbueModifier:
                    case BoardUpdateMetadata.BoardUpdateType.StatusCure:
                    case BoardUpdateMetadata.BoardUpdateType.StatusApply:
                        yield return StartCoroutine(AnimatePieceSpin(lastMovedPiece, fx, fy));
                        lastMovedPiece.Setup(boardUpdateMetadata[i].piece, lastMovedPiece.x, lastMovedPiece.y);
                        lastMovedPiece.squareBelow = squares[(lastMovedPiece.x + (lastMovedPiece.y << 3))];
                        break;
                }
            }
            else
            {
                PieceScript pieceActive = pieces[fx + (fy << 3)];

                //Events that apply to the last moved piece need to be handled in a special way (because it might move on top of a piece to capture but there are events that apply to it vs the piece that was there before)
                //The events that apply to the last moved that aren't handled by the last moved bool should end up in this case
                if (pieceActive == null && lastMovedPiece != null && lastMovedPiece.x == fx && lastMovedPiece.y == fy)
                {
                    pieceActive = lastMovedPiece;
                }
                switch (boardUpdateMetadata[i].type)
                {
                    case BoardUpdateMetadata.BoardUpdateType.Move:
                    case BoardUpdateMetadata.BoardUpdateType.Shift:
                        yield return StartCoroutine(AnimatePieceShift(pieceActive, tx, ty));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.Capture:
                        yield return StartCoroutine(AnimatePieceCapture(pieceActive, fx, fy));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.Spawn:
                        yield return StartCoroutine(AnimatePieceSpawn(boardUpdateMetadata[i].piece, fx, fy, tx, ty));
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.TypeChange:
                        yield return StartCoroutine(AnimatePieceSpin(pieceActive, fx, fy));
                        if (pieceActive != null)
                        {
                            pieceActive.Setup(boardUpdateMetadata[i].piece, pieceActive.x, pieceActive.y);
                            pieceActive.squareBelow = squares[(pieceActive.x + (pieceActive.y << 3))];
                        }
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.AlignmentChange:
                        yield return StartCoroutine(AnimatePieceSpin(pieceActive, fx, fy));
                        if (pieceActive != null)
                        {
                            pieceActive.Setup(boardUpdateMetadata[i].piece, pieceActive.x, pieceActive.y);
                            pieceActive.squareBelow = squares[(pieceActive.x + (pieceActive.y << 3))];
                        }
                        break;
                    case BoardUpdateMetadata.BoardUpdateType.ImbueModifier:
                    case BoardUpdateMetadata.BoardUpdateType.StatusCure:
                    case BoardUpdateMetadata.BoardUpdateType.StatusApply:
                        yield return StartCoroutine(AnimatePieceSpin(pieceActive, fx, fy));
                        if (pieceActive != null)
                        {
                            pieceActive.Setup(boardUpdateMetadata[i].piece, pieceActive.x, pieceActive.y);
                            pieceActive.squareBelow = squares[(pieceActive.x + (pieceActive.y << 3))];
                        }
                        break;
                }
            }
        }

        bool found = false;
        for (int i = 0; i < 64; i++)
        {
            if (pieces[i] == lastMovedPiece)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Destroy(lastMovedPiece.gameObject);
        }

        FixBoardBasedOnPosition();
        animating = false;
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
        moveMetadata = new Dictionary<uint, MoveMetadata>();
        MoveGenerator.GenerateMovesForPlayer(moveList, ref board, board.blackToMove ? PieceAlignment.Black : PieceAlignment.White, moveMetadata);
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
        enemyMoveMetadata = new Dictionary<uint, MoveMetadata>();
        Board copy = new Board(board);
        copy.ApplyNullMove(false);
        MoveGenerator.GenerateMovesForPlayer(enemyMoveList, ref copy, copy.blackToMove ? PieceAlignment.Black : PieceAlignment.White, enemyMoveMetadata);
        //MoveGeneratorInfoEntry.GenerateMovesForPlayer(enemyMoveList, ref board, PieceAlignment.Black);

        copy.globalData.mbtactiveInverse.MakeInverse(copy.globalData.mbtactive);
        for (int i = 0; i < squares.Count; i++)
        {
            if (copy.globalData.mbtactiveInverse.Get(i % 8, i / 8) != 0)
            {
                if (copy.blackToMove)
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

    public override void Update()
    {
        backgroundA.color = backgroundColorWhite;
        backgroundB.color = backgroundColorBlack;

        chessAI.searchDuration = moveThinkTime;

        if (awaitingMove)
        {
            thinkingText.text = "Depth " + chessAI.currentDepth + ": (" + chessAI.TranslateEval(chessAI.bestEvaluation) + ") " + Piece.GetPieceType(board.pieces[Move.GetFromX(chessAI.bestMove) + (Move.GetFromY(chessAI.bestMove) << 3)]) + " " + Move.ConvertToStringMinimal(chessAI.bestMove);
        }
        else
        {
            if (board.globalData.enemyModifier == 0)
            {
                thinkingText.text = "";
            }
            else
            {
                thinkingText.text = board.globalData.enemyModifier.ToString();
            }
        }

        turnText.text = "Turn " + (board.turn + (board.blackToMove ? 0.5f : 0)) + "\n<size=50%>" + (board.blackToMove ? "Black" : "White") + " to move</size>";
        if (board.bonusPly > 0)
        {
            turnText.text += "\n<size=50%>Bonus Move</size>";
        }

        /*
        float kingValue = (GlobalPieceManager.GetPieceTableEntry(PieceType.King).pieceValueX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE);
        scoreText.text = "Pieces (" + (board.whitePerPlayerInfo.pieceCount - board.blackPerPlayerInfo.pieceCount) + ")\n" + (board.whitePerPlayerInfo.pieceCount) + "\n<color=#000000>" + (board.blackPerPlayerInfo.pieceCount) + "</color>";
        scoreText.text += "\n\nValues (" + (((board.whitePerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - (board.blackPerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE)) / 2f) + ")\n" + (((board.whitePerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - kingValue) / 2f) + "\n" + (board.whitePerPlayerInfo.pieceValueSumX2 / GlobalPieceManager.KING_VALUE_BONUS) + " king(s)\n<color=#000000>" + (((board.blackPerPlayerInfo.pieceValueSumX2 & GlobalPieceManager.KING_VALUE_BONUS_MINUS_ONE) - kingValue) / 2f) + "\n" + (board.blackPerPlayerInfo.pieceValueSumX2 / GlobalPieceManager.KING_VALUE_BONUS) + " king(s)\n</color>";

        int whiteMoves = 0;
        int blackMoves = 0;
        if (board.blackToMove)
        {
            blackMoves = moveList.Count;
            whiteMoves = enemyMoveList.Count;
        }
        else
        {
            blackMoves = enemyMoveList.Count;
            whiteMoves = moveList.Count;
        }
        scoreText.text += "\n\nMoves Available";
        scoreText.text += "\n" + whiteMoves;
        scoreText.text += "\n<color=#000000>" + blackMoves + "</color>";
        */

        if (gameOver)
        {
            if (winnerPA == PieceAlignment.Null)
            {
                turnText.text += "\nDraw";
            }
            else
            {
                turnText.text += "\n" + winnerPA + " Wins";
            }
        }
        else
        {
            if (checkMoveTrail != null)
            {
                turnText.text += "\nCheck";
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
                StartCoroutine(chessAI.BestMoveCoroutine(errorMove));
            }

            if (whiteIsAI && !board.blackToMove && chessAI.moveFound && !animating)
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
                            WinBattle();
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

                    //Failsafe behavior: try again?
                    /*
                    whiteIsAI = false;
                    chessAI.moveFound = false;
                    Debug.Log("Self play ended");
                    drawError = true;
                    gameOver = true;
                    return;
                    */
                }

                int lastPly = board.ply;
                int lastBonusPly = board.bonusPly;
                errorMove = false;
                if (bestMove != 0)
                {
                    whiteIsAI = false;
                    TryMove(pieces[bestX + bestY * 8], PieceAlignment.White, bestX, bestY, bestToX, bestToY);
                    if (!animating)
                    {
                        FixBoardBasedOnPosition();
                    }
                    moveDelay = moveDelayValue;
                    whiteIsAI = true;
                }

                if (lastPly == board.ply && lastBonusPly == board.bonusPly)
                {
                    Debug.LogError("White AI attempted an illegal move, retrying");
                    chessAI.moveFound = false;
                    awaitingMove = false;
                    errorMove = true;
                }
                return;
            }

            if (board.blackToMove && blackIsAI && !awaitingMove)
            {
                awaitingMove = true;
                chessAI.board = board;
                chessAI.moveFound = false;
                chessAI.searchTime = 0;
                StartCoroutine(chessAI.BestMoveCoroutine(errorMove));
            }

            if (board.blackToMove && blackIsAI && chessAI.moveFound && !animating)
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

    public override bool CanSelectPieces()
    {
        return !animating;
    }
}
