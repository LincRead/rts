using UnityEngine;
using System.Collections;

public class FActor : MonoBehaviour, LockStep
{

    protected FPoint Fpos;
    protected FPoint Fvelocity = FPoint.Create(FInt.Create(0), FInt.Create(0));

    protected virtual void Start()
    {
        Fpos.X = FInt.FromFloat(transform.position.x);
        Fpos.Y = FInt.FromFloat(transform.position.y);
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
}
