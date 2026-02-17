using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.FOLLOWER)]
	public class FollowerDefinitions : IDefinition
	{
		[CheatDetails("Spawn Follower (Worker)", "Spawns and auto-indoctrinates a follower as a worker", false, 0)]
		public static void SpawnWorkerFollower()
		{
			CultUtils.SpawnFollower(FollowerRole.Worker);
			CultUtils.PlayNotification("Worker follower spawned!");
		}

		[CheatDetails("Spawn Follower (Worshipper)", "Spawns and auto-indoctrinates a follower as a worshipper", false, 0)]
		public static void SpawnWorkerWorshipper()
		{
			CultUtils.SpawnFollower(FollowerRole.Worshipper);
			CultUtils.PlayNotification("Worshipper follower spawned!");
		}

		[CheatDetails("Spawn 'Arrived' Follower", "Spawns a follower ready for indoctrination", false, 0)]
		public static void SpawnArrivedFollower()
		{
			FollowerManager.CreateNewRecruit(FollowerLocation.Base, NotificationCentre.NotificationType.NewRecruit);
			CultUtils.PlayNotification("New follower arrived!");
		}

		[CheatDetails("Spawn Child Follower", "Spawns a child follower at the base", false, 0)]
		public static void SpawnChildFollower()
		{
			try
			{
				FollowerManager.CreateNewFollower(FollowerLocation.Base, PlayerFarming.Instance.transform.position, false).Brain.MakeChild();
				CultUtils.PlayNotification("Child follower spawned!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to spawn child: " + ex.Message);
				CultUtils.PlayNotification("Failed to spawn child!");
			}
		}

		[CheatDetails("Turn all Followers Young", "Changes the age of all followers to young", false, 0)]
		[CheatWIP]
		public static void TurnAllFollowersYoung()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.TurnFollowerYoung(followerInfo);
			}
			CultUtils.PlayNotification("All followers are young now!");
		}

		[CheatDetails("Turn all Followers Old", "Changes the age of all followers to old", false, 0)]
		[CheatWIP]
		public static void TurnAllFollowersOld()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.TurnFollowerOld(followerInfo);
			}
			CultUtils.PlayNotification("All followers are old now!");
		}

		[CheatDetails("Kill All Followers", "Kills all followers at the Base", false, 0)]
		public static void KillAllFollowers()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.KillFollower(CultUtils.GetFollower(followerInfo), false);
			}
			CultUtils.PlayNotification("All followers killed!");
		}

		[CheatDetails("Kill Random Follower", "Kills a random follower", false, 0)]
		public static void KillRandomFollower()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("KillRandomFollower", Array.Empty<object>()).GetValue();
				CultUtils.PlayNotification("Random follower killed!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to kill random follower: " + ex.Message);
				CultUtils.PlayNotification("Failed to kill random follower!");
			}
		}

		[CheatDetails("Revive All Followers", "Revive all currently dead followers", false, 0)]
		public static void ReviveAllFollowers()
		{
			foreach (FollowerInfo followerInfo in CheatUtils.CloneList<FollowerInfo>(DataManager.Instance.Followers_Dead))
			{
				CultUtils.ReviveFollower(followerInfo);
			}
			CultUtils.PlayNotification("All followers revived!");
		}

		[CheatDetails("Remove Sickness", "Clears sickness from all followers, cleanups any vomit, poop or dead bodies and clears outhouses", false, 0)]
		public static void RemoveSickness()
		{
			CultUtils.ClearPoop();
			CultUtils.ClearBodies();
			CultUtils.ClearVomit();
			CultUtils.ClearOuthouses();
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.CureIllness(followerInfo);
			}
			CultUtils.PlayNotification("Cured all followers :)");
		}

		[CheatDetails("Convert Dissenting Followers", "Converts dissenting followers back to regular followers", false, 0)]
		public static void ConvertAllDissenting()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.ConvertDissenting(followerInfo);
			}
			CultUtils.PlayNotification("Converted all followers :)");
		}

		[CheatDetails("Clear Faith", "Set the current faith to zero", false, 0)]
		public static void ClearFaith()
		{
			CultUtils.ModifyFaith(0f, "Cleared faith :)", true);
		}

		[CheatDetails("Remove Hunger", "Clears starvation from any followers and maximizes satiation for all followers", false, 0)]
		public static void RemoveHunger()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.MaximizeSatiationAndRemoveStarvation(followerInfo);
			}
			CultUtils.PlayNotification("Everyone is full! :)");
		}

		[CheatDetails("Max Faith", "Clear the cult's thoughts and gives them large positive ones", false, 0)]
		public static void MaxFaith()
		{
			CultUtils.ClearAndAddPositiveFollowerThought();
		}

		[CheatDetails("Level Up All Followers", "Sets all follower levels to max (10)", false, 0)]
		public static void LevelUpAllFollowers()
		{
			try
			{
				foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
				{
					followerInfo.XPLevel = 10;
					Traverse.Create(followerInfo).Field("XP").SetValue(0f);
				}
				CultUtils.PlayNotification("All followers leveled to max!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to level followers: " + ex.Message);
				CultUtils.PlayNotification("Failed to level followers!");
			}
		}

		[CheatDetails("Increase Follower Loyalty", "Levels up loyalty for all followers by 1", false, 0)]
		public static void IncreaseFollowerLoyalty()
		{
			try
			{
				foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
				{
					Follower followerFromInfo = CultUtils.GetFollowerFromInfo(followerInfo);
					if (followerFromInfo != null && followerFromInfo.Brain != null)
					{
						Traverse.Create(followerFromInfo.Brain).Method("AddAdoration", new Type[]
						{
							typeof(int),
							typeof(float)
						}, null).GetValue(new object[] { 0, 100f });
					}
				}
				CultUtils.PlayNotification("Follower loyalty increased!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to increase loyalty: " + ex.Message);
				CultUtils.PlayNotification("Failed to increase loyalty!");
			}
		}

		[CheatDetails("Make All Followers Immortal", "Adds the Immortal trait to all followers", false, 0)]
		public static void MakeAllFollowersImmortal()
		{
			try
			{
				int num = 0;
				foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
				{
					if (!followerInfo.Traits.Contains(FollowerTrait.TraitType.Immortal))
					{
						followerInfo.Traits.Add(FollowerTrait.TraitType.Immortal);
						num++;
					}
				}
				CultUtils.PlayNotification(string.Format("{0} follower(s) made immortal!", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to make followers immortal: " + ex.Message);
				CultUtils.PlayNotification("Failed to make followers immortal!");
			}
		}

		[CheatDetails("Max All Follower Stats", "Max out faith, satiation and clear starvation for all followers", false, 0)]
		public static void MaxAllFollowerStats()
		{
			foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
			{
				CultUtils.SetFollowerFaith(followerInfo, 100f);
				CultUtils.MaximizeSatiationAndRemoveStarvation(followerInfo);
				CultUtils.CureIllness(followerInfo);
			}
			CultUtils.PlayNotification("All follower stats maxed!");
		}

		[CheatDetails("Remove Exhaustion", "Clears exhaustion from all followers so they can work again", false, 0)]
		public static void RemoveExhaustion()
		{
			try
			{
				int num = 0;
				foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
				{
					if (followerInfo.Exhaustion > 0f)
					{
						followerInfo.Exhaustion = 0f;
						num++;
					}
				}
				CultUtils.PlayNotification((num > 0) ? string.Format("{0} follower(s) rested!", num) : "No exhausted followers!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to remove exhaustion: " + ex.Message);
				CultUtils.PlayNotification("Failed to remove exhaustion!");
			}
		}

		[CheatDetails("Give Follower Tokens", "Gives 10 follower tokens", false, 0)]
		public static void GiveFollowerTokens()
		{
			DataManager.Instance.FollowerTokens += 10;
			CultUtils.PlayNotification("10 follower tokens added!");
		}

		[CheatDetails("Reset All Follower Outfits", "EMERGENCY: Resets all follower outfits to default Acolyte Robes (fixes loading issues caused by clothing cheat)", false, 0)]
		public static void ResetAllFollowerOutfits()
		{
			try
			{
				int num = 0;
				foreach (FollowerInfo followerInfo in DataManager.Instance.Followers)
				{
					if (followerInfo.Outfit == FollowerOutfitType.Custom || followerInfo.Clothing != FollowerClothingType.None || (followerInfo.Clothing != FollowerClothingType.None && followerInfo.Clothing != FollowerClothingType.Naked && TailorManager.GetClothingData(followerInfo.Clothing) == null))
					{
						followerInfo.Outfit = FollowerOutfitType.Follower;
						followerInfo.Clothing = FollowerClothingType.None;
						num++;
					}
				}
				foreach (FollowerInfo followerInfo2 in DataManager.Instance.Followers_Dead)
				{
					if (followerInfo2.Outfit == FollowerOutfitType.Custom || followerInfo2.Clothing != FollowerClothingType.None || (followerInfo2.Clothing != FollowerClothingType.None && followerInfo2.Clothing != FollowerClothingType.Naked && TailorManager.GetClothingData(followerInfo2.Clothing) == null))
					{
						followerInfo2.Outfit = FollowerOutfitType.Follower;
						followerInfo2.Clothing = FollowerClothingType.None;
						num++;
					}
				}
				using (List<int>.Enumerator enumerator2 = DataManager.Instance.Followers_Elderly_IDs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						int followerID = enumerator2.Current;
						FollowerInfo followerInfo3 = DataManager.Instance.Followers.Find((FollowerInfo f) => f.ID == followerID);
						if (followerInfo3 != null && followerInfo3.Outfit != FollowerOutfitType.Old)
						{
							followerInfo3.Outfit = FollowerOutfitType.Old;
							followerInfo3.Clothing = FollowerClothingType.None;
						}
					}
				}
				CultUtils.PlayNotification(string.Format("Reset {0} follower outfit(s) to default Acolyte Robes!", num));
				Debug.Log(string.Format("[CheatMenu] Reset {0} follower outfits to default (None/Follower) - game should now load properly", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to reset follower outfits: " + ex.Message);
				CultUtils.PlayNotification("Failed to reset outfits!");
			}
		}
	}
}
