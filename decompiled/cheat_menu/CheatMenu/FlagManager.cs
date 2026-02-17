using System;
using System.Collections.Generic;

namespace CheatMenu
{
	public sealed class FlagManager
	{
		private FlagManager()
		{
		}

		[Init]
		[EnforceOrderFirst(10)]
		public void Init()
		{
			this._cheatFlags = new Dictionary<string, bool>();
		}

		public static void SetFlagValue(string flagID, bool value)
		{
			FlagManager.Instance._cheatFlags[flagID] = value;
		}

		public static bool IsFlagEnabledStr(string flagID)
		{
			bool flag;
			FlagManager.Instance._cheatFlags.TryGetValue(flagID, out flag);
			return flag;
		}

		public static bool IsFlagEnabled(string flagID)
		{
			bool flag;
			FlagManager.Instance._cheatFlags.TryGetValue(flagID, out flag);
			return flag;
		}

		public static void FlipFlagValue(string flagID)
		{
			bool flag;
			FlagManager.Instance._cheatFlags.TryGetValue(flagID, out flag);
			FlagManager.Instance._cheatFlags[flagID] = !flag;
		}

		public static FlagManager Instance { get; } = new FlagManager();

		private Dictionary<string, bool> _cheatFlags = new Dictionary<string, bool>();
	}
}
