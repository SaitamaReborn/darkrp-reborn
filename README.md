# DarkRP Reborn

**Le roleplay urbain nouvelle génération sur [s&box](https://sbox.game).** Ville
vivante, 29 métiers, économie persistante, 10 langues — l'héritier spirituel de
DarkRP, reconstruit de zéro sur le moteur Source 2 de Facepunch.

▶ **Jouer** : [sbox.game/rebornstudio/cityliferp](https://sbox.game/rebornstudio/cityliferp)
· 📚 **Documentation** : [`docs/`](docs/) (guide complet fr/en)
· 🧩 **Créer un addon** : [démarrer ici](docs/addons/csharp-demarrage.mdx)

---

Ce dépôt est la **partie ouverte** du projet : tout ce qu'il faut pour créer,
tester et soumettre des **addons** — sans le code source du jeu.

```text
devkit/      Projet s&box OUVRABLE : serveur simulé + contrat d'API du jeu.
             → ouvrez devkit/devkit.sbproj dans l'éditeur, appuyez sur Play.
template/    Squelette d'addon C# à copier pour démarrer.
addons/      Les addons communautaires. Soumettez le vôtre par Pull Request.
docs/        Toute la documentation (fr/en) : addons C#, addons Lua,
             hébergement serveur, migration depuis Garry's Mod.
```

## Créer votre addon en 5 minutes

1. Installez [s&box](https://sbox.game) (Steam, gratuit) puis clonez ce dépôt.
2. Ouvrez `devkit/devkit.sbproj` dans l'éditeur s&box → **Play** : un serveur
   simulé démarre (joueurs fictifs, console à l'écran, commandes `devkit_*`).
3. Copiez `template/addon-csharp/` vers `devkit/code/MyAddons/<votre-ident>/`
   et codez : événements du jeu, commandes chat, argent, timers, sauvegarde —
   tout le contrat est documenté dans [la référence API](docs/addons/csharp-api.mdx).

```csharp
public sealed class MonAddon : RebornAddon
{
	public override string Ident => "mon-addon";
	public override string Name => "Mon premier addon";

	public override void OnAddonLoaded()
	{
		RebornEvents.PlayerJoined += p => p.Chat( $"Bonjour {p.Name} !" );
		Reborn.RegisterChatCommand( "salut", "Dit bonjour", ( player, args ) =>
			player.Notify( "Salut !", $"Tu as {player.Money}$ en poche." ), Ident );
	}
}
```

Aucun enregistrement : chaque classe dérivée de `RebornAddon` est détectée et
chargée automatiquement, côté serveur. Votre code tourne **à l'identique** dans
le simulateur du Dev Kit et sur le serveur en production.

## Soumettre votre addon

Fork → votre dossier dans `addons/<votre-ident>/` (avec sa fiche `addon.md`) →
**Pull Request**. Après revue ([checklist](CONTRIBUTING.md)), l'addon est
intégré au jeu et déployé au publish suivant — votre nom s'affiche en jeu dans
`csaddon_list`. Guide complet : [soumettre son addon](docs/addons/csharp-livrer.mdx).

Vous venez de Garry's Mod ? Les **addons Lua/GLua** (syntaxe GMod, `hook.Add`,
`DarkRP.createJob`…) et la **migration de serveur en une commande** sont aussi
documentés : [venir de Garry's Mod](docs/compat/migration.mdx).

## English

This repository is the **open half** of DarkRP Reborn: everything you need to
build, test and submit addons — without the game's source code. Open
`devkit/devkit.sbproj` in the s&box editor, press Play, copy the template into
`devkit/code/MyAddons/<your-ident>/` and start coding against the public API
contract (`RebornAddon`, `Reborn`, `RebornEvents`, `IRebornPlayer`). Docs in
English: [getting started](docs/en/addons/csharp-getting-started.mdx),
[API reference](docs/en/addons/csharp-api.mdx),
[submitting](docs/en/addons/csharp-shipping.mdx). Submit via Pull Request into
`addons/<your-ident>/`.

## Licence

Le Dev Kit, le template et la documentation sont librement utilisables pour
créer des addons DarkRP Reborn. Le jeu lui-même reste propriété de Reborn
Studio ; la redistribution du contenu de ce dépôt en dehors de cet usage n'est
pas autorisée. En soumettant un addon, vous en restez l'auteur (crédité en jeu)
et vous autorisez son intégration au jeu publié.
