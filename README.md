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

## Installation
- Make sure you have BepInEx installed ([Guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html))
- Download the zip from the releases tab
- Copy the CheatMenu folder to BepInEx/Plugins folder
- **Important**: Some COTL mods require that the BepInEx configuration has a diferent entrypoint. Download this [file]() and place it in BepInEx/config
- Start the game and enjoy the mod!

### Dependencies
[BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21)

### Developer Dependencies  
- [Gaze](https://github.com/wtetsu/gaze)  
    - Used for file watching to run the build when changes occur.
- [dotnet-script](https://github.com/filipw/dotnet-script)
    - Used to run the readme generator
- [pdb2mdb](https://gist.github.com/jbevain/ba23149da8369e4a966f)
    - Used to create visual studio debugging files

### Developer Setup
- Ensure dotnet is in your ```PATH```
- Install [Gaze](https://github.com/wtetsu/gaze) and put it in your ```PATH```
- Download [pdb2mdb](https://gist.github.com/jbevain/ba23149da8369e4a966f) and put it in the ```tools``` directory (even if you don't need the visual studio debugging symbols).
- Set environment variable ```CULT_OF_THE_LAMB_PATH``` to the root directory of the game
- Run either ```./build.bat``` or ```./watch.bat``` under scripts 

### License / Credits
Originally created by Wicked7000. Now maintained by **XUnfairX**.

Feel free to look around the code and modify for personal use. If you want to release a version of your code, please open an issue or pull request!

If you just want to add a specific 'Cheat' to the mod, feel free to open a pull request or open an issue.

### Ethics
Cheats that unlock DLC content or content that is intended to be locked will not be added to this mod.
