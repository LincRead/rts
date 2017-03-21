using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : IHeapItem<Node>
{
    public Node parent;
    public Vector2 worldPosition;
    public FPoint _FworldPosition;
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
    private Grid grid;

    [HideInInspector]
    public List<FActor> actorsStandingHere = new List<FActor>();

    public Node(bool _walkable, Vector2 _worldPosition, int _gridPosX, int _gridPosY, Grid grid)
    {
        walkable = _walkable;

        // Perfect rounding
        decimal dPosX = System.Math.Round((decimal)_worldPosition.x, 2);
        decimal dPosY = System.Math.Round((decimal)_worldPosition.y, 2);

        worldPosition = new Vector2((float)dPosX, (float)dPosY);
        FInt Fx = FInt.FromParts((int)dPosX, (int)((dPosX - (int)dPosX) * 1000));
        FInt Fy = FInt.FromParts((int)dPosY, (int)((dPosY - (int)dPosY) * 1000));
        _FworldPosition = FPoint.Create(Fx, Fy);

        gridPosX = _gridPosX;
        gridPosY = _gridPosY;

        this.grid = grid;
    }

    public void CreateTile(Sprite sprite)
    {
        GameObject node = GameObject.Instantiate(Resources.Load("Tiles/Grass"), worldPosition, Quaternion.identity) as GameObject;
        node.GetComponent<SpriteRenderer>().sprite = sprite;
        node.transform.SetParent(grid.transform);
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