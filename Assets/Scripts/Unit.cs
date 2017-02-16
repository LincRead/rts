using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, LockStep
{
    protected FPoint positionReal;
    protected FInt velocityX = FInt.Create(0);
    protected FInt velocityY = FInt.Create(0);

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

    void Start()
    {
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

    public void LockStepUpdate()
    {
        HandleCurrentState();
        HandleMovement();
        // Collision Detection
        HandleAnimations();
        Move();
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
        FInt directionX = parentSquad.positionReal.X - positionReal.X;
        FInt directionY = parentSquad.positionReal.Y - positionReal.Y;
        FPoint directionNorm = FPoint.Normalize(FPoint.Create(directionX, directionY));

        velocityX = parentSquad.unitMoveSpeed * directionNorm.X;
        velocityY = parentSquad.unitMoveSpeed * directionNorm.Y;
    }

    void HandleMovement()
    {

    }

    void Move()
    {
        positionReal.X += velocityX;
        positionReal.Y += velocityY;
    }

    void HandleAnimations()
    {
        if(velocityX > 0)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }

        else if(velocityX < 0)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        if(velocityX == 0 && velocityY == 0)
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
        parentSquad.RemoveUnit(gameObject);
        currentState = UNIT_STATES.DYING;
        // Trigger Kill animation
        Invoke("Destroy", 1f); // TODO: get death anim duration
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    public Vector3 GetRealPosToVector3()
    {
        return new Vector3(positionReal.X.ToFloat(), positionReal.Y.ToFloat());
    }
}
