using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public delegate void EventDelegate<T>(T e) where T : GameEvent;

/// <summary>
/// Delegate Event handler
/// </summary>
public class EventManager : Singleton<EventManager>
{
    private delegate void EventDelegate(GameEvent e);

    private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
    private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();

    public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        if (delegateLookup.ContainsKey(del)) return;

        EventDelegate internalDelegate = (e) => del((T)e);
        delegateLookup[del] = internalDelegate;

        EventDelegate tempDel;
        if (delegates.TryGetValue(typeof(T), out tempDel))
        {
            delegates[typeof(T)] = tempDel += internalDelegate;
        }
        else
        {
            delegates[typeof(T)] = internalDelegate;
        }
    }

    public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        EventDelegate internalDelegate;
        if (delegateLookup.TryGetValue(del, out internalDelegate))
        {
            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
            {
                tempDel -= internalDelegate;
                if (tempDel == null) delegates.Remove(typeof(T));
                else delegates[typeof(T)] = tempDel;
            }

            delegateLookup.Remove(del);
        }
    }

    public void Trigger(GameEvent e)
    {
        EventDelegate del;
        if (delegates.TryGetValue(e.GetType(), out del))
        {
            del.Invoke(e);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Print All GameEvent Types")]
    private void PrintAllGameEventTypes()
    {
        IEnumerable<Type> childs;
        IEnumerable<Type> grandChilds;
        childs = (from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.BaseType == typeof(GameEvent)
                        select t);
        grandChilds = (from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(typeof(GameEvent)) && !childs.Contains(t)
                        select t);
        string classes = "";
        foreach (var e in childs) classes += e + "\n";
        classes += "\n";
        foreach (var e in grandChilds) classes += e + "\n";
        Debug.Log("GameEvent child classes (" + (childs.Count() + grandChilds.Count()) + "):\n\n" + classes);
    }
#endif
}