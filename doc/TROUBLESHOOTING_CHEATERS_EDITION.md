# Troubleshooting: Cheaters Edition Features

This guide helps troubleshoot if the "Cheaters Edition" label or DLC authentication isn't working.

---

## Issue #1: "Cheaters Edition ??" Not Showing in Main Menu

### Expected Behavior
When CheatMenu is loaded, the version text in the bottom-left corner of the main menu should show:
```
Version 1.5.16.1000 - Cheaters Edition ??
```

### Common Causes

#### 1. Patch Applied Too Late
The version text is set when `VersionNumber.OnEnable()` is called. If the main menu loads before our patch is applied, you won't see the text.

**Solution**: Restart the game. The patch should apply on the next launch.

#### 2. VersionNumber Method Signature Changed
If the game updated and changed the `VersionNumber` class, our patch might not find the method.

**Check BepInEx Console**:
```
[CheatMenu] ? VersionNumber.OnEnable successfully patched
```

If you see:
```
[CheatMenu] VersionNumber patch failed: [error]
```

The game's `VersionNumber` class may have changed.

#### 3. Text Component Field Name Changed
Our patch looks for a field called `Text` in the `VersionNumber` class.

**Check BepInEx Console**:
```
[CheatMenu] Prefix_VersionNumber_OnEnable called!
[CheatMenu] ? Version text set to: 1.5.16.1000 - Cheaters Edition ??
```

If you see:
```
[CheatMenu] Text component was null!
```

The field name may have changed in a game update.

### How to Verify Patch Applied

1. **Launch the game**
2. **Check BepInEx console** (BepInEx/LogOutput.log)
3. **Look for these messages**:
   ```
   [CheatMenu] ?? Welcome to Cult of the Lamb: Cheaters Edition! ??
   [CheatMenu] ? VersionNumber.OnEnable successfully patched
   ```
4. **When you reach main menu**, look for:
   ```
   [CheatMenu] Prefix_VersionNumber_OnEnable called!
   [CheatMenu] ? Version text set to: 1.5.16.1000 - Cheaters Edition ??
   ```

If you see "called!" but no "set to", the Text component couldn't be found.

### Manual Verification

Even if the text doesn't show, the mod is still working! To verify:

1. Open cheat menu with **M** key
2. All cheats should work normally
3. DLC content should be accessible (see below)

---

## Issue #2: DLC Authentication Not Working

### Expected Behavior
When CheatMenu loads and DataManager is ready, you should see:
```
[CheatMenu] DataManager ready! Authenticating all DLC packs...
[CheatMenu] ? Base DLC packs authenticated!
[CheatMenu] ? Woolhaven (MAJOR_DLC) authenticated!
[CheatMenu] ========================================
[CheatMenu] ?? CHEATERS EDITION FULLY ACTIVATED! ??
[CheatMenu] ========================================
```

### Common Causes

#### 1. DataManager Not Ready Yet
DLC authentication happens in the Update loop when `DataManager.Instance != null`.

**Solution**: Wait a few seconds after game launch. Check console logs.

#### 2. DataManager Fields Changed
If Massive Monster renamed the DLC fields in a game update, our code won't find them.

**Check BepInEx Console** for:
```
[CheatMenu] Failed to authenticate DLC packs: [error message]
```

The error message will tell you what went wrong.

### How to Verify DLC Authentication

#### Method 1: Check Console Logs
Look for the "FULLY ACTIVATED" message in BepInEx/LogOutput.log

#### Method 2: Test DLC Content
1. Open cheat menu (M key)
2. Go to **Cult** ? **Unlock All Clothing**
3. If no errors appear, DLC is authenticated
4. Try **Give All Clothing Items** cheat
5. Followers should get DLC clothing without errors

#### Method 3: Check DataManager Directly
If you have a console mod or debug tool:
```csharp
DataManager.Instance.DLC_Cultist_Pack  // Should be true
DataManager.Instance.DLC_Sinful_Pack   // Should be true
DataManager.Instance.DLC_Pilgrim_Pack  // Should be true
DataManager.Instance.DLC_Heretic_Pack  // Should be true
DataManager.Instance.MAJOR_DLC         // Should be true (Woolhaven)
```

### What If Authentication Fails?

Even if authentication fails, most cheats will still work. You may encounter errors when using:
- **Give All Clothing Items** (might skip DLC clothing)
- **Spawn Follower** (might revert DLC skins to defaults)
- **Unlock All Structures** (might skip DLC decorations)

The errors will be logged but won't crash the game.

---

## Issue #3: Timing Issues

### Symptom
Console shows:
```
[CheatMenu] Failed to authenticate DLC packs: Object reference not set to an instance of an object
```

### Cause
`DataManager.Instance` was null when authentication attempted.

### Solution
This is normal on first frame! The mod will keep trying every Update until DataManager exists.

**Wait ~5 seconds** and check console again. You should see the success message.

---

## Getting Help

If issues persist:

1. **Check BepInEx/LogOutput.log**
2. **Copy the CheatMenu log lines** (everything starting with `[CheatMenu]`)
3. **Report on GitHub Issues**: https://github.com/firebirdjsb/cheat-menu-cotl/issues
4. **Include**:
   - Game version
   - CheatMenu version
   - Log excerpt showing the error
   - What you were doing when it failed

---

## Known Limitations

### Version Text Display
- Only updates when `VersionNumber.OnEnable()` is called
- If main menu was already open, restart game to see it
- Does NOT affect the game version in Settings menu

### DLC Authentication
- Only affects cheat functionality
- Does NOT give you actual DLC content you don't own
- Only prevents errors when using cheats that reference DLC
- **Woolhaven DLC**: You still need to own it to access Woolhaven areas in-game

---

## Success Indicators

### ? Everything Working
**Console output**:
```
[CheatMenu] ?? Welcome to Cult of the Lamb: Cheaters Edition! ??
[CheatMenu] ? VersionNumber.OnEnable successfully patched
[CheatMenu] DataManager ready! Authenticating all DLC packs...
[CheatMenu] ? Base DLC packs authenticated!
[CheatMenu] ? Woolhaven (MAJOR_DLC) authenticated!
[CheatMenu] ========================================
[CheatMenu] ?? CHEATERS EDITION FULLY ACTIVATED! ??
[CheatMenu] ========================================
```

**Main menu**: Shows "Cheaters Edition ??" in version text

**In-game**: All cheats work without DLC-related errors

---

## Quick Fix Checklist

- [ ] BepInEx 5.4.21+ installed correctly
- [ ] CheatMenu.dll in BepInEx/plugins/CheatMenu/
- [ ] Game restarted after installing mod
- [ ] Checked BepInEx/LogOutput.log for errors
- [ ] Waited ~5 seconds after game launch
- [ ] Main menu fully loaded before checking version text
- [ ] No conflicting mods installed

If all checked and still not working, report the issue with logs!
