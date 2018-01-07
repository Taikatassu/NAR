using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float zSpeed = 4f;
    [SerializeField]
    float xSpeed = 3f;

    bool controlsLocked = false;
    bool leftInputButtonDown = false;
    bool rightInputButtonDown = false;
    int input = 0;
    float currentXVelocity = 0f;
    float xAcceleration = 0.075f;
    float refVelocity;

    bool isPaused = false;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChanged -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool newState)
    {
        isPaused = newState;
    }

    private void OnLevelRestart()
    {
        currentXVelocity = 0f;
    }

    private void Update()
    {
        if (!isPaused)
        {
            //"Move player forward" automatically
            EventManager.BroadcastPlayerMovement(new Vector2(0, zSpeed * Time.deltaTime));

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
                currentXVelocity = Mathf.SmoothDamp(currentXVelocity, targetXVelocity, ref refVelocity, xAcceleration);
                //Move player on x axis according current velocity
                EventManager.BroadcastPlayerMovement(new Vector2(currentXVelocity * Time.deltaTime, 0));

                input = 0;
            }
        }
    }

    private void OnLevelIntroStart()
    {
        controlsLocked = true;
        leftInputButtonDown = false;
        rightInputButtonDown = false;
    }

    private void OnLevelIntroFinished()
    {
        controlsLocked = false;
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
