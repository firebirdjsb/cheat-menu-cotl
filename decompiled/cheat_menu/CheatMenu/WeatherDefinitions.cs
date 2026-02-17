using System;
using UnityEngine;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.WEATHER)]
	public class WeatherDefinitions : IDefinition
	{
		private static void SetWeather(WeatherSystemController.WeatherType weatherType, WeatherSystemController.WeatherStrength strength, string displayName)
		{
			try
			{
				if (WeatherSystemController.Instance != null)
				{
					WeatherSystemController.Instance.SetWeather(weatherType, strength, 0f, true, true);
					CultUtils.PlayNotification("Weather set to " + displayName + "!");
				}
				else
				{
					CultUtils.PlayNotification("Weather system not available!");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to set weather " + displayName + ": " + ex.Message);
				CultUtils.PlayNotification("Failed to set " + displayName + "!");
			}
		}

		[CheatDetails("Clear Weather", "Clear all weather effects", false, 0)]
		public static void WeatherClear()
		{
			try
			{
				if (WeatherSystemController.Instance != null)
				{
					WeatherSystemController.Instance.StopCurrentWeather(0f);
					CultUtils.PlayNotification("Weather cleared!");
				}
				else
				{
					CultUtils.PlayNotification("Weather system not available!");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to clear weather: " + ex.Message);
				CultUtils.PlayNotification("Failed to clear weather!");
			}
		}

		[CheatDetails("Rain (Light)", "Set weather to light rain", false, 10)]
		public static void WeatherRainLight()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Light, "Light Rain");
		}

		[CheatDetails("Rain (Heavy)", "Set weather to heavy rain", false, 11)]
		public static void WeatherRainHeavy()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Heavy, "Heavy Rain");
		}

		[CheatDetails("Wind", "Set weather to wind", false, 20)]
		public static void WeatherWindLight()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Windy, WeatherSystemController.WeatherStrength.Light, "Wind");
		}

		[CheatDetails("Snow (Dusting)", "Set weather to dusting snow", false, 30)]
		public static void WeatherSnowDusting()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Dusting, "Dusting Snow");
		}

		[CheatDetails("Snow (Light)", "Set weather to light snow", false, 31)]
		public static void WeatherSnowLight()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Light, "Light Snow");
		}

		[CheatDetails("Snow (Medium)", "Set weather to medium snow", false, 32)]
		public static void WeatherSnowMedium()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Medium, "Medium Snow");
		}

		[CheatDetails("Snow (Heavy)", "Set weather to heavy snow", false, 33)]
		public static void WeatherSnowHeavy()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Heavy, "Heavy Snow");
		}

		[CheatDetails("Blizzard", "Set weather to extreme blizzard", false, 34)]
		public static void WeatherBlizzard()
		{
			WeatherDefinitions.SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Extreme, "Blizzard");
		}

		[CheatDetails("Blood Moon", "Triggers the orange Blood Moon effect (spooky lighting and music)", false, 40)]
		public static void TriggerBloodMoon()
		{
			try
			{
				DataManager.Instance.LastHalloween = TimeManager.TotalElapsedGameTime;
				if (LocationManager._Instance != null)
				{
					LocationManager._Instance.EnableBloodMoon();
				}
				try
				{
					AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.blood_moon);
				}
				catch
				{
				}
				CultUtils.PlayNotification("Blood Moon activated!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to trigger Blood Moon: " + ex.Message);
				CultUtils.PlayNotification("Failed to trigger Blood Moon!");
			}
		}

		[CheatDetails("Disable Blood Moon", "Removes the Blood Moon effect and restores normal lighting", false, 41)]
		public static void DisableBloodMoon()
		{
			try
			{
				DataManager.Instance.LastHalloween = 0f;
				if (LocationManager._Instance != null)
				{
					LocationManager._Instance.DisableBloodMoon();
				}
				CultUtils.PlayNotification("Blood Moon disabled!");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Failed to disable Blood Moon: " + ex.Message);
				CultUtils.PlayNotification("Failed to disable Blood Moon!");
			}
		}

		[CheatDetails("Season: Spring", "Change current season to Spring", false, 50)]
		public static void SetSeasonSpring()
		{
			SeasonsManager.CurrentSeason = SeasonsManager.Season.Spring;
			if (WeatherSystemController.Instance != null)
			{
				WeatherSystemController.Instance.StopCurrentWeather(0f);
			}
			CultUtils.PlayNotification("Season set to Spring!");
		}

		[CheatDetails("Season: Winter", "Change current season to Winter", false, 51)]
		public static void SetSeasonWinter()
		{
			SeasonsManager.CurrentSeason = SeasonsManager.Season.Winter;
			CultUtils.PlayNotification("Season set to Winter!");
		}
	}
}
