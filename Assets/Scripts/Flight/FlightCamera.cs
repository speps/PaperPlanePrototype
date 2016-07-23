using UnityEngine;
using System.Collections;

public class FlightCamera : MonoBehaviour
{
    public static float maxDistance = 0.5f;
    public Vector3 tubeCamDelta;
    public float fovMin = 60, fovMax = 90;
    public float fadeSpeed = 2.0f;

    bool initDone = false;
    Vector3 startPosition;
    Quaternion startRotation;

    GameObject flightControllerObject;
    GameObject flightCamera;
    FlightController flightController;
    GameObject plane;
    Fader fader;
    private float fadeFov = 0;
    private bool initFade = true;
    private bool boostTrail = false;
    
    // Use this for initialization
    void Start()
    {
        
    }

    public void Reset()
    {
        Reset(false);
    }

    public void Reset(bool force)
    {
        if (!initDone || force)
        {
            startPosition = transform.position;
            startRotation = transform.rotation;

            initDone = true;
        }
    }

    public void Init()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (flightControllerObject == null)
        {
            flightControllerObject = GameObject.Find("FlightController");
            flightController = (FlightController)flightControllerObject.GetComponent(typeof(FlightController));
            plane = ((FlightController)flightControllerObject.GetComponent(typeof(FlightController))).PlaneModel;
        }
        if (fader == null)
        {
            GameObject faderObject = GameObject.FindWithTag("CurrentCamera");
            fader = faderObject.GetComponent<Fader>();
        }

        if (flightController.CurrentGameplayType == FlightController.GameplayType.NormalFlight)
        {
            CameraNormalFlight();
        }
        else if (flightController.CurrentGameplayType == FlightController.GameplayType.LevelLimits)
        {
            CameraNormalFlight();
        }
        else if (flightController.CurrentGameplayType == FlightController.GameplayType.TubeVertical)
        {
            CameraTubeVertical();
        }
        else if (flightController.CurrentGameplayType == FlightController.GameplayType.TubeHorizontal)
        {
            CameraTubeHorizontal();
        }
        else if(flightController.CurrentGameplayType == FlightController.GameplayType.FlyThrough)
        {
            CameraFlyThrough();
        }
        else if(flightController.CurrentGameplayType == FlightController.GameplayType.FlySurface)
        {
            CameraFlySurface();
        }

        FadeFov();
    }

    public void InitCamera()
    {
        switch (flightController.CurrentGameplayType)
        {
            case FlightController.GameplayType.TubeVertical:
                /* Place camera */
                {
                    ObjectTriggerTube trigger = (ObjectTriggerTube)flightController.CurrentTrigger;

                    Vector3 triggerCenter = trigger.GetPosition(flightController.transform.position);
                    Vector3 orthoVelocity = Vector3.Cross(trigger.Direction, flightController.velocity);
                    Vector3 orthoProj = Helper.ProjectOntoLine(triggerCenter, triggerCenter + orthoVelocity, flightController.transform.position);

                    tubeCamDelta = (orthoProj - triggerCenter).normalized * trigger.radius * 1.5f;
                }

                break;

            case FlightController.GameplayType.TubeHorizontal:
                /* Place camera */
                {
                    ObjectTriggerTube trigger = (ObjectTriggerTube)flightController.CurrentTrigger;

                    Vector3 triggerCenter = trigger.GetPosition(flightController.transform.position);
                    Vector3 orthoVelocity = Vector3.Cross(trigger.Direction, Vector3.Project(flightController.velocity, Vector3.up));
                    Vector3 orthoProj = Helper.ProjectOntoLine(triggerCenter, triggerCenter + orthoVelocity, flightController.transform.position);

                    tubeCamDelta = (orthoProj - triggerCenter).normalized * trigger.radius * 1.5f;
                }

                break;
        }
    }

    void CameraNormalFlight()
    {
        //if (fader.fadeAccum <= 0.0 || fader.fadeDir < 0)
        {
            Vector3 delta;
            if (flightControllerObject.rigidbody == null || flightControllerObject.rigidbody.isKinematic)
            {
                delta = flightControllerObject.transform.position - transform.position;
            }
            else
            {
                delta = plane.transform.position - transform.position;
            }

            float distance = delta.magnitude;
            // Keep distance from camera
            {
                float diff = (0.01f + 1.0f * (1.0f - flightController.GameplayAlpha)) * (distance - maxDistance) / distance;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 0.1f);
                transform.position += delta * diff;
            }
        }
        /*else // Fix camera on plane back to smooth with initial position
        {
            //Vector3 dir = flightController.velocity;
            //Vector3 up = flightController.transform.rotation * Vector3.up;

            Vector3 flightPosition = Vector3.zero;
            Quaternion flightRotation = Quaternion.identity;
            if (flightControllerObject.rigidbody == null || flightControllerObject.rigidbody.isKinematic)
            {
                flightPosition = flightControllerObject.transform.position;
                flightRotation = flightControllerObject.transform.rotation;
            }
            else
            {
                flightPosition = plane.transform.position;
                flightRotation = plane.transform.rotation;
            }

            Vector3 positionTarget = flightPosition + (flightController.transform.rotation * Vector3.back) * maxDistance;

            transform.rotation = Quaternion.Slerp(transform.rotation, flightRotation, Helper.EaseOut(fader.fadeAccum) * 0.5f);
            transform.position += (positionTarget - transform.position) * Helper.EaseOut(fader.fadeAccum) * 0.5f;
        }*/
    }

    void CameraTubeVertical()
    {
        ObjectTriggerTube trigger = (ObjectTriggerTube)flightController.CurrentTrigger;

        Vector3 proj = trigger.GetPosition(flightController.transform.position);

        Vector3 lookDir = flightController.transform.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 0.1f + 0.4f * flightController.GameplayAlpha);
        transform.position += ((proj + tubeCamDelta) - transform.position) * (0.001f + 0.05f * flightController.GameplayAlpha);
    }


    public void FadeOn()
    {
        initFade = false;
    }

    public void FadeOff()
    {
        initFade = true;
    }

    void CameraTubeHorizontal()
    {
        ObjectTriggerTube trigger = (ObjectTriggerTube)flightController.CurrentTrigger;

        Vector3 proj = trigger.GetPosition(flightController.transform.position);

        Vector3 delta = proj - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 0.1f + 0.4f * flightController.GameplayAlpha);
        transform.position += ((proj + tubeCamDelta) - transform.position) * (0.01f + 0.1f * flightController.GameplayAlpha);
    }


    void CameraFlyThrough()
    {
        if (initFade)
        {
            fadeFov = 0;
            initFade = false;
        }
        
        
        //Debug.Log(Mathf.Lerp(60, 90, fadeFov));
        FadeFov();
        
        Vector3 delta;
        if (flightControllerObject.rigidbody == null || flightControllerObject.rigidbody.isKinematic)
        {
            delta = flightControllerObject.transform.position - transform.position;
        }
        else
        {
            delta = plane.transform.position - transform.position;
        }

        float distance = delta.magnitude;
        // Maximum distance from camera
        if (distance > 0)
        {
            float diff = (0.1f + 1.0f * (1.0f - flightController.GameplayAlpha)) * (distance - maxDistance) / distance;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 0.1f);
            transform.position += delta * diff;
            
        }
    }

    void CameraFlySurface()
    {
        ObjectTriggerFlySurface trigger = (ObjectTriggerFlySurface)flightController.CurrentTrigger;
        Vector3 dir = trigger.transform.TransformDirection(trigger.Direction);

        Vector3 flightPosition = Vector3.zero;
        //Quaternion flightRotation = Quaternion.identity;
        if (flightControllerObject.rigidbody == null || flightControllerObject.rigidbody.isKinematic)
        {
            flightPosition = flightControllerObject.transform.position;
            //flightRotation = flightControllerObject.transform.rotation;
        }
        else
        {
            flightPosition = plane.transform.position;
            //flightRotation = plane.transform.rotation;
        }

        Vector3 positionTarget = flightPosition + (flightController.transform.rotation * Vector3.back) * maxDistance + dir * (-0.2f * Mathf.Max(0.0f, trigger.cameraRatio));

        transform.position += (positionTarget - transform.position) * (0.01f + 0.1f * flightController.GameplayAlpha);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flightPosition - transform.position), 0.1f);
    }

    void OnDrawGizmos()
    {
        /*if (flightController.CurrentTrigger is ObjectTriggerTube)
        {
            ObjectTriggerTube trigger = (ObjectTriggerTube)flightController.CurrentTrigger;

            //Vector3 triggerCenter = trigger.GetPosition(flightController.transform.position);
            //Vector3 orthoVelocity = Vector3.Cross(trigger.Direction, flightController.velocity);
            //Vector3 orthoProj = Helper.ProjectOntoLine(triggerCenter, triggerCenter + orthoVelocity, flightController.transform.position);

            Gizmos.color = Color.red;
            //Gizmos.DrawSphere(orthoProj, 0.1f);
        }*/
    }

    void FadeFov()
    {
        if(initFade == false)
        {
            fadeFov += Time.deltaTime*fadeSpeed;
            fadeFov = Mathf.Clamp01(fadeFov);
        }
        else
        {
            fadeFov -= Time.deltaTime*fadeSpeed;
            fadeFov = Mathf.Clamp01(fadeFov);
        }
        GameObject currentCamera = GameObject.FindWithTag("CurrentCamera");
        if(currentCamera != null) currentCamera.camera.fieldOfView = Mathf.Lerp(fovMin, fovMax, fadeFov);
        
    }
}
