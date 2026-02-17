using System;
using Rewired;
using UnityEngine;

namespace CheatMenu
{
	public static class RewiredInputHelper
	{
		public static bool ShouldSuppressR3
		{
			get
			{
				return Time.unscaledTime < RewiredInputHelper.s_r3SuppressUntil;
			}
		}

		[Init]
		public static void Init()
		{
			RewiredInputHelper.s_initialized = false;
			RewiredInputHelper.s_player = null;
			RewiredInputHelper.s_r3SuppressUntil = 0f;
		}

		private static Player GetPlayer()
		{
			if (RewiredInputHelper.s_player != null)
			{
				return RewiredInputHelper.s_player;
			}
			try
			{
				if (!ReInput.isReady)
				{
					return null;
				}
				RewiredInputHelper.s_player = ReInput.players.GetPlayer(0);
				if (RewiredInputHelper.s_player != null && !RewiredInputHelper.s_initialized)
				{
					RewiredInputHelper.s_initialized = true;
					Debug.Log("[CheatMenu] Rewired player 0 acquired for controller input");
				}
			}
			catch
			{
				RewiredInputHelper.s_player = null;
			}
			return RewiredInputHelper.s_player;
		}

		public static bool IsReady
		{
			get
			{
				return RewiredInputHelper.GetPlayer() != null;
			}
		}

		public static bool IsControllerConnected()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					return player.controllers.joystickCount > 0;
				}
				catch
				{
				}
			}
			string[] joystickNames = Input.GetJoystickNames();
			if (joystickNames != null)
			{
				string[] array = joystickNames;
				for (int i = 0; i < array.Length; i++)
				{
					if (!string.IsNullOrEmpty(array[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int GetNavigationVertical()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					float num = 0f;
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.axisCount > 3)
						{
							float axisRaw = joystick.GetAxisRaw(3);
							if (Mathf.Abs(axisRaw) > Mathf.Abs(num))
							{
								num = axisRaw;
							}
						}
					}
					if (num > 0.5f)
					{
						return 1;
					}
					if (num < -0.5f)
					{
						return -1;
					}
				}
				catch
				{
				}
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				return 1;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				return -1;
			}
			return 0;
		}

		public static int GetNavigationHorizontal()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					float num = 0f;
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.axisCount > 2)
						{
							float axisRaw = joystick.GetAxisRaw(2);
							if (Mathf.Abs(axisRaw) > Mathf.Abs(num))
							{
								num = axisRaw;
							}
						}
					}
					if (num > 0.5f)
					{
						return 1;
					}
					if (num < -0.5f)
					{
						return -1;
					}
				}
				catch
				{
				}
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				return 1;
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				return -1;
			}
			return 0;
		}

		public static bool GetSelectPressed()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.buttonCount > 0 && joystick.GetButtonDown(0))
						{
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
			return false;
		}

		public static bool GetBackPressed()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.buttonCount > 1 && joystick.GetButtonDown(1))
						{
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
			return false;
		}

		public static bool GetMenuPressed()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.buttonCount > 7 && joystick.GetButtonDown(7))
						{
							return true;
						}
						if (joystick.buttonCount > 6 && joystick.GetButtonDown(6))
						{
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
			return false;
		}

		public static bool GetToggleMenuPressed()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.buttonCount > 9 && joystick.GetButtonDown(9))
						{
							RewiredInputHelper.s_r3SuppressUntil = Time.unscaledTime + RewiredInputHelper.R3_SUPPRESS_DURATION;
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
			return false;
		}

		public static bool IsR3Held()
		{
			Player player = RewiredInputHelper.GetPlayer();
			if (player != null)
			{
				try
				{
					foreach (Joystick joystick in player.controllers.Joysticks)
					{
						if (joystick.buttonCount > 9 && joystick.GetButton(9))
						{
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
			return false;
		}

		private static Player s_player;

		private static bool s_initialized = false;

		private static float s_r3SuppressUntil = 0f;

		private static readonly float R3_SUPPRESS_DURATION = 0.3f;
	}
}
