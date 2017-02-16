using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : MonoBehaviour, LockStep {

    [Header("Unit types")]
    public GameObject unitPrefab;

    private int playerIndex = -1;

    protected enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    protected SQUAD_STATES state = SQUAD_STATES.IDLE;

    // Add Circle Collider as look sense

    [Header("Units")]
    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public FInt unitMoveSpeed = FInt.FromParts(0, 500);
    List<GameObject> units = new List<GameObject>();

    [HideInInspector]
    public FPoint positionReal;
    Pathfinding pathFinding;
    List<Node> path;
    int currentWaypointTarget = 0;

    void Start ()
    {
        pathFinding = GetComponent<Pathfinding>();

        GameObject newUnit = Instantiate(unitPrefab, GetRealPosToVector3(), Quaternion.identity) as GameObject;
        AddUnit(newUnit);
    }

    void Update ()
    {
        pathFinding.DetectCurrentPathfindingNode(new Vector2(positionReal.X.ToFloat(), positionReal.Y.ToFloat()));

        if (Input.GetMouseButtonDown(0))
        {
            currentWaypointTarget = 0;
            Vector2 mousePos = Input.mousePosition;
            path = pathFinding.FindPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            state = SQUAD_STATES.MOVE_TO_TARGET;
        }

        SmoothMovement();
    }

    float lerpTime = 1f;
    float currentLerpTime;
    void SmoothMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            GetRealPosToVector3(), 
            Time.deltaTime * 5);
    }

    public void LockStepUpdate()
    {
        currentLerpTime = 0.0f;

        switch (state)
        {
            case SQUAD_STATES.IDLE: HandleIdle(); break;
            case SQUAD_STATES.MOVE_TO_TARGET: HandleMoveToTarget(); break;
            case SQUAD_STATES.CHASE_SQUAD: HandleChaseSquad(); break;
        }

        foreach (GameObject unit in units)
            unit.GetComponent<Unit>().LockStepUpdate();
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

    public void AddUnit(GameObject newUnit)
    {
        units.Add(newUnit);
        newUnit.GetComponent<Unit>().SetSquad(this);
    }

    public void RemoveUnit(GameObject unitToRemove)
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
        FInt directionX = currTargetNode._FworldPosition.X - positionReal.X;
        FInt directionY = currTargetNode._FworldPosition.Y - positionReal.Y;
        FPoint directionNorm = FPoint.Normalize(FPoint.Create(directionX, directionY));

        positionReal.X += unitMoveSpeed * directionNorm.X;
        positionReal.Y += unitMoveSpeed * directionNorm.Y;
    }

    public int GetSquadSize()
    {
        return units.Count;
    }

    public Vector3 GetRealPosToVector3()
    {
        return new Vector3(positionReal.X.ToFloat(), positionReal.Y.ToFloat());
    }
}
