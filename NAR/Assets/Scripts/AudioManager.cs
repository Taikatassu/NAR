using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource[] backgroundMusicSources;
    int backgroundMusicIndex = 0;

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

    private void PlayRandomBackgroundMusic()
    {
        for (int i = 0; i < backgroundMusicSources.Length; i++)
        {
            backgroundMusicSources[i].Stop();
        }

        backgroundMusicIndex = Random.Range(0, backgroundMusicSources.Length);
        backgroundMusicSources[backgroundMusicIndex].Play();
    }

    private void OnPauseStateChanged(bool newState)
    {
        if (newState)
        {
            backgroundMusicSources[backgroundMusicIndex].Pause();
        }
        else
        {
            backgroundMusicSources[backgroundMusicIndex].UnPause();
        }
    }

    private void OnLevelIntroStart()
    {
        PlayRandomBackgroundMusic();
    }
}
