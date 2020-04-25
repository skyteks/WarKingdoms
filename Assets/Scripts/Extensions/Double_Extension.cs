using System;

/// <summary>
/// This class adds some extension methods for double
/// </summary>
public static class Double_Extension
{
    public static float ToFloat(this double value)
    {
        if (float.IsPositiveInfinity(Convert.ToSingle(value)))
        {
            return float.MaxValue;
        }
        if (float.IsNegativeInfinity(Convert.ToSingle(value)))
        {
            return float.MinValue;
        }
        return Convert.ToSingle(value);
    }
}