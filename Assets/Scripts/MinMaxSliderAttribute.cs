using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{
    public readonly float Max;
    public readonly float Min;
    public readonly float Step;

    public MinMaxSliderAttribute(float min, float max, float step = 0.01f)
    {
        this.Min = min;
        this.Max = max;
        this.Step = step;
    }
}
