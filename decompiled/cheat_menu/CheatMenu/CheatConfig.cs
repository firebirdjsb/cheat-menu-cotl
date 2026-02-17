using System;
using BepInEx.Configuration;
using UnityEngine;

namespace CheatMenu
{
	public class CheatConfig
	{
		public CheatConfig(ConfigFile config)
		{
			this.GuiKeybind = config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "GUIKey"), new KeyboardShortcut(KeyCode.M, Array.Empty<KeyCode>()), new ConfigDescription("The key pressed to open and close the CheatMenu GUI", null, Array.Empty<object>()));
			this.BackCategory = config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Back Category"), new KeyboardShortcut(KeyCode.N, Array.Empty<KeyCode>()), new ConfigDescription("The key pressed to go back to the previous category/menu", null, Array.Empty<object>()));
			this.CloseGuiOnEscape = config.Bind<bool>(new ConfigDefinition("Options", "Close GUI on escape"), true, new ConfigDescription("Disable/Enable closing the cheat menu GUI when escape is pressed", null, Array.Empty<object>()));
			this.ControllerSupport = config.Bind<bool>(new ConfigDefinition("Controller", "Enable Controller Support"), true, new ConfigDescription("Enable controller/gamepad support for menu navigation. Uses the game's detected controller via Rewired. R3=Open/Close, A=Select, B=Back, Stick/D-Pad=Navigate.", null, Array.Empty<object>()));
			CheatConfig.Instance = this;
		}

		public static CheatConfig Instance { get; set; }

		public ConfigEntry<KeyboardShortcut> GuiKeybind;

		public ConfigEntry<KeyboardShortcut> BackCategory;

		public ConfigEntry<bool> CloseGuiOnEscape;

		public ConfigEntry<bool> ControllerSupport;
	}
}
