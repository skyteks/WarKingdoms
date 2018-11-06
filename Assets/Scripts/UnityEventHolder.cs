using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UnityEvent Holder for Unity callbacks
/// </summary>
public class UnityEventHolder : MonoBehaviour
{
    public UnityEvent OnAwake;
    public UnityEvent OnStart;
    public UnityEvent OnEnabled;
    public UnityEvent OnDisabled;
    public UnityEvent OnDestroyed;

    void Awake()
    {
        InvokeEvent(OnAwake);
    }

    void Start()
    {
        InvokeEvent(OnStart);

    }

    void OnEnable()
    {
        InvokeEvent(OnEnabled);

    }

    void OnDisable()
    {
        InvokeEvent(OnDisabled);

    }

    void OnDestroy()
    {
        InvokeEvent(OnDestroyed);

    }

    public static void InvokeEvent(UnityEvent e)
    {
        if (e != null)
        {
            e.Invoke();
        }
    }
}
