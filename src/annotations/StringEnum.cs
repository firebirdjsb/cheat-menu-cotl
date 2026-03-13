using System;

namespace CheatMenu;

/// <summary>
/// Attribute for marking enum values with string representations.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class StringEnum : Attribute
{
    public string Value { get; }

    public StringEnum(string value)
    {
        Value = value;
    }
}
