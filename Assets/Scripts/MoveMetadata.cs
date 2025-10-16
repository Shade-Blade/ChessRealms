
//I will keep this in a list data structure
//so a move from A to B will be in a list showing the complete path from A to B
//I can make this data structure set more inefficient because it isn't something the chess engine should work with
using System.Collections.Generic;

//Index this with Dictionary<uint, MoveMetadata>
//The root of the tree should be the pieces original position

public class MoveMetadata
{
    public uint piece;
    public int fx;
    public int fy;
    public int tx;
    public int ty;
    public PathType path;
    public Move.SpecialType moveSpecial;
    public List<MoveMetadata> predecessors;
    public List<MoveMetadata> successors;

    public enum PathType
    {
        Slider,
        Leaper,
        Teleport,

        SliderCylinder,
        SliderTubular,
        SliderReflected,
        LeaperCylinder,
        LeaperTubular,
        LeaperReflected,
        //No teleport cylinder or teleport tubular etc because that doesn't really do anything
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
            successor.predecessors.Add(this);
        }
    }

    public List<MoveMetadata> SearchForPath(int targetX, int targetY, MoveMetadata treeBase)
    {
        //Fail to search: return null
        List<MoveMetadata> output = new List<MoveMetadata>();

        SubSearch(targetX, targetY, treeBase, output);

        if (output.Count == 0)
        {
            return null;
        }

        //sub search doesn't include the first one so add it here later
        output.Insert(0, treeBase);

        //fix it
        output = TryFixPath(targetX, targetY, output);

        return output;
    }
    public MoveMetadata SubSearch(int targetX, int targetY, MoveMetadata parent, List<MoveMetadata> outputList)
    {
        MoveMetadata result = null;

        foreach (MoveMetadata child in parent.successors)
        {
            if (child.tx == targetX && child.ty == targetY)
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
    public List<MoveMetadata> TryFixPath(int targetX, int targetY, List<MoveMetadata> oldList)
    {
        //The correct side to take is determined by the ending path so iteration goes in reverse
        List<MoveMetadata> output = new List<MoveMetadata>();

        for (int i = oldList.Count - 1; i >= 0; i--)
        {
            //try to do a diamond check for adding i
            //A different path from 
            bool didAdd = false;
            if (i - 1 >= 0 && i + 2 <= oldList.Count - 1)
            {
                //old path
                //  ...     ...
                //      i+3
                //  i+2       x
                //      i+1
                //  (i?)     (i?)
                //      i-1

                //Want to check delta between +2 and +1 vs 0 and -1 is the same

                int dxA = (oldList[i + 2].tx - oldList[i + 2].fx);
                int dyA = (oldList[i + 2].ty - oldList[i + 2].fy);

                MoveMetadata bestCandidate = null;

                foreach (MoveMetadata suc in oldList[i + 1].predecessors)
                {
                    if (!(oldList[i - 1].successors.Contains(suc)))
                    {
                        continue;
                    }

                    bestCandidate = this;

                    int dxB = (suc.tx - suc.fx);
                    int dyB = (suc.ty - suc.fy);

                    if (dxA == dxB && dyA == dyB)
                    {
                        output.Insert(0, oldList[i]);
                        didAdd = true;
                        break;
                    }
                }

                if (!didAdd)
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

        return output;
    }
}