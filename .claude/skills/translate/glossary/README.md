# Glossary, Medic Warcasket-specific terminology

Per-language files here (`German.md`, `Russian.md`, `ChineseSimplified.md`,
and so on, named after the RimWorld language folder) hold everything about
a language's translation that is specific to this mod: the medic set's own
coined terms, the Workshop title pattern, the charge nouns with that
language's plural-lookup constraint, and anything pending native review.

Every roster language has a seeded file, written before any translation
pass ran and before this set's own text was final. Each carries, in this
order: a pointer to where the family's VFEP vocabulary for that language
lives; the set-specific rows; the charge-noun constraint; the list of terms
the first pass must still ground or coin; and what is pending native
review.

The VFEP warcasket vocabulary (how "warcasket", the parts, "entomb",
VFEP's set names and research labels, the spacer openers and the
10th-generation paragraph are rendered, and which community VFEP
translation, if any, they were grounded against) is the family's, shared
with Shipcracker, Toxin and Overlord Warcasket so the sets read as one
family in-game. It lives upstream in the `l10n/` submodule at
`l10n/languages/<Language>.md`, under "Grounded common vocabulary, VFE
Pirates warcasket domain" (canonical checkout: `~/dev/rimworld-l10n`),
beside the family-shared LanguageWorker mechanics, style and corpus rules
and the vanilla-grounded common vocabulary. A VFEP term is never changed
here: reconcile it upstream, then let each set's shipped files follow.

When a future translation pass coins a new Medic Warcasket-specific term,
record it here. If a pass instead surfaces a correction to shared
mechanics or vocabulary, send that fix upstream to the l10n repo rather
than duplicating it here.
