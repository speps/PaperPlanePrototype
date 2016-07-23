public abstract class Constraint
{
    bool active;
    public bool Active
    {
        get { return active; }
    }

    public void Activate()
    {
        active = true;
    }
    public void Deactivate()
    {
        active = false;
    }

    public abstract void Apply();
}
