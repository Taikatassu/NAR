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

    public enum ECollectibleType
    {
        ScoreMultiplier,
    }

    private void OnDisable()
    {
        EventManager.OnEnvironmentColorChange -= OnEnvironmentColorChange;
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;
    }

    public void Initialize()
    {
        movementController = GetComponent<ObjectMovementController>();
        collectibleMeshMaterial = meshHolder.GetComponentInChildren<Renderer>().material;
        collectibleEffectMaterial = effectsHolder.GetComponentInChildren<Renderer>().material;

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition)
    {
        Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
        spawnPosition -= new Vector3(currentGridOffset.x, 0, currentGridOffset.y);
        transform.position = spawnPosition;

        SetColor(EventManager.BroadcastRequestCurrentEnvironmentColor());
        EventManager.OnEnvironmentColorChange += OnEnvironmentColorChange;
        triggerController.OnTriggerEnterEvent += OnTriggerEnterEvent;

        movementController.Activate();

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        EventManager.OnEnvironmentColorChange -= OnEnvironmentColorChange;
        triggerController.OnTriggerEnterEvent -= OnTriggerEnterEvent;

        movementController.Deactivate();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (transform.position.z <= -10)
        {
            Despawn();
        }
    }

    #region Color management
    private void OnEnvironmentColorChange(Color color)
    {
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        color = FindComplementaryColor(color);

        float oldAlpha = collectibleMeshMaterial.GetColor("_TintColor").a;
        color.a = oldAlpha;
        collectibleMeshMaterial.SetColor("_TintColor", color);

        oldAlpha = collectibleEffectMaterial.GetColor("_TintColor").a;
        color.a = oldAlpha;
        collectibleEffectMaterial.SetColor("_TintColor", color);
    }

    private Color FindComplementaryColor(Color startColor)
    {
        float h = 0;
        float s = 0;
        float v = 0;
        Color.RGBToHSV(startColor, out h, out s, out v);
        h = (h + 0.5f) % 1;

        Color complementaryColor = Color.HSVToRGB(h, s, v);

        return complementaryColor;
    }
    #endregion

    private void OnTriggerEnterEvent(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            EventManager.BroadcastCollectibleCollected((int)collectibleType);

            Despawn();
        }
    }
}
