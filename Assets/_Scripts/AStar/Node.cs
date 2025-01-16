using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    
    // cost of moving to this node from the start node
    public int gCost;
    // cost of moving to the target node from this node
    public int hCost;
    // fcost represents a best guess at how good a route through that node might be
    public int fCost => gCost + hCost;
    
    public int gridX;
    public int gridY;
    
    public Node parent;

    public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Node otherNode)
        {
            return this.worldPosition == otherNode.worldPosition && this.walkable == otherNode.walkable;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return worldPosition.GetHashCode() ^ walkable.GetHashCode();
    }
}
