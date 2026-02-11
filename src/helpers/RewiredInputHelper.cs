using System;
using Rewired;
using UnityEngine;
using UnityAnnotationHelpers;

namespace CheatMenu;

/// <summary>
/// Reads controller input through Rewired (the input system COTL uses).
/// Since COTL uses Rewired exclusively for controller input, Unity's
/// Input.GetKey(KeyCode.JoystickButton*) will never fire for gamepads.
/// This helper reads directly from the Rewired Player object.
/// </summary>
public static class RewiredInputHelper {
    private static Rewired.Player s_player;
    private static bool s_initialized = false;

    [Init]
    public static void Init(){
        s_initialized = false;
        s_player = null;
    }

    private static Rewired.Player GetPlayer(){
        if(s_player != null) return s_player;

        try {
            if(!ReInput.isReady) return null;
            s_player = ReInput.players.GetPlayer(0);
            if(s_player != null && !s_initialized){
                s_initialized = true;
                UnityEngine.Debug.Log("[CheatMenu] Rewired player 0 acquired for controller input");
            }
        } catch {
            s_player = null;
        }
        return s_player;
    }

    /// <summary>
    /// Whether the Rewired player is available.
    /// </summary>
    public static bool IsReady => GetPlayer() != null;

    /// <summary>
    /// Check if any controller/joystick is currently connected to the Rewired player.
    /// </summary>
    public static bool IsControllerConnected(){
        var p = GetPlayer();
        if(p != null){
            try { return p.controllers.joystickCount > 0; } catch { }
        }
        string[] joysticks = Input.GetJoystickNames();
        if(joysticks != null){
            foreach(string name in joysticks){
                if(!string.IsNullOrEmpty(name)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets vertical navigation input from the controller.
    /// Returns 1 for up, -1 for down, 0 for none.
    /// Reads the left stick Y axis and D-pad from the Rewired player
    /// by scanning all connected joysticks directly.
    /// </summary>
    public static int GetNavigationVertical(){
        var p = GetPlayer();
        if(p != null){
            try {
                // Read all axes from every connected joystick and pick the one with the
                // largest absolute value. This covers left stick, right stick AND D-pad
                // regardless of how the game's Rewired InputManager maps them to actions.
                float best = 0f;
                foreach(Joystick j in p.controllers.Joysticks){
                    // Left stick Y is typically axis index 1
                    if(j.axisCount > 1){
                        float v = j.GetAxisRaw(1);
                        if(Mathf.Abs(v) > Mathf.Abs(best)) best = v;
                    }
                    // D-pad Y is typically axis index 7 on XInput, but can vary.
                    // Walk all axes past index 1 looking for significant input.
                    for(int a = 4; a < j.axisCount; a++){
                        float v = j.GetAxisRaw(a);
                        if(Mathf.Abs(v) > Mathf.Abs(best)) best = v;
                    }
                }
                if(best > 0.5f) return 1;
                if(best < -0.5f) return -1;
            } catch { }
        }

        // Keyboard fallback
        if(Input.GetKey(KeyCode.UpArrow)) return 1;
        if(Input.GetKey(KeyCode.DownArrow)) return -1;
        return 0;
    }

    /// <summary>
    /// Gets horizontal navigation input from the controller.
    /// Returns 1 for right, -1 for left, 0 for none.
    /// </summary>
    public static int GetNavigationHorizontal(){
        var p = GetPlayer();
        if(p != null){
            try {
                float best = 0f;
                foreach(Joystick j in p.controllers.Joysticks){
                    if(j.axisCount > 0){
                        float v = j.GetAxisRaw(0);
                        if(Mathf.Abs(v) > Mathf.Abs(best)) best = v;
                    }
                    for(int a = 4; a < j.axisCount; a++){
                        float v = j.GetAxisRaw(a);
                        if(Mathf.Abs(v) > Mathf.Abs(best)) best = v;
                    }
                }
                if(best > 0.5f) return 1;
                if(best < -0.5f) return -1;
            } catch { }
        }

        if(Input.GetKey(KeyCode.RightArrow)) return 1;
        if(Input.GetKey(KeyCode.LeftArrow)) return -1;
        return 0;
    }

    /// <summary>
    /// Check for "select / confirm" (A / Cross) press this frame.
    /// Reads the south-face button directly from each connected joystick.
    /// </summary>
    public static bool GetSelectPressed(){
        var p = GetPlayer();
        if(p != null){
            try {
                foreach(Joystick j in p.controllers.Joysticks){
                    // Button 0 is south face (A on Xbox, Cross on PS)
                    if(j.buttonCount > 0 && j.GetButtonDown(0)) return true;
                }
            } catch { }
        }
        return false;
    }

    /// <summary>
    /// Check for "back / cancel" (B / Circle) press this frame.
    /// </summary>
    public static bool GetBackPressed(){
        var p = GetPlayer();
        if(p != null){
            try {
                foreach(Joystick j in p.controllers.Joysticks){
                    // Button 1 is east face (B on Xbox, Circle on PS)
                    if(j.buttonCount > 1 && j.GetButtonDown(1)) return true;
                }
            } catch { }
        }
        return false;
    }

    /// <summary>
    /// Check for "menu / start / pause" press this frame.
    /// Reads button indices 6 and 7 (Back/Start on Xbox, Share/Options on PS).
    /// </summary>
    public static bool GetMenuPressed(){
        var p = GetPlayer();
        if(p != null){
            try {
                foreach(Joystick j in p.controllers.Joysticks){
                    // 6 = Back/Select/Share, 7 = Start/Options
                    if(j.buttonCount > 7 && j.GetButtonDown(7)) return true;
                    if(j.buttonCount > 6 && j.GetButtonDown(6)) return true;
                }
            } catch { }
        }
        return false;
    }
}
