using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public GameObject healthBarPrefab;
    public GameObject regenIconPrefab;

    GameObject healthBarInstance;
    GameObject regenIconInstance;
    SpriteRenderer regenIconInstanceSpriteRenderer;

    public float regeneratePerSecond = 2f;
    public float fastRegenerateMultiplier = 3f;

    SpriteRenderer spriteRenderer;
    FInt maxHitpoints = FInt.Create(0);
    FInt hitpoints = FInt.Create(0);
    FInt zeroHitpoints = FInt.Create(0);
    FInt FregeneratePerTick;
    FInt FfastRegenerateMultiplier;

    void Start () {
        healthBarInstance = GameObject.Instantiate(healthBarPrefab,
            new Vector3(transform.position.x, transform.position.y + 1.05f, 0.0f), Quaternion.identity) as GameObject;
        healthBarInstance.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());

        regenIconInstance = GameObject.Instantiate(regenIconPrefab,
            new Vector3(transform.position.x, transform.position.y + 1.3f, 0.0f), Quaternion.identity) as GameObject;
        regenIconInstance.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());
        regenIconInstanceSpriteRenderer = regenIconInstance.GetComponent<SpriteRenderer>();
        regenIconInstanceSpriteRenderer.enabled = false;

        spriteRenderer = healthBarInstance.GetComponent<SpriteRenderer>();
        FregeneratePerTick = FInt.FromFloat((regeneratePerSecond / 20));
        FfastRegenerateMultiplier = FInt.FromFloat(fastRegenerateMultiplier);
    }

    void Update()
    {
        if(maxHitpoints != 0)
        {
            float scaleX = ((float)hitpoints / maxHitpoints.ToFloat());
            healthBarInstance.GetComponent<Transform>().localScale = new Vector3(scaleX * 1f, 1.3f, 0.0f);

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

    public void RegenerateNothing()
    {
        regenIconInstanceSpriteRenderer.enabled = false;
    }

    public void Regenerate()
    {
        if (hitpoints == zeroHitpoints)
            return;

        hitpoints += FregeneratePerTick;

        if (hitpoints > maxHitpoints)
            hitpoints = maxHitpoints;

        regenIconInstanceSpriteRenderer.enabled = false;
    }

    public void FastRegenerate()
    {
        if (hitpoints == zeroHitpoints)
            return;

        hitpoints += FregeneratePerTick * FfastRegenerateMultiplier;

        if (hitpoints > maxHitpoints)
        {
            hitpoints = maxHitpoints;
            regenIconInstanceSpriteRenderer.enabled = false;
        }

        else
            regenIconInstanceSpriteRenderer.enabled = true;
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
