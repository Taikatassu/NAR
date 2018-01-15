using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource[] backgroundMusicSources;
    int backgroundMusicIndex = 0;

    [SerializeField]
    Text audioPopUpText;
    Vector3 audioPopUpRefVelocity = Vector3.zero;
    Vector3 audioPopUpTargetPosition;
    bool displayingAudioPopUp = false;
    float audioPopUpSmoothTime = 0.5f;
    float audioPopUpDuration = 5f;
    float audioPopUpStartTime = 0f;
    float audioPopUpFadeStrength = 3f;
    float audioPopUpTextOriginalAlpha = 0f;
    float audioPopUpShadowOriginalAlpha = 0f;

    private void OnEnable()
    {
        EventManager.OnLevelIntroStart += OnLevelIntroStart;
        EventManager.OnPauseStateChange += OnPauseStateChanged;

        SetAudioPopUpText("");
        audioPopUpTextOriginalAlpha = audioPopUpText.color.a;
        audioPopUpShadowOriginalAlpha = audioPopUpText.GetComponent<Shadow>().effectColor.a;
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

        StartAudioPopUp();
    }

    #region Subscribers
    private void OnPauseStateChanged(bool newState)
    {
        if (newState)
        {
            backgroundMusicSources[backgroundMusicIndex].Pause();
            StartAudioPopUp();
        }
        else
        {
            backgroundMusicSources[backgroundMusicIndex].UnPause();
            ResetAudioPopUp();
        }
    }

    private void OnLevelIntroStart()
    {
        PlayRandomBackgroundMusic();
    }
    #endregion

    #region Update loop
    private void Update()
    {
        if (displayingAudioPopUp)
        {
            PlayAudioPopUp();
        }
    }
    #endregion

    #region Audio track popup
    private void StartAudioPopUp()
    {
        ResetAudioPopUp();

        audioPopUpStartTime = Time.time;
        SetAudioPopUpText(backgroundMusicSources[backgroundMusicIndex].clip.name + "  ");
        ColorHelper.SetTextColor(audioPopUpText, EventManager.BroadcastRequestCurrentEnvironmentColor(), true);
        displayingAudioPopUp = true;
    }

    private void ResetAudioPopUp()
    {
        audioPopUpText.rectTransform.position = new Vector3(Screen.width / 2, Screen.height * -0.1f, 0);
        audioPopUpTargetPosition = new Vector3(Screen.width / 2, Screen.height * 0.05f, 0);
        SetAudioPopUpText("");
        displayingAudioPopUp = false;

        Color audioPopUpFadeColor;
        audioPopUpFadeColor = audioPopUpText.color;
        audioPopUpFadeColor.a = audioPopUpTextOriginalAlpha;
        audioPopUpText.color = audioPopUpFadeColor;

        audioPopUpFadeColor = audioPopUpText.GetComponent<Shadow>().effectColor;
        audioPopUpFadeColor.a = audioPopUpShadowOriginalAlpha;
        audioPopUpText.GetComponent<Shadow>().effectColor = audioPopUpFadeColor;
    }

    private void SetAudioPopUpText(string newPopUpText)
    {
        audioPopUpText.text = newPopUpText;
    }

    private void PlayAudioPopUp()
    {
        audioPopUpText.rectTransform.position = Vector3.SmoothDamp(audioPopUpText.rectTransform.position,
            audioPopUpTargetPosition, ref audioPopUpRefVelocity, audioPopUpSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        float timeSinceStartedPopUp = Time.time - audioPopUpStartTime;

        if (timeSinceStartedPopUp > audioPopUpDuration * 0.75f)
        {
            Color audioPopUpFadeColor;
            audioPopUpFadeColor = audioPopUpText.color;
            audioPopUpFadeColor.a -= audioPopUpFadeStrength * Time.unscaledDeltaTime;
            audioPopUpText.color = audioPopUpFadeColor;

            audioPopUpFadeColor = audioPopUpText.GetComponent<Shadow>().effectColor;
            audioPopUpFadeColor.a -= audioPopUpFadeStrength * Time.unscaledDeltaTime;
            audioPopUpText.GetComponent<Shadow>().effectColor = audioPopUpFadeColor;
        }

        if (timeSinceStartedPopUp > audioPopUpDuration)
        {
            ResetAudioPopUp();
        }
    }
    #endregion
}
