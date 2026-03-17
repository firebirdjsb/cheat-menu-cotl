using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Harmony patches for FollowersNameManager to fix the "Required atlas size exceeds supported max (4096x4096)" error.
/// 
/// The original code limits the follower name atlas to 1024x1024, which can overflow when there are many followers.
/// This patch increases the maximum atlas size to 4096x4096 to accommodate more followers.
/// 
/// With the original settings:
/// - Each cell: 200x24 pixels (192 text + 8 padding)
/// - Max columns in 1024: 5
/// - Max rows in 1024: 42
/// - Max followers: ~210
/// 
/// With the patch (4096x4096):
/// - Max columns in 4096: 20
/// - Max rows in 4096: 170
/// - Max followers: ~3400
/// </summary>
public static class FollowersNameManagerPatches
{
    private const int NEW_MAX_ATLAS_WIDTH = 4096;
    private const int NEW_MAX_ATLAS_HEIGHT = 4096;
    
    /// <summary>
    /// Initializes the FollowersNameManager patches.
    /// </summary>
    [Init]
    public static void Init()
    {
        try
        {
            // Patch GenerateAtlas to increase max atlas size
            MethodInfo prefixMethod = typeof(FollowersNameManagerPatches).GetMethod("Prefix_GenerateAtlas", 
                BindingFlags.Static | BindingFlags.Public);
            
            // Use ReflectionHelper to find and patch the type
            Type followersNameManagerType = HarmonyLib.AccessTools.TypeByName("FollowersNameManager");
            
            if (followersNameManagerType != null)
            {
                string patchResult = ReflectionHelper.PatchMethodPrefix(
                    followersNameManagerType, 
                    "GenerateAtlas", 
                    prefixMethod, 
                    BindingFlags.Instance | BindingFlags.NonPublic, 
                    silent: false);
                    
                if (patchResult != null)
                {
                    UnityEngine.Debug.Log("[CheatMenu] FollowersNameManager.GenerateAtlas successfully patched (atlas size: 4096x4096)");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[CheatMenu] FollowersNameManager.GenerateAtlas patch not applied (method not found)");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[CheatMenu] FollowersNameManager type not found - patch not applied");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"[CheatMenu] FollowersNameManager patch error: {e.Message}");
        }
    }

    /// <summary>
    /// Prefix patch for FollowersNameManager.GenerateAtlas.
    /// Increases the max atlas width and height from 1024 to 4096 before the atlas generation logic runs.
    /// Also initializes the texture with proper dimensions.
    /// </summary>
    public static void Prefix_GenerateAtlas(object __instance)
    {
        try
        {
            // Use Traverse to access private fields
            var traverse = Traverse.Create(__instance);
            
            // Get current values
            int currentMaxWidth = traverse.Field("maxAtlasWidth").GetValue<int>();
            int currentMaxHeight = traverse.Field("maxAtlasHeight").GetValue<int>();
            
            // Only modify if they're still at default 1024
            if (currentMaxWidth == 1024 || currentMaxHeight == 1024)
            {
                traverse.Field("maxAtlasWidth").SetValue(NEW_MAX_ATLAS_WIDTH);
                traverse.Field("maxAtlasHeight").SetValue(NEW_MAX_ATLAS_HEIGHT);
                
                UnityEngine.Debug.Log($"[CheatMenu] Increased FollowersNameManager atlas size: {currentMaxWidth}x{currentMaxHeight} -> {NEW_MAX_ATLAS_WIDTH}x{NEW_MAX_ATLAS_HEIGHT}");
            }
            
            // Also ensure the texture is properly initialized with the new size
            // The texture is created in Initialize() but may have 0 dimensions initially
            var atlasTexture = traverse.Field("atlasTexture").GetValue<UnityEngine.Texture2D>();
            if (atlasTexture == null || atlasTexture.width == 0 || atlasTexture.height == 0)
            {
                // Create a new texture with proper dimensions
                var newTexture = new UnityEngine.Texture2D(NEW_MAX_ATLAS_WIDTH, NEW_MAX_ATLAS_HEIGHT, UnityEngine.TextureFormat.RGBA32, false);
                newTexture.filterMode = UnityEngine.FilterMode.Bilinear;
                newTexture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
                
                // Fill with transparent pixels
                var pixels = new UnityEngine.Color32[NEW_MAX_ATLAS_WIDTH * NEW_MAX_ATLAS_HEIGHT];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new UnityEngine.Color32(0, 0, 0, 0);
                }
                newTexture.SetPixels32(pixels);
                newTexture.Apply();
                
                traverse.Field("atlasTexture").SetValue(newTexture);
                UnityEngine.Debug.Log("[CheatMenu] Created new FollowerNamesAtlas_Texture with 4096x4096 dimensions");
            }
        }
        catch (Exception e)
        {
            // Silently fail - don't break the game if the patch doesn't work
            UnityEngine.Debug.LogWarning($"[CheatMenu] FollowersNameManager prefix error: {e.Message}");
        }
    }

    /// <summary>
    /// Unpatches the FollowersNameManager patches.
    /// </summary>
    [Unload]
    public static void UnpatchAll()
    {
        try
        {
            Type followersNameManagerType = Type.GetType("FollowersNameManager, Assembly-CSharp");
            if (followersNameManagerType != null)
            {
                ReflectionHelper.UnpatchTracked(followersNameManagerType, "GenerateAtlas");
            }
        }
        catch { }
    }
}
