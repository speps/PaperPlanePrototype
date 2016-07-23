using UnityEngine;



public class FadeTrigger : ObjectTriggerBase {


    public override void Build()
    {
        CapsuleCollider c = (CapsuleCollider)gameObject.AddComponent(typeof(CapsuleCollider));
        c.isTrigger = true;
        Rigidbody rb = (Rigidbody)gameObject.AddComponent(typeof(Rigidbody));
        rb.isKinematic = true;
        gameplayType = FlightController.GameplayType.LevelLimits;
    }
}
