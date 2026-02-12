using System;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.WEATHER)]
public class WeatherDefinitions : IDefinition{

    private static void SetWeather(WeatherSystemController.WeatherType weatherType, WeatherSystemController.WeatherStrength strength, string displayName){
        try {
            if(WeatherSystemController.Instance != null){
                WeatherSystemController.Instance.SetWeather(weatherType, strength, 0f, true, true);
                CultUtils.PlayNotification($"Weather set to {displayName}!");
            } else {
                CultUtils.PlayNotification("Weather system not available!");
            }
        } catch(Exception e){
            Debug.LogWarning($"Failed to set weather {displayName}: {e.Message}");
            CultUtils.PlayNotification($"Failed to set {displayName}!");
        }
    }

    [CheatDetails("Weather: Rain (Light)", "Set weather to light rain")]
    public static void WeatherRainLight(){
        SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Light, "Light Rain");
    }

    [CheatDetails("Weather: Rain (Heavy)", "Set weather to heavy rain")]
    public static void WeatherRainHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Heavy, "Heavy Rain");
    }

    [CheatDetails("Weather: Windy", "Set weather to windy")]
    public static void WeatherWindy(){
        SetWeather(WeatherSystemController.WeatherType.Windy, WeatherSystemController.WeatherStrength.Medium, "Windy");
    }

    [CheatDetails("Weather: Snow (Dusting)", "Set weather to dusting snow")]
    public static void WeatherSnowDusting(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Dusting, "Dusting Snow");
    }

    [CheatDetails("Weather: Snow (Light)", "Set weather to light snow")]
    public static void WeatherSnowLight(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Light, "Light Snow");
    }

    [CheatDetails("Weather: Snow (Medium)", "Set weather to medium snow")]
    public static void WeatherSnowMedium(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Medium, "Medium Snow");
    }

    [CheatDetails("Weather: Snow (Heavy)", "Set weather to heavy snow")]
    public static void WeatherSnowHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Heavy, "Heavy Snow");
    }

    [CheatDetails("Weather: Blizzard", "Set weather to extreme blizzard")]
    public static void WeatherBlizzard(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Extreme, "Blizzard");
    }

    [CheatDetails("Weather: Heat", "Set weather to heat wave")]
    public static void WeatherHeat(){
        SetWeather(WeatherSystemController.WeatherType.Heat, WeatherSystemController.WeatherStrength.Medium, "Heat");
    }

    [CheatDetails("Weather: Clear", "Clear all weather effects")]
    public static void WeatherClear(){
        try {
            if(WeatherSystemController.Instance != null){
                WeatherSystemController.Instance.StopCurrentWeather(0f);
                CultUtils.PlayNotification("Weather cleared!");
            } else {
                CultUtils.PlayNotification("Weather system not available!");
            }
        } catch(Exception e){
            Debug.LogWarning($"Failed to clear weather: {e.Message}");
            CultUtils.PlayNotification("Failed to clear weather!");
        }
    }

    [CheatDetails("Season: Spring", "Change current season to Spring")]
    public static void SetSeasonSpring(){
        SeasonsManager.CurrentSeason = SeasonsManager.Season.Spring;
        if(WeatherSystemController.Instance != null){
            WeatherSystemController.Instance.StopCurrentWeather(0f);
        }
        CultUtils.PlayNotification("Season set to Spring!");
    }

    [CheatDetails("Season: Winter", "Change current season to Winter")]
    public static void SetSeasonWinter(){
        SeasonsManager.CurrentSeason = SeasonsManager.Season.Winter;
        CultUtils.PlayNotification("Season set to Winter!");
    }
}