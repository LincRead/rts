using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    int currentTick = 0;
    float timeBetweenTicks = .05f;
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
            currentTick++;
            LockStepUpdate();
        }
    }

    void LockStepUpdate()
    {
        GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
        for(int i = 0; i < squads.Length; i++)
            squads[i].GetComponent<Squad>().LockStepUpdate();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int i = 0; i < obstacles.Length; i++)
            obstacles[i].GetComponent<FActor>().LockStepUpdate();
    }
}
