using System.Collections;
using UnityEngine;

public class SketchCamera : MonoBehaviour
{
    public GameObject sketchController= null;
    public GameObject target = null;
    GameObject targetAnimate = null;

    GameObject previousCamera;
    PlayMode previousPlayMode;

    bool exit = false;
    bool animating = true;

    Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public void Reset()
    {
        target = null;

        previousPlayMode = Main.Instance.PlayMode;
        previousCamera = Main.Instance.PreviousCamera;

        transform.position = previousCamera.transform.position;
        transform.rotation = previousCamera.transform.rotation;
    }

    void AnimateRotation(Quaternion v, Animate.Event ev)
    {
        transform.rotation = v;
        if (ev == Animate.Event.End)
        {
            Main.Instance.SketchController.StartAnimation();
            animating = false;
        }
    }

    void AnimatePosition(Vector3 v, Animate.Event ev)
    {
        transform.position = v;
    }

    public void StartAnimation(GameObject to, float duration)
    {
        targetAnimate = to;
        animating = true;

        //Hashtable props = new Hashtable();
        //props.Add("rotation", to.transform.rotation*Quaternion.Euler(new Vector3(0,180,0)));
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.InOut);
        //props.Add("drive", typeof(Ani.Drive.Slerp));
        //props.Add("atEnd", new AtEndDelegate(Main.Instance.SketchController.Animate));

        //Ani.Mate.Stop(transform);
        //Ani.Mate.To(transform, duration, props);

        StartCoroutine(Animate.LerpTo(0.0f, duration, this.transform.rotation, to.transform.rotation * Quaternion.Euler(new Vector3(0, 180, 0)), AnimateRotation, Animate.QuadraticInOut));

        //props.Clear();
        //props.Add("position", to.transform.position);
        //props.Add("easing", typeof(Ani.Easing.Quintic));
        //props.Add("direction", Ani.Easing.InOut);

        //Ani.Mate.Stop(this);
        //Ani.Mate.To(this, duration, props);

        StartCoroutine(Animate.LerpTo(0.0f, duration, this.transform.position, to.transform.position, AnimatePosition, Animate.QuadraticInOut));
    }

    public void Follow(GameObject cam)
    {
        target = cam;
    }

    public void Update()
    {
        if (target != null && !animating)
        {
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation*Quaternion.Euler(new Vector3(0, 180, 0));
        }
    }
}
