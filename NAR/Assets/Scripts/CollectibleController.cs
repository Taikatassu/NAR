using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class CollectibleController : MonoBehaviour
{
    ObjectMovementController movementController;
    TriggerEventController triggerController;

    public enum ECollectibleType
    {
        ScoreMultiplier,

    }

    private void Start()
    {
        Initialize();
        Spawn();
    }

    private void OnDisable()
    {
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;
    }

    public void Initialize()
    {
        triggerController = GetComponentInChildren<TriggerEventController>(true);
        movementController = GetComponent<ObjectMovementController>();
    }

    public void Spawn()
    {
        triggerController.OnTriggerEnterEvent += OnTriggerEnterEvent;
    }

    public void Despawn()
    {
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;

        movementController.Deactivate();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnterEvent(Collider col)
    {
        Debug.Log("Collectible collected");
        // TODO: Broadcast collection event with collectible type

        Despawn();
    }
}
