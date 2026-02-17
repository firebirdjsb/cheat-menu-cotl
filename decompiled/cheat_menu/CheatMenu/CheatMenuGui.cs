using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CheatMenu
{
	public static class CheatMenuGui
	{
		[Init]
		public static void Init()
		{
			CheatMenuGui.CurrentButtonY = 0;
			CheatMenuGui.TotalWindowCalculatedHeight = 0;
			CheatMenuGui.s_scrollParams = new GUIUtils.ScrollableWindowParams("Cult Cheat Menu", CheatMenuGui.GetMenuRect(0f), null, null);
			CheatMenuGui.s_guiContent = DefinitionManager.BuildGUIContentFn();
			CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
			CheatMenuGui.s_selectedButtonIndex = 0;
			CheatMenuGui.s_totalButtons = 0;
			CheatMenuGui.s_currentButtonCounter = 0;
			CheatMenuGui.s_animationProgress = 0f;
			CheatMenuGui.s_animatingIn = false;
			CheatMenuGui.s_animatingOut = false;
			CheatMenuGui.s_pendingClose = false;
			CheatMenuGui.s_controllerSelectPressed = false;
			CheatMenuGui.s_needsScrollUpdate = false;
		}

		private static Rect GetMenuRect(float progress)
		{
			float num = 10f;
			float num2 = num;
			float num3 = (float)(Screen.height - CheatMenuGui.MENU_HEIGHT) - num;
			float num4 = num;
			float num5 = (float)(Screen.height + 50);
			float num6 = 1f - (1f - progress) * (1f - progress);
			float num7 = Mathf.Lerp(num4, num2, num6);
			float num8 = Mathf.Lerp(num5, num3, num6);
			return new Rect(num7, num8, (float)CheatMenuGui.MENU_WIDTH, (float)CheatMenuGui.MENU_HEIGHT);
		}

		public static bool IsWithinCategory()
		{
			return CheatMenuGui.CurrentCategory > CheatCategoryEnum.NONE;
		}

		public static bool IsWithinSpecificCategory(string categoryString)
		{
			CheatCategoryEnum enumFromName = CheatCategoryEnumExtensions.GetEnumFromName(categoryString);
			return CheatMenuGui.ShouldShowCategory(enumFromName) && enumFromName.Equals(CheatMenuGui.CurrentCategory);
		}

		private static bool IsButtonSelected(int buttonIndex)
		{
			return CheatConfig.Instance.ControllerSupport.Value && CheatMenuGui.s_selectedButtonIndex == buttonIndex;
		}

		private static GUIStyle GetButtonStyleWithHover(GUIStyle baseStyle, int buttonIndex)
		{
			if (!CheatMenuGui.IsButtonSelected(buttonIndex))
			{
				return baseStyle;
			}
			return new GUIStyle(baseStyle)
			{
				normal = new GUIStyleState
				{
					background = TextureHelper.GetSolidTexture(new Color(0.75f, 0.15f, 0.15f, 1f), true),
					textColor = new Color(0.95f, 0.92f, 0.88f, 1f)
				}
			};
		}

		private static bool ShouldShowCategory(CheatCategoryEnum category)
		{
			return CultUtils.IsInGame();
		}

		public static bool CategoryButton(string categoryText)
		{
			CheatCategoryEnum enumFromName = CheatCategoryEnumExtensions.GetEnumFromName(categoryText);
			if (!CheatMenuGui.ShouldShowCategory(enumFromName))
			{
				return false;
			}
			int buttonHeight = GUIUtils.GetButtonHeight();
			int buttonSpacing = GUIUtils.GetButtonSpacing();
			int num = CheatMenuGui.s_currentButtonCounter++;
			GUIStyle buttonStyleWithHover = CheatMenuGui.GetButtonStyleWithHover(GUIUtils.GetCategoryButtonStyle(), num);
			bool flag = GUI.Button(new Rect(5f, (float)CheatMenuGui.CurrentButtonY, (float)(CheatMenuGui.MENU_WIDTH - 10), (float)buttonHeight), ">> " + categoryText + " <<", buttonStyleWithHover);
			CheatMenuGui.TotalWindowCalculatedHeight += buttonHeight + buttonSpacing;
			CheatMenuGui.CurrentButtonY += buttonHeight + buttonSpacing;
			if (CheatMenuGui.IsButtonSelected(num) && CheatMenuGui.s_controllerSelectPressed)
			{
				flag = true;
			}
			if (flag)
			{
				if (enumFromName == CheatCategoryEnum.ANIMATION)
				{
					try
					{
						AnimationDefinitions.OpenAnimationBrowser();
					}
					catch
					{
					}
					CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
					CheatMenuGui.s_selectedButtonIndex = 0;
				}
				else
				{
					CheatMenuGui.CurrentCategory = enumFromName;
					CheatMenuGui.s_selectedButtonIndex = 0;
				}
			}
			return flag;
		}

		public static bool BackButton()
		{
			int buttonHeight = GUIUtils.GetButtonHeight();
			int buttonSpacing = GUIUtils.GetButtonSpacing();
			int num = CheatMenuGui.s_currentButtonCounter++;
			GUIStyle buttonStyleWithHover = CheatMenuGui.GetButtonStyleWithHover(GUIUtils.GetGUIButtonStyle(), num);
			bool flag = GUI.Button(new Rect(5f, (float)CheatMenuGui.CurrentButtonY, (float)(CheatMenuGui.MENU_WIDTH - 10), (float)buttonHeight), "< Back", buttonStyleWithHover);
			CheatMenuGui.TotalWindowCalculatedHeight += buttonHeight + buttonSpacing;
			CheatMenuGui.CurrentButtonY += buttonHeight + buttonSpacing;
			if (CheatMenuGui.IsButtonSelected(num) && CheatMenuGui.s_controllerSelectPressed)
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					GUIManager.CloseGuiFunction("AnimationBrowser");
				}
				catch
				{
				}
				CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
				CheatMenuGui.s_selectedButtonIndex = 0;
			}
			return flag;
		}

		public static bool Button(string text)
		{
			int buttonSpacing = GUIUtils.GetButtonSpacing();
			int num = CheatMenuGui.s_currentButtonCounter++;
			GUIStyle buttonStyleWithHover = CheatMenuGui.GetButtonStyleWithHover(GUIUtils.GetGUIButtonStyle(), num);
			bool flag = GUI.Button(new Rect(5f, (float)CheatMenuGui.CurrentButtonY, (float)(CheatMenuGui.MENU_WIDTH - 10), (float)GUIUtils.GetButtonHeight()), text, buttonStyleWithHover);
			if (CheatMenuGui.IsButtonSelected(num) && CheatMenuGui.s_controllerSelectPressed)
			{
				flag = true;
			}
			CheatMenuGui.CurrentButtonY += GUIUtils.GetButtonHeight() + buttonSpacing;
			CheatMenuGui.TotalWindowCalculatedHeight += GUIUtils.GetButtonHeight() + buttonSpacing;
			return flag;
		}

		public static bool ButtonWithFlag(string onText, string offText, string flagID)
		{
			bool flag = FlagManager.IsFlagEnabled(flagID);
			string text = (flag ? ("[ON] " + onText) : ("[OFF] " + offText));
			int num = CheatMenuGui.s_currentButtonCounter++;
			GUIStyle buttonStyleWithHover = CheatMenuGui.GetButtonStyleWithHover(flag ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle(), num);
			int buttonSpacing = GUIUtils.GetButtonSpacing();
			bool flag2 = GUI.Button(new Rect(5f, (float)CheatMenuGui.CurrentButtonY, (float)(CheatMenuGui.MENU_WIDTH - 10), (float)GUIUtils.GetButtonHeight()), text, buttonStyleWithHover);
			if (CheatMenuGui.IsButtonSelected(num) && CheatMenuGui.s_controllerSelectPressed)
			{
				flag2 = true;
			}
			if (flag2)
			{
				FlagManager.FlipFlagValue(flagID);
			}
			CheatMenuGui.CurrentButtonY += GUIUtils.GetButtonHeight() + buttonSpacing;
			CheatMenuGui.TotalWindowCalculatedHeight += GUIUtils.GetButtonHeight() + buttonSpacing;
			return flag2;
		}

		public static bool ButtonWithFlagS(string text, string flagID)
		{
			return CheatMenuGui.ButtonWithFlag(text + " (ON)", text + " (OFF)", flagID);
		}

		public static bool ButtonWithFlagP(string text, string flagID)
		{
			return CheatMenuGui.ButtonWithFlagS(text, flagID);
		}

		private static void ResetLayoutValues()
		{
			CheatMenuGui.CurrentButtonY = 0;
			CheatMenuGui.TotalWindowCalculatedHeight = 0;
			CheatMenuGui.s_totalButtons = CheatMenuGui.s_currentButtonCounter;
			CheatMenuGui.s_currentButtonCounter = 0;
			CheatMenuGui.s_controllerSelectPressed = false;
		}

		[OnGui]
		public static void OnGUI()
		{
			if (CheatMenuGui.GuiEnabled || CheatMenuGui.s_animatingOut)
			{
				CheatMenuGui.s_scrollParams.Title = "Cult Cheat Menu";
				if (CheatMenuGui.IsWithinCategory())
				{
					CheatMenuGui.s_scrollParams.Title = "Cult Cheat Menu - " + CheatMenuGui.CurrentCategory.GetCategoryName();
				}
				CheatMenuGui.s_scrollParams.ClientRect = CheatMenuGui.GetMenuRect(CheatMenuGui.s_animationProgress);
				GUIUtils.ScrollableWindowParams scrollableWindowParams = CheatMenuGui.s_scrollParams;
				Action action;
				if ((action = CheatMenuGui.<>O.<0>__CheatWindow) == null)
				{
					action = (CheatMenuGui.<>O.<0>__CheatWindow = new Action(CheatMenuGui.CheatWindow));
				}
				CheatMenuGui.s_scrollParams = GUIUtils.CustomWindowScrollableLocked(scrollableWindowParams, action);
				CheatMenuGui.DrawKeybindHints();
			}
			else if (CultUtils.IsInGame())
			{
				CheatMenuGui.DrawPersistentHint();
			}
			if (CheatMenuGui.GuiEnabled && !CheatMenuGui.InputBlockedForModal)
			{
				Action[] allGuiFunctions = GUIManager.GetAllGuiFunctions();
				if (allGuiFunctions.Length != 0)
				{
					foreach (Action action2 in allGuiFunctions)
					{
						try
						{
							action2();
						}
						catch
						{
						}
					}
				}
			}
		}

		private static void DrawKeybindHints()
		{
			int menu_WIDTH = CheatMenuGui.MENU_WIDTH;
			int num = 25;
			float num2 = 10f;
			float num3 = 1f - (1f - CheatMenuGui.s_animationProgress) * (1f - CheatMenuGui.s_animationProgress);
			int num4 = (int)num2;
			int num5 = (int)(Mathf.Lerp((float)(Screen.height + 50), (float)(Screen.height - CheatMenuGui.MENU_HEIGHT) - num2, num3) - (float)num - 2f);
			if (CheatMenuGui.InputBlockedForModal)
			{
				return;
			}
			string text = CheatConfig.Instance.GuiKeybind.Value.MainKey.ToString();
			string text2 = CheatConfig.Instance.BackCategory.Value.MainKey.ToString();
			string text3;
			if (CheatConfig.Instance.ControllerSupport.Value)
			{
				text3 = (CheatMenuGui.IsWithinCategory() ? string.Concat(new string[] { "[B/", text2, "] Back | [LeftStickClick/", text, "] Close | [Right Stick] Nav | [A] Select" }) : ("[LeftStickClick/" + text + "] Toggle | [Right Stick] Nav | [A] Select"));
			}
			else
			{
				text3 = (CheatMenuGui.IsWithinCategory() ? ("[" + text2 + "] Back | [ESC] Close") : ("[" + text + "] Toggle | [ESC] Close"));
			}
			GUIStyle guistyle = new GUIStyle(GUIUtils.GetGUILabelStyle(menu_WIDTH, 0.65f))
			{
				normal = new GUIStyleState
				{
					textColor = new Color(0.95f, 0.92f, 0.88f, 0.8f),
					background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.05f, 0.08f, 0.85f), true)
				},
				padding = new RectOffset(8, 8, 5, 5),
				fontSize = 11
			};
			GUI.Label(new Rect((float)num4, (float)num5, (float)menu_WIDTH, (float)num), text3, guistyle);
		}

		private static void DrawPersistentHint()
		{
			float num = 10f;
			int num2 = 22;
			float num3 = (float)((int)num);
			int num4 = Screen.height - num2 - (int)num;
			string text;
			if (CheatConfig.Instance.ControllerSupport.Value)
			{
				text = string.Format("[LeftStickClick/{0}] Open Cheat Menu", CheatConfig.Instance.GuiKeybind.Value.MainKey);
			}
			else
			{
				text = string.Format("[{0}] Open Cheat Menu", CheatConfig.Instance.GuiKeybind.Value.MainKey);
			}
			int num5 = 180;
			GUIStyle guistyle = new GUIStyle(GUIUtils.GetGUILabelStyle(num5, 0.6f))
			{
				normal = new GUIStyleState
				{
					textColor = new Color(0.95f, 0.92f, 0.88f, 0.5f),
					background = TextureHelper.GetSolidTexture(new Color(0.12f, 0.05f, 0.08f, 0.55f), true)
				},
				padding = new RectOffset(8, 8, 4, 4),
				fontSize = 10,
				alignment = 4
			};
			GUI.Label(new Rect(num3, (float)num4, (float)num5, (float)num2), text, guistyle);
		}

		private static void CheatWindow()
		{
			CheatMenuGui.s_guiContent();
			CheatMenuGui.s_scrollParams.ScrollHeight = (float)CheatMenuGui.TotalWindowCalculatedHeight;
			if (CheatMenuGui.s_needsScrollUpdate && CheatConfig.Instance.ControllerSupport.Value && CheatMenuGui.s_totalButtons > 0)
			{
				int buttonHeight = GUIUtils.GetButtonHeight();
				int buttonSpacing = GUIUtils.GetButtonSpacing();
				float num = (float)(CheatMenuGui.s_selectedButtonIndex * (buttonHeight + buttonSpacing));
				float num2 = (float)(CheatMenuGui.MENU_HEIGHT - 33);
				float y = CheatMenuGui.s_scrollParams.ScrollPosition.y;
				if (num < y)
				{
					CheatMenuGui.s_scrollParams.ScrollPosition.y = num;
				}
				else if (num + (float)buttonHeight > y + num2)
				{
					CheatMenuGui.s_scrollParams.ScrollPosition.y = num + (float)buttonHeight - num2;
				}
				CheatMenuGui.s_needsScrollUpdate = false;
			}
			CheatMenuGui.ResetLayoutValues();
		}

		private static void StartOpenAnimation()
		{
			CheatMenuGui.s_animatingIn = true;
			CheatMenuGui.s_animatingOut = false;
			CheatMenuGui.s_pendingClose = false;
			CheatMenuGui.s_animationProgress = 0f;
			CheatMenuGui.GuiEnabled = true;
			CheatMenuGui.s_scrollParams.WindowID = null;
		}

		public static void OpenMenuAtRoot()
		{
			CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
			CheatMenuGui.s_selectedButtonIndex = 0;
			CheatMenuGui.StartOpenAnimation();
		}

		private static void StartCloseAnimation()
		{
			CheatMenuGui.s_animatingOut = true;
			CheatMenuGui.s_animatingIn = false;
			CheatMenuGui.s_pendingClose = true;
		}

		[Update]
		public static void Update()
		{
			if (CheatMenuGui.s_animatingIn)
			{
				CheatMenuGui.s_animationProgress += Time.unscaledDeltaTime * CheatMenuGui.s_animationSpeed;
				if (CheatMenuGui.s_animationProgress >= 1f)
				{
					CheatMenuGui.s_animationProgress = 1f;
					CheatMenuGui.s_animatingIn = false;
				}
			}
			if (CheatMenuGui.s_animatingOut)
			{
				CheatMenuGui.s_animationProgress -= Time.unscaledDeltaTime * CheatMenuGui.s_animationSpeed;
				if (CheatMenuGui.s_animationProgress <= 0f)
				{
					CheatMenuGui.s_animationProgress = 0f;
					CheatMenuGui.s_animatingOut = false;
					if (CheatMenuGui.s_pendingClose)
					{
						CheatMenuGui.GuiEnabled = false;
						CheatMenuGui.s_pendingClose = false;
					}
				}
			}
			bool guiEnabled = CheatMenuGui.GuiEnabled;
			bool keyDown = Input.GetKeyDown(CheatConfig.Instance.GuiKeybind.Value.MainKey);
			bool flag = CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetToggleMenuPressed();
			if (!CheatMenuGui.InputBlockedForModal && (keyDown || flag))
			{
				if (!CheatMenuGui.GuiEnabled && !CheatMenuGui.s_animatingIn && CultUtils.IsInGame())
				{
					CheatMenuGui.StartOpenAnimation();
					CheatMenuGui.s_selectedButtonIndex = 0;
				}
				else if (CheatMenuGui.GuiEnabled && !CheatMenuGui.s_animatingOut)
				{
					CheatMenuGui.StartCloseAnimation();
				}
			}
			if (!CheatMenuGui.InputBlockedForModal && CheatMenuGui.GuiEnabled && CheatConfig.Instance.ControllerSupport.Value && !CheatMenuGui.s_animatingIn && !CheatMenuGui.s_animatingOut)
			{
				float unscaledTime = Time.unscaledTime;
				if (unscaledTime - CheatMenuGui.s_lastNavigationTime > CheatMenuGui.s_navigationDelay)
				{
					int navigationVertical = RewiredInputHelper.GetNavigationVertical();
					if (navigationVertical > 0)
					{
						if (CheatMenuGui.s_selectedButtonIndex > 0)
						{
							CheatMenuGui.s_selectedButtonIndex--;
						}
						else if (CheatMenuGui.s_totalButtons > 0)
						{
							CheatMenuGui.s_selectedButtonIndex = CheatMenuGui.s_totalButtons - 1;
						}
						CheatMenuGui.s_lastNavigationTime = unscaledTime;
						CheatMenuGui.s_needsScrollUpdate = true;
					}
					else if (navigationVertical < 0)
					{
						if (CheatMenuGui.s_totalButtons > 0 && CheatMenuGui.s_selectedButtonIndex < CheatMenuGui.s_totalButtons - 1)
						{
							CheatMenuGui.s_selectedButtonIndex++;
						}
						else
						{
							CheatMenuGui.s_selectedButtonIndex = 0;
						}
						CheatMenuGui.s_lastNavigationTime = unscaledTime;
						CheatMenuGui.s_needsScrollUpdate = true;
					}
				}
				if (RewiredInputHelper.GetSelectPressed() && !RewiredInputHelper.IsR3Held())
				{
					CheatMenuGui.s_controllerSelectPressed = true;
				}
			}
			if (CheatMenuGui.GuiEnabled && !CheatMenuGui.s_animatingOut && Input.GetKeyDown(CheatConfig.Instance.BackCategory.Value.MainKey) && CheatMenuGui.CurrentCategory != CheatCategoryEnum.NONE)
			{
				CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
				GUIManager.ClearAllGuiBasedCheats();
				CheatMenuGui.s_selectedButtonIndex = 0;
			}
			if (CheatMenuGui.GuiEnabled && !CheatMenuGui.s_animatingOut && CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetBackPressed())
			{
				if (CheatMenuGui.CurrentCategory != CheatCategoryEnum.NONE)
				{
					CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
					GUIManager.ClearAllGuiBasedCheats();
					CheatMenuGui.s_selectedButtonIndex = 0;
				}
				else
				{
					CheatMenuGui.StartCloseAnimation();
				}
			}
			if (CheatMenuGui.GuiEnabled && !CheatMenuGui.s_animatingOut && Input.GetKeyDown(KeyCode.Escape) && CheatConfig.Instance.CloseGuiOnEscape.Value)
			{
				CheatMenuGui.StartCloseAnimation();
			}
			if (guiEnabled && !CheatMenuGui.GuiEnabled)
			{
				GUIManager.ClearAllGuiBasedCheats();
				CheatMenuGui.s_selectedButtonIndex = 0;
			}
		}

		public static bool GuiEnabled = false;

		public static bool InputBlockedForModal = false;

		public static CheatCategoryEnum CurrentCategory = CheatCategoryEnum.NONE;

		public static int CurrentButtonY = 0;

		public static int TotalWindowCalculatedHeight = 0;

		private static int s_selectedButtonIndex = 0;

		private static int s_totalButtons = 0;

		private static int s_currentButtonCounter = 0;

		private static float s_lastNavigationTime = 0f;

		private static float s_navigationDelay = 0.15f;

		private static bool s_controllerSelectPressed = false;

		private static bool s_needsScrollUpdate = false;

		private static float s_animationProgress = 0f;

		private static float s_animationSpeed = 4.5f;

		private static bool s_animatingIn = false;

		private static bool s_animatingOut = false;

		private static bool s_pendingClose = false;

		private static Action s_guiContent;

		private static GUIUtils.ScrollableWindowParams s_scrollParams;

		private static readonly int MENU_WIDTH = 350;

		private static readonly int MENU_HEIGHT = 400;

		[CompilerGenerated]
		private static class <>O
		{
			public static Action <0>__CheatWindow;
		}
	}
}
