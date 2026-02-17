using System;

namespace CheatMenu
{
	public enum CheatCategoryEnum
	{
		[StringEnum("NONE")]
		NONE,
		[StringEnum("Health")]
		HEALTH,
		[StringEnum("Combat")]
		COMBAT,
		[StringEnum("Resources")]
		RESOURCE,
		[StringEnum("Cult")]
		CULT,
		[StringEnum("Follower")]
		FOLLOWER,
		[StringEnum("Farming")]
		FARMING,
		[StringEnum("Companion")]
		COMPANION,
		[StringEnum("Weather")]
		WEATHER,
		[StringEnum("DLC")]
		DLC,
		[StringEnum("Splitscreen")]
		SPLITSCREEN,
		[StringEnum("Misc")]
		MISC,
		[StringEnum("Animation")]
		ANIMATION
	}
}
