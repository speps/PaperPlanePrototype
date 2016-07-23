using UnityEngine;

public class CollisionPropagate : MonoBehaviour
{
    public FlightController.CollisionType type = FlightController.CollisionType.Normal;

    FlightController flightController;

    protected void CheckController()
    {
        if (flightController == null)
        {
            flightController = (FlightController)GameObject.Find("FlightController").GetComponent(typeof(FlightController));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent(typeof(FadeTrigger)) == null)
        {
            CheckController();
            flightController.TriggerEnter(gameObject, other, type);
        }
    }

    void OnTriggerExit(Collider other)
    {
        FadeTrigger fadeTrigger = other.GetComponent<FadeTrigger>();
        if(fadeTrigger != null)
        {
            CheckController();
            flightController.ReachLevelLimits(fadeTrigger);
        }
        else
        {
            CheckController();
            flightController.TriggerExit(gameObject, other, type);
        }
        
    }
}
