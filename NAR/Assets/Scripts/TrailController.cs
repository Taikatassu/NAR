using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    // TODO: Figure out how to create a trail effect on non-moving object with a lineRenderer:
    // https://answers.unity.com/questions/1032656/trail-renderer-on-a-non-moving-object.html

    LineRenderer trail;
    //Vector3 offset = new Vector3(0, 0, -0.25f);
    //int trailResolution = 10;
    //Vector3[] lineSegmentPositions;
    //Vector3[] lineSegmentVelocities;
    //float lagTime = 0.25f;

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
        //trail.positionCount = trailResolution;

        //lineSegmentPositions = new Vector3[trailResolution];
        //lineSegmentVelocities = new Vector3[trailResolution];

        //for (int i = 0; i < trailResolution; i++)
        //{
        //    lineSegmentVelocities[i] = Vector3.zero;

        //    if (i == 0)
        //    {
        //        trail.SetPosition(i, transform.position);
        //    }
        //    else
        //    {
        //        lineSegmentPositions[i] = transform.position + (offset * i);
        //    }
        //}
    }

    private void UpdateTrailPoints(Vector2 positionChange)
    {
        //for (int i = 0; i < lineSegmentPositions.Length; i++)
        //{
        //    lineSegmentVelocities[i] = Vector3.zero;

        //    if (i == 0)
        //    {
        //        lineSegmentPositions[i] = transform.position;
        //        trail.SetPosition(i, lineSegmentPositions[i]);
        //    }
        //    else
        //    {
        //        Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
        //        lineSegmentPositions[i].x -= currentGridOffset.x;
        //        lineSegmentPositions[i].z -= currentGridOffset.y;
        //        trail.SetPosition(i, Vector3.SmoothDamp(trail.GetPosition(i), lineSegmentPositions[i], ref lineSegmentVelocities[i], lagTime));
        //    }

        //}
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        UpdateTrailPoints(playerMovementVector);
    }
}
