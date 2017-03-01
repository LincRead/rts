using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Boid, LockStep
{
    public enum UNIT_STATES
    {
        IDLE,
        MOVE,
        CHASING,
        ATTACKING,
        DYING
    }

    int hitpoints = 2;

    [Header("Prefabs")]
    public GameObject healthBarPrefab;
    GameObject healthBar;

    [Header("Debug state")]
    public UNIT_STATES currentState = UNIT_STATES.MOVE;

    [HideInInspector]
    public bool isLeader = false;

    Animator animator;
    Squad parentSquad;
    FActor targetEnemy;
    List<FActor> obstacles = new List<FActor>(20);
    FPoint FaheadFull;
    FPoint FaheadHalf;
    FPoint FidleVelocity = FPoint.Create();
    FInt maxSeeAhead = FInt.FromParts(1, 0);

    bool canFindNewTarget = true;

    List<FActor> friendlyActorsClose = new List<FActor>(30);
    List<FActor> enemyActorsClose = new List<FActor>(30);
    List<Node> neighbours;
    Grid grid;

    float timeBetweenAttacks = 0.5f;
    float timeSinceLastAttack = 0.0f; 

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

        GameObject[] obstaclesArray = GameObject.FindGameObjectsWithTag("Obstacle");
        for (var i = 0; i < obstaclesArray.Length; i++)
            obstacles.Add(obstaclesArray[i].GetComponent<FActor>());

        healthBar = GameObject.Instantiate(healthBarPrefab, 
            new Vector3(transform.position.x, transform.position.y - 0.1f, 0.0f), Quaternion.identity) as GameObject;
        healthBar.GetComponent<Transform>().SetParent(transform);
    }

    public void SetSquad(Squad squad)
    {
        parentSquad = squad;
        hitpoints = squad.unitMaxHitpoints;
    }

    void Update()
    {
        DebugStateWithColor();
        SmoothMovement();
    }

    void DebugStateWithColor()
    {
        if (isLeader)
            spriteRenderer.color = Color.red;
        else if (currentState == UNIT_STATES.ATTACKING && playerID == 0)
            spriteRenderer.color = Color.green;
        else if (playerID == 1)
            spriteRenderer.color = Color.blue;
        else
            spriteRenderer.color = Color.white;
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

        transform.position = Vector3.Lerp(
            transform.position,
            GetRealPosToVector3(),
            t
        );
    }

    public override void LockStepUpdate()
    {
        base.LockStepUpdate();

        // Reset interpolation
        currentLerpTime = 0.0f;

        FindCloseUnits();

        // Find new target enemy
        FindNewTargetEnemy();

        // Found target enemy
        if(targetEnemy != null)
            TargetEnemy();

        // Need to handle being in attack mode when no longer having a target
        else if(currentState == UNIT_STATES.ATTACKING || currentState == UNIT_STATES.CHASING)
        {
            if (isLeader)
                parentSquad.SetFPosition(GetFPosition());

            MoveToTarget();
        }

        HandleCurrentState();
        AvoidObstacles();  // ALWAYS avoid obstacles
        HandleAnimations();
        ExecuteMovement();
    }

    public void MoveToTarget()
    {
        targetEnemy = null;
        currentState = UNIT_STATES.MOVE;
        canFindNewTarget = false;
        Invoke("CanFindNewTarget", 1f); // Todo lockstep
    }

    void CanFindNewTarget() { canFindNewTarget = true; }

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
            case UNIT_STATES.MOVE: HandleMovingToTarget(); break;
            case UNIT_STATES.CHASING: HandleChasingUnit(); break;
            case UNIT_STATES.ATTACKING: HandleAttacking(); break;
            case UNIT_STATES.DYING: HandleDying(); break;
        }
    }

    void HandleIdling()
    {
        Fvelocity = FidleVelocity;
    }

    void HandleChasingUnit()
    {
        Fvelocity = FidleVelocity;

        // Steer away from friendly units
        FPoint seperation = ComputeSeperation(friendlyActorsClose);
        AddSteeringForce(seperation, FInt.FromParts(1, 0));

        // Find a way around friendly units
        FPoint avoidance = ComputeObstacleAvoidance(friendlyActorsClose);
        AddSteeringForce(avoidance, FInt.FromParts(0, 300));

        // Steer towards enemy target

        FPoint seek = ComputeSeek(targetEnemy, false);
        AddSteeringForce(seek, FInt.FromParts(0, 300));
    }

    void HandleAttacking()
    {
        Fvelocity = FidleVelocity;

        timeSinceLastAttack  += Time.deltaTime;

        if (timeSinceLastAttack >= timeBetweenAttacks)
        {
            timeSinceLastAttack = 0.0f;
            Attack();
        } 
    }

    void HandleDying()
    {

    }

    void HandleMovingToTarget()
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
    }

    void HandleAnimations()
    {
        if (currentState != UNIT_STATES.ATTACKING && currentState != UNIT_STATES.CHASING)
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

        else if (targetEnemy != null)
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

        if (currentState == UNIT_STATES.IDLE)
        {
            animator.SetBool("moving", false);
        }

        else
        {
            animator.SetBool("moving", true);
        }
    }

    void AvoidObstacles()
    {
        FPoint avoidance = ComputeObstacleAvoidance(obstacles);
        AddSteeringForce(avoidance, FInt.FromParts(1, 0));
    }

    void ExecuteMovement()
    {
        Fpos.X += Fvelocity.X * parentSquad.unitMoveSpeed;
        Fpos.Y += Fvelocity.Y * parentSquad.unitMoveSpeed;
    }

    void AddSteeringForce(FPoint steeringForce, FInt weight)
    {
        Fvelocity.X += steeringForce.X * weight;
        Fvelocity.Y += steeringForce.Y * weight;
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

    FPoint ComputeObstacleAvoidance(List<FActor> actors)
    {
        FaheadFull = FPoint.VectorAdd(GetFPosition(), FPoint.Normalize(Fvelocity));
        FaheadFull = FPoint.VectorMultiply(FaheadFull, maxSeeAhead);

        FaheadHalf = FPoint.VectorAdd(GetFPosition(), FPoint.Normalize(Fvelocity));
        FaheadHalf = FPoint.VectorMultiply(FaheadHalf, maxSeeAhead * FInt.FromParts(0, 500));

        FPoint steer = FPoint.Create();

        for (int i = 0; i < actors.Count; i++)
        {
            FActor obstacle = actors[i].GetComponent<FActor>();
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

    void FindNewTargetEnemy()
    {
        FInt shortestDistance = FInt.Create(1000);
        for (int i = 0; i < enemyActorsClose.Count; i++)
        {
            FInt dist = FindDistanceToUnit(enemyActorsClose[i]);
            if (dist < FInt.Create(3) && dist < shortestDistance)
            {
                shortestDistance = dist;
                targetEnemy = enemyActorsClose[i];
            }
        }
    }

    void TargetEnemy()
    {
        // This is reset every time a move command is given
        // Don't chase or attack anyone unless true
        if (canFindNewTarget)
        {
            FPoint FAttackPoint = FPoint.VectorAdd(GetFPosition(), Fvelocity);
            if (LineIntersectsObstacle(FAttackPoint, targetEnemy))
            {
                // Reset timer before first blow
                if(currentState != UNIT_STATES.ATTACKING)
                    timeSinceLastAttack = 0.0f;

                currentState = UNIT_STATES.ATTACKING;
            }

            else if (parentSquad.leader.currentState != UNIT_STATES.MOVE 
                && (currentState != UNIT_STATES.ATTACKING || FindDistanceToUnit(targetEnemy) > targetEnemy.GetFBoundingRadius() * 2))
            {
                currentState = UNIT_STATES.CHASING;
            }
        }
    }

    void Attack()
    {
        // Trigger attack anim

        Unit unitScript = targetEnemy.GetComponent<Unit>();

        // Damage target enemy
        unitScript.Damage(parentSquad.unitAttackDamage);

        // Reset state
        if (unitScript.GetHitPoints() == 0)
        {
            targetEnemy = null;
            MoveToTarget();
        }
    }

    void Damage(int damageValue)
    {
        hitpoints -= damageValue;

        if (hitpoints <= 0)
        {
            hitpoints = 0;
            Kill();
        }
    }

    void Kill()
    {
        parentSquad.RemoveUnit(this);
        currentState = UNIT_STATES.DYING;

        if (isLeader)
            parentSquad.FindNewLeader();

        Invoke("Destroy", 0.1f); // TODO: get death anim duration
        // Trigger Kill animation
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

    bool LineIntersectsObstacle(FPoint ahead, FActor obstacle)
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

    bool LineIntersectsObstacle(FPoint aheadHalf, FPoint aheadFull, FActor obstacle)
    {
        FInt radius = obstacle.GetFBoundingRadius();
        FInt distA = (aheadFull.X - obstacle.GetFPosition().X) * (aheadFull.X - obstacle.GetFPosition().X) + (aheadFull.Y - obstacle.GetFPosition().Y) * (aheadFull.Y - obstacle.GetFPosition().Y);
        FInt distB = (aheadHalf.X - obstacle.GetFPosition().X) * (aheadHalf.X - obstacle.GetFPosition().X) + (aheadHalf.Y - obstacle.GetFPosition().Y) * (aheadHalf.Y - obstacle.GetFPosition().Y);
        return distA <= radius || distB < radius;
    }

    int GetHitPoints() { return hitpoints; }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.2f, 1.0f, 0.8f);
        Gizmos.DrawWireSphere(GetRealPosToVector3(), boundingRadius);
    }
}
