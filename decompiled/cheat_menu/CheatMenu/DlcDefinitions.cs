using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.DLC)]
	public class DlcDefinitions : IDefinition
	{
		[CheatDetails("Give DLC Seeds", "Gives x20 of all DLC farming seeds", false, 0)]
		public static void GiveDlcSeeds()
		{
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

		[CheatDetails("Give All Drinks", "Gives x10 of every drink type (DLC Brewing)", false, 0)]
		public static void GiveAllDlcDrinks()
		{
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

		[CheatDetails("Give Brewing Ingredients", "Gives x30 hops, grapes, chilli, cotton, snow fruit, milk", false, 0)]
		public static void GiveBrewingCrops()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.HOPS, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.GRAPES, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHILLI, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SNOW_FRUIT, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 30);
			CultUtils.PlayNotification("Brewing ingredients added!");
		}

		[CheatDetails("Give All Forge Materials", "Gives forge flames, magma, lightning shards, soot, charcoal", false, 0)]
		public static void GiveAllForgeMaterials()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FORGE_FLAME, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ELECTRIFIED_MAGMA, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LIGHTNING_SHARD, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
			CultUtils.PlayNotification("All forge materials added!");
		}

		[CheatDetails("Give Sin Items Bundle", "Gives sin drinks, soot, sozo seeds, charcoal, magma stones", false, 0)]
		public static void GiveSinItemsBundle()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DRINK_SIN, 10);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SOOT, 50);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SEED_SOZO, 20);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.CHARCOAL, 30);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MAGMA_STONE, 20);
			CultUtils.PlayNotification("Sin items bundle added!");
		}

		[CheatDetails("Give Broken Weapons", "Gives one of each broken weapon for repair", false, 0)]
		public static void GiveBrokenWeapons()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_HAMMER, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_SWORD, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_DAGGER, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_GAUNTLETS, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_AXE, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_BLUNDERBUSS, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BROKEN_WEAPON_CHAIN, 1);
			CultUtils.PlayNotification("Broken weapons added!");
		}

		[CheatDetails("Give Legendary Weapon Fragments", "Gives x10 Legendary Weapon Fragments", false, 0)]
		public static void GiveLegendaryFragments()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.LEGENDARY_WEAPON_FRAGMENT, 10);
			CultUtils.PlayNotification("Legendary fragments added!");
		}

		[CheatDetails("Give DLC Collectibles", "Gives rot eyes, lore stones, illegible letters, weapon fragments", false, 0)]
		public static void GiveDlcCollectibles()
		{
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

		[CheatDetails("Give DLC Necklace", "Gives the DLC exclusive necklace", false, 0)]
		public static void GiveDlcNecklace()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.DLC_NECKLACE, 1);
			CultUtils.PlayNotification("DLC necklace added!");
		}

		[CheatDetails("Give Flockade Pieces", "Gives x5 Flockade building pieces", false, 0)]
		public static void GiveFlockadePieces()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FLOCKADE_PIECE, 5);
			CultUtils.PlayNotification("Flockade pieces added!");
		}

		[CheatDetails("Give Webber Skull & Ratau Staff", "Gives key story collectibles", false, 0)]
		public static void GiveStoryCollectibles()
		{
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WEBBER_SKULL, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.RATAU_STAFF, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_SCYLLA, 1);
			CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.ILLEGIBLE_LETTER_CHARYBDIS, 1);
			CultUtils.PlayNotification("Story collectibles added!");
		}

		[CheatDetails("Reset DLC Dungeon", "Resets the DLC dungeon map progress", false, 0)]
		public static void ResetDlcMap()
		{
			try
			{
				DataManager.Instance.DLCDungeonNodesCompleted.Clear();
				CultUtils.PlayNotification("DLC dungeon progress reset!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to reset DLC map: " + ex.Message);
				CultUtils.PlayNotification("Failed to reset DLC map!");
			}
		}

		[CheatDetails("Empty Furnace Fuel", "Empties the DLC furnace fuel", false, 0)]
		public static void EmptyFurnaceFuel()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("EmptyFurnaceFuel", Array.Empty<object>()).GetValue();
				CultUtils.PlayNotification("Furnace fuel emptied!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to empty furnace: " + ex.Message);
				CultUtils.PlayNotification("Failed to empty furnace!");
			}
		}

		[CheatDetails("Fill Furnace Fuel", "Fills all DLC furnaces to max fuel", false, 0)]
		public static void FillFurnaceFuel()
		{
			try
			{
				int num = 0;
				foreach (Interaction_DLCFurnace interaction_DLCFurnace in Interaction_DLCFurnace.Furnaces)
				{
					if (interaction_DLCFurnace != null && interaction_DLCFurnace.Structure != null && interaction_DLCFurnace.Structure.Brain != null)
					{
						interaction_DLCFurnace.Structure.Brain.Data.Fuel = interaction_DLCFurnace.Structure.Brain.Data.MaxFuel;
						num++;
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Filled {0} furnace(s)!", num) : "No furnaces found!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to fill furnace: " + ex.Message);
				CultUtils.PlayNotification("Failed to fill furnace!");
			}
		}

		[CheatDetails("Make Spies Leave", "Forces all spy followers to leave your cult", false, 0)]
		public static void MakeSpiesLeave()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("MakeSpiesLeave", Array.Empty<object>()).GetValue();
				CultUtils.PlayNotification("Spies removed!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to remove spies: " + ex.Message);
				CultUtils.PlayNotification("Failed to remove spies!");
			}
		}

		[CheatDetails("Finish All Missions", "Instantly completes all follower missionary tasks", false, 0)]
		public static void FinishAllMissions()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("FinishAllMissions", Array.Empty<object>()).GetValue();
				CultUtils.PlayNotification("All missions completed!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to finish missions: " + ex.Message);
				CultUtils.PlayNotification("Failed to finish missions!");
			}
		}

		[CheatDetails("Unlock Winter Mode", "Unlocks the winter survival game mode", false, 0)]
		public static void UnlockWinterMode()
		{
			try
			{
				Type type = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					type = assemblies[i].GetType("PersistenceManager");
					if (type != null)
					{
						break;
					}
				}
				if (type != null)
				{
					object value = Traverse.Create(type).Property("PersistentData", null).GetValue();
					if (value != null)
					{
						Traverse.Create(value).Property("UnlockedWinterMode", null).SetValue(true);
						Traverse.Create(type).Method("Save", Array.Empty<object>()).GetValue();
						CultUtils.PlayNotification("Winter mode unlocked!");
					}
					else
					{
						CultUtils.PlayNotification("PersistentData not available!");
					}
				}
				else
				{
					CultUtils.PlayNotification("PersistenceManager not found!");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to unlock winter mode: " + ex.Message);
				CultUtils.PlayNotification("Failed to unlock winter mode!");
			}
		}
	}
}
