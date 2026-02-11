using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.WEATHER)]
public class WeatherDefinitions : IDefinition{

    private static bool TrySetWeatherViaCheatConsole(string methodName){
        try {
            MethodInfo method = typeof(CheatConsole).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if(method != null){
                method.Invoke(null, null);
                return true;
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"CheatConsole.{methodName} failed: {e.Message}");
        }
        return false;
    }

    private static bool TrySetWeatherViaBiomeManager(string weatherTypeName){
        try {
            if(BiomeBaseManager.Instance == null) return false;

            // Try SimSetWeather with enum parameter
            Type weatherEnumType = typeof(BiomeBaseManager).GetNestedType("WeatherType", BindingFlags.Public | BindingFlags.NonPublic);
            if(weatherEnumType == null){
                // Try as a top-level enum
                weatherEnumType = typeof(BiomeBaseManager).Assembly.GetType("WeatherType");
            }

            if(weatherEnumType != null && Enum.IsDefined(weatherEnumType, weatherTypeName)){
                object weatherVal = Enum.Parse(weatherEnumType, weatherTypeName);
                MethodInfo simMethod = typeof(BiomeBaseManager).GetMethod("SimSetWeather", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(simMethod != null){
                    simMethod.Invoke(BiomeBaseManager.Instance, new object[]{ weatherVal });
                    return true;
                }
            }

            // Fallback: try calling the method directly by name on the instance
            string[] methodNames = new[] { $"Set{weatherTypeName}", weatherTypeName, $"Sim{weatherTypeName}" };
            foreach(string name in methodNames){
                MethodInfo directMethod = typeof(BiomeBaseManager).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(directMethod != null && directMethod.GetParameters().Length == 0){
                    directMethod.Invoke(BiomeBaseManager.Instance, null);
                    return true;
                }
            }
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"BiomeBaseManager weather set failed: {e.Message}");
        }
        return false;
    }

    private static void SetWeather(string cheatConsoleName, string weatherTypeName, string displayName){
        try {
            if(TrySetWeatherViaCheatConsole(cheatConsoleName)){
                CultUtils.PlayNotification($"Weather set to {displayName}!");
                return;
            }
            if(TrySetWeatherViaBiomeManager(weatherTypeName)){
                CultUtils.PlayNotification($"Weather set to {displayName}!");
                return;
            }
            CultUtils.PlayNotification($"Failed to set {displayName} - weather system not found!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to set weather {displayName}: {e.Message}");
            CultUtils.PlayNotification($"Failed to set {displayName}!");
        }
    }

    [CheatDetails("Weather: Rain", "Set weather to raining")]
    public static void WeatherRain(){
        SetWeather("Rain", "Rain", "Rain");
    }

    [CheatDetails("Weather: Windy", "Set weather to windy")]
    public static void WeatherWindy(){
        SetWeather("Wind", "Windy", "Windy");
    }

    [CheatDetails("Weather: Clear", "Set weather to clear")]
    public static void WeatherClear(){
        SetWeather("ClearWeather", "Clear", "Clear");
    }

    [CheatDetails("Weather: Snow", "Set weather to snowing (winter)")]
    public static void WeatherSnow(){
        SetWeather("Snow", "Snow", "Snow");
    }
}