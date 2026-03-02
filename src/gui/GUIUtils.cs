using System;
using UnityEngine;
using System.Collections.Generic;

namespace CheatMenu;

public static class GUIUtils {    
    private static Font s_uiFont;
    
    private static GUIStyle s_buttonStyle = null;
    private static GUIStyle s_selectedButtonStyle = null;
    private static GUIStyle s_titleBarStyle = null;
    private static GUIStyle s_categoryButtonStyle = null;
    private static GUIStyle s_subGroupButtonStyle = null;

    // Cult of the Lamb Theme Colors
    private static readonly Color CULT_DARK_RED = new Color(0.18f, 0.09f, 0.11f, 0.98f);      // Dark burgundy background
    private static readonly Color CULT_RED = new Color(0.65f, 0.13f, 0.18f, 1f);              // Crimson red for accents
    private static readonly Color CULT_BLOOD_RED = new Color(0.75f, 0.15f, 0.15f, 1f);        // Blood red for hover
    private static readonly Color CULT_BONE_WHITE = new Color(0.95f, 0.92f, 0.88f, 1f);       // Bone white text
    private static readonly Color CULT_DARK_PURPLE = new Color(0.15f, 0.08f, 0.18f, 1f);      // Dark purple for selected
    private static readonly Color CULT_GOLD = new Color(0.85f, 0.75f, 0.45f, 1f);             // Golden accents
    private static readonly Color CULT_BLACK = new Color(0.08f, 0.08f, 0.1f, 0.95f);          // Nearly black for panels
    private static readonly Color CULT_SHADOW = new Color(0.12f, 0.05f, 0.08f, 1f);           // Shadow red for depth

    [Init]
    public static void Init(){
        string[] fonts = Font.GetOSInstalledFontNames();
        List<string> fontsList = new(fonts);

        s_uiFont = fontsList.Contains("Arial") ? Font.CreateDynamicFontFromOSFont("Arial", 16) : Font.CreateDynamicFontFromOSFont(fonts[0], 16);
    }

    [Unload]
    public static void Unload()
    {
        UnityEngine.Object.Destroy(s_uiFont);
        s_buttonStyle = null;
        s_selectedButtonStyle = null;
        s_titleBarStyle = null;
        s_categoryButtonStyle = null;
        s_subGroupButtonStyle = null;
    }

    public static GUIStyle GetGUIWindowStyle()
    {
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_DARK_RED, true),
            textColor = CULT_BONE_WHITE,
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            active = normalStyle,
            alignment = TextAnchor.MiddleCenter,
            font = s_uiFont,
            wordWrap = true,
            padding = new RectOffset(8, 8, 8, 8),
            border = new RectOffset(3, 3, 3, 3)
        };

        return styleObj;
    }


    public static GUIStyle GetGUILabelStyle(int width, float sizeModifier = 1.0f)
    {
        GUIStyleState normalStyle = new()
        {
            textColor = CULT_BONE_WHITE
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            active = normalStyle,
            alignment = TextAnchor.MiddleCenter,
            font = s_uiFont,
            fontSize = (int)(width / 20 * sizeModifier),
            wordWrap = true
        };

        return styleObj;
    }

    public static GUIStyle GetGUIPanelStyle(int width)
    {
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_BLACK, true),
            textColor = CULT_BONE_WHITE
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            alignment = TextAnchor.MiddleCenter,
            font = s_uiFont,
            fontSize = Mathf.Max(14, width / 20),
            wordWrap = true,
            padding = new RectOffset(12, 12, 12, 12),
            border = new RectOffset(2, 2, 2, 2)
        };

        return styleObj;
    }

    public static GUIStyle GetGUIButtonSelectedStyle()
    {
        if(s_selectedButtonStyle != null)
        {
            return s_selectedButtonStyle;
        }

        // Active/Selected - Dark purple with gold text
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_DARK_PURPLE, true),
            textColor = CULT_GOLD
        };

        // Hover - Lighter purple
        GUIStyleState hoverStyle = new()
        {
            textColor = CULT_GOLD,
            background = TextureHelper.GetSolidTexture(new Color(0.2f, 0.12f, 0.25f, 1f), true)
        };

        // Clicked - Darker purple
        GUIStyleState activeStyle = new()
        {
            textColor = CULT_BONE_WHITE,
            background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.06f, 0.15f, 1f), true)
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            active = activeStyle,
            onActive = activeStyle,
            hover = hoverStyle,
            onHover = hoverStyle,
            font = s_uiFont,
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(6, 6, 5, 5),
            fontStyle = FontStyle.Bold,
            border = new RectOffset(2, 2, 2, 2)
        };

        s_selectedButtonStyle = styleObj;
        return styleObj;
    }

    public static GUIStyle GetTitleBarStyle()
    {
        if(s_titleBarStyle != null)
        {
            return s_titleBarStyle;
        }

        // Title bar - Deep crimson with bone white text and skull icon
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_RED, true),
            textColor = CULT_BONE_WHITE
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            font = s_uiFont,
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(6, 6, 4, 4),
            border = new RectOffset(2, 2, 2, 2)
        };

        s_titleBarStyle = styleObj;
        return styleObj;
    }

    public static GUIStyle GetCategoryButtonStyle()
    {
        if(s_categoryButtonStyle != null)
        {
            return s_categoryButtonStyle;
        }

        // Category buttons - Shadow red with crimson border effect
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_SHADOW, true),
            textColor = CULT_BONE_WHITE
        };

        // Hover - Blood red
        GUIStyleState hoverStyle = new()
        {
            textColor = CULT_BONE_WHITE,
            background = TextureHelper.GetSolidTexture(CULT_BLOOD_RED, true)
        };

        // Clicked - Darker shadow
        GUIStyleState activeStyle = new()
        {
            textColor = CULT_GOLD,
            background = TextureHelper.GetSolidTexture(new Color(0.08f, 0.03f, 0.05f, 1f), true)
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            active = activeStyle,
            onActive = activeStyle,
            hover = hoverStyle,
            onHover = hoverStyle,
            font = s_uiFont,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(8, 8, 6, 6),
            margin = new RectOffset(2, 2, 2, 2),
            fontStyle = FontStyle.Bold,
            border = new RectOffset(2, 2, 2, 2)
        };

        s_categoryButtonStyle = styleObj;
        return styleObj;
    }

    /// <summary>
    /// Sub-group drill-down buttons: a slightly lighter shade than category buttons
    /// so users can visually distinguish top-level categories from sub-group drills.
    /// </summary>
    public static GUIStyle GetSubGroupButtonStyle()
    {
        if(s_subGroupButtonStyle != null) return s_subGroupButtonStyle;

        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(new Color(0.22f, 0.10f, 0.13f, 0.98f), true),
            textColor = CULT_BONE_WHITE
        };
        GUIStyleState hoverStyle = new()
        {
            textColor = CULT_BONE_WHITE,
            background = TextureHelper.GetSolidTexture(CULT_BLOOD_RED, true)
        };
        GUIStyleState activeStyle = new()
        {
            textColor = CULT_GOLD,
            background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.06f, 0.08f, 1f), true)
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            active = activeStyle,
            onActive = activeStyle,
            hover = hoverStyle,
            onHover = hoverStyle,
            font = s_uiFont,
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(12, 8, 5, 5),
            margin = new RectOffset(2, 2, 1, 1),
            fontStyle = FontStyle.Normal,
            border = new RectOffset(2, 2, 2, 2)
        };

        s_subGroupButtonStyle = styleObj;
        return styleObj;
    }

    public static GUIStyle GetGUIButtonStyle()
    {
        if(s_buttonStyle != null)
        {
            return s_buttonStyle;
        }

        // Regular buttons - Dark with crimson border
        GUIStyleState normalStyle = new()
        {
            background = TextureHelper.GetSolidTexture(CULT_BLACK, true),
            textColor = CULT_BONE_WHITE
        };

        // Hover - Crimson highlight
        GUIStyleState hoverStyle = new()
        {
            textColor = CULT_BONE_WHITE,
            background = TextureHelper.GetSolidTexture(CULT_RED, true)
        };

        // Clicked - Blood red
        GUIStyleState activeStyle = new()
        {
            textColor = CULT_BONE_WHITE,
            background = TextureHelper.GetSolidTexture(CULT_BLOOD_RED, true)
        };

        GUIStyle styleObj = new()
        {
            normal = normalStyle,
            onNormal = normalStyle,
            active = activeStyle,
            onActive = activeStyle,
            hover = hoverStyle,
            onHover = hoverStyle,
            font = s_uiFont,
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(6, 6, 5, 5),
            margin = new RectOffset(2, 2, 2, 2),
            border = new RectOffset(2, 2, 2, 2)
        };

        s_buttonStyle = styleObj;
        return styleObj;
    }

    public static Rect GetCenterRect(int width, int height){
        var centerX = (Screen.width - width) / 2;
        var centerY = (Screen.height - height) / 2;
        return new Rect(centerX, centerY, width, height);
    }

    public static void TitleBar(string titleText, float width){
        string decoratedTitle = $"+ {titleText} +";
        GUI.Box(new Rect(0, 0, width, 26), decoratedTitle, GetTitleBarStyle());
    }

    public struct WindowParams{
        public Rect ClientRect;
        public string Title;
        public int? WindowID = null;

        public WindowParams(string title, Rect clientRect){
            ClientRect = clientRect;
            Title = title;
        }
    }

    public static WindowParams CustomWindow(WindowParams windowParams, Action guiContents){
        GUI.DragWindow(new Rect(0, 0, windowParams.ClientRect.width, 30));
        WindowParams newWindowParams = new(windowParams.Title, windowParams.ClientRect);
        if(newWindowParams.WindowID == null){
            newWindowParams.WindowID = GUIManager.GetNextAvailableWindowID();
        }

        Rect newRect = GUI.Window((int)newWindowParams.WindowID, windowParams.ClientRect, delegate {
            TitleBar(newWindowParams.Title, newWindowParams.ClientRect.width);
            guiContents();
        }, "", GetGUIWindowStyle());
        return newWindowParams;
    }

    public class ScrollableWindowParams{
        public Vector2 ScrollPosition;
        public string Title;
        public Rect ClientRect;
        public float ScrollHeight;
        public int? WindowID = null;

        public ScrollableWindowParams(string title, Rect clientRect, float? scrollHeight = null, Vector2? scrollPosition = null){
            Title = title;
            ClientRect = clientRect;
            ScrollHeight = scrollHeight == null ? ClientRect.height * 2 : (float)scrollHeight;
            ScrollPosition = scrollPosition == null ? Vector2.zero : (Vector2)scrollPosition;
        }
    }

    public static ScrollableWindowParams CustomWindowScrollable(ScrollableWindowParams scrollParams, Action guiContents){        
        if(scrollParams.WindowID == null){
            scrollParams.WindowID = GUIManager.GetNextAvailableWindowID();
        }

        scrollParams.ClientRect = GUI.Window((int)scrollParams.WindowID, scrollParams.ClientRect, delegate {
            TitleBar(scrollParams.Title, scrollParams.ClientRect.width);
            Rect modifiedScrollPosition = new(
                5, 
                28, 
                scrollParams.ClientRect.width - 10,
                scrollParams.ClientRect.height - 33
            );
            scrollParams.ScrollPosition = GUI.BeginScrollView(
                modifiedScrollPosition, 
                scrollParams.ScrollPosition, 
                new Rect(0, 0, scrollParams.ClientRect.width - 35, scrollParams.ScrollHeight),
                false,
                true
            );
            guiContents();
            GUI.EndScrollView();
            GUI.DragWindow(new Rect(0, 0, 10000, 28));
        }, "", GetGUIWindowStyle());

        return scrollParams;
    }

    public static ScrollableWindowParams CustomWindowScrollableLocked(ScrollableWindowParams scrollParams, Action guiContents){        
        if(scrollParams.WindowID == null){
            scrollParams.WindowID = GUIManager.GetNextAvailableWindowID();
        }

        // Draw directly using GUI groups instead of GUI.Window to support animation
        // GUI.Window caches position by ID and ignores rect changes after the first frame
        Rect rect = scrollParams.ClientRect;
        GUI.Box(rect, "", GetGUIWindowStyle());
        GUI.BeginGroup(rect);
        TitleBar(scrollParams.Title, rect.width);
        Rect modifiedScrollPosition = new(
            5, 
            28, 
            rect.width - 10,
            rect.height - 33
        );
        scrollParams.ScrollPosition = GUI.BeginScrollView(
            modifiedScrollPosition, 
            scrollParams.ScrollPosition, 
            new Rect(0, 0, rect.width - 35, scrollParams.ScrollHeight),
            false,
            true
        );
        guiContents();
        GUI.EndScrollView();
        GUI.EndGroup();

        return scrollParams;
    }

    // 0 neither button clicked, 1 first button clicked, 2 second button clicked
    public static int ToggleButton(Rect sizeAndPlacement, string buttonOneText, string buttonTwoText, int state = 0){
        int halfWidth = (int)(sizeAndPlacement.width / 2) - 2;
        bool firstClicked = GUI.Button(
            new Rect(sizeAndPlacement.x, sizeAndPlacement.y, halfWidth, sizeAndPlacement.height),
            buttonOneText,
            state == 1 ? GetGUIButtonSelectedStyle() : GetGUIButtonStyle()
        );
        bool secondClicked = GUI.Button(
            new Rect(sizeAndPlacement.x + halfWidth + 4, sizeAndPlacement.y, halfWidth, sizeAndPlacement.height),
            buttonTwoText,
            state == 2 ? GetGUIButtonSelectedStyle() : GetGUIButtonStyle()
        );
        if(firstClicked || secondClicked){
            return firstClicked ? 1 : 2;
        }
        return state;
    }

    public static int GetButtonHeight(){
        return 30;
    }

    public static int GetButtonSpacing(){
        return 2;
    }

    public static bool Button(int y, int width, string buttonText){
        var btn = GUI.Button(new Rect(5, y, width, GetButtonHeight()), buttonText, GetGUIButtonStyle());
        return btn;
    }
}