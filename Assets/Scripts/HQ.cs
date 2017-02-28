using UnityEngine;
using System.Collections;

public class HQ : MonoBehaviour {

    public float timeBetweenSpawn = 3f;
    private float timeSinceSpawn = 0.0f;

    public GameObject unitPrefab;

    public int playerIndex = -1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        timeSinceSpawn += Time.deltaTime;
        if(timeSinceSpawn >= timeBetweenSpawn)
        {
            GameObject spawnedUnit = GameObject.Instantiate(unitPrefab, transform.position + new Vector3(0.0f, -1f, 0.0f), Quaternion.identity) as GameObject;
            GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
            for(int i = 0; i < squads.Length; i++)
            {
                if (squads[i].GetComponent<Squad>().playerID == this.playerIndex)
                    squads[i].GetComponent<Squad>().AddUnit(spawnedUnit.GetComponent<Unit>());
            }
            timeSinceSpawn = 0.0f;
        }
    }
}
