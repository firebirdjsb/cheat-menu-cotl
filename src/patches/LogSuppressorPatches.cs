using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu.Patches
{
    /// <summary>
    /// Patches Unity's debug logging to filter out specific warnings.
    /// This silences the Spine.AnimationReferenceAsset warnings that pollute the log.
    /// </summary>
    public static class LogSuppressorPatches
    {
        // Messages to suppress
        private static readonly HashSet<string> SuppressedMessagePatterns = new HashSet<string>
        {
            "Spine.Unity.AnimationReferenceAsset must be instantiated using the ScriptableObject.CreateInstance",
            "Spine.Unity.AnimationReferenceAsset must be instantiated using ScriptableObject.CreateInstance",
            "SPINE: AnimationState waiting for processing when queue empty",
            "SkeletonAnimationLODGlobalManager.Update",
            "SkeletonAnimation.Update, ignoring attempt to queue a skeleton on thread more than once",
            "ignoring attempt to queue a skeleton on thread",
            "BiomeLightingSettings must be instantiated using the ScriptableObject.CreateInstance",
            "Coroutine couldn't be started because",
            "OutlineEffect does not support Unity Post-processing stack v2",
            "Parent of RectTransform is being set with parent property",
            // Twitch API errors - these happen when offline or not logged in
            "Org.OpenAPITools.Client.ApiException: HTTP/1.1 401 Unauthorized",
            "TwitchRequest.ConnectionTest",
        };
        
        [Init]
        public static void Init()
        {
            try
            {
                // Patch Debug.LogWarning to filter out our suppressed messages
                var debugType = typeof(UnityEngine.Debug);
                
                // Try various LogWarning overloads
                PatchLogMethod(debugType, "LogWarning", new[] { typeof(object), typeof(object) });
                PatchLogMethod(debugType, "LogWarning", new[] { typeof(string) });
                PatchLogMethod(debugType, "LogWarning", new[] { typeof(string), typeof(object) });
                
                // Also patch Log and LogError
                PatchLogMethod(debugType, "Log", new[] { typeof(object) });
                PatchLogMethod(debugType, "Log", new[] { typeof(string), typeof(object) });
                PatchLogMethod(debugType, "LogError", new[] { typeof(object) });
                PatchLogMethod(debugType, "LogError", new[] { typeof(string), typeof(object) });
                
                Debug.Log("[CheatMenu] Log suppressor initialized");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CheatMenu] Log suppressor init error: {ex.Message}");
            }
        }
        
        private static void PatchLogMethod(Type debugType, string methodName, Type[] paramTypes)
        {
            try
            {
                var method = debugType.GetMethod(methodName, 
                    BindingFlags.Static | BindingFlags.Public,
                    null, paramTypes, null);
                    
                if (method != null)
                {
                    string prefixName = methodName switch
                    {
                        "Log" => "Prefix_Log",
                        "LogError" => "Prefix_LogError",
                        _ => "Prefix_LogWarning"
                    };
                    
                    var prefix = typeof(LogSuppressorPatches).GetMethod(
                        prefixName,
                        BindingFlags.Static | BindingFlags.Public);
                        
                    if (prefix != null)
                    {
                        var harmony = new Harmony("CheatMenu.LogSuppressor." + methodName);
                        harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                    }
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Prefix for Debug.Log - returns false to suppress specific messages
        /// </summary>
        public static bool Prefix_Log(object message)
        {
            return ShouldLog(message?.ToString());
        }
        
        /// <summary>
        /// Prefix for Debug.LogError - returns false to suppress specific messages
        /// </summary>
        public static bool Prefix_LogError(object message)
        {
            return ShouldLog(message?.ToString());
        }
        
        /// <summary>
        /// Prefix for Debug.LogWarning - returns false to suppress specific messages  
        /// </summary>
        public static bool Prefix_LogWarning(object message)
        {
            return ShouldLog(message?.ToString());
        }
        
        /// <summary>
        /// Prefix for Debug.LogWarning with format args
        /// </summary>
        public static bool Prefix_LogWarning(object format, object args)
        {
            string msg = format?.ToString();
            if (args != null)
            {
                try { msg = string.Format(msg, args); }
                catch { }
            }
            return ShouldLog(msg);
        }
        
        private static bool ShouldLog(string message)
        {
            if (string.IsNullOrEmpty(message)) return true;
            
            // Check if message matches any suppressed pattern
            foreach (var pattern in SuppressedMessagePatterns)
            {
                if (message.Contains(pattern))
                {
                    return false; // Suppress this log
                }
            }
            
            return true; // Allow all other logs
        }
        
        [Unload]
        public static void Unload()
        {
            // Harmony will automatically unpatch when the domain is unloaded
        }
    }
}
