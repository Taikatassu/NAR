using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDespawner : MonoBehaviour
{
    TriggerEventController obstacleDespawnTriggerController;
    [SerializeField]
    bool isActive = true;

    private void OnEnable()
    {
        obstacleDespawnTriggerController = GetComponent<TriggerEventController>();
        obstacleDespawnTriggerController.OnTriggerEnterEvent += OnObstacleDespawnTriggerEnterEvent;
    }

    private void OnDisable()
    {
        obstacleDespawnTriggerController.OnTriggerEnterEvent -= OnObstacleDespawnTriggerEnterEvent;
    }

    private void OnObstacleDespawnTriggerEnterEvent(Collider col)
    {
        if (isActive)
        {
            ObstacleController obstacleController = col.GetComponentInParent<ObstacleController>();
            if (obstacleController != null)
            {
                obstacleController.Despawn();
            }
        }
    }
}
