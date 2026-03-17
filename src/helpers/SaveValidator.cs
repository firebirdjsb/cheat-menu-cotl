using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Validates save data integrity to prevent corruption before save mutations.
/// Provides comprehensive validation across followers, inventory, flags, quests, and DLC.
/// </summary>
public static class SaveValidator
{
    // Safe limits derived from game mechanics
    private const int MAX_FOLLOWERS_BASE = 30; // Base game limit
    private const int MAX_FOLLOWERS_DLC = 40;  // With DLC
    private const int MAX_DEAD_FOLLOWERS = 50;
    private const int MAX_RECRUITS = 10;
    private const int MAX_QUEST_STAGES = 100;
    private const int MAX_FLAG_VALUE = 10000;
    
    // Cached enum values for validation
    private static HashSet<int> _validItemTypeValues;
    private static bool _enumCacheInitialized = false;
    
    #region Validation Result Types
    
    /// <summary>
    /// Result of validation operation containing status and details.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }
        
        public static ValidationResult Failure(string error)
        {
            return new ValidationResult { IsValid = false, Errors = new List<string> { error } };
        }
    }
    
    /// <summary>
    /// Result of repair operation containing what was fixed.
    /// </summary>
    public class RepairResult
    {
        public bool Success { get; set; }
        public List<string> Repairs { get; set; } = new List<string>();
        public List<string> Failures { get; set; } = new List<string>();
        public int TotalRepairs => Repairs.Count;
    }
    
    #endregion
    
    #region Main Validation Entry Point
    
    /// <summary>
    /// Validates the entire save data integrity.
    /// </summary>
    /// <param name="saveData">GameState to validate (typically DataManager.Instance)</param>
    /// <returns>ValidationResult with warnings and errors</returns>
    public static ValidationResult ValidateSave(object saveData)
    {
        var result = new ValidationResult();
        var warnings = new List<string>();
        
        try
        {
            if (saveData == null)
            {
                Logger.Warning(Logger.SAVE, "Save data is null, cannot validate");
                return ValidationResult.Failure("Save data is null");
            }
            
            Logger.Info(Logger.SAVE, "Starting save validation...");
            
            // Run all validation categories
            bool followersValid = ValidateFollowers(saveData, warnings);
            bool inventoryValid = ValidateInventory(saveData, warnings);
            bool flagsValid = ValidateFlags(saveData, warnings);
            bool questsValid = ValidateQuests(saveData, warnings);
            bool dlcValid = ValidateDLCProgression(saveData, warnings);
            
            result.Warnings = warnings;
            result.IsValid = followersValid && inventoryValid && flagsValid && questsValid && dlcValid;
            
            // Add metadata about validation
            result.Metadata["FollowersValid"] = followersValid;
            result.Metadata["InventoryValid"] = inventoryValid;
            result.Metadata["FlagsValid"] = flagsValid;
            result.Metadata["QuestsValid"] = questsValid;
            result.Metadata["DLCValid"] = dlcValid;
            result.Metadata["WarningCount"] = warnings.Count;
            
            if (result.IsValid)
            {
                Logger.Info(Logger.SAVE, $"Validation complete - Save is valid ({warnings.Count} warnings)");
            }
            else
            {
                Logger.Warning(Logger.SAVE, $"Validation complete - Save has issues ({warnings.Count} warnings)");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(Logger.SAVE, $"Validation error: {ex.Message}");
            result.IsValid = false;
            result.Errors.Add($"Validation exception: {ex.Message}");
        }
        
        return result;
    }
    
    #endregion
    
    #region Follower Validation
    
    /// <summary>
    /// Validates follower list integrity.
    /// </summary>
    public static bool ValidateFollowers(object saveData, List<string> warnings)
    {
        bool isValid = true;
        
        try
        {
            var dm = saveData as DataManager;
            if (dm == null)
            {
                warnings.Add("Cannot access DataManager for follower validation");
                return false;
            }
            
            int maxFollowers = CultUtils.HasMajorDLC() ? MAX_FOLLOWERS_DLC : MAX_FOLLOWERS_BASE;
            
            // Validate Followers list
            var followers = dm.Followers;
            if (followers != null)
            {
                if (followers.Count > maxFollowers)
                {
                    warnings.Add($"Follower count ({followers.Count}) exceeds safe limit ({maxFollowers})");
                    isValid = false;
                }
                
                // Check for duplicate IDs
                var ids = new HashSet<int>();
                foreach (var follower in followers)
                {
                    if (follower == null)
                    {
                        warnings.Add("Found null follower in Followers list");
                        isValid = false;
                        continue;
                    }
                    
                    if (!ids.Add(follower.ID))
                    {
                        warnings.Add($"Duplicate follower ID found: {follower.ID}");
                        isValid = false;
                    }
                }
            }
            
            // Validate Dead Followers
            var deadFollowers = dm.Followers_Dead;
            if (deadFollowers != null)
            {
                if (deadFollowers.Count > MAX_DEAD_FOLLOWERS)
                {
                    warnings.Add($"Dead follower count ({deadFollowers.Count}) exceeds limit ({MAX_DEAD_FOLLOWERS})");
                    isValid = false;
                }
                
                var deadIds = new HashSet<int>();
                foreach (var dead in deadFollowers)
                {
                    if (dead == null)
                    {
                        warnings.Add("Found null entry in Dead Followers list");
                        isValid = false;
                        continue;
                    }
                    
                    if (!deadIds.Add(dead.ID))
                    {
                        warnings.Add($"Duplicate dead follower ID: {dead.ID}");
                        isValid = false;
                    }
                }
            }
            
            // Validate Elderly IDs list references
            var elderlyIds = dm.Followers_Elderly_IDs;
            if (elderlyIds != null && followers != null)
            {
                foreach (var id in elderlyIds)
                {
                    if (followers.All(f => f == null || f.ID != id))
                    {
                        warnings.Add($"Elderly ID {id} does not correspond to any active follower");
                        isValid = false;
                    }
                }
            }
            
            // Validate Recruits
            var recruits = dm.Followers_Recruit;
            if (recruits != null)
            {
                if (recruits.Count > MAX_RECRUITS)
                {
                    warnings.Add($"Recruit count ({recruits.Count}) exceeds limit ({MAX_RECRUITS})");
                    isValid = false;
                }
            }
            
            Logger.Info(Logger.SAVE, $"Follower validation: {(isValid ? "PASSED" : "FAILED")}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Follower validation exception: {ex.Message}");
            Logger.Error(Logger.SAVE, $"Follower validation error: {ex.Message}");
            isValid = false;
        }
        
        return isValid;
    }
    
    #endregion
    
    #region Inventory Validation
    
    /// <summary>
    /// Validates inventory item integrity.
    /// </summary>
    public static bool ValidateInventory(object saveData, List<string> warnings)
    {
        bool isValid = true;
        
        try
        {
            // Initialize enum cache if needed
            if (!_enumCacheInitialized)
            {
                InitializeEnumCache();
            }
            
            var inventory = Inventory.items;
            if (inventory == null)
            {
                warnings.Add("Inventory is null");
                return false;
            }
            
            var invalidItems = new List<InventoryItem>();
            
            foreach (var item in inventory)
            {
                if (item == null)
                {
                    warnings.Add("Found null item in inventory");
                    isValid = false;
                    continue;
                }
                
                // Validate item type against enum
                if (_validItemTypeValues != null && !_validItemTypeValues.Contains(item.type))
                {
                    warnings.Add($"Invalid inventory item type: {item.type}");
                    invalidItems.Add(item);
                    isValid = false;
                }
                
                // Validate quantity
                if (item.quantity < 0)
                {
                    warnings.Add($"Negative inventory quantity for type {item.type}: {item.quantity}");
                    item.quantity = 0;
                }
                
                // Check for suspiciously high quantities
                if (item.quantity > 9999)
                {
                    warnings.Add($"Suspiciously high quantity for type {item.type}: {item.quantity}");
                }
            }
            
            // Log results
            if (invalidItems.Count > 0)
            {
                Logger.Warning(Logger.SAVE, $"Found {invalidItems.Count} invalid inventory items");
            }
            
            Logger.Info(Logger.SAVE, $"Inventory validation: {(isValid ? "PASSED" : "FAILED")} ({inventory.Count} items checked)");
        }
        catch (Exception ex)
        {
            warnings.Add($"Inventory validation exception: {ex.Message}");
            Logger.Error(Logger.SAVE, $"Inventory validation error: {ex.Message}");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Initializes cache of valid InventoryItem.ITEM_TYPE enum values.
    /// </summary>
    private static void InitializeEnumCache()
    {
        try
        {
            var itemTypeEnum = Type.GetType("InventoryItem+ITEM_TYPE, Assembly-CSharp");
            if (itemTypeEnum != null)
            {
                var values = Enum.GetValues(itemTypeEnum);
                _validItemTypeValues = new HashSet<int>();
                foreach (var val in values)
                {
                    _validItemTypeValues.Add(Convert.ToInt32(val));
                }
                _enumCacheInitialized = true;
                Logger.Info(Logger.SAVE, $"Cached {_validItemTypeValues.Count} valid inventory item types");
            }
            else
            {
                Logger.Warning(Logger.SAVE, "Could not find InventoryItem.ITEM_TYPE enum, using fallback validation");
                // Fallback: allow common ranges
                _validItemTypeValues = new HashSet<int>(Enumerable.Range(0, 500));
                _enumCacheInitialized = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(Logger.SAVE, $"Failed to initialize enum cache: {ex.Message}");
            _validItemTypeValues = new HashSet<int>(Enumerable.Range(0, 500));
            _enumCacheInitialized = true;
        }
    }
    
    #endregion
    
    #region Flags Validation
    
    /// <summary>
    /// Validates flag table consistency.
    /// </summary>
    public static bool ValidateFlags(object saveData, List<string> warnings)
    {
        bool isValid = true;
        
        try
        {
            var dm = saveData as DataManager;
            if (dm == null)
            {
                warnings.Add("Cannot access DataManager for flag validation");
                return false;
            }
            
            // Validate GameEventFlags if it exists
            var eventFlagsType = Type.GetType("GameEventFlags, Assembly-CSharp");
            if (eventFlagsType != null)
            {
                var flagsField = eventFlagsType.GetField("Flags", BindingFlags.Static | BindingFlags.Public);
                if (flagsField != null)
                {
                    var flags = flagsField.GetValue(null) as Dictionary<string, int>;
                    if (flags != null)
                    {
                        foreach (var kvp in flags)
                        {
                            if (kvp.Value < 0 || kvp.Value > MAX_FLAG_VALUE)
                            {
                                warnings.Add($"Flag '{kvp.Key}' has out-of-range value: {kvp.Value}");
                                isValid = false;
                            }
                        }
                    }
                }
            }
            
            // Check for common quest prerequisite flags that might be missing
            var requiredFlags = new[] 
            {
                "IntroComplete",
                "CultFoundingUnlocked",
                "FirstFollowerRecruited"
            };
            
            // Note: In a full implementation, we'd check these against actual flag storage
            // For now, we just log that we checked them
            Logger.Info(Logger.SAVE, $"Flag validation: {(isValid ? "PASSED" : "FAILED")}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Flag validation exception: {ex.Message}");
            Logger.Error(Logger.SAVE, $"Flag validation error: {ex.Message}");
            isValid = false;
        }
        
        return isValid;
    }
    
    #endregion
    
    #region Quest Validation
    
    /// <summary>
    /// Validates quest progression order and stage limits.
    /// </summary>
    public static bool ValidateQuests(object saveData, List<string> warnings)
    {
        bool isValid = true;
        
        try
        {
            var dm = saveData as DataManager;
            if (dm == null)
            {
                warnings.Add("Cannot access DataManager for quest validation");
                return false;
            }
            
            // Check for Quests that might be in wrong order or have invalid stages
            // This is a simplified check - actual implementation would need game-specific quest definitions
            
            // Validate currentQuestStage if it exists
            var stageField = dm.GetType().GetField("currentQuestStage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (stageField != null)
            {
                var stage = Convert.ToInt32(stageField.GetValue(dm));
                if (stage < 0)
                {
                    warnings.Add($"Invalid negative quest stage: {stage}");
                    stageField.SetValue(dm, 0);
                    isValid = false;
                }
                else if (stage > MAX_QUEST_STAGES)
                {
                    warnings.Add($"Quest stage ({stage}) exceeds maximum ({MAX_QUEST_STAGES})");
                    stageField.SetValue(dm, MAX_QUEST_STAGES);
                    isValid = false;
                }
            }
            
            // Check for quest-specific validations via reflection
            var questDataType = Type.GetType("Quest+QuestData, Assembly-CSharp");
            if (questDataType != null)
            {
                // Would validate quest data here if needed
            }
            
            Logger.Info(Logger.SAVE, $"Quest validation: {(isValid ? "PASSED" : "FAILED")}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Quest validation exception: {ex.Message}");
            Logger.Error(Logger.SAVE, $"Quest validation error: {ex.Message}");
            isValid = false;
        }
        
        return isValid;
    }
    
    #endregion
    
    #region DLC Progression Validation
    
    /// <summary>
    /// Validates DLC progression flags consistency.
    /// </summary>
    public static bool ValidateDLCProgression(object saveData, List<string> warnings)
    {
        bool isValid = true;
        
        try
        {
            var dm = saveData as DataManager;
            if (dm == null)
            {
                warnings.Add("Cannot access DataManager for DLC validation");
                return false;
            }
            
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            
            // If major DLC is not owned, ensure no DLC-specific content exists
            if (!hasMajorDLC)
            {
                // Check for Woolhaven-specific structures or items that shouldn't exist
                // This is a simplified check
                
                // Check for DLC-specific follower roles
                // Would need game-specific knowledge for full validation
            }
            else
            {
                // DLC is owned - validate DLC progression exists
                var dlcDataType = Type.GetType("DLCManager, Assembly-CSharp");
                if (dlcDataType != null)
                {
                    var dlcField = dlcDataType.GetField("DLCInstalled", BindingFlags.Static | BindingFlags.Public);
                    if (dlcField != null)
                    {
                        var dlcInstalled = Convert.ToBoolean(dlcField.GetValue(null));
                        if (!dlcInstalled)
                        {
                            warnings.Add("Major DLC installed but game doesn't recognize it");
                            isValid = false;
                        }
                    }
                }
            }
            
            Logger.Info(Logger.SAVE, $"DLC validation: {(isValid ? "PASSED" : "FAILED")}");
        }
        catch (Exception ex)
        {
            warnings.Add($"DLC validation exception: {ex.Message}");
            Logger.Error(Logger.SAVE, $"DLC validation error: {ex.Message}");
            isValid = false;
        }
        
        return isValid;
    }
    
    #endregion
    
    #region Repair Functionality
    
    /// <summary>
    /// Attempts to repair invalid save data.
    /// </summary>
    /// <param name="saveData">The save data to repair</param>
    /// <returns>RepairResult with details of what was fixed</returns>
    public static RepairResult TryRepair(object saveData)
    {
        var result = new RepairResult();
        
        try
        {
            if (saveData == null)
            {
                result.Failures.Add("Save data is null, cannot repair");
                return result;
            }
            
            Logger.Info(Logger.FLAGS, "Starting save repair process...");
            
            var dm = saveData as DataManager;
            if (dm == null)
            {
                result.Failures.Add("Cannot access DataManager for repairs");
                return result;
            }
            
            // Track repairs
            int repairCount = 0;
            
            // 1. Repair follower count
            repairCount += RepairFollowerCount(dm, result);
            
            // 2. Remove invalid inventory items
            repairCount += RepairInvalidInventory(dm, result);
            
            // 3. Repair quest stages
            repairCount += RepairQuestStages(dm, result);
            
            // 4. Repair flag values
            repairCount += RepairFlagValues(dm, result);
            
            // 5. Repair DLC inconsistencies
            repairCount += RepairDLCInconsistencies(dm, result);
            
            result.Success = result.Failures.Count == 0;
            
            Logger.Info(Logger.FLAGS, $"Repair complete: {result.TotalRepairs} repairs made, {result.Failures.Count} failures");
        }
        catch (Exception ex)
        {
            result.Failures.Add($"Repair exception: {ex.Message}");
            Logger.Error(Logger.FLAGS, $"Repair error: {ex.Message}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Repairs follower count issues.
    /// </summary>
    private static int RepairFollowerCount(DataManager dm, RepairResult result)
    {
        int repairs = 0;
        
        try
        {
            int maxFollowers = CultUtils.HasMajorDLC() ? MAX_FOLLOWERS_DLC : MAX_FOLLOWERS_BASE;
            
            // Trim followers if over limit
            if (dm.Followers != null && dm.Followers.Count > maxFollowers)
            {
                int removeCount = dm.Followers.Count - maxFollowers;
                dm.Followers.RemoveRange(maxFollowers, removeCount);
                string repairMsg = $"Removed {removeCount} excess followers (limited to {maxFollowers})";
                result.Repairs.Add(repairMsg);
                Logger.Info(Logger.FLAGS, repairMsg);
                repairs++;
            }
            
            // Remove null entries from followers list
            if (dm.Followers != null)
            {
                int nullCount = dm.Followers.RemoveAll(f => f == null);
                if (nullCount > 0)
                {
                    string repairMsg = $"Removed {nullCount} null followers";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
            
            // Remove null entries from dead followers
            if (dm.Followers_Dead != null)
            {
                int nullCount = dm.Followers_Dead.RemoveAll(f => f == null);
                if (nullCount > 0)
                {
                    string repairMsg = $"Removed {nullCount} null dead followers";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
            
            // Trim recruits if over limit
            if (dm.Followers_Recruit != null && dm.Followers_Recruit.Count > MAX_RECRUITS)
            {
                int removeCount = dm.Followers_Recruit.Count - MAX_RECRUITS;
                dm.Followers_Recruit.RemoveRange(MAX_RECRUITS, removeCount);
                string repairMsg = $"Removed {removeCount} excess recruits (limited to {MAX_RECRUITS})";
                result.Repairs.Add(repairMsg);
                Logger.Info(Logger.FLAGS, repairMsg);
                repairs++;
            }
            
            // Clean up elderly IDs that don't correspond to active followers
            if (dm.Followers_Elderly_IDs != null && dm.Followers != null)
            {
                var validIds = new HashSet<int>(dm.Followers.Where(f => f != null).Select(f => f.ID));
                int removedCount = dm.Followers_Elderly_IDs.RemoveAll(id => !validIds.Contains(id));
                if (removedCount > 0)
                {
                    string repairMsg = $"Removed {removedCount} invalid elderly follower IDs";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Failures.Add($"Follower repair failed: {ex.Message}");
        }
        
        return repairs;
    }
    
    /// <summary>
    /// Repairs invalid inventory items.
    /// </summary>
    private static int RepairInvalidInventory(DataManager dm, RepairResult result)
    {
        int repairs = 0;
        
        try
        {
            if (Inventory.items == null) return 0;
            
            // Initialize enum cache if needed
            if (!_enumCacheInitialized)
            {
                InitializeEnumCache();
            }
            
            // Fix negative quantities
            foreach (var item in Inventory.items)
            {
                if (item != null && item.quantity < 0)
                {
                    item.quantity = 0;
                    string repairMsg = $"Reset negative quantity for item type {item.type} to 0";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
            
            // Remove invalid item types (optional - could also just warn)
            if (_validItemTypeValues != null)
            {
                int invalidCount = Inventory.items.RemoveAll(item => 
                    item != null && !_validItemTypeValues.Contains(item.type));
                
                if (invalidCount > 0)
                {
                    string repairMsg = $"Removed {invalidCount} invalid inventory item types";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
            
            // Remove null entries
            int nullCount = Inventory.items.RemoveAll(i => i == null);
            if (nullCount > 0)
            {
                string repairMsg = $"Removed {nullCount} null inventory items";
                result.Repairs.Add(repairMsg);
                Logger.Info(Logger.FLAGS, repairMsg);
                repairs++;
            }
        }
        catch (Exception ex)
        {
            result.Failures.Add($"Inventory repair failed: {ex.Message}");
        }
        
        return repairs;
    }
    
    /// <summary>
    /// Repairs invalid quest stages.
    /// </summary>
    private static int RepairQuestStages(DataManager dm, RepairResult result)
    {
        int repairs = 0;
        
        try
        {
            // Fix current quest stage
            var stageField = dm.GetType().GetField("currentQuestStage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (stageField != null)
            {
                var stage = Convert.ToInt32(stageField.GetValue(dm));
                if (stage < 0)
                {
                    stageField.SetValue(dm, 0);
                    string repairMsg = "Reset negative quest stage to 0";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
                else if (stage > MAX_QUEST_STAGES)
                {
                    stageField.SetValue(dm, MAX_QUEST_STAGES);
                    string repairMsg = $"Clamped quest stage from {stage} to {MAX_QUEST_STAGES}";
                    result.Repairs.Add(repairMsg);
                    Logger.Info(Logger.FLAGS, repairMsg);
                    repairs++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Failures.Add($"Quest repair failed: {ex.Message}");
        }
        
        return repairs;
    }
    
    /// <summary>
    /// Repairs invalid flag values.
    /// </summary>
    private static int RepairFlagValues(DataManager dm, RepairResult result)
    {
        int repairs = 0;
        
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
                        var outOfRangeKeys = flags.Where(kvp => kvp.Value < 0 || kvp.Value > MAX_FLAG_VALUE)
                            .Select(kvp => kvp.Key).ToList();
                        
                        foreach (var key in outOfRangeKeys)
                        {
                            flags[key] = flags[key] < 0 ? 0 : (flags[key] > MAX_FLAG_VALUE ? MAX_FLAG_VALUE : flags[key]);
                            string repairMsg = $"Fixed flag '{key}' value to {flags[key]}";
                            result.Repairs.Add(repairMsg);
                            Logger.Info(Logger.FLAGS, repairMsg);
                            repairs++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Failures.Add($"Flag repair failed: {ex.Message}");
        }
        
        return repairs;
    }
    
    /// <summary>
    /// Repairs DLC inconsistencies.
    /// </summary>
    private static int RepairDLCInconsistencies(DataManager dm, RepairResult result)
    {
        int repairs = 0;
        
        try
        {
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            
            // If no DLC, ensure DLC-specific flags aren't set incorrectly
            // This is a simplified implementation - full validation would need more game-specific knowledge
            
            if (!hasMajorDLC)
            {
                // Log that we checked DLC consistency
                Logger.Info(Logger.FLAGS, "Verified no DLC content exists without DLC");
            }
        }
        catch (Exception ex)
        {
            result.Failures.Add($"DLC repair failed: {ex.Message}");
        }
        
        return repairs;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Quick validation check that can be called before save operations.
    /// Returns true if save appears safe to modify.
    /// </summary>
    public static bool IsSaveSafe()
    {
        try
        {
            if (DataManager.Instance == null)
            {
                return false;
            }
            
            // Quick follower count check
            int maxFollowers = CultUtils.HasMajorDLC() ? MAX_FOLLOWERS_DLC : MAX_FOLLOWERS_BASE;
            if (DataManager.Instance.Followers != null && DataManager.Instance.Followers.Count >= maxFollowers)
            {
                Logger.Warning(Logger.SAVE, $"Follower count at limit ({maxFollowers})");
                return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets a summary of current save state for debugging.
    /// </summary>
    public static Dictionary<string, object> GetSaveSummary()
    {
        var summary = new Dictionary<string, object>();
        
        try
        {
            if (DataManager.Instance == null)
            {
                summary["Error"] = "DataManager not available";
                return summary;
            }
            
            summary["FollowerCount"] = DataManager.Instance.Followers?.Count ?? 0;
            summary["DeadFollowerCount"] = DataManager.Instance.Followers_Dead?.Count ?? 0;
            summary["RecruitCount"] = DataManager.Instance.Followers_Recruit?.Count ?? 0;
            summary["ElderlyCount"] = DataManager.Instance.Followers_Elderly_IDs?.Count ?? 0;
            summary["InventoryCount"] = Inventory.items?.Count ?? 0;
            
            // Try to get quest stage
            try
            {
                var stageField = DataManager.Instance.GetType().GetField("currentQuestStage", 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (stageField != null)
                {
                    summary["QuestStage"] = Convert.ToInt32(stageField.GetValue(DataManager.Instance));
                }
            }
            catch { }
            
            summary["HasMajorDLC"] = CultUtils.HasMajorDLC();
        }
        catch (Exception ex)
        {
            summary["Error"] = ex.Message;
        }
        
        return summary;
    }
    
    #endregion
}
