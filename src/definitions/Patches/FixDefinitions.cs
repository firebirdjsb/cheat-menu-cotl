using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Fix patches for game issues - requires owning the relevant DLC content
/// </summary>
[CheatCategory(CheatCategoryEnum.MISC)]
public class FixDefinitions : IDefinition {

    // ── Clothing Fixes ───────────────────────────────────────────────────────────
    [CheatDetails("Fix Quest Clothing Bug", "Removes quest-specific clothing from unlocked list to fix softlocks", subGroup: "Patches/Fix")]
    public static void FixQuestClothingBug(){
        try {
            // Get quest-specific clothing types from shared helper
            var questClothingToRemove = CultDefinitions.GetQuestClothingTypes();
            
            int removedCount = 0;
            foreach(var clothingType in questClothingToRemove){
                if(DataManager.Instance.UnlockedClothing.Contains(clothingType)){
                    DataManager.Instance.UnlockedClothing.Remove(clothingType);
                    removedCount++;
                }
                if(DataManager.Instance.ClothingAssigned.Contains(clothingType)){
                    DataManager.Instance.ClothingAssigned.Remove(clothingType);
                }
            }
            
            CultUtils.PlayNotification($"Fixed quest clothing bug! Removed {removedCount} item(s). Reload save to fix softlocks.");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] FixQuestClothingBug failed: {e.Message}");
            CultUtils.PlayNotification("Failed to fix quest clothing!");
        }
    }
}
