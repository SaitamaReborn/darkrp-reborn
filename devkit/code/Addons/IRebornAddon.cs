using System;
using System.Collections.Generic;
using Sandbox;

namespace DarkRPReborn.Addons;

/// <summary>
/// Base class every C# addon derives from. Addons are component-based services:
/// any non-abstract subclass found in the compiled project is spawned
/// automatically by <see cref="RebornAddonSystem"/> at boot (host side), one
/// GameObject per addon. This file is part of the PUBLIC addon contract and is
/// mirrored verbatim in the community Dev Kit - keep it free of any private
/// game types.
/// </summary>
public abstract class RebornAddon : Component
{
	/// <summary>Stable identifier: lowercase letters, digits and hyphens only
	/// (e.g. "vip-greeter"). Never change it after release - server data and
	/// the integration folder name derive from it.</summary>
	public abstract string Ident { get; }

	/// <summary>Display name shown in logs and listings.</summary>
	public abstract string Name { get; }

	public virtual string Author => "";
	public virtual string Version => "1.0";

	/// <summary>Called once by the loader after the host API is bound and the
	/// addon object exists. Subscribe to <see cref="RebornEvents"/> and register
	/// chat commands here. Regular component lifecycle (OnStart, OnUpdate,
	/// OnFixedUpdate) also applies afterwards.</summary>
	public virtual void OnAddonLoaded() { }

	/// <summary>Called when the addon is being torn down (server shutdown).</summary>
	public virtual void OnAddonUnloaded() { }
}

/// <summary>
/// The player handle addons work with. Wraps the real pawn on the game server
/// and a simulated player inside the Dev Kit, so addon code compiles and runs
/// in both without referencing private game classes.
/// </summary>
public interface IRebornPlayer
{
	/// <summary>Display name (Steam name on the real server).</summary>
	string Name { get; }

	/// <summary>SteamID64 as a string ("0" for simulated players).</summary>
	string SteamId { get; }

	/// <summary>Current job name (e.g. "Policier"). Empty if unknown.</summary>
	string Job { get; }

	/// <summary>Cash on hand.</summary>
	long Money { get; }

	/// <summary>Two-letter language code of this player's UI ("fr", "en", ...).</summary>
	string Language { get; }

	/// <summary>World position of the pawn (Vector3.Zero in the Dev Kit).</summary>
	Vector3 Position { get; }

	/// <summary>Current health (base scale 0..100).</summary>
	float Health { get; }

	/// <summary>Host-authoritative money grant. No-op off-host.</summary>
	void GiveMoney( int amount, string reason = "" );

	/// <summary>Host-authoritative charge. Returns false (and takes nothing)
	/// if the player cannot afford it.</summary>
	bool TakeMoney( int amount, string reason = "" );

	/// <summary>Set cash on hand to an exact amount (host-authoritative).</summary>
	void SetMoney( long amount );

	/// <summary>Move the pawn to a world position (host side).</summary>
	void Teleport( Vector3 position );

	/// <summary>Set health, clamped to 0..100. 0 is lethal.</summary>
	void SetHealth( float health );

	/// <summary>Switch this player's job by job id or display name. Goes through
	/// the game's normal job rules (max workers, vote-gated jobs may open a
	/// vote instead of switching instantly). Returns false when no job matches.</summary>
	bool TrySetJob( string jobIdOrName );

	/// <summary>Put items in this player's inventory by item id (catalog or
	/// addon-registered). Returns false when the id is unknown or the
	/// inventory refuses (full).</summary>
	bool GiveItem( string itemId, int amount = 1 );

	/// <summary>Toast notification on this player's screen.</summary>
	void Notify( string title, string message, RebornNotifyType type = RebornNotifyType.Info );

	/// <summary>Private system line in this player's chat.</summary>
	void Chat( string message );

	/// <summary>The pawn's GameObject on the real server; null in the Dev Kit.
	/// Use it for world interactions (position, spawning props nearby...).</summary>
	GameObject GameObject { get; }
}

/// <summary>Runtime job definition for <see cref="Reborn.RegisterJob"/> - the
/// job shows up in the F4 menu like any data-driven one.</summary>
public sealed class RebornJobSpec
{
	/// <summary>Display name, unique among jobs. Required.</summary>
	public string Name { get; set; }
	public string Description { get; set; } = "";
	public float Salary { get; set; } = 250f;
	/// <summary>0 = unlimited.</summary>
	public int MaxWorkers { get; set; } = 0;
	/// <summary>True = taking the job opens a server-wide vote.</summary>
	public bool Vote { get; set; } = false;
	/// <summary>Hex color like "#5aa8ff" (empty = default).</summary>
	public string Color { get; set; } = "";
	/// <summary>Existing F4 category name to file the job under (empty = none).</summary>
	public string Category { get; set; } = "";
	public List<string> Weapons { get; set; }
	public List<string> Items { get; set; }
}

/// <summary>Runtime item definition for <see cref="Reborn.RegisterItem"/> -
/// registered into the item database, usable with IRebornPlayer.GiveItem.</summary>
public sealed class RebornItemSpec
{
	/// <summary>Stable item id, unique. Required.</summary>
	public string Id { get; set; }
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	/// <summary>Icon path or thumb: protocol (empty = default).</summary>
	public string Icon { get; set; } = "";
	public int MaxStack { get; set; } = 1;
	/// <summary>Base value in $.</summary>
	public int Value { get; set; } = 0;
	public float HungerRestore { get; set; } = 0f;
	public float ThirstRestore { get; set; } = 0f;
	public float HealthRestore { get; set; } = 0f;
	public string WorldModel { get; set; } = "";
	/// <summary>Item type name (Material, Consumable...). Default Material.</summary>
	public string Type { get; set; } = "Material";
}

public enum RebornNotifyType
{
	Info,
	Success,
	Warning,
	Error,
}

/// <summary>Payload of <see cref="RebornEvents.PlayerChat"/>. Observe-only in
/// v1: the message is already broadcast when addons see it.</summary>
public sealed class RebornChatEvent
{
	public IRebornPlayer Speaker { get; init; }
	public string Text { get; init; }
}
