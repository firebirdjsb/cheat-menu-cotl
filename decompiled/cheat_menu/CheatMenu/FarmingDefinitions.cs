using System;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.FARMING)]
	public class FarmingDefinitions : IDefinition
	{
		private static void SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE animalType, string animalName)
		{
			try
			{
				if (Interaction_Ranch.Ranches == null || Interaction_Ranch.Ranches.Count == 0)
				{
					CultUtils.PlayNotification("No ranch found! Build a ranch first.");
				}
				else
				{
					Interaction_Ranch interaction_Ranch = null;
					foreach (Interaction_Ranch interaction_Ranch2 in Interaction_Ranch.Ranches)
					{
						if (interaction_Ranch2.Brain != null && !interaction_Ranch2.IsFull)
						{
							interaction_Ranch = interaction_Ranch2;
							break;
						}
					}
					if (interaction_Ranch == null)
					{
						CultUtils.PlayNotification("All ranches are full!");
					}
					else
					{
						DataManager.Instance.AnimalID++;
						StructuresData.Ranchable_Animal ranchable_Animal = new StructuresData.Ranchable_Animal
						{
							Type = animalType,
							ID = DataManager.Instance.AnimalID,
							Age = 1,
							Satiation = 50f,
							Level = 1,
							Horns = global::UnityEngine.Random.Range(1, 6),
							Ears = global::UnityEngine.Random.Range(1, 6),
							Head = global::UnityEngine.Random.Range(1, 6),
							Colour = global::UnityEngine.Random.Range(0, 10),
							TimeSinceLastWash = TimeManager.TotalElapsedGameTime + 4800f + global::UnityEngine.Random.Range(-120f, 120f),
							TimeSincePoop = TimeManager.TotalElapsedGameTime + 1920f + global::UnityEngine.Random.Range(-120f, 120f)
						};
						interaction_Ranch.AddAnimal(ranchable_Animal, null);
						CultUtils.PlayNotification(animalName + " spawned in ranch!");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to spawn " + animalName + ": " + ex.Message);
				CultUtils.PlayNotification("Failed to spawn " + animalName + "!");
			}
		}

		[CheatDetails("Spawn Goat at Player", "Spawns a live goat at the player's position", false, 0)]
		public static void SpawnGoat()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_GOAT, "Goat");
		}

		[CheatDetails("Spawn Cow at Player", "Spawns a live cow at the player's position", false, 0)]
		public static void SpawnCow()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_COW, "Cow");
		}

		[CheatDetails("Spawn Llama at Player", "Spawns a live llama at the player's position", false, 0)]
		public static void SpawnLlama()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_LLAMA, "Llama");
		}

		[CheatDetails("Spawn Turtle at Player", "Spawns a live turtle at the player's position", false, 0)]
		public static void SpawnTurtle()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_TURTLE, "Turtle");
		}

		[CheatDetails("Spawn Crab at Player", "Spawns a live crab at the player's position", false, 0)]
		public static void SpawnCrab()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_CRAB, "Crab");
		}

		[CheatDetails("Spawn Spider at Player", "Spawns a live spider at the player's position", false, 0)]
		public static void SpawnSpider()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_SPIDER, "Spider");
		}

		[CheatDetails("Spawn Snail at Player", "Spawns a live snail at the player's position", false, 0)]
		public static void SpawnSnail()
		{
			FarmingDefinitions.SpawnAnimalAtPlayer(InventoryItem.ITEM_TYPE.ANIMAL_SNAIL, "Snail");
		}

		[CheatDetails("Give x5 All Animals", "Adds x5 of each animal type to inventory", false, 0)]
		public static void GiveAllAnimals()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_GOAT, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_COW, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_LLAMA, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_TURTLE, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_CRAB, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_SPIDER, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ANIMAL_SNAIL, 5);
			CultUtils.PlayNotification("All animals added!");
		}

		[CheatDetails("Collect All Animal Products", "Collects products from all ready animals (wool, shells, silk, etc.)", false, 0)]
		public static void CollectAllAnimalProducts()
		{
			try
			{
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in AnimalData.GetAnimals())
				{
					if (ranchable_Animal.WorkedReady && !ranchable_Animal.WorkedToday)
					{
						foreach (InventoryItem inventoryItem in Interaction_Ranchable.GetWorkLoot(ranchable_Animal))
						{
							CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)inventoryItem.type, inventoryItem.quantity);
							num += inventoryItem.quantity;
						}
						ranchable_Animal.WorkedToday = true;
						ranchable_Animal.WorkedReady = false;
						ranchable_Animal.GrowthStage = 0;
						Interaction_Ranchable animal = Interaction_Ranch.GetAnimal(ranchable_Animal);
						if (animal != null)
						{
							animal.UpdateSkin();
						}
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Collected {0} item(s) from animals!", num) : "No animals ready for collection!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to collect animal products: " + ex.Message);
				CultUtils.PlayNotification("Failed to collect animal products!");
			}
		}

		[CheatDetails("Feed All Animals", "Fills hunger for all ranch animals", false, 0)]
		public static void FeedAllAnimals()
		{
			try
			{
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in AnimalData.GetAnimals())
				{
					ranchable_Animal.Satiation = 100f;
					ranchable_Animal.EatenToday = true;
					num++;
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Fed {0} animal(s)!", num) : "No animals to feed!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to feed animals: " + ex.Message);
				CultUtils.PlayNotification("Failed to feed animals!");
			}
		}

		[CheatDetails("Clean All Pens", "Cleans all stinky animals and resets wash timers", false, 0)]
		public static void CleanAllPens()
		{
			try
			{
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in AnimalData.GetAnimals())
				{
					ranchable_Animal.TimeSinceLastWash = TimeManager.TotalElapsedGameTime + 4800f;
					if (ranchable_Animal.Ailment == Interaction_Ranchable.Ailment.Stinky)
					{
						ranchable_Animal.Ailment = Interaction_Ranchable.Ailment.None;
					}
					Interaction_Ranchable animal = Interaction_Ranch.GetAnimal(ranchable_Animal);
					if (animal != null)
					{
						animal.Clean(false);
					}
					num++;
				}
				foreach (Structures_Ranch structures_Ranch in StructureManager.GetAllStructuresOfType<Structures_Ranch>())
				{
					if (structures_Ranch.Data != null)
					{
						int totalPoops = structures_Ranch.Data.TotalPoops;
						if (totalPoops > 0)
						{
							CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, totalPoops);
							structures_Ranch.Data.TotalPoops = 0;
						}
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Cleaned {0} animal(s)!", num) : "No animals to clean!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clean pens: " + ex.Message);
				CultUtils.PlayNotification("Failed to clean pens!");
			}
		}

		[CheatDetails("Give All Wool", "Gives x10 wool and x5 of each special wool type", false, 0)]
		public static void GiveAllWool()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WOOL, 10);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_RANCHER, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_LAMBWAR, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_BLACKSMITH, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_TAROT, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SPECIAL_WOOL_DECORATION, 5);
			CultUtils.PlayNotification("All wool added!");
		}

		[CheatDetails("Give Eggs & Yolks", "Gives x5 follower eggs and x10 yolks", false, 0)]
		public static void GiveEggs()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.EGG_FOLLOWER, 5);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.YOLK, 10);
			CultUtils.PlayNotification("Eggs and yolks added!");
		}

		[CheatDetails("Give Special Poop", "Gives x20 of each special poop type", false, 0)]
		public static void GiveSpecialPoop()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_GOLD, 20);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_RAINBOW, 20);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_GLOW, 20);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_DEVOTION, 20);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP_ROTSTONE, 20);
			CultUtils.PlayNotification("Special poop added!");
		}

		[CheatDetails("Give Milk", "Gives x20 Milk", false, 0)]
		public static void GiveMilk()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 20);
			CultUtils.PlayNotification("Milk added!");
		}

		[CheatDetails("Force Grow All Animals", "Instantly matures all baby animals and makes them harvest-ready", false, 0)]
		public static void ForceGrowAllAnimals()
		{
			CultUtils.ForceGrowAllAnimals();
		}

		[CheatDetails("Add Halos to Animals", "Adds a glowing pink halo above all ranch animals", false, 0)]
		public static void AddHalosToAnimals()
		{
			CultUtils.AddHalosToAnimals();
		}

		[CheatDetails("Ascend All Animals", "Ascends all ranch animals with full animation, sound effects, and visual resource drops", false, 0)]
		public static void AscendAllAnimals()
		{
			CultUtils.AscendAllAnimals();
		}
	}
}
