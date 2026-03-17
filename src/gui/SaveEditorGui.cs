using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CheatMenu;

public static class SaveEditorGui
{
    public static bool IsOpen = false;
    
    // Track if the save editor is currently closing (to prevent re-opening during close)
    public static bool IsClosing = false;
    
    // Window dimensions - dynamically calculated based on screen size
    private static int _windowWidth = 1100;
    private static int _windowHeight = 750;
    
    // Save selection screen dimensions - dynamically calculated
    private static int _saveSelectionWidth = 1100;
    private static int _saveSelectionHeight = 700;
    
    // Tab configuration - matching cheat menu style
    private static string[] _tabs = new string[] { 
        "Player", "Cult", "Followers", "Inventory", "Buildings", 
        "Resources", "Time", "Flags", "Quests", "Story", "Woolhaven", "All" 
    };
    private static int _selectedTab = 11; // Default to "All"
    
    // Search and filter
    private static string _searchQuery = "";
    private static Vector2 _listScrollPosition = Vector2.zero;
    private static Vector2 _detailScrollPosition = Vector2.zero;
    
    /// <summary>
    /// Validates input value for a field type before applying.
    /// Returns null if valid, or an error message if invalid.
    /// </summary>
    private static string ValidateFieldValue(Type fieldType, string value) {
        if (string.IsNullOrEmpty(value)) {
            return null; // Allow empty for reset
        }
        
        try {
            if (fieldType == typeof(int)) {
                int.Parse(value);
            } else if (fieldType == typeof(float)) {
                float.Parse(value);
            } else if (fieldType == typeof(double)) {
                double.Parse(value);
            } else if (fieldType == typeof(long)) {
                long.Parse(value);
            } else if (fieldType == typeof(bool)) {
                bool.Parse(value);
            } else if (fieldType == typeof(uint)) {
                uint.Parse(value);
            }
        } catch (FormatException) {
            return $"Invalid {fieldType.Name} format";
        }
        
        return null; // Valid
    }

    // All cached fields
    private static List<SaveFieldInfo> _allFields = null;
    
    // Currently displayed/filtered fields
    private static List<SaveFieldInfo> _displayedFields = new List<SaveFieldInfo>();
    
    // Selected field for editing
    private static SaveFieldInfo _selectedField = null;
    private static string _editValue = "";
    
    // Nested item editing
    private static int _selectedNestedIndex = -1; // Index of nested item being edited
    private static string _selectedNestedPath = ""; // Path to nested field
    
    // Export/Import status
    private static string _statusMessage = "";
    private static float _statusTimer = 0;
    
    // Save file selection
    private static List<SaveFileInfo> _saveFiles = new List<SaveFileInfo>();
    private static int _selectedSaveIndex = -1;
    private static bool _isAtSaveSelection = true;
    private static bool _showImportFilePicker = false;
    private static string[] _importFileList = new string[0];
    
    // Filter options - hide null values by default
    private static bool _hideNullValues = true;
    private static HashSet<string> _nonNullFieldNames = new HashSet<string>(); // Tracks fields that have ever been non-null
    
    // Cult of the Lamb theme colors - using GUIUtils constants
    // Colors are defined in GUIUtils.cs to avoid duplication
    private static Color CULT_DARK_RED => GUIUtils.CULT_DARK_RED;
    private static Color CULT_RED => GUIUtils.CULT_RED;
    private static Color CULT_BONE_WHITE => GUIUtils.CULT_BONE_WHITE;
    private static Color CULT_GOLD => GUIUtils.CULT_GOLD;
    private static Color CULT_BLACK => GUIUtils.CULT_BLACK;
    private static Color CULT_DARK_PURPLE => GUIUtils.CULT_DARK_PURPLE;
    
    [Init]
    public static void Init()
    {
        IsOpen = false;
        _selectedTab = 9;
        _searchQuery = "";
        _listScrollPosition = Vector2.zero;
        _detailScrollPosition = Vector2.zero;
        _allFields = null;
        _displayedFields.Clear();
        _selectedField = null;
        _selectedNestedIndex = -1;
        _saveFiles.Clear();
        _selectedSaveIndex = -1;
        _isAtSaveSelection = true;
        _hideNullValues = true;
        _nonNullFieldNames.Clear();
    }
    
    public static void Open()
    {
        // Check if save editor is unlocked via console
        if(!CheatMenuGui.IsSaveEditorUnlocked()) {
            CultUtils.PlayNotification("Save Editor is locked! Press ~ to unlock");
            return;
        }
        
        // Allow opening save editor from main menu to access save selector
        // At main menu, show save selector to load .mp saves
        // In-game, directly edit current save
        
        // Close cheat menu when opening save editor (if it's enabled)
        if (CheatMenuGui.GuiEnabled)
        {
            CheatMenuGui.GuiEnabled = false;
        }
        
        // At main menu, we can open to show save selector (no DataManager needed)
        // In-game, we need DataManager to be loaded
        if (!CultUtils.IsInGame())
        {
            // At main menu - show save selector to load saves
            _isAtSaveSelection = true;
            ScanSaveFiles();
        }
        else
        {
            // In-game - validate DataManager is loaded
            if (DataManager.Instance == null)
            {
                UnityEngine.Debug.LogError("[SaveEditor] DataManager.Instance is null - cannot open save editor");
                return;
            }
            
            // When in-game, skip save selection and directly edit the current save
            // The user is forced to use the currently loaded save
            _isAtSaveSelection = false;
            _selectedSaveIndex = -1;
            _selectedField = null;
            
            // Cache all fields from DataManager
            CacheAllFields();
            
            // Apply current filter
            ApplyFilter();
            
            UnityEngine.Debug.Log("[SaveEditor] Opened in-game mode - editing current save");
        }
        
        // Calculate dimensions based on screen size
        CalculateWindowDimensions();
        
        // Reset scroll positions to avoid issues with stale scroll positions
        _listScrollPosition = Vector2.zero;
        _detailScrollPosition = Vector2.zero;
        
        // Skip scanning saves in-game since we're using the current save
        if (CultUtils.IsInGame())
        {
            _saveFiles.Clear();
        }
        
        IsOpen = true;
    }
    
    /// <summary>
    /// Calculate window dimensions based on current screen size
    /// </summary>
    private static void CalculateWindowDimensions()
    {
        // Use 95% of screen size for better visibility (save selection)
        _saveSelectionWidth = (int)(Screen.width * 0.95f);
        _saveSelectionHeight = (int)(Screen.height * 0.95f);
        
        // Ensure minimum sizes
        _saveSelectionWidth = Math.Max(_saveSelectionWidth, 800);
        _saveSelectionHeight = Math.Max(_saveSelectionHeight, 600);
        
        // Use 90% of screen size for editor window - wider and taller
        _windowWidth = (int)(Screen.width * 0.92f);
        _windowHeight = (int)(Screen.height * 0.92f);
        
        // Ensure minimum sizes
        _windowWidth = Math.Max(_windowWidth, 1000);
        _windowHeight = Math.Max(_windowHeight, 700);
    }
    
    private static void ScanSaveFiles()
    {
        _saveFiles.Clear();
        
        try
        {
            // Find the saves folder - use Unity's Application.persistentDataPath via reflection
            // This is the correct way to get the saves path as the game uses it
            string savesPath = null;
            
            try
            {
                // Get Application.persistentDataPath via reflection
                var applicationType = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule");
                if (applicationType != null)
                {
                    var persistentDataPathProperty = applicationType.GetProperty("persistentDataPath", BindingFlags.Static | BindingFlags.Public);
                    if (persistentDataPathProperty != null)
                    {
                        var persistentDataPath = persistentDataPathProperty.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(persistentDataPath))
                        {
                            savesPath = Path.Combine(persistentDataPath, "saves");
                            UnityEngine.Debug.Log($"[SaveEditor] Using Unity persistentDataPath: {savesPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[SaveEditor] Error getting Unity path: {ex.Message}");
            }
            
            // Fallback: try to construct the path manually
            if (savesPath == null || !Directory.Exists(savesPath))
            {
                string[] possiblePaths = new string[]
                {
                    // Try LocalApplicationData then manually go to LocalLow
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), "Massive Monster", "Cult Of The Lamb", "saves"),
                    // Try the username-based path
                    @"C:\Users\" + Environment.UserName + @"\AppData\LocalLow\Massive Monster\Cult Of The Lamb\saves"
                };
                
                foreach (var path in possiblePaths)
                {
                    UnityEngine.Debug.Log($"[SaveEditor] Checking fallback path: {path}");
                    if (Directory.Exists(path))
                    {
                        savesPath = path;
                        UnityEngine.Debug.Log($"[SaveEditor] Found saves at: {savesPath}");
                        break;
                    }
                }
            }
            
            if (savesPath == null)
            {
                Debug.Log("[SaveEditor] Saves folder not found");
                return;
            }
            
            // Scan for .mp files
            var mpFiles = Directory.GetFiles(savesPath, "*.mp", SearchOption.AllDirectories);
            foreach (var file in mpFiles)
            {
                var info = new FileInfo(file);
                
                // Check if it's Woolhaven (usually in woolhaven subfolder or has woolhaven in name)
                bool isWoolhaven = file.Contains("woolhaven") || file.Contains("Woolhaven");
                
                _saveFiles.Add(new SaveFileInfo
                {
                    FilePath = file,
                    FileName = info.Name,
                    IsWoolhaven = isWoolhaven,
                    LastModified = info.LastWriteTime,
                    FileSize = info.Length
                });
            }
            
            // Scan for .json files (our exports)
            var jsonFiles = Directory.GetFiles(savesPath, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                var info = new FileInfo(file);
                
                // Check if it's Woolhaven
                bool isWoolhaven = file.Contains("woolhaven") || file.Contains("Woolhaven");
                
                _saveFiles.Add(new SaveFileInfo
                {
                    FilePath = file,
                    FileName = info.Name,
                    IsWoolhaven = isWoolhaven,
                    LastModified = info.LastWriteTime,
                    FileSize = info.Length
                });
            }
            
            // Sort by last modified (newest first)
            _saveFiles.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            
            // If in-game, filter to only show the current save slot
            if (CultUtils.IsInGame())
            {
                try
                {
                    // Get the current save slot
                    var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
                    if (saveAndLoadType != null)
                    {
                        var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                        if (saveSlotField != null)
                        {
                            int currentSlot = (int)saveSlotField.GetValue(null);
                            UnityEngine.Debug.Log($"[SaveEditor] In-game, filtering to current save slot: {currentSlot}");
                            
                            // Filter to only show saves matching the current slot
                            _saveFiles = _saveFiles.Where(s => 
                                s.FileName.Contains($"slot_{currentSlot}") || 
                                s.FileName.Contains($"Slot_{currentSlot}") ||
                                s.FileName.Contains($"slot_{currentSlot - 10}") ||
                                s.FileName.Contains($"Slot_{currentSlot - 10}")
                            ).ToList();
                            
                            UnityEngine.Debug.Log($"[SaveEditor] Filtered to {_saveFiles.Count} save file(s)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[SaveEditor] Error filtering saves: {ex.Message}");
                }
            }
            
            Debug.Log($"[SaveEditor] Found {_saveFiles.Count} save files");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveEditor] Error scanning saves: {ex.Message}");
        }
    }
    
    private static void CacheAllFields()
    {
        if (_allFields != null) return;
        
        _allFields = new List<SaveFieldInfo>();
        
        try
        {
            Type dmType = typeof(DataManager);
            // Only get declared fields - no inherited fields from Object base class
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            
            // Get top-level fields
            foreach (var field in dmType.GetFields(flags))
            {
                // Skip Unity engine objects that can't be serialized
                if (IsUnityEngineType(field.FieldType))
                    continue;
                    
                _allFields.Add(new SaveFieldInfo
                {
                    Name = field.Name,
                    FieldType = field.FieldType,
                    Category = DetermineCategory(field.Name),
                    IsReadable = true,
                    IsWritable = !field.IsInitOnly && !field.IsLiteral,
                    FieldInfo = field,
                    MemberInfo = field
                });
                
                // Get nested fields (level 1) - INCLUDE collections, just limit depth
                if (!field.FieldType.IsPrimitive && !field.FieldType.IsEnum && field.FieldType != typeof(string) && !IsUnityEngineType(field.FieldType))
                {
                    try { CacheNestedFields(field.FieldType, field.Name + ".", DetermineCategory(field.Name), 1); } catch { }
                }
            }
            
            // Get top-level properties
            foreach (var prop in dmType.GetProperties(flags))
            {
                if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                    continue;
                    
                // Skip Unity engine objects
                if (IsUnityEngineType(prop.PropertyType))
                    continue;
                    
                _allFields.Add(new SaveFieldInfo
                {
                    Name = prop.Name,
                    FieldType = prop.PropertyType,
                    Category = DetermineCategory(prop.Name),
                    IsReadable = true,
                    IsWritable = true,
                    PropertyInfo = prop,
                    MemberInfo = prop
                });
                
                // Get nested fields (level 1) - INCLUDE collections, just limit depth
                if (!prop.PropertyType.IsPrimitive && !prop.PropertyType.IsEnum && prop.PropertyType != typeof(string) && !IsUnityEngineType(prop.PropertyType))
                {
                    try { CacheNestedFields(prop.PropertyType, prop.Name + ".", DetermineCategory(prop.Name), 1); } catch { }
                }
            }
            
            // Also cache MetaData fields if available
            try {
                var metaProperty = typeof(DataManager).GetProperty("MetaData", BindingFlags.Public | BindingFlags.Instance);
                if (metaProperty != null) {
                    var metaData = metaProperty.GetValue(DataManager.Instance);
                    if (metaData != null) {
                        var metaType = metaData.GetType();
                        UnityEngine.Debug.Log($"[SaveEditor] Caching MetaData fields from type: {metaType.Name}");
                        
                        // Get MetaData fields
                        foreach (var field in metaType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                            if (IsUnityEngineType(field.FieldType)) continue;
                            
                            _allFields.Add(new SaveFieldInfo {
                                Name = "MetaData." + field.Name,
                                FieldType = field.FieldType,
                                Category = "MetaData",
                                IsReadable = true,
                                IsWritable = !field.IsInitOnly && !field.IsLiteral,
                                FieldInfo = field,
                                MemberInfo = field,
                                ParentObject = metaData
                            });
                        }
                        
                        // Get MetaData properties
                        foreach (var prop in metaType.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                            if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0) continue;
                            if (IsUnityEngineType(prop.PropertyType)) continue;
                            
                            _allFields.Add(new SaveFieldInfo {
                                Name = "MetaData." + prop.Name,
                                FieldType = prop.PropertyType,
                                Category = "MetaData",
                                IsReadable = true,
                                IsWritable = true,
                                PropertyInfo = prop,
                                MemberInfo = prop,
                                ParentObject = metaData
                            });
                        }
                    }
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogWarning($"[SaveEditor] Error caching MetaData: {ex.Message}");
            }
            
            Debug.Log($"[SaveEditor] Cached {_allFields.Count} fields from DataManager (depth=1, includes MetaData)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveEditor] Error caching fields: {ex.Message}");
        }
    }
    
    private static void CacheNestedFields(Type type, string prefix, string category, int depth)
    {
        // Limit depth to 1 for nested fields
        if (depth > 1) return;
        
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        
        // Handle collections: IEnumerable (includes List, Array, Dictionary, etc.)
        // Skip actual iteration - just get element types
        if (type.GetInterface("IEnumerable") != null && type != typeof(string))
        {
            // For Dictionary, get both key and value types
            if (type.GetInterface("IDictionary") != null)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var keyType = genericArgs[0];
                    var valueType = genericArgs[1];
                    if (keyType != null && !keyType.IsPrimitive && !keyType.IsEnum && keyType != typeof(string) && !IsUnityEngineType(keyType))
                        CacheNestedFields(keyType, prefix + "[key]", category, depth + 1);
                    if (valueType != null && !valueType.IsPrimitive && !valueType.IsEnum && valueType != typeof(string) && !IsUnityEngineType(valueType))
                        CacheNestedFields(valueType, prefix + "[value]", category, depth + 1);
                }
            }
            else
            {
                // For List, Array, etc.
                var elementType = type.GetGenericArguments().Length > 0 ? type.GetGenericArguments()[0] : (type.GetElementType() ?? type);
                if (elementType != null && !elementType.IsPrimitive && !elementType.IsEnum && elementType != typeof(string) && !IsUnityEngineType(elementType))
                    CacheNestedFields(elementType, prefix + "[]", category, depth + 1);
            }
            return;
        }
        
        foreach (var field in type.GetFields(flags))
        {
            if (IsUnityEngineType(field.FieldType)) continue;
            string fullName = prefix + field.Name;
            _allFields.Add(new SaveFieldInfo { Name = fullName, FieldType = field.FieldType, Category = DetermineCategory(fullName), IsReadable = true, IsWritable = !field.IsInitOnly && !field.IsLiteral, FieldInfo = field, MemberInfo = field });
            
            // Continue recursion for non-collection complex types
            if (depth < 2 && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum && field.FieldType != typeof(string) && !IsUnityEngineType(field.FieldType) && field.FieldType.GetInterface("IEnumerable") == null)
                try { CacheNestedFields(field.FieldType, fullName + ".", DetermineCategory(fullName), depth + 1); } catch { }
        }
        
        foreach (var prop in type.GetProperties(flags))
        {
            if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0 || IsUnityEngineType(prop.PropertyType)) continue;
            _allFields.Add(new SaveFieldInfo { Name = prefix + prop.Name, FieldType = prop.PropertyType, Category = DetermineCategory(prefix + prop.Name), IsReadable = true, IsWritable = true, PropertyInfo = prop, MemberInfo = prop });
        }
    }
    
    /// <summary>
    /// Checks if a type is a Unity engine type that shouldn't be serialized
    /// </summary>
    private static bool IsUnityEngineType(Type t)
    {
        // Skip Unity engine base types
        if (t.Namespace != null && t.Namespace.StartsWith("UnityEngine"))
        {
            // Allow some Unity types that are serializable
            if (t == typeof(UnityEngine.Vector2) || 
                t == typeof(UnityEngine.Vector3) || 
                t == typeof(UnityEngine.Vector4) ||
                t == typeof(UnityEngine.Quaternion) ||
                t == typeof(UnityEngine.Color) ||
                t == typeof(UnityEngine.Color32) ||
                t == typeof(UnityEngine.Rect) ||
                t == typeof(UnityEngine.Bounds))
            {
                return false;
            }
            return true;
        }
        return false;
    }
    
    private static bool IsValidSaveType(Type t)
    {
        // Accept ALL types now - we handle them in GetFieldValue
        return true;
    }
    
    private static string DetermineCategory(string fieldName)
    {
        string lower = fieldName.ToLower();
        
        // Quests & Objectives
        if (lower.Contains("quest") || lower.Contains("objective") || lower.Contains("mission") ||
            lower.Contains("completed") || lower.Contains("active") || lower.Contains("progress"))
            return "Quests";
        
        // Story & Campaign
        if (lower.Contains("story") || lower.Contains("campaign") || lower.Contains("chapter") ||
            lower.Contains("final") || lower.Contains("ending") || lower.Contains("intro") ||
            lower.Contains("tutorial"))
            return "Story";
        
        // Player stats
        if (lower.Contains("xp") || lower.Contains("level") || lower.Contains("ability") ||
            lower.Contains("doctrine") || lower.Contains("upgrade") || lower.Contains("stat"))
            return "Player";
        
        // Cult related
        if (lower.Contains("cult") || lower.Contains("faith") || lower.Contains("trait") ||
            lower.Contains("sermon") || lower.Contains("shrine") || lower.Contains("altar") ||
            lower.Contains("doctrine"))
            return "Cult";
        
        // Followers
        if (lower.Contains("follower") || lower.Contains("recruit") || lower.Contains("dead"))
            return "Followers";
        
        // Inventory/Items
        if (lower.Contains("inventory") || lower.Contains("item") || lower.Contains("weapon") ||
            lower.Contains("armor") || lower.Contains("equipment") || lower.Contains("tarot") ||
            lower.Contains("relic") || lower.Contains("costume") || lower.Contains("clothing") ||
            lower.Contains("tarot") || lower.Contains("trinket"))
            return "Inventory";
        
        // Buildings & Structures
        if (lower.Contains("structure") || lower.Contains("building") || lower.Contains("farm") ||
            lower.Contains("room") || lower.Contains("revealed") || lower.Contains("unlocked"))
            return "Buildings";
        
        // Resources
        if (lower.Contains("gold") || lower.Contains("soul") || lower.Contains("seed") ||
            lower.Contains("wood") || lower.Contains("stone") || lower.Contains("bone") ||
            lower.Contains("heretic") || lower.Contains("resource") || lower.Contains("meat") ||
            lower.Contains("berry") || lower.Contains("log") || lower.Contains("token") ||
            lower.Contains("ammo") || lower.Contains("arrow"))
            return "Resources";
        
        // Time
        if (lower.Contains("time") || lower.Contains("day") || lower.Contains("season") ||
            lower.Contains("winter") || lower.Contains("hour"))
            return "Time";
        
        // Flags & Progression
        if (lower.Contains("flag") || lower.Contains("unlock") || lower.Contains("discover") ||
            lower.Contains("complete") || lower.Contains("achievement") || lower.Contains("beaten") ||
            lower.Contains("active") || lower.Contains("revealed") ||
            lower.Contains("rescued") || lower.Contains("npghost") || lower.Contains("seen") ||
            lower.Contains("collected") || lower.Contains("obtained"))
            return "Flags";
        
        // Woolhaven DLC (includes Sinful, Heretic, Pilgrim DLC content)
        if (lower.Contains("woolhaven") || lower.Contains("black soul") || lower.Contains("golden egg") ||
            lower.Contains("major_dlc") || lower.Contains("dlc_") || lower.Contains("wool") ||
            lower.Contains("ranch") || lower.Contains("sheep") || lower.Contains("llama") ||
            lower.Contains("goat") || lower.Contains("cow") || lower.Contains("turtle") ||
            lower.Contains("sinful") || lower.Contains("heresy") || lower.Contains("heresy_dlc") ||
            lower.Contains("heretic") || lower.Contains("heretic_dlc") ||
            lower.Contains("pilgrim") || lower.Contains("pilgrim_dlc"))
            return "Woolhaven";
        
        // Default to Player
        return "Player";
    }
    
    private static void ApplyFilter()
    {
        _displayedFields.Clear();
        
        string selectedCategory = _tabs[_selectedTab];
        string query = _searchQuery.ToLower().Trim();
        
        // Check if user has Woolhaven DLC
        bool hasWoolhavenDLC = false;
        try {
            hasWoolhavenDLC = CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC;
        } catch { }
        
        foreach (var field in _allFields)
        {
            // Filter by tab/category
            bool matchesTab = (selectedCategory == "All") || 
                              (field.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
            
            // Filter by search query
            bool matchesSearch = string.IsNullOrEmpty(query) ||
                                 field.Name.ToLower().Contains(query);
            
            // Filter DLC content - hide Woolhaven content if user doesn't own the DLC
            bool isDLCContent = field.Category.Equals("Woolhaven", StringComparison.OrdinalIgnoreCase) ||
                               field.Name.ToLower().Contains("woolhaven") ||
                               field.Name.ToLower().Contains("dlc_") ||
                               field.Name.ToLower().Contains("major_dlc") ||
                               field.Name.ToLower().Contains("ranch") ||
                               field.Name.ToLower().Contains("wool") ||
                               field.Name.ToLower().Contains("sheep") ||
                               field.Name.ToLower().Contains("llama") ||
                               field.Name.ToLower().Contains("goat") ||
                               field.Name.ToLower().Contains("cow") ||
                               field.Name.ToLower().Contains("turtle") ||
                               field.Name.ToLower().Contains("snail") ||
                               field.Name.ToLower().Contains("crab") ||
                               field.Name.ToLower().Contains("spider") ||
                               field.Name.ToLower().Contains("forge") ||
                               field.Name.ToLower().Contains("furnace") ||
                               field.Name.ToLower().Contains("brewery") ||
                               field.Name.ToLower().Contains("tavern") ||
                               field.Name.ToLower().Contains("distillery") ||
                               field.Name.ToLower().Contains("sinful") ||
                               field.Name.ToLower().Contains("heretic") ||
                               field.Name.ToLower().Contains("cultist") ||
                               field.Name.ToLower().Contains("pilgrim");
            
            bool passesDLCFilter = hasWoolhavenDLC || !isDLCContent;
            
            if (matchesTab && matchesSearch && passesDLCFilter)
            {
                _displayedFields.Add(field);
            }
        }
        
        // Sort alphabetically
        _displayedFields.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        
        // Clear selection if not in list
        if (_selectedField != null && !_displayedFields.Contains(_selectedField))
        {
            _selectedField = null;
        }
    }
    
    // Get live game value - reads directly from DataManager, shows actual list contents
    private static string GetFieldValue(SaveFieldInfo field)
    {
        try
        {
            if (DataManager.Instance == null) return "No Data";
            
            // Use ParentObject if set (for MetaData fields), otherwise use DataManager
            object parentObj = field.ParentObject ?? DataManager.Instance;
            
            object value = null;
            
            // Check if this is a nested field (contains '.' in name)
            if (field.Name.Contains("."))
            {
                value = GetNestedFieldValue(field.Name);
            }
            else
            {
                if (field.FieldInfo != null)
                {
                    value = field.FieldInfo.GetValue(parentObj);
                }
                else if (field.PropertyInfo != null)
                {
                    value = field.PropertyInfo.GetValue(parentObj, null);
                }
            }
            
            if (value == null) return "null";
            
            // Handle different types - with truncation for long values
            if (value is bool b) return b ? "True" : "False";
            if (value is int i) return i.ToString();
            if (value is float f)
            {
                string str = f.ToString("F2");
                // Truncate very long numbers
                if (str.Length > 15) str = str.Substring(0, 12) + "...";
                return str;
            }
            if (value is double d)
            {
                string str = d.ToString("F2");
                // Truncate very long numbers
                if (str.Length > 15) str = str.Substring(0, 12) + "...";
                return str;
            }
            if (value is long l)
            {
                string str = l.ToString();
                if (str.Length > 15) str = str.Substring(0, 12) + "...";
                return str;
            }
            if (value is string s)
            {
                if (s.Length > 25) return s.Substring(0, 22) + "...";
                return s;
            }
            
            // Handle enums - return the enum value name
            if (value.GetType().IsEnum)
            {
                return value.ToString();
            }
            
            // Handle dictionaries - show count
            if (value is System.Collections.IDictionary dict)
            {
                if (dict.Count == 0) return "[empty]";
                return "(" + dict.Count + " entries)";
            }
            
            // Handle lists - show count
            if (value is System.Collections.IList list)
            {
                if (list.Count == 0) return "[empty]";
                return "(" + list.Count + " items)";
            }
            
            // Handle arrays - show count
            if (value is Array arr)
            {
                if (arr.Length == 0) return "[empty]";
                return "(" + arr.Length + " items)";
            }
            
            // For all other complex objects, try to get readable name
            return GetReadableObjectName(value);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Gets a nested field value by traversing the path
    /// </summary>
    private static object GetNestedFieldValue(string fieldPath)
    {
        try
        {
            string[] parts = fieldPath.Split('.');
            if (parts.Length < 1) return null;
            
            // Get the root object (DataManager.Instance)
            object current = DataManager.Instance;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            
            // Traverse to the final object
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                
                // Check for array/list index (e.g., "items[0]")
                int bracketIndex = part.IndexOf('[');
                if (bracketIndex >= 0)
                {
                    string listName = part.Substring(0, bracketIndex);
                    string indexStr = part.Substring(bracketIndex + 1, part.IndexOf(']') - bracketIndex - 1);
                    int index = int.Parse(indexStr);
                    
                    var listField = current.GetType().GetField(listName, flags);
                    var listProp = current.GetType().GetProperty(listName, flags);
                    
                    object list = listField != null ? listField.GetValue(current) : (listProp != null ? listProp.GetValue(current) : null);
                    if (list is System.Collections.IList listObj && index < listObj.Count)
                    {
                        current = listObj[index];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var field = current.GetType().GetField(part, flags);
                    var prop = current.GetType().GetProperty(part, flags);
                    
                    current = field != null ? field.GetValue(current) : (prop != null ? prop.GetValue(current) : null);
                    if (current == null) return null;
                }
            }
            
            return current;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gets a readable name from an object
    /// </summary>
    private static string GetReadableObjectName(object obj)
    {
        if (obj == null) return "null";
        
        // Try to get Name property
        var nameProp = obj.GetType().GetProperty("Name");
        if (nameProp != null)
        {
            var name = nameProp.GetValue(obj);
            if (name != null) return name.ToString();
        }
        
        // Try to get ID property
        var idProp = obj.GetType().GetProperty("ID");
        if (idProp != null)
        {
            var id = idProp.GetValue(obj);
            if (id != null) return id.ToString();
        }
        
        // Try to get Type property (common for inventory items)
        var typeProp = obj.GetType().GetProperty("Type");
        if (typeProp != null)
        {
            var type = typeProp.GetValue(obj);
            if (type != null) return type.ToString();
        }
        
        // Return type name for complex objects
        return $"[{obj.GetType().Name}]";
    }
    
    /// <summary>
    /// Gets detailed field value with actual names for lists/arrays (for detail panel)
    /// </summary>
    private static string GetFieldValueDetailed(SaveFieldInfo field)
    {
        try
        {
            if (DataManager.Instance == null) return "No Data";
            
            object value = null;
            
            // Check if this is a nested field (contains '.' in name)
            if (field.Name.Contains("."))
            {
                value = GetNestedFieldValue(field.Name);
            }
            else
            {
                if (field.FieldInfo != null)
                {
                    value = field.FieldInfo.GetValue(DataManager.Instance);
                }
                else if (field.PropertyInfo != null)
                {
                    value = field.PropertyInfo.GetValue(DataManager.Instance, null);
                }
            }
            
            if (value == null) return "null";
            
            // Handle different types
            if (value is bool b) return b ? "True" : "False";
            if (value is int i) return i.ToString();
            if (value is float f) return f.ToString("F2");
            if (value is double d) return d.ToString("F2");
            if (value is long l) return l.ToString();
            if (value is string s) return s;
            
            // Handle enums - return the enum value name
            if (value.GetType().IsEnum)
            {
                return value.ToString();
            }
            
            // Handle lists - show actual items
            if (value is System.Collections.IList list)
            {
                if (list.Count == 0) return "[empty]";
                return GetListItemsDetailed(list);
            }
            
            // Handle arrays - show actual items
            if (value is Array arr)
            {
                if (arr.Length == 0) return "[empty]";
                return GetArrayItemsDetailed(arr);
            }
            
            // For all other complex objects, show type name
            return $"[{value.GetType().Name}]";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Gets detailed list items with actual names
    /// </summary>
    private static string GetListItemsDetailed(System.Collections.IList list)
    {
        if (list.Count == 0) return "[empty]";
        
        var items = new List<string>();
        for (int idx = 0; idx < list.Count; idx++)
        {
            items.Add($"{idx}: {GetObjectName(list[idx])}");
        }
        return string.Join("\n", items);
    }
    
    /// <summary>
    /// Gets detailed array items with actual names
    /// </summary>
    private static string GetArrayItemsDetailed(Array arr)
    {
        if (arr.Length == 0) return "[empty]";
        
        var items = new List<string>();
        for (int idx = 0; idx < arr.Length; idx++)
        {
            items.Add($"{idx}: {GetObjectName(arr.GetValue(idx))}");
        }
        return string.Join("\n", items);
    }
    
    /// <summary>
    /// Gets the name or ID from an object for display
    /// </summary>
    private static string GetObjectName(object item)
    {
        if (item == null) return "null";
        
        // Handle enums - return the enum value name
        if (item.GetType().IsEnum)
        {
            return item.ToString();
        }
        
        // Try to get common properties that contain names/IDs
        string[] nameProperties = { "Name", "ID", "SkinName", "DisplayName", "Title", "CharacterName" };
        
        foreach (var propName in nameProperties)
        {
            try
            {
                var prop = item.GetType().GetProperty(propName);
                if (prop != null)
                {
                    var val = prop.GetValue(item, null);
                    if (val != null)
                        return val.ToString();
                }
            }
            catch { }
        }
        
        // Try fields
        string[] nameFields = { "Name", "ID", "SkinName", "DisplayName" };
        foreach (var fieldName in nameFields)
        {
            try
            {
                var field = item.GetType().GetField(fieldName);
                if (field != null)
                {
                    var val = field.GetValue(item);
                    if (val != null)
                        return val.ToString();
                }
            }
            catch { }
        }
        
        // Fall back to type name
        return item.GetType().Name;
    }
    
    /// <summary>
    /// Gets default value for a type
    /// </summary>
    private static object GetDefaultValue(Type type)
    {
        if (type == null || type == typeof(string))
            return "";
        if (type == typeof(int))
            return 0;
        if (type == typeof(float))
            return 0f;
        if (type == typeof(double))
            return 0.0;
        if (type == typeof(bool))
            return false;
        if (type.IsEnum)
            return Enum.GetValues(type).GetValue(0);
        // For complex types, return null (user will need to edit manually)
        return null;
    }
    
    // Convert a list item to string representation
    private static string ListItemToString(object item)
    {
        if (item == null) return "null";
        if (item is bool b) return b ? "T" : "F";
        if (item is int i) return i.ToString();
        if (item is float f) return f.ToString("F1");
        if (item is double d) return d.ToString("F1");
        if (item is long l) return l.ToString();
        if (item is string s) return $"\"{s}\"";
        
        // Try to get the name or ID from game objects
        try
        {
            // Check for common properties like Name, ID, etc.
            var nameProp = item.GetType().GetProperty("Name");
            if (nameProp != null)
            {
                var name = nameProp.GetValue(item, null);
                if (name != null) return name.ToString();
            }
            
            var idProp = item.GetType().GetProperty("ID");
            if (idProp != null)
            {
                var id = idProp.GetValue(item, null);
                if (id != null) return id.ToString();
            }
        }
        catch { }
        
        // Fallback to type name
        string typeName = item.GetType().Name;
        return typeName;
    }
    
    private static void SetFieldValue(SaveFieldInfo field, string valueStr)
    {
        try
        {
            if (DataManager.Instance == null) return;
            
            object convertedValue = null;
            Type targetType = field.FieldType;
            
            // Handle nullable types
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }
            
            if (targetType == typeof(bool))
            {
                convertedValue = valueStr.ToLower() == "true" || valueStr == "1";
            }
            else if (targetType == typeof(int))
            {
                convertedValue = int.Parse(valueStr);
            }
            else if (targetType == typeof(float))
            {
                convertedValue = float.Parse(valueStr);
            }
            else if (targetType == typeof(double))
            {
                convertedValue = double.Parse(valueStr);
            }
            else if (targetType == typeof(long))
            {
                convertedValue = long.Parse(valueStr);
            }
            else if (targetType == typeof(string))
            {
                convertedValue = valueStr;
            }
            else if (targetType == typeof(byte))
            {
                convertedValue = byte.Parse(valueStr);
            }
            else if (targetType == typeof(char))
            {
                convertedValue = char.Parse(valueStr);
            }
            else
            {
                ShowStatus("Unsupported type: " + targetType.Name);
                return;
            }
            
            // Use ParentObject if set (for MetaData fields), otherwise use DataManager
            object parentObj = field.ParentObject ?? DataManager.Instance;
            
            // Check if this is a nested field (contains '.' in name)
            if (field.Name.Contains("."))
            {
                // Handle nested field setting
                SetNestedFieldValue(field.Name, convertedValue, targetType);
            }
            else if (field.FieldInfo != null)
            {
                field.FieldInfo.SetValue(parentObj, convertedValue);
            }
            else if (field.PropertyInfo != null)
            {
                field.PropertyInfo.SetValue(parentObj, convertedValue);
            }
            
            ShowStatus("Value updated! Saving...");
            
            // Auto-save after changes
            try {
                SaveCurrentChanges();
            } catch { }
            
            // Refresh the display
            _editValue = GetFieldValue(field);
        }
        catch (Exception ex)
        {
            ShowStatus($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sets a nested field value by traversing the path
    /// </summary>
    private static void SetNestedFieldValue(string fieldPath, object value, Type valueType)
    {
        try
        {
            string[] parts = fieldPath.Split('.');
            if (parts.Length < 2) return;
            
            // Get the root object (DataManager.Instance)
            object current = DataManager.Instance;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            
            // Traverse to the parent object
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];
                
                // Check for array/list index (e.g., "items[0]")
                int bracketIndex = part.IndexOf('[');
                if (bracketIndex >= 0)
                {
                    string listName = part.Substring(0, bracketIndex);
                    string indexStr = part.Substring(bracketIndex + 1, part.IndexOf(']') - bracketIndex - 1);
                    int index = int.Parse(indexStr);
                    
                    var listField = current.GetType().GetField(listName, flags);
                    var listProp = current.GetType().GetProperty(listName, flags);
                    
                    object list = listField != null ? listField.GetValue(current) : (listProp != null ? listProp.GetValue(current) : null);
                    if (list is System.Collections.IList listObj)
                    {
                        current = listObj[index];
                    }
                    else
                    {
                        return; // Not a list
                    }
                }
                else
                {
                    var field = current.GetType().GetField(part, flags);
                    var prop = current.GetType().GetProperty(part, flags);
                    
                    current = field != null ? field.GetValue(current) : (prop != null ? prop.GetValue(current) : null);
                    if (current == null) return;
                }
            }
            
            // Set the value on the final object
            string lastPart = parts[parts.Length - 1];
            int lastBracketIndex = lastPart.IndexOf('[');
            
            if (lastBracketIndex >= 0)
            {
                // Setting an array/list element
                string listName = lastPart.Substring(0, lastBracketIndex);
                string indexStr = lastPart.Substring(lastBracketIndex + 1, lastPart.IndexOf(']') - lastBracketIndex - 1);
                int index = int.Parse(indexStr);
                
                var listField = current.GetType().GetField(listName, flags);
                var listProp = current.GetType().GetProperty(listName, flags);
                
                object list = listField != null ? listField.GetValue(current) : (listProp != null ? listProp.GetValue(current) : null);
                if (list is System.Collections.IList listObj)
                {
                    listObj[index] = value;
                }
            }
            else
            {
                // Setting a regular field/property
                var field = current.GetType().GetField(lastPart, flags);
                var prop = current.GetType().GetProperty(lastPart, flags);
                
                if (field != null)
                {
                    field.SetValue(current, value);
                }
                else if (prop != null)
                {
                    prop.SetValue(current, value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveEditor] SetNestedFieldValue error: {ex.Message}");
        }
    }
    
    private static void ShowStatus(string message)
    {
        _statusMessage = message;
        _statusTimer = 3.0f;
    }
    
    public static void Close()
    {
        UnityEngine.Debug.Log("[SaveEditor] Close() called");
        
        // Check if we're in-game BEFORE unloading (since unload sets Loaded=false)
        bool wasInGame = CultUtils.IsInGame();
        
        // CRITICAL: Set IsOpen to false FIRST to prevent any callback loops
        // This must be done before any other cleanup to avoid re-triggering the editor
        IsOpen = false;
        
        // Re-enable the EventSystem before closing (it was disabled when loading a save)
        ReEnableEventSystem();
        
        // Reset state for next open
        _isAtSaveSelection = true;
        _selectedSaveIndex = -1;
        _selectedField = null;
        
        UnityEngine.Debug.Log("[SaveEditor] Closed successfully");
    }
    
    /// <summary>
    /// Re-enables the EventSystem after closing the save editor
    /// </summary>
    private static void ReEnableEventSystem()
    {
        try
        {
            var eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            if (eventSystemType != null)
            {
                var eventSystem = UnityEngine.Object.FindObjectOfType(eventSystemType);
                if (eventSystem != null)
                {
                    // Re-enable EventSystem
                    var enabledProperty = eventSystemType.GetProperty("enabled");
                    if (enabledProperty != null)
                    {
                        enabledProperty.SetValue(eventSystem, true);
                        UnityEngine.Debug.Log("[SaveEditor] EventSystem re-enabled");
                    }
                    
                    // Try to re-enable input modules
                    var inputModulesField = eventSystemType.GetField("m_SystemInputModules");
                    if (inputModulesField != null)
                    {
                        var inputModules = inputModulesField.GetValue(eventSystem) as UnityEngine.Component[];
                        if (inputModules != null)
                        {
                            foreach (var module in inputModules)
                            {
                                var moduleType = module.GetType();
                                var moduleEnabled = moduleType.GetProperty("enabled");
                                if (moduleEnabled != null)
                                {
                                    moduleEnabled.SetValue(module, true);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error re-enabling EventSystem: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Export complete save to jsonSaves folder (as .mp file copy for complete data)
    /// </summary>
    private static void ExportCompleteSave()
    {
        try
        {
            string pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string jsonSavesPath = Path.Combine(pluginsPath, "jsonSaves");
            
            if (!Directory.Exists(jsonSavesPath))
                Directory.CreateDirectory(jsonSavesPath);
            
            // Get current save slot
            int slot = 0;
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                if (saveSlotField != null)
                    slot = (int)saveSlotField.GetValue(null);
            }
            
            string savesPath = GetSavesPath();
            if (string.IsNullOrEmpty(savesPath))
            {
                ShowStatus("Could not find saves path");
                return;
            }
            
            // Copy slot file (game uses slot_X.mp format)
            string mpFile = Path.Combine(savesPath, $"slot_{slot}.mp");
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string destMp = Path.Combine(jsonSavesPath, $"slot_{slot}_{timestamp}.mp");
            
            if (File.Exists(mpFile))
            {
                File.Copy(mpFile, destMp, true);
                UnityEngine.Debug.Log($"[SaveEditor] Copied save to: {destMp}");
            }
            else
            {
                // Try alternate naming (save_X.mp)
                mpFile = Path.Combine(savesPath, $"save_{slot}.mp");
                if (File.Exists(mpFile))
                {
                    destMp = Path.Combine(jsonSavesPath, $"save_{slot}_{timestamp}.mp");
                    File.Copy(mpFile, destMp, true);
                    UnityEngine.Debug.Log($"[SaveEditor] Copied save to: {destMp}");
                }
            }
            
            // Copy meta file if exists (slot_X.mp or meta_X.mp)
            string metaFile = Path.Combine(savesPath, $"meta_{slot}.mp");
            string destMeta = Path.Combine(jsonSavesPath, $"meta_{slot}_{timestamp}.mp");
            
            if (File.Exists(metaFile))
            {
                File.Copy(metaFile, destMeta, true);
                UnityEngine.Debug.Log($"[SaveEditor] Copied meta to: {destMeta}");
            }
            
            ShowStatus($"Complete save exported! ({timestamp})");
        }
        catch (Exception ex)
        {
            ShowStatus($"Export failed: {ex.Message}");
            UnityEngine.Debug.LogError($"[SaveEditor] Export error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Import complete save from jsonSaves folder (copies .mp file back)
    /// </summary>
    private static void ImportCompleteSave(string mpFilePath)
    {
        try
        {
            string savesPath = GetSavesPath();
            if (string.IsNullOrEmpty(savesPath))
            {
                ShowStatus("Could not find saves path");
                return;
            }
            
            // Get current save slot
            int slot = 0;
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                if (saveSlotField != null)
                    slot = (int)saveSlotField.GetValue(null);
            }
            
            // Copy .mp file - handle both slot_X.mp and save_X.mp formats
            string destMp = Path.Combine(savesPath, $"save_{slot}.mp");
            
            // Check if the imported file is a slot_X or save_X format
            string importedFileName = Path.GetFileName(mpFilePath).ToLower();
            if (importedFileName.Contains("slot_"))
            {
                // If importing slot_X, write to slot_X
                destMp = Path.Combine(savesPath, $"slot_{slot}.mp");
            }
            
            File.Copy(mpFilePath, destMp, true);
            UnityEngine.Debug.Log($"[SaveEditor] Copied save from: {mpFilePath} to {destMp}");
            
            // Copy meta file - try both slot_X.mp and meta_X.mp naming
            // Look for meta file in the import folder with matching slot/timestamp
            string importedBaseName = Path.GetFileNameWithoutExtension(mpFilePath);
            string importDir = Path.GetDirectoryName(mpFilePath);
            
            string metaFilePath = Path.Combine(importDir, $"meta_{slot}.mp");
            
            // Also try with the original filename pattern
            if (!File.Exists(metaFilePath))
            {
                // Try to find meta file with same base name
                string altMetaPath = importedFileName.Replace("save_", "meta_").Replace("slot_", "meta_");
                metaFilePath = Path.Combine(importDir, altMetaPath);
                if (!File.Exists(metaFilePath))
                {
                    metaFilePath = Path.ChangeExtension(mpFilePath, ".mp").Replace("save_", "meta_").Replace("slot_", "meta_");
                }
            }
            
            if (File.Exists(metaFilePath))
            {
                string destMeta = Path.Combine(savesPath, $"meta_{slot}.mp");
                File.Copy(metaFilePath, destMeta, true);
                UnityEngine.Debug.Log($"[SaveEditor] Copied meta from: {metaFilePath} to {destMeta}");
            }
            
            ShowStatus("Complete save imported! Reloading...");
            
            // Reload the save
            ReloadCurrentSave();
        }
        catch (Exception ex)
        {
            ShowStatus($"Import failed: {ex.Message}");
            UnityEngine.Debug.LogError($"[SaveEditor] Import error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reload the current save slot
    /// </summary>
    private static void ReloadCurrentSave()
    {
        try
        {
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                // Get SAVE_SLOT
                int slot = 0;
                var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                if (saveSlotField != null)
                    slot = (int)saveSlotField.GetValue(null);
                
                // Call Load method
                var loadMethod = saveAndLoadType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
                if (loadMethod != null)
                {
                    loadMethod.Invoke(null, new object[] { slot });
                    UnityEngine.Debug.Log($"[SaveEditor] Reloaded save slot {slot}");
                    return;
                }
                
                // Try LoadGame
                var loadGameMethod = saveAndLoadType.GetMethod("LoadGame", BindingFlags.Public | BindingFlags.Static);
                if (loadGameMethod != null)
                {
                    loadGameMethod.Invoke(null, new object[] { slot });
                    UnityEngine.Debug.Log($"[SaveEditor] Reloaded save slot {slot} via LoadGame");
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[SaveEditor] Reload error: {ex.Message}");
        }
    }
    private static void SaveCurrentChanges()
    {
        // First ensure AllowSaving is enabled and DisableSaving is disabled
        try
        {
            if (DataManager.Instance != null)
            {
                var allowSavingField = typeof(DataManager).GetField("AllowSaving", BindingFlags.Public | BindingFlags.Instance);
                var disableSavingField = typeof(DataManager).GetField("DisableSaving", BindingFlags.Public | BindingFlags.Instance);
                
                if (allowSavingField != null)
                {
                    allowSavingField.SetValue(DataManager.Instance, true);
                }
                if (disableSavingField != null)
                {
                    disableSavingField.SetValue(DataManager.Instance, false);
                }
                UnityEngine.Debug.Log("[SaveEditor] Set AllowSaving=true, DisableSaving=false");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Failed to set save flags: {ex.Message}");
        }
        
        // Try invoking the private Saving() method directly (this does the actual work)
        try
        {
            UnityEngine.Debug.Log("[SaveEditor] Attempting to invoke Saving() method directly...");
            
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                // Get the private Saving method
                var savingMethod = saveAndLoadType.GetMethod("Saving", BindingFlags.NonPublic | BindingFlags.Static);
                if (savingMethod != null)
                {
                    savingMethod.Invoke(null, null);
                    ShowStatus("Game saved!");
                    UnityEngine.Debug.Log("[SaveEditor] Saving() method invoked successfully");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Saving() direct invoke failed: {ex.GetType().Name} - {ex.Message}");
        }
        
        // Try using COTLDataReadWriter directly (this is what the game uses internally)
        try
        {
            UnityEngine.Debug.Log("[SaveEditor] Attempting COTLDataReadWriter save...");
            
            // Get the COTLDataReadWriter type
            var cotlDataReadWriterType = Type.GetType("COTLDataReadWriter`1, Assembly-CSharp");
            if (cotlDataReadWriterType != null && DataManager.Instance != null)
            {
                // Create a generic COTLDataReadWriter<DataManager> instance
                var genericType = cotlDataReadWriterType.MakeGenericType(typeof(DataManager));
                var instance = Activator.CreateInstance(genericType);
                
                // Get current slot
                var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
                int slot = 0;
                if (saveAndLoadType != null)
                {
                    var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                    if (saveSlotField != null)
                    {
                        slot = (int)saveSlotField.GetValue(null);
                    }
                }
                
                // Build save filename (game uses slot_X.json which gets saved as .mp internally)
                string saveFilename = string.Format("slot_{0}.json", slot);
                
                // Get the Write method
                var writeMethod = genericType.GetMethod("Write", new[] { typeof(DataManager), typeof(string), typeof(bool), typeof(bool) });
                if (writeMethod != null)
                {
                    UnityEngine.Debug.Log($"[SaveEditor] Calling COTLDataReadWriter<DataManager>.Write({saveFilename}, true, true)");
                    
                    // Write with encryption and backup
                    writeMethod.Invoke(instance, new object[] { DataManager.Instance, saveFilename, true, true });
                    
                    // Also save meta data - use the game's SaveAndLoad system!
                    try {
                        var saveAndLoadTypeRef = typeof(DataManager).Assembly.GetType("SaveAndLoad");
                        if (saveAndLoadTypeRef != null) {
                            // Get Singleton<SaveAndLoad>.Instance
                            var singletonType = typeof(DataManager).Assembly.GetType("Singleton`1").MakeGenericType(saveAndLoadTypeRef);
                            var instanceProp = singletonType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                            if (instanceProp != null) {
                                var saveAndLoadInstance = instanceProp.GetValue(null);
                                if (saveAndLoadInstance != null) {
                                    // Get _metaReadWriter field
                                    var metaReadWriterField = saveAndLoadTypeRef.GetField("_metaReadWriter", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (metaReadWriterField != null) {
                                        var metaReadWriter = metaReadWriterField.GetValue(saveAndLoadInstance);
                                        if (metaReadWriter != null) {
                                            // Get MetaData from DataManager
                                            var metaProperty = typeof(DataManager).GetProperty("MetaData", BindingFlags.Public | BindingFlags.Instance);
                                            var metaData = metaProperty?.GetValue(DataManager.Instance);
                                            if (metaData != null) {
                                                // Call Write method: Write(MetaData data, string fileName, bool createBackup, bool saveAsync)
                                                var writeMethodRef = metaReadWriter.GetType().GetMethod("Write", new[] { typeof(object), typeof(string), typeof(bool), typeof(bool) });
                                                if (writeMethodRef != null) {
                                                    string metaFilename = $"meta_{slot}.json";
                                                    writeMethodRef.Invoke(metaReadWriter, new object[] { metaData, metaFilename, true, true });
                                                    UnityEngine.Debug.Log($"[SaveEditor] Saved meta via SaveAndLoad to {metaFilename}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        UnityEngine.Debug.LogWarning($"[SaveEditor] Error using SaveAndLoad: {ex.Message}");
                    }
                    
                    ShowStatus($"Saved to slot {slot}!");
                    UnityEngine.Debug.Log($"[SaveEditor] COTLDataReadWriter save successful to {saveFilename}");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] COTLDataReadWriter save failed: {ex.GetType().Name} - {ex.Message}");
            UnityEngine.Debug.LogWarning($"[SaveEditor] Stack trace: {ex.StackTrace}");
        }
        
        // Try calling SaveAndLoad.Save() as fallback
        try
        {
            UnityEngine.Debug.Log("[SaveEditor] Attempting SaveAndLoad.Save()...");
            
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var saveMethod = saveAndLoadType.GetMethod("Save", BindingFlags.Public | BindingFlags.Static);
                if (saveMethod != null)
                {
                    saveMethod.Invoke(null, null);
                    ShowStatus("Game saved!");
                    UnityEngine.Debug.Log("[SaveEditor] Game save method successful");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] SaveAndLoad.Save() failed: {ex.GetType().Name} - {ex.Message}");
        }
        
        UnityEngine.Debug.LogWarning("[SaveEditor] All save methods failed");
        ShowStatus("Save failed! Check logs.");
    }

    /// <summary>
    /// Unloads the save (sets Loaded=false) without resetting DataManager to avoid rendering issues
    /// </summary>
    private static void UnloadSaveAndData()
    {
        try
        {
            // Reset SaveAndLoad.Loaded to prevent game from thinking we're in-game
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var loadedField = saveAndLoadType.GetField("Loaded", BindingFlags.Static | BindingFlags.Public);
                if (loadedField != null)
                {
                    loadedField.SetValue(null, false);
                    UnityEngine.Debug.Log("[SaveEditor] Reset SaveAndLoad.Loaded = false");
                }
                
                // Also reset SAVE_SLOT to default
                var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                if (saveSlotField != null)
                {
                    saveSlotField.SetValue(null, 5);
                }
            }
            
            UnityEngine.Debug.Log("[SaveEditor] Save unloaded");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error unloading save: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Unloads the DataManager by clearing its instance
    /// </summary>
    private static void UnloadDataManager()
    {
        try
        {
            var dataManagerType = typeof(DataManager);
            
            // Get the Instance property
            var instanceProperty = dataManagerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            if (instanceProperty != null)
            {
                // Set Instance to null to unload
                instanceProperty.SetValue(null, null);
                UnityEngine.Debug.Log("[SaveEditor] DataManager.Instance set to null");
            }
            
            UnityEngine.Debug.Log("[SaveEditor] DataManager unloaded");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error unloading DataManager: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Resets the game state to prevent errors when returning to main menu
    /// </summary>
    private static void ResetGameState()
    {
        try
        {
            // Reset SaveAndLoad.Loaded to prevent game from thinking we're in-game
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var loadedField = saveAndLoadType.GetField("Loaded", BindingFlags.Static | BindingFlags.Public);
                if (loadedField != null)
                {
                    loadedField.SetValue(null, false);
                    UnityEngine.Debug.Log("[SaveEditor] Reset SaveAndLoad.Loaded = false");
                }
                
                // Also reset SAVE_SLOT to default
                var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                if (saveSlotField != null)
                {
                    saveSlotField.SetValue(null, 5);
                }
            }
            
            // NOTE: Do NOT call DataManager.ResetData() as it can cause softlocks
            // The scene reload will handle clearing the data properly
            // Just disable dark mode toggle to prevent freeze
            DisableDarkMode();
            
            UnityEngine.Debug.Log("[SaveEditor] Game state reset complete");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error resetting game state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Disables dark mode by finding and toggling off the dark mode setting
    /// </summary>
    private static void DisableDarkMode()
    {
        try
        {
            // Find AccessibilitySettings in the scene
            var accessibilitySettingsType = Type.GetType("Lamb.UI.SettingsMenu.AccessibilitySettings, Assembly-CSharp");
            if (accessibilitySettingsType == null)
            {
                // Try alternate assembly name
                accessibilitySettingsType = Type.GetType("Lamb.UI.AccessibilitySettings, Assembly-CSharp");
            }
            
            if (accessibilitySettingsType != null)
            {
                // Find all AccessibilitySettings instances in the scene
                var instances = UnityEngine.Object.FindObjectsOfType(accessibilitySettingsType);
                foreach (var instance in instances)
                {
                    // Try to find dark mode field/property
                    var darkModeField = accessibilitySettingsType.GetField("darkMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (darkModeField != null)
                    {
                        darkModeField.SetValue(instance, false);
                        UnityEngine.Debug.Log("[SaveEditor] Disabled dark mode via field");
                    }
                    else
                    {
                        var darkModeProperty = accessibilitySettingsType.GetProperty("darkMode", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (darkModeProperty != null)
                        {
                            darkModeProperty.SetValue(instance, false);
                            UnityEngine.Debug.Log("[SaveEditor] Disabled dark mode via property");
                        }
                    }
                    
                    // Just set the darkMode field to false directly WITHOUT calling OnDarkModeToggleValueChanged
                    // because that would trigger our patch and re-open the save editor!
                    if (darkModeField != null)
                    {
                        darkModeField.SetValue(instance, false);
                        UnityEngine.Debug.Log("[SaveEditor] Disabled dark mode via field (no toggle change)");
                    }
                    else
                    {
                        var darkModeProperty = accessibilitySettingsType.GetProperty("darkMode", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (darkModeProperty != null)
                        {
                            darkModeProperty.SetValue(instance, false);
                            UnityEngine.Debug.Log("[SaveEditor] Disabled dark mode via property (no toggle change)");
                        }
                    }
                    
                    // DO NOT call OnDarkModeToggleValueChanged here - it would re-open the save editor!
                    // The dark mode setting will be updated when the user next opens settings
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[SaveEditor] Could not find AccessibilitySettings type");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error disabling dark mode: {ex.Message}");
        }
    }
    
    public static void Draw()
    {
        if (!IsOpen) return;
        
        // Ensure window has focus
        GUI.FocusWindow(999999);
        
        // At save selection, we don't need DataManager
        if (_isAtSaveSelection)
        {
            DrawSaveSelectionScreen();
            return;
        }
        
        // When editing, we need DataManager - validate it's loaded
        if (DataManager.Instance == null) 
        {
            // Draw error message
            float errX = (Screen.width - 400) / 2;
            float errY = (Screen.height - 100) / 2;
            
            GUI.color = CULT_DARK_RED;
            GUI.DrawTexture(new Rect(errX, errY, 400, 100), Texture2D.whiteTexture);
            GUI.color = CULT_BONE_WHITE;
            GUI.Label(new Rect(errX + 20, errY + 20, 360, 30), "Data Not Loaded");
            GUI.Label(new Rect(errX + 20, errY + 50, 360, 30), "DataManager is not available");
            
            UnityEngine.Debug.LogError("[SaveEditor] DataManager.Instance is null - cannot draw editor");
            return;
        }
        
        // Validate that fields are cached
        if (_allFields == null || _allFields.Count == 0)
        {
            // Cache fields if not already done
            CacheAllFields();
            if (_allFields == null || _allFields.Count == 0)
            {
                UnityEngine.Debug.LogError("[SaveEditor] No fields cached - cannot draw editor");
                return;
            }
            ApplyFilter();
        }
        
        // Center the window on screen with dynamic dimensions
        float x = (Screen.width - _windowWidth) / 2;
        float y = (Screen.height - _windowHeight) / 2;
        
        // Draw background
        GUI.color = CULT_DARK_RED;
        GUI.DrawTexture(new Rect(x, y, _windowWidth, _windowHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        Rect windowRect = new Rect(x, y, _windowWidth, _windowHeight);
        GUI.Window(999999, windowRect, DrawWindow, "Save Editor - Cult of the Lamb");
    }
    
    private static void DrawSaveSelectionScreen()
    {
        // Center the window on screen with dynamic dimensions
        float x = (Screen.width - _saveSelectionWidth) / 2;
        float y = (Screen.height - _saveSelectionHeight) / 2;
        
        // Draw background
        GUI.color = CULT_DARK_RED;
        GUI.DrawTexture(new Rect(x, y, _saveSelectionWidth, _saveSelectionHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        Rect windowRect = new Rect(x, y, _saveSelectionWidth, _saveSelectionHeight);
        GUI.Window(999999, windowRect, DrawSaveFileSelectionWindow, "Save Editor - Select Save");
    }
    
    private static void DrawSaveFileSelectionWindow(int windowId)
    {
        // Draw close button
        Rect closeButtonRect = new Rect(_saveSelectionWidth - 35, 8, 25, 25);
        GUI.color = CULT_RED;
        if (GUI.Button(closeButtonRect, "X"))
        {
            Close();
            return;
        }
        GUI.color = Color.white;
        
        DrawSaveFileSelection();
    }
    
    private static void DrawSaveEditorWindow()
    {
        // Center the window on screen with dynamic dimensions
        float x = (Screen.width - _windowWidth) / 2;
        float y = (Screen.height - _windowHeight) / 2;
        
        // Draw background
        GUI.color = CULT_DARK_RED;
        GUI.DrawTexture(new Rect(x, y, _windowWidth, _windowHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        Rect windowRect = new Rect(x, y, _windowWidth, _windowHeight);
        GUI.Window(999999, windowRect, DrawWindow, "Save Editor - Cult of the Lamb");
    }
    
    private static Vector2 _saveListScrollPosition = Vector2.zero;
    
    private static void DrawSaveFileSelection()
    {
        int contentY = 50;
        int contentHeight = _saveSelectionHeight - 80;
        int contentWidth = _saveSelectionWidth - 40;
        
        // Title
        GUI.color = CULT_GOLD;
        GUI.Label(new Rect(20, contentY, contentWidth, 30), "<size=20><b>Select Save File to Edit</b></size>");
        contentY += 40;
        
        // Instructions
        GUI.color = CULT_BONE_WHITE;
        GUI.Label(new Rect(20, contentY, contentWidth, 20), "Select a save file from the list below:");
        contentY += 30;
        
        // Save file count
        GUI.color = CULT_RED;
        GUI.Label(new Rect(20, contentY, contentWidth, 20), $"Found {_saveFiles.Count} save files");
        contentY += 30;
        
        // Save file list
        GUI.color = CULT_BLACK;
        GUI.Box(new Rect(20, contentY, contentWidth, contentHeight - 100), "");
        GUI.color = Color.white;
        
        _saveListScrollPosition = GUI.BeginScrollView(
            new Rect(25, contentY + 5, contentWidth - 10, contentHeight - 110),
            _saveListScrollPosition,
            new Rect(0, 0, contentWidth - 30, Math.Max(200, _saveFiles.Count * 50))
        );
        
        int saveY = 0;
        int saveHeight = 45;
        
        for (int i = 0; i < _saveFiles.Count; i++)
        {
            var save = _saveFiles[i];
            bool isSelected = (_selectedSaveIndex == i);
            
            // Background
            if (isSelected)
            {
                GUI.color = CULT_DARK_PURPLE;
            }
            else if (saveY % 2 == 0)
            {
                GUI.color = new Color(0.12f, 0.08f, 0.1f, 0.8f);
            }
            else
            {
                GUI.color = new Color(0.08f, 0.05f, 0.07f, 0.8f);
            }
            GUI.DrawTexture(new Rect(0, saveY, contentWidth - 30, saveHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // File name - wider column
            GUI.color = CULT_BONE_WHITE;
            GUI.Label(new Rect(10, saveY + 5, 300, 20), save.FileName);
            
            // Type indicator - wider column
            GUI.color = save.IsWoolhaven ? CULT_GOLD : CULT_RED;
            string typeText = save.IsWoolhaven ? "[Woolhaven]" : "[Base Game]";
            GUI.Label(new Rect(10, saveY + 25, 150, 18), typeText);
            
            // Info - more space
            GUI.color = CULT_GOLD;
            string infoText = $"{save.FileSize / 1024}KB - {save.LastModified:g}";
            GUI.Label(new Rect(contentWidth - 280, saveY + 10, 200, 18), infoText);
            
            // Select button
            GUI.color = isSelected ? CULT_GOLD : CULT_RED;
            if (GUI.Button(new Rect(contentWidth - 75, saveY + 10, 60, 25), "Select"))
            {
                _selectedSaveIndex = i;
            }
            
            saveY += saveHeight;
        }
        
        GUI.EndScrollView();
        
        // Bottom buttons - use absolute positioning near bottom of window
        int buttonY = _saveSelectionHeight - 60;
        
        // Bottom buttons - evenly spaced
        int buttonWidth = (_saveSelectionWidth - 60) / 2;
        int buttonHeight = 35;
        
        // Refresh button
        GUI.color = CULT_BLACK;
        if (GUI.Button(new Rect(20, buttonY, buttonWidth, buttonHeight), "Refresh Saves"))
        {
            ScanSaveFiles();
        }
        
        // Continue button
        if (_selectedSaveIndex >= 0)
        {
            GUI.color = CULT_RED;
            if (GUI.Button(new Rect(30 + buttonWidth, buttonY, buttonWidth, buttonHeight), "Edit Selected Save"))
            {
                LoadSelectedSave();
            }
        }
        else
        {
            GUI.color = Color.gray;
            GUI.Box(new Rect(30 + buttonWidth, buttonY, buttonWidth, buttonHeight), "Select a save");
        }
    }
    
    /// <summary>
    /// Tries to load a save file using the game's SaveAndLoad system
    /// </summary>
    private static bool TryLoadSaveFile(SaveFileInfo save)
    {
        try
        {
            // First check if we're in game - if so, DataManager already has data
            if (CultUtils.IsInGame())
            {
                UnityEngine.Debug.Log("[SaveEditor] Already in game, DataManager has data");
                return true;
            }
            
            // Try to find SaveAndLoad type and load method
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType == null)
            {
                UnityEngine.Debug.LogWarning("[SaveEditor] Could not find SaveAndLoad type");
                return false;
            }
            
            // Try to extract the slot number from the filename (e.g., slot_0.mp -> 0 or slot_0.json -> 0)
            int slotNumber = -1;
            string fileName = save.FileName.ToLower();
            if (fileName.Contains("slot_"))
            {
                // Extract slot number from filename like "slot_0.mp" or "slot_0.json"
                string numStr = fileName.Replace("slot_", "").Replace(".mp", "").Replace(".json", "").Trim();
                if (int.TryParse(numStr, out slotNumber))
                {
                    UnityEngine.Debug.Log($"[SaveEditor] Extracted slot number: {slotNumber}");
                }
            }
            
            // Try to find and call Load method with slot number (the correct method name is "Load", not "LoadGame")
            if (slotNumber >= 0)
            {
                var loadMethod = saveAndLoadType.GetMethod("Load", new[] { typeof(int) });
                if (loadMethod != null)
                {
                    UnityEngine.Debug.Log($"[SaveEditor] Calling SaveAndLoad.Load({slotNumber})");
                    loadMethod.Invoke(null, new object[] { slotNumber });
                    
                    // Also explicitly load the meta file for this slot
                    LoadMetaFileForSlot(slotNumber);
                    
                    // Wait a moment for loading to complete (async operation)
                    System.Threading.Thread.Sleep(500);
                    
                    // Check if save is now loaded
                    if (CultUtils.IsInGame())
                    {
                        UnityEngine.Debug.Log("[SaveEditor] Save loaded successfully!");
                        return true;
                    }
                }
            }
            
            // Try alternate method: LoadByPath(string path)
            var loadByPathMethod = saveAndLoadType.GetMethod("LoadByPath", new[] { typeof(string) });
            if (loadByPathMethod != null)
            {
                UnityEngine.Debug.Log($"[SaveEditor] Calling SaveAndLoad.LoadByPath with path: {save.FilePath}");
                loadByPathMethod.Invoke(null, new object[] { save.FilePath });
                
                System.Threading.Thread.Sleep(500);
                
                if (CultUtils.IsInGame())
                {
                    UnityEngine.Debug.Log("[SaveEditor] Save loaded successfully via LoadByPath!");
                    return true;
                }
            }
            
            // Try to get Singleton instance and call Load method on it
            var instanceProperty = saveAndLoadType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            if (instanceProperty != null)
            {
                var instance = instanceProperty.GetValue(null);
                if (instance != null)
                {
                    // Try Load(int) on instance
                    var loadIntMethod = instance.GetType().GetMethod("Load", new[] { typeof(int) });
                    if (loadIntMethod != null && slotNumber >= 0)
                    {
                        UnityEngine.Debug.Log($"[SaveEditor] Calling SaveAndLoad.Instance.Load({slotNumber})");
                        loadIntMethod.Invoke(instance, new object[] { slotNumber });
                        
                        System.Threading.Thread.Sleep(500);
                        
                        if (CultUtils.IsInGame())
                        {
                            UnityEngine.Debug.Log("[SaveEditor] Save loaded via Instance.Load()!");
                            return true;
                        }
                    }
                    
                    // Try LoadByPath on instance
                    var loadPathMethod = instance.GetType().GetMethod("LoadByPath", new[] { typeof(string) });
                    if (loadPathMethod != null)
                    {
                        UnityEngine.Debug.Log($"[SaveEditor] Calling SaveAndLoad.Instance.LoadByPath({save.FilePath})");
                        loadPathMethod.Invoke(instance, new object[] { save.FilePath });
                        
                        System.Threading.Thread.Sleep(500);
                        
                        if (CultUtils.IsInGame())
                        {
                            UnityEngine.Debug.Log("[SaveEditor] Save loaded via Instance.LoadByPath()!");
                            return true;
                        }
                    }
                }
            }
            
            UnityEngine.Debug.LogWarning("[SaveEditor] Could not find suitable load method");
            return false;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[SaveEditor] Error loading save: {ex.Message}");
            return false;
        }
    }
    
    private static void LoadSelectedSave()
    {
        if (_selectedSaveIndex < 0 || _selectedSaveIndex >= _saveFiles.Count)
            return;
        
        var save = _saveFiles[_selectedSaveIndex];
        
        // Check if it's a JSON file we exported
        if (save.FileName.EndsWith(".json"))
        {
            // For JSON files, we can try to load them
            ShowStatus("JSON save files can be imported");
        }
        else
        {
            // For .mp files - try to load the save using the game's save system
            bool loaded = TryLoadSaveFile(save);
            if (loaded)
            {
                // Validate data is properly loaded
                if (DataManager.Instance != null)
                {
                    ShowStatus("Save loaded! Data ready for editing.");
                }
                else
                {
                    ShowStatus("Warning: DataManager not available");
                }
            }
            else
            {
                ShowStatus("Could not load save - editing may not work from main menu");
            }
        }
        
        // Switch to edit mode - this will show the editor instead of selector
        _isAtSaveSelection = false;
        
        // Reset scroll positions for editor
        _listScrollPosition = Vector2.zero;
        _detailScrollPosition = Vector2.zero;
        
        // Disable Unity UI EventSystem to prevent it from interfering with our OnGUI
        try
        {
            // Use reflection to find and disable input modules
            var eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            if (eventSystemType != null)
            {
                var eventSystem = UnityEngine.Object.FindObjectOfType(eventSystemType);
                if (eventSystem != null)
                {
                    // Disable EventSystem
                    var enabledProperty = eventSystemType.GetProperty("enabled");
                    if (enabledProperty != null)
                    {
                        enabledProperty.SetValue(eventSystem, false);
                    }
                    
                    // Try to get and disable input modules
                    var inputModulesField = eventSystemType.GetField("m_SystemInputModules");
                    if (inputModulesField != null)
                    {
                        var inputModules = inputModulesField.GetValue(eventSystem) as UnityEngine.Component[];
                        if (inputModules != null)
                        {
                            foreach (var module in inputModules)
                            {
                                var moduleType = module.GetType();
                                var moduleEnabled = moduleType.GetProperty("enabled");
                                if (moduleEnabled != null)
                                {
                                    moduleEnabled.SetValue(module, false);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[SaveEditor] Error disabling EventSystem: {ex.Message}");
        }
        
        // Cache all fields from DataManager
        CacheAllFields();
        
        // Apply current filter
        ApplyFilter();
    }
    
    private static void DrawWindow(int windowId)
    {
        // Note: We don't consume events here because that would prevent our own GUI controls from working.
        // Instead, we rely on the window covering the entire screen to block the main menu.
        // The EventSystem is disabled in LoadSelectedSave() to prevent Unity UI from receiving input.
        
        // Make sure this window has focus
        GUI.FocusWindow(windowId);
        
        // Draw close button - moved to be with tabs
        Rect closeButtonRect = new Rect(_windowWidth - 35, 8, 25, 25);
        GUI.color = CULT_RED;
        if (GUI.Button(closeButtonRect, "X"))
        {
            Close();
            return;
        }
        GUI.color = Color.white;
        
        // Update status timer
        if (_statusTimer > 0)
        {
            _statusTimer -= Time.deltaTime;
        }
        
        // Draw tabs - 4 columns
        int columnsPerRow = 4;
        int buttonWidth = (_windowWidth - 50) / columnsPerRow;
        int buttonHeight = 28;
        
        // Check if user has Woolhaven DLC
        bool hasWoolhavenDLC = false;
        try {
            hasWoolhavenDLC = CultUtils.IsInGame() && DataManager.Instance.MAJOR_DLC;
        } catch { }
        
        // If user doesn't have DLC and is on Woolhaven tab, switch to All tab
        if (!hasWoolhavenDLC && _selectedTab == 10) // Woolhaven is at index 10
        {
            _selectedTab = 11; // Switch to All
        }
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < columnsPerRow; col++)
            {
                int index = row * columnsPerRow + col;
                if (index >= _tabs.Length) break;
                
                // Skip Woolhaven tab if user doesn't own the DLC
                string tabName = _tabs[index];
                if (tabName == "Woolhaven" && !hasWoolhavenDLC) continue;
                
                Rect buttonRect = new Rect(10 + col * buttonWidth, 8 + row * buttonHeight, buttonWidth - 5, buttonHeight - 2);
                bool isSelected = (_selectedTab == index);
                
                // Set background color
                if (isSelected)
                {
                    GUI.color = CULT_DARK_PURPLE;
                }
                else
                {
                    GUI.color = CULT_BLACK;
                }
                GUI.DrawTexture(buttonRect, Texture2D.whiteTexture);
                
                // Set text color (must be done AFTER drawing background)
                GUI.color = CULT_BONE_WHITE;
                
                // Use GUI.Button for proper click handling
                if (GUI.Button(buttonRect, _tabs[index]))
                {
                    _selectedTab = index;
                    ApplyFilter();
                }
                
                GUI.color = Color.white;
            }
        }
        
        // Search bar - below tabs
        int searchY = 8 + 3 * buttonHeight + 5;
        GUI.color = CULT_BLACK;
        GUI.DrawTexture(new Rect(10, searchY, _windowWidth - 20, 25), Texture2D.whiteTexture);
        GUI.color = Color.white;
        
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(15, searchY + 3, 60, 20), "Search:");
        _searchQuery = GUI.TextField(new Rect(75, searchY + 3, 250, 20), _searchQuery);
        
        Rect filterBtnRect = new Rect(330, searchY + 3, 70, 20);
        GUI.color = CULT_BLACK;
        GUI.DrawTexture(filterBtnRect, Texture2D.whiteTexture);
        GUI.color = CULT_BONE_WHITE;
        if (GUI.Button(filterBtnRect, "Filter"))
        {
            ApplyFilter();
        }
        
        // Status message
        if (_statusTimer > 0 && !string.IsNullOrEmpty(_statusMessage))
        {
            GUI.color = CULT_GOLD;
            GUI.Label(new Rect(410, searchY + 3, 300, 20), _statusMessage);
            GUI.color = Color.white;
        }
        
        // Field count
        GUI.color = CULT_BONE_WHITE;
        GUI.Label(new Rect(_windowWidth - 160, searchY + 3, 150, 20), $"{_displayedFields.Count} of {_allFields.Count}");
        GUI.color = Color.white;
        
        // Draw split view - left list, right detail - wider left panel
        int listY = searchY + 32;
        int listHeight = _windowHeight - listY - 50;
        int listWidth = (_windowWidth / 2) + 100; // Wider left panel
        int detailX = listWidth + 15;
        int detailWidth = _windowWidth - listWidth - 25;
        
        // Left panel - field list with dark background
        GUI.color = CULT_BLACK;
        GUI.Box(new Rect(10, listY, listWidth, listHeight), "");
        GUI.color = Color.white;
        
        // Column headers - moved Value and Edit columns to the right
        GUI.color = CULT_DARK_RED;
        GUI.DrawTexture(new Rect(10, listY, listWidth, 20), Texture2D.whiteTexture);
        GUI.color = CULT_BONE_WHITE;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(25, listY + 1, 380, 16), "Field");
        GUI.Label(new Rect(300, listY + 1, 200, 16), "Value");
        GUI.Label(new Rect(520, listY + 1, 50, 16), "Edit");
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        
        _listScrollPosition = GUI.BeginScrollView(
            new Rect(15, listY + 22, listWidth - 10, listHeight - 28),
            _listScrollPosition,
            new Rect(0, 0, listWidth - 30, Math.Max(600, _displayedFields.Count * 24))
        );
        
        int fieldY = 0;
        int rowHeight = 24;
        
        foreach (var field in _displayedFields)
        {
            // Get live value from game
            string liveValue = GetFieldValue(field);
            
            // Track non-null fields - if a field has ever been non-null, keep showing it
            bool isNull = (liveValue == "null" || liveValue == "[empty]");
            if (!isNull)
            {
                _nonNullFieldNames.Add(field.Name);
            }
            
            // Skip null values unless the field has been non-null before (or toggle is off)
            if (_hideNullValues && isNull && !_nonNullFieldNames.Contains(field.Name))
            {
                continue;
            }
            
            bool isSelected = (_selectedField == field);
            
            // Alternating row colors
            if (fieldY % 2 == 0)
            {
                GUI.color = new Color(0.12f, 0.08f, 0.1f, 0.8f);
            }
            else
            {
                GUI.color = new Color(0.08f, 0.05f, 0.07f, 0.8f);
            }
            GUI.DrawTexture(new Rect(0, fieldY, listWidth - 30, rowHeight), Texture2D.whiteTexture);
            
            // Highlight selected
            if (isSelected)
            {
                GUI.color = CULT_DARK_PURPLE;
                GUI.DrawTexture(new Rect(0, fieldY, listWidth - 30, rowHeight), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            
            // Field name - widened more for very long nested names
            Rect nameRect = new Rect(15, fieldY + 3, 380, 16);
            string displayName = field.Name;
            if (displayName.Length > 45)
            {
                displayName = displayName.Substring(0, 42) + "...";
            }
            GUI.color = CULT_BONE_WHITE;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.Label(nameRect, displayName);
            
            // Field value - LIVE from game
            if (liveValue.Length > 25)
            {
                liveValue = liveValue.Substring(0, 22) + "...";
            }
            
            // Moved Value column left - WIDENED to 200px
            Rect valueRect = new Rect(300, fieldY + 3, 200, 16);
            GUI.color = CULT_GOLD;
            GUI.Label(valueRect, liveValue);
            
            // Edit button
            Rect editBtnRect = new Rect(520, fieldY + 1, 50, 20);
            GUI.color = CULT_RED;
            if (GUI.Button(editBtnRect, "Edit"))
            {
                _selectedField = field;
                _editValue = GetFieldValue(field); // Get LIVE value
                _selectedNestedIndex = -1; // Reset nested selection when selecting new field
            }
            
            GUI.color = Color.white;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            
            fieldY += rowHeight;
        }
        
        GUI.EndScrollView();
        
        // Right panel - detail/edit area
        GUI.color = CULT_BLACK;
        GUI.Box(new Rect(detailX, listY, detailWidth, listHeight), "");
        GUI.color = Color.white;
        
        // Header with back button
        GUI.color = CULT_DARK_RED;
        GUI.DrawTexture(new Rect(detailX, listY, detailWidth, 22), Texture2D.whiteTexture);
        GUI.color = CULT_BONE_WHITE;
        
        // Back button on the left of header
        GUI.color = CULT_RED;
        if (GUI.Button(new Rect(detailX + 5, listY + 2, 50, 18), "< Back"))
        {
            _selectedField = null;
            _selectedNestedIndex = -1;
        }
        
        GUI.color = CULT_BONE_WHITE;
        GUI.Label(new Rect(detailX + 60, listY + 2, detailWidth - 70, 18), "Edit Value");
        GUI.color = Color.white;
        
        // Calculate dynamic scroll height based on field type
        int scrollHeight = 400;
        if (_selectedField != null)
        {
            var fieldType = _selectedField.FieldType;
            if (fieldType != null && (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))))
            {
                scrollHeight = 2000; // More height for lists/arrays with many items
            }
        }
        
        _detailScrollPosition = GUI.BeginScrollView(
            new Rect(detailX + 5, listY + 24, detailWidth - 10, listHeight - 30),
            _detailScrollPosition,
            new Rect(0, 0, detailWidth - 30, scrollHeight)
        );
        
        int editY = 0;
        
        if (_selectedField != null)
        {
            // Field info
            GUI.color = CULT_GOLD;
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), $"Field: {_selectedField.Name}");
            editY += 22;
            
            GUI.color = CULT_BONE_WHITE;
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), $"Type: {_selectedField.FieldType.Name}");
            editY += 22;
            
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), $"Category: {_selectedField.Category}");
            editY += 22;
            
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), $"Writable: {_selectedField.IsWritable}");
            editY += 30;
            
            // Current value - LIVE - show full string, toggle for bool
            GUI.color = CULT_DARK_RED;
            GUI.DrawTexture(new Rect(10, editY, detailWidth - 30, 25), Texture2D.whiteTexture);
            GUI.color = CULT_BONE_WHITE;
            GUI.Label(new Rect(15, editY + 3, detailWidth - 40, 20), "Current Value (LIVE):");
            editY += 28;
            
            // Get detailed value for display - shows actual names instead of counts
            string currentValue = GetFieldValueDetailed(_selectedField);
            
            // Check if it's a list - show expanded with edit buttons for each item
            if (_selectedField.FieldType.IsGenericType && _selectedField.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                try {
                    object listValue = null;
                    if (_selectedField.FieldInfo != null)
                        listValue = _selectedField.FieldInfo.GetValue(DataManager.Instance);
                    else if (_selectedField.PropertyInfo != null)
                        listValue = _selectedField.PropertyInfo.GetValue(DataManager.Instance, null);
                    
                    if (listValue is System.Collections.IList list)
                    {
                        GUI.color = CULT_GOLD;
                        // Show each list item with an edit button
                        for (int i = 0; i < list.Count; i++)
                        {
                            string itemName = GetObjectName(list[i]);
                            string itemDisplay = $"{i}: {itemName}";
                            
                            // Highlight selected item
                            if (_selectedNestedIndex == i && _selectedNestedPath == _selectedField.Name)
                            {
                                GUI.color = CULT_DARK_PURPLE;
                                GUI.DrawTexture(new Rect(10, editY, detailWidth - 30, 26), Texture2D.whiteTexture);
                            }
                            
                            // Show item name (truncate if too long)
                            if (itemDisplay.Length > 50)
                                itemDisplay = itemDisplay.Substring(0, 47) + "...";
                            
                            GUI.color = CULT_GOLD;
                            GUI.Label(new Rect(15, editY + 4, detailWidth - 100, 20), itemDisplay);
                            
                            // Edit button for this item
                            GUI.color = CULT_RED;
                            if (GUI.Button(new Rect(detailWidth - 85, editY, 50, 18), "Edit"))
                            {
                                _selectedNestedIndex = i;
                                _selectedNestedPath = _selectedField.Name;
                                _editValue = list[i]?.ToString() ?? "null";
                            }
                            
                            // Delete button for this item
                            if (GUI.Button(new Rect(detailWidth - 35, editY, 30, 18), "X"))
                            {
                                // Remove item from list
                                list.RemoveAt(i);
                                _selectedNestedIndex = -1;
                                break;
                            }
                            
                            editY += 24;
                        }
                        
                        // Add button to add new item
                        GUI.color = CULT_RED;
                        if (GUI.Button(new Rect(10, editY, 80, 20), "+ Add Item"))
                        {
                            // Get the element type and add a default value
                            Type elementType = _selectedField.FieldType.GetGenericArguments()[0];
                            object defaultValue = GetDefaultValue(elementType);
                            list.Add(defaultValue);
                        }
                        editY += 26;
                    }
                    else
                    {
                        GUI.color = CULT_GOLD;
                        GUI.Label(new Rect(10, editY, detailWidth - 30, 50), currentValue);
                        editY += 55;
                    }
                }
                catch
                {
                    GUI.color = CULT_GOLD;
                    GUI.Label(new Rect(10, editY, detailWidth - 30, 50), currentValue);
                    editY += 55;
                }
            }
            else if (_selectedField.FieldType.IsArray)
            {
                try {
                    object arrValue = null;
                    if (_selectedField.FieldInfo != null)
                        arrValue = _selectedField.FieldInfo.GetValue(DataManager.Instance);
                    else if (_selectedField.PropertyInfo != null)
                        arrValue = _selectedField.PropertyInfo.GetValue(DataManager.Instance, null);
                    
                    if (arrValue is Array arr)
                    {
                        GUI.color = CULT_GOLD;
                        // Show each array item with an edit button
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string itemName = GetObjectName(arr.GetValue(i));
                            string itemDisplay = i + ": " + itemName;
                            
                            // Highlight selected item
                            if (_selectedNestedIndex == i && _selectedNestedPath == _selectedField.Name)
                            {
                                GUI.color = CULT_DARK_PURPLE;
                                GUI.DrawTexture(new Rect(10, editY, detailWidth - 30, 22), Texture2D.whiteTexture);
                            }
                            
                            // Show item name (truncate if too long)
                            if (itemDisplay.Length > 50)
                                itemDisplay = itemDisplay.Substring(0, 47) + "...";
                            
                            GUI.color = CULT_GOLD;
                            GUI.Label(new Rect(15, editY + 4, detailWidth - 100, 20), itemDisplay);
                            
                            // Edit button for this item
                            GUI.color = CULT_RED;
                            if (GUI.Button(new Rect(detailWidth - 85, editY, 50, 18), "Edit"))
                            {
                                _selectedNestedIndex = i;
                                _selectedNestedPath = _selectedField.Name;
                                _editValue = arr.GetValue(i)?.ToString() ?? "null";
                            }
                            
                            editY += 24;
                        }
                    }
                    else
                    {
                        GUI.color = CULT_GOLD;
                        GUI.Label(new Rect(10, editY, detailWidth - 30, 50), currentValue);
                        editY += 55;
                    }
                }
                catch
                {
                    GUI.color = CULT_GOLD;
                    GUI.Label(new Rect(10, editY, detailWidth - 30, 50), currentValue);
                    editY += 55;
                }
            }
            else
            {
                // For strings, show full value
                if (_selectedField.FieldType == typeof(string))
                {
                    GUI.color = CULT_GOLD;
                    int stringHeight = Math.Max(50, (currentValue.Length / 40 + 1) * 18 + 10);
                    GUI.Label(new Rect(10, editY, detailWidth - 30, stringHeight), currentValue);
                    editY += stringHeight + 5;
                }
                else
                {
                    GUI.color = CULT_GOLD;
                    GUI.Label(new Rect(10, editY, detailWidth - 30, 50), currentValue);
                    editY += 55;
                }
            }
            
            // Nested item editing - show edit controls for selected nested item
            if (_selectedNestedIndex >= 0 && _selectedNestedPath == _selectedField.Name)
            {
                editY += 10;
                GUI.color = CULT_DARK_RED;
                GUI.DrawTexture(new Rect(10, editY, detailWidth - 30, 25), Texture2D.whiteTexture);
                GUI.color = CULT_BONE_WHITE;
                GUI.Label(new Rect(15, editY + 3, detailWidth - 40, 20), $"Edit Item [{_selectedNestedIndex}]:");
                editY += 28;
                
                // Get the list/array and the selected item
                try {
                    object collectionValue = null;
                    if (_selectedField.FieldInfo != null)
                        collectionValue = _selectedField.FieldInfo.GetValue(DataManager.Instance);
                    else if (_selectedField.PropertyInfo != null)
                        collectionValue = _selectedField.PropertyInfo.GetValue(DataManager.Instance, null);
                    
                    if (collectionValue is System.Collections.IList list)
                    {
                        if (_selectedNestedIndex >= 0 && _selectedNestedIndex < list.Count)
                        {
                            object item = list[_selectedNestedIndex];
                            
                            // Show current value
                            GUI.color = CULT_GOLD;
                            string currentItemStr = item != null ? item.ToString() : "null";
                            GUI.Label(new Rect(10, editY, detailWidth - 30, 20), "Current: " + currentItemStr);
                            editY += 24;
                            
                            // Show edit field
                            GUI.color = CULT_BONE_WHITE;
                            GUI.Label(new Rect(10, editY, 80, 20), "New Value:");
                            _editValue = GUI.TextField(new Rect(90, editY, detailWidth - 120, 20), _editValue);
                            editY += 26;
                            
                            // Save and Cancel buttons
                            GUI.color = CULT_RED;
                            if (GUI.Button(new Rect(10, editY, 80, 20), "Save"))
                            {
                                // Validate before applying
                                Type itemType = item?.GetType() ?? typeof(string);
                                string validationError = ValidateFieldValue(itemType, _editValue);
                                if (validationError != null) {
                                    ShowStatus(validationError);
                                } else {
                                    // Try to convert and set the value
                                    try {
                                        if (itemType == typeof(int))
                                            list[_selectedNestedIndex] = int.Parse(_editValue);
                                        else if (itemType == typeof(float))
                                            list[_selectedNestedIndex] = float.Parse(_editValue);
                                        else if (itemType == typeof(double))
                                            list[_selectedNestedIndex] = double.Parse(_editValue);
                                        else if (itemType == typeof(bool))
                                            list[_selectedNestedIndex] = bool.Parse(_editValue);
                                        else
                                            list[_selectedNestedIndex] = _editValue;
                                    }
                                    catch { }
                                    _selectedNestedIndex = -1;
                                }
                            }
                            if (GUI.Button(new Rect(100, editY, 80, 20), "Cancel"))
                            {
                                _selectedNestedIndex = -1;
                            }
                            editY += 26;
                        }
                    }
                }
                catch { }
                
                editY += 10;
            }
            
            // Edit controls based on type
            if (_selectedField.IsWritable)
            {
                // For booleans, show toggle button
                if (_selectedField.FieldType == typeof(bool))
                {
                    GUI.color = CULT_BONE_WHITE;
                    GUI.Label(new Rect(10, editY, detailWidth - 20, 20), "Value:");
                    editY += 22;
                    
                    // Toggle button
                    bool currentBool = GetFieldValue(_selectedField) == "True";
                    string toggleLabel = currentBool ? "Currently: TRUE (click to toggle)" : "Currently: FALSE (click to toggle)";
                    
                    GUI.color = currentBool ? CULT_GOLD : CULT_RED;
                    if (GUI.Button(new Rect(10, editY, detailWidth - 30, 26), toggleLabel))
                    {
                        // Toggle the boolean
                        SetFieldValue(_selectedField, currentBool ? "False" : "True");
                        _editValue = GetFieldValue(_selectedField);
                    }
                    editY += 32;
                }
                else
                {
                    GUI.color = CULT_BONE_WHITE;
                    GUI.Label(new Rect(10, editY, detailWidth - 20, 20), "New Value:");
                    editY += 22;
                    
                    _editValue = GUI.TextField(new Rect(10, editY, detailWidth - 30, 22), _editValue);
                    editY += 28;
                    
                    // Apply button
                    GUI.color = CULT_RED;
                    if (GUI.Button(new Rect(10, editY, detailWidth - 30, 26), "Apply Change"))
                    {
                        // Validate before applying
                        string validationError = ValidateFieldValue(_selectedField.FieldType, _editValue);
                        if (validationError != null) {
                            ShowStatus(validationError);
                        } else {
                            SetFieldValue(_selectedField, _editValue);
                        }
                    }
                    editY += 32;
                    
                    // Quick value buttons (not for strings or numbers only)
                    if (_selectedField.FieldType != typeof(string) && _selectedField.FieldType != typeof(int))
                    {
                        GUI.color = CULT_BLACK;
                        GUI.Box(new Rect(10, editY, detailWidth - 30, 60), "Quick Values");
                        editY += 5;
                        
                        int quickBtnWidth = (detailWidth - 50) / 3;
                        
                        GUI.color = CULT_DARK_PURPLE;
                        if (GUI.Button(new Rect(15, editY + 2, quickBtnWidth, 22), "0"))
                        {
                            _editValue = "0";
                        }
                        if (GUI.Button(new Rect(20 + quickBtnWidth, editY + 2, quickBtnWidth, 22), "1"))
                        {
                            _editValue = "1";
                        }
                        if (GUI.Button(new Rect(25 + quickBtnWidth * 2, editY + 2, quickBtnWidth, 22), "Max"))
                        {
                            _editValue = "999999";
                        }
                        editY += 30;
                    }
                    
                    // SAVE button after applying changes
                    GUI.color = CULT_GOLD;
                    if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "SAVE - Save Changes to Disk"))
                    {
                        SaveCurrentChanges();
                    }
                }
            }
            else
            {
                GUI.color = Color.gray;
                GUI.Label(new Rect(10, editY, detailWidth - 20, 20), "(This field is read-only)");
            }
            GUI.color = Color.white;
        }
        else
        {
            // Quick actions when no field selected
            GUI.color = CULT_GOLD;
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), "<b>Quick Actions</b>");
            editY += 28;
            
            // Save button - saves changes to disk
            GUI.color = CULT_GOLD;
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "SAVE - Save Changes to Disk"))
            {
                SaveCurrentChanges();
            }
            editY += 34;
            
            // Export button
            GUI.color = CULT_RED;
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "Export Current Save as .json"))
            {
                ExportSave();
            }
            editY += 34;
            
            // Export complete save as .mp (full copy)
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "Export Complete Save (.mp)"))
            {
                ExportCompleteSave();
            }
            editY += 34;
            
            // Import .json button - opens file selection
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "Import .json/.mp File"))
            {
                // Toggle file selection dropdown
                _showImportFilePicker = !_showImportFilePicker;
            }
            editY += 34;
            
            // File picker dropdown
            if (_showImportFilePicker)
            {
                DrawImportFilePicker(ref editY, detailWidth);
            }
            
            // Validation and Repair buttons
            editY += 10;
            GUI.color = CULT_GOLD;
            GUI.Label(new Rect(10, editY, detailWidth - 20, 20), "<b>Save Tools</b>");
            editY += 28;
            
            // Validate Save button
            GUI.color = new Color(0.3f, 0.6f, 0.9f);
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "Validate Save Data"))
            {
                ValidateSaveData();
            }
            editY += 34;
            
            // Repair Softlock button
            GUI.color = new Color(0.9f, 0.5f, 0.2f);
            if (GUI.Button(new Rect(10, editY, detailWidth - 30, 28), "Repair Softlock Issues"))
            {
                RepairSoftlockIssues();
            }
            editY += 34;
            
            GUI.color = Color.white;
        }
        
        GUI.EndScrollView();
    }
    
    private static void MaxPlayerStats()
    {
        try
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.XP = 999999;
                DataManager.Instance.Level = 99;
                DataManager.Instance.AbilityPoints = 99;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveEditor] Error maxing stats: {ex.Message}");
        }
    }
    
    
    private static void UnlockAllLocations()
    {
        try
        {
            var structureTypes = Enum.GetValues(typeof(StructureBrain.TYPES));
            foreach (var type in structureTypes)
            {
                try
                {
                    if (!DataManager.Instance.RevealedStructures.Contains((StructureBrain.TYPES)type))
                    {
                        DataManager.Instance.RevealedStructures.Add((StructureBrain.TYPES)type);
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveEditor] Error unlocking locations: {ex.Message}");
        }
    }
    
    private static void ExportSave()
    {
        try
        {
            // Get BepInEx plugins path
            string pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string jsonSavesPath = Path.Combine(pluginsPath, "jsonSaves");
            
            // Create folder if not exists
            if (!Directory.Exists(jsonSavesPath))
            {
                Directory.CreateDirectory(jsonSavesPath);
            }
            
            // Create filename with timestamp - proper JSON extension
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string jsonPath = Path.Combine(jsonSavesPath, $"save_{timestamp}.json");
            
            // Build JSON from all DataManager fields recursively
            var jsonObject = new Dictionary<string, object>();
            
            foreach (var field in _allFields)
            {
                try
                {
                    if (DataManager.Instance == null) continue;
                    
                    object value = null;
                    
                    if (field.FieldInfo != null)
                    {
                        value = field.FieldInfo.GetValue(DataManager.Instance);
                    }
                    else if (field.PropertyInfo != null)
                    {
                        value = field.PropertyInfo.GetValue(DataManager.Instance, null);
                    }
                    
                    // Recursively serialize the value
                    object serializedValue = SerializeValueToJson(value, field.Name, 0);
                    jsonObject[field.Name] = serializedValue;
                }
                catch
                {
                    // Skip fields that can't be read
                }
            }
            
            // Write proper JSON to file
            string jsonContent = SerializeDictionaryToJson(jsonObject, 0);
            File.WriteAllText(jsonPath, jsonContent);
            
            // Also copy the meta file
            try
            {
                int slot = 0;
                var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
                if (saveAndLoadType != null)
                {
                    var saveSlotField = saveAndLoadType.GetField("SAVE_SLOT", BindingFlags.Static | BindingFlags.Public);
                    if (saveSlotField != null)
                        slot = (int)saveSlotField.GetValue(null);
                }
                
                string savesPath = GetSavesPath();
                if (!string.IsNullOrEmpty(savesPath))
                {
                    string metaFile = Path.Combine(savesPath, $"meta_{slot}.mp");
                    if (File.Exists(metaFile))
                    {
                        string destMeta = Path.Combine(jsonSavesPath, $"meta_{timestamp}.mp");
                        File.Copy(metaFile, destMeta, true);
                        UnityEngine.Debug.Log($"[SaveEditor] Copied meta to: {destMeta}");
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[SaveEditor] Meta copy failed: {ex.Message}");
            }
            
            ShowStatus($"Exported to: jsonSaves folder ({jsonObject.Count} fields)");
            Debug.Log($"[SaveEditor] Save exported to: {jsonPath}");
        }
        catch (Exception ex)
        {
            ShowStatus($"Export failed: {ex.Message}");
            Debug.LogError($"[SaveEditor] Export error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Recursively serializes a value to JSON-compatible object
    /// </summary>
    private static object SerializeValueToJson(object value, string path, int depth)
    {
        if (value == null) return null;
        
        // Primitives
        if (value is bool b) return b;
        if (value is int i) return i;
        if (value is float f) return f;
        if (value is double d) return d;
        if (value is long l) return l;
        if (value is byte b2) return b2;
        if (value is char c) return c.ToString();
        
        // String - escape for JSON
        if (value is string s) return s;
        
        // Enum - return the enum value name
        if (value.GetType().IsEnum)
        {
            return value.ToString();
        }
        
        // Dictionary - serialize as JSON object
        if (value is System.Collections.IDictionary dict)
        {
            var result = new Dictionary<string, object>();
            foreach (var key in dict.Keys)
            {
                string keyStr = key?.ToString() ?? "null";
                result[keyStr] = SerializeValueToJson(dict[key], path + "." + keyStr, depth + 1);
            }
            return result;
        }
        
        // List - serialize as JSON array
        if (value is System.Collections.IList list)
        {
            var result = new List<object>();
            for (int idx = 0; idx < list.Count; idx++)
            {
                result.Add(SerializeValueToJson(list[idx], path + "[" + idx + "]", depth + 1));
            }
            return result;
        }
        
        // Array - serialize as JSON array
        if (value is Array arr)
        {
            var result = new List<object>();
            for (int idx = 0; idx < arr.Length; idx++)
            {
                result.Add(SerializeValueToJson(arr.GetValue(idx), path + "[" + idx + "]", depth + 1));
            }
            return result;
        }
        
        // Complex object - serialize its fields recursively (limit depth)
        if (depth < 3)
        {
            var result = new Dictionary<string, object>();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            
            foreach (var field in value.GetType().GetFields(flags))
            {
                try {
                    if (IsUnityEngineType(field.FieldType)) continue;
                    object fieldValue = field.GetValue(value);
                    result[field.Name] = SerializeValueToJson(fieldValue, path + "." + field.Name, depth + 1);
                } catch { }
            }
            
            foreach (var prop in value.GetType().GetProperties(flags))
            {
                try {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;
                    if (IsUnityEngineType(prop.PropertyType)) continue;
                    object propValue = prop.GetValue(value, null);
                    result[prop.Name] = SerializeValueToJson(propValue, path + "." + prop.Name, depth + 1);
                } catch { }
            }
            
            if (result.Count > 0) return result;
        }
        
        // Fallback - return string representation
        return value.ToString();
    }
    
    /// <summary>
    /// Serializes a dictionary to JSON string
    /// </summary>
    private static string SerializeDictionaryToJson(Dictionary<string, object> dict, int indent)
    {
        if (dict == null || dict.Count == 0) return "{}";
        
        var sb = new StringBuilder();
        string indentStr = new string(' ', indent * 2);
        string indentStr2 = new string(' ', (indent + 1) * 2);
        
        sb.Append("{\n");
        
        int count = 0;
        foreach (var kvp in dict)
        {
            count++;
            string key = EscapeJsonString(kvp.Key);
            string value = SerializeValueToJsonString(kvp.Value, indent + 1);
            
            sb.Append(indentStr2);
            sb.Append($"\"{key}\": {value}");
            
            if (count < dict.Count)
                sb.Append(",\n");
            else
                sb.Append("\n");
        }
        
        sb.Append(indentStr);
        sb.Append("}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Serializes a value to JSON string
    /// </summary>
    private static string SerializeValueToJsonString(object value, int indent)
    {
        if (value == null) return "null";
        
        // String - escape and quote
        if (value is string s) return "\"" + EscapeJsonString(s) + "\"";
        
        // Bool, number - don't quote
        if (value is bool b) return b ? "true" : "false";
        if (value is int i) return i.ToString();
        if (value is float f) return f.ToString("F6");
        if (value is double d) return d.ToString("F6");
        if (value is long l) return l.ToString();
        if (value is byte b2) return b2.ToString();
        
        // List/Array - serialize as JSON array
        if (value is List<object> list)
        {
            if (list.Count == 0) return "[]";
            
            var sb = new StringBuilder();
            string indentStr = new string(' ', indent * 2);
            string indentStr2 = new string(' ', (indent + 1) * 2);
            
            sb.Append("[\n");
            
            for (int idx = 0; idx < list.Count; idx++)
            {
                sb.Append(indentStr2);
                sb.Append(SerializeValueToJsonString(list[idx], indent + 1));
                
                if (idx < list.Count - 1)
                    sb.Append(",\n");
                else
                    sb.Append("\n");
            }
            
            sb.Append(indentStr);
            sb.Append("]");
            
            return sb.ToString();
        }
        
        // Dictionary - serialize as JSON object
        if (value is Dictionary<string, object> dict)
        {
            return SerializeDictionaryToJson(dict, indent);
        }
        
        // Fallback - quote the string
        return "\"" + EscapeJsonString(value.ToString()) + "\"";
    }
    
    /// <summary>
    /// Escapes a string for JSON
    /// </summary>
    private static string EscapeJsonString(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '\"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
    
    private static void ImportSave()
    {
        try
        {
            string pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string jsonSavesPath = Path.Combine(pluginsPath, "jsonSaves");
            
            if (!Directory.Exists(jsonSavesPath))
            {
                ShowStatus("No jsonSaves folder found. Export first.");
                return;
            }
            
            // Check for .mp files first (complete save)
            var mpFiles = Directory.GetFiles(jsonSavesPath, "save_*.mp");
            if (mpFiles.Length > 0)
            {
                Array.Sort(mpFiles);
                string latestMp = mpFiles[mpFiles.Length - 1];
                ImportCompleteSave(latestMp);
                return;
            }
            
            // Fall back to .json files
            var txtFiles = Directory.GetFiles(jsonSavesPath, "save_*.json");
            if (txtFiles.Length == 0)
            {
                ShowStatus("No save files found. Export first.");
                return;
            }
            
            // Get most recent
            Array.Sort(txtFiles);
            string latestFile = txtFiles[txtFiles.Length - 1];
            
            ImportFromFile(latestFile);
        }
        catch (Exception ex)
        {
            ShowStatus($"Import failed: {ex.Message}");
            Debug.LogError($"[SaveEditor] Import error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Draws a file picker dropdown for selecting JSON save files to import
    /// </summary>
    private static void DrawImportFilePicker(ref int editY, int detailWidth)
    {
        string pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string jsonSavesPath = Path.Combine(pluginsPath, "jsonSaves");
        
        // Load file list if not already loaded
        if (_importFileList == null || _importFileList.Length == 0)
        {
            if (Directory.Exists(jsonSavesPath))
            {
                // Get both .json and .mp files (save_*.json, save_*.mp, meta_*.mp)
                var jsonFiles = Directory.GetFiles(jsonSavesPath, "save_*.json");
                var saveMpFiles = Directory.GetFiles(jsonSavesPath, "save_*.mp");
                var metaMpFiles = Directory.GetFiles(jsonSavesPath, "meta_*.mp");
                var allFiles = jsonFiles.Concat(saveMpFiles).Concat(metaMpFiles).ToArray();
                Array.Sort(allFiles);
                _importFileList = allFiles;
            }
        }
        
        if (_importFileList.Length == 0)
        {
            // No files found
            GUI.Label(new Rect(20, editY, detailWidth - 40, 24), "No save files found in jsonSaves folder");
            editY += 28;
            
            if (GUI.Button(new Rect(20, editY, detailWidth - 40, 24), "Close"))
            {
                _showImportFilePicker = false;
            }
            editY += 28;
            return;
        }
        
        // Show file list (limit to 8 visible)
        int maxVisible = Math.Min(8, _importFileList.Length);
        int listHeight = maxVisible * 28;
        
        GUI.Box(new Rect(10, editY, detailWidth - 20, listHeight + 60), "Select file to import:");
        
        int listY = editY + 25;
        for (int i = 0; i < maxVisible; i++)
        {
            string fileName = Path.GetFileName(_importFileList[i]);
            if (GUI.Button(new Rect(20, listY, detailWidth - 40, 24), fileName))
            {
                // Handle .mp file imports (save_*.mp or meta_*.mp) - copy back to game saves
                if (fileName.EndsWith(".mp"))
                {
                    ImportCompleteSave(_importFileList[i]);
                }
                else
                {
                    ImportFromFile(_importFileList[i]);
                }
                _showImportFilePicker = false;
            }
            listY += 28;
        }
        
        // Show more button if there are more files
        if (_importFileList.Length > maxVisible)
        {
            GUI.Label(new Rect(20, listY, detailWidth - 40, 20), $"... and {_importFileList.Length - maxVisible} more files");
            listY += 24;
        }
        
        editY += listHeight + 65;
    }
    
    /// <summary>
    /// Import from a specific file path (supports both JSON and key=value formats)
    /// </summary>
    private static void ImportFromFile(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);
            int applied = 0;
            
            // Check if it's JSON format
            if (content.TrimStart().StartsWith("{"))
            {
                // Parse JSON format
                applied = ImportFromJson(content);
            }
            else
            {
                // Legacy key=value format
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                    
                    int eqIndex = line.IndexOf('=');
                    if (eqIndex > 0)
                    {
                        string key = line.Substring(0, eqIndex).Trim();
                        string value = line.Substring(eqIndex + 1).Trim();
                        
                        var field = _allFields.FirstOrDefault(f => f.Name == key);
                        if (field != null && field.IsWritable)
                        {
                            try
                            {
                                SetFieldValue(field, value);
                                applied++;
                            }
                            catch { }
                        }
                    }
                }
            }
            
            ShowStatus($"Imported {applied} values!");
            Debug.Log($"[SaveEditor] Imported {applied} values from: {filePath}");
        }
        catch (Exception ex)
        {
            ShowStatus($"Import failed: {ex.Message}");
            Debug.LogError($"[SaveEditor] Import error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Import from JSON content - recursively handles nested objects
    /// </summary>
    private static int ImportFromJson(string jsonContent)
    {
        int applied = 0;
        
        try
        {
            // Parse the JSON into a dictionary
            var jsonDict = ParseJsonSimple(jsonContent);
            
            // Apply to DataManager using the field path
            applied = ApplyJsonToDataManager(jsonDict, "");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveEditor] JSON parse error: {ex.Message}");
        }
        
        return applied;
    }
    
    /// <summary>
    /// Simple JSON parser that returns nested dictionaries/lists
    /// </summary>
    private static object ParseJsonSimple(string json)
    {
        json = json.Trim();
        
        if (json.StartsWith("{"))
            return ParseJsonObject(json);
        if (json.StartsWith("["))
            return ParseJsonArray(json);
        
        return json.Trim('"');
    }
    
    private static Dictionary<string, object> ParseJsonObject(string json)
    {
        var result = new Dictionary<string, object>();
        
        // Remove outer braces
        if (json.StartsWith("{") && json.EndsWith("}"))
            json = json.Substring(1, json.Length - 2);
        
        int depth = 0;
        int start = 0;
        bool inString = false;
        
        for (int i = 0; i <= json.Length; i++)
        {
            char c = (i < json.Length) ? json[i] : ',';
            
            if (c == '"' && (i == 0 || json[i-1] != '\\')) inString = !inString;
            if (inString) continue;
            
            if (c == '{' || c == '[') depth++;
            else if (c == '}' || c == ']') depth--;
            else if (c == ',' && depth == 0)
            {
                string pair = json.Substring(start, i - start).Trim();
                start = i + 1;
                
                if (string.IsNullOrEmpty(pair)) continue;
                
                int colon = pair.IndexOf(':');
                if (colon <= 0) continue;
                
                string key = pair.Substring(0, colon).Trim().Trim('"');
                string value = pair.Substring(colon + 1).Trim();
                
                result[key] = ParseJsonSimple(value);
            }
        }
        
        return result;
    }
    
    private static List<object> ParseJsonArray(string json)
    {
        var result = new List<object>();
        
        if (json.StartsWith("[") && json.EndsWith("]"))
            json = json.Substring(1, json.Length - 2);
        
        int depth = 0;
        int start = 0;
        bool inString = false;
        
        for (int i = 0; i <= json.Length; i++)
        {
            char c = (i < json.Length) ? json[i] : ',';
            
            if (c == '"' && (i == 0 || json[i-1] != '\\')) inString = !inString;
            if (inString) continue;
            
            if (c == '{' || c == '[') depth++;
            else if (c == '}' || c == ']') depth--;
            else if (c == ',' && depth == 0)
            {
                string item = json.Substring(start, i - start).Trim();
                start = i + 1;
                
                if (!string.IsNullOrEmpty(item))
                    result.Add(ParseJsonSimple(item));
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Recursively apply JSON to DataManager using field paths
    /// </summary>
    private static int ApplyJsonToDataManager(object json, string prefix)
    {
        int applied = 0;
        
        if (json is Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                string fieldPath = string.IsNullOrEmpty(prefix) ? kvp.Key : prefix + "." + kvp.Key;
                
                if (kvp.Value is Dictionary<string, object> || kvp.Value is List<object>)
                {
                    // Recurse into nested objects/arrays
                    applied += ApplyJsonToDataManager(kvp.Value, fieldPath);
                }
                else
                {
                    // Try to find and set this field
                    string valueStr = kvp.Value?.ToString() ?? "";
                    var field = _allFields.FirstOrDefault(f => f.Name == fieldPath);
                    if (field != null && field.IsWritable)
                    {
                        try
                        {
                            SetFieldValue(field, valueStr);
                            applied++;
                        }
                        catch { }
                    }
                }
            }
        }
        else if (json is List<object> list)
        {
            // For arrays, we can't easily map to fields without knowing indices
            // Skip for now - arrays would need special handling
        }
        
        return applied;
    }
    
    /// <summary>
    /// Explicitly loads the meta file for a given slot number
    /// </summary>
    private static void LoadMetaFileForSlot(int slotNumber)
    {
        try
        {
            // Get the saves path
            string savesPath = GetSavesPath();
            if (string.IsNullOrEmpty(savesPath) || !Directory.Exists(savesPath))
            {
                UnityEngine.Debug.LogWarning("[SaveEditor] Saves path not found for meta file");
                return;
            }
            
            // Construct meta file path (meta_0.mp, meta_1.mp, etc.)
            string metaFilePath = Path.Combine(savesPath, $"meta_{slotNumber}.mp");
            
            if (File.Exists(metaFilePath))
            {
                // The game's SaveAndLoad.Load(slot) method already loads both the main save
                // AND the meta file automatically via its internal LoadMetaData call.
                // We can see this in the logs: "Load MetaData" is called after reading the main save.
                UnityEngine.Debug.Log($"[SaveEditor] Meta file will be loaded automatically by SaveAndLoad.Load({slotNumber})");
                return;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[SaveEditor] Meta file not found: {metaFilePath}");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[SaveEditor] Error with meta file: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the saves folder path
    /// </summary>
    private static string GetSavesPath()
    {
        try
        {
            // Try to get from SaveAndLoad
            var saveAndLoadType = Type.GetType("SaveAndLoad, Assembly-CSharp");
            if (saveAndLoadType != null)
            {
                var savesFolderProperty = saveAndLoadType.GetProperty("savesFolder", BindingFlags.Static | BindingFlags.Public);
                if (savesFolderProperty != null)
                {
                    string path = savesFolderProperty.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(path))
                        return path;
                }
            }
            
            // Fallback: use persistentDataPath
            string persistentDataPath = Application.persistentDataPath;
            return Path.Combine(persistentDataPath, "saves");
        }
        catch
        {
            return Path.Combine(Application.persistentDataPath, "saves");
        }
    }
    
    /// <summary>
    /// Validates the current save data for potential issues
    /// </summary>
    private static void ValidateSaveData()
    {
        try
        {
            if (DataManager.Instance == null)
            {
                ShowStatus("DataManager not available");
                return;
            }
            
            var result = SaveValidator.ValidateSave(DataManager.Instance);
            
            if (result.IsValid)
            {
                ShowStatus("Save validation passed!");
                CheatMenu.Logger.Info(CheatMenu.Logger.SAVE, "Save validation passed");
            }
            else
            {
                string message = "Save validation issues found: " + string.Join(", ", result.Errors.Take(5));
                ShowStatus(message);
                CheatMenu.Logger.Warning(CheatMenu.Logger.SAVE, message);
            }
            
            // Also show warnings count
            if (result.Warnings.Count > 0)
            {
                ShowStatus($"Validation: {result.Warnings.Count} warnings, {result.Errors.Count} errors");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Validation error: {ex.Message}");
            CheatMenu.Logger.Error(CheatMenu.Logger.SAVE, $"Validation failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Scans and repairs softlock issues in the save data
    /// </summary>
    private static void RepairSoftlockIssues()
    {
        try
        {
            if (DataManager.Instance == null)
            {
                ShowStatus("DataManager not available");
                return;
            }
            
            var repairer = new SoftlockRepair();
            var report = repairer.ScanAndRepair(DataManager.Instance);
            
            if (report.WasRepaired)
            {
                ShowStatus($"Repaired {report.RepairsPerformed.Count} issues!");
                CheatMenu.Logger.Info(CheatMenu.Logger.SAVE, $"Softlock repair completed: {report.RepairsPerformed.Count} repairs");
            }
            else if (report.DetectedIssues.Count > 0)
            {
                ShowStatus($"Found {report.DetectedIssues.Count} issues (cannot repair automatically)");
                CheatMenu.Logger.Warning(CheatMenu.Logger.SAVE, $"Softlock scan found {report.DetectedIssues.Count} unrepairable issues");
            }
            else
            {
                ShowStatus("No softlock issues detected!");
                CheatMenu.Logger.Info(CheatMenu.Logger.SAVE, "No softlock issues found");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Repair error: {ex.Message}");
            CheatMenu.Logger.Error(CheatMenu.Logger.SAVE, $"Softlock repair failed: {ex.Message}");
        }
    }
}

// Helper class for field info
public class SaveFieldInfo
{
    public string Name { get; set; }
    public Type FieldType { get; set; }
    public string Category { get; set; }
    public bool IsReadable { get; set; }
    public bool IsWritable { get; set; }
    public FieldInfo FieldInfo { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public MemberInfo MemberInfo { get; set; }
    public object ParentObject { get; set; } // For MetaData fields
}

// Helper class for save file info
public class SaveFileInfo
{
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public bool IsWoolhaven { get; set; }
    public DateTime LastModified { get; set; }
    public long FileSize { get; set; }
}
