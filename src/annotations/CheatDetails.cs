using System;

namespace CheatMenu;

/// <summary>
/// Attribute for marking cheat methods with display metadata.
/// Applied to static methods in definition classes to expose them in the cheat menu.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CheatDetails : Attribute
{
    /// <summary>
    /// Creates a CheatDetails attribute for simple cheats or mode cheats with same state.
    /// </summary>
    /// <param name="title">Display title in the UI.</param>
    /// <param name="description">Tooltip description shown in the menu.</param>
    /// <param name="isFlagCheat">Whether this is a toggle/flag cheat.</param>
    /// <param name="sortOrder">UI ordering priority (lower shows first).</param>
    /// <param name="subGroup">Sub-group name for organizing related cheats.</param>
    public CheatDetails(string title, string description, bool isFlagCheat = false, int sortOrder = 0, string subGroup = null)
    {
        Title = title;
        Description = description;
        IsFlagCheat = isFlagCheat;
        SortOrder = sortOrder;
        SubGroup = subGroup;
    }

    /// <summary>
    /// Creates a CheatDetails attribute for mode cheats with different on/off titles.
    /// </summary>
    /// <param name="title">Base title for the cheat.</param>
    /// <param name="offTitle">Title shown when cheat is OFF.</param>
    /// <param name="onTitle">Title shown when cheat is ON.</param>
    /// <param name="description">Tooltip description.</param>
    /// <param name="isFlagCheat">Must be true for multi-name flags.</param>
    /// <param name="sortOrder">UI ordering priority.</param>
    /// <param name="subGroup">Sub-group for organization.</param>
    public CheatDetails(string title, string offTitle, string onTitle, string description, bool isFlagCheat = true, int sortOrder = 0, string subGroup = null)
    {
        if (!isFlagCheat)
        {
            throw new Exception("Multi name flag cheat can not have isFlagCheat set to false!");
        }

        Title = title;
        OffTitle = offTitle;
        OnTitle = onTitle;
        Description = description;
        IsFlagCheat = true;
        IsMultiNameFlagCheat = true;
        SortOrder = sortOrder;
        SubGroup = subGroup;
    }

    /// <summary>The display title of the cheat.</summary>
    public string Title { get; }

    /// <summary>The description shown in tooltip.</summary>
    public string Description { get; }

    /// <summary>Title shown when flag is ON.</summary>
    public string OnTitle { get; }

    /// <summary>Title shown when flag is OFF.</summary>
    public string OffTitle { get; }

    /// <summary>Whether this is a toggle/flag cheat.</summary>
    public bool IsFlagCheat { get; }

    /// <summary>Whether this cheat has different on/off titles.</summary>
    public bool IsMultiNameFlagCheat { get; }

    /// <summary>UI sort order (lower values appear first).</summary>
    public int SortOrder { get; }

    /// <summary>Sub-group for organizing cheats in UI.</summary>
    public string SubGroup { get; }
}
