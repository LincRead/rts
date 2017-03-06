﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : Boid, LockStep {

    [Header("Unit types")]
    public GameObject unitPrefab;

    [HideInInspector]
    public int faceDir = 1;

    FPoint FPosLast;
    FPoint FAverageUnitFVelocity;

    public enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    public SQUAD_STATES state = SQUAD_STATES.IDLE;

    [Header("Units")]
    public int unitMaxHitpoints = 100;
    public int unitAttackDamage = 10;
    public FInt unitMoveSpeed = FInt.FromParts(0, 400);
    public FInt unitHealthRegenerateSpeed = FInt.FromParts(0, 50);
    List<Unit> units = new List<Unit>(30);

    // Find closest target and set as leader of squad
    [HideInInspector]
    public Unit leader;
    FInt closetDistUnitToTarget;
    FInt minDistClosestUnitToTarget = FInt.FromParts(0, 320);

    GameController gameController;

    protected override void Awake()
    {
        base.Awake();

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    protected override void Start()
    {
        base.Start();

        InitUnits(1);
    }

    void InitUnits(int num)
    {
        for (var i = 0; i < num; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + (i % 6) * 0.2f, transform.position.y + (i % 2) * 0.3f);
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

        if (playerID == gameController.playerID && Input.GetMouseButtonDown(0))
        {
            // Store so we can revert by end of loop
            FPosLast = Fpos;

            Node node = pathFinding.GetNodeFromPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Fpos = node._FworldPosition;
            CalculateClosestUnitToNode();

            if (closetDistUnitToTarget >= minDistClosestUnitToTarget)
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

        UpdateSquadDirection();
        transform.position = GetRealPosToVector3();
    }

    void MoveToTarget(int x, int y)
    {
        MoveToNode(x, y);
    }

    void UpdateSquadDirection()
    {
        if (units.Count < 1)
            return;

        FAverageUnitFVelocity = FPoint.Create();
        for (int i = 0; i < units.Count; i++)
        {
            FAverageUnitFVelocity.X += units[i].GetFVelocity().X;
            FAverageUnitFVelocity.Y += units[i].GetFVelocity().Y;
        }

        FAverageUnitFVelocity.X = FAverageUnitFVelocity.X / units.Count;
        FAverageUnitFVelocity.Y = FAverageUnitFVelocity.Y / units.Count;

        if (FAverageUnitFVelocity.X > FInt.FromParts(0, 100))
            faceDir = 1;
        if (FAverageUnitFVelocity.X < FInt.FromParts(0, 100) * -1)
            faceDir = -1;
    }

    public void MoveToNode(int x, int y)
    {
        Node newTargetNode = pathFinding.GetNodeFromGridPos(x, y);
        Fpos = newTargetNode._FworldPosition;
        state = SQUAD_STATES.MOVE_TO_TARGET;

        for (int i = 0; i < units.Count; i++)
            units[i].MoveToTarget();
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
        closetDistUnitToTarget = FInt.Create(1000);

        // Find unit closest to target
        for (int i = 0; i < units.Count; i++)
        {
            FInt dist = FindDistanceToUnit(units[i]);
            if (dist < closetDistUnitToTarget)
            {
                closetDistUnitToTarget = dist;

                // Don't pick units who are not yet merged with squad as leader
                if(!units[i].IsMergingWithSquad())
                    closestUnit = units[i];
            }
        }

        return closestUnit;
    }

    public override void LockStepUpdate()
    {
        FindNewLeader();

        for (int i = 0; i < units.Count; i++)
            units[i].GetComponent<FActor>().LockStepUpdate();
    }

    public void AddUnit(Unit newUnit)
    {
        units.Add(newUnit);
        Unit unitScript = newUnit.GetComponent<Unit>();
        unitScript.SetSquad(this);
        unitScript.playerID = playerID;
    }

    public void RemoveUnit(Unit unitToRemove) { units.Remove(unitToRemove); }

    FInt FindDistanceToUnit(FActor unit)
    {
        return ((unit.GetFPosition().X - Fpos.X) * (unit.GetFPosition().X - Fpos.X)) + ((unit.GetFPosition().Y - Fpos.Y) * (unit.GetFPosition().Y - Fpos.Y));
    }

    public int GetSquadSize() { return units.Count; }

    public List<Unit> GetUnits() { return units;  }
}
