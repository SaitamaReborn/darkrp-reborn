# DarkRP Reborn - Dev Kit

Le Dev Kit est un projet s&box **ouvrable tel quel dans l'éditeur** pour développer
des addons C# DarkRP Reborn **sans le code source du jeu**. Il contient :

- `code/Addons/` - le **contrat d'API public** (`RebornAddon`, `Reborn`, `RebornEvents`,
  `IRebornPlayer`) : copie exacte des fichiers embarqués dans le jeu. Ne les modifiez
  pas - votre addon doit compiler contre le contrat tel quel.
- `code/Devkit/` - un **serveur simulé** : faux joueurs (argent, métiers), chat,
  console à l'écran. Sur le vrai serveur, la même API est branchée sur les vrais
  systèmes du jeu ; votre code ne change pas d'une ligne.
- `code/MyAddons/` - vos addons. Un dossier par addon, ident en minuscules-tirets.
  `exemple-bienvenue/` montre tout : événements, commande chat, argent, timers,
  sauvegarde.

## Démarrer

1. Installez [s&box](https://sbox.game) (Steam).
2. Ouvrez `devkit.sbproj` dans l'éditeur s&box.
3. Appuyez sur **Play** : la console du Dev Kit s'affiche, Alice et Bob rejoignent
   le serveur simulé, et votre addon reçoit les événements.
4. Pilotez la simulation depuis la console s&box :
   - `devkit_players` - liste les joueurs simulés
   - `devkit_join Nom` / `devkit_leave Nom`
   - `devkit_say "bonjour"` - chat public / `devkit_say "/coucou"` - commande d'addon
   - `devkit_die Nom` - mort + respawn
   - `devkit_job Nom Policier` - changement de métier
   - `csaddon_list` - addons chargés + commandes enregistrées

5. Créez le vôtre : copiez `../template/addon-csharp/` dans `code/MyAddons/<votre-ident>/`,
   renommez, itérez. Le hotload de l'éditeur recharge votre code à la sauvegarde.

## Livrer votre addon

Ouvrez une Pull Request sur le dépôt public
[SaitamaReborn/darkrp-reborn](https://github.com/SaitamaReborn/darkrp-reborn) : votre dossier dans `addons/<votre-ident>/`
avec sa fiche `addon.md`. Après revue, l'addon est intégré au jeu dans
`code/Addons/<votre-ident>/` et déployé au prochain publish - il tourne alors
avec les vrais joueurs, sans modification.

Documentation complète : voir `docs/` (Créer des addons -> Addons C#).
