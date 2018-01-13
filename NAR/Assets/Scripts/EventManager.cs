using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    public delegate void EmptyVoid();
    public delegate void Vector3Void(Vector3 vec2);
    public delegate Vector2 EmptyVector2();
    public delegate void BoolVoid(bool boolean);
    public delegate void ColorVoid(Color color);
    public delegate Color EmptyColor();
    public delegate void IntVoid(int integer);
    public delegate void IntIntVoid(int integer1, int integer2);
    public delegate void GameObjectVoid(GameObject gameObject);
    public delegate int EmptyInt();

    public static event Vector3Void OnPlayerMovement;
    public static void BroadcastPlayerMovement(Vector3 playerMovement)
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

    public static event BoolVoid OnPauseStateChange;
    public static void BroadcastPauseStateChange(bool newState)
    {
        if (OnPauseStateChange != null)
        {
            OnPauseStateChange(newState);
        }
    }

    public static event ColorVoid OnEnvironmentColorChange;
    public static void BroadcastEnvironmentColorChange(Color newColor)
    {
        if (OnEnvironmentColorChange != null)
        {
            OnEnvironmentColorChange(newColor);
        }
    }

    public static event EmptyColor OnRequestCurrentEnvironmentColor;
    public static Color BroadcastRequestCurrentEnvironmentColor()
    {
        if (OnRequestCurrentEnvironmentColor != null)
        {
            return OnRequestCurrentEnvironmentColor();
        }

        Debug.LogWarning("No subscriptions on 'OnRequestCurrentEnvironmentColor' event!");
        return new Color();
    }

    public static event IntVoid OnCollectibleCollected;
    public static void BroadcastCollectibleCollected(int collectibleType)
    {
        if (OnCollectibleCollected != null)
        {
            OnCollectibleCollected(collectibleType);
        }
    }

    public static event GameObjectVoid OnObstacleHit;
    public static void BroadcastObstacleHit(GameObject hitObstacle)
    {
        if (OnObstacleHit != null)
        {
            OnObstacleHit(hitObstacle);
        }
    }

    public static event IntIntVoid OnScoreMultiplierChange;
    public static void BroadcastScoreMultiplierChange(int newMultiplier, int multiplierTier)
    {
        if (OnScoreMultiplierChange != null)
        {
            OnScoreMultiplierChange(newMultiplier, multiplierTier);
        }
    }

    public static event EmptyInt OnRequestCurrentScoreMultiplier;
    public static int BroadcastRequestCurrentScoreMultiplier()
    {
        if (OnRequestCurrentScoreMultiplier != null)
        {
            return OnRequestCurrentScoreMultiplier();
        }

        Debug.LogWarning("No subscriptions on 'OnRequestCurrentScoreMultiplier' event!");
        return 1;
    }
}
