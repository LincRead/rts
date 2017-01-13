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

    int curHitpoints = 2;

    Squad parentSquad;

    float deathAnimLength = 1f;

	void Start ()
    {

	}

    void SetSquad(Squad squad)
    {
        parentSquad = squad;
        curHitpoints = squad.unitMaxHitpoints;
    }
	
    void Update()
    {
        HandleCurrentState();

        // Do Slerp movement
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

    public void LockStepUpdate()
    {
        // Update movement

    }

    void Attack()
    {
        // Trigger attack anim
    }

    void Damage(int damageValue)
    {
        hp -= damageValue;

        // TODO play damaged anim

        if (0 <= 0)
            Kill();
    }

     void Kill()
    {
        currentState = UNIT_STATES.DYING;
        parentSquad.RemoveUnit(gameObject);
        Invoke("Destroy", deathAnimLength);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
