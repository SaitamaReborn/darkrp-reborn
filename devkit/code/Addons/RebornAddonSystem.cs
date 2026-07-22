using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace DarkRPReborn.Addons;

/// <summary>
/// Loader for C# addons. Host-only. At boot it (1) binds the highest-priority
/// <see cref="RebornHost"/> implementation compiled into the project, then
/// (2) spawns every non-abstract <see cref="RebornAddon"/> subclass on its own
/// child GameObject - one addon, one auto-discovered component service.
/// PUBLIC contract file, mirrored verbatim in the community Dev Kit.
/// </summary>
public sealed class RebornAddonSystem : Component
{
	public static RebornAddonSystem Instance { get; private set; }

	readonly List<RebornAddon> _loaded = new();

	/// <summary>Loaded addons, boot order (sorted by Ident).</summary>
	public IReadOnlyList<RebornAddon> Loaded => _loaded;

	protected override void OnStart()
	{
		// Addon logic is authoritative: it runs only where the game state is
		// owned. In the Dev Kit's single-player editor session that is also
		// true (the local session is the host).
		if ( !Networking.IsHost )
		{
			Enabled = false;
			return;
		}

		Instance = this;
		BindHost();
		SpawnAddons();
	}

	void BindHost()
	{
		if ( Reborn.Host != null && Reborn.Host.IsValid() ) return;

		var hostTypes = Game.TypeLibrary.GetTypes<RebornHost>()
			.Where( t => !t.IsAbstract )
			.ToList();
		if ( hostTypes.Count == 0 )
		{
			Log.Warning( "[csaddons] no RebornHost implementation compiled in - addons disabled." );
			Enabled = false;
			return;
		}

		// Highest Priority wins: the real game binding (100) beats the Dev Kit
		// simulator (0) if both are ever compiled together.
		TypeDescription best = null;
		int bestPriority = int.MinValue;
		foreach ( var td in hostTypes )
		{
			var probe = Components.Create( td, false ) as RebornHost;
			if ( probe == null ) continue;
			if ( probe.Priority > bestPriority )
			{
				if ( Reborn.Host != null ) Reborn.Host.Destroy();
				bestPriority = probe.Priority;
				best = td;
				Reborn.Host = probe;
			}
			else
			{
				probe.Destroy();
			}
		}

		if ( Reborn.Host == null )
		{
			Log.Warning( "[csaddons] RebornHost creation failed - addons disabled." );
			Enabled = false;
			return;
		}

		Reborn.Host.Enabled = true;
		Log.Info( $"[csaddons] host bound: {best?.Name} (priority {bestPriority})" );
	}

	void SpawnAddons()
	{
		var types = Game.TypeLibrary.GetTypes<RebornAddon>()
			.Where( t => !t.IsAbstract )
			.ToList();

		foreach ( var td in types.OrderBy( t => t.Name, StringComparer.OrdinalIgnoreCase ) )
		{
			try
			{
				// One host-local object per addon so an addon can parent its own
				// GameObjects cleanly and be inspected in the editor scene tree.
				var go = new GameObject( true, $"addon:{td.Name}" );
				go.Parent = GameObject;
				go.NetworkMode = NetworkMode.Never;

				var addon = go.Components.Create( td, true ) as RebornAddon;
				if ( addon == null ) { go.Destroy(); continue; }

				addon.OnAddonLoaded();
				_loaded.Add( addon );
				Log.Info( $"[csaddons] loaded {addon.Ident} \"{addon.Name}\" v{addon.Version}"
					+ (string.IsNullOrEmpty( addon.Author ) ? "" : $" by {addon.Author}") );
			}
			catch ( Exception e )
			{
				Log.Warning( $"[csaddons] {td.Name} failed to load: {e.Message}" );
			}
		}

		Log.Info( $"[csaddons] {_loaded.Count} C# addon(s) active." );
	}

	protected override void OnFixedUpdate()
	{
		Reborn.PumpTimers();
	}

	protected override void OnDestroy()
	{
		foreach ( var addon in _loaded )
		{
			try { if ( addon.IsValid() ) addon.OnAddonUnloaded(); }
			catch ( Exception e ) { Log.Warning( $"[csaddons] {addon.Ident} OnAddonUnloaded threw: {e.Message}" ); }
		}
		_loaded.Clear();
		if ( Instance == this ) Instance = null;
	}

	/// <summary>Central tap fed by the gamemode's hook dispatch (same call
	/// sites as the Lua addon hooks). Cheap no-op until a host is bound.</summary>
	public static void OnGameHook( string eventName, object[] args )
	{
		var host = Reborn.Host;
		if ( host == null || !host.IsValid() ) return;
		try { host.OnGameHook( eventName, args ); }
		catch ( Exception e ) { Log.Warning( $"[csaddons] hook '{eventName}' bridge threw: {e.Message}" ); }
	}

	[ConCmd( "csaddon_list" )]
	public static void ListAddons()
	{
		var sys = Instance;
		if ( sys == null || !sys.IsValid() ) { Log.Info( "[csaddons] system not running (host only)." ); return; }
		Log.Info( $"[csaddons] {sys._loaded.Count} addon(s):" );
		foreach ( var a in sys._loaded )
			Log.Info( $"  - {a.Ident} \"{a.Name}\" v{a.Version}{(string.IsNullOrEmpty( a.Author ) ? "" : $" by {a.Author}")}" );
		foreach ( var (name, desc) in Reborn.ChatCommands )
			Log.Info( $"  /{name} - {desc}" );
	}
}
