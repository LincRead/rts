using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : FActor, LockStep
{
    Animator animator;

    protected enum UNIT_STATES
    {
        DEFAULT,
        ATTACKING,
        DYING
    }

    int hitpoints = 2;

    private UNIT_STATES currentState = UNIT_STATES.DEFAULT;

    Squad parentSquad;

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
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

    float lerpTime = 1f;
    float currentLerpTime;
    void SmoothMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            GetRealPosToVector3(),
            Time.deltaTime * 5);
    }

    public override void LockStepUpdate()
    {
        HandleCurrentState();
        // Collision Detection
        HandleAnimations();
        ExecuteMovement();
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

    // TEMP test functionality.
    void HandleMoving()
    {
        FInt radius = FInt.FromParts(1, 0);
        FInt seperationStrength = FInt.FromParts(1, 100);

        if (parentSquad.state == Squad.SQUAD_STATES.IDLE)
        {
            if (FindDistanceToUnit(parentSquad.GetComponent<FActor>()) > 3)
                MoveTowardsSquadLeader();
            else
                Fvelocity = FPoint.Create();

            return;
        }

        List<FActor> actors = new List<FActor>(parentSquad.GetUnits());
        actors.Add(parentSquad.GetComponent<FActor>());
        actors.Remove(this);

        FPoint alignment = ComputeAlignment(actors, radius);
        FPoint cohesion = ComputeCohesion(actors, radius);
        FPoint seperation = ComputeSeperation(actors, radius);

        Fvelocity.X = cohesion.X * FInt.FromParts(1, 0) + seperation.X * seperationStrength + alignment.X;
        Fvelocity.Y = cohesion.Y * FInt.FromParts(1, 0) + seperation.Y * seperationStrength + alignment.Y;
        Fvelocity = FPoint.Normalize(Fvelocity);
    }

    FPoint ComputeAlignment(List<FActor> actors, FInt radius)
    {
        FPoint averageVelocity = FPoint.Create();
        int neighborCount = 0;

        for(int i = 0; i < actors.Count; i++)
        {
            if (FindDistanceToUnit(actors[i]) < radius || actors[i] == parentSquad.GetComponent<FActor>())
            {
                neighborCount++;
                averageVelocity = FPoint.VectorAdd(averageVelocity, actors[i].GetComponent<FActor>().GetFVelocity());
            }
        }

        if (neighborCount == 0)
            return averageVelocity;

        averageVelocity = FPoint.VectorDivide(averageVelocity, neighborCount);

        return FPoint.Normalize(FPoint.Create(averageVelocity.X, averageVelocity.Y));
    }

    FPoint ComputeCohesion(List<FActor> actors, FInt radius)
    {
        FPoint averagePosition = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            if (FindDistanceToUnit(actors[i]) < radius || actors[i] == parentSquad.GetComponent<FActor>())
            {
                neighborCount++;
                averagePosition = FPoint.VectorAdd(averagePosition, actors[i].GetComponent<FActor>().GetFPosition());
            }
        }

        if (neighborCount == 0)
            return averagePosition;

        averagePosition = FPoint.VectorDivide(averagePosition, neighborCount);

        FInt directionX = averagePosition.X - Fpos.X;
        FInt directionY = averagePosition.Y - Fpos.Y;

        return FPoint.Normalize(FPoint.Create(directionX, directionY));
    }

    FPoint ComputeSeperation(List<FActor> actors, FInt radius)
    {
        FPoint vector = FPoint.Create();
        int neighborCount = 0;

        for (int i = 0; i < actors.Count; i++)
        {
            if (FindDistanceToUnit(actors[i]) < radius)
            {
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

        return FPoint.Normalize(FPoint.Create(vector.X, vector.Y));
    }

    FInt FindDistanceToUnit(FActor unit)
    {
        FInt distX = unit.GetComponent<FActor>().GetFPosition().X - Fpos.X;
        FInt distY = unit.GetComponent<FActor>().GetFPosition().Y - Fpos.Y;

        return FPoint.Sqrt(distX * distX + distY * distY);
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
        if (Fvelocity.X > 0)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        else if(Fvelocity.X < 0)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
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
