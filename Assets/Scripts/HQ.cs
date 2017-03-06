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
            GameObject spawnedUnit = GameObject.Instantiate(unitPrefab, transform.position + new Vector3(0.0f, -2f, 0.0f), Quaternion.identity) as GameObject;
            GameObject[] squads = GameObject.FindGameObjectsWithTag("Squad");
            for (int i = 0; i < squads.Length; i++)
            {
                if (squads[i].GetComponent<Squad>().playerID == this.playerID)
                    squads[i].GetComponent<Squad>().AddUnit(spawnedUnit.GetComponent<Unit>());
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
