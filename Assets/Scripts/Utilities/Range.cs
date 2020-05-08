using UnityEngine;

[System.Serializable]
public struct Range
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
}