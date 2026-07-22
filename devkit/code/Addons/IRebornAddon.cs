using System;
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

	/// <summary>Host-authoritative money grant. No-op off-host.</summary>
	void GiveMoney( int amount, string reason = "" );

	/// <summary>Host-authoritative charge. Returns false (and takes nothing)
	/// if the player cannot afford it.</summary>
	bool TakeMoney( int amount, string reason = "" );

	/// <summary>Toast notification on this player's screen.</summary>
	void Notify( string title, string message, RebornNotifyType type = RebornNotifyType.Info );

	/// <summary>Private system line in this player's chat.</summary>
	void Chat( string message );

	/// <summary>The pawn's GameObject on the real server; null in the Dev Kit.
	/// Use it for world interactions (position, spawning props nearby...).</summary>
	GameObject GameObject { get; }
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
