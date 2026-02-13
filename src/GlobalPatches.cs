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

       // Patch Follower.Init to add defensive skeleton validation before outfit application
       try {
           MethodInfo followerInitPatch = typeof(GlobalPatches).GetMethod("Prefix_Follower_Init", BindingFlags.Static | BindingFlags.Public);
           Type[] initParams = new Type[] { typeof(FollowerBrain), typeof(FollowerOutfit) };
           string initResult = ReflectionHelper.PatchMethodPrefix(typeof(Follower), "Init", followerInitPatch, BindingFlags.Instance | BindingFlags.Public, initParams, silent: true);
           if(initResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Follower.Init successfully patched (skeleton validation)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.Log($"[CheatMenu] Follower.Init patch not applied: {e.Message}");
       }

       // Patch Interaction_WolfBase.Update to make friendly wolf follow player
       try {
           MethodInfo wolfUpdatePatch = typeof(GlobalPatches).GetMethod("Prefix_WolfBase_Update", BindingFlags.Static | BindingFlags.Public);
           string wolfResult = ReflectionHelper.PatchMethodPrefix(typeof(Interaction_WolfBase), "Update", wolfUpdatePatch, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
           if(wolfResult == null){
               wolfResult = ReflectionHelper.PatchMethodPrefix(typeof(Interaction_WolfBase), "Update", wolfUpdatePatch, BindingFlags.Instance | BindingFlags.Public, silent: true);
           }
           if(wolfResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Interaction_WolfBase.Update successfully patched (friendly wolf)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.Log($"[CheatMenu] Interaction_WolfBase.Update patch not applied: {e.Message}");
       }
    }

    [Unload]
    public static void UnpatchAll()
    {
        ReflectionHelper.UnpatchTracked(typeof(Interactor), "Update");
        ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "UnlockAbility");
        ReflectionHelper.UnpatchTracked(typeof(PlayerFarming), "Bleat");
        ReflectionHelper.UnpatchTracked(typeof(Follower), "Init");
        ReflectionHelper.UnpatchTracked(typeof(Interaction_WolfBase), "Update");
        ReflectionHelper.UnpatchTracked(typeof(VersionNumber), "OnEnable");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateCultistDLC");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateHereticDLC");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateSinfulDLC");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticatePilgrimDLC");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticateMajorDLC");
        ReflectionHelper.UnpatchTracked(typeof(GameManager), "AuthenticatePrePurchaseDLC");
        ReflectionHelper.UnpatchTracked(typeof(BlunderAmmo), "UseAmmo");
        ReflectionHelper.UnpatchTracked(typeof(PlayerArrows), "RestockArrow");
        ReflectionHelper.UnpatchTracked(typeof(MiniMap), "StartMap");
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

    /// <summary>
    /// Intercepts Interaction_WolfBase.Update to make the friendly wolf follow the player.
    /// </summary>
    public static bool Prefix_WolfBase_Update(Interaction_WolfBase __instance)
    {
        return CultUtils.HandleFriendlyWolfUpdate(__instance);
    }

    /// <summary>
    /// Validates follower skeleton data before outfit application to prevent null reference crashes.
    /// This is a defensive patch that catches issues from ANY source, not just our cheats.
    /// </summary>
    public static bool Prefix_Follower_Init(Follower __instance, FollowerBrain brain, FollowerOutfit outfit)
    {
        // Let the original method run, but add protection
        try {
            // Validate that the follower has critical components initialized
            if(__instance == null || brain == null || outfit == null){
                UnityEngine.Debug.LogWarning("[CheatMenu] Follower.Init called with null parameters - skipping outfit application");
                return false;
            }

            // Use Traverse to safely access Spine property without needing spine-unity reference
            var spineTraverse = Traverse.Create(__instance).Property("Spine");
            object spine = spineTraverse.GetValue();
            
            if(spine == null){
                UnityEngine.Debug.LogWarning($"[CheatMenu] Follower {brain.Info.Name} spawning with null Spine - will retry outfit after spawn");
                
                // Temporarily set outfit to default to prevent crash
                brain.Info.Outfit = FollowerOutfitType.Follower;
                brain.Info.Clothing = FollowerClothingType.Naked;
            }
            else{
                // Check skeleton data
                var skeletonTraverse = Traverse.Create(spine).Property("skeleton");
                object skeleton = skeletonTraverse.GetValue();
                
                if(skeleton != null){
                    var dataTraverse = Traverse.Create(skeleton).Property("Data");
                    object data = dataTraverse.GetValue();
                    
                    if(data == null){
                        UnityEngine.Debug.LogWarning($"[CheatMenu] Follower {brain.Info.Name} has null skeleton Data - resetting to default outfit");
                        brain.Info.Outfit = FollowerOutfitType.Follower;
                        brain.Info.Clothing = FollowerClothingType.Naked;
                    }
                }
            }

            // All checks passed, let the original Init run
            return true;
        } catch(Exception e){
            UnityEngine.Debug.LogError($"[CheatMenu] Error in Follower.Init validation: {e.Message}");
            // Let the original method try anyway, but we've logged the issue
            return true;
        }
    }
}



