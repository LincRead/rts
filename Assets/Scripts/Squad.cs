using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : MonoBehaviour, LockStep {

    private int playerIndex = -1;

    protected enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    List<GameObject> units = new List<GameObject>();

    // Add Circle Collider as look sense

    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public float unitMoveSpeed = 1f;

    // Pathfinding
    [HideInInspector]
    public List<Node> path = new List<Node>();
    protected Grid grid = null;
    protected Node currentStandingOnNode;
    protected Node startNode;
    protected Node destinationNode;

    void Start ()
    {
        SetupPathfinding();
    }

    void SetupPathfinding()
    {
        GameObject gridObj = GameObject.FindGameObjectWithTag("Grid");

        if (gridObj)
            grid = gridObj.GetComponent<Grid>();
    }

    void Update ()
    {
        DetectCurrentPathfindingNode();

        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            FindPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public void LockStepUpdate()
    {
        foreach (GameObject unit in units)
            unit.GetComponent<Unit>().LockStepUpdate();
    }

    public void AddUnit(GameObject newUnit)
    {
        units.Add(newUnit);
    }

    public void RemoveUnit(GameObject unitToRemove)
    {
        units.Remove(unitToRemove);
    }

    void DetectCurrentPathfindingNode()
    {
        Node node = grid.GetNodeFromWorldPoint(transform.position);

        // Outside grid
        if (node == null)
        {
            Debug.LogError(name + " is standing outside grid");
            return;
        }

        // Standing on another node than currently stored
        if (currentStandingOnNode != node)
        {
            // Clear the previous node this controller was standing on
            if (currentStandingOnNode != null)
            {
                currentStandingOnNode.squadStandingHere = false;
            }

            // Store the node this controller is currently standing on
            currentStandingOnNode = node;
            node.squadStandingHere = true;
        }
    }

    protected void FindPath(Vector2 endPos)
    {
        // Find start and destination nodes
        Node newDestinationNode = grid.GetNodeFromWorldPoint(endPos);

        if (newDestinationNode == null)
            return;

        // Set new destination node
        if (newDestinationNode != destinationNode)
        {
            startNode = currentStandingOnNode;
            destinationNode = newDestinationNode;
        }

        else
            // Same as before, so don't have to find new path
            return;

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == destinationNode)
            {
                RetracePath(startNode, destinationNode);
                return;
            }

            List<Node> nodesToCheck = grid.GetNeighbours(currentNode);
            foreach (Node neighbour in nodesToCheck)
            {
                if (closedSet.Contains(neighbour))
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
    }

    void RetracePath(Node startNode, Node endNode)
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
    }

    int GetDistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridPosX - nodeB.gridPosX);
        int distY = Mathf.Abs(nodeA.gridPosY - nodeB.gridPosY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    public int GetSquadSize()
    {
        return units.Count;
    }
}
