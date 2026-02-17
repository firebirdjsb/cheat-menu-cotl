using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CheatMenu
{
	public static class GUIManager
	{
		[Init]
		public static void Init()
		{
			GUIManager.s_guiFunctions = new Dictionary<string, Action>();
		}

		public static Action[] GetAllGuiFunctions()
		{
			return GUIManager.s_guiFunctions.Values.ToArray<Action>();
		}

		public static int GetNextAvailableWindowID()
		{
			int num = GUIManager.s_nextAvailableWindowID;
			GUIManager.s_nextAvailableWindowID++;
			return num;
		}

		public static void ClearAllGuiBasedCheats()
		{
			foreach (string text in GUIManager.s_guiFunctions.Keys.ToArray<string>())
			{
				GUIManager.RemoveGuiFunction(text);
				FlagManager.SetFlagValue(text, false);
			}
		}

		public static void SetGuiWindowFunction(GUIUtils.WindowParams windowParams, Action innerContent)
		{
			GUIManager.SetGuiFunction(delegate
			{
				windowParams = GUIUtils.CustomWindow(windowParams, innerContent);
			});
		}

		public static string SetGuiWindowScrollableFunction(GUIUtils.ScrollableWindowParams windowParams, Action innerContent)
		{
			return GUIManager.SetGuiFunctionInternal(Definition.GetCheatFlagID(ReflectionHelper.GetCallingMethod()), delegate
			{
				windowParams = GUIUtils.CustomWindowScrollable(windowParams, innerContent);
			});
		}

		private static string SetGuiFunctionInternal(string flagId, Action guiFunction)
		{
			GUIManager.s_guiFunctions[flagId] = guiFunction;
			Debug.Log("[GUIManager] " + flagId + " has registered its GUI function");
			return flagId;
		}

		public static string SetGuiFunctionKey(string flagId, Action guiFunction)
		{
			GUIManager.SetGuiFunctionInternal(flagId, guiFunction);
			return flagId;
		}

		public static string SetGuiFunction(Action guiFunction)
		{
			string cheatFlagID = Definition.GetCheatFlagID(ReflectionHelper.GetCallingMethod());
			GUIManager.SetGuiFunctionInternal(cheatFlagID, guiFunction);
			return cheatFlagID;
		}

		private static void RemoveGuiFunction(string key)
		{
			if (GUIManager.s_guiFunctions.ContainsKey(key))
			{
				GUIManager.s_guiFunctions.Remove(key);
				Debug.Log("[GUIManager] " + key + " has removed its GUI function");
			}
		}

		public static void RemoveGuiFunction()
		{
			GUIManager.RemoveGuiFunction(Definition.GetCheatFlagID(ReflectionHelper.GetCallingMethod()));
		}

		public static void CloseGuiFunction(string key)
		{
			GUIManager.RemoveGuiFunction(key);
			FlagManager.SetFlagValue(key, false);
		}

		public static void RemoveGuiFunction(MethodInfo callingMethod)
		{
			GUIManager.RemoveGuiFunction(Definition.GetCheatFlagID(callingMethod));
		}

		private static Dictionary<string, Action> s_guiFunctions;

		private static int s_nextAvailableWindowID;
	}
}
