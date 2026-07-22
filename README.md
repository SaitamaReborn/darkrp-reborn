# DarkRP Reborn

**Next-generation city roleplay on [s&box](https://sbox.game).** A living city,
29 jobs, a persistent economy, 10 languages — the spiritual successor of DarkRP,
rebuilt from scratch on Facepunch's Source 2 engine.

▶ **Play**: [sbox.game/rebornstudio/cityliferp](https://sbox.game/rebornstudio/cityliferp)
· 📚 **Documentation**: [`docs/`](docs/) (full guide, EN/FR)
· 🧩 **Build an addon**: [start here](docs/en/addons/csharp-getting-started.mdx)

---

This repository is the **open half** of the project: everything you need to
build, test and submit **addons** — without the game's source code.

```text
devkit/      An OPENABLE s&box project: simulated server + the game's API contract.
             → open devkit/devkit.sbproj in the editor, press Play.
template/    A starter C# addon skeleton to copy.
addons/      Community addons. Submit yours with a Pull Request.
docs/        The full documentation (EN/FR): C# addons, Lua addons,
             server hosting, migrating from Garry's Mod.
```

## Build your addon in 5 minutes

1. Install [s&box](https://sbox.game) (Steam, free) and clone this repository.
2. Open `devkit/devkit.sbproj` in the s&box editor → **Play**: a simulated
   server boots (fake players, on-screen console, `devkit_*` console commands).
3. Copy `template/addon-csharp/` to `devkit/code/MyAddons/<your-ident>/` and
   start coding: game events, chat commands, money, timers, persistence — the
   whole contract is covered in the [API reference](docs/en/addons/csharp-api.mdx).

```csharp
public sealed class MyAddon : RebornAddon
{
	public override string Ident => "my-addon";
	public override string Name => "My first addon";

	public override void OnAddonLoaded()
	{
		RebornEvents.PlayerJoined += p => p.Chat( $"Hello {p.Name}!" );
		Reborn.RegisterChatCommand( "hi", "Says hi", ( player, args ) =>
			player.Notify( "Hi!", $"You are carrying {player.Money}$." ), Ident );
	}
}
```

No registration: every class deriving from `RebornAddon` is discovered and
loaded automatically, server side. Your code runs **identically** in the Dev Kit
simulator and on the production server.

## Submit your addon

Fork → your folder under `addons/<your-ident>/` (with its `addon.md` sheet) →
**Pull Request**. After review ([checklist](CONTRIBUTING.md)), the addon is
integrated into the game and deployed with the next publish — your name shows
in game in `csaddon_list`. Full guide: [submitting your addon](docs/en/addons/csharp-shipping.mdx).

Coming from Garry's Mod? **Lua/GLua addons** (GMod syntax, `hook.Add`,
`DarkRP.createJob`…) and the **one-command server migration** are documented
too: [coming from Garry's Mod](docs/en/compat/migration.mdx).

## Français

Ce dépôt est la **partie ouverte** de DarkRP Reborn : tout ce qu'il faut pour
créer, tester et soumettre des addons — sans le code source du jeu. Ouvrez
`devkit/devkit.sbproj` dans l'éditeur s&box, appuyez sur Play, copiez le
template dans `devkit/code/MyAddons/<votre-ident>/` et codez contre le contrat
d'API public. Doc en français : [démarrer](docs/addons/csharp-demarrage.mdx),
[référence API](docs/addons/csharp-api.mdx),
[soumettre](docs/addons/csharp-livrer.mdx). Soumission par Pull Request dans
`addons/<votre-ident>/`.

## License

The Dev Kit, the template and the documentation are freely usable to create
DarkRP Reborn addons. The game itself remains the property of Reborn Studio;
redistributing this repository's content outside of that purpose is not
permitted. By submitting an addon you remain its author (credited in game) and
you allow its integration into the published game.
