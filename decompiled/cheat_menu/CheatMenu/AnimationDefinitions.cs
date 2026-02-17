using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.ANIMATION)]
	public class AnimationDefinitions : IDefinition
	{
		[CheatDetails("Feast Ritual", "Perform the feast ritual - eat delicious food", false, 0)]
		public static void PlayFeastRitual()
		{
			try
			{
				if (PlayerFarming.Instance == null)
				{
					CultUtils.PlayNotification("Must be in game!");
				}
				else
				{
					PlayerFarming instance = PlayerFarming.Instance;
					instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
					if (instance.simpleSpineAnimator != null)
					{
						instance.simpleSpineAnimator.Animate("rituals/feast-start", 0, false);
						instance.simpleSpineAnimator.AddAnimate("rituals/feast-eat", 0, true, 0f);
						CultUtils.PlayNotification("Performing feast ritual!");
						GameManager.GetInstance().StartCoroutine(AnimationDefinitions.ResetPlayerAfterDelay(10f));
					}
					else
					{
						CultUtils.PlayNotification("Failed to perform ritual!");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] PlayFeastRitual error: " + ex.Message);
				CultUtils.PlayNotification("Feast ritual failed!");
			}
		}

		[Init]
		public static void Init()
		{
			AnimationDefinitions.s_animationGui = new GUIUtils.ScrollableWindowParams("Animation Browser", GUIUtils.GetCenterRect(650, 700), null, null);
			AnimationDefinitions.s_runtimeAnimations = new List<ValueTuple<string, List<string>>>();
		}

		public static Action GetAnimationGuiDelegate()
		{
			Action action;
			if ((action = AnimationDefinitions.<>O.<0>__AnimationGuiContents) == null)
			{
				action = (AnimationDefinitions.<>O.<0>__AnimationGuiContents = new Action(AnimationDefinitions.AnimationGuiContents));
			}
			return action;
		}

		public static void OpenAnimationBrowser()
		{
			CheatMenuGui.GuiEnabled = false;
			CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
			CheatMenuGui.InputBlockedForModal = true;
			GUIManager.ClearAllGuiBasedCheats();
			AnimationDefinitions.PopulateRuntimeAnimations();
			AnimationDefinitions.s_animationGuiOpen = true;
			AnimationDefinitions.s_animSelectedIndex = 0;
			AnimationDefinitions.s_animNeedsScrollUpdate = true;
		}

		public static void CloseAnimationBrowser()
		{
			AnimationDefinitions.s_animationGuiOpen = false;
			CheatMenuGui.InputBlockedForModal = false;
			CheatMenuGui.CurrentCategory = CheatCategoryEnum.NONE;
			try
			{
				CheatMenuGui.OpenMenuAtRoot();
			}
			catch
			{
				CheatMenuGui.GuiEnabled = true;
			}
		}

		[CheatDetails("Animation Menu", "Open runtime animation browser", "Close animation browser", "List and play all Spine animations discovered at runtime", true, 0)]
		public static void ToggleAnimationMenu(bool flag)
		{
			AnimationDefinitions.PopulateRuntimeAnimations();
			CultUtils.PlayNotification("Animation list refreshed");
		}

		private static void PopulateRuntimeAnimations()
		{
			AnimationDefinitions.s_runtimeAnimations.Clear();
			if (!CultUtils.IsInGame() || PlayerFarming.Instance == null)
			{
				List<string> list = new List<string> { "sit", "dance", "pray", "laugh", "cry", "celebrate", "idle" };
				AnimationDefinitions.s_runtimeAnimations.Add(new ValueTuple<string, List<string>>("Player", list));
				return;
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				SkeletonAnimation spine = PlayerFarming.Instance.Spine;
				SkeletonDataAsset skeletonDataAsset = ((spine != null) ? spine.skeletonDataAsset : null);
				if (skeletonDataAsset != null)
				{
					SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);
					if (skeletonData != null)
					{
						foreach (Animation animation in skeletonData.Animations)
						{
							if (animation != null && !string.IsNullOrEmpty(animation.Name))
							{
								hashSet.Add(animation.Name);
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
			if (hashSet.Count > 0)
			{
				List<string> list2 = new List<string>(hashSet);
				list2.Sort(StringComparer.OrdinalIgnoreCase);
				AnimationDefinitions.s_runtimeAnimations.Add(new ValueTuple<string, List<string>>("Player", list2));
				return;
			}
			List<string> list3 = new List<string>(new string[]
			{
				"sit", "dance", "bleat", "pray", "laugh", "cry", "victory", "shocked", "worship", "tired",
				"celebrate", "build", "sacrifice", "kiss", "eat", "feast", "ritual", "idle", "react", "reactions",
				"eat-react-bad", "kiss-follower"
			});
			AnimationDefinitions.s_runtimeAnimations.Add(new ValueTuple<string, List<string>>("Player (Whitelist)", list3));
		}

		private static void AnimationGuiContents()
		{
			if (!AnimationDefinitions.s_animationGuiOpen && !CheatMenuGui.IsWithinSpecificCategory("Animation"))
			{
				AnimationDefinitions.s_animSelectedIndex = 0;
				return;
			}
			if (AnimationDefinitions.s_runtimeAnimations == null || AnimationDefinitions.s_runtimeAnimations.Count == 0)
			{
				AnimationDefinitions.PopulateRuntimeAnimations();
			}
			float num = 10f;
			int num2 = 350;
			int num3 = 400;
			Rect rect = new Rect(num, (float)(Screen.height - num3) - num, (float)num2, (float)num3);
			GUI.BeginGroup(rect);
			GUI.Box(new Rect(0f, 0f, rect.width, rect.height), "", GUIUtils.GetGUIPanelStyle((int)rect.width));
			GUI.Label(new Rect(10f, 8f, rect.width - 20f, 22f), "Animations", GUIUtils.GetGUILabelStyle((int)rect.width, 0.9f));
			int num4 = 34;
			GUI.Label(new Rect(10f, (float)num4, rect.width - 20f, 20f), "Select an animation to play on the player (controller & mouse)", GUIUtils.GetGUILabelStyle((int)rect.width, 0.7f));
			num4 += 10;
			Rect rect2 = new Rect(10f, (float)num4, rect.width - 20f, rect.height - (float)num4 - 12f);
			List<ValueTuple<string, string, bool>> list = new List<ValueTuple<string, string, bool>>();
			for (int i = 0; i < AnimationDefinitions.s_runtimeAnimations.Count; i++)
			{
				ValueTuple<string, List<string>> valueTuple = AnimationDefinitions.s_runtimeAnimations[i];
				list.Add(new ValueTuple<string, string, bool>("-- " + valueTuple.Item1 + " --", null, true));
				List<string> list2 = new List<string>(new HashSet<string>(valueTuple.Item2, StringComparer.OrdinalIgnoreCase));
				list2.Sort(StringComparer.OrdinalIgnoreCase);
				foreach (string text in list2)
				{
					list.Add(new ValueTuple<string, string, bool>(text, text, false));
				}
			}
			float num5 = 0f;
			using (List<ValueTuple<string, string, bool>>.Enumerator enumerator2 = list.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.Item3)
					{
						num5 += 22f;
					}
					else
					{
						num5 += (float)(GUIUtils.GetButtonHeight() + GUIUtils.GetButtonSpacing());
					}
				}
			}
			num5 += 10f;
			AnimationDefinitions.s_animationGui.ScrollHeight = num5;
			AnimationDefinitions.s_animationGui.ScrollPosition = GUI.BeginScrollView(rect2, AnimationDefinitions.s_animationGui.ScrollPosition, new Rect(0f, 0f, rect2.width - 16f, AnimationDefinitions.s_animationGui.ScrollHeight));
			float num6 = 0f;
			List<int> list3 = new List<int>();
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j].Item3)
				{
					list3.Add(j);
				}
			}
			if (CheatConfig.Instance.ControllerSupport.Value && list3.Count > 0)
			{
				float unscaledTime = Time.unscaledTime;
				int navigationVertical = RewiredInputHelper.GetNavigationVertical();
				if (unscaledTime - AnimationDefinitions.s_animLastNavigationTime > AnimationDefinitions.s_animNavDelay)
				{
					if (navigationVertical > 0)
					{
						AnimationDefinitions.s_animSelectedIndex = Math.Max(0, AnimationDefinitions.s_animSelectedIndex - 1);
						AnimationDefinitions.s_animLastNavigationTime = unscaledTime;
						AnimationDefinitions.s_animNeedsScrollUpdate = true;
					}
					else if (navigationVertical < 0)
					{
						AnimationDefinitions.s_animSelectedIndex = Math.Min(list3.Count - 1, AnimationDefinitions.s_animSelectedIndex + 1);
						AnimationDefinitions.s_animLastNavigationTime = unscaledTime;
						AnimationDefinitions.s_animNeedsScrollUpdate = true;
					}
				}
				if (RewiredInputHelper.GetSelectPressed())
				{
					AnimationDefinitions.s_animControllerSelectPressed = true;
				}
			}
			int num7 = 0;
			float num8 = 0f;
			for (int k = 0; k < list.Count; k++)
			{
				ValueTuple<string, string, bool> valueTuple2 = list[k];
				if (valueTuple2.Item3)
				{
					GUI.Label(new Rect(0f, num6, rect2.width, 20f), valueTuple2.Item1, GUIUtils.GetGUILabelStyle((int)rect.width, 0.75f));
					num6 += 22f;
				}
				else
				{
					bool flag = CheatConfig.Instance.ControllerSupport.Value && num7 == AnimationDefinitions.s_animSelectedIndex;
					GUIStyle guistyle = (flag ? GUIUtils.GetGUIButtonSelectedStyle() : GUIUtils.GetGUIButtonStyle());
					bool flag2 = GUI.Button(new Rect(0f, num6, rect2.width - 4f, (float)GUIUtils.GetButtonHeight()), valueTuple2.Item1, guistyle);
					if (flag)
					{
						num8 = num6;
					}
					if (flag2 || (flag && AnimationDefinitions.s_animControllerSelectPressed))
					{
						AnimationDefinitions.PlayAnimationOnPlayer(valueTuple2.Item2);
						AnimationDefinitions.s_animControllerSelectPressed = false;
					}
					num6 += (float)(GUIUtils.GetButtonHeight() + GUIUtils.GetButtonSpacing());
					num7++;
				}
			}
			AnimationDefinitions.s_animControllerSelectPressed = false;
			AnimationDefinitions.s_animationGui.ScrollHeight = num6 + 10f;
			if (AnimationDefinitions.s_animNeedsScrollUpdate && CheatConfig.Instance.ControllerSupport.Value)
			{
				float num9 = (float)GUIUtils.GetButtonHeight();
				float height = rect2.height;
				float y = AnimationDefinitions.s_animationGui.ScrollPosition.y;
				if (num8 < y)
				{
					AnimationDefinitions.s_animationGui.ScrollPosition.y = num8;
				}
				else if (num8 + num9 > y + height)
				{
					AnimationDefinitions.s_animationGui.ScrollPosition.y = num8 + num9 - height;
				}
				AnimationDefinitions.s_animNeedsScrollUpdate = false;
			}
			GUI.EndScrollView();
			GUI.EndGroup();
		}

		[OnGui]
		private static void AnimationModalGui()
		{
			if (!AnimationDefinitions.s_animationGuiOpen)
			{
				return;
			}
			AnimationDefinitions.AnimationGuiContents();
			if ((CheatConfig.Instance.ControllerSupport.Value && RewiredInputHelper.GetBackPressed()) || Input.GetKeyDown(KeyCode.Escape))
			{
				AnimationDefinitions.CloseAnimationBrowser();
			}
		}

		private static void PlayAnimationOnPlayer(string animName)
		{
			try
			{
				if (PlayerFarming.Instance == null)
				{
					CultUtils.PlayNotification("Must be in game to play animations");
				}
				else
				{
					PlayerFarming instance = PlayerFarming.Instance;
					instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
					if (instance.simpleSpineAnimator != null)
					{
						instance.simpleSpineAnimator.Animate(animName, 0, false);
						instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
						GameManager.GetInstance().StartCoroutine(AnimationDefinitions.ResetPlayerAfterDelay(4f));
						CultUtils.PlayNotification("Playing: " + animName);
					}
					else
					{
						CultUtils.PlayNotification("Player animator not available");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[CheatMenu] PlayAnimationOnPlayer failed: " + ex.Message);
				CultUtils.PlayNotification("Failed to play animation");
			}
		}

		private static IEnumerator ResetPlayerAfterDelay(float delaySeconds)
		{
			AnimationDefinitions.<ResetPlayerAfterDelay>d__18 <ResetPlayerAfterDelay>d__ = new AnimationDefinitions.<ResetPlayerAfterDelay>d__18(0);
			<ResetPlayerAfterDelay>d__.delaySeconds = delaySeconds;
			return <ResetPlayerAfterDelay>d__;
		}

		private static GUIUtils.ScrollableWindowParams s_animationGui;

		[TupleElementNames(new string[] { "source", "anims" })]
		private static List<ValueTuple<string, List<string>>> s_runtimeAnimations;

		private static bool s_animationGuiOpen = false;

		private static int s_animSelectedIndex = 0;

		private static float s_animLastNavigationTime = 0f;

		private static float s_animNavDelay = 0.15f;

		private static bool s_animControllerSelectPressed = false;

		private static bool s_animNeedsScrollUpdate = false;

		[CompilerGenerated]
		private static class <>O
		{
			public static Action <0>__AnimationGuiContents;
		}
	}
}
