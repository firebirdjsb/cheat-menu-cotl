using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheatMenu
{
	public static class GUIUtils
	{
		[Init]
		public static void Init()
		{
			string[] osinstalledFontNames = Font.GetOSInstalledFontNames();
			GUIUtils.s_uiFont = (new List<string>(osinstalledFontNames).Contains("Arial") ? Font.CreateDynamicFontFromOSFont("Arial", 16) : Font.CreateDynamicFontFromOSFont(osinstalledFontNames[0], 16));
		}

		[Unload]
		public static void Unload()
		{
			global::UnityEngine.Object.Destroy(GUIUtils.s_uiFont);
			GUIUtils.s_buttonStyle = null;
			GUIUtils.s_selectedButtonStyle = null;
			GUIUtils.s_titleBarStyle = null;
			GUIUtils.s_categoryButtonStyle = null;
		}

		public static GUIStyle GetGUIWindowStyle()
		{
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_DARK_RED, true),
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			return new GUIStyle
			{
				normal = guistyleState,
				active = guistyleState,
				alignment = 4,
				font = GUIUtils.s_uiFont,
				wordWrap = true,
				padding = new RectOffset(8, 8, 8, 8),
				border = new RectOffset(3, 3, 3, 3)
			};
		}

		public static GUIStyle GetGUILabelStyle(int width, float sizeModifier = 1f)
		{
			GUIStyleState guistyleState = new GUIStyleState
			{
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			return new GUIStyle
			{
				normal = guistyleState,
				active = guistyleState,
				alignment = 4,
				font = GUIUtils.s_uiFont,
				fontSize = (int)((float)(width / 20) * sizeModifier),
				wordWrap = true
			};
		}

		public static GUIStyle GetGUIPanelStyle(int width)
		{
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_BLACK, true),
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			return new GUIStyle
			{
				normal = guistyleState,
				onNormal = guistyleState,
				alignment = 4,
				font = GUIUtils.s_uiFont,
				fontSize = Mathf.Max(14, width / 20),
				wordWrap = true,
				padding = new RectOffset(12, 12, 12, 12),
				border = new RectOffset(2, 2, 2, 2)
			};
		}

		public static GUIStyle GetGUIButtonSelectedStyle()
		{
			if (GUIUtils.s_selectedButtonStyle != null)
			{
				return GUIUtils.s_selectedButtonStyle;
			}
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_DARK_PURPLE, true),
				textColor = GUIUtils.CULT_GOLD
			};
			GUIStyleState guistyleState2 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_GOLD,
				background = TextureHelper.GetSolidTexture(new Color(0.2f, 0.12f, 0.25f, 1f), true)
			};
			GUIStyleState guistyleState3 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_BONE_WHITE,
				background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.06f, 0.15f, 1f), true)
			};
			GUIStyle guistyle = new GUIStyle();
			guistyle.normal = guistyleState;
			guistyle.onNormal = guistyleState;
			guistyle.active = guistyleState3;
			guistyle.onActive = guistyleState3;
			guistyle.hover = guistyleState2;
			guistyle.onHover = guistyleState2;
			guistyle.font = GUIUtils.s_uiFont;
			guistyle.fontSize = 11;
			guistyle.alignment = 4;
			guistyle.padding = new RectOffset(6, 6, 5, 5);
			guistyle.fontStyle = 1;
			guistyle.border = new RectOffset(2, 2, 2, 2);
			GUIUtils.s_selectedButtonStyle = guistyle;
			return guistyle;
		}

		public static GUIStyle GetTitleBarStyle()
		{
			if (GUIUtils.s_titleBarStyle != null)
			{
				return GUIUtils.s_titleBarStyle;
			}
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_RED, true),
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			GUIStyle guistyle = new GUIStyle();
			guistyle.normal = guistyleState;
			guistyle.onNormal = guistyleState;
			guistyle.font = GUIUtils.s_uiFont;
			guistyle.fontSize = 14;
			guistyle.alignment = 4;
			guistyle.fontStyle = 1;
			guistyle.padding = new RectOffset(6, 6, 4, 4);
			guistyle.border = new RectOffset(2, 2, 2, 2);
			GUIUtils.s_titleBarStyle = guistyle;
			return guistyle;
		}

		public static GUIStyle GetCategoryButtonStyle()
		{
			if (GUIUtils.s_categoryButtonStyle != null)
			{
				return GUIUtils.s_categoryButtonStyle;
			}
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_SHADOW, true),
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			GUIStyleState guistyleState2 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_BONE_WHITE,
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_BLOOD_RED, true)
			};
			GUIStyleState guistyleState3 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_GOLD,
				background = TextureHelper.GetSolidTexture(new Color(0.08f, 0.03f, 0.05f, 1f), true)
			};
			GUIStyle guistyle = new GUIStyle();
			guistyle.normal = guistyleState;
			guistyle.onNormal = guistyleState;
			guistyle.active = guistyleState3;
			guistyle.onActive = guistyleState3;
			guistyle.hover = guistyleState2;
			guistyle.onHover = guistyleState2;
			guistyle.font = GUIUtils.s_uiFont;
			guistyle.fontSize = 12;
			guistyle.alignment = 4;
			guistyle.padding = new RectOffset(8, 8, 6, 6);
			guistyle.margin = new RectOffset(2, 2, 2, 2);
			guistyle.fontStyle = 1;
			guistyle.border = new RectOffset(2, 2, 2, 2);
			GUIUtils.s_categoryButtonStyle = guistyle;
			return guistyle;
		}

		public static GUIStyle GetGUIButtonStyle()
		{
			if (GUIUtils.s_buttonStyle != null)
			{
				return GUIUtils.s_buttonStyle;
			}
			GUIStyleState guistyleState = new GUIStyleState
			{
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_BLACK, true),
				textColor = GUIUtils.CULT_BONE_WHITE
			};
			GUIStyleState guistyleState2 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_BONE_WHITE,
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_RED, true)
			};
			GUIStyleState guistyleState3 = new GUIStyleState
			{
				textColor = GUIUtils.CULT_BONE_WHITE,
				background = TextureHelper.GetSolidTexture(GUIUtils.CULT_BLOOD_RED, true)
			};
			GUIStyle guistyle = new GUIStyle();
			guistyle.normal = guistyleState;
			guistyle.onNormal = guistyleState;
			guistyle.active = guistyleState3;
			guistyle.onActive = guistyleState3;
			guistyle.hover = guistyleState2;
			guistyle.onHover = guistyleState2;
			guistyle.font = GUIUtils.s_uiFont;
			guistyle.fontSize = 11;
			guistyle.alignment = 4;
			guistyle.padding = new RectOffset(6, 6, 5, 5);
			guistyle.margin = new RectOffset(2, 2, 2, 2);
			guistyle.border = new RectOffset(2, 2, 2, 2);
			GUIUtils.s_buttonStyle = guistyle;
			return guistyle;
		}

		public static Rect GetCenterRect(int width, int height)
		{
			float num = (float)((Screen.width - width) / 2);
			int num2 = (Screen.height - height) / 2;
			return new Rect(num, (float)num2, (float)width, (float)height);
		}

		public static void TitleBar(string titleText, float width)
		{
			string text = "+ " + titleText + " +";
			GUI.Box(new Rect(0f, 0f, width, 26f), text, GUIUtils.GetTitleBarStyle());
		}

		public static GUIUtils.WindowParams CustomWindow(GUIUtils.WindowParams windowParams, Action guiContents)
		{
			GUI.DragWindow(new Rect(0f, 0f, windowParams.ClientRect.width, 30f));
			GUIUtils.WindowParams newWindowParams = new GUIUtils.WindowParams(windowParams.Title, windowParams.ClientRect);
			if (newWindowParams.WindowID == null)
			{
				newWindowParams.WindowID = new int?(GUIManager.GetNextAvailableWindowID());
			}
			GUI.Window(newWindowParams.WindowID.Value, windowParams.ClientRect, delegate
			{
				GUIUtils.TitleBar(newWindowParams.Title, newWindowParams.ClientRect.width);
				guiContents();
			}, "", GUIUtils.GetGUIWindowStyle());
			return newWindowParams;
		}

		public static GUIUtils.ScrollableWindowParams CustomWindowScrollable(GUIUtils.ScrollableWindowParams scrollParams, Action guiContents)
		{
			if (scrollParams.WindowID == null)
			{
				scrollParams.WindowID = new int?(GUIManager.GetNextAvailableWindowID());
			}
			scrollParams.ClientRect = GUI.Window(scrollParams.WindowID.Value, scrollParams.ClientRect, delegate
			{
				GUIUtils.TitleBar(scrollParams.Title, scrollParams.ClientRect.width);
				Rect rect = new Rect(5f, 28f, scrollParams.ClientRect.width - 10f, scrollParams.ClientRect.height - 33f);
				scrollParams.ScrollPosition = GUI.BeginScrollView(rect, scrollParams.ScrollPosition, new Rect(0f, 0f, scrollParams.ClientRect.width - 35f, scrollParams.ScrollHeight), false, true);
				guiContents();
				GUI.EndScrollView();
				GUI.DragWindow(new Rect(0f, 0f, 10000f, 28f));
			}, "", GUIUtils.GetGUIWindowStyle());
			return scrollParams;
		}

		public static GUIUtils.ScrollableWindowParams CustomWindowScrollableLocked(GUIUtils.ScrollableWindowParams scrollParams, Action guiContents)
		{
			if (scrollParams.WindowID == null)
			{
				scrollParams.WindowID = new int?(GUIManager.GetNextAvailableWindowID());
			}
			Rect clientRect = scrollParams.ClientRect;
			GUI.Box(clientRect, "", GUIUtils.GetGUIWindowStyle());
			GUI.BeginGroup(clientRect);
			GUIUtils.TitleBar(scrollParams.Title, clientRect.width);
			Rect rect = new Rect(5f, 28f, clientRect.width - 10f, clientRect.height - 33f);
			scrollParams.ScrollPosition = GUI.BeginScrollView(rect, scrollParams.ScrollPosition, new Rect(0f, 0f, clientRect.width - 35f, scrollParams.ScrollHeight), false, true);
			guiContents();
			GUI.EndScrollView();
			GUI.EndGroup();
			return scrollParams;
		}

		public static int ToggleButton(Rect sizeAndPlacement, string buttonOneText, string buttonTwoText, int state = 0)
		{
			int num = (int)(sizeAndPlacement.width / 2f) - 2;
			bool flag = GUI.Button(new Rect(sizeAndPlacement.x, sizeAndPlacement.y, (float)num, sizeAndPlacement.height), buttonOneText, (state == 1) ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle());
			bool flag2 = GUI.Button(new Rect(sizeAndPlacement.x + (float)num + 4f, sizeAndPlacement.y, (float)num, sizeAndPlacement.height), buttonTwoText, (state == 2) ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle());
			if (!flag && !flag2)
			{
				return state;
			}
			if (!flag)
			{
				return 2;
			}
			return 1;
		}

		public static int GetButtonHeight()
		{
			return 30;
		}

		public static int GetButtonSpacing()
		{
			return 2;
		}

		public static bool Button(int y, int width, string buttonText)
		{
			return GUI.Button(new Rect(5f, (float)y, (float)width, (float)GUIUtils.GetButtonHeight()), buttonText, GUIUtils.GetGUIButtonStyle());
		}

		private static Font s_uiFont;

		private static GUIStyle s_buttonStyle = null;

		private static GUIStyle s_selectedButtonStyle = null;

		private static GUIStyle s_titleBarStyle = null;

		private static GUIStyle s_categoryButtonStyle = null;

		private static readonly Color CULT_DARK_RED = new Color(0.18f, 0.09f, 0.11f, 0.98f);

		private static readonly Color CULT_RED = new Color(0.65f, 0.13f, 0.18f, 1f);

		private static readonly Color CULT_BLOOD_RED = new Color(0.75f, 0.15f, 0.15f, 1f);

		private static readonly Color CULT_BONE_WHITE = new Color(0.95f, 0.92f, 0.88f, 1f);

		private static readonly Color CULT_DARK_PURPLE = new Color(0.15f, 0.08f, 0.18f, 1f);

		private static readonly Color CULT_GOLD = new Color(0.85f, 0.75f, 0.45f, 1f);

		private static readonly Color CULT_BLACK = new Color(0.08f, 0.08f, 0.1f, 0.95f);

		private static readonly Color CULT_SHADOW = new Color(0.12f, 0.05f, 0.08f, 1f);

		public struct WindowParams
		{
			public WindowParams(string title, Rect clientRect)
			{
				this.WindowID = null;
				this.ClientRect = clientRect;
				this.Title = title;
			}

			public Rect ClientRect;

			public string Title;

			public int? WindowID;
		}

		public class ScrollableWindowParams
		{
			public ScrollableWindowParams(string title, Rect clientRect, float? scrollHeight = null, Vector2? scrollPosition = null)
			{
				this.Title = title;
				this.ClientRect = clientRect;
				this.ScrollHeight = ((scrollHeight == null) ? (this.ClientRect.height * 2f) : scrollHeight.Value);
				this.ScrollPosition = ((scrollPosition == null) ? Vector2.zero : scrollPosition.Value);
			}

			public Vector2 ScrollPosition;

			public string Title;

			public Rect ClientRect;

			public float ScrollHeight;

			public int? WindowID;
		}
	}
}
