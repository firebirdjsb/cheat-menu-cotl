using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.MISC)]
public class MiscDefinitions : IDefinition{
    [CheatDetails("Noclip", "Noclip (OFF)", "Noclip (ON)", "Collide with nothing!", true, subGroup: "Debug")]
    public static void Noclip(){
        Traverse.Create(typeof(CheatConsole)).Method("ToggleNoClip").GetValue();
        CultUtils.PlayNotification("Noclip toggled!");
    }

    [CheatDetails("FPS Debug", "FPS Debug (OFF)", "FPS Debug (ON)", "Displays the built-in FPS Debug menu", true, subGroup: "Debug")]
    public static void FPSDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("FPS").GetValue();
        CultUtils.PlayNotification("FPS debug toggled!");
    }

    [CheatDetails("Follower Debug", "Follower Debug (OFF)", "Follower Debug (ON)", "Shows Follower Debug Information", true, subGroup: "Debug")]
    public static void FollowerDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("FollowerDebug").GetValue();
        CultUtils.PlayNotification("Follower debug toggled!");
    }

    [CheatDetails("Structure Debug", "Structure Debug (OFF)", "Structure Debug (ON)", "Shows Structure Debug Information", true, subGroup: "Debug")]
    public static void StructureDebug(){
        Traverse.Create(typeof(CheatConsole)).Method("StructureDebug").GetValue();
        CultUtils.PlayNotification("Structure debug toggled!");
    }

    [CheatDetails("Hide/Show UI", "Hide UI", "Show UI", "Show/Hide the UI of the game", true, subGroup: "Debug")]
    public static void ShowUI(bool flag){
        if(flag){
            CheatConsole.HideUI();
            CultUtils.PlayNotification("UI hidden!");
        } else {
            CheatConsole.ShowUI();
            CultUtils.PlayNotification("UI shown!");
        }        
    }

    [CheatDetails("Skip Hour", "Skip an hour of game time", subGroup: "Time")]
    public static void SkipHour(){
       Traverse.Create(typeof(CheatConsole)).Method("SkipHour").GetValue();
       CultUtils.PlayNotification("Skipped 1 hour!");
    }

    [CheatDetails("Skip Day", "Skip a full day of game time", subGroup: "Time")]
    public static void SkipDay(){
        for(int i = 0; i < 24; i++){
            Traverse.Create(typeof(CheatConsole)).Method("SkipHour").GetValue();
        }
        CultUtils.PlayNotification("Skipped 1 day!");
    }

    [CheatDetails("Game Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the game speed using time scale", true, subGroup: "Speed")]
    public static void GameSpeedDouble(bool flag){
        Time.timeScale = flag ? 2f : 1f;
        CultUtils.PlayNotification(flag ? "Game speed x2!" : "Game speed normal!");
    }

    [CheatDetails("Game Speed x4", "Speed x4 (OFF)", "Speed x4 (ON)", "Quadruples the game speed using time scale", true, subGroup: "Speed")]
    public static void GameSpeedQuadruple(bool flag){
        Time.timeScale = flag ? 4f : 1f;
        CultUtils.PlayNotification(flag ? "Game speed x4!" : "Game speed normal!");
    }

    [CheatDetails("Pause Simulation", "Pause Sim (OFF)", "Pause Sim (ON)", "Pause game simulation (followers stop acting)", true, subGroup: "Speed")]
    public static void PauseSimulation(bool flag){
        if(flag){
            SimulationManager.Pause();
            CultUtils.PlayNotification("Simulation paused!");
        } else {
            SimulationManager.UnPause();
            CultUtils.PlayNotification("Simulation unpaused!");
        }
    }

    [CheatDetails("Skip To Night", "Skips time forward to the night phase", subGroup: "Time")]
    public static void SkipToNight(){
        try {
            CheatConsole.SkipToPhase(DayPhase.Night);
            CultUtils.PlayNotification("Skipped to night!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to skip to night: {e.Message}");
            CultUtils.PlayNotification("Failed to skip to night!");
        }
    }

    [CheatDetails("Skip To Dawn", "Skips time forward to the dawn phase", subGroup: "Time")]
    public static void SkipToDawn(){
        try {
            CheatConsole.SkipToPhase(DayPhase.Dawn);
            CultUtils.PlayNotification("Skipped to dawn!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to skip to dawn: {e.Message}");
            CultUtils.PlayNotification("Failed to skip to dawn!");
        }
    }

    [CheatDetails("End Knucklebones", "Ends the current knucklebones game", subGroup: "MiniGames")]
    public static void EndKnucklebones(){
        try {
            Traverse.Create(typeof(CheatConsole)).Method("EndKnucklebones").GetValue();
            CultUtils.PlayNotification("Knucklebones ended!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to end knucklebones: {e.Message}");
            CultUtils.PlayNotification("Not in a knucklebones game!");
        }
    }

    [CheatDetails("Auto Win Flockade", "Makes the player win the current Flockade game (requires being in a game)", subGroup: "MiniGames")]
    public static void AutoWinFlockade(){
        try {
            // Find the FlockadeGameScreen instance
            var flockadeGameScreenType = Type.GetType("Flockade.FlockadeGameScreen, Assembly-CSharp");
            if(flockadeGameScreenType == null){
                CultUtils.PlayNotification("Flockade not available!");
                return;
            }
            
            var flockadeGameScreen = UnityEngine.Object.FindObjectOfType(flockadeGameScreenType);
            if(flockadeGameScreen == null){
                CultUtils.PlayNotification("Not in a Flockade game!");
                return;
            }
            
            // Get the player field
            var playerField = flockadeGameScreenType.GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);
            if(playerField == null){
                CultUtils.PlayNotification("Failed to find player!");
                return;
            }
            
            var player = playerField.GetValue(flockadeGameScreen);
            if(player == null){
                CultUtils.PlayNotification("Player not found!");
                return;
            }
            
            // Get the Victories property
            var victoriesProperty = player.GetType().GetProperty("Victories");
            if(victoriesProperty == null){
                CultUtils.PlayNotification("Failed to find victories!");
                return;
            }
            
            var victories = victoriesProperty.GetValue(player);
            if(victories == null){
                CultUtils.PlayNotification("Victories not found!");
                return;
            }
            
            // Get the Count property of the victory counter
            var countProperty = victories.GetType().GetProperty("Count");
            if(countProperty == null){
                // Try getting Max property for the maximum
                var maxProperty = victories.GetType().GetProperty("Maximum");
                if(maxProperty != null){
                    // Set count to max (2 wins for best of 3)
                    countProperty = victories.GetType().GetProperty("Count");
                    if(countProperty != null){
                        countProperty.SetValue(victories, maxProperty.GetValue(victories));
                    }
                }
            } else {
                // Set count to 2 (win best of 3)
                countProperty.SetValue(victories, 2);
            }
            
            // Now force end the game
            var forceEndGameMethod = flockadeGameScreenType.GetMethod("ForceEndGame");
            if(forceEndGameMethod != null){
                forceEndGameMethod.Invoke(flockadeGameScreen, null);
                CultUtils.PlayNotification("Flockade auto-win!");
            } else {
                CultUtils.PlayNotification("Failed to end game!");
            }
        } catch(Exception e){
            Debug.LogWarning($"Failed to auto-win Flockade: {e.Message}");
            CultUtils.PlayNotification("Failed to auto-win Flockade!");
        }
    }

    [CheatDetails("Stop Time In Crusade", "Stop Time (OFF)", "Stop Time (ON)", "Stops base time from passing while in a crusade", true, subGroup: "Time")]
    public static void StopTimeInCrusade(bool flag){
        try {
            SettingsManager.Settings.Accessibility.StopTimeInCrusade = flag;
            CultUtils.PlayNotification(flag ? "Time stops during crusades!" : "Time passes during crusades!");
        } catch(Exception e){
            Debug.LogWarning($"Failed to toggle crusade time stop: {e.Message}");
            CultUtils.PlayNotification("Failed to toggle crusade time!");
        }
    }

    // MOVED TO COMBAT TAB
    // private static float s_originalRunSpeed = -1f;
    // [CheatDetails("Player Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the player's movement speed without affecting the world", true, subGroup: "Speed")]
    // public static void PlayerSpeedDouble(bool flag){
    //     try {
    //         if(PlayerFarming.Instance != null){
    //             var controller = PlayerFarming.Instance.playerController;
    //             if(flag){
    //                 if(s_originalRunSpeed < 0f){
    //                     s_originalRunSpeed = controller.DefaultRunSpeed;
    //                 }
    //                 controller.RunSpeed = s_originalRunSpeed * 2f;
    //                 controller.DefaultRunSpeed = s_originalRunSpeed * 2f;
    //             } else {
    //                 if(s_originalRunSpeed >= 0f){
    //                     controller.RunSpeed = s_originalRunSpeed;
    //                     controller.DefaultRunSpeed = s_originalRunSpeed;
    //                 }
    //                 s_originalRunSpeed = -1f;
    //             }
    //         }
    //         CultUtils.PlayNotification(flag ? "Player speed x2!" : "Player speed normal!");
    //     } catch(Exception e){
    //         Debug.LogWarning($"Failed to set player speed: {e.Message}");
    //         CultUtils.PlayNotification("Failed to toggle player speed!");
    //     }
    // }

    [CheatDetails("Save Editor", "Opens a save editor window for real-time save editing", subGroup: "Save")]
    public static void OpenSaveEditor(){
        // Check if save editor is unlocked via console
        if(!CheatMenuGui.IsSaveEditorUnlocked()) {
            CultUtils.PlayNotification("Save Editor is locked! Press ~ to unlock");
            return;
        }
        // Allow opening at main menu to access save selector
        SaveEditorGui.Open();
    }
}