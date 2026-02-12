# cheat-menu-cotl

> **Version 1.1.2** - Additional Follower Fixes

A comprehensive cheat menu mod for **Cult of the Lamb** that provides an easily accessible **compact GUI** with 100+ cheats, full controller support, and extensive quality-of-life features.

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/firebirdjsb/cheat-menu-cotl)

---

## 🎮 What's New in v1.1.2

### 🐛 Additional Bug Fixes
- **Fixed**: FollowerBrainInfo.Protection NullReferenceException for followers not in scene
- **Fixed**: GiveAllClothing now only updates followers that are active in the current scene
- **Fixed**: SpawnFollower checks if follower is active before calling CheckChangeState()
- **Improved**: Scene-awareness - operations only affect loaded followers

This patch addresses additional follower-related errors discovered after v1.1.1 release.

---

## 🎮 What's in v1.1.0

### 🌦️ Weather System Complete Rewrite
- **Direct WeatherSystemController Integration** - Uses the game's native `WeatherType` and `WeatherStrength` enums
- **All Weather Types Supported**: Rain (Light/Heavy), Wind, Snow (Light Dusting/Blizzard), Heat Wave, Clear
- **Season Control** - Switch between Spring and Winter seasons
- **Bug Fix**: Weather changes now properly use game's internal API instead of deprecated methods

### 🎯 New Combat/QOL Category
- **Kill All Enemies** - Instantly eliminate all enemies in the current room
- **Unlock All Weapons** - Unlock Axe, Dagger, Gauntlet, Hammer + all curse packs
- **Unlock All Tarot Cards** - Unlock every tarot card in the game
- **Unlock All Fleeces** - Get all fleece skins for the Lamb
- **Unlock EVERYTHING** - One-click unlock for ALL upgrades, rituals, weapons, structures, and tarot
- **Show All Map Locations** - Toggle to reveal all locations on the world map
- **Enable Black Souls** - Activate the black souls currency system
- **Enable Tarot Building** - Unlock the tarot card reading building

### 📦 30+ New Resource Items
- **Materials**: Silk Thread (x50), Crystals (x50), Bones (x50), Lumber (x100), Stone (x100)
- **Special Items**: God Tears (x10), Relics (x10), Talisman (x10), Trinket Cards (x10)
- **Food & Flowers**: All Flowers, All Meals (18 types), All Seeds
- **Fish**: All 11 fish types including DLC varieties
- **Refined Materials**: Refined lumber, stone, gold, nuggets, rope (x50 each)
- **Gold Coins**: x500 per use
- **Give ALL Items**: One button to add x10 of every item type in the game!

### 🎮 Controller Support Enhancements

#### Rewired-Based Input System (Bug Fix)
- **Fixed**: Controller input now properly integrates with game's Rewired system instead of Unity's Input
- **Fixed**: R3 button press no longer triggers in-game "bahhh/bleat" action when opening menu
- **Fixed**: Controller navigation now uses right stick to avoid conflicting with game's D-pad controls
- **New**: R3 suppression window (300ms) prevents double-action when toggling menu
- **New**: Visual feedback with red hover glow shows currently selected option

#### Controller Mappings
- **R3 (Right Stick Click)** - Open/Close menu
- **A/Cross Button** - Select/Activate options
- **B/Circle Button** - Go back (or close menu from main screen)
- **Right Stick** - Navigate menu options (avoids D-pad conflict with game)
- **Fallback**: Arrow keys for keyboard users

#### Supported Controllers
- Xbox One/Series controllers
- PlayStation DualShock 4 & DualSense
- Nintendo Switch Pro Controller
- Steam Deck built-in controls
- Generic USB/Bluetooth gamepads

### 🖥️ UI/UX Improvements

#### Slide Animation
- **New**: Menu slides in from bottom-left corner with smooth easing
- **New**: Animation plays on both open and close
- **Fixed**: Animation works even when game is paused (uses `unscaledDeltaTime`)
- **Fixed**: Window ID properly resets to prevent position caching issues

#### Compact Design
- **Window Size**: 350x400px - more compact, less intrusive
- **Fixed Position**: Bottom-left corner, no accidental dragging
- **Improved Hints Bar**: Shows context-aware keybindings above the menu
- **Cult Theme**: Dark burgundy/crimson styling matching game aesthetic

#### Visual Polish
- **New**: Red highlight glow for controller-selected buttons
- **New**: Clear [ON]/[OFF] toggle indicators
- **Improved**: Category arrows (>> <<) for better navigation clarity
- **Improved**: Bone white text with golden accents

### 🔧 Technical Improvements & Bug Fixes

#### Assembly-Verified APIs
- **Fixed**: All game APIs now verified against decompiled Assembly-CSharp.dll
- **Fixed**: Proper enum types used throughout (WeatherType, WeatherStrength, etc.)
- **Fixed**: Graceful error handling for missing or changed methods
- **Fixed**: PlayerFarming.Bleat patched to suppress R3 double-action

#### Code Quality Improvements
- **New**: `RewiredInputHelper` - Clean abstraction for controller input
- **Fixed**: Better error handling with try-catch protection on all risky operations
- **Improved**: Detailed logging with clear warnings instead of crashes
- **Improved**: Compatibility mode - mod functions even if some patches fail
- **Fixed**: Controller selection no longer fires when R3 is held during toggle
- **Fixed**: Auto-scroll keeps selected button visible in menu

#### Bug Fixes
- **Fixed**: R3 button (Right Stick Click) no longer triggers in-game "bahhh" action when opening menu
- **Fixed**: Controller A button suppressed during R3 toggle window to prevent accidental selections
- **Fixed**: Navigation delay (150ms) prevents menu scrolling too fast
- **Fixed**: Controller navigation properly scrolls to keep selected item visible
- **Fixed**: Menu window ID resets on open to prevent Unity IMGUI position caching
- **Fixed**: Animations use `Time.unscaledTime` so they work correctly when game is paused
- **Fixed**: Weather system uses proper `WeatherSystemController` API instead of deprecated methods
- **Fixed**: Landscape clearing now properly iterates all structure types
- **Fixed**: Janitor station clearing returns poop to player inventory
- **Fixed**: Close animation properly disables GUI when complete
- **Fixed** (v1.1.1): Follower costume NullReferenceException spam eliminated

---

## 🚀 Features & Usage

Press the **`M`** key to activate the cheat menu and click any button to enable/disable specific cheats or enter that category. **`N`** key goes back from a category. Both keys can be changed in the BepInEx config folder.

### Controller Support
**Full gamepad/controller support** via Rewired (the game's native input system)! Navigate the menu using:
- **Right Stick** - Navigate between options (highlighted with red glow)
- **R3 (Right Stick Click)** - Open/close menu
- **B/Circle Button** - Go back to previous menu (or close menu from main)
- **A/Cross Button** - Select/activate option

Works with Xbox, PlayStation, Nintendo Switch Pro, Steam Deck, and generic controllers!

---

## 📋 Complete Cheat Categories

### Combat/QOL (New in 1.1.0)
- Kill All Enemies
- Unlock All Weapons
- Unlock All Tarot Cards
- Enable Tarot Building
- Enable Black Souls
- Unlock All Fleeces
- Unlock EVERYTHING
- Show All Map Locations (Toggle)

### Weather (Rewritten in 1.1.0)
- Rain (Light/Heavy)
- Wind
- Snow (Light/Blizzard)
- Heat Wave
- Clear Weather
- Season: Spring
- Season: Winter

### Resources (30+ items, many new in 1.1.0)
- Basic Resources, Seeds, Fish, Food
- Necklaces (all 18 types including DLC)
- Materials & Refined Materials
- Flowers, Meals, Trinkets
- Give ALL Items (New!)

### Cult (15+ options)
- Building, Structure & Ritual management
- Cleanup cheats (poop, vomit, bodies, trees, grass, rubble)
- Teleport to Cult
- Free Building Mode
- Doctrine & Ritual customization

### Followers (10+ options)
- Spawn, Kill, Revive followers
- Faith & Hunger management
- Sickness removal
- Age manipulation
- Convert dissenting followers

### Health
- Godmode
- Heal, Add hearts (Black/Blue)
- Die command

### Misc
- Noclip, Skip Hour
- Debug displays (FPS, Follower, Structure)
- Hide/Show UI
- Complete All Quests

### DLC
- Auto Shear Wool (Woolhaven)

For detailed cheat descriptions, see: [Available Cheats](doc/cheats.md)

---

## 🔄 Migration from Previous Versions

### From v1.0.x to v1.1.x

1. **Backup your config** (optional): Copy `BepInEx/config/org.xunfairx.cheat_menu.cfg`
2. **Delete old files**: Remove old CheatMenu files from `BepInEx/plugins/CheatMenu`
3. **Install v1.1.2**: Extract the new version to `BepInEx/plugins/`
4. **Config updates**: New options will auto-generate with defaults
5. **Controller users**: Enable controller support in config if needed (enabled by default)
6. **Keybind check**: Previous keybinds may need reconfiguration in the config file

### From v1.1.0/v1.1.1 to v1.1.2

- Simple drop-in replacement - just replace the DLL
- No config changes required
- Fixes additional follower scene-awareness issues

### From v1.1.0 to v1.1.1

- Simple drop-in replacement - just replace the DLL
- No config changes required
- Fixes follower costume bug that was causing log spam

### What Changed

- Weather cheats now use different API calls - old saves compatible
- Controller input completely rewritten - test your controller after update
- New categories added - explore the menu to find new features
- Some resource items have increased quantities (e.g., materials now give x50)
- Follower operations now have proper null checking and error handling

---

## 📦 Installation & Requirements

### Dependencies
- **BepInEx 5.4.21+** (Mod loader framework)
- **Cult of the Lamb v1.5.16.1000+** (Latest game version recommended)

### Installation Steps
1. Install BepInEx 5.4.21+ if not already installed
2. Extract `CheatMenu.dll` to `BepInEx/plugins/CheatMenu/`
3. Launch the game
4. Press **M** (or **R3** on controller) to open the menu once in-game

### Configuration
Config file location: `BepInEx/config/org.xunfairx.cheat_menu.cfg`

Key settings:
```ini
[Controller]
Enable Controller Support = true

[Keybinds]
GuiKeybind = M
BackCategory = N
CloseGuiOnEscape = true
```

---

## 🐛 Known Issues & Troubleshooting

### Controller Issues
- **Controller not detected**: Ensure controller is connected before launching game
- **Wrong buttons**: Some generic controllers may have different button mappings
- **Navigation too sensitive**: Built-in 150ms delay should prevent this, but you can navigate carefully

### Menu Issues
- **Menu won't open**: Make sure you're in-game (not main menu)
- **Animation stuck**: Press ESC to force close, then reopen
- **Window position wrong**: Delete config file to reset

### Game Compatibility
- **Weather not working**: Ensure game is updated to latest version
- **Crashes on specific cheats**: Some cheats may not work if game updates change APIs
- **Mod not loading**: Check BepInEx console for errors

Report issues on [GitHub Issues](https://github.com/firebirdjsb/cheat-menu-cotl/issues)

---

## 📝 Changelog

### Version 1.1.2 (Current - Patch Release)
- **Fixed**: FollowerBrainInfo.Protection NullReferenceException for followers not in scene
- **Fixed**: GiveAllClothing now only updates followers active in the current scene
- **Fixed**: SpawnFollower checks if follower is active before calling CheckChangeState()
- **Improved**: Scene-awareness checks prevent accessing unloaded follower data
- **Improved**: Deferred outfit updates for followers not in scene

### Version 1.1.1 (Patch Release)
- **Fixed**: Follower costume NullReferenceException spam in BepInEx logs
- **Fixed**: SpawnFollower now properly validates follower components
- **Fixed**: GiveAllClothing safely handles outfit updates with null checks
- **Fixed**: TurnFollowerYoung/Old functions handle outfit changes gracefully
- **Improved**: Comprehensive error handling for all follower operations

### Version 1.1.0
- Complete weather system rewrite with proper API integration
- 30+ new resource items
- New Combat/QOL category with 8 major features
- Full Rewired-based controller support with bug fixes
- R3 suppression to prevent double-action with in-game bahhh
- Slide animation with smooth easing
- UI improvements and visual polish
- Assembly-verified API compatibility
- Better error handling and logging
- Multiple controller-related bug fixes

For full changelog history: [doc/changelogs/](doc/changelogs/)

---

## 🙏 Credits

- **Original Author**: Wicked7000
- **Current Maintainer**: XUnfairX / firebirdjsb
- **Contributors**: Community bug reports and suggestions
- **Repository**: https://github.com/firebirdjsb/cheat-menu-cotl

---

## 📜 License

This mod is provided as-is for educational and entertainment purposes. Use at your own risk. Not affiliated with Massive Monster or Devolver Digital.

---

**Enjoy the most comprehensive Cult of the Lamb cheat menu! 🐑✨**

*Compatible with Cult of the Lamb v1.5.16.1000+*
