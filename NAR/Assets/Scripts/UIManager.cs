using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject buttonHolder;
    [SerializeField]
    GameObject pauseMenuHolder;

    public enum EUIState
    {
        Disabled,
        HUD,
        PauseMenu,
    }

    private void OnEnable()
    {
        EventManager.OnPauseStateChange += OnPauseStateChanged;
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
    }

    private void OnDisable()
    {
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
    }

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
        ChangeUIState(EUIState.HUD);
    }

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

    private void ChangeUIState(EUIState newState)
    {
        switch (newState)
        {
            case EUIState.Disabled:
                buttonHolder.SetActive(false);
                pauseMenuHolder.SetActive(false);
                break;
            case EUIState.HUD:
                buttonHolder.SetActive(true);
                pauseMenuHolder.SetActive(false);
                break;
            case EUIState.PauseMenu:
                buttonHolder.SetActive(false);
                pauseMenuHolder.SetActive(true);
                break;
            default:
                break;
        }
    }

}
