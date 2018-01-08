using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    GameObject obstaclePrefab;
    [SerializeField]
    int obstaclePoolSize = 10;
    List<ObstacleController> obstaclePool = new List<ObstacleController>();
    float lastObstacleSpawnTime = 0f;
    float obstacleSpawnCooldownTimer = 0f;
    float obstacleSpawnCooldownDuration = 0.5f;

    int[] obstacleXAxisSpawnPositions = new int[13] { -4, -3, -2, -1, 0, 0, 0, 0, 0, 1, 2, 3, 4 };
    int[] obstacleZAxisSpawnPositions = new int[4] { 6, 7, 8, 9 };

    bool spawnObstacles = false;
    bool runningLevelIntro = false;
    float levelStartTime = 0f;
    float levelIntroDuration = 10f;

    bool isPaused = false;

    private void Start()
    {
        for (int i = 0; i < obstaclePoolSize; i++)
        {
            AddObstacleToPool();
        }

        StartLevel();
    }

    private void OnEnable()
    {
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChanged -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool newState)
    {
        isPaused = newState;
        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    private void StartLevel()
    {
        levelStartTime = Time.time;
        spawnObstacles = false;
        runningLevelIntro = true;

        EventManager.BroadcastLevelIntroStart();
    }

    private void OnLevelRestart()
    {
        RestartLevel();
    }

    private void RestartLevel()
    {
        ResetObstacles();
        StartLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.BroadcastPauseStateChanged(!isPaused);
        }

        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                EventManager.BroadcastLevelRestart();
            }

            if (runningLevelIntro)
            {
                float levelStartTimer = levelIntroDuration - (Time.time - levelStartTime);

                if (levelStartTimer <= 0)
                {
                    levelStartTimer = 0;
                }
                //Debug.Log("Level starting in: " + levelStartTimer);

                if (Time.time - levelStartTime >= levelIntroDuration)
                {
                    runningLevelIntro = false;
                    spawnObstacles = true;
                    EventManager.BroadcastLevelIntroFinished();
                }
            }

            if (spawnObstacles)
            {
                if (Time.time - lastObstacleSpawnTime >= obstacleSpawnCooldownTimer)
                {
                    SpawnObstacle();
                    lastObstacleSpawnTime = Time.time;
                    obstacleSpawnCooldownTimer = obstacleSpawnCooldownDuration;
                }
            }
        }
    }

    private void SpawnObstacle()
    {
        Vector2 spawnPosition = new Vector2(0f, 5f);
        spawnPosition.x = obstacleXAxisSpawnPositions[Random.Range(0, obstacleXAxisSpawnPositions.Length)];
        spawnPosition.y = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)];

        if (spawnPosition.x < -1 || spawnPosition.x > 1)
        {
            spawnPosition.y -= 2;
        }

        //Prevent multiple obstacles spawning at the same location
        if (ObstacleExsistsInGivenPosition(spawnPosition))
        {
            SpawnObstacle();
        }
        else
        {
            GetAvailableObstacle().Spawn(spawnPosition);
        }
    }

    private bool ObstacleExsistsInGivenPosition(Vector2 positionToCheck)
    {
        foreach (ObstacleController oc in obstaclePool)
        {
            if (oc.gameObject.activeSelf)
            {
                Vector3 existingObstaclePos = oc.transform.position;
                Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();
                
                if (Mathf.Approximately(existingObstaclePos.x + currentGridOffset.x, positionToCheck.x)
                && Mathf.Approximately(existingObstaclePos.z + currentGridOffset.y, positionToCheck.y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private ObstacleController GetAvailableObstacle()
    {
        for (int i = 0; i < obstaclePool.Count; i++)
        {
            if (!obstaclePool[i].gameObject.activeSelf)
            {
                return obstaclePool[i];
            }
        }

        return AddObstacleToPool();
    }

    private ObstacleController AddObstacleToPool()
    {
        ObstacleController newObstacle = Instantiate(obstaclePrefab, transform).GetComponent<ObstacleController>();
        newObstacle.Initialize();
        obstaclePool.Add(newObstacle);

        return newObstacle;
    }

    private void ResetObstacles()
    {
        foreach (ObstacleController oc in obstaclePool)
        {
            if (oc.gameObject.activeSelf)
            {
                oc.Despawn();
            }
        }
    }
}
