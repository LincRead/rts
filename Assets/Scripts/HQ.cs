using UnityEngine;
using System.Collections;

public class HQ : FActor {

    [Header("HQ")]
    public bool spawnUnits = true;
    public int ticksBetweenSpawn = 40;
    private int ticksSinceSpawn = 0;

    public GameObject unitPrefab;

    public override void LockStepUpdate()
    {
        base.LockStepUpdate();

        if (!spawnUnits)
            return;

        ticksSinceSpawn++;
        if (ticksSinceSpawn == ticksBetweenSpawn)
        {
            GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
            for (int i = 0; i < squads.Length; i++)
            {
                if (squads[i].GetComponent<Squad>().playerID == this.playerID)
                {
                    GameObject spawnedUnit = GameObject.Instantiate(
                        unitPrefab,
                        // Make sure units don't spawn at the exact same position, or selse seperation steering won't work
                        transform.position + new Vector3(0.0f + (0.2f * squads[i].GetComponent<Squad>().GetMergingUnits()), -2f - (0.2f * squads[i].GetComponent<Squad>().GetMergingUnits()), 0.0f),
                        Quaternion.identity) as GameObject;

                    squads[i].GetComponent<Squad>().AddUnit(spawnedUnit.GetComponent<Unit>());
                }
            }

            ticksSinceSpawn = 0;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetRealPosToVector3(), FboundingRadius.ToFloat());
    }
}
