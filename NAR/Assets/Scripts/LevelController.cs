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
    [SerializeField]
    int obstaclePoolSize = 20;
    List<ObstacleController> obstaclePool = new List<ObstacleController>();

    bool spawningObstacles = false;
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
    int[] obstacleZAxisSpawnPositions = new int[6] { 8, 9, 10, 11, 12, 13 };

    bool runningLevelIntro = false;
    float levelStartTime = 0f;
    float levelIntroDuration = 10f;

    [SerializeField]
    GameObject collectiblePrefab;
    [SerializeField]
    int collectiblePoolSize = 3;
    List<CollectibleController> collectiblePool = new List<CollectibleController>();

    bool spawningCollectibles = false;
    float firstCollectibleSpawnDelay = 10f;
    float collectibleSpawnCooldownDuration = 0f;
    float lastCollectibleSpawnTime = 0f;
    float collectibleSpawnCooldownMin = 10f;
    float collectibleSpawnCooldownMax = 10f;
    int[] collectibleXAxisSpawnPositions = new int[7] { -3, -2, -1, 0, 1, 2, 3 };
    int[] collectibleZAxisSpawnPositions = new int[4] { 10, 11, 12, 13 };

    [SerializeField]
    GameObject enemyPrefab;
    [SerializeField]
    int enemyPoolSize = 4;
    List<EnemyController> enemyPool = new List<EnemyController>();
    [SerializeField]
    bool spawnEnemies = true;

    bool spawningEnemies = false;
    float firstEnemySpawnDelay = 10f;
    float enemySpawnCooldown = 10f;
    float enemySpawnCooldownMin = 10f;
    float enemySpawnCooldownMax = 20f;
    float lastEnemySpawnTime = 0f;
    int[] enemyXAxisSpawnPositions = new int[4] { -5, -4, 4, 5 };
    int enemyZAxisSpawnPosition = -3;

    //[SerializeField]
    //float environmentColorPhaseDuration = 50f;
    //[SerializeField]
    //float environmentColorChangeDuration = 10f;
    [SerializeField]
    bool useChanginEnvironmentColors = true;
    [SerializeField]
    Color[] environmentColors;
    Color currentEnvironmentColor;
    //float environmentColorPhaseStartTime = 0f;
    //float environmentColorChangeStartTime = 0f;
    int currentEnvironmentColorIndex = 0;
    int chaningColors = 0;

    [SerializeField]
    Text scoreValueText;
    //bool countingScore = false;
    //float scorePerOneUnitTraveled = 1f;
    //float rawScore = 0f;
    //int currentScore = 0;
    [SerializeField]
    Image scoreTimerSliderBackgroundImage;
    [SerializeField]
    Image scoreTimerSliderFillImage;

    [SerializeField]
    Slider scoreMultiplierTimerSlider;
    float scoreMultiplierStartTime = 0f;
    float scoreMultiplierDuration = 12f;
    int[] scoreMultiplierTiers = new int[6] { 1, 5, 10, 15, 20, 25 };
    int currentScoreMultiplier = 1;
    int highestScoreMultiplier = 1;

    [SerializeField]
    Text scorePopUpText;
    Vector3 scorePopUpRefVelocity = Vector3.zero;
    Vector3 scorePopUpTargetPosition;
    bool displayingScorePopUp = false;
    float scorePopUpSmoothTime = 0.5f;
    float scorePopUpDuration = 5f;
    float scorePopUpStartTime = 0f;
    float scorePopUpFadeStrength = 3f;
    float scorePopUpTextOriginalAlpha = 0f;
    float scorePopUpShadowOriginalAlpha = 0f;

    int[] totalCollectedCollectibles;
    int[] currentCollectedCollectibles;

    bool isPaused = false;

    private void OnEnable()
    {
        EventManager.OnLevelRestart += OnLevelRestart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnPlayerMovement += OnPlayerMovement;
        EventManager.OnScoreMultiplierChange += OnScoreMultiplierChange;
        EventManager.OnRequestCurrentEnvironmentColor += OnRequestCurrentEnvironmentColor;
        EventManager.OnRequestCurrentScoreMultiplier += OnRequestCurrentScoreMultiplier;
        EventManager.OnCollectibleCollected += OnCollectibleCollected;
        EventManager.OnPlayerDamaged += OnPlayerDamaged;

        SetScorePopUpText("");
        scorePopUpTextOriginalAlpha = scorePopUpText.color.a;
        scorePopUpShadowOriginalAlpha = scorePopUpText.GetComponent<Shadow>().effectColor.a;
    }

    private void OnDisable()
    {
        EventManager.OnLevelRestart -= OnLevelRestart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnPlayerMovement -= OnPlayerMovement;
        EventManager.OnScoreMultiplierChange -= OnScoreMultiplierChange;
        EventManager.OnRequestCurrentEnvironmentColor -= OnRequestCurrentEnvironmentColor;
        EventManager.OnRequestCurrentScoreMultiplier -= OnRequestCurrentScoreMultiplier;
        EventManager.OnCollectibleCollected -= OnCollectibleCollected;
        EventManager.OnPlayerDamaged -= OnPlayerDamaged;
    }

    #region Subscribers
    private void OnLevelRestart()
    {
        Debug.Log("*** Collectibles Collected ***");

        for (int i = 0; i < currentCollectedCollectibles.Length; i++)
        {
            Debug.Log("Tier " + (i + 1) + ": " + currentCollectedCollectibles[i] + " pcs");
        }

        Debug.Log(" *** ");


        for (int i = 0; i < totalCollectedCollectibles.Length; i++)
        {
            totalCollectedCollectibles[i] += currentCollectedCollectibles[i];
        }

        RestartLevel();
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

    private void OnPlayerMovement(Vector3 movementVector)
    {
        CheckPlayerMovementDirection(movementVector);
    }

    private void OnScoreMultiplierChange(int newScoreMultiplier, int multiplierTier)
    {
        currentEnvironmentColorIndex = multiplierTier;
        currentEnvironmentColor = environmentColors[currentEnvironmentColorIndex];
        SetScoreTimerSliderColors(currentEnvironmentColor);
        EventManager.BroadcastEnvironmentColorChange(currentEnvironmentColor);

        currentScoreMultiplier = newScoreMultiplier;
    }

    private Color OnRequestCurrentEnvironmentColor()
    {
        return currentEnvironmentColor;
    }

    private int OnRequestCurrentScoreMultiplier()
    {
        return currentScoreMultiplier;
    }

    private void OnCollectibleCollected(int collectibleTypeIndex)
    {
        CollectibleController.ECollectibleType collectibleType = (CollectibleController.ECollectibleType)collectibleTypeIndex;

        currentCollectedCollectibles[GetScoreMultiplierTierIndex(currentScoreMultiplier)]++;

        switch (collectibleType)
        {
            case CollectibleController.ECollectibleType.ScoreMultiplier:
                IncreaseScoreMultiplier();
                break;
            default:
                break;
        }
    }

    private void OnPlayerDamaged()
    {
        DecreaseScoreMultiplier();
    }
    #endregion

    #region Level initialization
    private void Start()
    {
        totalCollectedCollectibles = new int[scoreMultiplierTiers.Length];
        for (int i = 0; i < totalCollectedCollectibles.Length; i++)
        {
            totalCollectedCollectibles[i] = 0;
        }

        currentCollectedCollectibles = new int[totalCollectedCollectibles.Length];
        for (int i = 0; i < currentCollectedCollectibles.Length; i++)
        {
            currentCollectedCollectibles[i] = 0;
        }

        InitializeSpawnablePools();
        StartLevel();
    }

    private void InitializeSpawnablePools()
    {
        InitializeObstaclePool();
        InitializeCollectiblePool();
        InitializeEnemyPool();
    }

    private void StartLevel()
    {
        spawningObstacles = false;
        spawningCollectibles = false;
        spawningEnemies = false;
        //countingScore = false;
        //rawScore = 0;
        //currentScore = 0;
        SetScoreText("SPEED: x" + currentScoreMultiplier.ToString());

        InitializeEnvironmentColors();
        ResetScoreMultiplier();

        currentCollectedCollectibles = new int[totalCollectedCollectibles.Length];
        for (int i = 0; i < currentCollectedCollectibles.Length; i++)
        {
            currentCollectedCollectibles[i] = 0;
        }

        levelStartTime = Time.time;
        runningLevelIntro = true;
        EventManager.BroadcastLevelIntroStart();
    }

    private void RestartLevel()
    {
        ResetObstacles();
        ResetCollectibles();
        ResetEnemies();

        StartScorePopUp();

        StartLevel();
    }
    #endregion

    #region Update loop
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.BroadcastPauseStateChange(!isPaused);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            EventManager.BroadcastLevelRestart();
        }
        else if (!isPaused & runningLevelIntro && Input.anyKeyDown)
        {
            SkipIntro();
        }

        if (!isPaused)
        {
            if (runningLevelIntro)
            {
                ManageLevelIntro();
            }

            if (spawningObstacles)
            {
                ManageObstacleSpawning();
            }

            if (spawningCollectibles)
            {
                ManageCollectibleSpawning();
            }

            if (spawnEnemies && spawningEnemies)
            {
                ManageEnemySpawning();
            }

            if (displayingScorePopUp)
            {
                PlayFinalScorePopUpEffect();
            }

            //if (useChanginEnvironmentColors)
            //{
            //    ManageEnvironmentColorChanging();
            //}

            ManageScoreMultiplier();
        }
    }
    #endregion

    #region Player movement direction check
    private void CheckPlayerMovementDirection(Vector3 movementVector)
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
    #endregion

    #region Level intro
    private void ManageLevelIntro()
    {
        //float levelStartTimer = levelIntroDuration - (Time.time - levelStartTime);

        //if (levelStartTimer <= 0)
        //{
        //    levelStartTimer = 0;
        //}
        //Debug.Log("Level starting in: " + levelStartTimer);

        if (Time.time - levelStartTime >= levelIntroDuration)
        {
            FinishLevelIntro();
        }
    }

    private void FinishLevelIntro()
    {
        spawningObstacles = true;
        spawningCollectibles = true;
        spawningEnemies = true;
        //countingScore = true;

        runningLevelIntro = false;
        EventManager.BroadcastLevelIntroFinished();
    }

    public void SkipIntro()
    {
        ResetScorePopUp();
        FinishLevelIntro();
    }
    #endregion

    #region Score management
    private void ManageScoreMultiplier()
    {
        if (currentScoreMultiplier > 1)
        {
            float scoreMultiplierTimer = Time.time - scoreMultiplierStartTime;
            float scoreMultiplierTimerPercentage = scoreMultiplierTimer / scoreMultiplierDuration;
            float scoreMultiplierTimeLeft = Mathf.Clamp01(1 - scoreMultiplierTimerPercentage);

            scoreMultiplierTimerSlider.value = scoreMultiplierTimeLeft;

            if (Time.time - scoreMultiplierStartTime > scoreMultiplierDuration)
            {
                EventManager.BroadcastLevelRestart();
                ResetScoreMultiplier();
            }

            string scoreText = "SPEED: x" + currentScoreMultiplier.ToString();
            SetScoreText(scoreText);

            scoreValueText.gameObject.SetActive(true);
        }
        else
        {
            scoreValueText.gameObject.SetActive(false);
        }
    }

    private int GetScoreMultiplierTierIndex(int scoreMultiplier)
    {
        for (int i = 0; i < scoreMultiplierTiers.Length - 1; i++)
        {
            if (scoreMultiplier < scoreMultiplierTiers[i + 1])
            {
                return i;
            }
        }

        return scoreMultiplierTiers.Length - 1;
    }

    private void IncreaseScoreMultiplier()
    {
        scoreMultiplierStartTime = Time.time;
        currentScoreMultiplier++;

        if (currentScoreMultiplier > highestScoreMultiplier)
        {
            highestScoreMultiplier = currentScoreMultiplier;
        }

        EventManager.BroadcastScoreMultiplierChange(currentScoreMultiplier, GetScoreMultiplierTierIndex(currentScoreMultiplier));
    }

    private void DecreaseScoreMultiplier()
    {
        currentScoreMultiplier = scoreMultiplierTiers[GetScoreMultiplierTierIndex(currentScoreMultiplier)] - 1;

        if (currentScoreMultiplier < 1)
        {
            EventManager.BroadcastLevelRestart();
            ResetScoreMultiplier();
        }

        EventManager.BroadcastScoreMultiplierChange(currentScoreMultiplier, GetScoreMultiplierTierIndex(currentScoreMultiplier));
    }

    private void ResetScoreMultiplier()
    {
        currentScoreMultiplier = 1;
        highestScoreMultiplier = currentScoreMultiplier;
        EventManager.BroadcastScoreMultiplierChange(currentScoreMultiplier, GetScoreMultiplierTierIndex(currentScoreMultiplier));
    }

    //private void ManageScore(Vector3 movementVector)
    //{
    //    string scoreText = "SPEED: x" + currentScoreMultiplier.ToString();
    //    SetScoreText(scoreText);
    //}

    private void SetScoreText(string newScoreText)
    {
        scoreValueText.text = newScoreText;
    }

    private void SetScoreTimerSliderColors(Color environmentColor)
    {
        scoreTimerSliderBackgroundImage.color = environmentColor;
        scoreTimerSliderFillImage.color = ColorHelper.FindComplementaryColor(environmentColor);
    }

    private void SetTextColors(Text textComponent, Color environmentColor)
    {
        scoreTimerSliderBackgroundImage.color = environmentColor;
        scoreTimerSliderFillImage.color = ColorHelper.FindComplementaryColor(environmentColor);
    }

    #region Score PopUp management
    private void StartScorePopUp()
    {
        ResetScorePopUp();

        if (highestScoreMultiplier > 1)
        {
            scorePopUpStartTime = Time.time;
            SetScorePopUpText("MAX SPEED: x" + highestScoreMultiplier.ToString());
            ColorHelper.SetTextColor(scorePopUpText, currentEnvironmentColor, true);
            displayingScorePopUp = true;
        }
    }

    private void ResetScorePopUp()
    {
        scorePopUpText.rectTransform.position = new Vector3(Screen.width / 2, Screen.height * 1.1f, 0);
        scorePopUpTargetPosition = new Vector3(Screen.width / 2, Screen.height * 0.75f, 0);
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
            scorePopUpTargetPosition, ref scorePopUpRefVelocity, scorePopUpSmoothTime);

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
    #endregion
    #endregion

    #region Environment color management
    private void InitializeEnvironmentColors()
    {
        chaningColors = 0;
        currentEnvironmentColorIndex = 0;
        //environmentColorPhaseStartTime = Time.time;

        currentEnvironmentColor = environmentColors[currentEnvironmentColorIndex];
        SetScoreTimerSliderColors(currentEnvironmentColor);
        EventManager.BroadcastEnvironmentColorChange(currentEnvironmentColor);
    }

    //private void ManageEnvironmentColorChanging()
    //{
    //    if (chaningColors != 0)
    //    {
    //        ChangeColors(chaningColors);
    //    }
    //    //else if (Time.time - environmentColorPhaseStartTime > environmentColorPhaseDuration)
    //    //{
    //    //    StartChangingColors();
    //    //}
    //}

    //private void StartChangingColors(int direction)
    //{
    //    //environmentColorChangeStartTime = Time.time;
    //    chaningColors = direction;
    //}

    //private void ChangeColors(int direction)
    //{
    //    chaningColors = 0;
    //    //float timeSinceStarted = Time.time - environmentColorChangeStartTime;
    //    //float percentageCompleted = timeSinceStarted / environmentColorChangeDuration;

    //    if (direction == 1)
    //    {
    //        GoToNextEnvironmentColorIndex();



    //        //currentEnvironmentColor = Color.Lerp(environmentColors[currentEnvironmentColorIndex],
    //        //    environmentColors[GetNextEnvironmentColorIndex(currentEnvironmentColorIndex)], percentageCompleted);

    //        //if (percentageCompleted >= 1)
    //        //{
    //        //    chaningColors = 0;
    //        //    GoToNextEnvironmentColorIndex();
    //        //}
    //    }
    //    else if (direction == -1)
    //    {
    //        GoToPreviousEnvironmentColorIndex();



    //        //currentEnvironmentColor = Color.Lerp(environmentColors[currentEnvironmentColorIndex],
    //        //    environmentColors[GetPreviousEnvironmentColorIndex(currentEnvironmentColorIndex)], percentageCompleted);

    //        //if (percentageCompleted >= 1)
    //        //{
    //        //    chaningColors = 0;
    //        //    GoToPreviousEnvironmentColorIndex();
    //        //}
    //    }

    //    currentEnvironmentColor = environmentColors[currentEnvironmentColorIndex];
    //    SetScoreTimerSliderColors(currentEnvironmentColor);
    //    EventManager.BroadcastEnvironmentColorChange(currentEnvironmentColor);
    //}

    //private void GoToNextEnvironmentColorIndex()
    //{
    //    currentEnvironmentColorIndex = GetNextEnvironmentColorIndex(currentEnvironmentColorIndex);
    //    //environmentColorPhaseStartTime = Time.time;
    //}

    //private void GoToPreviousEnvironmentColorIndex()
    //{
    //    currentEnvironmentColorIndex = GetPreviousEnvironmentColorIndex(currentEnvironmentColorIndex);
    //    //environmentColorPhaseStartTime = Time.time;
    //}

    //private int GetNextEnvironmentColorIndex(int currentIndex)
    //{
    //    if (currentIndex + 1 < environmentColors.Length)
    //    {
    //        return currentIndex + 1;
    //    }

    //    return 0;
    //}

    //private int GetPreviousEnvironmentColorIndex(int currentIndex)
    //{
    //    if (currentIndex - 1 >= 0)
    //    {
    //        return currentIndex - 1;
    //    }

    //    return 0;
    //}
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

    private bool ObstacleExistsInGivenPosition(Vector3 positionToCheck)
    {
        foreach (ObstacleController oc in obstaclePool)
        {
            if (oc.gameObject.activeSelf)
            {
                Vector3 existingObstaclePos = oc.transform.position;
                Vector2 currentGridOffset = EventManager.BroadcastRequestGridOffset();

                if (Mathf.Abs((existingObstaclePos.x + currentGridOffset.x) - positionToCheck.x) <= obstacleAtSamePositionThreshold
                && Mathf.Abs((existingObstaclePos.z + currentGridOffset.y) - positionToCheck.z) <= obstacleAtSamePositionThreshold)
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

    private void InitializeObstaclePool()
    {
        if (obstaclePool.Count > 0)
        {
            foreach (ObstacleController oc in obstaclePool)
            {
                oc.Despawn();
                Destroy(oc.gameObject);
            }

            obstaclePool = new List<ObstacleController>();
        }

        for (int i = 0; i < obstaclePoolSize; i++)
        {
            AddObstacleToPool();
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

    public bool TrySpawnObstacleAtPosition(Vector3 spawnPosition)
    {
        //Prevent multiple obstacles spawning at the same location
        if (!ObstacleExistsInGivenPosition(spawnPosition))
        {
            GetAvailableObstacle().Spawn(spawnPosition);
            return true;
        }

        return false;
    }

    private void SpawnRandomObstacle()
    {
        Vector3 spawnPosition = Vector3.zero;
        spawnPosition.x = obstacleXAxisSpawnPositions[Random.Range(0, obstacleXAxisSpawnPositions.Length)];
        spawnPosition.z = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)]
            + (currentScoreMultiplier - 1);

        if (!TrySpawnObstacleAtPosition(spawnPosition))
        {
            SpawnRandomObstacle();
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
        Vector3 spawnPosition = new Vector3(0f, 0f, 5f + (currentScoreMultiplier - 1));
        spawnPosition.z = obstacleZAxisSpawnPositions[Random.Range(0, obstacleZAxisSpawnPositions.Length)];

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
        Vector3 spawnPosition = new Vector3(-3f, 0f, 4f + (currentScoreMultiplier - 1));

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
        Vector3 spawnPosition = new Vector3(3f, 0f, 4f + (currentScoreMultiplier - 1));

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

    #region Collectible spawning
    private void ManageCollectibleSpawning()
    {
        float finalSpawnCooldownDuration = collectibleSpawnCooldownDuration - currentScoreMultiplier;
        if (finalSpawnCooldownDuration < collectibleSpawnCooldownDuration / 3)
        {
            finalSpawnCooldownDuration = collectibleSpawnCooldownDuration / 3;
        }

        if (Time.time - lastCollectibleSpawnTime >= finalSpawnCooldownDuration)
        {
            SpawnCollectible();
            lastCollectibleSpawnTime = Time.time;
            RandomizeCollectibleSpawnCooldownDuration();
        }
    }

    private void SpawnCollectible()
    {
        Vector3 spawnPosition = Vector3.zero;
        spawnPosition.x = collectibleXAxisSpawnPositions[Random.Range(0, collectibleXAxisSpawnPositions.Length)];
        spawnPosition.z = collectibleZAxisSpawnPositions[Random.Range(0, collectibleZAxisSpawnPositions.Length)]
            + (currentScoreMultiplier - 1);

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

        collectibleSpawnCooldownDuration = firstCollectibleSpawnDelay;
        lastCollectibleSpawnTime = Time.time;
    }

    private void InitializeCollectiblePool()
    {
        if (collectiblePool.Count > 0)
        {
            foreach (CollectibleController cc in collectiblePool)
            {
                cc.Despawn();
                Destroy(cc.gameObject);
            }

            collectiblePool = new List<CollectibleController>();
        }

        for (int i = 0; i < collectiblePoolSize; i++)
        {
            AddCollectibleToPool();
        }

        collectibleSpawnCooldownDuration = firstCollectibleSpawnDelay;
        lastCollectibleSpawnTime = Time.time;
    }
    #endregion

    #region Enemy spawning
    private void ManageEnemySpawning()
    {
        if (Time.time - lastEnemySpawnTime >= enemySpawnCooldown)
        {
            SpawnEnemy();
            lastEnemySpawnTime = Time.time;
            RandomizeEnemySpawnCooldown();
        }
    }

    private void RandomizeEnemySpawnCooldown()
    {
        enemySpawnCooldown = Random.Range(enemySpawnCooldownMin, enemySpawnCooldownMax);
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = Vector3.zero;
        spawnPosition.x = enemyXAxisSpawnPositions[Random.Range(0, enemyXAxisSpawnPositions.Length)];
        spawnPosition.z = enemyZAxisSpawnPosition;

        GetAvailableEnemy().Spawn(spawnPosition, this);
    }

    private EnemyController AddEnemyToPool()
    {
        EnemyController newEnemy = Instantiate(enemyPrefab, transform).GetComponent<EnemyController>();
        newEnemy.Initialize();
        enemyPool.Add(newEnemy);

        return newEnemy;
    }

    private EnemyController GetAvailableEnemy()
    {
        for (int i = 0; i < enemyPool.Count; i++)
        {
            if (!enemyPool[i].gameObject.activeSelf)
            {
                return enemyPool[i];
            }
        }

        return AddEnemyToPool();
    }

    private void ResetEnemies()
    {
        foreach (EnemyController ec in enemyPool)
        {
            if (ec.gameObject.activeSelf)
            {
                ec.Despawn();
            }
        }

        enemySpawnCooldown = firstEnemySpawnDelay;
        lastEnemySpawnTime = Time.time;
    }

    private void InitializeEnemyPool()
    {
        if (enemyPool.Count > 0)
        {
            foreach (EnemyController ec in enemyPool)
            {
                ec.Despawn();
                Destroy(ec.gameObject);
            }

            enemyPool = new List<EnemyController>();
        }

        for (int i = 0; i < enemyPoolSize; i++)
        {
            AddEnemyToPool();
        }

        enemySpawnCooldown = firstEnemySpawnDelay;
        lastEnemySpawnTime = Time.time;
    }
    #endregion
}
