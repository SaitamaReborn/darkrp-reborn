using Sandbox;
using DarkRPReborn.Addons;

// Example addon: everything a DarkRP Reborn C# addon can do, in one file.
// Copy this folder, rename the class and Ident, and build your own.
// The loader finds every RebornAddon subclass automatically - no registration.

public sealed class WelcomeAddon : RebornAddon
{
	public override string Ident => "exemple-bienvenue";
	public override string Name => "Message de bienvenue";
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

		// Chat command: players type /coucou in chat.
		Reborn.RegisterChatCommand( "coucou", "Petit cadeau de bienvenue (100$)", ( player, args ) =>
		{
			player.GiveMoney( 100, "Cadeau de bienvenue" );
			player.Chat( "Voilà 100$ pour bien démarrer !" );
		}, Ident );

		// Repeating timer, host-side.
		Reborn.Every( 300f, () => Reborn.Broadcast( "Bienvenue sur DarkRP Reborn - tapez /coucou !" ) );

		Reborn.Log( Ident, $"chargé ({_joinCount} joueurs accueillis à ce jour)" );
	}

	void OnPlayerJoined( IRebornPlayer player )
	{
		_joinCount++;
		Reborn.SaveData( Ident, _joinCount.ToString() );

		player.Notify( "Bienvenue !", $"Bonjour {player.Name}, bon jeu sur le serveur.", RebornNotifyType.Success );
		Reborn.Broadcast( $"{player.Name} vient d'arriver en ville. Souhaitez-lui la bienvenue !" );
	}

	void OnJobChanged( IRebornPlayer player, string oldJob, string newJob )
	{
		player.Chat( $"Nouveau métier : {newJob}. Bonne chance !" );
	}

	public override void OnAddonUnloaded()
	{
		RebornEvents.PlayerJoined -= OnPlayerJoined;
		RebornEvents.JobChanged -= OnJobChanged;
	}
}
