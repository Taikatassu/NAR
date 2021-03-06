﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    bool goalReachingOn = false;
    Vector3 startingPosition = Vector3.zero;
    float remainingLevelTimeThresholdToStartMovingGoal = 30;
    float remainingLevelTime = 0f;
    float levelDuration = 0f;
    float currentLevelTime = 0f;
    float audioFinishThreshold = 5f;
    bool audioFinishThresholdReached = false;

    private void OnEnable()
    {
        startingPosition = transform.position;

        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnPlayerMovement += OnPlayerMovement;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnPlayerMovement -= OnPlayerMovement;
    }

    private void OnPlayerMovement(Vector3 movementVector)
    {
        if (goalReachingOn)
        {
            ManageGoalPosition(movementVector);
        }
    }

    private void OnLevelIntroStart()
    {
        ResetGoalState();
        StartLevelDurationChecking();
    }

    private void ManageGoalPosition(Vector3 movementVector)
    {
        EventManager.BroadcastRequestCurrentAudioTimeInfo(out levelDuration, out currentLevelTime);
        remainingLevelTime = levelDuration - currentLevelTime;

        if (remainingLevelTime < remainingLevelTimeThresholdToStartMovingGoal)
        {
            float percentageCompleted = (levelDuration > remainingLevelTimeThresholdToStartMovingGoal)
                ? 1 - (remainingLevelTime / remainingLevelTimeThresholdToStartMovingGoal)
                : 1 - (remainingLevelTime / levelDuration);

            float newGoalZPosition = Mathf.Lerp(startingPosition.z, 0, percentageCompleted);

            Vector3 newPosition = Vector3.zero;
            newPosition.z = newGoalZPosition;

            Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
            newPosition -= new Vector3(currentGridOffset.x, 0, currentGridOffset.y);

            transform.position = newPosition;

            if (remainingLevelTime < audioFinishThreshold && !audioFinishThresholdReached)
            {
                audioFinishThresholdReached = true;
                EventManager.BroadcastLevelFinished();
            }

            if(remainingLevelTime <= 0)
            {
                goalReachingOn = false;
            }
        }
    }

    private void StartLevelDurationChecking()
    {
        EventManager.BroadcastRequestCurrentAudioTimeInfo(out levelDuration, out currentLevelTime);
        remainingLevelTime = levelDuration - currentLevelTime;
        audioFinishThresholdReached = false;

        goalReachingOn = true;
    }

    private void ResetGoalState()
    {
        goalReachingOn = false;
        transform.position = startingPosition;
    }
}
