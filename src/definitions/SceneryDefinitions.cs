using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.SCENERY)]
public class SceneryDefinitions : IDefinition {

    private static bool s_hideAllScenery    = false;
    private static bool s_disableAllShadows = false;

    private static readonly HashSet<GameObject> s_disabledObjects = new HashSet<GameObject>();

    [Init]
    public static void Init() {
        s_hideAllScenery    = false;
        s_disableAllShadows = false;
        s_disabledObjects.Clear();
        SceneManager.sceneLoaded += OnSceneLoaded;

        try {
            MethodInfo m = typeof(SceneryDefinitions).GetMethod(nameof(Postfix_Grass_Start), BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPostfix(typeof(Grass), "Start", m, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
        } catch(Exception e) { Debug.LogWarning($"[Scenery] Grass.Start patch: {e.Message}"); }

        try {
            MethodInfo m = typeof(SceneryDefinitions).GetMethod(nameof(Postfix_LongGrass_OnEnable), BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPostfix(typeof(LongGrass), "OnEnable", m, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
        } catch(Exception e) { Debug.LogWarning($"[Scenery] LongGrass.OnEnable patch: {e.Message}"); }

        try {
            MethodInfo m = typeof(SceneryDefinitions).GetMethod(nameof(Postfix_RandomBushPicker_OnEnable), BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPostfix(typeof(RandomBushPicker), "OnEnable", m, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
        } catch(Exception e) { Debug.LogWarning($"[Scenery] RandomBushPicker.OnEnable patch: {e.Message}"); }

        try {
            MethodInfo m = typeof(SceneryDefinitions).GetMethod(nameof(Postfix_RandomGrassPicker_OnEnable), BindingFlags.Static | BindingFlags.Public);
            ReflectionHelper.PatchMethodPostfix(typeof(RandomGrassPicker), "OnEnable", m, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
        } catch(Exception e) { Debug.LogWarning($"[Scenery] RandomGrassPicker.OnEnable patch: {e.Message}"); }
    }

    [Unload]
    public static void Unload() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RestoreAll();
        if (s_disableAllShadows) GraphicsSettingsUtilities.UpdateShadows(true);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameManager gm = GameManager.GetInstance();
        if (gm != null) gm.StartCoroutine(DelayedApply());
    }

    private static IEnumerator DelayedApply() {
        yield return null;
        yield return null;
        if (s_hideAllScenery) {
            ScanAndDisable<Grass>();
            ScanAndDisable<LongGrass>();
            ScanAndDisable<RandomBushPicker>();
            ScanAndDisable<RandomGrassPicker>();
        }
    }

    private static void ScanAndDisable<T>() where T : Component {
        foreach (var c in UnityEngine.Object.FindObjectsOfType<T>()) {
            if (c == null || c.gameObject == null) continue;
            if (c.gameObject.activeSelf) {
                c.gameObject.SetActive(false);
                s_disabledObjects.Add(c.gameObject);
            }
        }
    }

    private static void RestoreAll() {
        s_hideAllScenery = false;
        foreach (var go in s_disabledObjects) {
            try { if (go != null) go.SetActive(true); } catch { }
        }
        s_disabledObjects.Clear();
    }

    public static void Postfix_Grass_Start(Grass __instance) {
        if (s_hideAllScenery && __instance?.gameObject != null && __instance.gameObject.activeSelf) {
            __instance.gameObject.SetActive(false);
            s_disabledObjects.Add(__instance.gameObject);
        }
    }

    public static void Postfix_LongGrass_OnEnable(LongGrass __instance) {
        if (s_hideAllScenery && __instance?.gameObject != null && __instance.gameObject.activeSelf) {
            __instance.gameObject.SetActive(false);
            s_disabledObjects.Add(__instance.gameObject);
        }
    }

    public static void Postfix_RandomBushPicker_OnEnable(RandomBushPicker __instance) {
        if (s_hideAllScenery && __instance?.gameObject != null && __instance.gameObject.activeSelf) {
            __instance.gameObject.SetActive(false);
            s_disabledObjects.Add(__instance.gameObject);
        }
    }

    public static void Postfix_RandomGrassPicker_OnEnable(RandomGrassPicker __instance) {
        if (s_hideAllScenery && __instance?.gameObject != null && __instance.gameObject.activeSelf) {
            __instance.gameObject.SetActive(false);
            s_disabledObjects.Add(__instance.gameObject);
        }
    }

    [CheatDetails("Hide All Scenery", "All Scenery (OFF)", "All Scenery (ON)",
        "Hides all grass, long grass, bushes and flowers across every loaded and future scene", true)]
    public static void ToggleHideAllScenery(bool flag) {
        s_hideAllScenery = flag;
        if (flag) {
            ScanAndDisable<Grass>();
            ScanAndDisable<LongGrass>();
            ScanAndDisable<RandomBushPicker>();
            ScanAndDisable<RandomGrassPicker>();
            CultUtils.PlayNotification("All scenery hidden!");
        } else {
            RestoreAll();
            CultUtils.PlayNotification("All scenery restored!");
        }
    }

    [CheatDetails("Disable All Shadows", "All Shadows (OFF)", "All Shadows (ON)",
        "Globally disables all shadow rendering including player and enemy shadows", true)]
    public static void ToggleDisableAllShadows(bool flag) {
        s_disableAllShadows = flag;
        GraphicsSettingsUtilities.UpdateShadows(!flag);
        CultUtils.PlayNotification(flag ? "All shadows disabled!" : "Shadows restored!");
    }
}
