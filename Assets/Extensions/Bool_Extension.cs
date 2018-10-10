using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bool_Extension
{
    public static float ToFloat(this bool boolean)
    {
        return boolean ? 1f : 0f;
    }

    public static float ToFloatLeadingSign(this bool boolean)
    {
        return boolean ? 1f : -1f;
    }

    public static int ToInt(this bool boolean)
    {
        return boolean ? 1 : 0;
    }

    public static int ToIntLeadingSign(this bool boolean)
    {
        return boolean ? 1 : -1;
    }
}
