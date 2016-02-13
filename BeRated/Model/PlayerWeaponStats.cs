﻿using BeRated.Common;

namespace BeRated.Model
{
	public class PlayerWeaponStats
	{
		public string Weapon { get; set; }
		public int Kills { get; set; }
		public int Headshots { get; set; }

		public decimal HeadshotRatio { get { return Ratio.Get(Headshots, Kills).Value; } }

		private PlayerWeaponStats()
		{
		}

		public PlayerWeaponStats(string weapon)
		{
			Weapon = weapon;
			Kills = 0;
			Headshots = 0;
		}
	}
}
