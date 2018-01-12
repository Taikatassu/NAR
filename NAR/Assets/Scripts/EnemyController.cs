using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectMovementController))]
public class EnemyController : MonoBehaviour
{
    ObjectMovementController movementController;

    float preferredXDistanceFromPlayer = 1.5f;

    float xSpeed = 3.5f;
    float currentXVelocity = 0f;
    float refXVelocity = 0f;
    float xAccelerationLag = 0.15f;

    float preferredZDistanceFromPlayer = -1.5f;
    float zSpeed = 5f;
    float currentZVelocity = 0f;
    float refZVelocity = 0f;
    float zAccelerationLag = 0.15f;
    float zAccelerationLagSpeedBump = 1f;

    float scoreMultiplier = 1f;
    float speedMultiplierPerScoreMultiplier = 0.25f;

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

    LevelController levelController;

    private void OnDisable()
    {
        EventManager.OnScoreMultiplierChange -= OnScoreMultiplierChange;
    }

    public void Initialize()
    {
        movementController = GetComponent<ObjectMovementController>();

        Despawn();
    }

    public void Spawn(Vector3 spawnPosition, LevelController levelControllerReference)
    {
        levelController = levelControllerReference;
        transform.position = spawnPosition;

        scoreMultiplier = EventManager.BroadcastRequestCurrentScoreMultiplier();
        EventManager.OnScoreMultiplierChange += OnScoreMultiplierChange;

        movementController.Activate();

        gameObject.SetActive(true);
    }

    public void Despawn()
    {
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
            if (transform.position.x > preferredXDistanceFromPlayer)
            {
                xDirection = -1;
            }
            else if (transform.position.x < -preferredXDistanceFromPlayer)
            {
                xDirection = 1;
            }

            float targetXVelocity = xSpeed * xDirection/* * ((scoreMultiplier - 1) * speedMultiplierPerScoreMultiplier + 1)*/;
            currentXVelocity = Mathf.SmoothDamp(currentXVelocity, targetXVelocity, ref refXVelocity, xAccelerationLag);
        }

        int zDirection = 1;
        float zSpeedCatchUpMultiplier = 1f;

        if (!isAttacking)
        {
            zDirection = 0;
            if (transform.position.z < preferredZDistanceFromPlayer)
            {
                //zSpeedCatchUpMultiplier = 1.5f;
                zDirection = 1;
            }
            //else if (transform.position.z > -0.15f)
            //{
            //    zDirection = -1;
            //}
            else if (!isCharging)
            {
                SetChargeEffectState(true);
                chargeStartTime = Time.time;
                isCharging = true;
            }
        }

        float targetZVelocity = zSpeed * zSpeedCatchUpMultiplier * zDirection * zAccelerationLagSpeedBump * attackZSpeedMultiplier 
            * ((scoreMultiplier - 1) * speedMultiplierPerScoreMultiplier + 1);
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

    private void OnScoreMultiplierChange(int newScoreMultiplier, int newScoreMultiplierTier)
    {
        scoreMultiplier = newScoreMultiplier;
        //zAccelerationLag = zAccelerationLagSpeedBump;
        //StartCoroutine(ResetZAccelerationLag(0.5f));
    }

    IEnumerator ResetZAccelerationLag(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        zAccelerationLag = 0.15f;
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
        attackZSpeedMultiplier = attackZSpeedMultiplierMax;
        attackStartTime = Time.time;
        isAttacking = true;
    }

    private void ManageAttack()
    {
        Vector3 obstacleSpawnPosition = new Vector3(Mathf.RoundToInt(transform.position.x), 0, Mathf.FloorToInt(transform.position.z));
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
        //StopCoroutine("ResetZAccelerationLag");
        zAccelerationLag = 0.15f;
    }

    private void OnTriggerEnter(Collider col)
    {

        if (isCharging && col.CompareTag("PlayerTrail"))
        {
            Despawn();
        }
    }
}
