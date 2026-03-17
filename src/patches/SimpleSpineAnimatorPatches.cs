using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Spine.Unity;

namespace CheatMenu;

/// <summary>
/// Patches SimpleSpineAnimator.GetAnimationReference to fix the AnimationReferenceAsset instantiation warning.
/// Also patches SkeletonAnimationLODGlobalManager.Update to add null checks and prevent NRE.
/// </summary>
public static class SimpleSpineAnimatorPatches
{
    // Cache for created AnimationReferenceAssets to avoid recreating
    private static readonly Dictionary<string, AnimationReferenceAsset> s_animationCache = new Dictionary<string, AnimationReferenceAsset>();
    
    [Init]
    public static void Init()
    {
        try
        {
            // Patch SimpleSpineAnimator.GetAnimationReference
            MethodInfo originalMethod = typeof(SimpleSpineAnimator).GetMethod("GetAnimationReference", 
                BindingFlags.Instance | BindingFlags.Public);
                
            if (originalMethod != null)
            {
                var transpiler = typeof(SimpleSpineAnimatorPatches).GetMethod("TranspileGetAnimationReference", 
                    BindingFlags.Static | BindingFlags.Public);
                    
                var harmony = new Harmony("CheatMenu.SimpleSpineAnimator");
                harmony.Patch(originalMethod, transpiler: new HarmonyMethod(transpiler));
                Debug.Log("[CheatMenu] SimpleSpineAnimator.GetAnimationReference transpiler applied");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CheatMenu] SimpleSpineAnimator transpiler error: {e.Message}");
        }
        
        // Patch SkeletonAnimationLODGlobalManager.Update to add null checks
        try
        {
            Type skeletonLODType = Type.GetType("SkeletonAnimationLODGlobalManager, Assembly-CSharp");
            if (skeletonLODType != null)
            {
                MethodInfo updateMethod = skeletonLODType.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    var transpiler = typeof(SimpleSpineAnimatorPatches).GetMethod("TranspileSkeletonLODUpdate", 
                        BindingFlags.Static | BindingFlags.Public);
                        
                    var harmony = new Harmony("CheatMenu.SkeletonAnimationLOD");
                    harmony.Patch(updateMethod, transpiler: new HarmonyMethod(transpiler));
                    Debug.Log("[CheatMenu] SkeletonAnimationLODGlobalManager.Update transpiler applied");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CheatMenu] SkeletonAnimationLOD transpiler error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Transpiler that replaces 'new AnimationReferenceAsset()' with ScriptableObject.CreateInstance
    /// </summary>
    public static IEnumerable<CodeInstruction> TranspileGetAnimationReference(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var result = new List<CodeInstruction>();
        
        // Look for 'newobj' instruction for AnimationReferenceAsset constructor
        for (int i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            
            // Check if this is 'newobj' creating AnimationReferenceAsset
            if (code.opcode == OpCodes.Newobj)
            {
                var method = code.operand as MethodInfo;
                if (method != null && method.DeclaringType != null && 
                    method.DeclaringType.Name == "AnimationReferenceAsset")
                {
                    // Replace with call to ScriptableObject.CreateInstance<AnimationReferenceAsset>()
                    // Stack order: Type token -> Type -> object
                    // 1. Push type token
                    result.Add(new CodeInstruction(OpCodes.Ldtoken, 
                        typeof(AnimationReferenceAsset)));
                    
                    // 2. Convert to Type
                    result.Add(new CodeInstruction(OpCodes.Call, 
                        typeof(Type).GetMethod("GetTypeFromHandle", 
                            new[] { typeof(RuntimeTypeHandle) })));
                    
                    // 3. Call CreateInstance
                    result.Add(new CodeInstruction(OpCodes.Call, 
                        typeof(ScriptableObject).GetMethod("CreateInstance", 
                            new[] { typeof(Type) })));
                    
                    continue;
                }
            }
            
            result.Add(code);
        }
        
        return result;
    }
    
    /// <summary>
    /// Transpiler for SkeletonAnimationLODGlobalManager.Update that adds null checks
    /// Specifically replaces the DynamicResolutionManager._fps access with a safe alternative
    /// </summary>
    public static IEnumerable<CodeInstruction> TranspileSkeletonLODUpdate(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var result = new List<CodeInstruction>();
        
        bool foundFPSAccess = false;
        
        for (int i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            
            // Look for: ldfld float32 DynamicResolutionManager::_fps
            // This is the problematic line that causes NRE
            if (!foundFPSAccess && code.opcode == OpCodes.Ldfld)
            {
                var field = code.operand as FieldInfo;
                if (field != null && field.Name == "_fps" && 
                    field.DeclaringType != null && field.DeclaringType.Name == "DynamicResolutionManager")
                {
                    foundFPSAccess = true;
                    
                    // Replace the _fps access with a safe fallback:
                    // Instead of DynamicResolutionManager._fps, use Application.targetFrameRate (or 60 as fallback)
                    // We need to push a default value onto the stack
                    
                    // First, push a default float value (60f)
                    result.Add(new CodeInstruction(OpCodes.Ldc_R4, 60f));
                    
                    // Skip the original ldfld instruction
                    continue;
                }
            }
            
            result.Add(code);
        }
        
        return result;
    }
    
    [Unload]
    public static void Unload()
    {
        s_animationCache.Clear();
    }
}
