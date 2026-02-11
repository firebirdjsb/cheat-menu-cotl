using System;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace CheatMenu;

[CheatCategory(CheatCategoryEnum.WEATHER)]
public class WeatherDefinitions : IDefinition{

    private static void SetWeather(string weatherName, string displayName){
        try {
            if(BiomeBaseManager.Instance == null){
                CultUtils.PlayNotification("Weather system not available!");
                return;
            }

            // Try calling SimSetWeather directly via Traverse
            Traverse biomeTraverse = Traverse.Create(BiomeBaseManager.Instance);

            // Look for Weather enum on BiomeBaseManager or in the assembly
            Type weatherEnumType = typeof(BiomeBaseManager).GetNestedType("Weather", BindingFlags.Public | BindingFlags.NonPublic);
            if(weatherEnumType == null){
                weatherEnumType = typeof(BiomeBaseManager).GetNestedType("WeatherType", BindingFlags.Public | BindingFlags.NonPublic);
            }
            if(weatherEnumType == null){
                weatherEnumType = typeof(BiomeBaseManager).Assembly.GetType("WeatherType");
            }
            if(weatherEnumType == null){
                weatherEnumType = typeof(BiomeBaseManager).Assembly.GetType("BiomeBaseManager+Weather");
            }

            if(weatherEnumType != null && Enum.IsDefined(weatherEnumType, weatherName)){
                object weatherVal = Enum.Parse(weatherEnumType, weatherName);

                // Try SetWeather first, then SimSetWeather
                string[] methodNames = new[]{ "SetWeather", "SimSetWeather" };
                foreach(string methodName in methodNames){
                    MethodInfo method = typeof(BiomeBaseManager).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if(method != null){
                        method.Invoke(BiomeBaseManager.Instance, new object[]{ weatherVal });
                        CultUtils.PlayNotification($"Weather set to {displayName}!");
                        return;
                    }
                }
            }

            // Fallback: try CheatConsole static methods
            string[] consoleMethods = new[]{ weatherName, $"Set{weatherName}", $"Weather{weatherName}" };
            foreach(string name in consoleMethods){
                MethodInfo method = typeof(CheatConsole).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if(method != null && method.GetParameters().Length == 0){
                    method.Invoke(null, null);
                    CultUtils.PlayNotification($"Weather set to {displayName}!");
                    return;
                }
            }

            CultUtils.PlayNotification($"Failed to set {displayName} - weather method not found!");
        } catch(Exception e){
            UnityEngine.Debug.LogWarning($"Failed to set weather {displayName}: {e.Message}");
            CultUtils.PlayNotification($"Failed to set {displayName}!");
        }
    }

    [CheatDetails("Weather: Rain", "Set weather to raining")]
    public static void WeatherRain(){
        SetWeather("Rain", "Rain");
    }

    [CheatDetails("Weather: Windy", "Set weather to windy")]
    public static void WeatherWindy(){
        SetWeather("Windy", "Windy");
    }

    [CheatDetails("Weather: Clear", "Set weather to clear")]
    public static void WeatherClear(){
        SetWeather("Clear", "Clear");
    }

    [CheatDetails("Weather: Blood Rain", "Set weather to blood rain")]
    public static void WeatherBloodRain(){
        SetWeather("BloodRain", "Blood Rain");
    }
}