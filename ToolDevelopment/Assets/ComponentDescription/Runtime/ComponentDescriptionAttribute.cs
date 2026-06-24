using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ComponentDescriptionAttribute : Attribute
{
    public string Description { get; }

    public ComponentDescriptionAttribute(string description)
    {
        Description = description;
    }
}
