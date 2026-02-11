using System;
using UnityEngine;
using UnityAnnotationHelpers;

namespace CheatMenu;

public static class CheatMenuGui {    
public static bool GuiEnabled = false;
public static CheatCategoryEnum CurrentCategory = CheatCategoryEnum.NONE;
public static int CurrentButtonY = 0;
public static int TotalWindowCalculatedHeight = 0;
    
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
        s_selectedButtonIndex = 0;
        s_totalButtons = 0;
        s_currentButtonCounter = 0;
        s_animationProgress = 0f;
        s_animatingIn = false;
        s_animatingOut = false;
        s_pendingClose = false;
        s_controllerSelectPressed = false;
        s_needsScrollUpdate = false;
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
        return categoryEnum.Equals(CurrentCategory);
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

    public static bool CategoryButton(string categoryText){
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
            CurrentCategory = CheatCategoryEnumExtensions.GetEnumFromName(categoryText);
            s_selectedButtonIndex = 0;
        }
        return btn;
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
            CurrentCategory = CheatCategoryEnum.NONE;
            s_selectedButtonIndex = 0;
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
            s_scrollParams.Title = "Cult Cheat Menu";
            if(IsWithinCategory()){
                s_scrollParams.Title = $"Cult Cheat Menu - {CurrentCategory.GetCategoryName()}";
            }
            
            s_scrollParams.ClientRect = GetMenuRect(s_animationProgress);
            s_scrollParams = GUIUtils.CustomWindowScrollableLocked(s_scrollParams, CheatWindow);
            
            DrawKeybindHints();
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
        if(CheatConfig.Instance.ControllerSupport.Value) {
            hintText = IsWithinCategory() 
                ? $"[B] Back | [Start] Close | [Stick/D-Pad] Nav | [A] Select" 
                : $"[Start] Close | [Stick/D-Pad] Nav | [A] Select";
        } else {
            hintText = IsWithinCategory() 
                ? $"[{CheatConfig.Instance.BackCategory.Value.MainKey}] Back | [ESC] Close" 
                : $"[{CheatConfig.Instance.GuiKeybind.Value.MainKey}] Toggle | [ESC] Close";
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

    private static void CheatWindow()
    {
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

    [Update]
    public static void Update()
    {                
        // Handle animation
        if(s_animatingIn){
            s_animationProgress += Time.deltaTime * s_animationSpeed;
            if(s_animationProgress >= 1f){
                s_animationProgress = 1f;
                s_animatingIn = false;
            }
        }
        if(s_animatingOut){
            s_animationProgress -= Time.deltaTime * s_animationSpeed;
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
        bool keyDown = Input.GetKeyDown(CheatConfig.Instance.GuiKeybind.Value.MainKey);
        bool controllerMenuDown = CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetMenuPressed();
        
        if(CultUtils.IsInGame() && (keyDown || controllerMenuDown)){
            if(!GuiEnabled && !s_animatingIn){
                StartOpenAnimation();
                s_selectedButtonIndex = 0;
            } else if(GuiEnabled && !s_animatingOut) {
                StartCloseAnimation();
            }
        } else if((keyDown || controllerMenuDown) && !CultUtils.IsInGame()) {
            NotificationHandler.CreateNotification("Cheat Menu can only be opened once in game!", 2);
        }
        
        // Controller navigation when menu is open
        if(GuiEnabled && CheatConfig.Instance.ControllerSupport.Value && !s_animatingIn && !s_animatingOut) {
            float currentTime = Time.time;
            
            if(currentTime - s_lastNavigationTime > s_navigationDelay) {
                int navVertical = RewiredInputHelper.GetNavigationVertical();
                
                if(navVertical > 0){
                    if(s_selectedButtonIndex > 0){
                        s_selectedButtonIndex--;
                        s_lastNavigationTime = currentTime;
                        s_needsScrollUpdate = true;
                    }
                } else if(navVertical < 0){
                    if(s_totalButtons > 0 && s_selectedButtonIndex < s_totalButtons - 1){
                        s_selectedButtonIndex++;
                        s_lastNavigationTime = currentTime;
                        s_needsScrollUpdate = true;
                    }
                }
            }
            
            // Controller select button
            if(RewiredInputHelper.GetSelectPressed()){
                s_controllerSelectPressed = true;
            }
        }
        
        if(GuiEnabled && !s_animatingOut && Input.GetKeyDown(CheatConfig.Instance.BackCategory.Value.MainKey) && CurrentCategory!= CheatCategoryEnum.NONE)
        {
            CurrentCategory = CheatCategoryEnum.NONE;
            GUIManager.ClearAllGuiBasedCheats();
            s_selectedButtonIndex = 0;
        }
        
        // Controller back button
        if(GuiEnabled && !s_animatingOut && CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetBackPressed())
        {
            if(CurrentCategory != CheatCategoryEnum.NONE){
                CurrentCategory = CheatCategoryEnum.NONE;
                GUIManager.ClearAllGuiBasedCheats();
                s_selectedButtonIndex = 0;
            } else {
                StartCloseAnimation();
            }
        }

        if(GuiEnabled && !s_animatingOut && Input.GetKeyDown(KeyCode.Escape) && CheatConfig.Instance.CloseGuiOnEscape.Value)
        {
            StartCloseAnimation();
        }

        if(localGuiEnabled == true && GuiEnabled == false){
            GUIManager.ClearAllGuiBasedCheats();
            s_selectedButtonIndex = 0;
        }
    }
}