using System;

namespace CheatMenu;

/// <summary>
/// Attribute to mark cheat methods as Work In Progress.
/// WIP cheats are hidden in release builds.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CheatWIP : Attribute { }
