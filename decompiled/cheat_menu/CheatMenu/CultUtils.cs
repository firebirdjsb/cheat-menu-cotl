using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace CheatMenu
{
	internal class CultUtils
	{
		public static bool IsInGame()
		{
			return SaveAndLoad.Loaded;
		}

		public static void GiveDocterineStone()
		{
			DataManager.Instance.FirstDoctrineStone = true;
			DataManager.Instance.ForceDoctrineStones = true;
			DataManager.Instance.CompletedDoctrineStones++;
			Action onIncreaseCount = PlayerDoctrineStone.OnIncreaseCount;
			if (onIncreaseCount != null)
			{
				onIncreaseCount();
			}
			CultUtils.PlayNotification("Commandment stone given!");
		}

		public static void CompleteObjective(ObjectivesData objective)
		{
			objective.Complete();
			Action action = delegate
			{
				FieldInfo field = typeof(ObjectiveManager).GetField("OnObjectiveCompleted", BindingFlags.Static | BindingFlags.NonPublic);
				if (field != null && field.GetValue(null) != null)
				{
					object value = field.GetValue(null);
					if (value != null)
					{
						ObjectiveManager.ObjectiveUpdated objectiveUpdated = value as ObjectiveManager.ObjectiveUpdated;
						if (objectiveUpdated != null)
						{
							objectiveUpdated(objective);
						}
					}
				}
			};
			MethodInfo method = typeof(ObjectiveManager).GetMethod("InvokeOrQueue", BindingFlags.Static | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(null, new object[] { action });
			}
			DataManager.Instance.Objectives.Remove(objective);
			DataManager.Instance.CompletedObjectives.Add(objective);
		}

		public static void CompleteAllQuests()
		{
			if (DataManager.Instance.Objectives.Count > 0)
			{
				ObjectivesData[] array = DataManager.Instance.Objectives.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					CultUtils.CompleteObjective(array[i]);
				}
			}
		}

		public static void ClearAllDocterines()
		{
			CultUtils.ClearUnlockedRituals();
			DataManager.Instance.CultTraits.Clear();
			DoctrineUpgradeSystem.UnlockedUpgrades.Clear();
			foreach (SermonCategory sermonCategory in CultUtils.GetAllSermonCategories())
			{
				DoctrineUpgradeSystem.SetLevelBySermon(sermonCategory, 0);
			}
			UpgradeSystem.UnlockAbility(UpgradeSystem.PrimaryRitual1, false);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Bonfire);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_ReadMind);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice);
			CultUtils.PlayNotification("Cleared all docterines!");
		}

		public static void RemoveDocterineUpgrades()
		{
			List<DoctrineUpgradeSystem.DoctrineType> list = new List<DoctrineUpgradeSystem.DoctrineType>();
			foreach (DoctrineUpgradeSystem.DoctrineType doctrineType in DoctrineUpgradeSystem.UnlockedUpgrades)
			{
				list.Add(doctrineType);
			}
			foreach (DoctrineUpgradeSystem.DoctrineType doctrineType2 in DoctrineUpgradeSystem.UnlockedUpgrades)
			{
				SermonCategory category = DoctrineUpgradeSystem.GetCategory(doctrineType2);
				if (category != SermonCategory.Special && category != SermonCategory.PlayerUpgrade)
				{
					list.Remove(doctrineType2);
				}
			}
			DoctrineUpgradeSystem.UnlockedUpgrades = list;
		}

		public static List<SermonCategory> GetAllSermonCategories()
		{
			List<SermonCategory> list = new List<SermonCategory>();
			foreach (object obj in Enum.GetValues(typeof(SermonCategory)))
			{
				SermonCategory sermonCategory = (SermonCategory)obj;
				if (!DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(sermonCategory).StartsWith("DoctrineUpgradeSystem"))
				{
					list.Add(sermonCategory);
				}
			}
			return list;
		}

		public static int[] GetDoctrineCategoryState(SermonCategory category, List<DoctrineUpgradeSystem.DoctrineType> upgrades = null)
		{
			List<DoctrineUpgradeSystem.DoctrineType> list = upgrades;
			if (list == null)
			{
				list = DoctrineUpgradeSystem.UnlockedUpgrades;
			}
			Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>> allDoctrinePairs = CultUtils.GetAllDoctrinePairs();
			int[] array = new int[4];
			for (int i = 0; i < array.Length; i++)
			{
				Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType> tuple = allDoctrinePairs[category][i];
				if (list.Contains(tuple.Item1))
				{
					array[i] = 1;
				}
				else if (list.Contains(tuple.Item2))
				{
					array[i] = 2;
				}
				else
				{
					array[i] = 0;
				}
			}
			return array;
		}

		public static void ReapplyAllDoctrinesWithChanges(SermonCategory overridenCategory, int[] stateMap)
		{
			DataManager.Instance.CultTraits.Clear();
			List<DoctrineUpgradeSystem.DoctrineType> list = new List<DoctrineUpgradeSystem.DoctrineType>();
			foreach (DoctrineUpgradeSystem.DoctrineType doctrineType in DoctrineUpgradeSystem.UnlockedUpgrades)
			{
				list.Add(doctrineType);
			}
			CultUtils.RemoveDocterineUpgrades();
			List<SermonCategory> allSermonCategories = CultUtils.GetAllSermonCategories();
			Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>> allDoctrinePairs = CultUtils.GetAllDoctrinePairs();
			foreach (SermonCategory sermonCategory in allSermonCategories)
			{
				int[] array = ((sermonCategory == overridenCategory) ? stateMap : CultUtils.GetDoctrineCategoryState(sermonCategory, list));
				for (int i = 0; i < array.Length; i++)
				{
					int num = array[i];
					if (num == 1)
					{
						DoctrineUpgradeSystem.UnlockAbility(allDoctrinePairs[sermonCategory][i].Item1);
					}
					else if (num == 2)
					{
						DoctrineUpgradeSystem.UnlockAbility(allDoctrinePairs[sermonCategory][i].Item2);
					}
					if (num != 0)
					{
						DoctrineUpgradeSystem.SetLevelBySermon(sermonCategory, i + 1);
					}
				}
			}
		}

		public static void ClearAllCultTraits()
		{
			foreach (FollowerTrait.TraitType traitType in DataManager.Instance.CultTraits)
			{
				Debug.Log(FollowerTrait.GetLocalizedTitle(traitType) ?? "");
			}
			DataManager.Instance.CultTraits.Clear();
		}

		public static Dictionary<UpgradeSystem.Type, UpgradeSystem.Type> GetDictionaryRitualPairs()
		{
			Dictionary<UpgradeSystem.Type, UpgradeSystem.Type> dictionary = new Dictionary<UpgradeSystem.Type, UpgradeSystem.Type>();
			for (int i = 0; i < UpgradeSystem.SecondaryRitualPairs.Length - 1; i += 2)
			{
				UpgradeSystem.Type type = UpgradeSystem.SecondaryRitualPairs[i];
				UpgradeSystem.Type type2 = UpgradeSystem.SecondaryRitualPairs[i + 1];
				dictionary[type] = type2;
				dictionary[type2] = type;
			}
			return dictionary;
		}

		public static void ClearUnlockedRituals()
		{
			foreach (UpgradeSystem.Type type in UpgradeSystem.SecondaryRitualPairs)
			{
				UpgradeSystem.UnlockedUpgrades.Remove(type);
			}
			foreach (UpgradeSystem.Type type2 in UpgradeSystem.SecondaryRituals)
			{
				UpgradeSystem.UnlockedUpgrades.Remove(type2);
			}
			UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.PrimaryRitual1);
		}

		public static void PlayNotification(string message)
		{
			if (NotificationCentre.Instance)
			{
				NotificationCentre.Instance.PlayGenericNotification(message, NotificationBase.Flair.None);
			}
		}

		public static void ClearBaseTrees()
		{
			try
			{
				FollowerLocation followerLocation = FollowerLocation.Base;
				foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, StructureBrain.TYPES.TREE, false))
				{
					structureBrain.Remove();
				}
				CultUtils.PlayNotification("Trees cleared!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clear trees: " + ex.Message);
				CultUtils.PlayNotification("Failed to clear trees!");
			}
		}

		public static void ClearBaseRubble()
		{
			try
			{
				int num = 0;
				foreach (StructureBrain.TYPES types in new StructureBrain.TYPES[]
				{
					StructureBrain.TYPES.RUBBLE,
					StructureBrain.TYPES.RUBBLE_BIG,
					StructureBrain.TYPES.ROCK,
					StructureBrain.TYPES.BLOOD_STONE
				})
				{
					FollowerLocation followerLocation = FollowerLocation.Base;
					foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, types, false))
					{
						structureBrain.Remove();
						num++;
					}
				}
				CultUtils.PlayNotification(string.Format("Rubble cleared! ({0} items)", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clear rubble: " + ex.Message);
				CultUtils.PlayNotification("Failed to clear rubble!");
			}
		}

		private static bool IsLandscapeType(string typeName)
		{
			return typeName.Contains("GRASS") || typeName.Contains("WEED") || typeName.Contains("BUSH") || typeName.Contains("SHRUB") || typeName.Contains("FERN") || typeName.Contains("PLANT") || typeName.Contains("FOLIAGE") || typeName.Contains("STUMP") || typeName.Contains("SAPLING") || typeName.Contains("DECORATION_ENVIRONMENT");
		}

		public static void ClearBaseGrass()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(FollowerLocation)))
				{
					FollowerLocation followerLocation = (FollowerLocation)obj;
					foreach (object obj2 in Enum.GetValues(typeof(StructureBrain.TYPES)))
					{
						if (CultUtils.IsLandscapeType(obj2.ToString()))
						{
							try
							{
								foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, (StructureBrain.TYPES)obj2, false))
								{
									structureBrain.Remove();
									num++;
								}
							}
							catch
							{
							}
						}
					}
				}
				foreach (object obj3 in Enum.GetValues(typeof(StructureBrain.TYPES)))
				{
					if (CultUtils.IsLandscapeType(obj3.ToString()))
					{
						try
						{
							foreach (StructureBrain structureBrain2 in StructureManager.GetAllStructuresOfType((StructureBrain.TYPES)obj3, false, false))
							{
								structureBrain2.Remove();
								num++;
							}
						}
						catch
						{
						}
					}
				}
				CultUtils.PlayNotification(string.Format("Landscape cleared! ({0} items)", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clear landscape: " + ex.Message);
				CultUtils.PlayNotification("Failed to clear landscape!");
			}
		}

		public static void ClearVomit()
		{
			FollowerLocation followerLocation = FollowerLocation.Base;
			foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, StructureBrain.TYPES.VOMIT, false))
			{
				structureBrain.Remove();
			}
			CultUtils.PlayNotification("Vomit cleared!");
		}

		public static async void ClearPoop()
		{
			int poopCount = 0;
			FollowerLocation followerLocation = FollowerLocation.Base;
			foreach (StructureBrain structureBrain in StructureManager.GetAllStructuresOfType(in followerLocation, StructureBrain.TYPES.POOP, false))
			{
				structureBrain.Remove();
				int num = poopCount;
				poopCount = num + 1;
			}
			foreach (object obj in Enum.GetValues(typeof(StructureBrain.TYPES)))
			{
				if (obj.ToString().Contains("POOP") && (StructureBrain.TYPES)obj != StructureBrain.TYPES.POOP)
				{
					try
					{
						followerLocation = FollowerLocation.Base;
						foreach (StructureBrain structureBrain2 in StructureManager.GetAllStructuresOfType(in followerLocation, (StructureBrain.TYPES)obj, false))
						{
							structureBrain2.Remove();
							int num = poopCount;
							poopCount = num + 1;
						}
					}
					catch
					{
					}
				}
			}
			try
			{
				foreach (Interaction_Daycare interaction_Daycare in Interaction_Daycare.Daycares)
				{
					if (!(interaction_Daycare == null) && !(interaction_Daycare.Structure == null))
					{
						List<InventoryItem> inventory = interaction_Daycare.Structure.Inventory;
						if (inventory != null && inventory.Count > 0)
						{
							foreach (InventoryItem inventoryItem in inventory)
							{
								if (inventoryItem.type == 39 && inventoryItem.quantity > 0)
								{
									CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, inventoryItem.quantity);
									int num = poopCount;
									poopCount = num + 1;
								}
							}
							inventory.RemoveAll((InventoryItem i) => i.type == 39);
							interaction_Daycare.UpdatePoopStates();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to clear nursery poop: " + ex.Message);
			}
			CultUtils.ClearJanitorStations();
			await AsyncHelper.WaitSeconds(1);
			foreach (PickUp pickUp in PickUp.PickUps)
			{
				if (pickUp.type == InventoryItem.ITEM_TYPE.POOP)
				{
					pickUp.PickMeUp();
				}
			}
			CultUtils.PlayNotification(string.Format("Poop cleared! ({0} sources)", poopCount));
		}

		public static void ClearJanitorStations()
		{
			try
			{
				int num = 0;
				int num2 = 0;
				List<Structures_JanitorStation> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_JanitorStation>();
				foreach (Structures_JanitorStation structures_JanitorStation in allStructuresOfType)
				{
					if (structures_JanitorStation != null && structures_JanitorStation.SoulCount > 0)
					{
						num2 += structures_JanitorStation.SoulCount;
						num++;
					}
				}
				if (num2 > 0 && PlayerFarming.Instance != null)
				{
					foreach (Structures_JanitorStation structures_JanitorStation2 in allStructuresOfType)
					{
						if (structures_JanitorStation2 != null)
						{
							structures_JanitorStation2.SoulCount = 0;
						}
					}
					foreach (JanitorStation janitorStation in JanitorStation.JanitorStations)
					{
						if (janitorStation != null)
						{
							Traverse.Create(janitorStation).Field("previousSoulCount").SetValue(-1);
						}
					}
					PlayerFarming.Instance.playerChoreXPBarController.AddChoreXP(PlayerFarming.Instance, (float)num2);
				}
				if (num > 0 || num2 > 0)
				{
					CultUtils.PlayNotification(string.Format("Janitor stations collected! ({0} XP from {1} stations)", num2, num));
				}
				else
				{
					CultUtils.PlayNotification("No janitor stations with XP found!");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to clear janitor stations: " + ex.Message);
			}
		}

		public static void SetFollowerFaith(FollowerInfo followerInfo, float value)
		{
			followerInfo.Faith = Mathf.Clamp(value, 0f, 100f);
		}

		public static void SetFollowerSatiation(FollowerInfo followerInfo, float value)
		{
			followerInfo.Satiation = Mathf.Clamp(value, 0f, 100f);
		}

		public static void SetFollowerStarvation(FollowerInfo followerInfo, float value)
		{
			if (value > 0f)
			{
				followerInfo.Starvation = Mathf.Clamp(value, 0f, 75f);
				return;
			}
			followerInfo.Starvation = 0f;
		}

		public static void ConvertDissenting(FollowerInfo followerInfo)
		{
			try
			{
				if (followerInfo != null)
				{
					if (followerInfo.HasThought(Thought.Dissenter))
					{
						Follower followerFromInfo = CultUtils.GetFollowerFromInfo(followerInfo);
						if (followerFromInfo != null)
						{
							followerFromInfo.RemoveCursedState(Thought.Dissenter);
						}
						CultUtils.SetFollowerFaith(followerInfo, 100f);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] ConvertDissenting error: " + ex.Message);
			}
		}

		public static Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>> GetAllDoctrinePairs()
		{
			Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>> dictionary = new Dictionary<SermonCategory, List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>>();
			foreach (object obj in Enum.GetValues(typeof(SermonCategory)))
			{
				SermonCategory sermonCategory = (SermonCategory)obj;
				List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>> list = new List<Tuple<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>>();
				if (DoctrineUpgradeSystem.GetSermonReward(sermonCategory, 1, true) != DoctrineUpgradeSystem.DoctrineType.None)
				{
					for (int i = 1; i <= 4; i++)
					{
						list.Add(Tuple.Create<DoctrineUpgradeSystem.DoctrineType, DoctrineUpgradeSystem.DoctrineType>(DoctrineUpgradeSystem.GetSermonReward((SermonCategory)obj, i, true), DoctrineUpgradeSystem.GetSermonReward((SermonCategory)obj, i, false)));
					}
				}
				dictionary[sermonCategory] = list;
			}
			return dictionary;
		}

		public static void ClearOuthouses()
		{
			FollowerLocation followerLocation = FollowerLocation.Base;
			List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(in followerLocation, StructureBrain.TYPES.OUTHOUSE, false);
			followerLocation = FollowerLocation.Base;
			List<StructureBrain> allStructuresOfType2 = StructureManager.GetAllStructuresOfType(in followerLocation, StructureBrain.TYPES.OUTHOUSE_2, false);
			StructureBrain[] array = CheatUtils.Concat<StructureBrain>(allStructuresOfType.ToArray(), allStructuresOfType2.ToArray());
			int num = 0;
			StructureBrain[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Structures_Outhouse structures_Outhouse = array2[i] as Structures_Outhouse;
				if (structures_Outhouse != null)
				{
					int poopCount = structures_Outhouse.GetPoopCount();
					if (poopCount > 0)
					{
						CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.POOP, poopCount);
						num += poopCount;
					}
					structures_Outhouse.Data.Inventory.Clear();
				}
			}
			try
			{
				foreach (Interaction_Outhouse interaction_Outhouse in Interaction_Outhouse.Outhouses)
				{
					if (interaction_Outhouse != null)
					{
						interaction_Outhouse.UpdateGauge();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to update outhouse gauges: " + ex.Message);
			}
			CultUtils.PlayNotification((num > 0) ? string.Format("Outhouses cleared! ({0} poop)", num) : "Outhouses already clean!");
		}

		public static void MaximizeSatiationAndRemoveStarvation(FollowerInfo followerInfo)
		{
			try
			{
				if (followerInfo != null)
				{
					CultUtils.SetFollowerSatiation(followerInfo, 100f);
					CultUtils.SetFollowerStarvation(followerInfo, 0f);
					if (followerInfo.HasThought(Thought.BecomeStarving))
					{
						Follower followerFromInfo = CultUtils.GetFollowerFromInfo(followerInfo);
						if (followerFromInfo != null)
						{
							followerFromInfo.RemoveCursedState(Thought.BecomeStarving);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] MaximizeSatiationAndRemoveStarvation error: " + ex.Message);
			}
		}

		public static void AddInventoryItem(InventoryItem.ITEM_TYPE type, int amount)
		{
			try
			{
				Inventory.AddItem((int)type, amount, false);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("[CheatMenu] AddInventoryItem fallback for {0}: {1}", type, ex.Message));
				try
				{
					Inventory.AddItem(type, amount, false);
				}
				catch (Exception ex2)
				{
					Debug.LogWarning(string.Format("[CheatMenu] AddInventoryItem failed for {0}: {1}", type, ex2.Message));
				}
			}
		}

		public static float CalculateCurrentFaith()
		{
			float num = 0f;
			List<ThoughtData> value = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
			if (value == null)
			{
				return num;
			}
			foreach (ThoughtData thoughtData in value)
			{
				int i = 0;
				float num2 = -1f;
				while (i <= thoughtData.Quantity)
				{
					if (i == 0)
					{
						num2 += thoughtData.Modifier;
					}
					else
					{
						num2 += (float)thoughtData.StackModifier;
					}
					i++;
				}
				num += num2;
			}
			return num;
		}

		public static float GetCurrentFaith()
		{
			return CultFaithManager.CurrentFaith;
		}

		public static ThoughtData HasThought(FollowerInfo follower, Thought thoughtType)
		{
			foreach (ThoughtData thoughtData in follower.Thoughts)
			{
				if (thoughtData.ThoughtType == thoughtType)
				{
					return thoughtData;
				}
			}
			return null;
		}

		public static List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>> GetRitualPairs()
		{
			List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>> list = new List<Tuple<UpgradeSystem.Type, UpgradeSystem.Type>>();
			for (int i = 0; i < UpgradeSystem.SecondaryRitualPairs.Length; i += 2)
			{
				list.Add(new Tuple<UpgradeSystem.Type, UpgradeSystem.Type>(UpgradeSystem.SecondaryRitualPairs[i], UpgradeSystem.SecondaryRitualPairs[i + 1]));
			}
			return list;
		}

		public static void RenameCult(Action<string> onNameConfirmed = null)
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("RenameCult", Array.Empty<object>()).GetValue();
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to rename cult: " + ex.Message);
			}
		}

		public static void TurnFollowerYoung(FollowerInfo follower)
		{
			try
			{
				if (follower == null)
				{
					Debug.LogWarning("[CheatMenu] TurnFollowerYoung: follower is null");
				}
				else
				{
					Follower followerFromInfo = CultUtils.GetFollowerFromInfo(follower);
					if (followerFromInfo == null)
					{
						Debug.LogWarning(string.Format("[CheatMenu] TurnFollowerYoung: Could not find follower {0}", follower.ID));
					}
					else
					{
						followerFromInfo.RemoveCursedState(Thought.OldAge);
						followerFromInfo.Brain.ClearThought(Thought.OldAge);
						follower.Age = 0;
						follower.OldAge = false;
						followerFromInfo.Brain.CheckChangeState();
						DataManager.Instance.Followers_Elderly_IDs.Remove(follower.ID);
						if (followerFromInfo.Outfit != null && followerFromInfo.Outfit.CurrentOutfit == FollowerOutfitType.Old)
						{
							try
							{
								followerFromInfo.SetOutfit(FollowerOutfitType.Follower, false, Thought.None);
							}
							catch (Exception ex)
							{
								Debug.LogWarning(string.Format("[CheatMenu] Failed to set young outfit for follower {0}: {1}", follower.ID, ex.Message));
							}
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CheatMenu] TurnFollowerYoung error: " + ex2.Message);
			}
		}

		public static void TurnFollowerOld(FollowerInfo follower)
		{
			try
			{
				if (follower == null)
				{
					Debug.LogWarning("[CheatMenu] TurnFollowerOld: follower is null");
				}
				else
				{
					Follower followerFromInfo = CultUtils.GetFollowerFromInfo(follower);
					if (followerFromInfo == null)
					{
						Debug.LogWarning(string.Format("[CheatMenu] TurnFollowerOld: Could not find follower {0}", follower.ID));
					}
					else
					{
						CultFaithManager.RemoveThought(Thought.OldAge);
						try
						{
							followerFromInfo.Brain.ApplyCurseState(Thought.OldAge, Thought.None, false);
						}
						catch (Exception ex)
						{
							Debug.LogWarning(string.Format("[CheatMenu] Failed to apply old age curse for follower {0}: {1}", follower.ID, ex.Message));
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CheatMenu] TurnFollowerOld error: " + ex2.Message);
			}
		}

		public static FollowerInfo GetFollowerInfo(Follower follower)
		{
			return follower.Brain._directInfoAccess;
		}

		public static Follower GetFollowerFromInfo(FollowerInfo follower)
		{
			Follower follower2;
			try
			{
				if (follower == null)
				{
					follower2 = null;
				}
				else
				{
					follower2 = FollowerManager.FindFollowerByID(follower.ID);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] GetFollowerFromInfo error for ID " + ((follower != null) ? follower.ID.ToString() : "null") + ": " + ex.Message);
				follower2 = null;
			}
			return follower2;
		}

		public static void SetFollowerIllness(FollowerInfo follower, float value)
		{
			follower.Illness = Mathf.Clamp(value, 0f, 100f);
		}

		public static void ClearAllThoughts()
		{
			List<ThoughtData> value = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
			if (value != null)
			{
				value.Clear();
			}
			CultFaithManager.GetFaith(0f, 0f, true, NotificationBase.Flair.Positive, "Cleared follower thoughts!", -1, Array.Empty<string>());
		}

		public static void ClearAndAddPositiveFollowerThought()
		{
			List<ThoughtData> value = Traverse.Create(typeof(CultFaithManager)).Field("Thoughts").GetValue<List<ThoughtData>>();
			if (value != null)
			{
				value.Clear();
			}
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultFaithManager.AddThought(Thought.TestPositive, followerInfo.ID, 999f, Array.Empty<string>());
			}
			ThoughtData data = FollowerThoughts.GetData(Thought.TestPositive);
			CultFaithManager.GetFaith(0f, data.Modifier, true, NotificationBase.Flair.Positive, "Cleared follower thoughts and added positive test thougtht!", -1, Array.Empty<string>());
		}

		public static void SetFollowerHunger(FollowerInfo follower, float value)
		{
			follower.Satiation = Mathf.Clamp(value, 0f, 100f);
		}

		public static Follower GetFollower(FollowerInfo followerInfo)
		{
			return FollowerManager.FindFollowerByID(followerInfo.ID);
		}

		public static void KillFollower(Follower follower, bool withNotification = false)
		{
			NotificationCentre.NotificationType notificationType = (withNotification ? NotificationCentre.NotificationType.Died : NotificationCentre.NotificationType.None);
			follower.Die(notificationType, false, 1, "dead", null, null, false);
		}

		public static void ReviveFollower(FollowerInfo follower)
		{
			if (DataManager.Instance.Followers_Dead_IDs.Contains(follower.ID))
			{
				DataManager.Instance.Followers_Dead.Remove(follower);
				DataManager.Instance.Followers_Dead_IDs.Remove(follower.ID);
				Follower follower2 = FollowerManager.CreateNewFollower(follower, PlayerFarming.Instance.transform.position, false);
				if (follower.Age > follower.LifeExpectancy)
				{
					follower.LifeExpectancy = follower.Age + global::UnityEngine.Random.Range(20, 30);
				}
				else
				{
					follower.LifeExpectancy += global::UnityEngine.Random.Range(20, 30);
				}
				follower2.Brain.ResetStats();
			}
		}

		public static void SpawnFollower(FollowerRole role)
		{
			try
			{
				Follower follower = FollowerManager.CreateNewFollower(PlayerFarming.Location, PlayerFarming.Instance.transform.position, false);
				if (follower == null || follower.Brain == null || follower.Brain.Info == null)
				{
					Debug.LogWarning("[CheatMenu] Failed to spawn follower - null reference");
					CultUtils.PlayNotification("Failed to spawn follower!");
				}
				else
				{
					follower.Brain.Info.FollowerRole = role;
					if (follower.Outfit != null && follower.gameObject != null && follower.gameObject.activeInHierarchy)
					{
						follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
						try
						{
							follower.SetOutfit(FollowerOutfitType.Follower, false, Thought.None);
						}
						catch (Exception ex)
						{
							Debug.LogWarning("[CheatMenu] Failed to set follower outfit: " + ex.Message);
						}
					}
					if (role == FollowerRole.Worker)
					{
						follower.Brain.Info.WorkerPriority = WorkerPriority.Rubble;
						follower.Brain.Stats.WorkerBeenGivenOrders = true;
						try
						{
							if (follower.Brain != null && follower.Brain.Info != null)
							{
								follower.Brain.CheckChangeState();
							}
						}
						catch (Exception ex2)
						{
							Debug.LogWarning("[CheatMenu] Failed to change worker state: " + ex2.Message);
						}
					}
					FollowerInfo followerInfo = CultUtils.GetFollowerInfo(follower);
					if (followerInfo != null)
					{
						CultUtils.SetFollowerIllness(followerInfo, 0f);
						CultUtils.SetFollowerHunger(followerInfo, 100f);
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.LogWarning("[CheatMenu] SpawnFollower error: " + ex3.Message);
				CultUtils.PlayNotification("Failed to spawn follower!");
			}
		}

		public static void ClearBodies()
		{
			int num = 0;
			foreach (DeadWorshipper deadWorshipper in new List<DeadWorshipper>(DeadWorshipper.DeadWorshippers))
			{
				if (!(deadWorshipper == null) && deadWorshipper.followerInfo != null)
				{
					try
					{
						CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 5);
						CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, 2);
						if (deadWorshipper.followerInfo.Necklace != InventoryItem.ITEM_TYPE.NONE)
						{
							CultUtils.AddInventoryItem(deadWorshipper.followerInfo.Necklace, 1);
						}
						deadWorshipper.followerInfo.Necklace = InventoryItem.ITEM_TYPE.NONE;
						StructureManager.RemoveStructure(deadWorshipper.Structure.Brain);
						num++;
					}
					catch (Exception ex)
					{
						Debug.LogWarning("[CheatMenu] Failed to clear dead follower: " + ex.Message);
					}
				}
			}
			int num2 = 0;
			try
			{
				foreach (Interaction_Ranchable interaction_Ranchable in new List<Interaction_Ranchable>(Interaction_Ranchable.DeadRanchables))
				{
					if (!(interaction_Ranchable == null) && interaction_Ranchable.Animal != null && interaction_Ranchable.CurrentState == Interaction_Ranchable.State.Dead)
					{
						try
						{
							foreach (InventoryItem inventoryItem in Interaction_Ranchable.GetMeatLoot(interaction_Ranchable.Animal))
							{
								CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)inventoryItem.type, inventoryItem.quantity);
							}
							CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.BONE, Structures_Ranch.GetAnimalGrowthState(interaction_Ranchable.Animal));
							if (interaction_Ranchable.ranch != null)
							{
								interaction_Ranchable.ranch.Brain.RemoveAnimal(interaction_Ranchable.Animal);
							}
							DataManager.Instance.BreakingOutAnimals.Remove(interaction_Ranchable.Animal);
							DataManager.Instance.DeadAnimalsTemporaryList.Add(interaction_Ranchable.Animal);
							global::UnityEngine.Object.Destroy(interaction_Ranchable.gameObject);
							num2++;
						}
						catch (Exception ex2)
						{
							Debug.LogWarning("[CheatMenu] Failed to clear dead animal: " + ex2.Message);
						}
					}
				}
			}
			catch (Exception ex3)
			{
				Debug.LogWarning("[CheatMenu] Failed to clear dead animals: " + ex3.Message);
			}
			if (num == 0 && num2 == 0)
			{
				CultUtils.PlayNotification("No dead bodies found!");
				return;
			}
			CultUtils.PlayNotification(string.Format("Bodies cleared! ({0} follower, {1} animal)", num, num2));
		}

		public static void CureIllness(FollowerInfo follower)
		{
			try
			{
				if (follower != null)
				{
					follower.Illness = 0f;
					Follower followerFromInfo = CultUtils.GetFollowerFromInfo(follower);
					if (followerFromInfo != null)
					{
						followerFromInfo.RemoveCursedState(Thought.Ill);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] CureIllness error: " + ex.Message);
			}
		}

		public static void ForceGrowAllAnimals()
		{
			try
			{
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in AnimalData.GetAnimals())
				{
					if (ranchable_Animal.Age < 2)
					{
						ranchable_Animal.Age = 2;
					}
					ranchable_Animal.GrowthStage = 0;
					ranchable_Animal.WorkedReady = true;
					ranchable_Animal.WorkedToday = false;
					ranchable_Animal.Satiation = 100f;
					Interaction_Ranchable animal = Interaction_Ranch.GetAnimal(ranchable_Animal);
					if (animal != null)
					{
						animal.UpdateSkin();
					}
					num++;
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Force grew {0} animal(s)!", num) : "No animals to grow!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] ForceGrowAllAnimals error: " + ex.Message);
				CultUtils.PlayNotification("Failed to force grow animals!");
			}
		}

		public static void SpawnFriendlyWolf()
		{
			CultUtils.SpawnFriendlyWolfInternal(true);
		}

		private static void SpawnFriendlyWolfInternal(bool userInitiated)
		{
			try
			{
				if (PlayerFarming.Instance == null)
				{
					if (userInitiated)
					{
						CultUtils.PlayNotification("Must be in game!");
					}
				}
				else if (CultUtils.FriendlyWolf != null)
				{
					if (userInitiated)
					{
						CultUtils.PlayNotification("You already have a friendly wolf!");
					}
				}
				else
				{
					Vector3 position = PlayerFarming.Instance.transform.position;
					Interaction_WolfBase.WolfTarget = 1;
					Interaction_WolfBase.WolfCount = 0;
					Interaction_WolfBase.WolfFled = 0;
					Interaction_WolfBase.WolfDied = 0;
					CultUtils.s_wolfRespawning = true;
					Interaction_WolfBase.SpawnWolf(position, null, false, delegate(Interaction_WolfBase wolf)
					{
						CultUtils.s_wolfRespawning = false;
						if (wolf != null)
						{
							wolf.CurrentState = Interaction_WolfBase.State.Animating;
							wolf.SecondaryInteractable = false;
							CultUtils.FriendlyWolf = wolf;
							CultUtils.s_wolfCurrentAnim = "";
							CultUtils.s_wolfIsRunning = false;
							CultUtils.s_wolfVelocity = Vector3.zero;
							CultUtils.s_wolfAttackCooldown = 0f;
							CultUtils.s_wolfPetting = false;
							CultUtils.s_wolfAnimHoldTimer = 0f;
							CultUtils.s_wolfShouldExist = true;
							CultUtils.s_wolfAttackAnimName = null;
							CultUtils.s_wolfCombatTransitionTimer = 0f;
							CultUtils.DiscoverWolfAttackAnimation(wolf);
							Interaction_WolfBase.ResetWolvesEnounterData();
							Debug.Log("[CheatMenu] Friendly wolf spawned and following player!");
						}
					});
					if (userInitiated)
					{
						CultUtils.PlayNotification("Friendly wolf spawned!");
					}
				}
			}
			catch (Exception ex)
			{
				CultUtils.s_wolfRespawning = false;
				Debug.LogWarning("[CheatMenu] SpawnFriendlyWolf error: " + ex.Message);
				if (userInitiated)
				{
					CultUtils.PlayNotification("Failed to spawn friendly wolf!");
				}
			}
		}

		public static void DismissFriendlyWolf()
		{
			try
			{
				CultUtils.s_wolfShouldExist = false;
				CultUtils.s_wolfRespawning = false;
				if (CultUtils.FriendlyWolf != null)
				{
					global::UnityEngine.Object.Destroy(CultUtils.FriendlyWolf.gameObject);
					CultUtils.FriendlyWolf = null;
					CultUtils.s_wolfPetting = false;
					CultUtils.PlayNotification("Friendly wolf dismissed!");
				}
				else
				{
					List<Interaction_WolfBase> list = new List<Interaction_WolfBase>(Interaction_WolfBase.Wolfs);
					int num = 0;
					foreach (Interaction_WolfBase interaction_WolfBase in list)
					{
						if (interaction_WolfBase != null)
						{
							global::UnityEngine.Object.Destroy(interaction_WolfBase.gameObject);
							num++;
						}
					}
					Interaction_WolfBase.Wolfs.Clear();
					Interaction_WolfBase.ResetWolvesEnounterData();
					CultUtils.PlayNotification((num > 0) ? string.Format("Dismissed {0} wolf/wolves!", num) : "No wolves to dismiss!");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] DismissFriendlyWolf error: " + ex.Message);
				CultUtils.PlayNotification("Failed to dismiss wolf!");
			}
		}

		public static void PetFriendlyWolf()
		{
			try
			{
				if (CultUtils.FriendlyWolf == null)
				{
					CultUtils.PlayNotification("No friendly wolf to pet!");
				}
				else if (PlayerFarming.Instance == null)
				{
					CultUtils.PlayNotification("Must be in game!");
				}
				else if (!CultUtils.s_wolfPetting)
				{
					GameManager.GetInstance().StartCoroutine(CultUtils.PetWolfCoroutine());
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] PetFriendlyWolf error: " + ex.Message);
				CultUtils.PlayNotification("Failed to pet wolf!");
			}
		}

		private static IEnumerator PetWolfCoroutine()
		{
			return new CultUtils.<PetWolfCoroutine>d__76(0);
		}

		[Update]
		public static void UpdateFriendlyWolf()
		{
			if (!CultUtils.s_wolfShouldExist)
			{
				return;
			}
			if (CultUtils.s_wolfRespawning)
			{
				return;
			}
			if (PlayerFarming.Instance == null)
			{
				return;
			}
			if (CultUtils.FriendlyWolf == null || !CultUtils.FriendlyWolf)
			{
				CultUtils.FriendlyWolf = null;
				CultUtils.s_wolfPetting = false;
				CultUtils.s_wolfTargetEnemy = null;
				CultUtils.s_wolfCurrentAnim = "";
				CultUtils.s_wolfIsRunning = false;
				CultUtils.s_wolfVelocity = Vector3.zero;
				CultUtils.s_wolfAttackCooldown = 0f;
				CultUtils.s_wolfAnimHoldTimer = 0f;
				CultUtils.s_wolfAttackAnimName = null;
				CultUtils.s_wolfCombatTransitionTimer = 0f;
				Debug.Log("[CheatMenu] Friendly wolf lost (scene change) - respawning...");
				CultUtils.SpawnFriendlyWolfInternal(false);
			}
		}

		public static bool HandleFriendlyWolfUpdate(Interaction_WolfBase wolf)
		{
			if (wolf != CultUtils.FriendlyWolf || CultUtils.FriendlyWolf == null)
			{
				return true;
			}
			if (PlayerFarming.Instance == null)
			{
				return false;
			}
			if (CultUtils.s_wolfPetting)
			{
				return false;
			}
			try
			{
				Vector3 position = PlayerFarming.Instance.transform.position;
				Vector3 position2 = wolf.transform.position;
				float num = Vector3.Distance(position2, position);
				SkeletonAnimation spine = wolf.Spine;
				StateMachine value = Traverse.Create(wolf).Field("stateMachine").GetValue<StateMachine>();
				UnitObject unitObject = wolf.UnitObject;
				if (unitObject != null)
				{
					unitObject.ClearPaths();
					unitObject.UsePathing = false;
				}
				bool flag = GameManager.IsDungeon(PlayerFarming.Location);
				bool flag2 = false;
				if (flag && CultUtils.WolfDungeonCombat)
				{
					CultUtils.s_wolfAttackCooldown -= Time.deltaTime;
					CultUtils.s_wolfCombatTransitionTimer -= Time.deltaTime;
					if (CultUtils.s_wolfTargetEnemy != null)
					{
						bool flag3 = false;
						try
						{
							flag3 = !CultUtils.s_wolfTargetEnemy || CultUtils.s_wolfTargetEnemy.HP <= 0f || !CultUtils.s_wolfTargetEnemy.enabled;
						}
						catch
						{
							flag3 = true;
						}
						if (flag3)
						{
							CultUtils.s_wolfTargetEnemy = null;
							CultUtils.s_wolfCombatTransitionTimer = 0.7f;
						}
					}
					if (CultUtils.s_wolfTargetEnemy == null)
					{
						CultUtils.s_wolfTargetEnemy = CultUtils.FindClosestEnemy(position2, 6f);
					}
					if (CultUtils.s_wolfTargetEnemy != null)
					{
						flag2 = true;
						Vector3 position3 = CultUtils.s_wolfTargetEnemy.transform.position;
						float num2 = Vector3.Distance(position2, position3);
						if (value != null)
						{
							float angle = Utils.GetAngle(position2, position3);
							value.facingAngle = angle;
							value.LookAngle = angle;
						}
						if (CultUtils.s_wolfCombatTransitionTimer > 0f)
						{
							wolf.transform.position = Vector3.SmoothDamp(position2, position3, ref CultUtils.s_wolfVelocity, 0.24499999f, 5.85f, Time.deltaTime);
							CultUtils.SetWolfAnimation(spine, "run", false);
						}
						else
						{
							string text = CultUtils.s_wolfAttackAnimName ?? "idle";
							float num3 = ((CultUtils.s_wolfCurrentAnim == text || CultUtils.s_wolfCurrentAnim == "idle") ? 2.25f : 1.5f);
							if (num2 <= num3)
							{
								if (CultUtils.s_wolfAttackCooldown <= 0f)
								{
									CultUtils.s_wolfTargetEnemy.DealDamage(2f, wolf.gameObject, position2, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
									CultUtils.s_wolfAttackCooldown = 1.8f;
									try
									{
										AudioManager.Instance.PlayOneShot("event:/dlc/env/dog/dog_basic_attack_bite", wolf.transform.position);
									}
									catch
									{
									}
									CultUtils.SetWolfAnimation(spine, text, true);
								}
								else if (CultUtils.s_wolfAttackCooldown < 0.54f)
								{
									CultUtils.SetWolfAnimation(spine, "idle", false);
								}
								if (num2 > 1.5f)
								{
									wolf.transform.position = Vector3.SmoothDamp(position2, position3, ref CultUtils.s_wolfVelocity, 0.35f, 2.25f, Time.deltaTime);
								}
								else
								{
									CultUtils.s_wolfVelocity = Vector3.zero;
								}
							}
							else
							{
								wolf.transform.position = Vector3.SmoothDamp(position2, position3, ref CultUtils.s_wolfVelocity, 0.24499999f, 5.85f, Time.deltaTime);
								CultUtils.SetWolfAnimation(spine, "run", false);
							}
						}
					}
				}
				else
				{
					CultUtils.s_wolfTargetEnemy = null;
				}
				if (!flag2)
				{
					if (num > 8f)
					{
						Vector3 vector = position;
						try
						{
							float num4 = (PlayerFarming.Instance.state.facingAngle + 180f) * 0.017453292f;
							vector += new Vector3(Mathf.Cos(num4) * 2f, Mathf.Sin(num4) * 2f, 0f);
						}
						catch
						{
							vector += new Vector3(-2f, 0f, 0f);
						}
						wolf.transform.position = vector;
						CultUtils.s_wolfVelocity = Vector3.zero;
						CultUtils.s_wolfIsRunning = false;
						CultUtils.SetWolfAnimation(spine, "idle", true);
					}
					else
					{
						if (!CultUtils.s_wolfIsRunning && num > 3.5f)
						{
							CultUtils.s_wolfIsRunning = true;
						}
						else if (CultUtils.s_wolfIsRunning && num < 1.8f)
						{
							CultUtils.s_wolfIsRunning = false;
							CultUtils.s_wolfVelocity = Vector3.zero;
						}
						if (CultUtils.s_wolfIsRunning)
						{
							Vector3 vector2 = position;
							wolf.transform.position = Vector3.SmoothDamp(position2, vector2, ref CultUtils.s_wolfVelocity, 0.35f, 4.5f, Time.deltaTime);
							if (value != null)
							{
								float angle2 = Utils.GetAngle(position2, position);
								value.facingAngle = angle2;
								value.LookAngle = angle2;
							}
							CultUtils.SetWolfAnimation(spine, "run", false);
						}
						else
						{
							if (value != null)
							{
								float angle3 = Utils.GetAngle(position2, position);
								value.facingAngle = angle3;
								value.LookAngle = angle3;
							}
							CultUtils.SetWolfAnimation(spine, "idle", false);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] FriendlyWolf update error: " + ex.Message);
			}
			return false;
		}

		private static Health FindClosestEnemy(Vector3 position, float maxRange)
		{
			Health health = null;
			float num = maxRange;
			try
			{
				foreach (Health health2 in Health.team2)
				{
					if (!(health2 == null) && health2)
					{
						try
						{
							if (health2.enabled && health2.HP > 0f)
							{
								float num2 = Vector3.Distance(position, health2.transform.position);
								if (num2 < num)
								{
									num = num2;
									health = health2;
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			return health;
		}

		private static void SetWolfAnimation(SkeletonAnimation spineAnim, string animName, bool force)
		{
			if (spineAnim == null)
			{
				return;
			}
			try
			{
				CultUtils.s_wolfAnimHoldTimer -= Time.deltaTime;
				string text = CultUtils.s_wolfAttackAnimName ?? "idle";
				bool flag = animName == text;
				if (force || (CultUtils.s_wolfCurrentAnim != animName && CultUtils.s_wolfAnimHoldTimer <= 0f))
				{
					spineAnim.AnimationState.SetAnimation(0, animName, !flag);
					CultUtils.s_wolfCurrentAnim = animName;
					if (flag)
					{
						CultUtils.s_wolfAnimHoldTimer = 0.35f;
					}
					else
					{
						CultUtils.s_wolfAnimHoldTimer = 0.15f;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] SetWolfAnimation error (" + animName + "): " + ex.Message);
			}
		}

		private static void DiscoverWolfAttackAnimation(Interaction_WolfBase wolf)
		{
			try
			{
				SkeletonAnimation spine = wolf.Spine;
				if (spine == null || spine.skeleton == null || spine.skeleton.Data == null)
				{
					CultUtils.s_wolfAttackAnimName = "charge_attack";
					Debug.LogWarning("[CheatMenu] Wolf skeleton not ready, defaulting to charge_attack");
				}
				else
				{
					ExposedList<Animation> animations = spine.skeleton.Data.Animations;
					List<string> list = new List<string>();
					foreach (Animation animation in animations)
					{
						list.Add(animation.Name);
					}
					Debug.Log("[CheatMenu] Wolf available animations: " + string.Join(", ", list));
					if (list.Contains("charge_attack"))
					{
						CultUtils.s_wolfAttackAnimName = "charge_attack";
					}
					else if (list.Contains("howl"))
					{
						CultUtils.s_wolfAttackAnimName = "howl";
					}
					else
					{
						CultUtils.s_wolfAttackAnimName = "idle";
					}
					Debug.Log("[CheatMenu] Wolf attack animation set to: " + CultUtils.s_wolfAttackAnimName);
				}
			}
			catch (Exception ex)
			{
				CultUtils.s_wolfAttackAnimName = "charge_attack";
				Debug.LogWarning("[CheatMenu] DiscoverWolfAttackAnimation error: " + ex.Message);
			}
		}

		public static void AscendAllAnimals()
		{
			try
			{
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in new List<StructuresData.Ranchable_Animal>(AnimalData.GetAnimals()))
				{
					if (ranchable_Animal != null)
					{
						Interaction_Ranchable animal = Interaction_Ranch.GetAnimal(ranchable_Animal);
						if (animal != null && animal.gameObject != null && animal.gameObject.activeInHierarchy)
						{
							try
							{
								animal.StartCoroutine(CultUtils.AscendAnimalCoroutine(animal, ranchable_Animal));
								goto IL_008E;
							}
							catch (Exception ex)
							{
								Debug.LogWarning("[CheatMenu] Failed to start ascend coroutine: " + ex.Message);
								CultUtils.RemoveAnimalImmediate(animal, ranchable_Animal);
								goto IL_008E;
							}
							goto IL_007B;
						}
						goto IL_007B;
						IL_008E:
						num++;
						continue;
						IL_007B:
						CultUtils.CollectAnimalResources(ranchable_Animal, Vector3.zero, false);
						CultUtils.RemoveAnimalData(animal, ranchable_Animal);
						goto IL_008E;
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Ascended {0} animal(s)!", num) : "No animals to ascend!");
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CheatMenu] AscendAllAnimals error: " + ex2.Message);
				CultUtils.PlayNotification("Failed to ascend animals!");
			}
		}

		private static IEnumerator AscendAnimalCoroutine(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal)
		{
			CultUtils.<AscendAnimalCoroutine>d__83 <AscendAnimalCoroutine>d__ = new CultUtils.<AscendAnimalCoroutine>d__83(0);
			<AscendAnimalCoroutine>d__.ranchable = ranchable;
			<AscendAnimalCoroutine>d__.animal = animal;
			return <AscendAnimalCoroutine>d__;
		}

		private static void CollectAnimalResources(StructuresData.Ranchable_Animal animal, Vector3 spawnPos, bool spawnVisual)
		{
			List<InventoryItem> meatLoot = Interaction_Ranchable.GetMeatLoot(animal);
			foreach (InventoryItem inventoryItem in meatLoot)
			{
				inventoryItem.quantity = Mathf.RoundToInt((float)inventoryItem.quantity);
			}
			if (spawnVisual && spawnPos != Vector3.zero)
			{
				int num = Mathf.Min((meatLoot.Count > 0) ? meatLoot[0].quantity : 0, 10);
				if (num > 0)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT, num, spawnPos, 4f, null);
				}
				using (List<InventoryItem>.Enumerator enumerator = meatLoot.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						InventoryItem inventoryItem2 = enumerator.Current;
						Inventory.AddItem(inventoryItem2.type, Mathf.Max(0, inventoryItem2.quantity - num), false);
					}
					goto IL_0109;
				}
			}
			foreach (InventoryItem inventoryItem3 in meatLoot)
			{
				CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)inventoryItem3.type, inventoryItem3.quantity);
			}
			IL_0109:
			foreach (InventoryItem inventoryItem4 in Interaction_Ranchable.GetWorkLoot(animal))
			{
				int num2 = inventoryItem4.quantity * 3;
				if (spawnVisual && spawnPos != Vector3.zero)
				{
					InventoryItem.Spawn((InventoryItem.ITEM_TYPE)inventoryItem4.type, num2, spawnPos, 4f, null);
				}
				else
				{
					CultUtils.AddInventoryItem((InventoryItem.ITEM_TYPE)inventoryItem4.type, num2);
				}
			}
			if (animal.Type == InventoryItem.ITEM_TYPE.ANIMAL_COW)
			{
				if (spawnVisual && spawnPos != Vector3.zero)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MILK, 5, spawnPos, 4f, null);
					return;
				}
				CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.MILK, 5);
			}
		}

		private static void RemoveAnimalImmediate(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal)
		{
			CultUtils.CollectAnimalResources(animal, Vector3.zero, false);
			CultUtils.RemoveAnimalData(ranchable, animal);
			if (ranchable != null && ranchable.gameObject != null)
			{
				global::UnityEngine.Object.Destroy(ranchable.gameObject);
			}
		}

		private static void RemoveAnimalData(Interaction_Ranchable ranchable, StructuresData.Ranchable_Animal animal)
		{
			if (ranchable != null && ranchable.ranch != null)
			{
				ranchable.ranch.Brain.RemoveAnimal(animal);
			}
			DataManager.Instance.BreakingOutAnimals.Remove(animal);
			DataManager.Instance.DeadAnimalsTemporaryList.Add(animal);
		}

		private static string GetAnimationAnimalName(InventoryItem.ITEM_TYPE type)
		{
			if (type <= InventoryItem.ITEM_TYPE.ANIMAL_SNAIL)
			{
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_GOAT)
				{
					return "goat";
				}
				switch (type)
				{
				case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE:
					return "turtle";
				case InventoryItem.ITEM_TYPE.ANIMAL_CRAB:
					return "crab";
				case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER:
					return "spider";
				case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL:
					return "snail";
				}
			}
			else
			{
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_COW)
				{
					return "cow";
				}
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_LLAMA)
				{
					return "llama";
				}
			}
			return "goat";
		}

		private static float GetAnimalHaloHeight(InventoryItem.ITEM_TYPE type)
		{
			if (type <= InventoryItem.ITEM_TYPE.ANIMAL_SNAIL)
			{
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_GOAT)
				{
					return 0.45f;
				}
				switch (type)
				{
				case InventoryItem.ITEM_TYPE.ANIMAL_TURTLE:
					return 0.25f;
				case InventoryItem.ITEM_TYPE.ANIMAL_CRAB:
					return 0.2f;
				case InventoryItem.ITEM_TYPE.ANIMAL_SPIDER:
					return 0.25f;
				case InventoryItem.ITEM_TYPE.ANIMAL_SNAIL:
					return 0.25f;
				}
			}
			else
			{
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_COW)
				{
					return 0.55f;
				}
				if (type == InventoryItem.ITEM_TYPE.ANIMAL_LLAMA)
				{
					return 0.55f;
				}
			}
			return 0.4f;
		}

		public static void AddHalosToAnimals()
		{
			try
			{
				CultUtils.RemoveAnimalHalos();
				int num = 0;
				foreach (StructuresData.Ranchable_Animal ranchable_Animal in AnimalData.GetAnimals())
				{
					if (ranchable_Animal != null)
					{
						Interaction_Ranchable animal = Interaction_Ranch.GetAnimal(ranchable_Animal);
						if (!(animal == null) && !(animal.gameObject == null) && animal.gameObject.activeInHierarchy)
						{
							try
							{
								float animalHaloHeight = CultUtils.GetAnimalHaloHeight(ranchable_Animal.Type);
								GameObject gameObject = new GameObject("CheatMenu_AnimalHalo");
								gameObject.transform.SetParent(animal.transform);
								gameObject.transform.localPosition = new Vector3(0f, animalHaloHeight, -5f);
								SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
								spriteRenderer.sprite = CultUtils.CreateHaloSprite();
								spriteRenderer.color = new Color(1f, 0.3f, 0.7f, 0.9f);
								spriteRenderer.sortingLayerName = "Above";
								spriteRenderer.sortingOrder = 1000;
								spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
								spriteRenderer.material.SetFloat("_PixelSnap", 0f);
								gameObject.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
								GameObject gameObject2 = new GameObject("HaloGlow");
								gameObject2.transform.SetParent(gameObject.transform);
								gameObject2.transform.localPosition = Vector3.zero;
								gameObject2.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
								SpriteRenderer spriteRenderer2 = gameObject2.AddComponent<SpriteRenderer>();
								spriteRenderer2.sprite = CultUtils.CreateGlowSprite();
								spriteRenderer2.color = new Color(1f, 0.2f, 0.6f, 0.5f);
								spriteRenderer2.sortingLayerName = "Above";
								spriteRenderer2.sortingOrder = 999;
								Shader shader = Shader.Find("Particles/Additive");
								if (shader == null)
								{
									shader = Shader.Find("Legacy Shaders/Particles/Additive");
								}
								if (shader == null)
								{
									shader = Shader.Find("Sprites/Default");
								}
								spriteRenderer2.material = new Material(shader);
								GameObject gameObject3 = new GameObject("HaloLight");
								gameObject3.transform.SetParent(gameObject.transform);
								gameObject3.transform.localPosition = Vector3.zero;
								Light light = gameObject3.AddComponent<Light>();
								light.type = LightType.Point;
								light.color = new Color(1f, 0.2f, 0.6f);
								light.intensity = 3f;
								light.range = 2.5f;
								light.renderMode = LightRenderMode.Auto;
								CultUtils.s_activeHalos.Add(gameObject);
								num++;
							}
							catch (Exception ex)
							{
								Debug.LogWarning("[CheatMenu] Failed to add halo to animal: " + ex.Message);
							}
						}
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("Glowing halos added to {0} animal(s)!", num) : "No animals to add halos to!");
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CheatMenu] AddHalosToAnimals error: " + ex2.Message);
				CultUtils.PlayNotification("Failed to add halos!");
			}
		}

		private static Sprite CreateHaloSprite()
		{
			if (CultUtils.s_haloSprite != null)
			{
				return CultUtils.s_haloSprite;
			}
			int num = 128;
			Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBA32, false);
			float num2 = (float)num / 2f;
			float num3 = 0.85f;
			float num4 = 0.4f;
			float num5 = 0.55f;
			float num6 = 0.2f;
			float num7 = 0.12f;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num8 = ((float)j - num2) / num2;
					float num9 = ((float)i - num2) / num2;
					float num10 = num8 * num8 / (num3 * num3) + num9 * num9 / (num4 * num4);
					float num11 = num8 * num8 / (num5 * num5) + num9 * num9 / (num6 * num6);
					float num12 = Mathf.Clamp01((1f - num10) / num7);
					float num13 = Mathf.Clamp01((num11 - 1f) / num7);
					float num14 = num12 * num13;
					float num15 = num8 * num8 / ((num3 + 0.2f) * (num3 + 0.2f)) + num9 * num9 / ((num4 + 0.15f) * (num4 + 0.15f));
					float num16 = Mathf.Clamp01(1f - num15) * 0.25f;
					float num17 = Mathf.Max(num14, num16);
					if (num17 > 0.01f)
					{
						float num18 = Mathf.Lerp(1f, 1f, num14);
						float num19 = Mathf.Lerp(0.4f, 0.2f, num14);
						float num20 = Mathf.Lerp(0.8f, 0.6f, num14);
						texture2D.SetPixel(j, i, new Color(num18, num19, num20, num17));
					}
					else
					{
						texture2D.SetPixel(j, i, Color.clear);
					}
				}
			}
			texture2D.Apply();
			CultUtils.s_haloSprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f), 128f);
			return CultUtils.s_haloSprite;
		}

		private static Sprite CreateGlowSprite()
		{
			if (CultUtils.s_glowSprite != null)
			{
				return CultUtils.s_glowSprite;
			}
			int num = 64;
			Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBA32, false);
			float num2 = (float)num / 2f;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num3 = ((float)j - num2) / num2;
					float num4 = ((float)i - num2) / num2;
					float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
					float num6 = Mathf.Clamp01(1f - num5);
					num6 *= num6;
					texture2D.SetPixel(j, i, new Color(1f, 1f, 1f, num6));
				}
			}
			texture2D.Apply();
			CultUtils.s_glowSprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f), 64f);
			return CultUtils.s_glowSprite;
		}

		private static void RemoveAnimalHalos()
		{
			foreach (GameObject gameObject in CultUtils.s_activeHalos)
			{
				if (gameObject != null)
				{
					global::UnityEngine.Object.Destroy(gameObject);
				}
			}
			CultUtils.s_activeHalos.Clear();
		}

		public static void ModifyFaith(float value, string notifMessage, bool shouldNotify = true)
		{
			NotificationBase.Flair flair = NotificationBase.Flair.Positive;
			float currentFaith = CultUtils.GetCurrentFaith();
			if (currentFaith > value)
			{
				flair = NotificationBase.Flair.Negative;
			}
			float num = CultUtils.CalculateCurrentFaith();
			float num2 = ((currentFaith < value) ? (value - num) : (num - value));
			NotificationData notificationData = (shouldNotify ? new NotificationData(notifMessage, 0f, -1, flair, new string[0]) : null);
			CultFaithManager.StaticFaith = num2;
			CultFaithManager.Instance.BarController.SetBarSize(value / 85f, true, true, notificationData);
		}

		public static Interaction_WolfBase FriendlyWolf = null;

		public static bool WolfDungeonCombat = true;

		private static string s_wolfCurrentAnim = "";

		private static bool s_wolfIsRunning = false;

		private static Vector3 s_wolfVelocity = Vector3.zero;

		private static float s_wolfAttackCooldown = 0f;

		private static bool s_wolfPetting = false;

		private static bool s_wolfShouldExist = false;

		private static bool s_wolfRespawning = false;

		private static float s_wolfAnimHoldTimer = 0f;

		private const float WOLF_ANIM_HOLD_MIN = 0.15f;

		private static string s_wolfAttackAnimName = null;

		private static float s_wolfCombatTransitionTimer = 0f;

		private const float WOLF_COMBAT_TRANSITION_MIN = 0.7f;

		private const float WOLF_FOLLOW_SPEED = 4.5f;

		private const float WOLF_TELEPORT_DIST = 8f;

		private const float WOLF_START_FOLLOW_DIST = 3.5f;

		private const float WOLF_STOP_FOLLOW_DIST = 1.8f;

		private const float WOLF_SMOOTH_TIME = 0.35f;

		private const float WOLF_ATTACK_RANGE = 1.5f;

		private const float WOLF_DETECT_RANGE = 6f;

		private const float WOLF_ATTACK_COOLDOWN = 1.8f;

		private const float WOLF_ATTACK_DAMAGE = 2f;

		private static Health s_wolfTargetEnemy = null;

		private static List<GameObject> s_activeHalos = new List<GameObject>();

		private static Sprite s_haloSprite;

		private static Sprite s_glowSprite;
	}
}
