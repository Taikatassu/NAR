using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    protected bool isPaused = false;

    public enum ECameraType
    {
        Intro,
        Chase,
    }

    public ECameraType cameraType;

    public delegate void CameraControllerVoid(CameraController cameraController);

    public event CameraControllerVoid OnCameraSequenceFinished;
    public void BroadcastCameraSequenceFinished(CameraController sender)
    {
        if (OnCameraSequenceFinished != null)
        {
            OnCameraSequenceFinished(sender);
        }
    }

    public virtual void RemoveThisComponent()
    {
        Destroy(this);
    }

    public virtual void ActivateCamera()
    {
        gameObject.SetActive(true);
    }

    public virtual void DeactivateCamera()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.OnPauseStateChange += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool newState)
    {
        isPaused = newState;
    }
}
