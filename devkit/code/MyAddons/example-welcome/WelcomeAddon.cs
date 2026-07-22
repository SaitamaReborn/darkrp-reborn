using Sandbox;
using DarkRPReborn.Addons;

// Example addon: everything a DarkRP Reborn C# addon can do, in one file.
// Copy this folder, rename the class and Ident, and build your own.
// The loader finds every RebornAddon subclass automatically - no registration.

public sealed class WelcomeAddon : RebornAddon
{
	public override string Ident => "example-welcome";
	public override string Name => "Welcome message";
	public override string Author => "DarkRP Reborn";
	public override string Version => "1.0";

	int _joinCount;

	public override void OnAddonLoaded()
	{
		// Persisted state: one JSON blob per addon, survives restarts.
		var saved = Reborn.LoadData( Ident );
		if ( saved != null ) int.TryParse( saved, out _joinCount );

		// Typed events - same game moments as the Lua hook system.
		RebornEvents.PlayerJoined += OnPlayerJoined;
		RebornEvents.JobChanged += OnJobChanged;

		// Chat command: players type /welcome in chat.
		Reborn.RegisterChatCommand( "welcome", "A small welcome gift (100$)", ( player, args ) =>
		{
			player.GiveMoney( 100, "Welcome gift" );
			player.Chat( "Here is 100$ to get you started!" );
		}, Ident );

		// Repeating timer, host-side.
		Reborn.Every( 300f, () => Reborn.Broadcast( "Welcome to DarkRP Reborn - type /welcome!" ) );

		Reborn.Log( Ident, $"loaded ({_joinCount} players welcomed so far)" );
	}

	void OnPlayerJoined( IRebornPlayer player )
	{
		_joinCount++;
		Reborn.SaveData( Ident, _joinCount.ToString() );

		player.Notify( "Welcome!", $"Hello {player.Name}, have a good time on the server.", RebornNotifyType.Success );
		Reborn.Broadcast( $"{player.Name} just arrived in town. Say hi!" );
	}

	void OnJobChanged( IRebornPlayer player, string oldJob, string newJob )
	{
		player.Chat( $"New job: {newJob}. Good luck!" );
	}

	public override void OnAddonUnloaded()
	{
		RebornEvents.PlayerJoined -= OnPlayerJoined;
		RebornEvents.JobChanged -= OnJobChanged;
	}
}
