using BepInEx;
using System;
using System.Reflection;

namespace CheatMenu;

[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.2.0")]
public class Plugin : BaseUnityPlugin
{    
    private UnityAnnotationHelper _annotationHelper;
    private Action _updateFn = null;
    private Action _onGUIFn = null;

    public void Awake()
    {        
        new CheatConfig(Config);

        UnityEngine.Debug.Log("[CheatMenu] Welcome to Cult of the Lamb: Cheaters Edition!");

        // RunAllInit() must run first — it calls ReflectionHelper.Init() which creates
        // the Harmony instance needed by all subsequent patches.
        _annotationHelper = new UnityAnnotationHelper();
        _annotationHelper.RunAllInit();

        // Patch DLC authentication methods BEFORE GameManager.Start() runs CheckDLCStatus.
        // GameManager.Authenticate*DLC() calls SteamApps.BIsSubscribedApp() and if it returns
        // false the game actively deactivates the DLC (removes skins, structures, fleeces).
        // By patching these to return true, the game's own Activate*DLC() flow runs naturally
        // which properly unlocks all skins, structures, clothing and fleeces for each pack.
        // This is still early enough — GameManager.Start() runs after all Awake() calls.
        PatchDLCAuthentication();

        // Patch VersionNumber.OnEnable so the main menu shows "Cheaters Edition"
        PatchVersionText();

        _onGUIFn = _annotationHelper.BuildRunAllOnGuiDelegate();
        _updateFn = _annotationHelper.BuildRunAllUpdateDelegate();
        UnityEngine.Debug.Log("[CheatMenu] Patching and loading completed!");
    }

    public void OnDisable()
    {
        _annotationHelper.RunAllUnload();
    }

    public void OnGUI()
    {
        _onGUIFn();
    }

    public void Update()
    {        
        _updateFn();
    }

    private void PatchDLCAuthentication()
    {
        string[] authMethods = new string[] {
            "AuthenticateCultistDLC",
            "AuthenticateHereticDLC",
            "AuthenticateSinfulDLC",
            "AuthenticatePilgrimDLC",
            "AuthenticateMajorDLC",
            "AuthenticatePrePurchaseDLC"
        };

        MethodInfo patchMethod = typeof(Plugin).GetMethod(
            nameof(Prefix_AuthenticateDLC_ReturnTrue),
            BindingFlags.Static | BindingFlags.Public
        );

        if(patchMethod == null) {
            UnityEngine.Debug.LogError("[CheatMenu] Prefix_AuthenticateDLC_ReturnTrue method not found via reflection!");
            return;
        }

        int patchedCount = 0;
        foreach(string methodName in authMethods)
        {
            try {
                // Verify the target method exists first
                MethodInfo targetMethod = typeof(GameManager).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                if(targetMethod == null) {
                    UnityEngine.Debug.LogWarning($"[CheatMenu] GameManager.{methodName} not found (game version changed?)");
                    continue;
                }

                string result = ReflectionHelper.PatchMethodPrefix(
                    typeof(GameManager),
                    methodName,
                    patchMethod,
                    BindingFlags.Static | BindingFlags.Public,
                    silent: true
                );
                if(result != null) {
                    patchedCount++;
                }
            } catch(Exception e) {
                UnityEngine.Debug.LogWarning($"[CheatMenu] Failed to patch {methodName}: {e.Message}");
            }
        }
        UnityEngine.Debug.Log($"[CheatMenu] DLC authentication: {patchedCount}/{authMethods.Length} methods patched");
    }

    /// <summary>
    /// Prefix patch for all GameManager.Authenticate*DLC() methods.
    /// Sets __result to true and skips the original SteamApps.BIsSubscribedApp check.
    /// </summary>
    public static bool Prefix_AuthenticateDLC_ReturnTrue(ref bool __result)
    {
        __result = true;
        return false; // Skip original method
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
                UnityEngine.Debug.Log("[CheatMenu] ? VersionNumber.OnEnable patched");
            } else {
                UnityEngine.Debug.LogWarning("[CheatMenu] VersionNumber.OnEnable patch failed (method not found)");
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

