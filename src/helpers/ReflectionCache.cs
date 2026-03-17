using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace CheatMenu;

/// <summary>
/// Provides a high-performance reflection caching system for cheat methods.
/// Scans the executing assembly once at startup and caches methods with specific attributes,
/// eliminating the need for repeated reflection calls during runtime.
/// </summary>
/// <remarks>
/// This class is designed to replace repeated assembly scanning that occurs in the menu system.
/// It caches the following attribute types:
/// - CheatCategory (class-level)
/// - CheatDetails (method-level)
/// - RequiresDLC (method-level)
/// - Lifecycle attributes: Init, Unload, OnGui, Update, EnforceOrderFirst, EnforceOrderLast
/// </remarks>
public static class ReflectionCache
{
    #region Private Fields

    /// <summary>
    /// Cached methods grouped by their declaring type.
    /// </summary>
    private static readonly Dictionary<Type, List<MethodInfo>> cachedMethods = new();

    /// <summary>
    /// Fast lookup dictionary for cheat methods by full name (ClassName.MethodName).
    /// </summary>
    private static readonly Dictionary<string, MethodInfo> cheatLookup = new();

    /// <summary>
    /// Methods grouped by category name (from CheatCategory attribute on declaring class).
    /// </summary>
    private static readonly Dictionary<string, List<MethodInfo>> categoryMethods = new();

    /// <summary>
    /// All methods with CheatDetails attribute.
    /// </summary>
    private static readonly List<MethodInfo> allCheatMethods = new();

    /// <summary>
    /// Methods with Init attribute, sorted by EnforceOrderFirst order.
    /// </summary>
    private static readonly List<MethodInfo> initMethods = new();

    /// <summary>
    /// Methods with Unload attribute.
    /// </summary>
    private static readonly List<MethodInfo> unloadMethods = new();

    /// <summary>
    /// Methods with OnGui attribute.
    /// </summary>
    private static readonly List<MethodInfo> onGuiMethods = new();

    /// <summary>
    /// Methods with Update attribute.
    /// </summary>
    private static readonly List<MethodInfo> updateMethods = new();

    /// <summary>
    /// Indicates whether the cache has been initialized.
    /// </summary>
    private static bool isInitialized = false;

    /// <summary>
    /// Lock object for thread-safe initialization.
    /// </summary>
    private static readonly object initLock = new();

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the cached methods grouped by type.
    /// </summary>
    public static IReadOnlyDictionary<Type, List<MethodInfo>> CachedMethods => cachedMethods;

    /// <summary>
    /// Gets the cheat lookup dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, MethodInfo> CheatLookup => cheatLookup;

    /// <summary>
    /// Gets the category methods dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, List<MethodInfo>> CategoryMethods => categoryMethods;

    /// <summary>
    /// Gets all cached cheat methods.
    /// </summary>
    public static IReadOnlyList<MethodInfo> AllCheatMethods => allCheatMethods;

    /// <summary>
    /// Gets all init methods.
    /// </summary>
    public static IReadOnlyList<MethodInfo> InitMethods => initMethods;

    /// <summary>
    /// Gets all unload methods.
    /// </summary>
    public static IReadOnlyList<MethodInfo> UnloadMethods => unloadMethods;

    /// <summary>
    /// Gets all OnGui methods.
    /// </summary>
    public static IReadOnlyList<MethodInfo> OnGuiMethods => onGuiMethods;

    /// <summary>
    /// Gets all Update methods.
    /// </summary>
    public static IReadOnlyList<MethodInfo> UpdateMethods => updateMethods;

    /// <summary>
    /// Gets whether the cache has been initialized.
    /// </summary>
    public static bool IsInitialized => isInitialized;

    #endregion

    #region Public Methods

    /// <summary>
    /// Scans the executing assembly once at startup and caches all methods with relevant attributes.
    /// This method is safe to call multiple times but will only perform the scan once.
    /// </summary>
    /// <remarks>
    /// Should be called during plugin initialization. Wraps all reflection calls in try/catch
    /// and logs errors with [CHEAT] prefix for diagnostic purposes.
    /// </remarks>
    public static void ScanAssembly()
    {
        lock (initLock)
        {
            if (isInitialized)
            {
                return;
            }

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (Type type in types)
                {
                    try
                    {
                        ScanType(type);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CHEAT] Error scanning type {type?.FullName}: {ex.Message}");
                    }
                }

                // Sort init methods by EnforceOrderFirst order
                SortInitMethods();

                isInitialized = true;
                Console.WriteLine($"[CHEAT] Reflection cache initialized: {allCheatMethods.Count} cheats, {categoryMethods.Count} categories");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] FATAL ERROR during assembly scan: {ex.Message}");
                Console.WriteLine($"[CHEAT] Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Retrieves a cached MethodInfo by its full name (ClassName.MethodName).
    /// </summary>
    /// <param name="fullName">The full name of the method in format "ClassName.MethodName".</param>
    /// <returns>The MethodInfo if found, otherwise null.</returns>
    public static MethodInfo GetCheatMethod(string fullName)
    {
        if (string.IsNullOrEmpty(fullName) || !isInitialized)
        {
            return null;
        }

        cheatLookup.TryGetValue(fullName, out MethodInfo method);
        return method;
    }

    /// <summary>
    /// Retrieves all cheat methods belonging to a specific category.
    /// </summary>
    /// <param name="category">The category name to filter by (case-sensitive key from CheatCategory).</param>
    /// <returns>List of methods in the specified category, or empty list if category not found.</returns>
    public static List<MethodInfo> GetCheatsByCategory(string category)
    {
        if (string.IsNullOrEmpty(category) || !isInitialized)
        {
            return new List<MethodInfo>();
        }

        if (categoryMethods.TryGetValue(category, out List<MethodInfo> methods))
        {
            return methods;
        }

        return new List<MethodInfo>();
    }

    /// <summary>
    /// Retrieves all cheat methods belonging to a specific CheatCategoryEnum.
    /// </summary>
    /// <param name="categoryEnum">The category enum value.</param>
    /// <returns>List of methods in the specified category.</returns>
    public static List<MethodInfo> GetCheatsByCategory(CheatCategoryEnum categoryEnum)
    {
        string categoryName = categoryEnum.GetCategoryName();
        return GetCheatsByCategory(categoryName);
    }

    /// <summary>
    /// Gets all cached cheat methods.
    /// </summary>
    /// <returns>All methods that have CheatDetails attribute.</returns>
    public static List<MethodInfo> GetAllCheats()
    {
        if (!isInitialized)
        {
            return new List<MethodInfo>();
        }

        return new List<MethodInfo>(allCheatMethods);
    }

    /// <summary>
    /// Gets all lifecycle methods (Init, Unload, OnGui, Update).
    /// </summary>
    /// <returns>Dictionary of lifecycle method types to their method lists.</returns>
    public static Dictionary<string, List<MethodInfo>> GetLifecycleMethods()
    {
        return new Dictionary<string, List<MethodInfo>>
        {
            { "Init", new List<MethodInfo>(initMethods) },
            { "Unload", new List<MethodInfo>(unloadMethods) },
            { "OnGui", new List<MethodInfo>(onGuiMethods) },
            { "Update", new List<MethodInfo>(updateMethods) }
        };
    }

    /// <summary>
    /// Checks if a specific cheat method exists in the cache.
    /// </summary>
    /// <param name="fullName">The full name of the method.</param>
    /// <returns>True if the method exists in cache.</returns>
    public static bool HasCheat(string fullName)
    {
        return !string.IsNullOrEmpty(fullName) && isInitialized && cheatLookup.ContainsKey(fullName);
    }

    /// <summary>
    /// Gets the CheatDetails attribute from a cached method.
    /// </summary>
    /// <param name="method">The method to get details for.</param>
    /// <returns>CheatDetails attribute if present, otherwise null.</returns>
    public static CheatDetails GetCheatDetails(MethodInfo method)
    {
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.GetCustomAttribute<CheatDetails>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the RequiresDLC attribute from a cached method.
    /// </summary>
    /// <param name="method">The method to get DLC requirement for.</param>
    /// <returns>RequiresDLC attribute if present, otherwise null.</returns>
    public static RequiresDLC GetDlcRequirement(MethodInfo method)
    {
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.GetCustomAttribute<RequiresDLC>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clears the cache. Mainly useful for testing.
    /// </summary>
    public static void Clear()
    {
        lock (initLock)
        {
            cachedMethods.Clear();
            cheatLookup.Clear();
            categoryMethods.Clear();
            allCheatMethods.Clear();
            initMethods.Clear();
            unloadMethods.Clear();
            onGuiMethods.Clear();
            updateMethods.Clear();
            isInitialized = false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Scans a single type for methods with relevant attributes.
    /// </summary>
    private static void ScanType(Type type)
    {
        if (type == null || !type.IsClass)
        {
            return;
        }

        // Get the category from the class (CheatCategory attribute)
        CheatCategory categoryAttr = null;
        try
        {
            categoryAttr = type.GetCustomAttribute<CheatCategory>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CHEAT] Error getting CheatCategory for {type.FullName}: {ex.Message}");
        }

        string categoryName = categoryAttr?.Category.GetCategoryName() ?? "Misc";

        // Get all static public methods
        MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        
        List<MethodInfo> typeMethods = new();
        
        foreach (MethodInfo method in methods)
        {
            bool addedToCache = false;

            // Check for CheatDetails attribute (main cheat indicator)
            CheatDetails cheatDetails = null;
            try
            {
                cheatDetails = method.GetCustomAttribute<CheatDetails>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error getting CheatDetails for {method.Name}: {ex.Message}");
            }

            if (cheatDetails != null)
            {
                // Add to allCheatMethods
                allCheatMethods.Add(method);
                addedToCache = true;

                // Add to cheatLookup
                string fullName = $"{type.Name}.{method.Name}";
                cheatLookup[fullName] = method;

                // Add to categoryMethods
                if (!categoryMethods.TryGetValue(categoryName, out List<MethodInfo> categoryList))
                {
                    categoryList = new List<MethodInfo>();
                    categoryMethods[categoryName] = categoryList;
                }
                categoryList.Add(method);
            }

            // Check for RequiresDLC attribute (informational, but cache it)
            RequiresDLC requiresDlc = null;
            try
            {
                requiresDlc = method.GetCustomAttribute<RequiresDLC>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error getting RequiresDLC for {method.Name}: {ex.Message}");
            }

            // Check for lifecycle attributes
            bool isLifecycleMethod = false;

            // Init methods
            try
            {
                if (method.GetCustomAttribute<Init>() != null)
                {
                    initMethods.Add(method);
                    isLifecycleMethod = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error checking Init attribute for {method.Name}: {ex.Message}");
            }

            // Unload methods
            try
            {
                if (method.GetCustomAttribute<Unload>() != null)
                {
                    unloadMethods.Add(method);
                    isLifecycleMethod = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error checking Unload attribute for {method.Name}: {ex.Message}");
            }

            // OnGui methods
            try
            {
                if (method.GetCustomAttribute<OnGui>() != null)
                {
                    onGuiMethods.Add(method);
                    isLifecycleMethod = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error checking OnGui attribute for {method.Name}: {ex.Message}");
            }

            // Update methods
            try
            {
                if (method.GetCustomAttribute<Update>() != null)
                {
                    updateMethods.Add(method);
                    isLifecycleMethod = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error checking Update attribute for {method.Name}: {ex.Message}");
            }

            // Add to type's method list if it's a cheat or lifecycle method
            if (addedToCache || isLifecycleMethod)
            {
                typeMethods.Add(method);
            }
        }

        // Add to cachedMethods if we found any relevant methods
        if (typeMethods.Count > 0)
        {
            cachedMethods[type] = typeMethods;
        }
    }

    /// <summary>
    /// Sorts init methods by EnforceOrderFirst order, then normal methods, then EnforceOrderLast.
    /// </summary>
    private static void SortInitMethods()
    {
        List<(MethodInfo method, int order)> enforceFirstMethods = new();
        List<MethodInfo> normalMethods = new();
        List<MethodInfo> enforceLastMethods = new();

        foreach (MethodInfo method in initMethods)
        {
            try
            {
                EnforceOrderFirst enforceFirst = method.GetCustomAttribute<EnforceOrderFirst>();
                EnforceOrderLast enforceLast = method.GetCustomAttribute<EnforceOrderLast>();

                if (enforceFirst != null)
                {
                    enforceFirstMethods.Add((method, enforceFirst.Order));
                }
                else if (enforceLast != null)
                {
                    enforceLastMethods.Add(method);
                }
                else
                {
                    normalMethods.Add(method);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHEAT] Error sorting init method {method.Name}: {ex.Message}");
                normalMethods.Add(method);
            }
        }

        // Sort enforceFirst by order
        enforceFirstMethods.Sort((a, b) => a.order.CompareTo(b.order));

        // Rebuild initMethods list
        initMethods.Clear();
        initMethods.AddRange(enforceFirstMethods.Select(x => x.method));
        initMethods.AddRange(normalMethods);
        initMethods.AddRange(enforceLastMethods);
    }

    #endregion
}
