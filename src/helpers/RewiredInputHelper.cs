using System;
using Rewired;
using UnityEngine;

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

    // When R3 is consumed by the cheat menu, suppress the in-game action for a short window
    private static float s_r3SuppressUntil = 0f;
    private static readonly float R3_SUPPRESS_DURATION = 0.3f;

    /// <summary>
    /// Whether the in-game R3 action (e.g. bahhh/bleat) should be suppressed.
    /// </summary>
    public static bool ShouldSuppressR3 => Time.unscaledTime < s_r3SuppressUntil;

    [Init]
    public static void Init(){
        s_initialized = false;
        s_player = null;
        s_r3SuppressUntil = 0f;
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
    /// Reads the right stick Y axis only (D-pad is left for the game).
    /// </summary>
    public static int GetNavigationVertical(){
        var p = GetPlayer();
        if(p != null){
            try {
                float best = 0f;
                foreach(Joystick j in p.controllers.Joysticks){
                    // Right stick Y is typically axis index 3
                    if(j.axisCount > 3){
                        float v = j.GetAxisRaw(3);
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
    /// Reads the right stick X axis only (D-pad is left for the game).
    /// </summary>
    public static int GetNavigationHorizontal(){
        var p = GetPlayer();
        if(p != null){
            try {
                float best = 0f;
                foreach(Joystick j in p.controllers.Joysticks){
                    // Right stick X is typically axis index 2
                    if(j.axisCount > 2){
                        float v = j.GetAxisRaw(2);
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

    /// <summary>
    /// Check for R3 (Right Stick Click) press to open/close the cheat menu.
    /// R3 is typically button 9. When detected, starts a suppression window
    /// so the in-game bahhh/bleat action is blocked.
    /// </summary>
    public static bool GetToggleMenuPressed(){
        var p = GetPlayer();
        if(p != null){
            try {
                foreach(Joystick j in p.controllers.Joysticks){
                    bool r3Pressed = j.buttonCount > 9 && j.GetButtonDown(9);
                    if(r3Pressed){
                        s_r3SuppressUntil = Time.unscaledTime + R3_SUPPRESS_DURATION;
                        return true;
                    }
                }
            } catch { }
        }
        return false;
    }

    /// <summary>
    /// Returns true if R3 (button 9) is currently held.
    /// Used to suppress A-button select while the toggle press is active.
    /// </summary>
    public static bool IsR3Held(){
        var p = GetPlayer();
        if(p != null){
            try {
                foreach(Joystick j in p.controllers.Joysticks){
                    if(j.buttonCount > 9 && j.GetButton(9)) return true;
                }
            } catch { }
        }
        return false;
    }
}
