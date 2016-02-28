﻿namespace BeRated.Model
{
	public class GeneralPlayerStats : PlayerInfo
	{
		public decimal? KillsPerRound { get; set; }
		public decimal? KillDeathRatio { get; set; }

		public int Kills { get; set; }
		public int Deaths { get; set; }

		public int GamesPlayed { get; set; }
		public decimal? GameWinRatio { get; set; }

		public int RoundsPlayed { get; set; }
		public decimal? RoundWinRatio { get; set; }

		public double? StartKillRating { get; set; }
		public double EndKillRating { get; set; }
	}
}
