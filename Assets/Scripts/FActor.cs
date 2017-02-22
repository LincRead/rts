using UnityEngine;
using System.Collections;

public class FActor : MonoBehaviour, LockStep
{
    protected FPoint Fpos;
    protected FPoint Fvelocity = FPoint.Create(FInt.Create(0), FInt.Create(0));

    protected bool colliding = false;

    private SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        Fpos.X = FInt.FromFloat(transform.localPosition.x);
        Fpos.Y = FInt.FromFloat(transform.localPosition.y);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void LockStepUpdate()
    {

    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    public FPoint GetFPosition()
    {
        return Fpos;
    }

    public FPoint GetFVelocity()
    {
        return Fvelocity;
    }

    public Vector3 GetRealPosToVector3()
    {
        return new Vector3(Fpos.X.ToFloat(), Fpos.Y.ToFloat(), Fpos.Y.ToFloat());
    }

    public FRectangle GetCollisionRectangle()
    {
        FInt centerY = Fpos.Y - FInt.FromFloat(GetComponent<SpriteRenderer>().bounds.size.y / 2);

        if (spriteRenderer != null && spriteRenderer.sprite.pivot.y == 0)
            centerY += FInt.FromFloat(GetComponent<SpriteRenderer>().bounds.size.y / 2);

        return FRectangle.Create(
            Fpos.X - FInt.FromFloat(GetComponent<SpriteRenderer>().bounds.size.x / 2),
            centerY,
            FInt.FromFloat(GetComponent<SpriteRenderer>().bounds.size.x),
            FInt.FromFloat(GetComponent<SpriteRenderer>().bounds.size.y));
    }

    protected void ResetVelocity()
    {
        Fvelocity.X = FInt.Create(0);
        Fvelocity.Y = FInt.Create(0);
    }

    void OnDrawGizmos()
    {
        // Show grid size
        if(colliding)
            Gizmos.color = new Color(1.0f, 0.0f, 1.0f, 0.5f);
        else
            Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);

        FRectangle rect = GetCollisionRectangle();
        Gizmos.DrawWireCube(GetRealPosToVector3(), new Vector3(rect.W.ToFloat(), rect.H.ToFloat(), 0.0f));
    }
}
