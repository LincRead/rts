using UnityEngine;
using System.Collections;

public class ClickIndicator : MonoBehaviour {

    public float showActionForSecs = 2f;
    private float timeSinceActionActivated = 0.0f;

    public Sprite attackSprite;
    public Sprite moveSprite;
    public Sprite moveCameraSprite;

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
        ActivateBounceEffect(pos);
        spriteRenderer.sprite = attackSprite;
    }

    public void ActivateMoveSprite(Vector2 pos)
    {
        ActivateBounceEffect(pos);
        spriteRenderer.sprite = moveSprite;
    }

    void ActivateBounceEffect(Vector2 pos)
    {
        transform.position = pos;
        spriteRenderer.enabled = true;
        timeSinceActionActivated = 0.0f;

        LeanTween.scale(gameObject, new Vector3(0.75f, 0.75f, 1.0f), 0.25f);
        LeanTween.scale(gameObject, new Vector3(1.0f, 1.0f, 1.0f), 0.25f).setDelay(0.25f);
    }

    public void ActivateMoveCamera()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = moveCameraSprite;
        timeSinceActionActivated = 0.0f;
    }

    public void DeactivateMoveCamera()
    {
        spriteRenderer.enabled = false;
    }
}
