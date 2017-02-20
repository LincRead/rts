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
        currentLerpTime = 0.0f;
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

    void HandleMoving()
    {
        if (parentSquad.state == Squad.SQUAD_STATES.IDLE)
        {
            // Todo: increase this as unit count increases
            if (FindDistanceToUnit(parentSquad.GetComponent<FActor>()) > FInt.FromParts(2, 500))
                MoveTowardsSquadLeader();
            else
                Fvelocity = FPoint.Create();

            return;
        }

        FInt radius = FInt.FromParts(0, 800);
        FInt cohesionStrength = FInt.FromParts(1, 0);
        FInt seperationStrength = FInt.FromParts(1, 100);
        FInt alignmentStrength = FInt.FromParts(0, 600);

        List<FActor> actors = new List<FActor>(parentSquad.GetUnits());
        actors.Add(parentSquad.GetComponent<FActor>());
        actors.Remove(this);

        FPoint alignment = ComputeAlignment(actors, radius);
        FPoint cohesion = ComputeCohesion(actors, radius);
        FPoint seperation = ComputeSeperation(actors, radius);

        Fvelocity.X = cohesion.X * cohesionStrength + seperation.X * seperationStrength + alignment.X * alignmentStrength;
        Fvelocity.Y = cohesion.Y * cohesionStrength + seperation.Y * seperationStrength + alignment.Y * alignmentStrength; ;
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
