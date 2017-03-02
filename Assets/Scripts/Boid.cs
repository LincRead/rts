using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boid : FActor, LockStep
{
    protected Pathfinding pathFinding;
    protected List<Node> path;
    protected Node currentStandingNode;
    protected int currentWaypointTarget = 0;

    protected override void Start()
    {
        base.Start();

        pathFinding = GetComponent<Pathfinding>();
    }

    public override void LockStepUpdate()
    {
        currentStandingNode = pathFinding.DetectCurrentPathfindingNode(new Vector2(Fpos.X.ToFloat(), Fpos.Y.ToFloat()));

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

    protected bool LineIntersectsObstacle(FPoint ahead, FActor obstacle)
    {
        if (obstacle == null)
            return false;

        FInt radius = obstacle.GetFBoundingRadius() * 2;

        Gizmos.color = Color.yellow;
        if (playerID == 0)
            Debug.DrawLine(new Vector2(ahead.X.ToFloat(), ahead.Y.ToFloat()), new Vector2(obstacle.GetFPosition().X.ToFloat(), obstacle.GetFPosition().Y.ToFloat()));

        FInt distA = (ahead.X - obstacle.GetFPosition().X) * (ahead.X - obstacle.GetFPosition().X) + (ahead.Y - obstacle.GetFPosition().Y) * (ahead.Y - obstacle.GetFPosition().Y);
        return distA <= radius;
    }

    protected bool LineIntersectsObstacle(FPoint aheadHalf, FPoint aheadFull, FActor obstacle)
    {
        FInt radius = obstacle.GetFBoundingRadius();
        FInt distA = (aheadFull.X - obstacle.GetFPosition().X) * (aheadFull.X - obstacle.GetFPosition().X) + (aheadFull.Y - obstacle.GetFPosition().Y) * (aheadFull.Y - obstacle.GetFPosition().Y);
        FInt distB = (aheadHalf.X - obstacle.GetFPosition().X) * (aheadHalf.X - obstacle.GetFPosition().X) + (aheadHalf.Y - obstacle.GetFPosition().Y) * (aheadHalf.Y - obstacle.GetFPosition().Y);
        return distA <= radius || distB < radius;
    }
}
