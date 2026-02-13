using System;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.RESOURCE)]
public class ResourceDefinitions : IDefinition{

    [CheatDetails("Give Resources", "Gives 100 of the main primary resources")]
    public static void GiveResources(){
        Traverse.Create(typeof(CheatConsole)).Method("GiveResources").GetValue();
        CultUtils.PlayNotification("Resources added!");
    }

    [CheatDetails("Give Commandment Stone", "Gives a Commandment Stone")]
    public static void GiveCommandmentStone(){
        CultUtils.GiveDocterineStone();
    }

    [CheatDetails("Give Monster Heart", "Gives x10 hearts of the heretic")]
    public static void GiveMonsterHeart(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MONSTER_HEART, 10);
        CultUtils.PlayNotification("Monster hearts added!");
    }

    [CheatDetails("Give Food", "Gives x10 of all farming-based foods")]
    public static void GiveFarmingFood(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CAULIFLOWER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BERRY, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BEETROOT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAT, 10);
        CultUtils.PlayNotification("Food added to inventory!");
    }

    [CheatDetails("Give All Fish", "Gives x10 of all fish types including DLC")]
    public static void GiveAllFish(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BIG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CRAB, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_LOBSTER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_OCTOPUS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SQUID, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SWORDFISH, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BLOWFISH, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_COD, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_PIKE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CATFISH, 10);
        CultUtils.PlayNotification("All fish added!");
    }

    [CheatDetails("Give Fertilizer", "Gives x100 Fertilizer (Poop)")]
    public static void GivePoop(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, 100);
        CultUtils.PlayNotification("Fertilizer added!");
    }

    [CheatDetails("Give Follower Meat", "Gives x10 Follower Meat")]
    public static void GiveFollowerMeat(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 10);
        CultUtils.PlayNotification("Follower meat added!");
    }

    [CheatDetails("Give All Necklaces", "Gives one of every necklace type in the game")]
    public static void GiveAllNecklaces(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_1, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_2, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_3, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_4, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_5, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Loyalty, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Demonic, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Dark, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Light, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Missionary, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Gold_Skull, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Bell, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Deaths_Door, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Winter, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Frozen, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Weird, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_Targeted, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DLC_NECKLACE, 1);
        CultUtils.PlayNotification("All necklaces added!");
    }

    [CheatDetails("Give Small Gift", "Gives x10 small gifts")]
    public static void GiveSmallGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_SMALL, 10);
        CultUtils.PlayNotification("Small gifts added!");
    }

    [CheatDetails("Give Big Gift", "Gives x10 big gifts")]
    public static void GiveBigGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_MEDIUM, 10);
        CultUtils.PlayNotification("Big gifts added!");
    }

    [CheatDetails("Give Gold Coins", "Gives x500 Gold Coins")]
    public static void GiveGoldCoins(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 500);
        CultUtils.PlayNotification("500 gold coins added!");
    }

    [CheatDetails("Give All Seeds", "Gives x10 of every seed type in the game")]
    public static void GiveSeeds(){
        try {
            int addedCount = 0;
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                string itemName = itemType.ToString();
                if(itemName.StartsWith("SEED")){
                    CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)itemType, 10);
                    addedCount++;
                }
            }
            CultUtils.PlayNotification($"Seeds added ({addedCount} types)!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to add seeds: {e.Message}");
            CultUtils.PlayNotification("Failed to add some seeds!");
        }
    }

    [CheatDetails("Give Crystals", "Gives x50 Crystal Shards")]
    public static void GiveCrystals(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CRYSTAL, 50);
        CultUtils.PlayNotification("Crystals added!");
    }

    [CheatDetails("Give Bones", "Gives x50 Bones")]
    public static void GiveBones(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, 50);
        CultUtils.PlayNotification("Bones added!");
    }

    [CheatDetails("Give Lumber", "Gives x100 Lumber")]
    public static void GiveLumber(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LOG, 100);
        CultUtils.PlayNotification("Lumber added!");
    }

    [CheatDetails("Give Stone", "Gives x100 Stone")]
    public static void GiveStone(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.STONE, 100);
        CultUtils.PlayNotification("Stone added!");
    }

    [CheatDetails("Give Silk Thread", "Gives x50 Silk Thread")]
    public static void GiveSilk(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SILK_THREAD, 50);
        CultUtils.PlayNotification("Silk thread added!");
    }

    [CheatDetails("Give God Tears", "Gives x10 God Tears")]
    public static void GiveGodTears(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOD_TEAR, 10);
        CultUtils.PlayNotification("God tears added!");
    }

    [CheatDetails("Give Relics", "Gives x10 Relics")]
    public static void GiveRelicsItem(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RELIC, 10);
        CultUtils.PlayNotification("Relics added!");
    }

    [CheatDetails("Give Talisman", "Gives x10 Talisman pieces")]
    public static void GiveTalisman(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.TALISMAN, 10);
        CultUtils.PlayNotification("Talisman pieces added!");
    }

    [CheatDetails("Give Trinket Cards", "Gives x10 Trinket Cards")]
    public static void GiveTrinketCards(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.TRINKET_CARD, 10);
        CultUtils.PlayNotification("Trinket cards added!");
    }

    [CheatDetails("Give Flowers", "Gives x10 of each flower type")]
    public static void GiveFlowers(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWERS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_RED, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_WHITE, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOWER_PURPLE, 10);
        CultUtils.PlayNotification("Flowers added!");
    }

    [CheatDetails("Give All Meals", "Gives x10 of every cooked meal type")]
    public static void GiveAllMeals(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GRASS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEAT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_BERRIES, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_SPICY, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_EGG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_SNOW_FRUIT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_BAD, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_GOOD, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_MILK_GREAT, 10);
        CultUtils.PlayNotification("All meals added!");
    }

    [CheatDetails("Give Refined Materials", "Gives x50 refined lumber, stone, gold, nuggets, rope")]
    public static void GiveRefinedMaterials(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LOG_REFINED, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.STONE_REFINED, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOLD_REFINED, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GOLD_NUGGET, 50);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ROPE, 50);
        CultUtils.PlayNotification("Refined materials added!");
    }

    [CheatDetails("Give Ability Points", "Gives 10 upgrade ability points")]
    public static void GiveAbilityPoints(){
        UpgradeSystem.AbilityPoints += 10;
        CultUtils.PlayNotification("10 ability points added!");
    }

    [CheatDetails("Give Disciple Points", "Gives 10 disciple ability points")]
    public static void GiveDisciplePoints(){
        UpgradeSystem.DisciplePoints += 10;
        CultUtils.PlayNotification("10 disciple points added!");
    }

    [CheatDetails("Give 100 Souls", "Gives 100 souls to the Player")]
    public static void GiveSouls(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.GetSoul(100);
            CultUtils.PlayNotification("100 souls added!");
        }
    }

    [CheatDetails("Give 200 Black Souls", "Gives 200 black souls (sin currency)")]
    public static void GiveBlackSouls(){
        try {
            DataManager.Instance.BlackSouls += 200;
            if(PlayerFarming.Instance != null){
                PlayerFarming.Instance.GetBlackSoul(200, true, true);
            }
            CultUtils.PlayNotification("200 black souls added!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] GiveBlackSouls error: {e.Message}");
            CultUtils.PlayNotification("Failed to add black souls!");
        }
    }

    [CheatDetails("Give Arrows", "Gives arrows / unlocks arrow ability")]
    public static void GiveArrows(){
        if(!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Combat_Arrows)){
            UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Combat_Arrows, false);
        }
        DataManager.Instance.PLAYER_ARROW_AMMO = 99;
        CultUtils.PlayNotification("Arrows added!");
    }

    [CheatDetails("Give All Items", "Gives x10 of every single item type in the game")]
    public static void GiveAllItems(){
        try {
            int addedCount = 0;
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)itemType;
                if(type == InventoryItem.ITEM_TYPE.NONE) continue;
                try {
                    CultUtils.AddInventoryItem(type, 10);
                    addedCount++;
                } catch { }
            }
            CultUtils.PlayNotification($"All items added ({addedCount} types)!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to add all items: {e.Message}");
            CultUtils.PlayNotification("Failed to add some items!");
        }
    }
}