using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CheatMenu;

/// <summary>
/// Auto-generates definitions from game assembly enums at startup.
/// Parses enums like InventoryItem.ITEM_TYPE, FollowerTrait, DoctrineType, etc.
/// and creates definition tables that can be used by the cheat menu.
/// </summary>
public static class DefinitionGenerator
{
    #region Private Fields

    /// <summary>
    /// Cached dictionary of all discovered enums from the game assembly.
    /// Key: enum type name, Value: dictionary of enum value names to integer values.
    /// </summary>
    private static Dictionary<string, Dictionary<string, int>> _cachedEnums = null;

    /// <summary>
    /// Cached generated definitions organized by category.
    /// Key: category name, Value: list of definition entries.
    /// </summary>
    private static Dictionary<string, List<DefinitionEntry>> _cachedDefinitions = null;

    /// <summary>
    /// Flag indicating whether the generator has been initialized.
    /// </summary>
    private static bool _isInitialized = false;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private static readonly object _initLock = new();

    #endregion

    #region Definition Entry

    /// <summary>
    /// Represents a single generated definition entry from an enum value.
    /// </summary>
    public class DefinitionEntry
    {
        /// <summary>
        /// The display name of the definition.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The integer value of the enum.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The category this definition belongs to.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The source enum type name.
        /// </summary>
        public string EnumTypeName { get; set; }

        public DefinitionEntry(string name, int value, string category, string enumTypeName)
        {
            Name = name;
            Value = value;
            Category = category;
            EnumTypeName = enumTypeName;
        }
    }

    #endregion

    #region Target Enum Configuration

    /// <summary>
    /// Configuration for target enums to scan from the game assembly.
    /// Maps friendly category names to enum type names.
    /// </summary>
    private static readonly Dictionary<string, string[]> TargetEnums = new()
    {
        { "Inventory", new[] { "InventoryItem+ITEM_TYPE", "InventoryItem.ITEM_TYPE" } },
        { "FollowerTrait", new[] { "FollowerTrait", "FollowerTrait+TraitType", "FollowerTrait.TraitType" } },
        { "Doctrine", new[] { "DoctrineUpgradeSystem+DoctrineType", "DoctrineUpgradeSystem.DoctrineType", "DoctrineType" } },
        { "Building", new[] { "StructureBrain+TYPES", "StructureBrain.TYPES" } },
        { "Weapon", new[] { "WeaponType", "InventoryWeapon+WeaponType" } },
        { "Curse", new[] { "EquipmentType" } },
    };

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the definition generator and caches all enum data from the game assembly.
    /// Should be called at mod startup.
    /// </summary>
    [Init]
    [EnforceOrderFirst(15)]
    public static void Init()
    {
        lock (_initLock)
        {
            if (_isInitialized) return;

            try
            {
                CheatMenu.Logger.Info(CheatMenu.Logger.INIT, "Starting Definition Generator initialization...");

                // Load all enums from game assembly
                _cachedEnums = GetAllGameEnums();
                _cachedDefinitions = new Dictionary<string, List<DefinitionEntry>>();

                // Generate definitions for each target category
                foreach (var category in TargetEnums.Keys)
                {
                    var definitions = GenerateDefinitionsForCategory(category);
                    if (definitions.Count > 0)
                    {
                        _cachedDefinitions[category] = definitions;
                        CheatMenu.Logger.Info(CheatMenu.Logger.INIT, $"Generated {definitions.Count} definitions for {category}");
                    }
                    else
                    {
                        CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"No definitions found for {category}, will use manual fallback");
                    }
                }

                _isInitialized = true;
                CheatMenu.Logger.Info(CheatMenu.Logger.INIT, "Definition Generator initialized successfully");
            }
            catch (Exception ex)
            {
                CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"Definition Generator initialization failed: {ex.Message}");
                // Initialize with empty caches to allow fallback to manual definitions
                _cachedEnums = new Dictionary<string, Dictionary<string, int>>();
                _cachedDefinitions = new Dictionary<string, List<DefinitionEntry>>();
                _isInitialized = true; // Mark as initialized to prevent retry crashes
            }
        }
    }

    /// <summary>
    /// Scans the game assembly for enums and generates all definitions.
    /// </summary>
    public static void GenerateAllDefinitions()
    {
        if (!_isInitialized)
        {
            Init();
        }
    }

    /// <summary>
    /// Gets enum values from the game assembly by enum type name.
    /// </summary>
    /// <param name="enumTypeName">The full name of the enum type.</param>
    /// <returns>Dictionary mapping enum value names to their integer values.</returns>
    public static Dictionary<string, int> GetEnumValues(string enumTypeName)
    {
        if (_cachedEnums == null)
        {
            Init();
        }

        if (_cachedEnums.TryGetValue(enumTypeName, out var values))
        {
            return values;
        }

        // Try to find the enum with alternative naming
        foreach (var kvp in _cachedEnums)
        {
            if (kvp.Key.EndsWith(enumTypeName) || kvp.Key.Contains(enumTypeName))
            {
                return kvp.Value;
            }
        }

        CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"Enum '{enumTypeName}' not found in game assembly, using empty fallback");
        return new Dictionary<string, int>();
    }

    /// <summary>
    /// Generates definitions for a specific category.
    /// </summary>
    /// <param name="category">The category name (e.g., "Inventory", "FollowerTrait").</param>
    /// <returns>List of definition entries.</returns>
    public static List<DefinitionEntry> GenerateDefinitions(string category)
    {
        if (_cachedDefinitions == null)
        {
            Init();
        }

        if (_cachedDefinitions.TryGetValue(category, out var definitions))
        {
            return definitions;
        }

        return new List<DefinitionEntry>();
    }

    /// <summary>
    /// Checks if an enum exists in the game assembly.
    /// </summary>
    /// <param name="enumTypeName">The enum type name to check.</param>
    /// <returns>True if the enum exists, false otherwise.</returns>
    public static bool EnumExists(string enumTypeName)
    {
        if (_cachedEnums == null)
        {
            Init();
        }

        if (_cachedEnums.ContainsKey(enumTypeName))
        {
            return true;
        }

        // Check partial matches
        foreach (var key in _cachedEnums.Keys)
        {
            if (key.EndsWith(enumTypeName) || key.Contains(enumTypeName))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all discovered enums from the game assembly.
    /// </summary>
    /// <returns>Dictionary of all enums.</returns>
    public static Dictionary<string, Dictionary<string, int>> GetAllEnums()
    {
        if (_cachedEnums == null)
        {
            Init();
        }

        return _cachedEnums ?? new Dictionary<string, Dictionary<string, int>>();
    }

    /// <summary>
    /// Gets a specific definition entry by category and value.
    /// </summary>
    /// <param name="category">The category to search.</param>
    /// <param name="value">The enum value to find.</param>
    /// <returns>The definition entry if found, null otherwise.</returns>
    public static DefinitionEntry GetDefinition(string category, int value)
    {
        var definitions = GenerateDefinitions(category);
        return definitions.FirstOrDefault(d => d.Value == value);
    }

    /// <summary>
    /// Gets all definitions for a specific category.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <returns>List of definition entries.</returns>
    public static List<DefinitionEntry> GetDefinitionsForCategory(string category)
    {
        return GenerateDefinitions(category);
    }

    /// <summary>
    /// Clears the cached definitions (useful for debugging or re-generation).
    /// </summary>
    public static void ClearCache()
    {
        _cachedEnums = null;
        _cachedDefinitions = null;
        _isInitialized = false;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets all enum types from the game assembly.
    /// </summary>
    private static Dictionary<string, Dictionary<string, int>> GetAllGameEnums()
    {
        var result = new Dictionary<string, Dictionary<string, int>>();

        try
        {
            // Get all loaded assemblies and find the main game assembly
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly gameAssembly = null;

            // Try to find the main game assembly (usually has "Assembly-CSharp" or similar)
            foreach (var assembly in assemblies)
            {
                string assemblyName = assembly.GetName().Name;
                if (assemblyName != null && 
                    (assemblyName.Contains("Assembly-CSharp") || 
                     assemblyName.Contains("Cult of the Lamb") ||
                     (!assemblyName.StartsWith("BepInEx") && 
                      !assemblyName.StartsWith("System") && 
                      !assemblyName.StartsWith("Unity") &&
                      !assemblyName.StartsWith("Microsoft"))))
                {
                    // Prefer the main game assembly
                    if (gameAssembly == null || assemblyName == "Assembly-CSharp")
                    {
                        gameAssembly = assembly;
                    }
                }
            }

            if (gameAssembly == null)
            {
                CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, "Could not find game assembly, using fallback");
                return result;
            }

            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, $"Scanning game assembly: {gameAssembly.GetName().Name}");

            // Get all types from the game assembly
            Type[] types;
            try
            {
                types = gameAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
                CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"Partial assembly load: {types.Length} types loaded");
            }

            // Find all enum types
            foreach (var type in types)
            {
                if (type == null) continue;

                // Check if it's an enum
                if (type.IsEnum)
                {
                    var enumValues = GetEnumValuesInternal(type);
                    if (enumValues.Count > 0)
                    {
                        result[type.FullName] = enumValues;
                        
                        // Also add with simplified name
                        string simpleName = type.Name;
                        if (!result.ContainsKey(simpleName))
                        {
                            result[simpleName] = enumValues;
                        }
                    }
                }
                // Check for nested enums (like InventoryItem+ITEM_TYPE)
                else if (type.IsClass || type.IsValueType)
                {
                    var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var nestedType in nestedTypes)
                    {
                        if (nestedType.IsEnum)
                        {
                            var enumValues = GetEnumValuesInternal(nestedType);
                            if (enumValues.Count > 0)
                            {
                                string fullName = $"{type.FullName}+{nestedType.Name}";
                                result[fullName] = enumValues;
                            }
                        }
                    }
                }
            }

            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, $"Found {result.Count} enum types in game assembly");
        }
        catch (Exception ex)
        {
            CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"Error scanning game assembly: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Gets all values of an enum type.
    /// </summary>
    private static Dictionary<string, int> GetEnumValuesInternal(Type enumType)
    {
        var result = new Dictionary<string, int>();

        try
        {
            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    try
                    {
                        int value = (int)field.GetValue(null);
                        result[field.Name] = value;
                    }
                    catch
                    {
                        // Skip if we can't get the value
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CheatMenu.Logger.Warning(CheatMenu.Logger.INIT, $"Error reading enum {enumType.Name}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Generates definition entries for a specific category.
    /// </summary>
    private static List<DefinitionEntry> GenerateDefinitionsForCategory(string category)
    {
        var result = new List<DefinitionEntry>();

        if (!TargetEnums.TryGetValue(category, out var enumNames))
        {
            return result;
        }

        // Try each enum name variant
        foreach (var enumName in enumNames)
        {
            var enumValues = GetEnumValues(enumName);
            if (enumValues.Count > 0)
            {
                foreach (var kvp in enumValues)
                {
                    result.Add(new DefinitionEntry(kvp.Key, kvp.Value, category, enumName));
                }
                break; // Use first successful match
            }
        }

        return result;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets the integer value for a named enum value.
    /// </summary>
    public static int GetEnumValue(string category, string name)
    {
        var definitions = GenerateDefinitions(category);
        var entry = definitions.FirstOrDefault(d => 
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return entry?.Value ?? -1;
    }

    /// <summary>
    /// Gets the display name for an enum value.
    /// </summary>
    public static string GetEnumName(string category, int value)
    {
        var entry = GetDefinition(category, value);
        return entry?.Name ?? $"Unknown_{value}";
    }

    /// <summary>
    /// Checks if a category has any generated definitions.
    /// </summary>
    public static bool HasDefinitions(string category)
    {
        var definitions = GenerateDefinitions(category);
        return definitions.Count > 0;
    }

    /// <summary>
    /// Gets the count of definitions for a category.
    /// </summary>
    public static int GetDefinitionCount(string category)
    {
        var definitions = GenerateDefinitions(category);
        return definitions.Count;
    }

    #endregion
}
