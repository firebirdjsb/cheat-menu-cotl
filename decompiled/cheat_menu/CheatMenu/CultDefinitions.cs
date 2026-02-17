using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.CULT)]
	public class CultDefinitions : IDefinition
	{
		[Init]
		public static void Init()
		{
			CultDefinitions.s_ritualGui = new GUIUtils.ScrollableWindowParams("Unlock All Rituals", GUIUtils.GetCenterRect(650, 700), null, null);
			CultDefinitions.s_docterineGui = new GUIUtils.ScrollableWindowParams("Change Doctrines", GUIUtils.GetCenterRect(650, 700), null, null);
		}

		[CheatDetails("Teleport to Cult", "Teleports the player to the Base", false, 0)]
		public static void TeleportToBase()
		{
			Traverse.Create(typeof(CheatConsole)).Method("ReturnToBase", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("Teleported to base!");
		}

		[CheatDetails("Rename Cult", "Bring up the UI to rename the cult", false, 0)]
		public static void RenameCult()
		{
			CultUtils.RenameCult(null);
		}

		[CheatDetails("Allow Shrine Creation", "Allow Shrine Creation (OFF)", "Allow Shrine Creation (ON)", "Allows the Shrine to be created from the building menu", true, 0)]
		public static void AllowShrineCreation(bool flag)
		{
			DataManager.Instance.BuildShrineEnabled = flag;
			CultUtils.PlayNotification(flag ? "Shrine creation enabled!" : "Shrine creation disabled!");
		}

		[CheatDetails("Clear Base Rubble", "Removes any stones and large rubble", false, 0)]
		public static void ClearBaseRubble()
		{
			CultUtils.ClearBaseRubble();
		}

		[CheatDetails("Clear Base Trees", "Removes all trees in the base", false, 0)]
		public static void ClearBaseTrees()
		{
			CultUtils.ClearBaseTrees();
		}

		[CheatDetails("Clear Base Landscape", "Removes all grass, weeds, bushes, ferns, stumps and landscape entities from all areas", false, 0)]
		public static void ClearBaseGrass()
		{
			CultUtils.ClearBaseGrass();
		}

		[CheatDetails("Clear Vomit", "Clear any vomit on the floor!", false, 0)]
		public static void ClearVomit()
		{
			CultUtils.ClearVomit();
		}

		[CheatDetails("Clear Poop", "Clear any poop on the floor and janitor stations, giving the fertilizer directly!", false, 0)]
		public static void ClearPoop()
		{
			CultUtils.ClearPoop();
		}

		[CheatDetails("Clear Dead bodies", "Clears dead follower and animal bodies, giving meat and bones!", false, 0)]
		public static void ClearDeadBodies()
		{
			CultUtils.ClearBodies();
		}

		public static bool Prefix_UpgradeSystem_AddCooldown()
		{
			return false;
		}

		[CheatDetails("Auto Clear Ritual Cooldowns", "Set ritual cooldowns to zero while active", true, 0)]
		public static void ZeroRitualCooldown(bool flagStatus)
		{
			UpgradeSystem.ClearAllCoolDowns();
			if (flagStatus)
			{
				ReflectionHelper.PatchMethodPrefix(typeof(UpgradeSystem), "AddCooldown", ReflectionHelper.GetMethodStaticPublic("Prefix_UpgradeSystem_AddCooldown"), BindingFlags.Static | BindingFlags.Public, null, false);
				CultUtils.PlayNotification("Ritual cooldowns auto-cleared!");
				return;
			}
			ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "AddCooldown");
			CultUtils.PlayNotification("Ritual cooldown clearing disabled!");
		}

		public static bool Prefix_CostFormatter_FormatCost(StructuresData.ItemCost itemCost, ref string __result)
		{
			__result = CostFormatter.FormatCost(itemCost.CostItem, 0, Inventory.GetItemQuantity(itemCost.CostItem), false, true);
			return false;
		}

		[CheatDetails("Free Building Mode", "Buildings can be placed for free", true, 0)]
		public static void FreeBuildingMode(bool flagStatus)
		{
			if (flagStatus)
			{
				MethodInfo method = typeof(CultDefinitions).GetMethod("Prefix_CostFormatter_FormatCost");
				ReflectionHelper.PatchMethodPrefix(typeof(CostFormatter), "FormatCost", method, BindingFlags.Static | BindingFlags.Public, new Type[]
				{
					typeof(StructuresData.ItemCost),
					typeof(bool),
					typeof(bool)
				}, false);
			}
			else
			{
				ReflectionHelper.UnpatchTracked(typeof(CostFormatter), "FormatCost");
			}
			Traverse.Create(typeof(CheatConsole)).Field("BuildingsFree").SetValue(flagStatus);
			CultUtils.PlayNotification(flagStatus ? "Free building enabled!" : "Free building disabled!");
		}

		[CheatDetails("Build All Structures", "Instantly build all structures", false, 0)]
		public static void BuildAllStructures()
		{
			Traverse.Create(typeof(CheatConsole)).Method("BuildAll", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("All structures built!");
		}

		[CheatDetails("Unlock All Structures", "Unlocks all buildings including DLC", false, 0)]
		public static void UnlockAllStructures()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("UnlockAllStructures", Array.Empty<object>()).GetValue();
			}
			catch (Exception ex)
			{
				Debug.LogWarning("CheatConsole.UnlockAllStructures failed: " + ex.Message);
			}
			try
			{
				foreach (object obj in Enum.GetValues(typeof(StructureBrain.TYPES)))
				{
					StructureBrain.TYPES types = (StructureBrain.TYPES)obj;
					StructuresData.SetRevealed(types);
					if (!DataManager.Instance.UnlockedStructures.Contains(types))
					{
						DataManager.Instance.UnlockedStructures.Add(types);
					}
					if (!DataManager.Instance.RevealedStructures.Contains(types))
					{
						DataManager.Instance.RevealedStructures.Add(types);
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("Structure iteration unlock failed: " + ex2.Message);
			}
			CultUtils.PlayNotification("All structures unlocked!");
		}

		[CheatDetails("Clear Outhouses", "Clears all outhouses of poop and adds the contents to your inventory.", false, 0)]
		public static void ClearAllOuthouses()
		{
			CultUtils.ClearOuthouses();
		}

		[CheatDetails("Repair All Structures", "Repairs all damaged structures in the base", false, 0)]
		public static void RepairAllStructures()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(StructureBrain.TYPES)))
				{
					FollowerLocation followerLocation = FollowerLocation.Base;
					foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, (StructureBrain.TYPES)obj, false))
					{
						if (structureBrain.Data != null)
						{
							Traverse traverse = Traverse.Create(structureBrain.Data).Field("DamagedDurability");
							if (!traverse.FieldExists())
							{
								traverse = Traverse.Create(structureBrain.Data).Property("DamagedDurability", null);
							}
							if (traverse.GetValue() != null && traverse.GetValue<float>() > 0f)
							{
								traverse.SetValue(0f);
								num++;
							}
						}
					}
				}
				CultUtils.PlayNotification(string.Format("Repaired {0} structure(s)!", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to repair structures: " + ex.Message);
				CultUtils.PlayNotification("Failed to repair structures!");
			}
		}

		[CheatDetails("Harvest All Farms", "Instantly harvests all ready crops from farm plots", false, 0)]
		public static void HarvestAllFarms()
		{
			try
			{
				int num = 0;
				foreach (Structures_FarmerPlot structures_FarmerPlot in StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>())
				{
					if (structures_FarmerPlot.IsFullyGrown)
					{
						structures_FarmerPlot.Harvest();
						num++;
					}
				}
				CultUtils.PlayNotification(string.Format("Harvested {0} farm(s)!", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to harvest farms: " + ex.Message);
				CultUtils.PlayNotification("Failed to harvest farms!");
			}
		}

		[CheatDetails("Grow All Crops", "Instantly grows all planted crops to full", false, 0)]
		public static void GrowAllCrops()
		{
			try
			{
				int num = 0;
				foreach (Structures_FarmerPlot structures_FarmerPlot in StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>())
				{
					if (!structures_FarmerPlot.IsFullyGrown && structures_FarmerPlot.HasPlantedSeed())
					{
						structures_FarmerPlot.ForceFullyGrown();
						num++;
					}
				}
				CultUtils.PlayNotification(string.Format("Grew {0} crop(s) to full!", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to grow crops: " + ex.Message);
				CultUtils.PlayNotification("Failed to grow crops!");
			}
		}

		[CheatDetails("Change Rituals", "Change Rituals", "Change Rituals (Close)", "Lets you change the selected Rituals along with unlocking not yet acquired ones", true, 0)]
		public static void ChangeAllRituals(bool flag)
		{
			if (flag)
			{
				CultDefinitions.<>c__DisplayClass22_0 CS$<>8__locals1 = new CultDefinitions.<>c__DisplayClass22_0();
				CS$<>8__locals1.pairs = CultUtils.GetRitualPairs();
				CS$<>8__locals1.currentHeight = 20;
				CS$<>8__locals1.guiFunctionKey = "";
				CS$<>8__locals1.pairStates = new int[CS$<>8__locals1.pairs.Count];
				for (int i = 0; i < CS$<>8__locals1.pairStates.Length; i++)
				{
					Tuple<UpgradeSystem.Type, UpgradeSystem.Type> tuple = CS$<>8__locals1.pairs[i];
					bool flag2 = UpgradeSystem.UnlockedUpgrades.Contains(tuple.Item1);
					bool flag3 = UpgradeSystem.UnlockedUpgrades.Contains(tuple.Item1);
					if (flag2 || flag3)
					{
						CS$<>8__locals1.pairStates[i] = (flag2 ? 1 : 2);
					}
					CS$<>8__locals1.pairStates[i] = 0;
				}
				CS$<>8__locals1.guiFunctionKey = GUIManager.SetGuiWindowScrollableFunction(CultDefinitions.s_ritualGui, new Action(CS$<>8__locals1.<ChangeAllRituals>g__GuiContents|1));
				return;
			}
			GUIManager.RemoveGuiFunction();
		}

		[CheatDetails("Clear All Doctrines", "Clears all docterine categories and rewards (Apart from special rituals)", false, 0)]
		public static void ClearAllDoctrines()
		{
			CultUtils.ClearAllDocterines();
		}

		[CheatDetails("All Rituals", "All Rituals (Off)", "All Rituals (On)", "While enabled you will have access to all rituals (including both sides of every pair)", true, 0)]
		public static void UnlockAllRituals(bool flag)
		{
			CheatConsole.UnlockAllRituals = flag;
			CultUtils.PlayNotification(flag ? "All rituals unlocked!" : "Rituals reverted!");
		}

		[CheatDetails("Unlock All Clothing", "Unlocks all available follower clothing types", false, 0)]
		public static void UnlockAllClothing()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(FollowerClothingType)))
				{
					FollowerClothingType followerClothingType = (FollowerClothingType)obj;
					if (followerClothingType != FollowerClothingType.None && followerClothingType != FollowerClothingType.Count && !DataManager.Instance.ClothesUnlocked(followerClothingType))
					{
						DataManager.Instance.AddNewClothes(followerClothingType);
						num++;
					}
				}
				DataManager.Instance.UnlockedTailor = true;
				DataManager.Instance.RevealedTailor = true;
				CultUtils.PlayNotification(string.Format("All clothing unlocked! ({0} new items)", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to unlock clothing: " + ex.Message);
				CultUtils.PlayNotification("Failed to unlock clothing!");
			}
		}

		[CheatDetails("Give All Clothing Items", "Unlocks all clothing and assigns clothing to followers currently in the scene", false, 0)]
		public static void GiveAllClothing()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(FollowerClothingType)))
				{
					FollowerClothingType followerClothingType = (FollowerClothingType)obj;
					if (followerClothingType != FollowerClothingType.None && followerClothingType != FollowerClothingType.Count && !DataManager.Instance.ClothesUnlocked(followerClothingType))
					{
						DataManager.Instance.AddNewClothes(followerClothingType);
					}
				}
				DataManager.Instance.UnlockedTailor = true;
				DataManager.Instance.RevealedTailor = true;
				List<FollowerClothingType> list = new List<FollowerClothingType>();
				foreach (object obj2 in Enum.GetValues(typeof(FollowerClothingType)))
				{
					FollowerClothingType followerClothingType2 = (FollowerClothingType)obj2;
					if (followerClothingType2 != FollowerClothingType.None && followerClothingType2 != FollowerClothingType.Count && followerClothingType2 != FollowerClothingType.Naked && TailorManager.GetClothingData(followerClothingType2) != null)
					{
						list.Add(followerClothingType2);
					}
				}
				List<FollowerInfo> followers = DataManager.Instance.Followers;
				int num2 = 0;
				for (int i = 0; i < followers.Count; i++)
				{
					try
					{
						FollowerInfo followerInfo = followers[i];
						if (followerInfo != null)
						{
							Follower followerFromInfo = CultUtils.GetFollowerFromInfo(followerInfo);
							if (followerFromInfo != null && followerFromInfo.gameObject != null && followerFromInfo.gameObject.activeInHierarchy && followerFromInfo.Outfit != null)
							{
								try
								{
									FollowerClothingType followerClothingType3 = list[num2 % list.Count];
									followerInfo.Clothing = followerClothingType3;
									followerInfo.Outfit = FollowerOutfitType.Custom;
									followerFromInfo.SetOutfit(FollowerOutfitType.Custom, false, Thought.None);
									num++;
									num2++;
								}
								catch (Exception ex)
								{
									Debug.LogWarning(string.Format("[CheatMenu] Could not update follower {0} outfit: {1}", followerInfo.ID, ex.Message));
								}
							}
						}
					}
					catch (Exception ex2)
					{
						Debug.LogWarning(string.Format("[CheatMenu] Error setting clothing for follower {0}: {1}", i, ex2.Message));
					}
				}
				CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.COTTON, 50);
				CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.SILK_THREAD, 50);
				CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WOOL, 30);
				if (num > 0)
				{
					CultUtils.PlayNotification(string.Format("All clothing unlocked! {0} follower(s) dressed.", num));
				}
				else
				{
					CultUtils.PlayNotification("All clothing unlocked! (No followers in scene to dress)");
				}
			}
			catch (Exception ex3)
			{
				Debug.LogWarning("[CheatMenu] Failed to give clothing: " + ex3.Message);
				CultUtils.PlayNotification("Failed to give clothing!");
			}
		}

		private static GUIUtils.ScrollableWindowParams s_ritualGui;

		private static GUIUtils.ScrollableWindowParams s_docterineGui;
	}
}
