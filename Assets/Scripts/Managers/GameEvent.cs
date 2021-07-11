using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event Base Blueprint
/// </summary>
public abstract class GameEvent
{
    public virtual bool useInUnityAnalytics { get { return false; } }

    public virtual Dictionary<string, object> GetData() { return null; }
}