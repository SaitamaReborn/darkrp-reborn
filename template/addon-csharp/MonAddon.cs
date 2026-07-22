using Sandbox;
using DarkRPReborn.Addons;

// Template d'addon C# DarkRP Reborn.
// 1. Copiez ce dossier dans devkit/code/MyAddons/<votre-ident>/
// 2. Renommez la classe et remplissez Ident/Name/Author.
// 3. Appuyez sur Play dans l'éditeur s&box : l'addon est chargé automatiquement.
// (Code comments in English in real addons - this template is the one exception
// so first-time French readers can follow along.)

public sealed class MonAddon : RebornAddon
{
	// Minuscules, chiffres et tirets uniquement. Ne changez plus jamais l'ident
	// après publication : la sauvegarde et le dossier d'intégration en dépendent.
	public override string Ident => "mon-addon";
	public override string Name => "Mon premier addon";
	public override string Author => "Votre pseudo";
	public override string Version => "1.0";

	public override void OnAddonLoaded()
	{
		// Événements du jeu : RebornEvents.PlayerJoined, PlayerSpawned, PlayerLeft,
		// PlayerDied, PlayerChat, JobChanged, ServerReady.
		RebornEvents.PlayerJoined += p => p.Chat( $"Bonjour {p.Name} !" );

		// Commande chat : les joueurs tapent /salut
		Reborn.RegisterChatCommand( "salut", "Dit bonjour", ( player, args ) =>
		{
			player.Notify( "Salut !", $"Tu as {player.Money}$ en poche.", RebornNotifyType.Info );
		}, Ident );

		Reborn.Log( Ident, "chargé !" );
	}
}
