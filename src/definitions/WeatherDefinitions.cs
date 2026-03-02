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

    [CheatDetails("Wind", "Set weather to wind", sortOrder: 20)]
    public static void WeatherWindLight(){
        SetWeather(WeatherSystemController.WeatherType.Windy, WeatherSystemController.WeatherStrength.Light, "Wind");
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

    [CheatDetails("Blood Moon", "Triggers the orange Blood Moon effect (spooky lighting and music)", sortOrder: 40)]
    public static void TriggerBloodMoon(){
        try {
            DataManager.Instance.LastHalloween = TimeManager.TotalElapsedGameTime;
            if(LocationManager._Instance != null){
                LocationManager._Instance.EnableBloodMoon();
            }
            try {
                AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.blood_moon);
            } catch {}
            CultUtils.PlayNotification("Blood Moon activated!");
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to trigger Blood Moon: {e.Message}");
            CultUtils.PlayNotification("Failed to trigger Blood Moon!");
        }
    }

    [CheatDetails("Disable Blood Moon", "Fully removes the Blood Moon effect, resets music, and prevents re-trigger", sortOrder: 41)]
    public static void DisableBloodMoon(){
        try {
            // FollowerBrainStats.IsBloodMoon is true when
            //   TimeManager.TotalElapsedGameTime - DataManager.Instance.LastHalloween < 3600f
            // Push LastHalloween far enough into the past so IsBloodMoon returns false immediately.
            // LocationManager.ReEnableRitualEffects() checks IsBloodMoon and will re-enable the
            // blood moon visuals on scene loads if we don't clear this properly.
            DataManager.Instance.LastHalloween = TimeManager.TotalElapsedGameTime - 7200f;

            // Disable blood moon visuals and lighting via the game's own method
            if(LocationManager._Instance != null){
                LocationManager._Instance.DisableBloodMoon();
            }

            // Explicitly clear the halloweenLutActive flag on all LocationManagers
            // so ReEnableRitualEffects() doesn't restore the LUT on transitions
            try {
                foreach(var kvp in LocationManager.LocationManagers){
                    if(kvp.Value != null){
                        try { HarmonyLib.Traverse.Create(kvp.Value).Field("halloweenLutActive").SetValue(false); } catch {}
                    }
                }
            } catch {}

            // Force LightingManager out of blood moon override state
            try {
                if(LightingManager.Instance != null){
                    LightingManager.Instance.globalOverrideSettings = null;
                    LightingManager.Instance.inGlobalOverride = false;
                    LightingManager.Instance.isTODTransition = true;
                    LightingManager.Instance.lerpActive = true;
                    LightingManager.Instance.transitionDurationMultiplier = 1f;
                    LightingManager.Instance.UpdateLighting(false);
                }
            } catch {}

            // Reset music from blood moon theme back to normal (respect winter season)
            try {
                if(SeasonsManager.CurrentSeason == SeasonsManager.Season.Winter){
                    AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.winter_random);
                } else {
                    AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.StandardAmbience);
                }
            } catch {}

            // Clear any blood moon related boolean flags on DataManager via reflection
            try {
                var dm = DataManager.Instance;
                var dmType = dm.GetType();
                foreach(var f in dmType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)){
                    string nm = f.Name.ToLowerInvariant();
                    if(f.FieldType == typeof(bool) && (nm.Contains("bloodmoon") || nm.Contains("blood_moon") || nm.Contains("halloween"))){
                        try { f.SetValue(dm, false); } catch {}
                    }
                }
                foreach(var p in dmType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)){
                    string nm = p.Name.ToLowerInvariant();
                    if(p.PropertyType == typeof(bool) && p.CanWrite && (nm.Contains("bloodmoon") || nm.Contains("blood_moon") || nm.Contains("halloween"))){
                        try { p.SetValue(dm, false); } catch {}
                    }
                }
            } catch {}

            // Force clear weather effects that may be tied to the blood moon
            try {
                if(WeatherSystemController.Instance != null){
                    WeatherSystemController.Instance.StopCurrentWeather(0f);
                }
            } catch {}

            CultUtils.PlayNotification("Blood Moon fully disabled!");
        } catch(System.Exception e){
            UnityEngine.Debug.LogWarning($"Failed to disable Blood Moon: {e.Message}");
            CultUtils.PlayNotification("Failed to disable Blood Moon!");
        }
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