﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : Boid, LockStep {

    [Header("Unit type")]
    public GameObject unitPrefab;

    [HideInInspector]
    public int faceDir = 1;

    public int numStartingUnits = 10;

    FPoint FPosLast;
    FPoint FAverageUnitFVelocity;

    public enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    protected SQUAD_STATES state = SQUAD_STATES.IDLE;

    [Header("Unit stats")]
    public int unitMaxHitpoints = 100;
    public int unitAttackDamage = 10;
    public float unitMoveSpeed = 0.4f;
    List<Unit> units = new List<Unit>(30);

    // Find closest target and set as leader of squad
    [HideInInspector]
    public Unit leader;

    FInt closestDistToEnemyUnit;
    FInt minDistClosestUnitToTarget = FInt.FromParts(0, 320);

    GameController gameController;
    Gold gold;

    [Header("References")]
    public HQ hq;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        spriteRenderer.enabled = false;

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gold = GetComponent<Gold>();

        InitUnits(numStartingUnits);
        FindNewLeader();

        for (int i = 0; i < units.Count; i++)
            units[i].currentState = Unit.UNIT_STATES.IDLE;
    }

    void InitUnits(int num)
    {
        for (var i = 0; i < num; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + (i % 4) * 0.45f, transform.position.y + (i % 5) * 0.45f);
            GameObject newUnit = Instantiate(unitPrefab, pos, Quaternion.identity) as GameObject;
            newUnit.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
            AddUnit(newUnit.GetComponent<Unit>());
            newUnit.GetComponent<Unit>().CancelMergingWithSquad(); // Initial units are already merged with squad
        }
    }

    void Update ()
    {
        if (!gameController.gameReady)
            return;

        if (playerID == gameController.playerID 
            && (Input.GetMouseButtonUp(0) && gameController.IsValidSquadInput()))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Node node = pathFinding.GetNodeFromPoint(mousePosition);

            // Clicked on an obstcale
            // Find a node closest to obstacle that is walkable
            if (!node.walkable)
                node = FindClosestWalkableNode(node);

            if (node != null && node.walkable)
            {
                // Store so we can revert by end of loop
                FPosLast = Fpos;

                Fpos = node._FworldPosition;
                CalculateClosestUnitToNode();

                // Check if any units on target node belongs to an enemy unit
                bool enemyIsStandingOnNode = false;

                List<Node> nodesToCheck = new List<Node>(); // = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>().GetNeighbours(node);
                nodesToCheck.Add(node);

                for (int i = 0; i < nodesToCheck.Count; i++)
                {
                    for (int j = 0; j < nodesToCheck[i].actorsStandingHere.Count; j++)
                    {
                        if (nodesToCheck[i].actorsStandingHere[j].playerID != gameController.playerID)
                        {
                            enemyIsStandingOnNode = true;
                            break;
                        }
                    }
                }

                // TODO check radius instead when BSP is implemented
                // Maybe same algorithm as FindNewTargetEnemy in Unit.cs
                // Show type of action activated
                ClickIndicator cind = GameObject.FindGameObjectWithTag("ClickIndicator").GetComponent<ClickIndicator>();
                if (enemyIsStandingOnNode)
                    cind.ActivateAttack(mousePosition);
                else
                    cind.ActivateMoveSprite(mousePosition);

                if (closestDistToEnemyUnit >= minDistClosestUnitToTarget || enemyIsStandingOnNode)
                {
                    if (gameController.IsMultiplayer())
                    {
                        gameController.SetNextCommand(0, node.gridPosX, node.gridPosY);

                        // Don't change before Server serves the command
                        Fpos = FPosLast;
                    }

                    else
                    {
                        MoveToTarget(node.gridPosX, node.gridPosY);
                    }
                }

                else
                {
                    Fpos = FPosLast;
                }
            }
        }

        UpdateSquadDirection();
        transform.position = GetRealPosToVector3();
    }

    Node FindClosestWalkableNode(Node node)
    {
        List <Node> nodes = pathFinding.GetGrid().GetNeighbours(node);
        Node closestNode = null;

        FInt shortestDistance = FlargeNumber;
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].walkable)
            {
                FInt dist = GetDistanceBetweenPoints(node._FworldPosition, nodes[i]._FworldPosition);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    closestNode = nodes[i];
                }
            }
        }

        return closestNode;
    }

    public void MoveToTarget(int x, int y)
    {
        Node newTargetNode = pathFinding.GetNodeFromGridPos(x, y);
        Fpos = newTargetNode._FworldPosition;

        FindNewLeader();

        state = SQUAD_STATES.MOVE_TO_TARGET;

        for (int i = 0; i < units.Count; i++)
            units[i].MoveToTarget();
    }

    void UpdateSquadDirection()
    {
        if (units.Count < 1)
            return;

        FAverageUnitFVelocity = FPoint.Create();
        int numUnitsCounted = 0;
        for (int i = 0; i < units.Count; i++)
        {
            if(units[i] == leader)
            {
                FAverageUnitFVelocity.X += units[i].GetFVelocity().X * 2;
                FAverageUnitFVelocity.Y += units[i].GetFVelocity().Y * 2;
                numUnitsCounted++;
            }

            else if(!units[i].IsMergingWithSquad())
            {
                FAverageUnitFVelocity.X += units[i].GetFVelocity().X;
                FAverageUnitFVelocity.Y += units[i].GetFVelocity().Y;
                numUnitsCounted++;
            }
        }

        if (numUnitsCounted == 0)
            return;

        FAverageUnitFVelocity.X = FAverageUnitFVelocity.X / numUnitsCounted;
        FAverageUnitFVelocity.Y = FAverageUnitFVelocity.Y / numUnitsCounted;

        if (FAverageUnitFVelocity.X > FInt.FromParts(0, 100))
            faceDir = 1;
        if (FAverageUnitFVelocity.X < FInt.FromParts(0, 100) * -1)
            faceDir = -1;
    }

    public void FindNewLeader()
    {
        // Tell old leader his or her leadership is over
        if (leader != null)
            leader.GetComponent<Unit>().isLeader = false;

        // Set leader
        leader = CalculateClosestUnitToNode();

        // Let new leader know he is the leader
        if(leader != null)
            leader.isLeader = true;
    }

    Unit CalculateClosestUnitToNode()
    {
        Unit closestUnit = null;
        closestDistToEnemyUnit = FInt.Create(1000);

        // Find unit closest to target
        for (int i = 0; i < units.Count; i++)
        {
            FInt dist = GetDistanceToFActor(units[i]);
            if (dist < closestDistToEnemyUnit)
            {
                closestDistToEnemyUnit = dist;

                // Don't pick units who are not yet merged with squad as leader
                if(!units[i].IsMergingWithSquad() || leader == null)
                {
                    closestUnit = units[i];
                    closestUnit.CancelMergingWithSquad();
                }
            }
        }

        return closestUnit;
    }

    public override void LockStepUpdate()
    {
        for (int i = 0; i < units.Count; i++)
            units[i].GetComponent<FActor>().LockStepUpdate();

        gold.LockstepUpdate();
    }

    public void AddUnit(Unit newUnit)
    {
        units.Add(newUnit);
        Unit unitScript = newUnit.GetComponent<Unit>();
        unitScript.playerID = playerID;
        unitScript.SetSquad(this);
    }

    public int GetMergingUnits()
    {
        int numMergingUnits = 0;

        for(int i = 0; i < units.Count; i++)
        {
            if (units[i].IsMergingWithSquad())
                numMergingUnits++;
        }

        return numMergingUnits;
    }

    public void RemoveUnit(Unit unitToRemove) { units.Remove(unitToRemove); }

    public int GetSquadSize() { return units.Count; }

    public int GetGold() { return gold.GetAmount();  }

    public List<Unit> GetUnits() { return units;  }

    public HQ GetHQ()
    {
        return hq;
    }
}
