﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ashod;
using Ashod.Database;
using BeRated.Cache;
using BeRated.Model;
using BeRated.Server;
using Microsoft.Owin;

namespace BeRated.App
{
	public class RatingApp : BaseApp, IQueryPerformanceLogger
    {
        private Configuration _Configuration;
		private CacheManager _Cache;

		private Dictionary<string, CacheEntry> _WebCache = new Dictionary<string, CacheEntry>();

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
			_Cache = new CacheManager(_Configuration.LogDirectory, _Configuration.ConnectionString);
        }

		public override void Dispose()
		{
			_Cache.Dispose();
			base.Dispose();
		}

		public override void OnResponse(IOwinContext context, string markup)
		{
			_WebCache[context.Request.Uri.PathAndQuery] = new CacheEntry(markup);
			int maximumCacheSize = _Configuration.CacheSize.Value * 1024 * 1024;
			int cacheSize = 0;
			foreach (var pair in _WebCache)
				cacheSize += pair.Value.Markup.Length;
			var pairs = _WebCache.OrderBy(pair => pair.Value.Time).ToList();
			while (cacheSize > maximumCacheSize && pairs.Any())
			{
				var pair = pairs.First();
				pairs.RemoveAt(0);
				_WebCache.Remove(pair.Key);
				cacheSize -= pair.Value.Markup.Length;
			}
		}

        public void Initialize()
        {
            base.Initialize(_Configuration.ViewPath);
			_Cache.Run();
        }

        void IQueryPerformanceLogger.OnQueryEnd(string query, TimeSpan timeSpan)
        {
            string message = string.Format("Executed query in {0} ms: {1}", timeSpan.TotalMilliseconds, query);
            if (timeSpan.TotalMilliseconds < 250)
                Logger.Log(message);
            else if (timeSpan.TotalMilliseconds < 1000)
                Logger.Warning(message);
            else
                Logger.Error(message);
        }

        [Controller]
        public GeneralStats General()
        {
			var constraints = GetTimeConstraints();
			var stats = new GeneralStats
            {
                Players = GetGeneralPlayerStats(constraints),
                Teams = GetTeamStats(constraints),
            };
			return stats;
		}

        [Controller]
        public PlayerStats Player(int id)
        {
			var constraints = GetTimeConstraints();
            var playerStats = new PlayerStats();
            playerStats.Id = id;
            var idParameter = new CommandParameter("player_id", id);
            var startParameter = new CommandParameter("time_start", constraints.Start);
            var endParameter = new CommandParameter("time_end", constraints.End);
			/*
            playerStats.Name = connection.ScalarFunction<string>("get_player_name", idParameter);
			using (var reader = connection.ReadFunction("get_player_games", idParameter))
			{
				var games = reader.ReadAll<PlayerGame>();
				playerStats.Games = games.OrderByDescending(game => game.GameTime).ToList();
			}
			using (var reader = connection.ReadFunction("get_player_encounter_stats", idParameter, startParameter, endParameter))
			{
				var encounters = reader.ReadAll<PlayerEncounterStats>();
				playerStats.Encounters = encounters.OrderByDescending(player => player.Encounters).ToList();
			}
			using (var reader = connection.ReadFunction("get_player_weapon_stats", idParameter, startParameter, endParameter))
            {
                var weapons = reader.ReadAll<PlayerWeaponStats>();
				playerStats.Weapons = weapons.OrderByDescending(weapon => weapon.Kills).ToList();
            }
            using (var reader = connection.ReadFunction("get_player_purchases", idParameter, startParameter, endParameter))
            {
                var purchases = reader.ReadAll<PlayerItemStats>();
				playerStats.Purchases = purchases.OrderByDescending(item => item.TimesPurchased).ToList();
            }
			*/
            return playerStats;
        }

		[Controller]
		public TeamMatchupStats Matchup(string team1, string team2)
		{
            Func<string, List<PlayerInfo>> readPlayers = (playerIdString) =>
			{
				var playerIdParameter = new CommandParameter("player_id_string", playerIdString);
				/*
				using (var reader = connection.ReadFunction("get_player_names", playerIdParameter))
				{
					return reader.ReadAll<PlayerInfo>();
				}
				*/
				throw new NotImplementedException();
			};
			Func<bool, GameOutcomes> readOutcomes = (precise) =>
			{
				var teamParameter1 = new CommandParameter("player_id_string1", team1);
				var teamParameter2 = new CommandParameter("player_id_string2", team2);
				var preciseParameter = new CommandParameter("precise", precise);
				/*
				using (var reader = connection.ReadFunction("get_matchup_stats", teamParameter1, teamParameter2, preciseParameter))
				{
					return reader.ReadAll<GameOutcomes>().First();
				}
				*/
				throw new NotImplementedException();
			};
			var matchup = new TeamMatchupStats
			{
				Team1 = readPlayers(team1),
				Team2 = readPlayers(team2),
				ImpreciseOutcomes = readOutcomes(false),
				PreciseOutcomes = readOutcomes(true),
			};
			return matchup;
		}

		private TimeConstraints GetTimeConstraints()
		{
			int? days = GetDays();
			var timeConstraints = new TimeConstraints(days);
			return timeConstraints;
		}

		private int? GetDays()
		{
			int? days = 0;
			string daysString;
			if (Context.Current.Cookies.TryGetValue("days", out daysString))
			{
				if (daysString != string.Empty)
					days = int.Parse(daysString);
				else
					days = null;
			}
			return days;
		}

        private decimal? GetRatio(int numerator, int denominator)
        {
            if (denominator != 0)
                return (decimal)numerator / denominator;
            else
                return null;
        }

        private List<GeneralPlayerStats> GetGeneralPlayerStats(TimeConstraints constraints)
        {
            var stats = _Cache.Players.Select(player =>
            {
                var matchingKills = player.Kills.Where(kill => constraints.Match(kill.Time));
                var matchingDeaths = player.Deaths.Where(death => constraints.Match(death.Time));
                int kills = matchingKills.Count();
                int deaths = matchingDeaths.Count();

                var matchingWins = player.Wins.Where(game => constraints.Match(game.Time));
                var matchingGames = player.Games.Where(game => constraints.Match(game.Time));
                int wins = matchingWins.Count();
                int games = matchingGames.Count();

                var matchingRounds = player.Rounds.Where(round => constraints.Match(round.Time));
                var matchingRoundsWon = player.RoundsWon.Where(round => constraints.Match(round.Time));
                int roundsWon = matchingRoundsWon.Count();
                int roundsPlayed = matchingRounds.Count();

                return new GeneralPlayerStats
                {
                    SteamId = player.SteamId,
		            Name = player.Name,
		            KillDeathRatio = GetRatio(kills, deaths),
		            Kills = kills,
		            Deaths = deaths,
		            GamesPlayed = games,
		            GameWinRatio = GetRatio(wins, games),
		            RoundsPlayed = roundsPlayed,
		            RoundWinRatio = GetRatio(roundsWon, roundsPlayed),
                };
            }).ToList();
            return stats;
        }

        private List<TeamStats> GetTeamStats(TimeConstraints constraints)
        {
            var teams = new List<TeamStats>();
            var games = _Cache.Games.Where(game => constraints.Match(game.Time));
            foreach (var game in games)
            {
                AddGameToTeamStats(game.Terrorists, true, game.Outcome, teams);
                AddGameToTeamStats(game.CounterTerrorists, false, game.Outcome, teams);
            }
            return teams;
        }

        private bool IsSameTeam(List<PlayerInfo> team1, List<Player> team2)
        {
            if (team1.Count != team2.Count)
                return false;
            foreach (var player1 in team1)
            {
                if (!team2.Any(player2 => player1.SteamId == player2.SteamId))
                    return false;
            }
            return true;
        }

        private void AddGameToTeamStats(List<Player> players, bool isTerroristTeam, GameOutcome outcome, List<TeamStats> teams)
        {
            var team = teams.FirstOrDefault(teamStats => IsSameTeam(teamStats.Players, players));
            if (team == null)
            {
                var playerInfos = players.Select(player => new PlayerInfo(player.SteamId, player.Name)).ToList();
                team = new TeamStats(playerInfos);
                teams.Add(team);
            }
            if (outcome == GameOutcome.Draw)
                team.Draws++;
            else if (isTerroristTeam && outcome == GameOutcome.TerroristsWin)
                team.Wins++;
            else
                team.Losses++;
        }
    }
}
