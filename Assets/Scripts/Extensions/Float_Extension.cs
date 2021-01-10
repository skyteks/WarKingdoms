using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class adds some extension methods for float
/// </summary>
public static class Float_Extension
{
    /// <summary>
    /// Remaps a value from one scale to another
    /// </summary>
    /// <param name="value">The value you want to scale</param>
    /// <param name="inMin">Current scale minimum</param>
    /// <param name="inMax">Current scale maximum</param>
    /// <param name="outMin">Desired scale minimum</param>
    /// <param name="outMax">Desired scale maximum</param>
    /// <returns>The remaped value</returns>
    public static float LinearRemap(this float value, float inMin, float inMax, float outMin = 0f, float outMax = 1f)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    /// <summary>
    /// Returns the sine of angle in degrees
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    public static float Sin(this float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float tmp = Mathf.Sin(rad);
        return tmp;
    }

    /// <summary>
    /// Returns the cosine of angle in degrees
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    public static float Cos(this float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float tmp = Mathf.Cos(rad);
        return tmp;
    }

    /// <summary>
    /// Returns the tangent of angle in degrees
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    public static float Tan(this float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float tmp = Mathf.Tan(rad);
        return tmp;
    }

    /// <summary>
    /// Returns the arc-tangent of an angle - the angle in degrees whose tangent is tan
    /// </summary>
    /// <param name="tan"></param>
    /// <returns></returns>
    public static float ATan(this float tan)
    {
        float tmp = Mathf.Atan(tan);
        float deg = tmp * Mathf.Rad2Deg;
        return deg;
    }

    /// <summary>
    /// Returns the angle in degrees whos Tan is x/y
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static float ATan2(this float y, float x)
    {
        float tmp = Mathf.Atan2(y, x);
        float deg = tmp * Mathf.Rad2Deg;
        return deg;
    }

    public static float Sign(this float value)
    {
        return (value == 0f) ? 0f : Mathf.Sign(value);
    }

    public static bool IsInRange(this float value, Range range)
    {
        return value >= range.min && value <= range.max;
    }

    public static bool IsInRange(this float value, float center, float offset)
    {
        return value >= center - offset && value <= center + offset;
    }

    public static bool IsInRange(this float value, float center, float negativeOffset, float positiveOffset)
    {
        return value >= center - negativeOffset && value <= center + positiveOffset;
    }

    public static float SignOr0(this float value)
    {
        if (value == 0f) return 0f;
        return Sign(value);
    }
}
