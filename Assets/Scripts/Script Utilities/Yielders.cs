using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Object Pool for WaitForSeconds
/// </summary>
public static class Yielders
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    static Dictionary<float, WaitForSeconds> timeInterval = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());
    //static Dictionary<float, WaitForSecondsRealtime> realtimeInterval = new Dictionary<float, WaitForSecondsRealtime>(100, new FloatComparer());
    static WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    static WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

    public static WaitForEndOfFrame EndOfFrame
    {
        get { return endOfFrame; }
    }

    public static WaitForFixedUpdate FixedUpdate
    {
        get { return fixedUpdate; }
    }

    public static WaitForSeconds Get(float seconds)
    {
        WaitForSeconds wfs;
        if (!timeInterval.TryGetValue(seconds, out wfs)) timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }

    //public static WaitForSecondsRealtime GetRealtime(float seconds)
    //{
    //    WaitForSecondsRealtime wfs;
    //    if (!realtimeInterval.TryGetValue(seconds, out wfs)) realtimeInterval.Add(seconds, wfs = new WaitForSecondsRealtime(seconds));
    //    return wfs;
    //}
}