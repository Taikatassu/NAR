using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    // TODO: Fix holes in the trail
    // Fix or hide (full screen fade) the initial pieces falling behind
    // Implement segment pooling!

    [SerializeField]
    Material trailMaterial;
    [SerializeField]
    float maxLifetime = 0.5f;
    float lifetime = 0.5f;
    [SerializeField]
    float minimumVertexDistance = 0.05f;
    Queue<float> spawnTimes;

    List<Transform> trailSegments;

    Vector3 lastPlayerMovementVector = Vector3.zero;

    public void OnScoreMultiplierTimerChange(float timerValue, float timerMaxValue)
    {
        float percentageCompleted = timerValue / timerMaxValue;

        lifetime = Mathf.Lerp(maxLifetime, 0, percentageCompleted);
    }

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
        spawnTimes = new Queue<float>();
        trailSegments = new List<Transform>();

        Transform newSegment = CreateNewTrailSegment();
        newSegment.position = transform.position;

        trailSegments.Add(newSegment);

        InvokeRepeating("UpdateTrailPoints", 0f, 1f / 600);
    }

    private void UpdateTrailPoints()
    {
        while (spawnTimes.Count > 0 && spawnTimes.Peek() + lifetime < Time.time)
        {
            DeleteOldestSegment();
        }

        for (int i = 0; i < trailSegments.Count; i++)
        {
            trailSegments[i].position -= lastPlayerMovementVector;
        }

        FillTrailGaps();
        CheckSegmentPositionValidity();
        CorrectTrailSegmentFacing();

        lastPlayerMovementVector = Vector3.zero;
    }

    private void FillTrailGaps()
    {
        if (trailSegments.Count < 2 || Vector3.Distance(transform.position, trailSegments[1].position) > minimumVertexDistance)
        {
            AddNewSegment();
        }
    }

    private void CheckSegmentPositionValidity()
    {
        for (int i = 1; i < trailSegments.Count - 1; i++)
        {
            if (trailSegments[i].position == trailSegments[i - 1].position)
            {
                trailSegments[i].position = (trailSegments[i - 1].position + trailSegments[i + 1].position) / 2f;
            }
        }
    }

    private void CorrectTrailSegmentFacing()
    {
        for (int i = 0; i < trailSegments.Count - 1; i++)
        {
            trailSegments[i].LookAt(trailSegments[i + 1]);
        }
    }

    private void AddNewSegment()
    {
        Transform newSegment = CreateNewTrailSegment();
        newSegment.position = transform.position;
        trailSegments.Insert(0, newSegment);
    }

    private Transform CreateNewTrailSegment()
    {
        Transform newSegment = new GameObject().transform;

        //Transform newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        //newSegmentSide.SetParent(newSegment);
        //newSegmentSide.localEulerAngles = new Vector3(0, 90, 0);
        //newSegmentSide.localPosition = new Vector3(0, 0, 0.5f);
        //newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        //newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        //newSegmentSide.SetParent(newSegment);
        //newSegmentSide.localEulerAngles = new Vector3(0, -90, 0);
        //newSegmentSide.localPosition = new Vector3(0, 0, 0.5f);
        //newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        //newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        //newSegmentSide.SetParent(newSegment);
        //newSegmentSide.localEulerAngles = new Vector3(90, 0, 0);
        //newSegmentSide.localPosition = new Vector3(0, 0.4f, 0.5f);
        //newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        //newSegmentSide = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        //newSegmentSide.SetParent(newSegment);
        //newSegmentSide.localEulerAngles = new Vector3(-90, 0, 0);
        //newSegmentSide.localPosition = new Vector3(0, -0.4f, 0.5f);
        //newSegmentSide.GetComponent<Renderer>().material = trailMaterial;

        Transform newSegmentMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        newSegmentMesh.SetParent(newSegment);
        //newSegmentSide.localEulerAngles = new Vector3(0, 90, 0);
        newSegmentMesh.localPosition = new Vector3(0, 0, 0.5f);
        newSegmentMesh.GetComponent<Renderer>().material = trailMaterial;
        newSegmentMesh.gameObject.tag = "PlayerTrail";

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
        CancelInvoke("UpdateTrailPoints");

        for (int i = 0; i < trailSegments.Count; i++)
        {
            Destroy(trailSegments[i].gameObject);
        }

        InitializeTrail();
    }

    private void OnPlayerMovement(Vector3 playerMovementVector)
    {
        lastPlayerMovementVector = playerMovementVector;
        //UpdateTrailPoints();
    }

    private void OnLevelRestart()
    {
        ResetTrail();
    }
}
