# .steamworkshop

Publishing metadata for the mod's Steam Workshop page. Nothing in this folder
ships with the mod (the StageMod manifest never matches it) or is loaded by
RimWorld. A `Media/` folder for Workshop images can live here later.

## Description/

One file per language, named after the RimWorld language folders in
`1.6/Languages/`. English is the source of truth; the others are
machine-assisted first passes pending native review. Format:

- Line 1: the Workshop title for that language
- Line 2: blank
- Rest: the BBCode description

Title convention: the English title leans on VFE Pirates' own vocabulary
("warcasket") so players browsing for more sets find the mod; every localized
title must contain the term VFE Pirates' own translation for that language
uses for "warcasket" (the `translate` skill's glossary records it). Titles are
fully localized with no English brand appended: Workshop search is
language-agnostic (any language's title matches regardless of UI language)
and the preview thumbnail already carries the English name. The mod has no
settings, so there is no Keyed settings-category value the title must match.

Player-facing rules from CLAUDE.md apply here too: no em dashes. Numbers on
the page must match the defs; when a tuning change moves one, update this
file in the same commit.

Steam has no API for per-language Workshop text, so updated files are pasted
manually into the Workshop page's edit UI (note Steam's own language names
differ: schinese, koreana, brazilian, latam, ...). The `release` skill diffs
`English.txt` against the last stable release tag and refreshes the translations
whenever it changed.

Every shipped language has a description file; the non-English ones are
machine-assisted first passes pending native review. The page does not exist
until the first release.
