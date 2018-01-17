using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class CollectibleController : MonoBehaviour
{
    // TODO: Effects on spawning and despawning
    // TODO: Marker (light pillar) stays active when the collectible is collected

    ObjectMovementController movementController;
    [SerializeField]
    TriggerEventController triggerController;
    [SerializeField]
    ECollectibleType collectibleType;
    [SerializeField]
    Transform meshHolder;
    Material collectibleMeshMaterial;
    [SerializeField]
    Transform effectsHolder;
    Material collectibleEffectMaterial;
    [SerializeField]
    SphereCollider despawnAreaCollider;
    float despawnAreaColliderOriginalRadius = 0f;

    bool isCollected = false;

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
        collectibleMeshMaterial = meshHolder.GetComponentInChildren<Renderer>().material;
        collectibleEffectMaterial = effectsHolder.GetComponentInChildren<Renderer>().material;
        despawnAreaColliderOriginalRadius = despawnAreaCollider.radius;

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition)
    {
        Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
        spawnPosition -= new Vector3(currentGridOffset.x, 0, currentGridOffset.y);
        transform.position = spawnPosition;

        SetColor(EventManager.BroadcastRequestCurrentEnvironmentColor());
        triggerController.OnTriggerEnterEvent += OnTriggerEnterEvent;
        
        despawnAreaCollider.radius = despawnAreaColliderOriginalRadius + EventManager.BroadcastRequestCurrentScoreMultiplier() * 0.2f;

        movementController.Activate();

        isCollected = false;
        meshHolder.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;

        despawnAreaCollider.radius = despawnAreaColliderOriginalRadius;

        movementController.Deactivate();

        gameObject.SetActive(false);
    }

    private void Collect()
    {
        isCollected = true;
        meshHolder.gameObject.SetActive(false);

        EventManager.BroadcastCollectibleCollected((int)collectibleType);
    }

    private void Update()
    {
        if (transform.position.z <= -10)
        {
            Despawn();
        }
    }

    #region Color management
    private void SetColor(Color color)
    {
        color = ColorHelper.FindComplementaryColor(color);

        float oldAlpha = collectibleMeshMaterial.GetColor("_TintColor").a;
        color.a = oldAlpha;
        collectibleMeshMaterial.SetColor("_TintColor", color);

        oldAlpha = collectibleEffectMaterial.GetColor("_TintColor").a;
        color.a = oldAlpha;
        collectibleEffectMaterial.SetColor("_TintColor", color);
    }
    #endregion

    private void OnTriggerEnterEvent(Collider col)
    {
        if (!isCollected && col.CompareTag("Player"))
        {
            Collect();
        }
    }
}
