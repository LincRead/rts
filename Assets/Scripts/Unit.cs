using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// I want to make the world a better place.

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

    [Header("Debug state")]
    public UNIT_STATES currentState = UNIT_STATES.MOVE;

    [HideInInspector]
    public bool isLeader = false;

    // Movement
    FInt moveSpeed;
    FPoint FidleVelocity = FPoint.Create();
    bool mergingWithSquad = true;

    // Desired FVelocity to get to target without seperation, obstacle avoidance etc.
    FPoint FdirectionVelocity = FPoint.Create();

    // Targeting enemy
    FActor targetEnemy;
    bool canFindNewTarget = true;

    // Preset numbers
    FPoint FaheadFull;
    FPoint FaheadHalf;

    FInt FradiusCloseToHQ = FInt.Create(5);
    FInt FradiusDetectEnemy = FInt.Create(5);

    Animator animator;
    Grid grid;
    Squad parentSquad;

    List<FActor> friendlyActorsClose = new List<FActor>(30);
    List<FActor> enemyActorsClose = new List<FActor>(30);
    List<FActor> obstacles = new List<FActor>(20);
    List<Node> neighbours = new List<Node>();

    // Health
    Health health;

    // Attack
    int ticksBetweenAttacks = 50;
    int ticksSinceLastAttack = 0;
    bool canCancelAttack = true;
    
    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

        if (health == null)
            Debug.LogError("Unit always needs a Health script atftached");
    } 

    protected override void Start()
    {
        base.Start();

        // Todo don't need to store all obstacles here, can do it in squad or GameController and fetch them
        GameObject[] obstaclesArray = GameObject.FindGameObjectsWithTag("Obstacle");
        for (var i = 0; i < obstaclesArray.Length; i++)
            obstacles.Add(obstaclesArray[i].GetComponent<FActor>());
    }

    public void SetSquad(Squad squad)
    {
        parentSquad = squad;
        health.SetMaxHitpoints(squad.unitMaxHitpoints);
        health.SetHitpoints(squad.unitMaxHitpoints);
        health.SetBelongsToEnemyUnit(playerID != GameController.Manager.playerID);
        moveSpeed = FInt.FromFloat(squad.unitMoveSpeed);
    }

    void Update()
    {
        DebugStateWithColor();
        SmoothMovement();
    }

    float currentLerpTime = 0.0f;
    float lerpTime = 0.3f;
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
        if (currentState != UNIT_STATES.DYING)
            base.LockStepUpdate();
        else
            return;

        // Update health
        if ((currentState == UNIT_STATES.IDLE || currentState == UNIT_STATES.MOVE)
            && isCloseToHQ())
            health.Regenerate();
        else
            health.ToggleOffRegenerateSymbol();

        // Reset interpolation
        currentLerpTime = 0.0f;

        FindCloseFriendlyUnits();
        FindCloseEnemyUnits();

        // Find new target enemy
        // Don't find new target if already attacking!
        if (currentState != UNIT_STATES.ATTACKING)
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
        HandleFaceDir();
        ExecuteMovement();
        Debug.DrawLine(new Vector2(Fpos.X.ToFloat(), Fpos.Y.ToFloat()), new Vector2(Fpos.X.ToFloat() + Fvelocity.X.ToFloat(), Fpos.Y.ToFloat() + +Fvelocity.Y.ToFloat()));
    }

    public void MoveToTarget()
    {
        targetEnemy = null;
        currentState = UNIT_STATES.MOVE;
        canCancelAttack = true;
        canFindNewTarget = false;
        Invoke("CanFindNewTarget", 1f); // Todo lockstep
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

        if(currentState != UNIT_STATES.ATTACKING)
            animator.SetBool("attacking", false);
    }

    void HandleIdling()
    {
        Fvelocity = FidleVelocity;

        if(mergingWithSquad)
        {
            FPoint seperation = ComputeSeperation(friendlyActorsClose);
            AddSteeringForce(seperation, FInt.FromParts(0, 700));
        }
    }

    void HandleChasingUnit()
    {
        Fvelocity = FidleVelocity;

        // Steer towards enemy target
        FPoint seek = ComputeSeek(targetEnemy, false);
        AddSteeringForce(seek, FInt.FromParts(1, 0));

        // Steer away from friendly units
        FPoint seperation = ComputeSeperation(friendlyActorsClose);
        AddSteeringForce(seperation, FInt.FromParts(0, 700));

        // Find a way around friendly units
        // Too much resources spent on this one!!!
        //FPoint avoidance = ComputeObstacleAvoidance(friendlyActorsClose);
        //AddSteeringForce(avoidance, FInt.FromParts(0, 200));

        // Don't move through enemy units
        FPoint seperationEnemyUnits = ComputeSeperation(enemyActorsClose);
        AddSteeringForce(seperationEnemyUnits, FInt.FromParts(1, 0));

        // Desired velocity
        FdirectionVelocity = seek;
    }

    void HandleAttacking()
    {
        Fvelocity = FidleVelocity;

        ticksSinceLastAttack++;

        if (ticksSinceLastAttack >= ticksBetweenAttacks)
        {
            canCancelAttack = true;
            ticksSinceLastAttack = 0;
            Attack();
        } 
    }

    void HandleDying()
    {
        // Doesnt't really do anything other than waiting for animation to finish...
    }

    void HandleMovingToTarget()
    {
        // Move to target if leader exists
        Unit leader = parentSquad.leader;
        if (leader == null)
            return;

        List<Node> newPath;
        Node leaderTargetNode = pathFinding.GetNodeFromPoint(parentSquad.GetRealPosToVector3());
        FPoint seek = FidleVelocity;

        Fvelocity = FidleVelocity;

        int maxOffset = 1;

        int nodeOffsetFromLeaderNodeX = pathFinding.currentStandingOnNode.gridPosX - leader.GetCurrentNode().gridPosX;
        int nodeOffsetFromLeaderNodeY = pathFinding.currentStandingOnNode.gridPosY - leader.GetCurrentNode().gridPosY;

        if (nodeOffsetFromLeaderNodeX < -maxOffset) nodeOffsetFromLeaderNodeX = -maxOffset;
        if (nodeOffsetFromLeaderNodeX > maxOffset) nodeOffsetFromLeaderNodeX = maxOffset;

        if (nodeOffsetFromLeaderNodeY < -maxOffset) nodeOffsetFromLeaderNodeY = -maxOffset;
        if (nodeOffsetFromLeaderNodeY > maxOffset) nodeOffsetFromLeaderNodeY = maxOffset;

        Node myTargetNode = pathFinding.GetNodeFromGridPos(
            leaderTargetNode.gridPosX + nodeOffsetFromLeaderNodeX,
            leaderTargetNode.gridPosY + nodeOffsetFromLeaderNodeY);

        // If node is unwalkable, go to same node as leader
        if (myTargetNode.walkable)
            newPath = pathFinding.FindPath(myTargetNode);
        else
            newPath = pathFinding.FindPath(leaderTargetNode);

        // Set path to follow
        if (newPath != null)
            path = newPath;

        // Follow path if path exists
        if (path != null && path.Count > 0)
        {
            if (GetDistanceBetweenPoints(path[0]._FworldPosition, GetFPosition()) < FInt.FromParts(0, 500))
                path.RemoveAt(0);

            if (path.Count > 0)
                seek = ComputeSeek(path[0]._FworldPosition, true);
        }

        // If no path, go directly to target position
        else
            seek = ComputeSeek(parentSquad.GetFPosition(), true);

        // Set desired velocity
        FdirectionVelocity = seek;

        // Leader ignores other units in squad
        if (isLeader)
        {
            AddSteeringForce(seek, FInt.FromParts(1, 0));
        }

        else
        {
            // Follow path...
            AddSteeringForce(seek, FInt.FromParts(0, 800));

            // ...but also stick to leader for cohesion with group
            FPoint seekLeader = ComputeSeek(parentSquad.leader, false);
            AddSteeringForce(seekLeader, FInt.FromParts(0, 200));

            // Avoid other units in squad
            FPoint seperation = ComputeSeperation(friendlyActorsClose);
            AddSteeringForce(seperation, FInt.FromParts(0, 700));
        }

        // Only calculate this if we know there are enemies close by
        if(!canFindNewTarget)
        {
            FPoint seperationEnemyUnits = ComputeSeperation(enemyActorsClose);
            AddSteeringForce(seperationEnemyUnits, FInt.FromParts(1, 0));
        }
    }

    void HandleAnimations()
    {
        if (currentState == UNIT_STATES.IDLE)
        {
            animator.SetBool("moving", false);
        }

        else
        {
            animator.SetBool("moving", true);
        }
    }

    void HandleFaceDir()
    {
        float scale = 1f;

        if (currentState == UNIT_STATES.MOVE)
        {
            if(mergingWithSquad)
            {
                if (Fpos.X < parentSquad.GetFPosition().X)
                {
                    transform.localScale = new Vector3(scale, scale, 1f);
                }

                else if (Fpos.X > parentSquad.GetFPosition().X)
                {
                    transform.localScale = new Vector3(-scale, scale, 1f);
                }
            }
            else
            {
                if (parentSquad.faceDir == 1)
                {
                    transform.localScale = new Vector3(scale, scale, 1f);
                }

                else if (parentSquad.faceDir == -1)
                {
                    transform.localScale = new Vector3(-scale, scale, 1f);
                }
            }

        }

        else if (targetEnemy != null)
        {
            if (Fpos.X < (targetEnemy.GetFPosition().X - FboundingRadius) && FdirectionVelocity.X > 0)
            {
                transform.localScale = new Vector3(scale, scale, 1f);
            }

            else if (Fpos.X > (targetEnemy.GetFPosition().X + FboundingRadius) && FdirectionVelocity.X < 0)
            {
                transform.localScale = new Vector3(-scale, scale, 1f);
            }
        }

        else if (parentSquad.leader != null)
        {
            transform.localScale = parentSquad.leader.GetComponent<Transform>().localScale;
        }
    }

    protected void AvoidObstacles()
    {
        FPoint avoidance = ComputeObstacleAvoidance(obstacles);
        AddSteeringForce(avoidance, FInt.FromParts(1, 0));
    }

    protected void ExecuteMovement()
    {
        // Other units should be able to keep up with leader
        if(isLeader)
        {
            Fvelocity.X = Fvelocity.X * FInt.FromParts(0, 600);
            Fvelocity.Y = Fvelocity.Y * FInt.FromParts(0, 600);
        }

        FInt one = FInt.Create(1);
        if (Fvelocity.X > one) Fvelocity.X = one;
        if (Fvelocity.X < one * -1) Fvelocity.X = one * -1;
        if (Fvelocity.Y > one) Fvelocity.Y = one;
        if (Fvelocity.Y < one * -1) Fvelocity.Y = one * -1;

        Fpos.X += Fvelocity.X * moveSpeed;
        Fpos.Y += Fvelocity.Y * moveSpeed;

        if (Fpos.X > grid.FmaxX) Fpos.X = grid.FmaxX;
        if (Fpos.X < grid.FminX) Fpos.X = grid.FminX;
        if (Fpos.Y > grid.FmaxY) Fpos.Y = grid.FmaxY;
        if (Fpos.Y < grid.FminY) Fpos.Y = grid.FminY;
    }

    protected void AddSteeringForce(FPoint steeringForce, FInt weight)
    {
        Fvelocity.X += steeringForce.X * weight;
        Fvelocity.Y += steeringForce.Y * weight;
    }

    protected FPoint ComputeSeek(FActor target, bool slowDown)
    {
        if (target == null)
            return FidleVelocity;

        return ComputeSeek(target.GetFPosition(), slowDown);
    }

    protected FPoint ComputeSeek(FPoint targetPosition, bool slowDown)
    {
        FPoint steer = FidleVelocity;
        FInt desiredSlowArea = FInt.FromParts(0, 270);
        FInt dist = GetDistanceToFActor(parentSquad);

        steer = FPoint.VectorSubtract(targetPosition, Fpos);

        if (slowDown && dist < desiredSlowArea)
        {
            // Inside the slowing area
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, moveSpeed);
            steer = FPoint.VectorMultiply(steer, (dist / desiredSlowArea));

            // Reaching squad target destination
            if (dist < desiredSlowArea / 2 && currentState != UNIT_STATES.ATTACKING)
            {
                Idle();
            }
        }

        else
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, moveSpeed);
        }

        steer = FPoint.VectorSubtract(steer, Fvelocity);

        return steer;
    }

    protected FPoint ComputeCohesion(List<FActor> actors)
    {
        FPoint steer = FidleVelocity;
        int neighborCount = 0;
        FInt posx = FInt.Create(0);
        FInt posy = FInt.Create(0);

        for (int i = 0; i < actors.Count; i++)
        {
            posx += actors[i].GetComponent<Unit>().Fpos.X;
            posy += actors[i].GetComponent<Unit>().Fpos.Y;
            neighborCount++;
        }

        if (neighborCount > 0)
        {
            FInt averageX = posx / neighborCount;
            FInt averageY = posy / neighborCount;
            FPoint average = FPoint.Create(averageX, averageY);

            steer = FPoint.VectorSubtract(average, Fpos);
        }

        if (steer.X != 0 || steer.Y != 0)
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, moveSpeed);
            steer = FPoint.VectorSubtract(steer, Fvelocity);
        }

        return steer;
    }

    protected FPoint ComputeSeperation(List<FActor> actors)
    {
        FPoint steer = FidleVelocity;
        FInt desiredseparation = FInt.FromParts(0, 270);
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            FInt dist = GetDistanceToFActor(actors[i]);

            if (dist > 0 && dist < desiredseparation)
            {
                if(actors[i].playerID == playerID)
                {
                    // Reaching squad target destination (based on other units having reached it
                    // Not every single unit can reach the same point
                    if (actors[i].GetComponent<Unit>().currentState == UNIT_STATES.IDLE)
                    {
                        Idle();
                    }

                    // We reached squad, so we are no longer merging with it
                    if(mergingWithSquad && !actors[i].GetComponent<Unit>().mergingWithSquad)
                        mergingWithSquad = false;
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
            steer = FPoint.VectorMultiply(steer, moveSpeed);
            steer = FPoint.VectorSubtract(steer, Fvelocity);
        }

        return steer;
    }

    protected FPoint ComputeObstacleAvoidance(List<FActor> actors)
    {
        FaheadFull = FPoint.Normalize(Fvelocity);
        FaheadFull = FPoint.VectorMultiply(FaheadFull, FInt.FromParts(0, 500));
        FaheadFull = FPoint.VectorAdd(FaheadFull, GetFPosition());

        FaheadHalf = FPoint.Normalize(Fvelocity);
        FaheadHalf = FPoint.VectorMultiply(FaheadHalf, FInt.FromParts(0, 250));
        FaheadHalf = FPoint.VectorAdd(FaheadHalf, GetFPosition());

        FPoint steer = FidleVelocity;
        for (int i = 0; i < actors.Count; i++)
        {
            FActor obstacle = actors[i].GetComponent<FActor>();
            if (LineIntersectsObstacle(FaheadFull, FaheadHalf, Fpos, obstacle))
            {
                steer.X = FaheadFull.X - obstacle.GetFPosition().X;
                steer.Y = FaheadFull.Y - obstacle.GetFPosition().Y;
                break;
            }
        }

        if (steer.X != 0 || steer.Y != 0)
        {
            steer = FPoint.Normalize(steer);
            steer = FPoint.VectorMultiply(steer, FInt.FromParts(0, 300));
            steer = FPoint.VectorSubtract(steer, Fvelocity);
        }

        return steer;
    }

    void FindNewTargetEnemy()
    {
        if (enemyActorsClose.Count == 0)
        {
            targetEnemy = null;
            return;
        }

        FInt shortestDistance = FlargeNumber;
        for (int i = 0; i < enemyActorsClose.Count; i++)
        {
            if (enemyActorsClose[i].GetComponent<Unit>().currentState == UNIT_STATES.DYING)
            {
                if (targetEnemy == enemyActorsClose[i])
                {
                    targetEnemy = null;
                }
                   
            }

            else
            {
                FInt dist = GetDistanceToFActor(enemyActorsClose[i]);
                if (dist < FradiusDetectEnemy && dist < shortestDistance)
                {
                    shortestDistance = dist;
                    targetEnemy = enemyActorsClose[i];
                }
            }
        }
    }

    void TargetEnemy()
    {
        // This is reset every time a move command is given
        // Don't chase or attack anyone unless true
        if (canFindNewTarget && parentSquad.leader != null)
        {
            FPoint FAttackPoint = FPoint.VectorAdd(GetFPosition(), FdirectionVelocity);
            if (LineIntersectsObstacle(FAttackPoint, targetEnemy))
            {
                // Reset timer before first blow
                if(currentState != UNIT_STATES.ATTACKING)
                {
                    ticksSinceLastAttack = 0;
                    canCancelAttack = false;
                    animator.SetBool("attacking", true);
                    animator.Play("Attack");
                }
                    
                currentState = UNIT_STATES.ATTACKING;
            }

            else if (parentSquad.leader.currentState != UNIT_STATES.MOVE
                && (currentState != UNIT_STATES.ATTACKING || GetDistanceToFActor(targetEnemy) > targetEnemy.GetFBoundingRadius() * 2)
                && canCancelAttack)
            {
                currentState = UNIT_STATES.CHASING;
            }
        }
    }

    void Idle()
    {
        currentState = UNIT_STATES.IDLE;
        Fvelocity = FidleVelocity;
        canCancelAttack = true;
    }

    void Attack()
    {
        Unit unitScript = targetEnemy.GetComponent<Unit>();
        unitScript.Damage(parentSquad.unitAttackDamage);
    }

    void Damage(int damageValue)
    {
        health.ChangeHitpoints(-damageValue);

        if (health.HasZeroHitpoints())
            Kill();
    }

    void Kill()
    {
        parentSquad.RemoveUnit(this);
        currentState = UNIT_STATES.DYING;

        if (isLeader)
            parentSquad.FindNewLeader();

        pathFinding.RemoveStandingOnNode();

        health.Destroy();

        // Trigger Kill animation

        Invoke("Destroy", 1.0f); // TODO: get death anim duration
    }

    void FindCloseFriendlyUnits()
    {
        if (currentStandingNode == null)
            return;

        friendlyActorsClose.Clear();

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
                }
            }
        }
    }

    void FindCloseEnemyUnits()
    {
        if (currentStandingNode == null)
            return;

        enemyActorsClose.Clear();

        for (int n = 0; n < neighbours.Count; n++)
        {
            for (int i = 0, len = neighbours[n].actorsStandingHere.Count; i < len; i++)
            {
                if (neighbours[n].actorsStandingHere[i])
                {
                    if (neighbours[n].actorsStandingHere[i].playerID != playerID)
                        enemyActorsClose.Add(neighbours[n].actorsStandingHere[i]);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.2f, 1.0f, 0.8f);
        Gizmos.DrawWireSphere(GetRealPosToVector3(), boundingRadius);
    }

    void DebugStateWithColor()
    {
        if (IsDead())
            spriteRenderer.color = Color.black;
        else if (isLeader)
        {
            if (playerID == 0)
                spriteRenderer.color = Color.red;
            else
                spriteRenderer.color = Color.magenta;
        }
        else if (playerID == 1)
            spriteRenderer.color = Color.blue;
        else
            spriteRenderer.color = Color.white;
    }

    bool isCloseToHQ()
    {
        if (parentSquad.GetHQ() == null)
            return false;

        return (GetDistanceBetweenPoints(GetFPosition(), parentSquad.GetHQ().GetFPosition()) < FradiusCloseToHQ * FradiusCloseToHQ);
    }

    void CanFindNewTarget() { canFindNewTarget = true; }

    public bool IsDead() { return health.HasZeroHitpoints(); }

    public void CancelMergingWithSquad() { mergingWithSquad = false; }

    public bool IsMergingWithSquad() { return mergingWithSquad;  }

    public Node GetCurrentNode() { return pathFinding.currentStandingOnNode;  }
}
