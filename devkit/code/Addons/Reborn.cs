using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRPReborn.Addons;

/// <summary>
/// The static API surface addons code against. Every call routes through the
/// bound <see cref="RebornHost"/>: the real game binds its systems, the Dev Kit
/// binds a simulator - addon code is identical in both. PUBLIC contract file,
/// mirrored verbatim in the community Dev Kit: no private game types in here.
/// </summary>
public static partial class Reborn
{
	/// <summary>Contract version. Bumped when the addon-facing API grows.</summary>
	public const int ApiVersion = 3;

	/// <summary>Bound by <see cref="RebornAddonSystem"/> at boot. Null until then.</summary>
	public static RebornHost Host { get; internal set; }

	/// <summary>Every connected player (simulated players in the Dev Kit).</summary>
	public static IEnumerable<IRebornPlayer> Players => Host?.Players ?? Enumerable.Empty<IRebornPlayer>();

	/// <summary>Find a player by (case-insensitive) name prefix or exact SteamID64.</summary>
	public static IRebornPlayer FindPlayer( string nameOrSteamId )
	{
		if ( string.IsNullOrWhiteSpace( nameOrSteamId ) ) return null;
		var all = Players.ToList();
		var exact = all.FirstOrDefault( p => p.SteamId == nameOrSteamId
			|| string.Equals( p.Name, nameOrSteamId, StringComparison.OrdinalIgnoreCase ) );
		if ( exact != null ) return exact;
		return all.FirstOrDefault( p => p.Name?.StartsWith( nameOrSteamId, StringComparison.OrdinalIgnoreCase ) == true );
	}

	/// <summary>System line in everyone's chat.</summary>
	public static void Broadcast( string message ) => Host?.Broadcast( message );

	/// <summary>Toast on everyone's screen.</summary>
	public static void BroadcastNotify( string title, string message, RebornNotifyType type = RebornNotifyType.Info )
	{
		foreach ( var p in Players ) p.Notify( title, message, type );
	}

	/// <summary>Prefixed server log ("[addon:ident] ...").</summary>
	/// <remarks>Routed through <see cref="RebornLogSink"/>: inside this class the
	/// method name shadows the engine's global Log object.</remarks>
	public static void Log( string addonIdent, string message )
		=> RebornLogSink.Info( $"[addon:{addonIdent}] {message}" );

	// ---- world ------------------------------------------------------------

	/// <summary>Spawn a physics prop in the world (host side). Returns the
	/// GameObject on the real server, null in the Dev Kit (no world) or when
	/// the model is missing. Pass your Ident so the prop is tracked per addon.</summary>
	public static GameObject SpawnProp( string modelPath, Vector3 position, float yawDegrees = 0f, string addonIdent = "" )
		=> Host?.SpawnProp( modelPath, position, yawDegrees, addonIdent );

	// ---- runtime content --------------------------------------------------

	/// <summary>Register a new job at runtime - it appears in the F4 menu on
	/// every client, like any built-in job. Returns null on success, else a
	/// human-readable error (duplicate name, missing field...).</summary>
	public static string RegisterJob( RebornJobSpec spec )
		=> Host == null ? "addon host not bound" : Host.RegisterJob( spec );

	/// <summary>Register a new inventory item at runtime - usable with
	/// IRebornPlayer.GiveItem. Returns null on success, else an error.</summary>
	public static string RegisterItem( RebornItemSpec spec )
		=> Host == null ? "addon host not bound" : Host.RegisterItem( spec );

	// ---- chat commands ----------------------------------------------------
	// Native gamemode commands always win, then Lua addons, then these - a C#
	// addon can never shadow a built-in command.

	sealed class ChatCommand
	{
		public string Name;
		public string Description;
		public string OwnerIdent;
		public Action<IRebornPlayer, string[]> Handler;
	}

	static readonly Dictionary<string, ChatCommand> _commands = new( StringComparer.OrdinalIgnoreCase );

	/// <summary>Register a /command. Name without the slash. Later registrations
	/// of the same name replace the earlier one (last addon loaded wins).</summary>
	public static void RegisterChatCommand( string name, string description, Action<IRebornPlayer, string[]> handler, string ownerIdent = "" )
	{
		if ( string.IsNullOrWhiteSpace( name ) || handler == null ) return;
		name = name.Trim().TrimStart( '/' );
		_commands[name] = new ChatCommand { Name = name, Description = description ?? "", OwnerIdent = ownerIdent, Handler = handler };
	}

	/// <summary>All registered addon commands (for /help style listings).</summary>
	public static IEnumerable<(string Name, string Description)> ChatCommands
		=> _commands.Values.Select( c => (c.Name, c.Description) );

	/// <summary>Called by the host's command router with its native player
	/// object. Returns true when an addon command handled it.</summary>
	public static bool TryExecuteChatCommand( string name, object nativePlayer, string[] args )
	{
		if ( Host == null || !_commands.TryGetValue( name ?? "", out var cmd ) ) return false;
		var player = Host.WrapNative( nativePlayer );
		if ( player == null ) return false;
		try { cmd.Handler( player, args ?? Array.Empty<string>() ); }
		catch ( Exception e ) { RebornLogSink.Warn( $"[addon:{cmd.OwnerIdent}] /{cmd.Name} threw: {e.Message}" ); }
		return true;
	}

	// ---- timers -----------------------------------------------------------
	// Pumped by RebornAddonSystem.OnFixedUpdate (host side).

	sealed class Timer
	{
		public float NextFire;
		public float Repeat;   // 0 = one-shot
		public Action Callback;
	}

	static readonly List<Timer> _timers = new();

	/// <summary>Run once after <paramref name="seconds"/>.</summary>
	public static void After( float seconds, Action callback )
	{
		if ( callback == null ) return;
		_timers.Add( new Timer { NextFire = Time.Now + MathF.Max( 0f, seconds ), Repeat = 0f, Callback = callback } );
	}

	/// <summary>Run every <paramref name="seconds"/> until server shutdown.</summary>
	public static void Every( float seconds, Action callback )
	{
		if ( callback == null ) return;
		seconds = MathF.Max( 0.05f, seconds );
		_timers.Add( new Timer { NextFire = Time.Now + seconds, Repeat = seconds, Callback = callback } );
	}

	internal static void PumpTimers()
	{
		for ( int i = _timers.Count - 1; i >= 0; i-- )
		{
			var t = _timers[i];
			if ( Time.Now < t.NextFire ) continue;
			try { t.Callback(); }
			catch ( Exception e ) { RebornLogSink.Warn( $"[addons] timer callback threw: {e.Message}" ); }
			if ( t.Repeat > 0f ) t.NextFire = Time.Now + t.Repeat;
			else _timers.RemoveAt( i );
		}
	}

	// ---- per-addon persistence -------------------------------------------
	// One JSON blob per addon under data/csaddons/. Survives restarts on the
	// server; lands in the project's data folder inside the Dev Kit.

	public static void SaveData( string addonIdent, string json )
	{
		try
		{
			FileSystem.Data.CreateDirectory( "csaddons" );
			FileSystem.Data.WriteAllText( $"csaddons/{Sanitize( addonIdent )}.json", json ?? "" );
		}
		catch ( Exception e ) { RebornLogSink.Warn( $"[addon:{addonIdent}] SaveData failed: {e.Message}" ); }
	}

	public static string LoadData( string addonIdent )
	{
		try
		{
			var path = $"csaddons/{Sanitize( addonIdent )}.json";
			return FileSystem.Data.FileExists( path ) ? FileSystem.Data.ReadAllText( path ) : null;
		}
		catch { return null; }
	}

	static string Sanitize( string ident )
	{
		ident = (ident ?? "addon").ToLowerInvariant();
		var chars = ident.Where( c => char.IsLetterOrDigit( c ) || c == '-' || c == '_' ).ToArray();
		return chars.Length > 0 ? new string( chars ) : "addon";
	}
}

/// <summary>Tiny indirection so code inside <see cref="Reborn"/> (whose Log
/// method shadows the engine's global logger) can still write server logs.</summary>
internal static class RebornLogSink
{
	public static void Info( string message ) => Log.Info( message );
	public static void Warn( string message ) => Log.Warning( message );
}

/// <summary>
/// Typed events addons subscribe to. Raised host-side by the bound
/// <see cref="RebornHost"/> from the same dispatch points as the Lua hook
/// system, so both addon worlds see the same game moments. Each handler is
/// isolated: one addon throwing never breaks the others.
/// </summary>
public static class RebornEvents
{
	/// <summary>Server booted, systems ready (GMod: InitPostEntity).</summary>
	public static event Action ServerReady;

	/// <summary>First spawn of a joining player (GMod: PlayerInitialSpawn).</summary>
	public static event Action<IRebornPlayer> PlayerJoined;

	/// <summary>Every (re)spawn, including after death (GMod: PlayerSpawn).</summary>
	public static event Action<IRebornPlayer> PlayerSpawned;

	/// <summary>Player left the server (GMod: PlayerDisconnected).</summary>
	public static event Action<IRebornPlayer> PlayerLeft;

	/// <summary>Player died (GMod: PlayerDeath).</summary>
	public static event Action<IRebornPlayer> PlayerDied;

	/// <summary>Public chat message, observe-only (GMod: PlayerSay).</summary>
	public static event Action<RebornChatEvent> PlayerChat;

	/// <summary>Job switch committed: (player, oldJobName, newJobName)
	/// (GMod: OnPlayerChangedTeam).</summary>
	public static event Action<IRebornPlayer, string, string> JobChanged;

	static void SafeRaise( Delegate ev, params object[] args )
	{
		if ( ev == null ) return;
		foreach ( var handler in ev.GetInvocationList() )
		{
			try { handler.DynamicInvoke( args ); }
			catch ( Exception e ) { Log.Warning( $"[addons] event handler threw: {e.InnerException?.Message ?? e.Message}" ); }
		}
	}

	internal static void RaiseServerReady() => SafeRaise( ServerReady );
	internal static void RaisePlayerJoined( IRebornPlayer p ) { if ( p != null ) SafeRaise( PlayerJoined, p ); }
	internal static void RaisePlayerSpawned( IRebornPlayer p ) { if ( p != null ) SafeRaise( PlayerSpawned, p ); }
	internal static void RaisePlayerLeft( IRebornPlayer p ) { if ( p != null ) SafeRaise( PlayerLeft, p ); }
	internal static void RaisePlayerDied( IRebornPlayer p ) { if ( p != null ) SafeRaise( PlayerDied, p ); }
	internal static void RaisePlayerChat( IRebornPlayer p, string text ) { if ( p != null ) SafeRaise( PlayerChat, new RebornChatEvent { Speaker = p, Text = text ?? "" } ); }
	internal static void RaiseJobChanged( IRebornPlayer p, string oldJob, string newJob ) { if ( p != null ) SafeRaise( JobChanged, p, oldJob ?? "", newJob ?? "" ); }
}

/// <summary>
/// The bridge between the addon API and whatever world it runs in. The real
/// game ships a high-priority implementation bound to its systems; the Dev Kit
/// ships a simulator. The loader picks the non-abstract subclass with the
/// highest <see cref="Priority"/>.
/// </summary>
public abstract class RebornHost : Component
{
	/// <summary>Higher wins when several implementations are compiled in.</summary>
	public abstract int Priority { get; }

	public abstract IEnumerable<IRebornPlayer> Players { get; }

	public abstract void Broadcast( string message );

	/// <summary>Wrap the host's native player object (game pawn, simulated
	/// player...) into the addon-facing handle. Null if not a player.</summary>
	public abstract IRebornPlayer WrapNative( object nativePlayer );

	/// <summary>Fed by the gamemode's central hook dispatch. Implementations
	/// translate native args and raise the typed <see cref="RebornEvents"/>.</summary>
	public abstract void OnGameHook( string eventName, object[] args );

	/// <summary>See <see cref="Reborn.SpawnProp"/>.</summary>
	public abstract GameObject SpawnProp( string modelPath, Vector3 position, float yawDegrees, string addonIdent );

	/// <summary>See <see cref="Reborn.RegisterJob"/>. Null = success.</summary>
	public abstract string RegisterJob( RebornJobSpec spec );

	/// <summary>See <see cref="Reborn.RegisterItem"/>. Null = success.</summary>
	public abstract string RegisterItem( RebornItemSpec spec );

	/// <summary>Transport for the addon UI framework: deliver a payload to ONE
	/// player's screen. Kinds: "menu", "menu_close", "hud", "hud_remove".</summary>
	public abstract void SendUi( IRebornPlayer target, string kind, string json );
}
