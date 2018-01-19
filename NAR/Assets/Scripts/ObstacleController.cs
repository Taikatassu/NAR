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
    Material obstacleMaterial;
    float originalAlpha1 = 0.0f;
    float originalAlpha2 = 0.0f;

    //float gridSpacing = 3;

    [SerializeField]
    Transform obstacleMesh;
    [SerializeField]
    float obstacleMeshStartPosY = -1f;
    [SerializeField]
    float obstacleMeshEndPosY = 0f;
    float obstacleMeshAnimationRefVelocity;
    float obstacleMeshAnimationSmoothTime = 0.5f;
    float spawnTime = 0f;
    float spawnAnimationDelay = 0.5f;
    bool spawnMeshPositionInitialized = false;

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
            if (!spawnMeshPositionInitialized && Time.time - spawnTime > spawnAnimationDelay / 2)
            {
                spawnMeshPositionInitialized = true;
                obstacleMesh.localPosition = new Vector3(0, obstacleMeshStartPosY, 0);
            }

            if (Time.time - spawnTime > spawnAnimationDelay)
            {
                RunSpawnAnimation();
            }
        }
    }

    public void Initialize()
    {
        triggerController = GetComponentInChildren<TriggerEventController>(true);
        movementController = GetComponent<ObjectMovementController>();

        obstacleMaterial = GetComponentInChildren<Renderer>().material;
        originalAlpha1 = obstacleMaterial.GetColor("_Color").a;
        originalAlpha2 = obstacleMaterial.GetColor("_EmissionColor").a;

        //gridSpacing = obstacleMaterial.GetFloat("_GridSpacing");

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

        //ModifyGridOffset(EOffsetDirection.XOffset, currentGridOffset.x, false);
        //ModifyGridOffset(EOffsetDirection.ZOffset, currentGridOffset.y, false);

        obstacleMesh.localPosition = new Vector3(0, -1.5f, 0);

        movementController.Activate();

        spawnTime = Time.time;

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

        Color color = obstacleMaterial.GetColor("_Color");
        color.a = originalAlpha1;
        obstacleMaterial.SetColor("_Color", color);

        //color = obstacleMaterial.GetColor("_EmissionColor");
        //color.a = originalAlpha2;
        obstacleMaterial.SetColor("_EmissionColor", color);

        spawnMeshPositionInitialized = false;
    }
    Color emissionColor;
    private void OnPlayerMovement(Vector3 playerMovementVector)
    {
        //if (playerMovementVector.z != 0)
        //{
        //    ModifyGridOffset(EOffsetDirection.ZOffset, playerMovementVector.z);
        //}
        //if (playerMovementVector.x != 0)
        //{
        //    ModifyGridOffset(EOffsetDirection.XOffset, playerMovementVector.x);
        //}


        float distanceFromCamera = (cameraTransform.position - transform.position).magnitude;
        if (distanceFromCamera <= distanceFromCameraToSTartFade)
        {
            float percentageFaded = 1 - ((distanceFromCamera - distanceFromCameraToFinishFade) / fadeZoneDistance);
            float newAlpha = Mathf.Lerp(originalAlpha1, 0, percentageFaded);
            Color color = obstacleMaterial.GetColor("_Color");
            color.a = newAlpha;
            obstacleMaterial.SetColor("_Color", color);

            newAlpha = Mathf.Lerp(originalAlpha2, 0f, percentageFaded);
            //Color emissionColor = obstacleMaterial.GetColor("_EmissionColor");
            Color finalColor = emissionColor * Mathf.LinearToGammaSpace(newAlpha);
            obstacleMaterial.SetColor("_EmissionColor", finalColor);
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
        PlayerController playerController = col.GetComponentInParent<PlayerController>();
        if (col.CompareTag("Player"))
        {
            playerController.OnObstacleHit(gameObject);
        }
    }

    private void OnEnvironmentColorChange(Color color)
    {
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        float oldAlpha = obstacleMaterial.GetColor("_Color").a;
        color.a = oldAlpha;
        obstacleMaterial.SetColor("_Color", color);

        //oldAlpha = obstacleMaterial.GetColor("_EmissionColor").a;
        //color.a = oldAlpha;
        obstacleMaterial.SetColor("_EmissionColor", color);
        emissionColor = color;
    }

    private void RunSpawnAnimation()
    {
        float newPosition = Mathf.SmoothDamp(obstacleMesh.localPosition.y, obstacleMeshEndPosY, ref obstacleMeshAnimationRefVelocity,
            obstacleMeshAnimationSmoothTime);
        obstacleMesh.localPosition = new Vector3(0, newPosition, 0);
    }
}
