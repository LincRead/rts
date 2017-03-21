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
                        transform.position + new Vector3(
                            (FPoint.Sin(FInt.FromFloat(squads[i].GetComponent<Squad>().GetMergingUnits() * 1)) * FInt.FromParts(1, 300)).ToFloat(),
                            ((FInt.FromParts(2, 300) * -1) + (FPoint.Cos(FInt.FromFloat(squads[i].GetComponent<Squad>().GetMergingUnits())) * FInt.FromParts(0, 100))).ToFloat(),
                            0.0f),
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
