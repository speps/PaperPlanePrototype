using UnityEngine;

public abstract class ObjectTriggerBase : MonoBehaviour
{
    protected FlightController flightController;
    protected FlightController.GameplayType gameplayType = FlightController.GameplayType.None;

    public FlightController.GameplayType GameplayType
    {
        get { return gameplayType; }
    }

    protected float feedbackTimer = 0;
    protected float feedbackAccum = 0;
    protected bool activated = false;

    public abstract void Build();

    internal GameObject particleSystemFeedback = null;

    public string LinkedObject = "";

    void Start()
    {
        if (Application.isPlaying)
        {
            feedbackTimer = GameData.Instance.Tweak.FeedbackTimer;
            Build();
        }
    }

    public void PlaySignal()
    {
        feedbackAccum = 0;
        if(particleSystemFeedback != null) particleSystemFeedback.particleEmitter.emit = true;
    }

    public void Activate()
    {
        //Debug.Log("Activate trigger");

        if (!activated)
        {
            particleSystemFeedback.GetComponent<ParticleRenderer>().material.mainTexture = (Texture)Resources.Load("Particles/img/" + particleSystemFeedback.GetComponent<ParticleRenderer>().material.mainTexture.name + "_color");
            activated = true;
        }

        GameObject go = ObjectPopUp.Find(LinkedObject);
        if (go != null)
        {
            ObjectPopUp objectPopUp = (ObjectPopUp)go.GetComponent(typeof(ObjectPopUp));
            if(objectPopUp.data.IsSketch)
            {
                objectPopUp.Touch();
                return;
            }
            //Debug.Log(string.Format("PopUp {0} state {1}", gameObject.name, objectPopUp.state));
            if (objectPopUp != null && objectPopUp.state == ObjectPopUp.State.Popped)
            {
                objectPopUp.GoToNextState();
            }
        }
    }

    public void Deactivate()
    {
        /*GameObject go = GameObject.Find(HelperConstants.PrefixPopUp + LinkedObject);
        if (go != null)
        {
            ObjectPopUp objectPopUp = (ObjectPopUp)go.GetComponent(typeof(ObjectPopUp));
            if(objectPopUp.data.IsSketch)
            {
                return;
            }
            if (objectPopUp != null && objectPopUp.state == ObjectPopUp.State.Activated)
            {
                objectPopUp.GoToPreviousState();
            }
        }*/
    }


    protected void CheckController()
    {
        if (flightController == null)
        {
            flightController = (FlightController)GameObject.Find("FlightController").GetComponent(typeof(FlightController));
        }
    }
}
