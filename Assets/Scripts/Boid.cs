using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boid : FActor, LockStep
{
    protected Pathfinding pathFinding;
    protected List<Node> path;
    protected Node currentStandingNode;
    protected int currentWaypointTarget = 0;

    [Header("Boid debug values")]
    public Vector2 debugVectorValue;

    protected override void Start()
    {
        base.Start();

        pathFinding = GetComponent<Pathfinding>();
    }

    public override void LockStepUpdate()
    {
        currentStandingNode = pathFinding.DetectCurrentPathfindingNode(new Vector2(Fpos.X.ToFloat(), Fpos.Y.ToFloat()));

        debugVectorValue = new Vector2(Fvelocity.X.ToFloat(), Fvelocity.Y.ToFloat());

        base.LockStepUpdate();
    }

    protected void FollowPath()
    {
        Node currTargetNode = path[currentWaypointTarget];

        // Reached target
        if (currTargetNode.gridPosX == pathFinding.currentStandingOnNode.gridPosX
            && currTargetNode.gridPosY == pathFinding.currentStandingOnNode.gridPosY)
        {
            currentWaypointTarget++;
            if (currentWaypointTarget >= path.Count)
            {
                ReachedTarget();
                return;
            }
        }

        currTargetNode = path[currentWaypointTarget];
        FInt directionX = currTargetNode._FworldPosition.X - Fpos.X;
        FInt directionY = currTargetNode._FworldPosition.Y - Fpos.Y;
        Fvelocity = FPoint.Normalize(FPoint.Create(directionX, directionY));
    }

    protected virtual void ReachedTarget()
    {

    }
}
