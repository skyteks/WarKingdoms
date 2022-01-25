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

    public static Vector3Int ToVector3XZ(this Vector2Int vector2, int y = 0)
    {
        return new Vector3Int(vector2.x, y, vector2.y);
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

    public static Vector3Int ToVector3Int(this Vector2Int vector2, int z = 0)
    {
        return new Vector3Int(vector2.y, vector2.x, z);
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

    public static Vector2 ToFloat(this Vector2Int vector2Int)
    {
        return new Vector2(vector2Int.x, vector2Int.y);
    }

    public static Vector3 ToFloat(this Vector3Int vector3Int)
    {
        return new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
    }

    public static Vector2Int ToRoundInt(this Vector2 vector2)
    {
        return new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
    }

    public static Vector3Int ToRoundInt(this Vector3 vector3)
    {
        return new Vector3Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y), Mathf.RoundToInt(vector3.z));
    }

    public static Vector2Int ToFloorInt(this Vector2 vector2)
    {
        return new Vector2Int(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y));
    }

    public static Vector3Int ToFloorInt(this Vector3 vector3)
    {
        return new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
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

    public static Color ToSumWithoutAlpha(this Color color, Vector3 addition)
    {
        color.r += addition.x;
        color.g += addition.y;
        color.b += addition.z;
        return color;
    }

    public static Color ToSumWithoutAlpha(this Color color, Color addition)
    {
        color.r += addition.r;
        color.g += addition.g;
        color.b += addition.b;
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

    public static int Angle(this Vector2Int vector, Vector2Int otherVector)
    {
        return Mathf.RoundToInt(Vector2.Angle(vector.ToFloat(), otherVector.ToFloat()));
    }

    public static int SignedAngle(this Vector2Int vector, Vector2Int otherVector)
    {
        return Mathf.RoundToInt(Vector2.SignedAngle(vector.ToFloat(), otherVector.ToFloat()));
    }

    public static bool InBounds(this Vector2 vector, Vector2 upper, Vector2 lower = default(Vector2))
    {
        return vector.x >= lower.x && vector.y >= lower.y && vector.x <= upper.x && vector.y <= upper.y;
    }

    public static bool InBounds(this Vector2Int vector, Vector2Int upper, Vector2Int lower = default(Vector2Int))
    {
        return vector.x >= lower.x && vector.y >= lower.y && vector.x <= upper.x && vector.y <= upper.y;
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

    #region BoundingSphere
    public static BoundingSphere GetBoundingBall(this IList<Vector3> points)
    {
        List<Vector3> boundaryPoints = new List<Vector3>(4);
        return BallWithBounds(points, boundaryPoints);
    }

    private static BoundingSphere BallWithBounds(IList<Vector3> contained, List<Vector3> boundaryPoints)
    {
        if (contained.Count == 0 || boundaryPoints.Count == 4)
        {
            switch (boundaryPoints.Count)
            {
                case 0:
                    return new BoundingSphere(Vector4.zero);
                case 1:
                    return new BoundingSphere(boundaryPoints[0], 0);
                case 2:
                    var halfSpan = 0.5f * (boundaryPoints[1] - boundaryPoints[0]);
                    return new BoundingSphere(
                             boundaryPoints[0] + halfSpan,
                             halfSpan.magnitude
                    );
                case 3:
                    return TriangleCircumSphere(
                             boundaryPoints[0],
                             boundaryPoints[1],
                             boundaryPoints[2]
                    );
                case 4:
                    return TetrahedronCircumSphere(
                             boundaryPoints[0],
                             boundaryPoints[1],
                             boundaryPoints[2],
                             boundaryPoints[3]
                    );
            }
        }

        int last = contained.Count - 1;
        int removeAt = Random.Range(0, contained.Count);

        Vector3 removed = contained[removeAt];
        contained[removeAt] = contained[last];
        contained.RemoveAt(last);

        var ball = BallWithBounds(contained, boundaryPoints);

        if (!ball.Contains(removed))
        {
            boundaryPoints.Add(removed);
            ball = BallWithBounds(contained, boundaryPoints);
            boundaryPoints.RemoveAt(boundaryPoints.Count - 1);
        }

        contained.Add(removed);
        return ball;
    }

    private static BoundingSphere TriangleCircumSphere(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 A = a - c;
        Vector3 B = b - c;

        Vector3 cross = Vector3.Cross(A, B);

        Vector3 center = c + Vector3.Cross(A.sqrMagnitude * B - B.sqrMagnitude * A, cross)
                       / (2f * cross.sqrMagnitude);

        float radius = Vector3.Distance(a, center);

        return new BoundingSphere(center, radius);
    }

    private static BoundingSphere TetrahedronCircumSphere(Vector3 p1,Vector3 p2,Vector3 p3,Vector3 p4)
    {
        // Construct a matrix with the vectors as columns 
        // (Xs on one row, Ys on the next... and the last row all 1s)
        Matrix4x4 matrix = new Matrix4x4(p1, p2, p3, p4);
        matrix.SetRow(3, Vector4.one);

        float a = matrix.determinant;

        // Copy the matrix so we can modify it 
        // and still read rows from the original.
        var D = matrix;
        Vector3 center;

        Vector4 squares = new Vector4(
                 p1.sqrMagnitude,
                 p2.sqrMagnitude,
                 p3.sqrMagnitude,
                 p4.sqrMagnitude
        );

        D.SetRow(0, squares);
        center.x = D.determinant;

        D.SetRow(1, matrix.GetRow(0));
        center.y = -D.determinant;

        D.SetRow(2, matrix.GetRow(1));
        center.z = D.determinant;

        center /= 2f * a;
        return new BoundingSphere(center, Vector3.Distance(p1, center));
    }

    public static bool Contains(this BoundingSphere sphere, Vector3 point)
    {
        return Vector3.Distance(sphere.position, point) <= sphere.radius;
    }

    public static bool Intersects(this BoundingSphere sphere1, BoundingSphere sphere2)
    {
        return Vector3.Distance(sphere1.position, sphere2.position) <= sphere1.radius + sphere2.radius;
    }
    #endregion
}
