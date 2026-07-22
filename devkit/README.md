# DarkRP Reborn - Dev Kit

The Dev Kit is an s&box project you can **open as-is in the editor** to build
DarkRP Reborn C# addons **without the game's source code**. It contains:

- `code/Addons/` - the **public API contract** (`RebornAddon`, `Reborn`,
  `RebornEvents`, `IRebornPlayer`): exact copies of the files shipped inside the
  game. Do not modify them - your addon must compile against the contract as-is.
- `code/Devkit/` - a **simulated server**: fake players (money, jobs), chat,
  an on-screen console. On the real server the same API is bound to the game's
  real systems; your code does not change by a single line.
- `code/MyAddons/` - your addons. One folder per addon, lowercase-hyphens ident.
  `example-welcome/` shows everything: events, chat command, money, timers,
  persistence.

## Getting started

1. Install [s&box](https://sbox.game) (Steam).
2. Open `devkit.sbproj` in the s&box editor.
3. Press **Play**: the Dev Kit console appears, Alice and Bob join the simulated
   server, and your addon receives the events.
4. Drive the simulation from the s&box console:
   - `devkit_players` - list the simulated players
   - `devkit_join Name` / `devkit_leave Name`
   - `devkit_say "hello"` - public chat / `devkit_say "/welcome"` - addon command
   - `devkit_die Name` - death + respawn
   - `devkit_job Name Police` - job change
   - `csaddon_list` - loaded addons + registered commands

5. Build yours: copy `../template/addon-csharp/` to `code/MyAddons/<your-ident>/`,
   rename, iterate. The editor hotload picks up your code on save.

## Shipping your addon

Open a Pull Request on the public repository
[SaitamaReborn/darkrp-reborn](https://github.com/SaitamaReborn/darkrp-reborn):
your folder under `addons/<your-ident>/` with its `addon.md` sheet. After
review, the addon is integrated into the game's `code/Addons/<your-ident>/` and
deployed with the next publish - it then runs with real players, unchanged.

Full documentation: see `docs/` (Build addons -> C# addons).
