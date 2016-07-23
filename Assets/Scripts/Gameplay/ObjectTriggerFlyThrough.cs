using UnityEngine;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class ObjectTriggerFlyThrough : ObjectTriggerBase
{
    Mesh tubeMesh;
    private Helper.TubeType tubeType = Helper.TubeType.Horizontal;
    private float length = 1.0f;
    private float radius = 1.0f;
    private int resolution = 12;

    public float minimumDistance = 0.0f;

    public bool approachAid = false;
    public float radiusEnter = 0.0f;
    public float radiusExit = 0.0f;
    public float approachLength = 1.0f;
    public bool enterBothSides = false;
    public float approachForceRatio = 1.0f;

    public float particleRadius = 1.0f;
    public int particleEmission = 0;
    public float particleLifeScale = 1.0f;

    private static GameObject particlePrefab;

    public enum ColliderType
    {
        Box,
        Tube
    }

    public ColliderType colliderType = ColliderType.Box;

    void Awake()
    {
        if (particlePrefab == null)
        {
            particlePrefab = (GameObject)Resources.Load("Particles/FlyThroughParticle");
        }
    }

    void Update()
    {
        radiusEnter = Mathf.Clamp(radiusEnter, 0.0f, radiusEnter);
        radiusExit = Mathf.Clamp(radiusExit, 0.0f, radiusExit);
        approachLength = Mathf.Clamp(approachLength, 0.0f, approachLength);

        if (feedbackAccum < feedbackTimer)
        {

            feedbackAccum += Time.deltaTime;
            Vector3 angleVect = Vector3.forward;
            angleVect *= 180 * Time.deltaTime;
        }
        else
        {
            if(particleSystemFeedback != null) particleSystemFeedback.particleEmitter.emit = false;
        }
    }

    public override void Build()
    {
        Mesh feedbackMesh = new Mesh();

        if(colliderType == ColliderType.Box)
        {
            Vector3[] feedbackPlane = new Vector3[4];

            int[] triangles = new int[3 * 2];

            for (int i = 0; i < 4; ++i)
                feedbackPlane[i] = Vector3.zero;
            feedbackPlane[0].y = -0.5f * particleRadius;
            feedbackPlane[1].y = 0.5f * particleRadius;
            feedbackPlane[2].y = 0.5f * particleRadius;
            feedbackPlane[3].y = -0.5f * particleRadius;

            feedbackPlane[0].x = 0.5f * particleRadius;
            feedbackPlane[1].x = 0.5f * particleRadius;
            feedbackPlane[2].x = -0.5f * particleRadius;
            feedbackPlane[3].x = -0.5f * particleRadius;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 3;

            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;

            feedbackMesh.vertices = feedbackPlane;
            feedbackMesh.triangles = triangles;
            feedbackMesh.name = "SurfaceParticle";
            feedbackMesh.RecalculateBounds();
            feedbackMesh.RecalculateNormals();
        }

        if (colliderType == ColliderType.Tube)
        {
            feedbackMesh = Helper.BuildDisk(tubeType, length, radius*particleRadius, resolution);

        }





        gameplayType = FlightController.GameplayType.FlyThrough;

        if (rigidbody == null)
        {
            Rigidbody rb = (Rigidbody)gameObject.AddComponent(typeof(Rigidbody));
            rb.isKinematic = true;
        }

        if (colliderType == ColliderType.Box)
        {
            BoxCollider current = (BoxCollider)gameObject.AddComponent(typeof(BoxCollider));
            current.isTrigger = true;
        }

        if (colliderType == ColliderType.Tube)
        {
            tubeMesh = Helper.BuildTube(tubeType, length, radius, resolution);
            MeshCollider c = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
            c.sharedMesh = tubeMesh;
            c.isTrigger = true;
            c.convex = true;
        }

        if (approachAid && radiusEnter > 0.0f && radiusExit > 0.0f)
        {
            GameObject goEnter = new GameObject("ApproachEnter");
            goEnter.transform.parent = transform;
            goEnter.transform.localPosition = -Vector3.forward * 0.5f;
            goEnter.transform.localRotation = Quaternion.identity;
            goEnter.transform.localScale = Vector3.one;

            ObjectTriggerApproach approachEnter = goEnter.AddComponent <ObjectTriggerApproach>();
            approachEnter.parent = this;
            approachEnter.length = approachLength / transform.localScale.z;
            approachEnter.radius1 = radiusExit;
            approachEnter.radius2 = radiusEnter;
            approachEnter.offset = -Vector3.forward * approachEnter.length * 0.5f;

            if (enterBothSides)
            {
                GameObject goExit = new GameObject("ApproachExit");
                goExit.transform.parent = transform;
                goExit.transform.localPosition = Vector3.forward * 0.5f;
                goExit.transform.localRotation = Quaternion.identity;
                goExit.transform.localScale = Vector3.one;

                ObjectTriggerApproach approachExit = goExit.AddComponent<ObjectTriggerApproach>();
                approachExit.parent = this;
                approachExit.length = approachLength / transform.localScale.z;
                approachExit.radius1 = radiusEnter;
                approachExit.radius2 = radiusExit;
                approachExit.offset = Vector3.forward * approachExit.length * 0.5f;
            }
        }

        particleSystemFeedback = (GameObject)GameObject.Instantiate(particlePrefab);
        if(particleSystemFeedback != null)
        {
            
        }
        particleSystemFeedback.name = "FlyThroughParticleSystem";
        particleSystemFeedback.transform.parent = transform;
        particleSystemFeedback.transform.localPosition = Vector3.forward * -0.5f * particleLifeScale;
        particleSystemFeedback.transform.localRotation = Quaternion.identity;

        // Clone material so we can change color of particles per trigger
        particleSystemFeedback.renderer.material = (Material)Instantiate(particleSystemFeedback.renderer.material);
        if (particleEmission > 0)
            particleSystemFeedback.particleEmitter.minEmission = particleSystemFeedback.particleEmitter.maxEmission = particleEmission;

        if (tubeType == Helper.TubeType.Horizontal)
        {
            particleSystemFeedback.particleEmitter.localVelocity = Vector3.forward*GameData.Instance.Tweak.FlyThroughParticleVelocity;
            particleSystemFeedback.particleEmitter.minEnergy = particleSystemFeedback.particleEmitter.maxEnergy = particleLifeScale * transform.localScale.z / GameData.Instance.Tweak.FlyThroughParticleVelocity;
            particleSystemFeedback.GetComponent<ParticleRenderer>().uvAnimationCycles = particleSystemFeedback.particleEmitter.minEnergy;
        }
        particleSystemFeedback.particleEmitter.emit = false;
        MeshFilter meshFilter = (MeshFilter)particleSystemFeedback.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = feedbackMesh;
        particleSystemFeedback.transform.localScale = Vector3.one;
    }


    public Vector3 GetPosition(Vector3 position)
    {
        float t;
        return GetPosition(position, out t);
    }

    public Vector3 GetPosition(Vector3 position, out float t)
    {
        Vector3 delta = (tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward) * length * (tubeType == Helper.TubeType.Vertical ? transform.localScale.y : transform.localScale.z) * 0.5f;
        Vector3 a = transform.localToWorldMatrix.MultiplyPoint3x4(delta);
        Vector3 b = transform.localToWorldMatrix.MultiplyPoint3x4(-delta);

        Vector3 proj = Helper.ProjectOntoLine(a, b, position, out t);

        return proj;
    }

    void DrawCircle(int resolution, Vector3 position, float radius)
    {
        float angleDelta = 2 * Mathf.PI / Mathf.Max(1, resolution);

        Vector3 previousPoint = Vector3.zero;
        for (int n = 0; n <= resolution; ++n)
        {
            float angle = angleDelta * n;
            Vector3 p = Helper.GetCirclePoint(angle, radius, tubeType);

            Vector3 point = position + p;

            if (n > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            previousPoint = point;
        }
    }

    void OnDrawGizmos()
    {
        if (colliderType == ColliderType.Box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }

        if (colliderType == ColliderType.Tube)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;

            Vector3 delta = (tubeType == Helper.TubeType.Vertical ? Vector3.up : Vector3.forward) * length * 0.5f;

            Gizmos.DrawLine(delta, -delta);

            DrawCircle(resolution, delta, radius);
            DrawCircle(resolution, -delta, radius);

            Gizmos.DrawLine(delta + Helper.GetCirclePoint(0.0f, radius, tubeType), -delta + Helper.GetCirclePoint(0.0f, radius, tubeType));
            Gizmos.DrawLine(delta + Helper.GetCirclePoint(Mathf.PI, radius, tubeType), -delta + Helper.GetCirclePoint(Mathf.PI, radius, tubeType));
            Gizmos.DrawLine(delta + Helper.GetCirclePoint(Mathf.PI / 2.0f, radius, tubeType), -delta + Helper.GetCirclePoint(Mathf.PI / 2.0f, radius, tubeType));
            Gizmos.DrawLine(delta + Helper.GetCirclePoint(3.0f * Mathf.PI / 2.0f, radius, tubeType), -delta + Helper.GetCirclePoint(3.0f * Mathf.PI / 2.0f, radius, tubeType));

            Gizmos.matrix = Matrix4x4.identity;
        }

        if (approachAid)
        {
            Gizmos.color = Color.gray;

            Gizmos.matrix = transform.localToWorldMatrix;

            DrawCircle(resolution, -Vector3.forward * 0.5f, radiusExit);
            DrawCircle(resolution, -Vector3.forward * 0.5f - Vector3.forward * approachLength / transform.localScale.z, radiusEnter);

            if (enterBothSides)
            {
                DrawCircle(resolution, Vector3.forward * 0.5f, radiusExit);
                DrawCircle(resolution, Vector3.forward * 0.5f + Vector3.forward * approachLength / transform.localScale.z, radiusEnter);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + transform.TransformDirection(Vector3.forward));
        
        /*GameObject flightControllerObject = GameObject.Find("FlightController");
        
        if (flightControllerObject != null)
        {
            Vector3 fligthProject = GetPosition(flightControllerObject.transform.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fligthProject, 0.1f);
        }*/
        

    }



}
