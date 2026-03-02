using System;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.HEALTH)]
public class HealthDefinitions : IDefinition{

    [CheatDetails("Godmode", "Godmode (OFF)", "Godmode (ON)", "Full invincibility - no damage taken", true, subGroup: "Modes")]
    public static void GodMode(bool flag){
        foreach(var player in PlayerFarming.players){
            player.health.GodMode = flag ? Health.CheatMode.God : Health.CheatMode.None;
        }
        CultUtils.PlayNotification(flag ? "Godmode ON!" : "Godmode OFF!");
    }

    [CheatDetails("Demigod Mode", "Demigod (OFF)", "Demigod (ON)", "Take damage but cannot die below 1 HP", true, subGroup: "Modes")]
    public static void DemiGodMode(bool flag){
        foreach(var player in PlayerFarming.players){
            player.health.GodMode = flag ? Health.CheatMode.Demigod : Health.CheatMode.None;
        }
        CultUtils.PlayNotification(flag ? "Demigod ON!" : "Demigod OFF!");
    }

    [CheatDetails("Immortal Mode", "Immortal (OFF)", "Immortal (ON)", "Cannot die at all even at 0 HP", true, subGroup: "Modes")]
    public static void ImmortalMode(bool flag){
        foreach(var player in PlayerFarming.players){
            player.health.GodMode = flag ? Health.CheatMode.Immortal : Health.CheatMode.None;
        }
        CultUtils.PlayNotification(flag ? "Immortal ON!" : "Immortal OFF!");
    }
    
    [CheatDetails("Heal x1", "Heals a Red Heart of the Player", subGroup: "Heal")]
    public static void HealRed(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.Heal(2f);
            CultUtils.PlayNotification("Healed 1 red heart!");
        }
    }

    [CheatDetails("Full Heal", "Fully heals the Player to max HP", subGroup: "Heal")]
    public static void FullHeal(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.Heal(999f);
            CultUtils.PlayNotification("Fully healed!");
        }
    }

    [CheatDetails("Add x1 Red Heart", "Permanently adds a Red Heart container", subGroup: "Hearts")]
    public static void AddRedHeart(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.totalHP += 2f;
            PlayerFarming.Instance.health.Heal(2f);
            CultUtils.PlayNotification("Red heart added!");
        }
    }

    [CheatDetails("Add x1 Blue Heart", "Adds a Blue Heart to the Player", subGroup: "Hearts")]
    public static void AddBlueHeart(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.BlueHearts += 2;
            CultUtils.PlayNotification("Blue heart added!");
        }
    }

    [CheatDetails("Add x1 Black Heart", "Adds a Black Heart to the Player", subGroup: "Hearts")]
    public static void AddBlackHeart(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.BlackHearts += 2;
            CultUtils.PlayNotification("Black heart added!");
        }
    }

    [CheatDetails("Add x1 Spirit Heart", "Adds a full Spirit Heart to the Player", subGroup: "Hearts")]
    public static void AddSpiritHeart(){
        if(PlayerFarming.Instance != null){
            ((HealthPlayer)PlayerFarming.Instance.health).TotalSpiritHearts += 2f;
            CultUtils.PlayNotification("Spirit heart added!");
        }
    }

    [CheatDetails("Add x1 Fire Heart", "Adds a Fire Heart to the Player", subGroup: "Hearts")]
    public static void AddFireHeart(){
        if(PlayerFarming.Instance != null){
            ((HealthPlayer)PlayerFarming.Instance.health).FireHearts += 2f;
            CultUtils.PlayNotification("Fire heart added!");
        }
    }

    [CheatDetails("Add x1 Ice Heart", "Adds an Ice Heart to the Player", subGroup: "Hearts")]
    public static void AddIceHeart(){
        if(PlayerFarming.Instance != null){
            ((HealthPlayer)PlayerFarming.Instance.health).IceHearts += 2f;
            CultUtils.PlayNotification("Ice heart added!");
        }
    }

    [CheatDetails("Unlimited HP", "Unlimited HP (OFF)", "Unlimited HP (ON)", "Uses the game's built-in accessibility unlimited HP option", true, subGroup: "Modes")]
    public static void UnlimitedHP(bool flag){
        try {
            SettingsManager.Settings.Accessibility.UnlimitedHP = flag;
            CultUtils.PlayNotification(flag ? "Unlimited HP ON!" : "Unlimited HP OFF!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to toggle unlimited HP: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle unlimited HP!");
        }
    }

    [CheatDetails("Die", "Kills the Player instantly", subGroup: "Heal")]
    public static void Die(){
        if(PlayerFarming.Instance != null){
            PlayerFarming.Instance.health.DealDamage(9999f, PlayerFarming.Instance.gameObject, PlayerFarming.Instance.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
            CultUtils.PlayNotification("You died!");
        }
    }
}