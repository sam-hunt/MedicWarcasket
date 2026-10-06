---
name: translate
description: Generate, update, or audit mod localization (DefInjected only) for a target language, grounded in Vanilla Factions Expanded - Pirates warcasket terminology plus vanilla Core/Odyssey apparel and medical terminology for Medic Warcasket's single warcasket set. Use when asked to add a language, update translations, or check translation freshness.
argument-hint: "[language, e.g. German | update | check]"
---

# Translate

Produce or refresh localization files for Medic Warcasket. English is the
source of truth; every other language derives from it.

**Translation passes are deferred until shortly before release** and run
only on explicit request, one language at a time (see `l10n/process.md`'s
non-negotiables). Generating the key expectations sidecar is always fine;
generating translations is not, until the English text is final.

**The family-wide process lives in the `l10n/` submodule, load these first,
and only these** (progressive disclosure; if `l10n/` is empty, run
`git submodule update --init`):

- `l10n/process.md`, non-negotiables, file/format conventions, terminology
  grounding method, and the generation / update / audit workflows. This is
  the workflow authority; follow it step by step.
- `l10n/languages/<Language>.md`, the target language's engine mechanics,
  style rules, and vanilla-grounded common vocabulary. Read ONLY the target
  language's file.
- `glossary/<Language>.md` (beside this file), this mod's own coined-term
  table for the target language. Read it in the same pass; if it does not
  exist yet, seed it per `glossary/README.md` from the sibling mod's file
  before translating.
- `l10n/lessons.md`, cross-language lessons; read when generating a new
  language, skim otherwise. Its free-prose register lesson applies with
  full force here: the three descriptions are lore paragraphs with no
  vanilla sentence to mirror, so a separate register read of every
  description and Workshop sentence is part of every pass.
- `l10n/workshop.md`, the `.steamworkshop/` conventions, whenever the pass
  touches the Workshop description (every initial generation does).

**Where learnings land:** mod-independent findings (engine mechanics, a
language's grammar rule, corpus style facts) go in the `l10n/` submodule,
edit the canonical checkout at `~/dev/rimworld-l10n`, commit and tag there.
Mod-specific findings (coined terms, phrasing decisions) go in
`glossary/<Language>.md`.

**Before any pass, bump the pin:** run `l10n/tools/bump-consumer.sh` (fetches
upstream's release tags, checks out the latest, commits the pointer as `chore:
Bump l10n submodule vOLD -> vNEW`; no-op when already current). This is one of
the three moments a pin moves (release, pass start, new upstream major), never
per upstream commit. If it reports a MAJOR bump, read the upstream release
notes for the shim or flow edit this repo owes before continuing.

## This mod's translation surface

- **No Keyed strings and no English Languages tree at all.** English is
  served entirely by the def XML's own fields; there is nothing under
  `1.6/Languages/English/`. The translation surface is DefInjected only,
  plus the Workshop page under `.steamworkshop/`.
- **Enumerate the key set from `Scripts/expected-injections.json`, never
  from a Languages folder or by scanning `1.6/Defs/`.** The sidecar is a
  dump of what the live game walks; regenerate it (game closed) with
  `python3 Scripts/refresh-translation-expectations.py` whenever the
  checker reports it stale. Take the English source text for each
  `<!-- EN: -->` comment from the sidecar's `english` field.
- **Def type folders** (the game rolls a def type without its own database
  into its base; the checker maps these via `DEF_TYPE_ALIASES` in
  `Scripts/check-translations.py`):
  - `DefInjected/ThingDef/` for the three `VFEPirates.WarcasketDef`s
    (`MDWC_Warcasket_Medic`, `MDWC_WarcasketShoulders_Medic`,
    `MDWC_WarcasketHelmet_Medic`): `label`, `description`, and
    `shortDescription` (a VFEP field shown in the foundry's part picker;
    translate it like any other). A `WarcasketDef` folder would never load.
  - Never translate or place a non-`required` sidecar entry (texture paths
    such as `shieldTexPath`) in any language file.
  - When the set's abilities land they will add def types (VEF's
    `VEF.Abilities.AbilityDef` is namespace-qualified; a `StatDef` or
    `HediffDef` is bare); the regenerated sidecar names them, so add the
    folders it lists rather than guessing.
- **The three descriptions share their second and third paragraphs
  verbatim** (the refit lore and the medic-series paragraph); only the
  first paragraph differs per part. Keep the shared paragraphs
  byte-identical across the three defs in every language, and keep each
  def's `shortDescription` identical to its description's first paragraph,
  as the English does. Paragraph breaks are the literal two-character
  `\n` sequences the def XML uses.
- **Compat roots carry no strings.** `1.6/Mods/VanillaGravshipExpanded/`
  ships only a patch with no translatable fields. Everything lands in the
  main `1.6/Languages/<Language>/` tree today. If a gated root ever gains a
  labelled def, its DefInjected must move into that root's own
  `Languages/` with a gate-suffixed filename (see CLAUDE.md's Localization
  and Optional-Content Gating section), never the main tree.
- **Workshop page:** `.steamworkshop/Description/<Language>.txt`, per
  `l10n/workshop.md` and the folder's own `README.md`. The title's anchor
  term is "warcasket"; every localized title must contain the rendering of
  "warcasket" recorded in that language's glossary. There is no Keyed
  title key to keep in step with (`WORKSHOP_TITLE_KEY` is `None`).

## This mod's grounding domain

Domain mod: **Vanilla Factions Expanded - Pirates (VFEP)**, plus vanilla
Core and Odyssey. **VFEP ships English only**, so its warcasket vocabulary
is not available from VFEP itself for any other language. For each target
language, check first whether a community "Vanilla Expanded" translation
covers VFEP in that language and ground terms against it; where none does,
coin the term and record it in `glossary/<Language>.md` rather than
inventing silently at translation time. The sibling Shipcracker Warcasket
repo's glossaries already record that search per language; start there
(see `glossary/README.md`).

Terms that MUST be grounded before use:

- from VFEP's own vocabulary: "warcasket" itself, the VFEP set names our
  text or Workshop page references (siegebreaker, guardian, brute),
  "warcasket foundry", "shoulders" / "pauldrons", "helmet", "shell";
- from vanilla Core (ground against the Core tar per `l10n/process.md`):
  apparel terms (armor, helmet, shoulder pads), plasteel, uranium, spacer
  tech level, energy shield / shield bubble, medic / doctor, medicine,
  tend / tending, wound, bandage, "squad";
- from vanilla Odyssey (ground against the Odyssey tar): vacuum.

Grep the tars for just this handful of terms; never extract or read a
whole tar. The vanilla-grounded answers for common words live in
`l10n/languages/<Language>.md`; this mod's own coined terms and VFEP-term
decisions live in `glossary/<Language>.md`.

## Workflows

Follow `l10n/process.md`'s Initial generation / Update pass / Audit-only
workflows verbatim. This mod's specifics on top:

- The checker: `python3 Scripts/check-translations.py` (`--strict` for new
  languages). Sidecar regen: `python3
  Scripts/refresh-translation-expectations.py` (game must be closed; drives
  the deployed L10nProbe, which must have this mod ticked in its settings).
- There is no compat-root routing to do today (see above); everything
  lands in the main tree.
- The public roster is CONTRIBUTING.md's localization table, update it in
  the same commit as any language addition or native review.
- Machine-assisted passes are run as one Opus subagent per language with a
  bounded brief (the key list, the grounded VFEP terms, the language file
  and glossary, the register gate); the lead reviews every diff and owns
  the commit.
