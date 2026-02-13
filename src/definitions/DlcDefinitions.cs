using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.DLC)]
public class DlcDefinitions : IDefinition{

    [CheatDetails("Give DLC Seeds", "Gives x20 of all DLC farming seeds")]
    public static void GiveDlcSeeds(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_PUMPKIN, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_BEETROOT, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_MUSHROOM, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_FLOWER_WHITE, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_FLOWER_RED, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_FLOWER_PURPLE, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_TREE, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_HOPS, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_GRAPES, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_COTTON, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SOZO, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SNOW_FRUIT, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_CHILLI, 20);
        CultUtils.PlayNotification("DLC seeds added!");
    }

    [CheatDetails("Give All Drinks", "Gives x10 of every drink type (DLC Brewing)")]
    public static void GiveAllDlcDrinks(){
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

    [CheatDetails("Give Brewing Ingredients", "Gives x30 hops, grapes, chilli, cotton, snow fruit, milk")]
    public static void GiveBrewingCrops(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.HOPS, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GRAPES, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHILLI, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SNOW_FRUIT, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 30);
        CultUtils.PlayNotification("Brewing ingredients added!");
    }

    [CheatDetails("Give All Forge Materials", "Gives forge flames, magma, lightning shards, soot, charcoal")]
    public static void GiveAllForgeMaterials(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FORGE_FLAME, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ELECTRIFIED_MAGMA, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LIGHTNING_SHARD, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
        CultUtils.PlayNotification("All forge materials added!");
    }

    [CheatDetails("Give Sin Items Bundle", "Gives sin drinks, soot, sozo seeds, charcoal, magma stones")]
    public static void GiveSinItemsBundle(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_SIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SOZO, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 20);
        CultUtils.PlayNotification("Sin items bundle added!");
    }

    [CheatDetails("Give Broken Weapons", "Gives one of each broken weapon for repair")]
    public static void GiveBrokenWeapons(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_HAMMER, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_SWORD, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_DAGGER, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_GAUNTLETS, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_AXE, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_BLUNDERBUSS, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_CHAIN, 1);
        CultUtils.PlayNotification("Broken weapons added!");
    }

    [CheatDetails("Give Legendary Weapon Fragments", "Gives x10 Legendary Weapon Fragments")]
    public static void GiveLegendaryFragments(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LEGENDARY_WEAPON_FRAGMENT, 10);
        CultUtils.PlayNotification("Legendary fragments added!");
    }

    [CheatDetails("Give DLC Collectibles", "Gives rot eyes, lore stones, illegible letters, weapon fragments")]
    public static void GiveDlcCollectibles(){
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

    [CheatDetails("Give DLC Necklace", "Gives the DLC exclusive necklace")]
    public static void GiveDlcNecklace(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DLC_NECKLACE, 1);
        CultUtils.PlayNotification("DLC necklace added!");
    }

    [CheatDetails("Give Flockade Pieces", "Gives x5 Flockade building pieces")]
    public static void GiveFlockadePieces(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOCKADE_PIECE, 5);
        CultUtils.PlayNotification("Flockade pieces added!");
    }

    [CheatDetails("Give Webber Skull & Ratau Staff", "Gives key story collectibles")]
    public static void GiveStoryCollectibles(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WEBBER_SKULL, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RATAU_STAFF, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_SCYLLA, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_CHARYBDIS, 1);
        CultUtils.PlayNotification("Story collectibles added!");
    }

    [CheatDetails("Reset DLC Dungeon", "Resets the DLC dungeon map progress")]
    public static void ResetDlcMap(){
        try {
            DataManager.Instance.DLCDungeonNodesCompleted.Clear();
            CultUtils.PlayNotification("DLC dungeon progress reset!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to reset DLC map: {e.Message}");
            CultUtils.PlayNotification("Failed to reset DLC map!");
        }
    }

    [CheatDetails("Empty Furnace Fuel", "Empties the DLC furnace fuel")]
    public static void EmptyFurnaceFuel(){
        try {
            Traverse.Create(typeof(CheatConsole)).Method("EmptyFurnaceFuel").GetValue();
            CultUtils.PlayNotification("Furnace fuel emptied!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to empty furnace: {e.Message}");
            CultUtils.PlayNotification("Failed to empty furnace!");
        }
    }

    [CheatDetails("Fill Furnace Fuel", "Fills all DLC furnaces to max fuel")]
    public static void FillFurnaceFuel(){
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

    [CheatDetails("Make Spies Leave", "Forces all spy followers to leave your cult")]
    public static void MakeSpiesLeave(){
        try {
            Traverse.Create(typeof(CheatConsole)).Method("MakeSpiesLeave").GetValue();
            CultUtils.PlayNotification("Spies removed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to remove spies: {e.Message}");
            CultUtils.PlayNotification("Failed to remove spies!");
        }
    }

    [CheatDetails("Finish All Missions", "Instantly completes all follower missionary tasks")]
    public static void FinishAllMissions(){
        try {
            Traverse.Create(typeof(CheatConsole)).Method("FinishAllMissions").GetValue();
            CultUtils.PlayNotification("All missions completed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to finish missions: {e.Message}");
            CultUtils.PlayNotification("Failed to finish missions!");
        }
    }

    [CheatDetails("Unlock Winter Mode", "Unlocks the winter survival game mode")]
    public static void UnlockWinterMode(){
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
