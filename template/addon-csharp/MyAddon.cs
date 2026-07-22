using Sandbox;
using DarkRPReborn.Addons;

// DarkRP Reborn C# addon template.
// 1. Copy this folder to devkit/code/MyAddons/<your-ident>/
// 2. Rename the class and fill in Ident/Name/Author.
// 3. Press Play in the s&box editor: the addon is loaded automatically.

public sealed class MyAddon : RebornAddon
{
	// Lowercase letters, digits and hyphens only. Never change the ident after
	// release: saved data and the integration folder name derive from it.
	public override string Ident => "my-addon";
	public override string Name => "My first addon";
	public override string Author => "Your nickname";
	public override string Version => "1.0";

	public override void OnAddonLoaded()
	{
		// Game events: RebornEvents.PlayerJoined, PlayerSpawned, PlayerLeft,
		// PlayerDied, PlayerChat, JobChanged, ServerReady.
		RebornEvents.PlayerJoined += p => p.Chat( $"Hello {p.Name}!" );

		// Chat command: players type /hi
		Reborn.RegisterChatCommand( "hi", "Says hi", ( player, args ) =>
		{
			player.Notify( "Hi!", $"You are carrying {player.Money}$.", RebornNotifyType.Info );
		}, Ident );

		Reborn.Log( Ident, "loaded!" );
	}
}
