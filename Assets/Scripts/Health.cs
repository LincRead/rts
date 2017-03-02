using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public GameObject healthBarPrefab;
    GameObject healthBarInstance;
    SpriteRenderer spriteRenderer;
    int maxHitpoints = 0;
    int hitpoints = 0;

    void Start () {
        healthBarInstance = GameObject.Instantiate(healthBarPrefab,
        new Vector3(transform.position.x, transform.position.y + 0.6f, 0.0f), Quaternion.identity) as GameObject;
        healthBarInstance.GetComponent<Transform>().SetParent(gameObject.GetComponent<Transform>());
        spriteRenderer = healthBarInstance.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if(maxHitpoints != 0)
        {
            float scaleX = ((float)hitpoints / (float)maxHitpoints);
            healthBarInstance.GetComponent<Transform>().localScale = new Vector3(scaleX * 1.5f, 3.0f, 0.0f);

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

    public void ChangeHitpoints(int value)
    {
        hitpoints += value;

        if (hitpoints < 0)
            hitpoints = 0;
    }

    public void SetMaxHitpoints(int maxHP) {  maxHitpoints = maxHP; }
    public void SetHitpoints(int HP) { hitpoints = HP; }

    public int GetHitpoints() { return hitpoints; }
    public bool IsHitpointsZero() { return hitpoints == 0; }
}
