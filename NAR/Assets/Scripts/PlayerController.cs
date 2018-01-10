﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // TODO: Create proper mesh for the player (spaceship, scifi-bike?)
    // Implement animations / effect for when the player is moving sideways
    //      - Or just tilt the player mesh slightly to the moving direction?
    // Add a sideways dash feature? (cooldown / double tap to activate?)

    [SerializeField]
    float zSpeed = 4f;
    [SerializeField]
    float xSpeed = 3f;

    int scoreMultiplier = 1;
    float scoreMultiplierStartTime = 0f;
    float scoreMultiplierDuration = 12f;
    float speedMultiplierPerScoreMultiplier = 0.25f;

    bool controlsLocked = false;
    bool leftInputButtonHeld = false;
    bool rightInputButtonHeld = false;
    int input = 0;
    float currentXVelocity = 0f;
    float xAccelerationLag = 0.075f;
    float refXVelocity;
    float currentZVelocity = 0f;
    float zAccelerationLag = 0.1f;
    float refZVelocity;

    float lastInputTime = 0f;
    float dashTimeWindow = 0.2f;
    float dashDuration = 0.05f;
    float dashStartTime = 0f;
    float dashInvulnerabilityDuration = 0.2f;
    int lastInputKeyPressed = 0;
    int dashDirection = 0;
    bool isDashing = false;
    float dashXSpeed = 14f;
    float refDashVelocity;
    float dashAccelerationLag = 0.025f;
    float dashCooldownDuration = 0.35f;

    float scoreMultiplierUseInvulnerabilityDuration = 0.5f;
    float scoreMultiplierUseInvulnerabilityStartTime = 0f;

    bool isPaused = false;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnCollectibleCollected += OnCollectibleCollected;
        EventManager.OnObstacleHit += OnObstacleHit;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnCollectibleCollected -= OnCollectibleCollected;
        EventManager.OnObstacleHit -= OnObstacleHit;
    }

    private void OnPauseStateChanged(bool newState)
    {
        isPaused = newState;
    }

    private void OnLevelRestart()
    {
        ResetSpeed();
    }

    private void ResetSpeed()
    {
        ResetDash();
        currentXVelocity = 0f;
        currentZVelocity = zSpeed;
        ResetScoreMultiplier();
    }

    private void OnLevelIntroStart()
    {
        controlsLocked = true;
        leftInputButtonHeld = false;
        rightInputButtonHeld = false;
        ResetSpeed();
    }

    private void OnLevelIntroFinished()
    {
        controlsLocked = false;
    }

    private void OnCollectibleCollected(int collectibleTypeIndex)
    {
        CollectibleController.ECollectibleType collectibleType = (CollectibleController.ECollectibleType)collectibleTypeIndex;

        switch (collectibleType)
        {
            case CollectibleController.ECollectibleType.ScoreMultiplier:
                IncreaseScoreMultiplier();
                break;
            default:
                break;
        }
    }

    private void OnObstacleHit(GameObject hitObstacle)
    {
        // Check if invulnerable saves are not available
        if (Time.time - dashStartTime > dashInvulnerabilityDuration 
            && Time.time - scoreMultiplierUseInvulnerabilityStartTime > scoreMultiplierUseInvulnerabilityDuration)
        {
            if(!DecreaseScoreMultiplier())
            {
                EventManager.BroadcastLevelRestart();
            }
        }
        else
        {
            hitObstacle.GetComponent<ObstacleController>().Despawn();
        }
    }

    private void CheckForDashInput()
    {
        int newInputDirection = 0;

        if (Input.GetKeyDown(KeyCode.A))
        {
            newInputDirection--;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            newInputDirection++;
        }

        if (newInputDirection != 0)
        {
            if (lastInputKeyPressed == newInputDirection)
            {
                StartDash(newInputDirection);
                newInputDirection = 0;
            }

            lastInputTime = Time.time;
            lastInputKeyPressed = newInputDirection;
        }

        if (Time.time - lastInputTime > dashTimeWindow)
        {
            lastInputKeyPressed = 0;
            lastInputButtonPressed = 0;
        }
    }

    private void StartDash(int newDashDirection)
    {
        dashStartTime = Time.time;
        isDashing = true;
        dashDirection = newDashDirection;
    }

    private void Dash()
    {
        float dashTargetXVelocity = dashXSpeed * dashDirection;
        currentXVelocity = Mathf.SmoothDamp(currentXVelocity, dashTargetXVelocity, ref refDashVelocity, dashAccelerationLag);

        if (Time.time - dashStartTime > dashDuration)
        {
            ResetDash();
        }
    }

    private void ResetDash()
    {

        isDashing = false;
    }

    private void Update()
    {
        if (!isPaused)
        {
            ManageSpeedMultiplier();

            if (isDashing)
            {
                Dash();
            }
            else if (!controlsLocked)
            {
                if (leftInputButtonHeld || Input.GetKey(KeyCode.A))
                {
                    input--;
                }

                if (rightInputButtonHeld || Input.GetKey(KeyCode.D))
                {
                    input++;
                }

                input = Mathf.Clamp(input, -1, 1);

                //Accelerate movement on x axis towards desired velocity according to input
                float targetXVelocity = xSpeed * input;
                currentXVelocity = Mathf.SmoothDamp(currentXVelocity, targetXVelocity, ref refXVelocity, xAccelerationLag);

                input = 0;

                if(Time.time - dashStartTime > dashCooldownDuration)
                {
                    CheckForDashInput();
                }
            }

            float targetZVelocity = zSpeed * ((scoreMultiplier - 1) * speedMultiplierPerScoreMultiplier + 1);
            currentZVelocity = Mathf.SmoothDamp(currentZVelocity, targetZVelocity, ref refZVelocity, zAccelerationLag);
            
            EventManager.BroadcastPlayerMovement(new Vector3(currentXVelocity, 0, currentZVelocity) * Time.deltaTime);
        }
    }

    private void ManageSpeedMultiplier()
    {
        if (scoreMultiplier > 1)
        {
            if (Time.time - scoreMultiplierStartTime > scoreMultiplierDuration)
            {
                ResetScoreMultiplier();
            }
        }
    }

    private void IncreaseScoreMultiplier()
    {
        scoreMultiplierStartTime = Time.time;
        scoreMultiplier++;
        EventManager.BroadcastScoreMultiplierChange(scoreMultiplier);
    }

    private bool DecreaseScoreMultiplier()
    {
        if(scoreMultiplier > 1)
        {
            scoreMultiplierUseInvulnerabilityStartTime = Time.time;
            scoreMultiplier--;
            EventManager.BroadcastScoreMultiplierChange(scoreMultiplier);
            return true;
        }

        return false;
    }

    private void ResetScoreMultiplier()
    {
        scoreMultiplier = 1;
        EventManager.BroadcastScoreMultiplierChange(scoreMultiplier);
    }

    int lastInputButtonPressed = 0;
    public void XInputButtonPressed(int direction)
    {
        switch (direction)
        {
            case -1:
                leftInputButtonHeld = true;
                break;
            case 1:
                rightInputButtonHeld = true;
                break;
        }

        if (!controlsLocked)
        {
            if (direction == lastInputButtonPressed)
            {
                StartDash(direction);
                direction = 0;
            }

            lastInputTime = Time.time;
            lastInputButtonPressed = direction;
        }
    }

    public void XInputButtonReleased(int direction)
    {
        switch (direction)
        {
            case -1:
                leftInputButtonHeld = false;
                break;
            case 1:
                rightInputButtonHeld = false;
                break;
        }
    }
}
