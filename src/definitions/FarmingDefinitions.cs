using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.FARMING)]
public class FarmingDefinitions : IDefinition{

    private static void SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE animalType, string animalName){
        try {
            if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
            if(Interaction_Ranch.Ranches == null || Interaction_Ranch.Ranches.Count == 0){
                CultUtils.PlayNotification("No ranch found! Build a ranch first.");
                return;
            }
            Interaction_Ranch targetRanch = null;
            foreach(var ranch in Interaction_Ranch.Ranches){
                if(ranch.Brain != null && !ranch.IsFull){
                    targetRanch = ranch;
                    break;
                }
            }
            if(targetRanch == null){
                CultUtils.PlayNotification("All ranches are full!");
                return;
            }
            DataManager.Instance.AnimalID++;
            StructuresData.Ranchable_Animal animal = new StructuresData.Ranchable_Animal{
                Type = animalType,
                ID = DataManager.Instance.AnimalID,
                Age = 1,
                Satiation = 50f,
                Level = 1,
                Horns = UnityEngine.Random.Range(1, 6),
                Ears = UnityEngine.Random.Range(1, 6),
                Head = UnityEngine.Random.Range(1, 6),
                Colour = UnityEngine.Random.Range(0, 10),
                TimeSinceLastWash = TimeManager.TotalElapsedGameTime + 4800f + UnityEngine.Random.Range(-120f, 120f),
                TimeSincePoop = TimeManager.TotalElapsedGameTime + 1920f + UnityEngine.Random.Range(-120f, 120f),
            };
            targetRanch.AddAnimal(animal, null);
            CultUtils.PlayNotification($"{animalName} spawned in ranch!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to spawn {animalName}: {e.Message}");
            CultUtils.PlayNotification($"Failed to spawn {animalName}!");
        }
    }

    [CheatDetails("Spawn Goat at Player", "Spawns a live goat at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnGoat(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_GOAT, "Goat");
    }

    [CheatDetails("Spawn Cow at Player", "Spawns a live cow at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnCow(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_COW, "Cow");
    }

    [CheatDetails("Spawn Llama at Player", "Spawns a live llama at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnLlama(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_LLAMA, "Llama");
    }

    [CheatDetails("Spawn Turtle at Player", "Spawns a live turtle at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnTurtle(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_TURTLE, "Turtle");
    }

    [CheatDetails("Spawn Crab at Player", "Spawns a live crab at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnCrab(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_CRAB, "Crab");
    }

    [CheatDetails("Spawn Spider at Player", "Spawns a live spider at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnSpider(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_SPIDER, "Spider");
    }

    [CheatDetails("Spawn Snail at Player", "Spawns a live snail at the player's position", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void SpawnSnail(){
        SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_SNAIL, "Snail");
    }

    [CheatDetails("Give x5 All Animals", "Adds x5 of each animal type to inventory (Woolhaven DLC required)", subGroup: "Spawn")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveAllAnimals(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_GOAT, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_COW, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_LLAMA, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_TURTLE, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_CRAB, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_SPIDER, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_SNAIL, 5);
        CultUtils.PlayNotification("All animals added!");
    }

    [CheatDetails("Collect All Animal Products", "Collects products from all ready animals (wool, shells, silk, etc.) (Woolhaven DLC required)", subGroup: "Manage")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void CollectAllAnimalProducts(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        try {
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                if(animal.WorkedReady && !animal.WorkedToday){
                    List<InventoryItem> loot = Interaction_Ranchable.GetWorkLoot(animal);
                    foreach(var item in loot){
                        CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)item.type, item.quantity);
                        count += item.quantity;
                    }
                    animal.WorkedToday = true;
                    animal.WorkedReady = false;
                    animal.GrowthStage = 0;
                    Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                    if(ranchable != null){
                        ranchable.UpdateSkin();
                    }
                }
            }
            CultUtils.PlayNotification(count > 0 ? $"Collected {count} item(s) from animals!" : "No animals ready for collection!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to collect animal products: {e.Message}");
            CultUtils.PlayNotification("Failed to collect animal products!");
        }
    }

    [CheatDetails("Feed All Animals", "Fills hunger for all ranch animals (Woolhaven DLC required)", subGroup: "Manage")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void FeedAllAnimals(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        try {
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                animal.Satiation = Interaction_Ranchable.MAX_HUNGER;
                animal.EatenToday = true;
                count++;
            }
            CultUtils.PlayNotification(count > 0 ? $"Fed {count} animal(s)!" : "No animals to feed!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to feed animals: {e.Message}");
            CultUtils.PlayNotification("Failed to feed animals!");
        }
    }

    [CheatDetails("Clean All Pens", "Cleans all stinky animals and resets wash timers (Woolhaven DLC required)", subGroup: "Manage")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void CleanAllPens(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        try {
            int count = 0;
            var animals = AnimalData.GetAnimals();
            foreach(var animal in animals){
                animal.TimeSinceLastWash = TimeManager.TotalElapsedGameTime + 4800f;
                if(animal.Ailment == Interaction_Ranchable.Ailment.Stinky){
                    animal.Ailment = Interaction_Ranchable.Ailment.None;
                }
                Interaction_Ranchable ranchable = Interaction_Ranch.GetAnimal(animal);
                if(ranchable != null){
                    ranchable.Clean(false);
                }
                count++;
            }
            // Also collect poop from ranch inventories
            var ranches = StructureManager.GetAllStructuresOfType<Structures_Ranch>();
            foreach(var ranch in ranches){
                if(ranch.Data != null){
                    int poopCount = ranch.Data.TotalPoops;
                    if(poopCount > 0){
                        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, poopCount);
                        ranch.Data.TotalPoops = 0;
                    }
                }
            }
            CultUtils.PlayNotification(count > 0 ? $"Cleaned {count} animal(s)!" : "No animals to clean!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to clean pens: {e.Message}");
            CultUtils.PlayNotification("Failed to clean pens!");
        }
    }

    [CheatDetails("Give All Wool", "Gives x10 wool and x5 of each special wool type (Woolhaven DLC required)", subGroup: "Products")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveAllWool(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WOOL, 10);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_RANCHER, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_LAMBWAR, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_BLACKSMITH, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_TAROT, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_DECORATION, 5);
        CultUtils.PlayNotification("All wool added!");
    }

    [CheatDetails("Give Eggs & Yolks", "Gives x5 follower eggs and x10 yolks (Woolhaven DLC required)", subGroup: "Products")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveEggs(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.EGG_FOLLOWER, 5);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.YOLK, 10);
        CultUtils.PlayNotification("Eggs and yolks added!");
    }

    [CheatDetails("Give Special Poop", "Gives x20 of each special poop type (Woolhaven DLC required)", subGroup: "Products")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveSpecialPoop(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_GOLD, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_RAINBOW, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_GLOW, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_DEVOTION, 20);
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_ROTSTONE, 20);
        CultUtils.PlayNotification("Special poop added!");
    }

    [CheatDetails("Give Milk", "Gives x20 Milk (Woolhaven DLC required)", subGroup: "Products")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void GiveMilk(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 20);
        CultUtils.PlayNotification("Milk added!");
    }

    [CheatDetails("Force Grow All Animals", "Instantly matures all baby animals and makes them harvest-ready (Woolhaven DLC required)", subGroup: "Manage")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void ForceGrowAllAnimals(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.ForceGrowAllAnimals();
    }

    [CheatDetails("Add Halos to Animals", "Adds a glowing pink halo above all ranch animals (Woolhaven DLC required)", subGroup: "Special")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void AddHalosToAnimals(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AddHalosToAnimals();
    }

    [CheatDetails("Ascend All Animals", "Ascends all ranch animals with full animation, sound effects, and visual resource drops (Woolhaven DLC required)", subGroup: "Special")]
    [RequiresDLC(DlcRequirement.MajorDLC)]
    public static void AscendAllAnimals(){
        if(!CultUtils.HasMajorDLC()){ CultUtils.PlayNotification("Requires Woolhaven DLC!"); return; }
        CultUtils.AscendAllAnimals();
    }
}
