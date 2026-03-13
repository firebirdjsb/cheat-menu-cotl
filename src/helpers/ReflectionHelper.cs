using System.Reflection;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;

namespace CheatMenu;

/// <summary>
/// Utility class for runtime reflection and Harmony patching operations.
/// Provides centralized methods for discovering methods, applying patches,
/// and managing patch lifecycle with tracking for proper cleanup.
/// </summary>
/// <remarks>
/// This helper abstracts away Harmony complexity and provides a consistent interface
/// for patching game methods. All patches are tracked for proper unpatching on mod unload.
/// </remarks>
public static class ReflectionHelper {
    /// <summary>
    /// Internal tracker for applied patches to enable proper cleanup.
    /// </summary>
    private class PatchTrackerDetails {
        public MethodInfo OriginalMethod;
        public HarmonyPatchType PatchType;
        
        public PatchTrackerDetails(MethodInfo originalMethod, HarmonyPatchType patchType){
            OriginalMethod = originalMethod;
            PatchType = patchType;
        }
    }

    private readonly static string HarmonyId = "org.xunfairx.cheat_menu";
    private static Harmony s_harmonyInstance;
    private static Dictionary<string, PatchTrackerDetails> s_patchTracker;

    /// <summary>
    /// Initializes the Harmony instance and patch tracker.
    /// Must be called before any patching operations.
    /// </summary>
    [Init]
    [EnforceOrderFirst(9)]
    public static void Init(){
        s_harmonyInstance = new Harmony(HarmonyId);
        s_patchTracker = new Dictionary<string, PatchTrackerDetails>();
    }

    /// <summary>
    /// Unpatches all Harmony patches created by this plugin.
    /// Called during plugin shutdown to restore the game to its original state.
    /// </summary>
    [Unload]
    public static void Unload(){
        s_harmonyInstance.UnpatchSelf();
    }

    /// <summary>
    /// Generates a unique key for tracking patches.
    /// </summary>
    private static string GetPatchTrackerKey(Type classDef, string methodName){
        return $"{classDef.Name}-{methodName}";
    }

    /// <summary>
    /// Records a patch in the tracker for later cleanup.
    /// </summary>
    private static string TrackPatch(Type classDef, MethodInfo method, HarmonyPatchType patchType){
        string patchKey = GetPatchTrackerKey(classDef, method.Name);
        s_patchTracker[patchKey] = new PatchTrackerDetails(method, patchType);
        return patchKey;
    }

    /// <summary>
    /// Gets the method that called the current method (2 frames up in the stack).
    /// Useful for automatically determining which cheat invoked a helper method.
    /// </summary>
    /// <returns>The MethodBase of the calling method.</returns>
    public static MethodBase GetCallingMethod(){
        return new StackFrame(2).GetMethod();
    }

    /// <summary>
    /// Searches the call stack to find the first method with a specific attribute.
    /// Used to automatically identify which cheat method initiated an operation.
    /// </summary>
    /// <typeparam name="T">The attribute type to search for.</typeparam>
    /// <returns>The MethodBase of the first method with the attribute, or null if not found.</returns>
    public static MethodBase GetFirstMethodInHierarchyWithAnnotation<T>() where T : Attribute{
        StackTrace trace = new();
        StackFrame[] frames = trace.GetFrames();
        foreach(var frame in frames){
            MethodBase method = frame.GetMethod();
            if(method == null) continue;
            T attributeValue = HasAttribute<T>(method);
            if(attributeValue != null){
                return frame.GetMethod();
            }
        }
        return null;
    }

    /// <summary>
    /// Removes a previously applied patch by method name.
    /// </summary>
    /// <param name="classDef">The class containing the patched method.</param>
    /// <param name="methodName">The name of the method to unpatch.</param>
    public static void UnpatchTracked(Type classDef, String methodName){
        string patchKey = GetPatchTrackerKey(classDef, methodName);
        if (s_patchTracker.TryGetValue(patchKey, out PatchTrackerDetails patchTrackedDetails))
        {
            s_harmonyInstance.Unpatch(patchTrackedDetails.OriginalMethod, patchTrackedDetails.PatchType, HarmonyId);
            UnityEngine.Debug.Log($"[ReflectionHelper] {classDef.Name}-{methodName} was unpatched to original state.");
        }
    }

    /// <summary>
    /// Gets an attribute from an enum value's definition.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="value">The enum value to get the attribute from.</param>
    /// <returns>The attribute if found; otherwise default value.</returns>
    public static T GetAttributeOfTypeEnum<T>(Enum value){
        Type enumType = value.GetType();
        MemberInfo[] memInfo = enumType.GetMember(value.ToString());
        object[] attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : default;
    }

    /// <summary>
    /// Checks if a type has a specific attribute.
    /// </summary>
    /// <typeparam name="T">The attribute type to look for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>The attribute if present; otherwise null.</returns>
    public static T HasAttribute<T>(Type type) where T : Attribute {
        return (T)type.GetCustomAttribute(typeof(T));
    }

    /// <summary>
    /// Checks if a method has a specific attribute.
    /// </summary>
    /// <typeparam name="T">The attribute type to look for.</typeparam>
    /// <param name="method">The method to check.</param>
    /// <returns>The attribute if present; otherwise null.</returns>
    public static T HasAttribute<T>(MethodBase method) where T : Attribute {
        return (T)method.GetCustomAttribute(typeof(T));
    }

    /// <summary>
    /// Checks if a method has a specific attribute (MethodInfo overload).
    /// </summary>
    /// <typeparam name="T">The attribute type to look for.</typeparam>
    /// <param name="method">The method to check.</param>
    /// <returns>The attribute if present; otherwise null.</returns>
    public static T HasAttribute<T>(MethodInfo method) where T : Attribute {
        return (T)method.GetCustomAttribute(typeof(T));
    }

    /// <summary>
    /// Gets all loadable types from an assembly, handling reflection exceptions.
    /// </summary>
    /// <param name="assembly">The assembly to get types from.</param>
    /// <returns>A list of all loadable types.</returns>
    public static List<Type> GetLoadableTypes(Assembly assembly){
        try
        {
            return assembly.GetTypes().ToList();
        }
        catch(ReflectionTypeLoadException e){
            return e.Types.Where(t => t != null).ToList();
        }
    }

    /// <summary>
    /// Applies a Harmony prefix patch to a game method.
    /// Prefix patches run before the original method and can modify parameters or skip execution.
    /// </summary>
    /// <param name="classDef">The class containing the method to patch.</param>
    /// <param name="methodName">The name of the method to patch.</param>
    /// <param name="patchMethod">The prefix method to apply.</param>
    /// <param name="flags">Binding flags to find the method (defaults to all).</param>
    /// <param name="typeParams">Optional array of parameter types for method overload resolution.</param>
    /// <param name="silent">If true, suppresses error logging when method not found.</param>
    /// <returns>The patch tracker key if successful; otherwise null.</returns>
    public static string PatchMethodPrefix(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false){
        if(patchMethod == null){
            UnityEngine.Debug.Log($"[ReflectionHelper] Can't patch method, passed patchMethod is null!");
            return null;
        }
        
        MethodInfo methodInfo;        
        if(typeParams == null){
            //Without passing any type params
            methodInfo = classDef.GetMethod(
                methodName, 
                flags
            );
        } else {
            methodInfo = classDef.GetMethod(
                methodName, 
                flags,
                Type.DefaultBinder, 
                typeParams, 
                null
            );
        }
         
        if(methodInfo == null){
            if(!silent){
                UnityEngine.Debug.LogError($"[ReflectionHelper] Method was not patched, unable to find method info {methodName} (Report To XUnfairX!)");
            }
            return null;
        }

        s_harmonyInstance.Patch(methodInfo, prefix: new HarmonyMethod(patchMethod));
        UnityEngine.Debug.Log($"[ReflectionHelper] {classDef.Name}-{methodName} was patched with cheat replacement: {patchMethod.Name}");
        return TrackPatch(classDef, methodInfo, HarmonyPatchType.Prefix);
    }

    /// <summary>
    /// Applies a Harmony postfix patch to a game method.
    /// Postfix patches run after the original method and can read/modify return values.
    /// </summary>
    /// <param name="classDef">The class containing the method to patch.</param>
    /// <param name="methodName">The name of the method to patch.</param>
    /// <param name="patchMethod">The postfix method to apply.</param>
    /// <param name="flags">Binding flags to find the method.</param>
    /// <param name="typeParams">Optional parameter types for overload resolution.</param>
    /// <param name="silent">If true, suppresses error logging.</param>
    /// <returns>The patch tracker key if successful; otherwise null.</returns>
    public static string PatchMethodPostfix(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false){
        if(patchMethod == null){
            UnityEngine.Debug.Log($"[ReflectionHelper] Can't patch method (postfix), passed patchMethod is null!");
            return null;
        }
        
        MethodInfo methodInfo;        
        if(typeParams == null){
            methodInfo = classDef.GetMethod(methodName, flags);
        } else {
            methodInfo = classDef.GetMethod(methodName, flags, Type.DefaultBinder, typeParams, null);
        }
         
        if(methodInfo == null){
            if(!silent){
                UnityEngine.Debug.LogError($"[ReflectionHelper] Method was not patched (postfix), unable to find method info {methodName} (Report To XUnfairX!)");
            }
            return null;
        }

        s_harmonyInstance.Patch(methodInfo, postfix: new HarmonyMethod(patchMethod));
        UnityEngine.Debug.Log($"[ReflectionHelper] {classDef.Name}-{methodName} was patched (postfix) with: {patchMethod.Name}");
        return TrackPatch(classDef, methodInfo, HarmonyPatchType.Postfix);
    }

    /// <summary>
    /// Applies a Harmony finalizer patch to a game method.
    /// Finalizers run after the original method regardless of exceptions.
    /// </summary>
    /// <param name="classDef">The class containing the method to patch.</param>
    /// <param name="methodName">The name of the method to patch.</param>
    /// <param name="patchMethod">The finalizer method to apply.</param>
    /// <param name="flags">Binding flags to find the method.</param>
    /// <param name="typeParams">Optional parameter types for overload resolution.</param>
    /// <param name="silent">If true, suppresses error logging.</param>
    /// <returns>The patch tracker key if successful; otherwise null.</returns>
    public static string PatchMethodFinalizer(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false){
        if(patchMethod == null){
            UnityEngine.Debug.Log($"[ReflectionHelper] Can't patch method (finalizer), passed patchMethod is null!");
            return null;
        }
        
        MethodInfo methodInfo;        
        if(typeParams == null){
            methodInfo = classDef.GetMethod(methodName, flags);
        } else {
            methodInfo = classDef.GetMethod(methodName, flags, Type.DefaultBinder, typeParams, null);
        }
         
        if(methodInfo == null){
            if(!silent){
                UnityEngine.Debug.LogError($"[ReflectionHelper] Method was not patched (finalizer), unable to find method info {methodName} (Report To XUnfairX!)");
            }
            return null;
        }

        s_harmonyInstance.Patch(methodInfo, finalizer: new HarmonyMethod(patchMethod));
        UnityEngine.Debug.Log($"[ReflectionHelper] {classDef.Name}-{methodName} was patched (finalizer) with: {patchMethod.Name}");
        return TrackPatch(classDef, methodInfo, HarmonyPatchType.Finalizer);
    }

    /// <summary>
    /// Gets a static public method from the calling method's declaring type.
    /// Used internally for patch method discovery.
    /// </summary>
    public static MethodInfo GetMethodStaticPublic(string name){   
        UnityEngine.Debug.Log(GetCallingMethod().DeclaringType);
        return GetCallingMethod().DeclaringType.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
    }

    /// <summary>
    /// Gets a static public method from a specific type.
    /// </summary>
    /// <param name="type">The type to get the method from.</param>
    /// <param name="name">The method name.</param>
    /// <returns>The MethodInfo if found; otherwise null.</returns>
    public static MethodInfo GetMethodStaticPublic(Type type, string name){
        return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
    }

    /// <summary>
    /// Gets a static private method from the calling method's declaring type.
    /// </summary>
    public static MethodInfo GetMethodStaticPrivate(string name){        
        return GetCallingMethod().DeclaringType.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
    }

    /// <summary>
    /// Gets a static private method from a specific type.
    /// </summary>
    public static MethodInfo GetMethodStaticPrivate(Type type, string name){
        return type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
    }

    /// <summary>
    /// Finds all methods in the executing assembly that have a specific custom attribute.
    /// Used to discover cheat methods marked with [CheatDetails].
    /// </summary>
    /// <param name="annotationType">The attribute type to search for.</param>
    /// <returns>List of MethodInfo objects that have the attribute.</returns>
    public static List<MethodInfo> GetAllMethodsInAssemblyWithAnnotation(Type annotationType){
        List<MethodInfo> methods = new();
        Type[] executionTypes = Assembly.GetExecutingAssembly().GetTypes();
        
        foreach(Type innerType in executionTypes){
            MethodInfo[] innerMethodsStatic = innerType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo[] innerMethodsNonStatic = innerType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo[] combinedInnerMethods = CheatUtils.Concat(innerMethodsNonStatic, innerMethodsStatic);
            foreach(MethodInfo method in combinedInnerMethods){
                MethodInfo hasAttributeCustom = typeof(ReflectionHelper).GetMethod("HasAttribute", new Type[]{typeof(MethodInfo)})
                             .MakeGenericMethod(new Type[] { annotationType });
                object returnAttribute = hasAttributeCustom.Invoke(null, new object[]{method});
                if(returnAttribute != null){
                    methods.Add(method);
                }
            }
        }

        return methods;
    }
}