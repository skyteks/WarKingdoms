using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ReferenceHolder<T> : MonoBehaviour where T : Object
{
    public T reference;
    public UnityAction<T> OnSend;

    public void SendEvent()
    {
        if (OnSend != null) OnSend.Invoke(reference);
    }
}