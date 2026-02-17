using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	public static class ReflectionHelper
	{
		[Init]
		[EnforceOrderFirst(9)]
		public static void Init()
		{
			ReflectionHelper.s_harmonyInstance = new Harmony(ReflectionHelper.HarmonyId);
			ReflectionHelper.s_patchTracker = new Dictionary<string, ReflectionHelper.PatchTrackerDetails>();
		}

		[Unload]
		public static void Unload()
		{
			ReflectionHelper.s_harmonyInstance.UnpatchSelf();
		}

		private static string GetPatchTrackerKey(Type classDef, string methodName)
		{
			return classDef.Name + "-" + methodName;
		}

		private static string TrackPatch(Type classDef, MethodInfo method, HarmonyPatchType patchType)
		{
			string patchTrackerKey = ReflectionHelper.GetPatchTrackerKey(classDef, method.Name);
			ReflectionHelper.s_patchTracker[patchTrackerKey] = new ReflectionHelper.PatchTrackerDetails(method, patchType);
			return patchTrackerKey;
		}

		public static MethodBase GetCallingMethod()
		{
			return new StackFrame(2).GetMethod();
		}

		public static MethodBase GetFirstMethodInHierarchyWithAnnotation<T>() where T : Attribute
		{
			foreach (StackFrame stackFrame in new StackTrace().GetFrames())
			{
				Debug.Log("frame: " + stackFrame.GetMethod().Name);
				if (ReflectionHelper.HasAttribute<T>(stackFrame.GetMethod()) != null)
				{
					return stackFrame.GetMethod();
				}
			}
			return null;
		}

		public static void UnpatchTracked(Type classDef, string methodName)
		{
			string patchTrackerKey = ReflectionHelper.GetPatchTrackerKey(classDef, methodName);
			ReflectionHelper.PatchTrackerDetails patchTrackerDetails;
			if (ReflectionHelper.s_patchTracker.TryGetValue(patchTrackerKey, out patchTrackerDetails))
			{
				ReflectionHelper.s_harmonyInstance.Unpatch(patchTrackerDetails.OriginalMethod, patchTrackerDetails.PatchType, ReflectionHelper.HarmonyId);
				Debug.Log(string.Concat(new string[] { "[ReflectionHelper] ", classDef.Name, "-", methodName, " was unpatched to original state." }));
			}
		}

		public static T GetAttributeOfTypeEnum<T>(Enum value)
		{
			object[] customAttributes = value.GetType().GetMember(value.ToString())[0].GetCustomAttributes(typeof(T), false);
			if (customAttributes.Length == 0)
			{
				return default(T);
			}
			return (T)((object)customAttributes[0]);
		}

		public static T HasAttribute<T>(Type type) where T : Attribute
		{
			return (T)((object)type.GetCustomAttribute(typeof(T)));
		}

		public static T HasAttribute<T>(MethodBase method) where T : Attribute
		{
			return (T)((object)method.GetCustomAttribute(typeof(T)));
		}

		public static T HasAttribute<T>(MethodInfo method) where T : Attribute
		{
			return (T)((object)method.GetCustomAttribute(typeof(T)));
		}

		public static List<Type> GetLoadableTypes(Assembly assembly)
		{
			List<Type> list;
			try
			{
				list = assembly.GetTypes().ToList<Type>();
			}
			catch (ReflectionTypeLoadException ex)
			{
				list = ex.Types.Where((Type t) => t != null).ToList<Type>();
			}
			return list;
		}

		public static string PatchMethodPrefix(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false)
		{
			if (patchMethod == null)
			{
				Debug.Log("[ReflectionHelper] Can't patch method, passed patchMethod is null!");
				return null;
			}
			MethodInfo methodInfo;
			if (typeParams == null)
			{
				methodInfo = classDef.GetMethod(methodName, flags);
			}
			else
			{
				methodInfo = classDef.GetMethod(methodName, flags, Type.DefaultBinder, typeParams, null);
			}
			if (methodInfo == null)
			{
				if (!silent)
				{
					Debug.LogError("[ReflectionHelper] Method was not patched, unable to find method info " + methodName + " (Report To XUnfairX!)");
				}
				else
				{
					Debug.LogWarning("[ReflectionHelper] Method was not patched, unable to find method info " + methodName + " (game version may have changed)");
				}
				return null;
			}
			ReflectionHelper.s_harmonyInstance.Patch(methodInfo, new HarmonyMethod(patchMethod), null, null, null, null);
			Debug.Log(string.Concat(new string[] { "[ReflectionHelper] ", classDef.Name, "-", methodName, " was patched with cheat replacement: ", patchMethod.Name }));
			return ReflectionHelper.TrackPatch(classDef, methodInfo, 1);
		}

		public static string PatchMethodPostfix(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false)
		{
			if (patchMethod == null)
			{
				Debug.Log("[ReflectionHelper] Can't patch method (postfix), passed patchMethod is null!");
				return null;
			}
			MethodInfo methodInfo;
			if (typeParams == null)
			{
				methodInfo = classDef.GetMethod(methodName, flags);
			}
			else
			{
				methodInfo = classDef.GetMethod(methodName, flags, Type.DefaultBinder, typeParams, null);
			}
			if (methodInfo == null)
			{
				if (!silent)
				{
					Debug.LogError("[ReflectionHelper] Method was not patched (postfix), unable to find method info " + methodName + " (Report To XUnfairX!)");
				}
				else
				{
					Debug.LogWarning("[ReflectionHelper] Method was not patched (postfix), unable to find method info " + methodName + " (game version may have changed)");
				}
				return null;
			}
			ReflectionHelper.s_harmonyInstance.Patch(methodInfo, null, new HarmonyMethod(patchMethod), null, null, null);
			Debug.Log(string.Concat(new string[] { "[ReflectionHelper] ", classDef.Name, "-", methodName, " was patched (postfix) with: ", patchMethod.Name }));
			return ReflectionHelper.TrackPatch(classDef, methodInfo, 2);
		}

		public static string PatchMethodFinalizer(Type classDef, string methodName, MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, Type[] typeParams = null, bool silent = false)
		{
			if (patchMethod == null)
			{
				Debug.Log("[ReflectionHelper] Can't patch method (finalizer), passed patchMethod is null!");
				return null;
			}
			MethodInfo methodInfo;
			if (typeParams == null)
			{
				methodInfo = classDef.GetMethod(methodName, flags);
			}
			else
			{
				methodInfo = classDef.GetMethod(methodName, flags, Type.DefaultBinder, typeParams, null);
			}
			if (methodInfo == null)
			{
				if (!silent)
				{
					Debug.LogError("[ReflectionHelper] Method was not patched (finalizer), unable to find method info " + methodName + " (Report To XUnfairX!)");
				}
				else
				{
					Debug.LogWarning("[ReflectionHelper] Method was not patched (finalizer), unable to find method info " + methodName + " (game version may have changed)");
				}
				return null;
			}
			ReflectionHelper.s_harmonyInstance.Patch(methodInfo, null, null, null, new HarmonyMethod(patchMethod), null);
			Debug.Log(string.Concat(new string[] { "[ReflectionHelper] ", classDef.Name, "-", methodName, " was patched (finalizer) with: ", patchMethod.Name }));
			return ReflectionHelper.TrackPatch(classDef, methodInfo, 4);
		}

		public static MethodInfo GetMethodStaticPublic(string name)
		{
			Debug.Log(ReflectionHelper.GetCallingMethod().DeclaringType);
			return ReflectionHelper.GetCallingMethod().DeclaringType.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
		}

		public static MethodInfo GetMethodStaticPublic(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
		}

		public static MethodInfo GetMethodStaticPrivate(string name)
		{
			return ReflectionHelper.GetCallingMethod().DeclaringType.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
		}

		public static MethodInfo GetMethodStaticPrivate(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
		}

		public static List<MethodInfo> GetAllMethodsInAssemblyWithAnnotation(Type annotationType)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in CheatUtils.Concat<MethodInfo>(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), methods))
				{
					if (typeof(ReflectionHelper).GetMethod("HasAttribute", new Type[] { typeof(MethodInfo) }).MakeGenericMethod(new Type[] { annotationType }).Invoke(null, new object[] { methodInfo }) != null)
					{
						list.Add(methodInfo);
					}
				}
			}
			return list;
		}

		private static readonly string HarmonyId = "org.xunfairx.cheat_menu";

		private static Harmony s_harmonyInstance;

		private static Dictionary<string, ReflectionHelper.PatchTrackerDetails> s_patchTracker;

		private class PatchTrackerDetails
		{
			public PatchTrackerDetails(MethodInfo originalMethod, HarmonyPatchType patchType)
			{
				this.OriginalMethod = originalMethod;
				this.PatchType = patchType;
			}

			public MethodInfo OriginalMethod;

			public HarmonyPatchType PatchType;
		}
	}
}
