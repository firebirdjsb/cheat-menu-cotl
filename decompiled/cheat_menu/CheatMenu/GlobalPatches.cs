using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace CheatMenu
{
	public class GlobalPatches
	{
		public static FollowerClothingType GetSafeDefaultClothing()
		{
			return FollowerClothingType.None;
		}

		[Init]
		[EnforceOrderLast]
		public static void Init()
		{
			MethodInfo method = typeof(GlobalPatches).GetMethod("Prefix_Interactor_Update", BindingFlags.Static | BindingFlags.Public);
			ReflectionHelper.PatchMethodPrefix(typeof(Interactor), "Update", method, BindingFlags.Instance | BindingFlags.NonPublic, null, false);
			try
			{
				MethodInfo method2 = typeof(GlobalPatches).GetMethod("Prefix_UpgradeSystem_UnlockAbility", BindingFlags.Static | BindingFlags.Public);
				if (ReflectionHelper.PatchMethodPrefix(typeof(UpgradeSystem), "UnlockAbility", method2, BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(UpgradeSystem.Type) }, true) != null)
				{
					Debug.Log("[CheatMenu] UpgradeSystem.UnlockAbility successfully patched");
				}
			}
			catch (Exception ex)
			{
				Debug.Log("[CheatMenu] UpgradeSystem.UnlockAbility patch not applied (game version may have changed): " + ex.Message);
			}
			try
			{
				MethodInfo method3 = typeof(GlobalPatches).GetMethod("Prefix_PlayerFarming_Bleat", BindingFlags.Static | BindingFlags.Public);
				string text = ReflectionHelper.PatchMethodPrefix(typeof(PlayerFarming), "Bleat", method3, BindingFlags.Instance | BindingFlags.NonPublic, null, true);
				if (text == null)
				{
					text = ReflectionHelper.PatchMethodPrefix(typeof(PlayerFarming), "Bleat", method3, BindingFlags.Instance | BindingFlags.Public, null, true);
				}
				if (text != null)
				{
					Debug.Log("[CheatMenu] PlayerFarming.Bleat successfully patched (R3 suppression)");
				}
			}
			catch (Exception ex2)
			{
				Debug.Log("[CheatMenu] PlayerFarming.Bleat patch not applied (game version may have changed): " + ex2.Message);
			}
			try
			{
				MethodInfo method4 = typeof(GlobalPatches).GetMethod("Prefix_Follower_Init", BindingFlags.Static | BindingFlags.Public);
				Type[] array = new Type[]
				{
					typeof(FollowerBrain),
					typeof(FollowerOutfit)
				};
				if (ReflectionHelper.PatchMethodPrefix(typeof(Follower), "Init", method4, BindingFlags.Instance | BindingFlags.Public, array, true) != null)
				{
					Debug.Log("[CheatMenu] Follower.Init successfully patched (skeleton validation)");
				}
			}
			catch (Exception ex3)
			{
				Debug.Log("[CheatMenu] Follower.Init patch not applied: " + ex3.Message);
			}
			try
			{
				MethodInfo method5 = typeof(GlobalPatches).GetMethod("Prefix_WolfBase_Update", BindingFlags.Static | BindingFlags.Public);
				string text2 = ReflectionHelper.PatchMethodPrefix(typeof(Interaction_WolfBase), "Update", method5, BindingFlags.Instance | BindingFlags.NonPublic, null, true);
				if (text2 == null)
				{
					text2 = ReflectionHelper.PatchMethodPrefix(typeof(Interaction_WolfBase), "Update", method5, BindingFlags.Instance | BindingFlags.Public, null, true);
				}
				if (text2 != null)
				{
					Debug.Log("[CheatMenu] Interaction_WolfBase.Update successfully patched (friendly wolf)");
				}
			}
			catch (Exception ex4)
			{
				Debug.Log("[CheatMenu] Interaction_WolfBase.Update patch not applied: " + ex4.Message);
			}
			try
			{
				MethodInfo method6 = typeof(GlobalPatches).GetMethod("Prefix_Skin_AddSkin", BindingFlags.Static | BindingFlags.Public);
				if (ReflectionHelper.PatchMethodPrefix(typeof(Skin), "AddSkin", method6, BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Skin) }, true) != null)
				{
					Debug.Log("[CheatMenu] Spine.Skin.AddSkin successfully patched (null safety)");
				}
			}
			catch (Exception ex5)
			{
				Debug.Log("[CheatMenu] Spine.Skin.AddSkin patch not applied: " + ex5.Message);
			}
			try
			{
				MethodInfo method7 = typeof(GlobalPatches).GetMethod("Finalizer_Follower_Init", BindingFlags.Static | BindingFlags.Public);
				Type[] array2 = new Type[]
				{
					typeof(FollowerBrain),
					typeof(FollowerOutfit)
				};
				if (ReflectionHelper.PatchMethodFinalizer(typeof(Follower), "Init", method7, BindingFlags.Instance | BindingFlags.Public, array2, true) != null)
				{
					Debug.Log("[CheatMenu] Follower.Init finalizer successfully patched (crash recovery)");
				}
			}
			catch (Exception ex6)
			{
				Debug.Log("[CheatMenu] Follower.Init finalizer patch not applied: " + ex6.Message);
			}
			try
			{
				MethodInfo method8 = typeof(GlobalPatches).GetMethod("Prefix_Follower_Tick", BindingFlags.Static | BindingFlags.Public);
				if (ReflectionHelper.PatchMethodPrefix(typeof(Follower), "Tick", method8, BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(float) }, true) != null)
				{
					Debug.Log("[CheatMenu] Follower.Tick successfully patched (null Outfit guard)");
				}
			}
			catch (Exception ex7)
			{
				Debug.Log("[CheatMenu] Follower.Tick patch not applied: " + ex7.Message);
			}
		}

		[Unload]
		public static void UnpatchAll()
		{
			ReflectionHelper.UnpatchTracked(typeof(Interactor), "Update");
			ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "UnlockAbility");
			ReflectionHelper.UnpatchTracked(typeof(PlayerFarming), "Bleat");
			ReflectionHelper.UnpatchTracked(typeof(Follower), "Init");
			ReflectionHelper.UnpatchTracked(typeof(Follower), "Tick");
			ReflectionHelper.UnpatchTracked(typeof(Skin), "AddSkin");
			ReflectionHelper.UnpatchTracked(typeof(Interaction_WolfBase), "Update");
			ReflectionHelper.UnpatchTracked(typeof(VersionNumber), "OnEnable");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateCultistDLC");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateHereticDLC");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateSinfulDLC");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticatePilgrimDLC");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateMajorDLC");
			ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticatePrePurchaseDLC");
			ReflectionHelper.UnpatchTracked(typeof(BlunderAmmo), "UseAmmo");
			ReflectionHelper.UnpatchTracked(typeof(PlayerArrows), "RestockArrow");
			ReflectionHelper.UnpatchTracked(typeof(MiniMap), "StartMap");
		}

		public static bool Prefix_UpgradeSystem_UnlockAbility(UpgradeSystem.Type Type)
		{
			UpgradeSystem.Type type;
			if (CultUtils.GetDictionaryRitualPairs().TryGetValue(Type, out type) && UpgradeSystem.UnlockedUpgrades.Contains(type))
			{
				UpgradeSystem.UnlockedUpgrades.Remove(type);
			}
			return true;
		}

		public static bool Prefix_Interactor_Update()
		{
			return !CheatMenuGui.GuiEnabled;
		}

		public static bool Prefix_PlayerFarming_Bleat()
		{
			return !CheatConfig.Instance.ControllerSupport.Value || !RewiredInputHelper.ShouldSuppressR3;
		}

		public static bool Prefix_WolfBase_Update(Interaction_WolfBase __instance)
		{
			return CultUtils.HandleFriendlyWolfUpdate(__instance);
		}

		public static bool Prefix_Follower_Init(Follower __instance, FollowerBrain brain, FollowerOutfit outfit)
		{
			bool flag;
			try
			{
				if (__instance == null || brain == null || outfit == null)
				{
					Debug.LogWarning("[CheatMenu] Follower.Init called with null parameters - skipping");
					flag = false;
				}
				else
				{
					if (brain.Info.Clothing != FollowerClothingType.None && brain.Info.Clothing != FollowerClothingType.Naked && TailorManager.GetClothingData(brain.Info.Clothing) == null)
					{
						Debug.LogWarning(string.Format("[CheatMenu] Follower {0} has clothing type '{1}' with no ClothingData - resetting to default (None/Follower)", brain.Info.Name, brain.Info.Clothing));
						brain.Info.Clothing = FollowerClothingType.None;
						brain.Info.Outfit = FollowerOutfitType.Follower;
					}
					SkeletonAnimation value = Traverse.Create(__instance).Field("Spine").GetValue<SkeletonAnimation>();
					if (value == null || value.skeleton == null || value.skeleton.Data == null)
					{
						Debug.LogWarning("[CheatMenu] Follower " + brain.Info.Name + " has null Spine/skeleton/Data - resetting to safe defaults");
						brain.Info.Outfit = FollowerOutfitType.Follower;
						brain.Info.Clothing = FollowerClothingType.None;
						flag = true;
					}
					else
					{
						SkeletonData data = value.skeleton.Data;
						string skinName = brain.Info.SkinName;
						if (!string.IsNullOrEmpty(skinName) && data.FindSkin(skinName) == null)
						{
							if (data.FindSkin("Cat") != null)
							{
								brain.Info.SkinName = "Cat";
								Debug.LogWarning(string.Concat(new string[]
								{
									"[CheatMenu] Follower ",
									brain.Info.Name,
									" had invalid skin '",
									skinName,
									"' - reset to 'Cat'"
								}));
							}
							else
							{
								ExposedList<Skin> skins = data.Skins;
								if (skins != null && skins.Count > 0)
								{
									brain.Info.SkinName = skins.Items[0].Name;
									Debug.LogWarning(string.Concat(new string[]
									{
										"[CheatMenu] Follower ",
										brain.Info.Name,
										" had invalid skin '",
										skinName,
										"' - reset to '",
										brain.Info.SkinName,
										"'"
									}));
								}
							}
							brain.Info.Outfit = FollowerOutfitType.Follower;
							brain.Info.Clothing = FollowerClothingType.None;
						}
						flag = true;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[CheatMenu] Error in Follower.Init validation: " + ex.Message);
				flag = true;
			}
			return flag;
		}

		public static bool Prefix_Follower_Tick(Follower __instance)
		{
			if (__instance.Outfit == null)
			{
				return false;
			}
			try
			{
				FollowerBrain brain = __instance.Brain;
				if (((brain != null) ? brain.Info : null) != null && __instance.Brain.Info.Clothing != FollowerClothingType.None && __instance.Brain.Info.Clothing != FollowerClothingType.Naked)
				{
					int id = __instance.Brain.Info.ID;
					if (!GlobalPatches.s_fixedFollowerIds.Contains(id) && TailorManager.GetClothingData(__instance.Brain.Info.Clothing) == null)
					{
						Debug.LogWarning(string.Format("[CheatMenu] Follower '{0}' has invalid clothing '{1}' - resetting to default (None/Follower)", __instance.Brain.Info.Name, __instance.Brain.Info.Clothing));
						__instance.Brain.Info.Clothing = FollowerClothingType.None;
						__instance.Brain.Info.Outfit = FollowerOutfitType.Follower;
						GlobalPatches.s_fixedFollowerIds.Add(id);
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static bool Prefix_Skin_AddSkin(Skin skin)
		{
			return skin != null;
		}

		public static Exception Finalizer_Follower_Init(Exception __exception, Follower __instance, FollowerBrain brain, FollowerOutfit outfit)
		{
			if (__exception != null)
			{
				string text;
				if (brain == null)
				{
					text = null;
				}
				else
				{
					FollowerBrainInfo info = brain.Info;
					text = ((info != null) ? info.Name : null);
				}
				string text2 = text ?? "Unknown";
				Debug.LogWarning("[CheatMenu] Follower.Init exception caught and suppressed for '" + text2 + "': " + __exception.Message);
				Debug.LogWarning("[CheatMenu] Stack: " + __exception.StackTrace);
				try
				{
					if (((brain != null) ? brain.Info : null) != null)
					{
						brain.Info.Outfit = FollowerOutfitType.Follower;
						brain.Info.Clothing = FollowerClothingType.None;
					}
					if (__instance != null)
					{
						if (__instance.Brain == null && brain != null)
						{
							Traverse.Create(__instance).Field("Brain").SetValue(brain);
						}
						Traverse traverse = Traverse.Create(__instance).Field("Outfit");
						if (traverse.GetValue() == null && outfit != null)
						{
							traverse.SetValue(outfit);
						}
					}
					Debug.Log("[CheatMenu] Reset follower '" + text2 + "' to safe defaults after crash");
				}
				catch (Exception ex)
				{
					Debug.LogError("[CheatMenu] Error during Follower.Init crash recovery for '" + text2 + "': " + ex.Message);
				}
				return null;
			}
			return null;
		}

		private static HashSet<int> s_fixedFollowerIds = new HashSet<int>();
	}
}
