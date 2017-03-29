using UnityEngine;
using System.Collections;

public class FActor : MonoBehaviour, LockStep
{
    [Header("FActor")]
    public int playerID = -1;

    protected FPoint Fpos;
    protected FPoint Fvelocity = FPoint.Create(FInt.Create(0), FInt.Create(0));

    public float boundingRadius = 1;
    protected FInt FboundingRadius;

    protected FInt FlargeNumber = FInt.Create(1000); // Memory

    protected Transform myTransform;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        myTransform = GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        Fpos.X = FInt.FromFloat(transform.localPosition.x);
        Fpos.Y = FInt.FromFloat(transform.localPosition.y);
        FboundingRadius = FInt.FromFloat(boundingRadius);
        transform.position = GetRealPosToVector3();
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

    public Vector3 GetRealPosToVector3() {
        float z = 0.0f;

        if (spriteRenderer != null)
            z = spriteRenderer.bounds.min.y;

        return new Vector3(Fpos.X.ToFloat(), Fpos.Y.ToFloat(), z);
    }

    public FInt GetDistanceToFActor(FActor actor)
    {
        if (actor == null) return FInt.Create(1000);
        return ((actor.GetFPosition().X - Fpos.X) * (actor.GetFPosition().X - Fpos.X)) + ((actor.GetFPosition().Y - Fpos.Y) * (actor.GetFPosition().Y - Fpos.Y));
    }

    public FInt GetDistanceBetweenPoints(FPoint a, FPoint b)
    {
        return ((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y));
    }

    public void SetFPosition(FPoint FposNew) { Fpos = FposNew; }
    public FPoint GetFPosition() { return Fpos; }
    public FPoint GetFVelocity() { return Fvelocity; }
    public FInt GetFBoundingRadius() { return FboundingRadius; }
}
