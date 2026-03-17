using System;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

/// <summary>
/// Definition class containing cheats for giving resources and items.
/// Provides cheat methods for adding various item types to player inventory.
/// </summary>
/// <remarks>
/// Uses a slider-controlled quantity system (ItemSpawnQty) for flexible item amounts.
/// Most DLC-specific items are gated behind ownership checks.
/// </remarks>
[CheatCategory(CheatCategoryEnum.RESOURCE)]
public class ResourceDefinitions : IDefinition{

    // Controlled by the Item Qty slider in the Resources category menu
    /// <summary>
    /// Quantity multiplier for item spawn cheats. Controlled by UI slider.
    /// </summary>
    public static int ItemSpawnQty = 1;

    [CheatDetails("Give Resources", "Gives 100 of the main primary resources (Woolhaven DLC resources require ownership)", subGroup: "Materials")]
    public static void GiveResources(){
        // Give base game resources
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LOG, 100);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.STONE, 100);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, 100);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAT, 100);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BERRY, 100);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, 100);
        
        // Only give Woolhaven resources if player owns the DLC
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
                // Add any Woolhaven-specific resources here if needed
            }
        } catch { }
        
        CultUtils.PlayNotification("Resources added!" + (CultUtils.HasMajorDLC() ? "" : " (Woolhaven resources skipped)"));
    }

    [CheatDetails("Give Commandment Stone", "Gives a Commandment Stone", subGroup: "Currency")]
    public static void GiveCommandmentStone(){
        CultUtils.GiveDoctrineStone();
    }

    [CheatDetails("Give Forgotten Commandment Stone", "Gives a Forgotten Commandment Stone (Crystal Doctrine Stone)", subGroup: "Currency")]
    public static void GiveForgottenCommandmentStone(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x Forgotten Commandment Stones added!");
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

    [CheatDetails("Give All Fish", "Gives qty of all fish types (qty controlled by slider, DLC fish require ownership)", subGroup: "Food")]
    public static void GiveAllFish(){
        // Base game fish
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BIG, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_CRAB, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_LOBSTER, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_OCTOPUS, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SQUID, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_SWORDFISH, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FISH_BLOWFISH, ItemSpawnQty);
        
        // DLC fish (Woolhaven) - only add if player owns the DLC
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
                // Add any Woolhaven-specific fish here if needed
            }
        } catch { }
        
        CultUtils.PlayNotification($"{ItemSpawnQty}x of all fish added!" + (CultUtils.HasMajorDLC() ? "" : " (DLC fish skipped)"));
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

    [CheatDetails("Give All Necklaces", "Gives one of every necklace type in the game (Woolhaven DLC necklaces require ownership)", subGroup: "Gifts & More")]
    public static void GiveAllNecklaces(){
        // Base game necklaces (no DLC required)
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
        
        // Woolhaven DLC necklaces - require Major DLC
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
        CultUtils.PlayNotification($"{ItemSpawnQty}x of all necklaces added!" + (CultUtils.HasMajorDLC() ? "" : " (Woolhaven necklaces skipped)"));
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

    [CheatDetails("Give All Seeds", "Gives x10 of every seed type in the game (Woolhaven seeds require DLC)", subGroup: "Seeds & Plants")]
    public static void GiveSeeds(){
        try {
            int addedCount = 0;
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                string itemName = itemType.ToString();
                if(itemName.StartsWith("SEED")){
                    // Only filter SNOW_FRUIT and CHILLI seeds as they are Woolhaven DLC
                    if(!hasMajorDLC && (itemName.Contains("SNOW_FRUIT") || itemName.Contains("CHILLI"))) continue;
                    CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)itemType, ItemSpawnQty);
                    addedCount++;
                }
            }
            CultUtils.PlayNotification($"{ItemSpawnQty}x seeds added ({addedCount} types)!" + (!hasMajorDLC ? " (Woolhaven seeds skipped)" : ""));
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

    [CheatDetails("Give Grass", "Gives qty Grass (qty controlled by slider)", subGroup: "Seeds & Plants")]
    public static void GiveGrass(){
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GRASS, ItemSpawnQty);
        CultUtils.PlayNotification($"{ItemSpawnQty}x grass added!");
    }

    [CheatDetails("Give All Meals", "Gives qty of every cooked meal type (qty controlled by slider)", subGroup: "Food")]
    public static void GiveAllMeals(){
        // Base game and free update meals
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
        // Free update meals (Sins of the Flesh - Jan 2024)
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_SPICY, ItemSpawnQty);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MEAL_EGG, ItemSpawnQty);
        // Woolhaven DLC meals (require Major DLC)
        try {
            if(CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC){
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

    // DISABLED: This doesn't work properly - commenting out
    // [CheatDetails("Give Disciple Points", "Gives qty disciple ability points (qty controlled by slider)", subGroup: "Currency")]
    // public static void GiveDisciplePoints(){
    //     UpgradeSystem.DisciplePoints += ItemSpawnQty;
    //     CultUtils.PlayNotification($"{ItemSpawnQty} disciple points added!");
    // }

    [CheatDetails("Give Souls", "Gives qty souls to the Player (qty controlled by slider)", subGroup: "Currency")]
    public static void GiveSouls(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.GetSoul(ItemSpawnQty);
            CultUtils.PlayNotification($"{ItemSpawnQty} souls added!");
        }
    }

    // DISABLED: Black Souls don't work properly - commenting out
    // [CheatDetails("Give Black Souls", "Gives qty black souls (sin currency, qty controlled by slider)", subGroup: "Currency")]
    // public static void GiveBlackSouls(){
    //     try {
    //         DataManager.Instance.BlackSouls += ItemSpawnQty;
    //         if(PlayerFarming.Instance != null){
    //             PlayerFarming.Instance.GetBlackSoul(ItemSpawnQty, true, true);
    //         }
    //         CultUtils.PlayNotification($"{ItemSpawnQty} black souls added!");
    //     } catch(Exception e){
    //         Debug.LogWarning($"[CheatMenu] GiveBlackSouls error: {e.Message}");
    //         CultUtils.PlayNotification("Failed to add black souls!");
    //     }
    // }

    [CheatDetails("Give Pleasure Points", "Gives qty pleasure points (sin currency, qty controlled by slider)", subGroup: "Currency")]
    public static void GivePleasurePoints(){
        try {
            CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.PLEASURE_POINT, ItemSpawnQty);
            CultUtils.PlayNotification($"{ItemSpawnQty} pleasure points added!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] GivePleasurePoints error: {e.Message}");
            CultUtils.PlayNotification("Failed to add pleasure points!");
        }
    }

    [CheatDetails("Give All Items", "Gives qty of every single item type in the game (DLC items require ownership)")]
    public static void GiveAllItems(){
        try {
            int addedCount = 0;
            int skippedDlc = 0;
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            bool hasSinfulDLC = CultUtils.HasSinfulDLC();
            bool hasHereticDLC = CultUtils.HasHereticDLC();
            bool hasPilgrimDLC = CultUtils.HasPilgrimDLC();
            
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)itemType;
                if(type == InventoryItem.ITEM_TYPE.NONE) continue;
                string itemName = type.ToString().ToUpperInvariant();
                
                // Skip souls (handled by GiveSouls/GiveBlackSouls) and fleeces
                // Note: SOUL_FRAGMENT is now in the PaidDLC_Woolhaven check below
                if((itemName.Contains("SOUL") && !itemName.Contains("FRAGMENT")) || itemName.Contains("FLEECE")) continue;
                
                // YNGYA_GHOST is a quest item that can cause softlocks - never give it
                if(itemName.Contains("YNGYA_GHOST")) continue;
                
                // Special wools (SPECIAL_WOOL_RANCHER, SPECIAL_WOOL_LAMBWAR, etc.) are campaign/quest items - NEVER give them
                if(itemName.Contains("SPECIAL_WOOL")) continue;
                
                // FILTER: FLOWERS (Sacred Flower/Sacred Flame) - campaign/quest item, never give it
                if(itemName.Contains("FLOWERS")) continue;
                
                // FILTER: FORGE_FLAME (Sacred Flame) - campaign/quest item, never give it
                if(itemName.Contains("FORGE_FLAME")) continue;
                
                // FILTER: FLEECE items - these are campaign/quest rewards (Fleece1-11 correspond to SPECIAL_WOOL_*)
                // The SPECIAL_WOOL filter above catches the enum names, but add explicit filter for clarity
                if(itemName.Contains("FLEECE")) continue;
                
                // Never give quest/campaign items that add to "never spawn list"
                if(CultUtils.ShouldNeverGiveItem(itemName)) continue;
                
                // FILTER: Never spawn these items - they are special/meta items that shouldn't be given
                // RATAU_STAFF - quest item
                // BOP - DLC quest item
                // FOUND_ITEM_DECORATION_ALT, FOUND_ITEM_DECORATION - special decoration items
                // UNUSED - unused items in the enum
                // DISCIPLE_POINTS - special currency (not actual inventory item)
                // TRINKET_CARD_UNLOCKED - special unlock item
                // PERMANENT_HALF_HEART, BLACK_HEART - special health items
                // FOUND_ITEM_WEAPON, FOUND_ITEM_CURSE - special discovery items
                // RED_HEART, HALF_HEART, BLUE_HEART, HALF_BLUE_HEART - special heart items
                // TIME_TOKEN - special time item
                // GENERIC - generic placeholder item
                if(itemName.Contains("RATAU_STAFF") || 
                   itemName.Contains("BOP") ||
                   itemName.Contains("FOUND_ITEM_DECORATION") ||
                   itemName.Contains("UNUSED") ||
                   itemName.Contains("DISCIPLE_POINTS") ||
                   itemName.Contains("TRINKET_CARD_UNLOCKED") ||
                   itemName.Contains("PERMANENT_HALF_HEART") ||
                   itemName.Contains("BLACK_HEART") ||
                   itemName.Contains("FOUND_ITEM_WEAPON") ||
                   itemName.Contains("FOUND_ITEM_CURSE") ||
                   itemName.Contains("RED_HEART") ||
                   itemName.Contains("HALF_HEART") ||
                   itemName.Contains("BLUE_HEART") ||
                   itemName.Contains("TIME_TOKEN") ||
                   itemName.Contains("GENERIC")) continue;
                
                // Check DLC ownership for each item
                bool shouldSkip = false;
                
                // COD, PIKE, CATFISH - always skip (Woolhaven DLC exclusive fish)
                if(itemName.Contains("COD") || itemName.Contains("PIKE") || itemName.Contains("CATFISH")){
                    shouldSkip = true;
                }
                
                // Check DLC ownership for each item
                if(!shouldSkip && !hasMajorDLC && (
                    // Ranch animals
                    itemName.Contains("ANIMAL_") ||
                    // Wool and Milk (Woolhaven)
                    itemName == "WOOL" ||
                    itemName == "MILK" ||
                    itemName.Contains("MEAL_MILK") ||
                    // Special wools (Woolhaven) - NOW MOVED ABOVE AS UNCONDITIONAL SKIP
                    // Cold weather crops (Woolhaven)
                    itemName.Contains("SNOW_FRUIT") ||
                    itemName.Contains("CHILLI") ||
                    // Purple and White flowers (Forget-me-not, Snowdrop - Woolhaven DLC)
                    itemName.Contains("FLOWER_PURPLE") ||
                    itemName.Contains("FLOWER_WHITE") ||
                    itemName.Contains("SEED_FLOWER_PURPLE") ||
                    itemName.Contains("SEED_FLOWER_WHITE") ||
                    // New resources (Woolhaven)
                    itemName.Contains("SOOT") ||
                    itemName.Contains("FORGE_FLAME") ||
                    // Rotburn fertilizer and currency (Woolhaven)
                    itemName.Contains("POOP_ROTSTONE") ||
                    itemName.Contains("ROTBURN") ||
                    // Calcified rot (Woolhaven DLC structure unlock)
                    itemName.Contains("CALCIFIED") ||
                    // Rotten eye of the witness (Woolhaven DLC story item)
                    itemName.Contains("BEHOLDER_EYE_ROT") ||
                    // DLC fish (Cod, Pike, Catfish - Woolhaven)
                    itemName.Contains("COD") ||
                    itemName.Contains("PIKE") ||
                    itemName.Contains("CATFISH") ||
                    // Lightning Shard (Woolhaven)
                    itemName.Contains("LIGHTNING_SHARD") ||
                    // Charged shard (Woolhaven)
                    itemName.Contains("ELECTRIFIED_MAGMA") ||
                    // Magma Stone (Woolhaven)
                    itemName.Contains("MAGMA_STONE") ||
                    // Snow chunk (Woolhaven)
                    itemName.Contains("SNOW_CHUNK") ||
                    // Legendary weapons (Woolhaven)
                    itemName.Contains("BROKEN_WEAPON") ||
                    itemName.Contains("REPAIRED_WEAPON") ||
                    itemName.Contains("LEGENDARY_WEAPON") ||
                    // DLC story items (Woolhaven)
                    itemName.Contains("FLOCKADE") ||
                    itemName.Contains("YEW_CURSED") ||
                    itemName.Contains("YEW_HOLY") ||
                    itemName.Contains("BOP") ||
                    itemName.Contains("ILLEGIBLE_LETTER") ||
                    itemName.Contains("FISHING_ROD") ||
                    // DLC necklaces (Woolhaven)
                    itemName.Contains("NECKLACE_DEATHS_DOOR") ||
                    itemName.Contains("NECKLACE_WINTER") ||
                    itemName.Contains("NECKLACE_FROZEN") ||
                    itemName.Contains("NECKLACE_WEIRD") ||
                    itemName.Contains("NECKLACE_TARGETED") ||
                    itemName.Contains("DLC_NECKLACE") ||
                    // Soul Fragment (Woolhaven DLC)
                    itemName.Contains("SOUL_FRAGMENT") ||
                    // All drinks except BEER, WINE, COCKTAIL, GIN, EGGNOG, POOP_JUICE are Woolhaven DLC
                    (itemName.Contains("DRINK_") && 
                     !itemName.Contains("BEER") && 
                     !itemName.Contains("WINE") && 
                     !itemName.Contains("COCKTAIL") && 
                     !itemName.Contains("GIN") && 
                     !itemName.Contains("EGGNOG") && 
                     !itemName.Contains("POOP_JUICE"))
                )){
                    shouldSkip = true;
                }
                
                // Sinful DLC items
                if(!hasSinfulDLC && itemName.Contains("SINFUL")){
                    shouldSkip = true;
                }
                
                // Heretic DLC items
                if(!hasHereticDLC && itemName.Contains("HERETIC")){
                    shouldSkip = true;
                }
                
                // Pilgrim DLC items
                if(!hasPilgrimDLC && itemName.Contains("PILGRIM")){
                    shouldSkip = true;
                }
                
                if(shouldSkip){
                    Debug.Log($"[CheatMenu] GiveAllItems SKIPPED (DLC): {itemName} (MajorDLC: {hasMajorDLC}, SinfulDLC: {hasSinfulDLC}, HereticDLC: {hasHereticDLC}, PilgrimDLC: {hasPilgrimDLC})");
                    skippedDlc++;
                    continue;
                }
                
                try {
                    CultUtils.AddInventoryItem(type, ItemSpawnQty);
                    Debug.Log($"[CheatMenu] GiveAllItems ADDED: {itemName} x{ItemSpawnQty}");
                    addedCount++;
                } catch(Exception ex) { 
                    Debug.LogWarning($"[CheatMenu] GiveAllItems FAILED to add: {itemName} - {ex.Message}");
                }
            }
            string msg = $"{ItemSpawnQty}x of all items added ({addedCount} types)!";
            if(skippedDlc > 0) msg += $" ({skippedDlc} DLC items skipped)";
            CultUtils.PlayNotification(msg);
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] GiveAllItems error: {e.Message}");
            CultUtils.PlayNotification("Failed to add some items!");
        }
    }

    // ── Souls & Fleece (separate to control via slider) ─────────────────────

    // DISABLED: Doesn't work properly - commenting out
    // [CheatDetails("Give Fleeces", "Gives qty of all fleece types (qty controlled by slider, DLC fleeces require ownership)", subGroup: "Currency")]
    // public static void GiveFleeces(){
    //     try {
    //         int addedCount = 0;
    //         int skippedDlc = 0;
    //         bool hasMajorDLC = CultUtils.HasMajorDLC();
    //         bool hasHereticDLC = CultUtils.HasHereticDLC();
    //         
    //         foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
    //             InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)itemType;
    //             string itemName = type.ToString();
    //             
    //             if(!itemName.Contains("FLEECE")) continue;
    //             
    //             // Skip DLC fleeces if not owned
    //             if(!hasMajorDLC && (itemName.Contains("DLC") || itemName.Contains("FORGE") || itemName.Contains("BREWERY") || itemName.Contains("WINTER") || itemName.Contains("SNOW") || itemName.Contains("WOOLHAVEN"))){
    //                 skippedDlc++;
    //                 continue;
    //             }
    //             // Skip Heretic DLC fleeces if not owned
    //             if(!hasHereticDLC && itemName.Contains("HERETIC")){
    //                 skippedDlc++;
    //                 continue;
    //             }
    //             
    //             try {
    //                 CultUtils.AddInventoryItem(type, ItemSpawnQty);
    //                 addedCount++;
    //             } catch { }
    //         }
    //         
    //         string msg = $"{ItemSpawnQty}x of all fleeces added ({addedCount} types)!";
    //         if(skippedDlc > 0) msg += $" ({skippedDlc} DLC fleeces skipped)";
    //         CultUtils.PlayNotification(msg);
    //     } catch(Exception e){
    //         Debug.LogWarning($"[CheatMenu] GiveFleeces error: {e.Message}");
    //         CultUtils.PlayNotification("Failed to add some fleeces!");
    //     }
    // }

    // ── Inventory Management ──────────────────────────────────────────────────

    [CheatDetails("Clear Inventory", "Removes all items from the player's inventory")]
    public static void ClearInventory(){
        try {
            int clearedCount = 0;
            var inventory = Inventory.items;
            if(inventory != null){
                clearedCount = inventory.Count;
                inventory.Clear();
            }
            Debug.Log($"[CheatMenu] Cleared {clearedCount} items from inventory");
            CultUtils.PlayNotification($"Inventory cleared ({clearedCount} items removed)!");
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] ClearInventory error: {e.Message}");
            CultUtils.PlayNotification("Failed to clear inventory!");
        }
    }
}