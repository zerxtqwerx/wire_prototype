using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField] Transform startTransform, endTransform;
    [SerializeField] int segmentCount = 10;
    [SerializeField] float totalLength = 10;

    [SerializeField] float radius = 0.5f;
    [SerializeField] int sides = 4;

    [SerializeField] float totalWeight = 10;
    [SerializeField] float drag = 1;
    [SerializeField] float angularDrag = 1;

    [SerializeField] bool usePhysics = false;

    Transform[] segments = new Transform[0];
    [SerializeField] Transform segmentParent;

    private float prevSegmentCount;
    private float prevTotalLength;
    private float prevTotalWeight;
    private float prevDrag;
    private float prevAngularDrag;
    private float prevRadius;

    private MeshDataRope meshData;

    private Vector3[] vertices;
    private int[,] vertexIndicesMap;
    private Mesh mesh;
    private bool createTriangles;

    void Start()
    {
        /*segments = new Transform[segmentCount];
        GenerateSegments();*/
        vertices = new Vector3[segmentCount * sides * 3];
        GenerateMesh();
    }

    private void Update()
    {
        if (prevSegmentCount != segmentCount)
        {
            RemoveSegments();
            segments = new Transform[segmentCount];
            RemoveSegments();
            GenerateSegments();
            GenerateMesh();
        }
        prevSegmentCount = segmentCount;

        if (totalLength != prevTotalLength || prevDrag != drag || prevTotalWeight != totalWeight || prevAngularDrag != angularDrag)
        {
            UpdateWire();
            GenerateMesh();
        }
        prevTotalLength = totalLength;
        prevTotalWeight = totalWeight;
        prevDrag = drag;
        prevAngularDrag = angularDrag;

        if (prevRadius != radius && usePhysics)
        {
            UpdateRadius();
            GenerateMesh();
        }
        prevRadius = radius;
        UpdateMesh();
    }

    private void UpdateRadius()
    {
        for(int i = 0; i < segments.Length; i++) 
        {
            SetRadiusOnSegment(segments[i], radius);
        }
    }

    void UpdateMesh()
    {
        GenerateVertices();
        meshFilter.mesh.vertices = vertices;
    }
    private void GenerateMesh()
    {
        createTriangles = true;

        if(meshData == null)
        {
            meshData = new MeshDataRope(sides, segmentCount + 1, false);
        }
        else
        {
            meshData.ResetMesh(sides, segmentCount + 1, false);
        }

        GenerateIndicesMap();
        GenerateVertices();
        meshData.ProcessMesh();
        mesh = meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;

        createTriangles = false;
    }
    private void GenerateIndicesMap()
    {
        vertexIndicesMap = new int[segmentCount + 1, sides + 1];
        int meshVertexIndex = 0;
        for(int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            for(int sideIndex = 0; sideIndex < sides; sideIndex++)
            {
                vertexIndicesMap[segmentIndex, sideIndex] = meshVertexIndex;
                meshVertexIndex++;
            }
        }
    }

    private void GenerateVertices()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            GenerateCircleVerticesAndTriangles(segments[i], i);
        }
    }

    private void GenerateCircleVerticesAndTriangles(Transform segmentTransform, int segmentIndex)
    {
        float angleDiff = 360 / sides;
        Quaternion diffRotation = Quaternion.FromToRotation(Vector3.forward, segmentTransform.forward);
        for(int sideIndex = 0; sideIndex < sides; sideIndex++)
        {
            float angleInRad = sideIndex * angleDiff * Mathf.Deg2Rad;
            float x = -1 * radius * Mathf.Cos(angleInRad);
            float y = radius * Mathf.Sin(angleInRad);

            Vector3 pointOffset = new(x, y, 0);

            Vector3 pointRotated = diffRotation * pointOffset;

            Vector3 pointRotatedAtCenterOfTransform = segmentTransform.position + pointRotated;

            int vertexIndex = segmentIndex * sides + sideIndex;
            vertices[vertexIndex] = pointRotatedAtCenterOfTransform;
            //tempVertecies.Add(pointRotatedAtCenterOfTransform);

            if (createTriangles)
            {
                meshData.AddVertex(pointRotatedAtCenterOfTransform, new(0, 0), vertexIndex);

                bool createThisTriangle = segmentIndex < segmentCount - 1;
                if(createThisTriangle)
                {
                    int currentIncrement = 1;
                    int a = vertexIndicesMap[segmentIndex, sideIndex];
                    int b = vertexIndicesMap[segmentIndex + currentIncrement , sideIndex];
                    int c = vertexIndicesMap[segmentIndex, sideIndex + currentIncrement];
                    int d = vertexIndicesMap[segmentIndex + currentIncrement, sideIndex + currentIncrement];

                    bool isLastGap = sideIndex == sides - 1;
                    if(isLastGap)
                    {
                        c = vertexIndicesMap[segmentIndex, 0];
                        d = vertexIndicesMap[segmentIndex + currentIncrement, 0];
                    }
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }
    }

    private void SetRadiusOnSegment(Transform transform, float radius)
    {
        SphereCollider sphereCollider = transform.gameObject.GetComponent<SphereCollider>();
        sphereCollider.radius = radius;
    }

    private void UpdateWire()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            if(i != 0)
            {
                UpdateLengthOnSegment(segments[i], totalLength / segmentCount);
            }
            UpdateWeightOnSegment(segments[i], totalWeight, drag, angularDrag);
        }
    }

    private void UpdateWeightOnSegment(Transform transform, float totalWeight, float drag, float angularDrag)
    {
        Rigidbody rigidbody = transform.gameObject.GetComponent<Rigidbody>();
        rigidbody.mass = totalWeight / segmentCount;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag; 
    }
    private void UpdateLengthOnSegment(Transform transform, float v)
    {
        ConfigurableJoint joint = transform.gameObject.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            joint.connectedAnchor = Vector3.forward * totalLength / segmentCount;
        }
    }

    private void RemoveSegments()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            if (segments[i] != null)
            {
                Destroy(segments[i].gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            Gizmos.DrawWireSphere(segments[i].position, radius);
        }

        for(int i = 0; i < vertices.Length; i ++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
    private void GenerateSegments()
    {
        JoinSegment(startTransform, null, true);
        Transform prevTransform = startTransform;
        Vector3 direction = (endTransform.position - startTransform.position); //.normalized;

        for(int i = 0; i < segmentCount; i++)
        {
            GameObject segment = new GameObject($"segment_{i}");
            segment.transform.SetParent(segmentParent);
            segments[i] = segment.transform;

            Vector3 pos = prevTransform.position + (direction / segmentCount);
            segment.transform.position = pos;

            JoinSegment(segment.transform, prevTransform);

            prevTransform = segment.transform;
        }
        JoinSegment(endTransform, prevTransform, true, true);
    }

    private void JoinSegment(Transform current, Transform connectedTransform, bool isKinetic = false, bool isCloseConnected = false)
    {
        if (current.gameObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = current.gameObject.AddComponent<Rigidbody>();
            rigidbody.mass = totalWeight / segmentCount;
            rigidbody.isKinematic = isKinetic;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;
        }

        if(usePhysics)
        {
            SphereCollider sphereCollider = current.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = radius;
        }

        if(connectedTransform != null)
        {
            ConfigurableJoint joint = current.gameObject.AddComponent<ConfigurableJoint>();

            if(joint == null)
            {
                joint = current.gameObject.AddComponent<ConfigurableJoint>();
            }

            joint.connectedBody = connectedTransform.gameObject.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;

            if (isCloseConnected)
            {
                joint.connectedAnchor = Vector3.forward * 0.1f;
            }
            else
            {
                joint.connectedAnchor = Vector3.forward * (totalLength / segmentCount);
            }

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit softJointLimit = new SoftJointLimit();
            softJointLimit.limit = 0;
            joint.angularZLimit = softJointLimit;

            JointDrive jointDrive = new JointDrive();
            jointDrive.positionDamper = 0;
            jointDrive.positionSpring = 0;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;

        }
    }
}
