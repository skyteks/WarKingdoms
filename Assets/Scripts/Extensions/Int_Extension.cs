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

    public static Vector2Int ToGridPosition(this int index, int width)
    {
        int x = index % width;
        int y = index / width;
        return new Vector2Int(x, y);
    }

    public static int ToIndex(this Vector2Int position, int width)
    {
        return position.x + position.y * width;
    }
}