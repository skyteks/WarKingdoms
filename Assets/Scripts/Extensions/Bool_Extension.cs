using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for bool
/// </summary>
public static class Bool_Extension
{
    public static float ToFloat(this bool boolean)
    {
        return boolean ? 1f : 0f;
    }

    public static float ToSignFloat(this bool boolean)
    {
        return boolean ? 1f : -1f;
    }

    public static int ToInt(this bool boolean)
    {
        return boolean ? 1 : 0;
    }

    public static int ToSignInt(this bool boolean)
    {
        return boolean ? 1 : -1;
    }
}
