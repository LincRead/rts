using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Boid, LockStep
{
    protected enum UNIT_STATES
    {
        DEFAULT,
        ATTACKING,
        DYING
    }

    int hitpoints = 2;

    private UNIT_STATES currentState = UNIT_STATES.DEFAULT;

    Animator animator;
    Squad parentSquad;
    GameObject[] obstacles;
    FPoint spawnPosOffsetSqaud;

    protected override void Start()
    {
        base.Start();
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        animator = GetComponent<Animator>();
        spawnPosOffsetSqaud = FPoint.VectorSubtract(parentSquad.GetFPosition(), GetFPosition());
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
        //t = Mathf.Sin(t * Mathf.PI * 0.5f);

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
            case UNIT_STATES.DEFAULT: HandleMoving(); break;
            case UNIT_STATES.ATTACKING: HandleAttacking(); break;
            case UNIT_STATES.DYING: HandleDying(); break;
        }
    }

    void HandleIdling()
    {

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

    void HandleCollision()
    {
        FRectangle r1 = GetCollisionRectangle();
        for(int i = 0; i < obstacles.Length; i++)
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

    void HandleMoving()
    {
        if(parentSquad.state == Squad.SQUAD_STATES.IDLE)
        {
            Fvelocity = FPoint.Create();

            if (path != null)
                path.Clear();

            // Todo: move to position before done?

            return;
        }

        FInt radius = FInt.FromParts(0, 800);

        FInt cohesionStrength = FInt.FromParts(1, 0);
        FInt seperationStrength = FInt.FromParts(10, 0);
        FInt alignmentStrength = FInt.FromParts(1, 0);

        if (path != null)
            path.Clear();

        List<FActor> actors = new List<FActor>();

        Grid grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        List<Node> neighbours = grid.GetNeighbours(currentStandingNode);
        for (int n = 0; n < neighbours.Count; n++)
        {
            for (int i = 0, len = neighbours[n].actorsStandingHere.Count; i < len; i++)
            {
                if (neighbours[n].actorsStandingHere[i] != this)
                {
                    actors.Add(neighbours[n].actorsStandingHere[i]);
                }
            }
        }

        actors.Remove(parentSquad.GetComponent<FActor>());
        FPoint cohesion = ComputeCohesion(actors);
        FPoint alignment = ComputeSeekLeader(parentSquad);
        FPoint seperation = ComputeSeperation(actors);

        Fvelocity.X = seperation.X * seperationStrength + alignment.X * alignmentStrength;
        Fvelocity.Y = seperation.Y * seperationStrength + alignment.Y * alignmentStrength;
        Fvelocity = FPoint.Normalize(Fvelocity);

        velocity = new Vector2(Fvelocity.X.ToFloat(), Fvelocity.Y.ToFloat());
        valignment = new Vector2(alignment.X.ToFloat(), alignment.Y.ToFloat());
        vseperation = new Vector2(seperation.X.ToFloat(), seperation.Y.ToFloat());
    }

    public Vector2 velocity;
    public Vector2 valignment;
    public Vector2 vseperation;

    FPoint ComputeSeekLeader(FActor leader)
    {
        FInt directionX = leader.GetFPosition().X - Fpos.X;
        FInt directionY = leader.GetFPosition().Y - Fpos.Y;

        return FPoint.Normalize(FPoint.Create(directionX, directionY));
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

        //return averageVelocity;

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

        FInt directionX = averagePosition.X - Fpos.X;
        FInt directionY = averagePosition.Y - Fpos.Y;

        // return FPoint.Create(directionX, directionY);

        return FPoint.Normalize(FPoint.Create(directionX, directionY));
    }


    FPoint ComputeSeperation(List<FActor> actors)
    {
        FPoint vector = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            FInt dist = FindDistanceToUnit(actors[i]);
            if (dist < FInt.FromParts(0, 500) && dist != 0)
            {
                FInt vx = (actors[i].GetFPosition().X - Fpos.X);
                FInt vy = (actors[i].GetFPosition().Y - Fpos.Y);

                neighborCount++;

                vector.X += (actors[i].GetFPosition().X - Fpos.X);
                vector.Y += (actors[i].GetFPosition().Y - Fpos.Y);
            }
        }

        if (neighborCount == 0)
            return vector;

        vector = FPoint.VectorDivide(vector, neighborCount);
        vector.X *= -1;
        vector.Y *= -1;

        //return vector;

        return FPoint.Create(vector.X, vector.Y);
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        FInt distX = unit.GetComponent<FActor>().GetFPosition().X - Fpos.X;
        FInt distY = unit.GetComponent<FActor>().GetFPosition().Y - Fpos.Y;

        return FPoint.Sqrt((distX * distX) + (distY * distY));
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
        if (GetFVelocity().X > 0)
        {
            transform.localScale = new Vector3(-.6f, .6f, 1f);
        }

        else if(GetFVelocity().X < 0)
        {
            transform.localScale = new Vector3(.6f, .6f, 1.0f);
        }

        if(GetFVelocity().X == 0 && GetFVelocity().Y == 0)
        {
            animator.SetBool("moving", false);
        }

        else
        {
            animator.SetBool("moving", true);
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
