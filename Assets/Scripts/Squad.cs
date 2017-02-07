using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Squad : MonoBehaviour, LockStep {

    private int playerIndex = -1;

    protected enum SQUAD_STATES
    {
        IDLE,
        MOVE_TO_TARGET,
        CHASE_SQUAD
    }

    List<GameObject> units = new List<GameObject>();

    // Add Circle Collider as look sense

    public int unitMaxHitpoints = 2;
    public int unitAttackDamage = 1;
    public float unitMoveSpeed = 1f;

	void Start ()
    {
	
	}

	void Update ()
    {

	}

    public void LockStepUpdate()
    {
        foreach (GameObject unit in units)
            unit.GetComponent<Unit>().LockStepUpdate();
    }

    public void AddUnit(GameObject newUnit)
    {
        units.Add(newUnit);
    }

    public void RemoveUnit(GameObject unitToRemove)
    {
        units.Remove(unitToRemove);
    }

    public void FindPath(Vector3 target)
    {
        // A*

    }

    public int GetSquadSize()
    {
        return units.Count;
    }
}
