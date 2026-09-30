# Glossary, Medic Warcasket-specific terminology

Per-language files here (`German.md`, `Russian.md`, `ChineseSimplified.md`,
and so on, named after the RimWorld language folder) hold everything about
a language's translation that is specific to this mod: the VFEP warcasket
vocabulary decisions for that language (how "warcasket", "entomb", the set
names and "warcasket foundry" are rendered, and which community VFEP
translation, if any, they were grounded against), the medic set's own
coined terms, and anything pending native review.

No per-language file exists yet: the first translation pass creates each
one. **Seed it from the sibling mod's glossary** at
`../ShipcrackerWarcasket/.claude/skills/translate/glossary/<Language>.md`,
which already records, per language, the community VFEP source (or the
absence of one), the rendering of "warcasket", the part nouns
(shell / pauldrons / helmet), the foundry, "entomb", "spacer-tech" and the
VFEP set names. Copy those rows and their grounding notes verbatim, drop
the Shipcracker-only rows (breach jump, thrusters, hull, and so on), and
add this set's own coinages. A term that has since been corrected by a
native reviewer in the sibling repo is the one to carry, not the original.

Family-shared, mod-independent findings, LanguageWorker mechanics, style
and corpus rules, and vanilla-grounded common vocabulary (armor, helmet,
plasteel, quality tiers, tech levels, and so on), live upstream in the
`l10n/` submodule at `l10n/languages/<Language>.md` (canonical checkout:
`~/dev/rimworld-l10n`), since they apply to any mod in the family, not
just this one. The VFEP warcasket vocabulary is now shared by more than
one mod in the family; if a pass finds itself copying the same rows a
third time, that is the signal to move them upstream instead.

When a future translation pass coins a new Medic Warcasket-specific term,
record it here. If a pass instead surfaces a correction to shared
mechanics or vocabulary, send that fix upstream to the l10n repo rather
than duplicating it here.
