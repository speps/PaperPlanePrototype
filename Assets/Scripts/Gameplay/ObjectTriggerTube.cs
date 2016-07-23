using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class ObjectTriggerTube : ObjectTriggerBase
{
    public Helper.TubeType tubeType = Helper.TubeType.Vertical;
    public float length = 1.0f;
    public float radius = 0.5f;
    int resolution = 12;
    public float winLaps = 1.5f;
    public bool negateParticlesRotation;

    public Vector3 Direction
    {
        get { return tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward; }
    }
    private static GameObject particlePrefab;

    Mesh tubeMesh;

    void Awake()
    {
        if (particlePrefab == null)
        {
            particlePrefab = (GameObject)Resources.Load("Particles/TubeParticle");
        }
    }

    public override void Build()
    {
        gameplayType = tubeType == Helper.TubeType.Vertical ? FlightController.GameplayType.TubeVertical : FlightController.GameplayType.TubeHorizontal;

        Mesh feedbackMesh = new Mesh();
        feedbackMesh = Helper.BuildDisk(tubeType, length, radius, resolution);

        tubeMesh = Helper.BuildTube(tubeType, length, radius, resolution);
        MeshCollider c = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
        c.sharedMesh = tubeMesh;
        c.isTrigger = true;
        c.convex = true;

        particleSystemFeedback = (GameObject)GameObject.Instantiate(particlePrefab);
        particleSystemFeedback.name = "ParticleSystem";
        particleSystemFeedback.transform.parent = transform;
        particleSystemFeedback.transform.localPosition = -Direction * length * 0.5f;
        particleSystemFeedback.transform.localRotation = Quaternion.identity;

        // Clone material so we can change color of particles per trigger
        particleSystemFeedback.renderer.material = (Material)Instantiate(particleSystemFeedback.renderer.material);

        if (tubeType == Helper.TubeType.Horizontal || tubeType == Helper.TubeType.Vertical)
        {
            particleSystemFeedback.particleEmitter.localVelocity = Direction * GameData.Instance.Tweak.TubeParticleVelocity;
            particleSystemFeedback.particleEmitter.minEnergy = particleSystemFeedback.particleEmitter.maxEnergy = length / GameData.Instance.Tweak.TubeParticleVelocity;
            particleSystemFeedback.GetComponent<ParticleRenderer>().uvAnimationCycles = particleSystemFeedback.particleEmitter.minEnergy;
        }
        particleSystemFeedback.particleEmitter.emit = false;

        MeshFilter meshFilter = (MeshFilter)particleSystemFeedback.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = feedbackMesh;
        particleSystemFeedback.transform.localScale = Vector3.one;

        Rigidbody rb = (Rigidbody)gameObject.AddComponent(typeof(Rigidbody));
        rb.isKinematic = true;
        //GameObject.Destroy(particlePrefab);
    }



    void DrawCircle(int resolution, Vector3 position)
    {
        float angleDelta = 2 * Mathf.PI / Mathf.Max(1, resolution);

        Vector3 previousPoint = Vector3.zero;
        for (int n = 0; n <= resolution; ++n)
        {
            float angle = angleDelta * n;
            Vector3 p = GetCirclePoint(angle);

            Vector3 point = position + p;

            if (n > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            previousPoint = point;
        }
    }

    Vector3 GetCirclePoint(float angle)
    {
        return new Vector3(radius * Mathf.Cos(angle), tubeType == Helper.TubeType.Vertical ? 0.0f : radius * Mathf.Sin(angle), tubeType == Helper.TubeType.Vertical ? radius * Mathf.Sin(angle) : 0.0f);
    }

    public Vector3 GetPosition(Vector3 position)
    {
        float t;
        return GetPosition(position, out t);
    }

    public Vector3 GetPosition(Vector3 position, out float t)
    {
        Vector3 delta = (tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward) * length * 0.5f;
        Vector3 a = transform.localToWorldMatrix.MultiplyPoint3x4(delta);
        Vector3 b = transform.localToWorldMatrix.MultiplyPoint3x4(-delta);

        Vector3 proj = Helper.ProjectOntoLine(a, b, position, out t);

        return proj;
    }

    void OnDrawGizmos()
    {
        float angle = Vector3.Angle(Vector3.up, transform.up);

        //Debug.Log(string.Format("Angle {0}", angle));

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = tubeType == Helper.TubeType.Vertical ? Color.blue : Color.red;

        Vector3 delta = (tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward) * length * 0.5f;

        Gizmos.DrawLine(delta, -delta);

        DrawCircle(resolution, delta);
        DrawCircle(resolution, -delta);

        Gizmos.DrawLine(delta + GetCirclePoint(0.0f), -delta + GetCirclePoint(0.0f));
        Gizmos.DrawLine(delta + GetCirclePoint(Mathf.PI), -delta + GetCirclePoint(Mathf.PI));
        Gizmos.DrawLine(delta + GetCirclePoint(Mathf.PI / 2.0f), -delta + GetCirclePoint(Mathf.PI / 2.0f));
        Gizmos.DrawLine(delta + GetCirclePoint(3.0f * Mathf.PI / 2.0f), -delta + GetCirclePoint(3.0f * Mathf.PI / 2.0f));

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.yellow;

        //Debug.Log(string.Format("Angle {0}", ));

        //if(drawSignal) Gizmos.DrawSphere(transform.position, 0.1f);

        /*GameObject flightControllerObject = GameObject.Find("FlightController");
        if (flightControllerObject != null && tubeType == Helper.TubeType.Horizontal)
        {
            FlightController flightController = (FlightController)flightControllerObject.GetComponent(typeof(FlightController));
            if (flightController != null)
            {
                Vector3 triggerCenter = GetPosition(flightController.transform.position);
                Vector3 orthoVelocity = Vector3.Cross(Direction, Vector3.Project(flightController.velocity, Vector3.up));
                Vector3 orthoProj = Helper.ProjectOntoLine(triggerCenter, triggerCenter + orthoVelocity, flightController.transform.position);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(orthoProj, 0.05f);
                Gizmos.DrawLine(triggerCenter, triggerCenter + orthoVelocity);
            }
        }*/
    }

    /*void OnTriggerEnter(Collider other)
    {
        CheckController();
        Debug.Log("Entered");
        flightController.GameplayStart(tubeType == TubeType.Vertical ? FlightController.GameplayType.TubeVertical : FlightController.GameplayType.TubeHorizontal);
    }

    void OnTriggerExit(Collider other)
    {
        CheckController();
        Debug.Log("Exited");
        flightController.GameplayEnd(tubeType == TubeType.Vertical ? FlightController.GameplayType.TubeVertical : FlightController.GameplayType.TubeHorizontal);
    }*/

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            transform.localScale = Vector3.one;
            return;
        }

        if(feedbackAccum < feedbackTimer)
        {
            feedbackAccum += Time.deltaTime;
        }
        else
        {
            particleSystemFeedback.particleEmitter.emit = false;
        }

        Vector3 angleVect = Direction;
        angleVect *= (negateParticlesRotation ? -1 : +1) *GameData.Instance.Tweak.TubeParticleAngularVelocity * Time.deltaTime;
        if (particleSystemFeedback != null) particleSystemFeedback.transform.localRotation *= Quaternion.Euler(angleVect.x, angleVect.y, angleVect.z);
    }
}
