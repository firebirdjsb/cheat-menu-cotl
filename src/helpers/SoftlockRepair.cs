using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Represents a detected softlock issue in the save data.
/// </summary>
public class SoftlockIssue
{
    /// <summary>
    /// Type of issue (e.g., "StoryProgression", "Bishop", "AreaUnlock", "Crusade", "DLC")
    /// </summary>
    public string IssueType { get; set; }

    /// <summary>
    /// Human-readable description of the issue.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Name of the flag or field involved in the issue.
    /// </summary>
    public string FlagName { get; set; }

    /// <summary>
    /// Current value that is causing the issue.
    /// </summary>
    public object CurrentValue { get; set; }

    /// <summary>
    /// Expected value to fix the issue.
    /// </summary>
    public object ExpectedValue { get; set; }

    /// <summary>
    /// Whether this issue can be automatically repaired.
    /// </summary>
    public bool CanRepair { get; set; }

    /// <summary>
    /// Description of the repair action that will be performed.
    /// </summary>
    public string RepairAction { get; set; }
}

/// <summary>
/// Report containing all detected issues and repairs performed.
/// </summary>
public class RepairReport
{
    /// <summary>
    /// Whether any repairs were performed.
    /// </summary>
    public bool WasRepaired { get; set; }

    /// <summary>
    /// List of all detected issues.
    /// </summary>
    public List<SoftlockIssue> DetectedIssues { get; set; } = new List<SoftlockIssue>();

    /// <summary>
    /// List of repairs that were performed.
    /// </summary>
    public List<string> RepairsPerformed { get; set; } = new List<string>();

    /// <summary>
    /// List of issues that could not be repaired.
    /// </summary>
    public List<string> UnrepairableIssues { get; set; } = new List<string>();
}

/// <summary>
/// System for detecting and repairing progression softlocks in save data.
/// Scans for broken progression flags and repairs them on demand or automatically.
/// </summary>
public class SoftlockRepair
{
    #region Main Entry Points

    /// <summary>
    /// Scans the save data for softlock issues and repairs them.
    /// </summary>
    /// <param name="saveData">GameState to scan and repair (typically DataManager.Instance)</param>
    /// <returns>RepairReport with detected issues and repairs performed</returns>
    public RepairReport ScanAndRepair(object saveData)
    {
        var report = new RepairReport();

        try
        {
            if (saveData == null)
            {
                Logger.Warning(Logger.FLAGS, "Save data is null, cannot scan for softlocks");
                return report;
            }

            var dm = saveData as DataManager;
            if (dm == null)
            {
                Logger.Warning(Logger.FLAGS, "Cannot cast save data to DataManager");
                return report;
            }

            Logger.Info(Logger.FLAGS, "Starting softlock scan...");

            // Run all detection categories
            var storyIssues = DetectStoryProgressionIssues(dm);
            var bishopIssues = DetectBishopIssues(dm);
            var areaIssues = DetectAreaUnlockIssues(dm);
            var crusadeIssues = DetectCrusadeUnlockIssues(dm);
            var dlcIssues = DetectDLCProgressionIssues(dm);

            // Combine all issues
            report.DetectedIssues.AddRange(storyIssues);
            report.DetectedIssues.AddRange(bishopIssues);
            report.DetectedIssues.AddRange(areaIssues);
            report.DetectedIssues.AddRange(crusadeIssues);
            report.DetectedIssues.AddRange(dlcIssues);

            Logger.Info(Logger.FLAGS, $"Detected {report.DetectedIssues.Count} potential softlock issues");

            // Perform repairs for each category
            RepairStoryProgression(dm, storyIssues);
            RepairBishopIssues(dm, bishopIssues);
            RepairAreaUnlocks(dm, areaIssues);
            RepairCrusadeUnlocks(dm, crusadeIssues);
            RepairDLCProgression(dm, dlcIssues);

            // Compile repair report
            var repairable = report.DetectedIssues.Where(i => i.CanRepair).ToList();
            var unrepairable = report.DetectedIssues.Where(i => !i.CanRepair).ToList();

            report.WasRepaired = repairable.Any();
            report.RepairsPerformed = repairable.Select(i => i.RepairAction).Where(a => !string.IsNullOrEmpty(a)).ToList();
            report.UnrepairableIssues = unrepairable.Select(i => $"{i.IssueType}: {i.Description}").ToList();

            if (report.WasRepaired)
            {
                Logger.Info(Logger.FLAGS, $"Softlock repair complete - {report.RepairsPerformed.Count} repairs performed");
            }
            else
            {
                Logger.Info(Logger.FLAGS, "No softlocks detected or repairable issues found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Softlock scan error: {ex.Message}");
        }

        return report;
    }

    #endregion

    #region Detection Methods

    /// <summary>
    /// Detects issues with main story progression order.
    /// </summary>
    public List<SoftlockIssue> DetectStoryProgressionIssues(DataManager dm)
    {
        var issues = new List<SoftlockIssue>();

        try
        {
            // Check current quest stage
            var stageField = dm.GetType().GetField("currentQuestStage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (stageField != null)
            {
                var stage = Convert.ToInt32(stageField.GetValue(dm));

                // Check for negative or unreasonably high stages
                if (stage < 0)
                {
                    issues.Add(new SoftlockIssue
                    {
                        IssueType = "StoryProgression",
                        Description = "Negative quest stage detected",
                        FlagName = "currentQuestStage",
                        CurrentValue = stage,
                        ExpectedValue = 0,
                        CanRepair = true,
                        RepairAction = $"Reset quest stage from {stage} to 0"
                    });
                }
                else if (stage > 50)
                {
                    // Very high stage might indicate corruption
                    issues.Add(new SoftlockIssue
                    {
                        IssueType = "StoryProgression",
                        Description = $"Unusually high quest stage: {stage}",
                        FlagName = "currentQuestStage",
                        CurrentValue = stage,
                        ExpectedValue = Math.Min(stage, 50),
                        CanRepair = true,
                        RepairAction = $"Clamped quest stage from {stage} to 50"
                    });
                }
            }

            // Check for intro completion flag
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        // Check if player has advanced story without completing intro
                        int introComplete = 0;
                        flags.TryGetValue("IntroComplete", out introComplete);
                        if (introComplete == 0)
                        {
                            // Check if player has recruited followers without intro complete (possible softlock)
                            if (dm.Followers != null && dm.Followers.Count > 0)
                            {
                                issues.Add(new SoftlockIssue
                                {
                                    IssueType = "StoryProgression",
                                    Description = "Followers exist but intro not marked complete",
                                    FlagName = "IntroComplete",
                                    CurrentValue = 0,
                                    ExpectedValue = 1,
                                    CanRepair = true,
                                    RepairAction = "Mark IntroComplete flag as set"
                                });
                            }
                        }

                        // Check for mid-game flags that shouldn't be set too early
                        CheckStoryFlagInconsistencies(flags, issues);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Story progression detection error: {ex.Message}");
        }

        return issues;
    }

    /// <summary>
    /// Checks for story flag inconsistencies.
    /// </summary>
    private void CheckStoryFlagInconsistencies(Dictionary<string, int> flags, List<SoftlockIssue> issues)
    {
        // Check for crusade unlocked without gate opened
        int crusadeUnlocked = 0;
        flags.TryGetValue("CrusadeUnlocked", out crusadeUnlocked);
        if (crusadeUnlocked == 1)
        {
            int gateOpened = 0;
            flags.TryGetValue("GateOpened", out gateOpened);
            if (gateOpened == 0)
            {
                issues.Add(new SoftlockIssue
                {
                    IssueType = "StoryProgression",
                    Description = "Crusade unlocked but gate not opened - possible softlock",
                    FlagName = "GateOpened",
                    CurrentValue = 0,
                    ExpectedValue = 1,
                    CanRepair = true,
                    RepairAction = "Set GateOpened flag to 1"
                });
            }
        }

        // Check for boss defeated without appropriate progression
        var bishopDefeatedFlags = new[] { "BishopSkullDefeated", "BishopHeartDefeated", "BishopShieldDefeated", "BishopSwordDefeated" };
        foreach (var flag in bishopDefeatedFlags)
        {
            int defeated = 0;
            if (flags.TryGetValue(flag, out defeated) && defeated == 1)
            {
                // This is valid progression, just log it
                Logger.Info(Logger.FLAGS, $"Detected bishop defeated: {flag}");
            }
        }
    }

    /// <summary>
    /// Detects issues with Bishop defeat flags.
    /// </summary>
    public List<SoftlockIssue> DetectBishopIssues(DataManager dm)
    {
        var issues = new List<SoftlockIssue>();

        try
        {
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        // Known bishop flags from the game
                        var bishopFlags = new Dictionary<string, string>
                        {
                            { "BishopSkullDefeated", "Skull" },
                            { "BishopHeartDefeated", "Heart" },
                            { "BishopShieldDefeated", "Shield" },
                            { "BishopSwordDefeated", "Sword" }
                        };

                        foreach (var kvp in bishopFlags)
                        {
                            int defeated = 0;
                            if (flags.TryGetValue(kvp.Key, out defeated) && defeated == 1)
                            {
                                // Check if the corresponding area progression is set
                                string areaFlag = $"Area{kvp.Value}Unlocked";
                                int areaUnlocked = 0;
                                flags.TryGetValue(areaFlag, out areaUnlocked);
                                if (areaUnlocked == 0)
                                {
                                    issues.Add(new SoftlockIssue
                                    {
                                        IssueType = "Bishop",
                                        Description = $"Bishop {kvp.Value} defeated but area not unlocked",
                                        FlagName = areaFlag,
                                        CurrentValue = 0,
                                        ExpectedValue = 1,
                                        CanRepair = true,
                                        RepairAction = $"Set {areaFlag} to 1"
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Bishop detection error: {ex.Message}");
        }

        return issues;
    }

    /// <summary>
    /// Detects issues with area unlock flags.
    /// </summary>
    public List<SoftlockIssue> DetectAreaUnlockIssues(DataManager dm)
    {
        var issues = new List<SoftlockIssue>();

        try
        {
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        // Check for areas that should be unlocked together
                        // An area unlocked without its prerequisite is a softlock
                        
                        // Check area progression chain
                        var areaProgression = new[] { "AreaSkull", "AreaHeart", "AreaShield", "AreaSword" };
                        for (int i = 0; i < areaProgression.Length; i++)
                        {
                            string areaName = areaProgression[i];
                            string unlockedFlag = $"{areaName}Unlocked";

                            int unlocked = 0;
                            flags.TryGetValue(unlockedFlag, out unlocked);
                            if (unlocked == 1)
                            {
                                // Check if previous area is also unlocked (progression order)
                                if (i > 0)
                                {
                                    string prevUnlocked = $"{areaProgression[i - 1]}Unlocked";
                                    int prevUnlockedValue = 0;
                                    flags.TryGetValue(prevUnlocked, out prevUnlockedValue);
                                    if (prevUnlockedValue == 0)
                                    {
                                        issues.Add(new SoftlockIssue
                                        {
                                            IssueType = "AreaUnlock",
                                            Description = $"{areaName} unlocked but {areaProgression[i - 1]} is not",
                                            FlagName = prevUnlocked,
                                            CurrentValue = 0,
                                            ExpectedValue = 1,
                                            CanRepair = true,
                                            RepairAction = $"Set {prevUnlocked} to 1"
                                        });
                                    }
                                }

                                // Check if structure for this area exists (optional - might not be a softlock)
                                Logger.Info(Logger.FLAGS, $"Area {areaName} is unlocked");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Area unlock detection error: {ex.Message}");
        }

        return issues;
    }

    /// <summary>
    /// Detects issues with crusade unlock flags.
    /// </summary>
    public List<SoftlockIssue> DetectCrusadeUnlockIssues(DataManager dm)
    {
        var issues = new List<SoftlockIssue>();

        try
        {
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        // Check crusade unlocked flag
                        int crusadeUnlocked = 0;
                        flags.TryGetValue("CrusadeUnlocked", out crusadeUnlocked);
                        if (crusadeUnlocked == 1)
                        {
                            // Gate should be opened for crusade to work
                            int gateOpened = 0;
                            flags.TryGetValue("GateOpened", out gateOpened);
                            if (gateOpened == 0)
                            {
                                issues.Add(new SoftlockIssue
                                {
                                    IssueType = "Crusade",
                                    Description = "Crusade unlocked but gate not opened",
                                    FlagName = "GateOpened",
                                    CurrentValue = 0,
                                    ExpectedValue = 1,
                                    CanRepair = true,
                                    RepairAction = "Set GateOpened to 1"
                                });
                            }

                            // Check if follower count is too low for crusade
                            int followerCount = dm.Followers?.Count ?? 0;
                            if (followerCount < 3)
                            {
                                issues.Add(new SoftlockIssue
                                {
                                    IssueType = "Crusade",
                                    Description = $"Crusade unlocked but only {followerCount} followers (need at least 3)",
                                    FlagName = "FollowerCount",
                                    CurrentValue = followerCount,
                                    ExpectedValue = "At least 3 followers",
                                    CanRepair = false, // Cannot auto-add followers
                                    RepairAction = ""
                                });
                            }
                        }

                        // Check if gate is opened but crusade not unlocked
                        int gateOpenedValue = 0;
                        flags.TryGetValue("GateOpened", out gateOpenedValue);
                        if (gateOpenedValue == 1)
                        {
                            int crusadeUnlockedValue = 0;
                            flags.TryGetValue("CrusadeUnlocked", out crusadeUnlockedValue);
                            if (crusadeUnlockedValue == 0)
                            {
                                issues.Add(new SoftlockIssue
                                {
                                    IssueType = "Crusade",
                                    Description = "Gate opened but crusade not unlocked",
                                    FlagName = "CrusadeUnlocked",
                                    CurrentValue = 0,
                                    ExpectedValue = 1,
                                    CanRepair = true,
                                    RepairAction = "Set CrusadeUnlocked to 1"
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Crusade unlock detection error: {ex.Message}");
        }

        return issues;
    }

    /// <summary>
    /// Detects issues with DLC progression flags (especially Woolhaven).
    /// </summary>
    public List<SoftlockIssue> DetectDLCProgressionIssues(DataManager dm)
    {
        var issues = new List<SoftlockIssue>();

        try
        {
            bool hasMajorDLC = CultUtils.HasMajorDLC();

            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        // Check for Woolhaven (major DLC) specific flags
                        var woolhavenFlags = new[] 
                        { 
                            "WoolhavenGateOpen", 
                            "WoolhavenUnlocked",
                            "RanchUnlocked",
                            "FurnaceUnlocked",
                            "ForgeUnlocked"
                        };

                        foreach (var flag in woolhavenFlags)
                        {
                            int value = 0;
                            flags.TryGetValue(flag, out value);
                            if (value == 1 && !hasMajorDLC)
                            {
                                // DLC content exists but DLC not owned - softlock
                                issues.Add(new SoftlockIssue
                                {
                                    IssueType = "DLC",
                                    Description = $"DLC flag {flag} is set but player doesn't own the DLC",
                                    FlagName = flag,
                                    CurrentValue = 1,
                                    ExpectedValue = 0,
                                    CanRepair = false, // Cannot remove DLC content
                                    RepairAction = ""
                                });
                            }
                            else if (value == 0 && hasMajorDLC)
                            {
                                // Check if this is a progression issue
                                if (flag == "WoolhavenGateOpen" || flag == "WoolhavenUnlocked")
                                {
                                    // Check if player is at appropriate quest stage for Woolhaven
                                    var stageField = dm.GetType().GetField("currentQuestStage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (stageField != null)
                                    {
                                        var stage = Convert.ToInt32(stageField.GetValue(dm));
                                        if (stage >= 20) // Assuming late game
                                        {
                                            issues.Add(new SoftlockIssue
                                            {
                                                IssueType = "DLC",
                                                Description = $"Woolhaven gate not open at quest stage {stage}",
                                                FlagName = flag,
                                                CurrentValue = 0,
                                                ExpectedValue = 1,
                                                CanRepair = true,
                                                RepairAction = $"Set {flag} to 1"
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Check for DLC structures without DLC ownership using reflection
            if (!hasMajorDLC)
            {
                var structuresField = dm.GetType().GetField("Structures", BindingFlags.Instance | BindingFlags.Public);
                if (structuresField != null)
                {
                    var structures = structuresField.GetValue(dm) as List<object>;
                    if (structures != null)
                    {
                        var dlcStructureTypes = new[] { "Ranch", "Furnace", "Forge", "Tavern", "Brewery", "Flockade", "Heater" };
                        foreach (var structure in structures)
                        {
                            if (structure != null)
                            {
                                var structIdField = structure.GetType().GetField("_StructID", BindingFlags.Instance | BindingFlags.Public);
                                if (structIdField != null)
                                {
                                    var structId = structIdField.GetValue(structure) as string;
                                    if (!string.IsNullOrEmpty(structId))
                                    {
                                        foreach (var dlcType in dlcStructureTypes)
                                        {
                                            if (structId.IndexOf(dlcType, StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                issues.Add(new SoftlockIssue
                                                {
                                                    IssueType = "DLC",
                                                    Description = $"DLC structure {structId} exists without DLC",
                                                    FlagName = structId,
                                                    CurrentValue = "Exists",
                                                    ExpectedValue = "Should not exist",
                                                    CanRepair = false,
                                                    RepairAction = ""
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"DLC progression detection error: {ex.Message}");
        }

        return issues;
    }

    #endregion

    #region Repair Methods

    /// <summary>
    /// Repairs story progression issues.
    /// </summary>
    public void RepairStoryProgression(DataManager dm, List<SoftlockIssue> issues)
    {
        try
        {
            foreach (var issue in issues.Where(i => i.CanRepair && i.IssueType == "StoryProgression"))
            {
                switch (issue.FlagName)
                {
                    case "currentQuestStage":
                        var stageField = dm.GetType().GetField("currentQuestStage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (stageField != null)
                        {
                            int newStage = Convert.ToInt32(issue.ExpectedValue);
                            stageField.SetValue(dm, newStage);
                            Logger.Info(Logger.FLAGS, $"Repaired story progression: {issue.RepairAction}");
                        }
                        break;

                    case "IntroComplete":
                        SetEventFlag("IntroComplete", 1);
                        Logger.Info(Logger.FLAGS, $"Repaired Woolhaven gate progression: {issue.RepairAction}");
                        break;

                    case "GateOpened":
                        SetEventFlag("GateOpened", 1);
                        Logger.Info(Logger.FLAGS, $"Repaired story progression: {issue.RepairAction}");
                        break;

                    default:
                        // Try to set as event flag
                        if (issue.ExpectedValue is int intValue)
                        {
                            SetEventFlag(issue.FlagName, intValue);
                            Logger.Info(Logger.FLAGS, $"Repaired story progression: {issue.RepairAction}");
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Story progression repair error: {ex.Message}");
        }
    }

    /// <summary>
    /// Repairs bishop-related issues.
    /// </summary>
    public void RepairBishopIssues(DataManager dm, List<SoftlockIssue> issues)
    {
        try
        {
            foreach (var issue in issues.Where(i => i.CanRepair && i.IssueType == "Bishop"))
            {
                if (issue.ExpectedValue is int intValue)
                {
                    SetEventFlag(issue.FlagName, intValue);
                    Logger.Info(Logger.FLAGS, $"Repaired bishop issue: {issue.RepairAction}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Bishop repair error: {ex.Message}");
        }
    }

    /// <summary>
    /// Repairs area unlock issues.
    /// </summary>
    public void RepairAreaUnlocks(DataManager dm, List<SoftlockIssue> issues)
    {
        try
        {
            foreach (var issue in issues.Where(i => i.CanRepair && i.IssueType == "AreaUnlock"))
            {
                if (issue.ExpectedValue is int intValue)
                {
                    SetEventFlag(issue.FlagName, intValue);
                    Logger.Info(Logger.FLAGS, $"Repaired area unlock: {issue.RepairAction}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Area unlock repair error: {ex.Message}");
        }
    }

    /// <summary>
    /// Repairs crusade unlock issues.
    /// </summary>
    public void RepairCrusadeUnlocks(DataManager dm, List<SoftlockIssue> issues)
    {
        try
        {
            foreach (var issue in issues.Where(i => i.CanRepair && i.IssueType == "Crusade"))
            {
                switch (issue.FlagName)
                {
                    case "GateOpened":
                        SetEventFlag("GateOpened", 1);
                        Logger.Info(Logger.FLAGS, $"Repaired crusade unlock: {issue.RepairAction}");
                        break;

                    case "CrusadeUnlocked":
                        SetEventFlag("CrusadeUnlocked", 1);
                        Logger.Info(Logger.FLAGS, $"Repaired crusade unlock: {issue.RepairAction}");
                        break;

                    default:
                        if (issue.ExpectedValue is int intValue)
                        {
                            SetEventFlag(issue.FlagName, intValue);
                            Logger.Info(Logger.FLAGS, $"Repaired crusade unlock: {issue.RepairAction}");
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Crusade unlock repair error: {ex.Message}");
        }
    }

    /// <summary>
    /// Repairs DLC progression issues.
    /// </summary>
    public void RepairDLCProgression(DataManager dm, List<SoftlockIssue> issues)
    {
        try
        {
            foreach (var issue in issues.Where(i => i.CanRepair && i.IssueType == "DLC"))
            {
                if (issue.ExpectedValue is int intValue)
                {
                    SetEventFlag(issue.FlagName, intValue);
                    Logger.Info(Logger.FLAGS, $"Repaired Woolhaven gate progression: {issue.RepairAction}");
                }
            }

            // Log unrepairable issues
            var unrepairable = issues.Where(i => !i.CanRepair).ToList();
            foreach (var issue in unrepairable)
            {
                Logger.Warning(Logger.FLAGS, $"Cannot repair DLC issue: {issue.Description}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"DLC progression repair error: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets an event flag value in the game's flag system.
    /// </summary>
    private void SetEventFlag(string flagName, int value)
    {
        try
        {
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        flags[flagName] = value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Failed to set event flag {flagName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an event flag value from the game's flag system.
    /// </summary>
    private int GetEventFlag(string flagName, int defaultValue = 0)
    {
        try
        {
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null && flags.TryGetValue(flagName, out int value))
                    {
                        return value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.FLAGS, $"Failed to get event flag {flagName}: {ex.Message}");
        }

        return defaultValue;
    }

    #endregion
}
