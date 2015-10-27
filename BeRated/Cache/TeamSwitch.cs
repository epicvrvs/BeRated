﻿using System;

namespace BeRated.Cache
{
	class TeamSwitch
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Player { get; set; }
		public string PreviousTeam { get; set; }
		public string CurrentTeam { get; set; }
	}
}