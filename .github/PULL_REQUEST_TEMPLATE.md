## Mon addon

- **Ident** : `mon-addon`
- **Nom affiché** :
- **Version** :
- **Description (1 phrase)** :

## Ce que l'addon fait en jeu

<!-- Commandes ajoutées, événements écoutés, ce que le joueur voit. -->

## Checklist (cochez avant d'ouvrir la PR)

- [ ] Testé dans le Dev Kit (`devkit_join`, `devkit_say "/macommande"`, ...)
- [ ] N'utilise QUE le contrat public (`RebornAddon`, `Reborn`, `RebornEvents`, `IRebornPlayer`)
- [ ] Pas d'allocations par frame dans `OnUpdate`/`OnFixedUpdate`
- [ ] Argent uniquement via `GiveMoney`/`TakeMoney`
- [ ] `addon.md` remplie (ident, auteur, version, description)
- [ ] J'accepte l'intégration de cet addon au jeu publié (je reste crédité comme auteur)
