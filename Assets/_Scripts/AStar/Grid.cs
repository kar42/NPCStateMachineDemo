using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Mask for unwalkable areas")]
    public LayerMask unwalkableMask;
    [Header("Size of Grid")]
    public Vector2 gridWorldSize;
    [Header("Radius of each node in the grid. (Start game with Gizmos on to see the grid.)")]
    // Radius of each node in the grid
    public float nodeRadius;
    [Header("Player Object")]
    public Transform player;
    
    [Header("Player Visibility")]
    public float playerVisibility = 79f;
    
    public float offsetX;
    public float offsetY;
    
    // 2D grid
    Node[,] grid;

    private float nodeDiameter;
    // Number of nodes in the X and Y direction
    private int gridSizeX, gridSizeY;

    public List<Node> path;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position - new Vector2(gridWorldSize.x / 2, gridWorldSize.y / 2);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + new Vector2(x * nodeDiameter + nodeRadius, y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }
    
    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        
        float percentX = (worldPosition.x +offsetX+ gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y +offsetY+ gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // Debugging logs
        /*Debug.Log($"World Position: {worldPosition}");
        Debug.Log($"Percent X: {percentX}, Percent Y: {percentY}");
        Debug.Log($"Player Position A* Grid X: {x}, Grid Y: {y}");*/

        return grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x==0 && y==0)
                {
                    // We dont want to check the node itself
                    continue;
                }
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                // Check if the node is within the grid
                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    // Add the node to the neighbours list
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }

        return neighbours;
    }

    private void OnDrawGizmos()
    {
        // Draw the boundaries of the grid in the scene
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null)
        {
            Node playerNode = NodeFromWorldPoint(player.position);

            foreach (Node n in grid)
            {
                // Determine colors for nodes
                Color transparentWhite = new Color(1f, 1f, 1f, 0.3f);
                Color transparentRed = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.color = (n.walkable) ? transparentWhite : transparentRed;

                // Check if the node is within the player's visibility radius
                if (Vector2.Distance(n.worldPosition, playerNode.worldPosition) <= playerVisibility)
                {
                    //Highlighting the player
                    Gizmos.color = Color.green;
                }

                if (path != null)
                {
                    if(path.Contains(n))
                        Gizmos.color = Color.black;
                }

                float size = nodeDiameter - 0.2f; 
                Gizmos.DrawCube(n.worldPosition, new Vector2(size, size));
            }
        }
    }



}