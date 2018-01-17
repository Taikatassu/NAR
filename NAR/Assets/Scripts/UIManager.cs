using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // TODO: Relocate all UI related functionality to this script
    //          - ScorePopUp
    //          - AudioPopUp

    [SerializeField]
    GameObject buttonHolder;
    [SerializeField]
    GameObject pauseMenuHolder;
    [SerializeField]
    GameObject scoreDisplay;
    [SerializeField]
    GameObject skipIntroButtonHolder;
    [SerializeField]
    Image[] uiButtonImagesToFade;
    Text[] uiButtonTextsToFade;
    float[] uiButtonOriginalAlphas;
    float uiButtonFadeStartTime;
    float uiButtonFadeDuration = 5f;
    bool uiButtonsFading = false;

    public enum EUIState
    {
        Disabled,
        HUD,
        PauseMenu,
        Intro,
    }

    private void OnEnable()
    {
        InitializeUIButtonFading();

        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
        EventManager.OnLevelFinished += OnLevelFinished;
    }

    private void OnDisable()
    {
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
        EventManager.OnLevelFinished -= OnLevelFinished;
    }

    #region EventManager event subscribers
    private void OnPauseStateChanged(bool newState)
    {
        if (newState)
        {
            ChangeUIState(EUIState.PauseMenu);
        }
        else
        {
            ChangeUIState(EUIState.HUD);
        }
    }

    private void OnLevelIntroStart()
    {
        ChangeUIState(EUIState.Intro);
    }

    private void OnLevelIntroFinished()
    {
        ChangeUIState(EUIState.HUD);

        if (Application.isMobilePlatform)
        {
            StartUIButtonFade();
        }
        else
        {
            SetButtonAlphaToZero();
        }
    }

    private void OnLevelFinished()
    {
        ChangeUIState(EUIState.Intro);
    }
    #endregion

    #region Button event subscribers
    public void PauseButtonPressed(bool newState)
    {
        EventManager.BroadcastPauseStateChange(newState);
    }

    public void RestartButtonPressed()
    {
        EventManager.BroadcastPauseStateChange(false);
        EventManager.BroadcastLevelRestart();
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }
    #endregion

    private void Update()
    {
        ManageUIButtonFading();
    }

    #region UI button fade management
    private void ManageUIButtonFading()
    {
        if (uiButtonsFading)
        {
            UpdateUIButtonFadeState();
        }
    }

    private void InitializeUIButtonFading()
    {
        uiButtonOriginalAlphas = new float[uiButtonImagesToFade.Length];
        uiButtonTextsToFade = new Text[uiButtonImagesToFade.Length];

        for (int i = 0; i < uiButtonImagesToFade.Length; i++)
        {
            uiButtonOriginalAlphas[i] = uiButtonImagesToFade[i].color.a;
            uiButtonTextsToFade[i] = uiButtonImagesToFade[i].GetComponentInChildren<Text>();

            Color newColor = uiButtonTextsToFade[i].color;
            newColor.a = uiButtonOriginalAlphas[i];
            uiButtonTextsToFade[i].color = newColor;
        }
    }

    private void StartUIButtonFade()
    {
        ResetUIButtonFadeState();

        uiButtonFadeStartTime = Time.time;
        uiButtonsFading = true;
    }

    private void UpdateUIButtonFadeState()
    {
        float timeSinceStarted = Time.time - uiButtonFadeStartTime;
        float percentageCompleted = timeSinceStarted / uiButtonFadeDuration;

        for (int i = 0; i < uiButtonImagesToFade.Length; i++)
        {
            float newAlpha = Mathf.Lerp(uiButtonOriginalAlphas[i], 0, percentageCompleted);

            Color newColor = uiButtonImagesToFade[i].color;
            newColor.a = newAlpha;
            uiButtonImagesToFade[i].color = newColor;

            newColor = uiButtonTextsToFade[i].color;
            newColor.a = newAlpha;
            uiButtonTextsToFade[i].color = newColor;
        }

        if (percentageCompleted >= 1)
        {
            uiButtonsFading = false;
        }
    }

    private void ResetUIButtonFadeState()
    {
        for (int i = 0; i < uiButtonImagesToFade.Length; i++)
        {
            Color newColor = uiButtonImagesToFade[i].color;
            newColor.a = uiButtonOriginalAlphas[i];
            uiButtonImagesToFade[i].color = newColor;

            newColor = uiButtonTextsToFade[i].color;
            newColor.a = uiButtonOriginalAlphas[i];
            uiButtonTextsToFade[i].color = newColor;
        }
    }

    private void SetButtonAlphaToZero()
    {
        for (int i = 0; i < uiButtonImagesToFade.Length; i++)
        {
            Color newColor = uiButtonImagesToFade[i].color;
            newColor.a = 0;
            uiButtonImagesToFade[i].color = newColor;

            newColor = uiButtonTextsToFade[i].color;
            newColor.a = 0;
            uiButtonTextsToFade[i].color = newColor;
        }
    }
    #endregion

    private void ChangeUIState(EUIState newState)
    {
        switch (newState)
        {
            case EUIState.Disabled:
                buttonHolder.SetActive(false);
                pauseMenuHolder.SetActive(false);
                scoreDisplay.SetActive(false);
                skipIntroButtonHolder.SetActive(false);
                break;
            case EUIState.HUD:
                buttonHolder.SetActive(true);
                pauseMenuHolder.SetActive(false);
                scoreDisplay.SetActive(true);
                skipIntroButtonHolder.SetActive(false);
                break;
            case EUIState.PauseMenu:
                buttonHolder.SetActive(false);
                pauseMenuHolder.SetActive(true);
                scoreDisplay.SetActive(false);
                skipIntroButtonHolder.SetActive(false);
                break;
            case EUIState.Intro:
                buttonHolder.SetActive(false);
                pauseMenuHolder.SetActive(false);
                scoreDisplay.SetActive(false);
                skipIntroButtonHolder.SetActive(true);
                break;
            default:
                break;
        }
    }

}
