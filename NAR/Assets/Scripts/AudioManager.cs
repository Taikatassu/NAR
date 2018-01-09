using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource backgroundMusicSource;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnLevelIntroStart -= OnLevelIntroStart;
        EventManager.OnPauseStateChange -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool newState)
    {
        if (newState)
        {
            backgroundMusicSource.Pause();
        }
        else
        {
            backgroundMusicSource.UnPause();
        }
    }

    private void OnLevelIntroStart()
    {
        backgroundMusicSource.Play();
    }
}
