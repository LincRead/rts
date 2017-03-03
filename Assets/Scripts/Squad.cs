using UnityEngine;
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
    public int unitMaxHitpoints = 10;
    public int unitAttackDamage = 1;
    public FInt unitMoveSpeed = FInt.FromParts(0, 400);
    List<Unit> units = new List<Unit>(30);

    // Find closest target and set as leader of squad
    [HideInInspector]
    public Unit leader;
    FInt closetDistUnitToTarget;
    FInt minDistClosestUnitToTarget = FInt.FromParts(0, 320);

    protected override void Start()
    {
        base.Start();

        InitUnits(30);
    }

    void InitUnits(int num)
    {
        for (var i = 0; i < num; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + (i % 6) * 0.5f, transform.position.y + (i % 3) * 0.5f);
            GameObject newUnit = Instantiate(unitPrefab, pos, Quaternion.identity) as GameObject;
            newUnit.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
            AddUnit(newUnit.GetComponent<Unit>());
        }
    }

    void Update ()
    {
        FPosLast = Fpos;

        // Todo: put all commands in a chunk within a tick to send over network
        if (playerID == 0 && Input.GetMouseButtonDown(0))
            if (!SetNewMoveToTarget())
                return;

        FindNewLeader();
        UpdateSquadDirection();
        transform.position = GetRealPosToVector3();
    }

    void UpdateSquadDirection()
    {
        FAverageUnitFVelocity = FPoint.Create();
        for (int i = 0; i < units.Count; i++)
        {
            FAverageUnitFVelocity.X += units[i].GetFVelocity().X;
            FAverageUnitFVelocity.Y += units[i].GetFVelocity().Y;
        }

        FAverageUnitFVelocity.X = FAverageUnitFVelocity.X / units.Count;
        FAverageUnitFVelocity.Y = FAverageUnitFVelocity.Y / units.Count;

        if (FAverageUnitFVelocity.X > FInt.FromParts(0, 50))
            faceDir = 1;
        if (FAverageUnitFVelocity.X < FInt.FromParts(0, 50) * -1)
            faceDir = -1;
    }

    bool SetNewMoveToTarget()
    {
        Node newTargetNode = pathFinding.GetNodeFromPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (newTargetNode == null)
            return false;

        Fpos = newTargetNode._FworldPosition;

        FindNewLeader();

        if (closetDistUnitToTarget >= minDistClosestUnitToTarget)
        {
            state = SQUAD_STATES.MOVE_TO_TARGET;

            for (int i = 0; i < units.Count; i++)
                units[i].MoveToTarget();

            // Success
            return true;
        }

        else
        {
            // Don't change target pos for units
            Fpos = FPosLast;

            // Failed to set new target
            return false;
        }
    }

    public void FindNewLeader()
    {
        Unit newLeader = null;

        // Tell old leader his or her leadership is over
        if (leader != null)
            leader.GetComponent<Unit>().isLeader = false;

        closetDistUnitToTarget = FInt.Create(1000);

        // Find unit closest to target
        for (int i = 0; i < units.Count; i++)
        {
            FInt dist = FindDistanceToUnit(units[i]);
            if (dist < closetDistUnitToTarget)
            {
                closetDistUnitToTarget = dist;
                newLeader = units[i];
            }
        }

        // Set leader
        leader = newLeader;

        // Let new leader know he is the leader
        if(leader != null)
            leader.isLeader = true;
    }

    public override void LockStepUpdate()
    {
        for(int i = 0; i < units.Count; i++)
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
