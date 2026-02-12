using UnityEngine;
using HarmonyLib;
using System;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.SPLITSCREEN)]
public class SplitscreenDefinitions : IDefinition{

    private static PlayerFarming GetPlayer2(){
        if(PlayerFarming.players.Count > 1){
            return PlayerFarming.players[1];
        }
        return null;
    }

    [CheatDetails("P2: Heal x1", "Heals a Red Heart of Player 2")]
    public static void P2HealRed(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.Heal(2f);
            CultUtils.PlayNotification("P2: Healed 1 red heart!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Full Heal", "Fully heals Player 2 to max HP")]
    public static void P2FullHeal(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.Heal(999f);
            CultUtils.PlayNotification("P2: Fully healed!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Add x1 Red Heart", "Permanently adds a Red Heart container to Player 2")]
    public static void P2AddRedHeart(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.totalHP += 2f;
            p2.health.Heal(2f);
            CultUtils.PlayNotification("P2: Red heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Add x1 Blue Heart", "Adds a Blue Heart to Player 2")]
    public static void P2AddBlueHeart(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.BlueHearts += 2;
            CultUtils.PlayNotification("P2: Blue heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Add x1 Black Heart", "Adds a Black Heart to Player 2")]
    public static void P2AddBlackHeart(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.BlackHearts += 2;
            CultUtils.PlayNotification("P2: Black heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Add x1 Spirit Heart", "Adds a Spirit Heart to Player 2")]
    public static void P2AddSpiritHeart(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.TotalSpiritHearts += 2f;
            CultUtils.PlayNotification("P2: Spirit heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Godmode", "P2: Godmode (OFF)", "P2: Godmode (ON)", "Full invincibility for Player 2", true)]
    public static void P2GodMode(bool flag){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.GodMode = flag ? Health.CheatMode.God : Health.CheatMode.None;
            CultUtils.PlayNotification(flag ? "P2: Godmode ON!" : "P2: Godmode OFF!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }

    [CheatDetails("P2: Die", "Kills Player 2")]
    public static void P2Die(){
        var p2 = GetPlayer2();
        if(p2 != null){
            p2.health.DealDamage(9999f, p2.gameObject, p2.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
            CultUtils.PlayNotification("P2: You died!");
        } else {
            CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
        }
    }
}
