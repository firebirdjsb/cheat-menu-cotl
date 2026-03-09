# Cheat Menu for Cult of the Lamb - Source Code Documentation

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Overview](#architecture-overview)
3. [File Structure](#file-structure)
4. [Core Components](#core-components)
5. [Annotation System](#annotation-system)
6. [GUI System](#gui-system)
7. [Cheat Registration and Discovery](#cheat-registration-and-discovery)
8. [Data Flow](#data-flow)
9. [Configuration System](#configuration-system)
10. [Harmony Patching](#harmony-patching)
11. [Usage Examples](#usage-examples)
12. [Key Classes and Relationships](#key-classes-and-relationships)

---

## Project Overview

This is a BepInEx plugin for the game "Cult of the Lamb" that provides a comprehensive cheat menu with various gameplay modifications. The mod allows players to toggle godmode, add resources, manage followers, modify cult properties, and much more.

### Version
- Current Version: 1.3.2
- Namespace: `CheatMenu`
- Assembly: `CheatMenu.dll`

### Dependencies
- BepInEx (plugin framework)
- HarmonyLib (for runtime patching)
- UnityEngine (game integration)

---

## Architecture Overview

The cheat menu uses a **declarative annotation-driven architecture** where cheats are defined as static methods decorated with custom attributes. The system uses reflection to discover, categorize, and render these cheats at runtime.

### Design Patterns Used

1. **Annotation-Driven Development**: Cheats are defined using C# attributes (`[CheatDetails]`, `[CheatCategory]`, etc.)
2. **Reflection-Based Discovery**: The system scans assemblies for cheat methods at startup
3. **Dynamic IL Generation**: GUI rendering uses IL emit for optimized button generation
4. **Singleton Pattern**: Used in `FlagManager`, `CheatConfig`, and various managers
5. **Lifecycle Attribute System**: Custom `[Init]`, `[Unload]`, `[OnGui]`, `[Update]` attributes for managing component lifecycles

---

## File Structure

```
src/
├── Plugin.cs                     # Main BepInEx entry point
├── CheatConfig.cs                # Configuration management
├── CheatUtils.cs                 # Utility functions
├── CultUtils.cs                  # Game interaction utilities (84KB)
├── DefinitionManager.cs          # Cheat discovery and grouping
├── FlagManager.cs                # Toggle state management
├── GlobalPatches.cs              # Harmony patches
├── annotations/
│   ├── CheatCategory.cs          # Category assignment attribute
│   ├── CheatDetails.cs           # Cheat metadata attribute
│   ├── CheatFlag.cs              # Flag cheat marker
│   ├── CheatWIP.cs               # Work-in-progress marker
│   ├── LifecycleAttributes.cs    # Init/Unload/OnGui/Update attributes
│   ├── RequiresDLC.cs            # DLC requirement attribute
│   └── StringEnum.cs             # String enum helper
├── definitions/
│   ├── Definition.cs             # Cheat definition model
│   ├── AnimationDefinitions.cs   # Animation cheats
│   ├── CompanionDefinitions.cs   # Companion cheats
│   ├── CultDefinitions.cs        # Cult management cheats
│   ├── DlcDefinitions.cs         # DLC-specific cheats
│   ├── FarmingDefinitions.cs     # Farming cheats
│   ├── FollowerDefinitions.cs    # Follower management cheats
│   ├── HealthDefinitions.cs     # Health cheats
│   ├── MiscDefinitions.cs       # Miscellaneous cheats
│   ├── QolDefinitions.cs        # Quality of life cheats
│   ├── ResourceDefinitions.cs   # Resource spawning cheats
│   ├── SceneryDefinitions.cs    # Scenery cheats
│   ├── SplitscreenDefinitions.cs # Splitscreen cheats
│   └── WeatherDefinitions.cs    # Weather cheats
├── enums/
│   ├── CheatCategoryEnum.cs     # Category enum definitions
│   └── DlcRequirement.cs        # DLC requirement enum
├── gui/
│   ├── CheatMenuGui.cs          # Main GUI rendering
│   ├── GUIManager.cs            # GUI window management
│   ├── GUIUtils.cs              # GUI utilities
│   └── NotificationHandler.cs   # Notification system
├── helpers/
│   ├── AsyncHelper.cs           # Async utilities
│   ├── ReflectionHelper.cs      # Reflection and patching
│   ├── RewiredInputHelper.cs    # Controller input handling
│   ├── TextureHelper.cs         # Texture generation
│   └── UnityAnnotationHelper.cs # Lifecycle management
├── interfaces/
│   └── IDefinition.cs           # Definition marker interface
└── patches/
    └── (additional patches)
```

---

## Core Components

### 1. Plugin.cs

The main entry point for the BepInEx plugin.

**Key Responsibilities:**
- Initializes the cheat menu on game startup
- Creates the `CheatConfig` instance
- Initializes the annotation helper system
- Sets up version text patching
- Routes `OnGUI()` and `Update()` calls

**Key Methods:**
- [`Awake()`](src/Plugin.cs:14): Plugin initialization
- [`OnDisable()`](src/Plugin.cs:43): Cleanup on plugin unload
- [`OnGUI()`](src/Plugin.cs:54): GUI rendering delegate
- [`Update()`](src/Plugin.cs:63): Update loop delegate
- [`PatchVersionText()`](src/Plugin.cs:72): Patches version display

```csharp
[BepInPlugin("org.xunfairx.cheat_menu", "Cheat Menu", "1.3.2")]
public class Plugin : BaseUnityPlugin
{
    private UnityAnnotationHelper _annotationHelper;
    private Action _updateFn = null;
    private Action _onGUIFn = null;

    public void Awake()
    {
        new CheatConfig(Config);
        _annotationHelper = new UnityAnnotationHelper();
        _annotationHelper.RunAllInit();
        _onGUIFn = _annotationHelper.BuildRunAllOnGuiDelegate();
        _updateFn = _annotationHelper.BuildRunAllUpdateDelegate();
    }
}
```

### 2. CheatConfig.cs

Manages configuration entries for the cheat menu.

**Configuration Options:**
- `GuiKeybind` (KeyboardShortcut): Key to open/close the menu (default: M)
- `BackCategory` (KeyboardShortcut): Key to go back in menu (default: N)
- `CloseGuiOnEscape` (bool): Close menu on Escape press (default: true)
- `ControllerSupport` (bool): Enable controller navigation (default: true)

```csharp
public class CheatConfig{
    public ConfigEntry<KeyboardShortcut> GuiKeybind;
    public ConfigEntry<KeyboardShortcut> BackCategory;
    public ConfigEntry<bool> CloseGuiOnEscape;
    public ConfigEntry<bool> ControllerSupport;

    public CheatConfig(ConfigFile config){
        GuiKeybind = config.Bind(
            new ConfigDefinition("Keybinds", "GUIKey"),
            new KeyboardShortcut(UnityEngine.KeyCode.M),
            new ConfigDescription("The key pressed to open and close the CheatMenu GUI")
        );
        // ... more config entries
        Instance = this;
    }

    public static CheatConfig Instance { get; set; }
}
```

### 3. FlagManager.cs

A singleton class that manages toggle states for mode cheats.

**Key Methods:**
- [`SetFlagValue(string flagID, bool value)`](src/FlagManager.cs:18): Sets a flag's value
- [`IsFlagEnabled(string flagID)`](src/FlagManager.cs:28): Checks if a flag is enabled
- [`FlipFlagValue(string flagID)`](src/FlagManager.cs:34): Toggles a flag's value

```csharp
public sealed class FlagManager
{
    private Dictionary<string, bool> _cheatFlags = new();

    [Init]
    [EnforceOrderFirst(10)]
    public void Init(){
        _cheatFlags = new Dictionary<string, bool>();
    }

    public static void SetFlagValue(string flagID, bool value){
        Instance._cheatFlags[flagID] = value;
    }

    public static bool IsFlagEnabled(string flagID)
    {
        Instance._cheatFlags.TryGetValue(flagID, out bool flag);
        return flag;
    }

    public static FlagManager Instance { get; } = new FlagManager();
}
```

### 4. CultUtils.cs

A comprehensive utility class for interacting with the game's systems.

**Key Categories:**

#### Game State Checks
- [`IsInGame()`](src/CultUtils.cs:16): Checks if a save is loaded

#### DLC Helpers
- [`HasMajorDLC()`](src/CultUtils.cs:21): Checks Woolhaven ownership
- [`HasSinfulDLC()`](src/CultUtils.cs:22): Checks Sinful Pack ownership
- [`HasCultistDLC()`](src/CultUtils.cs:23): Checks Cultist Pack ownership
- [`HasHereticDLC()`](src/CultUtils.cs:24): Checks Heretic Pack ownership
- [`HasPilgrimDLC()`](src/CultUtils.cs:25): Checks Pilgrim Pack ownership

#### Follower Management
- [`RemoveSpyFromRecentFollowers(int recentCount)`](src/CultUtils.cs:84): Removes spy trait

#### Doctrine/Cult Management
- [`GiveDocterineStone()`](src/CultUtils.cs:113): Gives a commandment stone
- [`CompleteObjective(ObjectivesData objective)`](src/CultUtils.cs:124): Completes an objective
- [`CompleteAllQuests()`](src/CultUtils.cs:145): Completes all active quests
- [`ClearAllDocterines()`](src/CultUtils.cs:155): Resets all doctrine upgrades
- [`GetAllSermonCategories()`](src/CultUtils.cs:188): Gets available sermon categories

#### Structures
- [`ClearBerryBushes()`](src/CultUtils.cs:61): Removes berry bushes from base

### 5. DefinitionManager.cs

Manages cheat discovery, grouping, and GUI generation.

**Key Methods:**
- [`GetAllCheatMethods()`](src/DefinitionManager.cs:9): Discovers all cheats via reflection
- [`CheatFunctionToDetails(List<Definition> allCheats)`](src/DefinitionManager.cs:28): Maps method names to definitions
- [`GroupCheatsByCategory(List<Definition> allCheats)`](src/DefinitionManager.cs:41): Groups cheats by category
- [`BuildGUIContentFn()`](src/DefinitionManager.cs:65): Generates IL code for GUI rendering

### 6. GlobalPatches.cs

Contains Harmony patches for game methods.

**Patches Applied:**
- `Interactor.Update`: Disables interaction when GUI is open
- `UpgradeSystem.UnlockAbility`: Fixes ritual pair display
- `PlayerFarming.Bleat`: Suppresses bleat sound when using R3
- `Follower.Init`: Validates follower data to prevent crashes
- `Follower.Tick`: Guards against null outfit errors
- `Interaction_WolfBase.Update`: Makes friendly wolf follow player
- `Spine.Skin.AddSkin`: Prevents null reference exceptions

---

## Annotation System

The cheat menu uses custom C# attributes to declaratively define cheats.

### CheatCategory

Assigns a cheat method to a category.

```csharp
[CheatCategory(CheatCategoryEnum.HEALTH)]
public class HealthDefinitions : IDefinition{
    // Cheat methods here
}
```

**Categories:**
- HEALTH
- COMBAT
- RESOURCE
- CULT
- FOLLOWER
- FARMING
- COMPANION
- WEATHER
- DLC
- SPLITSCREEN
- MISC
- ANIMATION
- SCENERY

### CheatDetails

Provides metadata for a cheat method.

**Simple Cheat:**
```csharp
[CheatDetails("Heal x1", "Heals a Red Heart of the Player", subGroup: "Heal")]
public static void HealRed(){
    // Implementation
}
```

**Toggle Cheat (Mode Cheat):**
```csharp
[CheatDetails("Godmode", "Godmode (OFF)", "Godmode (ON)", "Full invincibility", true, subGroup: "Modes")]
public static void GodMode(bool flag){
    // flag is true when ON, false when OFF
}
```

**Parameters:**
- `title`: Display name in menu
- `description`: Tooltip/help text
- `isFlagCheat`: Whether it's a toggle (default: false)
- `sortOrder`: Display order within category
- `subGroup`: Optional sub-group for organizing related cheats

### RequiresDLC

Marks a cheat as requiring specific DLC.

```csharp
[RequiresDLC(DlcRequirement.MajorDLC)]
[CheatDetails("Give Woolhaven Resources", "Gives Woolhaven-specific resources")]
public static void GiveWoolhavenResources(){
    // Implementation
}
```

### CheatWIP

Marks a cheat as work-in-progress (excluded from release builds).

```csharp
[CheatWIP]
[CheatDetails("Experimental Feature", "This feature is experimental")]
public static void ExperimentalFeature(){
    // Implementation
}
```

### Lifecycle Attributes

Used to mark methods for automatic lifecycle management.

```csharp
[Init]                    // Called during initialization
[Unload]                  // Called during cleanup
[OnGui]                   // Called every GUI frame
[Update]                  // Called every game update
[EnforceOrderFirst(10)]   // Run first with priority 10
[EnforceOrderLast]        // Run last in initialization
```

---

## GUI System

### CheatMenuGui.cs

Main GUI rendering class using Unity's IMGUI.

**Key State Variables:**
- `GuiEnabled`: Whether menu is visible
- `CurrentCategory`: Currently selected category
- `CurrentSubGroup`: Currently selected sub-group
- `CurrentButtonY`: Current Y position for button layout

**Key Methods:**
- [`Init()`](src/gui/CheatMenuGui.cs:42): Initializes GUI state
- [`GetMenuRect(float progress)`](src/gui/CheatMenuGui.cs:65): Calculates animated menu position
- [`IsWithinCategory()`](src/gui/CheatMenuGui.cs:81): Checks if in a category
- [`CategoryButton(string category)`](src/gui/CheatMenuGui.cs): Renders category button
- [`Button(string title)`](src/gui/CheatMenuGui.cs): Renders simple cheat button
- [`ButtonWithFlag(string onTitle, string offTitle, string flagId)`](src/gui/CheatMenuGui.cs): Renders toggle button
- [`BackButton()`](src/gui/CheatMenuGui.cs): Renders back button
- [`HasRequiredDLC(int dlcRequirement)`](src/gui/CheatMenuGui.cs:141): Checks DLC for visibility

**Features:**
- Slide-in animation
- Controller navigation support
- Scrollable content
- Sub-group navigation
- DLC-gated visibility

### GUIManager.cs

Manages dynamic GUI windows for complex cheats.

**Key Methods:**
- [`SetGuiFunction(Action guiFunction)`](src/gui/GUIManager.cs:60): Registers a GUI function
- [`SetGuiWindowScrollableFunction(GUIUtils.ScrollableWindowParams, Action)`](src/gui/GUIManager.cs:42): Creates scrollable window
- [`CloseGuiFunction(string key)`](src/gui/GUIManager.cs:79): Closes a GUI function
- [`ClearAllGuiBasedCheats()`](src/gui/GUIManager.cs:28): Clears all registered functions

### GUIUtils.cs

Provides utility functions for GUI rendering including:
- Button styling
- Scrollable window management
- Slider controls
- Window rendering

---

## Cheat Registration and Discovery

The registration flow works as follows:

```
1. Plugin.Awake()
   │
   ├── Creates CheatConfig
   │
   ├── Creates UnityAnnotationHelper
   │
   └── UnityAnnotationHelper.RunAllInit()
       │
       ├── ReflectionHelper.Init() [Order: 9]
       │   └── Creates Harmony instance
       │
       ├── FlagManager.Init() [Order: 10]
       │   └── Initializes flag dictionary
       │
       ├── CheatCategoryEnumExtensions.Init()
       │   └── Initializes enum caches
       │
       ├── CheatMenuGui.Init()
       │   └── Sets up GUI state
       │   └── Calls DefinitionManager.BuildGUIContentFn()
       │       │
       │       ├── GetAllCheatMethods()
       │       │   └── Scans assembly for [CheatDetails] methods
       │       │
       │       ├── GroupCheatsByCategory()
       │       │   └── Organizes by category and sort order
       │       │
       │       └── Builds IL for efficient rendering
       │
       └── GlobalPatches.Init()
           └── Applies Harmony patches
```

### Adding a New Cheat

1. Choose or create the appropriate definition class in `src/definitions/`
2. Add a static method with the cheat logic
3. Apply `[CheatDetails]` attribute
4. Apply `[RequiresDLC]` if DLC-specific

**Example:**
```csharp
// In HealthDefinitions.cs
[CheatDetails("My Custom Heal", "Heals the player completely", subGroup: "Heal")]
public static void MyCustomHeal(){
    PlayerFarming.Instance.health.Heal(999f);
    CultUtils.PlayNotification("Custom heal applied!");
}
```

---

## Data Flow

### Text-Based Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         PLUGIN LOADING                              │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Plugin.Awake()                                                     │
│  ├── new CheatConfig(Config)                                        │
│  ├── UnityAnnotationHelper.RunAllInit()                            │
│  └── PatchVersionText()                                            │
└─────────────────────────────────────────────────────────────────────┘
                                │
            ┌───────────────────┼───────────────────┐
            ▼                   ▼                   ▼
┌───────────────────┐ ┌───────────────────┐ ┌─────────────────────────┐
│ ReflectionHelper  │ │    FlagManager    │ │  CheatMenuGui.Init()   │
│ - Creates Harmony │ │ - Init flags dict │ │ - GetAllCheatMethods() │
│ - Patching logic  │ │                   │ │ - GroupCheatsByCategory│
└───────────────────┘ └───────────────────┘ │ - BuildGUIContentFn()  │
                                             └─────────────────────────┘
                                                        │
                                                        ▼
                                             ┌─────────────────────────┐
                                             │  IL Generated GUI       │
                                             │  Content Method         │
                                             └─────────────────────────┘
                                                        │
                                                        ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         RUNTIME                                     │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Player Input (M key or R3)                                         │
│  └── CheatMenuGui.GuiEnabled = !GuiEnabled                         │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  OnGUI() Loop                                                       │
│  ├── If GuiEnabled:                                                 │
│  │   └── Render animated menu frame                                 │
│  │       ├── Category buttons (if no category selected)            │
│  │       ├── SubGroup buttons (if in category)                     │
│  │       └── Cheat buttons (filtered by sub-group/DLC)             │
│  │                                                                     │
│  └── If button clicked:                                             │
│      └── Execute cheat method via IL-generated delegate            │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Cheat Execution                                                    │
│  ├── Simple cheats: Direct method call                              │
│  │   └── CheatMethod()                                              │
│  │       └── CultUtils.PlayNotification()                            │
│  │                                                                     │
│  └── Mode cheats: Toggle flag + method call                         │
│      ├── FlagManager.SetFlagValue(flagId, true/false)              │
│      └── CheatMethod(true/false)                                    │
└─────────────────────────────────────────────────────────────────────┘
```

### GUI Rendering Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│  DefinitionManager.BuildGUIContentFn()                              │
│  (Called once at startup, generates IL)                             │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│  IL-Generated Delegate: s_guiContent()                            │
│  ├── Checks IsWithinCategory()                                      │
│  │   └── FALSE: Render CategoryButtons                             │
│  │         └── "Health", "Combat", "Resources", etc.              │
│  │                                                                     │
│  │   └── TRUE: Render BackButton                                    │
│  │                                                                     │
│  ├── For each category:                                             │
│  │   └── IsWithinSpecificCategory()                                 │
│  │       └── TRUE: Render SubGroupButtons                           │
│  │                                                                     │
│  └── For each cheat in category:                                    │
│      ├── Check SubGroup filter                                       │
│      ├── Check DLC requirement                                       │
│      └── Render Button (simple or toggle)                           │
│          └── On click: Execute method                               │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Configuration System

The configuration uses BepInEx's `ConfigFile` system.

### Configuration File Location
`BepInEx/config/org.xunfairx.cheat_menu.cfg`

### Default Configuration
```ini
[Keybinds]
GUIKey = M
Back Category = N

[Options]
Close GUI on escape = True

[Controller]
Enable Controller Support = True
```

### Keybinds
- **GUIKey (M)**: Opens/closes the cheat menu
- **Back Category (N)**: Goes back to previous menu level

---

## Harmony Patching

The mod uses HarmonyLib for runtime method patching.

### ReflectionHelper.cs

Centralized patching management.

```csharp
public static class ReflectionHelper {
    private readonly static string HarmonyId = "org.xunfairx.cheat_menu";
    private static Harmony s_harmonyInstance;
    private static Dictionary<string, PatchTrackerDetails> s_patchTracker;

    [Init]
    [EnforceOrderFirst(9)]
    public static void Init(){
        s_harmonyInstance = new Harmony(HarmonyId);
        s_patchTracker = new Dictionary<string, PatchTrackerDetails>();
    }

    public static string PatchMethodPrefix(Type classDef, string methodName, 
        MethodInfo patchMethod, BindingFlags flags = BindingFlags.Default, 
        Type[] typeParams = null, bool silent = false)
```

### GlobalPatches.cs

Contains all patch implementations.

| Method Patched | Purpose |
|---------------|---------|
| `Interactor.Update` | Disable interaction when GUI open |
| `UpgradeSystem.UnlockAbility` | Fix ritual pair display |
| `PlayerFarming.Bleat` | Suppress R3 bleat sound |
| `Follower.Init` | Validate follower data |
| `Follower.Tick` | Guard null outfit |
| `Interaction_WolfBase.Update` | Make wolf follow player |
| `Spine.Skin.AddSkin` | Prevent null reference |

---

## Usage Examples

### Example 1: Simple One-Time Cheat

```csharp
// In any Definitions class
[CheatDetails("Full Heal", "Fully heals the player")]
public static void FullHeal(){
    if(PlayerFarming.Instance != null){
        PlayerFarming.Instance.health.Heal(999f);
        CultUtils.PlayNotification("Fully healed!");
    }
}
```

### Example 2: Toggle Cheat (Mode Cheat)

```csharp
[CheatDetails("Godmode", "Godmode (OFF)", "Godmode (ON)", 
    "Full invincibility", true, subGroup: "Modes")]
public static void GodMode(bool flag){
    foreach(var player in PlayerFarming.players){
        player.health.GodMode = flag ? Health.CheatMode.God : Health.CheatMode.None;
    }
    CultUtils.PlayNotification(flag ? "Godmode ON!" : "Godmode OFF!");
}
```

### Example 3: DLC-Gated Cheat

```csharp
[RequiresDLC(DlcRequirement.MajorDLC)]
[CheatDetails("Woolhaven Resources", "Gives Woolhaven-specific resources")]
public static void GiveWoolhavenResources(){
    // Only runs if player owns Woolhaven DLC
    CultUtils.AddInventoryItem(InventoryItem.ITEM_TYPE.WOOL, 100);
}
```

### Example 4: Sub-Group Organization

```csharp
// Grouped under "Hearts" sub-group
[CheatDetails("Add Red Heart", "Adds a red heart container", subGroup: "Hearts")]
public static void AddRedHeart(){ /* ... */ }

[CheatDetails("Add Blue Heart", "Adds a blue heart", subGroup: "Hearts")]
public static void AddBlueHeart(){ /* ... */ }
```

### Example 5: GUI Window Cheat

```csharp
[CheatDetails("Item Spawner", "Opens item spawner GUI")]
public static void OpenItemSpawner(){
    // Set the GUI function for this cheat
    GUIManager.SetGuiWindowScrollableFunction(
        new GUIUtils.ScrollableWindowParams("Item Spawner", 400, 500),
        () => {
            // Render GUI content
            GUILayout.Label("Select Item Type:");
            // Add your GUI controls here
        }
    );
}
```

---

## Key Classes and Relationships

### Class Dependency Diagram

```
Plugin
  │
  ├── CheatConfig (singleton)
  │   └── ConfigEntry<KeyboardShortcut>
  │   └── ConfigEntry<bool>
  │
  ├── UnityAnnotationHelper
  │   ├── Init() methods collection
  │   ├── Unload() methods collection
  │   ├── OnGui() methods collection
  │   └── Update() methods collection
  │
  ├── ReflectionHelper
  │   └── Harmony instance
  │
  ├── DefinitionManager
  │   ├── Definition
  │   │   ├── CheatDetails
  │   │   ├── CheatCategory
  │   │   └── RequiresDLC
  │   │
  │   └── IDefinition (marker interface)
  │
  ├── FlagManager (singleton)
  │
  ├── CheatMenuGui
  │   ├── GUIUtils
  │   ├── GUIManager
  │   └── NotificationHandler
  │
  └── GlobalPatches
      └── Harmony patches
```

### Class Summary Table

| Class | Type | Purpose |
|-------|------|---------|
| `Plugin` | Main Entry | BepInEx plugin bootstrap |
| `CheatConfig` | Config | User preferences and keybinds |
| `CheatUtils` | Utilities | Helper functions |
| `CultUtils` | Utilities | Game system interactions |
| `DefinitionManager` | Manager | Cheat discovery and grouping |
| `FlagManager` | Manager | Toggle state storage |
| `GlobalPatches` | Patches | Harmony method modifications |
| `CheatMenuGui` | GUI | Menu rendering |
| `GUIManager` | GUI | Dynamic window management |
| `GUIUtils` | GUI | GUI helper functions |
| `ReflectionHelper` | Helpers | Reflection and patching |
| `UnityAnnotationHelper` | Helpers | Lifecycle management |
| `CheatDetails` | Attribute | Cheat metadata |
| `CheatCategory` | Attribute | Category assignment |
| `RequiresDLC` | Attribute | DLC gating |

---

## Appendices

### Appendix A: Cheat Categories

| Enum | Display Name | Description |
|------|--------------|-------------|
| `HEALTH` | Health | Player health modifications |
| `COMBAT` | Combat | Combat-related cheats |
| `RESOURCE` | Resources | Resource spawning |
| `CULT` | Cult | Cult management |
| `FOLLOWER` | Follower | Follower management |
| `FARMING` | Farming | Farming cheats (DLC) |
| `COMPANION` | Companion | Companion cheats |
| `WEATHER` | Weather | Weather control |
| `DLC` | DLC | DLC-specific features |
| `SPLITSCREEN` | Splitscreen | Multiplayer cheats |
| `MISC` | Misc | Miscellaneous |
| `ANIMATION` | Animation | Animation cheats |
| `SCENERY` | Scenery | Scenery modifications |

### Appendix B: DLC Requirements

| Enum | DLC Name |
|------|----------|
| `None` | No DLC required |
| `MajorDLC` | Woolhaven (Cult of the Lamb: Expansion) |
| `SinfulDLC` | Sinful Pack |
| `CultistDLC` | Cultist Pack |
| `HereticDLC` | Heretic Pack |
| `PilgrimDLC` | Pilgrim Pack |

### Appendix C: Game References

The mod interacts with various game systems:
- `PlayerFarming` - Player controller
- `DataManager` - Save data
- `Inventory` - Item inventory
- `StructureManager` - Building management
- `FollowerManager` - Follower management
- `DoctrineUpgradeSystem` - Doctrine upgrades
- `UpgradeSystem` - Technology tree

---

*Documentation generated for Cheat Menu v1.3.2*
*For the game "Cult of the Lamb"*
