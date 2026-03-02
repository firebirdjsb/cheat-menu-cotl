using System;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.RESOURCE)]
public class ResourceDefinitions : IDefinition{

    // Controlled by the Item Qty slider in the Resources category menu
    public static int ItemSpawnQty = 1;

    [CheatDetails("Give Resources", "Gives 100 of the main primary resources", subGroup: "Currency")]
    public static void GiveResources(){
        Traverse.Create(typeof(CheatConsole)).Method("GiveResources").GetValue();
        CultUtils.PlayNotification("Resources added!");
    }

    [CheatDetails("Give Commandment Stone", "Gives a Commandment Stone", subGroup: "Currency")]
    public static void GiveCommandmentStone(){
        CultUtils.GiveDocterineStone();
    }

    [CheatDetails("Give Monster Heart", "Gives hearts of the heretic (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveMonsterHeart(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MONSTER_HEART, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x monster hearts added!");
    }

    [CheatDetails("Give Food", "Gives qty of all farming-based foods (qty controlled by slider)", subGroup: "Food")]
    public static void GiveFarmingFood(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CAULIFLOWER, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BERRY, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BEETROOT, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAT, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x food added to inventory!");
    }

    [CheatDetails("Give All Fish", "Gives qty of all fish types (qty controlled by slider)", subGroup: "Food")]
    public static void GiveAllFish(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BIG, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CRAB, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_LOBSTER, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_OCTOPUS, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SQUID, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SWORDFISH, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BLOWFISH, ItemSpawnQty);
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_COD, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_PIKE, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CATFISH, ItemSpawnQty);
            }
        } catch { }
        CultUtils.PlayNotification($"{ItemSpawnQty}x of all fish added!");
    }

    [CheatDetails("Give Fertilizer", "Gives qty Fertilizer/Poop (qty controlled by slider)", subGroup: "Food")]
    public static void GivePoop(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x fertilizer added!");
    }

    [CheatDetails("Give Follower Meat", "Gives qty Follower Meat (qty controlled by slider)", subGroup: "Food")]
    public static void GiveFollowerMeat(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x follower meat added!");
    }

    [CheatDetails("Give All Necklaces", "Gives one of every necklace type in the game", subGroup: "Gifts & More")]
    public static void GiveAllNecklaces(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_1, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_2, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_3, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_4, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_5, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Loyalty, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Demonic, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Dark, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Light, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Missionary, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Gold_Skull, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Bell, ItemSpawnQty);
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Deaths_Door, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Winter, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Frozen, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Weird, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Targeted, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DLC_NECKLACE, ItemSpawnQty);
            }
        } catch { }
        CultUtils.PlayNotification($"{ItemSpawnQty}x of all necklaces added!");
    }

    [CheatDetails("Give Small Gift", "Gives qty small gifts (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveSmallGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_SMALL, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x small gifts added!");
    }

    [CheatDetails("Give Big Gift", "Gives qty big gifts (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveBigGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_MEDIUM, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x big gifts added!");
    }

    [CheatDetails("Give Gold Coins", "Gives qty Gold Coins (qty controlled by slider)", subGroup: "Currency")]
    public static void GiveGoldCoins(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x gold coins added!");
    }

    [CheatDetails("Give All Seeds", "Gives x10 of every seed type in the game (DLC seeds require Woolhaven)", subGroup: "Seeds & Plants")]
    public static void GiveSeeds(){
        try {
            int addedCount = 0;
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                string itemName = itemType.ToString();
                if(itemName.StartsWith("SEED")){
                    if(!hasMajorDLC && (itemName.Contains("HOPS") || itemName.Contains("GRAPES") || itemName.Contains("COTTON") || itemName.Contains("SOZO") || itemName.Contains("SNOW_FRUIT") || itemName.Contains("CHILLI"))) continue;
                    CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)itemType, ItemSpawnQty);
                    addedCount++;
                }
            }
            CultUtils.PlayNotification($"{ItemSpawnQty}x seeds added ({addedCount} types)!" + (!hasMajorDLC ? " (DLC seeds skipped)" : ""));
        } catch(Exception e){
            Debug.LogWarning($"Failed to add seeds: {e.Message}");
            CultUtils.PlayNotification("Failed to add some seeds!");
        }
    }

    [CheatDetails("Give Crystals", "Gives qty Crystal Shards (qty controlled by slider)", subGroup: "Materials")]
    public static void GiveCrystals(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CRYSTAL, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x crystals added!");
    }

    [CheatDetails("Give Bones", "Gives qty Bones (qty controlled by slider)", subGroup: "Materials")]
    public static void GiveBones(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x bones added!");
    }

    [CheatDetails("Give Lumber", "Gives qty Lumber (qty controlled by slider)", subGroup: "Materials")]
    public static void GiveLumber(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LOG, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x lumber added!");
    }

    [CheatDetails("Give Stone", "Gives qty Stone (qty controlled by slider)", subGroup: "Materials")]
    public static void GiveStone(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.STONE, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x stone added!");
    }

    [CheatDetails("Give Silk Thread", "Gives x50 Silk Thread", subGroup: "Materials")]
    public static void GiveSilk(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SILK_THREAD, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x silk thread added!");
    }

    [CheatDetails("Give God Tears", "Gives qty God Tears (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveGodTears(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOD_TEAR, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x god tears added!");
    }

    [CheatDetails("Give Relics", "Gives qty Relics (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveRelicsItem(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RELIC, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x relics added!");
    }

    [CheatDetails("Give Talisman", "Gives qty Talisman pieces (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveTalisman(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.TALISMAN, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x talisman pieces added!");
    }

    [CheatDetails("Give Trinket Cards", "Gives qty Trinket Cards (qty controlled by slider)", subGroup: "Gifts & More")]
    public static void GiveTrinketCards(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.TRINKET_CARD, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x trinket cards added!");
    }

    [CheatDetails("Give Flowers", "Gives qty of each flower type (qty controlled by slider)", subGroup: "Seeds & Plants")]
    public static void GiveFlowers(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWERS, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_RED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_WHITE, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_PURPLE, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x of each flower added!");
    }

    [CheatDetails("Give All Meals", "Gives qty of every cooked meal type (qty controlled by slider)", subGroup: "Food")]
    public static void GiveAllMeals(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GRASS, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEAT, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_BERRIES, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT, ItemSpawnQty);
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_SPICY, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_EGG, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_SNOW_FRUIT, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_BAD, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_GOOD, ItemSpawnQty);
                CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_GREAT, ItemSpawnQty);
            }
        } catch { }
        CultUtils.PlayNotification($"{ItemSpawnQty}x of all meals added!");
    }

    [CheatDetails("Give Refined Materials", "Gives qty refined lumber, stone, gold, nuggets, rope (qty controlled by slider)", subGroup: "Materials")]
    public static void GiveRefinedMaterials(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LOG_REFINED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.STONE_REFINED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOLD_REFINED, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOLD_NUGGET, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ROPE, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x refined materials added!");
    }

    [CheatDetails("Give Ability Points", "Gives qty upgrade ability points (qty controlled by slider)", subGroup: "Currency")]
    public static void GiveAbilityPoints(){
        UpgradeSystem.AbilityPoints += ItemSpawnQty;
        CultUtils.PlayNotification($"{ItemSpawnQty} ability points added!");
    }

    [CheatDetails("Give Disciple Points", "Gives qty disciple ability points (qty controlled by slider)", subGroup: "Currency")]
    public static void GiveDisciplePoints(){
        UpgradeSystem.DisciplePoints += ItemSpawnQty;
        CultUtils.PlayNotification($"{ItemSpawnQty} disciple points added!");
    }

    [CheatDetails("Give Souls", "Gives qty souls to the Player (qty controlled by slider)", subGroup: "Currency")]
    public static void GiveSouls(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.GetSoul(ItemSpawnQty);
            CultUtils.PlayNotification($"{ItemSpawnQty} souls added!");
        }
    }

    [CheatDetails("Give Black Souls", "Gives qty black souls (sin currency, qty controlled by slider)", subGroup: "Currency")]
    public static void GiveBlackSouls(){
        try {
            DataManager.Instance.BlackSouls += ItemSpawnQty;
            if(PlayerFarming.Instance != null){
                PlayerFarming.Instance.GetBlackSoul(ItemSpawnQty, true, true);
            }
            CultUtils.PlayNotification($"{ItemSpawnQty} black souls added!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] GiveBlackSouls error: {e.Message}");
            CultUtils.PlayNotification("Failed to add black souls!");
        }
    }

    [CheatDetails("Give Arrows", "Gives arrows / unlocks arrow ability", subGroup: "Currency")]
    public static void GiveArrows(){
        if(!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Combat_Arrows)){
            UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Combat_Arrows, false);
        }
        DataManager.Instance.PLAYER_ARROW_AMMO = ItemSpawnQty;
        CultUtils.PlayNotification($"{ItemSpawnQty}x arrows added!");
    }

    [CheatDetails("Give All Items", "Gives qty of every single item type in the game (DLC items require ownership)")]
    public static void GiveAllItems(){
        try {
            int addedCount = 0;
            int skippedDlc = 0;
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)itemType;
                if(type == InventoryItem.ITEM_TYPE.NONE) continue;
                string itemName = type.ToString();
                if(!hasMajorDLC && (itemName.Contains("DLC") || itemName.Contains("FORGE_FLAME") || itemName.Contains("MAGMA") || itemName.Contains("ELECTRIFIED") || itemName.Contains("LIGHTNING_SHARD") || itemName.Contains("CHARCOAL") || itemName.Contains("SOOT") || itemName.Contains("BROKEN_WEAPON") || itemName.Contains("LEGENDARY_WEAPON") || itemName.Contains("FLOCKADE") || itemName.Contains("RATAU_STAFF") || itemName.Contains("WEBBER_SKULL") || itemName.Contains("ILLEGIBLE_LETTER") || itemName.Contains("LORE_STONE") || itemName.Contains("SPECIAL_WOOL") || itemName.Contains("ANIMAL_") || itemName.Contains("YOLK") || itemName.Contains("EGG_FOLLOWER"))){
                    skippedDlc++;
                    continue;
                }
                try {
                    CultUtils.AddInventoryItem(type, ItemSpawnQty);
                    addedCount++;
                } catch { }
            }
            string msg = $"{ItemSpawnQty}x of all items added ({addedCount} types)!";
            if(skippedDlc > 0) msg += $" ({skippedDlc} DLC items skipped)";
            CultUtils.PlayNotification(msg);
        } catch(Exception e){
            Debug.LogWarning($"Failed to add all items: {e.Message}");
            CultUtils.PlayNotification("Failed to add some items!");
        }
    }
}