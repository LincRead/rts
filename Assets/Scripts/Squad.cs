using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : Boid, LockStep {

    [Header("Unit types")]
    public GameObject unitPrefab;

    public int playerIndex = -1;

    public enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    public SQUAD_STATES state = SQUAD_STATES.IDLE;

    // Add Circle Collider as look sense

    [Header("Units")]
    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public FInt unitMoveSpeed = FInt.FromParts(0, 300);
    List<FActor> units = new List<FActor>();

    protected override void Start()
    {
        base.Start();

        pathFinding = GetComponent<Pathfinding>();

        for (var i = 0; i < 30; i++)
        {
            Vector2 pos = new Vector2(transform.position.x + (i % 5f) * 0.5f, transform.position.y + (i % 2f) * 0.5f);
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
                List<Node> newPath = pathFinding.FindPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (newPath != null && newPath.Count > 0)
                {
                    path = newPath;
                    currentWaypointTarget = 0;
                    state = SQUAD_STATES.MOVE_TO_TARGET;
                }
            }
        }

        transform.position = GetRealPosToVector3();
    }

    public override void LockStepUpdate()
    {
        base.LockStepUpdate();

        switch (state)
        {
            case SQUAD_STATES.IDLE: HandleIdle(); break;
            case SQUAD_STATES.MOVE_TO_TARGET: HandleMovingToTarget(); break;
            case SQUAD_STATES.CHASE_SQUAD: HandleChaseSquad(); break;
        }

        for(int i = 0; i < units.Count; i++)
            units[i].GetComponent<FActor>().LockStepUpdate();

        Move();
    }

    public Vector2 velocity;

    void Move()
    {
        Fpos.X += unitMoveSpeed * Fvelocity.X;
        Fpos.Y += unitMoveSpeed * Fvelocity.Y;
    }

    void HandleIdle()
    {

    }

    void HandleMovingToTarget()
    {
        if (path != null && path.Count > 0)
        {
            FollowPath();
        }
    }

    protected override void ReachedTarget()
    {
        Fvelocity.X = FInt.Create(0);
        Fvelocity.Y = FInt.Create(0);
        state = SQUAD_STATES.IDLE;
    }

    void HandleChaseSquad()
    {

    }

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
