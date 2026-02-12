# Controller Support Guide

## Quick Reference - v1.1.0

The cheat menu now features **full Rewired-based controller support** with bug fixes for a seamless experience!

### Key Updates in v1.1.0
- **Fixed**: R3 button no longer triggers in-game "bahhh/bleat" action
- **Fixed**: Navigation now uses **Right Stick** instead of D-Pad (avoids conflicts)
- **New**: R3 suppression window prevents double-actions
- **New**: Visual red glow shows selected buttons

---

## Controller Mappings (Updated)

| Action | Button | Notes |
|--------|--------|-------|
| **Open/Close Menu** | R3 (Right Stick Click) | Button 9 - No longer triggers bahhh! |
| **Navigate** | Right Stick | Axis 2 & 3 - D-Pad left for game |
| **Select/Activate** | A / Cross | Button 0 - Suppressed during R3 toggle |
| **Go Back** | B / Circle | Button 1 |
| **Keyboard Fallback** | Arrow Keys | Always available |

---

## Supported Controllers

### Xbox One/Series Controllers
```
        [LB]         [RB]
    [LT]               [RT]
    
    [D-Pad]       [Y]
              [X]   [B] ? Go Back
           [A] ? Select
    
    [View] [Menu]
    
    Click [Right Stick] ? Open/Close Menu
```

### PlayStation DualShock 4 & DualSense
```
        [L1]         [R1]
    [L2]               [R2]
    
    [D-Pad]       [?]
              [?]   [?] ? Go Back
           [?] ? Select
    
    [Share] [Options]
    
    Click [R3] ? Open/Close Menu
```

### Nintendo Switch Pro Controller
```
        [L]          [R]
    [ZL]               [ZR]
    
    [D-Pad]      [X]
              [Y]   [A] ? Go Back
           [B] ? Select
    
    [-] [+]
    
    Click [Right Stick] ? Open/Close Menu
```

### Steam Deck Built-in Controls
- **Right Stick**: Navigate menu options
- **R3 (Click Right Stick)**: Open/Close menu
- **A Button**: Select/Activate
- **B Button**: Go Back
- **View Button**: Alternative menu toggle

---

## Button Reference Table

| Button Index | KeyCode | Xbox | PlayStation | Switch | Common Use |
|--------------|---------|------|-------------|--------|------------|
| 0 | JoystickButton0 | A | Cross (?) | B | Select/Confirm |
| 1 | JoystickButton1 | B | Circle (?) | A | Back/Cancel |
| 2 | JoystickButton2 | X | Square (?) | Y | - |
| 3 | JoystickButton3 | Y | Triangle (?) | X | - |
| 4 | JoystickButton4 | LB | L1 | L | - |
| 5 | JoystickButton5 | RB | R1 | R | - |
| 6 | JoystickButton6 | View/Back | Share | - (Minus) | Alt Menu Toggle |
| 7 | JoystickButton7 | Menu/Start | Options | + (Plus) | - |
| 8 | JoystickButton8 | L3 | L3 | L3 | Left Stick Click |
| 9 | JoystickButton9 | R3 | R3 | R3 | **Menu Toggle** |

---

## Configuration

Config file location: `BepInEx/config/org.xunfairx.cheat_menu.cfg`

```ini
[Controller]

## Enable controller/gamepad support
# Setting type: Boolean
# Default value: true
Enable Controller Support = true
```

### Custom Button Mappings (Advanced)

While the mod now uses Rewired directly (handles button mapping automatically), you can still customize keyboard fallbacks:

```ini
[Keybinds]
GuiKeybind = M
BackCategory = N
CloseGuiOnEscape = true
```

---

## How It Works (Technical)

### Rewired Integration (v1.1.0)
The mod now uses **RewiredInputHelper** to read input directly from the game's Rewired Player object:

1. **GetPlayer()** - Acquires Rewired Player 0
2. **GetNavigationVertical/Horizontal()** - Reads Right Stick (axis 2 & 3)
3. **GetToggleMenuPressed()** - Checks R3 (button 9) with suppression
4. **GetSelectPressed()** - Checks A/Cross (button 0) but blocks if R3 held
5. **GetBackPressed()** - Checks B/Circle (button 1)

### Bug Fixes Applied
- **R3 Suppression**: 300ms window after R3 press blocks in-game Bleat action
- **A Button Guard**: Select input ignored while R3 is held down
- **Navigation Delay**: 150ms delay between navigation inputs prevents scroll spam
- **Right Stick Only**: Uses axis 2/3 instead of D-Pad to avoid game control conflicts

---

## Troubleshooting

### Controller Not Detected
1. **Ensure controller connected** before launching game
2. **Check Rewired**: Game should recognize the controller
3. **Verify in-game**: Test if game itself responds to controller
4. **Check logs**: BepInEx console shows "[CheatMenu] Rewired player 0 acquired"

### Wrong Buttons Mapping
- Mod uses Rewired, so mappings should match the game automatically
- If issues persist, try reconfiguring controller in game settings
- Check if controller is Steam Input or native mode

### R3 Still Triggers Bahhh
- Ensure you're on version 1.1.0
- Check BepInEx logs for "[CheatMenu] PlayerFarming.Bleat successfully patched"
- If patch failed, mod still works but R3 may trigger bahhh

### Navigation Too Sensitive
- Built-in 150ms delay should prevent this
- Try smaller stick movements
- Navigation only triggers above 0.5 axis threshold

### Menu Opens But Can't Navigate
- Check if Right Stick is working (in-game camera should move)
- Try keyboard arrow keys as fallback
- Verify "Enable Controller Support = true" in config

---

## Testing Your Setup

1. **Launch game** with controller connected
2. **Enter gameplay** (not main menu)
3. **Press R3** - Menu should slide in from bottom-left
4. **Move Right Stick** - Red glow should highlight buttons
5. **Press A/Cross** - Should activate selected option
6. **Press B/Circle** - Should go back or close menu
7. **Check in-game** - Pressing R3 should NOT trigger bahhh/bleat

---

## Compatibility Notes

### What Works
- ? Xbox One, Xbox Series X|S controllers
- ? PlayStation DualShock 4
- ? PlayStation DualSense (PS5 controller)
- ? Nintendo Switch Pro Controller
- ? Steam Deck built-in controls
- ? Generic USB/Bluetooth gamepads
- ? Simultaneous keyboard + controller input

### Known Limitations
- D-Pad navigation removed (conflicts with game controls)
- Some generic controllers may require Steam Input
- Button 9 (R3) must exist on controller
- Analog stick required (digital-only controllers unsupported)

---

## Version History

| Version | Controller Support Level |
|---------|-------------------------|
| 1.1.0 | **Full Rewired integration** with R3 suppression, right stick nav |
| 1.0.5 | Basic controller support, D-Pad navigation |
| 1.0.4 | Keyboard only |

---

## Credits

- **Rewired Integration**: XUnfairX / firebirdjsb
- **Original Controller Support**: Community feedback
- **Bug Reports**: Thank you to all testers who reported the R3/bahhh issue!

---

**Current Version**: 1.1.0  
**Controller Support**: Full Rewired Integration  
**Tested**: Xbox, PlayStation, Switch Pro, Steam Deck, Generic USB controllers

*Need help? Report issues on [GitHub](https://github.com/firebirdjsb/cheat-menu-cotl/issues)*
