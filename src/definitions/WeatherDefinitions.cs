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

    [CheatDetails("Clear Weather", "Clear all weather effects", sortOrder: 0)]
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

    [CheatDetails("Rain (Light)", "Set weather to light rain", sortOrder: 10)]
    public static void WeatherRainLight(){
        SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Light, "Light Rain");
    }

    [CheatDetails("Rain (Heavy)", "Set weather to heavy rain", sortOrder: 11)]
    public static void WeatherRainHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Raining, WeatherSystemController.WeatherStrength.Heavy, "Heavy Rain");
    }

    [CheatDetails("Wind (Light)", "Set weather to light wind", sortOrder: 20)]
    public static void WeatherWindLight(){
        SetWeather(WeatherSystemController.WeatherType.Windy, WeatherSystemController.WeatherStrength.Light, "Light Wind");
    }

    [CheatDetails("Wind (Heavy)", "Set weather to heavy wind", sortOrder: 21)]
    public static void WeatherWindHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Windy, WeatherSystemController.WeatherStrength.Heavy, "Heavy Wind");
    }

    [CheatDetails("Snow (Dusting)", "Set weather to dusting snow", sortOrder: 30)]
    public static void WeatherSnowDusting(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Dusting, "Dusting Snow");
    }

    [CheatDetails("Snow (Light)", "Set weather to light snow", sortOrder: 31)]
    public static void WeatherSnowLight(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Light, "Light Snow");
    }

    [CheatDetails("Snow (Medium)", "Set weather to medium snow", sortOrder: 32)]
    public static void WeatherSnowMedium(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Medium, "Medium Snow");
    }

    [CheatDetails("Snow (Heavy)", "Set weather to heavy snow", sortOrder: 33)]
    public static void WeatherSnowHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Heavy, "Heavy Snow");
    }

    [CheatDetails("Blizzard", "Set weather to extreme blizzard", sortOrder: 34)]
    public static void WeatherBlizzard(){
        SetWeather(WeatherSystemController.WeatherType.Snowing, WeatherSystemController.WeatherStrength.Extreme, "Blizzard");
    }

    [CheatDetails("Heat (Light)", "Set weather to light heat", sortOrder: 40)]
    public static void WeatherHeatLight(){
        SetWeather(WeatherSystemController.WeatherType.Heat, WeatherSystemController.WeatherStrength.Light, "Light Heat");
    }

    [CheatDetails("Heat (Heavy)", "Set weather to heavy heat wave", sortOrder: 41)]
    public static void WeatherHeatHeavy(){
        SetWeather(WeatherSystemController.WeatherType.Heat, WeatherSystemController.WeatherStrength.Heavy, "Heavy Heat");
    }

    [CheatDetails("Season: Spring", "Change current season to Spring", sortOrder: 50)]
    public static void SetSeasonSpring(){
        SeasonsManager.CurrentSeason = SeasonsManager.Season.Spring;
        if(WeatherSystemController.Instance != null){
            WeatherSystemController.Instance.StopCurrentWeather(0f);
        }
        CultUtils.PlayNotification("Season set to Spring!");
    }

    [CheatDetails("Season: Winter", "Change current season to Winter", sortOrder: 51)]
    public static void SetSeasonWinter(){
        SeasonsManager.CurrentSeason = SeasonsManager.Season.Winter;
        CultUtils.PlayNotification("Season set to Winter!");
    }
}