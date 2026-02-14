# ?? v1.2.0 - Major Update

The biggest update yet! Complete menu reorganization, weather overhaul, and 150+ cheats across 10 well-organized categories.

## ??? Menu Reorganization
- **Reorganized all categories** into a logical flow: **Health ? Combat ? Resources ? Cult ? Follower ? Farming ? Weather ? DLC ? Splitscreen ? Misc**
- **Fixed misplaced items**:
  - Give Souls, Give Black Souls, Give Arrows moved from Health ? **Resources**
  - Player Speed x2, Stop Time In Crusade moved from Combat ? **Misc**
- **Removed unused categories** (Rituals, Structures) that had no cheats assigned
- **Consistent ordering** - categories always appear in the same order

## ??? Weather Menu Overhaul
- **Logically grouped**: Clear ? Rain (Light/Heavy) ? Wind (Light/Heavy) ? Snow (Dusting ? Light ? Medium ? Heavy ? Blizzard) ? Heat (Light/Heavy) ? Seasons
- **Cleaned up names** - removed redundant "Weather:" prefix
- **Custom sort ordering** keeps related weather types together

## ?? Technical Improvements
- New `SortOrder` system in `CheatDetails` attribute for precise menu item ordering
- Deterministic category rendering via enum order
- Cleaner codebase with dead code removed

## ?? All Changes Since v1.1.0

### v1.1.1 - Follower Costume Bug Fix
- Fixed follower costume NullReferenceException spam in BepInEx logs

### v1.1.2 - Scene-Awareness Fixes
- Fixed FollowerBrainInfo.Protection NullReferenceException
- GiveAllClothing now only updates active followers

### v1.1.3 - Critical Loading Fix
- Fixed game hanging at loading screen after using "Give All Clothing Items"
- Added "Reset All Follower Outfits" emergency cheat

### v1.1.4 - Enhanced Protection
- Defensive patch on Follower.Init() validates skeleton data before outfit application
- Protects against crashes from any mod or game bug

### v1.1.5 - Cheaters Edition ??
- Game version displays as "Cheaters Edition" in main menu
- Automatic DLC authentication for all packs including Woolhaven
- Janitor station XP fix for "Clear Poop" cheat

### v1.2.0 - Major Update (this release)
- Complete menu reorganization
- Weather overhaul with logical grouping
- Fixed all misplaced menu items
- Added SortOrder system
- 150+ cheats across 10 categories

## ?? Installation
1. Install BepInEx 5.4.21+
2. Extract `CheatMenu.dll` to `BepInEx/plugins/CheatMenu/`
3. Launch game, press **M** (or **R3** on controller) to open menu

## ?? Upgrading from v1.1.x
- Drop-in replacement - just replace the DLL
- Some cheats moved to different categories (see above)
- No config changes required

---
**Compatible with Cult of the Lamb v1.5.16.1000+**
