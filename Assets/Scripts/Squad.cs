using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : FActor, LockStep {

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
    public FInt unitMoveSpeed = FInt.FromParts(0, 500);
    List<FActor> units = new List<FActor>();

    Pathfinding pathFinding;
    List<Node> path;
    int currentWaypointTarget = 0;

    protected override void Start()
    {
        base.Start();

        pathFinding = GetComponent<Pathfinding>();

        for (var i = 0; i < 5; i++)
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
        pathFinding.DetectCurrentPathfindingNode(new Vector2(Fpos.X.ToFloat(), Fpos.Y.ToFloat()));

        if(playerIndex == 0)
            if (Input.GetMouseButtonDown(0))
            {
                currentWaypointTarget = 0;
                Vector2 mousePos = Input.mousePosition;
                path = pathFinding.FindPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                state = SQUAD_STATES.MOVE_TO_TARGET;
            }

        transform.position = GetRealPosToVector3();
    }

    public override void LockStepUpdate()
    {
        switch (state)
        {
            case SQUAD_STATES.IDLE: HandleIdle(); break;
            case SQUAD_STATES.MOVE_TO_TARGET: HandleMoveToTarget(); break;
            case SQUAD_STATES.CHASE_SQUAD: HandleChaseSquad(); break;
        }

        foreach (FActor unit in units)
            unit.GetComponent<FActor>().LockStepUpdate();
    }

    void HandleIdle()
    {

    }

    void HandleMoveToTarget()
    {
        if (path != null && path.Count > 0)
        {
            FollowPath();
        }
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

    void FollowPath()
    {
        Node currTargetNode = path[currentWaypointTarget];

        // Reached target
        if (currTargetNode.gridPosX == pathFinding.currentStandingOnNode.gridPosX
            && currTargetNode.gridPosY == pathFinding.currentStandingOnNode.gridPosY)
        {
            currentWaypointTarget++;
            if (currentWaypointTarget >= path.Count)
            {
                state = SQUAD_STATES.IDLE;
                return;
            }
        }

        currTargetNode = path[currentWaypointTarget];
        FInt directionX = currTargetNode._FworldPosition.X - Fpos.X;
        FInt directionY = currTargetNode._FworldPosition.Y - Fpos.Y;
        Fvelocity = FPoint.Normalize(FPoint.Create(directionX, directionY));

        Fpos.X += unitMoveSpeed * Fvelocity.X;
        Fpos.Y += unitMoveSpeed * Fvelocity.Y;
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
