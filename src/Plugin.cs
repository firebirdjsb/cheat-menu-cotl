using BepInEx;
using System;
using System.Reflection;

namespace CheatMenu;

[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.3.2")]
public class Plugin : BaseUnityPlugin
{    
    private UnityAnnotationHelper _annotationHelper;
    private Action _updateFn = null;
    private Action _onGUIFn = null;

    public void Awake()
    {        
        new CheatConfig(Config);

        UnityEngine.Debug.Log("[CheatMenu] ========================");
        UnityEngine.Debug.Log("[CheatMenu] Starting Cheat Menu v1.3.2...");
        UnityEngine.Debug.Log("[CheatMenu] Cult of the Lamb: Cheaters Edition!");
        UnityEngine.Debug.Log("[CheatMenu] ========================");

        try {
            // RunAllInit() must run first — it calls ReflectionHelper.Init() which creates
            // the Harmony instance needed by all subsequent patches.
            _annotationHelper = new UnityAnnotationHelper();
            _annotationHelper.RunAllInit();
            UnityEngine.Debug.Log("[CheatMenu] Annotation system initialized successfully");

            // Patch VersionNumber.OnEnable so the main menu shows "Cheaters Edition"
            PatchVersionText();

            _onGUIFn = _annotationHelper.BuildRunAllOnGuiDelegate();
            _updateFn = _annotationHelper.BuildRunAllUpdateDelegate();
            UnityEngine.Debug.Log("[CheatMenu] Patching and loading completed successfully!");
            UnityEngine.Debug.Log("[CheatMenu] Cheat Menu is ready to use!");
        } catch(Exception e) {
            UnityEngine.Debug.LogError($"[CheatMenu] FATAL ERROR during initialization: {e.Message}");
            UnityEngine.Debug.LogError($"[CheatMenu] Stack trace: {e.StackTrace}");
        }
    }

    public void OnDisable()
    {
        try {
            UnityEngine.Debug.Log("[CheatMenu] Shutting down Cheat Menu...");
            _annotationHelper?.RunAllUnload();
            UnityEngine.Debug.Log("[CheatMenu] Cheat Menu disabled successfully");
        } catch(Exception e) {
            UnityEngine.Debug.LogError($"[CheatMenu] Error during shutdown: {e.Message}");
        }
    }

    public void OnGUI()
    {
        try {
            _onGUIFn?.Invoke();
        } catch(Exception e) {
            UnityEngine.Debug.LogError($"[CheatMenu] OnGUI error: {e.Message}");
        }
    }

    public void Update()
    {        
        try {
            _updateFn?.Invoke();
        } catch(Exception e) {
            UnityEngine.Debug.LogError($"[CheatMenu] Update error: {e.Message}");
        }
    }

    private void PatchVersionText()
    {
        try {
            MethodInfo versionPatch = typeof(Plugin).GetMethod(
                nameof(Prefix_VersionNumber_OnEnable),
                BindingFlags.Static | BindingFlags.Public
            );
            // VersionNumber.OnEnable is private
            string result = ReflectionHelper.PatchMethodPrefix(
                typeof(VersionNumber),
                "OnEnable",
                versionPatch,
                BindingFlags.Instance | BindingFlags.NonPublic,
                silent: true
            );
            if(result != null) {
                UnityEngine.Debug.Log("[CheatMenu] ✓ VersionNumber.OnEnable patched");
            } else {
                UnityEngine.Debug.LogWarning("[CheatMenu] ✗ VersionNumber.OnEnable patch failed (method not found)");
            }
        } catch(Exception e) {
            UnityEngine.Debug.LogWarning($"[CheatMenu] VersionNumber patch failed: {e.Message}");
        }
    }

    /// <summary>
    /// Replaces VersionNumber.OnEnable to display "Cheaters Edition ??".
    /// VersionNumber has a public field "Text" of type UnityEngine.UI.Text.
    /// The original method simply does: this.Text.text = Application.version;
    /// We use Traverse to avoid needing a UnityEngine.UI assembly reference.
    /// </summary>
    public static bool Prefix_VersionNumber_OnEnable(VersionNumber __instance)
    {
        try {
            // "Text" is a public field on VersionNumber (type: UnityEngine.UI.Text)
            object textComponent = HarmonyLib.Traverse.Create(__instance).Field("Text").GetValue();
            if(textComponent != null) {
                // "text" is a property on UnityEngine.UI.Text
                HarmonyLib.Traverse.Create(textComponent).Property("text")
                    .SetValue($"{UnityEngine.Application.version} - Cheaters Edition");
                return false;
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"[CheatMenu] Version text patch error: {e.Message}");
        }
        return true;
    }
}
