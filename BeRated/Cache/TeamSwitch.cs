﻿using System;

namespace BeRated.Cache
{
	class TeamSwitch
	{
		public DateTime Time { get; set; }
		public PlayerIdentity Player { get; set; }
		public Team PreviousTeam { get; set; }
		public Team CurrentTeam { get; set; }
	}
}
