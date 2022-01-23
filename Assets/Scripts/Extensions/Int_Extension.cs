using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Int_Extension
{
    public static int Sign(this int value)
    {
        return value > 0 ? 1 : -1;
    }

    public static int SignOr0(this int value)
    {
        return (value == 0) ? 0 : Sign(value);
    }
}