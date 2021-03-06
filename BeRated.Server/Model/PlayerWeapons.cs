﻿using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerWeapons : PlayerInfo
	{
		public List<PlayerWeaponStats> Weapons { get; set; }

        public PlayerWeapons(string name, string steamId, List<PlayerWeaponStats> weapons)
            : base(name, steamId)
        {
            Weapons = weapons;
        }
	}
}
