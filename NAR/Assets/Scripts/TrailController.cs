using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    // TODO: Fix holes in the trail
    // Implement segment pooling!

    LineRenderer trail;
    [SerializeField]
    Material trailMaterial;

    [SerializeField]
    float lifetime = 0.5f;
    [SerializeField]
    float minimumVertexDistance = 0.05f;
    List<Vector3> points;
    Queue<float> spawnTimes;

    List<Transform> trailSegments;

    private void OnEnable()
    {
        InitializeTrail();
        EventManager.OnPlayerMovement += OnPlayerMovement;
        EventManager.OnLevelRestart += OnLevelRestart;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnLevelRestart -= OnLevelRestart;
    }

    private void InitializeTrail()
    {
        Debug.Log("InitializeTrail");
        //trail = GetComponent<LineRenderer>();

        points = new List<Vector3>() { transform.position };
        //trail.SetPositions(points.ToArray());

        spawnTimes = new Queue<float>();
        trailSegments = new List<Transform>();

        Transform newSegment = CreateNewTrailSegment();
        newSegment.position = transform.position;

        trailSegments.Add(newSegment);
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

        if (points.Count < 2 || Vector3.Distance(transform.position, trailSegments[1].position) > minimumVertexDistance)
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
        Debug.Log("AddNewSegment, trailSegments.Count: " + trailSegments.Count);

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
        newSegmentSide.localPosition = new Vector3(0, 0.4f, 0.5f);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        newSegmentSide.SetParent(newSegment);
        newSegmentSide.localEulerAngles = new Vector3(-90, 0, 0);
        newSegmentSide.localPosition = new Vector3(0, -0.4f, 0.5f);
        newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        newSegment.localScale = new Vector3(0.01f, 0.2f, 0.15f);
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

    private void ResetTrail()
    {
        Debug.Log("ResetTrail");
        for (int i = 0; i < trailSegments.Count; i++)
        {
            Destroy(trailSegments[i].gameObject);
        }

        InitializeTrail();
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        UpdateTrailPoints(playerMovementVector);
    }

    private void OnLevelRestart()
    {
        ResetTrail();
    }
}
