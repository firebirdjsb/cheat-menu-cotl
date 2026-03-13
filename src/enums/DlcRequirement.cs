namespace CheatMenu;

/// <summary>
/// DLC requirements for cheat methods.
/// </summary>
public enum DlcRequirement
{
    /// <summary>No DLC required.</summary>
    None = 0,

    /// <summary>Requires the Spooky or血肉 major DLC.</summary>
    MajorDLC = 1,

    /// <summary>Requires the Sinful DLC.</summary>
    SinfulDLC = 2,

    /// <summary>Requires the Cultist DLC.</summary>
    CultistDLC = 3,

    /// <summary>Requires the Heretic DLC.</summary>
    HereticDLC = 4,

    /// <summary>Requires the Pilgrim DLC.</summary>
    PilgrimDLC = 5
}
