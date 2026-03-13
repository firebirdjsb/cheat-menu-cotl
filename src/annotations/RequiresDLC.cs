using System;

namespace CheatMenu;

/// <summary>
/// Attribute to mark cheat methods that require specific DLC content.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequiresDLC : Attribute
{
    public DlcRequirement Requirement { get; }

    public RequiresDLC(DlcRequirement requirement)
    {
        Requirement = requirement;
    }
}
