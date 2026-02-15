# cheat-menu-cotl

> **Version 1.2.5** - Animation Update 🎬✨
A comprehensive cheat menu mod for **Cult of the Lamb** that provides an easily accessible **compact GUI** with 150+ cheats, full controller support, a friendly wolf companion system, extensive quality-of-life features, and 11 pre-made player animations.

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/firebirdjsb/cheat-menu-cotl)

---

## 🎮 What's New in v1.2.5 - Animation Update

### 🎬 New: Animation Tab
- **11 pre-made player animations** with verified names from assembly dump
- Smooth animation playback with working animation states
- All animations tested and verified to work correctly
- Easy one-click access to pose and animate your character

### 🎯 Latest Updates & Fixes (v1.2.1 - v1.2.5)
- **Fixed**: Clear ALL rubble types in ClearBaseRubble (properly clears all landscape rubble)
- **Enhanced**: Keyboard counterparts now display in menu hints
- **Renamed**: Controller button R3 → LeftStickClick for clarity
- **Fixed**: Thunderstore manifest compatibility (semver + correct dependency ID)

---

## 📚 Previous Major Features in v1.2.0 - MAJOR UPDATE

### 🐺 New: Companion Category
- **Spawn Friendly Wolf** — A tame wolf that follows you everywhere with smooth movement and auto-respawn across scene transitions
- **Wolf Dungeon Combat** — Toggle your wolf attacking enemies in dungeons with melee damage and attack animations
- **Pet Wolf** — Walk up to and pet your wolf with full animation, sound effects, hearts, and camera shake
- **Dismiss Wolf** — Dismiss your friendly wolf or clear all spawned wolves

### 🗂️ Complete Menu Reorganization
- **Reorganized all categories** into a logical flow: Health → Combat → Resources → Cult → Follower → Farming → Companion → Weather → DLC → Splitscreen → Misc
- **Fixed misplaced items**: Souls, Black Souls, and Arrows moved from Health → Resources; Player Speed and Stop Time moved from Combat → Misc
- **Removed unused categories** (Rituals, Structures) that had no cheats assigned
- **Consistent category ordering** - categories now always appear in the same order

### 🐾 Farming Improvements
- **Add Halos to Animals** — Glowing pink procedural halos above ranch animals with per-creature height tuning
- **Ascend All Animals** — Full ascension animation with chromatic aberration, sound effects, and visual resource drops

### 👥 Follower Improvements
- **8 new follower cheats**: Spawn Child, Kill Random, Level Up All, Increase Loyalty, Make Immortal, Max All Stats, Remove Exhaustion, Give Follower Tokens

### 🌦️ Weather Menu Overhaul
- **Logically grouped** weather items: Clear → Rain (Light/Heavy) → Wind (Light/Heavy) → Snow (Dusting/Light/Medium/Heavy/Blizzard) → Heat (Light/Heavy) → Seasons
- **Cleaned up names** - removed redundant "Weather:" prefix for cleaner display
- **Custom sort ordering** ensures related weather types stay together instead of being scattered alphabetically

### ⚔️ Combat Category (Refined)
- Focused on combat-specific cheats: Kill All Enemies, One Hit Kill, Unlimited Relics/Ammo/Fervour
- Dungeon tools: Auto Reveal Map, Reveal Map, Clear Status Effects
- Unlock progression: Weapons, Tarot Cards, Fleeces, Crown Abilities, EVERYTHING
- Removed non-combat items (Player Speed, Stop Time) to proper categories

### 🔧 Technical Improvements
- **New `SortOrder` system** for precise item ordering within categories via `CheatDetails` attribute
- **Deterministic category rendering** - enum order controls category button order
- **Cleaner codebase** - removed dead enum values and properly separated concerns

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
