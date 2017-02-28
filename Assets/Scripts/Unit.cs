using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Boid, LockStep
{
    public enum UNIT_STATES
    {
        IDLE,
        MOVE,
        CHASE,
        ATTACKING,
        DYING
    }

    int hitpoints = 2;

    [Header("Debug state")]
    public UNIT_STATES currentState = UNIT_STATES.MOVE;

    [HideInInspector]
    public bool isLeader = false;

    Animator animator;
    Squad parentSquad;
    FActor targetEnemy;
    GameObject[] obstacles;
    FPoint FdirToLeader;
    FPoint FaheadFull;
    FPoint FaheadHalf;
    FPoint FidleVelocity = FPoint.Create();
    FInt maxSeeAhead = FInt.FromParts(1, 0);

    bool canFindNewTarget = true;

    List<FActor> friendlyActorsClose = new List<FActor>(30);
    List<FActor> enemyActorsClose = new List<FActor>(30);
    List<Node> neighbours;
    Grid grid;

    protected override void Start()
    {
        base.Start();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        animator = GetComponent<Animator>();
    }

    public void SetSquad(Squad squad)
    {
        parentSquad = squad;
        hitpoints = squad.unitMaxHitpoints;
    }

    void Update()
    {
        if(isLeader)
            spriteRenderer.color = Color.red;
        else if (currentState == UNIT_STATES.ATTACKING && playerID == 0)
            spriteRenderer.color = Color.blue;
        else if(playerID == 1)
            spriteRenderer.color = Color.magenta;
        else
            spriteRenderer.color = Color.white;

        SmoothMovement();
    }

    float currentLerpTime = 0.0f;
    float lerpTime = 1;
    void SmoothMovement()
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime >= lerpTime)
            currentLerpTime = lerpTime;

        float t = currentLerpTime / lerpTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f);

        //transform.position = GetRealPosToVector3();
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
        FindCloseUnits();
        FindDirToLeader();

        // Attack?
        FInt shortestDistance = FInt.Create(1000);
        targetEnemy = null;
        for (int i = 0; i < enemyActorsClose.Count; i++)
        {
            FInt dist = FindDistanceToUnit(enemyActorsClose[i]);
            if(dist < shortestDistance)
            {
                shortestDistance = dist;
                targetEnemy = enemyActorsClose[i];
            }
        }

        if(targetEnemy != null)
        {
            if(canFindNewTarget)
            {
                FPoint FAttackPoint = FPoint.VectorAdd(GetFPosition(), Fvelocity);
                if (LineIntersectsObstacle(FAttackPoint, targetEnemy))
                {
                    currentState = UNIT_STATES.ATTACKING;
                }

                else if (currentState == UNIT_STATES.IDLE)
                    currentState = UNIT_STATES.CHASE;
            }
        }

        // No more targets
        else if(currentState == UNIT_STATES.ATTACKING)
        {
            if (isLeader)
                parentSquad.SetFPosition(GetFPosition());

            MoveToTarget();
        }

        HandleCurrentState();

        HandleAnimations();
        ExecuteMovement();
        //HandleCollision();
    }

    public void MoveToTarget()
    {
        currentState = UNIT_STATES.MOVE; currentState = UNIT_STATES.MOVE;
        canFindNewTarget = false;
        Invoke("CanFindNewTarget", 1f); // todo lockstep
    }

    void CanFindNewTarget()
    {
        canFindNewTarget = true;
    }

    void FindDirToLeader()
    {
        if(parentSquad.leader == null)
        {
            FdirToLeader = FidleVelocity;
            return;
        }

        if(isLeader)
        {
            FdirToLeader = FPoint.VectorSubtract(parentSquad.GetFPosition(), Fpos);
        }

        else
        {
            FdirToLeader = FPoint.VectorSubtract(parentSquad.leader.GetFPosition(), Fpos);
            FdirToLeader = FPoint.Normalize(FdirToLeader);
        }
    }

    void FindCloseUnits()
    {
        friendlyActorsClose.Clear();
        enemyActorsClose.Clear();

        neighbours = grid.GetNeighbours(currentStandingNode);
        neighbours.Add(currentStandingNode);

        for (int n = 0; n < neighbours.Count; n++)
        {
            for (int i = 0, len = neighbours[n].actorsStandingHere.Count; i < len; i++)
            {
                if (neighbours[n].actorsStandingHere[i])
                {
                    if (neighbours[n].actorsStandingHere[i].playerID == playerID)
                        friendlyActorsClose.Add(neighbours[n].actorsStandingHere[i]);
                    else
                        enemyActorsClose.Add(neighbours[n].actorsStandingHere[i]);
                }
            }
        }
    }

    void HandleCurrentState()
    {
        switch (currentState)
        {
            case UNIT_STATES.IDLE: HandleIdling(); break;
            case UNIT_STATES.MOVE: HandleMoving(); break;
            case UNIT_STATES.CHASE: HandleChasingUnit(); break;
            case UNIT_STATES.ATTACKING: HandleAttacking(); break;
            case UNIT_STATES.DYING: HandleDying(); break;
        }
    }

    void HandleIdling()
    {
        Fvelocity = FidleVelocity;
        //FPoint seperation = ComputeSeperation(friendlyActorsClose);
        //AddSteeringForce(seperation, FInt.FromParts(1, 0));
    }

    void HandleChasingUnit()
    {
        Fvelocity = FidleVelocity;

        // Steer away from friendly units
        FPoint seperation = ComputeSeperation(friendlyActorsClose);
        AddSteeringForce(seperation, FInt.FromParts(0, 600));

        // Steer towards enemy target
        FPoint seek = ComputeSeek(targetEnemy, false);
        AddSteeringForce(seek, FInt.FromParts(0, 300));
    }

    void HandleAttacking()
    {
        Fvelocity = FidleVelocity;

        // Attack
        Attack();
    }

    void HandleDying()
    {

    }

    void HandleMoving()
    {
        Fvelocity = FidleVelocity;

        if (isLeader)
        {
            FPoint seek = ComputeSeek(parentSquad, true);
            AddSteeringForce(seek, FInt.FromParts(1, 0));
        }

        else if (parentSquad.leader != null)
        {
            FPoint seek = ComputeSeek(parentSquad.leader, true);
            AddSteeringForce(seek, FInt.FromParts(1, 0));
        }

        FPoint seperation = ComputeSeperation(friendlyActorsClose);
        AddSteeringForce(seperation, FInt.FromParts(0, 600));

        if(!canFindNewTarget)
        {
            FPoint seperationEnemyUnits = ComputeSeperation(enemyActorsClose);
            AddSteeringForce(seperationEnemyUnits, FInt.FromParts(1, 0));
        }

        FPoint avoidance = ComputeObstacleAvoidance();
        AddSteeringForce(avoidance, FInt.FromParts(1, 0));
    }

    void AddSteeringForce(FPoint steeringForce, FInt weight)
    {
        Fvelocity.X += steeringForce.X * weight;
        Fvelocity.Y += steeringForce.Y * weight;
    }

    bool LineIntersectsObstacle(FPoint ahead, FActor obstacle)
    {
        if (obstacle == null)
            return false;

        FInt radius = obstacle.GetFBoundingRadius() * 2;

        Gizmos.color = Color.yellow;
        if(playerID == 0)
            Debug.DrawLine(new Vector2(ahead.X.ToFloat(), ahead.Y.ToFloat()), new Vector2(obstacle.GetFPosition().X.ToFloat(), obstacle.GetFPosition().Y.ToFloat()));

        FInt distA = (ahead.X - obstacle.GetFPosition().X) * (ahead.X - obstacle.GetFPosition().X) + (ahead.Y - obstacle.GetFPosition().Y) * (ahead.Y - obstacle.GetFPosition().Y);
        return distA <= radius;
    }

    bool LineIntersectsObstacle(FPoint aheadHalf, FPoint aheadFull, FActor obstacle)
    {
        FInt radius = FInt.Create(1);
        FInt distA = (aheadFull.X- obstacle.GetFPosition().X) * (aheadFull.X - obstacle.GetFPosition().X) + (aheadFull.Y - obstacle.GetFPosition().Y) * (aheadFull.Y - obstacle.GetFPosition().Y);
        FInt distB = (aheadHalf.X - obstacle.GetFPosition().X) * (aheadHalf.X - obstacle.GetFPosition().X) + (aheadHalf.Y - obstacle.GetFPosition().Y) * (aheadHalf.Y - obstacle.GetFPosition().Y);
        return distA <= radius || distB < radius;
    }

    FPoint ComputeSeek(FActor leader, bool slowDown)
    {
        if (leader == null)
            return FidleVelocity;

        FPoint steer = FidleVelocity;
        FInt desiredSlowArea = FInt.FromParts(1, 0);
        FInt dist = FindDistanceToUnit(parentSquad);
        steer = FPoint.VectorSubtract(leader.GetFPosition(), Fpos);

        if (slowDown && dist < desiredSlowArea)
        {
            // Inside the slowing area
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, parentSquad.unitMoveSpeed);
            steer = FPoint.VectorMultiply(steer, (dist / desiredSlowArea));

            if (leader && dist < desiredSlowArea / 2 && currentState != UNIT_STATES.ATTACKING)
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
        FPoint steer = FidleVelocity;
        FInt desiredseparation = FInt.FromParts(0, 350);
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

    FPoint ComputeObstacleAvoidance()
    {
        FaheadFull = FPoint.VectorAdd(GetFPosition(), FPoint.Normalize(Fvelocity));
        FaheadFull = FPoint.VectorMultiply(FaheadFull, maxSeeAhead);

        FaheadHalf = FPoint.VectorAdd(GetFPosition(), FPoint.Normalize(Fvelocity));
        FaheadHalf = FPoint.VectorMultiply(FaheadHalf, maxSeeAhead * FInt.FromParts(0, 500));

        FPoint steer = FPoint.Create();

        for (int i = 0; i < obstacles.Length; i++)
        {
            FActor obstacle = obstacles[i].GetComponent<FActor>();
            if (LineIntersectsObstacle(FaheadHalf, FaheadFull, obstacle))
            {
                steer.X = FaheadFull.X - obstacle.GetFPosition().X;
                steer.Y = FaheadFull.Y - obstacle.GetFPosition().Y;
                steer = FPoint.Normalize(steer);
            }
        }

        if (steer.X != 0 || steer.Y != 0)
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, parentSquad.unitMoveSpeed);
            steer = FPoint.VectorSubtract(steer, Fvelocity);
        }

        return steer;
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        return ((unit.GetFPosition().X - Fpos.X) * (unit.GetFPosition().X - Fpos.X)) + ((unit.GetFPosition().Y - Fpos.Y) * (unit.GetFPosition().Y - Fpos.Y));
    }

    FInt FindDistanceToPoint(FPoint a, FPoint b)
    {
        FPoint dist = FPoint.VectorSubtract(a, b);
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
        if(currentState != UNIT_STATES.ATTACKING && currentState != UNIT_STATES.CHASE)
        {
            if (parentSquad.faceDir == -1)
            {
                transform.localScale = new Vector3(-.6f, .6f, 1f);
            }

            else if (parentSquad.faceDir == 1)
            {
                transform.localScale = new Vector3(.6f, .6f, 1.0f);
            }
        }

        else if(targetEnemy != null)
        {
            if (Fpos.X < targetEnemy.GetFPosition().X)
            {
                transform.localScale = new Vector3(-.6f, .6f, 1f);
            }

            else if (Fpos.X > targetEnemy.GetFPosition().X)
            {
                transform.localScale = new Vector3(.6f, .6f, 1.0f);
            }
        }


        if(currentState == UNIT_STATES.IDLE)
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
        //targetEnemy.GetComponent<Unit>().Kill();
        
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

        if (isLeader)
            parentSquad.FindNewLeader();

        Invoke("Destroy", 0f); // TODO: get death anim duration
        // Trigger Kill animation
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.2f, 1.0f, 0.8f);
        Gizmos.DrawWireSphere(GetRealPosToVector3(), boundingRadius);
    }
}
