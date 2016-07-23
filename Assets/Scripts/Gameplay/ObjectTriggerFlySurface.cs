using UnityEngine;

public class ObjectTriggerFlySurface : ObjectTriggerBase
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public float minimumDistance = 0.0f;

    public Axis direction = Axis.X;
    public bool positive = true;
    public bool turnAlsoLifts = false;

    public float cameraRatio = 1.0f;

    private static GameObject particlePrefab;

    public Vector3 Direction
    {
        get
        {
            Vector3 dir = Vector3.right;
            if (direction == Axis.Y)
            {
                dir = Vector3.up;
            }
            else if (direction == Axis.Z)
            {
                dir = Vector3.forward;
            }
            if (!positive)
                dir *= -1;

            return dir;
        }
    }

    public void Awake()
    {
        if (particlePrefab == null)
        {
            particlePrefab = (GameObject)Resources.Load("Particles/FlySurfaceParticle");
        }
    }

    public override void Build()
    {
        gameplayType = FlightController.GameplayType.FlySurface;
        BoxCollider current = (BoxCollider)gameObject.AddComponent(typeof(BoxCollider));
        current.isTrigger = true;
        current.size = Vector3.one;

        /* Create mesh for particle mesh renderer */
        Mesh feedbackMesh = new Mesh();
        Vector3[] feedbackFace = new Vector3[4];
        int[] triangles = new int[3*2];


        for (int i = 0; i < 4; ++i)
            feedbackFace[i] = Vector3.zero + Direction.normalized * 0.5f;

        if(direction == Axis.X)
        {
            

            feedbackFace[0].z = -0.5f;
            feedbackFace[1].z = -0.5f;
            feedbackFace[2].z = 0.5f;
            feedbackFace[3].z = 0.5f;

            feedbackFace[0].y = -0.5f;
            feedbackFace[1].y = 0.5f;
            feedbackFace[2].y = 0.5f;
            feedbackFace[3].y = -0.5f;

            triangles[0] = 3; triangles[3] = 2;
            triangles[1] = 1; triangles[4] = 1;
            triangles[2] = 0; triangles[5] = 3;

            feedbackMesh.vertices = feedbackFace;
            feedbackMesh.triangles = triangles;
            feedbackMesh.name = "SurfaceParticle";
            feedbackMesh.RecalculateBounds();
            feedbackMesh.RecalculateNormals();
        }

        if(direction == Axis.Z)
        {
            feedbackFace[0].x = -0.5f;
            feedbackFace[1].x = -0.5f;
            feedbackFace[2].x = 0.5f;
            feedbackFace[3].x = 0.5f;

            feedbackFace[0].y = -0.5f;
            feedbackFace[1].y = 0.5f;
            feedbackFace[2].y = 0.5f;
            feedbackFace[3].y = -0.5f;

            triangles[0] = 3; triangles[3] = 2;
            triangles[1] = 1; triangles[4] = 1;
            triangles[2] = 0; triangles[5] = 3;

            feedbackMesh.vertices = feedbackFace;
            feedbackMesh.triangles = triangles;
            feedbackMesh.name = "SurfaceParticle";
            feedbackMesh.RecalculateBounds();
            feedbackMesh.RecalculateNormals();
        }

        if(direction == Axis.Y)
        {
            feedbackFace[0].z = -0.5f;
            feedbackFace[1].z = -0.5f;
            feedbackFace[2].z = 0.5f;
            feedbackFace[3].z = 0.5f;

            feedbackFace[0].x = -0.5f;
            feedbackFace[1].x = 0.5f;
            feedbackFace[2].x = 0.5f;
            feedbackFace[3].x = -0.5f;

            triangles[0] = 3; triangles[3] = 2;
            triangles[1] = 1; triangles[4] = 1;
            triangles[2] = 0; triangles[5] = 3;

            feedbackMesh.vertices = feedbackFace;
            feedbackMesh.triangles = triangles;
            feedbackMesh.name = "SurfaceParticle";
            feedbackMesh.RecalculateBounds();
            feedbackMesh.RecalculateNormals();
        }

        particleSystemFeedback = (GameObject)GameObject.Instantiate(particlePrefab);
        if (particleSystemFeedback != null)
        {
            particleSystemFeedback.name = "SurfaceParticleSystem";
            particleSystemFeedback.transform.parent = transform;
            particleSystemFeedback.transform.localPosition = Vector3.zero;
            particleSystemFeedback.transform.localRotation = Quaternion.identity;

            // Clone material so we can change color of particles per trigger
            particleSystemFeedback.renderer.material = (Material)Instantiate(particleSystemFeedback.renderer.material);

            particleSystemFeedback.particleEmitter.localVelocity = Direction*GameData.Instance.Tweak.FlySurfaceParticleVelocity;
            particleSystemFeedback.particleEmitter.minEnergy = particleSystemFeedback.particleEmitter.maxEnergy = transform.localScale.z / GameData.Instance.Tweak.FlySurfaceParticleVelocity;
            particleSystemFeedback.GetComponent<ParticleRenderer>().uvAnimationCycles = particleSystemFeedback.particleEmitter.minEnergy;
            particleSystemFeedback.particleEmitter.emit = false;
            MeshFilter meshFilter = (MeshFilter)particleSystemFeedback.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = feedbackMesh;
            particleSystemFeedback.transform.localScale = Vector3.one;
        }
        
        
        
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Vector3 dir = Direction;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Vector3.zero, dir);

        Gizmos.matrix = Matrix4x4.identity;
    }

    void Update()
    {
        if(feedbackAccum < feedbackTimer)
        {
            
            feedbackAccum += Time.deltaTime;
            Vector3 angleVect = Direction;
            angleVect *= 180 * Time.deltaTime;
            //if (particleSystem != null) particleSystem.transform.localRotation *= Quaternion.Euler(angleVect.x, angleVect.y, angleVect.z);
        }
        else
        {
            particleSystemFeedback.particleEmitter.emit = false;
            //Debug.Log("Tube disabled:"+gameObject.name);
        }
    }
}
