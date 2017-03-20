using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public GameObject healthBarOutlinePrefab;
    public GameObject healthBarPrefab;
    public GameObject regenIconPrefab;

    GameObject healthBarOutlineInstance;
    GameObject healthBarInstance;
    GameObject regenIconInstance;
    SpriteRenderer regenIconInstanceSpriteRenderer;

    SpriteRenderer spriteRenderer;
    SpriteRenderer healthBarSpriteRenderer;
    SpriteRenderer healthBarOutlineSpriteRenderer;

    Transform healthBarTransform;
    Transform healthBarOutlineTransform;
    Transform regenIconTransform;

    float healthBarDefaultWidth = 0.0f;

    public float offsetY = 0.0f;

    public int regeneratePerSecond = 2;
    int ticksSinceRegenerate = 0;

    FInt maxHitpoints = FInt.Create(0);
    FInt hitpoints = FInt.Create(0);
    FInt zeroHitpoints = FInt.Create(0);

    void Awake () {
        healthBarOutlineInstance = GameObject.Instantiate(healthBarOutlinePrefab,
            transform.position, Quaternion.identity) as GameObject;

        healthBarInstance = GameObject.Instantiate(healthBarPrefab,
            transform.position, Quaternion.identity) as GameObject;

        healthBarSpriteRenderer = healthBarInstance.GetComponent<SpriteRenderer>();
        healthBarSpriteRenderer.enabled = false;
        healthBarSpriteRenderer.sortingOrder = 1;

        healthBarOutlineSpriteRenderer = healthBarOutlineInstance.GetComponent<SpriteRenderer>();
        healthBarOutlineSpriteRenderer.enabled = false;

        healthBarTransform = healthBarInstance.GetComponent<Transform>();
        healthBarOutlineTransform = healthBarOutlineInstance.GetComponent<Transform>();

        healthBarDefaultWidth = healthBarSpriteRenderer.bounds.size.x;

        // Set up health regen icon
        regenIconInstance = GameObject.Instantiate(regenIconPrefab,
            new Vector3(transform.position.x, transform.position.y + offsetY + 1.45f, 0.0f), Quaternion.identity) as GameObject;
        regenIconTransform = regenIconInstance.GetComponent<Transform>();
        regenIconTransform.SetParent(gameObject.GetComponent<Transform>());
        regenIconTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        LeanTween.scale(regenIconInstance.gameObject, new Vector3(1.3f, 1.3f, 1.0f), 0.35f).setLoopPingPong();
        regenIconInstanceSpriteRenderer = regenIconInstance.GetComponent<SpriteRenderer>();
        //regenIconInstanceSpriteRenderer.enabled = false;

        // Mics
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(maxHitpoints != 0 && healthBarTransform != null)
        {
            float scaleX = ((float)hitpoints / maxHitpoints.ToFloat());
            healthBarTransform.localScale = new Vector3(scaleX * 1f, 1.0f, 0.0f);

            healthBarOutlineTransform.position = new Vector3(transform.position.x, transform.position.y + spriteRenderer.bounds.size.y + offsetY, transform.position.z);

            healthBarTransform.position = new Vector3(
                healthBarOutlineTransform.position.x - (((healthBarDefaultWidth / 2) * (1 - scaleX))),
                healthBarOutlineTransform.position.y,
                healthBarOutlineTransform.position.z);

            if (scaleX <= 0.5f)
                healthBarSpriteRenderer.color = Color.red;
            else if (scaleX < 0.65f)
                healthBarSpriteRenderer.color = new Color(1.0f, 0.7f, 0.0f);
            else if (scaleX < 0.8f)
                healthBarSpriteRenderer.color = Color.yellow;
            else
                healthBarSpriteRenderer.color = Color.green;
        }
    }

    public void Regenerate()
    {
        ticksSinceRegenerate++;

        if(ticksSinceRegenerate == 20)
        {
            ticksSinceRegenerate = 0;

            if (hitpoints == zeroHitpoints)
                return;

            ChangeHitpoints(regeneratePerSecond);
        }

        if (hitpoints >= maxHitpoints)
        {
            hitpoints = maxHitpoints;
            ToggleOffRegenerateSymbol();
        }

        else
        {
            regenIconInstanceSpriteRenderer.enabled = true;
        }
    }

    public void ToggleOffRegenerateSymbol()
    {
        regenIconInstanceSpriteRenderer.enabled = false;
    }

    public void ChangeHitpoints(int value)
    {
        if (hitpoints <= 0)
            return; // Already dead

        hitpoints += FInt.Create(value);

        if (hitpoints < zeroHitpoints)
            hitpoints = zeroHitpoints;

        if(hitpoints < maxHitpoints)
        {
            healthBarSpriteRenderer.enabled = true;
            healthBarOutlineSpriteRenderer.enabled = true;
        }

        else
        {
            healthBarSpriteRenderer.enabled = false;
            healthBarOutlineSpriteRenderer.enabled = false;
        }

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

    public void Destroy()
    {
        Destroy(healthBarInstance.gameObject);
        Destroy(healthBarOutlineInstance.gameObject);
        Destroy(regenIconInstance.gameObject);
    }
}
