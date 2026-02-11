# ?? Controller Support Quick Reference

## Default Controller Layout

### Xbox Controller
```
        [LB]         [RB]
         ?             ?
    [LT] ?             ? [RT]
         ?             ?
         ?             ?
    
    [D-Pad]       [Y]
       ?          ?
     ?   ?    [X]   [B]  ? Go Back
       ?          ?
   Navigate    [A] ? Select
    
    [Back] ? Open/Close Menu
    [Start]
```

### PlayStation Controller
```
        [L1]         [R1]
         ?             ?
    [L2] ?             ? [R2]
         ?             ?
         ?             ?
    
    [D-Pad]       [?]
       ?          ?
     ?   ?    [?]   [?]  ? Go Back
       ?          ?
   Navigate    [?] ? Select
    
    [Select] ? Open/Close Menu
    [Start]
```

### Nintendo Switch Pro Controller
```
        [L]          [R]
         ?            ?
    [ZL] ?            ? [ZR]
         ?            ?
         ?            ?
    
    [D-Pad]      [X]
       ?          ?
     ?   ?    [Y]   [A]  ? Go Back
       ?          ?
   Navigate   [B] ? Select
    
    [-] ? Open/Close Menu
    [+]
```

## Button Mappings

| Action | Xbox | PlayStation | Switch | KeyCode |
|--------|------|-------------|--------|---------|
| **Open/Close Menu** | Back | Select | - (Minus) | JoystickButton6 |
| **Go Back** | B | Circle (?) | A | JoystickButton1 |
| **Select/Activate** | A | Cross (?) | B | JoystickButton0 |
| **Navigate Up** | D-Pad ? | D-Pad ? | D-Pad ? | Vertical Axis |
| **Navigate Down** | D-Pad ? | D-Pad ? | D-Pad ? | Vertical Axis |

## KeyCode Reference

For configuration in BepInEx config file:

| Button | KeyCode | Common Use |
|--------|---------|------------|
| Button 0 | `JoystickButton0` | A / Cross / B |
| Button 1 | `JoystickButton1` | B / Circle / A |
| Button 2 | `JoystickButton2` | X / Square / Y |
| Button 3 | `JoystickButton3` | Y / Triangle / X |
| Button 4 | `JoystickButton4` | LB / L1 / L |
| Button 5 | `JoystickButton5` | RB / R1 / R |
| Button 6 | `JoystickButton6` | Back / Select / - |
| Button 7 | `JoystickButton7` | Start / Start / + |
| Button 8 | `JoystickButton8` | L3 (stick press) |
| Button 9 | `JoystickButton9` | R3 (stick press) |

## Configuration

Edit `BepInEx/config/org.xunfairx.cheat_menu.cfg`:

```ini
[Controller]

## Enable controller/gamepad support for menu navigation
# Setting type: Boolean
# Default value: true
Enable Controller Support = true

## Controller button to open/close menu (default: Back/Select button)
# Setting type: KeyCode
# Default value: JoystickButton6
Menu Button = JoystickButton6

## Controller button to go back (default: B/Circle button)
# Setting type: KeyCode
# Default value: JoystickButton1
Back Button = JoystickButton1

## Controller button to select/activate (default: A/Cross button)
# Setting type: KeyCode
# Default value: JoystickButton0
Select Button = JoystickButton0
```

## Custom Configurations

### Using Triggers to Open Menu
```ini
Menu Button = JoystickButton4  # Use LB/L1
```

### Using Start Button
```ini
Menu Button = JoystickButton7  # Use Start
```

### Swap A and B
```ini
Select Button = JoystickButton1
Back Button = JoystickButton0
```

## Troubleshooting

### Controller Not Detected
1. Ensure controller is plugged in before launching game
2. Test controller works in other games
3. Try different USB port
4. Update controller drivers

### Wrong Buttons
1. Check if controller is recognized as Xbox-style or DirectInput
2. Test button numbers with a controller tester app
3. Manually configure KeyCodes in config file
4. Some controllers may have offset button numbers

### Navigation Too Fast
The mod includes a 0.15s delay between navigations. If still too fast:
1. Navigate more carefully with smaller stick movements
2. Or code can be modified to increase `s_navigationDelay`

### Controller and Keyboard Conflict
Both work simultaneously! This is intentional. To disable keyboard:
1. There's no config option to disable keyboard currently
2. You can simply use controller exclusively

## Steam Deck Specific

### Built-in Controls
All Steam Deck built-in controls work perfectly:
- **D-Pad**: Navigate menu
- **A Button**: Select
- **B Button**: Go Back
- **View Button (left)**: Open/Close menu

### Steam Input
If using Steam Input:
1. Set controller type to "Gamepad"
2. Or use default "Steam Deck" layout
3. Avoid keyboard+mouse emulation

## Testing Your Controller

1. Open cheat menu with configured button
2. Try navigating with D-Pad
3. Select an option with A/Cross/B
4. Go back with B/Circle/A
5. Close menu with Back/Select/-

If any don't work, note which button numbers ARE detected and update config accordingly.

## Generic Controller Tips

For non-standard controllers:
1. Download a button testing tool (like "Game Controller Tester")
2. Press each button and note the button number
3. Update config with correct KeyCodes
4. Button 0-9 usually work, higher numbers may not

## Future Features (Not Yet Implemented)

Planned improvements:
- Visual highlight showing selected button
- Analog stick support (currently D-Pad only)
- Shoulder button shortcuts
- Trigger sensitivity adjustment
- Controller-specific profiles

---

**Current Version**: 1.0.5  
**Controller Support**: Full  
**Tested Controllers**: Xbox One/Series, DualShock 4, DualSense, Switch Pro, Steam Deck  

?? Happy gaming with your controller! ?
