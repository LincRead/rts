using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, LockStep {

    protected enum UNIT_STATES
    {
        IDLE,
        MOVING,
        ATTACKING,
        DYING
    }

    private UNIT_STATES currentState = UNIT_STATES.IDLE;

    int hitpoints = 2;

    Squad parentSquad;

	void Start ()
    {

	}

    void SetSquad(Squad squad)
    {
        parentSquad = squad;
        hitpoints = squad.unitMaxHitpoints;
    }
	
    void Update()
    {
        // Do Slerp movement
    }

    public void LockStepUpdate()
    {
        HandleCurrentState();
    }

    void HandleCurrentState()
    {
        switch(currentState)
        {
            case UNIT_STATES.IDLE: HandleIdling(); break;
            case UNIT_STATES.MOVING: HandleMoving(); break;
            case UNIT_STATES.ATTACKING: HandleAttacking(); break;
            case UNIT_STATES.DYING: HandleDying(); break;
        }
    }

    void HandleIdling()
    {

    }

    void HandleMoving()
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
        currentState = UNIT_STATES.IDLE;
    }

    void Move()
    {
        currentState = UNIT_STATES.MOVING;

        // Trigger move anim
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
}
