using System;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.COMPANION)]
	public class CompanionDefinitions : IDefinition
	{
		[CheatDetails("Spawn Friendly Wolf", "Spawns a tame wolf that follows you (limit 1)", false, 0)]
		public static void SpawnFriendlyWolf()
		{
			CultUtils.SpawnFriendlyWolf();
		}

		[CheatDetails("Dismiss Wolf", "Dismisses your friendly wolf or clears all spawned wolves", false, 0)]
		public static void DismissFriendlyWolf()
		{
			CultUtils.DismissFriendlyWolf();
		}

		[CheatDetails("Pet Wolf", "Pet your friendly wolf!", false, 0)]
		public static void PetFriendlyWolf()
		{
			CultUtils.PetFriendlyWolf();
		}

		[CheatDetails("Wolf Dungeon Combat", "Combat (OFF)", "Combat (ON)", "Wolf attacks enemies in dungeons", true, 0)]
		public static void ToggleWolfDungeonCombat(bool flag)
		{
			CultUtils.WolfDungeonCombat = flag;
			CultUtils.PlayNotification(flag ? "Wolf dungeon combat ON!" : "Wolf dungeon combat OFF!");
		}
	}
}
