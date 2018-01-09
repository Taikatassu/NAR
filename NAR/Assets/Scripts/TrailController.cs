using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    // https://answers.unity.com/questions/1343711/trail-effect-for-non-moving-object.html
    // TODO: Fix jagged edges on the trail
    // Non-billboard version of the trail
    //      - Create a mesh for the trail in script, instead of using LineRenderer component?

    LineRenderer trail;
    [SerializeField]
    Material trailMaterial;

    float lifetime = 0.5f;
    float minimumVertexDistance = 0.05f;
    List<Vector3> points;
    Queue<float> spawnTimes = new Queue<float>();

    List<Transform> trailSegments;

    private void OnEnable()
    {
        InitializeTrail();
        EventManager.OnPlayerMovement += OnPlayerMovement;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
    }

    private void InitializeTrail()
    {
        //trail = GetComponent<LineRenderer>();

        points = new List<Vector3>() { transform.position };
        //trail.SetPositions(points.ToArray());

        trailSegments = new List<Transform>();

        Transform newSegment = CreateNewTrailSegment();
        newSegment.position = transform.position;

        trailSegments.Add(newSegment);

        //TODO: Continue creating trail from quads

    }

    private void UpdateTrailPoints(Vector2 positionChange)
    {
        while (spawnTimes.Count > 0 && spawnTimes.Peek() + lifetime < Time.time)
        {
            //RemovePoint();
            DeleteOldestSegment();
        }

        Vector3 diff = new Vector3(-positionChange.x, 0, -positionChange.y);
        for (int i = 0; i < trailSegments.Count; i++)
        {
            //points[i] += diff;
            trailSegments[i].position += diff;
        }

        if (points.Count < 2 || Vector3.Distance(transform.position, points[1]) > minimumVertexDistance)
        {
            //AddPoint(transform.position);
            AddNewSegment();
        }

        for (int i = 0; i < trailSegments.Count - 1; i++)
        {
            trailSegments[i].LookAt(trailSegments[i + 1]);
        }

        //points[0] = transform.position;

        //trail.positionCount = points.Count;
        //trail.SetPositions(points.ToArray());
    }

    //void AddPoint(Vector3 position)
    //{
    //    points.Insert(1, position);
    //    spawnTimes.Enqueue(Time.time);
    //}

    //void RemovePoint()
    //{
    //    spawnTimes.Dequeue();
    //    points.RemoveAt(points.Count - 1);
    //}

    private void AddNewSegment()
    {
        Transform newSegment = CreateNewTrailSegment();
        newSegment.position = transform.position;
        trailSegments.Insert(1, newSegment);
    }

    private Transform CreateNewTrailSegment()
    {
        Transform newSegment = new GameObject().transform;

        Transform newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        newSegmentSide.SetParent(newSegment);
        newSegmentSide.localEulerAngles = new Vector3(0, 90, 0);
        newSegmentSide.localPosition = new Vector3(0, 0, 0.5f);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        newSegmentSide.SetParent(newSegment);
        newSegmentSide.localEulerAngles = new Vector3(0, -90, 0);
        newSegmentSide.localPosition = new Vector3(0, 0, 0.5f);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        newSegmentSide.SetParent(newSegment);
        newSegmentSide.localEulerAngles = new Vector3(90, 0, 0);
        newSegmentSide.localPosition = new Vector3(0, 0.4f, 0);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        newSegmentSide.SetParent(newSegment);
        newSegmentSide.localEulerAngles = new Vector3(-90, 0, 0);
        newSegmentSide.localPosition = new Vector3(0, -0.4f, 0);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegment.localScale = new Vector3(0.01f, 0.2f, 0.1f);
        newSegment.SetParent(transform);
        newSegment.localEulerAngles = new Vector3(0, 180, 0);

        spawnTimes.Enqueue(Time.time);

        return newSegment;
    }

    private void DeleteOldestSegment()
    {
        //TODO: Create pool for trail segments
        spawnTimes.Dequeue();

        Transform segmentToDelete = trailSegments[trailSegments.Count - 1];
        Destroy(segmentToDelete.gameObject);
        trailSegments.RemoveAt(trailSegments.Count - 1);
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        UpdateTrailPoints(playerMovementVector);
    }
}
