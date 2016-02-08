﻿using System.Collections.Generic;

namespace BeRated.Model
{
	public class TeamStats
	{
		public List<PlayerInfo> Players { get; private set; }

		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Draws { get; set; }

        public int Games
        {
            get
            {
                return Wins + Losses + Draws;
            }
        }

		public decimal? WinRatio
        {
            get
            {
                int games = Games;
                if (games > 0)
                    return (decimal)Wins / games;
                else
                    return null;
            }
        }

        public TeamStats(List<PlayerInfo> players)
        {
            Players = players;

            Wins = 0;
            Losses = 0;
            Draws = 0;
        }
	}
}
