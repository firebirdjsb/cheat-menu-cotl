using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.2.0")]
	public class Plugin : BaseUnityPlugin
	{
		public void Awake()
		{
			new CheatConfig(base.Config);
			Debug.Log("[CheatMenu] Welcome to Cult of the Lamb: Cheaters Edition!");
			this._annotationHelper = new UnityAnnotationHelper();
			this._annotationHelper.RunAllInit();
			this.PatchDLCAuthentication();
			this.PatchVersionText();
			this._onGUIFn = this._annotationHelper.BuildRunAllOnGuiDelegate();
			this._updateFn = this._annotationHelper.BuildRunAllUpdateDelegate();
			Debug.Log("[CheatMenu] Patching and loading completed!");
		}

		public void OnDisable()
		{
			this._annotationHelper.RunAllUnload();
		}

		public void OnGUI()
		{
			this._onGUIFn();
		}

		public void Update()
		{
			this._updateFn();
		}

		private void PatchDLCAuthentication()
		{
			string[] array = new string[] { "AuthenticateCultistDLC", "AuthenticateHereticDLC", "AuthenticateSinfulDLC", "AuthenticatePilgrimDLC", "AuthenticateMajorDLC", "AuthenticatePrePurchaseDLC" };
			MethodInfo method = typeof(Plugin).GetMethod("Prefix_AuthenticateDLC_ReturnTrue", BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				Debug.LogError("[CheatMenu] Prefix_AuthenticateDLC_ReturnTrue method not found via reflection!");
				return;
			}
			int num = 0;
			foreach (string text in array)
			{
				try
				{
					if (typeof(GameManager).GetMethod(text, BindingFlags.Static | BindingFlags.Public) == null)
					{
						Debug.LogWarning("[CheatMenu] GameManager." + text + " not found (game version changed?)");
					}
					else if (ReflectionHelper.PatchMethodPrefix(typeof(GameManager), text, method, BindingFlags.Static | BindingFlags.Public, null, true) != null)
					{
						num++;
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[CheatMenu] Failed to patch " + text + ": " + ex.Message);
				}
			}
			Debug.Log(string.Format("[CheatMenu] DLC authentication: {0}/{1} methods patched", num, array.Length));
		}

		public static bool Prefix_AuthenticateDLC_ReturnTrue(ref bool __result)
		{
			__result = true;
			return false;
		}

		private void PatchVersionText()
		{
			try
			{
				MethodInfo method = typeof(Plugin).GetMethod("Prefix_VersionNumber_OnEnable", BindingFlags.Static | BindingFlags.Public);
				if (ReflectionHelper.PatchMethodPrefix(typeof(VersionNumber), "OnEnable", method, BindingFlags.Instance | BindingFlags.NonPublic, null, true) != null)
				{
					Debug.Log("[CheatMenu] ? VersionNumber.OnEnable patched");
				}
				else
				{
					Debug.LogWarning("[CheatMenu] VersionNumber.OnEnable patch failed (method not found)");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] VersionNumber patch failed: " + ex.Message);
			}
		}

		public static bool Prefix_VersionNumber_OnEnable(VersionNumber __instance)
		{
			try
			{
				object value = Traverse.Create(__instance).Field("Text").GetValue();
				if (value != null)
				{
					Traverse.Create(value).Property("text", null).SetValue(Application.version + " - Cheaters Edition");
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] Version text patch error: " + ex.Message);
			}
			return true;
		}

		private UnityAnnotationHelper _annotationHelper;

		private Action _updateFn;

		private Action _onGUIFn;
	}
}
