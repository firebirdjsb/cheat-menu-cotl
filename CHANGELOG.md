# Changelog

All notable changes to the Cult of the Lamb Cheat Menu mod will be documented in this file.

## [1.1.2] - 2024 (Current Release - Patch)

### Bug Fixes
- **Fixed**: FollowerBrainInfo.Protection NullReferenceException for followers not in scene
- **Fixed**: GiveAllClothing now only updates followers that are active in the current scene
- **Fixed**: SpawnFollower checks if follower is active before calling CheckChangeState()
- **Improved**: Scene-awareness checks prevent accessing unloaded follower data

### Technical Details
- Added `gameObject.activeInHierarchy` checks before updating follower outfits
- Wrapped `CheckChangeState()` calls in try-catch with initialization validation
- Deferred outfit updates for followers not in scene - they apply when they load
- Better detection of which followers are actually loaded and accessible

---

## [1.1.1] - 2024 (Patch Release)

### Bug Fixes
- **Fixed**: Follower costume NullReferenceException spam in BepInEx logs
- **Fixed**: SpawnFollower now properly validates follower components before setting outfits
- **Fixed**: GiveAllClothing safely handles follower outfit updates with proper null checks
- **Fixed**: TurnFollowerYoung/Old functions now handle outfit changes gracefully
- **Improved**: All follower-related operations have comprehensive error handling

### Technical Details
- Added null checks to 8 methods in CultUtils.cs
- Completely rewrote GiveAllClothing() in CultDefinitions.cs
- All follower outfit changes now wrapped in try-catch blocks
- Scene-aware follower updates (only updates loaded followers)
- Better error logging with [CheatMenu] prefix

---

## [1.1.0] - 2024

### Major Features
- **Weather System Complete Rewrite** - Direct WeatherSystemController integration with native enums
- **New Combat/QOL Category** - 8 new major features including Kill All Enemies, Unlock Everything
- **30+ New Resource Items** - Materials, special items, flowers, refined materials, and more
- **Enhanced Controller Support** - Full Rewired-based input system with bug fixes

### New Features
- Weather types: Rain (Light/Heavy), Wind, Snow (Light/Blizzard), Heat Wave, Clear
- Season control: Spring and Winter
- Kill all enemies in current room
- Unlock all weapons (Axe, Dagger, Gauntlet, Hammer + curse packs)
- Unlock all tarot cards
- Unlock all fleeces
- Unlock EVERYTHING button (one-click unlock for all upgrades, rituals, weapons, structures, tarot)
- Show all map locations toggle
- Enable Black Souls currency system
- Enable Tarot Building
- Give All Items button (x10 of every item type)
- New resources: Silk Thread, Crystals, Bones, God Tears, Relics, Talisman, Trinket Cards, all flowers, all meals, all fish, refined materials

### UI/UX Improvements
- Slide animation from bottom-left corner with smooth easing
- Animation works when game is paused (uses unscaledDeltaTime)
- Red highlight glow for controller-selected buttons
- Clear [ON]/[OFF] toggle indicators
- Category arrows (>> <<) for better navigation
- Context-aware keybinding hints bar
- Fixed position at bottom-left corner

### Bug Fixes
- **Fixed**: R3 button no longer triggers in-game "bahhh/bleat" action
- **Fixed**: Controller input now properly uses Rewired system instead of Unity Input
- **Fixed**: Controller navigation uses right stick to avoid D-pad conflicts
- **Fixed**: R3 suppression window (300ms) prevents double-action
- **Fixed**: Controller A button suppressed during R3 toggle window
- **Fixed**: Navigation delay (150ms) prevents menu scrolling too fast
- **Fixed**: Auto-scroll keeps selected button visible
- **Fixed**: Menu window ID resets to prevent Unity IMGUI caching
- **Fixed**: Animations use Time.unscaledTime for paused compatibility
- **Fixed**: Weather system uses proper WeatherSystemController API
- **Fixed**: Landscape clearing properly iterates all structure types
- **Fixed**: Janitor station clearing returns poop to inventory
- **Fixed**: Close animation properly disables GUI

### Technical Improvements
- All game APIs verified against decompiled Assembly-CSharp.dll
- Proper enum types used throughout (WeatherType, WeatherStrength, etc.)
- Graceful error handling for missing/changed methods
- PlayerFarming.Bleat patched to suppress R3 double-action
- RewiredInputHelper - clean abstraction for controller input
- Try-catch protection on all risky operations
- Detailed logging with clear warnings instead of crashes
- Compatibility mode - mod functions even if some patches fail

---

## [1.0.6] - 2024

### UI/UX Improvements
- Compact menu design: 650x750px ? 500x600px (23% narrower, 20% shorter)
- Better space efficiency: 40% more content visible without scrolling
- Tighter button layout: 45px ? 35px button height (22% reduction)
- Optimized spacing: 5px ? 3px button spacing (40% reduction)
- Smaller fonts: 2-4px reduction across all UI elements
- Reduced padding: 12px ? 8px throughout (33% reduction)
- Compact title bar: 35px ? 28px height
- Cleaner back button: "< Back to Categories" ? "< Back"
- Smaller hints bar: 550x35px ? 500x30px

### Project Organization
- Cleaned documentation: Removed 19+ unnecessary markdown files
- Removed doc/old/ directory
- Removed misplaced Assembly-CSharp.dll from root
- Streamlined file structure to essential files only

---

## [1.0.5] - 2024

### Features Added
- Full controller/gamepad support via Rewired input system
- R3 (Right Stick Click) to open/close menu
- D-Pad/Left Stick navigation with visual feedback
- A/Cross button to select, B/Circle to go back
- Controller compatibility: Xbox, PlayStation, Switch Pro, Steam Deck

### UI Improvements
- Cult theme styling: Dark burgundy/crimson colors
- Bone white text with golden accents
- Red hover glow for controller-selected buttons
- Improved visual feedback for navigation

---

## [1.0.4] - 2024

### Features Added
- Auto Shear Wool (Woolhaven DLC support)
- Additional resource items
- Improved follower management

---

## [1.0.3] - 2024

### Features Added
- Change All Rituals (with sub-menu)
- Change All Doctrines (with sub-menu)
- Allow Shrine Creation
- Clear All Doctrines
- Clear Outhouses (adds contents to inventory)
- Clear Base Trees
- Clear Dead Bodies (gives follower meat)
- Clear Faith
- Clear Poop (gives fertilizer)
- Clear Vomit
- Complete All Quests
- Convert Dissenting Followers
- Give Big Gift (x10)
- Give Commandment Stone
- Give Follower Meat (x10)
- Give Follower Necklaces
- Give Small Gift (x10)
- Max Faith
- Remove Hunger
- Remove Sickness
- Revive All Followers
- Turn All Followers Old
- Turn All Followers Young
- All Rituals toggle

### Improvements
- Cheats now ordered by name
- Cheats can have sub-menus

---

## [1.0.2] - 2024

### Initial Release Features
- Basic cheat menu GUI
- Health cheats (Godmode, Heal, Add Hearts, Die)
- Follower management (Spawn, Kill, Faith, Hunger, Illness)
- Cult management (Buildings, Rituals, Doctrines)
- Resources (Materials, Seeds, Fish, Food, Necklaces)
- Weather control (basic implementation)
- Misc utilities (Noclip, Skip Hour, Debug displays)
- Keyboard navigation support

---

## Version History

| Version | Release Date | Key Features |
|---------|--------------|--------------|
| 1.1.2 | 2024 | Follower scene management fixes (patch release) |
| 1.1.1 | 2024 | Follower costume bug fix (patch release) |
| 1.1.0 | 2024 | Weather rewrite, Combat/QOL category, 30+ new items, controller bug fixes |
| 1.0.6 | 2024 | Compact UI, documentation cleanup |
| 1.0.5 | 2024 | Controller support, Cult theme |
| 1.0.4 | 2024 | Woolhaven DLC support |
| 1.0.3 | 2024 | Rituals/Doctrines, sub-menus |
| 1.0.2 | 2024 | Initial release |

---

## Migration Notes

### From 1.1.0 to 1.1.1
- Simple drop-in replacement
- Just replace the DLL file
- No config changes needed
- Fixes follower costume log spam

### From 1.0.x to 1.1.x
1. Backup your config file (optional)
2. Delete old CheatMenu files from BepInEx/plugins/CheatMenu
3. Install v1.1.1 as fresh installation
4. Config will auto-generate with new options
5. Test controller support (enabled by default)
6. Reconfigure keybinds if needed

### Breaking Changes
- Weather API calls changed (old saves compatible)
- Controller input system completely rewritten
- Some resource item quantities increased

---

## Credits

- **Original Author**: Wicked7000
- **Current Maintainer**: XUnfairX / firebirdjsb
- **Contributors**: Community bug reports and suggestions
- **Repository**: https://github.com/firebirdjsb/cheat-menu-cotl
