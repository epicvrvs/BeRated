﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BeRated.Cache
{
	class Player
	{
		public string Name { get; private set; }

		public string SteamId { get; private set; }

		public List<Kill> Kills { get; set; }
        public List<Kill> Assists { get; set; }
		public List<Kill> Deaths { get; set; }

		public List<Round> RoundsWon { get; set; }
		public List<Round> RoundsLost { get; set; }

        public IEnumerable<Round> Rounds
        {
            get
            {
                return RoundsWon.Concat(RoundsLost).OrderBy(round => round.Time);
            }
        }

		public List<Game> Wins { get; set; }
		public List<Game> Losses { get; set; }
		public List<Game> Draws { get; set; }

        public IEnumerable<Game> Games
        {
            get
            {
                return Wins.Concat(Losses).Concat(Draws).OrderBy(game => game.Time);
            }
        }

		public List<Purchase> Purchases { get; private set; }

        private DateTime _NameTime;

		public Player(string name, string steamId, DateTime time)
		{
			Name = name;
			SteamId = steamId;

			Kills = new List<Kill>();
            Assists = new List<Kill>();
			Deaths = new List<Kill>();

			RoundsWon = new List<Round>();
			RoundsLost = new List<Round>();

			Wins = new List<Game>();
			Losses = new List<Game>();
			Draws = new List<Game>();

			Purchases = new List<Purchase>();

            _NameTime = time;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, SteamId);
		}

        public void SetName(string name, DateTime time)
        {
            if (time < _NameTime)
                return;
            Name = name;
            _NameTime = time;
        }
	}
}
