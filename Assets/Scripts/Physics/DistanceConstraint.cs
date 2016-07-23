using UnityEngine;

public class DistanceConstraint : Constraint
{
    public Vector3 a, b;

    public float restLength;
    public float stiffness;

    public DistanceConstraint(float restLength, float stiffness)
    {
        this.restLength = restLength;
        this.stiffness = stiffness;
    }

    public override void Apply()
    {
        Vector3 delta = b - a;
        float distance = delta.magnitude;

        float diff = stiffness * (distance - restLength) / distance;
        a = a + delta * diff;
        b = b - delta * diff;
    }
}
