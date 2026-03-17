# 🍇 Cheat Menu for Cult of the Lamb

> **v1.3.9** — The ultimate cheat menu with 150+ cheats, full controller support, and QoL features.

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/firebirdjsb/cheat-menu-cotl)
[![BepInEx](https://img.shields.io/badge/BepInEx-5.4.21+-green)](https://github.com/BepInEx)

---

## ✨ What's New (v1.3.9)

- **Menu Restructuring** — Resources menu reorganized for better usability
- **Clear Inventory** — Now accessible at top-level Resources menu (under back button)
- **Give Resources** — Moved from Currency to Materials subGroup
- **Deleted some unused cheats**

---
## ✨ What's New (v1.3.7)

 **Save Editor** — password LOCKED in-game save file editor with follower, player, cult, inventory, and game state editing 
## [1.3.7] - 2026 (Woolhaven Gate & Quest Clothing Fix)

### 🐛 Bug Fixes
- **Fixed "Set Buried Fleeces"** — The cheat now properly removes special wool items (SPECIAL_WOOL_RANCHER, SPECIAL_WOOL_LAMBWAR, SPECIAL_WOOL_BLACKSMITH, SPECIAL_WOOL_TAROT, SPECIAL_WOOL_DECORATION) from inventory in addition to setting the NPC rescue flags. This fixes the Woolhaven gate not opening after using the cheat.

### ⚠️ Clothing Bug Fixes
- **Fixed "Unlock All Clothing" / "Give All Clothing"** — These cheats now skip quest-specific clothing types that are given by NPCs during gameplay. Unlocking these early can cause softlocks when the game tries to give them through quest progression.
- **Excluded Quest Clothing:** Special_1-7, Normal_MajorDLC_1/3/6, Winter_1-6, Apple_1-2
- **Added "Fix Quest Clothing Bug"** — New cheat to fix affected saves by removing quest-specific clothing from the unlocked list. Players whose games are stuck can run this cheat and reload their save to fix the softlock.

## 🚀 Quick Start

1. Install **BepInEx 5.4.21+**
2. Drop `CheatMenu.dll` into `BepInEx/plugins/CheatMenu/`
3. Press **M** (or **R3** on controller) to open the menu

---

## 🎮 Controls

| Input | Action |
|-------|--------|
| **M** | Open/close menu |
| **N** | Go back |
| **R3** | Controller: open/close |
| **A** | Select |
| **B** | Back/close |

Full gamepad support for Xbox, PlayStation, Switch Pro, Steam Deck!

---

## 🕹️ Features

### 💾 Save Editor
Full in-game save file editor with follower, player, cult, inventory, and game state editing

### Health & Combat
Godmode, One Hit Kill, Unlimited Ammo, Unlock All Weapons & Tarots, **Real-time Equipment Swapping**

### Resources (40+)
All seeds, fish, food, materials, crystals, gold, souls, arrows

### Cult Management
Teleport, Free Building, Unlock All Structures, Cleanup (rubble, trees, poop), Harvest All

### Followers
Spawn Worker/Worshipper/Child, Kill All, Revive All, Level Up, Make Immortal

### Farming
Spawn animals (goat, cow, llama, turtle...), Give All Wool, Eggs, Milk

### Weather
Clear, Rain, Snow, Wind, Heat — full control

### DLC
All DLC content properly gated — only shows items you own

---

## ⚙️ Config

Location: `BepInEx/config/org.xunfairx.cheat_menu.cfg`

```ini
[Controller]
Enable Controller Support = true

[Keybinds]
GUIKey = M
Back Category = N
```

---

## 🐛 Issues?

Report on [GitHub Issues](https://github.com/firebirdjsb/cheat-menu-cotl/issues)

---

*Made with ❤️ for the Cult of the Lamb community*
