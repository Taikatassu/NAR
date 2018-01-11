using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class CollectibleController : MonoBehaviour
{
    // TODO: Effects on spawning and despawning
    // More visible indication of the collectibles's position
    //      - Light pillar?

    ObjectMovementController movementController;
    [SerializeField]
    TriggerEventController triggerController;
    [SerializeField]
    ECollectibleType collectibleType;

    public enum ECollectibleType
    {
        ScoreMultiplier,

    }

    private void OnDisable()
    {
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;
    }

    public void Initialize()
    {
        movementController = GetComponent<ObjectMovementController>();

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition)
    {
        Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
        spawnPosition -= new Vector3(currentGridOffset.x, 0, currentGridOffset.y);
        transform.position = spawnPosition;

        triggerController.OnTriggerEnterEvent += OnTriggerEnterEvent;

        movementController.Activate();

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;

        movementController.Deactivate();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnterEvent(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            EventManager.BroadcastCollectibleCollected((int)collectibleType);

            Despawn();
        }
    }
}
