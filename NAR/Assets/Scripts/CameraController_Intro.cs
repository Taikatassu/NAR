using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController_Intro : CameraController
{
    [SerializeField]
    Vector3[] waypoints = new Vector3[4] { new Vector3(4, 1, 0),
                                           new Vector3(3, 1, 0),
                                           new Vector3(0, 2, -1),
                                           new Vector3(0, 0.8f , -2),};

    bool runningIntroSequence = false;
    int currentWaypointIndex = 0;
    float waypointCompleteDistanceThreshold = 0.1f;
    Vector3 velocity;
    float smoothTime = 1.25f;

    private void Start()
    {
        cameraType = ECameraType.Intro;
    }

    public override void ActivateCamera()
    {
        InitializeIntroSequence();

        base.ActivateCamera();
    }

    public override void DeactivateCamera()
    {
        runningIntroSequence = false;

        base.DeactivateCamera();
    }

    public void InitializeIntroSequence()
    {
        currentWaypointIndex = 0;
        transform.position = waypoints[currentWaypointIndex];
        runningIntroSequence = true;
    }

    private void Update()
    {
        if (!isPaused)
        {
            if (runningIntroSequence)
            {
                if ((transform.position - waypoints[currentWaypointIndex]).magnitude <= waypointCompleteDistanceThreshold)
                {
                    if (currentWaypointIndex + 1 < waypoints.Length)
                    {
                        currentWaypointIndex++;
                    }
                    else
                    {
                        runningIntroSequence = false;
                        BroadcastCameraSequenceFinished(this);
                    }
                }

                transform.position = Vector3.SmoothDamp(transform.position, waypoints[currentWaypointIndex], ref velocity, smoothTime);
                transform.LookAt(Vector3.zero);
            }
        }
    }
}
