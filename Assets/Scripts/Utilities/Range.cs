using System;
using UnityEngine;

[Serializable]
public struct Range : IEquatable<Range>
{
    public float min;
    public float max;

    public Range(float minValue, float maxValue)
    {
        min = minValue;
        max = maxValue;
    }

    public static implicit operator Range(float value)
    {
        return new Range() { max = value, min = value };
    }

    public float Lerp(float t)
    {
        return Mathf.Lerp(min, max, t);
    }

    public float InverseLerp(float value)
    {
        return Mathf.InverseLerp(min, max, value);
    }

    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }

    public override int GetHashCode()
    {
        var hashCode = -897720056;
        hashCode = hashCode * -1521134295 + min.GetHashCode();
        hashCode = hashCode * -1521134295 + max.GetHashCode();
        return hashCode;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Range))
        {
            return false;
        }
        var other = (Range)obj;
        return min == other.min && max == other.max; ;
    }

    bool IEquatable<Range>.Equals(Range other)
    {
        return Equals(this, other);
    }

    public static bool operator ==(Range lhs, Range rhs)
    {
        return Equals(lhs, rhs);
    }

    public static bool operator !=(Range lhs, Range rhs)
    {
        return Equals(lhs, rhs);
    }
}