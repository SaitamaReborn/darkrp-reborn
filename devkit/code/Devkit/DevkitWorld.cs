using System;
using System.Linq;
using Sandbox;
using DarkRPReborn.Addons;

namespace DarkRPReborn.Devkit;

/// <summary>
/// Boots the Dev Kit session: mounts the on-screen console, simulates a small
/// server (two fake players joining) and exposes devkit_* console commands to
/// drive events at your addon exactly like the real gamemode would.
/// </summary>
public sealed class DevkitWorld : Component
{
	public static DevkitWorld Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;

		// On-screen console (same mounting pattern as the game's dynamic UIs).
		var ui = new GameObject( true, "DevkitConsoleUI" );
		ui.Parent = GameObject;
		ui.NetworkMode = NetworkMode.Never;
		ui.Components.Create<ScreenPanel>();
		ui.Components.Create<DevkitConsole>();

		DevkitConsole.Push( "DarkRP Reborn - Dev Kit" );
		DevkitConsole.Push( "Votre addon C# tourne ici contre un serveur simulé." );
		DevkitConsole.Push( "Commandes console : devkit_players, devkit_join <nom>, devkit_leave <nom>," );
		DevkitConsole.Push( "  devkit_say <texte> (ou devkit_say \"/cmd args\"), devkit_die <nom>, devkit_job <nom> <métier>" );
		DevkitConsole.Push( "csaddon_list liste les addons chargés." );

		// The loader on the same scene object has already bound the stub host by
		// the time OnStart order settles; simulate boot + two joins right after.
		Reborn.After( 0.5f, () =>
		{
			RebornAddonSystem.OnGameHook( "InitPostEntity", Array.Empty<object>() );
			SimulateJoin( "Alice" );
		} );
		Reborn.After( 1.5f, () => SimulateJoin( "Bob" ) );
	}

	protected override void OnDestroy()
	{
		if ( Instance == this ) Instance = null;
	}

	static RebornHostStub Stub => Reborn.Host as RebornHostStub;

	static void SimulateJoin( string name )
	{
		var stub = Stub;
		if ( stub == null ) return;
		var p = stub.AddFakePlayer( name );
		DevkitConsole.Push( $"[serveur] {name} a rejoint." );
		RebornAddonSystem.OnGameHook( "PlayerInitialSpawn", new object[] { p } );
		RebornAddonSystem.OnGameHook( "PlayerSpawn", new object[] { p } );
	}

	[ConCmd( "devkit_players" )]
	public static void CmdPlayers()
	{
		foreach ( var p in Reborn.Players )
			Log.Info( $"  {p.Name} | {p.Job} | {p.Money}$ | steamid {p.SteamId}" );
	}

	[ConCmd( "devkit_join" )]
	public static void CmdJoin( string name = "Charlie" ) => SimulateJoin( name );

	[ConCmd( "devkit_leave" )]
	public static void CmdLeave( string name = "Bob" )
	{
		var stub = Stub;
		var p = stub?.FindStub( name );
		if ( p == null ) { Log.Warning( $"[devkit] joueur inconnu : {name}" ); return; }
		RebornAddonSystem.OnGameHook( "PlayerDisconnected", new object[] { p } );
		stub.RemoveStub( p );
		DevkitConsole.Push( $"[serveur] {name} a quitté." );
	}

	/// <summary>Speaks as the first fake player. Text starting with "/" is
	/// routed through the addon chat-command registry, like real chat.</summary>
	[ConCmd( "devkit_say" )]
	public static void CmdSay( string text )
	{
		var speaker = Stub?.Players?.FirstOrDefault();
		if ( speaker == null || string.IsNullOrWhiteSpace( text ) ) return;

		if ( text.StartsWith( '/' ) )
		{
			var parts = text[1..].Split( ' ', StringSplitOptions.RemoveEmptyEntries );
			if ( parts.Length == 0 ) return;
			DevkitConsole.Push( $"[{speaker.Name}] {text}" );
			if ( !Reborn.TryExecuteChatCommand( parts[0], speaker, parts.Skip( 1 ).ToArray() ) )
				DevkitConsole.Push( $"[serveur] /{parts[0]} : aucune commande d'addon ne répond." );
			return;
		}

		DevkitConsole.Push( $"[{speaker.Name}] {text}" );
		RebornAddonSystem.OnGameHook( "PlayerSay", new object[] { speaker, text } );
	}

	[ConCmd( "devkit_die" )]
	public static void CmdDie( string name = "Alice" )
	{
		var p = Stub?.FindStub( name );
		if ( p == null ) { Log.Warning( $"[devkit] joueur inconnu : {name}" ); return; }
		DevkitConsole.Push( $"[serveur] {name} est mort." );
		RebornAddonSystem.OnGameHook( "PlayerDeath", new object[] { p } );
		RebornAddonSystem.OnGameHook( "PlayerSpawn", new object[] { p } );
	}

	[ConCmd( "devkit_job" )]
	public static void CmdJob( string name, string job )
	{
		var p = Stub?.FindStub( name );
		if ( p == null || string.IsNullOrWhiteSpace( job ) ) { Log.Warning( $"[devkit] usage: devkit_job <nom> <métier>" ); return; }
		var old = p.Job;
		p.Job = job;
		DevkitConsole.Push( $"[serveur] {name} : {old} -> {job}" );
		RebornAddonSystem.OnGameHook( "OnPlayerChangedTeam", new object[] { p, old, job } );
	}
}
