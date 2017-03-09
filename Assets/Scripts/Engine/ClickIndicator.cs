using UnityEngine;
using System.Collections;

public class ClickIndicator : MonoBehaviour {

    public float showActionForSecs = 2f;
    private float timeSinceActionActivated = 0.0f;

    public Sprite attackSprite;
    public Sprite moveSprite;

    SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(spriteRenderer.enabled == true)
        {
            timeSinceActionActivated += Time.deltaTime;
            if (timeSinceActionActivated >= showActionForSecs)
            {
                spriteRenderer.enabled = false;
            }
        }
	}

    public void ActivateAttack(Vector2 pos)
    {
        Activate(pos);
        spriteRenderer.sprite = attackSprite;
    }

    public void ActivateMoveSprite(Vector2 pos)
    {
        Activate(pos);
        spriteRenderer.sprite = moveSprite;
    }

    void Activate(Vector2 pos)
    {
        transform.position = pos;
        spriteRenderer.enabled = true;
        timeSinceActionActivated = 0.0f;
    }
}
