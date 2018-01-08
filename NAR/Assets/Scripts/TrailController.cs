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

    float lifetime = 0.5f;
    float minimumVertexDistance = 0.05f;
    List<Vector3> points;
    Queue<float> spawnTimes = new Queue<float>();

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
        trail = GetComponent<LineRenderer>();

        points = new List<Vector3>() { transform.position }; 
        trail.SetPositions(points.ToArray());
    }

    private void UpdateTrailPoints(Vector2 positionChange)
    {
        while (spawnTimes.Count > 0 && spawnTimes.Peek() + lifetime < Time.time)
        {
            RemovePoint();
        }
        
        Vector3 diff = new Vector3(-positionChange.x, 0, -positionChange.y);
        for (int i = 1; i < points.Count; i++)
        {
            points[i] += diff;
        }
        
        if (points.Count < 2 || Vector3.Distance(transform.position, points[1]) > minimumVertexDistance)
        {
            AddPoint(transform.position);
        }
        
        points[0] = transform.position;
        
        trail.positionCount = points.Count;
        trail.SetPositions(points.ToArray());
    }

    void AddPoint(Vector3 position)
    {
        points.Insert(1, position);
        spawnTimes.Enqueue(Time.time);
    }

    void RemovePoint()
    {
        spawnTimes.Dequeue();
        points.RemoveAt(points.Count - 1);
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        UpdateTrailPoints(playerMovementVector);
    }
}
