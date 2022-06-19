using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enum_Extension
{
    public static bool HasFlag(this byte myEnum, byte flag)
    {
        return (myEnum & flag) == flag;
    }

    public static bool HasFlag(this int myEnum, int flag)
    {
        return (myEnum & flag) == flag;
    }
}
