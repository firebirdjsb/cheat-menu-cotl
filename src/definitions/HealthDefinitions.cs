using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.HEALTH)]
public class HealthDefinitions : IDefinition{

    [CheatDetails("Godmode", "Gives Invincibility", true)]
    public static void GodMode(){
        Traverse.Create(typeof(CheatConsole)).Method("ToggleGodMode").GetValue();
        CultUtils.PlayNotification("Godmode toggled!");
    }
    
    [CheatDetails("Heal x1", "Heals a Red Heart of the Player")]
    public static void HealRed(){
        GameObject gameObject = GameObject.FindWithTag("Player");
        if (gameObject != null)
        {
            gameObject.GetComponent<Health>().Heal(2f);
            CultUtils.PlayNotification("Healed 1 red heart!");
        }
    }

    [CheatDetails("Add x1 Blue Heart", "Adds a Blue Heart to the Player")]
    public static void AddBlueHeart(){
        GameObject gameObject = GameObject.FindWithTag("Player");
        if (gameObject != null)
        {
            gameObject.GetComponent<Health>().BlueHearts += 2;
            CultUtils.PlayNotification("Blue heart added!");
        }
    }

    [CheatDetails("Add x1 Black Heart", "Adds a Black Heart to the Player")]
    public static void AddBlackHeart(){
        GameObject gameObject = GameObject.FindWithTag("Player");
        if (gameObject != null)
        {
            gameObject.GetComponent<Health>().BlackHearts += 2;
            CultUtils.PlayNotification("Black heart added!");
        }
    }

    [CheatDetails("Full Heal", "Fully heals the Player to max HP")]
    public static void FullHeal(){
        GameObject gameObject = GameObject.FindWithTag("Player");
        if (gameObject != null)
        {
            gameObject.GetComponent<Health>().Heal(999f);
            CultUtils.PlayNotification("Fully healed!");
        }
    }

    [CheatDetails("Add x1 Red Heart", "Permanently adds a Red Heart to the Player")]
    public static void AddRedHeart(){
        try {
            GameObject gameObject = GameObject.FindWithTag("Player");
            if (gameObject != null)
            {
                Health health = gameObject.GetComponent<Health>();
                Traverse healthTraverse = Traverse.Create(health);
                Traverse hpField = healthTraverse.Field("totalHP");
                if(hpField.FieldExists()){
                    hpField.SetValue(hpField.GetValue<float>() + 2f);
                } else {
                    Traverse hpProp = healthTraverse.Property("TotalHP");
                    if(hpProp.PropertyExists()){
                        hpProp.SetValue(hpProp.GetValue<float>() + 2f);
                    }
                }
                health.Heal(2f);
                CultUtils.PlayNotification("Red heart added!");
            }
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to add red heart: {e.Message}");
            CultUtils.PlayNotification("Failed to add red heart!");
        }
    }

    [CheatDetails("Die", "Kills the Player")]
    public static void Die(){
        GameObject gameObject = GameObject.FindWithTag("Player");
        if (gameObject != null)
        {
            Health healthComp = gameObject.GetComponent<Health>();
            healthComp.DealDamage(9999f, gameObject, gameObject.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
            CultUtils.PlayNotification("You died!");
        }
    }

    [CheatDetails("Add x1 Spirit Heart", "Adds a Spirit Heart to the Player")]
    public static void AddSpiritHeart(){
        try {
            GameObject gameObject = GameObject.FindWithTag("Player");
            if (gameObject != null)
            {
                Health health = gameObject.GetComponent<Health>();
                HarmonyLib.Traverse healthTraverse = HarmonyLib.Traverse.Create(health);
                HarmonyLib.Traverse spiritField = healthTraverse.Property("SpiritHearts");
                if(spiritField.PropertyExists()){
                    spiritField.SetValue(spiritField.GetValue<float>() + 2f);
                } else {
                    HarmonyLib.Traverse spiritField2 = healthTraverse.Field("SpiritHearts");
                    if(spiritField2.FieldExists()){
                        spiritField2.SetValue(spiritField2.GetValue<float>() + 2f);
                    }
                }
                CultUtils.PlayNotification("Spirit heart added!");
            }
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to add spirit heart: {e.Message}");
            CultUtils.PlayNotification("Spirit hearts not available in this version!");
        }
    }
}