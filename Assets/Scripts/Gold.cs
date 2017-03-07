using UnityEngine;
using System.Collections;

public class Gold : MonoBehaviour {

    public int startAmount;
    int ticksSinceLastGoldGeneration = 0;
    int ticksBetweenGoldGeneration = 40;
    int currAmount;

    GameController gameController;

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        currAmount = startAmount;
    }
	
	// Update is called once per frame
	public void LockstepUpdate () {
        ticksSinceLastGoldGeneration++;
        if (ticksSinceLastGoldGeneration >= ticksBetweenGoldGeneration)
        {
            ticksSinceLastGoldGeneration = 0;
            currAmount++;

            if (currAmount > 9999)
                currAmount = 999;
        }    
	}

    public int GetAmount()
    {
        return currAmount;
    }
}
