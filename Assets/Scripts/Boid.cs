using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boid : FActor, LockStep
{
    protected Pathfinding pathFinding;
    protected List<Node> path;
    protected Node currentStandingNode;
    protected int currentWaypointTarget = 0;

    protected override void Awake()
    {
        pathFinding = GetComponent<Pathfinding>();

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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

        FInt radius = obstacle.GetFBoundingRadius();

        Gizmos.color = Color.yellow;
        if (playerID == 0)
            Debug.DrawLine(new Vector2(ahead.X.ToFloat(), ahead.Y.ToFloat()), new Vector2(obstacle.GetFPosition().X.ToFloat(), obstacle.GetFPosition().Y.ToFloat()));

        FInt distA = (ahead.X - obstacle.GetFPosition().X) * (ahead.X - obstacle.GetFPosition().X) + (ahead.Y - obstacle.GetFPosition().Y) * (ahead.Y - obstacle.GetFPosition().Y);
        return distA <= radius;
    }

    protected bool LineIntersectsObstacle(FPoint aheadFull,  FPoint aheadHalf, FPoint currPos, FActor obstacle)
    {
        Debug.DrawLine(new Vector2(aheadFull.X.ToFloat(), aheadFull.Y.ToFloat()), new Vector2(GetFPosition().X.ToFloat(), GetFPosition().Y.ToFloat()), Color.red);
        Debug.DrawLine(new Vector2(aheadHalf.X.ToFloat(), aheadHalf.Y.ToFloat()), new Vector2(GetFPosition().X.ToFloat(), GetFPosition().Y.ToFloat()), Color.yellow);
        Debug.DrawLine(new Vector2(currPos.X.ToFloat(), currPos.Y.ToFloat()), new Vector2(GetFPosition().X.ToFloat(), GetFPosition().Y.ToFloat()), Color.green);

        FInt radius = obstacle.GetFBoundingRadius();
        FInt distA = Distance(aheadFull, obstacle.GetFPosition());
        FInt distB = Distance(aheadHalf, obstacle.GetFPosition());
        //FInt distC = Distance(currPos, obstacle.GetFPosition());

        return distA <= radius * radius || distB <= radius * radius; // || distC <= radius * radius;
    }

    FInt Distance(FPoint a, FPoint b)
    {
        return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }
}
