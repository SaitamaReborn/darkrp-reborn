using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using DarkRPReborn.Addons;

namespace DarkRPReborn.Devkit;

/// <summary>
/// Dev Kit binding of the addon API: a small city simulator. Fake players with
/// money and jobs, chat routed to the on-screen console. On the real server the
/// exact same addon code talks to real pawns instead - this class is replaced
/// by the game's own high-priority host, nothing else changes.
/// </summary>
public sealed class RebornHostStub : RebornHost
{
	public override int Priority => 0;

	readonly List<StubPlayer> _players = new();

	public override IEnumerable<IRebornPlayer> Players => _players;

	public override void Broadcast( string message )
		=> DevkitConsole.Push( $"[chat] {message}" );

	public override IRebornPlayer WrapNative( object nativePlayer )
		=> nativePlayer as IRebornPlayer ?? _players.FirstOrDefault();

	public override void OnGameHook( string eventName, object[] args )
	{
		var p = args != null && args.Length > 0 ? WrapNative( args[0] ) : null;
		switch ( eventName )
		{
			case "InitPostEntity": RebornEvents.RaiseServerReady(); break;
			case "PlayerInitialSpawn": RebornEvents.RaisePlayerJoined( p ); break;
			case "PlayerSpawn": RebornEvents.RaisePlayerSpawned( p ); break;
			case "PlayerDisconnected": RebornEvents.RaisePlayerLeft( p ); break;
			case "PlayerDeath": RebornEvents.RaisePlayerDied( p ); break;
			case "PlayerSay": RebornEvents.RaisePlayerChat( p, args?.Length > 1 ? args[1] as string : "" ); break;
			case "OnPlayerChangedTeam":
				RebornEvents.RaiseJobChanged( p,
					args?.Length > 1 ? args[1] as string : "",
					args?.Length > 2 ? args[2] as string : "" );
				break;
		}
	}

	public StubPlayer AddFakePlayer( string name )
	{
		var p = new StubPlayer { Name = name, SteamId = (76560000000000000L + _players.Count + 1).ToString() };
		_players.Add( p );
		return p;
	}

	public StubPlayer FindStub( string name )
		=> _players.FirstOrDefault( p => string.Equals( p.Name, name, StringComparison.OrdinalIgnoreCase ) );

	public bool RemoveStub( StubPlayer p ) => _players.Remove( p );

	public sealed class StubPlayer : IRebornPlayer
	{
		public string Name { get; set; } = "Joueur";
		public string SteamId { get; set; } = "0";
		public string Job { get; set; } = "Citoyen";
		public long Money { get; set; } = 1500;
		public string Language { get; set; } = "fr";
		public GameObject GameObject => null;

		public void GiveMoney( int amount, string reason = "" )
		{
			if ( amount <= 0 ) return;
			Money += amount;
			DevkitConsole.Push( $"[money] +{amount}$ pour {Name}{(string.IsNullOrEmpty( reason ) ? "" : $" ({reason})")} -> {Money}$" );
		}

		public bool TakeMoney( int amount, string reason = "" )
		{
			if ( amount <= 0 || Money < amount ) return false;
			Money -= amount;
			DevkitConsole.Push( $"[money] -{amount}$ pour {Name}{(string.IsNullOrEmpty( reason ) ? "" : $" ({reason})")} -> {Money}$" );
			return true;
		}

		public void Notify( string title, string message, RebornNotifyType type = RebornNotifyType.Info )
			=> DevkitConsole.Push( $"[notif:{type} -> {Name}] {title} : {message}" );

		public void Chat( string message )
			=> DevkitConsole.Push( $"[chat -> {Name}] {message}" );
	}
}
