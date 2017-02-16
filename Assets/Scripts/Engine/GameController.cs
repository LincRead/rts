using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    int currentTick = 0;
    float timeBetweenTicks = .2f;
    float timeSinceLastTick = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    void FixedUpdate()
    {
        timeSinceLastTick += Time.deltaTime;
        if (timeSinceLastTick >= timeBetweenTicks)
        {
            timeSinceLastTick = 0.0f;
            LockStepUpdate();
        }
    }

    void LockStepUpdate()
    {
        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        foreach (GameObject squad in squads)
            squad.GetComponent<Squad>().LockStepUpdate();
    }
}
