using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This class adds some extension methods for Rect
/// </summary>
public static class Rect_Extension
{
    public static Rect[] SplitOnXAxis(this Rect rect, float percentageFirst)
    {
        percentageFirst = Mathf.Clamp01(percentageFirst);
        Rect part1 = new Rect(rect.x, rect.y, rect.width * percentageFirst, rect.height);
        Rect part2 = new Rect(rect.x + rect.width * percentageFirst, rect.y, rect.width * (1f - percentageFirst), rect.height);
        return new Rect[] { part1, part2 };
    }

    public static Rect[] SplitOnYAxis(this Rect rect, float percentageFirst)
    {
        percentageFirst = Mathf.Clamp01(percentageFirst);
        Rect part1 = new Rect(rect.x, rect.y, rect.width, rect.height * percentageFirst);
        Rect part2 = new Rect(rect.x, rect.y + rect.width * percentageFirst, rect.width, rect.height * (1f - percentageFirst));
        return new Rect[] { part1, part2 };
    }

    public static Rect[] SplitOnXAxis(this Rect rect, float percentageFirst, params float[] percentagesOthers)
    {
        Rect[] parts = new Rect[percentagesOthers.Length + 1];
        float totalValue = percentageFirst + percentagesOthers.Sum();
        float valueCounter = 0f;
        for (int i = 0; i < parts.Length; i++)
        {
            float value = i == 0 ? percentageFirst : percentagesOthers[i - 1];
            //parts[i] = new Rect(rect.x + rect.width * value, rect.y, rect.width * (value / totalValue), rect.height);
            Rect tmp = new Rect(rect);
            tmp.xMin = rect.xMin + rect.width * (valueCounter / totalValue);
            valueCounter += value;
            tmp.xMax = rect.xMin + rect.width * (valueCounter / totalValue);
            parts[i] = tmp;
        }
        return parts;
    }

    public static Rect ToScaled(this Rect rect, Vector2 scale)
    {
        float width = rect.width * scale.x;
        float height = rect.height * scale.y;
        return new Rect(rect.x - (width - rect.width) / 2f, rect.y - (height - rect.height) / 2f, width, height);
    }

    public static Vector2 GetTopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMax);
    }

    public static Vector2 GetTopRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMax);
    }

    public static Vector2 GetBottomLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Vector2 GetBottomRight(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMin);
    }

    // 2D bounds points
    public static Vector2 GetTopLeft2D(this Bounds bounds)
    {
        return new Vector2(bounds.min.x, bounds.max.z);
    }

    public static Vector2 GetTopRight2D(this Bounds bounds)
    {
        return new Vector2(bounds.max.x, bounds.max.z);
    }

    public static Vector2 GetBottomLeft2D(this Bounds bounds)
    {
        return new Vector2(bounds.min.x, bounds.min.z);
    }

    public static Vector2 GetBottomRight2D(this Bounds bounds)
    {
        return new Vector2(bounds.max.x, bounds.min.z);
    }

    // 3D bounds points
    public static Vector3 GetTopLeftFront(this Bounds bounds)
    {
        return new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
    }

    public static Vector3 GetTopLeftBack(this Bounds bounds)
    {
        return new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
    }

    public static Vector3 GetTopRightBack(this Bounds bounds)
    {
        return new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
    }

    public static Vector3 GetTopRightFront(this Bounds bounds)
    {
        return new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
    }

    public static Vector3 GetBottomLeftFront(this Bounds bounds)
    {
        return new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
    }

    public static Vector3 GetBottomLeftBack(this Bounds bounds)
    {
        return new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
    }

    public static Vector3 GetBottomRightBack(this Bounds bounds)
    {
        return new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
    }

    public static Vector3 GetBottomRightFront(this Bounds bounds)
    {
        return new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
    }

    public static bool Contains(this Bounds bounds, Bounds otherBounds)
    {
        return bounds.Contains(otherBounds.min) && bounds.Contains(otherBounds.max);
    }
}
