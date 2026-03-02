using System;

namespace CheatMenu;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresDLC : Attribute {
    public DlcRequirement Requirement { get; }

    public RequiresDLC(DlcRequirement requirement){
        Requirement = requirement;
    }
}
