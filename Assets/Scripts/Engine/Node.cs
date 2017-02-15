using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{
    public Node parent;
    public Vector2 worldPosition;
    public int gridPosX;
    public int gridPosY;
    int heapIndex;

    // Movement cost from the start point to this node
    public int gCost = 0;

    // Estimated movement cost from this node to target node
    public int hCost = 0;

    public bool walkable;
    public bool squadStandingHere = false;

    public GameObject tilePrefab;

    public Node(bool _walkable, Vector2 _worldPosition, int _gridPosX, int _gridPosY)
    {
        // Perfect rounding
        decimal dPosX = System.Math.Round((decimal)_worldPosition.x, 2);
        decimal dPosY = System.Math.Round((decimal)_worldPosition.y, 2);

        walkable = _walkable;
        worldPosition = new Vector2((float)dPosX, (float)dPosY);
        gridPosX = _gridPosX;
        gridPosY = _gridPosY;

        CreateTile();
    }

    public void CreateTile()
    {
        GameObject.Instantiate(Resources.Load("Tiles/Grass Tile"), worldPosition, Quaternion.identity);
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}