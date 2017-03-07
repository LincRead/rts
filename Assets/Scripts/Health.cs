using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public GameObject healthBarPrefab;
    public float regeneratePerSecond = 2f;
    GameObject healthBarInstance;
    SpriteRenderer spriteRenderer;
    FInt maxHitpoints = FInt.Create(0);
    FInt hitpoints = FInt.Create(0);
    FInt zeroHitpoints = FInt.Create(0);
    FInt FregeneratePerTick;

    void Start () {
        healthBarInstance = GameObject.Instantiate(healthBarPrefab,
        new Vector3(transform.position.x, transform.position.y + 0.6f, 0.0f), Quaternion.identity) as GameObject;
        healthBarInstance.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());
        spriteRenderer = healthBarInstance.GetComponent<SpriteRenderer>();
        FregeneratePerTick = FInt.FromFloat((regeneratePerSecond / 20));
    }

    void Update()
    {
        if(maxHitpoints != 0)
        {
            float scaleX = ((float)hitpoints / maxHitpoints.ToFloat());
            healthBarInstance.GetComponent<Transform>().localScale = new Vector3(scaleX * 1.8f, 2.5f, 0.0f);

            if (scaleX <= 0.4f)
                spriteRenderer.color = Color.red;
            else if (scaleX < 0.6f)
                spriteRenderer.color = new Color(1.0f, 0.7f, 0.0f);
            else if (scaleX < 0.8f)
                spriteRenderer.color = Color.yellow;
            else
                spriteRenderer.color = Color.green;
        }

        else
        {
            Debug.LogError("Max health can't be set to 0 for Health script");
        }
    }

    public void Regenerate()
    {
        if (hitpoints == zeroHitpoints)
            return;

        hitpoints += FregeneratePerTick;

        if (hitpoints > maxHitpoints)
            hitpoints = maxHitpoints;
    }

    public void ChangeHitpoints(int value)
    {
        hitpoints += value;

        if (hitpoints < zeroHitpoints)
            hitpoints = zeroHitpoints;
    }

    public void SetMaxHitpoints(int maxHP) {
        maxHitpoints = FInt.Create(maxHP);
    }

    public void SetHitpoints(int HP) {
        hitpoints = FInt.Create(HP);
    }

    public FInt GetHitpoints() {
        return hitpoints;
    }

    public bool IsHitpointsZero() {
        return hitpoints == zeroHitpoints;
    }
}
