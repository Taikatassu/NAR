using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    public delegate void EmptyVoid();
    public delegate void Vector2Void(Vector2 vec2);
    public delegate Vector2 EmptyVector2();
    public delegate void BoolVoid(bool boolean);

    public static event Vector2Void OnPlayerMovement;
    public static void BroadcastPlayerMovement(Vector2 playerMovement)
    {
        if (OnPlayerMovement != null)
        {
            OnPlayerMovement(playerMovement);
        }
    }

    public static event EmptyVector2 OnRequestGridOffset;
    public static Vector2 BroadcastRequestGridOffset()
    {
        if (OnRequestGridOffset != null)
        {
            return OnRequestGridOffset();
        }

        Debug.LogWarning("No subscriptions on 'OnRequestGridOffset' event!");
        return Vector2.zero;
    }

    public static event EmptyVoid OnLevelIntroFinished;
    public static void BroadcastLevelIntroFinished()
    {
        if (OnLevelIntroFinished != null)
        {
            OnLevelIntroFinished();
        }
    }

    public static event EmptyVoid OnLevelIntroStart;
    public static void BroadcastLevelIntroStart()
    {
        if (OnLevelIntroStart != null)
        {
            OnLevelIntroStart();
        }
    }

    public static event EmptyVoid OnLevelRestart;
    public static void BroadcastLevelRestart()
    {
        if (OnLevelRestart != null)
        {
            OnLevelRestart();
        }
    }

    public static event BoolVoid OnPauseStateChanged;
    public static void BroadcastPauseStateChanged(bool newState)
    {
        if (OnPauseStateChanged != null)
        {
            OnPauseStateChanged(newState);
        }
    }
}
