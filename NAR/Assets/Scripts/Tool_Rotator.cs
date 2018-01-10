using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_Rotator : MonoBehaviour
{
    public enum ERotationSpace
    {
        WORLD,
        LOCAL,
    }

    public Vector3 rotationPerSecond = Vector3.zero;
    public ERotationSpace rotationSpace = ERotationSpace.LOCAL;
    public bool isRotating = true;
    private bool isPaused = false;

    private void OnEnable()
    {
        EventManager.OnPauseStateChange += OnPauseStateChange;
    }

    private void OnDisable()
    {
        EventManager.OnPauseStateChange -= OnPauseStateChange;
    }

    private void Update()
    {
        if (!isPaused)
        {
            if (isRotating)
            {
                switch (rotationSpace)
                {
                    case ERotationSpace.WORLD:
                        transform.eulerAngles += rotationPerSecond * Time.deltaTime;
                        break;
                    case ERotationSpace.LOCAL:
                        transform.localEulerAngles += rotationPerSecond * Time.deltaTime;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void OnPauseStateChange(bool newState)
    {
        isPaused = newState;
    }
}
