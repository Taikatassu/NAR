using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventController : MonoBehaviour
{
    public delegate void ColliderVoid(Collider col);

    public event ColliderVoid OnTriggerEnterEvent;
    public void BroadcastOnTriggerEnterEvent(Collider col)
    {
        if(OnTriggerEnterEvent != null)
        {
            OnTriggerEnterEvent(col);
        }
    }

    public event ColliderVoid OnTriggerExitEvent;
    public void BroadcastOnTriggerExitEvent(Collider col)
    {
        if (OnTriggerExitEvent != null)
        {
            OnTriggerExitEvent(col);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        BroadcastOnTriggerEnterEvent(col);
    }

    private void OnTriggerExit(Collider col)
    {
        BroadcastOnTriggerExitEvent(col);
    }
}
