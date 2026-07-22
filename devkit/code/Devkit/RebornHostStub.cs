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

	public override GameObject SpawnProp( string modelPath, Vector3 position, float yawDegrees, string addonIdent )
	{
		DevkitConsole.Push( $"[world] prop spawned: {modelPath} at {position} (simulated - no world in the Dev Kit)" );
		return null;
	}

	readonly List<string> _jobs = new();
	readonly List<string> _items = new();

	public override string RegisterJob( RebornJobSpec spec )
	{
		if ( spec == null || string.IsNullOrWhiteSpace( spec.Name ) ) return "spec.Name is required";
		if ( _jobs.Contains( spec.Name ) ) return $"job '{spec.Name}' already exists";
		_jobs.Add( spec.Name );
		DevkitConsole.Push( $"[jobs] registered: {spec.Name} (salary {spec.Salary}, max {spec.MaxWorkers})" );
		return null;
	}

	public override string RegisterItem( RebornItemSpec spec )
	{
		if ( spec == null || string.IsNullOrWhiteSpace( spec.Id ) ) return "spec.Id is required";
		if ( _items.Contains( spec.Id ) ) return $"item '{spec.Id}' already exists";
		_items.Add( spec.Id );
		DevkitConsole.Push( $"[items] registered: {spec.Id} ({spec.Name})" );
		return null;
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
		public string Name { get; set; } = "Player";
		public string SteamId { get; set; } = "0";
		public string Job { get; set; } = "Citizen";
		public long Money { get; set; } = 1500;
		public string Language { get; set; } = "en";
		public GameObject GameObject => null;

		public void GiveMoney( int amount, string reason = "" )
		{
			if ( amount <= 0 ) return;
			Money += amount;
			DevkitConsole.Push( $"[money] +{amount}$ for {Name}{(string.IsNullOrEmpty( reason ) ? "" : $" ({reason})")} -> {Money}$" );
		}

		public bool TakeMoney( int amount, string reason = "" )
		{
			if ( amount <= 0 || Money < amount ) return false;
			Money -= amount;
			DevkitConsole.Push( $"[money] -{amount}$ for {Name}{(string.IsNullOrEmpty( reason ) ? "" : $" ({reason})")} -> {Money}$" );
			return true;
		}

		public Vector3 Position { get; set; } = Vector3.Zero;
		public float Health { get; private set; } = 100f;

		public void Teleport( Vector3 position )
		{
			Position = position;
			DevkitConsole.Push( $"[world] {Name} teleported to {position}" );
		}

		public void SetHealth( float health )
		{
			Health = System.Math.Clamp( health, 0f, 100f );
			DevkitConsole.Push( $"[health] {Name} -> {Health}" );
			if ( Health <= 0f )
			{
				DevkitConsole.Push( $"[server] {Name} died." );
				RebornAddonSystem.OnGameHook( "PlayerDeath", new object[] { this } );
				Health = 100f;
				RebornAddonSystem.OnGameHook( "PlayerSpawn", new object[] { this } );
			}
		}

		public void SetMoney( long amount )
		{
			if ( amount < 0 ) return;
			Money = amount;
			DevkitConsole.Push( $"[money] {Name} set to {Money}$" );
		}

		public bool TrySetJob( string jobIdOrName )
		{
			if ( string.IsNullOrWhiteSpace( jobIdOrName ) ) return false;
			var old = Job;
			Job = jobIdOrName;
			DevkitConsole.Push( $"[jobs] {Name}: {old} -> {Job}" );
			RebornAddonSystem.OnGameHook( "OnPlayerChangedTeam", new object[] { this, old, Job } );
			return true;
		}

		public bool GiveItem( string itemId, int amount = 1 )
		{
			if ( string.IsNullOrWhiteSpace( itemId ) || amount <= 0 ) return false;
			DevkitConsole.Push( $"[items] {Name} received {amount}x {itemId}" );
			return true;
		}

		public void Notify( string title, string message, RebornNotifyType type = RebornNotifyType.Info )
			=> DevkitConsole.Push( $"[notif:{type} -> {Name}] {title} : {message}" );

		public void Chat( string message )
			=> DevkitConsole.Push( $"[chat -> {Name}] {message}" );
	}
}
