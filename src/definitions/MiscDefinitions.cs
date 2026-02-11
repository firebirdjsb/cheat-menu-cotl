using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.MISC)]
public class MiscDefinitions : IDefinition{
    [CheatDetails("Noclip", "Collide with nothing!", true)]
    public static void Noclip(){
        Traverse.Create(typeof(CheatConsole)).Method("ToggleNoClip").GetValue();
        CultUtils.PlayNotification("Noclip toggled!");
    }

    [CheatDetails("FPS Debug", "Displays the built-in FPS Debug menu", true)]
    public static void FPSDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("FPS").GetValue();
        CultUtils.PlayNotification("FPS debug toggled!");
    }

    [CheatDetails("Follower Debug", "Shows Follower Debug Information", true)]
    public static void FollowerDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("FollowerDebug").GetValue();
        CultUtils.PlayNotification("Follower debug toggled!");
    }

    [CheatDetails("Structure Debug", "Shows Structure Debug Information", true)]
    public static void StructureDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("StructureDebug").GetValue();
        CultUtils.PlayNotification("Structure debug toggled!");
    }

    [CheatDetails("Hide/Show UI", "Hide UI", "Show UI", "Show/Hide the UI of the game", true)]
    public static void ShowUI(bool flag){
        if(flag){
            Traverse.Create(typeof(CheatConsole)).Method("HideUI").GetValue();
            CultUtils.PlayNotification("UI hidden!");
        } else {
            Traverse.Create(typeof(CheatConsole)).Method("ShowUI").GetValue();
            CultUtils.PlayNotification("UI shown!");
        }        
    }

    [CheatDetails("Skip Hour", "Skip an hour of game time")]
    public static void SkipHour(){
       Traverse.Create(typeof(CheatConsole)).Method("SkipHour").GetValue();
       CultUtils.PlayNotification("Skipped 1 hour!");
    }

    [CheatDetails("Complete All Quests", "Complete All Quests")]
    public static void CompleteAllQuests(){
       CultUtils.CompleteAllQuests();
       CultUtils.PlayNotification("All quests completed!");
    }

    [CheatDetails("Game Speed x2", "Game Speed x2 (OFF)", "Game Speed x2 (ON)", "Doubles the game speed using time scale", true)]
    public static void GameSpeedDouble(bool flag){
        Time.timeScale = flag ? 2f : 1f;
        CultUtils.PlayNotification(flag ? "Game speed x2!" : "Game speed normal!");
    }

    [CheatDetails("Game Speed x4", "Game Speed x4 (OFF)", "Game Speed x4 (ON)", "Quadruples the game speed using time scale", true)]
    public static void GameSpeedQuadruple(bool flag){
        Time.timeScale = flag ? 4f : 1f;
        CultUtils.PlayNotification(flag ? "Game speed x4!" : "Game speed normal!");
    }

    [CheatDetails("Skip Day", "Skip a full day of game time")]
    public static void SkipDay(){
        try {
            for(int i = 0; i < 24; i++){
                Traverse.Create(typeof(CheatConsole)).Method("SkipHour").GetValue();
            }
            CultUtils.PlayNotification("Skipped 1 day!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to skip day: {e.Message}");
            CultUtils.PlayNotification("Failed to skip day!");
        }
    }

    [CheatDetails("Unlock All Fleeces", "Unlocks all fleece skins for the Lamb")]
    public static void UnlockAllFleeces(){
        try {
            // Try CheatConsole method first
            bool success = false;
            try {
                MethodInfo unlockMethod = typeof(CheatConsole).GetMethod("UnlockAllFleeces", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if(unlockMethod != null){
                    unlockMethod.Invoke(null, null);
                    success = true;
                }
            } catch(Exception e){
                UnityEngine.Debug.LogWarning($"CheatConsole.UnlockAllFleeces failed: {e.Message}");
            }

            if(!success){
                // Try finding the fleece list via reflection with multiple possible names
                string[] fieldNames = new[] { "UnlockedFleeces", "PlayerFleeces", "unlockedFleeces", "playerFleeces", "Fleeces" };
                Traverse dmTraverse = Traverse.Create(DataManager.Instance);
                object fleeceList = null;

                foreach(string name in fieldNames){
                    Traverse fieldTraverse = dmTraverse.Field(name);
                    if(fieldTraverse.FieldExists()){
                        fleeceList = fieldTraverse.GetValue();
                        if(fleeceList != null) break;
                    }
                    Traverse propTraverse = dmTraverse.Property(name);
                    if(propTraverse.PropertyExists()){
                        fleeceList = propTraverse.GetValue();
                        if(fleeceList != null) break;
                    }
                }

                if(fleeceList != null && fleeceList.GetType().IsGenericType){
                    MethodInfo addMethod = fleeceList.GetType().GetMethod("Add");
                    MethodInfo containsMethod = fleeceList.GetType().GetMethod("Contains");
                    Type elementType = fleeceList.GetType().GetGenericArguments()[0];
                    if(addMethod != null && containsMethod != null){
                        foreach(var val in Enum.GetValues(elementType)){
                            if(!(bool)containsMethod.Invoke(fleeceList, new object[]{val})){
                                addMethod.Invoke(fleeceList, new object[]{val});
                            }
                        }
                        success = true;
                    }
                }

                // Last resort: search all fields/properties on DataManager for a list/hashset containing fleece-related types
                if(!success){
                    Type dmType = DataManager.Instance.GetType();
                    foreach(FieldInfo field in dmType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)){
                        if(field.Name.ToLower().Contains("fleece")){
                            object value = field.GetValue(DataManager.Instance);
                            if(value != null && value.GetType().IsGenericType){
                                MethodInfo addM = value.GetType().GetMethod("Add");
                                MethodInfo containsM = value.GetType().GetMethod("Contains");
                                Type elemType = value.GetType().GetGenericArguments()[0];
                                if(addM != null && containsM != null && elemType.IsEnum){
                                    foreach(var val in Enum.GetValues(elemType)){
                                        try {
                                            if(!(bool)containsM.Invoke(value, new object[]{val})){
                                                addM.Invoke(value, new object[]{val});
                                            }
                                        } catch { }
                                    }
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach(PropertyInfo prop in dmType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)){
                        if(prop.Name.ToLower().Contains("fleece") && !success){
                            object value = prop.GetValue(DataManager.Instance);
                            if(value != null && value.GetType().IsGenericType){
                                MethodInfo addM = value.GetType().GetMethod("Add");
                                MethodInfo containsM = value.GetType().GetMethod("Contains");
                                Type elemType = value.GetType().GetGenericArguments()[0];
                                if(addM != null && containsM != null && elemType.IsEnum){
                                    foreach(var val in Enum.GetValues(elemType)){
                                        try {
                                            if(!(bool)containsM.Invoke(value, new object[]{val})){
                                                addM.Invoke(value, new object[]{val});
                                            }
                                        } catch { }
                                    }
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            CultUtils.PlayNotification(success ? "All fleeces unlocked!" : "Could not find fleece data - check game version!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to unlock fleeces: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock fleeces!");
        }
    }

    [CheatDetails("Unlock All Decorations", "Unlocks all decoration types")]
    public static void UnlockAllDecorations(){
        try {
            bool success = false;
            MethodInfo method = typeof(CheatConsole).GetMethod("UnlockAllDecorations", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if(method != null){
                method.Invoke(null, null);
                success = true;
            }
            CultUtils.PlayNotification(success ? "All decorations unlocked!" : "UnlockAllDecorations not found in this game version!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to unlock decorations: {e.Message}");
            CultUtils.PlayNotification("Failed to unlock decorations!");
        }
    }
}