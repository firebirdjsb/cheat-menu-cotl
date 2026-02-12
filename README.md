<<<<<<< HEAD
# cheat-menu-cotl

> **Version 1.1.0** - Weather Fix, New Items, QOL & Assembly Update!

Provides a list of cheats/utilities in an easily accessible **compact GUI**.

## What's New in v1.1.0

- **Weather System Rewrite** - Uses `WeatherSystemController` directly with proper `WeatherType` and `WeatherStrength` enums. Supports Rain (Light/Heavy), Wind, Snow (Light/Blizzard), Heat, and Clear
- **New QOL Category** - Season switching, clear all cooldowns, pause simulation, max all follower stats, unlock everything in one click
- **New Resource Items** - Silk Thread, Sin Drinks, God Tears, Relics, Talisman, Soot, Trinket Cards, Flowers, DLC Necklaces, All Drinks, Give All Items
- **Assembly Dump Integration** - All APIs verified against the actual decompiled game assembly for accuracy
- **Controller Support** - Full Rewired-based gamepad input (D-pad, sticks, A/B/Start buttons)
- **Slide Animation** - Menu slides in from bottom-left corner with easing
- **Locked Position** - Menu is fixed in bottom-left, no accidental dragging

---

## Features & Usage
Press the ```M``` key to activate the cheat menu and click on any of the buttons to enable/disable the specific cheats or enter that specific cheat category. ```N``` key can be used to go back from a category. Both these keys can be changed in the configuration in the ```config``` folder.

### Controller Support
**Full gamepad/controller support** via Rewired (the game's native input system)! Navigate the menu using:
- **D-Pad / Left Stick** - Navigate between options (highlighted with red glow)
- **R3 (Right Stick Click)** - Open/close menu
- **B/Circle Button** - Go back to previous menu (or close menu from main)
- **A/Cross Button** - Select/activate option

Works with Xbox, PlayStation, Nintendo Switch Pro, and generic controllers!

To see what cheats/utilities the mod offers see below:
[Available Cheats](doc/cheats.md)

Latest changes: [1.1.0](doc/changelogs/1.1.0.md) - **Weather Fix, New Items, QOL & Assembly Update!**

**Note**: This version uses APIs verified against the decompiled game assembly. Compatible with Cult of the Lamb latest version.
=======
# Changelog 1.1.0
>>>>>>> c4d44a09e9f70981b2bfd0c15d720f46e85ef454

## 🎮 Weather Fix, New Items, QOL & Assembly Update!

This major release brings a complete weather system rewrite, tons of new items and cheats, a new Combat/QOL category, and full assembly-verified API compatibility.

---

## ⚡ Major Features

### Weather System Complete Rewrite
- **Direct WeatherSystemController Integration** - Uses the game's native `WeatherType` and `WeatherStrength` enums
- **All Weather Types Supported**:
  - Rain (Light & Heavy)
  - Wind
  - Snow (Light Dusting & Blizzard)
  - Heat Wave
  - Clear (stops all weather)
- **Season Control** - Switch between Spring and Winter seasons

### New Combat/QOL Category
- **Kill All Enemies** - Instantly eliminate all enemies in the current room
- **Unlock All Weapons** - Unlock Axe, Dagger, Gauntlet, Hammer + all curse packs
- **Unlock All Tarot Cards** - Unlock every tarot card in the game
- **Unlock All Fleeces** - Get all fleece skins for the Lamb
- **Unlock EVERYTHING** - One-click unlock for ALL upgrades, rituals, weapons, structures, and tarot
- **Show All Map Locations** - Toggle to reveal all locations on the world map
- **Enable Black Souls** - Activate the black souls currency system
- **Enable Tarot Building** - Unlock the tarot card reading building

### New Resource Items (30+ New Items!)
- **Materials**: Silk Thread (x50), Crystals (x50), Bones (x50), Lumber (x100), Stone (x100)
- **Special Items**: God Tears (x10), Relics (x10), Talisman (x10), Trinket Cards (x10)
- **Food & Flowers**: All Flowers, All Meals (18 types), All Seeds
- **Fish**: All 11 fish types including DLC varieties
- **Refined Materials**: Refined lumber, stone, gold, nuggets, rope (x50 each)
- **Gold Coins**: x500 per use
- **Give ALL Items**: One button to add x10 of every item type in the game!

---

## 🎮 Controller Support Enhancements

### Rewired-Based Input System
- Full integration with the game's native Rewired input system
- Works with whatever controller the game detects automatically
- **R3 (Right Stick Click)** - Open/Close menu
- **A/Cross Button** - Select/Activate options
- **B/Circle Button** - Go back (or close menu from main screen)
- **D-Pad / Left Stick** - Navigate menu options
- **Visual Feedback** - Red hover glow shows currently selected option

### Controller Compatibility
- Xbox One/Series controllers
- PlayStation DualShock 4 & DualSense
- Nintendo Switch Pro Controller
- Steam Deck built-in controls
- Generic USB/Bluetooth gamepads

---

## 🖥️ UI/UX Improvements

### Slide Animation
- Menu slides in from the bottom-left corner with smooth easing
- Animation plays on both open and close
- Works even when game is paused (uses unscaledDeltaTime)

### Compact Design
- **Window Size**: 350x400px - more compact, less intrusive
- **Fixed Position**: Bottom-left corner, no accidental dragging
- **Improved Hints Bar**: Shows context-aware keybindings above the menu
- **Cult Theme**: Dark burgundy/crimson styling matching game aesthetic

### Visual Polish
- Red highlight glow for controller-selected buttons
- Clear [ON]/[OFF] toggle indicators
- Category arrows (>> <<) for navigation clarity
- Bone white text with golden accents

---

## 🔧 Technical Improvements

### Assembly-Verified APIs
- All game APIs verified against decompiled Assembly-CSharp.dll
- Proper enum types used throughout (WeatherType, WeatherStrength, etc.)
- Graceful error handling for missing methods

### Code Quality
- **RewiredInputHelper** - Clean abstraction for controller input
- **Better Error Handling** - Try-catch protection on all risky operations
- **Detailed Logging** - Clear warnings instead of crashes
- **Compatibility Mode** - Mod functions even if some patches fail

---

## 📋 Complete Cheat List by Category

### Combat/QOL
- Kill All Enemies
- Unlock All Weapons
- Unlock All Tarot Cards
- Enable Tarot Building
- Enable Black Souls
- Unlock All Fleeces
- Unlock EVERYTHING
- Show All Map Locations (Toggle)

### Weather
- Rain (Light/Heavy)
- Wind
- Snow (Light/Blizzard)
- Heat Wave
- Clear Weather
- Season: Spring
- Season: Winter

### Resources (25+ options)
- Basic Resources, Seeds, Fish, Food
- Necklaces (all 18 types including DLC)
- Materials & Refined Materials
- Flowers, Meals, Trinkets
- Give ALL Items

### Cult (15+ options)
- Building, Structure & Ritual management
- Cleanup cheats (poop, vomit, bodies, trees, grass, rubble)
- Teleport to Cult
- Free Building Mode

### Followers (10+ options)
- Spawn, Kill, Revive followers
- Faith & Hunger management
- Sickness removal
- Age manipulation

### Health
- Godmode
- Heal, Add hearts (Black/Blue)
- Die command

### Misc
- Noclip, Skip Hour
- Debug displays (FPS, Follower, Structure)
- Hide/Show UI

### DLC
- Auto Shear Wool (Woolhaven)

---

## 🔄 Migration from Previous Versions

1. Delete old CheatMenu files from `BepInEx/plugins/CheatMenu`
2. Install v1.1.0 as fresh installation
3. Config will auto-generate with new options
4. Previous keybinds may need reconfiguration

---

## 📦 Dependencies

- **BepInEx 5.4.21+**
- **Cult of the Lamb v1.5.16.1000+**

---

## 🙏 Credits

- **Original Author**: Wicked7000
- **Current Maintainer**: XUnfairX
- **Repository**: https://github.com/firebirdjsb/cheat-menu-cotl

---

**Enjoy the most comprehensive Cult of the Lamb cheat menu! 🐑✨**
