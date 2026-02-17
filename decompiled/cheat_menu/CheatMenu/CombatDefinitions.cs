using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.COMBAT)]
	public class CombatDefinitions : IDefinition
	{
		private static bool IsInDungeon()
		{
			bool flag;
			try
			{
				flag = PlayerFarming.Instance != null && PlayerFarming.Location != FollowerLocation.Base;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		[CheatDetails("Kill All Enemies", "Kills all enemies in the current room", false, 0)]
		public static void KillAllEnemies()
		{
			if (!CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				return;
			}
			List<Health> list = new List<Health>(Health.team2);
			list.AddRange(Health.killAll);
			int num = 0;
			foreach (Health health in list)
			{
				if (health != null)
				{
					health.DealDamage(99999f, health.gameObject, health.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
					num++;
				}
			}
			CultUtils.PlayNotification(string.Format("Killed {0} enemies!", num));
		}

		[CheatDetails("Unlock All Weapons", "Unlocks all weapon types and curse packs", false, 0)]
		public static void UnlockAllWeapons()
		{
			DataManager.Instance.AddWeapon(EquipmentType.Axe);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer);
			DataManager.Instance.AddWeapon(EquipmentType.Blunderbuss);
			DataManager.Instance.AddWeapon(EquipmentType.Shield);
			DataManager.Instance.AddWeapon(EquipmentType.Chain);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack1, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack2, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack3, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack4, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack5, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponFervor, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponGodly, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponNecromancy, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponPoison, false);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponCritHit, false);
			CultUtils.PlayNotification("All weapons & curses unlocked!");
		}

		[CheatDetails("Unlock All Tarot Cards", "Unlocks all tarot cards", false, 0)]
		public static void UnlockAllTarotCards()
		{
			DataManager.Instance.PlayerFoundTrinkets.Clear();
			foreach (TarotCards.Card card in DataManager.AllTrinkets)
			{
				DataManager.Instance.PlayerFoundTrinkets.Add(card);
			}
			CultUtils.PlayNotification("All tarot cards unlocked!");
		}

		[CheatDetails("Enable Tarot Building", "Enables the tarot card reading building", false, 0)]
		public static void EnableTarotBuilding()
		{
			DataManager.Instance.HasTarotBuilding = true;
			CultUtils.PlayNotification("Tarot building enabled!");
		}

		[CheatDetails("Enable Black Souls", "Enables the black souls currency system", false, 0)]
		public static void EnableBlackSouls()
		{
			DataManager.Instance.BlackSoulsEnabled = true;
			CultUtils.PlayNotification("Black souls enabled!");
		}

		[CheatDetails("Unlock All Fleeces", "Unlocks all fleece types that change crusade abilities", false, 0)]
		public static void UnlockAllFleeces()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(UpgradeSystem.Type)))
				{
					string text = obj.ToString();
					if (text.Contains("Fleece") || text.Contains("FLEECE"))
					{
						UpgradeSystem.Type type = (UpgradeSystem.Type)obj;
						if (!UpgradeSystem.GetUnlocked(type))
						{
							UpgradeSystem.UnlockAbility(type, false);
							num++;
						}
					}
				}
				CultUtils.PlayNotification(string.Format("All fleeces unlocked! ({0} new)", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to unlock fleeces: " + ex.Message);
				CultUtils.PlayNotification("Failed to unlock fleeces!");
			}
		}

		[CheatDetails("Unlock EVERYTHING", "Unlocks all upgrades, rituals, weapons, structures, tarot", false, 0)]
		public static void UnlockAbsolutelyEverything()
		{
			CheatConsole.UnlockAllRituals = true;
			for (int i = 0; i < Enum.GetNames(typeof(UpgradeSystem.Type)).Length; i++)
			{
				UpgradeSystem.UnlockAbility((UpgradeSystem.Type)i, false);
			}
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
			DataManager.Instance.PlayerFoundTrinkets.Clear();
			foreach (TarotCards.Card card in DataManager.AllTrinkets)
			{
				DataManager.Instance.PlayerFoundTrinkets.Add(card);
			}
			DataManager.Instance.AddWeapon(EquipmentType.Axe);
			DataManager.Instance.AddWeapon(EquipmentType.Dagger);
			DataManager.Instance.AddWeapon(EquipmentType.Gauntlet);
			DataManager.Instance.AddWeapon(EquipmentType.Hammer);
			DataManager.Instance.AddWeapon(EquipmentType.Blunderbuss);
			DataManager.Instance.AddWeapon(EquipmentType.Shield);
			DataManager.Instance.AddWeapon(EquipmentType.Chain);
			DataManager.Instance.HasTarotBuilding = true;
			DataManager.Instance.BlackSoulsEnabled = true;
			CultUtils.PlayNotification("EVERYTHING unlocked!");
		}

		public static void Postfix_MiniMap_StartMap(MiniMap __instance)
		{
			if (!CombatDefinitions.s_mapRevealEnabled)
			{
				return;
			}
			try
			{
				__instance.DiscoverAll();
			}
			catch
			{
			}
		}

		private static void PatchMiniMapReveal()
		{
			if (CombatDefinitions.s_mapRevealPatched)
			{
				return;
			}
			try
			{
				MethodInfo method = typeof(CombatDefinitions).GetMethod("Postfix_MiniMap_StartMap", BindingFlags.Static | BindingFlags.Public);
				ReflectionHelper.PatchMethodPostfix(typeof(MiniMap), "StartMap", method, BindingFlags.Instance | BindingFlags.Public, null, true);
				CombatDefinitions.s_mapRevealPatched = true;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to patch MiniMap.StartMap: " + ex.Message);
			}
		}

		private static void UnpatchMiniMapReveal()
		{
			if (!CombatDefinitions.s_mapRevealPatched)
			{
				return;
			}
			ReflectionHelper.UnpatchTracked(typeof(MiniMap), "StartMap");
			CombatDefinitions.s_mapRevealPatched = false;
		}

		[CheatDetails("Auto Reveal Dungeon Map", "Map Reveal (OFF)", "Map Reveal (ON)", "Automatically reveals the dungeon map on every new floor", true, 0)]
		public static void ShowAllMapLocations(bool flag)
		{
			CombatDefinitions.s_mapRevealEnabled = flag;
			CheatConsole.ShowAllMapLocations = flag;
			if (flag)
			{
				CombatDefinitions.PatchMiniMapReveal();
				try
				{
					if (MiniMap.Instance != null)
					{
						MiniMap.Instance.DiscoverAll();
					}
					goto IL_004A;
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Map reveal failed: " + ex.Message);
					goto IL_004A;
				}
			}
			CombatDefinitions.UnpatchMiniMapReveal();
			IL_004A:
			CultUtils.PlayNotification(flag ? "Auto map reveal ON!" : "Auto map reveal OFF!");
		}

		[CheatDetails("Reveal Dungeon Map", "Reveals all rooms on the current dungeon floor map", false, 0)]
		public static void RevealDungeonMap()
		{
			try
			{
				bool flag = false;
				if (MiniMap.Instance != null)
				{
					MiniMap.Instance.DiscoverAll();
					flag = true;
				}
				if (PlayerFarming.Instance != null && !TrinketManager.HasTrinket(TarotCards.Card.Telescope))
				{
					TrinketManager.AddTrinket(new TarotCards.TarotCard(TarotCards.Card.Telescope, 0), PlayerFarming.Instance);
					flag = true;
				}
				CultUtils.PlayNotification(flag ? "Dungeon map revealed!" : "Not in a dungeon!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to reveal dungeon map: " + ex.Message);
				CultUtils.PlayNotification("Not in a dungeon or failed to reveal map!");
			}
		}

		public static bool Prefix_PlayerRelic_ResetChargedAmount(PlayerRelic __instance)
		{
			if (!CombatDefinitions.s_unlimitedRelicsEnabled)
			{
				return true;
			}
			__instance.FullyCharge();
			return false;
		}

		[CheatDetails("Unlimited Relics", "Unlimited Relics (OFF)", "Unlimited Relics (ON)", "Relics never run out of charges during combat", true, 0)]
		public static void UnlimitedRelics(bool flag)
		{
			if (flag && !CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedRelics"), false);
				return;
			}
			CombatDefinitions.s_unlimitedRelicsEnabled = flag;
			try
			{
				if (flag)
				{
					foreach (PlayerFarming playerFarming in PlayerFarming.players)
					{
						if (playerFarming != null && playerFarming.playerRelic != null)
						{
							playerFarming.playerRelic.FullyCharge();
						}
					}
					MethodInfo method = typeof(CombatDefinitions).GetMethod("Prefix_PlayerRelic_ResetChargedAmount", BindingFlags.Static | BindingFlags.Public);
					ReflectionHelper.PatchMethodPrefix(typeof(PlayerRelic), "ResetChargedAmount", method, BindingFlags.Instance | BindingFlags.Public, null, true);
				}
				else
				{
					ReflectionHelper.UnpatchTracked(typeof(PlayerRelic), "ResetChargedAmount");
				}
				CultUtils.PlayNotification(flag ? "Unlimited relics ON!" : "Unlimited relics OFF!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to set unlimited relics: " + ex.Message);
				CultUtils.PlayNotification("Failed to toggle unlimited relics!");
			}
		}

		public static bool Prefix_Health_DealDamage(Health __instance, ref float Damage, GameObject Attacker)
		{
			if (!CombatDefinitions.s_oneHitKillEnabled)
			{
				return true;
			}
			if (Attacker == null || __instance == null)
			{
				return true;
			}
			try
			{
				if (PlayerFarming.Instance != null && Attacker == PlayerFarming.Instance.gameObject && Health.team2.Contains(__instance))
				{
					Damage = 99999f;
				}
			}
			catch
			{
			}
			return true;
		}

		[CheatDetails("One Hit Kill", "One Hit Kill (OFF)", "One Hit Kill (ON)", "All your attacks instantly kill enemies", true, 0)]
		public static void OneHitKill(bool flag)
		{
			if (flag && !CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "OneHitKill"), false);
				return;
			}
			CombatDefinitions.s_oneHitKillEnabled = flag;
			if (flag)
			{
				try
				{
					MethodInfo method = typeof(CombatDefinitions).GetMethod("Prefix_Health_DealDamage", BindingFlags.Static | BindingFlags.Public);
					ReflectionHelper.PatchMethodPrefix(typeof(Health), "DealDamage", method, BindingFlags.Instance | BindingFlags.Public, null, true);
					goto IL_0097;
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Failed to patch one hit kill: " + ex.Message);
					goto IL_0097;
				}
			}
			ReflectionHelper.UnpatchTracked(typeof(Health), "DealDamage");
			IL_0097:
			CultUtils.PlayNotification(flag ? "One hit kill ON!" : "One hit kill OFF!");
		}

		public static bool Prefix_BlunderAmmo_UseAmmo(BlunderAmmo __instance, ref bool __result)
		{
			if (!CombatDefinitions.s_unlimitedAmmoEnabled)
			{
				return true;
			}
			try
			{
				if (__instance.gameObject.activeInHierarchy)
				{
					__instance.SetBlunderAmmo((float)__instance.blunderAmmoTotal);
				}
			}
			catch
			{
			}
			__result = true;
			return false;
		}

		public static bool Prefix_PlayerArrows_RestockArrow(PlayerArrows __instance)
		{
			if (!CombatDefinitions.s_unlimitedAmmoEnabled)
			{
				return true;
			}
			__instance.RestockAllArrows();
			return false;
		}

		private static void RefillAllAmmoImmediate()
		{
			try
			{
				if (!(PlayerFarming.Instance == null))
				{
					PlayerArrows component = PlayerFarming.Instance.GetComponent<PlayerArrows>();
					if (component != null)
					{
						component.RestockAllArrows();
					}
					if (PlayerFarming.Instance.playerWeapon != null && PlayerFarming.Instance.playerWeapon.blunderAmmo != null)
					{
						BlunderAmmo blunderAmmo = PlayerFarming.Instance.playerWeapon.blunderAmmo;
						if (blunderAmmo.gameObject.activeInHierarchy && blunderAmmo.blunderAmmo < (float)blunderAmmo.blunderAmmoTotal)
						{
							blunderAmmo.SetBlunderAmmo((float)blunderAmmo.blunderAmmoTotal);
						}
					}
				}
			}
			catch
			{
			}
		}

		private static void PatchAmmoMethods()
		{
			if (CombatDefinitions.s_ammoPatched)
			{
				return;
			}
			try
			{
				MethodInfo method = typeof(CombatDefinitions).GetMethod("Prefix_BlunderAmmo_UseAmmo", BindingFlags.Static | BindingFlags.Public);
				ReflectionHelper.PatchMethodPrefix(typeof(BlunderAmmo), "UseAmmo", method, BindingFlags.Instance | BindingFlags.Public, null, true);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Failed to patch BlunderAmmo.UseAmmo: " + ex.Message);
			}
			try
			{
				MethodInfo method2 = typeof(CombatDefinitions).GetMethod("Prefix_PlayerArrows_RestockArrow", BindingFlags.Static | BindingFlags.Public);
				ReflectionHelper.PatchMethodPrefix(typeof(PlayerArrows), "RestockArrow", method2, BindingFlags.Instance | BindingFlags.Public, null, true);
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("[CheatMenu] Failed to patch PlayerArrows.RestockArrow: " + ex2.Message);
			}
			CombatDefinitions.s_ammoPatched = true;
		}

		private static void UnpatchAmmoMethods()
		{
			if (!CombatDefinitions.s_ammoPatched)
			{
				return;
			}
			ReflectionHelper.UnpatchTracked(typeof(BlunderAmmo), "UseAmmo");
			ReflectionHelper.UnpatchTracked(typeof(PlayerArrows), "RestockArrow");
			CombatDefinitions.s_ammoPatched = false;
		}

		[CheatDetails("Unlimited Ammo", "Unlimited Ammo (OFF)", "Unlimited Ammo (ON)", "Arrows and blunderbuss never run out of ammo", true, 0)]
		public static void UnlimitedAmmo(bool flag)
		{
			if (flag && !CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedAmmo"), false);
				return;
			}
			CombatDefinitions.s_unlimitedAmmoEnabled = flag;
			try
			{
				if (flag)
				{
					CombatDefinitions.PatchAmmoMethods();
					CombatDefinitions.RefillAllAmmoImmediate();
				}
				else
				{
					CombatDefinitions.UnpatchAmmoMethods();
				}
				CultUtils.PlayNotification(flag ? "Unlimited ammo ON!" : "Unlimited ammo OFF!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to set unlimited ammo: " + ex.Message);
				CultUtils.PlayNotification("Failed to toggle unlimited ammo!");
			}
		}

		[CheatDetails("Unlimited Fervour", "Unlimited Fervour (OFF)", "Unlimited Fervour (ON)", "Fervour never depletes when casting curses", true, 0)]
		public static void UnlimitedFervour(bool flag)
		{
			if (flag && !CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedFervour"), false);
				return;
			}
			try
			{
				SettingsManager.Settings.Accessibility.UnlimitedFervour = flag;
				CultUtils.PlayNotification(flag ? "Unlimited fervour ON!" : "Unlimited fervour OFF!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to set unlimited fervour: " + ex.Message);
				CultUtils.PlayNotification("Failed to toggle unlimited fervour!");
			}
		}

		[CheatDetails("Clear Status Effects", "Removes poison, burn, ice, charm and electrified from the player", false, 0)]
		public static void ClearStatusEffects()
		{
			if (!CombatDefinitions.IsInDungeon())
			{
				CultUtils.PlayNotification("Must be in a dungeon to use this!");
				return;
			}
			try
			{
				foreach (PlayerFarming playerFarming in PlayerFarming.players)
				{
					if (playerFarming != null && playerFarming.health != null)
					{
						playerFarming.health.ClearAllStasisEffects();
						playerFarming.health.ClearPoison();
						playerFarming.health.ClearBurn();
					}
				}
				CultUtils.PlayNotification("Status effects cleared!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clear status effects: " + ex.Message);
				CultUtils.PlayNotification("Failed to clear status effects!");
			}
		}

		[CheatDetails("Unlock Crown Abilities", "Unlocks all crown abilities (arrows, grapple, fishing, etc.)", false, 0)]
		public static void UnlockAllCrownAbilities()
		{
			try
			{
				int num = 0;
				foreach (object obj in Enum.GetValues(typeof(CrownAbilities.TYPE)))
				{
					CrownAbilities.TYPE type = (CrownAbilities.TYPE)obj;
					if (!CrownAbilities.CrownAbilityUnlocked(type))
					{
						CrownAbilities.UnlockAbility(type);
						num++;
					}
				}
				CultUtils.PlayNotification(string.Format("Crown abilities unlocked! ({0} new)", num));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to unlock crown abilities: " + ex.Message);
				CultUtils.PlayNotification("Failed to unlock crown abilities!");
			}
		}

		[Update]
		public static void CombatUpdate()
		{
			if (CombatDefinitions.s_unlimitedAmmoEnabled)
			{
				try
				{
					CombatDefinitions.RefillAllAmmoImmediate();
				}
				catch
				{
				}
			}
		}

		private static bool s_oneHitKillEnabled;

		private static bool s_unlimitedRelicsEnabled;

		private static bool s_unlimitedAmmoEnabled;

		private static bool s_ammoPatched;

		private static bool s_mapRevealEnabled;

		private static bool s_mapRevealPatched;
	}
}
