# ?? Loading Screen Freeze Fix Guide

**Problem**: Game hangs at loading screen after using "Give All Clothing Items" cheat in v1.1.2

**Affected Versions**: v1.1.2 only

**Fixed In**: v1.1.3

---

## Symptoms

- Game loads to main menu fine
- When loading a save, the game freezes at the loading screen with the Lamb animation
- BepInEx console shows:
  ```
  NullReferenceException: Object reference not set to an instance of an object
  Spine.Skin.AddSkin (Spine.Skin skin)
  FollowerBrain.SetFollowerCostume (...)
  ```
- Game appears to be running in background but is glitched/frozen

---

## Root Cause

In v1.1.2, the "Give All Clothing Items" cheat modified the `FollowerInfo.Outfit` and `FollowerInfo.Clothing` data for **all followers** in your save file, including those not currently loaded in the scene.

When the game tries to spawn these followers via `LocationManager.SpawnFollowers()`, it attempts to apply the custom outfit/clothing data before the follower's Spine skeleton is fully initialized, causing a null reference crash during the skin application.

---

## The Fix

### Step-by-Step Instructions

1. **Download and Install v1.1.3**
   - Replace your `CheatMenu.dll` in `BepInEx/plugins/CheatMenu/` with the v1.1.3 version
   - This version includes the emergency fix

2. **Start Cult of the Lamb**
   - The game should reach the main menu normally

3. **Load Your Affected Save**
   - Click "Continue" or select your save
   - The loading screen will appear and likely freeze

4. **Wait ~10 Seconds**
   - Even though the screen is frozen, the mod is still loading
   - Wait at the frozen loading screen

5. **Open the Cheat Menu**
   - Press **M** key (or **R3** on controller)
   - The cheat menu should appear over the frozen loading screen

6. **Navigate to Followers Category**
   - Select "Followers" from the main menu

7. **Run the Emergency Fix**
   - Click/select **"Reset All Follower Outfits"**
   - You should see a notification: "Reset X follower outfit(s) to default!"

8. **Save Your Game**
   - The game might still be partially frozen
   - Try pressing ESC to access the pause menu
   - If you can access it, manually save
   - If not, the auto-save should capture the fix

9. **Restart the Game**
   - Close Cult of the Lamb completely
   - Restart and load your save
   - It should now load normally!

---

## What Gets Reset

The emergency fix resets:
- **All follower outfits** ? Default outfit (or Old outfit for elderly)
- **All follower clothing** ? Naked/Default

### What Stays Intact

Everything else in your save is preserved:
- ? All unlocked clothing types
- ? Tailor building unlocked
- ? All other cheats/unlocks
- ? Cult progress
- ? Follower stats, levels, traits
- ? Structures and resources
- ? Story progression

You can re-apply clothing to followers manually at the tailor or use the cheat again (safely) while in your base.

---

## Prevention - Using Clothing Cheat Safely

In v1.1.3, the "Give All Clothing Items" cheat has been fixed to prevent this issue:

### Safe Usage
- **Use it at your Base** when followers are visible
- **Use it while in the game world** (not during loading)
- The cheat now **only affects followers currently loaded in the scene**
- Unloaded followers will keep their default outfits (safe)

### What's Different in v1.1.3
- Only followers physically present in the scene get clothing
- Notification shows actual count: "X follower(s) dressed"
- If no followers are in the scene: "No followers in scene to dress"
- Zero risk of corrupting save data

---

## Still Having Issues?

If the fix doesn't work:

1. **Check BepInEx Console**
   - Look for `[CheatMenu]` log messages
   - Report any new error messages

2. **Try Multiple Times**
   - Sometimes you need to run the fix, save, restart, and run it again

3. **Backup Your Save**
   - Before trying anything else, backup your save files
   - Location: `%APPDATA%\..\LocalLow\Massive Monster\Cult of the Lamb\`

4. **Report the Issue**
   - GitHub Issues: https://github.com/firebirdjsb/cheat-menu-cotl/issues
   - Include your BepInEx console log
   - Mention you tried the emergency fix

---

## Technical Details

### What the Fix Does

```csharp
// Iterates all follower collections
foreach(var follower in DataManager.Instance.Followers){
    follower.Outfit = FollowerOutfitType.Follower;
    follower.Clothing = FollowerClothingType.Naked;
}
foreach(var follower in DataManager.Instance.Followers_Dead){
    follower.Outfit = FollowerOutfitType.Follower;
    follower.Clothing = FollowerClothingType.Naked;
}
// Special case for elderly
foreach(var followerID in DataManager.Instance.Followers_Elderly_IDs){
    info.Outfit = FollowerOutfitType.Old;
}
```

This resets the persistent `FollowerInfo` data so that when followers spawn, they use default outfit logic instead of trying to apply custom clothing before their Spine skeleton is initialized.

---

## Version History

- **v1.1.2**: Bug introduced - clothing cheat affected all followers
- **v1.1.3**: Bug fixed - clothing cheat only affects loaded followers + emergency fix added

---

**This guide should get you back to playing within 5 minutes!**
