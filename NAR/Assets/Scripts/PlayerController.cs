using System.Collections;
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

    float speedMultiplier = 1f;
    float speedMultiplierStartTime = 0f;
    float speedMultiplierDuration = 12f;

    bool controlsLocked = false;
    bool leftInputButtonDown = false;
    bool rightInputButtonDown = false;
    int input = 0;
    float currentXVelocity = 0f;
    float xAccelerationLag = 0.075f;
    float refXVelocity;
    float currentZVelocity = 0f;
    float zAccelerationLag = 0.25f;
    float refZVelocity;

    bool isPaused = false;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnCollectibleCollected += OnCollectibleCollected;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnCollectibleCollected -= OnCollectibleCollected;
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
        currentXVelocity = 0f;
        currentZVelocity = zSpeed;
        ResetSpeedMultiplier();
    }

    private void OnLevelIntroStart()
    {
        controlsLocked = true;
        leftInputButtonDown = false;
        rightInputButtonDown = false;
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
                IncreaseSpeedMultiplier();
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            ManageSpeedMultiplier();

            //"Move player forward" automatically
            //EventManager.BroadcastPlayerMovement(new Vector2(0, zSpeed * Time.deltaTime));

            if (!controlsLocked)
            {
                if (leftInputButtonDown || Input.GetKey(KeyCode.A))
                {
                    input--;
                }

                if (rightInputButtonDown || Input.GetKey(KeyCode.D))
                {
                    input++;
                }

                input = Mathf.Clamp(input, -1, 1);

                //Accelerate movement on x axis towards desired velocity according to input
                float targetXVelocity = xSpeed * input;
                currentXVelocity = Mathf.SmoothDamp(currentXVelocity, targetXVelocity, ref refXVelocity, xAccelerationLag);
                //Move player on x axis according current velocity
                //EventManager.BroadcastPlayerMovement(new Vector2(currentXVelocity * Time.deltaTime, 0));

                input = 0;
            }


            float targetZVelocity = zSpeed * speedMultiplier;
            currentZVelocity = Mathf.SmoothDamp(currentZVelocity, targetZVelocity, ref refZVelocity, zAccelerationLag);
            EventManager.BroadcastPlayerMovement(new Vector2(currentXVelocity * Time.deltaTime, currentZVelocity * Time.deltaTime));
        }
    }

    private void ManageSpeedMultiplier()
    {
        if (speedMultiplier > 1)
        {
            if (Time.time - speedMultiplierStartTime > speedMultiplierDuration)
            {
                ResetSpeedMultiplier();
            }
        }
    }

    private void IncreaseSpeedMultiplier()
    {
        speedMultiplier += 0.25f;
        speedMultiplierStartTime = Time.time;
    }

    private void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
    }

    public void XInputButtonPressed(int direction)
    {
        switch (direction)
        {
            case -1:
                leftInputButtonDown = true;
                break;
            case 1:
                rightInputButtonDown = true;
                break;
        }
    }

    public void XInputButtonReleased(int direction)
    {
        switch (direction)
        {
            case -1:
                leftInputButtonDown = false;
                break;
            case 1:
                rightInputButtonDown = false;
                break;
        }
    }
}
