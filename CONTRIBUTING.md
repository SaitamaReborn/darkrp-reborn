# Contribuer — soumettre un addon

Merci de contribuer à DarkRP Reborn ! Ce dépôt accepte les **addons C#**
développés avec le Dev Kit. Une Pull Request = un addon.

## Le circuit

1. Développez dans le Dev Kit (`devkit/`, voir
   [le guide](docs/addons/csharp-demarrage.mdx)).
2. Forkez ce dépôt, copiez votre dossier vers `addons/<votre-ident>/` :

```text
addons/
└── mon-addon/              ← ident : minuscules, chiffres, tirets (définitif)
    ├── addon.md            ← fiche : ident, auteur, version, description
    └── MonAddon.cs         ← vos classes (dérivées de RebornAddon)
```

3. Ouvrez une Pull Request (le template vous guide).
4. Revue par l'équipe → allers-retours éventuels → merge = intégration au jeu
   et déploiement au publish suivant. Vous restez l'auteur (`Author`, affiché
   en jeu dans `csaddon_list`).

## Checklist de revue

Accepté :

- Ne référence **que** le contrat public (`RebornAddon`, `Reborn`,
  `RebornEvents`, `IRebornPlayer`) et l'API s&box standard.
- Logique serveur : argent via `GiveMoney`/`TakeMoney`, jamais de valeur
  inventée côté client.
- `OnUpdate`/`OnFixedUpdate` sobres : pas d'allocations par frame, pas de scan
  de scène à chaque tick (cache + `Reborn.Every`).
- Textes joueurs propres, idéalement localisés via `p.Language`.
- Un addon par dossier, ident stable, `Version` renseignée, `addon.md` remplie.

Refusé d'office :

- Copie modifiée des fichiers du contrat (`devkit/code/Addons/*.cs`).
- Réflexion sur les types internes du jeu, accès fichiers hors
  `Reborn.SaveData`/`LoadData`, requêtes HTTP.
- Économie déséquilibrée (impression d'argent, boucles de récompense infinies).
- Code obfusqué ou illisible : la revue lit tout.

## Mises à jour

Même circuit : PR sur votre dossier avec `Version` incrémentée et le changelog
dans la description de la PR.

## Autre chose qu'un addon ?

Corrections de documentation bienvenues (PR sur `docs/`). Pour les bugs du jeu
lui-même, passez par le Discord — ce dépôt ne contient pas le code du jeu.
