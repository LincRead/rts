using UnityEngine;
using System.Collections;

public class FActor : MonoBehaviour, LockStep
{
    [Header("FActor")]
    public int playerID = -1;

    protected FPoint Fpos;
    protected FPoint Fvelocity = FPoint.Create(FInt.Create(0), FInt.Create(0));

    public float boundingRadius = 1;
    FInt FboundingRadius;
    protected bool colliding = false;

    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        Fpos.X = FInt.FromFloat(transform.localPosition.x);
        Fpos.Y = FInt.FromFloat(transform.localPosition.y);
        FboundingRadius = FInt.FromFloat(boundingRadius);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void LockStepUpdate()
    {

    }

    protected void ResetVelocity()
    {
        Fvelocity.X = FInt.Create(0);
        Fvelocity.Y = FInt.Create(0);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    public void SetFPosition(FPoint FposNew) { Fpos = FposNew; }
    public FPoint GetFPosition() { return Fpos; }
    public FPoint GetFVelocity() { return Fvelocity; }
    public Vector3 GetRealPosToVector3() { return new Vector3(Fpos.X.ToFloat(), Fpos.Y.ToFloat(), Fpos.Y.ToFloat()); }
    public FInt GetFBoundingRadius() { return FboundingRadius; }

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
}
