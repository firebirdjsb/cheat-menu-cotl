using UnityEngine;
using HarmonyLib;
using System;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.SPLITSCREEN)]
public class SplitscreenDefinitions : IDefinition{

    private static GameObject FindPlayer2(){
        try {
            // In COTL splitscreen, Player 2 is a second PlayerFarming instance
            PlayerFarming[] allPlayers = UnityEngine.Object.FindObjectsOfType<PlayerFarming>();
            if(allPlayers.Length > 1){
                // First player is PlayerFarming.Instance, second is Player 2
                foreach(var player in allPlayers){
                    if(player != PlayerFarming.Instance){
                        return player.gameObject;
                    }
                }
            }
        } catch { }

        try {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if(players.Length > 1){
                return players[1];
            }
        } catch { }

        return null;
    }

    private static Health GetPlayer2Health(){
        GameObject p2 = FindPlayer2();
        if(p2 != null){
            return p2.GetComponent<Health>();
        }
        return null;
    }

    [CheatDetails("P2: Heal x1", "Heals a Red Heart of Player 2")]
    public static void P2HealRed(){
        Health health = GetPlayer2Health();
        if(health != null){
            health.Heal(2f);
            CultUtils.PlayNotification("P2: Healed 1 red heart!");
        } else {
            CultUtils.PlayNotification("Player 2 not found!");
        }
    }

    [CheatDetails("P2: Add x1 Blue Heart", "Adds a Blue Heart to Player 2")]
    public static void P2AddBlueHeart(){
        Health health = GetPlayer2Health();
        if(health != null){
            health.BlueHearts += 2;
            CultUtils.PlayNotification("P2: Blue heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found!");
        }
    }

    [CheatDetails("P2: Add x1 Black Heart", "Adds a Black Heart to Player 2")]
    public static void P2AddBlackHeart(){
        Health health = GetPlayer2Health();
        if(health != null){
            health.BlackHearts += 2;
            CultUtils.PlayNotification("P2: Black heart added!");
        } else {
            CultUtils.PlayNotification("Player 2 not found!");
        }
    }

    [CheatDetails("P2: Die", "Kills Player 2")]
    public static void P2Die(){
        GameObject p2 = FindPlayer2();
        if(p2 != null){
            Health health = p2.GetComponent<Health>();
            health.DealDamage(9999f, p2, p2.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
            CultUtils.PlayNotification("P2: You died!");
        } else {
            CultUtils.PlayNotification("Player 2 not found!");
        }
    }

    [CheatDetails("P2: Godmode", "Gives Invincibility to Player 2", true)]
    public static void P2GodMode(bool flag){
        try {
            GameObject p2 = FindPlayer2();
            if(p2 != null){
                Health health = p2.GetComponent<Health>();
                if(health != null){
                    Traverse.Create(health).Property("Invincible").SetValue(flag);
                    CultUtils.PlayNotification(flag ? "P2: Godmode enabled!" : "P2: Godmode disabled!");
                } else {
                    CultUtils.PlayNotification("Player 2 health not found!");
                }
            } else {
                CultUtils.PlayNotification("Player 2 not found!");
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"P2 Godmode failed: {e.Message}");
            CultUtils.PlayNotification("P2: Godmode failed!");
        }
    }

    [CheatDetails("P2: Full Heal", "Fully heals Player 2")]
    public static void P2FullHeal(){
        Health health = GetPlayer2Health();
        if(health != null){
            health.Heal(999f);
            CultUtils.PlayNotification("P2: Fully healed!");
        } else {
            CultUtils.PlayNotification("Player 2 not found!");
        }
    }
}
