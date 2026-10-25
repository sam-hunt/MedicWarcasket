# CLAUDE.md

Guidance for Claude Code when working in this repository. This file holds only what applies
across the whole repo. Folder guides load on their own when work touches them: `1.6/CLAUDE.md`
(defs, patches, languages, gating), `Source/1.6/CLAUDE.md` (C#), `Scripts/CLAUDE.md` (l10n and
release tooling), `.steamworkshop/CLAUDE.md` (Workshop page).

## Project

**Medic Warcasket** is a RimWorld 1.6/1.7 mod adding one warcasket apparel set (armor, shoulder
pads, helmet) for Vanilla Factions Expanded - Pirates (VFEP): a combat medic on VFEP's
10th-generation spacer chassis. VFEP and Harmony are the hard dependencies (VEF comes with VFEP);
Odyssey and other optional content load only when present. C# (.NET Framework 4.7.2), Harmony,
XML defs. Def prefix `MDWC_`.

**State: mechanics in, tuning not.** The doctor passives, the mechanite dressing, the adrenaline
shot and the medic work trait are implemented; every protection, cost and shield value is still
Siegebreaker's. `TODOs.md` is the ledger. Each feature's design rationale is a `Docs/Specs/`
file; the sibling `../ShipcrackerWarcasket/` repo (same author, same VFEP base) is the worked
precedent for tuning rationale, render-tree art and the settings/ability idioms copied here.

## Documentation rules

- **The file's header is the record.** Per-item values, tuning numbers and decompile-verified
  call paths live in the header comment of the file they describe, with the _why_. No guide
  restates a def value or call path; a line joins a guide only when it constrains other files.
- **Comments describe the present, git describes the past.** No dates, previous values, dropped
  attempts or "seen in game" notes in comments; that history goes in the commit message. Def
  comments ship in the bundle, so keep them lean.
- **Never read a `Docs/Specs/` file whole** (400 to 900 lines each). Grep for the section you
  need, or delegate a spec-wide question to a subagent and keep its answer.
- **Dependency sources are read-only.** `../VanillaExpanded/VanillaFactionsExpanded-Pirates/`
  (its `CLAUDE.md` maps the warcasket subsystem) and
  `../VanillaExpanded/VanillaExpandedFramework/` are upstream checkouts; never commit into them.

## Build

```bash
dotnet build MedicWarcasket.sln -c Release   # builds 1.6/Assemblies/ and redeploys to Mods/MedicWarcasket
```

Compiles against the pinned `Krafs.Rimworld.Ref` 1.6 package; the local install (auto-detected,
or `RIMWORLD_PATH`, in WSL the Windows path) only decides deployment and serves as the decompile
target. Release builds deploy atomically through the csproj's `StageMod` target, the one file
manifest, which CI calls too; Debug builds never deploy. The csproj's comments cover path
detection, the compile-only VFEP/VEF references and the manifest's globbing. The Stop hook
`.claude/hooks/sync-mod.sh` (self-documenting, byte-identical across the mod family) rebuilds
after any turn that changed mod files and feeds a failing build back to the agent.
**Warnings are build errors** (`TreatWarningsAsErrors`, severities pinned in `.editorconfig`):
fix the code, not the severity.

## Layout

```
About/            Mod metadata
1.6/              Defs, Patches, Languages/English/Keyed, Mods/<compat roots>; serves 1.6 and 1.7
Source/1.6/       C# (net472)
Textures/         Art, version-independent, under VFEP's Warcasketlike/ layout
Scripts/          l10n shims and the smoke test             (not shipped)
l10n/             rimworld-l10n toolkit, pinned submodule   (not shipped)
Docs/Research/    VFEP warcasket stat tables and ability mechanics
Docs/Specs/       Design specs behind the landed features
.steamworkshop/   Workshop page text                        (not shipped)
LoadFolders.xml   Load roots per game version, compat roots gated by package id
```

## Cross-cutting rules

- **One def per file**, named after the def with the `MDWC_` prefix stripped.
- **Log with the `[Medic Warcasket]` prefix.** Root namespace `MedicWarcasket`.
- **No em dashes in player-facing text** (labels, descriptions, `Keyed/`, `About.xml`); reflow
  the sentence. Code comments, def comments and these guides are unaffected.
- **Settings override def fields, never code paths**, written onto the defs at startup and on
  window close, so defaults come from the XML and the def header stays the record.
- **Every startup read or write of defs goes through `Startup.Run`**, idempotent and re-run on
  every in-process play-data reload; never a bare static ctor.
- **English is the source of truth for text.** Translations come from the `/translate` skill,
  one language at a time, shortly before a release and never earlier.
- **`.claude/` is partly tracked** (`skills/`, `hooks/`, `settings.json`; the rest is personal).
  Editing a skill is a team-visible change that must keep in step with what it automates.

## Releases and debugging

Run the `/release` skill. It encodes the CHANGELOG layout, the version scheme (`X.Y.Z-rc.N`
candidates are CHANGELOG-less and Workshop-less prereleases) and the pre-release checks;
`.github/workflows/release.yml` fails a stable release whose CHANGELOG section is missing.

The `rimworld-logs` skill finds `Player.log` on 1.6 and 1.7 and disassembles the game, VFEP and
VEF assemblies; the local install's assembly, now the 1.7 one, is the source of truth over the
Krafs package. In game, VFEP and Vanilla Expanded Framework must load before this mod, which
appears as `MedicWarcasket` in the list.
