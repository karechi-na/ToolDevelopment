using UnityEngine;

public class IntButtonAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;

    public IntButtonAttribute(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}
