using System;

namespace CheatMenu;

/// <summary>
/// Attribute for marking definition classes with a category.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CheatCategory : Attribute
{
    public CheatCategory(CheatCategoryEnum category)
    {
        Category = category;
    }

    public CheatCategoryEnum Category { get; }
}
