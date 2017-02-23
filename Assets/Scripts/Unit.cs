using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Boid, LockStep
{
    public enum UNIT_STATES
    {
        IDLE,
        MOVE,
        ATTACKING,
        DYING
    }

    int hitpoints = 2;

    [HideInInspector]
    public UNIT_STATES currentState = UNIT_STATES.MOVE;

    [HideInInspector]
    public bool isLeader = false;

    Animator animator;
    Squad parentSquad;
    GameObject[] obstacles;
    FPoint squadPosOffset;

    protected override void Start()
    {
        base.Start();
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        animator = GetComponent<Animator>();
        squadPosOffset = FPoint.VectorSubtract(parentSquad.GetFPosition(), GetFPosition());
    }

    public void SetSquad(Squad squad)
    {
        parentSquad = squad;
        hitpoints = squad.unitMaxHitpoints;
    }

    void Update()
    {
        SmoothMovement();
    }

    float currentLerpTime = 0.0f;
    float lerpTime = 1f;
    void SmoothMovement()
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime >= lerpTime)
            currentLerpTime = lerpTime;

        float t = currentLerpTime / lerpTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f);

        transform.position = Vector3.Lerp(
            transform.position,
            GetRealPosToVector3(),
            t
        );
    }

    public override void LockStepUpdate()
    {
        base.LockStepUpdate();
        currentLerpTime = 0.0f;
        HandleCurrentState();
        HandleAnimations();
        ExecuteMovement();
        //HandleCollision();
    }

    void HandleCurrentState()
    {
        switch (currentState)
        {
            case UNIT_STATES.IDLE: HandleMoving(); break;
            case UNIT_STATES.MOVE: HandleMoving(); break;
            case UNIT_STATES.ATTACKING: HandleAttacking(); break;
            case UNIT_STATES.DYING: HandleDying(); break;
        }
    }

    void HandleIdling()
    {
        Fvelocity = FPoint.Create();
    }

    void HandleAttacking()
    {

    }

    void HandleDying()
    {

    }

    void Idle()
    {

    }

    void HandleMoving()
    {
        Fvelocity = FPoint.Create();
        List<FActor> actors = new List<FActor>();
        Grid grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        List<Node> neighbours = grid.GetNeighbours(currentStandingNode);
        neighbours.Add(currentStandingNode);

        for (int n = 0; n < neighbours.Count; n++)
        {
            for (int i = 0, len = neighbours[n].actorsStandingHere.Count; i < len; i++)
            {
                if (neighbours[n].actorsStandingHere[i] != parentSquad)
                {
                    actors.Add(neighbours[n].actorsStandingHere[i]);
                }
            }
        }

        if (currentState != UNIT_STATES.IDLE)
        {
            if (isLeader)
            {
                FPoint seek = ComputeSeek(parentSquad);
                AddSteeringForce(seek, FInt.FromParts(1, 0));
            }

            else if (parentSquad.leader)
            {
                FPoint seek = ComputeSeek(parentSquad.leader);
                AddSteeringForce(seek, FInt.FromParts(1, 0));
            }

            FPoint seperation = ComputeSeperation(actors);
            AddSteeringForce(seperation, FInt.FromParts(0, 500));
        }
    }

    void AddSteeringForce(FPoint steeringForce, FInt weight)
    {
        Fvelocity.X += steeringForce.X * weight;
        Fvelocity.Y += steeringForce.Y * weight;
    }

    FPoint ComputeAlignment(List<FActor> actors)
    {
        FPoint averageVelocity = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            averageVelocity = FPoint.VectorAdd(averageVelocity, actors[i].GetComponent<FActor>().GetFVelocity());
            neighborCount++;
        }

        if (neighborCount == 0)
            return averageVelocity;

        averageVelocity = FPoint.VectorDivide(averageVelocity, neighborCount);

        return FPoint.Normalize(FPoint.Create(averageVelocity.X, averageVelocity.Y));
    }

    FPoint ComputeCohesion(List<FActor> actors)
    {
        FPoint averagePosition = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            averagePosition = FPoint.VectorAdd(averagePosition, actors[i].GetComponent<FActor>().GetFPosition());
            neighborCount++;
        }

        if (neighborCount == 0)
            return averagePosition;

        averagePosition = FPoint.VectorDivide(averagePosition, neighborCount);
        averagePosition = FPoint.VectorSubtract(averagePosition, Fpos);

        return FPoint.Normalize(averagePosition);
    }

    FPoint ComputeSeek(FActor leader)
    {
        FPoint steer = FPoint.Create();
        FInt desiredSlowArea = FInt.FromParts(1, 0);
        FInt dist = FindDistanceToUnit(parentSquad);
        steer = FPoint.VectorSubtract(leader.GetFPosition(), Fpos);

        if (dist < desiredSlowArea)
        {
            // Inside the slowing area
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, parentSquad.unitMoveSpeed);
            steer = FPoint.VectorMultiply(steer, (dist / desiredSlowArea));

            if (leader && dist < desiredSlowArea / 2)
            {
                currentState = UNIT_STATES.IDLE;
            }
        }

        else
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, parentSquad.unitMoveSpeed);
        }

        steer = FPoint.VectorSubtract(steer, Fvelocity);

        return steer;
    }

    FPoint ComputeSeperation(List<FActor> actors)
    {
        FPoint steer = FPoint.Create();
        FInt desiredseparation = FInt.FromParts(0, 500);
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            FInt dist = FindDistanceToUnit(actors[i]);

            if (dist > 0 && dist < desiredseparation)
            {
                if(actors[i].GetComponent<Unit>().currentState == UNIT_STATES.IDLE)
                {
                    currentState = UNIT_STATES.IDLE;
                    Fvelocity = FPoint.Create();
                }

                FPoint diff = FPoint.VectorSubtract(Fpos, actors[i].GetFPosition());
                diff = FPoint.Normalize(diff);
                diff = FPoint.VectorDivide(diff, dist);
                steer = FPoint.VectorAdd(steer, diff);
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            steer = FPoint.VectorDivide(steer, neighborCount);
        }

        if (steer.X != 0 || steer.Y != 0)
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, parentSquad.unitMoveSpeed);
            steer = FPoint.VectorSubtract(steer, Fvelocity);
        }

        return steer;
    }

    FPoint ComputerSeekFormationPostion(FActor leader)
    {
        FInt distX = leader.GetFPosition().X - (GetFPosition().X + squadPosOffset.X);
        FInt distY = leader.GetFPosition().Y - (GetFPosition().Y + squadPosOffset.Y);

        FPoint vector;
        if (FPoint.Sqrt((distX * distX) + (distY * distY)) > FInt.Create(1))
            vector = FPoint.Normalize(FPoint.Create(distX, distY));
        else
            vector = FPoint.Normalize(FPoint.Create(distX, distY));

        return vector;
    }

    FPoint ComputeObstacleAvoidance(GameObject[] obstacles)
    {
        FPoint vector = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < obstacles.Length; i++)
        {
            FInt dist = FindDistanceToUnit(obstacles[i].GetComponent<FActor>());
            if (dist < FInt.FromParts(1, 700) && dist != 0)
            {
                FInt vx = (obstacles[i].GetComponent<FActor>().GetFPosition().X - Fpos.X);
                FInt vy = (obstacles[i].GetComponent<FActor>().GetFPosition().Y - Fpos.Y);

                if (vx != 0)
                    vx = FInt.FromParts(0, 600) / vx;

                if (vy != 0)
                    vy = FInt.FromParts(0, 600) / vy;

                neighborCount++;

                vector.X += vx;
                vector.Y += vy;
            }
        }

        if (neighborCount == 0)
            return vector;

        vector = FPoint.VectorDivide(vector, neighborCount);
        vector.X *= -1;
        vector.Y *= -1;

        return vector;
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        FPoint dist = FPoint.VectorSubtract(unit.GetFPosition(), Fpos);
        return FPoint.Sqrt((dist.X * dist.X) + (dist.Y * dist.Y));
    }

    void MoveTowardsSquadLeader()
    {
        FPoint FsquadPos = parentSquad.GetComponent<FActor>().GetFPosition();
        FInt directionX = FsquadPos.X - Fpos.X;
        FInt directionY = FsquadPos.Y - Fpos.Y;
        Fvelocity = FPoint.Normalize(FPoint.Create(directionX, directionY));
    }

    void ExecuteMovement()
    {
        Fpos.X += Fvelocity.X * parentSquad.unitMoveSpeed;
        Fpos.Y += Fvelocity.Y * parentSquad.unitMoveSpeed;
    }

    void HandleAnimations()
    {
        if (parentSquad.faceDir == -1)
        {
            transform.localScale = new Vector3(-.6f, .6f, 1f);
        }

        else if (parentSquad.faceDir == 1)
        {
            transform.localScale = new Vector3(.6f, .6f, 1.0f);
        }

        if(Fvelocity.X == 0 && Fvelocity.Y == 0)
        {
            animator.SetBool("moving", false);
        }

        else
        {
            animator.SetBool("moving", true);
        }
    }

    void HandleCollision()
    {
        FRectangle r1 = GetCollisionRectangle();
        for (int i = 0; i < obstacles.Length; i++)
        {
            FRectangle r2 = obstacles[i].GetComponent<FActor>().GetCollisionRectangle();

            if (r1.X < r2.X + r2.W &&
               r1.X + r1.W > r2.X &&
               r1.Y < r2.Y + r2.H &&
               r1.Y + r1.H > r2.Y)
            {
                if (Fvelocity.X < 0)
                    Fpos.X += parentSquad.unitMoveSpeed;
                if (Fvelocity.X > 0)
                    Fpos.X -= parentSquad.unitMoveSpeed;
            }

            r1 = GetCollisionRectangle();
            if (r1.X < r2.X + r2.W &&
               r1.X + r1.W > r2.X &&
               r1.Y < r2.Y + r2.H &&
               r1.Y + r1.H > r2.Y)
            {
                if (Fvelocity.Y < 0)
                    Fpos.Y += parentSquad.unitMoveSpeed;
                if (Fvelocity.Y > 0)
                    Fpos.Y -= parentSquad.unitMoveSpeed;
            }
        }
    }

    void Attack()
    {
        currentState = UNIT_STATES.ATTACKING;

        // Trigger attack anim
    }

    void Damage(int damageValue)
    {
        hitpoints -= damageValue;

        if (hitpoints <= 0)
            Kill();
    }

    void Kill()
    {
        parentSquad.RemoveUnit(this);
        currentState = UNIT_STATES.DYING;
        // Trigger Kill animation
        Invoke("Destroy", 1f); // TODO: get death anim duration
    }

}
