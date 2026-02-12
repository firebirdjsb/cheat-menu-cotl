using UnityEngine;

namespace CheatMenu;

public class NotificationHandler {

    private static string s_message;    
    private static float s_timeToDisplay;    
    private static float s_timer;    

    [OnGui]
    public static void OnGUI(){
        var width = Mathf.Min(Screen.width / 3, 400);
        var height = Mathf.Min(Screen.height / 8, 120);
        Rect sizeAndLocation = new Rect((Screen.width - width) / 2, Screen.height - height - 80, width, height);

        if(s_message != null){
            // Add fade in/out effect
            float alpha = 1f;
            if(s_timer < 0.3f) {
                alpha = s_timer / 0.3f;
            } else if(s_timeToDisplay - s_timer < 0.5f) {
                alpha = (s_timeToDisplay - s_timer) / 0.5f;
            }
            
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Window(2, sizeAndLocation, NotificationWindow, "", GUIUtils.GetGUIWindowStyle());
            GUI.color = oldColor;
            
            s_timer += Time.deltaTime;
            if(s_timer >= s_timeToDisplay){
                s_message = null;
                s_timer = 0f;
                s_timeToDisplay = 0f;
            }            
        }
    }
    
    private static void NotificationWindow(int id){
        var width = Mathf.Min(Screen.width / 3, 400);
        var height = Mathf.Min(Screen.height / 8, 120);

        // Add colored background panel for better visibility
        GUI.Box(new Rect(0, 0, width, height), "", GUIUtils.GetGUIPanelStyle(width));
        GUI.Label(new Rect(10, 10, width - 20, height - 20), s_message, GUIUtils.GetGUILabelStyle(width, 0.9f));
    }

    public static void CreateNotification(string message, int displayTimeSeconds){
        s_message = message;
        s_timeToDisplay = displayTimeSeconds;
        s_timer = 0f;
    }
}