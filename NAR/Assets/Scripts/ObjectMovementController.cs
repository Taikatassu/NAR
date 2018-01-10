using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovementController : MonoBehaviour
{
    public bool isIndependend = true;

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
        transform.position -= playerMovementVector;
    }
}
