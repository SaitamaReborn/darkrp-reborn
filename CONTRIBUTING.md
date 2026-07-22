# Contributing — submitting an addon

Thanks for contributing to DarkRP Reborn! This repository accepts **C# addons**
built with the Dev Kit. One Pull Request = one addon.

## The pipeline

1. Build your addon in the Dev Kit (`devkit/`, see
   [the guide](docs/en/addons/csharp-getting-started.mdx)).
2. Fork this repository and copy your folder to `addons/<your-ident>/`:

```text
addons/
└── my-addon/               ← your ident: lowercase, digits, hyphens (permanent)
    ├── addon.md            ← sheet: ident, author, version, description
    └── MyAddon.cs          ← your classes (deriving from RebornAddon)
```

3. Open a Pull Request (the template guides you).
4. Team review → possible back-and-forth → merge = integration into the game
   and deployment with the next publish. You remain the author (`Author`,
   shown in game in `csaddon_list`).

## Review checklist

Accepted:

- References **only** the public contract (`RebornAddon`, `Reborn`,
  `RebornEvents`, `IRebornPlayer`) and the standard s&box API.
- Server-authoritative logic: money via `GiveMoney`/`TakeMoney`, never a
  client-invented value.
- Lean `OnUpdate`/`OnFixedUpdate`: no per-frame allocations, no scene scans
  every tick (cache + `Reborn.Every`).
- Clean player-facing text, ideally localized via `p.Language`.
- One addon per folder, stable ident, `Version` filled in, `addon.md` complete.

Rejected outright:

- Modified copies of the contract files (`devkit/code/Addons/*.cs`).
- Reflection into the game's internal types, file access outside
  `Reborn.SaveData`/`LoadData`, HTTP requests.
- Unbalanced economy (money printing, infinite reward loops).
- Obfuscated or unreadable code: the review reads everything.

## Updates

Same pipeline: a PR on your folder with `Version` bumped and the changelog in
the PR description.

## Something other than an addon?

Documentation fixes are welcome (PR on `docs/`). For bugs in the game itself,
use the Discord — this repository does not contain the game's code.
