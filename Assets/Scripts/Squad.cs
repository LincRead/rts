using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : Boid, LockStep {

    [Header("Unit types")]
    public GameObject unitPrefab;

    public int playerIndex = -1;

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
    public FActor leader;

    // Add Circle Collider as look sense

    [Header("Units")]
    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public FInt unitMoveSpeed = FInt.FromParts(0, 600);
    List<FActor> units = new List<FActor>();

    protected override void Start()
    {
        base.Start();

        for (var i = 0; i < 30; i++)
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
        // Todo: put all commands in a chunk within a tick to send over network
        if (playerIndex == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                FPoint lastFPos = Fpos;
                Fpos = pathFinding.DetectCurrentPathfindingNode(Camera.main.ScreenToWorldPoint(Input.mousePosition))._FworldPosition;

                // Find new leader
                FActor newLeader = FindNewLeader();

                if (newLeader != null)
                {
                    // Tell old leader leadership is over
                    if (leader != null)
                        leader.GetComponent<Unit>().isLeader = false;

                    newLeader.GetComponent<Unit>().isLeader = true;

                    if (newLeader.GetFPosition().X < Fpos.X)
                        faceDir = -1;
                    else if (newLeader.GetFPosition().X > Fpos.X)
                        faceDir = 1;

                    // Remember new leader
                    leader = newLeader;

                    state = SQUAD_STATES.MOVE_TO_TARGET;

                    for (int i = 0; i < units.Count; i++)
                        units[i].GetComponent<Unit>().currentState = Unit.UNIT_STATES.MOVE;
                }

                else
                {
                    // Don't change
                    Fpos = lastFPos;
                }
            }
        }

        transform.position = GetRealPosToVector3();
    }

    FActor FindNewLeader()
    {
        FActor newLeader = null;
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
            return null;

        return newLeader;
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        FPoint dist = FPoint.VectorSubtract(unit.GetFPosition(), Fpos);
        return FPoint.Sqrt((dist.X * dist.X) + (dist.Y * dist.Y));
    }

    public override void LockStepUpdate()
    {
        base.LockStepUpdate();

        for(int i = 0; i < units.Count; i++)
            units[i].GetComponent<FActor>().LockStepUpdate();
    }

    public Vector2 velocity;


    public void AddUnit(Unit newUnit)
    {
        units.Add(newUnit);
        newUnit.GetComponent<Unit>().SetSquad(this);
    }

    public void RemoveUnit(FActor unitToRemove)
    {
        units.Remove(unitToRemove);
    }

    public int GetSquadSize()
    {
        return units.Count;
    }

    public List<FActor> GetUnits()
    {
        return units;
    }
}
