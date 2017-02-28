using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : Boid, LockStep {

    [Header("Unit types")]
    public GameObject unitPrefab;

    [HideInInspector]
    public int faceDir = 1;

    public enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    public SQUAD_STATES state = SQUAD_STATES.IDLE;

    [HideInInspector]
    public Unit leader;

    // Add Circle Collider as look sense

    [Header("Units")]
    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public FInt unitMoveSpeed = FInt.FromParts(0, 400);
    List<Unit> units = new List<Unit>(30);

    protected override void Start()
    {
        base.Start();

        for (var i = 0; i < 20; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + (i % 5f) * 0.2f, transform.position.y + (i % 2f) * 0.2f);
            GameObject newUnit = Instantiate(unitPrefab, pos, Quaternion.identity) as GameObject;
            newUnit.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
            //newUnit.transform.SetParent(transform);
            AddUnit(newUnit.GetComponent<Unit>());
        }
    }

    void Update ()
    {
        FPoint lastFPos = Fpos;

        // Todo: put all commands in a chunk within a tick to send over network
        if (playerID == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fpos = pathFinding.GetNodeFromPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition))._FworldPosition;

                state = SQUAD_STATES.MOVE_TO_TARGET;

                for (int i = 0; i < units.Count; i++)
                    units[i].MoveToTarget();
            }
        }

        FindNewLeader();

        if (leader != null)
        {
            leader.GetComponent<Unit>().isLeader = true;

            if (leader.GetFPosition().X < Fpos.X)
                faceDir = -1;
            else if (leader.GetFPosition().X > Fpos.X)
                faceDir = 1;
        }

        else
        {
            // Don't change
            Fpos = lastFPos;
        }

        transform.position = GetRealPosToVector3();
    }

    public void FindNewLeader()
    {
        Unit newLeader = null;

        // Tell old leader leadership is over
        if (leader != null)
            leader.GetComponent<Unit>().isLeader = false;

        FInt closetDistToTarget = FInt.Create(1000);

        for (int i = 0; i < units.Count; i++)
        {
            FInt dist = FindDistanceToUnit(units[i]);
            if (dist < closetDistToTarget)
            {
                closetDistToTarget = dist;
                newLeader = units[i];
            }
        }

        if (closetDistToTarget < FInt.FromFloat(1))
            leader = null;

        leader = newLeader;
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        FPoint dist = FPoint.VectorSubtract(unit.GetFPosition(), Fpos);
        return FPoint.Sqrt((dist.X * dist.X) + (dist.Y * dist.Y));
    }

    public override void LockStepUpdate()
    {
        //base.LockStepUpdate();

        for(int i = 0; i < units.Count; i++)
            units[i].GetComponent<FActor>().LockStepUpdate();
    }

    public Vector2 velocity;


    public void AddUnit(Unit newUnit)
    {
        units.Add(newUnit);
        Unit unitScript = newUnit.GetComponent<Unit>();
        unitScript.SetSquad(this);
        unitScript.playerID = playerID;
    }

    public void RemoveUnit(Unit unitToRemove)
    {
        units.Remove(unitToRemove);
    }

    public int GetSquadSize()
    {
        return units.Count;
    }

    public List<Unit> GetUnits()
    {
        return units;
    }
}
