using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CheatMenu
{
	public class NotificationHandler
	{
		[OnGui]
		public static void OnGUI()
		{
			int num = Mathf.Min(Screen.width / 3, 400);
			int num2 = Mathf.Min(Screen.height / 8, 120);
			Rect rect = new Rect((float)((Screen.width - num) / 2), (float)(Screen.height - num2 - 80), (float)num, (float)num2);
			if (NotificationHandler.s_message != null)
			{
				float num3 = 1f;
				if (NotificationHandler.s_timer < 0.3f)
				{
					num3 = NotificationHandler.s_timer / 0.3f;
				}
				else if (NotificationHandler.s_timeToDisplay - NotificationHandler.s_timer < 0.5f)
				{
					num3 = (NotificationHandler.s_timeToDisplay - NotificationHandler.s_timer) / 0.5f;
				}
				Color color = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, num3);
				int num4 = 2;
				Rect rect2 = rect;
				GUI.WindowFunction windowFunction;
				if ((windowFunction = NotificationHandler.<>O.<0>__NotificationWindow) == null)
				{
					windowFunction = (NotificationHandler.<>O.<0>__NotificationWindow = new GUI.WindowFunction(NotificationHandler.NotificationWindow));
				}
				GUI.Window(num4, rect2, windowFunction, "", GUIUtils.GetGUIWindowStyle());
				GUI.color = color;
				NotificationHandler.s_timer += Time.deltaTime;
				if (NotificationHandler.s_timer >= NotificationHandler.s_timeToDisplay)
				{
					NotificationHandler.s_message = null;
					NotificationHandler.s_timer = 0f;
					NotificationHandler.s_timeToDisplay = 0f;
				}
			}
		}

		private static void NotificationWindow(int id)
		{
			int num = Mathf.Min(Screen.width / 3, 400);
			int num2 = Mathf.Min(Screen.height / 8, 120);
			GUI.Box(new Rect(0f, 0f, (float)num, (float)num2), "", GUIUtils.GetGUIPanelStyle(num));
			GUI.Label(new Rect(10f, 10f, (float)(num - 20), (float)(num2 - 20)), NotificationHandler.s_message, GUIUtils.GetGUILabelStyle(num, 0.9f));
		}

		public static void CreateNotification(string message, int displayTimeSeconds)
		{
			NotificationHandler.s_message = message;
			NotificationHandler.s_timeToDisplay = (float)displayTimeSeconds;
			NotificationHandler.s_timer = 0f;
		}

		private static string s_message;

		private static float s_timeToDisplay;

		private static float s_timer;

		[CompilerGenerated]
		private static class <>O
		{
			public static GUI.WindowFunction <0>__NotificationWindow;
		}
	}
}
