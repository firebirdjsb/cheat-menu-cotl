using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.COMBAT)]
public class CombatDefinitions : IDefinition{

    private static bool s_oneHitKillEnabled = false;
    private static bool s_unlimitedRelicsEnabled = false;
    private static bool s_unlimitedAmmoEnabled = false;
    private static bool s_ammoPatched = false;

    private static bool IsInDungeon(){
        try {
            return PlayerFarming.Instance != null && PlayerFarming.Location != FollowerLocation.Base;
        } catch {
            return false;
        }
    }

    [CheatDetails("Kill All Enemies", "Kills all enemies in the current room")]
    public static void KillAllEnemies(){
        if(!IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            return;
        }
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

    private static bool s_mapRevealEnabled = false;
    private static bool s_mapRevealPatched = false;

    /// <summary>
    /// Harmony postfix for MiniMap.StartMap - automatically reveals the dungeon map
    /// every time a new floor/room set is generated while the toggle is on.
    /// </summary>
    public static void Postfix_MiniMap_StartMap(MiniMap __instance){
        if(!s_mapRevealEnabled) return;
        try {
            __instance.DiscoverAll();
        } catch { }
    }

    private static void PatchMiniMapReveal(){
        if(s_mapRevealPatched) return;
        try {
            MethodInfo patchMethod = typeof(CombatDefinitions).GetMethod("Postfix_MiniMap_StartMap", BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPostfix(
                typeof(MiniMap),
                "StartMap",
                patchMethod,
                BindingFlags.Instance | BindingFlags.Public,
                silent: true
            );
            s_mapRevealPatched = true;
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to patch MiniMap.StartMap: {e.Message}");
        }
    }

    private static void UnpatchMiniMapReveal(){
        if(!s_mapRevealPatched) return;
        ReflectionHelper.UnpatchTracked(typeof(MiniMap), "StartMap");
        s_mapRevealPatched = false;
    }

    [CheatDetails("Auto Reveal Dungeon Map", "Map Reveal (OFF)", "Map Reveal (ON)", "Automatically reveals the dungeon map on every new floor", true)]
    public static void ShowAllMapLocations(bool flag){
        s_mapRevealEnabled = flag;
        CheatConsole.ShowAllMapLocations = flag;
        if(flag){
            PatchMiniMapReveal();
            try {
                if(MiniMap.Instance != null){
                    MiniMap.Instance.DiscoverAll();
                }
            } catch(Exception e){
                Debug.LogWarning($"Map reveal failed: {e.Message}");
            }
        } else {
            UnpatchMiniMapReveal();
        }
        CultUtils.PlayNotification(flag ? "Auto map reveal ON!" : "Auto map reveal OFF!");
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

    public static bool Prefix_PlayerRelic_ResetChargedAmount(PlayerRelic __instance){
        if(!s_unlimitedRelicsEnabled) return true;
        // Instead of resetting, fully charge the relic so it can be used again immediately
        __instance.FullyCharge();
        return false;
    }

    [CheatDetails("Unlimited Relics", "Unlimited Relics (OFF)", "Unlimited Relics (ON)", "Relics never run out of charges during combat", true)]
    public static void UnlimitedRelics(bool flag){
        if(flag && !IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedRelics"), false);
            return;
        }
        s_unlimitedRelicsEnabled = flag;
        try {
            if(flag){
                // Fully charge all player relics immediately
                foreach(var player in PlayerFarming.players){
                    if(player != null && player.playerRelic != null){
                        player.playerRelic.FullyCharge();
                    }
                }
                // Patch ResetChargedAmount so relics re-charge after every use
                MethodInfo patchMethod = typeof(CombatDefinitions).GetMethod("Prefix_PlayerRelic_ResetChargedAmount", BindingFlags.Static | BindingFlags.Public);
                ReflectionHelper.PatchMethodPrefix(
                    typeof(PlayerRelic),
                    "ResetChargedAmount",
                    patchMethod,
                    BindingFlags.Instance | BindingFlags.Public,
                    silent: true
                );
            } else {
                ReflectionHelper.UnpatchTracked(typeof(PlayerRelic), "ResetChargedAmount");
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
        if(flag && !IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "OneHitKill"), false);
            return;
        }
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

    /// <summary>
    /// Harmony prefix for BlunderAmmo.UseAmmo - prevents ammo consumption and instantly refills.
    /// </summary>
    public static bool Prefix_BlunderAmmo_UseAmmo(BlunderAmmo __instance, ref bool __result){
        if(!s_unlimitedAmmoEnabled) return true;
        try {
            // Refill to max instead of consuming
            if(__instance.gameObject.activeInHierarchy){
                __instance.SetBlunderAmmo((float)__instance.blunderAmmoTotal);
            }
        } catch { }
        __result = true;
        return false;
    }

    /// <summary>
    /// Harmony prefix for PlayerArrows.RestockArrow - instant full restock when unlimited ammo is on.
    /// </summary>
    public static bool Prefix_PlayerArrows_RestockArrow(PlayerArrows __instance){
        if(!s_unlimitedAmmoEnabled) return true;
        // Skip the slow one-by-one restock, just fully restock instantly
        __instance.RestockAllArrows();
        return false;
    }

    private static void RefillAllAmmoImmediate(){
        try {
            if(PlayerFarming.Instance == null) return;
            // Refill arrows via PlayerArrows component (respects TOTAL_AMMO caps properly)
            var playerArrows = PlayerFarming.Instance.GetComponent<PlayerArrows>();
            if(playerArrows != null){
                playerArrows.RestockAllArrows();
            }
            // Refill blunderbuss ammo via PlayerWeapon.blunderAmmo
            // Only access when the BlunderAmmo component is active and initialized
            // to avoid null reference errors from AmmoChanged accessing inactive UI elements
            if(PlayerFarming.Instance.playerWeapon != null && PlayerFarming.Instance.playerWeapon.blunderAmmo != null){
                var blunderAmmo = PlayerFarming.Instance.playerWeapon.blunderAmmo;
                if(blunderAmmo.gameObject.activeInHierarchy && blunderAmmo.blunderAmmo < (float)blunderAmmo.blunderAmmoTotal){
                    blunderAmmo.SetBlunderAmmo((float)blunderAmmo.blunderAmmoTotal);
                }
            }
        } catch { }
    }

    private static void PatchAmmoMethods(){
        if(s_ammoPatched) return;
        try {
            // Patch BlunderAmmo.UseAmmo to prevent ammo consumption
            MethodInfo blunderPatch = typeof(CombatDefinitions).GetMethod("Prefix_BlunderAmmo_UseAmmo", BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPrefix(
                typeof(BlunderAmmo),
                "UseAmmo",
                blunderPatch,
                BindingFlags.Instance | BindingFlags.Public,
                silent: true
            );
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to patch BlunderAmmo.UseAmmo: {e.Message}");
        }
        try {
            // Patch PlayerArrows.RestockArrow to make reloading instant
            MethodInfo arrowPatch = typeof(CombatDefinitions).GetMethod("Prefix_PlayerArrows_RestockArrow", BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPrefix(
                typeof(PlayerArrows),
                "RestockArrow",
                arrowPatch,
                BindingFlags.Instance | BindingFlags.Public,
                silent: true
            );
        } catch(Exception e){
            Debug.LogWarning($"[CheatMenu] Failed to patch PlayerArrows.RestockArrow: {e.Message}");
        }
        s_ammoPatched = true;
    }

    private static void UnpatchAmmoMethods(){
        if(!s_ammoPatched) return;
        ReflectionHelper.UnpatchTracked(typeof(BlunderAmmo), "UseAmmo");
        ReflectionHelper.UnpatchTracked(typeof(PlayerArrows), "RestockArrow");
        s_ammoPatched = false;
    }

    [CheatDetails("Unlimited Ammo", "Unlimited Ammo (OFF)", "Unlimited Ammo (ON)", "Arrows and blunderbuss never run out of ammo", true)]
    public static void UnlimitedAmmo(bool flag){
        if(flag && !IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedAmmo"), false);
            return;
        }
        s_unlimitedAmmoEnabled = flag;
        try {
            if(flag){
                PatchAmmoMethods();
                RefillAllAmmoImmediate();
            } else {
                UnpatchAmmoMethods();
            }
            CultUtils.PlayNotification(flag ? "Unlimited ammo ON!" : "Unlimited ammo OFF!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to set unlimited ammo: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle unlimited ammo!");
        }
    }

    [CheatDetails("Unlimited Fervour", "Unlimited Fervour (OFF)", "Unlimited Fervour (ON)", "Fervour never depletes when casting curses", true)]
    public static void UnlimitedFervour(bool flag){
        if(flag && !IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            FlagManager.SetFlagValue(Definition.GetCheatFlagID(typeof(CombatDefinitions), "UnlimitedFervour"), false);
            return;
        }
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
        if(!IsInDungeon()){
            CultUtils.PlayNotification("Must be in a dungeon to use this!");
            return;
        }
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

    [Update]
    public static void CombatUpdate(){
        if(s_unlimitedAmmoEnabled){
            try {
                RefillAllAmmoImmediate();
            } catch { }
        }
    }
}

