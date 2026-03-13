using System;

namespace CheatMenu;

/// <summary>
/// Attribute to mark methods that work with flags/toggles.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CheatFlag : Attribute { }
