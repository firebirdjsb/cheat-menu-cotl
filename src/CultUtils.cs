using Lamb.UI;
using System;
using System.Collections;
using src.Extensions;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using Spine.Unity;

namespace CheatMenu;

using DoctrinePairs =  Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>>;

// ============================================================================
// MAIN FILE: CultUtils.cs
// This is a partial class aggregator. All implementations are in separate
// partial files for better code organization:
//   - CultUtils_Followers.cs  (follower spawning, stats, traits, etc.)
//   - CultUtils_Farming.cs    (farming, animals, ranching, etc.)
//   - CultUtils_Structures.cs (buildings, trees, cleanup, etc.)
//   - CultUtils_Cult.cs      (doctrine, faith, rituals, objectives)
//   - CultUtils_Wolf.cs      (friendly wolf companion system)
//   - CultUtils_Materials.cs  (inventory, resources)
//   - CultUtils_DLC.cs        (DLC helpers)
//   - CultUtils_Misc.cs      (utilities and other shared methods)
// ============================================================================

internal static partial class CultUtils {
    // -- Shared Fields (used across all partial files) ----------------------
    
    // Flag to track if we're currently spawning a follower from the cheat menu
    // Used to prevent the game's auto-spawn from creating duplicates
    public static bool IsSpawningFollowerFromCheat = false;

    // -- Shared DLC ownership helpers -------------------------------------
    public static bool HasMajorDLC()   => IsInGame() && DataManager.Instance.MAJOR_DLC;
    public static bool HasSinfulDLC()  => IsInGame() && DataManager.Instance.DLC_Sinful_Pack;
    public static bool HasCultistDLC() => IsInGame() && DataManager.Instance.DLC_Cultist_Pack;
    public static bool HasHereticDLC() => IsInGame() && DataManager.Instance.DLC_Heretic_Pack;
    public static bool HasPilgrimDLC() => IsInGame() && DataManager.Instance.DLC_Pilgrim_Pack;

    // Shared utility methods used across files
    public static bool IsInGame(){
        bool result = SaveAndLoad.Loaded;
        return result;
    }

    public static void PlayNotification(string message){
        if(NotificationCentre.Instance){
            NotificationCentre.Instance.PlayGenericNotification(message);
        }
    }

    public static void AddInventoryItem(InventoryItem.ITEM_TYPE type, int amount){
        try {
            Inventory.AddItem((int)type, amount, false);
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] AddInventoryItem fallback for {type}: {e.Message}");
            try {
                Inventory.AddItem(type, amount, false);
            } catch(Exception e2){
                UnityEngine.Debug.LogWarning($"[CheatMenu] AddInventoryItem failed for {type}: {e2.Message}");
            }
        }
    }
}
