using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.DLC)]
public class DlcDefinitions : IDefinition{

    // DLC ownership helpers (delegates to shared CultUtils methods)
    private static bool HasMajorDLC()   => CultUtils.HasMajorDLC();
    private static bool HasSinfulDLC()  => CultUtils.HasSinfulDLC();
    private static bool HasCultistDLC() => CultUtils.HasCultistDLC();
    private static bool HasHereticDLC() => CultUtils.HasHereticDLC();
    private static bool HasPilgrimDLC() => CultUtils.HasPilgrimDLC();

    // ── Twitch Drops (free – no paid DLC required) ─────────────────────────
    [CheatDetails("Unlock All Twitch Drops", "Unlocks all 20 Twitch-drop follower skins and decorations")]
    public static void UnlockAllTwitchDrops(){
        try {
            int unlocked = 0;
            if(DataManager.ActivateTwitchDrop1())  unlocked++;
            if(DataManager.ActivateTwitchDrop2())  unlocked++;
            if(DataManager.ActivateTwitchDrop3())  unlocked++;
            if(DataManager.ActivateTwitchDrop4())  unlocked++;
            if(DataManager.ActivateTwitchDrop5())  unlocked++;
            if(DataManager.ActivateTwitchDrop6())  unlocked++;
            if(DataManager.ActivateTwitchDrop7())  unlocked++;
            if(DataManager.ActivateTwitchDrop8())  unlocked++;
            if(DataManager.ActivateTwitchDrop9())  unlocked++;
            if(DataManager.ActivateTwitchDrop10()) unlocked++;
            if(DataManager.ActivateTwitchDrop11()) unlocked++;
            if(DataManager.ActivateTwitchDrop12()) unlocked++;
            if(DataManager.ActivateTwitchDrop13()) unlocked++;
            if(DataManager.ActivateTwitchDrop14()) unlocked++;
            if(DataManager.ActivateTwitchDrop15()) unlocked++;
            if(DataManager.ActivateTwitchDrop16()) unlocked++;
            if(DataManager.ActivateTwitchDrop17()) unlocked++;
            if(DataManager.ActivateTwitchDrop18()) unlocked++;
            if(DataManager.ActivateTwitchDrop19()) unlocked++;
            if(DataManager.ActivateTwitchDrop20()) unlocked++;
            if(DataManager.ActivateSupportStreamer()) unlocked++;
            CultUtils.PlayNotification(unlocked > 0 ? $"Unlocked {unlocked} Twitch drop(s)!" : "All Twitch drops already unlocked!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to unlock Twitch drops: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock Twitch drops!");
        }
    }

    // Major DLC items (requires MAJOR_DLC)
    [CheatDetails("Give DLC Seeds", "Gives x20 of each DLC seed type (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveDlcSeeds(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_HOPS, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_GRAPES, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_COTTON, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SOZO, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SNOW_FRUIT, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_CHILLI, 20);
        CultUtils.PlayNotification("DLC seeds added!");
    }

    [CheatDetails("Give All Drinks", "Gives x10 of every drink type (base drinks always, DLC drinks require Major DLC)")]
    public static void GiveAllDlcDrinks(){
        // Base game drinks - always give these
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_BEER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_WINE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_COCKTAIL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_EGGNOG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_GIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_POOP_JUICE, 10);
        
        // DLC drinks - only give if user has Major DLC
        if(HasMajorDLC()){
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_SIN, 10);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_CHILLI, 10);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_LIGHTNING, 10);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_GRASS, 10);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_MUSHROOM_JUICE, 10);
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_MILKSHAKE, 10);
            CultUtils.PlayNotification("All drinks (base + DLC) added!");
        } else {
            CultUtils.PlayNotification("Base drinks added! (DLC drinks require Major DLC)");
        }
    }

    [CheatDetails("Give Brewing Ingredients", "Gives x30 hops, grapes, chilli, cotton, snow fruit, milk (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveBrewingCrops(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.HOPS, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GRAPES, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHILLI, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SNOW_FRUIT, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 30);
        CultUtils.PlayNotification("Brewing ingredients added!");
    }

    [CheatDetails("Give All Forge Materials", "Gives forge flames, magma, lightning shards, soot, charcoal (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveAllForgeMaterials(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FORGE_FLAME, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ELECTRIFIED_MAGMA, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LIGHTNING_SHARD, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
        CultUtils.PlayNotification("All forge materials added!");
    }



    [CheatDetails("Give Broken Weapons", "Gives one of each broken weapon for repair (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveBrokenWeapons(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_HAMMER, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_SWORD, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_DAGGER, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_GAUNTLETS, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_AXE, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_BLUNDERBUSS, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_CHAIN, 1);
        CultUtils.PlayNotification("Broken weapons added!");
    }

    [CheatDetails("Give Legendary Weapon Fragments", "Gives x10 Legendary Weapon Fragments (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveLegendaryFragments(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LEGENDARY_WEAPON_FRAGMENT, 10);
        CultUtils.PlayNotification("Legendary fragments added!");
    }

    [CheatDetails("Give DLC Collectibles", "Gives rot eyes, lore stones, illegible letters, weapon fragments (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveDlcCollectibles(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BEHOLDER_EYE, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BEHOLDER_EYE_ROT, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LORE_STONE, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LEGENDARY_WEAPON_FRAGMENT, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_SCYLLA, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_CHARYBDIS, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WEBBER_SKULL, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RATAU_STAFF, 1);
        CultUtils.PlayNotification("DLC collectibles added!");
    }

    [CheatDetails("Give DLC Necklace", "Gives the DLC exclusive necklace (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveDlcNecklace(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DLC_NECKLACE, 1);
        CultUtils.PlayNotification("DLC necklace added!");
    }

    [CheatDetails("Give Flockade Pieces", "Gives x5 Flockade building pieces (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveFlockadePieces(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOCKADE_PIECE, 5);
        CultUtils.PlayNotification("Flockade pieces added!");
    }

    [CheatDetails("Give Webber Skull & Ratau Staff", "Gives key story collectibles (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveStoryCollectibles(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WEBBER_SKULL, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RATAU_STAFF, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_SCYLLA, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_CHARYBDIS, 1);
        CultUtils.PlayNotification("Story collectibles added!");
    }

    [CheatDetails("Set Buried Fleeces", "Sets all buried fleece flags and removes special wool from inventory (fixes Woolhaven gate)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SetBuriedFleeces(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            int setCount = 0;
            int removedCount = 0;
            
            // Set the NPC rescue flags - this is what the game checks for buried fleeces
            // The wool items should not be in inventory (buried), and these flags indicate rescue
            
            // Rancher
            DataManager.Instance.NPCGhostRancherRescued = true;
            setCount++;
            
            // Flockade (LambWar)
            DataManager.Instance.NPCGhostFlockadeRescued = true;
            setCount++;
            
            // Tarot
            DataManager.Instance.NPCGhostTarotRescued = true;
            setCount++;
            
            // Blacksmith
            DataManager.Instance.NPCGhostBlacksmithRescued = true;
            setCount++;
            
            // Deco (Decoration)
            DataManager.Instance.NPCGhostDecoRescued = true;
            setCount++;
            
            // Graveyard
            DataManager.Instance.NPCGhostGraveyardRescued = true;
            setCount++;
            
            // Remove special wool items from inventory (these are the "buried" wools)
            // The game checks that these are NOT in inventory to consider them "buried"
            var inventory = Inventory.items;
            if(inventory != null){
                // Count items before removal
                int rancherWool = inventory.FindAll(i => i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_RANCHER).Count;
                int lambwarWool = inventory.FindAll(i => i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_LAMBWAR).Count;
                int blacksmithWool = inventory.FindAll(i => i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_BLACKSMITH).Count;
                int tarotWool = inventory.FindAll(i => i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_TAROT).Count;
                int decoWool = inventory.FindAll(i => i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_DECORATION).Count;
                
                removedCount = rancherWool + lambwarWool + blacksmithWool + tarotWool + decoWool;
                
                // Remove all special wool items
                inventory.RemoveAll(i => 
                    i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_RANCHER ||
                    i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_LAMBWAR ||
                    i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_BLACKSMITH ||
                    i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_TAROT ||
                    i.type == (int)InventoryItem.ITEM_TYPE.SPECIAL_WOOL_DECORATION
                );
            }
            
            CultUtils.PlayNotification($"Set {setCount} fleece flags, removed {removedCount} wool item(s)!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to set buried fleeces: {e.Message}");
            CultUtils.PlayNotification("Failed to set buried fleeces!");
        }
    }

    [CheatDetails("Empty Furnace Fuel", "Empties the DLC furnace fuel (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void EmptyFurnaceFuel(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            Traverse.Create(typeof(CheatConsole)).Method("EmptyFurnaceFuel").GetValue();
            CultUtils.PlayNotification("Furnace fuel emptied!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to empty furnace: {e.Message}");
            CultUtils.PlayNotification("Failed to empty furnace!");
        }
    }

    [CheatDetails("Fill Furnace Fuel", "Fills all DLC furnaces to max fuel (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void FillFurnaceFuel(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            int count = 0;
            foreach(var furnace in Interaction_DLCFurnace.Furnaces){
                if(furnace != null && furnace.Structure != null && furnace.Structure.Brain != null){
                    furnace.Structure.Brain.Data.Fuel = furnace.Structure.Brain.Data.MaxFuel;
                    count++;
                }
            }
            CultUtils.PlayNotification(count > 0 ? $"Filled {count} furnace(s)!" : "No furnaces found!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to fill furnace: {e.Message}");
            CultUtils.PlayNotification("Failed to fill furnace!");
        }
    }

    [CheatDetails("Make Spies Leave", "Forces all spy followers to leave your cult (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void MakeSpiesLeave(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            Traverse.Create(typeof(CheatConsole)).Method("MakeSpiesLeave").GetValue();
            CultUtils.PlayNotification("Spies removed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to remove spies: {e.Message}");
            CultUtils.PlayNotification("Failed to remove spies!");
        }
    }

    [CheatDetails("Finish All Missions", "Instantly completes all follower missionary tasks and sets all mission flags (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void FinishAllMissions(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        int completed = 0;
        bool hasMajor = HasMajorDLC();
        bool hasSinful = HasSinfulDLC();
        bool hasCultist = HasCultistDLC();
        bool hasHeretic = HasHereticDLC();
        bool hasPilgrim = HasPilgrimDLC();
        
        try {
            // Try the built-in cheat console method first
            try {
                Traverse.Create(typeof(CheatConsole)).Method("FinishAllMissions").GetValue();
                completed++;
            } catch { }
            
            // Try to find and complete all mission-related fields in DataManager
            try {
                var dmType = typeof(DataManager);
                // Look for mission-related fields - only process if user has relevant DLC
                foreach(var field in dmType.GetFields(BindingFlags.Instance | BindingFlags.Public)){
                    string fieldName = field.Name;
                    bool shouldProcess = false;
                    
                    // Check if this field is DLC-specific and if user has that DLC
                    if(fieldName.Contains("Winter") || fieldName.Contains("Woolhaven") || fieldName.Contains("Major")){
                        shouldProcess = hasMajor;
                    } else if(fieldName.Contains("Sinful") || fieldName.Contains("Sin")){
                        shouldProcess = hasSinful;
                    } else if(fieldName.Contains("Cultist") || fieldName.Contains("Cult")){
                        shouldProcess = hasCultist;
                    } else if(fieldName.Contains("Heretic") || fieldName.Contains("Heretic")){
                        shouldProcess = hasHeretic;
                    } else if(fieldName.Contains("Pilgrim") || fieldName.Contains("Pilgrim")){
                        shouldProcess = hasPilgrim;
                    } else {
                        // Not DLC-specific, process it
                        shouldProcess = true;
                    }
                    
                    if(!shouldProcess) continue;
                    
                    try {
                        var val = field.GetValue(DataManager.Instance);
                        // Check if it's a list/collection we can clear or mark complete
                        if(val is System.Collections.IList list){
                            list.Clear();
                            completed++;
                        } else if(val is bool boolVal){
                            // If it's a bool that can be set to true
                            if(fieldName.Contains("Completed") || fieldName.Contains("Finished") || fieldName.Contains("Done")){
                                field.SetValue(DataManager.Instance, true);
                                completed++;
                            }
                        }
                    } catch { }
                }
            } catch { }
            
            // Try to find and invoke mission manager
            try {
                Type missionMgrType = null;
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                    foreach(var type in asm.GetTypes()){
                        if(type.Name != null && (type.Name.Contains("Mission") && type.Name.Contains("Manager"))){
                            missionMgrType = type;
                            break;
                        }
                    }
                    if(missionMgrType != null) break;
                }
                if(missionMgrType != null){
                    var instanceProp = missionMgrType.GetProperty("Instance");
                    if(instanceProp != null){
                        var mgr = instanceProp.GetValue(null);
                        if(mgr != null){
                            // Try CompleteAll or FinishAll methods
                            foreach(var method in missionMgrType.GetMethods()){
                                if(method.Name.Contains("Complete") || method.Name.Contains("Finish") || method.Name.Contains("Done")){
                                    try { method.Invoke(mgr, null); completed++; } catch { }
                                }
                            }
                        }
                    }
                }
            } catch { }
            
            CultUtils.PlayNotification($"All missions processed! ({completed} actions)");
        } catch(Exception e){
            Debug.LogWarning($"Failed to finish missions: {e.Message}");
            CultUtils.PlayNotification("Failed to finish missions!");
        }
    }

    [CheatDetails("Unlock Winter Mode", "Unlocks the winter survival game mode (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void UnlockWinterMode(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            Type persistenceType = null;
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                persistenceType = asm.GetType("PersistenceManager");
                if(persistenceType != null) break;
            }
            if(persistenceType != null){
                var persistentData = Traverse.Create(persistenceType).Property("PersistentData").GetValue();
                if(persistentData != null){
                    Traverse.Create(persistentData).Property("UnlockedWinterMode").SetValue(true);
                    Traverse.Create(persistenceType).Method("Save").GetValue();
                    CultUtils.PlayNotification("Winter mode unlocked!");
                } else {
                    CultUtils.PlayNotification("PersistentData not available!");
                }
            } else {
                CultUtils.PlayNotification("PersistenceManager not found!");
            }
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to unlock winter mode: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock winter mode!");
        }
    }

    [CheatDetails("Unlock All Relics", "Unlocks all base game relics. If Major DLC is owned, also unlocks Woolhaven DLC relics.")]
    public static void UnlockAllRelics(){
        try {
            bool hasDlc = HasMajorDLC();
            int unlocked = 0;
            int skippedDlc = 0;
            
            // Get all RelicType enum values
            var relicTypes = Enum.GetValues(typeof(RelicType));
            
            foreach(RelicType relicType in relicTypes){
                // Skip invalid/None relic type
                if(relicType == RelicType.None) continue;
                
                // Check if this relic is DLC-specific
                bool isDlcRelic = RelicData.GetRelicDLC(relicType);
                
                // If it's a DLC relic and player doesn't have the DLC, skip it
                if(isDlcRelic && !hasDlc){
                    skippedDlc++;
                    continue;
                }
                
                // Unlock the relic
                DataManager.UnlockRelic(relicType);
                unlocked++;
            }
            
            string message = hasDlc 
                ? $"Unlocked {unlocked} relic(s) (all base + DLC)!"
                : $"Unlocked {unlocked} base relic(s)! {skippedDlc} DLC relic(s) skipped (requires Major DLC)";
            
            CultUtils.PlayNotification(message);
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to unlock relics: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock relics!");
        }
    }

}
