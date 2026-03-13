using System.Reflection;
using System.Collections.Generic;
using System;

namespace CheatMenu;

/// <summary>
/// Extension methods for CheatCategoryEnum to handle string conversion.
/// Provides caching for efficient lookups.
/// </summary>
public static class CheatCategoryEnumExtensions {
    private static Dictionary<CheatCategoryEnum, string> s_forwardsCache = new();
    private static Dictionary<string, CheatCategoryEnum> s_backwardsCache = new();

    /// <summary>
    /// Initializes the category name caches.
    /// </summary>
    [Init]
    public static void Init(){
        s_forwardsCache = new Dictionary<CheatCategoryEnum, string>();
        s_backwardsCache = new Dictionary<string, CheatCategoryEnum>();
    }

    /// <summary>
    /// Gets the display name for a category enum value.
    /// Uses caching for performance.
    /// </summary>
    public static string GetCategoryName(this CheatCategoryEnum enumValue){
        if (s_forwardsCache.TryGetValue(enumValue, out string enumName))
        {
            return enumName;
        }

        StringEnum stringEnumValue = ReflectionHelper.GetAttributeOfTypeEnum<StringEnum>(enumValue);
        if(stringEnumValue == null){
            throw new Exception("Expected StringEnum on CheatCategory enum but not found!");
        }

        s_forwardsCache[enumValue] = stringEnumValue.Value;
        return stringEnumValue.Value;
    }

    /// <summary>
    /// Gets the enum value from its string name.
    /// Uses caching for performance.
    /// </summary>
    public static CheatCategoryEnum GetEnumFromName(string name){
        if (s_backwardsCache.TryGetValue(name, out CheatCategoryEnum enumValue))
        {
            return enumValue;
        }

        Type enumType = typeof(CheatCategoryEnum);
        FieldInfo[] fields = enumType.GetFields();
        foreach(var member in fields){
            StringEnum stringEnumAnnotation = (StringEnum)member.GetCustomAttribute(typeof(StringEnum));
            if(stringEnumAnnotation != null && stringEnumAnnotation.Value == name){
                CheatCategoryEnum enumVal = (CheatCategoryEnum)member.GetValue(null);
                s_backwardsCache[name] = enumVal;
                return enumVal;
            }
        }

        s_backwardsCache[name] = CheatCategoryEnum.NONE;
        return CheatCategoryEnum.NONE;
    }
}

/// <summary>
/// Enum defining all cheat menu categories.
/// Each value represents a major section of the cheat menu.
/// </summary>
public enum CheatCategoryEnum {
    /// <summary>No category (fallback).</summary>
    [StringEnum("NONE")]
    NONE,

    /// <summary>Health-related cheats.</summary>
    [StringEnum("Health")]
    HEALTH,

    /// <summary>Combat and dungeon cheats.</summary>
    [StringEnum("Combat")]
    COMBAT,

    /// <summary>Resource and item cheats.</summary>
    [StringEnum("Resources")]
    RESOURCE,

    /// <summary>Cult management cheats.</summary>
    [StringEnum("Cult")]
    CULT,

    /// <summary>Follower management cheats.</summary>
    [StringEnum("Follower")]
    FOLLOWER,

    /// <summary>Farming and ranching cheats.</summary>
    [StringEnum("Farming")]
    FARMING,

    /// <summary>Companion/dog cheats.</summary>
    [StringEnum("Companion")]
    COMPANION,

    /// <summary>Weather manipulation cheats.</summary>
    [StringEnum("Weather")]
    WEATHER,

    /// <summary>DLC-related cheats.</summary>
    [StringEnum("DLC")]
    DLC,

    /// <summary>Splitscreen cheats.</summary>
    [StringEnum("Splitscreen")]
    SPLITSCREEN,

    /// <summary>Miscellaneous cheats.</summary>
    [StringEnum("Misc")]
    MISC,

    /// <summary>Animation-related cheats.</summary>
    [StringEnum("Animation")]
    ANIMATION,

    /// <summary>Scenery/environment cheats.</summary>
    [StringEnum("Scenery")]
    SCENERY
}