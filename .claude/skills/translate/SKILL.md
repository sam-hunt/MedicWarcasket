---
name: translate
description: Generate, update, or audit mod localization (DefInjected plus one Keyed file) for a target language, grounded in Vanilla Factions Expanded - Pirates warcasket terminology plus vanilla Core/Odyssey apparel and medical terminology for Medic Warcasket's single warcasket set. Use when asked to add a language, update translations, or check translation freshness.
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
  table for the target language. Read it in the same pass. Every file is
  seeded: its inherited rows are the sibling Shipcracker Warcasket repo's
  VFEP warcasket vocabulary (warcasket, parts, foundry, entomb, set names,
  the VEF ability strings), copied verbatim so the sets read as one family
  in-game, and its closing list names the terms the first pass must still
  ground or coin. The pass fills that list in and never reopens an
  inherited row (a change there is made in the Shipcracker glossary first,
  then mirrored here and in Toxin Warcasket).
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

- **Two surfaces plus the Workshop page.** The defs' English is served by
  the def XML's own fields (DefInjected, below). Everything code prints is
  the one Keyed file, `1.6/Languages/English/Keyed/MedicWarcasket.xml`,
  translated to `1.6/Languages/<Language>/Keyed/MedicWarcasket.xml` with
  the same keys:
  - the mod settings window: `MDWC_SettingsCategory` (its header),
    `MDWC_SettingsUnavailable`, the section header, and each setting's
    label and `...Desc` tooltip (medicine, kits held, warmup, time per
    wound and cooldown in seconds, range in cells). Each tooltip takes one
    `{0}`, the default value (for the medicine, its capitalised label);
    keep it and the label's unit. `MDWC_DressingSummary` takes `{0}` the
    kits held, `{1}` the medicine's label (lowercase, in the game's own
    translation), `{2}` the severity per kit, `{3}` the warmup, `{4}` the
    seconds per wound and `{5}` the cooldown, the times in seconds before
    the unit `s`; `MDWC_DressingSummaryNoCooldown` takes `{0}` to `{4}`
    only. `MDWC_DefaultSuffix` keeps its leading space.
  - the mechanite dressing's runtime strings. `MDWC_NoMedicineLoaded` (the
    disabled reason) takes `{0}`, the medicine's label;
    `MDWC_NoWoundsToDress` and `MDWC_NothingToTreat` (invalid target,
    without and with the helmet) and `MDWC_HostileTarget` (a conscious
    enemy) take `{0}`, the target pawn's name; the floating texts take
    counts of wounds: `MDWC_WoundsTendedPartial` and
    `MDWC_BeamBroken` `{0}` tended of `{1}` found, `MDWC_WoundsTendedDry`
    (the suite ran out) `{0}` the medicine's label, `{1}` tended of `{2}`
    found; `MDWC_TargetQuality` (the targeting cursor label, first line)
    takes `{0}` the medicine's label and `{1}` the tend quality, already a
    percentage; `MDWC_TargetCost` (its second line) takes `{0}` the wound
    count and `{1}` the kits it costs, one decimal, and `MDWC_TargetCostShort`
    adds `{2}`, the kits loaded, one decimal; `MDWC_KitsLoaded` (the armor's
    inspect line) takes `{0}` the kits loaded, one decimal, and `{1}` the
    maximum; `MDWC_StatSeverityPerKit` and `...Desc` are the stat's label
    and tooltip, with no placeholders; `MDWC_GaugeTip` (the tank gauge's
    tooltip) takes `{0}` the sealed kits, `{1}` what is left of the open
    kit, already a percentage, `{2}` the severity per kit and `{3}` the
    kits colonists reload to. Medicine labels arrive in the game's own
    translation, capitalised in `MDWC_TargetQuality` (it opens its line)
    and lowercase elsewhere.
  Name the set's things with the same renderings as the DefInjected text
  (mechanite dressing, kit, medicine suite, warcasket).
- **Enumerate the key set from `Scripts/expected-injections.json`, never
  from a Languages folder or by scanning `1.6/Defs/`.** The sidecar is a
  dump of what the live game walks; regenerate it (game closed) with
  `python3 Scripts/refresh-translation-expectations.py` whenever the
  checker reports it stale. Take the English source text for each
  `<!-- EN: -->` comment from the sidecar's `english` field.
- **Def type folders.** Never translate or place a non-`required` sidecar
  entry (texture paths such as `shieldTexPath`) in any language file.
  - `DefInjected/ThingDef/` for the three `VFEPirates.WarcasketDef`s
    (`MDWC_Warcasket_Medic`, `MDWC_WarcasketShoulders_Medic`,
    `MDWC_WarcasketHelmet_Medic`): `label`, `description`,
    `shortDescription` (a VFEP field shown in the foundry's part picker;
    translate it like any other), and on the armor the medicine suite's
    `comps.CompDressingSuite.chargeNoun` ("kit", next bullet) and
    `comps.CompDressingSuite.cooldownGerund` (vanilla's default "on
    cooldown", which the def does not set but the probe dumps; the glossary
    inherits Shipcracker's rendering). The walker keys a comp by its comp
    class. The game rolls a def type without its own database into its
    base and the checker maps the element tag via `DEF_TYPE_ALIASES` in
    `Scripts/check-translations.py`; a `WarcasketDef` folder would never
    load. The same folder lists the dressing's four motes
    (`MDWC_Mote_DressingGlow/Beam/Overlay/Flash.label`): they inherit Core's
    `MoteBase` label "Mote", never shown in play; render it as Core's own
    mote label in the language (grep the Core tar for a `Mote_` label).
  - `DefInjected/VEF.Abilities.AbilityDef/` (the full type name; it is its
    own def database, so no alias) for `MDWC_MechaniteDressing`: `label`,
    `description`, `jobReportString` and `verbProperties.label` (VEF
    copies the label into the verb it generates for every ability; render
    it identically to `label`, as Shipcracker's languages do).
  - `DefInjected/JobDef/` for `MDWC_MechaniteDressing`: `reportString`. The
    driver never shows it (it reports the ability's `jobReportString`),
    but the walker lists it; render the two alike.
  - `DefInjected/TraitDef/` for `MDWC_WarcasketTrait_Medic`:
    `degreeDatas.0.label` and `degreeDatas.0.description`. The label shows
    in the Bio tab's traits list beside VFEP's own `warcasket` trait, so it
    mirrors that trait's label in the language (`<set> + <VFEP trait
    label>`, the glossary's label pattern); the description uses
    `{PAWN_nameDef}` and the pronoun tokens like vanilla's trait
    descriptions (VFEP's own uses only `{PAWN_nameDef}`). Nothing else on
    the trait is text: its work profile and effects are overwritten at
    startup from VFEP's trait, so no language file should carry them.
- **The charge noun "kit" is shown only through plural and lookup slots.**
  Vanilla prints the suite's `chargeNoun` through `{CHARGENOUN_plural}` and
  `{lookup: ...}` in five Keyed strings (charges remaining, max charges,
  reload cost, cost per charge, none left) and never bare. Every roster
  language has a one-word equivalent; render it so the worker survives it.
  A form the language worker's fallback would mangle (German's appended
  `s` on a noun with another plural) gets its forms from
  `1.6/Languages/<Language>/WordInfo/plural.txt` (`singular;plural`, one
  line; Russian also `case.txt` rows for both numbers), lowercase file
  names. French's worker never reads the table, so the French noun must be
  a single word. Japanese, Korean and Chinese need nothing. The checker
  ignores these files; record the forms and the lookup file relied on in
  the glossary's "Charge nouns" section (`l10n/lessons.md` has the
  decompile-verified worker map; the Toxin Warcasket glossaries are the
  worked example). If a language cannot be made to render "kit", fall
  back to its rendering of "charge", which VFEP's own sets already use.
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
  "warcasket" recorded in that language's glossary. The title is coupled to
  the Keyed `MDWC_SettingsCategory` (the settings window's header,
  `WORKSHOP_TITLE_KEY`), so translate the two together.

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
  tend / tending, tend quality and speed, wound, disease, infection,
  bandage, "squad", and the three medicine labels (herbal, industrial,
  glitterworld);
- from vanilla Odyssey (ground against the Odyssey tar): vacuum;
- this mod's own ability vocabulary, coined once and recorded in the
  glossary: mechanite dressing (the ability, and the verb "dress" in its
  strings; keep "mechanite" as vanilla renders "mechanites" in the
  language), kit (the charge noun and the settings' word; vanilla prints it
  as "Kits" and "Reload cost per kit"), medicine suite (the armor's
  reloadable), diagnostic visor.

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
- There is no compat-root routing to do today (see above); everything,
  Keyed included, lands in the main tree.
- The public roster is CONTRIBUTING.md's localization table, update it in
  the same commit as any language addition or native review.
- Machine-assisted passes are run as one Opus subagent per language with a
  bounded brief (the key list, the grounded VFEP terms, the language file
  and glossary, the register gate); the lead reviews every diff and owns
  the commit.
