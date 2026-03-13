using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Spine;
using UnityEngine;
using System.Reflection.Emit;
using System.Linq;

namespace CheatMenu;

public class GlobalPatches {
private static HashSet<int> s_fixedFollowerIds = new HashSet<int>();

    /// <summary>
    /// Patches FollowerManager.SpawnExistingRecruits to prevent duplicate spawns when using cheat menu.
    /// When CultUtils.IsSpawningFollowerFromCheat is true, this prevents the game from auto-spawning
    /// additional recruits after recruitment completes.
    /// </summary>
    public static bool Prefix_FollowerManager_SpawnExistingRecruits(Vector3 position){
        if(CultUtils.IsSpawningFollowerFromCheat){
            UnityEngine.Debug.Log("[CheatMenu] Blocked auto-spawn of existing recruits (cheat menu active)");
            return false; // Don't run the original method
        }
        return true; // Run the original method
    }

    /// <summary>
    /// Patches SkeletonAnimationLODGlobalManager.Update to prevent NullReferenceException spam.
    /// The game can throw NRE when:
    /// 1. DynamicResolutionManager._fps is accessed before initialization (can be Infinity)
    /// 2. CameraFollowTarget.Instance becomes null between check and use
    /// 3. The skeletons dictionary is accessed before initialization
    /// </summary>
    public static bool Prefix_SkeletonAnimationLODGlobalManager_Update(SkeletonAnimationLODGlobalManager __instance)
    {
        try
        {
            // Check if instance is null
            if (__instance == null)
            {
                return false;
            }
            
            // Use reflection to access the private 'skeletons' field safely
            var skeletonsField = typeof(SkeletonAnimationLODGlobalManager).GetField("skeletons", BindingFlags.NonPublic | BindingFlags.Instance);
            if (skeletonsField == null)
            {
                return false;
            }
            
            var skeletons = skeletonsField.GetValue(__instance) as Dictionary<Transform, SkeletonAnimationLODManager>;
            
            // If skeletons is null or empty, skip the update logic
            if (skeletons == null || skeletons.Count == 0)
            {
                return false;
            }
            
            // Check if CameraFollowTarget.Instance is available
            var cameraFollowTargetType = Type.GetType("CameraFollowTarget, Assembly-CSharp");
            if (cameraFollowTargetType != null)
            {
                var instanceProp = cameraFollowTargetType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    if (instance == null)
                    {
                        return false;
                    }
                }
            }
            
            return true; // Run original method
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[CheatMenu] SkeletonAnimationLODGlobalManager.Update patch error: {ex.Message}");
            return false; // Skip original on error
        }
    }

    /// <summary>
    /// Returns the default clothing type for follower reset: FollowerClothingType.None.
    /// In the game's assembly, Clothing=None is the actual default state for new followers
    /// (see FollowerInfo.NewCharacter - Clothing is never set, defaults to 0/None).
    /// When Clothing is None, FollowerBrain.GetClothingName returns GetOutfitName(Follower)
    /// which renders the default Acolyte Robes via GetRobesName (level-based).
    /// FollowerClothingType.Robe has NO ClothingData ScriptableObject asset, so using it
    /// would trigger the invalid clothing fix on every load.
    /// </summary>
    public static FollowerClothingType GetSafeDefaultClothing(){
        return FollowerClothingType.None;
    }
    [Init]
    [EnforceOrderLast]
    public static void Init()
    {
        // Suppress font loading errors for missing localization fonts (Chinese, Korean, Japanese, Arabic)
        // These errors occur during startup when Unity tries to load optional language fonts
        // Unfortunately, Unity logs these before our callback, so we can't fully suppress them
        // This is a known Unity limitation with missing font assets

        MethodInfo interactorPatch = typeof(GlobalPatches).GetMethod("Prefix_Interactor_Update", BindingFlags.Static | BindingFlags.Public);
       ReflectionHelper.PatchMethodPrefix(typeof(Interactor), "Update", interactorPatch, BindingFlags.Instance | BindingFlags.NonPublic);

       try {
           MethodInfo upgradeSystemPatch = typeof(GlobalPatches).GetMethod("Prefix_UpgradeSystem_UnlockAbility", BindingFlags.Static | BindingFlags.Public);
           string result = ReflectionHelper.PatchMethodPrefix(typeof(UpgradeSystem), "UnlockAbility", upgradeSystemPatch, BindingFlags.Static | BindingFlags.Public, new Type[]{typeof(UpgradeSystem.Type)}, silent: true);
           if(result != null) {
               UnityEngine.Debug.Log("[CheatMenu] UpgradeSystem.UnlockAbility successfully patched");
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] UpgradeSystem.UnlockAbility patch not applied (game version may have changed): {e.Message}");
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
           UnityEngine.Debug.LogWarning($"[CheatMenu] PlayerFarming.Bleat patch not applied (game version may have changed): {e.Message}");
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
           UnityEngine.Debug.LogWarning($"[CheatMenu] Follower.Init patch not applied: {e.Message}");
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
           UnityEngine.Debug.LogWarning($"[CheatMenu] Interaction_WolfBase.Update patch not applied: {e.Message}");
       }

       // Patch Spine.Skin.AddSkin to prevent NullReferenceException when FindSkin returns null
       // This is the root cause - SetFollowerCostume calls AddSkin(FindSkin(...)) without null checks
       try {
           MethodInfo addSkinPatch = typeof(GlobalPatches).GetMethod("Prefix_Skin_AddSkin", BindingFlags.Static | BindingFlags.Public);
           string addSkinResult = ReflectionHelper.PatchMethodPrefix(typeof(Spine.Skin), "AddSkin", addSkinPatch, BindingFlags.Instance | BindingFlags.Public, new Type[]{typeof(Spine.Skin)}, silent: true);
           if(addSkinResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Spine.Skin.AddSkin successfully patched (null safety)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] Spine.Skin.AddSkin patch not applied: {e.Message}");
       }

       // Patch Follower.Init with a finalizer to properly handle crashes and prevent half-initialized followers
       try {
           MethodInfo initFinalizerPatch = typeof(GlobalPatches).GetMethod("Finalizer_Follower_Init", BindingFlags.Static | BindingFlags.Public);
           Type[] initParams = new Type[] { typeof(FollowerBrain), typeof(FollowerOutfit) };
           string finalizerResult = ReflectionHelper.PatchMethodFinalizer(typeof(Follower), "Init", initFinalizerPatch, BindingFlags.Instance | BindingFlags.Public, initParams, silent: true);
           if(finalizerResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Follower.Init finalizer successfully patched (crash recovery)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] Follower.Init finalizer patch not applied: {e.Message}");
       }

       // Patch Follower.Tick to skip ticking when Outfit is null (prevents FollowerBrainInfo.get_Protection NRE spam)
       try {
           MethodInfo followerTickPatch = typeof(GlobalPatches).GetMethod("Prefix_Follower_Tick", BindingFlags.Static | BindingFlags.Public);
           string tickResult = ReflectionHelper.PatchMethodPrefix(typeof(Follower), "Tick", followerTickPatch, BindingFlags.Instance | BindingFlags.Public, new Type[]{typeof(float)}, silent: true);
           if(tickResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Follower.Tick successfully patched (null Outfit guard)");
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] Follower.Tick patch not applied: {e.Message}");
       }

       // Patch FollowerManager.SpawnExistingRecruits to prevent duplicate recruit spawning
       try {
           MethodInfo spawnPatch = typeof(GlobalPatches).GetMethod("Prefix_FollowerManager_SpawnExistingRecruits", BindingFlags.Static | BindingFlags.Public);
           string spawnResult = ReflectionHelper.PatchMethodPrefix(typeof(FollowerManager), "SpawnExistingRecruits", spawnPatch, BindingFlags.Static | BindingFlags.Public, new Type[]{typeof(Vector3)}, silent: true);
           if(spawnResult != null) {
               UnityEngine.Debug.Log("[CheatMenu] Patched SpawnExistingRecruits");
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] SpawnExistingRecruits patch not applied: {e.Message}");
       }

       // Patch SkeletonAnimationLODGlobalManager.Update to prevent NullReferenceException spam
       try {
           MethodInfo skeletonLODPatch = typeof(GlobalPatches).GetMethod("Prefix_SkeletonAnimationLODGlobalManager_Update", BindingFlags.Static | BindingFlags.Public);
           Type skeletonLODType = Type.GetType("SkeletonAnimationLODGlobalManager, Assembly-CSharp");
           if (skeletonLODType != null) {
               string patchResult = ReflectionHelper.PatchMethodPrefix(skeletonLODType, "Update", skeletonLODPatch, BindingFlags.Instance | BindingFlags.NonPublic, silent: true);
               if(patchResult != null) {
                   UnityEngine.Debug.Log("[CheatMenu] SkeletonAnimationLODGlobalManager.Update successfully patched (NRE fix)");
               }
           }
       } catch(Exception e) {
           UnityEngine.Debug.LogWarning($"[CheatMenu] SkeletonAnimationLODGlobalManager.Update patch not applied: {e.Message}");
       }
    }

    [Unload]
    public static void UnpatchAll()
    {
        ReflectionHelper.UnpatchTracked(typeof(Interactor), "Update");
        ReflectionHelper.UnpatchTracked(typeof(UpgradeSystem), "UnlockAbility");
        ReflectionHelper.UnpatchTracked(typeof(PlayerFarming), "Bleat");
        ReflectionHelper.UnpatchTracked(typeof(Follower), "Init");
        ReflectionHelper.UnpatchTracked(typeof(Follower), "Tick");
        ReflectionHelper.UnpatchTracked(typeof(Spine.Skin), "AddSkin");
        ReflectionHelper.UnpatchTracked(typeof(Interaction_WolfBase), "Update");
        ReflectionHelper.UnpatchTracked(typeof(VersionNumber), "OnEnable");
        ReflectionHelper.UnpatchTracked(typeof(BlunderAmmo), "UseAmmo");
        ReflectionHelper.UnpatchTracked(typeof(PlayerArrows), "RestockArrow");
        ReflectionHelper.UnpatchTracked(typeof(MiniMap), "OnBiomeGenerated");
        ReflectionHelper.UnpatchTracked(typeof(FollowerManager), "SpawnExistingRecruits");
        
        // Unpatch SkeletonAnimationLODGlobalManager.Update
        try {
            Type skeletonLODType = Type.GetType("SkeletonAnimationLODGlobalManager, Assembly-CSharp");
            if (skeletonLODType != null) {
                ReflectionHelper.UnpatchTracked(skeletonLODType, "Update");
            }
        } catch { }
        
        // Clear the fixed follower IDs set to prevent memory leak on new game load
        s_fixedFollowerIds.Clear();
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
    /// Validates follower data before outfit application to prevent null reference crashes during save loading.
    /// Only intervenes when actual errors would occur:
    /// - ClothingData is null for the follower's clothing type (causes NRE in get_Protection)
    /// - Spine skeleton data is null or incomplete (causes NRE in SetFollowerCostume)
    /// - SkinName doesn't exist in skeleton data (causes NRE in AddSkin)
    /// Does NOT change valid clothing types like Naked/None that the game uses intentionally.
    /// When a fix is needed, resets to Clothing=None + Outfit=Follower (the game's actual default
    /// Acolyte Robes state, as seen in FollowerInfo.NewCharacter from the assembly dump).
    /// </summary>
    public static bool Prefix_Follower_Init(Follower __instance, FollowerBrain brain, FollowerOutfit outfit)
    {
        try {
            if(__instance == null || brain == null || outfit == null){
                return false;
            }

            // Only fix clothing if the type has NO valid ClothingData (would cause NRE every tick)
            // None and Naked are valid game states - the game handles them specially:
            //   None -> GetClothingName returns GetOutfitName(Follower) -> default Acolyte Robes
            //   Naked -> used by Nudist trait and special outfit types
            if(brain.Info.Clothing != FollowerClothingType.None && brain.Info.Clothing != FollowerClothingType.Naked){
                var clothingData = TailorManager.GetClothingData(brain.Info.Clothing);
                if(clothingData == null){
                    brain.Info.Clothing = FollowerClothingType.None;
                    brain.Info.Outfit = FollowerOutfitType.Follower;
                }
            }

            // Spine is a public field on Follower, not a property
            var spineAnim = Traverse.Create(__instance).Field("Spine").GetValue<Spine.Unity.SkeletonAnimation>();

            if(spineAnim == null || spineAnim.skeleton == null || spineAnim.skeleton.Data == null){
                brain.Info.Outfit = FollowerOutfitType.Follower;
                brain.Info.Clothing = FollowerClothingType.None;
                // Still let the original Init run - it checks Spine != null before using AnimationState
                return true;
            }

            // Validate that the follower's SkinName exists in the skeleton data
            // If FindSkin returns null the game's SetFollowerCostume will crash on AddSkin(null)
            var skeletonData = spineAnim.skeleton.Data;
            string skinName = brain.Info.SkinName;
            if(!string.IsNullOrEmpty(skinName)){
                var foundSkin = skeletonData.FindSkin(skinName);
                if(foundSkin == null){
                    // The skin doesn't exist in skeleton data - find a valid fallback
                    var catSkin = skeletonData.FindSkin("Cat");
                    if(catSkin != null){
                        brain.Info.SkinName = "Cat";
                    } else {
                        // Cat doesn't exist either - use the first available skin
                        var skins = skeletonData.Skins;
                        if(skins != null && skins.Count > 0){
                            brain.Info.SkinName = skins.Items[0].Name;
                        }
                    }
                    // Also reset outfit to safe defaults since the skin was invalid
                    brain.Info.Outfit = FollowerOutfitType.Follower;
                    brain.Info.Clothing = FollowerClothingType.None;
                }
            }

        } catch(Exception e){
            UnityEngine.Debug.LogError($"[CheatMenu] Error in Follower.Init validation: {e.Message}");
        }

        // Let the original run - better to attempt than to skip entirely
        return true;
    }

    /// <summary>
    /// Guards against NullReferenceException spam in FollowerBrainInfo.get_Protection.
    ///    so TailorManager.GetClothingData() returns null and .ProtectionType NREs
    /// This prefix validates both conditions and auto-repairs invalid clothing data.
    /// </summary>
    public static bool Prefix_Follower_Tick(Follower __instance)
    {
        if(__instance.Outfit == null){
            return false;
        }
        try {
            // None and Naked are valid
            if(__instance.Brain?.Info != null
                && __instance.Brain.Info.Clothing != FollowerClothingType.None
                && __instance.Brain.Info.Clothing != FollowerClothingType.Naked){
                int followerId = __instance.Brain.Info.ID;
                if(!s_fixedFollowerIds.Contains(followerId)){
                    if(TailorManager.GetClothingData(__instance.Brain.Info.Clothing) == null){
                        __instance.Brain.Info.Clothing = FollowerClothingType.None;
                        __instance.Brain.Info.Outfit = FollowerOutfitType.Follower;
                        s_fixedFollowerIds.Add(followerId);
                    }
                }
            }
        } catch {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Null-safety patch for Spine.Skin.AddSkin - the root cause of follower costume crashes.
    /// FollowerBrain.SetFollowerCostume calls AddSkin(FindSkin(...)) 20+ times without null checks.
    /// When FindSkin returns null, AddSkin throws NullReferenceException.
    /// This patch simply skips the call when the argument is null.
    /// </summary>
    public static bool Prefix_Skin_AddSkin(Spine.Skin skin)
    {
        if(skin == null){
            return false;
        }
        return true;
    }

    /// <summary>
    /// Finalizer for Follower.Init - ensures followers are properly initialized even when Init crashes.
    /// Without this, a crash in Init leaves the Follower in a half-initialized state where:
    /// - Outfit field may be null, causing FollowerBrainInfo.get_Protection to throw on every Tick
    /// - Spine animation state is incomplete, causing SetOverrideOutfit crashes on path callbacks
    /// This finalizer catches the exception, resets follower data to safe defaults, and suppresses
    /// the error to prevent it from propagating up to LocationManager.SpawnFollowers.
    /// </summary>
    public static Exception Finalizer_Follower_Init(Exception __exception, Follower __instance, FollowerBrain brain, FollowerOutfit outfit)
    {
        if(__exception != null){
            string followerName = brain?.Info?.Name ?? "Unknown";
            UnityEngine.Debug.LogWarning($"[CheatMenu] Follower.Init exception caught and suppressed for '{followerName}': {__exception.Message}");
            UnityEngine.Debug.LogWarning($"[CheatMenu] Stack: {__exception.StackTrace}");

            try {
                // Reset the follower data to safe defaults so future spawns won't crash
                // Clothing=None + Outfit=Follower is the game's actual default (Acolyte Robes)
                if(brain?.Info != null){
                    brain.Info.Outfit = FollowerOutfitType.Follower;
                    brain.Info.Clothing = FollowerClothingType.None;
                }

                // Ensure critical fields are set so Follower.Tick doesn't crash
                if(__instance != null){
                    // Brain should already be set (it's assigned before the crash point)
                    if(__instance.Brain == null && brain != null){
                        Traverse.Create(__instance).Field("Brain").SetValue(brain);
                    }

                    // Outfit must be set or FollowerBrainInfo.get_Protection will NRE on every Tick
                    var outfitField = Traverse.Create(__instance).Field("Outfit");
                    if(outfitField.GetValue() == null && outfit != null){
                        outfitField.SetValue(outfit);
                    }
                }

                UnityEngine.Debug.Log($"[CheatMenu] Reset follower '{followerName}' to safe defaults after crash");
            } catch(Exception e){
                UnityEngine.Debug.LogError($"[CheatMenu] Error during Follower.Init crash recovery for '{followerName}': {e.Message}");
            }

            // Return null to suppress the original exception and prevent loading screen freeze
            return null;
        }
        return null;
    }
}






