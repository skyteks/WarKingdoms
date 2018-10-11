using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ReferenceHolder<T> : MonoBehaviour where T : Object
{
    [System.Serializable]
    public class ObjectEvent : UnityEvent<T> { }

    public T reference;
    public ObjectEvent sendEvent = new ObjectEvent();

    public void SendEvent()
    {
        if (sendEvent != null) sendEvent.Invoke(reference);
    }
}