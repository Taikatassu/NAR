using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovementController : MonoBehaviour
{
    public bool isIndependend = true;
    public bool excludeXAxis = false;
    public bool excludeYAxis = false;
    public bool excludeZAxis = false;

    public void OnEnable()
    {
        if (isIndependend)
        {
            EventManager.OnPlayerMovement -= OnPlayerMovement;
            EventManager.OnPlayerMovement += OnPlayerMovement;
        }
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
    }

    public void Activate()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnPlayerMovement += OnPlayerMovement;
    }

    public void Deactivate()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
    }

    private void OnPlayerMovement(Vector3 playerMovementVector)
    {
        if (excludeXAxis)
        {
            playerMovementVector.x = 0;
        }
        if (excludeYAxis)
        {
            playerMovementVector.y = 0;
        }
        if (excludeZAxis)
        {
            playerMovementVector.z = 0;
        }

        transform.position -= playerMovementVector;
    }
}
