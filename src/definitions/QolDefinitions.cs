using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.COMBAT)]
public class CombatDefinitions : IDefinition{

    private static bool s_oneHitKillEnabled = false;

    [CheatDetails("Kill All Enemies", "Kills all enemies in the current room")]
    public static void KillAllEnemies(){
        List<Health> enemies = new List<Health>(Health.team2);
        enemies.AddRange(Health.killAll);
        int count = 0;
        foreach(Health enemy in enemies){
            if(enemy != null){
                enemy.DealDamage(99999f, enemy.gameObject, enemy.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
                count++;
            }
        }
        CultUtils.PlayNotification($"Killed {count} enemies!");
    }

    [CheatDetails("Unlock All Weapons", "Unlocks all weapon types and curse packs")]
    public static void UnlockAllWeapons(){
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

    [CheatDetails("Unlock All Tarot Cards", "Unlocks all tarot cards")]
    public static void UnlockAllTarotCards(){
        DataManager.Instance.PlayerFoundTrinkets.Clear();
        foreach(TarotCards.Card card in DataManager.AllTrinkets){
            DataManager.Instance.PlayerFoundTrinkets.Add(card);
        }
        CultUtils.PlayNotification("All tarot cards unlocked!");
    }

    [CheatDetails("Enable Tarot Building", "Enables the tarot card reading building")]
    public static void EnableTarotBuilding(){
        DataManager.Instance.HasTarotBuilding = true;
        CultUtils.PlayNotification("Tarot building enabled!");
    }

    [CheatDetails("Enable Black Souls", "Enables the black souls currency system")]
    public static void EnableBlackSouls(){
        DataManager.Instance.BlackSoulsEnabled = true;
        CultUtils.PlayNotification("Black souls enabled!");
    }

    [CheatDetails("Unlock All Fleeces", "Unlocks all fleece types that change crusade abilities")]
    public static void UnlockAllFleeces(){
        try {
            int count = 0;
            foreach(var upgradeType in Enum.GetValues(typeof(UpgradeSystem.Type))){
                string typeName = upgradeType.ToString();
                if(typeName.Contains("Fleece") || typeName.Contains("FLEECE")){
                    UpgradeSystem.Type type = (UpgradeSystem.Type)upgradeType;
                    if(!UpgradeSystem.GetUnlocked(type)){
                        UpgradeSystem.UnlockAbility(type, false);
                        count++;
                    }
                }
            }
            CultUtils.PlayNotification($"All fleeces unlocked! ({count} new)");
        } catch(Exception e){
            Debug.LogWarning($"Failed to unlock fleeces: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock fleeces!");
        }
    }

    [CheatDetails("Unlock EVERYTHING", "Unlocks all upgrades, rituals, weapons, structures, tarot")]
    public static void UnlockAbsolutelyEverything(){
        CheatConsole.UnlockAllRituals = true;
        for(int i = 0; i < Enum.GetNames(typeof(UpgradeSystem.Type)).Length; i++){
            UpgradeSystem.UnlockAbility((UpgradeSystem.Type)i, false);
        }
        foreach(var structureType in Enum.GetValues(typeof(StructureBrain.TYPES))){
            StructureBrain.TYPES type = (StructureBrain.TYPES)structureType;
            StructuresData.SetRevealed(type);
            if(!DataManager.Instance.UnlockedStructures.Contains(type)){
                DataManager.Instance.UnlockedStructures.Add(type);
            }
            if(!DataManager.Instance.RevealedStructures.Contains(type)){
                DataManager.Instance.RevealedStructures.Add(type);
            }
        }
        DataManager.Instance.PlayerFoundTrinkets.Clear();
        foreach(TarotCards.Card card in DataManager.AllTrinkets){
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

    [CheatDetails("Show All Map Locations", "Show Maps (OFF)", "Show Maps (ON)", "Reveals all locations on the world map and all rooms in the current dungeon", true)]
    public static void ShowAllMapLocations(bool flag){
        CheatConsole.ShowAllMapLocations = flag;
        if(flag){
            try {
                if(MiniMap.Instance != null){
                    MiniMap.Instance.DiscoverAll();
                }
            } catch(Exception e){
                Debug.LogWarning($"Map reveal failed: {e.Message}");
            }
        }
        CultUtils.PlayNotification(flag ? "All map locations visible!" : "Map locations hidden!");
    }

    [CheatDetails("Reveal Dungeon Map", "Reveals all rooms on the current dungeon floor map")]
    public static void RevealDungeonMap(){
        try {
            bool revealed = false;
            if(MiniMap.Instance != null){
                MiniMap.Instance.DiscoverAll();
                revealed = true;
            }
            if(PlayerFarming.Instance != null && !TrinketManager.HasTrinket(TarotCards.Card.Telescope)){
                TrinketManager.AddTrinket(new TarotCards.TarotCard(TarotCards.Card.Telescope, 0), PlayerFarming.Instance);
                revealed = true;
            }
            CultUtils.PlayNotification(revealed ? "Dungeon map revealed!" : "Not in a dungeon!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to reveal dungeon map: {e.Message}");
            CultUtils.PlayNotification("Not in a dungeon or failed to reveal map!");
        }
    }

    [CheatDetails("Unlimited Relics", "Unlimited Relics (OFF)", "Unlimited Relics (ON)", "Relics never run out of charges during combat", true)]
    public static void UnlimitedRelics(bool flag){
        try {
            foreach(var player in PlayerFarming.players){
                if(player != null && player.playerRelic != null){
                    player.RelicChargeAmount = flag ? float.MaxValue : 0f;
                }
            }
            CultUtils.PlayNotification(flag ? "Unlimited relics ON!" : "Unlimited relics OFF!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set unlimited relics: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle unlimited relics!");
        }
    }

    public static bool Prefix_Health_DealDamage(Health __instance, ref float Damage, GameObject Attacker){
        if(!s_oneHitKillEnabled) return true;
        if(Attacker == null || __instance == null) return true;
        try {
            if(PlayerFarming.Instance != null && Attacker == PlayerFarming.Instance.gameObject && Health.team2.Contains(__instance)){
                Damage = 99999f;
            }
        } catch { }
        return true;
    }

    [CheatDetails("One Hit Kill", "One Hit Kill (OFF)", "One Hit Kill (ON)", "All your attacks instantly kill enemies", true)]
    public static void OneHitKill(bool flag){
        s_oneHitKillEnabled = flag;
        if(flag){
            try {
                MethodInfo patchMethod = typeof(CombatDefinitions).GetMethod("Prefix_Health_DealDamage", BindingFlags.Static | BindingFlags.Public);
                ReflectionHelper.PatchMethodPrefix(
                    typeof(Health),
                    "DealDamage",
                    patchMethod,
                    BindingFlags.Instance | BindingFlags.Public,
                    silent: true
                );
            } catch(Exception e){
                Debug.LogWarning($"Failed to patch one hit kill: {e.Message}");
            }
        } else {
            ReflectionHelper.UnpatchTracked(typeof(Health), "DealDamage");
        }
        CultUtils.PlayNotification(flag ? "One hit kill ON!" : "One hit kill OFF!");
    }

    [CheatDetails("Unlimited Ammo", "Unlimited Ammo (OFF)", "Unlimited Ammo (ON)", "Arrows and ranged weapons never run out of ammo", true)]
    public static void UnlimitedAmmo(bool flag){
        try {
            if(flag){
                DataManager.Instance.PLAYER_ARROW_AMMO = 99;
            }
            CultUtils.PlayNotification(flag ? "Unlimited ammo ON!" : "Unlimited ammo OFF!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set unlimited ammo: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle unlimited ammo!");
        }
    }

    [CheatDetails("Unlimited Fervour", "Unlimited Fervour (OFF)", "Unlimited Fervour (ON)", "Fervour never depletes when casting curses", true)]
    public static void UnlimitedFervour(bool flag){
        try {
            SettingsManager.Settings.Accessibility.UnlimitedFervour = flag;
            CultUtils.PlayNotification(flag ? "Unlimited fervour ON!" : "Unlimited fervour OFF!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set unlimited fervour: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle unlimited fervour!");
        }
    }

    [CheatDetails("Clear Status Effects", "Removes poison, burn, ice, charm and electrified from the player")]
    public static void ClearStatusEffects(){
        try {
            foreach(var player in PlayerFarming.players){
                if(player != null && player.health != null){
                    player.health.ClearAllStasisEffects();
                    player.health.ClearPoison();
                    player.health.ClearBurn();
                }
            }
            CultUtils.PlayNotification("Status effects cleared!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to clear status effects: {e.Message}");
            CultUtils.PlayNotification("Failed to clear status effects!");
        }
    }

    [CheatDetails("Unlock Crown Abilities", "Unlocks all crown abilities (arrows, grapple, fishing, etc.)")]
    public static void UnlockAllCrownAbilities(){
        try {
            int count = 0;
            foreach(var abilityType in Enum.GetValues(typeof(CrownAbilities.TYPE))){
                CrownAbilities.TYPE type = (CrownAbilities.TYPE)abilityType;
                if(!CrownAbilities.CrownAbilityUnlocked(type)){
                    CrownAbilities.UnlockAbility(type);
                    count++;
                }
            }
            CultUtils.PlayNotification($"Crown abilities unlocked! ({count} new)");
        } catch(Exception e){
            Debug.LogWarning($"Failed to unlock crown abilities: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock crown abilities!");
        }
    }

    [CheatDetails("Stop Time In Crusade", "Stop Time (OFF)", "Stop Time (ON)", "Stops base time from passing while in a crusade", true)]
    public static void StopTimeInCrusade(bool flag){
        try {
            SettingsManager.Settings.Accessibility.StopTimeInCrusade = flag;
            CultUtils.PlayNotification(flag ? "Time stops during crusades!" : "Time passes during crusades!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to toggle crusade time stop: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle crusade time!");
        }
    }

    private static float s_originalRunSpeed = -1f;

    [CheatDetails("Player Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the player's movement speed without affecting the world", true)]
    public static void PlayerSpeedDouble(bool flag){
        try {
            if(PlayerFarming.Instance != null){
                var controller = PlayerFarming.Instance.playerController;
                if(flag){
                    if(s_originalRunSpeed < 0f){
                        s_originalRunSpeed = controller.DefaultRunSpeed;
                    }
                    controller.RunSpeed = s_originalRunSpeed * 2f;
                    controller.DefaultRunSpeed = s_originalRunSpeed * 2f;
                } else {
                    if(s_originalRunSpeed >= 0f){
                        controller.RunSpeed = s_originalRunSpeed;
                        controller.DefaultRunSpeed = s_originalRunSpeed;
                    }
                    s_originalRunSpeed = -1f;
                }
            }
            CultUtils.PlayNotification(flag ? "Player speed x2!" : "Player speed normal!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set player speed: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle player speed!");
        }
    }
}

