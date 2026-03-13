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

// ============================================================================
// PARTIAL FILE: CultUtils_Structures.cs
// Contains: Building/structure cleanup - trees, rubble, grass, poop, 
//           outhouses, vomit, berry bushes, janitor stations
// ============================================================================

internal static partial class CultUtils {
    // Removes berry/berry-bush style structures from the base (best-effort)
    public static void ClearBerryBushes(){
        try {
            int removed = 0;
            foreach(var brainTypeObj in Enum.GetValues(typeof(StructureBrain.TYPES))){
                try {
                    var brainType = (StructureBrain.TYPES)brainTypeObj;
                    string typeName = brainType.ToString().ToUpperInvariant();
                    if(typeName.Contains("BERRY") || typeName.Contains("BUSH") || typeName.Contains("BERR")){
                        var structures = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, brainType);
                        foreach(var s in structures){
                            try { 
                                AddInventoryItem(InventoryItem.ITEM_TYPE.BERRY, 3);
                                s.Remove(); 
                                removed++; 
                            } catch { }
                        }
                    }
                } catch { }
            }
            PlayNotification(removed > 0 ? $"Berry bushes cleared! ({removed} bushes, {removed * 3} berries)" : "No berry bushes found!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] ClearBerryBushes failed: {e.Message}");
            PlayNotification("Failed to clear berry bushes!");
        }
    }

    public static void ClearBaseTrees(){
        try {
            int count = 0;
            foreach(var tree in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.TREE)){
                try {
                    try {
                        string[] candidates = new string[]{ "WOOD", "LOG", "FIREWOOD", "WOOD_LOG", "CHARCOAL" };
                        bool given = false;
                        foreach(var cand in candidates){
                            try {
                                try {
                                    var parsed = Enum.Parse(typeof(InventoryItem.ITEM_TYPE), cand, true);
                                    AddInventoryItem((InventoryItem.ITEM_TYPE)parsed, 3);
                                    given = true;
                                    break;
                                } catch { }
                            } catch { }
                        }
                        if(!given){
                            AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 3);
                        }
                    } catch { }
                    tree.Remove();
                    count++;
                } catch { }
            }
            PlayNotification(count > 0 ? $"Trees cleared! ({count} trees, resources added)" : "No trees found to clear!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear trees: {e.Message}");
            PlayNotification("Failed to clear trees!");
        }
    }

    public static void ClearBaseRubble(){
        try {
            int count = 0;
            var rubbleTypes = new[] { 
                StructureBrain.TYPES.RUBBLE,
                StructureBrain.TYPES.RUBBLE_BIG,
                StructureBrain.TYPES.ROCK,
                StructureBrain.TYPES.BLOOD_STONE
            };

            foreach(var rubbleType in rubbleTypes){
                foreach(var rubble in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, rubbleType)){
                    rubble.Remove();
                    count++;
                }
            }
            PlayNotification($"Rubble cleared! ({count} items)");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear rubble: {e.Message}");
            PlayNotification("Failed to clear rubble!");
        }
    }

    private static bool IsLandscapeType(string typeName){
        return typeName.Contains("GRASS") || typeName.Contains("WEED") || typeName.Contains("BUSH") 
            || typeName.Contains("SHRUB") || typeName.Contains("FERN") || typeName.Contains("PLANT") 
            || typeName.Contains("FOLIAGE") || typeName.Contains("STUMP") || typeName.Contains("SAPLING")
            || typeName.Contains("DECORATION_ENVIRONMENT");
    }

    public static void ClearBaseGrass(){
        try {
            int count = 0;
            foreach(var locValue in Enum.GetValues(typeof(FollowerLocation))){
                FollowerLocation loc = (FollowerLocation)locValue;
                foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                    string typeName = brainType.ToString();
                    if(IsLandscapeType(typeName)){
                        try {
                            var structures = StructureManager.GetAllStructuresOfType(loc, (StructureBrain.TYPES)brainType);
                            foreach(var structure in structures){
                                structure.Remove();
                                count++;
                            }
                        } catch { }
                    }
                }
            }
            foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
                string typeName = brainType.ToString();
                if(IsLandscapeType(typeName)){
                    try {
                        var remaining = StructureManager.GetAllStructuresOfType((StructureBrain.TYPES)brainType);
                        foreach(var structure in remaining){
                            structure.Remove();
                            count++;
                        }
                    } catch { }
                }
            }
            PlayNotification($"Landscape cleared! ({count} items)");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to clear landscape: {e.Message}");
            PlayNotification("Failed to clear landscape!");
        }
    }

    public static void ClearVomit(){
        foreach(var vomit in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.VOMIT)){
            vomit.Remove();
        }
        PlayNotification("Vomit cleared!");
    }

    public static async void ClearPoop(){
        int poopCount = 0;
        foreach(var poop in StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.POOP)){
            poop.Remove();
            poopCount++;
        }
        foreach(var brainType in Enum.GetValues(typeof(StructureBrain.TYPES))){
            string typeName = brainType.ToString();
            if(typeName.Contains("POOP") && (StructureBrain.TYPES)brainType != StructureBrain.TYPES.POOP){
                try {
                    var poopStructures = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, (StructureBrain.TYPES)brainType);
                    foreach(var ps in poopStructures){
                        ps.Remove();
                        poopCount++;
                    }
                } catch { }
            }
        }
        try {
            foreach(var daycare in Interaction_Daycare.Daycares){
                if(daycare == null || daycare.Structure == null) continue;
                var inventory = daycare.Structure.Inventory;
                if(inventory != null && inventory.Count > 0){
                    foreach(var item in inventory){
                        if(item.type == (int)InventoryItem.ITEM_TYPE.POOP && item.quantity > 0){
                            AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, item.quantity);
                            poopCount++;
                        }
                    }
                    inventory.RemoveAll(i => i.type == (int)InventoryItem.ITEM_TYPE.POOP);
                    daycare.UpdatePoopStates();
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear nursery poop: {e.Message}");
        }
        ClearJanitorStations();
        await AsyncHelper.WaitSeconds(1);
        foreach(var pickup in PickUp.PickUps){
            if(pickup.type == InventoryItem.ITEM_TYPE.POOP){
                pickup.PickMeUp();
            }
        }
        PlayNotification($"Poop cleared! ({poopCount} sources)");
    }

    public static void ClearJanitorStations(){
        try {
            int stationCount = 0;
            int totalSouls = 0;

            var janitorStations = StructureManager.GetAllStructuresOfType<Structures_JanitorStation>();
            foreach(var janitorStation in janitorStations){
                if(janitorStation != null && janitorStation.SoulCount > 0){
                    totalSouls += janitorStation.SoulCount;
                    stationCount++;
                }
            }

            if(totalSouls > 0 && PlayerFarming.Instance != null){
                foreach(var janitorStation in janitorStations){
                    if(janitorStation != null){
                        janitorStation.SoulCount = 0;
                    }
                }

                foreach(var sceneStation in JanitorStation.JanitorStations){
                    if(sceneStation != null){
                        Traverse.Create(sceneStation).Field("previousSoulCount").SetValue(-1);
                    }
                }

                PlayerFarming.Instance.playerChoreXPBarController.AddChoreXP(PlayerFarming.Instance, (float)totalSouls);
            }

            if(stationCount > 0 || totalSouls > 0){
                PlayNotification($"Janitor stations collected! ({totalSouls} XP from {stationCount} stations)");
            } else {
                PlayNotification("No janitor stations with XP found!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to clear janitor stations: {e.Message}");
        }
    }

    public static void ClearOuthouses(){
        List<StructureBrain> outhouse1 = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.OUTHOUSE);
        List<StructureBrain> outhouse2 = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.OUTHOUSE_2);
        StructureBrain[] outhouses = CheatUtils.Concat(outhouse1.ToArray(), outhouse2.ToArray());
        int totalPoop = 0;
        foreach(var outhouse in outhouses){
            if (outhouse is Structures_Outhouse outhouseStructure)
            {
                int poopCount = outhouseStructure.GetPoopCount();
                if(poopCount > 0){
                    AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, poopCount);
                    totalPoop += poopCount;
                }
                outhouseStructure.Data.Inventory.Clear();
            }
        }

        try {
            foreach(var outhouseInteraction in Interaction_Outhouse.Outhouses){
                if(outhouseInteraction != null){
                    outhouseInteraction.UpdateGauge();
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to update outhouse gauges: {e.Message}");
        }

        PlayNotification(totalPoop > 0 ? $"Outhouses cleared! ({totalPoop} poop)" : "Outhouses already clean!");
    }
}
