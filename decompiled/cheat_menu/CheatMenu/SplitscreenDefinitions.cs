using System;

namespace CheatMenu
{
	[CheatCategory(CheatCategoryEnum.SPLITSCREEN)]
	public class SplitscreenDefinitions : IDefinition
	{
		private static PlayerFarming GetPlayer2()
		{
			if (PlayerFarming.players.Count > 1)
			{
				return PlayerFarming.players[1];
			}
			return null;
		}

		[CheatDetails("P2: Heal x1", "Heals a Red Heart of Player 2", false, 0)]
		public static void P2HealRed()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.Heal(2f);
				CultUtils.PlayNotification("P2: Healed 1 red heart!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Full Heal", "Fully heals Player 2 to max HP", false, 0)]
		public static void P2FullHeal()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.Heal(999f);
				CultUtils.PlayNotification("P2: Fully healed!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Add x1 Red Heart", "Permanently adds a Red Heart container to Player 2", false, 0)]
		public static void P2AddRedHeart()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.totalHP += 2f;
				player.health.Heal(2f);
				CultUtils.PlayNotification("P2: Red heart added!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Add x1 Blue Heart", "Adds a Blue Heart to Player 2", false, 0)]
		public static void P2AddBlueHeart()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.BlueHearts += 2f;
				CultUtils.PlayNotification("P2: Blue heart added!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Add x1 Black Heart", "Adds a Black Heart to Player 2", false, 0)]
		public static void P2AddBlackHeart()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.BlackHearts += 2f;
				CultUtils.PlayNotification("P2: Black heart added!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Add x1 Spirit Heart", "Adds a Spirit Heart to Player 2", false, 0)]
		public static void P2AddSpiritHeart()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.TotalSpiritHearts += 2f;
				CultUtils.PlayNotification("P2: Spirit heart added!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Godmode", "P2: Godmode (OFF)", "P2: Godmode (ON)", "Full invincibility for Player 2", true, 0)]
		public static void P2GodMode(bool flag)
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.GodMode = (flag ? Health.CheatMode.God : Health.CheatMode.None);
				CultUtils.PlayNotification(flag ? "P2: Godmode ON!" : "P2: Godmode OFF!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}

		[CheatDetails("P2: Die", "Kills Player 2", false, 0)]
		public static void P2Die()
		{
			PlayerFarming player = SplitscreenDefinitions.GetPlayer2();
			if (player != null)
			{
				player.health.DealDamage(9999f, player.gameObject, player.transform.position, false, Health.AttackTypes.Melee, false, (Health.AttackFlags)0);
				CultUtils.PlayNotification("P2: You died!");
				return;
			}
			CultUtils.PlayNotification("Player 2 not found! Start co-op first.");
		}
	}
}
