using UnityEngine;

[System.Serializable]
public struct Range
{
    public float Min;
    public float Max;

    public Range(float min, float max)
    {
        this.Min = min;
        this.Max = max;
    }

    public static implicit operator Range(float value)
    {
        return new Range() { Max = value, Min = value };
    }

    public float Lerp(float t)
    {
        return Mathf.Lerp(this.Min, this.Max, t);
    }

    public float InverseLerp(float value)
    {
        return Mathf.InverseLerp(this.Min, this.Max, value);
    }
}