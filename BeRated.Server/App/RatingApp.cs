﻿using Ashod;
using Ashod.Database;
using BeRated.Model;
using BeRated.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeRated.App
{
	public class RatingApp : BaseApp, IQueryPerformanceLogger
    {
        private Configuration _Configuration;

        public RatingApp(Configuration configuration)
        {
            _Configuration = configuration;
        }

        public void Initialize()
        {
            base.Initialize(_Configuration.ViewPath);
        }

        void IQueryPerformanceLogger.OnQueryEnd(string commandText, TimeSpan timeSpan)
        {
            Logger.Log("Query: {0}\nTime elapsed: {1} ms", commandText, timeSpan.TotalMilliseconds);
        }

        [Controller]
        public GeneralStats General(int? days)
        {
            var watch = new PerformanceWatch();
            using (var connection = GetConnection())
            {
                watch.Print("Controller/General/GetConnection");
			    var constraints = new TimeConstraints(days);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
			    var stats = new GeneralStats
			    {
				    Days = days,
			    };
                using (var reader = connection.ReadFunction("get_all_player_stats", startParameter, endParameter))
                {
				    stats.Players = reader.ReadAll<GeneralPlayerStats>();
                }
                watch.Print("Controller/General/get_all_player_stats");
			    using (var reader = connection.ReadFunction("get_teams", startParameter, endParameter))
			    {
				    stats.Teams = reader.ReadAll<TeamStats>();
                }
                watch.Print("Controller/get_teams");
			    return stats;
            }
		}

        [Controller]
        public PlayerStats Player(int id, int? days)
        {
            var watch = new PerformanceWatch();
            using (var connection = GetConnection())
            {
                var constraints = new TimeConstraints(days);
                var playerStats = new PlayerStats();
                playerStats.Id = id;
                var idParameter = new CommandParameter("player_id", id);
                var startParameter = new CommandParameter("time_start", constraints.Start);
                var endParameter = new CommandParameter("time_end", constraints.End);
                playerStats.Name = connection.ScalarFunction<string>("get_player_name", idParameter);
			    playerStats.Days = days;
			    using (var reader = connection.ReadFunction("get_player_games", idParameter, startParameter, endParameter))
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
                watch.Print("Controller/Player");
                return playerStats;
            }
        }

		[Controller]
		public TeamMatchupStats Matchup(string team1, string team2)
		{
            var watch = new PerformanceWatch();
            using (var connection = GetConnection())
            {
                Func<string, List<PlayerInfo>> readPlayers = (playerIdString) =>
			    {
				    var playerIdParameter = new CommandParameter("player_id_string", playerIdString);
				    using (var reader = connection.ReadFunction("get_player_names", playerIdParameter))
				    {
					    return reader.ReadAll<PlayerInfo>();
				    }
			    };
			    Func<bool, GameOutcomes> readOutcomes = (precise) =>
			    {
				    var teamParameter1 = new CommandParameter("player_id_string1", team1);
				    var teamParameter2 = new CommandParameter("player_id_string2", team2);
				    var preciseParameter = new CommandParameter("precise", precise);
				    using (var reader = connection.ReadFunction("get_matchup_stats", teamParameter1, teamParameter2, preciseParameter))
				    {
					    return reader.ReadAll<GameOutcomes>().First();
				    }
			    };
			    var matchup = new TeamMatchupStats
			    {
				    Team1 = readPlayers(team1),
				    Team2 = readPlayers(team2),
				    ImpreciseOutcomes = readOutcomes(false),
				    PreciseOutcomes = readOutcomes(true),
			    };
                watch.Print("Controller/Player");
			    return matchup;
            }
		}

        private DatabaseConnection GetConnection()
        {
            var sqlConnection = new NpgsqlConnection(_Configuration.ConnectionString);
            var connection = new DatabaseConnection(sqlConnection, this);
            return connection;
        }
    }
}
