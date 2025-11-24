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

    public PieceScript selectedPiece;
    public ConsumableScript selectedConsumable;
    public BadgeScript selectedBadge;

    public bool setupMoves = false;

    //offset by SQUARE_SIZE
    //so this is the center of each square
    //Board coordinates for x, y
    //  (so 0,0 = bottom left corner square)
    //  Float values for drawing stuff at the edges of the board (-0.5, +7.5)
    public static Vector3 GetSpritePositionFromCoordinates(float x, float y, float z)
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

    public virtual void Start()
    {
        MakeBoard();
    }

    public virtual void MakeBoard()
    {
        squares = new List<SquareScript>();

        board = new Board();
        board.Setup();

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

        //RegenerateMoveList();

        //make pieces
        pieces = new List<PieceScript>();
        for (int i = 0; i < 64; i++)
        {
            pieces.Add(null);
        }
        FixBoardBasedOnPosition();
    }

    public virtual void SelectConsumable(ConsumableScript cs)
    {
        ResetSelected(false);

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
        selectedBadge = null;
    }
    public virtual void SelectBadge(BadgeScript bs)
    {
        selectedPiece = null;
        selectedConsumable = null;
        selectedBadge = bs;
    }

    public virtual void SelectPiece(PieceScript piece)
    {
        ResetSelected(false);

        if (piece is SetupPieceScript)
        {
            //It isn't a piece that has legal moves

            selectedPiece = piece;
            return;
        }

        selectedPiece = piece;
    }

    public virtual void ResetSelected(bool forceDeselect = true)
    {
        for (int i = 0; i < squares.Count; i++)
        {
            squares[i].ResetColor();
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

    public virtual bool TrySetupMove(PieceScript ps, int x, int y, int newX, int newY)
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

    public virtual bool IsSetupMoveLegal(PieceScript ps, uint move)
    {
        return Board.IsSetupMoveLegal(ref board, move);
    }

    public virtual bool TrySetupMove(PieceScript ps, uint move)
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

            ResetSelected(true);
            FixBoardBasedOnPosition();
            return true;
        }

        ResetSelected(true);
        FixBoardBasedOnPosition();
        return false;
    }

    public virtual void TryMove(PieceScript ps, Piece.PieceAlignment pa, int x, int y, int newX, int newY)
    {
        TrySetupMove(ps, x, y, newX, newY);
    }

    public virtual void FixBoardBasedOnPosition()
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
        for (int i = 0; i < pieces.Count; i++)
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


    public virtual void Update()
    {

    }

    public virtual bool CanSelectPieces()
    {
        return true;
    }
}
