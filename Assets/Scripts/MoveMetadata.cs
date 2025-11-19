
//I will keep this in a list data structure
//so a move from A to B will be in a list showing the complete path from A to B
//I can make this data structure set more inefficient because it isn't something the chess engine should work with
using System.Collections.Generic;
using UnityEngine;

//Index this with Dictionary<uint, MoveMetadata>
//The root of the tree should be the pieces original position

//only turn on serializable for path debug
//Unity inspector gets angry and laggy because there are cycles in the path data structures (because predecessor has pointer to successor, successor has pointer to predecessor)
//[System.Serializable]
public class MoveMetadata
{
    public uint piece;
    public int x;
    public int y;
    public PathType path;
    public Move.SpecialType moveSpecial;
    public List<MoveMetadata> predecessors;
    public List<MoveMetadata> successors;
    public bool terminalNode;
    public List<uint> pathTags;

    public enum PathType
    {
        Slider,
        Leaper,
        Teleport,

        SliderGiant,
        LeaperGiant,
        TeleportGiant,

        //can calculate these by seeing that the offset is too large
        //(for reflected it doesn't need anything because it is just a path turning in a different direction which just draws a normal line turn)
        /*
        SliderCylinder,
        SliderTubular,
        SliderReflected,
        LeaperCylinder,
        LeaperTubular,
        LeaperReflected,
        */

        //No teleport cylinder or teleport tubular etc because that doesn't really do anything
    }

    public MoveMetadata(uint piece, int tx, int ty, PathType path, Move.SpecialType moveSpecial, uint pathTag)
    {
        this.piece = piece;
        //this.fx = fx;
        //this.fy = fy;
        this.x = tx;
        this.y = ty;
        this.path = path;
        this.moveSpecial = moveSpecial;
        predecessors = new List<MoveMetadata>();
        successors = new List<MoveMetadata>();
        pathTags = new List<uint>();

        if (pathTag != 0)
        {
            pathTags.Add(pathTag);
        }
    }
    public MoveMetadata(uint piece, int tx, int ty, PathType path, Move.SpecialType moveSpecial, List<uint> pathTags)
    {
        this.piece = piece;
        //this.fx = fx;
        //this.fy = fy;
        this.x = tx;
        this.y = ty;
        this.path = path;
        this.moveSpecial = moveSpecial;
        predecessors = new List<MoveMetadata>();
        successors = new List<MoveMetadata>();
        this.pathTags = new List<uint>();

        //copy the list
        for (int i = 0; i < pathTags.Count; i++)
        {
            this.pathTags.Add(pathTags[i]);
        }
    }

    public static uint MakePathTag(MoveGeneratorInfoEntry.MoveGeneratorAtom mga, uint index)
    {
        return (((uint)mga << 16) + index);
    }
    public static uint MakePathTag(MoveGeneratorInfoEntry.MoveGeneratorAtom mga, uint index, uint indexB)
    {
        return (((uint)mga << 16) + (index << 8) + indexB);
    }

    public static string PathTagToString(uint pathTag)
    {
        return ((MoveGeneratorInfoEntry.MoveGeneratorAtom)(pathTag >> 16)) + " " + (pathTag & 0xffff);
    }

    public void AddSuccessor(MoveMetadata successor)
    {
        if (successors == null)
        {
            successors = new List<MoveMetadata>();
        }
        if (successor.predecessors == null)
        {
            successor.predecessors = new List<MoveMetadata>();
        }
        successors.Add(successor);
        successor.predecessors.Add(this);
    }
    public void AddPredecessor(MoveMetadata predecessor)
    {
        if (predecessors == null)
        {
            predecessors = new List<MoveMetadata>();
        }
        if (predecessor.successors == null)
        {
            predecessor.successors = new List<MoveMetadata>();
        }
        predecessors.Add(predecessor);
        predecessor.successors.Add(this);
    }
    //More useful: starts at the end and just goes back
    //If the paths are set up correctly they should not loop (Especially because there is some code to prevent overwriting)
    public List<MoveMetadata> TracePath(int startX, int startY, Move.Dir startDir = Move.Dir.Null)
    {
        MoveMetadata node = this;
        List<MoveMetadata> output = new List<MoveMetadata>();

        //Better version
        MoveMetadata subNode = node;

        output.Insert(0, subNode);
        /*
        int lastDeltaX = 0;
        int lastDeltaY = 0;
        //try to use Dir
        switch (startDir)
        {
            case Move.Dir.DownLeft:
                lastDeltaX = -1;
                lastDeltaY = -1;
                break;
            case Move.Dir.Down:
                lastDeltaX = 0;
                lastDeltaY = -1;
                break;
            case Move.Dir.DownRight:
                lastDeltaX = 1;
                lastDeltaY = -1;
                break;
            case Move.Dir.Left:
                lastDeltaX = -1;
                lastDeltaY = 0;
                break;
            case Move.Dir.Null:
                break;
            case Move.Dir.Right:
                lastDeltaX = 1;
                lastDeltaY = 0;
                break;
            case Move.Dir.UpLeft:
                lastDeltaX = -1;
                lastDeltaY = 1;
                break;
            case Move.Dir.Up:
                lastDeltaX = 0;
                lastDeltaY = 1;
                break;
            case Move.Dir.UpRight:
                lastDeltaX = 1;
                lastDeltaY = 1;
                break;
        }
        */

        if (node.pathTags.Count == 0)
        {
            Debug.LogError("Path with no tag");
            return output;
        }

        uint pathTag = node.pathTags[0];

        while (subNode != null)
        {
            MoveMetadata childToAdd = null;

            for (int i = 0; i < subNode.predecessors.Count; i++)
            {
                if (subNode.predecessors[i].pathTags.Contains(pathTag))
                {
                    //no backwards
                    if (output.Count > 1 && output[1].x == subNode.predecessors[i].x && output[1].y == subNode.predecessors[i].y)
                    {
                        continue;
                    }
                    childToAdd = subNode.predecessors[i];
                    break;
                }
            }

            if (childToAdd == null)
            {
                break;
            }

            subNode = childToAdd;

            //Infinite loop (some paths go in a circle)
            if (subNode.x == startX && subNode.y == startY)
            {
                break;
            }
            output.Insert(0, subNode);

            if (output.Count > 50)
            {
                Debug.LogError("Very long path");
                break;
            }
        }

        /*
        output.Insert(0, node);

        int lastDeltaX = 0;
        int lastDeltaY = 0;

        //build a path
        //Attempt to choose such that the deltas are the same (To fix path chatter? Otherwise the path might get confused if your piece can reach a square by multiple paths and display a path with characteristics of both paths)
        //Tryfixpath will fix crooked paths being wrong because of this code

        MoveMetadata subNode = node;
        while (subNode != null && subNode.predecessors != null && subNode.predecessors.Count > 0)
        {
            MoveMetadata childToAdd = subNode.predecessors[0];

            for (int i = 0; i < subNode.predecessors.Count; i++)
            {
                if (lastDeltaX == 0 && lastDeltaY == 0)
                {
                    //try to use Dir
                    switch (startDir)
                    {
                        case Move.Dir.DownLeft:
                            lastDeltaX = -1;
                            lastDeltaY = -1;
                            break;
                        case Move.Dir.Down:
                            lastDeltaX = 0;
                            lastDeltaY = -1;
                            break;
                        case Move.Dir.DownRight:
                            lastDeltaX = 1;
                            lastDeltaY = -1;
                            break;
                        case Move.Dir.Left:
                            lastDeltaX = -1;
                            lastDeltaY = 0;
                            break;
                        case Move.Dir.Null:
                            break;
                        case Move.Dir.Right:
                            lastDeltaX = 1;
                            lastDeltaY = 0;
                            break;
                        case Move.Dir.UpLeft:
                            lastDeltaX = -1;
                            lastDeltaY = 1;
                            break;
                        case Move.Dir.Up:
                            lastDeltaX = 0;
                            lastDeltaY = 1;
                            break;
                        case Move.Dir.UpRight:
                            lastDeltaX = 1;
                            lastDeltaY = 1;
                            break;
                    }
                }
                if (subNode.predecessors[i].x - subNode.x == lastDeltaX && subNode.predecessors[i].y - subNode.y == lastDeltaY)
                {
                    childToAdd = subNode.predecessors[i];
                }
            }

            //Try not to go backwards
            if (output.Count > 1 && output[1].x == childToAdd.x && output[1].y == childToAdd.y)
            {
                for (int i = 0; i < subNode.predecessors.Count; i++)
                {
                    if (subNode.predecessors[i].x != childToAdd.x || subNode.predecessors[i].y != childToAdd.y)
                    {
                        childToAdd = subNode.predecessors[i];
                        break;
                    }
                }
            }
            */

        //Debug.Log((childToAdd.x - subNode.x) + " " + (childToAdd.y - subNode.y) + " " + lastDeltaX + " " + lastDeltaY);

        /*
            lastDeltaX = childToAdd.x - subNode.x;
            lastDeltaY = childToAdd.y - subNode.y;
            subNode = childToAdd;

            //Infinite loop (some paths go in a circle)
            if (subNode.x == startX && subNode.y == startY)
            {
                break;
            }

            output.Insert(0, subNode);

            //escape?
            if (subNode.terminalNode)
            {
                //Debug.Log("Terminal node hit " + subNode.x + " " + subNode.y);
                break;
            }

            if (output.Count > 50)
            {
                Debug.LogError("Very long path");
                break;
            }
        }

        output = TryFixPath(startX, startY, output);
        */

        return output;
    }
    public bool IsDeltaCylindrical(int dx, int dy)
    {
        return (dx >= 4 || dx <= -4);
    }
    public bool IsDeltaTubular(int dx, int dy)
    {
        return (dy >= 4 || dy <= -4);
    }
    public MoveMetadata SubSearch(int targetX, int targetY, MoveMetadata parent, List<MoveMetadata> outputList)
    {
        MoveMetadata result = null;

        foreach (MoveMetadata child in parent.successors)
        {
            if (child.x == targetX && child.y == targetY)
            {
                return child;
            }
            result = SubSearch(targetX, targetY, parent, outputList);
            if (result != null)
            {
                outputList.Insert(0, result);
                return result;
            }
        }

        return null;
    }

    //Problem with above: double paths becoming inconsistent
    //          a
    //      x       x
    //          x
    //      x       x
    //          b
    //Path to A to B should exclusively go left or right, but the above code has no way to stop that
    //This can't be solved by search ordering as the direction change may be forced by a blocked path on one side
    //  (so a left bias would not work if the left path is blocked and the true path is actually to the right
    public List<MoveMetadata> TryFixPath(int startX, int startY, List<MoveMetadata> oldList)
    {
        //The correct side to take is determined by the ending path so iteration goes in reverse
        List<MoveMetadata> output = new List<MoveMetadata>();

        for (int i = oldList.Count - 1; i >= 0; i--)
        {
            bool didAdd = false;

            int pastX = 0;
            int pastY = 0;
            if (i == 0)
            {
                pastX = startX;
                pastY = startY;
            } else
            {
                pastX = oldList[i - 1].x;
                pastY = oldList[i - 1].y;
            }

            //if (i + 2 <= oldList.Count - 1)
            if (output.Count >= 2)
            {
                //old path
                //  ...     ...
                //  i+2       x
                //      i+1
                //  (i?)     (i?)
                //      i-1

                //Want to check delta between +2 and +1 vs 0 and -1 is the same

                //int dxA = (oldList[i + 2].x - oldList[i + 1].x);
                //int dyA = (oldList[i + 2].y - oldList[i + 1].y);
                int dxA = (output[1].x - output[0].x);
                int dyA = (output[1].y - output[0].y);

                MoveMetadata bestCandidate = null;

                foreach (MoveMetadata suc in oldList[i + 1].predecessors)
                {
                    if (i > 0 && !(oldList[i - 1].successors.Contains(suc)))
                    {
                        continue;
                    }

                    bestCandidate = suc;

                    int dxB = (oldList[i].x - pastX);
                    int dyB = (oldList[i].y - pastY);

                    //Debug.Log(Move.PositionToString(output[1].x, output[1].y) + " " + Move.PositionToString(output[0].x, output[0].y) + " " + Move.PositionToString(suc.x, suc.y) + "? " + (i <= 0 ? ("(start )" + Move.PositionToString(startX,startY)) : Move.PositionToString(oldList[i - 1].x, oldList[i - 1].y)));
                    //Debug.Log("Check " + dxA + " " + dyA + " vs " + dxB + " " + dyB);
                    if (dxA == dxB && dyA == dyB)
                    {
                        output.Insert(0, suc);
                        didAdd = true;
                        break;
                    }
                }

                if (!didAdd && bestCandidate != null)
                {
                    output.Insert(0, bestCandidate);
                    didAdd = true;
                }
            }

            if (didAdd)
            {
                continue;
            }

            //add it normally
            output.Insert(0, oldList[i]);
        }


        /*
        for (int i = oldList.Count - 1; i >= 0; i--)
        {
            //try to do a diamond check for adding i
            bool didAdd = false;


            //note: first square in the path is 0 which is 1 step beyond the start
            if (i >= 0 && i + 3 <= oldList.Count - 1)
            {
                //old path
                //  ...     ...
                //      i+3
                //  i+2       x
                //      i+1
                //  (i?)     (i?)
                //      i-1

                //Want to check delta between +2 and +1 vs 0 and -1 is the same

                int dxA = (oldList[i + 3].x - oldList[i + 2].x);
                int dyA = (oldList[i + 3].y - oldList[i + 2].y);

                MoveMetadata bestCandidate = null;

                foreach (MoveMetadata suc in oldList[i + 1].predecessors)
                {
                    if (i > 0 && !(oldList[i - 1].successors.Contains(suc)))
                    {
                        continue;
                    }

                    bestCandidate = suc;

                    int dxB = (oldList[i + 1].x - suc.x);
                    int dyB = (oldList[i + 1].y - suc.y);

                    Debug.Log(Move.PositionToString(oldList[i + 3].x, oldList[i + 3].y) + " " + Move.PositionToString(oldList[i + 2].x, oldList[i + 2].y) + " " + Move.PositionToString(oldList[i + 1].x, oldList[i + 1].y) + " " + Move.PositionToString(suc.x, suc.y) + "? " + (i <= 0 ? "(start)" : Move.PositionToString(oldList[i - 1].x, oldList[i - 1].y)));
                    Debug.Log("Check " + dxA + " " + dyA + " vs " + dxB + " " + dyB);
                    if (dxA == dxB && dyA == dyB)
                    {
                        output.Insert(0, suc);
                        didAdd = true;
                        break;
                    }
                }

                if (!didAdd && bestCandidate != null)
                {
                    output.Insert(0, bestCandidate);
                    didAdd = true;
                }
            }
            if (didAdd)
            {
                continue;
            }

            //add it normally
            output.Insert(0, oldList[i]);
        }

        //Run a different kind of iteration to fix position 0
        //  2       x
        //      1
        //  (0?)     (0?)
        //      (-1)

        if (output.Count > 2)
        {
            MoveMetadata bestCandidate0 = output[0];

            int dx2 = (output[2].x - output[1].x);
            int dy2 = (output[2].y - output[1].y);

            foreach (MoveMetadata suc in oldList[1].predecessors)
            {
                bestCandidate0 = suc;

                int dx0 = (suc.x - startX);
                int dy0 = (suc.y - startY);

                Debug.Log(Move.PositionToString(output[2].x, output[2].y) + " " + Move.PositionToString(output[1].x, output[1].y) + " " + Move.PositionToString(suc.x, suc.y) + "? " + Move.PositionToString(startX, startY) + " (true start)");
                Debug.Log("Check final " + dx2 + " " + dy2 + " vs " + dx0 + " " + dy0);
                if (dx2 == dx0 && dy2 == dy0)
                {
                    bestCandidate0 = suc;
                    break;
                }
            }

            output[0] = bestCandidate0;
        }
        */

        return output;
    }
}

//Another inefficient data structure
//Generated by board.ApplyMove
//This is after the move metadata path is used so there is no move metadata here
//This refers to anything that happens to other pieces that aren't the moved piece (so the specific special types of moves are handled elsewhere)
public class BoardUpdateMetadata
{
    public int fx;
    public int fy;
    public int tx;
    public int ty;
    //public Piece.PieceType pieceType;  //for type change stuff (promotion or other things)
    public uint piece;  //piece in question (for spawn, type / alignment change)
    public BoardUpdateType type;
    public bool wasLastMovedPiece;  //to distinguish last moved from thing that was on the square before

    public enum BoardUpdateType
    {
        Move,
        Capture,
        //Fire,     //Current setup doesn't let me do this yet (later I might add particles for each move so I add more enum values)
        Spawn,
        Shift,
        TypeChange,  //promotion / lycanthrope conversion
        AlignmentChange,
        ImbueModifier,
        StatusCure,
        StatusApply,
    }

    public BoardUpdateMetadata(int fx, int fy, int tx, int ty, uint piece, BoardUpdateType type, bool wasLastMoved)
    {
        this.fx = fx;
        this.fy = fy;
        this.tx = tx;
        this.ty = ty;
        this.piece = piece;
        this.type = type;
        this.wasLastMovedPiece = wasLastMoved;
    }
    public BoardUpdateMetadata(int fx, int fy, int tx, int ty, uint piece, BoardUpdateType type)
    {
        this.fx = fx;
        this.fy = fy;
        this.tx = tx;
        this.ty = ty;
        this.piece = piece;
        this.type = type;
    }

    public BoardUpdateMetadata(int fx, int fy, uint piece, BoardUpdateType type)
    {
        this.fx = fx;
        this.fy = fy;
        this.tx = -1;
        this.ty = -1;
        this.piece = piece;
        this.type = type;
    }
    public BoardUpdateMetadata(int fx, int fy, uint piece, BoardUpdateType type, bool wasLastMoved)
    {
        this.fx = fx;
        this.fy = fy;
        this.tx = -1;
        this.ty = -1;
        this.piece = piece;
        this.type = type;
        this.wasLastMovedPiece = wasLastMoved;
    }
}