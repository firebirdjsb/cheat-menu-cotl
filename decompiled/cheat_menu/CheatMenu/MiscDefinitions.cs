using System;
using HarmonyLib;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.MISC)]
	public class MiscDefinitions : IDefinition
	{
		[CheatDetails("Noclip", "Noclip (OFF)", "Noclip (ON)", "Collide with nothing!", true, 0)]
		public static void Noclip()
		{
			Traverse.Create(typeof(CheatConsole)).Method("ToggleNoClip", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("Noclip toggled!");
		}

		[CheatDetails("FPS Debug", "FPS Debug (OFF)", "FPS Debug (ON)", "Displays the built-in FPS Debug menu", true, 0)]
		public static void FPSDebug()
		{
			Traverse.Create(typeof(CheatConsole)).Method("FPS", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("FPS debug toggled!");
		}

		[CheatDetails("Follower Debug", "Follower Debug (OFF)", "Follower Debug (ON)", "Shows Follower Debug Information", true, 0)]
		public static void FollowerDebug()
		{
			Traverse.Create(typeof(CheatConsole)).Method("FollowerDebug", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("Follower debug toggled!");
		}

		[CheatDetails("Structure Debug", "Structure Debug (OFF)", "Structure Debug (ON)", "Shows Structure Debug Information", true, 0)]
		public static void StructureDebug()
		{
			Traverse.Create(typeof(CheatConsole)).Method("StructureDebug", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("Structure debug toggled!");
		}

		[CheatDetails("Hide/Show UI", "Hide UI", "Show UI", "Show/Hide the UI of the game", true, 0)]
		public static void ShowUI(bool flag)
		{
			if (flag)
			{
				CheatConsole.HideUI();
				CultUtils.PlayNotification("UI hidden!");
				return;
			}
			CheatConsole.ShowUI();
			CultUtils.PlayNotification("UI shown!");
		}

		[CheatDetails("Skip Hour", "Skip an hour of game time", false, 0)]
		public static void SkipHour()
		{
			Traverse.Create(typeof(CheatConsole)).Method("SkipHour", Array.Empty<object>()).GetValue();
			CultUtils.PlayNotification("Skipped 1 hour!");
		}

		[CheatDetails("Skip Day", "Skip a full day of game time", false, 0)]
		public static void SkipDay()
		{
			for (int i = 0; i < 24; i++)
			{
				Traverse.Create(typeof(CheatConsole)).Method("SkipHour", Array.Empty<object>()).GetValue();
			}
			CultUtils.PlayNotification("Skipped 1 day!");
		}

		[CheatDetails("Complete All Quests", "Completes all active quests and objectives", false, 0)]
		public static void CompleteAllQuests()
		{
			CultUtils.CompleteAllQuests();
			CultUtils.PlayNotification("All quests completed!");
		}

		[CheatDetails("Game Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the game speed using time scale", true, 0)]
		public static void GameSpeedDouble(bool flag)
		{
			Time.timeScale = (flag ? 2f : 1f);
			CultUtils.PlayNotification(flag ? "Game speed x2!" : "Game speed normal!");
		}

		[CheatDetails("Game Speed x4", "Speed x4 (OFF)", "Speed x4 (ON)", "Quadruples the game speed using time scale", true, 0)]
		public static void GameSpeedQuadruple(bool flag)
		{
			Time.timeScale = (flag ? 4f : 1f);
			CultUtils.PlayNotification(flag ? "Game speed x4!" : "Game speed normal!");
		}

		[CheatDetails("Pause Simulation", "Pause Sim (OFF)", "Pause Sim (ON)", "Pause game simulation (followers stop acting)", true, 0)]
		public static void PauseSimulation(bool flag)
		{
			if (flag)
			{
				SimulationManager.Pause();
				CultUtils.PlayNotification("Simulation paused!");
				return;
			}
			SimulationManager.UnPause();
			CultUtils.PlayNotification("Simulation unpaused!");
		}

		[CheatDetails("Skip To Night", "Skips time forward to the night phase", false, 0)]
		public static void SkipToNight()
		{
			try
			{
				CheatConsole.SkipToPhase(DayPhase.Night);
				CultUtils.PlayNotification("Skipped to night!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to skip to night: " + ex.Message);
				CultUtils.PlayNotification("Failed to skip to night!");
			}
		}

		[CheatDetails("Skip To Dawn", "Skips time forward to the dawn phase", false, 0)]
		public static void SkipToDawn()
		{
			try
			{
				CheatConsole.SkipToPhase(DayPhase.Dawn);
				CultUtils.PlayNotification("Skipped to dawn!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to skip to dawn: " + ex.Message);
				CultUtils.PlayNotification("Failed to skip to dawn!");
			}
		}

		[CheatDetails("End Knucklebones", "Ends the current knucklebones game", false, 0)]
		public static void EndKnucklebones()
		{
			try
			{
				Traverse.Create(typeof(CheatConsole)).Method("EndKnucklebones", Array.Empty<object>()).GetValue();
				CultUtils.PlayNotification("Knucklebones ended!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to end knucklebones: " + ex.Message);
				CultUtils.PlayNotification("Not in a knucklebones game!");
			}
		}

		[CheatDetails("Stop Time In Crusade", "Stop Time (OFF)", "Stop Time (ON)", "Stops base time from passing while in a crusade", true, 0)]
		public static void StopTimeInCrusade(bool flag)
		{
			try
			{
				SettingsManager.Settings.Accessibility.StopTimeInCrusade = flag;
				CultUtils.PlayNotification(flag ? "Time stops during crusades!" : "Time passes during crusades!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to toggle crusade time stop: " + ex.Message);
				CultUtils.PlayNotification("Failed to toggle crusade time!");
			}
		}

		[CheatDetails("Player Speed x2", "Speed x2 (OFF)", "Speed x2 (ON)", "Doubles the player's movement speed without affecting the world", true, 0)]
		public static void PlayerSpeedDouble(bool flag)
		{
			try
			{
				if (PlayerFarming.Instance != null)
				{
					PlayerController playerController = PlayerFarming.Instance.playerController;
					if (flag)
					{
						if (MiscDefinitions.s_originalRunSpeed < 0f)
						{
							MiscDefinitions.s_originalRunSpeed = playerController.DefaultRunSpeed;
						}
						playerController.RunSpeed = MiscDefinitions.s_originalRunSpeed * 2f;
						playerController.DefaultRunSpeed = MiscDefinitions.s_originalRunSpeed * 2f;
					}
					else
					{
						if (MiscDefinitions.s_originalRunSpeed >= 0f)
						{
							playerController.RunSpeed = MiscDefinitions.s_originalRunSpeed;
							playerController.DefaultRunSpeed = MiscDefinitions.s_originalRunSpeed;
						}
						MiscDefinitions.s_originalRunSpeed = -1f;
					}
				}
				CultUtils.PlayNotification(flag ? "Player speed x2!" : "Player speed normal!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to set player speed: " + ex.Message);
				CultUtils.PlayNotification("Failed to toggle player speed!");
			}
		}

		private static float s_originalRunSpeed = -1f;
	}
}
