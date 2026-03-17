using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CheatMenu;

public static class CheatMenuGui {
    /// <summary>
    /// Checks if the player is currently at the main menu (not in a game scene)
    /// </summary>
    public static bool IsAtMainMenu() {
        try {
            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name?.ToLower() ?? "";
            // Main menu scenes typically contain "menu" or "main" in their name
            return name.Contains("menu") || name.Contains("main") || name.Contains("title");
        } catch {
            // If we can't determine, assume not at main menu
            return false;
        }
    }
    
public static bool GuiEnabled = false;
public static bool InputBlockedForModal = false;
public static CheatCategoryEnum CurrentCategory = CheatCategoryEnum.NONE;
public static string CurrentSubGroup = null;
public static int CurrentButtonY = 0;
public static int TotalWindowCalculatedHeight = 0;

// Developer Console - Password protected experimental features
private static bool _devConsoleOpen = false;
private static string _devConsoleInput = "";
private static List<string> _devConsoleHistory = new List<string>();
private static bool _saveEditorUnlocked = false; // Requires password to access
private static bool _saveSelectorUnlocked = false; // Requires password to access

// XOR-obfuscated password data
// Obfuscated using XOR with key 0x5A
private static byte[] _obfuscatedPasswordData = new byte[] { 
    0x1E, 0x3F, 0x2C, 0x0E, 0x3F, 0x29, 0x2E 
};
private const byte _xorKey = 0x5A;

// Public accessor methods for save editor unlock status
public static bool IsSaveEditorUnlocked() => _saveEditorUnlocked;
public static bool IsSaveSelectorUnlocked() => _saveSelectorUnlocked;

// XOR-based password verification (much harder to reverse than Base64)
private static bool VerifyPassword(string input) {
    try {
        // XOR decode the password
        char[] chars = new char[_obfuscatedPasswordData.Length];
        for(int i = 0; i < _obfuscatedPasswordData.Length; i++) {
            chars[i] = (char)(_obfuscatedPasswordData[i] ^ _xorKey);
        }
        string correctPassword = new string(chars);
        
        // Case-insensitive comparison
        return string.Equals(input, correctPassword, StringComparison.OrdinalIgnoreCase);
    } catch {
        return false;
    }
}

// Controller navigation
private static int s_selectedButtonIndex = 0;
private static int s_totalButtons = 0;
private static int s_currentButtonCounter = 0;
private static float s_lastNavigationTime = 0f;
private static float s_navigationDelay = 0.15f;
private static bool s_controllerSelectPressed = false;
private static bool s_needsScrollUpdate = false;

// Slide animation
private static float s_animationProgress = 0f;
private static float s_animationSpeed = 4.5f;
private static bool s_animatingIn = false;
private static bool s_animatingOut = false;
private static bool s_pendingClose = false;

private static Action s_guiContent;
private static GUIUtils.ScrollableWindowParams s_scrollParams;

// Slider controller state
private static float s_sliderHoldTime = 0f;
private static float s_sliderAccum = 0f;

// Menu dimensions
private static readonly int MENU_WIDTH = 350;
private static readonly int MENU_HEIGHT = 400;

    [Init]
    public static void Init(){
        CurrentButtonY = 0;
        TotalWindowCalculatedHeight = 0;
        s_scrollParams = new (
            "Cult Cheat Menu",
            GetMenuRect(0f)
        );
        s_guiContent = DefinitionManager.BuildGUIContentFn();
        CurrentCategory = CheatCategoryEnum.NONE;
        CurrentSubGroup = null;
        s_selectedButtonIndex = 0;
        s_currentButtonCounter = 0;
        s_animationProgress = 0f;
        s_animatingIn = false;
        s_animatingOut = false;
        s_pendingClose = false;
        s_controllerSelectPressed = false;
        s_needsScrollUpdate = false;
        s_sliderHoldTime = 0f;
        s_sliderAccum = 0f;
        InputBlockedForModal = false;
    }

    private static Rect GetMenuRect(float progress){
        float margin = 10f;
        float targetX = margin;
        float targetY = Screen.height - MENU_HEIGHT - margin;
        
        float startX = margin;
        float startY = Screen.height + 50;
        
        float easedProgress = 1f - (1f - progress) * (1f - progress);
        
        float currentX = Mathf.Lerp(startX, targetX, easedProgress);
        float currentY = Mathf.Lerp(startY, targetY, easedProgress);
        
        return new Rect(currentX, currentY, MENU_WIDTH, MENU_HEIGHT);
    }

    public static bool IsWithinCategory() {
        return CurrentCategory != CheatCategoryEnum.NONE;
    }

    public static bool IsWithinSpecificCategory(string categoryString){
        var categoryEnum = CheatCategoryEnumExtensions.GetEnumFromName(categoryString);
        if(!ShouldShowCategory(categoryEnum)) return false;
        return categoryEnum.Equals(CurrentCategory);
    }

    public static bool IsWithinSubGroup(){
        return CurrentSubGroup != null;
    }

    public static bool IsWithinSpecificSubGroup(string subGroupName){
        return CurrentSubGroup == subGroupName;
    }

    public static bool SubGroupButton(string subGroupText){
        // At main menu, only show Save subGroup
        if(IsAtMainMenu()){
            if(subGroupText != "Save") return false;
        }
        
        int buttonHeight = GUIUtils.GetButtonHeight();
        int spacing = GUIUtils.GetButtonSpacing();
        int thisButtonIndex = s_currentButtonCounter++;

        GUIStyle style = GetButtonStyleWithHover(GUIUtils.GetSubGroupButtonStyle(), thisButtonIndex);
        var btn = GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, buttonHeight), $"> {subGroupText}", style);
        TotalWindowCalculatedHeight += buttonHeight + spacing;
        CurrentButtonY += buttonHeight + spacing;

        if(IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed){
            btn = true;
        }

        if(btn){
            CurrentSubGroup = subGroupText;
            s_selectedButtonIndex = 0;
        }
        return btn;
    }

    private static bool IsButtonSelected(int buttonIndex){
        return CheatConfig.Instance.ControllerSupport.Value && s_selectedButtonIndex == buttonIndex;
    }

    private static GUIStyle GetButtonStyleWithHover(GUIStyle baseStyle, int buttonIndex){
        if(!IsButtonSelected(buttonIndex)) return baseStyle;
        
        GUIStyle hoverStyle = new(baseStyle);
        hoverStyle.normal = new GUIStyleState(){
            background = TextureHelper.GetSolidTexture(new Color(0.75f, 0.15f, 0.15f, 1f), true),
            textColor = new Color(0.95f, 0.92f, 0.88f, 1f)
        };
        return hoverStyle;
    }

    private static bool ShouldShowCategory(CheatCategoryEnum category){
        // At main menu, only show Misc category
        if(IsAtMainMenu()){
            return category == CheatCategoryEnum.MISC;
        }
        if(category == CheatCategoryEnum.FARMING && !CultUtils.HasMajorDLC()) return false;
        return true;
    }

    public static bool HasRequiredDLC(int dlcRequirement){
        switch((DlcRequirement)dlcRequirement){
            case DlcRequirement.MajorDLC:   return CultUtils.HasMajorDLC();
            case DlcRequirement.SinfulDLC:  return CultUtils.HasSinfulDLC();
            case DlcRequirement.CultistDLC: return CultUtils.HasCultistDLC();
            case DlcRequirement.HereticDLC: return CultUtils.HasHereticDLC();
            case DlcRequirement.PilgrimDLC: return CultUtils.HasPilgrimDLC();
            default: return true;
        }
    }

    public static bool CategoryButton(string categoryText){
        var categoryEnum = CheatCategoryEnumExtensions.GetEnumFromName(categoryText);
        if(!ShouldShowCategory(categoryEnum)){
            return false;
        }

        int buttonHeight = GUIUtils.GetButtonHeight();
        int spacing = GUIUtils.GetButtonSpacing();
        int thisButtonIndex = s_currentButtonCounter++;

        GUIStyle style = GetButtonStyleWithHover(GUIUtils.GetCategoryButtonStyle(), thisButtonIndex);
        var btn = GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, buttonHeight), $">> {categoryText} <<", style);
        TotalWindowCalculatedHeight += buttonHeight + spacing;
        CurrentButtonY += buttonHeight + spacing;

        if(IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed){
            btn = true;
        }

        if(btn){
            if(categoryEnum == CheatCategoryEnum.ANIMATION){
                try {
                    AnimationDefinitions.OpenAnimationBrowser();
                } catch { }
                CurrentCategory = CheatCategoryEnum.NONE;
                CurrentSubGroup = null;
                s_selectedButtonIndex = 0;
            } else {
                CurrentCategory = categoryEnum;
                CurrentSubGroup = null;
                s_selectedButtonIndex = 0;
            }
        }
        return btn;
    }

    public static void OpenMenuAtRoot(){
        GuiEnabled = true;
        CurrentCategory = CheatCategoryEnum.NONE;
        InputBlockedForModal = false;
        StartOpenAnimation();
    }
    
    public static bool BackButton(){
        int buttonHeight = GUIUtils.GetButtonHeight();
        int spacing = GUIUtils.GetButtonSpacing();
        int thisButtonIndex = s_currentButtonCounter++;

        GUIStyle style = GetButtonStyleWithHover(GUIUtils.GetGUIButtonStyle(), thisButtonIndex);
        var btn = GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, buttonHeight), $"< Back", style);
        TotalWindowCalculatedHeight += buttonHeight + spacing;
        CurrentButtonY += buttonHeight + spacing;

        if(IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed){
            btn = true;
        }

        if(btn){
            try {
                GUIManager.CloseGuiFunction("AnimationBrowser");
            } catch { }
            if(CurrentSubGroup != null){
                CurrentSubGroup = null;
                s_selectedButtonIndex = 0;
            } else {
                CurrentCategory = CheatCategoryEnum.NONE;
                CurrentSubGroup = null;
                s_selectedButtonIndex = 0;
            }
        }
        return btn;
    }

    public static bool Button(string text) {
        int spacing = GUIUtils.GetButtonSpacing();
        int thisButtonIndex = s_currentButtonCounter++;

        GUIStyle style = GetButtonStyleWithHover(GUIUtils.GetGUIButtonStyle(), thisButtonIndex);
        var btn = GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, GUIUtils.GetButtonHeight()), text, style);

        if(IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed){
            btn = true;
        }

        if(btn) {
        }

        CurrentButtonY += GUIUtils.GetButtonHeight() + spacing;
        TotalWindowCalculatedHeight += GUIUtils.GetButtonHeight() + spacing;
        return btn;
    }

    public static bool ButtonWithFlag(string onText, string offText, string flagID){
        bool flag = FlagManager.IsFlagEnabled(flagID);
        var btnText = flag ? $"[ON] {onText}" : $"[OFF] {offText}";
        int thisButtonIndex = s_currentButtonCounter++;

        GUIStyle baseStyle = flag ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle();
        GUIStyle style = GetButtonStyleWithHover(baseStyle, thisButtonIndex);
        int spacing = GUIUtils.GetButtonSpacing();
        var btn = GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, GUIUtils.GetButtonHeight()), btnText, style);

        if(IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed){
            btn = true;
        }

        if (btn)
        {
            FlagManager.FlipFlagValue(flagID);
        }
        CurrentButtonY += GUIUtils.GetButtonHeight() + spacing;
        TotalWindowCalculatedHeight += GUIUtils.GetButtonHeight() + spacing;
        return btn;
    }

    public static bool ButtonWithFlagS(string text, string flagID){
        return ButtonWithFlag($"{text} (ON)", $"{text} (OFF)", flagID);
    }

    public static bool ButtonWithFlagP(string text, string flagID){
        return ButtonWithFlagS(text, flagID);
    }

    private static void ResetLayoutValues(){
        CurrentButtonY = 0;
        TotalWindowCalculatedHeight = 0;
        s_totalButtons = s_currentButtonCounter;
        s_currentButtonCounter = 0;
        s_controllerSelectPressed = false;
    }

    [OnGui]
    public static void OnGUI(){
        if (GuiEnabled || s_animatingOut){
            // Don't draw cheat menu if save editor is open
            if (SaveEditorGui.IsOpen) {
                SaveEditorGui.Draw();
                return;
            }
            
            s_scrollParams.Title = "Cult Cheat Menu";
            if(IsWithinCategory()){
                s_scrollParams.Title = $"Cult Cheat Menu - {CurrentCategory.GetCategoryName()}";
                if(CurrentSubGroup != null){
                    s_scrollParams.Title = $"{CurrentCategory.GetCategoryName()} > {CurrentSubGroup}";
                }
            }
            
            s_scrollParams.ClientRect = GetMenuRect(s_animationProgress);
            s_scrollParams = GUIUtils.CustomWindowScrollableLocked(s_scrollParams, CheatWindow);
            
            DrawKeybindHints();
        } else {
            // Always show hint at main menu too
            DrawPersistentHint();
        }

        // Draw Save Editor if open
        SaveEditorGui.Draw();
        
        // Draw Developer Console if open
        if(_devConsoleOpen) {
            DrawDevConsole();
        }

        Action[] guiFunctions = GUIManager.GetAllGuiFunctions();
        if(guiFunctions.Length > 0){
            foreach(Action guiFn in guiFunctions){
                guiFn();
            }
        }
    }

    private static void DrawKeybindHints()
    {
        int hintWidth = MENU_WIDTH;
        int hintHeight = 25;
        float margin = 10f;
        float easedProgress = 1f - (1f - s_animationProgress) * (1f - s_animationProgress);
        int hintX = (int)margin;
        // Position hint bar directly above the menu instead of at screen bottom
        float menuY = Mathf.Lerp(Screen.height + 50, Screen.height - MENU_HEIGHT - margin, easedProgress);
        int hintY = (int)(menuY - hintHeight - 2);
        
        string hintText;
        string kbToggle = CheatConfig.Instance.GuiKeybind.Value.MainKey.ToString();
        string kbBack = CheatConfig.Instance.BackCategory.Value.MainKey.ToString();
        if(CheatConfig.Instance.ControllerSupport.Value) {
            hintText = IsWithinCategory() 
                ? $"[B/{kbBack}] Back | [LeftStickClick/{kbToggle}] Close | [Right Stick] Nav | [A] Select" 
                : $"[LeftStickClick/{kbToggle}] Toggle | [Right Stick] Nav | [A] Select";
        } else {
            hintText = IsWithinCategory() 
                ? $"[{kbBack}] Back | [ESC] Close" 
                : $"[{kbToggle}] Toggle | [ESC] Close";
        }
        
        GUIStyle hintStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(hintWidth, 0.65f))
        {
            normal = new GUIStyleState() 
            { 
                textColor = new Color(0.95f, 0.92f, 0.88f, 0.8f),
                background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.05f, 0.08f, 0.85f), true)
            },
            padding = new RectOffset(8, 8, 5, 5),
            fontSize = 11
        };
        
        GUI.Label(new Rect(hintX, hintY, hintWidth, hintHeight), hintText, hintStyle);
    }
    
    // Developer Console - Password protected experimental features
    private static void DrawDevConsole() {
        // Console window dimensions
        int consoleWidth = 600;
        int consoleHeight = 400;
        int consoleX = (Screen.width - consoleWidth) / 2;
        int consoleY = (Screen.height - consoleHeight) / 2;
        
        // Dark background
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(consoleX, consoleY, consoleWidth, consoleHeight), Texture2D.whiteTexture);
        
        // Border
        GUI.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        GUI.DrawTexture(new Rect(consoleX, consoleY, consoleWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(consoleX, consoleY + consoleHeight - 2, consoleWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(consoleX, consoleY, 2, consoleHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(consoleX + consoleWidth - 2, consoleY, 2, consoleHeight), Texture2D.whiteTexture);
        
        // Title
        GUI.color = Color.white;
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(consoleX, consoleY + 10, consoleWidth, 30), "DEVELOPER CONSOLE", titleStyle);
        
        // Instructions
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 11;
        infoStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(consoleX + 10, consoleY + 40, consoleWidth - 20, 20), "Type 'help' for available commands. Use 'unlock [feature]' to unlock features.", infoStyle);
        
        // Show unlock status
        string statusText = "Locked: SaveEditor, SaveSelector";
        if(_saveEditorUnlocked && _saveSelectorUnlocked) {
            statusText = "Unlocked: All features";
        } else if(_saveEditorUnlocked) {
            statusText = "Unlocked: SaveEditor" + (_saveSelectorUnlocked ? ", SaveSelector" : "");
        } else if(_saveSelectorUnlocked) {
            statusText = "Unlocked: SaveSelector";
        }
        GUI.Label(new Rect(consoleX + 10, consoleY + 55, consoleWidth - 20, 20), statusText, infoStyle);
        
        // Command history
        GUIStyle historyStyle = new GUIStyle(GUI.skin.label);
        historyStyle.fontSize = 12;
        historyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        float historyY = consoleY + 80;
        int maxHistoryLines = 10;
        int startIdx = Math.Max(0, _devConsoleHistory.Count - maxHistoryLines);
        for(int i = startIdx; i < _devConsoleHistory.Count && i < startIdx + maxHistoryLines; i++) {
            GUI.Label(new Rect(consoleX + 10, historyY, consoleWidth - 20, 20), "> " + _devConsoleHistory[i], historyStyle);
            historyY += 18;
        }
        
        // Input field
        GUI.color = Color.white;
        _devConsoleInput = GUI.TextField(new Rect(consoleX + 10, consoleY + consoleHeight - 35, consoleWidth - 80, 25), _devConsoleInput);
        
        // Execute button
        if(GUI.Button(new Rect(consoleX + consoleWidth - 65, consoleY + consoleHeight - 35, 55, 25), "Execute")) {
            ExecuteConsoleCommand(_devConsoleInput);
            _devConsoleInput = "";
        }
    }
    
    private static void ExecuteConsoleCommand(string command) {
        if(string.IsNullOrWhiteSpace(command)) return;
        
        // Add to history
        _devConsoleHistory.Add(command);
        if(_devConsoleHistory.Count > 50) _devConsoleHistory.RemoveAt(0);
        
        string cmd = command.Trim().ToLower();
        string[] parts = cmd.Split(' ');
        
        switch(parts[0]) {
            case "help":
                _devConsoleHistory.Add("Available commands:");
                _devConsoleHistory.Add("  help - Show this help message");
                _devConsoleHistory.Add("  unlock saveeditor [password] - Unlock Save Editor & Selector (requires password)");
                _devConsoleHistory.Add("  lock - Lock all features");
                _devConsoleHistory.Add("  clear - Clear console history");
                _devConsoleHistory.Add("  status - Show unlock status");
                break;
                
            case "unlock":
                if(parts.Length < 2) {
                    _devConsoleHistory.Add("Usage: unlock saveeditor [password]");
                    break;
                }
                string feature = parts[1].ToLower(); // Make case insensitive
                string password = parts.Length > 2 ? parts[2] : "";
                
                if(!VerifyPassword(password)) {
                    _devConsoleHistory.Add("ERROR: Incorrect password!");
                    break;
                }
                
                if(feature == "saveeditor" || feature == "all") {
                    // unlock saveeditor also unlocks saveselector (same permission level)
                    _saveEditorUnlocked = true;
                    _saveSelectorUnlocked = true;
                    _devConsoleHistory.Add("SUCCESS: Save Editor & Selector unlocked!");
                } else {
                    _devConsoleHistory.Add("Unknown feature: " + feature);
                    _devConsoleHistory.Add("Usage: unlock saveeditor [password]");
                }
                break;
                
            case "lock":
                _saveEditorUnlocked = false;
                _saveSelectorUnlocked = false;
                _devConsoleHistory.Add("All features locked.");
                break;
                
            case "clear":
                _devConsoleHistory.Clear();
                break;
                
            case "status":
                _devConsoleHistory.Add("SaveEditor: " + (_saveEditorUnlocked ? "UNLOCKED" : "LOCKED"));
                _devConsoleHistory.Add("SaveSelector: " + (_saveSelectorUnlocked ? "UNLOCKED" : "LOCKED"));
                break;
                
            default:
                _devConsoleHistory.Add("Unknown command: " + parts[0]);
                _devConsoleHistory.Add("Type 'help' for available commands.");
                break;
        }
    }

    private static void DrawPersistentHint()
    {
        if (CheatConfig.Instance == null) return;
        
        float margin = 10f;
        int hintHeight = 22;
        int hintX = (int)margin;
        int hintY = Screen.height - hintHeight - (int)margin;

        string hintText;
        if(CheatConfig.Instance.ControllerSupport.Value) {
            hintText = $"[LeftStickClick/{CheatConfig.Instance.GuiKeybind.Value.MainKey}] Open Cheat Menu";
        } else {
            hintText = $"[{CheatConfig.Instance.GuiKeybind.Value.MainKey}] Open Cheat Menu";
        }

        int hintWidth = 180;
        GUIStyle hintStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(hintWidth, 0.6f))
        {
            normal = new GUIStyleState()
            {
                textColor = new Color(0.95f, 0.92f, 0.88f, 0.5f),
                background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.05f, 0.08f, 0.55f), true)
            },
            padding = new RectOffset(8, 8, 4, 4),
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(hintX, hintY, hintWidth, hintHeight), hintText, hintStyle);
    }

    private static void CheatWindow()
    {
        // Inject a root-level quick action so users can access it from the main page without entering a category
        // Don't show at main menu - only show in-game
        if(CurrentCategory == CheatCategoryEnum.NONE && CultUtils.IsInGame()){
            int buttonHeight = GUIUtils.GetButtonHeight();
            int spacing = GUIUtils.GetButtonSpacing();
            int thisButtonIndex = s_currentButtonCounter++;

            GUIStyle style = GetButtonStyleWithHover(GUIUtils.GetGUIButtonStyle(), thisButtonIndex);
            // Support controller select: treat controller press as a button click
            bool controllerPressed = IsButtonSelected(thisButtonIndex) && s_controllerSelectPressed;
            // Draw unlock everything button at the top of the menu
            if(GUI.Button(new Rect(5, CurrentButtonY, MENU_WIDTH - 10, buttonHeight), "Unlock EVERYTHING ", style) || controllerPressed){
                try {
                    // Provide immediate feedback to the user
                    CultUtils.PlayNotification("Running unlock sequence...");
                    // Call the global unlock helper on the Combat/QoL definitions
                    QolDefinitions.UnlockAbsolutelyEverything();
                    // Final notification is also emitted by the unlock method, but ensure at least one
                    CultUtils.PlayNotification("Unlock command executed");
                } catch(Exception e) {
                    UnityEngine.Debug.LogWarning($"Failed to call UnlockAbsolutelyEverything: {e.Message}");
                    CultUtils.PlayNotification("Failed to run unlock command");
                }
            }
            CurrentButtonY += buttonHeight + spacing;
            TotalWindowCalculatedHeight += buttonHeight + spacing;
        }

        // Combat level sliders — shown inside Weapon or Curse subGroups
        if(CurrentCategory == CheatCategoryEnum.COMBAT && CurrentSubGroup != null){
            int sliderH = 28;
            int spacing = GUIUtils.GetButtonSpacing();
            int sliderWidth = 140;

            // Controller: left/right on right stick adjusts level with acceleration
            if(CheatConfig.Instance.ControllerSupport.Value){
                int navH = RewiredInputHelper.GetNavigationHorizontal();
                if(navH != 0){
                    s_sliderHoldTime += Time.unscaledDeltaTime;
                    float accel = s_sliderHoldTime < 0.4f ? 1f : s_sliderHoldTime < 1.0f ? 5f : 20f;
                    s_sliderAccum += accel * Time.unscaledDeltaTime * 15f;
                    int steps = Mathf.FloorToInt(s_sliderAccum);
                    if(steps > 0){
                        s_sliderAccum -= steps;
                        if(CurrentSubGroup == "Weapon"){
                            CombatEquipmentDefinitions.WeaponLevelSliderValue = Mathf.Clamp(CombatEquipmentDefinitions.WeaponLevelSliderValue + steps * navH, 1, 99);
                        } else if(CurrentSubGroup == "Curse"){
                            CombatEquipmentDefinitions.CurseLevelSliderValue = Mathf.Clamp(CombatEquipmentDefinitions.CurseLevelSliderValue + steps * navH, 1, 99);
                        }
                    }
                } else {
                    s_sliderHoldTime = 0f;
                    s_sliderAccum = 0f;
                }
            }

            if(CurrentSubGroup == "Weapon"){
                // Weapon Level Slider
                GUIStyle wStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
                wStyle.alignment = TextAnchor.MiddleLeft;
                wStyle.fontSize = 12;
                GUI.Label(new Rect(5, CurrentButtonY, 60, sliderH), "Wpn Lv:", wStyle);

                float wRawVal = GUI.HorizontalSlider(
                    new Rect(65, CurrentButtonY + 9, sliderWidth, 14),
                    (float)CombatEquipmentDefinitions.WeaponLevelSliderValue,
                    1f,
                    99f
                );
                CombatEquipmentDefinitions.WeaponLevelSliderValue = Mathf.RoundToInt(Mathf.Clamp(wRawVal, 1f, 99f));

                GUIStyle wNumStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
                wNumStyle.alignment = TextAnchor.MiddleLeft;
                wNumStyle.fontSize = 12;
                GUI.Label(new Rect(65 + sliderWidth + 2, CurrentButtonY, 30, sliderH), CombatEquipmentDefinitions.WeaponLevelSliderValue.ToString(), wNumStyle);

                TotalWindowCalculatedHeight += sliderH + spacing;
                CurrentButtonY += sliderH + spacing;
            }
            else if(CurrentSubGroup == "Curse"){
                // Curse Level Slider
                GUIStyle cStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
                cStyle.alignment = TextAnchor.MiddleLeft;
                cStyle.fontSize = 12;
                GUI.Label(new Rect(5, CurrentButtonY, 60, sliderH), "Curse Lv:", cStyle);

                float cRawVal = GUI.HorizontalSlider(
                    new Rect(65, CurrentButtonY + 9, sliderWidth, 14),
                    (float)CombatEquipmentDefinitions.CurseLevelSliderValue,
                    1f,
                    99f
                );
                CombatEquipmentDefinitions.CurseLevelSliderValue = Mathf.RoundToInt(Mathf.Clamp(cRawVal, 1f, 99f));

                GUIStyle cNumStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
                cNumStyle.alignment = TextAnchor.MiddleLeft;
                cNumStyle.fontSize = 12;
                GUI.Label(new Rect(65 + sliderWidth + 2, CurrentButtonY, 30, sliderH), CombatEquipmentDefinitions.CurseLevelSliderValue.ToString(), cNumStyle);

                TotalWindowCalculatedHeight += sliderH + spacing;
                CurrentButtonY += sliderH + spacing;
            }
        }

        // Item quantity slider — shown at the top of the Resources category
        if(CurrentCategory == CheatCategoryEnum.RESOURCE){
            int sliderH = 28;
            int spacing = GUIUtils.GetButtonSpacing();
            int sliderWidth = 140;

            // Controller: left/right on right stick adjusts qty with acceleration
            if(CheatConfig.Instance.ControllerSupport.Value){
                int navH = RewiredInputHelper.GetNavigationHorizontal();
                if(navH != 0){
                    s_sliderHoldTime += Time.unscaledDeltaTime;
                    float accel = s_sliderHoldTime < 0.4f ? 1f : s_sliderHoldTime < 1.0f ? 5f : 20f;
                    s_sliderAccum += accel * Time.unscaledDeltaTime * 15f;
                    int steps = Mathf.FloorToInt(s_sliderAccum);
                    if(steps > 0){
                        s_sliderAccum -= steps;
                        ResourceDefinitions.ItemSpawnQty = Mathf.Clamp(ResourceDefinitions.ItemSpawnQty + steps * navH, 1, 999);
                    }
                } else {
                    s_sliderHoldTime = 0f;
                    s_sliderAccum = 0f;
                }
            }

            GUIStyle lStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
            lStyle.alignment = TextAnchor.MiddleLeft;
            lStyle.fontSize = 12;
            GUI.Label(new Rect(5, CurrentButtonY, 24, sliderH), "Qty:", lStyle);

            float rawVal = GUI.HorizontalSlider(
                new Rect(28, CurrentButtonY + 9, sliderWidth, 14),
                (float)ResourceDefinitions.ItemSpawnQty,
                1f,
                999f
            );
            ResourceDefinitions.ItemSpawnQty = Mathf.RoundToInt(Mathf.Clamp(rawVal, 1f, 999f));

            GUIStyle rStyle = new GUIStyle(GUIUtils.GetGUILabelStyle(MENU_WIDTH, 0.85f));
            rStyle.alignment = TextAnchor.MiddleLeft;
            rStyle.fontSize = 12;
            GUI.Label(new Rect(28 + sliderWidth + 2, CurrentButtonY, 35, sliderH), ResourceDefinitions.ItemSpawnQty.ToString(), rStyle);

            TotalWindowCalculatedHeight += sliderH + spacing;
            CurrentButtonY += sliderH + spacing;
        }

        s_guiContent();
        s_scrollParams.ScrollHeight = TotalWindowCalculatedHeight;
  // Auto-scroll to keep the selected controller button visible
        if(s_needsScrollUpdate && CheatConfig.Instance.ControllerSupport.Value && s_totalButtons > 0){
            int buttonHeight = GUIUtils.GetButtonHeight();
            int spacing = GUIUtils.GetButtonSpacing();
            float selectedY = s_selectedButtonIndex * (buttonHeight + spacing);
            float viewHeight = MENU_HEIGHT - 33; // account for title bar
            float currentScroll = s_scrollParams.ScrollPosition.y;

            if(selectedY < currentScroll){
                s_scrollParams.ScrollPosition.y = selectedY;
            } else if(selectedY + buttonHeight > currentScroll + viewHeight){
                s_scrollParams.ScrollPosition.y = selectedY + buttonHeight - viewHeight;
            }
            s_needsScrollUpdate = false;
        }
        ResetLayoutValues();
    }

    private static void StartOpenAnimation(){
        s_animatingIn = true;
        s_animatingOut = false;
        s_pendingClose = false;
        s_animationProgress = 0f;
        GuiEnabled = true;
        // Reset window ID so Unity IMGUI does not cache the old position
        s_scrollParams.WindowID = null;
    }

    private static void StartCloseAnimation(){
        s_animatingOut = true;
        s_animatingIn = false;
        s_pendingClose = true;
    }

    public static void GiveAllItems(){
        try {
            int addedCount = 0;
            int skippedDlc = 0;
            bool hasMajorDLC = CultUtils.HasMajorDLC();
            foreach(var itemType in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE))){
                InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)itemType;
                if(type == InventoryItem.ITEM_TYPE.NONE) continue;
                // Exclude SOULS and FLEECES
                string itemName = type.ToString().ToUpperInvariant();
                if(itemName.Contains("SOUL") || itemName.Contains("FLEECE")) continue;
                
                // FILTER: Never spawn these items - they are special/meta items that shouldn't be given
                // RATAU_STAFF - quest item
                // BOP - DLC quest item  
                // FOUND_ITEM_DECORATION_ALT, FOUND_ITEM_DECORATION - special decoration items
                // UNUSED - unused items in the enum
                // DISCIPLE_POINTS - special currency (not actual inventory item)
                // TRINKET_CARD_UNLOCKED - special unlock item
                // PERMANENT_HALF_HEART, BLACK_HEART - special health items
                // FOUND_ITEM_WEAPON, FOUND_ITEM_CURSE - special discovery items
                // RED_HEART, HALF_HEART, BLUE_HEART, HALF_BLUE_HEART - special heart items
                // TIME_TOKEN - special time item
                // GENERIC - generic placeholder item
                if(itemName.Contains("RATAU_STAFF") || 
                   itemName.Contains("BOP") ||
                   itemName.Contains("FOUND_ITEM_DECORATION") ||
                   itemName.Contains("UNUSED") ||
                   itemName.Contains("DISCIPLE_POINTS") ||
                   itemName.Contains("TRINKET_CARD_UNLOCKED") ||
                   itemName.Contains("PERMANENT_HALF_HEART") ||
                   itemName.Contains("BLACK_HEART") ||
                   itemName.Contains("FOUND_ITEM_WEAPON") ||
                   itemName.Contains("FOUND_ITEM_CURSE") ||
                   itemName.Contains("RED_HEART") ||
                   itemName.Contains("HALF_HEART") ||
                   itemName.Contains("BLUE_HEART") ||
                   itemName.Contains("TIME_TOKEN") ||
                   itemName.Contains("GENERIC")){
                    skippedDlc++;
                    continue;
                }
                // YNGYA_GHOST is a quest item that can cause softlocks - never give it
                if(itemName.Contains("YNGYA_GHOST")) continue;
                // Purple and White flowers (Forget-me-not, Snowdrop - Woolhaven DLC) - always skip
                if(itemName.Contains("FLOWER_PURPLE") || itemName.Contains("FLOWER_WHITE") || itemName.Contains("SEED_FLOWER_PURPLE") || itemName.Contains("SEED_FLOWER_WHITE")){
                    skippedDlc++;
                    continue;
                }
                // COD, PIKE, CATFISH - always skip (Woolhaven DLC exclusive fish)
                if(itemName.Contains("COD") || itemName.Contains("PIKE") || itemName.Contains("CATFISH")){
                    skippedDlc++;
                    continue;
                }
                // Woolhaven DLC necklaces - always skip if no DLC
                if(itemName.Contains("NECKLACE_DEATHS_DOOR") || itemName.Contains("NECKLACE_WINTER") || itemName.Contains("NECKLACE_FROZEN") || itemName.Contains("NECKLACE_WEIRD") || itemName.Contains("NECKLACE_TARGETED") || itemName.Contains("DLC_NECKLACE")){
                    if(!hasMajorDLC){
                        skippedDlc++;
                        continue;
                    }
                }
                if(!hasMajorDLC && (itemName.Contains("DLC") || itemName.Contains("FORGE_FLAME") || itemName.Contains("MAGMA") || itemName.Contains("ELECTRIFIED") || itemName.Contains("LIGHTNING_SHARD") || itemName.Contains("CHARCOAL") || itemName.Contains("SOOT") || itemName.Contains("BROKEN_WEAPON") || itemName.Contains("LEGENDARY_WEAPON") || itemName.Contains("FLOCKADE") || itemName.Contains("RATAU_STAFF") || itemName.Contains("WEBBER_SKULL") || itemName.Contains("ILLEGIBLE_LETTER") || itemName.Contains("LORE_STONE") || itemName.Contains("SPECIAL_WOOL") || itemName.Contains("ANIMAL_") || itemName.Contains("YOLK") || itemName.Contains("EGG_FOLLOWER") || itemName.Contains("ROTBURN") || itemName.Contains("BEHOLDER_EYE_ROT") || itemName.Contains("CALCIFIED") || itemName.Contains("WOOL") || itemName.Contains("MILK") || itemName.Contains("MEAL_MILK") || itemName.Contains("SNOW_FRUIT") || itemName.Contains("CHILLI") || itemName.Contains("SEED_SNOW_FRUIT") || itemName.Contains("SEED_CHILLI") || itemName.Contains("POOP_ROTSTONE") || itemName.Contains("COD") || itemName.Contains("PIKE") || itemName.Contains("CATFISH") || itemName.Contains("SNOW_CHUNK") || itemName.Contains("FISHING_ROD") || itemName.Contains("REPAIRED_WEAPON") || itemName.Contains("BOP") || itemName.Contains("YEW_CURSED") || itemName.Contains("YEW_HOLY") || itemName.Contains("DRINK_MUSHROOM_JUICE") || itemName.Contains("DRINK_CHILLI") || itemName.Contains("DRINK_LIGHTNING") || itemName.Contains("DRINK_SIN") || itemName.Contains("DRINK_GRASS") || itemName.Contains("DRINK_MILKSHAKE") || itemName.Contains("SOUL_FRAGMENT"))){
                    skippedDlc++;
                    continue;
                }
                try {
                    CultUtils.AddInventoryItem(type, ResourceDefinitions.ItemSpawnQty);
                    addedCount++;
                } catch { }
            }
            string msg = $"{ResourceDefinitions.ItemSpawnQty}x of all items added ({addedCount} types)!";
            if(skippedDlc > 0) msg += $" ({skippedDlc} DLC items skipped)";
            CultUtils.PlayNotification(msg);
        } catch(Exception e){
            Debug.LogWarning($"Failed to add all items: {e.Message}");
            CultUtils.PlayNotification("Failed to add some items!");
        }
    }

    [Update]
    public static void Update()
    {                
        // Handle animation (use unscaledDeltaTime so it works while paused)
        if(s_animatingIn){
            s_animationProgress += Time.unscaledDeltaTime * s_animationSpeed;
            if(s_animationProgress >= 1f){
                s_animationProgress = 1f;
                s_animatingIn = false;
            }
        }
        if(s_animatingOut){
            s_animationProgress -= Time.unscaledDeltaTime * s_animationSpeed;
            if(s_animationProgress <= 0f){
                s_animationProgress = 0f;
                s_animatingOut = false;
                if(s_pendingClose){
                    GuiEnabled = false;
                    s_pendingClose = false;
                }
            }
        }

        bool localGuiEnabled = GuiEnabled;
        bool keyDown = false;
        bool controllerComboDown = false;
        
        // Handle key input - with fallback if config not loaded
        if (CheatConfig.Instance != null) {
            keyDown = Input.GetKeyDown(CheatConfig.Instance.GuiKeybind.Value.MainKey);
            controllerComboDown = CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetToggleMenuPressed();
        } else {
            // Fallback: use M key if config not loaded yet
            keyDown = Input.GetKeyDown(KeyCode.M);
        }
        
        if(!InputBlockedForModal && (keyDown || controllerComboDown)){
            // Allow opening cheat menu on main menu to access Save Editor
            // At main menu, only Save subGroup will be shown
            if(IsAtMainMenu()){
                // At main menu, allow opening but only Save subGroup is visible
                if(!GuiEnabled && !s_animatingIn){
                    StartOpenAnimation();
                    s_selectedButtonIndex = 0;
                } else if(GuiEnabled && !s_animatingOut) {
                    StartCloseAnimation();
                }
            } else if(!GuiEnabled && !s_animatingIn){
                StartOpenAnimation();
                s_selectedButtonIndex = 0;
            } else if(GuiEnabled && !s_animatingOut) {
                StartCloseAnimation();
            }
        }
        
        // Developer Console - Open with ~ key (BackQuote is the ` or ~ key)
        if(Input.GetKeyDown(KeyCode.BackQuote)) {
            _devConsoleOpen = !_devConsoleOpen;
        }
        
        // Controller navigation when menu is open
        if(GuiEnabled && CheatConfig.Instance != null && CheatConfig.Instance.ControllerSupport.Value && !s_animatingIn && !s_animatingOut && !InputBlockedForModal) {
            float currentTime = Time.unscaledTime;
            
            if(currentTime - s_lastNavigationTime > s_navigationDelay) {
                int navVertical = RewiredInputHelper.GetNavigationVertical();
                
                if(navVertical > 0){
                    if(s_selectedButtonIndex > 0){
                        s_selectedButtonIndex--;
                    } else if(s_totalButtons > 0){
                        s_selectedButtonIndex = s_totalButtons - 1;
                    }
                    s_lastNavigationTime = currentTime;
                    s_needsScrollUpdate = true;
                } else if(navVertical < 0){
                    if(s_totalButtons > 0 && s_selectedButtonIndex < s_totalButtons - 1){
                        s_selectedButtonIndex++;
                    } else {
                        s_selectedButtonIndex = 0;
                    }
                    s_lastNavigationTime = currentTime;
                    s_needsScrollUpdate = true;
                }
            }
            
            // Controller select button - suppress if R3 is held (toggle press window)
            if(RewiredInputHelper.GetSelectPressed() && !RewiredInputHelper.IsR3Held()){
                s_controllerSelectPressed = true;
            }
        }
        
        if(GuiEnabled && !s_animatingOut && !InputBlockedForModal && Input.GetKeyDown(CheatConfig.Instance.BackCategory.Value.MainKey) && CurrentCategory != CheatCategoryEnum.NONE)
        {
            CurrentCategory = CheatCategoryEnum.NONE;
            GUIManager.ClearAllGuiBasedCheats();
            s_selectedButtonIndex = 0;
        }
        
        // Controller back button
        if(GuiEnabled && !s_animatingOut && CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetBackPressed() && !InputBlockedForModal)
        {
            if(CurrentCategory != CheatCategoryEnum.NONE){
                CurrentCategory = CheatCategoryEnum.NONE;
                GUIManager.ClearAllGuiBasedCheats();
                s_selectedButtonIndex = 0;
            } else {
                StartCloseAnimation();
            }
        }

        if(GuiEnabled && !s_animatingOut && Input.GetKeyDown(KeyCode.Escape) && CheatConfig.Instance.CloseGuiOnEscape.Value && !InputBlockedForModal)
        {
            StartCloseAnimation();
        }

        if(localGuiEnabled == true && GuiEnabled == false){
            GUIManager.ClearAllGuiBasedCheats();
            s_selectedButtonIndex = 0;
        }
    }
}
