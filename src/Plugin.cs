using BepInEx;
using System;
using System.Reflection;

namespace CheatMenu;

[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.3.4")]
public class Plugin : BaseUnityPlugin
{    
    private UnityAnnotationHelper _annotationHelper;
    private Action _updateFn = null;
    private Action _onGUIFn = null;

    public void Awake()
    {        
        Logger.LogInfo("========================");
        Logger.LogInfo("Starting Cheat Menu v1.3.4...");
        Logger.LogInfo("Cult of the Lamb: Cheaters Edition!");
        Logger.LogInfo("=========================");

        new CheatConfig(Config);

        try {
            // RunAllInit() must run first — it calls ReflectionHelper.Init() which creates
            // the Harmony instance needed by all subsequent patches.
            _annotationHelper = new UnityAnnotationHelper();
            
            _annotationHelper.RunAllInit();
            Logger.LogInfo("Annotation system initialized successfully");

            // Patch VersionNumber.OnEnable so the main menu shows "Cheaters Edition"
            PatchVersionText();

            _onGUIFn = _annotationHelper.BuildRunAllOnGuiDelegate();
            _updateFn = _annotationHelper.BuildRunAllUpdateDelegate();
            Logger.LogInfo("Patching and loading completed successfully!");
            Logger.LogInfo("Cheat Menu is ready to use!");
        } catch(Exception e) {
            Logger.LogError($"FATAL ERROR during initialization: {e.Message}");
        }
    }

    public void OnDisable()
    {
        try {
            Logger.LogInfo("Shutting down Cheat Menu...");
            _annotationHelper?.RunAllUnload();
            Logger.LogInfo("Cheat Menu disabled successfully");
        } catch(Exception e) {
            Logger.LogError($"Error during shutdown: {e.Message}");
        }
    }

    public void OnGUI()
    {
        try {
            _onGUIFn?.Invoke();
        } catch(Exception e) {
            Logger.LogError($"OnGUI error: {e.Message}");
        }
    }

    public void Update()
    {
        try {
            _updateFn?.Invoke();
        } catch(Exception e) {
            Logger.LogError($"Update error: {e.Message}");
        }
    }

    private void PatchVersionText()
    {
        try {
            MethodInfo patchMethod = typeof(Plugin).GetMethod("Prefix_VersionNumber_OnEnable", BindingFlags.Static | BindingFlags.Public);
            Type versionNumberType = HarmonyLib.AccessTools.TypeByName("VersionNumber");
            
            if(versionNumberType == null) {
                Logger.LogWarning("VersionNumber type not found");
                return;
            }

            var result = ReflectionHelper.PatchMethodPrefix(
                versionNumberType,
                "OnEnable",
                patchMethod,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if(result != null) {
                Logger.LogInfo("✓ VersionNumber.OnEnable patched");
            } else {
                Logger.LogWarning("✗ VersionNumber.OnEnable patch failed (method not found)");
            }
        } catch(Exception e) {
            Logger.LogWarning($"VersionNumber patch failed: {e.Message}");
        }
    }

    public static bool Prefix_VersionNumber_OnEnable(VersionNumber __instance)
    {
        try {
            // Use reflection to get the Text field - avoids needing UnityEngine.UI reference
            var textField = typeof(VersionNumber).GetField("Text", BindingFlags.Instance | BindingFlags.Public);
            if(textField != null) {
                var textComponent = textField.GetValue(__instance);
                if(textComponent != null) {
                    var textProperty = textComponent.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                    if(textProperty != null) {
                        textProperty.SetValue(textComponent, $"{UnityEngine.Application.version} - Cheaters Edition");
                    }
                }
            }
            return false;
        } catch(Exception e){
            // Can't use Logger in static method
            UnityEngine.Debug.LogWarning($"Version text patch error: {e.Message}");
        }
        return true;
    }
}
