using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{

    public Transform seeker;
    public Transform target;
    Grid grid;
    
    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }


    void FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        // While there are still nodes to check
        while (openSet.Count > 0)
        {
            // Get the node with the lowest fCost
            Node currentNode = openSet[0];
            // Loop through the open set to find the node with the lowest fCost
            for (int i = 1; i < openSet.Count; i++)
            {
                // If the fCost of the current node is lower than the fCost of the node we are checking
                if(openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost)
                {
                    // If the hCost of the current node is lower than the hCost of the node we are checking
                    if(openSet[i].hCost < currentNode.hCost)
                    {
                        // Set the current node to the node we are checking
                        currentNode = openSet[i];
                    }
                }
            }
            openSet.Remove(currentNode);
            // We are doing this because we don't want to check the same node twice
            closedSet.Add(currentNode);
            
            
            if(currentNode == targetNode)
            {
                // We found the path
                RetracePath(startNode, targetNode);
                return;
            }
            
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                // If the neighbour is not walkable or if the neighbour is in the closed set
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    // we need to set the fcost, so to do that we calc gcost and hcost
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    
                    // If the neighbour is not in the open set
                    if(!openSet.Contains(neighbour))
                    {
                        // Add the neighbour to the open set
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (distanceX > distanceY)
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        else
        {
            // If the distance in the y direction is greater than the distance in the x direction
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }
    
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        // We need to retrace the path from the end node to the start node
        Node currentNode = endNode;
        // While we haven't reached the start node
        while (currentNode != startNode)
        {
            // Add the current node to the path
            path.Add(currentNode);
            // Set the current node to the parent of the current node
            currentNode = currentNode.parent;
        }
        
        // We need to reverse the path because we are currently going from the end node to the start node
        path.Reverse();
        
        grid.path = path;
    }
}
