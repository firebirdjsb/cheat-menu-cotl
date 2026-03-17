using BepInEx;
using System;
using System.IO;
using System.Reflection;

namespace CheatMenu;

[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.3.9")]
public class Plugin : BaseUnityPlugin
{    
    private UnityAnnotationHelper _annotationHelper;
    private Action _updateFn = null;
    private Action _onGUIFn = null;
    
    // Version constant for backup checking
    private const string CHEAT_MENU_VERSION = "1.3.9";
    
    public void Awake()
    {        
        // Initialize the structured logger first - all other systems depend on this
        CheatMenu.Logger.Initialize(Logger);
        
        CheatMenu.Logger.Init("========================");
        CheatMenu.Logger.Init($"Starting Cheat Menu v{CHEAT_MENU_VERSION}...");
        CheatMenu.Logger.Init("Cult of the Lamb: Cheaters Edition!");
        CheatMenu.Logger.Init("=========================");

        new CheatConfig(Config);

        try {
            // Initialize ReflectionCache - scan assembly for cheat methods
            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, "Initializing Reflection Cache...");
            ReflectionCache.ScanAssembly();
            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, $"Reflection Cache ready: {ReflectionCache.AllCheatMethods.Count} cheats, {ReflectionCache.CategoryMethods.Count} categories");
            
            // Initialize DefinitionGenerator - generate definitions from game enums
            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, "Initializing Definition Generator...");
            DefinitionGenerator.GenerateAllDefinitions();
            CheatMenu.Logger.Info(CheatMenu.Logger.INIT, "Definition Generator initialized successfully");
            
            // RunAllInit() must run next — it calls ReflectionHelper.Init() which creates
            // the Harmony instance needed by all subsequent patches.
            _annotationHelper = new UnityAnnotationHelper();
            
            _annotationHelper.RunAllInit();
            CheatMenu.Logger.Init("Annotation system initialized successfully");

            // Patch VersionNumber.OnEnable so the main menu shows "Cheaters Edition"
            PatchVersionText();

            _onGUIFn = _annotationHelper.BuildRunAllOnGuiDelegate();
            _updateFn = _annotationHelper.BuildRunAllUpdateDelegate();
            
            // Backup saves on plugin load
            BackupSavesOnLoad();
            
            CheatMenu.Logger.Init("Patching and loading completed successfully!");
            CheatMenu.Logger.Init("Cheat Menu is ready to use!");
        } catch(Exception e) {
            CheatMenu.Logger.Error(CheatMenu.Logger.CHEAT, $"FATAL ERROR during initialization: {e.Message}");
        }
    }
    
    /// <summary>
    /// Backs up saves when the plugin loads, only if version changed
    /// </summary>
    private void BackupSavesOnLoad()
    {
        try
        {
            // Get the plugin directory
            string pluginPath = Path.GetDirectoryName(Info.Location);
            string jsonSavesPath = Path.Combine(pluginPath, "jsonSaves");
            string saveBackupsPath = Path.Combine(jsonSavesPath, "SaveBackups");
            string versionFilePath = Path.Combine(saveBackupsPath, "version.txt");
            
            // Create directories if they don't exist
            if (!Directory.Exists(jsonSavesPath))
            {
                Directory.CreateDirectory(jsonSavesPath);
            }
            if (!Directory.Exists(saveBackupsPath))
            {
                Directory.CreateDirectory(saveBackupsPath);
            }
            
            // Check if version file exists
            string currentVersion = CHEAT_MENU_VERSION;
            string savedVersion = null;
            
            if (File.Exists(versionFilePath))
            {
                savedVersion = File.ReadAllText(versionFilePath).Trim();
            }
            
            // Only backup if version changed or no backup exists
            if (savedVersion != currentVersion)
            {
                CheatMenu.Logger.Init($"Cheat Menu version changed ({savedVersion} -> {currentVersion}), creating save backup...");
                
                // Find the game saves directory
                string savesPath = GetGameSavesPath();
                
                if (savesPath != null && Directory.Exists(savesPath))
                {
                    // Create timestamped backup folder
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFolder = Path.Combine(saveBackupsPath, $"backup_{timestamp}");
                    Directory.CreateDirectory(backupFolder);
                    
                    // Copy all .mp files
                    int backupCount = 0;
                    var mpFiles = Directory.GetFiles(savesPath, "*.mp", SearchOption.AllDirectories);
                    foreach (var file in mpFiles)
                    {
                        string destFile = Path.Combine(backupFolder, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                        backupCount++;
                    }
                    
                    // Write version file
                    File.WriteAllText(versionFilePath, currentVersion);
                    
                    CheatMenu.Logger.Init($"Save backup created: {backupCount} files backed up to {backupFolder}");
                }
                else
                {
                    CheatMenu.Logger.Warning(CheatMenu.Logger.SAVE, "Could not find game saves directory for backup");
                }
            }
            else
            {
                CheatMenu.Logger.Init("Cheat Menu version unchanged, skipping backup");
            }
        }
        catch (Exception ex)
        {
            CheatMenu.Logger.Warning(CheatMenu.Logger.SAVE, $"Error during save backup: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the path to the game's saves folder
    /// </summary>
    private string GetGameSavesPath()
    {
        try
        {
            // Try to get from Unity's persistentDataPath
            var applicationType = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule");
            if (applicationType != null)
            {
                var persistentDataPathProperty = applicationType.GetProperty("persistentDataPath", BindingFlags.Static | BindingFlags.Public);
                if (persistentDataPathProperty != null)
                {
                    var persistentDataPath = persistentDataPathProperty.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(persistentDataPath))
                    {
                        return Path.Combine(persistentDataPath, "saves");
                    }
                }
            }
            
            // Fallback to manual path
            string[] possiblePaths = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), "Massive Monster", "Cult Of The Lamb", "saves"),
                @"C:\Users\" + Environment.UserName + @"\AppData\LocalLow\Massive Monster\Cult Of The Lamb\saves"
            };
            
            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
        }
        catch { }
        
        return null;
    }

    public void OnDisable()
    {
        try {
            CheatMenu.Logger.Init("Shutting down Cheat Menu...");
            _annotationHelper?.RunAllUnload();
            CheatMenu.Logger.Init("Cheat Menu disabled successfully");
        } catch(Exception e) {
            CheatMenu.Logger.Error(CheatMenu.Logger.CHEAT, $"Error during shutdown: {e.Message}");
        }
    }

    public void OnGUI()
    {
        try {
            _onGUIFn?.Invoke();
        } catch(Exception e) {
            CheatMenu.Logger.Error(CheatMenu.Logger.GUI, $"OnGUI error: {e.Message}");
        }
    }

    public void Update()
    {
        try {
            _updateFn?.Invoke();
        } catch(Exception e) {
            CheatMenu.Logger.Error(CheatMenu.Logger.CHEAT, $"Update error: {e.Message}");
        }
    }

    private void PatchVersionText()
    {
        try {
            MethodInfo patchMethod = typeof(Plugin).GetMethod("Prefix_VersionNumber_OnEnable", BindingFlags.Static | BindingFlags.Public);
            Type versionNumberType = HarmonyLib.AccessTools.TypeByName("VersionNumber");
            
            if(versionNumberType == null) {
                CheatMenu.Logger.Warning(CheatMenu.Logger.PATCH, "VersionNumber type not found");
                return;
            }

            var result = ReflectionHelper.PatchMethodPrefix(
                versionNumberType,
                "OnEnable",
                patchMethod,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if(result != null) {
                CheatMenu.Logger.Patch("VersionNumber.OnEnable patched");
            } else {
                CheatMenu.Logger.Warning(CheatMenu.Logger.PATCH, "VersionNumber.OnEnable patch failed (method not found)");
            }
        } catch(Exception e) {
            CheatMenu.Logger.Warning(CheatMenu.Logger.PATCH, $"VersionNumber patch failed: {e.Message}");
        }
    }

    public static bool Prefix_VersionNumber_OnEnable(VersionNumber __instance)
    {
        try {
            // Use reflection to get the Text field - avoids needing UnityEngine.UI reference
            var textField = typeof(VersionNumber).GetField("Text", BindingFlags.Instance | BindingFlags.Public);
            if(textField != null) {
                var textComponent = textField.GetValue(__instance);
                if(textComponent != null) {
                    var textProperty = textComponent.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                    if(textProperty != null) {
                        textProperty.SetValue(textComponent, $"{UnityEngine.Application.version} - Cheaters Edition");
                    }
                }
            }
            return false;
        } catch(Exception e){
            // Can't use Logger in static method
            UnityEngine.Debug.LogWarning($"Version text patch error: {e.Message}");
        }
        return true;
    }
}
