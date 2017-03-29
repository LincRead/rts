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
    float scaleX = 1.0f;

    bool belongsToEnemyUnit = false;

    public float offsetY = 0.0f;

    public int regeneratePerSecond = 2;
    int ticksSinceRegenerate = 0;

    FInt maxHitpoints = FInt.Create(0);
    FInt hitpoints = FInt.Create(0);
    FInt zeroHitpoints = FInt.Create(0);

    bool changedHitPoints = true;

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

        healthBarDefaultWidth = healthBarSpriteRenderer.bounds.size.x;

        // Set up health regen icon
        regenIconInstance = GameObject.Instantiate(regenIconPrefab,
            new Vector3(transform.position.x, transform.position.y + offsetY + 1.75f, 0.0f), Quaternion.identity) as GameObject;
        regenIconTransform = regenIconInstance.GetComponent<Transform>();
        regenIconTransform.SetParent(gameObject.GetComponent<Transform>());
        regenIconTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        LeanTween.scale(regenIconInstance.gameObject, new Vector3(1.3f, 1.3f, 1.0f), 0.35f).setLoopPingPong();
        regenIconInstanceSpriteRenderer = regenIconInstance.GetComponent<SpriteRenderer>();
        regenIconInstanceSpriteRenderer.enabled = false;

        // Mics
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        healthBarTransform = healthBarInstance.GetComponent<Transform>();
        healthBarOutlineTransform = healthBarOutlineInstance.GetComponent<Transform>();
    }

    void Update()
    {
        if (changedHitPoints) // Only re-calculate scaling if anything changed
        {
            scaleX = ((float)hitpoints / maxHitpoints.ToFloat());
            healthBarTransform.localScale = new Vector3(scaleX * 1f, 1.0f, 0.0f);
        }

        healthBarOutlineTransform.position = new Vector3(transform.position.x, transform.position.y + spriteRenderer.bounds.size.y + offsetY, transform.position.z);

        healthBarTransform.position = new Vector3(
            healthBarOutlineTransform.position.x - (((healthBarDefaultWidth / 2) * (1 - scaleX))),
            healthBarOutlineTransform.position.y,
            healthBarOutlineTransform.position.z);

        changedHitPoints = false;
    }

    public void Regenerate()
    {
        ticksSinceRegenerate++;

        if(ticksSinceRegenerate == 50)
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

        changedHitPoints = true;
    }

    public void SetBelongsToEnemyUnit(bool isEnemy)
    {
        belongsToEnemyUnit = isEnemy;

        if (belongsToEnemyUnit)
            healthBarSpriteRenderer.color = Color.red;
        else
            healthBarSpriteRenderer.color = Color.green;
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

    public bool HasZeroHitpoints() {
        return hitpoints == zeroHitpoints;
    }

    public void Destroy()
    {
        Destroy(healthBarInstance.gameObject);
        Destroy(healthBarOutlineInstance.gameObject);
        Destroy(regenIconInstance.gameObject);
        Destroy(gameObject);
    }
}
