using HarmonyLib;
using System;
using System.Collections.Generic;
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

    [CheatDetails("Give All Drinks", "Gives x10 of every drink type (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveAllDlcDrinks(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_BEER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_WINE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_COCKTAIL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_EGGNOG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_GIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_SIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_CHILLI, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_LIGHTNING, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_GRASS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_POOP_JUICE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_MUSHROOM_JUICE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_MILKSHAKE, 10);
        CultUtils.PlayNotification("All drinks added!");
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

    [CheatDetails("Give Sin Items Bundle", "Gives sin drinks, soot, sozo seeds, charcoal, magma stones (Sinful DLC required)")]
    [RequiresDLC(DlcRequirement.SinfulDLC)]
    public static void GiveSinItemsBundle(){
        if(!HasSinfulDLC()){ CultUtils.PlayNotification("Requires Sinful DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_SIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SOZO, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 20);
        CultUtils.PlayNotification("Sin items bundle added!");
    }

    [CheatDetails("Add Sin Currency", "Adds the proper SIN currency (Sinful DLC required)")]
    [RequiresDLC(DlcRequirement.SinfulDLC)]
    public static void AddSinCurrency(){
        if(!HasSinfulDLC()){ CultUtils.PlayNotification("Requires Sinful DLC!"); return; }
        try {
            // Many builds store sin currency in DataManager or a specific SinManager type. Try known locations.
            bool added = false;
            // Try DataManager first
            try {
                var dm = DataManager.Instance;
                var prop = dm.GetType().GetProperty("SinCount") ?? dm.GetType().GetProperty("SinCurrency") ?? dm.GetType().GetProperty("Sin");
                if(prop != null && prop.PropertyType == typeof(int)){
                    int cur = (int)prop.GetValue(dm);
                    prop.SetValue(dm, cur + 50);
                    added = true;
                }
            } catch { }

            if(!added){
                // Try to find a SinManager type in loaded assemblies
                foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){
                    try {
                        var sinType = asm.GetType("SinManager") ?? asm.GetType("SinCurrency") ?? asm.GetType("DLC.SinManager");
                        if(sinType == null) continue;
                        var instProp = sinType.GetProperty("Instance") ?? sinType.GetProperty("Singleton");
                        object inst = null;
                        if(instProp != null) inst = instProp.GetValue(null);
                        if(inst == null) continue;
                        // Try field then property names
                        try {
                            var fi = sinType.GetField("SinCount");
                            if(fi != null && fi.FieldType == typeof(int)){
                                int val = (int)fi.GetValue(inst);
                                fi.SetValue(inst, val + 50);
                                added = true; break;
                            }
                        } catch { }
                        try {
                            var pi = sinType.GetProperty("SinCount") ?? sinType.GetProperty("Sin");
                            if(pi != null && pi.PropertyType == typeof(int)){
                                int val = (int)pi.GetValue(inst);
                                pi.SetValue(inst, val + 50);
                                added = true; break;
                            }
                        } catch { }
                    } catch { }
                }
            }

            CultUtils.PlayNotification(added ? "Added 50 SIN currency (if present)" : "SIN currency type not found in this build");
        } catch(Exception e){
            Debug.LogWarning($"Failed to add SIN currency: {e.Message}");
            CultUtils.PlayNotification("Failed to add SIN currency");
        }
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

    [CheatDetails("Reset DLC Dungeon", "Resets the DLC dungeon map progress (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void ResetDlcMap(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            DataManager.Instance.DLCDungeonNodesCompleted.Clear();
            CultUtils.PlayNotification("DLC dungeon progress reset!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to reset DLC map: {e.Message}");
            CultUtils.PlayNotification("Failed to reset DLC map!");
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

    [CheatDetails("Finish All Missions", "Instantly completes all follower missionary tasks (Major DLC required)")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void FinishAllMissions(){
        if(!HasMajorDLC()){ CultUtils.PlayNotification("Requires Major DLC!"); return; }
        try {
            Traverse.Create(typeof(CheatConsole)).Method("FinishAllMissions").GetValue();
            CultUtils.PlayNotification("All missions completed!");
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

}
