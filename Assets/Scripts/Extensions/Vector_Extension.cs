using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for Vector2, Vector3, Vector4 and Color
/// </summary>
public static class Vector_Extension
{
    public static Vector2 ToWithX(this Vector2 vector, float newX)
    {
        vector.x = newX;
        return vector;
    }

    public static Vector2 ToWithY(this Vector2 vector, float newY)
    {
        vector.y = newY;
        return vector;
    }

    public static Vector3 ToWithX(this Vector3 vector, float newX)
    {
        vector.x = newX;
        return vector;
    }

    public static Vector3 ToWithY(this Vector3 vector, float newY)
    {
        vector.y = newY;
        return vector;
    }

    public static Vector3 ToWithZ(this Vector3 vector, float newZ)
    {
        vector.z = newZ;
        return vector;
    }

    public static Vector4 ToWithX(this Vector4 vector, float newX)
    {
        vector.x = newX;
        return vector;
    }

    public static Vector4 ToWithY(this Vector4 vector, float newY)
    {
        vector.y = newY;
        return vector;
    }

    public static Vector4 ToWithZ(this Vector4 vector, float newZ)
    {
        vector.z = newZ;
        return vector;
    }

    public static Vector4 ToWithW(this Vector4 vector, float newW)
    {
        vector.w = newW;
        return vector;
    }

    public static Color ToWithR(this Color color, float newR)
    {
        color.r = newR;
        return color;
    }

    public static Color ToWithG(this Color color, float newG)
    {
        color.g = newG;
        return color;
    }

    public static Color ToWithB(this Color color, float newB)
    {
        color.b = newB;
        return color;
    }

    public static Color ToWithA(this Color color, float newA)
    {
        color.a = newA;
        return color;
    }

    public static Vector3 ToVector3(this Color32 color32)
    {
        return new Vector3(color32.r, color32.g, color32.b);
    }

    public static Vector3Int ToVector3Int(this Color32 color32)
    {
        return new Vector3Int(color32.r, color32.g, color32.b);
    }

    public static Vector4 ToVector4(this Color color)
    {
        return new Vector4(color.r, color.g, color.b, color.a);
    }

    public static Color ToColor(this Vector3 vector3, float a = 1f)
    {
        return new Color(vector3.x, vector3.y, vector3.z, a);
    }

    public static Color ToColor(this Vector4 vector4)
    {
        return new Color(vector4.x, vector4.y, vector4.z, vector4.w);
    }

    public static Vector3 ToVector3XZ(this Vector2 vector2, float y = 0f)
    {
        return new Vector3(vector2.x, y, vector2.y);
    }

    public static Vector3 ToVector3ZY(this Vector2 vector2, float x = 0f)
    {
        return new Vector3(x, vector2.y, vector2.x);
    }

    public static Vector2 ToVector2XZ(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    public static Vector3 ToVector3XZY(this Vector3 vector3)
    {
        return new Vector3(vector3.x, vector3.z, vector3.y);
    }

    public static Vector3 ToVector3(this Vector2 vector2, float z = 0f)
    {
        return new Vector3(vector2.x, vector2.y, z);
    }

    public static Vector4 ToVector4(this Vector2 vector2, float z = 0f, float w = 0f)
    {
        return new Vector4(vector2.x, vector2.y, z, w);
    }

    public static Vector4 ToVector4(this Vector2 vector2, Vector2 secondVector2)
    {
        return new Vector4(vector2.x, vector2.y, secondVector2.x, secondVector2.y);
    }

    public static Vector4 ToVector4(this Vector3 vector3, float w = 0f)
    {
        return new Vector4(vector3.x, vector3.y, vector3.z, w);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="axisDirection">Unit vector in direction of an axis (eg, defines a line that passes through zero)</param>
    /// <param name="point">Point to find nearest on line for</param>
    /// <returns></returns>
    public static Vector3 NearestPointOnAxis(this Vector3 axisDirection, Vector3 point)
    {
        axisDirection.Normalize();
        var dot = Vector3.Dot(point, axisDirection);
        return axisDirection * dot;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lineDirection">Unit vector in direction of line</param>
    /// <param name="point">Point to find nearest on line for</param>
    /// <param name="pointOnLine">A point on the line (allowing us to define an actual line in space)</param>
    /// <returns></returns>
    public static Vector3 NearestPointOnLine(this Vector3 lineDirection, Vector3 point, Vector3 pointOnLine)
    {
        lineDirection.Normalize();
        var dot = Vector3.Dot(point - pointOnLine, lineDirection);
        return pointOnLine + (lineDirection * dot);
    }

    public static Vector2 ToScale(this Vector2 vector, Vector2 scale)
    {
        vector.x *= scale.x;
        vector.y *= scale.y;
        return vector;
    }

    public static Vector3 ToScale(this Vector3 vector, Vector3 scale)
    {
        vector.x *= scale.x;
        vector.y *= scale.y;
        vector.z *= scale.z;
        return vector;
    }

    public static Vector4 ToScale(this Vector4 vector, Vector4 scale)
    {
        vector.x *= scale.x;
        vector.y *= scale.y;
        vector.z *= scale.z;
        vector.w *= scale.w;
        return vector;
    }

    public static Color ToScaleWithoutAlpha(this Color color, float scale)
    {
        color.r *= scale;
        color.g *= scale;
        color.b *= scale;
        return color;
    }

    public static bool IsNaN(this Vector2 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y);
    }

    public static bool IsNaN(this Vector3 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.x);
    }

    public static bool IsNaN(this Vector4 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z) || float.IsNaN(vector.w);
    }

    public static Vector2 Abs(this Vector2 vector)
    {
        return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static Vector4 Abs(this Vector4 vector)
    {
        return new Vector4(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z), Mathf.Abs(vector.w));
    }

    /// <summary>
    /// Find closest point in collection
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Vector2 FindCentroid(this IList<Vector2> points)
    {
        if (points.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        if (points.Count == 1)
        {
            return points[0];
        }

        Vector2 minPoint = points[0];
        Vector2 maxPoint = points[0];

        foreach (Vector3 point in points)
        {
            if (point.x < minPoint.x)
            {
                minPoint.x = point.x;
            }

            if (point.x > maxPoint.x)
            {
                maxPoint.x = point.x;
            }

            if (point.y < minPoint.y)
            {
                minPoint.y = point.y;
            }

            if (point.y > maxPoint.y)
            {
                maxPoint.y = point.y;
            }
        }
        Vector2 centroid = minPoint + (maxPoint - minPoint) * 0.5f;
        return centroid;
    }

    /// <summary>
    /// Find the center point in collection
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Vector3 FindCentroid(this IList<Vector3> points)
    {
        if (points.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        if (points.Count == 1)
        {
            return points[0];
        }

        Vector3 minPoint = points[0];
        Vector3 maxPoint = points[0];

        foreach (Vector3 point in points)
        {
            if (point.x < minPoint.x)
            {
                minPoint.x = point.x;
            }

            if (point.x > maxPoint.x)
            {
                maxPoint.x = point.x;
            }

            if (point.y < minPoint.y)
            {
                minPoint.y = point.y;
            }

            if (point.y > maxPoint.y)
            {
                maxPoint.y = point.y;
            }

            if (point.z < minPoint.z)
            {
                minPoint.z = point.z;
            }

            if (point.z > maxPoint.z)
            {
                maxPoint.z = point.z;
            }
        }
        Vector3 centroid = minPoint + (maxPoint - minPoint) * 0.5f;
        return centroid;
    }

    public static int FindClosestIndexToPoint(this IList<Vector2> points, Vector2 otherPoint)
    {
        if (points.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        int closestIndex = -1;
        float closestDistance = float.PositiveInfinity;
        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector2.Distance(points[i], otherPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    public static int FindClosestIndexToPoint(this IList<Vector3> points, Vector3 otherPoint)
    {
        if (points.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        int closestIndex = -1;
        float closestDistance = float.PositiveInfinity;
        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(points[i], otherPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}
