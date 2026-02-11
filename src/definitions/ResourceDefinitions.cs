using System;
using System.Collections.Generic;
using System.Reflection;
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
        UnityEngine.Debug.Log("hi");
        CultUtils.GiveDocterineStone();
    }

    [CheatDetails("Give Monster Heart", "Gives a heart of the heretic")]
    public static void GiveMonsterHeart(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MONSTER_HEART, 10);
        CultUtils.PlayNotification("Monster hearts added!");
    }

    [CheatDetails("Give Food", "Gives all farming based foods")]
    public static void GiveFarmingFood(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CAULIFLOWER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BERRY, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BEETROOT, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAT, 10);
        CultUtils.PlayNotification("Food added to inventory!");
    }

    [CheatDetails("Give Fish", "Gives all types of fish (x10)")]
    public static void GiveFish(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BIG, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CRAB, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BLOWFISH, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_LOBSTER, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_OCTOPUS, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SQUID, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SWORDFISH, 10);
        CultUtils.PlayNotification("Fish added to inventory!");
    }

    [CheatDetails("Give Fertiziler", "Gives x100 Fertiziler (Poop)")]
    public static void GivePoop(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, 100);
        CultUtils.PlayNotification("Fertilizer added!");
    }

    [CheatDetails("Give Follower Meat", "Gives x10 Follower Meat")]
    public static void GiveFollowerMeat(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 10);
        CultUtils.PlayNotification("Follower meat added!");
    }

    [CheatDetails("Give Follower Necklaces", "Gives one of each of the various follower necklaces")]
    public static void GiveGifts(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_1, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_2, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_3, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_4, 1);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.Necklace_5, 1);
        CultUtils.PlayNotification("Necklaces added!");
    }

    [CheatDetails("Give Small Gift", "Gives you a 'small' gift x10")]
    public static void GiveSmallGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_SMALL, 10);
        CultUtils.PlayNotification("Small gifts added!");
    }

    [CheatDetails("Give Big Gift", "Gives you a 'big' gift x10")]
    public static void GiveBigGift(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GIFT_MEDIUM, 10);
        CultUtils.PlayNotification("Big gifts added!");
    }

    [CheatDetails("Give Gold Coins", "Gives x500 Gold Coins")]
    public static void GiveGoldCoins(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 500);
        CultUtils.PlayNotification("500 gold coins added!");
    }

    [CheatDetails("Give Seeds", "Gives x10 of common seed types")]
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
            UnityEngine.Debug.LogWarning($"Failed to add seeds: {e.Message}");
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
}