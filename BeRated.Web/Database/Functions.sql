set client_min_messages to warning;

create or replace function lock_tables() returns void as $$
begin
	lock player;
	lock kill;
	lock round;
	lock round_player;
	lock purchase;
end $$ language 'plpgsql';

create or replace function update_player(name text, steam_id text) returns integer as $$
declare
	player_id integer;
begin
	update player set name = update_player.name where player.steam_id = update_player.steam_id returning id into player_id;
	if not found then
		insert into player (name, steam_id) values (name, steam_id) returning id into player_id;
	end if;
	return player_id;
end $$ language 'plpgsql';

create or replace function get_team(team text) returns team_type as $$
begin
	if team = 'TERRORIST' then
		return 'terrorist'::team_type;
	elsif team = 'CT' then
		return 'counter_terrorist'::team_type;
	else
		raise exception 'Invalid team identifier: %s', team;
	end if;
end $$ language 'plpgsql';

create or replace function process_kill(kill_time timestamp, killer_name text, killer_steam_id text, killer_team text, killer_x integer, killer_y integer, killer_z integer, victim_name text, victim_steam_id text, victim_team text, victim_x integer, victim_y integer, victim_z integer, weapon text, headshot boolean) returns void as $$
declare
	killer_id integer;
	killer_team_enum team_type;
	victim_id integer;
	victim_team_enum team_type;
begin
	select update_player(killer_name, killer_steam_id) into killer_id;
	select update_player(victim_name, victim_steam_id) into victim_id;
	select get_team(killer_team) into killer_team_enum;
	select get_team(victim_team) into victim_team_enum;
	begin
		insert into kill
		(
			time,
			killer_id,
			killer_team,
			killer_vector,
			victim_id, victim_team,
			victim_vector,
			weapon,
			headshot
		)
		values
		(
			kill_time,
			killer_id,
			killer_team_enum,
			array[killer_x, killer_y, killer_z],
			victim_id,
			victim_team_enum,
			array[victim_x, victim_y, victim_z],
			weapon,
			headshot
		);
	exception when unique_violation then
	end;
end $$ language 'plpgsql';

create or replace function get_player_kills(player_id integer) returns int as $$
declare
	kills integer;
begin
	select count(*) from kill where killer_id = player_id and killer_id != victim_id into kills;
	return kills;
end $$ language 'plpgsql';

create or replace function get_player_deaths(player_id integer) returns int as $$
declare
	deaths integer;
begin
	select count(*) from kill where victim_id = player_id into deaths;
	return deaths;
end $$ language 'plpgsql';

create or replace function get_player_kill_death_ratio(player_id integer) returns double precision as $$
declare
	kills integer;
	deaths integer;
begin
	select get_player_kills(player_id) into kills;
	select get_player_deaths(player_id) into deaths;
	if deaths = 0 then
		return null;
	end if;
	return (kills::double precision) / deaths;
end $$ language 'plpgsql';

create or replace function get_player_stats() returns table
(
	id integer,
	steam_id text,
	name text,
	kills integer,
	deaths integer,
	kill_death_ratio double precision
) as $$ begin
	return query select
		player.id,
		player.steam_id,
		player.name,
		get_player_kills(player.id),
		get_player_deaths(player.id),
		get_player_kill_death_ratio(player.id)
	from player;
end $$ language 'plpgsql';