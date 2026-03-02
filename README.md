# cheat-menu-cotl

> **Version 1.3.0** - DLC Safety & Stability Update 🛡️🔧
A comprehensive cheat menu mod for **Cult of the Lamb** that provides an easily accessible **compact GUI** with 150+ cheats, full controller support, a friendly wolf companion system, extensive quality-of-life features, and 11 pre-made player animations.

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/firebirdjsb/cheat-menu-cotl)

---

## 🎮 What's New in v1.3.0 - DLC Safety & Stability Update

### 🛡️ DLC Ownership Gating (Major Fix)
- **"Unlock Everything"** no longer unlocks Woolhaven/DLC tab, buildings, or upgrades when you don't own the DLC
- Added `IsDlcContentName()` centralized helper covering all known Woolhaven keywords: Ranch, Forge, Furnace, Brewery, Barn, Spinning, Loom, Tavern, Distillery, Winter, Snow, and more
- **Unlock All Structures** no longer calls `CheatConsole.UnlockAllStructures()` without DLC (root cause of the Woolhaven tab appearing)
- **Unlock All Clothing** and **Give All Clothing** now skip DLC clothing types without DLC ownership
- **Give All Clothing** no longer gives WOOL and COTTON materials without the DLC
- **Upgrade unlock loop** expanded DLC filter beyond just "DLC"/"Dlc" to catch all Woolhaven-specific upgrade types

### 🩸 Blood Moon Fix (Persistence Bug)
- **Disable Blood Moon** now properly persists — the game's `FollowerBrainStats.IsBloodMoon` returns `true` when `LastHalloween` is within 3600 seconds of current time. Previously, setting it to current time meant blood moon stayed "active" for an hour and would re-enable on scene transitions via `ReEnableRitualEffects()`
- Now pushes `LastHalloween` 7200 seconds into the past so `IsBloodMoon` immediately returns `false`
- Explicitly clears `halloweenLutActive` on all LocationManagers to prevent re-trigger
- Force-resets LightingManager override state and transition settings
- Music reset now respects winter season (uses `winter_random` instead of `StandardAmbience` when in winter)
- Reflection scan now also clears `halloween`-named boolean fields on DataManager

### 👥 Spawn System Overhaul
- **Removed "Spawn at Circle" toggle** — all spawned followers now always arrive at the indoctrination circle
- This prevents bugs caused by auto-indoctrination (negative traits from resurrection RNG, follower state bugs)
- **Special skin followers** (Lamb, bishops, Abomination, etc.) now also use `CreateNewRecruit` instead of `CreateNewFollower`
- **Child followers** now also spawn as recruits for proper indoctrination
- Applies to Worker, Worshipper, Arrived, Child, and all 20+ special skin spawns

### 💬 Notification Polish
- **Rename Cult** now shows feedback notification on activation and on failure
- All cheats verified to have user-facing notification feedback on success and failure

### 📋 Previous Major Features (v1.2.x)

<details>
<summary>Click to expand v1.2.x changelog</summary>

#### v1.2.5 - Animation Update
- 11 pre-made player animations with verified names from assembly dump
- Smooth animation playback with working animation states

#### v1.2.0 - Major Update
- **Companion Category**: Friendly wolf with follow/combat/pet/dismiss
- **Complete menu reorganization** into logical flow
- **Farming improvements**: Animal halos, ascension animation
- **8 new follower cheats**: Spawn Child, Kill Random, Level Up, etc.
- **Weather menu overhaul** with sort ordering
- **Combat category refined** with unlock progression
- **SortOrder system** for precise item ordering

</details>

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

### Health
- Godmode, Demigod Mode, Immortal Mode (toggles)
- Heal x1, Full Heal
- Add hearts: Red, Blue, Black, Spirit, Fire, Ice
- Unlimited HP (toggle)
- Die

### Combat
- Kill All Enemies, One Hit Kill (toggle)
- Unlimited Relics, Unlimited Ammo, Unlimited Fervour (toggles)
- Auto Reveal Dungeon Map, Reveal Dungeon Map
- Clear Status Effects
- Unlock All Weapons, Tarot Cards, Fleeces, Crown Abilities
- Enable Tarot Building, Enable Black Souls
- Unlock EVERYTHING

### Resources (40+ items)
- Basic Resources, Seeds, Fish (11 types), Food, All Meals (18 types)
- Necklaces (all 18 types including DLC)
- Materials, Refined Materials, Flowers, Trinkets
- Crystals, Bones, Lumber, Stone, Silk Thread
- God Tears, Relics, Talisman, Gold Coins
- Souls, Black Souls, Arrows
- Ability Points, Disciple Points
- Give ALL Items

### Cult (20+ options)
- Teleport, Rename Cult, Allow Shrine Creation
- Building management: Free Building, Build All, Unlock All, Repair All
- Cleanup: Rubble, Trees, Landscape, Vomit, Poop, Dead Bodies, Outhouses
- Farming: Harvest All, Grow All Crops
- Rituals: Change Rituals (GUI), Clear Doctrines, All Rituals toggle
- Clothing: Unlock All, Give All Clothing Items
- Auto Clear Ritual Cooldowns

### Followers (15+ options)
- Spawn (Worker, Worshipper, Arrived, Child)
- Kill All, Kill Random, Revive All
- Remove Sickness, Remove Hunger, Remove Exhaustion
- Convert Dissenting, Max Faith, Clear Faith
- Level Up All, Increase Loyalty, Make Immortal
- Max All Stats, Give Follower Tokens
- Age manipulation (Young/Old)
- Reset All Outfits (emergency fix)

### Farming (20+ options)
- Spawn animals: Goat, Cow, Llama, Turtle, Crab, Spider, Snail
- Give x5 All Animals, All Wool, Eggs & Yolks, Special Poop, Milk
- Collect Products, Feed All, Clean All Pens, Force Grow
- Add Halos to Animals (glowing pink procedural halos)
- Ascend All Animals (full animation sequence with resource drops)

### Companion 🐺
- Spawn Friendly Wolf (follows you, auto-respawns across scenes)
- Wolf Dungeon Combat toggle (wolf attacks enemies in dungeons)
- Pet Wolf (full animation with hearts and camera shake)
- Dismiss Wolf

### Weather (14 options)
- Clear Weather
- Rain (Light, Heavy)
- Wind (Light, Heavy)
- Snow (Dusting, Light, Medium, Heavy), Blizzard
- Heat (Light, Heavy)
- Season: Spring, Winter

### DLC (15+ options)
- Seeds, Drinks, Brewing Ingredients, Forge Materials
- Sin Items, Broken Weapons, Legendary Fragments
- Collectibles, Necklace, Flockade Pieces, Story Items
- Reset DLC Dungeon, Fill/Empty Furnace
- Make Spies Leave, Finish Missions, Unlock Winter Mode

### Splitscreen
- Full Player 2 support: Heal, Hearts, Godmode, Die

### Misc
- Noclip, FPS/Follower/Structure Debug
- Hide/Show UI, Skip Hour/Day, Skip To Night/Dawn
- Complete All Quests, End Knucklebones
- Game Speed x2/x4, Pause Simulation
- Player Speed x2, Stop Time In Crusade

---

## 🔄 Migration from Previous Versions

### From v1.2.x to v1.3.0

1. **Backup your config** (optional): Copy `BepInEx/config/org.xunfairx.cheat_menu.cfg`
2. **Delete old files**: Remove old CheatMenu files from `BepInEx/plugins/CheatMenu`
3. **Install v1.3.0**: Extract the new version to `BepInEx/plugins/`
4. **Menu changes**:
   - The "Spawn at Circle" toggle has been removed — spawning always uses indoctrination circle now
   - Unlock cheats now properly skip DLC content you don't own (no more phantom Woolhaven tab)
   - Blood Moon disable is now persistent across scene transitions
5. **Config compatible**: No config changes required

### From v1.1.x to v1.2.0

1. **Backup your config** (optional): Copy `BepInEx/config/org.xunfairx.cheat_menu.cfg`
2. **Delete old files**: Remove old CheatMenu files from `BepInEx/plugins/CheatMenu`
3. **Install v1.2.0**: Extract the new version to `BepInEx/plugins/`
4. **Menu changes**: Categories are reorganized - some cheats moved to different categories:
   - Souls/Black Souls/Arrows are now in **Resources** (were in Health)
   - Player Speed x2 and Stop Time In Crusade are now in **Misc** (were in Combat)
   - Weather items renamed (no more "Weather:" prefix)
5. **Config compatible**: No config changes required

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

## 🐛 Known Issues

There are still bugs with this menu, especially with clothing and some other niche things. Please report issues or fix them by submitting a [pull request](https://github.com/firebirdjsb/cheat-menu-cotl/pulls).

Report issues on [GitHub Issues](https://github.com/firebirdjsb/cheat-menu-cotl/issues)

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

*Compatible with Cult of the Lamb v1.5.16.1000+ | 150+ cheats across 11 categories*
