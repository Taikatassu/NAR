using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    // TODO: Modify obstacle spawning to provide more challenge (less downtime for the player)
    // Make the environment more interesting
    //      - Grid material glowing periodically (color values from light to dark to light etc, or add emission to grid shader and change emission value?)
    //      - Score multipliers at certain thresholds? (every minute or so)
    // Actual goal reaching?
    //      - Gameplay separated by distinct waypoints?

    // TODO: Implement color changing to the level goal element as well!

    [SerializeField]
    GameObject obstaclePrefab;
    int obstaclePoolSize = 20;
    List<ObstacleController> obstaclePool = new List<ObstacleController>();

    bool spawnObstacles = false;
    float lastObstacleSpawnTime = 0f;
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

    [SerializeField]
    GameObject collectiblePrefab;
    int collectiblePoolSize = 5;
    List<CollectibleController> collectiblePool = new List<CollectibleController>();

    bool spawningCollectibles = false;
    float collectibleSpawnCooldownDuration = 0f;
    float lastCollectibleSpawnTime = 0f;
    float collectibleSpawnCooldownMin = 10f;
    float collectibleSpawnCooldownMax = 10f;
    int[] collectibleXAxisSpawnPositions = new int[7] { -3, -2, -1, 0, 1, 2, 3 };
    int[] collectibleZAxisSpawnPositions = new int[4] { 10, 11, 12, 13 };

    [SerializeField]
    float environmentColorPhaseDuration = 50f;
    [SerializeField]
    float environmentColorChangeDuration = 10f;
    [SerializeField]
    bool useChanginEnvironmentColors = true;
    [SerializeField]
    Color[] environmentColors;
    Color currentEnvironmentColor;
    float environmentColorPhaseStartTime = 0f;
    float environmentColorChangeStartTime = 0f;
    int currentEnvironmentColorIndex = 0;
    bool chaningColors = false;

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
    int scoreMultiplier = 1;
    float scoreMultiplierDuration = 12f;
    float scoreMultiplierStartTime = 0f;

    bool isPaused = false;

    private void Start()
    {
        for (int i = 0; i < obstaclePoolSize; i++)
        {
            AddObstacleToPool();
        }

        for (int i = 0; i < collectiblePoolSize; i++)
        {
            AddCollectibleToPool();
        }

        StartLevel();
    }

    private void OnEnable()
    {
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnPlayerMovement += OnPlayerMovement;
        EventManager.OnCollectibleCollected += OnCollectibleCollected;
        EventManager.OnRequestCurrentEnvironmentColor += OnRequestCurrentEnvironmentColor;

        SetScorePopUpText("");
        scorePopUpTextOriginalAlpha = scorePopUpText.color.a;
        scorePopUpShadowOriginalAlpha = scorePopUpText.GetComponent<Shadow>().effectColor.a;
    }

    private void OnDisable()
    {
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnCollectibleCollected -= OnCollectibleCollected;
        EventManager.OnRequestCurrentEnvironmentColor -= OnRequestCurrentEnvironmentColor;
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

    private void OnCollectibleCollected(int collectibleTypeIndex)
    {
        CollectibleController.ECollectibleType collectibleType = (CollectibleController.ECollectibleType)collectibleTypeIndex;

        switch (collectibleType)
        {
            case CollectibleController.ECollectibleType.ScoreMultiplier:
                IncreaseScoreMultiplier();
                break;
            default:
                break;
        }
    }

    private void StartLevel()
    {
        levelStartTime = Time.time;
        spawnObstacles = false;
        spawningCollectibles = false;
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
        ResetCollectibles();

        InitializeScorePopUp();
        ResetScoreMultiplier();

        StartLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.BroadcastPauseStateChange(!isPaused);
        }

        if(runningLevelIntro && Input.GetKeyDown(KeyCode.Space))
        {
            ResetScorePopUp();
            FinishLevelIntro();
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

            if (spawningCollectibles)
            {
                ManageCollectibleSpawning();
            }

            if (displayingScorePopUp)
            {
                PlayFinalScorePopUpEffect();
            }

            if (useChanginEnvironmentColors)
            {
                ManageEnvironmentColorChanging();
            }
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
            FinishLevelIntro();
        }
    }

    private void FinishLevelIntro()
    {
        runningLevelIntro = false;
        spawnObstacles = true;
        RandomizeCollectibleSpawnCooldownDuration();
        lastCollectibleSpawnTime = Time.time;
        spawningCollectibles = true;
        EventManager.BroadcastLevelIntroFinished();
        countingScore = true;
    }

    #region Score management
    private void ManageScore(Vector2 movementVector)
    {
        ManageScoreMultiplier();

        rawScore += scorePerOneUnitTraveled * scoreMultiplier * movementVector.y;
        currentScore = Mathf.FloorToInt(rawScore);

        string scoreText = (scoreMultiplier > 1) ? currentScore.ToString() + " x" + scoreMultiplier.ToString() : currentScore.ToString();
        SetScoreText(scoreText);
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

    private void ManageScoreMultiplier()
    {
        if (scoreMultiplier > 1)
        {
            if (Time.time - scoreMultiplierStartTime > scoreMultiplierDuration)
            {
                ResetScoreMultiplier();
            }
        }
    }

    private void IncreaseScoreMultiplier()
    {
        scoreMultiplier++;
        scoreMultiplierStartTime = Time.time;
    }

    private void ResetScoreMultiplier()
    {
        scoreMultiplier = 1;
    }
    #endregion

    #region Environment color management
    private void InitializeEnvironmentColors()
    {
        chaningColors = false;
        currentEnvironmentColorIndex = 0;
        environmentColorPhaseStartTime = Time.time;

        currentEnvironmentColor = environmentColors[currentEnvironmentColorIndex];
        EventManager.BroadcastEnvironmentColorChange(currentEnvironmentColor);
    }

    private void ManageEnvironmentColorChanging()
    {
        if (chaningColors)
        {
            ChangeColors();
        }
        else if (Time.time - environmentColorPhaseStartTime > environmentColorPhaseDuration)
        {
            StartChangingColors();
        }
    }

    private void StartChangingColors()
    {
        environmentColorChangeStartTime = Time.time;
        chaningColors = true;
    }

    private void ChangeColors()
    {
        float timeSinceStarted = Time.time - environmentColorChangeStartTime;
        float percentageCompleted = timeSinceStarted / environmentColorChangeDuration;
        currentEnvironmentColor = Color.Lerp(environmentColors[currentEnvironmentColorIndex],
            environmentColors[GetNextEnvironmentColorIndex(currentEnvironmentColorIndex)], percentageCompleted);

        EventManager.BroadcastEnvironmentColorChange(currentEnvironmentColor);

        if (percentageCompleted >= 1)
        {
            chaningColors = false;
            GoToNextEnvironmentColorIndex();
        }
    }

    private void GoToNextEnvironmentColorIndex()
    {
        currentEnvironmentColorIndex = GetNextEnvironmentColorIndex(currentEnvironmentColorIndex);
        environmentColorPhaseStartTime = Time.time;
    }

    private int GetNextEnvironmentColorIndex(int currentIndex)
    {
        if (currentIndex + 1 < environmentColors.Length)
        {
            return currentIndex + 1;
        }

        return 0;
    }

    private Color OnRequestCurrentEnvironmentColor()
    {
        return currentEnvironmentColor;
    }

    private void ResetEnvironmentColor()
    {

    }
    #endregion

    #region Collectible spawning
    private void ManageCollectibleSpawning()
    {
        if (Time.time - lastCollectibleSpawnTime >= collectibleSpawnCooldownDuration)
        {
            SpawnCollectible();
            lastCollectibleSpawnTime = Time.time;
            RandomizeCollectibleSpawnCooldownDuration();
        }
    }

    private void SpawnCollectible()
    {
        Vector2 spawnPosition; //= new Vector2(0f, 5f);
        spawnPosition.x = collectibleXAxisSpawnPositions[Random.Range(0, collectibleXAxisSpawnPositions.Length)];
        spawnPosition.y = collectibleZAxisSpawnPositions[Random.Range(0, collectibleZAxisSpawnPositions.Length)]
            + (scoreMultiplier - 1);

        // Prevent collectibles spawning at the same location as existing obstacles
        // TODO: Clear all nearby obstacles (large trigger that despawns all colliding obstacles?)
        if (ObstacleExistsInGivenPosition(spawnPosition))
        {
            SpawnCollectible();
        }
        else
        {
            GetAvailableCollectible().Spawn(spawnPosition);
        }
    }

    private CollectibleController AddCollectibleToPool()
    {
        CollectibleController newCollectible = Instantiate(collectiblePrefab, transform).GetComponent<CollectibleController>();
        newCollectible.Initialize();
        collectiblePool.Add(newCollectible);

        return newCollectible;
    }

    private void RandomizeCollectibleSpawnCooldownDuration()
    {
        collectibleSpawnCooldownDuration = Random.Range(collectibleSpawnCooldownMin, collectibleSpawnCooldownMax);
    }

    private CollectibleController GetAvailableCollectible()
    {
        for (int i = 0; i < collectiblePool.Count; i++)
        {
            if (!collectiblePool[i].gameObject.activeSelf)
            {
                return collectiblePool[i];
            }
        }

        return AddCollectibleToPool();
    }

    private void ResetCollectibles()
    {
        foreach (CollectibleController cc in collectiblePool)
        {
            if (cc.gameObject.activeSelf)
            {
                cc.Despawn();
            }
        }
    }
    #endregion

    #region Obstacle spawning
    private void ManageObstacleSpawning()
    {
        if (Time.time - lastObstacleSpawnTime >= obstacleSpawnCooldownDuration)
        {
            SpawnObstacle();
            lastObstacleSpawnTime = Time.time;
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
        spawnPosition.y = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)]
            + (scoreMultiplier - 1);

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
        Vector2 spawnPosition = new Vector2(0f, 5f + (scoreMultiplier - 1));
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
        Vector2 spawnPosition = new Vector2(-3f, 4f + (scoreMultiplier - 1));

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
        Vector2 spawnPosition = new Vector2(3f, 4f + (scoreMultiplier - 1));

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
