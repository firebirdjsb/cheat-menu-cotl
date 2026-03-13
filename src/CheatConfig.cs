using BepInEx.Configuration;
using UnityEngine;

namespace CheatMenu;

/// <summary>
/// Manages BepInEx configuration settings for the cheat menu.
/// Provides keybinds and options that players can customize.
/// </summary>
public class CheatConfig
{
    /// <summary>Keybind to open/close the cheat menu GUI.</summary>
    public ConfigEntry<KeyboardShortcut> GuiKeybind;

    /// <summary>Keybind to go back to previous category/menu.</summary>
    public ConfigEntry<KeyboardShortcut> BackCategory;

    /// <summary>Whether to close GUI when Escape is pressed.</summary>
    public ConfigEntry<bool> CloseGuiOnEscape;

    /// <summary>Enable controller/gamepad support for menu navigation.</summary>
    public ConfigEntry<bool> ControllerSupport;

    /// <summary>
    /// Gets the singleton instance of CheatConfig.
    /// </summary>
    public static CheatConfig Instance { get; private set; }

    /// <summary>
    /// Creates a new CheatConfig instance with default settings.
    /// </summary>
    /// <param name="config">The BepInEx ConfigFile to bind settings to.</param>
    public CheatConfig(ConfigFile config)
    {
        GuiKeybind = config.Bind(
            new ConfigDefinition("Keybinds", "GUIKey"),
            new KeyboardShortcut(KeyCode.M),
            new ConfigDescription("The key pressed to open and close the CheatMenu GUI")
        );

        BackCategory = config.Bind(
            new ConfigDefinition("Keybinds", "Back Category"),
            new KeyboardShortcut(KeyCode.N),
            new ConfigDescription("The key pressed to go back to the previous category/menu")
        );

        CloseGuiOnEscape = config.Bind(
            new ConfigDefinition("Options", "Close GUI on escape"),
            true,
            new ConfigDescription("Disable/Enable closing the cheat menu GUI when escape is pressed")
        );

        ControllerSupport = config.Bind(
            new ConfigDefinition("Controller", "Enable Controller Support"),
            true,
            new ConfigDescription("Enable controller/gamepad support for menu navigation. Uses the game's detected controller via Rewired. R3=Open/Close, A=Select, B=Back, Stick/D-Pad=Navigate.")
        );

        Instance = this;
    }
}
