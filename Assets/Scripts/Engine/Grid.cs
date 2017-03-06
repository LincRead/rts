using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;

    [Header("Size")]
    public int gridSizeX;
    public int gridSizeY;
    public float nodeRadius;
    private float nodeDiameter;
    public FInt FminX; // Units can't move beyond this point
    public FInt FmaxX; // Units can't move beyond this point
    public FInt FminY; // Units can't move beyond this point
    public FInt FmaxY; // Units can't move beyond this point

    // The actual size in pixels
    private Vector2 gridWorldSize;

    [Header("Debugging")]
    public bool debugPathsFound = true;
    public Color partOfPathDebugColor = Color.blue;
    public float debugNodeTypeAlpha = 0.4f;
    [HideInInspector]
    public List<List<Node>> pathsToDebug = new List<List<Node>>();

    private Node[,] nodes;

    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridWorldSize.x = gridSizeX * nodeDiameter;
        gridWorldSize.y = gridSizeY * nodeDiameter;
        FminX = FInt.FromFloat(-(gridSizeX / 2) * nodeDiameter);
        FmaxX = FInt.FromFloat((gridSizeX / 2) * nodeDiameter);
        FminY = FInt.FromFloat(-(gridSizeY / 2) * nodeDiameter);
        FmaxY = FInt.FromFloat((gridSizeY / 2) * nodeDiameter);
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        nodes = new Node[gridSizeX, gridSizeY];

        // Need to calculate this since pivot point is in center
        Vector3 gridWorldBottomLeftPos = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Calculate offset for each node
                Vector3 worldPoint = gridWorldBottomLeftPos + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

                bool walkable = true;

                if (Physics2D.OverlapArea(
                    new Vector2(worldPoint.x - nodeRadius + 1, worldPoint.y + nodeRadius - 1),
                    new Vector2(worldPoint.x + nodeRadius - 1, worldPoint.y - nodeRadius + 1),
                    unwalkableMask))
                    walkable = false;

                nodes[x, y] = new Node(walkable, worldPoint, x, y, this);

            }
        }
    }

    void Update()
    {

    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Current node
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridPosX + x;
                int checkY = node.gridPosY + y;

                if (checkX > -1 && checkX < gridSizeX && checkY > -1 && checkY < gridSizeY)
                    neighbours.Add(nodes[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public Node GetNodeFromWorldPoint(Vector2 worldPoint)
    {
        /*
        * Within boundaries of Grid.
        * Grid might not cover entire level in test cases.
        */
        if (worldPoint.x > -(gridWorldSize.x / 2)
            && worldPoint.x < (gridWorldSize.x / 2)
            && worldPoint.y > -(gridWorldSize.y / 2)
            && worldPoint.y < (gridWorldSize.y / 2))
        {
            float posXGridPos = worldPoint.x - (transform.position.x - (gridWorldSize.x / 2));
            float posYGridPos = worldPoint.y - (transform.position.y - (gridWorldSize.y / 2));
            int nodeX = (int)Mathf.Floor(posXGridPos / nodeDiameter);
            int nodeY = (int)Mathf.Floor(posYGridPos / nodeDiameter);

            if (nodeX > -1 && nodeY > -1 && nodeX < gridSizeX && nodeY < gridSizeY)
            {
                return nodes[nodeX, nodeY];
            }    
        }

        return null;
    }

    public Node GetNodeFromGridPos(int gridPosX, int gridPosY)
    {
        if (gridPosX < 0 || gridPosX > gridSizeX - 1 || gridPosY < 0 || gridPosY > gridSizeY - 1)
        {
            Debug.LogError("Trying to get a Node outside grid boundaries");
            return null;
        }

        return nodes[gridPosX, gridPosY];
    }

    /*
     * Debug grid size in Gizmo.
     * Debug which tiles are walkable and not.
     * Need to run game to see nodes in Editor Scene.
     * Grid gets created at Start().
     */
    void OnDrawGizmos()
    {
        // Show grid size
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1.0f));

        // Grid gets created at Start()
        if (nodes != null)
        {
            foreach (Node n in nodes)
            {
                if (n.squadStandingHere)
                    Gizmos.color = new Color(1.0f, 0.0f, 1.0f, 0.5f);
                else if(!n.walkable)
                    Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
                else
                    Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter / 2));
            }
        }

        if (pathsToDebug.Count > 0)
        {
            foreach (List<Node> path in pathsToDebug)
            {
                Gizmos.color = Color.blue;

                foreach (Node n in path)
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter / 3));
            }
        }
    }
}
