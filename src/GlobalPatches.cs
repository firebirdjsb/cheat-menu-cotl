using System;
using System.Reflection;
using HarmonyLib;

namespace CheatMenu;

public class GlobalPatches {
    [Init]
    [EnforceOrderLast]
    public static void Init()
    {
       MethodInfo interactorPatch = typeof(GlobalPatches).GetMethod("Prefix_Interactor_Update", BindingFlags.Static | BindingFlags.Public);
       ReflectionHelper.PatchMethodPrefix(typeof(Interactor), "Update", interactorPatch, BindingFlags.Instance | BindingFlags.NonPublic);

       try {
           MethodInfo upgradeSystemPatch = typeof(GlobalPatches).GetMethod("Prefix_UpgradeSystem_UnlockAbility", BindingFlags.Static | BindingFlags.Public);
           string result = ReflectionHelper.PatchMethodPrefix(typeof(UpgradeSystem), "UnlockAbility", upgradeSystemPatch, BindingFlags.Static | BindingFlags.Public, new Type[]{typeof(UpgradeSystem.Type)}, silent: true);
           if(result != null) {
               UnityEngine.Debug.Log("[CheatMenu] UpgradeSystem.UnlockAbility successfully patched");
           }
       } catch(Exception e) {
           UnityEngine.Debug.Log($"[CheatMenu] UpgradeSystem.UnlockAbility patch not applied (game version may have changed): {e.Message}");
       }

       // Patch PlayerFarming.Bleat to suppress the in-game bahhh when R3 is used for the cheat menu
       try {
           MethodInfo bleatPatch = typeof(GlobalPatches).GetMethod("Prefix_PlayerFarming_Bleat", BindingFlags.Static | BindingFlags.Public);
           string bleatResult = ReflectionHelper.PatchMethodPrefix(typeof(PlayerFarming), "Bleat", bleatPatch, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
           if(bleatResult == null){
               // Try public binding if private didn't work
               bleatResult = ReflectionHelper.PatchMethodPrefix(typeof(PlayerFarming), "Bleat", bleatPatch, BindingFlags.Instance | BindingFlags.Public, silent: true);
           }
           if(bleatResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] PlayerFarming.Bleat successfully patched (R3 suppression)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.Log($"[CheatMenu] PlayerFarming.Bleat patch not applied (game version may have changed): {e.Message}");
       }
    }

    [Unload]
    public static void UnpatchAll()
    {
        ReflectionHelper.UnpatchTracked(typeof(Interactor), "Update");
        ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "UnlockAbility");
        ReflectionHelper.UnpatchTracked(typeof(PlayerFarming), "Bleat");
    }

    //If because of our mod we try to active both sides of a pair of a ritual we want
    //to fix it so that it displays correctly in menus.
    public static bool Prefix_UpgradeSystem_UnlockAbility(UpgradeSystem.Type Type)
    {
        var dictionary = CultUtils.GetDictionaryRitualPairs();
        if(dictionary.TryGetValue(Type, out UpgradeSystem.Type matchingItem)){
            if(UpgradeSystem.UnlockedUpgrades.Contains(matchingItem)){
                UpgradeSystem.UnlockedUpgrades.Remove(matchingItem);
            }
        }

        return true;
    }
    
    public static bool Prefix_Interactor_Update()
    {
        return !CheatMenuGui.GuiEnabled;
    }

    /// <summary>
    /// Blocks the in-game bahhh/bleat action when R3 was just consumed by the cheat menu toggle.
    /// </summary>
    public static bool Prefix_PlayerFarming_Bleat()
    {
        if(CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.ShouldSuppressR3){
            return false;
        }
        return true;
    }
}

