using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class EnemyController : MonoBehaviour
{
    ObjectMovementController movementController;
    [SerializeField]
    GameObject enemyMeshHolder;
    Material[] enemyMaterials;
    Material enemyEffectMaterial;

    float preferredXDistanceFromPlayer = 1.5f;
    float preferredXDistanceErrorMargin = 0.25f;

    float xSpeed = 3.5f;
    float currentXVelocity = 0f;
    float refXVelocity = 0f;
    float xAccelerationLag = 0.15f;

    float preferredZDistanceFromPlayer = -1.5f;
    float zSpeed = 5f;
    float currentZVelocity = 0f;
    float refZVelocity = 0f;
    float zAccelerationLag = 0.15f;

    [SerializeField]
    ParticleSystem chargeEffect;
    bool isCharging = false;
    float chargeDuration = 5f;
    float chargeStartTime = 0f;

    bool isAttacking = false;
    float attackZSpeedMultiplier = 1f;
    float attackZSpeedMultiplierMax = 4f;
    float attackZSpeedMultiplierMin = 1f;
    float attackDuration = 3f;
    float attackStartTime = 0f;

    int currentScoreMultiplier = 0;

    LevelController levelController;

    private void OnDisable()
    {
        EventManager.OnEnvironmentColorChange -= OnEnvironmentColorChange;
        EventManager.OnScoreMultiplierChange -= OnScoreMultiplierChange;
    }

    private void OnEnvironmentColorChange(Color newEnvironmentColor)
    {
        for (int i = 0; i < enemyMaterials.Length; i++)
        {
            enemyMaterials[i].SetColor("_EmissionColor", newEnvironmentColor);
        }

        enemyEffectMaterial.SetColor("_TintColor", newEnvironmentColor);
    }

    private void OnScoreMultiplierChange(int newScoreMultiplier, int newScoreMultiplierTier)
    {
        currentScoreMultiplier = newScoreMultiplier;
    }

    public void Initialize()
    {
        movementController = GetComponent<ObjectMovementController>();

        Renderer[] enemyRenderers = enemyMeshHolder.GetComponentsInChildren<Renderer>();
        enemyMaterials = new Material[enemyRenderers.Length];
        enemyEffectMaterial = chargeEffect.GetComponentInChildren<Renderer>().material;

        for (int i = 0; i < enemyMaterials.Length; i++)
        {
            enemyMaterials[i] = enemyRenderers[i].material;
        }

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition, LevelController levelControllerReference)
    {
        levelController = levelControllerReference;
        transform.position = spawnPosition;

        EventManager.OnEnvironmentColorChange += OnEnvironmentColorChange;
        EventManager.OnScoreMultiplierChange += OnScoreMultiplierChange;

        Color environmentColor = EventManager.BroadcastRequestCurrentEnvironmentColor();

        for (int i = 0; i < enemyMaterials.Length; i++)
        {
            enemyMaterials[i].SetColor("_EmissionColor", environmentColor);
        }

        enemyEffectMaterial.SetColor("_TintColor", environmentColor);

        movementController.Activate();

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        EventManager.OnEnvironmentColorChange -= OnEnvironmentColorChange;
        EventManager.OnScoreMultiplierChange -= OnScoreMultiplierChange;

        ResetEnemyValues();
        movementController.Deactivate();

        SetChargeEffectState(false);
        gameObject.SetActive(false);
    }

    private void SetChargeEffectState(bool newState)
    {
        if (newState)
        {
            chargeEffect.Play(true);
        }
        else
        {
            chargeEffect.Stop(true);
        }
    }

    private void Update()
    {
        if (!isAttacking)
        {
            int xDirection = 0;

            if (transform.position.x > preferredXDistanceFromPlayer + preferredXDistanceErrorMargin)
            {
                xDirection = -1;
            }
            else if (transform.position.x < preferredXDistanceFromPlayer - preferredXDistanceErrorMargin && transform.position.x >= 0)
            {
                xDirection = 1;
            }
            else if (transform.position.x > -preferredXDistanceFromPlayer + preferredXDistanceErrorMargin && transform.position.x < 0)
            {
                xDirection = -1;
            }
            else if (transform.position.x < -preferredXDistanceFromPlayer - preferredXDistanceErrorMargin)
            {
                xDirection = 1;
            }

            float targetXVelocity = xSpeed * xDirection;
            currentXVelocity = Mathf.SmoothDamp(currentXVelocity, targetXVelocity, ref refXVelocity, xAccelerationLag);
        }

        int zDirection = 1;

        if (!isAttacking)
        {
            zDirection = 0;
            if (transform.position.z < preferredZDistanceFromPlayer)
            {
                zDirection = 1;
            }
            else if (transform.position.z > -0.15f)
            {
                zDirection = -1;
            }
            else if (!isCharging)
            {
                SetChargeEffectState(true);
                chargeStartTime = Time.time;
                isCharging = true;
            }
        }

        float targetZVelocity = zSpeed * zDirection * attackZSpeedMultiplier;
        currentZVelocity = Mathf.SmoothDamp(currentZVelocity, targetZVelocity, ref refZVelocity, zAccelerationLag);

        transform.position += new Vector3(currentXVelocity, 0, currentZVelocity) * Time.deltaTime;

        if (isAttacking)
        {
            ManageAttack();
        }
        else if (isCharging)
        {
            ManageCharging();
        }
    }

    private void ManageCharging()
    {
        if (Time.time - chargeStartTime > chargeDuration)
        {
            isCharging = false;
            SetChargeEffectState(false);
            StartAttack();
        }
    }

    private void StartAttack()
    {
        currentXVelocity = 0f;
        attackZSpeedMultiplier = attackZSpeedMultiplierMax + ((currentScoreMultiplier - 1) * 0.25f); // = attackZSpeedMultiplierMax;
        attackStartTime = Time.time;
        isAttacking = true;
    }

    private void ManageAttack()
    {
        Vector3 obstacleSpawnPosition = new Vector3(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z));
        levelController.TrySpawnObstacleAtPosition(obstacleSpawnPosition);
        obstacleSpawnPosition = new Vector3(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z) + 1);
        levelController.TrySpawnObstacleAtPosition(obstacleSpawnPosition);
        obstacleSpawnPosition = new Vector3(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z) - 1);
        levelController.TrySpawnObstacleAtPosition(obstacleSpawnPosition);

        if (Time.time - attackStartTime > attackDuration)
        {
            Despawn();
        }
    }

    private void ResetEnemyValues()
    {
        isCharging = false;
        isAttacking = false;
        attackZSpeedMultiplier = attackZSpeedMultiplierMin;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (isCharging && col.CompareTag("PlayerTrail"))
        {
            Despawn();
        }
    }
}
