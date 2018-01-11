using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject buttonHolder;
    [SerializeField]
    GameObject pauseMenuHolder;
    [SerializeField]
    GameObject scoreDisplay;
    [SerializeField]
    GameObject skipIntroButtonHolder;

    public enum EUIState
    {
        Disabled,
        HUD,
        PauseMenu,
        Intro,
    }

    private void OnEnable()
    {
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnLevelIntroFinished += OnLevelIntroFinished;
    }

    private void OnDisable()
    {
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnLevelIntroFinished -= OnLevelIntroFinished;
    }

    private void OnPauseStateChanged(bool newState)
    {
        Debug.Log("OnPauseStateChanged");
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
        Debug.Log("OnLevelIntroStart");
        ChangeUIState(EUIState.Intro);
    }

    private void OnLevelIntroFinished()
    {
        Debug.Log("OnLevelIntroFinished");
        ChangeUIState(EUIState.HUD);
    }

    public void PauseButtonPressed(bool newState)
    {
        EventManager.BroadcastPauseStateChange(newState);
    }

    public void RestartButtonPressed()
    {
        Debug.Log("RestartButtonPressed");
        EventManager.BroadcastPauseStateChange(false);
        EventManager.BroadcastLevelRestart();
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    private void ChangeUIState(EUIState newState)
    {
        Debug.Log("ChangeUIState");
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
