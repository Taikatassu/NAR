using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    GameObject mainCamera;

    private enum ECameraMode
    {
        Default,
        IntroCamera,
        ChaseCamera,
    }

    private ECameraMode currentCameraMode = ECameraMode.Default;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
    }

    private void SetCameraState(ECameraMode newCameraMode, bool forceReset = false)
    {
        if (forceReset || currentCameraMode != newCameraMode)
        {
            currentCameraMode = newCameraMode;

            switch (newCameraMode)
            {
                case ECameraMode.IntroCamera:
                    CameraController_Chase oldChaseCamera = mainCamera.GetComponent<CameraController_Chase>();
                    if (oldChaseCamera != null)
                    {
                        oldChaseCamera.RemoveThisComponent();
                    }

                    CameraController_Intro introCamera = (mainCamera.GetComponent<CameraController_Intro>() != null)
                        ? mainCamera.GetComponent<CameraController_Intro>() : mainCamera.AddComponent<CameraController_Intro>();

                    //CameraController_Intro newIntroCamera = mainCamera.AddComponent<CameraController_Intro>();
                    introCamera.OnCameraSequenceFinished -= OnCameraSequenceFinished;
                    introCamera.OnCameraSequenceFinished += OnCameraSequenceFinished;
                    introCamera.ActivateCamera();

                    break;

                case ECameraMode.ChaseCamera:
                    CameraController_Intro oldIntroCamera = mainCamera.GetComponent<CameraController_Intro>();
                    if (oldIntroCamera != null)
                    {
                        oldIntroCamera.RemoveThisComponent();
                    }

                    CameraController_Chase chaseCamera = (mainCamera.GetComponent<CameraController_Chase>() != null)
                        ? mainCamera.GetComponent<CameraController_Chase>() : mainCamera.AddComponent<CameraController_Chase>();

                    //CameraController_Chase newChaseCamera = mainCamera.AddComponent<CameraController_Chase>();
                    chaseCamera.ActivateCamera();

                    break;
            }
        }
    }

    private void OnCameraSequenceFinished(CameraController sender)
    {
        sender.OnCameraSequenceFinished -= OnCameraSequenceFinished;

        if (sender.cameraType == CameraController.ECameraType.Intro)
        {
            SetCameraState(ECameraMode.ChaseCamera);
        }
    }

    private void OnLevelIntroStart()
    {
        SetCameraState(ECameraMode.IntroCamera, true);
    }

    private void OnLevelIntroFinished()
    {
        SetCameraState(ECameraMode.ChaseCamera);
    }
}
