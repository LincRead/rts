using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    // Pathfinding
    [HideInInspector]
    public List<Node> path = new List<Node>();
    protected Grid grid = null;
    [HideInInspector]
    public Node currentStandingOnNode;
    protected Node startNode;
    protected Node destinationNode;
    protected FActor parentReference; 

    // Use this for initialization
    void Start()
    {
        GameObject gridObj = GameObject.FindGameObjectWithTag("Grid");

        if (gridObj)
            grid = gridObj.GetComponent<Grid>();

        parentReference = transform.GetComponent<FActor>();
    }

    public Node DetectCurrentPathfindingNode(Vector3 pos)
    {
        Node node = grid.GetNodeFromWorldPoint(pos);

        // Outside grid
        if (node == null)
        {
            Debug.LogError(name + " is standing outside of the Grid");
            return null;
        }

        // Standing on another node than currently stored
        if (currentStandingOnNode != node)
        {
            // Clear the previous node this controller was standing on
            if (currentStandingOnNode != null)
            {
                RemoveStandingOnNode();
            }

            // Store the node this controller is currently standing on
            currentStandingOnNode = node;
            node.squadStandingHere = true;
            node.actorsStandingHere.Add(parentReference);
        }

        return node;
    }

    public Node GetNodeFromPoint(Vector3 pos)
    {
        return grid.GetNodeFromWorldPoint(pos);
    }

    public Node GetNodeFromGridPos(int x, int y)
    {
        return grid.GetNodeFromGridPos(x, y);
    }


    public List<Node> FindPath(Node endNode)
    {
        return FindPath(endNode.worldPosition);
    }

    public List<Node> FindPath(Vector2 endPos)
    {
        // Find start and destination nodes
        Node newDestinationNode = grid.GetNodeFromWorldPoint(endPos);

        if (newDestinationNode == null)
            return null;

        // Set new destination node
        if (newDestinationNode != destinationNode)
        {
            startNode = currentStandingOnNode;
            destinationNode = newDestinationNode;
        }

        else
            // Same as before, so don't have to find new path
            return null;

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == destinationNode)
            {
                return RetracePath(startNode, destinationNode);
            }

            List<Node> nodesToCheck = grid.GetNeighbours(currentNode);
            foreach (Node neighbour in nodesToCheck)
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistanceBetweenNodes(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistanceBetweenNodes(neighbour, destinationNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        grid.pathsToDebug.Remove(path);
        path.Clear();

        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.pathsToDebug.Add(path);

        return path;
    }

    int GetDistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridPosX - nodeB.gridPosX);
        int distY = Mathf.Abs(nodeA.gridPosY - nodeB.gridPosY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
    
    public void RemoveStandingOnNode()
    {
        currentStandingOnNode.actorsStandingHere.Remove(parentReference);

        if (currentStandingOnNode.actorsStandingHere.Count == 0)
            currentStandingOnNode.squadStandingHere = false;
    }
}
