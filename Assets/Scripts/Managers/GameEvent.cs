using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent
{
    public virtual bool UseInUnityAnalytics { get { return false; } }

    public virtual Dictionary<string, object> GetData() { return null; }
}