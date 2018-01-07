using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController_Chase : CameraController
{
    Vector3 cameraPosOffset = new Vector3(0, 0.8f, -2);

    Vector3 velocity;
    float smoothTime = 0.1f;

    bool movementEffectStrengthLerping = false;
    float movementEffectStrengthPercentage = 0.0f;
    float movementEffectStrengthPercentageMin = 0f;
    float movementEffectStrengthPercentageMax = 1f;
    float movementEffectStrengthLerpDuration = 2f;
    float movementEffectStrenghtLerpStartTime;

    bool chaseActive = false;

    private void Start()
    {
        cameraType = ECameraType.Chase;
    }

    private void OnEnable()
    {
        EventManager.OnPlayerMovement += OnPlayerMovement;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
    }

    public override void ActivateCamera()
    {
        cameraPosOffset = transform.position;
        movementEffectStrengthPercentage = movementEffectStrengthPercentageMin;
        movementEffectStrenghtLerpStartTime = Time.time;
        movementEffectStrengthLerping = true;

        chaseActive = true;

        base.ActivateCamera();
    }

    public override void DeactivateCamera()
    {
        chaseActive = false;

        base.DeactivateCamera();
    }

    private void Update()
    {
        if (!isPaused)
        {
            if (chaseActive)
            {
                if (movementEffectStrengthLerping)
                {
                    float timeSinceStarted = Time.time - movementEffectStrenghtLerpStartTime;
                    float percentageCompleted = timeSinceStarted / movementEffectStrengthLerpDuration;
                    movementEffectStrengthPercentage = Mathf.Lerp(movementEffectStrengthPercentageMin, movementEffectStrengthPercentageMax,
                        percentageCompleted);
                }

                transform.position = Vector3.SmoothDamp(transform.position, cameraPosOffset, ref velocity, smoothTime);
                transform.LookAt(Vector3.zero);
            }
        }
    }

    private void OnPlayerMovement(Vector2 playerMovementVector)
    {
        if (chaseActive)
        {
            transform.position -= new Vector3(playerMovementVector.x, 0, playerMovementVector.y) * movementEffectStrengthPercentage;
        }
    }
}
