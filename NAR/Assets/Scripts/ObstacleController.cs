using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class ObstacleController : MonoBehaviour
{
    // TODO: Effects on spawning and despawning
    // TODO: Create a proper material / shader for the obstacles (remove obstacleGrid offsetting once obsolete)

    ObjectMovementController movementController;
    TriggerEventController triggerController;
    Material gridMaterial;
    float originalAlpha1 = 0.0f;
    float originalAlpha2 = 0.0f;
    
    float gridSpacing = 3;

    [SerializeField]
    Transform obstacleMesh;
    [SerializeField]
    float obstacleMeshStartPosY = -1f;
    [SerializeField]
    float obstacleMeshEndPosY = 0f;
    float obstacleMeshAnimationRefVelocity;
    float obstacleMeshAnimationSmoothTime = 0.5f;

    [SerializeField]
    float distanceFromCameraToSTartFade;
    [SerializeField]
    float distanceFromCameraToFinishFade;
    float fadeZoneDistance;
    Transform cameraTransform;

    bool isPaused = false;

    enum EOffsetDirection
    {
        XOffset,
        ZOffset
    }

    private void OnDisable()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;
    }

    private void Update()
    {
        if (!isPaused)
        {
            RunSpawnAnimation();
        }
    }

    public void Initialize()
    {
        triggerController = GetComponentInChildren<TriggerEventController>(true);
        movementController = GetComponent<ObjectMovementController>();

        gridMaterial = GetComponentInChildren<Renderer>().material;
        originalAlpha1 = gridMaterial.GetColor("_GridColor").a;
        originalAlpha2 = gridMaterial.GetColor("_OutsideColor").a;

        gridSpacing = gridMaterial.GetFloat("_GridSpacing");

        fadeZoneDistance = distanceFromCameraToSTartFade - distanceFromCameraToFinishFade;
        cameraTransform = Camera.main.transform;

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition)
    {
        Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
        spawnPosition -= new Vector3(currentGridOffset.x, 0, currentGridOffset.y);
        transform.position = spawnPosition;
        SetColor(EventManager.BroadcastRequestCurrentEnvironmentColor());

        EventManager.OnPlayerMovement += OnPlayerMovement;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnEnvironmentColorChange += OnEnvironmentColorChange;
        triggerController.OnTriggerEnterEvent += OnTriggerEnterEvent;

        ModifyGridOffset(EOffsetDirection.XOffset, currentGridOffset.x, false);
        ModifyGridOffset(EOffsetDirection.ZOffset, currentGridOffset.y, false);

        obstacleMesh.localPosition = new Vector3(0, obstacleMeshStartPosY, 0);

        movementController.Activate();

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnEnvironmentColorChange -= OnEnvironmentColorChange;
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;

        movementController.Deactivate();

        gameObject.SetActive(false);

        Color color = gridMaterial.GetColor("_GridColor");
        color.a = originalAlpha1;
        gridMaterial.SetColor("_GridColor", color);

        color = gridMaterial.GetColor("_OutsideColor");
        color.a = originalAlpha2;
        gridMaterial.SetColor("_OutsideColor", color);
    }

    private void OnPlayerMovement(Vector3 playerMovementVector)
    {
        if (playerMovementVector.z != 0)
        {
            ModifyGridOffset(EOffsetDirection.ZOffset, playerMovementVector.z);
        }
        if (playerMovementVector.x != 0)
        {
            ModifyGridOffset(EOffsetDirection.XOffset, playerMovementVector.x);
        }


        float distanceFromCamera = (cameraTransform.position - transform.position).magnitude;
        if (distanceFromCamera <= distanceFromCameraToSTartFade)
        {
            float percentageFaded = 1 - ((distanceFromCamera - distanceFromCameraToFinishFade) / fadeZoneDistance);
            float newAlpha = Mathf.Lerp(originalAlpha1, 0, percentageFaded);
            Color color = gridMaterial.GetColor("_GridColor");
            color.a = newAlpha;
            gridMaterial.SetColor("_GridColor", color);

            newAlpha = Mathf.Lerp(originalAlpha2, 0, percentageFaded);
            color = gridMaterial.GetColor("_OutsideColor");
            color.a = newAlpha;
            gridMaterial.SetColor("_OutsideColor", color);
        }

        if (transform.position.z <= -10)
        {
            Despawn();
        }
    }

    private void OnPauseStateChanged(bool newState)
    {
        isPaused = newState;
    }

    private void OnTriggerEnterEvent(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            EventManager.BroadcastObstacleHit(gameObject);
        }
    }

    private void OnEnvironmentColorChange(Color color)
    {
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        float oldAlpha = gridMaterial.GetColor("_GridColor").a;
        color.a = oldAlpha;
        gridMaterial.SetColor("_GridColor", color);
    }

    private void ModifyGridOffset(EOffsetDirection offsetToChange, float offsetValue, bool addToExistingOffset = true)
    {
        string offsetFloatName = "";

        switch (offsetToChange)
        {
            case EOffsetDirection.XOffset:
                offsetFloatName = "_PosXOffset";
                break;
            case EOffsetDirection.ZOffset:
                offsetFloatName = "_PosZOffset";
                break;
            default:
                break;
        }
        
        if (addToExistingOffset)
        {
            float oldOffset = gridMaterial.GetFloat(offsetFloatName);
            float newOffset = oldOffset + offsetValue;
            if (newOffset >= gridSpacing / 2)
            {
                newOffset -= gridSpacing;
            }
            else if (newOffset <= -gridSpacing / 2)
            {
                newOffset += gridSpacing;
            }

            gridMaterial.SetFloat(offsetFloatName, newOffset);
        }
        else
        {
            gridMaterial.SetFloat(offsetFloatName, offsetValue);
        }
    }

    private void RunSpawnAnimation()
    {
        float newPosition = Mathf.SmoothDamp(obstacleMesh.localPosition.y, obstacleMeshEndPosY, ref obstacleMeshAnimationRefVelocity, 
            obstacleMeshAnimationSmoothTime);
        obstacleMesh.localPosition = new Vector3(0, newPosition, 0);
    }
}
