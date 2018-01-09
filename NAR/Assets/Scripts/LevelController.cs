using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    // TODO: Modify obstacle spawning to provide more challenge (less downtime for the player)
    // Make the environment more interesting
    // DONE     - Grid material changing from color to color
    //      - Grid material glowing periodically (color values from light to dark to light etc, or add emission to grid shader and change emission value?)
    // DONE Score counter
    // DONE     - Score increases by time
    //      - Score multipliers at certain thresholds? (every minute or so)
    // Actual goal reaching?
    //      - Gameplay separated by distinct waypoints?
    // Collectibles?
    //      - Speed boosts
    //      - Score multipliers

    // TODO: Implement color changing to the level goal element as well!


    [SerializeField]
    GameObject obstaclePrefab;
    int obstaclePoolSize = 20;
    List<ObstacleController> obstaclePool = new List<ObstacleController>();

    bool spawnObstacles = false;
    float lastObstacleSpawnTime = 0f;
    float obstacleSpawnCooldownTimer = 0f;
    float obstacleSpawnCooldownDuration = 0.25f;
    float obstacleAtSamePositionThreshold = 0.5f;

    bool validDirection = false;
    int playerMovementDirection = 0;
    float directionChangeTime = 0f;
    float directionChangeValidationTime = 0.5f;
    float forwardDirectionThreshold = 0.05f;

    float directionalObstacleSpawnChance = 0.75f;
    int[] obstacleXAxisSpawnPositions = new int[13] { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6 };
    int[] obstacleZAxisSpawnPositions = new int[6] { 6, 7, 8, 9, 10, 11 };

    bool runningLevelIntro = false;
    float levelStartTime = 0f;
    float levelIntroDuration = 10f;

    bool isPaused = false;

    [SerializeField]
    float environmentColorChangePhaseDuration = 15f;
    [SerializeField]
    bool colorChanging = true;
    [SerializeField]
    Color[] environmentColors;
    float environmentColorChangePhaseStartTime = 0f;
    int currentEnvironmentColorIndex = 0;
    int nextEnvironmentColorIndex = 0;

    [SerializeField]
    Text scoreValueText;
    bool countingScore = false;
    float scorePerOneUnitTraveled = 1f;
    float rawScore = 0f;
    int currentScore = 0;

    [SerializeField]
    Text scorePopUpText;
    Vector3 scorePopUpRefVelocity = Vector3.zero;
    Vector3 scorePopUpPosition;
    bool displayingScorePopUp = false;
    float scorePopUpSmoothTime = 0.5f;
    float scorePopUpDuration = 5f;
    float scorePopUpStartTime = 0f;
    float scorePopUpFadeStrength = 3f;
    float scorePopUpTextOriginalAlpha = 0f;
    float scorePopUpShadowOriginalAlpha = 0f;


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
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnPlayerMovement += OnPlayerMovement;

        SetScorePopUpText("");
        scorePopUpTextOriginalAlpha = scorePopUpText.color.a;
        scorePopUpShadowOriginalAlpha = scorePopUpText.GetComponent<Shadow>().effectColor.a;
    }

    private void OnDisable()
    {
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnPlayerMovement -= OnPlayerMovement;
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

    private void OnPlayerMovement(Vector2 movementVector)
    {
        CheckDirection(movementVector);

        if (countingScore)
        {
            ManageScore(movementVector);
        }
    }

    private void OnLevelRestart()
    {
        RestartLevel();
    }

    private void StartLevel()
    {
        levelStartTime = Time.time;
        spawnObstacles = false;
        runningLevelIntro = true;
        countingScore = false;
        rawScore = 0;
        currentScore = 0;
        SetScoreText(currentScore.ToString());

        EventManager.BroadcastLevelIntroStart();
        InitializeEnvironmentColors();
    }

    private void RestartLevel()
    {
        ResetObstacles();

        InitializeScorePopUp();

        StartLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.BroadcastPauseStateChange(!isPaused);
        }

        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                EventManager.BroadcastLevelRestart();
            }

            if (runningLevelIntro)
            {
                ManageLevelIntro();
            }

            if (spawnObstacles)
            {
                ManageObstacleSpawning();
            }

            if (displayingScorePopUp)
            {
                PlayFinalScorePopUpEffect();
            }

            UpdateEnvironmentColor();
        }
    }

    private void CheckDirection(Vector2 movementVector)
    {
        int newPlayerMovementDirection = 0;

        if (movementVector.x < -forwardDirectionThreshold)
        {
            newPlayerMovementDirection = -1;
        }
        else if (movementVector.x > forwardDirectionThreshold)
        {
            newPlayerMovementDirection = 1;
        }
        //Debug.Log("newPlayerMovementDirection: " + newPlayerMovementDirection);

        if (playerMovementDirection == newPlayerMovementDirection)
        {
            if (!validDirection)
            {
                if (Time.time - directionChangeTime >= directionChangeValidationTime)
                {
                    validDirection = true;
                }
            }
        }
        else
        {
            validDirection = false;
            directionChangeTime = Time.time;
        }

        playerMovementDirection = newPlayerMovementDirection;
    }

    private void ManageLevelIntro()
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
            countingScore = true;
        }
    }

    #region Score management
    private void ManageScore(Vector2 movementVector)
    {
        rawScore += scorePerOneUnitTraveled * movementVector.y;
        currentScore = Mathf.FloorToInt(rawScore);

        SetScoreText(currentScore.ToString());
    }

    private void SetScoreText(string newScoreText)
    {
        scoreValueText.text = newScoreText;
    }

    private void InitializeScorePopUp()
    {
        ResetScorePopUp();

        if (currentScore > 0)
        {
            scorePopUpStartTime = Time.time;
            SetScorePopUpText(currentScore.ToString());
            displayingScorePopUp = true;
        }
    }

    private void ResetScorePopUp()
    {
        scorePopUpText.rectTransform.position = new Vector3(Screen.width / 2, Screen.height * 1.1f, 0);
        scorePopUpPosition = new Vector3(Screen.width / 2, Screen.height * 0.75f, 0);
        SetScorePopUpText("");
        displayingScorePopUp = false;

        Color scorePopUpFadeColor;
        scorePopUpFadeColor = scorePopUpText.color;
        scorePopUpFadeColor.a = scorePopUpTextOriginalAlpha;
        scorePopUpText.color = scorePopUpFadeColor;

        scorePopUpFadeColor = scorePopUpText.GetComponent<Shadow>().effectColor;
        scorePopUpFadeColor.a = scorePopUpShadowOriginalAlpha;
        scorePopUpText.GetComponent<Shadow>().effectColor = scorePopUpFadeColor;
    }

    private void SetScorePopUpText(string newPopUpText)
    {
        scorePopUpText.text = newPopUpText;
    }

    private void PlayFinalScorePopUpEffect()
    {
        scorePopUpText.rectTransform.position = Vector3.SmoothDamp(scorePopUpText.rectTransform.position,
            scorePopUpPosition, ref scorePopUpRefVelocity, scorePopUpSmoothTime);

        float timeSinceStartedPopUp = Time.time - scorePopUpStartTime;

        if (timeSinceStartedPopUp > scorePopUpDuration * 0.75f)
        {
            Color scorePopUpFadeColor;
            scorePopUpFadeColor = scorePopUpText.color;
            scorePopUpFadeColor.a -= scorePopUpFadeStrength * Time.deltaTime;
            Debug.Log(scorePopUpFadeColor.a);
            scorePopUpText.color = scorePopUpFadeColor;

            scorePopUpFadeColor = scorePopUpText.GetComponent<Shadow>().effectColor;
            scorePopUpFadeColor.a -= scorePopUpFadeStrength * Time.deltaTime;
            scorePopUpText.GetComponent<Shadow>().effectColor = scorePopUpFadeColor;
        }

        if (timeSinceStartedPopUp > scorePopUpDuration)
        {
            ResetScorePopUp();
        }
    }
    #endregion

    #region Environment color management
    private void InitializeEnvironmentColors()
    {
        currentEnvironmentColorIndex = 0;
        nextEnvironmentColorIndex = GetNextEnvironmentColorIndex();
        environmentColorChangePhaseStartTime = Time.time;

        EventManager.BroadcastEnvironmentColorChange(environmentColors[currentEnvironmentColorIndex]);
    }

    private void UpdateEnvironmentColor()
    {
        if (colorChanging)
        {
            float timeSinceStarted = Time.time - environmentColorChangePhaseStartTime;
            float percentageCompleted = timeSinceStarted / environmentColorChangePhaseDuration;
            EventManager.BroadcastEnvironmentColorChange(
                Color.Lerp(environmentColors[currentEnvironmentColorIndex],
                environmentColors[nextEnvironmentColorIndex],
                percentageCompleted));

            if (percentageCompleted >= 1)
            {
                GoToNextEnvironmentColor();
            }
        }
    }

    private void GoToNextEnvironmentColor()
    {
        currentEnvironmentColorIndex = nextEnvironmentColorIndex;
        nextEnvironmentColorIndex = GetNextEnvironmentColorIndex();
        environmentColorChangePhaseStartTime = Time.time;
    }

    private int GetNextEnvironmentColorIndex()
    {
        if (currentEnvironmentColorIndex + 1 < environmentColors.Length)
        {
            return currentEnvironmentColorIndex + 1;
        }

        return 0;
    }
    #endregion

    #region Obstacle spawning
    private void ManageObstacleSpawning()
    {
        if (Time.time - lastObstacleSpawnTime >= obstacleSpawnCooldownTimer)
        {
            SpawnObstacle();
            lastObstacleSpawnTime = Time.time;
            obstacleSpawnCooldownTimer = obstacleSpawnCooldownDuration;
        }
    }

    private bool ObstacleExistsInGivenPosition(Vector2 positionToCheck)
    {
        foreach (ObstacleController oc in obstaclePool)
        {
            if (oc.gameObject.activeSelf)
            {
                Vector3 existingObstaclePos = oc.transform.position;
                Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();

                //Debug.Log("x.1: existingObstaclePos.x + currentGridOffset.x: " + (existingObstaclePos.x + currentGridOffset.x));
                //Debug.Log("x.2: positionToCheck.x: " + positionToCheck.x);
                //Debug.Log("x.3: Mathf.Abs((existingObstaclePos.x + currentGridOffset.x) - positionToCheck.x): " + Mathf.Abs((existingObstaclePos.x + currentGridOffset.x) - positionToCheck.x));
                //Debug.Log("x.4: obstacleAtSamePositionThreshold: " + obstacleAtSamePositionThreshold);

                //Debug.Log("y.1: existingObstaclePos.z + currentGridOffset.y: " + (existingObstaclePos.z + currentGridOffset.y));
                //Debug.Log("y.2: positionToCheck.y: " + positionToCheck.y);
                //Debug.Log("y.3: Mathf.Abs((existingObstaclePos.z + currentGridOffset.y) - positionToCheck.y): " + Mathf.Abs((existingObstaclePos.z + currentGridOffset.y) - positionToCheck.y));
                //Debug.Log("y.4: obstacleAtSamePositionThreshold: " + obstacleAtSamePositionThreshold);

                if (Mathf.Abs((existingObstaclePos.x + currentGridOffset.x) - positionToCheck.x) <= obstacleAtSamePositionThreshold
                && Mathf.Abs((existingObstaclePos.z + currentGridOffset.y) - positionToCheck.y) <= obstacleAtSamePositionThreshold)
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

    private void SpawnObstacle()
    {
        if (validDirection)
        {
            float spawnTypeResult = Random.Range(0f, 1f);

            if (spawnTypeResult <= directionalObstacleSpawnChance)
            {
                SpawnDirectionalObstacle();

                return;
            }
        }

        SpawnRandomObstacle();
    }

    private void SpawnRandomObstacle()
    {
        Vector2 spawnPosition; //= new Vector2(0f, 5f);
        spawnPosition.x = obstacleXAxisSpawnPositions[Random.Range(0, obstacleXAxisSpawnPositions.Length)];
        spawnPosition.y = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)];

        //if (spawnPosition.x < -1 || spawnPosition.x > 1)
        //{
        //    spawnPosition.y -= 2;
        //}

        //Prevent multiple obstacles spawning at the same location
        if (ObstacleExistsInGivenPosition(spawnPosition))
        {
            SpawnRandomObstacle();
        }
        else
        {
            GetAvailableObstacle().Spawn(spawnPosition);
        }
    }

    private void SpawnDirectionalObstacle()
    {
        switch (playerMovementDirection)
        {
            case 0:
                SpawnFrontObstacle();
                break;
            case -1:
                SpawnLeftObstacle();
                break;
            case 1:
                SpawnRightObstacle();
                break;
            default:
                break;
        }
    }

    private void SpawnFrontObstacle()
    {
        Vector2 spawnPosition = new Vector2(0f, 5f);
        spawnPosition.y = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)];

        if (ObstacleExistsInGivenPosition(spawnPosition))
        {
            SpawnRandomObstacle();
        }
        else
        {
            GetAvailableObstacle().Spawn(spawnPosition);
        }
    }

    private void SpawnLeftObstacle()
    {
        Vector2 spawnPosition = new Vector2(-3f, 4f);

        if (ObstacleExistsInGivenPosition(spawnPosition))
        {
            SpawnRandomObstacle();
        }
        else
        {
            GetAvailableObstacle().Spawn(spawnPosition);
        }
    }

    private void SpawnRightObstacle()
    {
        Vector2 spawnPosition = new Vector2(3f, 4f);

        if (ObstacleExistsInGivenPosition(spawnPosition))
        {
            SpawnRandomObstacle();
        }
        else
        {
            GetAvailableObstacle().Spawn(spawnPosition);
        }
    }
    #endregion
}
